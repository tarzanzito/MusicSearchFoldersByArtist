using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;



namespace MusicManager
{
    public partial class Form1 : Form
    {
        #region Fields

        private readonly List<string> _folderList = new List<string>();
        private const string _root1 = @"\\NAS-QNAP\music\_COLLECTION";
        private const string _root2 = @"\\NAS-QNAP\music_lossless\_COLLECTION";
        private int _totalFolders;
        private bool _isFirstItem;
        private bool _isMarquee;
        private readonly string _folderRoot;

        #endregion

        #region Constructors

        public Form1(string folderRoot)
        {
            _folderRoot = folderRoot;   

            InitializeComponent();
        }

        #endregion

        #region UI Events

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"{this.Text} - Version: {System.Windows.Forms.Application.ProductVersion}";
            this.textBoxFolder.Text = _folderRoot;

        }

        //private void buttonSearch_Click(object sender, EventArgs e)
        //{
        //    textBoxArtist.Text = textBoxArtist.Text.Trim();

        //    ChangeFormStatus(false);

        //    try
        //    {
        //        if (backgroundWorker1.IsBusy == true)
        //            return;

        //        // set arguments to worker
        //        // Não devem ser usados directamente os conteudos que estão nos componentes dentro do RunWorkerAsync
        //        // deve receber object class com toda a info necessaria como arguments.
        //        WorkerArguments arguments = new WorkerArguments();
        //        arguments.Artist = textBoxArtist.Text;
        //        arguments.SearchType = (Utils.SearchType)comboBoxSearchType.SelectedIndex;
        //        //arguments.SearchInAllLetters = this.checkBoxSearchInAll.Checked;

        //        backgroundWorker1.RunWorkerAsync(arguments);
        //    }
        //    catch (Exception ex)
        //    {
        //        listBox1.Items.Add(ex.Message);
        //        ChangeFormStatus(true);
        //    }
        //}

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        //private void listBox1_DoubleClick(object sender, EventArgs e)
        //{
        //    string folder = $"\"{_folderList[listBox1.SelectedIndex]}\"";

        //    Process.Start("explorer.exe", folder);
        //}

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
                return;

            if (backgroundWorker1.WorkerSupportsCancellation == true)
                backgroundWorker1.CancelAsync();
        }



        #endregion

        #region BackgroundWorker events

        // e.Argument - deve receber object class com toda a info necessaria.
        // Não devem ser usados directamente os conteudos que estão nos componentes
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //get Argument
                WorkerArguments arguments = e.Argument as WorkerArguments;

                //init vars
                string baseArtist = Utils.RemoveDiacritics(arguments.Artist, Utils.TextCaseAction.ToUpper);
                if (baseArtist == null)
                    throw new Exception("The artist is empty.");

                if (arguments.SearchType == Utils.SearchType.CONSTany)
                {
                    //name*
                    string firstLetter = Utils.GetFirstLetter(baseArtist);
                    if (firstLetter == null)
                        throw new Exception("First letter is invalid.");

                    string rootFolder = $@"{_root1}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.MP3, arguments.SearchType, e);

                    rootFolder = $@"{_root2}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.FLAC, arguments.SearchType, e);
                }
                else
                { 
                    //*name*
                    string rootFolder = $@"{_root1}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.MP3, arguments.SearchType, e);

                    rootFolder = $@"{_root2}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.FLAC, arguments.SearchType, e);
                }
            }
            catch (Exception ex)
            {
              //  listBox1.Items.Add(ex.Message); //review throw in thread
                ChangeFormStatus(true);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_isMarquee)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Value = 0;
                _isMarquee = false;
                return;
            }

            if (_isFirstItem)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Maximum = _totalFolders;
                progressBar1.Value = 0;
                _isFirstItem = false;
                return;
            }

            // Não devem ser usados directamente os conteudos que estão nos componentes ou em fields da classe
            // deve receber object class com toda a info necessaria no "e.UserState".

            if (e.UserState != null)
            {
                WorkerProcessState WorkerProcessState = e.UserState as WorkerProcessState;
               // listBox1.Items.Add(WorkerProcessState.Collection.ToString() + " : " + WorkerProcessState.Artist);
                _folderList.Add(WorkerProcessState.Folder);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeFormStatus(true);
        }

        #endregion

        #region BackgroundWorker Methods

        private void GetDirectoriesAll(string rootDirectoryPath, string baseArtist, Utils.Collection collection, Utils.SearchType searchType, DoWorkEventArgs e)
        {
            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);

            foreach (string folderName in folderArray)
            {
                GetDirectories(folderName, baseArtist, collection, searchType, e);
            }
        }

        private void GetDirectories(string rootDirectoryPath, string baseArtist, Utils.Collection collection, Utils.SearchType searchType, DoWorkEventArgs e)
        {
            if (!Directory.Exists(rootDirectoryPath))
                return;

            _isMarquee = true;
            backgroundWorker1.ReportProgress(0);

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);
            _totalFolders = folderArray.Length;

            _isFirstItem = true;
            WorkerProcessState workerProcessState;
            backgroundWorker1.ReportProgress(0);

            //foreach (string folderName in folderArray)
            for (int item = 0; item < _totalFolders; item++)
            {
                workerProcessState = null;

                if (backgroundWorker1.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                string folderName = folderArray[item];
                string shortName = folderName.Replace(rootDirectoryPath, "").Replace(@"\", "").Replace("/", "");

                string name = Utils.RemoveDiacritics(shortName, Utils.TextCaseAction.ToUpper);

                int pos = name.IndexOf(baseArtist);

                if (pos != -1)
                {
                    bool addItem = false;
                    if (searchType == Utils.SearchType.CONSTany) // name*
                        addItem = (pos == 0);

                    if (searchType == Utils.SearchType.anyCONSTany) // *name*
                        addItem = (pos >= 0);

                    if (addItem)
                    {
                        // Não devem ser usados directamente os conteudos que estão nos componentes ou em fields da classe
                        // ReportProgress deve receber object class com toda a info necessaria no seguendo parametro.
                        workerProcessState = new WorkerProcessState();
                        workerProcessState.Collection = collection;
                        workerProcessState.Artist = shortName;
                        workerProcessState.Folder = folderName;
                    }
                }

                backgroundWorker1.ReportProgress(item, workerProcessState);
            }
        }

        #endregion

        #region Private Methods

        private void ChangeFormStatus(bool enabled)
        {
            if (enabled)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;


            textBoxArtist.Enabled = enabled;
            buttonSearch.Enabled = enabled;
            buttonCancel.Enabled = !enabled;

            progressBar1.Visible = !enabled;

            if (!enabled)
            {
                //listBox1.Items.Clear();
                _folderList.Clear();
                progressBar1.Style = ProgressBarStyle.Blocks;
            }
            progressBar1.Value = 0;

            System.Windows.Forms.Application.DoEvents();
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            //if (listBox1.SelectedItem == null)
            //    listBox1.SelectedIndex = 0;

           // string folder = _folderList[listBox1.SelectedIndex];
            ListDirectory2(_folderRoot);
        }

        //private void ListDirectory(string path)
        //{
        //    treeView1.Nodes.Clear();

        //    var rootDirectoryInfo = new DirectoryInfo(path);
        //    treeView1.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        //    RemoveDirectoryNode(treeView1.Nodes);
        //}

        private void ListDirectory2(string path)
        {
            treeView1.Nodes.Clear();

            var rootDirectoryInfo = new DirectoryInfo(path);
            MyTreeNode myTree = CreateDirectoryNode2(rootDirectoryInfo);

            RemoveDirectoryNode2(myTree);
        }

        //private void RemoveDirectoryNode(TreeNodeCollection nodes)
        //{
        //    foreach(TreeNode node in nodes)
        //    {
        //        if (node == null)
        //            continue;

        //        if ((node.Nodes.Count == 0) && (node.Tag == null))
        //            node.Remove();
        //        else
        //            RemoveDirectoryNode(node.Nodes);
        //    }
        //}

        private void RemoveDirectoryNode2(MyTreeNode myTree)
        {
            foreach (MyTreeNode node in myTree)
            {
                if (node == null)
                    continue;

                //            if ((node.Nodes.Count == 0) && (node.Tag == null))
                //                 node.Remove();
                //             else
                //                 RemoveDirectoryNode2(node.Nodes);
            }
        }

        private string xpto = ".MP3,.FLAC,.APE,.WAV,.WP,.CUE";

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {

            var directoryNode = new TreeNode(directoryInfo.Name);

            string ext = "";
            bool isSound = false;
            bool hasSounds = false;

            foreach (var file in directoryInfo.GetFiles())
            {
                ext = file.Extension.ToUpper();
                isSound = xpto.Contains(ext);
                if (!hasSounds)
                    if (isSound)
                        hasSounds = true;

                if (isSound)
                {
                    TreeNode treeNode = new TreeNode(file.Name);
                    treeNode.Tag = "F";
                    directoryNode.Nodes.Add(treeNode);
                    //directoryNode.Nodes.Add(new TreeNode(file.Name));
                }
            }

            foreach (var directory in directoryInfo.GetDirectories())
            {
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            }

            return directoryNode;
        }

        private MyTreeNode CreateDirectoryNode2(DirectoryInfo directoryInfo)
        {
            var myTreeNode = new MyTreeNode(directoryInfo.FullName, directoryInfo.Name);

            string ext = "";
            bool isSound = false;
            bool hasSounds = false;

            foreach (var file in directoryInfo.GetFiles())
            {
                ext = file.Extension.ToUpper();
                isSound = xpto.Contains(ext);
                if (!hasSounds)
                    if (isSound)
                        hasSounds = true;

                if (isSound)
                {
                    MyTreeNode treeNode2 = new MyTreeNode(file.Name, directoryInfo.Name, 'F');
                    myTreeNode.Nodes.Add(treeNode2);
                }
            }

            foreach (var directory in directoryInfo.GetDirectories())
            {
                myTreeNode.Nodes.Add(CreateDirectoryNode2(directory));
            }

            return myTreeNode;
        }








        private void button2_Click(object sender, EventArgs e)
        {
            //if (listBox1.SelectedItem == null)
            //    listBox1.SelectedIndex = 0;
        }

        private void textBoxArtist_Leave(object sender, EventArgs e)
        {
            ////if (this.comboBoxSearchType.Text == "name*")
            //    //this.l.l.l.labelSearchTypeX.Text = "Searching In Collection First Letter.";
            ////else
            //    this.label3.Text = string.Empty;    
        }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using static MusicManager.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace MusicManager
{
    public partial class Form1 : Form
    {
        private readonly List<string> _folderList = new List<string>();
        private const string _root1 = @"\\NAS-QNAP\music\_COLLECTION";
        private const string _root2 = @"\\NAS-QNAP\music_lossless\_COLLECTION";
        private int _totalFolders;
        private bool _isFirstItem;
        private bool _isMarquee;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"{this.Text} - Version: {System.Windows.Forms.Application.ProductVersion.ToString()}";
            comboBox1.SelectedIndex = 0;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            textBoxArtist.Text = textBoxArtist.Text.Trim();

            ChangeFormStatus(false);

            try
            {
                if (backgroundWorker1.IsBusy == true)
                    return;

                //set arguments to worker
                WorkerArguments arguments = new WorkerArguments();
                arguments.Artist = textBoxArtist.Text;
                arguments.SearchType = (Utils.SearchType)comboBox1.SelectedIndex;
                arguments.SearchInAllLetters = this.checkBoxSearchInAll.Checked;

                backgroundWorker1.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                ChangeFormStatus(true);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string folder = _folderList[listBox1.SelectedIndex];

            Process.Start("explorer.exe", folder);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
                return;

            if (backgroundWorker1.WorkerSupportsCancellation == true)
                backgroundWorker1.CancelAsync();
        }

        private void checkBoxSearchInAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSearchInAll.Checked)
                this.comboBox1.SelectedIndex = 1;
            else
                this.comboBox1.SelectedIndex = 0;
        }

        private void buttonProgArchives_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                listBox1.SelectedIndex = 0;

            string folder = _folderList[listBox1.SelectedIndex];

            string FolderName = Path.GetFileName(folder);
            string cleanName = FolderName.Replace("{", "").Replace("}", "").Replace(" ", "%20").Trim();
            string finalName = Utils.RemoveDiacritics(cleanName, Utils.TextCaseAction.ToUpper);

            Process.Start("http://www.progarchives.com/google-search-results.asp?cx=artists&q=" + finalName);
        }

        ///////////////////////////// 

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

                string firstLetter = Utils.GetFirstLetter(baseArtist);
                if (firstLetter == null)
                    throw new Exception("First letter is invalid.");

                //Process
                if (arguments.SearchInAllLetters)
                {
                    string rootFolder = $@"{_root1}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.MP3, arguments.SearchType, e);

                    rootFolder = $@"{_root2}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.FLAC, arguments.SearchType, e);
                }
                else
                {
                    string rootFolder = $@"{_root1}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.MP3, arguments.SearchType, e);

                    rootFolder = $@"{_root2}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.FLAC, arguments.SearchType, e);
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message); //review throw in thread
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

            if (e.UserState != null)
            {
                WorkerProcessState WorkerProcessState = e.UserState as WorkerProcessState;
                listBox1.Items.Add(WorkerProcessState.Collection.ToString() + " : " + WorkerProcessState.Artist);
                _folderList.Add(WorkerProcessState.Folder);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeFormStatus(true);
        }

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
                        workerProcessState = new WorkerProcessState();
                        workerProcessState.Collection = collection;
                        workerProcessState.Artist = shortName;
                        workerProcessState.Folder = folderName;
                    }
                }

                backgroundWorker1.ReportProgress(item, workerProcessState);
            }
        }

        ////////////////////////////// 

        private void ChangeFormStatus(bool enabled)
        {
            if (enabled)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;


            textBoxArtist.Enabled = enabled;
            buttonSearch.Enabled = enabled;
            buttonProgArchives.Enabled = enabled && (listBox1.Items.Count > 0);
            buttonCancel.Enabled = !enabled;
            listBox1.Enabled = enabled && (listBox1.Items.Count > 0); 
            comboBox1.Enabled = enabled;
            checkBoxSearchInAll.Enabled = enabled;
            progressBar1.Visible = !enabled;

            if (!enabled)
            {
                listBox1.Items.Clear();
                _folderList.Clear();
                progressBar1.Style = ProgressBarStyle.Blocks;
            }
            progressBar1.Value = 0;

            System.Windows.Forms.Application.DoEvents();
        }
    }
}


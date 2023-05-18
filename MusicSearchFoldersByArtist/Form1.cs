using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;


namespace MusicManager
{
    internal partial class Form1 : Form
    {
        #region Fields

        private readonly List<string> _folderList = new List<string>();
        private int _totalFolders;
        private bool _isFirstItem;
        private bool _isMarquee;
        private List<CollectionInfo> _collectionInfoArray;
        private bool _workerlocked;

        #endregion

        #region Constructors

        public Form1(List<CollectionInfo> collectionInfoArray)
        {
            _collectionInfoArray = collectionInfoArray;
            InitializeComponent();
        }

        #endregion

        #region UI Events

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"{this.Text} - Version: {System.Windows.Forms.Application.ProductVersion}";
            comboBoxSearchType.SelectedIndex = 0;
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            textBoxArtist.Text = textBoxArtist.Text.Trim();

            ChangeFormStatus(false);

            try
            {
                if (backgroundWorker1.IsBusy == true)
                    return;

                // set arguments to worker
                // Não devem ser usados directamente os conteudos que estão nos componentes UI dentro do RunWorkerAsync
                // deve receber object class com toda a info necessaria como arguments.
                Utils.SearchType searchType = (Utils.SearchType)comboBoxSearchType.SelectedIndex;
                WorkerArguments arguments = new WorkerArguments(textBoxArtist.Text, searchType);

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
            backgroundWorker1.CancelAsync();
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string folder = $"\"{_folderList[listBox1.SelectedIndex]}\"";

            Process.Start("explorer.exe", folder);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
                return;

            if (backgroundWorker1.WorkerSupportsCancellation == true)
                backgroundWorker1.CancelAsync();
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

        private void textBoxArtist_TextChanged(object sender, EventArgs e)
        {
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);
        }

        private void comboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);
        }

        #endregion

        #region BackgroundWorker events

        // e.Argument - deve receber object class com toda a info necessaria.
        // Não devem ser usados directamente os conteudos que estão nos componentes
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                // REF1 ////////////////////////////////
                //using (StreamWriter sw = File.CreateText("MyTest.txt"))
                //{
                //}
                ////////////////////////////////////////
                
                _workerlocked = false;

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

                    foreach (CollectionInfo item in _collectionInfoArray)
                    {
                        string rootFolder = $@"{item.Path}\{firstLetter}\";
                        GetDirectories(rootFolder, baseArtist, item.Name, arguments.SearchType, e);
                    }
                }
                else
                {
                    //*name*
                    foreach (CollectionInfo item in _collectionInfoArray)
                    {
                        string rootFolder = $@"{item.Path}\";
                        GetDirectoriesAll(rootFolder, baseArtist, item.Name, arguments.SearchType, e);
                    }
                }
            }
            catch (Exception ex)
            {
                backgroundWorker1.CancelAsync();
                listBox1.Items.Add(ex.Message); //review throw in thread
                ChangeFormStatus(true);
            }
        }
        
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_isMarquee)
            {
                _isMarquee = false;
                progressBar1.Value = 0;
                progressBar1.Style = ProgressBarStyle.Marquee;
            }

            if (_isFirstItem)
            {
                _isFirstItem = false;
                progressBar1.Value = 0;
                progressBar1.Maximum = _totalFolders;
                progressBar1.Style = ProgressBarStyle.Blocks;
            }

            if (e.UserState != null)
            {
                // Não devem ser usados directamente os conteudos que estão nos componentes ou em fields da classe
                // deve receber object class com toda a info necessaria no "e.UserState".
                WorkerProcessState WorkerProcessState = e.UserState as WorkerProcessState;
                listBox1.Items.Add(WorkerProcessState.CollectionName + " : " + WorkerProcessState.Artist);
                _folderList.Add(WorkerProcessState.Folder);
            }

            progressBar1.Value = e.ProgressPercentage;
            System.Windows.Forms.Application.DoEvents();

            _workerlocked = false;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeFormStatus(true);
        }

        #endregion

        #region BackgroundWorker Methods

        private void GetDirectoriesAll(string rootDirectoryPath, string baseArtist, string collectionName, Utils.SearchType searchType, DoWorkEventArgs e)
        {
            if (e.Cancel == true)
                return;

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);

            foreach (string folderName in folderArray)
            {
                GetDirectories(folderName, baseArtist, collectionName, searchType, e);
                if (e.Cancel == true)
                    break;
            }
        }

        private void GetDirectories(string rootDirectoryPath, string baseArtist, string collectionName, Utils.SearchType searchType, DoWorkEventArgs e)
        {
            if (e.Cancel == true)
                return;

            if (!Directory.Exists(rootDirectoryPath))
                return;

            _isMarquee = true;
            backgroundWorkerReportProgress(0, null);

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);
            _totalFolders = folderArray.Length;

            _isFirstItem = true;
            backgroundWorkerReportProgress(0, null);

            WorkerProcessState workerProcessState;
            for (int item = 0; item < _totalFolders; item++)
            {
                workerProcessState = null;
                //System.Threading.Thread.Sleep(500);
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
                        workerProcessState = new WorkerProcessState(collectionName, shortName, folderName);
                    }
                }

                backgroundWorkerReportProgress(item, workerProcessState);
            }
        }
        private void backgroundWorkerReportProgress(int percentProgress, object userState)
        {
            this._workerlocked = true;

            /// REF 1 /////////////////////////////////////////
            //if (userState != null)
            //{
            //    WorkerProcessState workerProcessState = userState as WorkerProcessState;
            //    using (StreamWriter sw = File.AppendText("MyTest.txt"))
            //    {
            //        sw.WriteLine(workerProcessState.CollectionName + "-" + workerProcessState.Artist + "-" + workerProcessState.Folder);
            //    }
            //}
            /////////////////////////////////////////////////////
            

            //isto tem um erro qualquer que por vezes não mostra os item todos encontrados na listbox.
            //Se aqui escrever num ficheiros os itens aparecem todos no ficheiro e na listbox não.
            //só pode ser na funcao que representa o evento ReportProgress ser lançado e o worker continuar e não esperar pelo fim da funcao
            backgroundWorker1.ReportProgress(percentProgress, userState);

            while (this._workerlocked) { } // se retirar esta linha certos item não aparecem na listbox mas aparecem no ficheiro
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
            buttonProgArchives.Enabled = enabled && (listBox1.Items.Count > 0);
            buttonCancel.Enabled = !enabled;
            listBox1.Enabled = enabled;// && (listBox1.Items.Count > 0); 
            comboBoxSearchType.Enabled = enabled;
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

        #endregion

        private void SearchTypeMessage(int val)
        {
            if (val == 0)
                this.labelSearchType.Text = "Search only in First Letter Folders. (Low-cost)";
            else
                this.labelSearchType.Text = "Search in ALL Collection folders.  (High-cost)";
        }

        private void buttonTree_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                listBox1.SelectedIndex = 0;

            string folder = "\"" + _folderList[listBox1.SelectedIndex] + "\"";
            string aaa = Directory.GetCurrentDirectory();
            Process.Start("MusicPlayByFoldersArtistYearAlbum", folder);
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace MusicManager
{
    internal partial class Form1 : Form
    {
        #region Fields

        private readonly List<string> _folderFoundList;
        private readonly List<CollectionInfo> _collectionInfoArray;
        private readonly string _progArchives;
        private readonly string _musicPlayerApplication;

        #endregion

        #region Constructors

        public Form1(AppConfigInfo appConfigInfo)
        {
            _collectionInfoArray = appConfigInfo.CollectionList;
            _progArchives = appConfigInfo.ProgArchives;
            _musicPlayerApplication = appConfigInfo.MusicPlayerApplication;
            _folderFoundList = new List<string>();

            InitializeComponent();
        }

        #endregion

        #region UI Events

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"{this.Text} - Version: {System.Windows.Forms.Application.ProductVersion}";

            buttonProgArchives.Visible = (_progArchives != null);
            buttonShowTree.Visible = (_musicPlayerApplication != null);

            comboBoxSearchType.SelectedIndex = 0;
            
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);

            ChangeFormStatus(true);
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
                listBoxFound.Items.Add(ex.Message);
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
            if (listBoxFound.SelectedItem == null)
                listBoxFound.SelectedIndex = 0;

            string folder = _folderFoundList[listBoxFound.SelectedIndex];
            if (!Directory.Exists(folder))
            {
                MessageBox.Show($"Folder not found. [{folder}]", "App ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderQuotes = $"\"{folder}\"";

            Process.Start("explorer.exe", folderQuotes);
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
            if (listBoxFound.Items.Count == 0)
                return;

            if (listBoxFound.SelectedItem == null)
                listBoxFound.SelectedIndex = 0;

            string folder = _folderFoundList[listBoxFound.SelectedIndex];

            string FolderName = Path.GetFileName(folder);
           
            string[] words = FolderName.Split('{');

            string artist = "";
            string country = "";

            if (words.Length > 0)
                artist = "'" + words[0].Trim() + "'";
            
            if (words.Length > 1)
                country = "'" + words[1].Replace("}", "").Trim() + "'";

            string filter = artist; // + " " + country; ////o filter stays only with 'artist'. ignore the 'country'

            string cleanfilter = filter.Replace(" ", "%20"); //url without spaces
            string finalFilter = Utils.RemoveDiacritics(cleanfilter, Utils.TextCaseAction.ToUpper);

            Process.Start(_progArchives + finalFilter);
        }

        private void textBoxArtist_TextChanged(object sender, EventArgs e)
        {
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);
        }

        private void comboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchTypeMessage(comboBoxSearchType.SelectedIndex);
        }

        private void buttonShowTree_Click(object sender, EventArgs e)
        {
            if (listBoxFound.Items.Count == 0)
                return;

            if (listBoxFound.SelectedItem == null)
                listBoxFound.SelectedIndex = 0;


            //Process[] pname = Process.GetProcessesByName("notepad");
            //if (pname.Length == 0)
            //    MessageBox.Show("nothing");
            //else
            //    MessageBox.Show("run");

            if (!File.Exists(this._musicPlayerApplication))
            {
                MessageBox.Show($"'File [{_musicPlayerApplication}] not found.", "App ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folder = "\"" + _folderFoundList[listBoxFound.SelectedIndex] + "\"";

            Process.Start(_musicPlayerApplication, folder);
        }

        #endregion

        #region BackgroundWorker events

        // e.Argument - deve receber object class com toda a info necessaria.
        // Não devem ser usados directamente os conteudos que estão nos componentes
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // REF1
                //using (StreamWriter sw = File.CreateText("MyTest.txt"))
                //{
                //}
                
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
                listBoxFound.Items.Add(ex.Message); //review throw in thread
                ChangeFormStatus(true);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                if (e.UserState.GetType() == typeof(int))
                {
                    int total = (int)e.UserState;
                    if (total == -1) //is like Marquee
                    {
                        progressBar1.Value = 0;
                        progressBar1.Maximum = 100;
                        progressBar1.Style = ProgressBarStyle.Marquee;
                    }
                    else //is First Item
                    {
                        progressBar1.Maximum = total;
                        progressBar1.Value = total--;
                        progressBar1.Style = ProgressBarStyle.Blocks;
                    }
                    System.Windows.Forms.Application.DoEvents();
                    return;
                }

                if (e.UserState.GetType() == typeof(WorkerProcessState))
                {
                    // Não devem ser usados directamente os conteudos que estão nos componentes ou em fields da classe
                    // deve receber object class com toda a info necessaria no "e.UserState".
                    WorkerProcessState WorkerProcessState = e.UserState as WorkerProcessState;
                    listBoxFound.Items.Add(WorkerProcessState.CollectionName + " : " + WorkerProcessState.Artist);
                    _folderFoundList.Add(WorkerProcessState.Folder);
                }
            }

            progressBar1.Value = e.ProgressPercentage;
            System.Windows.Forms.Application.DoEvents();
        }

        //private void backgroundWorker1_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        //{
        //    if (_isMarquee)
        //    {
        //        _isMarquee = false;
        //        progressBar1.Value = 0;
        //        progressBar1.Style = ProgressBarStyle.Marquee;
        //    }

        //    if (_isFirstItem)
        //    {
        //        _isFirstItem = false;
        //        progressBar1.Value = 0;
        //        progressBar1.Maximum = _totalFolders;
        //        progressBar1.Style = ProgressBarStyle.Blocks;
        //    }

        //    if (e.UserState != null)
        //    {
        //        if (e.UserState.GetType() == typeof(WorkerProcessState))
        //        {
        //            // Não devem ser usados directamente os conteudos que estão nos componentes ou em fields da classe
        //            // deve receber object class com toda a info necessaria no "e.UserState".
        //            WorkerProcessState WorkerProcessState = e.UserState as WorkerProcessState;
        //            listBox1.Items.Add(WorkerProcessState.CollectionName + " : " + WorkerProcessState.Artist);
        //            _folderList.Add(WorkerProcessState.Folder);
        //        }
        //    }

        //    progressBar1.Value = e.ProgressPercentage;
        //    System.Windows.Forms.Application.DoEvents();
        //}

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

            BackgroundWorkerReportProgress(0, -1); // signals - like Marquee
            //System.Threading.Thread.Sleep(2500);

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);
            int totalFolders = folderArray.Length;

            BackgroundWorkerReportProgress(0, totalFolders); //signals - First Item;
            //System.Threading.Thread.Sleep(2500);

            WorkerProcessState workerProcessState;

            for (int item = 0; item < totalFolders; item++)
            {
                if (backgroundWorker1.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                //System.Threading.Thread.Sleep(500);

                workerProcessState = null;

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
                        // ReportProgress deve receber object com toda a info necessaria no segundo parametro.
                        workerProcessState = new WorkerProcessState(collectionName, shortName, folderName);
                    }
                }

                BackgroundWorkerReportProgress(item, workerProcessState);

                //System.Threading.Thread.Sleep(500);
            }
        }

        private void BackgroundWorkerReportProgress(int percentProgress, object userState)
        {
            //REF1
            //if (userState != null)
            //{
            //    WorkerProcessState workerProcessState = userState as WorkerProcessState;
            //    using (StreamWriter sw = File.AppendText("MyTest.txt"))
            //    {
            //        sw.WriteLine(workerProcessState.CollectionName + "-" + workerProcessState.Artist + "-" + workerProcessState.Folder);
            //    }
            //}
           
            backgroundWorker1.ReportProgress(percentProgress, userState);
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
            if (buttonProgArchives.Visible)
                buttonProgArchives.Enabled = enabled && (listBoxFound.Items.Count > 0);
            if (buttonShowTree.Visible)
                buttonShowTree.Enabled = enabled && (listBoxFound.Items.Count > 0);
            buttonCancel.Enabled = !enabled;
            listBoxFound.Enabled = enabled && (listBoxFound.Items.Count > 0); 
            comboBoxSearchType.Enabled = enabled;
            progressBar1.Visible = !enabled;

            if (!enabled)
            {
                listBoxFound.Items.Clear();
                _folderFoundList.Clear();
                progressBar1.Style = ProgressBarStyle.Blocks;
            }
            progressBar1.Value = 0;

            System.Windows.Forms.Application.DoEvents();
        }

        private void SearchTypeMessage(int val)
        {
            if (val == 0)
                this.labelSearchType.Text = "Search only in First Letter Folders. (Low-cost)";
            else
                this.labelSearchType.Text = "Search in ALL Collection folders.  (High-cost)";
        }

        #endregion

    }
}


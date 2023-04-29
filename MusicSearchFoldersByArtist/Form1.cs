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
        private string _folderName;
        private string _folderFullName;
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

                Utils.SearchType searchType = (Utils.SearchType)comboBox1.SelectedIndex;
                List<object> parameters = new List<object>();
                parameters.Add(searchType);
                backgroundWorker1.RunWorkerAsync(parameters);

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

        ///////////////////////////// 

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //get parameters
                List<object> parameters = e.Argument as List<object>;
                Utils.SearchType searchType = (Utils.SearchType)parameters[0];

                //init vars
                string baseArtist = Utils.RemoveDiacritics(textBoxArtist.Text, Utils.TextCaseAction.ToUpper);
                if (baseArtist == null)
                    throw new Exception("The artist is empty.");

                string firstLetter = Utils.GetFirstLetter(baseArtist);
                if (firstLetter == null)
                    throw new Exception("First letter is invalid.");

                //Process
                if (checkBoxSearchInAll.Checked)
                {
                    string rootFolder = $@"{_root1}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.MP3, searchType, e);

                    rootFolder = $@"{_root2}\";
                    GetDirectoriesAll(rootFolder, baseArtist, Utils.Collection.FLAC, searchType, e);
                }
                else
                {
                    string rootFolder = $@"{_root1}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.MP3, searchType, e);

                    rootFolder = $@"{_root2}\{firstLetter}\";
                    GetDirectories(rootFolder, baseArtist, Utils.Collection.FLAC, searchType, e);
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

            //if (e.ProgressPercentage > progressBar1.Maximum)
            //{
            //}

            if ((_folderName != null) && (_folderFullName != null))
            {
                _folderList.Add(_folderFullName);
                listBox1.Items.Add(_folderName);
                _folderName = null;
                _folderFullName = null;
            }

            System.Windows.Forms.Application.DoEvents();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeFormStatus(true);
        }

        ////////////////////////////// 

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

            _folderName = null;
            _folderFullName = null;

            _isMarquee = true;
            backgroundWorker1.ReportProgress(0);

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);
            _totalFolders = folderArray.Length;

            _isFirstItem = true;
            backgroundWorker1.ReportProgress(0);

            //foreach (string folderName in folderArray)
            for (int item = 0; item < _totalFolders; item++)
            {
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
                        _folderName = collection.ToString() + " : " + shortName;
                        _folderFullName = folderName;
                    }
                }

                backgroundWorker1.ReportProgress(item);
            }
        }

        private void ChangeFormStatus(bool enabled)
        {
            if (enabled)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;


            textBoxArtist.Enabled = enabled;
            buttonSearch.Enabled = enabled;
            buttonCancel.Enabled = !enabled;
            listBox1.Enabled = enabled;
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


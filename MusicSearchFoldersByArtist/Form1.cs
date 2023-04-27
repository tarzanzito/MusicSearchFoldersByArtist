using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MusicFindArtist
{
    public partial class Form1 : Form
    {
        private readonly List<string> _folderList = new List<string>();
        private const string _root1 = @"\\NAS-QNAP\music\_COLLECTION";
        private const string _root2 = @"\\NAS-QNAP\music_lossless\_COLLECTION";

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            textBoxArtist.Enabled = false;
            buttonSearch.Enabled = false;
            listBox1.Enabled = false;
            comboBox1.Enabled = false;

            listBox1.Items.Clear();
            _folderList.Clear();

            textBoxArtist.Text = textBoxArtist.Text.Trim();
            //textBoxArtist.Text = Utils.AddasteriskAtEnd(textBoxArtist.Text);

            System.Windows.Forms.Application.DoEvents();

            try
            {

                string baseArtist = Utils.RemoveDiacritics(textBoxArtist.Text, Utils.TextCaseAction.ToUpper);
                if (baseArtist == null)
                    throw new Exception("The artist is empty.");

                string firstLetter = Utils.GetFirstLetter(baseArtist);
                if (firstLetter == null)
                    throw new Exception("First letter is invalid."); ;

                //string aaa = RemoveDiacritics("eéêëèiïaâäàåcç  test");

                string rootFolder = $@"{_root1}\{firstLetter}\";

                GetDirectories(rootFolder, baseArtist, "MP3_", comboBox1.SelectedIndex);

                rootFolder = $@"{_root2}\{firstLetter}\";
                GetDirectories(rootFolder, baseArtist, "FLAC", comboBox1.SelectedIndex);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }

            textBoxArtist.Enabled = true;
            buttonSearch.Enabled = true;
            listBox1.Enabled = true;
            comboBox1.Enabled = true;
            Cursor = Cursors.Default;

            System.Windows.Forms.Application.DoEvents();
        }

        private void GetDirectories(string rootDirectoryPath, string baseArtist, string autioType, int searchType)
        {
            if (!Directory.Exists(rootDirectoryPath))
                return;

            string[] folderArray = Directory.GetDirectories(rootDirectoryPath);

            foreach (string folderName in folderArray)
            {
                string shortName = folderName.Replace(rootDirectoryPath, "");

                string name = Utils.RemoveDiacritics(shortName, Utils.TextCaseAction.ToUpper);

                int pos = name.IndexOf(baseArtist);

                bool addItem = false;
                if (searchType == 0) // name*
                    addItem = (pos == 0);

                if (searchType == 1) // *name*
                    addItem = (pos >= 0);
                
                if (addItem){
                    listBox1.Items.Add(autioType + " : " + shortName);
                    _folderList.Add(folderName);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"{this.Text} - Version: {System.Windows.Forms.Application.ProductVersion.ToString()}";
            comboBox1.SelectedIndex = 0;
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
    }
}

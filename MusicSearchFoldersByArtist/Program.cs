
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MusicManager
{
    internal static class Program
    {
        [STAThread]
        public static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppConfigInfo appConfigInfo = ReadFromConfigFile();
            if (appConfigInfo == null)
                return 1;

            Application.Run(new FormMain(appConfigInfo));

            return 0;
        }

        private static AppConfigInfo ReadFromConfigFile()
        {
            //MusicCollectionsName array
            string collectionsName = System.Configuration.ConfigurationManager.AppSettings["MusicCollectionsName"];
            if (collectionsName == null)
            {
                MessageBox.Show("'App.Config' do not contains 'MusicCollectionsName' entry.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            string collectionsPath = System.Configuration.ConfigurationManager.AppSettings["MusicCollectionsPath"];
            if (collectionsPath == null)
            {
                MessageBox.Show("'App.Config' do not contains 'MusicCollectionsPath' entry.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            string[] nameArray = collectionsName.Split('|');
            string[] pathArray = collectionsPath.Split('|');

            if (pathArray.Length != nameArray.Length)
            {
                MessageBox.Show("'App.Config' entries 'MusicCollectionsName' and 'MusicCollectionsPath' do not contains the some number of items.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            ////mount list
            List<CollectionInfo> collectionInfoList = new List<CollectionInfo>();
            for (int i = 0; i < nameArray.Length; i++)
            {
                string name = nameArray[i].Trim();
                if (name == "")
                {
                    MessageBox.Show("'App.Config' entry 'MusicCollectionsName' has an empty item.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                string path = pathArray[i].Trim();
                if (path == "")
                {
                    MessageBox.Show("'App.Config' entry 'MusicCollectionsPath' has an empty item.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                CollectionInfo collectionInfo = new CollectionInfo(name, path);
                collectionInfoList.Add(collectionInfo);
            }

            //ProgArchives
            string progArchives = System.Configuration.ConfigurationManager.AppSettings["ProgArchives"];
            if (progArchives != null)
            {
                progArchives = progArchives.Trim();
                if (progArchives == "")
                    progArchives = null;
            }

            //MusicPlayerApplication
            string musicPlayerApplication = System.Configuration.ConfigurationManager.AppSettings["MusicPlayerApplication"];
            if (musicPlayerApplication != null)
            {
                musicPlayerApplication = musicPlayerApplication.Trim();
                if (musicPlayerApplication == "")
                    musicPlayerApplication = null;
                else
                {
                    if (!File.Exists(musicPlayerApplication))
                    {
                        MessageBox.Show($"'App.Config' entry 'MusicPlayerApplication' points to file that does not exist. [{musicPlayerApplication}]", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                }
            }

            return new AppConfigInfo(collectionInfoList, progArchives, musicPlayerApplication);
        }
    }
}

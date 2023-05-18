using MusicManager;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace MusicManager
{
    internal static class Program
    {
        [STAThread]
        public static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<CollectionInfo> collectionList = ReadNamesAndPathsFromConfig();
            if (collectionList == null)
                return 1;

            Application.Run(new Form1(collectionList));

            return 0;
        }

        private static List<CollectionInfo> ReadNamesAndPathsFromConfig()
        {

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

            string[] nameArray = collectionsName.Split(':');
            string[] pathArray = collectionsPath.Split(':');

            if (pathArray.Length != nameArray.Length)
            {
                MessageBox.Show("'App.Config' entries 'MusicCollectionsName' and 'MusicCollectionsPath' do not contains some number of items.", "App.Config ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            List<CollectionInfo> collectionInfoList = new List<CollectionInfo>();
            for (int i = 0; i < nameArray.Length; i++)
            {
                CollectionInfo collectionInfo = new CollectionInfo(nameArray[i], pathArray[i]);
                collectionInfoList.Add(collectionInfo);
            }

            return collectionInfoList;
        }
    }
}

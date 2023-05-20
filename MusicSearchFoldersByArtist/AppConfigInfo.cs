using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager
{
    internal class AppConfigInfo
    {
        public List<CollectionInfo> CollectionList { get; private set; }
        public string ProgArchives { get; private set; }
        public string MusicPlayerApplication { get; private set; }
        
        public AppConfigInfo(List<CollectionInfo> collectionList, string progArchives, string musicPlayerApplication)
        {
            CollectionList = collectionList;
            ProgArchives = progArchives;
            MusicPlayerApplication = musicPlayerApplication;
        }
    }
}

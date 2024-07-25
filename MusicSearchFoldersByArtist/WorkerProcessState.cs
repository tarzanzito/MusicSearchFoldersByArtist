using System;

namespace MusicManager
{
    internal class WorkerProcessState
    {
        public string CollectionName { get; private set; }
        public string Artist { get; private set; }
        public string Folder { get; private set; }
        public string LastDate { get; private set; }

        public WorkerProcessState(string collectionName, string artist, string lastDate, string folder)
        {
            CollectionName = collectionName;
            Artist = artist;
            LastDate = lastDate;
            Folder = folder;
        }   
    }
}

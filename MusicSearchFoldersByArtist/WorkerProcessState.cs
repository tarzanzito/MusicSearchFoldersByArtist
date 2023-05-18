using System;

namespace MusicManager
{
    internal class WorkerProcessState
    {
        public string CollectionName { get; private set; }
        public string Artist { get; private set; }
        public string Folder { get; private set; }

        public WorkerProcessState(string collectionName, string artist, string folder)
        {
            CollectionName = collectionName;
            Artist = artist;
            Folder = folder;
        }   
    }
}

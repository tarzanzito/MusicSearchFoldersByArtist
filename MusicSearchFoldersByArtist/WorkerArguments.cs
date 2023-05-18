
namespace MusicManager
{
    internal class WorkerArguments
    {
        public string Artist { get; private set; }
        public Utils.SearchType SearchType { get; private set; }

        public WorkerArguments(string artist, Utils.SearchType searchType)
        {
            Artist = artist;
            SearchType = searchType;
        }
    }
}

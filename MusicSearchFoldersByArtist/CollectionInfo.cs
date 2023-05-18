
namespace MusicManager
{
    internal class CollectionInfo
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public CollectionInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}

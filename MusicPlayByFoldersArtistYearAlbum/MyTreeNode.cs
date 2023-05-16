using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager
{
    public class MyTreeNode : IEnumerable
    {
        public char Tag { get; private set; }
        public string FileFullName { get; private set; }
        public string FileName { get; private set; }
        public List<MyTreeNode> Nodes { get; private set; }

        public MyTreeNode(string fullName, string fileName, char tag) : this(fullName, fileName)
        {
            Tag = tag;
        }

        public MyTreeNode(string fullFileName, string fileName)
        {
            FileFullName = fullFileName;
            FileName = fileName;
            Nodes = new List<MyTreeNode>();
            Tag = '\0';
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}




//using System.Collections;
//using System.ComponentModel;
//using System.Drawing.Design;
//using System.Globalization;

//namespace System.Windows.Forms
//{
//    public class TreeNodeCollection : IList, ICollection, IEnumerable
//    {
//        private TreeNode owner;

//        private int lastAccessedIndex = -1;

//        private int fixedIndex = -1;

//        internal int FixedIndex
//        {
//            get
//            {
//                return fixedIndex;
//            }
//            set
//            {
//                fixedIndex = value;
//            }
//        }

//        public virtual TreeNode this[int index]
//        {
//            get
//            {
//                if (index < 0 || index >= owner.childCount)
//                {
//                    throw new ArgumentOutOfRangeException("index");
//                }

//                return owner.children[index];
//            }
//            set
//            {
//                if (index < 0 || index >= owner.childCount)
//                {
//                    throw new ArgumentOutOfRangeException("index", SR.GetString("InvalidArgument", "index", index.ToString(CultureInfo.CurrentCulture)));
//                }

//                value.parent = owner;
//                value.index = index;
//                owner.children[index] = value;
//                value.Realize(insertFirst: false);
//            }
//        }

//        object IList.this[int index]
//        {
//            get
//            {
//                return this[index];
//            }
//            set
//            {
//                if (value is TreeNode)
//                {
//                    this[index] = (TreeNode)value;
//                    return;
//                }

//                throw new ArgumentException(SR.GetString("TreeNodeCollectionBadTreeNode"), "value");
//            }
//        }

//        public virtual TreeNode this[string key]
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(key))
//                {
//                    return null;
//                }

//                int index = IndexOfKey(key);
//                if (IsValidIndex(index))
//                {
//                    return this[index];
//                }

//                return null;
//            }
//        }

        
//        public int Count => owner.childCount;

//        object ICollection.SyncRoot => this;

//        bool ICollection.IsSynchronized => false;

//        bool IList.IsFixedSize => false;

//        public bool IsReadOnly => false;

//        internal TreeNodeCollection(TreeNode owner)
//        {
//            this.owner = owner;
//        }

//        public virtual TreeNode Add(string text)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual TreeNode Add(string key, string text)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual TreeNode Add(string key, string text, int imageIndex)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageIndex = imageIndex;
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual TreeNode Add(string key, string text, string imageKey)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageKey = imageKey;
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual TreeNode Add(string key, string text, int imageIndex, int selectedImageIndex)
//        {
//            TreeNode treeNode = new TreeNode(text, imageIndex, selectedImageIndex);
//            treeNode.Name = key;
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual TreeNode Add(string key, string text, string imageKey, string selectedImageKey)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageKey = imageKey;
//            treeNode.SelectedImageKey = selectedImageKey;
//            Add(treeNode);
//            return treeNode;
//        }

//        public virtual void AddRange(TreeNode[] nodes)
//        {
//            if (nodes == null)
//            {
//                throw new ArgumentNullException("nodes");
//            }

//            if (nodes.Length != 0)
//            {
//                TreeView treeView = owner.TreeView;
//                if (treeView != null && nodes.Length > 200)
//                {
//                    treeView.BeginUpdate();
//                }

//                owner.Nodes.FixedIndex = owner.childCount;
//                owner.EnsureCapacity(nodes.Length);
//                for (int num = nodes.Length - 1; num >= 0; num--)
//                {
//                    AddInternal(nodes[num], num);
//                }

//                owner.Nodes.FixedIndex = -1;
//                if (treeView != null && nodes.Length > 200)
//                {
//                    treeView.EndUpdate();
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Finds the tree nodes with specified key, optionally searching subnodes.
//        //
//        // Parameters:
//        //   key:
//        //     The name of the tree node to search for.
//        //
//        //   searchAllChildren:
//        //     true to search child nodes of tree nodes; otherwise, false.
//        //
//        // Returns:
//        //     An array of System.Windows.Forms.TreeNode objects whose System.Windows.Forms.TreeNode.Name
//        //     property matches the specified key.
//        public TreeNode[] Find(string key, bool searchAllChildren)
//        {
//            ArrayList arrayList = FindInternal(key, searchAllChildren, this, new ArrayList());
//            TreeNode[] array = new TreeNode[arrayList.Count];
//            arrayList.CopyTo(array, 0);
//            return array;
//        }

//        private ArrayList FindInternal(string key, bool searchAllChildren, TreeNodeCollection treeNodeCollectionToLookIn, ArrayList foundTreeNodes)
//        {
//            if (treeNodeCollectionToLookIn == null || foundTreeNodes == null)
//            {
//                return null;
//            }

//            for (int i = 0; i < treeNodeCollectionToLookIn.Count; i++)
//            {
//                if (treeNodeCollectionToLookIn[i] != null && WindowsFormsUtils.SafeCompareStrings(treeNodeCollectionToLookIn[i].Name, key, ignoreCase: true))
//                {
//                    foundTreeNodes.Add(treeNodeCollectionToLookIn[i]);
//                }
//            }

//            if (searchAllChildren)
//            {
//                for (int j = 0; j < treeNodeCollectionToLookIn.Count; j++)
//                {
//                    if (treeNodeCollectionToLookIn[j] != null && treeNodeCollectionToLookIn[j].Nodes != null && treeNodeCollectionToLookIn[j].Nodes.Count > 0)
//                    {
//                        foundTreeNodes = FindInternal(key, searchAllChildren, treeNodeCollectionToLookIn[j].Nodes, foundTreeNodes);
//                    }
//                }
//            }

//            return foundTreeNodes;
//        }

//        //
//        // Summary:
//        //     Adds a previously created tree node to the end of the tree node collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to add to the collection.
//        //
//        // Returns:
//        //     The zero-based index value of the System.Windows.Forms.TreeNode added to the
//        //     tree node collection.
//        //
//        // Exceptions:
//        //   T:System.ArgumentException:
//        //     The node is currently assigned to another System.Windows.Forms.TreeView.
//        public virtual int Add(TreeNode node)
//        {
//            return AddInternal(node, 0);
//        }

//        private int AddInternal(TreeNode node, int delta)
//        {
//            if (node == null)
//            {
//                throw new ArgumentNullException("node");
//            }

//            if (node.handle != IntPtr.Zero)
//            {
//                throw new ArgumentException(SR.GetString("OnlyOneControl", node.Text), "node");
//            }

//            TreeView treeView = owner.TreeView;
//            if (treeView != null && treeView.Sorted)
//            {
//                return owner.AddSorted(node);
//            }

//            node.parent = owner;
//            int num = owner.Nodes.FixedIndex;
//            if (num != -1)
//            {
//                node.index = num + delta;
//            }
//            else
//            {
//                owner.EnsureCapacity(1);
//                node.index = owner.childCount;
//            }

//            owner.children[node.index] = node;
//            owner.childCount++;
//            node.Realize(insertFirst: false);
//            if (treeView != null && node == treeView.selectedNode)
//            {
//                treeView.SelectedNode = node;
//            }

//            if (treeView != null && treeView.TreeViewNodeSorter != null)
//            {
//                treeView.Sort();
//            }

//            return node.index;
//        }

//        //
//        // Summary:
//        //     Adds an object to the end of the tree node collection.
//        //
//        // Parameters:
//        //   node:
//        //     The object to add to the tree node collection.
//        //
//        // Returns:
//        //     The zero-based index value of the System.Windows.Forms.TreeNode that was added
//        //     to the tree node collection.
//        //
//        // Exceptions:
//        //   T:System.Exception:
//        //     node is currently assigned to another System.Windows.Forms.TreeView control.
//        //
//        //   T:System.ArgumentNullException:
//        //     node is null.
//        int IList.Add(object node)
//        {
//            if (node == null)
//            {
//                throw new ArgumentNullException("node");
//            }

//            if (node is TreeNode)
//            {
//                return Add((TreeNode)node);
//            }

//            return Add(node.ToString()).index;
//        }

//        //
//        // Summary:
//        //     Determines whether the specified tree node is a member of the collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to locate in the collection.
//        //
//        // Returns:
//        //     true if the System.Windows.Forms.TreeNode is a member of the collection; otherwise,
//        //     false.
//        public bool Contains(TreeNode node)
//        {
//            return IndexOf(node) != -1;
//        }

//        //
//        // Summary:
//        //     Determines whether the collection contains a tree node with the specified key.
//        //
//        // Parameters:
//        //   key:
//        //     The name of the System.Windows.Forms.TreeNode to search for.
//        //
//        // Returns:
//        //     true to indicate the collection contains a System.Windows.Forms.TreeNode with
//        //     the specified key; otherwise, false.
//        public virtual bool ContainsKey(string key)
//        {
//            return IsValidIndex(IndexOfKey(key));
//        }

//        //
//        // Summary:
//        //     Determines whether the specified tree node is a member of the collection.
//        //
//        // Parameters:
//        //   node:
//        //     The object to find in the collection.
//        //
//        // Returns:
//        //     true if node is a member of the collection; otherwise, false.
//        bool IList.Contains(object node)
//        {
//            if (node is TreeNode)
//            {
//                return Contains((TreeNode)node);
//            }

//            return false;
//        }

//        //
//        // Summary:
//        //     Returns the index of the specified tree node in the collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to locate in the collection.
//        //
//        // Returns:
//        //     The zero-based index of the item found in the tree node collection; otherwise,
//        //     -1.
//        public int IndexOf(TreeNode node)
//        {
//            for (int i = 0; i < Count; i++)
//            {
//                if (this[i] == node)
//                {
//                    return i;
//                }
//            }

//            return -1;
//        }

//        //
//        // Summary:
//        //     Returns the index of the specified tree node in the collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to locate in the collection.
//        //
//        // Returns:
//        //     The zero-based index of the item found in the tree node collection; otherwise,
//        //     -1.
//        int IList.IndexOf(object node)
//        {
//            if (node is TreeNode)
//            {
//                return IndexOf((TreeNode)node);
//            }

//            return -1;
//        }

//        //
//        // Summary:
//        //     Returns the index of the first occurrence of a tree node with the specified key.
//        //
//        // Parameters:
//        //   key:
//        //     The name of the tree node to search for.
//        //
//        // Returns:
//        //     The zero-based index of the first occurrence of a tree node with the specified
//        //     key, if found; otherwise, -1.
//        public virtual int IndexOfKey(string key)
//        {
//            if (string.IsNullOrEmpty(key))
//            {
//                return -1;
//            }

//            if (IsValidIndex(lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, ignoreCase: true))
//            {
//                return lastAccessedIndex;
//            }

//            for (int i = 0; i < Count; i++)
//            {
//                if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, ignoreCase: true))
//                {
//                    lastAccessedIndex = i;
//                    return i;
//                }
//            }

//            lastAccessedIndex = -1;
//            return -1;
//        }

//        //
//        // Summary:
//        //     Inserts an existing tree node into the tree node collection at the specified
//        //     location.
//        //
//        // Parameters:
//        //   index:
//        //     The indexed location within the collection to insert the tree node.
//        //
//        //   node:
//        //     The System.Windows.Forms.TreeNode to insert into the collection.
//        //
//        // Exceptions:
//        //   T:System.ArgumentException:
//        //     The node is currently assigned to another System.Windows.Forms.TreeView.
//        public virtual void Insert(int index, TreeNode node)
//        {
//            if (node.handle != IntPtr.Zero)
//            {
//                throw new ArgumentException(SR.GetString("OnlyOneControl", node.Text), "node");
//            }

//            TreeView treeView = owner.TreeView;
//            if (treeView != null && treeView.Sorted)
//            {
//                owner.AddSorted(node);
//                return;
//            }

//            if (index < 0)
//            {
//                index = 0;
//            }

//            if (index > owner.childCount)
//            {
//                index = owner.childCount;
//            }

//            owner.InsertNodeAt(index, node);
//        }

//        //
//        // Summary:
//        //     Inserts an existing tree node in the tree node collection at the specified location.
//        //
//        // Parameters:
//        //   index:
//        //     The indexed location within the collection to insert the tree node.
//        //
//        //   node:
//        //     The System.Windows.Forms.TreeNode to insert into the collection.
//        //
//        // Exceptions:
//        //   T:System.ArgumentException:
//        //     node is currently assigned to another System.Windows.Forms.TreeView. -or- node
//        //     is not a System.Windows.Forms.TreeNode.
//        void IList.Insert(int index, object node)
//        {
//            if (node is TreeNode)
//            {
//                Insert(index, (TreeNode)node);
//                return;
//            }

//            throw new ArgumentException(SR.GetString("TreeNodeCollectionBadTreeNode"), "node");
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified text and inserts it at the specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string text)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified text and key, and inserts it into the
//        //     collection.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   key:
//        //     The name of the tree node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string key, string text)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified key, text, and image, and inserts it into
//        //     the collection at the specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   key:
//        //     The name of the tree node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        //   imageIndex:
//        //     The index of the image to display in the tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string key, string text, int imageIndex)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageIndex = imageIndex;
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified key, text, and image, and inserts it into
//        //     the collection at the specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   key:
//        //     The name of the tree node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        //   imageKey:
//        //     The key of the image to display in the tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string key, string text, string imageKey)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageKey = imageKey;
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified key, text, and images, and inserts it
//        //     into the collection at the specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   key:
//        //     The name of the tree node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        //   imageIndex:
//        //     The index of the image to display in the tree node.
//        //
//        //   selectedImageIndex:
//        //     The index of the image to display in the tree node when it is in a selected state.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string key, string text, int imageIndex, int selectedImageIndex)
//        {
//            TreeNode treeNode = new TreeNode(text, imageIndex, selectedImageIndex);
//            treeNode.Name = key;
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        //
//        // Summary:
//        //     Creates a tree node with the specified key, text, and images, and inserts it
//        //     into the collection at the specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The location within the collection to insert the node.
//        //
//        //   key:
//        //     The name of the tree node.
//        //
//        //   text:
//        //     The text to display in the tree node.
//        //
//        //   imageKey:
//        //     The key of the image to display in the tree node.
//        //
//        //   selectedImageKey:
//        //     The key of the image to display in the tree node when it is in a selected state.
//        //
//        // Returns:
//        //     The System.Windows.Forms.TreeNode that was inserted in the collection.
//        public virtual TreeNode Insert(int index, string key, string text, string imageKey, string selectedImageKey)
//        {
//            TreeNode treeNode = new TreeNode(text);
//            treeNode.Name = key;
//            treeNode.ImageKey = imageKey;
//            treeNode.SelectedImageKey = selectedImageKey;
//            Insert(index, treeNode);
//            return treeNode;
//        }

//        private bool IsValidIndex(int index)
//        {
//            if (index >= 0)
//            {
//                return index < Count;
//            }

//            return false;
//        }

//        //
//        // Summary:
//        //     Removes all tree nodes from the collection.
//        public virtual void Clear()
//        {
//            owner.Clear();
//        }

//        //
//        // Summary:
//        //     Copies the entire collection into an existing array at a specified location within
//        //     the array.
//        //
//        // Parameters:
//        //   dest:
//        //     The destination array.
//        //
//        //   index:
//        //     The index in the destination array at which storing begins.
//        public void CopyTo(Array dest, int index)
//        {
//            if (owner.childCount > 0)
//            {
//                Array.Copy(owner.children, 0, dest, index, owner.childCount);
//            }
//        }

//        //
//        // Summary:
//        //     Removes the specified tree node from the tree node collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to remove.
//        public void Remove(TreeNode node)
//        {
//            node.Remove();
//        }

//        //
//        // Summary:
//        //     Removes the specified tree node from the tree node collection.
//        //
//        // Parameters:
//        //   node:
//        //     The System.Windows.Forms.TreeNode to remove from the collection.
//        void IList.Remove(object node)
//        {
//            if (node is TreeNode)
//            {
//                Remove((TreeNode)node);
//            }
//        }

//        //
//        // Summary:
//        //     Removes a tree node from the tree node collection at a specified index.
//        //
//        // Parameters:
//        //   index:
//        //     The index of the System.Windows.Forms.TreeNode to remove.
//        public virtual void RemoveAt(int index)
//        {
//            this[index].Remove();
//        }

//        //
//        // Summary:
//        //     Removes the tree node with the specified key from the collection.
//        //
//        // Parameters:
//        //   key:
//        //     The name of the tree node to remove from the collection.
//        public virtual void RemoveByKey(string key)
//        {
//            int index = IndexOfKey(key);
//            if (IsValidIndex(index))
//            {
//                RemoveAt(index);
//            }
//        }

//        //
//        // Summary:
//        //     Returns an enumerator that can be used to iterate through the tree node collection.
//        //
//        // Returns:
//        //     An System.Collections.IEnumerator that represents the tree node collection.
//        public IEnumerator GetEnumerator()
//        {
//            return new WindowsFormsUtils.ArraySubsetEnumerator(owner.children, owner.childCount);
//        }
//    }
//}

//using System.Collections;
//using System.Collections.Specialized;
//using System.ComponentModel;
//using System.Drawing;
//using System.Drawing.Design;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.Runtime.Serialization;
//using System.Security.Permissions;
//using System.Text;

//namespace System.Windows.Forms
//{

//    public class MyTreeNode : MarshalByRefObject, ICloneable, ISerializable
//    {
//        internal class TreeNodeImageIndexer : ImageList.Indexer
//        {
//            public enum ImageListType
//            {
//                Default,
//                State
//            }

//            private TreeNode owner;

//            private ImageListType imageListType;

//            public override ImageList ImageList
//            {
//                get
//                {
//                    if (owner.TreeView != null)
//                    {
//                        if (imageListType == ImageListType.State)
//                        {
//                            return owner.TreeView.StateImageList;
//                        }

//                        return owner.TreeView.ImageList;
//                    }

//                    return null;
//                }
//                set
//                {
//                }
//            }

//            public TreeNodeImageIndexer(TreeNode node, ImageListType imageListType)
//            {
//                owner = node;
//                this.imageListType = imageListType;
//            }
//        }

//        private const int SHIFTVAL = 12;

//        private const int CHECKED = 8192;

//        private const int UNCHECKED = 4096;

//        private const int ALLOWEDIMAGES = 14;

//        internal const int MAX_TREENODES_OPS = 200;

//        internal OwnerDrawPropertyBag propBag;

//        internal IntPtr handle;

//        internal string text;

//        internal string name;

//        private const int TREENODESTATE_isChecked = 1;

//        private BitVector32 treeNodeState;

//        private TreeNodeImageIndexer imageIndexer;

//        private TreeNodeImageIndexer selectedImageIndexer;

//        private TreeNodeImageIndexer stateImageIndexer;

//        private string toolTipText = "";

//        private ContextMenu contextMenu;

//        private ContextMenuStrip contextMenuStrip;

//        internal bool nodesCleared;

//        internal int index;

//        internal int childCount;

//        internal TreeNode[] children;

//        internal TreeNode parent;

//        internal TreeView treeView;

//        private bool expandOnRealization;

//        private bool collapseOnRealization;

//        private TreeNodeCollection nodes;

//        private object userData;

//        private static readonly int insertMask = 35;

//        internal TreeNodeImageIndexer ImageIndexer
//        {
//            get
//            {
//                if (imageIndexer == null)
//                {
//                    imageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
//                }

//                return imageIndexer;
//            }
//        }

//        internal TreeNodeImageIndexer SelectedImageIndexer
//        {
//            get
//            {
//                if (selectedImageIndexer == null)
//                {
//                    selectedImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
//                }

//                return selectedImageIndexer;
//            }
//        }

//        internal TreeNodeImageIndexer StateImageIndexer
//        {
//            get
//            {
//                if (stateImageIndexer == null)
//                {
//                    stateImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.State);
//                }

//                return stateImageIndexer;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the background color of the tree node.
//        //
//        // Returns:
//        //     The background System.Drawing.Color of the tree node. The default is System.Drawing.Color.Empty.
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeBackColorDescr")]
//        public Color BackColor
//        {
//            get
//            {
//                if (propBag == null)
//                {
//                    return Color.Empty;
//                }

//                return propBag.BackColor;
//            }
//            set
//            {
//                Color backColor = BackColor;
//                if (value.IsEmpty)
//                {
//                    if (propBag != null)
//                    {
//                        propBag.BackColor = Color.Empty;
//                        RemovePropBagIfEmpty();
//                    }

//                    if (!backColor.IsEmpty)
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//                else
//                {
//                    if (propBag == null)
//                    {
//                        propBag = new OwnerDrawPropertyBag();
//                    }

//                    propBag.BackColor = value;
//                    if (!value.Equals(backColor))
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets the bounds of the tree node.
//        //
//        // Returns:
//        //     The System.Drawing.Rectangle that represents the bounds of the tree node.
//        [Browsable(false)]
//        public unsafe Rectangle Bounds
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                if (treeView == null || treeView.IsDisposed)
//                {
//                    return Rectangle.Empty;
//                }

//                NativeMethods.RECT lParam = default(NativeMethods.RECT);
//                *(IntPtr*)(&lParam.left) = Handle;
//                if ((int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 1, ref lParam) == 0)
//                {
//                    return Rectangle.Empty;
//                }

//                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
//            }
//        }

//        internal unsafe Rectangle RowBounds
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                NativeMethods.RECT lParam = default(NativeMethods.RECT);
//                *(IntPtr*)(&lParam.left) = Handle;
//                if (treeView == null || treeView.IsDisposed)
//                {
//                    return Rectangle.Empty;
//                }

//                if ((int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 0, ref lParam) == 0)
//                {
//                    return Rectangle.Empty;
//                }

//                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
//            }
//        }

//        internal bool CheckedStateInternal
//        {
//            get
//            {
//                return treeNodeState[1];
//            }
//            set
//            {
//                treeNodeState[1] = value;
//            }
//        }

//        internal bool CheckedInternal
//        {
//            get
//            {
//                return CheckedStateInternal;
//            }
//            set
//            {
//                CheckedStateInternal = value;
//                if (!(handle == IntPtr.Zero))
//                {
//                    TreeView treeView = TreeView;
//                    if (treeView != null && treeView.IsHandleCreated && !treeView.IsDisposed)
//                    {
//                        NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
//                        lParam.mask = 24;
//                        lParam.hItem = handle;
//                        lParam.stateMask = 61440;
//                        lParam.state |= (value ? 8192 : 4096);
//                        UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
//                    }
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets a value indicating whether the tree node is in a checked state.
//        //
//        // Returns:
//        //     true if the tree node is in a checked state; otherwise, false.
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeCheckedDescr")]
//        [DefaultValue(false)]
//        public bool Checked
//        {
//            get
//            {
//                return CheckedInternal;
//            }
//            set
//            {
//                TreeView treeView = TreeView;
//                if (treeView != null)
//                {
//                    if (!treeView.TreeViewBeforeCheck(this, TreeViewAction.Unknown))
//                    {
//                        CheckedInternal = value;
//                        treeView.TreeViewAfterCheck(this, TreeViewAction.Unknown);
//                    }
//                }
//                else
//                {
//                    CheckedInternal = value;
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets the shortcut menu that is associated with this tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.ContextMenu that is associated with the tree node.
//        [SRCategory("CatBehavior")]
//        [DefaultValue(null)]
//        [SRDescription("ControlContextMenuDescr")]
//        public virtual ContextMenu ContextMenu
//        {
//            get
//            {
//                return contextMenu;
//            }
//            set
//            {
//                contextMenu = value;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the shortcut menu associated with this tree node.
//        //
//        // Returns:
//        //     The System.Windows.Forms.ContextMenuStrip associated with the tree node.
//        [SRCategory("CatBehavior")]
//        [DefaultValue(null)]
//        [SRDescription("ControlContextMenuDescr")]
//        public virtual ContextMenuStrip ContextMenuStrip
//        {
//            get
//            {
//                return contextMenuStrip;
//            }
//            set
//            {
//                contextMenuStrip = value;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the first child tree node in the tree node collection.
//        //
//        // Returns:
//        //     The first child System.Windows.Forms.TreeNode in the System.Windows.Forms.TreeNode.Nodes
//        //     collection.
//        [Browsable(false)]
//        public TreeNode FirstNode
//        {
//            get
//            {
//                if (childCount == 0)
//                {
//                    return null;
//                }

//                return children[0];
//            }
//        }

//        private TreeNode FirstVisibleParent
//        {
//            get
//            {
//                TreeNode treeNode = this;
//                while (treeNode != null && treeNode.Bounds.IsEmpty)
//                {
//                    treeNode = treeNode.Parent;
//                }

//                return treeNode;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the foreground color of the tree node.
//        //
//        // Returns:
//        //     The foreground System.Drawing.Color of the tree node.
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeForeColorDescr")]
//        public Color ForeColor
//        {
//            get
//            {
//                if (propBag == null)
//                {
//                    return Color.Empty;
//                }

//                return propBag.ForeColor;
//            }
//            set
//            {
//                Color foreColor = ForeColor;
//                if (value.IsEmpty)
//                {
//                    if (propBag != null)
//                    {
//                        propBag.ForeColor = Color.Empty;
//                        RemovePropBagIfEmpty();
//                    }

//                    if (!foreColor.IsEmpty)
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//                else
//                {
//                    if (propBag == null)
//                    {
//                        propBag = new OwnerDrawPropertyBag();
//                    }

//                    propBag.ForeColor = value;
//                    if (!value.Equals(foreColor))
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets the path from the root tree node to the current tree node.
//        //
//        // Returns:
//        //     The path from the root tree node to the current tree node.
//        //
//        // Exceptions:
//        //   T:System.InvalidOperationException:
//        //     The node is not contained in a System.Windows.Forms.TreeView.
//        [Browsable(false)]
//        public string FullPath
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                if (treeView != null)
//                {
//                    StringBuilder stringBuilder = new StringBuilder();
//                    GetFullPath(stringBuilder, treeView.PathSeparator);
//                    return stringBuilder.ToString();
//                }

//                throw new InvalidOperationException(SR.GetString("TreeNodeNoParent"));
//            }
//        }

//        //
//        // Summary:
//        //     Gets the handle of the tree node.
//        //
//        // Returns:
//        //     The tree node handle.
//        [Browsable(false)]
//        public IntPtr Handle
//        {
//            get
//            {
//                if (handle == IntPtr.Zero)
//                {
//                    TreeView.CreateControl();
//                }

//                return handle;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the image list index value of the image displayed when the tree
//        //     node is in the unselected state.
//        //
//        // Returns:
//        //     A zero-based index value that represents the image position in the assigned System.Windows.Forms.ImageList.
//        [Localizable(true)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeImageIndexDescr")]
//        [TypeConverter(typeof(TreeViewImageIndexConverter))]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [DefaultValue(-1)]
//        [RelatedImageList("TreeView.ImageList")]
//        public int ImageIndex
//        {
//            get
//            {
//                return ImageIndexer.Index;
//            }
//            set
//            {
//                ImageIndexer.Index = value;
//                UpdateNode(2);
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the key for the image associated with this tree node when the node
//        //     is in an unselected state.
//        //
//        // Returns:
//        //     The key for the image associated with this tree node when the node is in an unselected
//        //     state.
//        [Localizable(true)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeImageKeyDescr")]
//        [TypeConverter(typeof(TreeViewImageKeyConverter))]
//        [DefaultValue("")]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [RelatedImageList("TreeView.ImageList")]
//        public string ImageKey
//        {
//            get
//            {
//                return ImageIndexer.Key;
//            }
//            set
//            {
//                ImageIndexer.Key = value;
//                UpdateNode(2);
//            }
//        }

//        //
//        // Summary:
//        //     Gets the position of the tree node in the tree node collection.
//        //
//        // Returns:
//        //     A zero-based index value that represents the position of the tree node in the
//        //     System.Windows.Forms.TreeNode.Nodes collection.
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeIndexDescr")]
//        public int Index => index;

//        //
//        // Summary:
//        //     Gets a value indicating whether the tree node is in an editable state.
//        //
//        // Returns:
//        //     true if the tree node is in editable state; otherwise, false.
//        [Browsable(false)]
//        public bool IsEditing
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                if (treeView != null)
//                {
//                    return treeView.editNode == this;
//                }

//                return false;
//            }
//        }

//        //
//        // Summary:
//        //     Gets a value indicating whether the tree node is in the expanded state.
//        //
//        // Returns:
//        //     true if the tree node is in the expanded state; otherwise, false.
//        [Browsable(false)]
//        public bool IsExpanded
//        {
//            get
//            {
//                if (handle == IntPtr.Zero)
//                {
//                    return expandOnRealization;
//                }

//                return (State & 0x20) != 0;
//            }
//        }

//        //
//        // Summary:
//        //     Gets a value indicating whether the tree node is in the selected state.
//        //
//        // Returns:
//        //     true if the tree node is in the selected state; otherwise, false.
//        [Browsable(false)]
//        public bool IsSelected
//        {
//            get
//            {
//                if (handle == IntPtr.Zero)
//                {
//                    return false;
//                }

//                return (State & 2) != 0;
//            }
//        }

//        //
//        // Summary:
//        //     Gets a value indicating whether the tree node is visible or partially visible.
//        //
//        // Returns:
//        //     true if the tree node is visible or partially visible; otherwise, false.
//        [Browsable(false)]
//        public unsafe bool IsVisible
//        {
//            get
//            {
//                if (handle == IntPtr.Zero)
//                {
//                    return false;
//                }

//                TreeView treeView = TreeView;
//                if (treeView.IsDisposed)
//                {
//                    return false;
//                }

//                NativeMethods.RECT lParam = default(NativeMethods.RECT);
//                *(IntPtr*)(&lParam.left) = Handle;
//                bool flag = (int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 1, ref lParam) != 0;
//                if (flag)
//                {
//                    Size clientSize = treeView.ClientSize;
//                    flag = lParam.bottom > 0 && lParam.right > 0 && lParam.top < clientSize.Height && lParam.left < clientSize.Width;
//                }

//                return flag;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the last child tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the last child tree node.
//        [Browsable(false)]
//        public TreeNode LastNode
//        {
//            get
//            {
//                if (childCount == 0)
//                {
//                    return null;
//                }

//                return children[childCount - 1];
//            }
//        }

//        //
//        // Summary:
//        //     Gets the zero-based depth of the tree node in the System.Windows.Forms.TreeView
//        //     control.
//        //
//        // Returns:
//        //     The zero-based depth of the tree node in the System.Windows.Forms.TreeView control.
//        [Browsable(false)]
//        public int Level
//        {
//            get
//            {
//                if (Parent == null)
//                {
//                    return 0;
//                }

//                return Parent.Level + 1;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the next sibling tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the next sibling tree node.
//        [Browsable(false)]
//        public TreeNode NextNode
//        {
//            get
//            {
//                if (index + 1 < parent.Nodes.Count)
//                {
//                    return parent.Nodes[index + 1];
//                }

//                return null;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the next visible tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the next visible tree node.
//        [Browsable(false)]
//        public TreeNode NextVisibleNode
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                if (treeView == null || treeView.IsDisposed)
//                {
//                    return null;
//                }

//                TreeNode firstVisibleParent = FirstVisibleParent;
//                if (firstVisibleParent != null)
//                {
//                    IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4362, 6, firstVisibleParent.Handle);
//                    if (intPtr != IntPtr.Zero)
//                    {
//                        return treeView.NodeFromHandle(intPtr);
//                    }
//                }

//                return null;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the font that is used to display the text on the tree node label.
//        //
//        // Returns:
//        //     The System.Drawing.Font that is used to display the text on the tree node label.
//        [Localizable(true)]
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeNodeFontDescr")]
//        [DefaultValue(null)]
//        public Font NodeFont
//        {
//            get
//            {
//                if (propBag == null)
//                {
//                    return null;
//                }

//                return propBag.Font;
//            }
//            set
//            {
//                Font nodeFont = NodeFont;
//                if (value == null)
//                {
//                    if (propBag != null)
//                    {
//                        propBag.Font = null;
//                        RemovePropBagIfEmpty();
//                    }

//                    if (nodeFont != null)
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//                else
//                {
//                    if (propBag == null)
//                    {
//                        propBag = new OwnerDrawPropertyBag();
//                    }

//                    propBag.Font = value;
//                    if (!value.Equals(nodeFont))
//                    {
//                        InvalidateHostTree();
//                    }
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets the collection of System.Windows.Forms.TreeNode objects assigned to the
//        //     current tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNodeCollection that represents the tree nodes assigned
//        //     to the current tree node.
//        [ListBindable(false)]
//        [Browsable(false)]
//        public TreeNodeCollection Nodes
//        {
//            get
//            {
//                if (nodes == null)
//                {
//                    nodes = new TreeNodeCollection(this);
//                }

//                return nodes;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the parent tree node of the current tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the parent of the current tree
//        //     node.
//        [Browsable(false)]
//        public TreeNode Parent
//        {
//            get
//            {
//                TreeView treeView = TreeView;
//                if (treeView != null && parent == treeView.root)
//                {
//                    return null;
//                }

//                return parent;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the previous sibling tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the previous sibling tree node.
//        [Browsable(false)]
//        public TreeNode PrevNode
//        {
//            get
//            {
//                int num = index;
//                int fixedIndex = parent.Nodes.FixedIndex;
//                if (fixedIndex > 0)
//                {
//                    num = fixedIndex;
//                }

//                if (num > 0 && num <= parent.Nodes.Count)
//                {
//                    return parent.Nodes[num - 1];
//                }

//                return null;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the previous visible tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the previous visible tree node.
//        [Browsable(false)]
//        public TreeNode PrevVisibleNode
//        {
//            get
//            {
//                TreeNode firstVisibleParent = FirstVisibleParent;
//                TreeView treeView = TreeView;
//                if (firstVisibleParent != null)
//                {
//                    if (treeView == null || treeView.IsDisposed)
//                    {
//                        return null;
//                    }

//                    IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4362, 7, firstVisibleParent.Handle);
//                    if (intPtr != IntPtr.Zero)
//                    {
//                        return treeView.NodeFromHandle(intPtr);
//                    }
//                }

//                return null;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the image list index value of the image that is displayed when the
//        //     tree node is in the selected state.
//        //
//        // Returns:
//        //     A zero-based index value that represents the image position in an System.Windows.Forms.ImageList.
//        [Localizable(true)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeSelectedImageIndexDescr")]
//        [TypeConverter(typeof(TreeViewImageIndexConverter))]
//        [DefaultValue(-1)]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RelatedImageList("TreeView.ImageList")]
//        public int SelectedImageIndex
//        {
//            get
//            {
//                return SelectedImageIndexer.Index;
//            }
//            set
//            {
//                SelectedImageIndexer.Index = value;
//                UpdateNode(32);
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the key of the image displayed in the tree node when it is in a
//        //     selected state.
//        //
//        // Returns:
//        //     The key of the image displayed when the tree node is in a selected state.
//        [Localizable(true)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeSelectedImageKeyDescr")]
//        [TypeConverter(typeof(TreeViewImageKeyConverter))]
//        [DefaultValue("")]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RelatedImageList("TreeView.ImageList")]
//        public string SelectedImageKey
//        {
//            get
//            {
//                return SelectedImageIndexer.Key;
//            }
//            set
//            {
//                SelectedImageIndexer.Key = value;
//                UpdateNode(32);
//            }
//        }

//        internal int State
//        {
//            get
//            {
//                if (handle == IntPtr.Zero)
//                {
//                    return 0;
//                }

//                TreeView treeView = TreeView;
//                if (treeView == null || treeView.IsDisposed)
//                {
//                    return 0;
//                }

//                NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
//                lParam.hItem = Handle;
//                lParam.mask = 24;
//                lParam.stateMask = 34;
//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_GETITEM, 0, ref lParam);
//                return lParam.state;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the key of the image that is used to indicate the state of the System.Windows.Forms.TreeNode
//        //     when the parent System.Windows.Forms.TreeView has its System.Windows.Forms.TreeView.CheckBoxes
//        //     property set to false.
//        //
//        // Returns:
//        //     The key of the image that is used to indicate the state of the System.Windows.Forms.TreeNode.
//        [Localizable(true)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeStateImageKeyDescr")]
//        [TypeConverter(typeof(ImageKeyConverter))]
//        [DefaultValue("")]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [RelatedImageList("TreeView.StateImageList")]
//        public string StateImageKey
//        {
//            get
//            {
//                return StateImageIndexer.Key;
//            }
//            set
//            {
//                if (StateImageIndexer.Key != value)
//                {
//                    StateImageIndexer.Key = value;
//                    if (treeView != null && !treeView.CheckBoxes)
//                    {
//                        UpdateNode(8);
//                    }
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the index of the image that is used to indicate the state of the
//        //     System.Windows.Forms.TreeNode when the parent System.Windows.Forms.TreeView has
//        //     its System.Windows.Forms.TreeView.CheckBoxes property set to false.
//        //
//        // Returns:
//        //     The index of the image that is used to indicate the state of the System.Windows.Forms.TreeNode.
//        //
//        // Exceptions:
//        //   T:System.ArgumentOutOfRangeException:
//        //     The specified index is less than -1 or greater than 14.
//        [Localizable(true)]
//        [TypeConverter(typeof(NoneExcludedImageIndexConverter))]
//        [DefaultValue(-1)]
//        [SRCategory("CatBehavior")]
//        [SRDescription("TreeNodeStateImageIndexDescr")]
//        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [RefreshProperties(RefreshProperties.Repaint)]
//        [RelatedImageList("TreeView.StateImageList")]
//        public int StateImageIndex
//        {
//            get
//            {
//                if (treeView != null && treeView.StateImageList != null)
//                {
//                    return StateImageIndexer.Index;
//                }

//                return -1;
//            }
//            set
//            {
//                if (value < -1 || value > 14)
//                {
//                    throw new ArgumentOutOfRangeException("StateImageIndex", SR.GetString("InvalidArgument", "StateImageIndex", value.ToString(CultureInfo.CurrentCulture)));
//                }

//                StateImageIndexer.Index = value;
//                if (treeView != null && !treeView.CheckBoxes)
//                {
//                    UpdateNode(8);
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the object that contains data about the tree node.
//        //
//        // Returns:
//        //     An System.Object that contains data about the tree node. The default is null.
//        [SRCategory("CatData")]
//        [Localizable(false)]
//        [Bindable(true)]
//        [SRDescription("ControlTagDescr")]
//        [DefaultValue(null)]
//        [TypeConverter(typeof(StringConverter))]
//        public object Tag
//        {
//            get
//            {
//                return userData;
//            }
//            set
//            {
//                userData = value;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the text displayed in the label of the tree node.
//        //
//        // Returns:
//        //     The text displayed in the label of the tree node.
//        [Localizable(true)]
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeTextDescr")]
//        public string Text
//        {
//            get
//            {
//                if (text != null)
//                {
//                    return text;
//                }

//                return "";
//            }
//            set
//            {
//                text = value;
//                UpdateNode(1);
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the text that appears when the mouse pointer hovers over a System.Windows.Forms.TreeNode.
//        //
//        // Returns:
//        //     Gets the text that appears when the mouse pointer hovers over a System.Windows.Forms.TreeNode.
//        [Localizable(false)]
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeToolTipTextDescr")]
//        [DefaultValue("")]
//        public string ToolTipText
//        {
//            get
//            {
//                return toolTipText;
//            }
//            set
//            {
//                toolTipText = value;
//            }
//        }

//        //
//        // Summary:
//        //     Gets or sets the name of the tree node.
//        //
//        // Returns:
//        //     A System.String that represents the name of the tree node.
//        [SRCategory("CatAppearance")]
//        [SRDescription("TreeNodeNodeNameDescr")]
//        public string Name
//        {
//            get
//            {
//                if (name != null)
//                {
//                    return name;
//                }

//                return "";
//            }
//            set
//            {
//                name = value;
//            }
//        }

//        //
//        // Summary:
//        //     Gets the parent tree view that the tree node is assigned to.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeView that represents the parent tree view that the
//        //     tree node is assigned to, or null if the node has not been assigned to a tree
//        //     view.
//        [Browsable(false)]
//        public TreeView TreeView
//        {
//            get
//            {
//                if (treeView == null)
//                {
//                    treeView = FindTreeView();
//                }

//                return treeView;
//            }
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class.
//        public TreeNode()
//        {
//            treeNodeState = default(BitVector32);
//        }

//        internal TreeNode(TreeView treeView)
//            : this()
//        {
//            this.treeView = treeView;
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class with the
//        //     specified label text.
//        //
//        // Parameters:
//        //   text:
//        //     The label System.Windows.Forms.TreeNode.Text of the new tree node.
//        public TreeNode(string text)
//            : this()
//        {
//            this.text = text;
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class with the
//        //     specified label text and child tree nodes.
//        //
//        // Parameters:
//        //   text:
//        //     The label System.Windows.Forms.TreeNode.Text of the new tree node.
//        //
//        //   children:
//        //     An array of child System.Windows.Forms.TreeNode objects.
//        public TreeNode(string text, TreeNode[] children)
//            : this()
//        {
//            this.text = text;
//            Nodes.AddRange(children);
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class with the
//        //     specified label text and images to display when the tree node is in a selected
//        //     and unselected state.
//        //
//        // Parameters:
//        //   text:
//        //     The label System.Windows.Forms.TreeNode.Text of the new tree node.
//        //
//        //   imageIndex:
//        //     The index value of System.Drawing.Image to display when the tree node is unselected.
//        //
//        //   selectedImageIndex:
//        //     The index value of System.Drawing.Image to display when the tree node is selected.
//        public TreeNode(string text, int imageIndex, int selectedImageIndex)
//            : this()
//        {
//            this.text = text;
//            ImageIndexer.Index = imageIndex;
//            SelectedImageIndexer.Index = selectedImageIndex;
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class with the
//        //     specified label text, child tree nodes, and images to display when the tree node
//        //     is in a selected and unselected state.
//        //
//        // Parameters:
//        //   text:
//        //     The label System.Windows.Forms.TreeNode.Text of the new tree node.
//        //
//        //   imageIndex:
//        //     The index value of System.Drawing.Image to display when the tree node is unselected.
//        //
//        //   selectedImageIndex:
//        //     The index value of System.Drawing.Image to display when the tree node is selected.
//        //
//        //   children:
//        //     An array of child System.Windows.Forms.TreeNode objects.
//        public TreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
//            : this()
//        {
//            this.text = text;
//            ImageIndexer.Index = imageIndex;
//            SelectedImageIndexer.Index = selectedImageIndex;
//            Nodes.AddRange(children);
//        }

//        //
//        // Summary:
//        //     Initializes a new instance of the System.Windows.Forms.TreeNode class using the
//        //     specified serialization information and context.
//        //
//        // Parameters:
//        //   serializationInfo:
//        //     The System.Runtime.Serialization.SerializationInfo that contains the data to
//        //     deserialize the class.
//        //
//        //   context:
//        //     The System.Runtime.Serialization.StreamingContext that contains the source and
//        //     destination of the serialized stream.
//        protected TreeNode(SerializationInfo serializationInfo, StreamingContext context)
//            : this()
//        {
//            Deserialize(serializationInfo, context);
//        }

//        internal int AddSorted(TreeNode node)
//        {
//            int result = 0;
//            string @string = node.Text;
//            TreeView treeView = TreeView;
//            if (childCount > 0)
//            {
//                if (treeView.TreeViewNodeSorter == null)
//                {
//                    CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
//                    if (compareInfo.Compare(children[childCount - 1].Text, @string) <= 0)
//                    {
//                        result = childCount;
//                    }
//                    else
//                    {
//                        int num = 0;
//                        int num2 = childCount;
//                        while (num < num2)
//                        {
//                            int num3 = (num + num2) / 2;
//                            if (compareInfo.Compare(children[num3].Text, @string) <= 0)
//                            {
//                                num = num3 + 1;
//                            }
//                            else
//                            {
//                                num2 = num3;
//                            }
//                        }

//                        result = num;
//                    }
//                }
//                else
//                {
//                    IComparer treeViewNodeSorter = treeView.TreeViewNodeSorter;
//                    int num = 0;
//                    int num2 = childCount;
//                    while (num < num2)
//                    {
//                        int num3 = (num + num2) / 2;
//                        if (treeViewNodeSorter.Compare(children[num3], node) <= 0)
//                        {
//                            num = num3 + 1;
//                        }
//                        else
//                        {
//                            num2 = num3;
//                        }
//                    }

//                    result = num;
//                }
//            }

//            node.SortChildren(treeView);
//            InsertNodeAt(result, node);
//            return result;
//        }

//        //
//        // Summary:
//        //     Returns the tree node with the specified handle and assigned to the specified
//        //     tree view control.
//        //
//        // Parameters:
//        //   tree:
//        //     The System.Windows.Forms.TreeView that contains the tree node.
//        //
//        //   handle:
//        //     The handle of the tree node.
//        //
//        // Returns:
//        //     A System.Windows.Forms.TreeNode that represents the tree node assigned to the
//        //     specified System.Windows.Forms.TreeView control with the specified handle.
//        public static TreeNode FromHandle(TreeView tree, IntPtr handle)
//        {
//            IntSecurity.ControlFromHandleOrLocation.Demand();
//            return tree.NodeFromHandle(handle);
//        }

//        private void SortChildren(TreeView parentTreeView)
//        {
//            if (childCount <= 0)
//            {
//                return;
//            }

//            TreeNode[] array = new TreeNode[childCount];
//            if (parentTreeView == null || parentTreeView.TreeViewNodeSorter == null)
//            {
//                CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
//                for (int i = 0; i < childCount; i++)
//                {
//                    int num = -1;
//                    for (int j = 0; j < childCount; j++)
//                    {
//                        if (children[j] != null)
//                        {
//                            if (num == -1)
//                            {
//                                num = j;
//                            }
//                            else if (compareInfo.Compare(children[j].Text, children[num].Text) <= 0)
//                            {
//                                num = j;
//                            }
//                        }
//                    }

//                    array[i] = children[num];
//                    children[num] = null;
//                    array[i].index = i;
//                    array[i].SortChildren(parentTreeView);
//                }

//                children = array;
//                return;
//            }

//            IComparer treeViewNodeSorter = parentTreeView.TreeViewNodeSorter;
//            for (int k = 0; k < childCount; k++)
//            {
//                int num2 = -1;
//                for (int l = 0; l < childCount; l++)
//                {
//                    if (children[l] != null)
//                    {
//                        if (num2 == -1)
//                        {
//                            num2 = l;
//                        }
//                        else if (treeViewNodeSorter.Compare(children[l], children[num2]) <= 0)
//                        {
//                            num2 = l;
//                        }
//                    }
//                }

//                array[k] = children[num2];
//                children[num2] = null;
//                array[k].index = k;
//                array[k].SortChildren(parentTreeView);
//            }

//            children = array;
//        }

//        //
//        // Summary:
//        //     Initiates the editing of the tree node label.
//        //
//        // Exceptions:
//        //   T:System.InvalidOperationException:
//        //     System.Windows.Forms.TreeView.LabelEdit is set to false.
//        public void BeginEdit()
//        {
//            if (handle != IntPtr.Zero)
//            {
//                TreeView treeView = TreeView;
//                if (!treeView.LabelEdit)
//                {
//                    throw new InvalidOperationException(SR.GetString("TreeNodeBeginEditFailed"));
//                }

//                if (!treeView.Focused)
//                {
//                    treeView.FocusInternal();
//                }

//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_EDITLABEL, 0, handle);
//            }
//        }

//        internal void Clear()
//        {
//            bool flag = false;
//            TreeView treeView = TreeView;
//            try
//            {
//                if (treeView != null)
//                {
//                    treeView.nodesCollectionClear = true;
//                    if (treeView != null && childCount > 200)
//                    {
//                        flag = true;
//                        treeView.BeginUpdate();
//                    }
//                }

//                while (childCount > 0)
//                {
//                    children[childCount - 1].Remove(notify: true);
//                }

//                children = null;
//                if (treeView != null && flag)
//                {
//                    treeView.EndUpdate();
//                }
//            }
//            finally
//            {
//                if (treeView != null)
//                {
//                    treeView.nodesCollectionClear = false;
//                }

//                nodesCleared = true;
//            }
//        }

//        //
//        // Summary:
//        //     Copies the tree node and the entire subtree rooted at this tree node.
//        //
//        // Returns:
//        //     The System.Object that represents the cloned System.Windows.Forms.TreeNode.
//        public virtual object Clone()
//        {
//            Type type = GetType();
//            TreeNode treeNode = null;
//            treeNode = ((!(type == typeof(TreeNode))) ? ((TreeNode)Activator.CreateInstance(type)) : new TreeNode(text, ImageIndexer.Index, SelectedImageIndexer.Index));
//            treeNode.Text = text;
//            treeNode.Name = name;
//            treeNode.ImageIndexer.Index = ImageIndexer.Index;
//            treeNode.SelectedImageIndexer.Index = SelectedImageIndexer.Index;
//            treeNode.StateImageIndexer.Index = StateImageIndexer.Index;
//            treeNode.ToolTipText = toolTipText;
//            treeNode.ContextMenu = contextMenu;
//            treeNode.ContextMenuStrip = contextMenuStrip;
//            if (!string.IsNullOrEmpty(ImageIndexer.Key))
//            {
//                treeNode.ImageIndexer.Key = ImageIndexer.Key;
//            }

//            if (!string.IsNullOrEmpty(SelectedImageIndexer.Key))
//            {
//                treeNode.SelectedImageIndexer.Key = SelectedImageIndexer.Key;
//            }

//            if (!string.IsNullOrEmpty(StateImageIndexer.Key))
//            {
//                treeNode.StateImageIndexer.Key = StateImageIndexer.Key;
//            }

//            if (childCount > 0)
//            {
//                treeNode.children = new TreeNode[childCount];
//                for (int i = 0; i < childCount; i++)
//                {
//                    treeNode.Nodes.Add((TreeNode)children[i].Clone());
//                }
//            }

//            if (propBag != null)
//            {
//                treeNode.propBag = OwnerDrawPropertyBag.Copy(propBag);
//            }

//            treeNode.Checked = Checked;
//            treeNode.Tag = Tag;
//            return treeNode;
//        }

//        private void CollapseInternal(bool ignoreChildren)
//        {
//            TreeView treeView = TreeView;
//            bool flag = false;
//            collapseOnRealization = false;
//            expandOnRealization = false;
//            if (treeView == null || !treeView.IsHandleCreated)
//            {
//                collapseOnRealization = true;
//                return;
//            }

//            if (ignoreChildren)
//            {
//                DoCollapse(treeView);
//            }
//            else
//            {
//                if (!ignoreChildren && childCount > 0)
//                {
//                    for (int i = 0; i < childCount; i++)
//                    {
//                        if (treeView.SelectedNode == children[i])
//                        {
//                            flag = true;
//                        }

//                        children[i].DoCollapse(treeView);
//                        children[i].Collapse();
//                    }
//                }

//                DoCollapse(treeView);
//            }

//            if (flag)
//            {
//                treeView.SelectedNode = this;
//            }

//            treeView.Invalidate();
//            collapseOnRealization = false;
//        }

//        //
//        // Summary:
//        //     Collapses the System.Windows.Forms.TreeNode and optionally collapses its children.
//        //
//        // Parameters:
//        //   ignoreChildren:
//        //     true to leave the child nodes in their current state; false to collapse the child
//        //     nodes.
//        public void Collapse(bool ignoreChildren)
//        {
//            CollapseInternal(ignoreChildren);
//        }

//        //
//        // Summary:
//        //     Collapses the tree node.
//        public void Collapse()
//        {
//            CollapseInternal(ignoreChildren: false);
//        }

//        private void DoCollapse(TreeView tv)
//        {
//            if (((uint)State & 0x20u) != 0)
//            {
//                TreeViewCancelEventArgs treeViewCancelEventArgs = new TreeViewCancelEventArgs(this, cancel: false, TreeViewAction.Collapse);
//                tv.OnBeforeCollapse(treeViewCancelEventArgs);
//                if (!treeViewCancelEventArgs.Cancel)
//                {
//                    UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), 4354, 1, Handle);
//                    tv.OnAfterCollapse(new TreeViewEventArgs(this));
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Loads the state of the System.Windows.Forms.TreeNode from the specified System.Runtime.Serialization.SerializationInfo.
//        //
//        // Parameters:
//        //   serializationInfo:
//        //     The System.Runtime.Serialization.SerializationInfo that describes the System.Windows.Forms.TreeNode.
//        //
//        //   context:
//        //     The System.Runtime.Serialization.StreamingContext that indicates the state of
//        //     the stream during deserialization.
//        protected virtual void Deserialize(SerializationInfo serializationInfo, StreamingContext context)
//        {
//            int num = 0;
//            int num2 = -1;
//            string text = null;
//            int num3 = -1;
//            string text2 = null;
//            int num4 = -1;
//            string text3 = null;
//            SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
//            while (enumerator.MoveNext())
//            {
//                SerializationEntry current = enumerator.Current;
//                switch (current.Name)
//                {
//                    case "PropBag":
//                        propBag = (OwnerDrawPropertyBag)serializationInfo.GetValue(current.Name, typeof(OwnerDrawPropertyBag));
//                        break;
//                    case "Text":
//                        Text = serializationInfo.GetString(current.Name);
//                        break;
//                    case "ToolTipText":
//                        ToolTipText = serializationInfo.GetString(current.Name);
//                        break;
//                    case "Name":
//                        Name = serializationInfo.GetString(current.Name);
//                        break;
//                    case "IsChecked":
//                        CheckedStateInternal = serializationInfo.GetBoolean(current.Name);
//                        break;
//                    case "ImageIndex":
//                        num2 = serializationInfo.GetInt32(current.Name);
//                        break;
//                    case "SelectedImageIndex":
//                        num3 = serializationInfo.GetInt32(current.Name);
//                        break;
//                    case "ImageKey":
//                        text = serializationInfo.GetString(current.Name);
//                        break;
//                    case "SelectedImageKey":
//                        text2 = serializationInfo.GetString(current.Name);
//                        break;
//                    case "StateImageKey":
//                        text3 = serializationInfo.GetString(current.Name);
//                        break;
//                    case "StateImageIndex":
//                        num4 = serializationInfo.GetInt32(current.Name);
//                        break;
//                    case "ChildCount":
//                        num = serializationInfo.GetInt32(current.Name);
//                        break;
//                    case "UserData":
//                        userData = current.Value;
//                        break;
//                }
//            }

//            if (text != null)
//            {
//                ImageKey = text;
//            }
//            else if (num2 != -1)
//            {
//                ImageIndex = num2;
//            }

//            if (text2 != null)
//            {
//                SelectedImageKey = text2;
//            }
//            else if (num3 != -1)
//            {
//                SelectedImageIndex = num3;
//            }

//            if (text3 != null)
//            {
//                StateImageKey = text3;
//            }
//            else if (num4 != -1)
//            {
//                StateImageIndex = num4;
//            }

//            if (num > 0)
//            {
//                TreeNode[] array = new TreeNode[num];
//                for (int i = 0; i < num; i++)
//                {
//                    array[i] = (TreeNode)serializationInfo.GetValue("children" + i, typeof(TreeNode));
//                }

//                Nodes.AddRange(array);
//            }
//        }

//        //
//        // Summary:
//        //     Ends the editing of the tree node label.
//        //
//        // Parameters:
//        //   cancel:
//        //     true if the editing of the tree node label text was canceled without being saved;
//        //     otherwise, false.
//        public void EndEdit(bool cancel)
//        {
//            TreeView treeView = TreeView;
//            if (treeView != null && !treeView.IsDisposed)
//            {
//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4374, cancel ? 1 : 0, 0);
//            }
//        }

//        internal void EnsureCapacity(int num)
//        {
//            int num2 = num;
//            if (num2 < 4)
//            {
//                num2 = 4;
//            }

//            if (children == null)
//            {
//                children = new TreeNode[num2];
//            }
//            else if (childCount + num > children.Length)
//            {
//                int num3 = childCount + num;
//                if (num == 1)
//                {
//                    num3 = childCount * 2;
//                }

//                TreeNode[] destinationArray = new TreeNode[num3];
//                Array.Copy(children, 0, destinationArray, 0, childCount);
//                children = destinationArray;
//            }
//        }

//        private void EnsureStateImageValue()
//        {
//            if (treeView != null && treeView.CheckBoxes && treeView.StateImageList != null)
//            {
//                if (!string.IsNullOrEmpty(StateImageKey))
//                {
//                    StateImageIndex = (Checked ? 1 : 0);
//                    StateImageKey = treeView.StateImageList.Images.Keys[StateImageIndex];
//                }
//                else
//                {
//                    StateImageIndex = (Checked ? 1 : 0);
//                }
//            }
//        }

//        //
//        // Summary:
//        //     Ensures that the tree node is visible, expanding tree nodes and scrolling the
//        //     tree view control as necessary.
//        public void EnsureVisible()
//        {
//            TreeView treeView = TreeView;
//            if (treeView != null && !treeView.IsDisposed)
//            {
//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4372, 0, Handle);
//            }
//        }

//        //
//        // Summary:
//        //     Expands the tree node.
//        public void Expand()
//        {
//            TreeView treeView = TreeView;
//            if (treeView == null || !treeView.IsHandleCreated)
//            {
//                expandOnRealization = true;
//                return;
//            }

//            ResetExpandedState(treeView);
//            if (!IsExpanded)
//            {
//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4354, 2, Handle);
//            }

//            expandOnRealization = false;
//        }

//        //
//        // Summary:
//        //     Expands all the child tree nodes.
//        public void ExpandAll()
//        {
//            Expand();
//            for (int i = 0; i < childCount; i++)
//            {
//                children[i].ExpandAll();
//            }
//        }

//        internal TreeView FindTreeView()
//        {
//            TreeNode treeNode = this;
//            while (treeNode.parent != null)
//            {
//                treeNode = treeNode.parent;
//            }

//            return treeNode.treeView;
//        }

//        private void GetFullPath(StringBuilder path, string pathSeparator)
//        {
//            if (parent != null)
//            {
//                parent.GetFullPath(path, pathSeparator);
//                if (parent.parent != null)
//                {
//                    path.Append(pathSeparator);
//                }

//                path.Append(text);
//            }
//        }

//        //
//        // Summary:
//        //     Returns the number of child tree nodes.
//        //
//        // Parameters:
//        //   includeSubTrees:
//        //     true if the resulting count includes all tree nodes indirectly rooted at this
//        //     tree node; otherwise, false.
//        //
//        // Returns:
//        //     The number of child tree nodes assigned to the System.Windows.Forms.TreeNode.Nodes
//        //     collection.
//        public int GetNodeCount(bool includeSubTrees)
//        {
//            int num = childCount;
//            if (includeSubTrees)
//            {
//                for (int i = 0; i < childCount; i++)
//                {
//                    num += children[i].GetNodeCount(includeSubTrees: true);
//                }
//            }

//            return num;
//        }

//        internal void InsertNodeAt(int index, TreeNode node)
//        {
//            EnsureCapacity(1);
//            node.parent = this;
//            node.index = index;
//            for (int num = childCount; num > index; num--)
//            {
//                (children[num] = children[num - 1]).index = num;
//            }

//            children[index] = node;
//            childCount++;
//            node.Realize(insertFirst: false);
//            if (TreeView != null && node == TreeView.selectedNode)
//            {
//                TreeView.SelectedNode = node;
//            }
//        }

//        private void InvalidateHostTree()
//        {
//            if (treeView != null && treeView.IsHandleCreated)
//            {
//                treeView.Invalidate();
//            }
//        }

//        internal void Realize(bool insertFirst)
//        {
//            TreeView treeView = TreeView;
//            if (treeView == null || !treeView.IsHandleCreated || treeView.IsDisposed)
//            {
//                return;
//            }

//            if (parent != null)
//            {
//                if (treeView.InvokeRequired)
//                {
//                    throw new InvalidOperationException(SR.GetString("InvalidCrossThreadControlCall"));
//                }

//                NativeMethods.TV_INSERTSTRUCT lParam = default(NativeMethods.TV_INSERTSTRUCT);
//                lParam.item_mask = insertMask;
//                lParam.hParent = parent.handle;
//                TreeNode prevNode = PrevNode;
//                if (insertFirst || prevNode == null)
//                {
//                    lParam.hInsertAfter = (IntPtr)(-65535);
//                }
//                else
//                {
//                    lParam.hInsertAfter = prevNode.handle;
//                }

//                lParam.item_pszText = Marshal.StringToHGlobalAuto(text);
//                lParam.item_iImage = ((ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : ImageIndexer.ActualIndex);
//                lParam.item_iSelectedImage = ((SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : SelectedImageIndexer.ActualIndex);
//                lParam.item_mask = 1;
//                lParam.item_stateMask = 0;
//                lParam.item_state = 0;
//                if (treeView.CheckBoxes)
//                {
//                    lParam.item_mask |= 8;
//                    lParam.item_stateMask |= 61440;
//                    lParam.item_state |= (CheckedInternal ? 8192 : 4096);
//                }
//                else if (treeView.StateImageList != null && StateImageIndexer.ActualIndex >= 0)
//                {
//                    lParam.item_mask |= 8;
//                    lParam.item_stateMask = 61440;
//                    lParam.item_state = StateImageIndexer.ActualIndex + 1 << 12;
//                }

//                if (lParam.item_iImage >= 0)
//                {
//                    lParam.item_mask |= 2;
//                }

//                if (lParam.item_iSelectedImage >= 0)
//                {
//                    lParam.item_mask |= 32;
//                }

//                bool flag = false;
//                IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4367, 0, 0);
//                if (intPtr != IntPtr.Zero)
//                {
//                    flag = true;
//                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4374, 0, 0);
//                }

//                handle = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_INSERTITEM, 0, ref lParam);
//                treeView.nodeTable[handle] = this;
//                UpdateNode(4);
//                Marshal.FreeHGlobal(lParam.item_pszText);
//                if (flag)
//                {
//                    UnsafeNativeMethods.PostMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_EDITLABEL, IntPtr.Zero, handle);
//                }

//                SafeNativeMethods.InvalidateRect(new HandleRef(treeView, treeView.Handle), null, erase: false);
//                if (parent.nodesCleared && (insertFirst || prevNode == null) && !treeView.Scrollable)
//                {
//                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 11, 1, 0);
//                    nodesCleared = false;
//                }
//            }

//            for (int num = childCount - 1; num >= 0; num--)
//            {
//                children[num].Realize(insertFirst: true);
//            }

//            if (expandOnRealization)
//            {
//                Expand();
//            }

//            if (collapseOnRealization)
//            {
//                Collapse();
//            }
//        }

//        //
//        // Summary:
//        //     Removes the current tree node from the tree view control.
//        public void Remove()
//        {
//            Remove(notify: true);
//        }

//        internal void Remove(bool notify)
//        {
//            bool isExpanded = IsExpanded;
//            for (int i = 0; i < childCount; i++)
//            {
//                children[i].Remove(notify: false);
//            }

//            if (notify && parent != null)
//            {
//                for (int j = index; j < parent.childCount - 1; j++)
//                {
//                    (parent.children[j] = parent.children[j + 1]).index = j;
//                }

//                parent.children[parent.childCount - 1] = null;
//                parent.childCount--;
//                parent = null;
//            }

//            expandOnRealization = isExpanded;
//            TreeView treeView = TreeView;
//            if (treeView == null || treeView.IsDisposed)
//            {
//                return;
//            }

//            if (handle != IntPtr.Zero)
//            {
//                if (notify && treeView.IsHandleCreated)
//                {
//                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4353, 0, handle);
//                }

//                this.treeView.nodeTable.Remove(handle);
//                handle = IntPtr.Zero;
//            }

//            this.treeView = null;
//        }

//        private void RemovePropBagIfEmpty()
//        {
//            if (propBag != null && propBag.IsEmpty())
//            {
//                propBag = null;
//            }
//        }

//        private void ResetExpandedState(TreeView tv)
//        {
//            NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
//            lParam.mask = 24;
//            lParam.hItem = handle;
//            lParam.stateMask = 64;
//            lParam.state = 0;
//            UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
//        }

//        private bool ShouldSerializeBackColor()
//        {
//            return BackColor != Color.Empty;
//        }

//        private bool ShouldSerializeForeColor()
//        {
//            return ForeColor != Color.Empty;
//        }

//        //
//        // Summary:
//        //     Saves the state of the System.Windows.Forms.TreeNode to the specified System.Runtime.Serialization.SerializationInfo.
//        //
//        // Parameters:
//        //   si:
//        //     The System.Runtime.Serialization.SerializationInfo that describes the System.Windows.Forms.TreeNode.
//        //
//        //   context:
//        //     The System.Runtime.Serialization.StreamingContext that indicates the state of
//        //     the stream during serialization
//        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
//        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
//        protected virtual void Serialize(SerializationInfo si, StreamingContext context)
//        {
//            if (propBag != null)
//            {
//                si.AddValue("PropBag", propBag, typeof(OwnerDrawPropertyBag));
//            }

//            si.AddValue("Text", text);
//            si.AddValue("ToolTipText", toolTipText);
//            si.AddValue("Name", Name);
//            si.AddValue("IsChecked", treeNodeState[1]);
//            si.AddValue("ImageIndex", ImageIndexer.Index);
//            si.AddValue("ImageKey", ImageIndexer.Key);
//            si.AddValue("SelectedImageIndex", SelectedImageIndexer.Index);
//            si.AddValue("SelectedImageKey", SelectedImageIndexer.Key);
//            if (treeView != null && treeView.StateImageList != null)
//            {
//                si.AddValue("StateImageIndex", StateImageIndexer.Index);
//            }

//            if (treeView != null && treeView.StateImageList != null)
//            {
//                si.AddValue("StateImageKey", StateImageIndexer.Key);
//            }

//            si.AddValue("ChildCount", childCount);
//            if (childCount > 0)
//            {
//                for (int i = 0; i < childCount; i++)
//                {
//                    si.AddValue("children" + i, children[i], typeof(TreeNode));
//                }
//            }

//            if (userData != null && userData.GetType().IsSerializable)
//            {
//                si.AddValue("UserData", userData, userData.GetType());
//            }
//        }

//        //
//        // Summary:
//        //     Toggles the tree node to either the expanded or collapsed state.
//        public void Toggle()
//        {
//            if (IsExpanded)
//            {
//                Collapse();
//            }
//            else
//            {
//                Expand();
//            }
//        }

//        //
//        // Summary:
//        //     Returns a string that represents the current object.
//        //
//        // Returns:
//        //     A string that represents the current object.
//        public override string ToString()
//        {
//            return "TreeNode: " + ((text == null) ? "" : text);
//        }

//        private void UpdateNode(int mask)
//        {
//            if (handle == IntPtr.Zero)
//            {
//                return;
//            }

//            TreeView treeView = TreeView;
//            NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
//            lParam.mask = 0x10 | mask;
//            lParam.hItem = handle;
//            if (((uint)mask & (true ? 1u : 0u)) != 0)
//            {
//                lParam.pszText = Marshal.StringToHGlobalAuto(text);
//            }

//            if (((uint)mask & 2u) != 0)
//            {
//                lParam.iImage = ((ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : ImageIndexer.ActualIndex);
//            }

//            if (((uint)mask & 0x20u) != 0)
//            {
//                lParam.iSelectedImage = ((SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : SelectedImageIndexer.ActualIndex);
//            }

//            if (((uint)mask & 8u) != 0)
//            {
//                lParam.stateMask = 61440;
//                if (StateImageIndexer.ActualIndex != -1)
//                {
//                    lParam.state = StateImageIndexer.ActualIndex + 1 << 12;
//                }
//            }

//            if (((uint)mask & 4u) != 0)
//            {
//                lParam.lParam = handle;
//            }

//            UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
//            if (((uint)mask & (true ? 1u : 0u)) != 0)
//            {
//                Marshal.FreeHGlobal(lParam.pszText);
//                if (treeView.Scrollable)
//                {
//                    treeView.ForceScrollbarUpdate(delayed: false);
//                }
//            }
//        }

//        internal void UpdateImage()
//        {
//            TreeView treeView = TreeView;
//            if (!treeView.IsDisposed)
//            {
//                NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
//                lParam.mask = 18;
//                lParam.hItem = Handle;
//                lParam.iImage = Math.Max(0, (ImageIndexer.ActualIndex >= treeView.ImageList.Images.Count) ? (treeView.ImageList.Images.Count - 1) : ImageIndexer.ActualIndex);
//                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
//            }
//        }

//        //
//        // Summary:
//        //     Populates a serialization information object with the data needed to serialize
//        //     the System.Windows.Forms.TreeNode.
//        //
//        // Parameters:
//        //   si:
//        //     A System.Runtime.Serialization.SerializationInfo that contains the data to serialize
//        //     the System.Windows.Forms.TreeNode.
//        //
//        //   context:
//        //     A System.Runtime.Serialization.StreamingContext that contains the destination
//        //     information for this serialization.
//        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
//        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
//        {
//            Serialize(si, context);
//        }
//    }
//}
//#if false // Decompilation log
//'13' items in cache
//------------------
//Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
//------------------
//Resolve: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Found single assembly: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Drawing.dll'
//------------------
//Resolve: 'System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Could not find by name: 'System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//------------------
//Resolve: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Found single assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Xml.dll'
//------------------
//Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
//------------------
//Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll'
//------------------
//Resolve: 'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Could not find by name: 'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//------------------
//Resolve: 'Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Could not find by name: 'Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//------------------
//Resolve: 'System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Found single assembly: 'System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Deployment.dll'
//------------------
//Resolve: 'System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//Could not find by name: 'System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//#endif

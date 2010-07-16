using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TreeView4;

namespace TreeView4
{
    class DisplayTreeView 
    {
        public LazyTreeView displayTree = new LazyTreeView();
        public LazyTreeNode AllNode { set; get;}
        private string displayNodeKey = "";
        private LazyTreeNode displayNodeParent;
        private int rootIndex;
        private Presenter presenter;
        

        public LazyTreeNode SelectedNode
        {
            get
            {
                return (displayTree.SelectedNode as LazyTreeNode);
            }

            set
            {

                displayTree.SelectedNode = value;
            }
         }

        public bool setDisplayNode(LazyTreeNode node)
        {
            displayTree.BeginUpdate();
            clearDisplay();

            TreeNode[] nodeRef = AllNode.Nodes.Find(node.Name, true);
            
            if(nodeRef.Count() < 1 && node.Name != AllNode.Name) return false;            
            
            if(node.Parent != null)
            {
                displayNodeParent = nodeRef[0].Parent as LazyTreeNode;
            }
            else
            {
                displayNodeParent = null;
            }

            if (node.Name == AllNode.Name) {
                nodeRef = new TreeNode[1];
                nodeRef[0] = AllNode; }
            
            rootIndex = nodeRef[0].Index;
            nodeRef[0].Remove();
            displayTree.Nodes.Add(nodeRef[0]);
            displayTree.Expand(nodeRef[0]);

            displayNodeKey = nodeRef[0].Name;
            displayTree.EndUpdate();
            return true;
        }

        public bool clearDisplay()
        {
            displayTree.BeginUpdate();
            if (displayNodeKey != "")
            {
                TreeNode[] displayNode = displayTree.Nodes.Find(displayNodeKey, true);
                if (displayNode.Count() > 0)
                {
                    displayTree.Nodes.Clear();
                    if (displayNodeParent == null)
                        AllNode = displayNode[0] as LazyTreeNode;
                    else
                    {
                        displayNodeParent.Nodes.Insert(rootIndex, displayNode[0]);    
                    }
                    
                }
                displayTree.EndUpdate();
                return true;

            }
            displayTree.EndUpdate();
            return false;
            
        }

        public void LoadTree(Presenter setPresenter)
        {
            LoadImageList();
            displayTree.BeginUpdate();
            presenter = setPresenter;
            displayTree.Nodes.Clear();
            AllNode = null;
            displayNodeKey = "";
            displayNodeParent = null;
            rootIndex = 0;


            displayTree.OnAfterFirstExpandNode += OnNodeExpand;
            

            LazyTreeNode root = new LazyTreeNode();
            root.Nodes.Clear();
            root.Text = "All";
            root.Name = "All";
            root.Tag = "All";
            
            LazyTreeNode newRootNode = null;

            #region Loading the Nodes with Special Root Labels

            foreach (RootNodes rootNode in Enum.GetValues(typeof(RootNodes)))
            {
                DataView dataView = presenter.GetNodeChildren(rootNode);

                if (dataView != null)
                {
                    string imgName = Enum.GetName(typeof (RootNodes), rootNode);
                    newRootNode = new LazyTreeNode
                    {
                        Name = Enum.GetName(typeof(RootNodes), rootNode),
                        Text = Enum.GetName(typeof(RootNodes), rootNode),
                        Tag = rootNode,
                       // SelectedImageKey = String.Format("TreeView4.Resources.{0}.bmp", imgName),
                       // ImageKey = String.Format("TreeView4.Resources.{0}.bmp", imgName)
            
                    };

                    if (newRootNode.Name == "WorkingSets")
                        newRootNode.Text = "Favorite Folders";

                    List<LazyTreeNode> children = DataViewToNodes(presenter.GetNodeChildren(rootNode));
                    newRootNode.Nodes.Clear();
                    newRootNode.Nodes.AddRange(children.ToArray());
                    newRootNode.IsLoaded = true;
                    root.Nodes.Add(newRootNode);
                }
            }

            #endregion

            root.IsLoaded = true;
            this.AllNode = root;
            this.setDisplayNode(root);
            this.SelectedNode = root;


           // displayTree.ShowLines = false;
           // displayTree.ShowPlusMinus = false;
            displayTree.EndUpdate();



        }
       
        public void ChangeSelectedNode(string ID)
        {

            displayTree.BeginUpdate();
            if(ID==null) return;
            clearDisplay();
            setDisplayNode(AllNode);
            
            TreeNode[] matchNodes = displayTree.Nodes.Find(ID, true);
            
            for(int i =0; i < matchNodes.Length; i++)
            {
                if (IsNodeInWorkingSet(matchNodes[i]) == true)
                {
                    matchNodes[i] = null;
                }
            }

            
            if(matchNodes.Length==0 || matchNodes[0]==null)
            {
                string parentID = presenter.GetNodeParent(ID);
                ChangeSelectedNode(parentID);
                matchNodes = AllNode.Nodes.Find(ID, true);

            }
            if(matchNodes.Length == 0) return;
            displayTree.Expand(matchNodes[0]);
            SelectedNode = matchNodes[0] as LazyTreeNode;
            displayTree.EndUpdate();
        }

        public static bool IsNodeInWorkingSet(TreeNode node)
        {
            if (node == null) return false;
            while (node.Parent != null)
            {
                RootNodes? rootType = node.Parent.Tag as RootNodes?;
                if (rootType == RootNodes.WorkingSets)
                    return true;
                else
                {
                    node = node.Parent;
                }
            }
            return false;
        }

        public void UpdateNode(RootNodes node)
        {
            
            TreeNode[] matchNodes = displayTree.Nodes.Find(Enum.GetName(typeof(RootNodes),node), true);
            if(matchNodes.Length == 0) return;
            matchNodes[0].Nodes.Clear();
            matchNodes[0].Nodes.AddRange(DataViewToNodes(presenter.GetNodeChildren(node)).ToArray());
            
        }

        public void UpdateNode(string ID, DataView dataView)
        {
            if(ID==null) return;

            TreeNode[] matchNodes = displayTree.Nodes.Find(ID, true);
            if(matchNodes.Count()==0)
            {
                matchNodes = AllNode.Nodes.Find(ID, true);

            }
            if (matchNodes.Count() == 0) return;

            matchNodes[0].Nodes.Clear();
            matchNodes[0].Nodes.AddRange(DataViewToNodes(presenter.GetNodeChildren(matchNodes[0].Name)).ToArray());
            
        }

        public LazyTreeNode GetNode(string key)
        {
            TreeNode[] matchNodes = displayTree.Nodes.Find(key, true);

            if (matchNodes.Length == 0)
            {
                matchNodes = AllNode.Nodes.Find(key, true);
            }

            if (matchNodes.Length == 1)
            {
                return matchNodes[0] as LazyTreeNode;
            }

            return null;
        }


        private ImageList imgList;

        private void LoadImageList()
        {
            imgList = new ImageList();
            imgList.ImageSize = new Size(16, 16);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            //displayTree.ImageList = imgList;


            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resNames = asm.GetManifestResourceNames();

            foreach(string s in resNames)
            {
                if(s.EndsWith(".bmp"))
                {
                    Stream imgStream = asm.GetManifestResourceStream(s);
                    if(imgStream!=null)
                    {
                        Bitmap bmp = Image.FromStream(imgStream) as Bitmap;
                        if(bmp!=null)
                        {
                            bmp.MakeTransparent(Color.Black);
                            imgList.Images.Add(s, bmp);
                        }
                    }
                }
            }


            



        }

        private List<LazyTreeNode> DataViewToNodes(DataView dataView)
        {

            var retTreeNodes = new List<LazyTreeNode>();
            foreach (DataRowView drv in dataView)
            {
                string typeName = Enum.GetName(typeof(NodeTypes), drv["Nodetype"]);
                var newTreeNode = new LazyTreeNode
                {
                    Name = drv["ID"].ToString(),
                    Text = drv["DisplayName"].ToString(),
                    Tag = (NodeTypes)drv["Nodetype"],
                    //               ImageKey = String.Format("TreeView4.Resources.{0}.bmp", typeName) ,
                    //                 SelectedImageKey = String.Format("TreeView4.Resources.{0}.bmp", typeName)


                };


                if ((NodeTypes)newTreeNode.Tag == NodeTypes.Page)
                {
                    newTreeNode.Nodes.Clear();
                    newTreeNode.IsLoaded = true;
                }

                retTreeNodes.Add(newTreeNode);
            }
            return retTreeNodes;
        }

        private void OnNodeExpand(object sender, DoWorkEventArgs e)
        {
            TreeViewEventArgs tvargs = (TreeViewEventArgs)e.Argument;
            LazyTreeNode lazyNode = (LazyTreeNode)tvargs.Node;

            var retList = new List<LazyTreeNode>();
            if (lazyNode != null)
            {
                if (lazyNode.Tag is NodeTypes)
                {
                    retList = DataViewToNodes(presenter.GetNodeChildren(lazyNode.Name));
                }
                else
                {
                    retList = DataViewToNodes(presenter.GetNodeChildren((RootNodes)lazyNode.Tag));
                }

            }
            e.Result = new object[] { retList };
        }




        
      
    }
}
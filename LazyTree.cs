// Class implements an load-on-demand Tree

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TreeView4
{
    public class LazyTreeView : TreeView
    {
        public delegate void LazyTreeNodeFirstExpand(object sender, DoWorkEventArgs e);

        public event LazyTreeNodeFirstExpand OnAfterFirstExpandNode;
        // Exectues the Nodes onAfterFirstExpand method if not already loaded.

        public void ExpandAll(TreeNode treeNode)
        {
            this.BeginUpdate();
            if (treeNode == null) return;

            Expand(treeNode);
            foreach (TreeNode tn in treeNode.Nodes)
            {
                ExpandAll(tn);
            }
            this.EndUpdate();
        }
        
        public void Expand(TreeNode treeNode)
        {
            this.BeginUpdate();
            if (treeNode == null) return;
            LazyTreeNode lazyNode = treeNode as LazyTreeNode;
            if (lazyNode == null)
                treeNode.Expand();
            else
            {
                OnAfterExpand(new TreeViewEventArgs(lazyNode));
                while (!lazyNode.IsLoaded)
                {
                    Application.DoEvents();
                }
                lazyNode.Expand();
            }
            this.EndUpdate();
        }
        
        public void RaiseOnAfterExpand(TreeViewEventArgs e)
        {
            OnAfterExpand(e);
            
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {

            var lazyNode = (e.Node as LazyTreeNode);
            if (lazyNode == null || lazyNode.IsLoaded || OnAfterFirstExpandNode == null) return;
            var bw = new BackgroundWorker();
            
            bw.DoWork += new DoWorkEventHandler(OnAfterFirstExpandNode);
            bw.RunWorkerCompleted += lazyNode.OnAfterFirstExpandComplete;
            bw.RunWorkerAsync(e);
            
            
        }

      
    }

    public class LazyTreeNode : TreeNode
    {
        public LazyTreeNode()
        {
            Nodes.Add(new DummyNode());
            IsLoaded = false;
        }

        public bool IsLoaded { get; set; }

        public override object Clone()
        {
            LazyTreeNode cloned = base.Clone() as LazyTreeNode;
            if (cloned == null)
            {
                return base.Clone();
            }
            else
            {
                cloned.IsLoaded = this.IsLoaded;
                return cloned;
            }
        }


        // this method should be overwritten by the deriving class to instruct the node on the specifics of what 
        // needs to be done. Add the child nodes to the e.results so that it can be added to the tree?
        public virtual void OnAfterFirstExpand(object sender, DoWorkEventArgs e)
        {
        }


        // This function adds the resultant nodes to the TreeView.
        // The resultant nodes must be put in e.Result by the above function
        public virtual void OnAfterFirstExpandComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                var oResult = e.Result as object[];
                if (oResult == null) return;
                var arrChildren = oResult[0] as List<LazyTreeNode>;
                if (arrChildren == null) return;
                TreeView.BeginUpdate();
                Nodes.Clear();
                Nodes.AddRange(arrChildren.ToArray());
                IsLoaded = true;
                TreeView.EndUpdate();
            }
        }

        #region Nested type: DummyNode

        public sealed class DummyNode : TreeNode
        {
            public DummyNode()
            {
                Text = "Loading ...";
                Name = "VIRTUALNODE";
                NodeFont = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Underline);
            }
        }

        #endregion
    }
}
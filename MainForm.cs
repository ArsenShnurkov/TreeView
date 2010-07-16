using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TreeView4
{
    public class MainForm : Form, ITreeView
    {

        #region Private Variables
        
        private ToolStripContainer toolContainer;
        private ToolStripComboBox tscbRootList;
        private ToolStrip tsWorkingSet;
        public ToolStripButton tsbAddWorkingSet;
        public ToolStripButton tsbRemoveWorkingSet;
        public ToolStripButton tsbAddLink;
        public ToolStripButton tsbRemoveLink;
        private ToolStrip tsMisc;
        private ToolStripButton tsbMakeRoot;
        private ToolStripButton tsbFindPage;
        private ToolStripButton tsbRefresh;

     
        private readonly Presenter presenter = new Presenter();
        private readonly DisplayTreeView treeManager = new DisplayTreeView();

       

        #endregion

        public MainForm()
        {
            InitializeComponent();
            presenter.add(this);
            
            // display Tree
            this.treeManager.displayTree.Visible = true;
            this.treeManager.displayTree.Height = toolContainer.ContentPanel.Height;
            this.treeManager.displayTree.Width = toolContainer.ContentPanel.Width;
            this.treeManager.displayTree.LabelEdit = true;
            this.treeManager.displayTree.NodeMouseClick += new TreeNodeMouseClickEventHandler(onNodeClicked);
            this.treeManager.displayTree.Enter += new EventHandler(displayTree_Enter);
            this.treeManager.displayTree.AfterSelect +=new TreeViewEventHandler(displayTree_AfterSelect);
            
            this.treeManager.displayTree.Dock = DockStyle.Fill;

            toolContainer.ContentPanel.Controls.Add(this.treeManager.displayTree);
            treeManager.LoadTree(presenter);
            this.RootsList(treeManager.AllNode);

            tsbRemoveWorkingSet.Enabled = false;
            tsbAddLinkWSEnabled = false;
            tsbRemoveLinkWSEnabled = false;
            
        }

       

        #region Interface functions

        public LazyTreeNode DisplaySelectedNode
        {
            get
            {
                return treeManager.SelectedNode;
            }
            set
            {
                treeManager.SelectedNode = value;
            }
        }

        public LazyTreeNode AllNode
        {


            get { return treeManager.AllNode; }
            set { treeManager.AllNode = value;}
        }

        public bool tsbRemoveWSEnabled
        {
            get
            {
                return tsbRemoveWorkingSet.Enabled;
            }
            set
            {
                tsbRemoveWorkingSet.Enabled = value;
            }
        }

        public bool tsbAddLinkWSEnabled
        {
            get
            {
                return tsbAddLink.Enabled;
            }
            set
            {
                tsbAddLink.Enabled = value;
            }
        }

        public bool tsbRemoveLinkWSEnabled
        {
            get
            {
                return tsbRemoveLink.Enabled;
            }
            set
            {
                tsbRemoveLink.Enabled = value;
            }
        }



        private class ComboBoxItem
        {
            public LazyTreeNode Node { get; set; }
            public ComboBoxItem(LazyTreeNode node)
            {
                Node = node;
            }
            public override string ToString()
            {
                string TagString = "";
                if (Node.Text == null)  return "";

                TagString = Node.Tag.ToString();

                switch(TagString)
                {
                    case "All" :
                    case "UnfiledNotes" :
                    case "LiveSharedSections" :
                    case "OpenSections" :
                    case "WorkingSets" :
                        TagString = "";
                        break;

                    case "Notebooks" :
                        TagString = "All ";
                        break;

                    case "Notebook":
                        TagString = "NB: ";
                        break;
                    case "SectionGroup":
                        TagString = "SG: ";
                        break;
                    case "Section":
                        TagString = "S: ";
                        break;
                    case "Page":
                        TagString = "P: ";
                        break;
                    case "WorkingSet":
                        TagString = "WS: ";
                        break;
                }


                return TagString + Node.Text;
            }

        }

        private void RootsListRecurser(LazyTreeNode node)
        {
            LazyTreeNode parentNode = (LazyTreeNode) node.Parent;

            if(parentNode!=null)
            {
                RootsListRecurser(parentNode);
            }
            
            tscbRootList.Items.Add(new ComboBoxItem(node));
           
        }

        public void RootsList(LazyTreeNode node)
        {
            // LazyTreeNode refNode = internalTree.Nodes[0] as LazyTreeNode ;
            // SetDisplayRoot(internalTreeMainRoot);
            LazyTreeNode currNode = this.treeManager.SelectedNode;
            this.tscbRootList.Items.Clear();
            this.treeManager.clearDisplay();
            RootsListRecurser(node);
            
            this.tscbRootList.SelectedIndexChanged -= new System.EventHandler(this.tscbRootListSelectionChanged);
            if (tscbRootList.Items.Count > 2)
            {
                this.tscbRootList.Items.RemoveAt(this.tscbRootList.Items.Count - 1);
            }
            this.tscbRootList.SelectedItem = this.tscbRootList.Items[this.tscbRootList.Items.Count - 1];
           

            this.tscbRootList.SelectedIndexChanged += new System.EventHandler(this.tscbRootListSelectionChanged);
            if(currNode!=null) SetDisplayRoot(currNode);

           
        }
        
        public void SetDisplayRoot(LazyTreeNode node)
        {
            treeManager.setDisplayNode(node);
        }

        public void ChangeSelectedNode(string ID)
        {
            treeManager.ChangeSelectedNode(ID);
        }

        public void RefreshView()
        {
            treeManager.LoadTree(presenter);
        }

        
        public void UpdateNode(string ID, DataView newView)
        {
            treeManager.UpdateNode(ID, newView);

        }

        public void UpdateNode(RootNodes node)
        {
            treeManager.UpdateNode(node);

        } 
        #endregion

        #region Form Events

        private void onNodeClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DisplaySelectedNode = e.Node as LazyTreeNode;
                presenter.OnNodeCliked(e.Node);
            }
        }

        void displayTree_Enter(object sender, EventArgs e)
        {
          //  LazyTreeNode node = sender as LazyTreeNode;
           // TreeNodeMouseClickEventArgs args = new TreeNodeMouseClickEventArgs(e.Node, MouseButtons.Left, 1, e.Node.Bounds.X, e.Node.Bounds.X);
           // onNodeClicked(sender, args);
        }

        private void displayTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNodeMouseClickEventArgs args = new TreeNodeMouseClickEventArgs(e.Node,MouseButtons.Left,1,e.Node.Bounds.X,e.Node.Bounds.X);
            onNodeClicked(sender,args);
        }





        private void MakeRootButton(object sender, EventArgs e)
        {
            presenter.MakeRootButtonClicked();
        }

        private void tscbRootListSelectionChanged(object sender, EventArgs e)
        {
            ComboBoxItem node = tscbRootList.SelectedItem as ComboBoxItem;
            if (node == null) return;
            presenter.ListSelectionChange(node.Node);
        }

        private void FindInTreeButton(object sender, EventArgs e)
        {
            presenter.FindCurrentPageInView();
        }

        private void RefreshViewButton(object sender, EventArgs e)
        {
            presenter.RefreshViewButtonClicked();
        }


        #endregion
        
        
        #region Initaialize Form
        private void InitializeComponent()
        {
            System.Windows.Forms.ToolStrip tsRoot;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tscbRootList = new System.Windows.Forms.ToolStripComboBox();
            this.toolContainer = new System.Windows.Forms.ToolStripContainer();
            this.tsMisc = new System.Windows.Forms.ToolStrip();
            this.tsbMakeRoot = new System.Windows.Forms.ToolStripButton();
            this.tsbFindPage = new System.Windows.Forms.ToolStripButton();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsWorkingSet = new System.Windows.Forms.ToolStrip();
            this.tsbAddWorkingSet = new System.Windows.Forms.ToolStripButton();
            this.tsbRemoveWorkingSet = new System.Windows.Forms.ToolStripButton();
            this.tsbAddLink = new System.Windows.Forms.ToolStripButton();
            this.tsbRemoveLink = new System.Windows.Forms.ToolStripButton();
            tsRoot = new System.Windows.Forms.ToolStrip();
            tsRoot.SuspendLayout();
            this.toolContainer.BottomToolStripPanel.SuspendLayout();
            this.toolContainer.TopToolStripPanel.SuspendLayout();
            this.toolContainer.SuspendLayout();
            this.tsMisc.SuspendLayout();
            this.tsWorkingSet.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsRoot
            // 
            tsRoot.BackColor = System.Drawing.Color.White;
            tsRoot.Dock = System.Windows.Forms.DockStyle.None;
            tsRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tscbRootList});
            tsRoot.Location = new System.Drawing.Point(3, 0);
            tsRoot.Name = "tsRoot";
            tsRoot.Padding = new System.Windows.Forms.Padding(0);
            tsRoot.Size = new System.Drawing.Size(211, 25);
            tsRoot.TabIndex = 0;
            tsRoot.Text = "toolStrip1";
            // 
            // tscbRootList
            // 
            this.tscbRootList.BackColor = System.Drawing.Color.White;
            this.tscbRootList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tscbRootList.Margin = new System.Windows.Forms.Padding(0);
            this.tscbRootList.Name = "tscbRootList";
            this.tscbRootList.Size = new System.Drawing.Size(200, 25);
            this.tscbRootList.SelectedIndexChanged += new System.EventHandler(this.tscbRootListSelectionChanged);
            // 
            // toolContainer
            // 
            // 
            // toolContainer.BottomToolStripPanel
            // 
            this.toolContainer.BottomToolStripPanel.BackColor = System.Drawing.Color.White;
            this.toolContainer.BottomToolStripPanel.Controls.Add(this.tsWorkingSet);
            this.toolContainer.BottomToolStripPanel.Controls.Add(this.tsMisc);
            this.toolContainer.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // toolContainer.ContentPanel
            // 
            this.toolContainer.ContentPanel.BackColor = System.Drawing.Color.White;
            this.toolContainer.ContentPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolContainer.ContentPanel.Size = new System.Drawing.Size(237, 508);
            this.toolContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolContainer.Location = new System.Drawing.Point(0, 0);
            this.toolContainer.Margin = new System.Windows.Forms.Padding(0);
            this.toolContainer.Name = "toolContainer";
            this.toolContainer.Size = new System.Drawing.Size(237, 564);
            this.toolContainer.TabIndex = 1;
            this.toolContainer.Text = "toolStripContainer1";
            // 
            // toolContainer.TopToolStripPanel
            // 
            this.toolContainer.TopToolStripPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.toolContainer.TopToolStripPanel.Controls.Add(tsRoot);
            this.toolContainer.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // tsMisc
            // 
            this.tsMisc.AllowItemReorder = true;
            this.tsMisc.BackColor = System.Drawing.Color.White;
            this.tsMisc.Dock = System.Windows.Forms.DockStyle.None;
            this.tsMisc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbMakeRoot,
            this.tsbFindPage,
            this.tsbRefresh});
            this.tsMisc.Location = new System.Drawing.Point(127, 0);
            this.tsMisc.Name = "tsMisc";
            this.tsMisc.Size = new System.Drawing.Size(96, 31);
            this.tsMisc.TabIndex = 1;
            // 
            // tsbMakeRoot
            // 
            this.tsbMakeRoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMakeRoot.Image = ((System.Drawing.Image)(resources.GetObject("tsbMakeRoot.Image")));
            this.tsbMakeRoot.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbMakeRoot.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbMakeRoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMakeRoot.Name = "tsbMakeRoot";
            this.tsbMakeRoot.Size = new System.Drawing.Size(28, 28);
            this.tsbMakeRoot.Text = "Make Selected Node the Root";
            this.tsbMakeRoot.ToolTipText = "Make Selected Node the Root";
            this.tsbMakeRoot.Click += new System.EventHandler(this.MakeRootButton);
            // 
            // tsbFindPage
            // 
            this.tsbFindPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFindPage.Image = ((System.Drawing.Image)(resources.GetObject("tsbFindPage.Image")));
            this.tsbFindPage.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbFindPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbFindPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFindPage.Name = "tsbFindPage";
            this.tsbFindPage.Size = new System.Drawing.Size(28, 28);
            this.tsbFindPage.Text = "Find Current Page in Tree";
            this.tsbFindPage.ToolTipText = "Find Current Page in Tree";
            this.tsbFindPage.Click += new System.EventHandler(this.FindInTreeButton);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbRefresh.Image")));
            this.tsbRefresh.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(28, 28);
            this.tsbRefresh.Text = "Refresh";
            this.tsbRefresh.Click += new System.EventHandler(this.RefreshViewButton);
            // 
            // tsWorkingSet
            // 
            this.tsWorkingSet.AllowItemReorder = true;
            this.tsWorkingSet.BackColor = System.Drawing.Color.White;
            this.tsWorkingSet.Dock = System.Windows.Forms.DockStyle.None;
            this.tsWorkingSet.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAddWorkingSet,
            this.tsbRemoveWorkingSet,
            this.tsbAddLink,
            this.tsbRemoveLink});
            this.tsWorkingSet.Location = new System.Drawing.Point(3, 0);
            this.tsWorkingSet.Name = "tsWorkingSet";
            this.tsWorkingSet.Size = new System.Drawing.Size(124, 31);
            this.tsWorkingSet.TabIndex = 1;
            // 
            // tsbAddWorkingSet
            // 
            this.tsbAddWorkingSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddWorkingSet.Image = ((System.Drawing.Image)(resources.GetObject("tsbAddWorkingSet.Image")));
            this.tsbAddWorkingSet.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddWorkingSet.ImageTransparentColor = System.Drawing.Color.Black;
            this.tsbAddWorkingSet.Name = "tsbAddWorkingSet";
            this.tsbAddWorkingSet.Size = new System.Drawing.Size(28, 28);
            this.tsbAddWorkingSet.Text = "Add Favorites Folder";
            this.tsbAddWorkingSet.ToolTipText = "Add Favorites Folder";
            this.tsbAddWorkingSet.Click += new System.EventHandler(this.tsbAddWorkingSet_Click);
            // 
            // tsbRemoveWorkingSet
            // 
            this.tsbRemoveWorkingSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRemoveWorkingSet.Image = ((System.Drawing.Image)(resources.GetObject("tsbRemoveWorkingSet.Image")));
            this.tsbRemoveWorkingSet.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRemoveWorkingSet.ImageTransparentColor = System.Drawing.Color.Black;
            this.tsbRemoveWorkingSet.Name = "tsbRemoveWorkingSet";
            this.tsbRemoveWorkingSet.Size = new System.Drawing.Size(28, 28);
            this.tsbRemoveWorkingSet.Text = "Remove Favorites Folder";
            this.tsbRemoveWorkingSet.Click += new System.EventHandler(this.tsbRemoveWorkingSet_Click);
            // 
            // tsbAddLink
            // 
            this.tsbAddLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddLink.Image = ((System.Drawing.Image)(resources.GetObject("tsbAddLink.Image")));
            this.tsbAddLink.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddLink.Name = "tsbAddLink";
            this.tsbAddLink.Size = new System.Drawing.Size(28, 28);
            this.tsbAddLink.Text = "Add Link";
            this.tsbAddLink.Click += new System.EventHandler(this.tsbAddLink_Click);
            // 
            // tsbRemoveLink
            // 
            this.tsbRemoveLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRemoveLink.Image = ((System.Drawing.Image)(resources.GetObject("tsbRemoveLink.Image")));
            this.tsbRemoveLink.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbRemoveLink.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRemoveLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRemoveLink.Name = "tsbRemoveLink";
            this.tsbRemoveLink.Size = new System.Drawing.Size(28, 28);
            this.tsbRemoveLink.Text = "Remove Link";
            this.tsbRemoveLink.Click += new System.EventHandler(this.tsbRemoveLink_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(237, 564);
            this.Controls.Add(this.toolContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "OneNote TreeView";
            this.Load += new System.EventHandler(this.MainForm_Load);
            tsRoot.ResumeLayout(false);
            tsRoot.PerformLayout();
            this.toolContainer.BottomToolStripPanel.ResumeLayout(false);
            this.toolContainer.BottomToolStripPanel.PerformLayout();
            this.toolContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolContainer.TopToolStripPanel.PerformLayout();
            this.toolContainer.ResumeLayout(false);
            this.toolContainer.PerformLayout();
            this.tsMisc.ResumeLayout(false);
            this.tsMisc.PerformLayout();
            this.tsWorkingSet.ResumeLayout(false);
            this.tsWorkingSet.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private void tsbAddWorkingSet_Click(object sender, EventArgs e)
        {
            presenter.AddWorkingSet();
        }

        private void tsbRemoveWorkingSet_Click(object sender, EventArgs e)
        {
            presenter.RemoveWorkingSet();

        }



        private void tsbAddLink_Click(object sender, EventArgs e)
        {
            presenter.AddWorkingSetLink(treeManager.SelectedNode, treeManager.GetNode("WorkingSets"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            presenter.Closing();
            base.OnClosing(e);
        }

        private void tsbRemoveLink_Click(object sender, EventArgs e)
        {
            presenter.RemoveWorkingSetLink();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }


       
    }
}
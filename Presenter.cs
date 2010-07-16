using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using TreeView4;
using OneNote =  Microsoft.Office.Interop.OneNote;

namespace TreeView4
{
    public class Presenter
    {
        private ITreeView view;
        private DataManager dataManager = new DataManager();

        public void add(ITreeView objTreeView)
        {
            view = objTreeView;
            dataManager.InitializeDataSet(Application.UserAppDataPath);
            //dataManager.GetNodeParent("{C9CE6368-C9FD-4878-A9B1-AC197404C1BC}{1}{B0}");

        }

        #region Interface with DataManager

        public DataView GetNodeChildren(RootNodes nodeType)
        {
            return dataManager.GetNodeChildren(nodeType);
        }

        public DataView GetNodeChildren(string name)
        {
            return dataManager.GetNodeChildren(name);
        }

        public string GetNodeParent(string ID)
        {
            return dataManager.GetNodeParent(ID);
        }

        #endregion


        private bool NavigateTo(string ID)
        {
            try
            {
                OneNote.Application oneApp = new OneNote.Application();
                oneApp.NavigateTo(ID, null, false);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void OnNodeCliked(TreeNode node)
        {
            view.tsbRemoveWSEnabled = false;
            view.tsbAddLinkWSEnabled = false;
            view.tsbRemoveLinkWSEnabled = false;
            

            NodeTypes? nodeType =  node.Tag as NodeTypes?;
            if (nodeType == NodeTypes.Page || nodeType == NodeTypes.Section ||
                nodeType == NodeTypes.SectionGroup || nodeType == NodeTypes.Notebook)
            {
                if (DisplayTreeView.IsNodeInWorkingSet(node) == false)
                {
                    view.tsbAddLinkWSEnabled = true;
                }
                NavigateTo(node.Name);
            }
            if(nodeType == NodeTypes.WorkingSet)
            {
                view.tsbRemoveWSEnabled = true;
            }

            LazyTreeNode parentNode = node.Parent as LazyTreeNode;
            if (parentNode != null)
            {
                NodeTypes? parentType = parentNode.Tag as NodeTypes?;
                if (parentType == NodeTypes.WorkingSet)
                    view.tsbRemoveLinkWSEnabled = true;
            }

        }

       
       
        public void FindCurrentPageInView()
        {
            string ID = dataManager.GetCurrentPageViewed();
            if(ID!=null)
            {
                view.ChangeSelectedNode(ID);
            }
        }

        public void MakeRootButtonClicked()
        {
            LazyTreeNode selectedNode = view.DisplaySelectedNode ;
            if (selectedNode == null ) return;
             
            if(selectedNode.Nodes.Count == 0 )
            {
                selectedNode = selectedNode.Parent as LazyTreeNode;
            }
            
            view.RootsList(selectedNode);
            view.SetDisplayRoot(selectedNode);
        }

        public void RefreshViewButtonClicked()
        {

            dataManager.workingSet.saveToFile(Application.UserAppDataPath);
            dataManager.InitializeDataSet(Application.UserAppDataPath);
            view.RefreshView();
            FindCurrentPageInView();
        }

        public void ListSelectionChange(LazyTreeNode node)
        {
            view.RootsList(node);
            view.SetDisplayRoot(node);

        }

        public void AddWorkingSet()
        {
            var frmWS = new WorkingSetName();
           
            frmWS.NewWorkingSetName += new WorkingSetName.WorkingSetNewName(AddWorkingSetHandler);
            frmWS.ShowDialog();
               
        }

        private void AddWorkingSetHandler(object sender, WorkingSetName.WorkingSetNameArgs e)
        {
            dataManager.workingSet.AddWorkingSet(e.Name);
            view.UpdateNode(RootNodes.WorkingSets);
        }

        public void RemoveWorkingSet()
        {

            view.tsbRemoveWSEnabled = false;
            dataManager.workingSet.DeleteWorkingSet(view.DisplaySelectedNode.Name);
            view.UpdateNode(RootNodes.WorkingSets);
        }

        public void AddWorkingSetLink(LazyTreeNode tn, LazyTreeNode ws)
        {
            var frmWS = new AddWSLink();
            frmWS.LoadList(ws);
            frmWS.WorkingSetSelectionMade += new AddWSLink.ChosenWorkingSet(frmWS_WorkingSetSelectionMade);
            AddLinkToWorkingSet = tn;
            frmWS.ShowDialog();


            
            //dataManager.workingSet.AddWorkingSetLink();
        }

        public LazyTreeNode AddLinkToWorkingSet { get; set; }
        private void frmWS_WorkingSetSelectionMade(object sender, AddWSLink.WorkingSetSelectedArgs e)
        {
            if(e.Node==null) return;

            dataManager.workingSet.AddWorkingSetLink(e.Node.Name,AddLinkToWorkingSet.Name);
            view.UpdateNode(e.Node.Name, GetNodeChildren(e.Node.Name) );
        }

        public void Closing()
        {
            dataManager.workingSet.saveToFile(Path.GetDirectoryName(Application.ExecutablePath));
        }

        public void RemoveWorkingSetLink()
        {
            view.tsbRemoveLinkWSEnabled = false;
            view.tsbRemoveWSEnabled = true;
            string workingSetID = view.DisplaySelectedNode.Parent.Name;
            dataManager.workingSet.RemoveWorkingSetLink(workingSetID, view.DisplaySelectedNode.Name);
            view.UpdateNode(workingSetID, GetNodeChildren(workingSetID));
        }
    }
}
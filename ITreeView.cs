using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using TreeView4;

namespace TreeView4
{
    public interface ITreeView
    {
        
        void RootsList(LazyTreeNode node);
        // void RootsVisible(List<RootNodes> list);
        void SetDisplayRoot(LazyTreeNode node);
        
        LazyTreeNode AllNode{ get; set;}
        
        LazyTreeNode DisplaySelectedNode{ get; set;}

        void ChangeSelectedNode(string ID);

        void UpdateNode(string ID, DataView newView);

        void UpdateNode(RootNodes node);
        
        bool tsbRemoveWSEnabled{get;set;}
        bool tsbAddLinkWSEnabled { get; set; }
        bool tsbRemoveLinkWSEnabled { get; set; }
        
        

        void RefreshView();
    }
}
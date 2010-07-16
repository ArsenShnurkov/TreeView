using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TreeView4
{
    public partial class AddWSLink : Form
    {

        public class WorkingSetSelectedArgs : System.EventArgs
        {
            public LazyTreeNode Node { get; set;}

            public WorkingSetSelectedArgs(LazyTreeNode lazyNode)
            {
                Node = lazyNode;
            }
        }

        public delegate void ChosenWorkingSet(object sender, WorkingSetSelectedArgs e);

        public event ChosenWorkingSet WorkingSetSelectionMade;


        public AddWSLink()
        {
            InitializeComponent();
            //LoadList(node);
        }

        public void LoadList(LazyTreeNode node)
        {
            foreach (LazyTreeNode tn in node.Nodes)
            {
                lbWorkingSets.DisplayMember = "Text";
                lbWorkingSets.Items.Add(tn);
            }

            if (lbWorkingSets.Items.Count > 0)
            {
                lbWorkingSets.SelectedItem = this.lbWorkingSets.Items[0];
            }
        }

        private void AddWSLink_Load(object sender, EventArgs e)
        {

        }



        private void btnOkClicked(object sender, EventArgs e)
        {
            LazyTreeNode newNode = lbWorkingSets.SelectedItem as LazyTreeNode;
            WorkingSetSelectedArgs args = new WorkingSetSelectedArgs(newNode);

            if(WorkingSetSelectionMade != null)
            {
                WorkingSetSelectionMade(this, args);
            }
            
            this.Close();
        }

      

        private void btnCancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lbWorkingSets_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        


    }
}

using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using OneNote = Microsoft.Office.Interop.OneNote;
using System.Linq;


namespace TreeView4
{
    public class DataManager
    {
        public DataSet dsOneNote { get; set;}
        //public DataSet dsWorkingSet { get; set; }
        public WorkingSet workingSet;

        public bool InitializeDataSet(string fileName)
        {
            try
            {
                dsOneNote = new DataSet("OneNoteHierarchy");
                var onApp = new OneNote.Application();
                string strONXml;
                onApp.GetHierarchy(null, OneNote.HierarchyScope.hsPages, out strONXml);
                var srONXml = new StringReader(strONXml);
                dsOneNote.ReadXml(srONXml);
                workingSet = new WorkingSet(fileName);
            }
            catch (Exception)
            {
               MessageBox.Show("Error accessing OneNote. Please make sure you have OneNote installed.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
               Application.Exit();
            }


            return true;
        }

        public NodeTypes? GetNodeType(string ID)
        {
            if(ID==null) return null;

            NodeTypes? retType = null;
            foreach (DataTable dt in dsOneNote.Tables)
            {
                int getIDCount = dt.Select(string.Format("ID='{0}'", ID)).Count();
                if (getIDCount > 0)
                {
                    retType = Enum.Parse(typeof (NodeTypes), dt.TableName, true) as NodeTypes?;
                    return retType;
                }
            }

            if(workingSet.dsWorkingSet.Tables["WorkingSet"].Select(string.Format("ID='{0}'", ID)).Count() > 0)
            {
                retType = NodeTypes.WorkingSet;
            }

            return retType;
        }

        public DataView GetNodeChildren(string ID)
        {
            NodeTypes? nodeType = GetNodeType(ID);
            if (nodeType == null) return null;

            string dtableName = Enum.GetName(typeof(NodeTypes), nodeType);
            var retTable = new DataTable("ChildNodes");
            retTable.Columns.Add(new DataColumn("ID", typeof(string)));
            retTable.Columns.Add(new DataColumn("DisplayName", typeof(string)));
            retTable.Columns.Add(new DataColumn("NodeType", typeof(NodeTypes)));
            
            if(dtableName == "WorkingSet")
            {
                DataView workingLinks = workingSet.NodeChildren(ID); //dsWorkingSet.Tables["WorkingSetLinks"].Select(string.Format("WorkingSetID='{0}'",ID));
                
                foreach (DataRowView rowView in workingLinks)
                {
                    if (GetNodeType(rowView["LinkID"].ToString()) == null)
                    {
                        workingSet.RemoveWorkingSetLink(ID, rowView["LinkID"].ToString());
                        continue;
                    }

                    DataRow dRowInfo = GetNodeInfo(rowView["LinkID"].ToString());
                    string copyID = dRowInfo["ID"].ToString();
                    string copyDisplayName = dRowInfo["name"].ToString();
                    NodeTypes copyNodeType = (NodeTypes)Enum.Parse(typeof(NodeTypes), dRowInfo.Table.TableName, true);
                    retTable.Rows.Add(new object[] { copyID, copyDisplayName, copyNodeType });
                }
            }

            else
            {
                // logic to get all the children from the tables and put into a single table
                foreach (DataRelation dRelation in dsOneNote.Tables[dtableName].ChildRelations)
                {
                    var dataViewNode = new DataView(dsOneNote.Tables[dtableName]) {Sort = "ID"};
                    Int32 drowindex = dataViewNode.Find(ID);
                    DataView dvTemp = dataViewNode[drowindex].CreateChildView(dRelation);

                    foreach (DataRowView rowView in dvTemp)
                    {
                        string copyID = rowView["ID"].ToString();
                        string copyDisplayName = rowView["name"].ToString();
                        NodeTypes copyNodeType = (NodeTypes)Enum.Parse(typeof(NodeTypes), dvTemp.Table.TableName, true);
                        retTable.Rows.Add(new object[] { copyID, copyDisplayName, copyNodeType });
                    }

                }

            }

            return retTable.DefaultView;
        }

        public DataRow GetNodeInfo(string ID)
        {
            
            try
            {
                string dtableName = Enum.GetName(typeof(NodeTypes), GetNodeType(ID));
                return dsOneNote.Tables[dtableName].Select(string.Format("ID='{0}'",ID))[0];

            }
            catch (Exception)
            {
                return null;
            }


        }

        public DataView GetNodeChildren(RootNodes specialID)
        {
            

            var retTable = new DataTable("RootNodes");
            retTable.Columns.Add(new DataColumn("ID", typeof(string)));
            retTable.Columns.Add(new DataColumn("DisplayName", typeof(string)));
            retTable.Columns.Add(new DataColumn("NodeType", typeof(NodeTypes)));

            switch (specialID)
            {
                case RootNodes.Notebooks:
                    {
                        if (dsOneNote.Tables.IndexOf("Notebook") < 0)
                            return null;

                        DataTable dataTable = dsOneNote.Tables["Notebook"];
                        foreach (DataRow rowView in dataTable.Rows)
                        {
                            string copyID = rowView["ID"].ToString();
                            string copyDisplayName = rowView["name"].ToString();
                            const NodeTypes copyNodeType = NodeTypes.Notebook;
                            retTable.Rows.Add(new object[] {copyID, copyDisplayName, copyNodeType});
                        }
                        return retTable.DefaultView;
                    }

                case (RootNodes.LiveSharedSections):
                case (RootNodes.OpenSections):
                case (RootNodes.UnfiledNotes):

                    if (dsOneNote.Tables.IndexOf(specialID.ToString()) < 0)
                        return null;
                    DataTable dTable = dsOneNote.Tables[specialID.ToString()];
                    return this.GetNodeChildren(dTable.Rows[0]["ID"].ToString());

                case (RootNodes.WorkingSets):
                    {
                        DataTable dTableWS = workingSet.dsWorkingSet.Tables["WorkingSet"];
                        foreach (DataRow rowView in dTableWS.Rows)
                        {
                            string copyID = rowView["ID"].ToString();
                            string copyDisplayName = rowView["name"].ToString();
                            const NodeTypes copyNodeType = NodeTypes.WorkingSet;
                            retTable.Rows.Add(new object[] {copyID, copyDisplayName, copyNodeType});
                        }
                        return retTable.DefaultView;
                    }
            }

            return null;
        }

        public string GetNodeParent(string ID)
        {
            NodeTypes? nodeType = GetNodeType(ID);
            if(nodeType == null) return null;
            string dtableName = Enum.GetName(typeof(NodeTypes), nodeType );
           
            foreach(DataRelation dRelation in dsOneNote.Tables[dtableName].ParentRelations)
            {
                DataRow dRow = dsOneNote.Tables[dtableName].Select(string.Format("ID='{0}'", ID))[0];
                DataRow parentRow = dRow.GetParentRow(dRelation);
                if(parentRow != null) return parentRow["ID"].ToString();
            }
            return null;

        }

        public string GetCurrentPageViewed()
        {
           
            string strXML;
            var xdoc = new XmlDocument();
            var onApp = new OneNote.Application();
            onApp.GetHierarchy(null, OneNote.HierarchyScope.hsPages, out strXML);
            // Load the xml into a document
            xdoc.LoadXml(strXML);
            string OneNoteNamespace = "http://schemas.microsoft.com/office/onenote/2007/onenote";

            var nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("one", OneNoteNamespace);

            // Find the page ID of the active page

            XmlElement xmlActivePage =
                (XmlElement) xdoc.SelectSingleNode("//one:Page[@isCurrentlyViewed=\"true\"]", nsmgr) ??
                ((XmlElement) xdoc.SelectSingleNode("//one:Section[@isCurrentlyViewed=\"true\"]", nsmgr) ??
                 (XmlElement) xdoc.SelectSingleNode("//one:SectionGroup[@isCurrentlyViewed=\"true\"]", nsmgr));
                                       
            string strActivePageID = xmlActivePage != null ? xmlActivePage.GetAttribute("ID") : null;
            
            return strActivePageID;
        

        }
    }
}
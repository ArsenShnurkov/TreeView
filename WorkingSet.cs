using System;
using System.Data;
using System.IO;
using TreeView4;

namespace TreeView4
{
    public class WorkingSet
    {
        public DataSet dsWorkingSet { get; set;}

        public WorkingSet(string fileDir)
        {
           
            dsWorkingSet = new DataSet("dsWorkingSet");

            if (File.Exists(fileDir + "\\favorites.xsd") && File.Exists(fileDir + "\\favorites.xml"))
            {
                dsWorkingSet.ReadXmlSchema(fileDir + "\\favorites.xsd");
                dsWorkingSet.ReadXml(fileDir+"\\favorites.xml");
            }

            if (dsWorkingSet.Tables.Count < 1)
            {

                DataTable dtWorkingSet = new DataTable("WorkingSet");
                DataColumn id = dtWorkingSet.Columns.Add("ID", typeof (string));
                dtWorkingSet.Columns.Add("Name", typeof (string));
                dtWorkingSet.Columns.Add("NumChildren", typeof (Int32));
                //dtWorkingSet.PrimaryKey = new[] {id};
                dsWorkingSet.Tables.Add(dtWorkingSet);
            }

            if (dsWorkingSet.Tables.Count < 2)
                {
                    DataTable dtWorkingSetLinks = new DataTable("WorkingSetLinks");
                    DataColumn wsID = dtWorkingSetLinks.Columns.Add("WorkingSetID", typeof (string));
                    DataColumn linkID = dtWorkingSetLinks.Columns.Add("LinkID", typeof (string));
                    dtWorkingSetLinks.Columns.Add("LinkNodeType", typeof (NodeTypes));
                    //dtWorkingSetLinks.PrimaryKey = new[] {wsID, linkID};
                    dsWorkingSet.Tables.Add(dtWorkingSetLinks);
                }

            dsWorkingSet.Tables["WorkingSet"].PrimaryKey = new[] {dsWorkingSet.Tables["WorkingSet"].Columns["ID"]};
            dsWorkingSet.Tables["WorkingSetLinks"].PrimaryKey = new[] { dsWorkingSet.Tables["WorkingSetLinks"].Columns["WorkingSetID"],
                                                                        dsWorkingSet.Tables["WorkingSetLinks"].Columns["LinkID"] 
                                                                      };



        }

        public string AddWorkingSet(string name)
        {
            try
            {
                string retGuid = Guid.NewGuid().ToString();
                dsWorkingSet.Tables["WorkingSet"].Rows.Add(new object[] {retGuid, name, 0});
                dsWorkingSet.AcceptChanges();
                return retGuid;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public void DeleteWorkingSet(string ID)
        {
            try
            {
                DataRow[] delRows = dsWorkingSet.Tables["WorkingSetLinks"].Select(string.Format("WorkingSetID='{0}'", ID));

                foreach (DataRow dr in delRows)
                {
                    dsWorkingSet.Tables["WorkingSetLinks"].Rows.Remove(dr);
                }

                DataRow delMainRow = dsWorkingSet.Tables["WorkingSet"].Rows.Find(ID);
                dsWorkingSet.Tables["WorkingSet"].Rows.Remove(delMainRow);
                dsWorkingSet.AcceptChanges();
            }
            catch
            {
                return;
            }
        }

        public void AddWorkingSetLink(string wsID, string linkID)
        {
            try
            {
                DataRow findRow = dsWorkingSet.Tables["WorkingSet"].Rows.Find(wsID);
                dsWorkingSet.Tables["WorkingSetLinks"].Rows.Add(new object[] {wsID, linkID, null});
                int count = (int)findRow["NumChildren"];
                findRow["NumChildren"] = count + 1;
                dsWorkingSet.AcceptChanges();
               
            }
            catch (Exception)
            {
                return;
            }
        }

        public void RemoveWorkingSetLink(string wsID, string linkID)
        {
            try
            {
                DataRow findRow = dsWorkingSet.Tables["WorkingSet"].Rows.Find(wsID);
                DataRow delRow = dsWorkingSet.Tables["WorkingSetLinks"].Rows.Find(new object[]{wsID,linkID}); 
                dsWorkingSet.Tables["WorkingSetLinks"].Rows.Remove(delRow);
                int count = (int)findRow["NumChildren"];
                findRow["NumChildren"] = count - 1;
                dsWorkingSet.AcceptChanges();
            }
            catch (Exception)
            {
                return;
            }
            
        }

        // retrurns all the links for a given working set ID
        public DataView NodeChildren(string wsID)
        {
           
            DataView dv = dsWorkingSet.Tables["WorkingSetLinks"].DefaultView;
            dv.RowFilter = string.Format("WorkingSetID='{0}'", wsID);
            return dv;
        }

        public void saveToFile(string fileDir)
        {
            dsWorkingSet.WriteXmlSchema(fileDir + "\\favorites.xsd");
            dsWorkingSet.WriteXml(fileDir + "\\favorites.xml");
        }

    }
}
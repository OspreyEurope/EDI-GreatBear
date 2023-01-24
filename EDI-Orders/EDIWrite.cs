using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    public class EDIWrite
    {
        #region Writes EDI File in Fact format
        public static void WriteEDIFact (SqlConnection con)
        {
            StreamWriter streamWriter = new StreamWriter("OSP_EDIWriteFile");
            streamWriter.Write("UNA:+.?'");
            int counter = 0;
            DataTable data = SharedFunctions.GetData(con,"OSP_Get_Responce_Data");
            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                string header = Lookups.WriteLookUp(nameArray[i]);
                string text = "";
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    text = text + row[j].ToString();
                    streamWriter.Write(header + "+" + text + "'");
                    counter++;
                }
            }
            streamWriter.Write("UNS+S'UNT+"+ counter +"'UNZ+"+ /**IDK ABOUT THIS PART*/ counter + "'");
            SharedFunctions.UpdateRecords(con, "OSP_Update_EDI_Order_Flags");
        }
        #endregion
    }
}
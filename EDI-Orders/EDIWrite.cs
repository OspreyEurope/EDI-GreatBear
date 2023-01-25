using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EDI_Orders
{
    public class EDIWrite
    {
        #region Writes EDI File in Fact format
        /**
         * This function writes the responce file, 
         * this can will be changed based on the file that needs to be produced,
         * this can be used as a basis but is currently set up to produce an edi
         * responce simialr to the EDIFact file that is being recieved.
         */
        public static void WriteEDIFact (SqlConnection con)
        {
            Console.WriteLine("Made to Write file");
            DataTable data = SharedFunctions.GetData(con, "OSP_Get_Responce_Data");
            StreamWriter streamWriter = new StreamWriter("C:\\Bespoke\\EDI\\OutputFiles" + data.Select("@OrderNumber") + ".txt");
            streamWriter.Write("UNA:+.?'");
            int counter = 0;
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            
            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                string header = Lookups.WriteLookUp(nameArray[i]);
                string text = "";
                if (header.Equals("@DelCountry"))
                {
                    WriteEDIFactProducts(con,streamWriter);
                }
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    text = text + row[j].ToString();
                    streamWriter.Write(header + "+" + text + "'");
                    counter++;
                }
            }
            streamWriter.Write("UNS+S'UNT+"+ counter +"'UNZ+"+ /**IDK ABOUT THIS PART*/ counter + "'");
            //SharedFunctions.UpdateRecords(con, "OSP_Update_EDI_Order_Flags");
        }
        #endregion

        #region Write products
        public static void WriteEDIFactProducts(SqlConnection con, StreamWriter sw)
        {
            Console.WriteLine("Made to Write Products");
            DataTable data = SharedFunctions.GetData(con, "");
            sw.Write("UNA:+.?'");
            int counter = 0;
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                string header = Lookups.WriteLookUp(nameArray[i]);
                string text = "";
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    text = text + row[j].ToString();
                    sw.Write(header + "+" + text + "'");
                    counter++;
                }
            }
            sw.Write("UNS+S'UNT+" + counter + "'UNZ+" + /**IDK ABOUT THIS PART*/ counter + "'");
            //SharedFunctions.UpdateRecords(con, "OSP_Update_EDI_Order_Flags");
        }
        #endregion
    }
}
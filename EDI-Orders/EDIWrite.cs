using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using File = System.IO.File;

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
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Write_Header_EDI");
            Console.WriteLine(data.Rows.Count);
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
                string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + row["OrderNumber"] + ".txt";
                StreamWriter streamWriter = new StreamWriter(file);
                Console.WriteLine(row["OrderNumber"]);
                streamWriter.WriteLine("UNA:5056302200001+.?'");
                string text = "";
                string tempLine = "";
                bool flag = false;
                string header = "UNA";
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                string prevHeader = "";
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    header = Lookups.WriteLookUp(nameArray[j]);
                    
                    text += row[j].ToString();
                    /**
                     * This switch case allows addresslines 1-4 to be written into a single line and also identifys when the products need to be written to the file.
                     */
                    if (header == "NAD+DP")
                    {
                        tempLine = tempLine + "+" + text;
                        flag = true;
                    }
                    else if ((header == "NAD+IV") && (header != prevHeader))
                    {
                        streamWriter.WriteLine(prevHeader + ":" + tempLine + "'");
                        tempLine = "";
                        tempLine = tempLine + "+" + text;
                        flag = true;
                    }
                    else if (header == prevHeader)
                    {
                        tempLine = tempLine + "+" + text;
                        flag = true;
                    }
                    else
                    {
                        if (flag == true)
                        {
                            streamWriter.WriteLine(prevHeader + ":" + tempLine + "'");
                            streamWriter.WriteLine(header + ":" + text + "'");
                            tempLine = "";
                            flag = false;
                        }
                        else if (prevHeader == "CUX")
                        {
                            Console.WriteLine("HI");
                            WriteEDIFactProducts(con, streamWriter, row["OrderNumber"].ToString());
                            streamWriter.WriteLine(header + ":" + text + "'");
                        }
                        else
                        {
                            streamWriter.WriteLine(header + ":" + text + "'");
                        }
                    }
                    prevHeader = header;
                    text = "";
                    counter++;
                }
                /**
                 * Generates the footer of the file
                 */
                streamWriter.WriteLine("UNS+S'");
                streamWriter.WriteLine("UNT+" + counter + "'");
                streamWriter.Close();
                var lineCount = File.ReadLines(file).Count();
                File.AppendAllText(file, "UNZ+" + (lineCount + 1) + "'");
            }
        }
        #endregion

        #region Write products
        public static void WriteEDIFactProducts(SqlConnection con, StreamWriter sw, string orderNo)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Write_Products_EDI", orderNo);
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
                string text = "";
                string prevHeader = "";
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    string header = Lookups.WriteLookUp(nameArray[j]);
                    if (header == "LIN")
                    {
                        text += "+" +row[j].ToString();
                    }
                    else 
                    {
                        if (prevHeader == "LIN")
                        {
                            sw.WriteLine(prevHeader + ":" + text + "'");
                            text = "";
                        }
                        text += row[j].ToString();
                        sw.WriteLine(header + ":" + text + "'");
                        text = "";
                    }
                    prevHeader = header;
                    
                }
            }
            //SharedFunctions.QueryDB(con, "OSP_Update_StatusID_KTN_Orders", orderNo);
        }
        #endregion

        #region Write Product List For Selected Warehouse
        public static void WriteProductList(SqlConnection con, string id,string s)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
            Console.WriteLine(data.Rows.Count);
            int counter = 0;
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + id + "_Product_List.txt";
            StreamWriter streamWriter = new StreamWriter(file);
            streamWriter.WriteLine("UNH:5056302200001+.?'");
            string text = "";
            string header = "";

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    header = Lookups.WriteLookUp(nameArray[j]);
                    text = row[j].ToString();
                    streamWriter.WriteLine(header + ":" + text + "'");
                }
            }
            /**
                 * Generates the footer of the file
                 */
            streamWriter.WriteLine("UNS+S'");
            streamWriter.WriteLine("UNT+" + counter + "'");
            streamWriter.Close();
            var lineCount = File.ReadLines(file).Count();
            File.AppendAllText(file, "UNZ+" + (lineCount + 1) + "'");
        }
        #endregion

        #region ASN Write
        public static void WriteASN(SqlConnection con, string id)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_PO_DATA", id);
            Console.WriteLine(data.Rows.Count);
            int counter = 0;
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            string file = "C:\\Bespoke\\EDI\\OutputFiles\\PO" + id + ".txt";
            StreamWriter streamWriter = new StreamWriter(file);
            streamWriter.WriteLine("UNH:5056302200001+.?'");
            string text = "";
            string header = "";
            string LIN = "";
            string MOA = "";

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                /**
                 * This sectin writes all the information in that data row into a single line in the EDI file.
                 */
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    header = Lookups.ASNLookup(nameArray[j]);
                    text = row[j].ToString();
                    if (header == "LIN")
                    {
                        if (LIN != "")
                        {
                            LIN = LIN + "+" + text;
                        }
                        else
                        {
                            LIN = text;
                        }
                    }
                    else if (header == "MOA")
                    {
                        if (MOA != "")
                        {
                            MOA = MOA + "+" + text;
                        }
                        else
                        {
                            MOA = text;
                        }
                    }
                    else if (header == "QTY")
                    {
                        streamWriter.WriteLine( "LIN:" + LIN + "'");
                        streamWriter.WriteLine("MOA:" + MOA + "'");
                        streamWriter.WriteLine(header + ":" + text + "'");
                    }
                    else
                    {
                        streamWriter.WriteLine(header + ":" + text + "'");
                    }
                }
            }
            /**
             * Generates the footer of the file
             */
            streamWriter.WriteLine("UNS+S'");
            streamWriter.WriteLine("UNT+" + counter + "'");
            streamWriter.Close();
            var lineCount = File.ReadLines(file).Count();
            File.AppendAllText(file, "UNZ+" + (lineCount + 1) + "'");
        }
        #endregion









        #region Write Product_List
        /**
         * Recreated write product list in the discussed fashion, using spaces to keep the segment lengths consistent regardless of data passed.
         * Check new file produced with KTN.
         * For now it is hardcoded for KTN due to the way EDI formats are.
         */
        public static void WriteProductList(SqlConnection con, string id)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
            Console.WriteLine(data.Rows.Count);
            int counter = 0;
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + id + "_Product_List.txt";
            StreamWriter streamWriter = new StreamWriter(file);
            streamWriter.WriteLine("UNH+00000001  +ITEMS               +R4        +KTN                                          +ITEMS                                                                      +OSPREY    +KTN       +" + DateTime.Now + "+204+" + id + "_Product_List.txt        '");


            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                List<string> columnNames = new List<string>();
                string[] nameArray = data.Columns.Cast<DataColumn>()
                             .Select(x => x.ColumnName)
                             .ToArray();
                string text = row["StockCode"].ToString();
                text = text.PadRight((30 - text.Length), ' ');
                streamWriter.WriteLine("LIN+" + text + "'");
                text = "";

                streamWriter.WriteLine("ACT+C'");
                streamWriter.WriteLine("NAD+CUS+OSPREY'");

                text = row["Name"].ToString();
                text = text.PadRight((60 - text.Length), ' ');
                streamWriter.WriteLine("IMD+" + text + "'");
                text = "";

                text = row["ProductGroup"].ToString();
                text = text.PadRight((20 - text.Length), ' ');
                streamWriter.WriteLine("GRI+ITG+" + text + "'");
                text = "";

                text = row["PartNumber"].ToString();
                text = text.PadRight((40 - text.Length), ' ');
                streamWriter.WriteLine("TRA+BC+" + text + "'");
                text = "";

                streamWriter.WriteLine("CFG+CONFIG1+Y+NEW'");
                streamWriter.WriteLine("PAD+PCS+Y'");
            }
            streamWriter.Close();
            var lineCount = File.ReadLines(file).Count();
            File.AppendAllText(file, "UNT+" + (lineCount + 1) + "+00000001  '");







        }
        #endregion
    }
}
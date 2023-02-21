﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using static System.Net.Mime.MediaTypeNames;
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

        #region Write Product List For Selected Warehouse Version 1 (Incorrect Formatting for KTN)
        //public static void WriteProductList(SqlConnection con, string id)
        //{
        //    DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
        //    Console.WriteLine(data.Rows.Count);
        //    int counter = 0;
        //    /**
        //     * Retrives the data from the database and then writes it line by line into a file.
        //     */
        //    string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + id + "_Product_List.txt";
        //    StreamWriter streamWriter = new StreamWriter(file);
        //    streamWriter.WriteLine("UNH:5056302200001+.?'");
        //    string text = "";
        //    string header = "";

        //    for (int i = 0; i < data.Rows.Count; i++)
        //    {
        //        DataRow row = data.Rows[i];
        //        List<string> columnNames = new List<string>();
        //        string[] nameArray = data.Columns.Cast<DataColumn>()
        //                     .Select(x => x.ColumnName)
        //                     .ToArray();
        //        /**
        //         * This sectin writes all the information in that data row into a single line in the EDI file.
        //         */
        //        for (int j = 0; j < data.Columns.Count; j++)
        //        {
        //            header = Lookups.WriteLookUp(nameArray[j]);
        //            text = row[j].ToString();
        //            streamWriter.WriteLine(header + ":" + text + "'");
        //        }
        //    }
        //    /**
        //         * Generates the footer of the file
        //         */
        //    streamWriter.WriteLine("UNS+S'");
        //    streamWriter.WriteLine("UNT+" + counter + "'");
        //    streamWriter.Close();
        //    var lineCount = File.ReadLines(file).Count();
        //    File.AppendAllText(file, "UNZ+" + (lineCount + 1) + "'");
        //}
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



        #region Write Products KTN Format
        public static int WriteProductsKTN (SqlConnection con, StreamWriter sw, string orderNo,int counter)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Write_Products_EDI", orderNo);
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            int item = 1;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];

                string text = (i+1).ToString();
                text = text.PadRight(30, ' ');
                text = text + row["ProductCode"].ToString();
                text = text.PadRight(55, ' ');
                text = text + row["ProductDescription"].ToString();
                text = text.PadRight(135, ' ');
                sw.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" + text.PadRight(165,' ') + "");
                text = "";
                counter++;

                //text = item.ToString();// row[""].ToString();            //Customer Stock Code
                //text = text.PadRight(108, ' ');
                ////text = text + row[""].ToString();
                //sw.WriteLine(counter.ToString().PadLeft(6, '0') + "PIADES" + text.PadRight(537, ' '));
                //text = "";
                //counter++;

                text = row["Quantity"].ToString();
                //Remove the decimal
                var temp = text.Split('.');
                text = "";
                foreach (string s in temp)
                {
                    text = text + s;
                }
                text = text.PadRight(18, ' ');
                sw.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYDEL" + text + "");
                text = "";
                counter++;

                text = row["UnitPrice"].ToString();
                //Remove the decimal
                temp = text.Split('.');
                text = "";
                foreach (string s in temp)
                {
                    text = text + s;
                }
                text = text + "0";
                text = text.PadRight(18, ' ');
                sw.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYPRC" + text + "");
                text = "";
                counter++;
                item++;
            }
            return counter;
        }
        #endregion

        #region Write Order For KTN
        public static void WriteOrder (SqlConnection con)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Write_Header_EDI");
            Console.WriteLine(data.Rows.Count);
            int counter = 13;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
                /**
                 * * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + row["OrderNumber"].ToString() + ".txt";
                string fileName = row["OrderNumber"].ToString() + ".txt";
                fileName = fileName.PadRight((35 - fileName.Length), ' ');
                StreamWriter streamWriter = new StreamWriter(file);
                Console.WriteLine(row["OrderNumber"]);
                streamWriter.WriteLine("000001UNH00000001            ORDER               R4        KTN                                          ORDER                                                                      OSPREY     KTN        " + DateTime.Now.ToString("yyyyMMddTHHmmss").PadRight(35, ' ') + "204" + fileName.PadRight(50, ' ') + "");

                string text = "";
                streamWriter.WriteLine("000002FACC  " + text.PadRight(35, ' ') + "");

                text = row["OrderType"].ToString();
                text = text.PadRight(30, ' ');
                streamWriter.WriteLine("000003TDT" + text.PadLeft(113, ' ') + "");
                text = "";

                text = row["Priority"].ToString();          //Oracle Order Number
                text = text.PadRight(80, ' ');
                streamWriter.WriteLine("000004RFFCR1" + text + "");
                text = "";

                text = row["OrderNumber"].ToString();                                      //Oracle Delivery Number
                text = text.PadRight(80, ' ');
                streamWriter.WriteLine("000005RFFCR2" + text + "");

                text = ""; //row[""].ToString();                                     //FCPN
                text = text.PadRight(80, ' ');
                streamWriter.WriteLine("000006RFFCR3" + text + "");
                text = "";

                text = row["OrderDate"].ToString();
                DateTime dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
                text = dateTime.ToString("yyyyMMdd");
                streamWriter.WriteLine("000007DTMDEL" + text.PadRight(35, ' ') + "102");     //Currently hardcoded as we do not have an eqevilant field

                text = row["OrderRequestedDate"].ToString();
                dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
                text = dateTime.ToString("yyyyMMdd");
                streamWriter.WriteLine("000008DTMLOA" + text.PadRight(35, ' ') + "102");     //Currently hardcoded as we do not have an eqevilant field

                string ID = row["DelPostalName"].ToString().PadRight(20, ' ').Substring(0, 10);
                text = ID;
                text = text.PadRight(20, ' ');
                text = text + row["DelPostalName"].ToString();
                text = text.PadRight(100, ' ');
                text = text + row["DelAddressLine1"].ToString() + "," + row["DelAddressLine2"].ToString() + "," + row["DelAddressLine3"].ToString() + "," + row["DelAddressLine4"].ToString();
                text = text.PadRight(180, ' ');
                text = text + row["DelPostCode"].ToString();
                text = text.PadRight(200, ' ');
                text = text + row["DelCity"].ToString();
                text = text.PadRight(280, ' ');
                text = text + row["DelCountryCode"].ToString();
                text = text.PadRight(290, ' ');
                if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                {
                    SqlConnection conDTC = new SqlConnection();
                    conDTC.ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString;
                    DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                    DataRow GDPR = GDPRData.Rows[0];
                    text = text + GDPR["TelephoneNo"].ToString(); //Phone Number
                    text = text.PadRight(340, ' ');
                    text = text + ""; // GDPR[""].ToString(); //Destination Contact
                    text = text.PadRight(443, ' ');
                    text = text + GDPR["EmailAddress"].ToString();
                    text = text.PadRight(493, ' ');
                    text = text + GDPR["PostalName"].ToString();  //Del Name 2
                    text = text.PadRight(766, ' ');
                    text = text + ""; //row[""].ToString();   //Del Address 2
                    text = text.PadRight(916, ' ');
                }
                else
                {
                    text = text.PadRight(443, ' ');
                    text = text + row["DelEmail"].ToString();
                    text = text.PadRight(493, ' ');
                }
                streamWriter.WriteLine("000009NADDES" + text.PadRight(996, ' ') + "");
                text = "";

                ID = row["InvoicePostalAddress"].ToString().Substring(0, 10);
                text = ID;
                text = text.PadRight(20, ' ');
                text = text + row["InvoicePostalAddress"].ToString();
                text = text.PadRight(100, ' ');
                text = text +  row["InvoiceAddressLine1"].ToString() + "," + row["InvoiceAddressLine2"].ToString() + "," + row["InvoiceAddressLine3"].ToString() + "," + row["InvoiceAddressLine4"].ToString();
                text = text.PadRight(180, ' ');
                text = text + row["InvoicePostCode"].ToString();
                text = text.PadRight(200, ' ');
                text = text + row["InvoiceCity"].ToString();
                text = text.PadRight(280, ' ');
                text = text + row["InvoiceCountryCode"].ToString();   
                text = text.PadRight(290, ' ');
                text = text + row["InvoiceCountry"].ToString();
                text = text.PadRight(653, ' ');
                text = text + row["CustomerVATCode"].ToString();
                text = text.PadRight(743, ' ');
                text = text + row["Currency"].ToString();
                streamWriter.WriteLine("000010NADINV" + text.PadRight(996, ' ') + "");
                text = "";

                text = "";// row[""].ToString();    // transporter Code
                //text = text.PadRight(( - text.Length), ' ');
                streamWriter.WriteLine("000011NADTRA" + text.PadRight(996, ' ') + "");
                text = "";

                //text = row["DeliveryRequirements"].ToString();
                //text = text.PadRight(123, ' ');
                //streamWriter.WriteLine("000012FTXDEL" + text + "");
                //text = "";

                text = row["Incoterms"].ToString();                                //Section reserved for incoterms
                text = text.PadRight(67, ' ');
                streamWriter.WriteLine("000012ALI" + text.PadLeft(204, ' ') + "");
                text = "";

                WriteProductsKTN(con, streamWriter, row["OrderNumber"].ToString(), counter);

                streamWriter.Close();
                var lineCount = File.ReadLines(file).Count();
                File.AppendAllText(file, (lineCount+1).ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
            }
        }
        #endregion

        #region Write ASN For KTN
        public static void WriteASNFile (SqlConnection con, string id)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_PO_DATA", id);
            Console.WriteLine(data.Rows.Count);
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            string file = "C:\\Bespoke\\EDI\\OutputFiles\\PO" + id + ".txt";
            StreamWriter streamWriter = new StreamWriter(file);
            string fileName = "PO" + id + ".txt";
            fileName = fileName.PadRight((35 - fileName.Length), ' ');
            streamWriter.WriteLine("000001UNH00000001            ASN                 R4        KTN                                           ASN                                                                       OSPREY     KTN       " + DateTime.Now.ToString("yyyyMMddTHHmmss") + "204" + fileName.PadRight((80 - fileName.Length), ' ') + "");
            streamWriter.WriteLine("000002FACC");

            int counter = 3;

            string text = "NORMAL";                                            //Currently hardcoded as we do not have an eqevilant field
            text = text.PadRight((80 - text.Length), ' ');
            streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFTYP" + text + "");

            text = "";
            counter++;

            text = data.Rows[0]["PurchaseOrderNumber"].ToString();
            text = text.PadRight((80 - text.Length), ' ');
            streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFCR " + text + "");
            text = "";
            counter++;

            text = data.Rows[0]["OrderRequestedDate"].ToString();
            DateTime dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
            text = dateTime.ToString("yyyyMMdd");
            streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text.PadRight(35, ' ') + "102");   //43 as it cuts off the time section however it is still counted using string.length
            text = "";
            counter++;

            streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "NADSUPKTN".PadRight(923, ' '));
            counter++;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];

                text = (i+1).ToString();
                text = text.PadRight(30, ' ');
                text = text + row["StockItemCode"].ToString();
                text = text.PadRight(55, ' ');
                text = text + row["ProductDescription"].ToString();
                text = text.PadRight(135, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" + text.PadRight(175, ' ') + "");
                text = "";
                counter++;

                text = row["Quantity"].ToString();
                var temp = text.Split('.');
                text = temp[0];
                text = text.PadRight((15 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYEXP" + text + "");
                text = "";
                counter++;

                //text = row["LotCode"].ToString();
                //text = text.PadRight((35 - text.Length), ' ');
                //streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRALNO" + text + "");
                //text = "";
                //counter++;

                text = row["OrderRequestedDate"].ToString();
                dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
                text = dateTime.ToString("yyyyMMdd").PadRight(35, ' '); //43 as it cuts off the time section however it is still counted using string.length
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text + "102");
                text = "";
                counter++;

                text = row["UnitPrice"].ToString();
                temp = text.Split('.');
                text = "";
                foreach (string s in temp)
                {
                    text = text + s;
                }
                text = text.Substring(0, text.Length - 3);
                text = text.PadRight(18, ' ');
                text = text + row["Currency"].ToString();
                text = text.PadRight(3, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MOA116" + text + "");
                text = "";
                counter++;
            }
            streamWriter.Close();
            var lineCount = File.ReadLines(file).Count();
            File.AppendAllText(file, counter.ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
        }
        #endregion

        #region Write Product List For KTN
        /**
         * Recreated write product list in the discussed fashion, using spaces to keep the segment lengths consistent regardless of data passed.
         * Check new file produced with KTN.
         * For now it is hardcoded for KTN due to the way EDI formats are.
         */
        public static void WriteProductList(SqlConnection con, string id)
        {
            DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
            Console.WriteLine(data.Rows.Count);
            /**
             * Retrives the data from the database and then writes it line by line into a file.
             */
            string file = "C:\\Bespoke\\EDI\\OutputFiles\\" + id + "_Product_List.txt";
            StreamWriter streamWriter = new StreamWriter(file);
            string fileName = id + "_Product_List.txt";
            fileName = fileName.PadRight(35, ' ');
            streamWriter.WriteLine("000001UNH00000001            ITEMS               R4        KTN                                          ITEMS                                                                      OSPREY     KTN        " + DateTime.Now.ToString("yyyyMMddTHHmmss").PadRight(35, ' ') + "204" + fileName.PadRight(50, ' ') + "");

            int counter = 2;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                DataRow row = data.Rows[i];
 
                string text = row["StockCode"].ToString();
                text = text.PadRight(25, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" +(i+1).ToString().PadRight(30) + text + "");
                text = "";
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "ACTC".PadRight(10, ' '));
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "NADCUSOSPREY              ".PadRight(80, ' '));
                counter++;

                text = row["Name"].ToString();
                text = text.PadRight(161, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "IMD" + text + "");
                text = "";
                counter++;

                text = row["ProductGroup"].ToString();
                text = text.PadRight(70, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "GRIITG" + text + "");
                text = "";
                counter++;

                text = row["PartNumber"].ToString();
                text = text.PadRight(35, ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRABC " + text + "");
                text = "";
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "CFGCONFIG1             YNEW         ");
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "PADPCS                      Y Y    Y");
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYSU 1                 ");
                counter++;

                //text = row["BoxWidth"].ToString().PadRight(12, ' ');
                //streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MEAWD " + text.PadRight(36, ' ') + "CMT");
                //counter++;

                //text = row["BoxHeight"].ToString().PadRight(12, ' ');
                //streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MEAHT " + text.PadRight(36, ' ') + "CMT");
                //counter++;

                //text = row["BoxLength"].ToString().PadRight(12, ' ');
                //streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MEADT " + text.PadRight(36, ' ') + "CMT");
                //counter++;

                if (row["BoxQuantity"].ToString() != "0")
                {
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "PADBOX                             ");
                    counter++;

                    text = row["BoxQuantity"].ToString().PadRight(15, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYSU " + text.PadRight(18, ' '));
                    counter++;
                }
            }
            streamWriter.Close();
            var lineCount = File.ReadLines(file).Count();
            File.AppendAllText(file, counter.ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
        }
        #endregion
    }
}
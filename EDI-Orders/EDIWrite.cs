using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using File = System.IO.File;

namespace EDI_Orders
{
    public class EDIWrite
    {
        #region KTN

        #region Write Products KTN Format
        /**
        * For now it is hardcoded for KTN due to the way EDI formats are.
        * The use of Padright/padleft is to ad spaces, due to the KTN format they require set cahrcter positions and set charcter lengths,
        * these pads ensure that the information is lined up correctly an it is readable by KTN.
        * The counter with pad left using 0 is the line number, again this is required in KTN's format.
        */
        public static int WriteProductsKTN(SqlConnection con, StreamWriter sw, string orderNo, int counter, string WHOrderNumber)
        {
            try
            {

                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_PRODUCTS_EDI", orderNo, WHOrderNumber);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                int item = 1;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];

                    string text = (i + 1).ToString();
                    text = text.PadRight(30, ' ');
                    text += row["ProductCode"].ToString();
                    text = text.PadRight(55, ' ');
                    text += row["ProductDescription"].ToString();
                    text = text.PadRight(135, ' ');
                    sw.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" + text.PadRight(165, ' ') + "");
                    text = "";
                    counter++;

                    text = row["Quantity"].ToString();
                    //Remove the decimal
                    var temp = text.Split('.');
                    text = "";
                    foreach (string s in temp)
                    {
                        text += s;
                    }
                    text = text.PadRight(18, ' ');
                    sw.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYDEL" + text + "");
                    text = "";
                    counter++;

                    text = row["UnitPrice"].ToString();
                    text = text.Replace('.', ',');
                    text += "0";
                    text = text.PadRight(18, ' ');
                    sw.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYPRC" + text + "");
                    text = "";
                    counter++;
                    item++;
                }

                return counter;
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write order items Failed to process, error message is: " + ex.Message + ex.ToString(), "");
                SharedFunctions.ErrorAlert("Write Order Products", ex);
                return counter;
            }
        }
        #endregion

        #region Write Order For KTN
        /**
        * For now it is hardcoded for KTN due to the way EDI formats are.
        * The use of Padright/padleft is to ad spaces, due to the KTN format they require set cahrcter positions and set charcter lengths,
        * these pads ensure that the information is lined up correctly an it is readable by KTN.
        * The counter with pad left using 0 is the line number, again this is required in KTN's format.
        */
        public static void WriteOrder(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_HEADER_EDI", id);
                int counter = 13;
                bool flag = false;
                con.Open();
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];
                    /**
                     * * Retrives the data from the database and then writes it line by line into a file.
                     */
                    string file = ConfigurationManager.AppSettings["Test"] + "/" + row["OrderNumber"].ToString() + ".txt";
                    string fileName = row["OrderNumber"].ToString() + ".txt";
                    fileName = fileName.PadRight((35 - fileName.Length), ' ');
                    FileStream f = new FileStream(file, FileMode.Create);
                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                    streamWriter.WriteLine("000001UNH00000001            ORDER               R4        KTN                                          ORDER                                                                      OSPREY     KTN        " + DateTime.Now.ToString("yyyyMMddHHmmss").PadRight(35, ' ') + "204" + fileName.PadRight(50, ' ') + "");

                    string text = "";
                    streamWriter.WriteLine("000002FACC  " + text.PadRight(35, ' ') + "");

                    text = row["OrderImportType"].ToString().PadRight(20, ' ').Substring(0, 10);
                    text = text.PadRight(30, ' ');
                    streamWriter.WriteLine("000003TDT" + text.PadLeft(113, ' ') + "");
                    text = "";

                    text = row["Priority"].ToString();          //Oracle Order Number
                    text = text.PadRight(80, ' ');
                    streamWriter.WriteLine("000004RFFCR1" + text + "");
                    text = "";

                    text = row["OrderNumber"].ToString() + row["WHOrderNumber"].ToString();                                      //Oracle Delivery Number
                    text = text.PadRight(80, ' ');
                    streamWriter.WriteLine("000005RFFCR2" + text + "");

                    text = row["OrderReference"].ToString();                                     //FCPN
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

                    string ID = row["CustomerAccountRef"].ToString();
                    if (ID == "")
                    {
                        ID = row["DelPostalName"].ToString().Substring(0, 10).PadRight(20, ' ');             //AccountRef
                    }
                    text = ID;            //Can be swapped for GLNs
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];


                        text = text.PadRight(20, ' ');
                        text += GDPR["PostalName"].ToString();
                        //text = text + "John Smith";
                        text = text.PadRight(100, ' ');
                        text += GDPR["AddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text += row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text += row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text += row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');

                        conDTC.Open();
                        SqlCommand Update = new SqlCommand("OSP_UPDATE_GDPR", conDTC)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        Update.Parameters.AddWithValue("@id", row["DelPostCode"].ToString());
                        Update.Parameters.AddWithValue("@id2", row["OrderReference"].ToString());
                        Update.Parameters.AddWithValue("@Date", DateTime.Now);
                        Update.Parameters.AddWithValue("@File", "WEB" + fileName);

                        Update.ExecuteNonQuery();
                        Update.Parameters.Clear();

                    }
                    else
                    {
                        text = text.PadRight(20, ' ');
                        text += row["DelPostalName"].ToString();
                        text = text.PadRight(100, ' ');
                        text += row["DelAddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text += row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text += row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text += row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');
                    }
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];
                        text += GDPR["TelephoneNo"].ToString(); //Phone Number
                        text = text.PadRight(473, ' ');
                        text += GDPR["PostalName"].ToString(); //Destination Contact
                        text = text.PadRight(523, ' ');
                        text += GDPR["EmailAddress"].ToString();
                        text = text.PadRight(653, ' ');
                        text += row["CustomerVATCode"].ToString();
                        text = text.PadRight(846, ' ');
                        text += GDPR["AddressLine2"].ToString(); // GDPR[""].ToString();  //Del add 2

                        flag = true;

                    }
                    else
                    {
                        text += row["DelTelephone"].ToString(); //Phone Number
                        text = text.PadRight(523, ' ');
                        text += row["DelEmail"].ToString();
                        text = text.PadRight(653, ' ');
                        text += row["CustomerVATCode"].ToString();
                        text = text.PadRight(846, ' ');
                        text += row["DelAddressLine2"].ToString(); // GDPR[""].ToString();  //Del Name 2
                    }

                    streamWriter.WriteLine("000009NADDES" + text.PadRight(996, ' ') + "");             //Can be swapped for GLNs but will need swapping to ensure the correct lcoation
                    text = "";

                    ID = row["CustomerAccountRef"].ToString();
                    if (ID == "")
                    {
                        ID = row["DelPostalName"].ToString().Substring(0, 10).PadRight(20, ' ');             //AccountRef
                    }
                    text = ID;            //Can be swapped for GLNs
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];


                        text = text.PadRight(20, ' ');
                        text += GDPR["PostalName"].ToString();
                        text = text.PadRight(100, ' ');
                        text += GDPR["AddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text += row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text += row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text += row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');

                        conDTC.Open();
                        SqlCommand Update = new SqlCommand("OSP_UPDATE_GDPR", conDTC)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        Update.Parameters.AddWithValue("@id", row["DelPostCode"].ToString());
                        Update.Parameters.AddWithValue("@id2", row["OrderReference"].ToString());
                        Update.Parameters.AddWithValue("@Date", DateTime.Now);
                        Update.Parameters.AddWithValue("@File", "WEB" + fileName);

                        Update.ExecuteNonQuery();
                        Update.Parameters.Clear();

                    }
                    else
                    {
                        text = text.PadRight(20, ' ');
                        text += row["DelPostalName"].ToString();
                        text = text.PadRight(100, ' ');
                        text += row["DelAddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text += row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text += row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text += row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');
                    }
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];
                        text += GDPR["TelephoneNo"].ToString(); //Phone Number
                        text = text.PadRight(473, ' ');
                        text += GDPR["PostalName"].ToString(); //Destination Contact
                        text = text.PadRight(523, ' ');
                        text += GDPR["EmailAddress"].ToString();
                        text = text.PadRight(653, ' ');
                        text += row["CustomerVATCode"].ToString();
                        text = text.PadRight(743, ' ');
                        text += row["Currency"].ToString();
                        text = text.PadRight(846, ' ');
                        text += GDPR["AddressLine2"].ToString();
                        flag = true;
                    }
                    else
                    {
                        text += row["DelTelephone"].ToString(); //Phone Number
                        text = text.PadRight(523, ' ');
                        text += row["DelEmail"].ToString();
                        text = text.PadRight(653, ' ');
                        text += row["CustomerVATCode"].ToString();
                        text = text.PadRight(743, ' ');
                        text += row["Currency"].ToString();
                        text = text.PadRight(846, ' ');
                        text += row["DelAddressLine2"].ToString();  //Del Add 2
                    }

                    streamWriter.WriteLine("000010NADINV" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = "";
                    streamWriter.WriteLine("000011NADTRA" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = row["Incoterms"].ToString();                                //Section reserved for incoterms
                    text = text.PadRight(10, ' ');
                    string temp = row["DelCity"].ToString().Substring(0, 25);
                    text = text + temp.PadRight(25, ' ');
                    text = text + row["DelCountryCode"].ToString().PadRight(3, ' ');
                    text = text.PadLeft(175, ' ');

                    streamWriter.WriteLine("000012ALI" + text.PadRight(204, ' ') + "");
                    text = "";

                    con.Close();
                    WriteProductsKTN(con, streamWriter, row["OrderNumber"].ToString(), counter, row["WHOrderNumber"].ToString());

                    streamWriter.Close();
                    var lineCount = File.ReadLines(file).Count();

                    FileInfo fi = new FileInfo(file);

                    File.AppendAllText(file, (lineCount + 1).ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
                    if (flag)
                    {
                        string name = Path.GetFileName(file);
                        File.Move(file, ConfigurationManager.AppSettings["Test"] + "/WEB" + name);
                    }
                    else if (fi.Length > 0)
                    {
                        File.Move(file, ConfigurationManager.AppSettings["Test"] + "/" + Path.GetFileName(file));
                    }
                    else
                    {
                        Exception ex = new Exception("Error: Blank file!");
                        SharedFunctions.ErrorAlert("Write Order", ex);
                    }
                    flag = false;
                    Console.WriteLine(i);
                    try
                    {
                        con.Open();
                        SqlCommand insertTracker = new SqlCommand("OSP_INSERT_TO_TRACKER", con)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        insertTracker.Parameters.AddWithValue("id2", row["OrderNumber"].ToString() + row["WHOrderNumber"].ToString());
                        insertTracker.Parameters.AddWithValue("id", fileName);


                        insertTracker.ExecuteNonQuery();
                        insertTracker.Parameters.Clear();
                        con.Close();
                        //SharedFunctions.QueryDB(con, "OSP_INSERT_TO_TRACKER", fileName, row["OrderNumber"].ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("Failed on write tracker: " + ex.Message, "");
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Order Failed to process, error message is: " + ex.Message + ex.ToString(), "");
                SharedFunctions.ErrorAlert("Write Order", ex);
            }
        }
        #endregion

        #region Write ASN For KTN
        /**
        * For now it is hardcoded for KTN due to the way EDI formats are.
        * The use of Padright/padleft is to ad spaces, due to the KTN format they require set cahrcter positions and set charcter lengths,
        * these pads ensure that the information is lined up correctly an it is readable by KTN.
        * The counter with pad left using 0 is the line number, again this is required in KTN's format.
        */
        public static void WriteASNFile(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_PO_DATA", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "/PO" + id + ".txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                fileName = fileName.PadRight((35 - fileName.Length), ' ');
                streamWriter.WriteLine("000001UNH00000001            ASN                 R4        KTN                                           ASN                                                                       OSPREY     KTN       " + DateTime.Now.ToString("yyyyMMddHHmmss") + "204" + fileName.PadRight((80 - fileName.Length), ' ') + "");
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

                text = data.Rows[0]["SupplierDocumentNumber"].ToString();
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFSUP" + text + "");
                text = "";
                counter++;

                text = data.Rows[0]["OrderRequestedDate"].ToString();
                DateTime dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
                text = dateTime.ToString("yyyyMMdd");
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text.PadRight(35, ' ') + "102");
                text = "";
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "NADSUPKTN".PadRight(923, ' '));
                counter++;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];

                    text = (i + 1).ToString();
                    text = text.PadRight(30, ' ');
                    text += row["StockItemCode"].ToString();
                    text = text.PadRight(55, ' ');
                    text += row["ProductDescription"].ToString();
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

                    text = row["UnitPrice"].ToString();
                    temp = text.Split('.');
                    text = "";
                    foreach (string s in temp)
                    {
                        text += s;
                    }
                    text = text.Substring(0, text.Length - 3);
                    text = text.PadRight(18, ' ');
                    text += row["Currency"].ToString();
                    text = text.PadRight(3, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MOA116" + text + "");
                    text = "";
                    counter++;
                }
                streamWriter.Close();
                var lineCount = File.ReadLines(file).Count();
                File.AppendAllText(file, counter.ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("PO Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write ASN", ex);
            }
        }
        #endregion

        #region Write Product List For KTN
        /**
         * For now it is hardcoded for KTN due to the way EDI formats are.
         * The use of Padright/padleft is to ad spaces, due to the KTN format they require set cahrcter positions and set charcter lengths,
         * these pads ensure that the information is lined up correctly an it is readable by KTN.
         * The counter with pad left using 0 is the line number, again this is required in KTN's format.
         */
        public static void WriteProductList(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "/" + id + "_Product_List.txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = id + "_Product_List.txt";
                fileName = fileName.PadRight(35, ' ');
                streamWriter.WriteLine("000001UNH00000001            ITEMS               R4        KTN                                          ITEMS                                                                      OSPREY     KTN        " + DateTime.Now.ToString("yyyyMMddHHmmss").PadRight(35, ' ') + "204" + fileName.PadRight(50, ' ') + "");

                int counter = 2;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];

                    string text = row["StockCode"].ToString();
                    text = text.PadRight(25, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" + (i + 1).ToString().PadRight(30) + text + "");
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

                    text = row["Category"].ToString();
                    text = text.PadRight(70, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "GRIITG" + text + "");
                    text = "";
                    counter++;

                    text = row["CountryOrigin"].ToString();     //Country Of origin field to be added
                    text = text.PadRight(42, ' ');
                    text += "YY";
                    text = text.PadRight(50, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRACOU" + text + "");
                    text = "";
                    counter++;

                    text = row["Colour"].ToString();      //This is the color field being added
                    text = text.PadRight(113, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "CHACOL" + text + "");
                    text = "";
                    counter++;

                    text = row["StyleCode"].ToString();      //This is the color field being added
                    text = text.PadRight(113, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "CHADES" + text + "");
                    text = "";
                    counter++;

                    text = row["PartNumber"].ToString();  //Barcode
                    text = text.PadRight(35, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRAEAN" + text + "");
                    text = "";
                    counter++;

                    text = row["BoxUPCCode"].ToString();  //Barcode for the Box
                    text = text.PadRight(35, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRABC " + text + "");
                    counter++;

                    text = row["HSCode"].ToString();  //Barcode for the Box
                    text = text.PadRight(35, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "TRACUS" + text + "");
                    counter++;

                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "CFGCONFIG1             YNEW         ");
                    counter++;

                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "PADPCS                      Y Y   Y");
                    counter++;

                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYSU 1                 ");
                    counter++;

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
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Produc List Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Product List", ex);
            }
        }
        #endregion

        #region Truck dels warehouse to warehouse
        /**
         * Is hardcoded for DSV to KTN
         */
        public static void WriteTruckDelsFile(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_DSV_TO_KTN_MOVEMENT", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "TRUCK" + id + ".txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                fileName = fileName.PadRight((35 - fileName.Length), ' ');
                streamWriter.WriteLine("000001UNH00000001            ASN                 R4        KTN                                           ASN                                                                       OSPREY     KTN       " + DateTime.Now.ToString("yyyyMMddHHmmss") + "204" + fileName.PadRight((80 - fileName.Length), ' ') + "");
                streamWriter.WriteLine("000002FACC");

                int counter = 3;

                string text = "Move";                                            //Currently hardcoded as we do not have an eqevilant field
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFTYP" + text + "");

                text = "";
                counter++;

                text = data.Rows[0]["DTK_TruckNo"].ToString();
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFCR " + text + "");
                text = "";
                counter++;

                text = DateTime.Now.ToShortDateString();
                DateTime dateTime = DateTime.ParseExact(text, "dd/MM/yyyy", null);    // hh:mm:ss
                text = dateTime.ToString("yyyyMMdd");
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text.PadRight(35, ' ') + "102");   //43 as it cuts off the time section however it is still counted using string.length
                text = "";
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "NADSUPDSV".PadRight(923, ' '));
                counter++;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];

                    text = (i + 1).ToString();
                    text = text.PadRight(30, ' ');
                    text += row["DTK_SKU"].ToString();
                    text = text.PadRight(55, ' ');
                    text += "";
                    text = text.PadRight(135, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "LIN" + text.PadRight(175, ' ') + "");
                    text = "";
                    counter++;

                    text = row["DTK_Item_Qty"].ToString();
                    var temp = text.Split('.');
                    text = temp[0];
                    text = text.PadRight((15 - text.Length), ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYEXP" + text + "");
                    text = "";
                    counter++;

                    text = DateTime.Now.ToShortDateString();
                    dateTime = DateTime.ParseExact(text, "dd/MM/yyyy", null);
                    text = dateTime.ToString("yyyyMMdd").PadRight(35, ' '); //43 as it cuts off the time section however it is still counted using string.length
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text + "102");
                    text = "";
                    counter++;
                }
                streamWriter.Close();
                var lineCount = File.ReadLines(file).Count();
                File.AppendAllText(file, counter.ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Warehouse to warehouse Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Truck Del To Warehouse", ex);
            }

        }
        #endregion

        #region Returns to KTN
        public static void WriteReturnResponse(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_RETURN_DATA", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "/RETURN" + id + ".txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                fileName = fileName.PadRight((35 - fileName.Length), ' ');
                streamWriter.WriteLine("000001UNH00000001            ASN                 R4        KTN                                           ASN                                                                       OSPREY     KTN       " + DateTime.Now.ToString("yyyyMMddHHmmss") + "204" + fileName.PadRight((80 - fileName.Length), ' ') + "");
                streamWriter.WriteLine("000002FACC");

                int counter = 3;

                string text = "RETURN";
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFTYP" + text + "");

                text = "";
                counter++;

                text = data.Rows[0]["ReturnOrderNumber"].ToString();
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFCR " + text + "");
                text = "";
                counter++;

                text = data.Rows[0]["EDIReturnRef"].ToString();
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFCR2" + text + "");
                text = "";
                counter++;

                text = data.Rows[0]["CustomerDocumentNumber"].ToString();
                text = text.PadRight((80 - text.Length), ' ');
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "RFFSUP" + text + "");
                text = "";
                counter++;


                text = data.Rows[0]["ReturnRequestedDate"].ToString();
                DateTime dateTime = DateTime.ParseExact(text, "dd/MM/yyyy hh:mm:ss", null);
                text = dateTime.ToString("yyyyMMdd");
                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "DTMPLA" + text.PadRight(35, ' ') + "102");
                text = "";
                counter++;

                streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "NADSUPKTN".PadRight(923, ' '));
                counter++;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];


                    text = (i + 1).ToString();
                    text = text.PadRight(30, ' ');
                    text += row["StockItemCode"].ToString();
                    text = text.PadRight(55, ' ');
                    text += row["ProductDescription"].ToString();
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

                    text = row["UnitPrice"].ToString();
                    temp = text.Split('.');
                    text = "";
                    foreach (string s in temp)
                    {
                        text += s;
                    }
                    text = text.Substring(0, text.Length - 3);
                    text = text.PadRight(18, ' ');
                    text += row["Currency"].ToString();
                    text = text.PadRight(3, ' ');
                    streamWriter.WriteLine(counter.ToString().PadLeft(6, '0') + "MOA116" + text + "");
                    text = "";
                    counter++;
                }
                streamWriter.Close();
                var lineCount = File.ReadLines(file).Count();
                File.AppendAllText(file, counter.ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("PO Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Return", ex);
            }
        }
        #endregion

        #endregion



        #region Great Bear
        #region Write Order Header
        public static void WriteOrderGB(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_HEADER_EDI", id);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    con.Open();
                    DataRow row = data.Rows[i];
                    /**
                     * * Retrives the data from the database and then writes it line by line into a file.
                     */
                    string file = ConfigurationManager.AppSettings["Test"] + "/" + row["OrderNumber"].ToString() + ".txt";
                    string fileName = row["OrderNumber"].ToString() + ".txt";
                    FileStream f = new FileStream(file, FileMode.Create);
                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);

                    string DateFormatting = DateFormatter(row["OrderRequestedDate"].ToString());

                    Console.WriteLine(data.Rows.Count);
                    Console.WriteLine(row["OrderNumber"]);

                    WritePLHeader(streamWriter, "940");

                    /**
                     * Writes the delivery information for the an order in the X12 format.
                     */

                    streamWriter.WriteLine("W05*C*" + row["OrderNumber"] + "*" + row["OrderReference"] + "*****" + row["OrderImporttype"] + "-" + row["Priority"] + "~");
                    streamWriter.WriteLine("N1*DE*Osprey Europe*9*5056302200001~");
                    #region GDPR data insert
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];

                        streamWriter.WriteLine("N1*BP*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N1*BT*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N3*" + GDPR["AddressLine1"] + "*" + GDPR["AddressLine2"] + "~");
                        streamWriter.WriteLine("N4*" + row["DelCity"] + "**" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                        streamWriter.WriteLine("N1*ST*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N3*" + GDPR["AddressLine1"] + "*" + GDPR["AddressLine2"] + "~");
                        streamWriter.WriteLine("N4*" + GDPR["AddressLine3"] + "*" + GDPR["AddressLine4"] + "*" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                    }
                    else
                    {
                        streamWriter.WriteLine("N1*BP*" + row["InvoicePostalAddress"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N1*BT*" + row["InvoicePostalAddress"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N3*" + row["InvoiceAddressLine1"] + "*" + row["InvoiceAddressLine2"] + "~");
                        streamWriter.WriteLine("N4*" + row["InvoiceCity"] + "**" + row["InvoicePostCode"] + "*" + row["InvoiceCountry"] + "~");
                        streamWriter.WriteLine("N1*ST*" + row["DelPostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.WriteLine("N3*" + row["DelAddressLine1"] + "*" + row["DelAddressLine2"] + "~");
                        streamWriter.WriteLine("N4*" + row["DelAddressLine3"] + "*" + row["DelAddressLine4"] + "*" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                    }
                    #endregion
                    streamWriter.WriteLine("N9*BR*~");
                    streamWriter.WriteLine("G62*02*" + DateFormatting + "~");
                    streamWriter.WriteLine("W66*M*~");
                    con.Close();

                    int total = WriteItemsGB(con, streamWriter, id, row["WHOrderNumber"].ToString());
                    //WriteItemsGB(con, streamWriter, id, row["WHOrderNumber"].ToString());
                    streamWriter.WriteLine("W79*" + total + "~");

                    WritePLFooter(streamWriter, data.Rows.Count, total + 15);

                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write order body for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write order body for GB ", ex);
            }
        }
        #endregion

        #region Date Formatter
        public static string DateFormatter(string date)
        {
            string DateFormatting = date;
            DateFormatting = DateFormatting.Substring(0, 10);
            string[] splitDate = DateFormatting.Split('/', '\t');
            DateFormatting = splitDate[2] + splitDate[1] + splitDate[0];
            return DateFormatting;
        }
        #endregion

        #region Write Order Lines
        public static int WriteItemsGB(SqlConnection con, StreamWriter sw, string id, string id2)
        {
            try
            {
                Console.WriteLine("Start of wrte lines");
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_PRODUCTS_EDI", id, id2);
                int total = 0;
                int counter = 1;
                int totalQty = 0;
                Console.WriteLine(data.Rows.Count);
                foreach (DataRow row in data.Rows)
                {
                    sw.WriteLine("LX*" + counter + "~");
                    sw.WriteLine("W01*" + row["Quantity"] + "*Each*" + row["PartNumber"] + "*VN*" + row["ProductCode"] + "*BP*******~");
                    totalQty = totalQty + (Int32.Parse(row["Quantity"].ToString()));
                    sw.WriteLine("N9*KK*" + row["SageLineID"] + "~");
                    counter += 3;
                }
                Console.WriteLine("End of wrte lines");
                return total;
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write items for Great Bear order failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write items for GB order ", ex);
                return -1;
            }
        }
        #endregion

        #region Product List individual files
        public static void WriteProductListGBItems (SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
                foreach (DataRow r in data.Rows)
                {
                    /**
                     * Retrives the data from the database and then writes it line by line into a file.
                     */
                    string file = ConfigurationManager.AppSettings["Test"] + "/" + "GREATBEAR" + "_Product_List_Item_" + r[0].ToString() + ".txt";
                    FileStream f = new FileStream(file, FileMode.Create);
                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                    string fileName = id + "_Product_List.txt";
                    fileName = fileName.PadRight(35, ' ');

                    int counter = 1;

                    WritePLHeader(streamWriter, "846");

                    streamWriter.WriteLine("ST*846*" + counter + "~");
                    streamWriter.WriteLine("LIN**VN*" + r[0].ToString() + "~");
                    streamWriter.WriteLine("PID*****" + r[1].ToString() + "~");
                    streamWriter.WriteLine("RFF*LEV*1*EA~");
                    streamWriter.WriteLine("RFF*EAN*" + r[3].ToString() + "~");
                    streamWriter.WriteLine("RFF*LEV*2*IP~");
                    streamWriter.WriteLine("RFF*LEV*3*CAS~");
                    streamWriter.WriteLine("RFF*EAN*" + r[4].ToString() + "*3~");
                    streamWriter.WriteLine("RFF*CGW*" + r[10].ToString() + "*KG~");
                    streamWriter.WriteLine("RFF*CHM*" + r[5].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CWM*" + r[7].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CLM*" + r[6].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CQT*" + r[2].ToString() + "*EA~");
                    streamWriter.WriteLine("SE*12*" + counter + "~");
                    counter++;
                    WritePLFooter(streamWriter, data.Rows.Count, 8);
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Product List Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Product List", ex);
            }
        }
        #endregion

        #region Write Product List
        public static void WriteProductListGB(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "/" + "GREATBEAR" + "_Product_List.txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = id + "_Product_List.txt";
                fileName = fileName.PadRight(35, ' ');

                int counter = 1;
                Console.WriteLine(data.Rows.Count);
                foreach (DataRow r in data.Rows)
                {
                    WritePLHeader(streamWriter, "846");

                    streamWriter.WriteLine("ST*846*" + counter + "~");
                    streamWriter.WriteLine("LIN**VN*" + r[0].ToString() + "~");
                    streamWriter.WriteLine("PID*****" + r[1].ToString() + "~");
                    streamWriter.WriteLine("RFF*LEV*1*EA~");
                    streamWriter.WriteLine("RFF*EAN*" + r[3].ToString() + "~");
                    streamWriter.WriteLine("RFF*LEV*2*IP~");
                    streamWriter.WriteLine("RFF*LEV*3*CAS~");
                    streamWriter.WriteLine("RFF*EAN*" + r[4].ToString() + "*3~");
                    streamWriter.WriteLine("RFF*CGW*" + r[10].ToString() + "*KG~");
                    streamWriter.WriteLine("RFF*CHM*" + r[5].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CWM*" + r[7].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CLM*" + r[6].ToString() + "*CM~");
                    streamWriter.WriteLine("RFF*CQT*" + r[2].ToString() + "*EA~");
                    streamWriter.WriteLine("SE*12*" + counter + "~");
                    counter++;
                    WritePLFooter(streamWriter, data.Rows.Count, 8);
                }
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Product List Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Product List", ex);
            }

        }
        #endregion

        #region Product List Header
        public static void WritePLHeader(StreamWriter sw, string MessageType)
        {
            try
            {
                sw.WriteLine("ISA*00*          *00*          *01*Osprey Europe  *ZZ*GreatBear      *" + DateTime.Now.ToString("ddMMyy") + "*" + DateTime.Now.ToString("hhmm") + "*U*004010*200*0*P~");
                sw.WriteLine("GS*IB*Osprey Europe*GreatBear*" + DateTime.Now.ToString("yyyyMMdd") + "*" + DateTime.Now.ToString("hhmm") + "*1*X*004010~");
                //sw.WriteLine("ST*" + MessageType + "*1~");
                //sw.WriteLine("BIA*C~");
                //sw.WriteLine("BSN*00*1*" + DateTime.Now.ToString("yyyyMMdd") + "~");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write header for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Header for GB", ex);
            }
        }
        #endregion

        #region Product List Footer
        public static void WritePLFooter(StreamWriter sw, int NoOfSegs, int lines)
        {
            try
            {
                //sw.WriteLine("SE*" + NoOfSegs + "*1~");
                sw.WriteLine("GE*" + NoOfSegs + "*1~");
                sw.WriteLine("IEA*" + (lines + 3) + "*200~");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write footer for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write footer for GB", ex);
            }
        }
        #endregion

        #region Write Purchase Order
        public static void WritePOGB(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_PO_DATA", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["Test"] + "/PO" + id + ".txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                int counter = 1;

                WritePLHeader(streamWriter, fileName);

                foreach (DataRow row in data.Rows)
                {
                    int QTY = convertToInt(row["Quantity"].ToString());
                    string DateFormatting = DateFormatter(row["OrderRequestedDate"].ToString());
                    streamWriter.WriteLine("ST*846*" + row["PrimaryKey"] + "~");
                    streamWriter.WriteLine("BSN*00*" + "PO" + id + "." + row["PrimaryKey"] + "*" + DateFormatting + "~");
                    streamWriter.WriteLine("HL*1**S~");
                    streamWriter.WriteLine("N1*SF**ZZ*" + row["SuppAccRef"].ToString().Replace("$", "") + "~");                           //Replace $
                    streamWriter.WriteLine("HL*2*" + row["PrimaryKey"] + "*O~");
                    streamWriter.WriteLine("PRF*" + id + "~");
                    streamWriter.WriteLine("HL*3*" + row["PrimaryKey"] + "*I~");
                    streamWriter.WriteLine("LIN*1*VN*" + row["StockItemCode"] + "~");
                    streamWriter.WriteLine("REF*ZZ*~");
                    streamWriter.WriteLine("SN1**" + QTY + "*EA~");
                    streamWriter.WriteLine("CTT*3*" + QTY + "~");
                    streamWriter.WriteLine("SE*12*" + row["PrimaryKey"] + "~");
                    counter++;
                }

                WritePLFooter(streamWriter, data.Rows.Count, 8);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write PO for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write PO for GB", ex);
            }
        }
        #endregion

        #region Convert To Int
        public static int convertToInt (string val)
        {
            int intVal = 0;
            string[] splitVal = val.Split('.');
            string trimmed = splitVal[0];
            intVal = Int32.Parse(trimmed); 
            return intVal;
        }
        #endregion
        #endregion

    }
}
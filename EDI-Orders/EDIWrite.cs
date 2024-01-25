using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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
                SharedFunctions.Writefile("OrderNumber: " + orderNo + " Write order items Failed to process, error message is: " + ex.Message + ex.ToString(), "");
                SharedFunctions.ErrorAlert("OrderNumber: " + orderNo + " Write Order Products", ex);
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
                    string file = ConfigurationManager.AppSettings["KTNOrders"] + "/" + row["OrderNumber"].ToString() + ".txt";
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
                        Console.WriteLine("error in query");
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        Console.WriteLine("Error on row reading       " + row["DelPostCode"].ToString() + "     " + row["OrderReference"].ToString() + "     " + GDPRData.ToString());
                        DataRow GDPR = GDPRData.Rows[0];
                        Console.WriteLine("Passed query");

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
                    Console.WriteLine("Made past first DTC");
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
                    Console.WriteLine("Made second DTC");
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
                    Console.WriteLine("Made past third DTC");
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
                    Console.WriteLine("Made past all DTC");
                    streamWriter.WriteLine("000010NADINV" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = "";
                    streamWriter.WriteLine("000011NADTRA" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = row["Incoterms"].ToString();                                //Section reserved for incoterms
                    text = text.PadRight(10, ' ');
                    string temp = row["DelCity"].ToString().PadRight(50, ' ').Substring(0, 25);
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
                        File.Move(file, ConfigurationManager.AppSettings["GDPROrder"] + "/WEB" + name);
                    }
                    else if (fi.Length > 0)
                    {
                        File.Move(file, ConfigurationManager.AppSettings["KTNOrders"] + "/" + Path.GetFileName(file));
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
                SharedFunctions.Writefile("OrderNumber: " + id + " Order Failed to process, error message is: " + ex.Message + ex.ToString(), "");
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
                SharedFunctions.Writefile("PO Failed to write: " + id + " PO Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("PO Failed to write: " + id + " Write ASN", ex);
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
                string file = ConfigurationManager.AppSettings["KTNItems"] + "/" + id + "_Product_List.txt";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = id + "_Product_List.txt";
                fileName = fileName.PadRight(35, ' ');
                streamWriter.WriteLine("000001UNH00000001            ITEMS               R4        KTN                                          ITEMS                                                                      OSPREY     KTN        " + DateTime.Now.ToString("yyyyMMddHHmmss").PadRight(35, ' ') + "204" + fileName.PadRight(50, ' ') + "");

                Console.WriteLine(data.Rows.Count);

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

                SharedFunctions.UpdateRecords(con, "OSP_UPDATE_PRODUCTLIST_SENT", "100994002");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Produc List Failed to process for KTN, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Product List for KTN", ex);
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
                SharedFunctions.Writefile("Warehouse to warehouse Failed to process for PO: " + id + ", error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Truck Del To Warehouse for PO " + id + "", ex);
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
                SharedFunctions.Writefile("Return: " + id + " Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Return: " + id + " failed to process.", ex);
            }
        }
        #endregion

        #endregion

       

        #region Great Bear
        #region Write Order Header
        /**
         * This section writes the inital aprt of the great bear order files,
         * This requires a db connection string and an id to be passed,
         * The ID is the order number to be written, the connection is the db to be used.
         */
        public static void WriteOrderGB(SqlConnection con, string id)
        {
            try
            {
                SqlConnection Orbis = new SqlConnection
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
                };
                /**
                 * This section is gathering the counters that are meant to progressively incriment each time a file is sent and entering them int sueable variables.
                 */
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_HEADER_EDI", id);
                DataTable GBCounters = SharedFunctions.QueryDB(Orbis, "OSP_GET_GBITEMS_VALS", "1", "2");
                DataRow GEGS = GBCounters.Rows[0];
                DataRow SEST = GBCounters.Rows[1];
                DataRow ISAIEA = GBCounters.Rows[2];
                int GEGSVal = Int32.Parse(GEGS[2].ToString());
                int SESTVal = Int32.Parse(SEST[2].ToString());
                int ISAIEAVal = Int32.Parse(ISAIEA[2].ToString());
                int total = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    bool GDPRFlag = false;
                    con.Open();
                    DataRow row = data.Rows[i];
                    /**
                     * Retrives the data from the database and then writes it line by line into a file.
                     */
                    string file = ConfigurationManager.AppSettings["GeneratingGB"] + "/" + row["OrderNumber"].ToString() + ".edi";
                    string fileName = row["OrderNumber"].ToString() + ".txt";
                    FileStream f = new FileStream(file, FileMode.Create);
                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                    string DateFormatting = DateFormatter(row["OrderRequestedDate"].ToString());
                    WritePLHeader(streamWriter, "940", GEGS[2].ToString(), ISAIEAVal.ToString());

                    /**
                     * Writes the delivery information for the an order in the X12 format.
                     */
                    streamWriter.Write("ST*940*" + (SESTVal).ToString() + "~");
                    streamWriter.Write("W05*C*" + row["OrderNumber"] + "-" + row["WHOrderNumber"] + "*" + row["OrderReference"] + "*****" + row["OrderImporttype"] + "-" + row["Priority"] + "~");
                    streamWriter.Write("N1*DE*Osprey Europe*9*5056302200001~");
                    #region GDPR data insert
                    /** 
                     * This section is in place to distinguish between GDPR and none GDPR orders so the correct data is inserted
                     */
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        GDPRFlag = true;
                        SqlConnection conDTC = new SqlConnection
                        {
                            ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString
                        };
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];
                        streamWriter.Write("N1*BP*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N1*BT*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N3*" + GDPR["AddressLine1"] + "~");                                                                                                 //+ "*" + GDPR["AddressLine2"] 
                        streamWriter.Write("N4*" + row["DelCity"] + "**" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                        streamWriter.Write("N1*ST*" + GDPR["PostalName"] + "*91*" + row["CustomerAccountRef"] + "-" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N3*" + GDPR["AddressLine1"] + "~");                                                                                              //+ "*" + GDPR["AddressLine2"] 
                        streamWriter.Write("N4*" + row["DelCity"] + "*" + GDPR["AddressLine3"] + "*" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                        streamWriter.Write("PER*CN*" + GDPR["PostalName"] + "*EM*" + GDPR["EmailAddress"] + "*TE*" + GDPR["TelephoneNo"] + "~");
                    }
                    else
                    {
                        streamWriter.Write("N1*BP*" + row["InvoicePostalAddress"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N1*BT*" + row["InvoicePostalAddress"] + "*91*" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N3*" + row["InvoiceAddressLine1"] + "~");                                                                                          // + "*" + row["InvoiceAddressLine2"]
                        streamWriter.Write("N4*" + row["InvoiceCity"] + "**" + row["InvoicePostCode"] + "*" + row["DelCountryCode"] + "~");
                        streamWriter.Write("N1*ST*" + row["DelPostalName"] + "*91*" + row["CustomerAccountRef"] + "-" + row["CustomerAccountRef"] + "~");
                        streamWriter.Write("N3*" + row["DelAddressLine1"] + "~");                                                                                                  //+ "*" + row["DelAddressLine2"]
                        streamWriter.Write("N4*" + row["DelCity"] + "*" + row["DelAddressLine3"] + "*" + row["DelPostCode"] + "*" + row["DelCountryCode"] + "~");
                        streamWriter.Write("PER*CN*" + row["DelPostalName"] + "*EM*" + row["DelEmail"] + "*TE*" + row["DelTelephone"] + "~");
                    }
                    #endregion

                    streamWriter.Write("G62*02*" + DateFormatting + "~");
                    streamWriter.Write("W66*PD*M***JDF~");
                    con.Close();
                    /**
                     * Retrives the total number of items and lines ready to be inserted into the final line of the footer.
                     */
                    total = WriteItemsGB(con, streamWriter, id, row["WHOrderNumber"].ToString(), SESTVal);
                    streamWriter.Write("W79*" + total + "~");
                    streamWriter.Write("SE*" + (total + 12) + "*" + (SESTVal).ToString() + "~");
                    WritePLFooter(streamWriter, data.Rows.Count, total + 15, GEGS[2].ToString(), ISAIEAVal.ToString());
                    streamWriter.Close();
                    /**
                     * This distinguishes as to where the file is placed requiring GDPR data to be placed in a different network to non GDPR orders.
                     */
                    if (GDPRFlag)
                    {
                        File.Move(file, ConfigurationManager.AppSettings["GDPROrderGB"] + "/" + "WEB" + Path.GetFileName(file));
                    }
                    else
                    {
                        File.Move(file, ConfigurationManager.AppSettings["GBOrders"] + "/" + Path.GetFileName(file));
                    }

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
                        SharedFunctions.QueryDB(con, "OSP_INSERT_TO_TRACKER", fileName, row["OrderNumber"].ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("Failed on write tracker: " + ex.Message, "");
                    }

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
                        SharedFunctions.QueryDB(con, "OSP_INSERT_TO_TRACKER", fileName, row["OrderNumber"].ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("Failed on write tracker: " + ex.Message, "");
                    }
                }
                SharedFunctions.UpdateCounters(Orbis, "OSP_UPDATE_GBITEMS_VALS", "1", "2", "3", (GEGSVal + 1).ToString(), (SESTVal + 1).ToString(), (ISAIEAVal + total).ToString());
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write order: " + id + " body for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write order: " + id + " body for GB ", ex);
            }
        }
        #endregion

        #region Date Formatter
        /**
         * This wass a recurring poblem of GB wanting dates in a specific format so this function does this.
         */
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
        /**
         * This section writes the orders items into the file.
         * This is done here as it can run multiple times and the data is stored in a seperate table.
         */
        public static int WriteItemsGB(SqlConnection con, StreamWriter sw, string id, string id2, int SESTVal)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_PRODUCTS_EDI", id, id2);
                int total = 0;
                int counter = 1;
                int totalQty = 0;
                /**
                 * Loops through every item in that order and writes relevant details.
                 */
                foreach (DataRow row in data.Rows)
                {
                    sw.Write("LX*" + counter + "~");
                    sw.Write("W01*" + row["Quantity"] + "*EA*" + row["PartNumber"].ToString().TrimStart('0') + "*VN*" + row["ProductCode"] + "********FG~");
                    sw.Write("G69*" + row["ProductCode"] + "~");
                    totalQty = totalQty + (Int32.Parse(row["Quantity"].ToString()));
                    sw.Write("N9*KK*" + row["SageLineID"] + "~");
                    counter ++;
                    total++;
                }
                return total;
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write items for Great Bear order: " + id + " failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write items for GB order: " + id, ex);
                return -1;
            }
        }
        #endregion

        #region Product List individual files
        /**
         * This solution creates a single file for each item that is to be sent across to GB,
         * This is required to allow for the files to be successfully processed.
         */
        public static void WriteProductListGBItems (SqlConnection con, string id)
        {
            try
            {
                SqlConnection Orbis = new SqlConnection
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
                };
                /**
                 * Loops through every item in that order and writes relevant details.
                 */
                DataTable data = SharedFunctions.QueryDB(con, "OSP_Get_Product_List", id);
                DataTable GBCounters = SharedFunctions.QueryDB(Orbis, "OSP_GET_GBITEMS_VALS", "1", "2");
                DataRow GEGS = GBCounters.Rows[0];
                DataRow SEST = GBCounters.Rows[1];
                DataRow ISAIEA = GBCounters.Rows[2];
                int GEGSVal = Int32.Parse(GEGS[2].ToString());
                int SESTVal = Int32.Parse(SEST[2].ToString());
                int ISAIEAVal = Int32.Parse(ISAIEA[2].ToString());
                int itemCount = 1;
                /**
                 * Loops for each item and creates a new file per item.
                 */
                foreach (DataRow r in data.Rows)
                {
                    /**
                     * Retrives the data from the database and then writes it line by line into a file.
                     */
                    string file = ConfigurationManager.AppSettings["GeneratingGB"] + "/" + "GREATBEAR" + "_Product_List_Item_" + itemCount + ".edi";
                    FileStream f = new FileStream(file, FileMode.Create);
                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);  
                    StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                    string fileName = id + "_Product_List.txt";
                    fileName = fileName.PadRight(35, ' ');

                    int counter = 1;

                    WritePLHeader(streamWriter, "846", GEGSVal.ToString(), ISAIEAVal.ToString());
                    streamWriter.Write("ST*846*" + SESTVal.ToString() + "~");
                    streamWriter.Write("BIA*C~");
                    streamWriter.Write("LIN**VN*" + r[0].ToString() + "~");
                    streamWriter.Write("PID*****" + r[1].ToString() + "~");
                    streamWriter.Write("REF*LEV*1*EA~");
                    streamWriter.Write("REF*EAN*" + r[3].ToString() + "*1~");
                    streamWriter.Write("REF*LEV*2*IP~");
                    streamWriter.Write("REF*LEV*3*CAS~");
                    streamWriter.Write("REF*EAN*" + r[4].ToString() + "*3~");
                    streamWriter.Write("REF*CGW*" + r[10].ToString() + "*KG~");
                    streamWriter.Write("REF*CHM*" + r[5].ToString() + "*CM~");
                    streamWriter.Write("REF*CWM*" + r[7].ToString() + "*CM~");
                    streamWriter.Write("REF*CLM*" + r[6].ToString() + "*CM~");
                    streamWriter.Write("REF*CQT*" + r[8].ToString() + "*EA~");
                    streamWriter.Write("SE*12*" + SESTVal.ToString() + "~");
                    counter++;
                    WritePLFooter(streamWriter, 1, 8, GEGSVal.ToString(), ISAIEAVal.ToString());
                    streamWriter.Close();
                    SharedFunctions.UpdateCounters(Orbis, "OSP_UPDATE_GBITEMS_VALS", "1", "2", "3", (GEGSVal + 1).ToString(), (SESTVal + 1).ToString(), (ISAIEAVal + 1).ToString());
                    GEGSVal++;
                    SESTVal++;
                    ISAIEAVal++;
                    itemCount++;
                    File.Move(file, ConfigurationManager.AppSettings["GBItems"] + "/" + Path.GetFileName(file));
                }
                con.Close();

                SharedFunctions.UpdateRecords(con, "OSP_UPDATE_PRODUCTLIST_SENT", "108016515");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Product List for GBD Failed to process, error message is: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Product List for GBD failed", ex);
            }
        }
        #endregion

        #region Product List Header
        /**
         * Write header for every file that is to be sent for GB.
         */
        public static void WritePLHeader(StreamWriter sw, string MessageType, string GSVal, string ISA)
        {
            try
            {
                sw.Write("ISA*00*          *00*          *01*Osprey Europe  *ZZ*GreatBear      *" + DateTime.Now.ToString("ddMMyy") + "*" + DateTime.Now.ToString("hhmm") + "*U*00401*" + ISA + "*0*P*>~");
                sw.Write("GS*IB*Osprey Europe*GreatBear*" + DateTime.Now.ToString("yyyyMMdd") + "*" + DateTime.Now.ToString("hhmm") + "*" + GSVal + "*X*004010~");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write header for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write Header for GB", ex);
            }
        }
        #endregion

        #region Product List Footer
        /**
         * Footer for every file that is to be sent for GB.
         */
        public static void WritePLFooter(StreamWriter sw, int NoOfSegs, int lines, string GEVal, string IEA)
        {
            try
            {
                sw.Write("GE*" + NoOfSegs + "*" + GEVal + "~");
                sw.Write("IEA*" + (lines + 3) + "*" + IEA + "~");
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write footer for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write footer for GB", ex);
            }
        }
        #endregion

        #region Write Purchase Order
        /**
         * Purchase order writing, this requires a PO number to write  the correct file and a connection string to the correct DB,
         * This method has also been adapted to work for returns for GB.
         */
        public static void WritePOGB(SqlConnection con, string id)
        {
            try
            {
                SqlConnection Orbis = new SqlConnection
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
                };
                /**
                 * Loops through every item in that order and writes relevant details.
                 */
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_PO_DATA", id);
                DataTable GBCounters = SharedFunctions.QueryDB(Orbis, "OSP_GET_GBITEMS_VALS", "1", "2");
                DataRow GEGS = GBCounters.Rows[0];
                DataRow SEST = GBCounters.Rows[1];
                DataRow ISAIEA = GBCounters.Rows[2];
                int GEGSVal = Int32.Parse(GEGS[2].ToString());
                int SESTVal = Int32.Parse(SEST[2].ToString());
                int ISAIEAVal = Int32.Parse(ISAIEA[2].ToString());
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["GeneratingGB"] + "/PO" + id + ".edi";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                int counter = 1;
                WritePLHeader(streamWriter, fileName, GEGS[2].ToString(), ISAIEAVal.ToString());
                foreach (DataRow row in data.Rows)
                {
                    int QTY = convertToInt(row["Quantity"].ToString());
                    string DateFormatting = DateFormatter(row["OrderRequestedDate"].ToString());
                    streamWriter.Write("ST*856*" + SESTVal.ToString() + "~");
                    streamWriter.Write("BSN*00*" + "PO" + id + "." + row["PrimaryKey"] + "*" + DateFormatting + "~");
                    streamWriter.Write("HL*1**S~");
                    streamWriter.Write("N1*SF**ZZ*" + row["SuppAccRef"].ToString().Replace("$", "") + "~");                           
                    streamWriter.Write("HL*2*" + row["PrimaryKey"] + "*O~");
                    streamWriter.Write("PRF*" + id + "~");
                    streamWriter.Write("HL*3*" + row["PrimaryKey"] + "*I~");
                    streamWriter.Write("LIN*1*VN*" + row["StockItemCode"] + "~");
                    //streamWriter.Write("REF*ZZ*~");
                    streamWriter.Write("SN1**" + QTY + "*EA~");
                    streamWriter.Write("CTT*3*" + QTY + "~");
                    streamWriter.Write("SE*12*" + SESTVal.ToString() + "~");
                    counter++;
                }

                WritePLFooter(streamWriter, data.Rows.Count, 8, GEGS[2].ToString(), ISAIEAVal.ToString());
                streamWriter.Close();
                SharedFunctions.UpdateCounters(Orbis, "OSP_UPDATE_GBITEMS_VALS", "1", "2", "3", (GEGSVal + 1).ToString(), (SESTVal + 1).ToString(), (ISAIEAVal + 1).ToString());
                File.Move(file, ConfigurationManager.AppSettings["GBASN"] + "/" + Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write PO: " + id + " for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write PO: " + id + " for GB", ex);
            }
        }
        #endregion

        #region Convert To Int
        /**
         * This was a reccuring problem that was found with the majority of the DB being varchars and not ints,
         * The standard conversion approach was not seeming to work as required so this function was created.
         */
        public static int convertToInt (string val)
        {
            int intVal = 0;
            string[] splitVal = val.Split('.');
            string trimmed = splitVal[0];
            intVal = Int32.Parse(trimmed); 
            return intVal;
        }
        #endregion

        #region Return To GB
        public static void ReturnToGB (SqlConnection con, string id)
        {
            try
            {
                SqlConnection Orbis = new SqlConnection
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
                };
                /**
                 * Loops through every item in that order and writes relevant details.
                 */
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_RETURN_DATA", id);
                DataTable GBCounters = SharedFunctions.QueryDB(Orbis, "OSP_GET_GBITEMS_VALS", "1", "2");
                DataRow GEGS = GBCounters.Rows[0];
                DataRow SEST = GBCounters.Rows[1];
                DataRow ISAIEA = GBCounters.Rows[2];
                int GEGSVal = Int32.Parse(GEGS[2].ToString());
                int SESTVal = Int32.Parse(SEST[2].ToString());
                int ISAIEAVal = Int32.Parse(ISAIEA[2].ToString());
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["GeneratingGB"] + "/RM" + id + ".edi";
                FileStream f = new FileStream(file, FileMode.Create);
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                StreamWriter streamWriter = new StreamWriter(f, utf8WithoutBom);
                string fileName = "PO" + id + ".txt";
                int counter = 1;
                WritePLHeader(streamWriter, fileName, GEGS[2].ToString(), ISAIEAVal.ToString());
                foreach (DataRow row in data.Rows)
                {
                    int QTY = convertToInt(row["Quantity"].ToString());
                    string DateFormatting = DateFormatter(row["ReturnRequestedDate"].ToString());
                    streamWriter.Write("ST*856*" + SESTVal.ToString() + "~");
                    streamWriter.Write("BSN*00*" + "RM" + id + "." + row["PrimaryKey"] + "*" + DateFormatting + "~");         //The difference between this and a PO is the RM over Po to preffice the number
                    streamWriter.Write("HL*1**S~");
                    streamWriter.Write("N1*SF**ZZ*" + row["CustAccRef"].ToString().Replace("$", "") + "~");
                    streamWriter.Write("HL*2*" + row["PrimaryKey"] + "*O~");
                    streamWriter.Write("PRF*" + id + "~");
                    streamWriter.Write("HL*3*" + row["PrimaryKey"] + "*I~");
                    streamWriter.Write("LIN*1*VN*" + row["StockItemCode"] + "~");
                    streamWriter.Write("SN1**" + QTY + "*EA~");
                    streamWriter.Write("CTT*3*" + QTY + "~");
                    streamWriter.Write("SE*12*" + SESTVal.ToString() + "~");
                    counter++;
                }
                WritePLFooter(streamWriter, data.Rows.Count, 8, GEGS[2].ToString(), ISAIEAVal.ToString());
                streamWriter.Close();
                SharedFunctions.UpdateCounters(Orbis, "OSP_UPDATE_GBITEMS_VALS", "1", "2", "3", (GEGSVal + 1).ToString(), (SESTVal + 1).ToString(), (ISAIEAVal + 1).ToString());
                File.Move(file, ConfigurationManager.AppSettings["GBASN"] + "/" + Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                SharedFunctions.Writefile("Write Preturn: " + id + " for Great Bear failed: " + ex.Message, "");
                SharedFunctions.ErrorAlert("Write return: " + id + " for GB", ex);
            }
        }
        #endregion
        #endregion

    }
}
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
        public static int WriteProductsKTN(SqlConnection con, StreamWriter sw, string orderNo, int counter)
        {
            try
            {
                
                DataTable data = SharedFunctions.QueryDB(con, "OSP_WRITE_PRODUCTS_EDI", orderNo);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                int item = 1;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];

                    string text = (i + 1).ToString();
                    text = text.PadRight(30, ' ');
                    text = text + row["ProductCode"].ToString();
                    text = text.PadRight(55, ' ');
                    text = text + row["ProductDescription"].ToString();
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
                        text = text + s;
                    }
                    text = text.PadRight(18, ' ');
                    sw.WriteLine(counter.ToString().PadLeft(6, '0') + "QTYDEL" + text + "");
                    text = "";
                    counter++;

                    text = row["UnitPrice"].ToString();
                    text = text.Replace('.', ',');
                    text = text + "0";
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
                SharedFunctions.Writefile("Write order items Failed to process, error message is: " + ex.Message + ex.ToString() , "");
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
        public static void WriteOrder (SqlConnection con, string id)
        {
            try { 
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

                    text = row["OrderImportType"].ToString().PadRight(20, ' ').Substring(0,10);
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
                        SqlConnection conDTC = new SqlConnection();
                        conDTC.ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString;
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];


                        text = text.PadRight(20, ' ');
                        text = text + GDPR["PostalName"].ToString();
                        //text = text + "John Smith";
                        text = text.PadRight(100, ' ');
                        text = text + GDPR["AddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text = text + row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text = text + row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text = text + row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');

                        conDTC.Open();
                        SqlCommand Update = new SqlCommand("OSP_UPDATE_GDPR", conDTC);
                        Update.CommandType = CommandType.StoredProcedure;
                        Update.Parameters.AddWithValue("@id", row["DelPostCode"].ToString());
                        Update.Parameters.AddWithValue("@id2", row["OrderReference"].ToString());
                        Update.Parameters.AddWithValue("@Date", DateTime.Now);
                        Update.Parameters.AddWithValue("@File", file);

                        Update.ExecuteNonQuery();
                        Update.Parameters.Clear();

                    }
                    else
                    {
                        text = text.PadRight(20, ' ');
                        text = text + row["DelPostalName"].ToString();
                        text = text.PadRight(100, ' ');
                        text = text + row["DelAddressLine1"].ToString();
                        text = text.PadRight(180, ' ');
                        text = text + row["DelPostCode"].ToString();
                        text = text.PadRight(200, ' ');
                        text = text + row["DelCity"].ToString();
                        text = text.PadRight(280, ' ');
                        text = text + row["DelCountryCode"].ToString();
                        text = text.PadRight(370, ' ');
                    }
                    if ((row["DelPostalName"].ToString() == "DTC Customer") || (row["DelPostalName"].ToString() == "Ecommerce"))
                    {
                        SqlConnection conDTC = new SqlConnection();
                        conDTC.ConnectionString = ConfigurationManager.ConnectionStrings["DTC"].ConnectionString;
                        DataTable GDPRData = SharedFunctions.QueryDB(conDTC, "OSP_GET_GDPR_DATA", row["DelPostCode"].ToString(), row["OrderReference"].ToString());
                        DataRow GDPR = GDPRData.Rows[0];
                        text = text + GDPR["TelephoneNo"].ToString(); //Phone Number
                        text = text.PadRight(473, ' ');
                        text = text + GDPR["PostalName"].ToString(); //Destination Contact
                        text = text.PadRight(523, ' ');
                        text = text + GDPR["EmailAddress"].ToString();
                        text = text.PadRight(573, ' ');
                        flag = true;
                        
                    }
                    else
                    {
                        text = text + row["DelTelephone"].ToString(); //Phone Number
                        text = text.PadRight(523, ' ');
                        text = text + row["DelEmail"].ToString();
                        text = text.PadRight(573, ' ');
                    }
                    text = text + row["DelAddressLine2"].ToString(); // GDPR[""].ToString();  //Del Name 2
                    text = text.PadRight(846, ' ');
                    streamWriter.WriteLine("000009NADDES" + text.PadRight(996, ' ') + "");             //Can be swapped for GLNs but will need swapping to ensure the correct lcoation
                    text = "";

                    ID = row["InvoicePostalAddress"].ToString().PadRight(10, ' ').Substring(0, 10);
                    text = ID;            //Can be swapped for GLNs
                    text = text.PadRight(20, ' ');
                    text = text + row["InvoicePostalAddress"].ToString();
                    text = text.PadRight(100, ' ');
                    text = text + row["InvoiceAddressLine1"].ToString();
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
                    text = text.PadRight(846, ' ');
                    text = text + row["InvoiceAddressLine2"].ToString();
                    streamWriter.WriteLine("000010NADINV" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = "";
                    streamWriter.WriteLine("000011NADTRA" + text.PadRight(996, ' ') + "");
                    text = "";

                    text = row["Incoterms"].ToString();                                //Section reserved for incoterms
                    text = text.PadRight(67, ' ');
                    streamWriter.WriteLine("000012ALI" + text.PadLeft(204, ' ') + "");
                    text = "";

                    con.Close();
                    WriteProductsKTN(con, streamWriter, row["OrderNumber"].ToString(), counter);

                    streamWriter.Close();
                    var lineCount = File.ReadLines(file).Count();

                    File.AppendAllText(file, (lineCount + 1).ToString().PadLeft(6, '0') + "UNT" + (lineCount + 1).ToString().PadRight(6, ' ') + "00000001            ");
                    if (flag)
                    {
                        string name = Path.GetFileName(file);
                        File.Move(file, ConfigurationManager.AppSettings["GDPROrder"] + "/WEB" + name);
                    }
                    flag = false;

                    try
                    {
                        SharedFunctions.QueryDB(con, "OSP_INSERT_TO_TRACKER", fileName, row["OrderNumber"].ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("Failed on write tracker: " + ex.Message, "");
                    }

                }
                con.Close();
            }
            catch(Exception ex)
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
                string file = ConfigurationManager.AppSettings["KTNASN"] + "/PO" + id + ".txt";
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
                string file = ConfigurationManager.AppSettings["KTNItems"] +"/"+ id + "_Product_List.txt";
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
                string file = ConfigurationManager.AppSettings["KTNASN"] + "TRUCK" + id + ".txt";
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
                    text = text + row["DTK_SKU"].ToString();
                    text = text.PadRight(55, ' ');
                    text = text + "";
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
        public static void WriteReturnResponce(SqlConnection con, string id)
        {
            try
            {
                DataTable data = SharedFunctions.QueryDB(con, "OSP_GET_RETURN_DATA", id);
                /**
                 * Retrives the data from the database and then writes it line by line into a file.
                 */
                string file = ConfigurationManager.AppSettings["KTNASN"] + "/RETURN" + id + ".txt";
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
        public static void WriteOrderGB (SqlConnection con, string id)
        {

        }
        #endregion

        #region Write Order Lines

        #endregion

        #region Write ASN

        #endregion

        #region Write Product List

        #endregion
        #endregion
    }
}
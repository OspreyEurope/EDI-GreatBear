using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EDI_Orders
{
    internal class GreatBear
    {
        #region Process GreatBear
        /**
         * This is the decision making for any files in the recieved file for the GB enviroment,
         * This function wil call the correct fucntinos to handle the file and to move it to the correct location.
         */
        public static void ProcessGreatBear(SqlConnection con)
        {
            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["GBHolding"]);
            string[] exists = Directory.GetFiles(ConfigurationManager.AppSettings["GBProcessed"]);
            foreach (string file in files)
            {
                /**
                 * This section is to check if the file already exists and if so it will quarantine the ileisntead which helps to maintain data integrity.
                 */
                if ((exists.Contains("RECCON" + file)) || (exists.Contains("PPLCON" + file)) || (exists.Contains("STKMVT" + file)))
                {
                    string name = Path.GetFileName(file);
                    File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + name);
                    SharedFunctions.Writefile("Write Product List Failed to process, error message is: The file already exists.", "");
                    SharedFunctions.ErrorAlert("Write Product List", new Exception("Repeat File"));
                }
                else
                {
                    //Console.WriteLine("Here");
                    try
                    {
                        string[][] Document = FileManipulation.readEDIFile(file);
                        string Decision = Document[2][1].Trim();
                        Console.WriteLine(Decision);
                        switch (Decision)
                        {
                            #region 846 - STKBAL
                            case "846":    //Items Message
                                WriteSTKBAL(con, Document, file);
                                break;
                            #endregion
                            #region 944 - RECCON
                            case "944":    //Inbound Receipt Confirmation Message
                                Console.WriteLine("WriteReccon");
                                WriteRECCON(con, Document, file);
                                break;
                            #endregion
                            #region 945 - PPLCON
                            case "945":    //Assembly Order Message
                                WritePPLCON(con, Document, file);
                                break;
                            #endregion
                            #region 947 - STKMVT
                            case "947":     //Stock adjust and status checks
                                WriteSTKMVT(con, Document, file);
                                break;
                            #endregion
                            #region 997 - File Acknowledgment
                            case "997":     //Functional acknowledgment message
                                Handle997(con, Document, file);
                                break;
                            #endregion
                            default:
                                break;

                        }
                    }
                    catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
                    }
                
            }
        }
        #endregion

        #region 997 File Handling
        /**
         * Due to GB sending 997 fies for every file they obtain, this section helps to handle these,
         * It will log the date, time, file type, file name and the warehouse to a table in the DB,
         * Then move the file to a 997 specific folder as to not confuse more important files.
         */
        public static void Handle997 (SqlConnection con, string[][] Document, string file)
        {
            try
            {
                con.Open();
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_997", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                string Date = Document[0][9];
                string Time = Document[0][10];
                string FileType = Document[4][1];
                string Warehouse = "GBD";

                storedProcedure.Parameters.AddWithValue("DateOfFile", Date);
                storedProcedure.Parameters.AddWithValue("TimeOfFile", Time);
                storedProcedure.Parameters.AddWithValue("Filename", file);
                storedProcedure.Parameters.AddWithValue("FileType", FileType);
                storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);

                storedProcedure.ExecuteNonQuery();
                storedProcedure.Parameters.Clear();

                con.Close();
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GB997"] + "/" + FileType + name);
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "PPLCON" + name);
                SharedFunctions.Writefile("There was an issue in 997 handling: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert(file + " moved to Quarantine", ex);
            }

        }
        #endregion

        #region Handle Line
        /*8
         * Utilized in the write PPLCON function to  find correct values from the file.
         */
        public static string[][] HandleLine(string[] line)
        {
            string[][] result = new string[15][];
            Console.WriteLine(line[0].Trim());
            string header = line[0].Trim();
            switch (header)
            {
                case "ISA":
                    //result[0] = new string[] { "Warehouse", line[6]};
                    //result[1] = new string[] { "DateRecieved", line[9] };
                    break;
                case "GS":

                    break;
                case "ST":

                    break;
                case "BIA":

                    break;
                case "LIN":
                    result[0] = new string[] { "ItemNumber", line[3] };
                    break;
                case "QTY":
                    result[0] = new string[] { "Quantity", line[2] };
                    break;
                case "CTT":

                    break;
                case "W17":

                    break;
                case "W07":
                    result[0] = new string[] { "QuantityReceived", line[1] };
                    break;
                case "N1":

                    break;
                case "N9":
                    result[0] = new string[] { "CarriertrackingNo", line[2] };            ///???????????????????????????????????????????
                    break;
                case "W06":
                    //result[0] = new string[] { "OrderNumber", line[2] };
                    //result[1] = new string[] { "DateShipped", line[4] };
                    //result[2] = new string[] { "CustomerOrderNumber", line[7] };          ///???????????????????????????????????????????
                    break;
                case "W27":
                    //result[0] = new string[] { "transporter", line[1] };                  ///???????????????????????????????????????????
                    break;
                case "LX":
                    result[0] = new string[] { "ItemNumber", line[1] };
                    break;
                case "W12":
                    result[0] = new string[] { "PackedQuantity", line[3] };
                    break;
                case "W03":

                    break;
                case "W15":

                    break;
                case "W19":

                    break;
                case "AK1":

                    break;
                case "AK2":

                    break;
                case "AK5":

                    break;
                case "MAN":

                    break;
                case "REF":
                    switch (line[1])
                    {
                        case "CN":
                            result[0] = new string[] { "ConNumber", line[2] };
                            break;
                    }
                    break;
                default:
                    //Console.WriteLine("Error");
                    break;
            }


            /**
             * Initially result is predefined as 15 long, this is due to the static nature of a 2D array when created.
             * This bit of code then reads how long the result should be,
             * cycles through result, adds the values to a new array of the length that result should be.
             * Then it returns that new array. If this is not done then there is index issues when adding parameters to the stored procedures.
             */
            int counter = 0;
            for (int x = 0; x < 15; x++)
            {
                if (result[x] != null)
                {
                    counter++;
                }
            }
            string[][] vals = new string[counter][];
            int count = 0;
            for (int y = 0; y < counter; y++)
            {
                vals[count] = result[count];
                count++;
            }

            return vals;
        }
        #endregion

        #region Write PPLCON
        /**
         * This is to handle the PPLCON's also known as 945 files for GB,
         * This is utilizing the same SP 
         */
        public static void WritePPLCON(SqlConnection con, string[][] Document, string file)
        {
            try
            {
                con.Open();
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_PPLCON", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                /**
                 * Establishes the vaiables that are to be inserted into the DB with blank values ready to be used.
                 */
                int p = 0;
                string Warehouse = "";
                string DateRecieved = "";
                string OrderNumber = "";
                string Dateshipped = "";
                string CustomerOrderNumber = "";
                string transporter = "";
                string ID = "";
                string ConNumber = "";
                string SSCC = "";
                string SSCCType = "";
                string PalletQty = "";
                string ItemNumber = "";
                string PackedQty = "";
                string CarrierTrackingNo = "";

                /**
                 * Runs through each line of the file and updates the value of the correct variable as it reaches each ine.
                 */
                foreach (string[] s in Document)
                {
                    string header = s[0].Trim();
                    switch (header)
                    {
                        case "ISA":
                            Warehouse = s[6].Trim();
                            if (Warehouse.Equals("GreatBear"))
                            {
                                Warehouse = "GBD";
                            }
                            DateRecieved = s[9].Trim();
                            break;
                        case "GS":
                            ID = s[6];
                            break;
                        case "W27":
                            transporter = s[2].Trim();
                            break;
                        case "W06":
                            OrderNumber = s[2].Trim();
                            Dateshipped = s[3].Trim();
                            CustomerOrderNumber = s[6].Trim();
                            break;
                    }

                    /**
                     * This section checks that all the correct location in the file has been reached before inserting the data into the DB.
                     */
                    if ((header == "LX" && p > 0) || header == "W03")
                    {
                        Console.WriteLine("Now hits insert");
                        storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);
                        storedProcedure.Parameters.AddWithValue("DateReceived", "20" + DateRecieved);
                        storedProcedure.Parameters.AddWithValue("OrderNumber", OrderNumber);
                        storedProcedure.Parameters.AddWithValue("Dateshipped", Dateshipped);
                        //storedProcedure.Parameters.AddWithValue("CustomerOrderNumber", CustomerOrderNumber);
                        storedProcedure.Parameters.AddWithValue("transporter", transporter);
                        storedProcedure.Parameters.AddWithValue("OriginalFileName", file);
                        storedProcedure.Parameters.AddWithValue("PL", "PACK");
                        storedProcedure.Parameters.AddWithValue("ID", ID);
                        storedProcedure.Parameters.AddWithValue("MessageType", "PPLCON");
                        storedProcedure.Parameters.AddWithValue("ConNumber", ConNumber);
                        storedProcedure.Parameters.AddWithValue("PackedQuantity", PackedQty);
                        storedProcedure.Parameters.AddWithValue("CarriertrackingNo", CarrierTrackingNo);


                        storedProcedure.ExecuteNonQuery();
                        storedProcedure.Parameters.Clear();
                        p++;

                        if (SSCC != "")
                        {
                            SharedFunctions.InsertDESADV(OrderNumber, ItemNumber, PalletQty, SSCC, SSCCType, con);
                            SSCC = "";
                        }
                    }
                    /**
                     * Check to ensure that the insert is not run too early.
                     */
                    if (header == "LX")
                    {
                        p++;
                    }
                    if (header == "N9")
                    {
                        if (s[1].Trim() == "CN")
                        {
                            CarrierTrackingNo = s[2];
                        }
                    }
                    else if (header == "W12")
                    {
                        PackedQty = s[2].Trim();
                        PalletQty = s[2].Trim();
                        ItemNumber = s[8].Trim();
                    }
                    else if (header == "MAN")
                    {
                        switch (s[1].Trim())
                        {
                            case "CP":
                                ConNumber = s[2].Trim();
                                //Consignment number
                                break;
                            case "AA":
                                SSCC = s[2];
                                //ConNumber = s[5];
                                SSCCType = "Pallet";
                                //SSCC Pallet
                                break;
                            case "GM":
                                SSCC = s[2];
                                ConNumber = s[5];
                                SSCCType = "Carton";
                                break;
                            //SSCC Carton
                            default:
                                //Error
                                break;
                        }
                    }
                    else
                    {
                        string[][] info = HandleLine(s);
                        foreach (string[] t in info)
                        {
                            storedProcedure.Parameters.AddWithValue(t[0], t[1]);
                        }
                    }
                }
                con.Close();
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBProcessed"] + "/PPLCON" + name);
            }
            catch (Exception ex) 
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "PPLCON" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert(file + " moved to Quarantine", ex);
            }
            }
        #endregion

        #region Write STKBAL
        /**
         * This writes the daily stock balance message into the DB,
         * This is recieved once a day every day.
         */
        public static void WriteSTKBAL(SqlConnection con, string[][] Document, string file)
        {
            try
            {
                con.Open();
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_STKBAL", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                string ItemNumber = "";
                string ItemDescription = "";
                string Qty = "";
                string date = "";

                foreach (string[] s in Document)
                {

                    switch (s[0].Trim())
                    {
                        case "ISA":
                            date = s[9].ToString();
                            break;
                        case "LIN":
                            ItemNumber = s[3].ToString();
                            Console.WriteLine(s[3].ToString());
                            break;
                        case "QTY":
                            Qty = s[2].ToString();
                            break;
                        default:
                            break;
                    }

                    if ((ItemNumber != "") && (Qty != ""))
                    {
                        storedProcedure.Parameters.AddWithValue("ItemDescription", ItemDescription);
                        storedProcedure.Parameters.AddWithValue("Quantity", Qty);
                        storedProcedure.Parameters.AddWithValue("ItemNumber", ItemNumber);
                        storedProcedure.Parameters.AddWithValue("DateOfMovement", date);
                        storedProcedure.Parameters.AddWithValue("Warehouse", "GBD");
                        storedProcedure.ExecuteNonQuery();
                        storedProcedure.Parameters.Clear();

                        ItemNumber = "";
                        ItemDescription = "";
                        Qty = "";

                    }
                }
                con.Close();
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBProcessed"] + "/STKBAL" + name);
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "STKBAL" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert(file + " moved to Quarantine", ex);
            }
        }

        #endregion

        #region Write STKMVT
        /**
         * This function handles stock movements on the rare occasion these files are recieved,
         * This will write them the same way as they are written for KTn.
         */
        public static void WriteSTKMVT(SqlConnection con, string[][] Document, string file)
        {
            try
            {
                con.Open();
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_STOCK_MOVEMENT", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                string Warehouse = Document[0][6];
                string DateRecieved = Document[1][4];
                string MessageType = Document[2][1];
                string ID = Document[2][2];
                string FileAction = "";

                foreach (string[] line in Document)
                {
                    if (line[0].Trim().Equals("N9") && line[1].Trim().Equals("ZZ"))
                    {
                        FileAction = line[2];
                    }
                }

                storedProcedure.Parameters.AddWithValue("ID", ID);
                storedProcedure.Parameters.AddWithValue("MessageType", MessageType);
                storedProcedure.Parameters.AddWithValue("DateReceived", DateRecieved);
                storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);
                storedProcedure.Parameters.AddWithValue("OriginalFileName", file);
                storedProcedure.Parameters.AddWithValue("FileAction", FileAction);

                storedProcedure.ExecuteNonQuery();
                storedProcedure.Parameters.Clear();

                WRiteSTKMVTItems(con, Document, DateRecieved, ID, file);
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBProcessed"] + "/STKMVT" + name);
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "STKMVT" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert(file + " moved to Quarantine", ex);
            }
        }
        #endregion

        #region STKMVT Items
        /**
         * This function will only be called if a stock movement is processed and is used as tthe details as to the movement and the items for the movement are in seperate tables.
         */
        public static void WRiteSTKMVTItems(SqlConnection con, string[][] Document, string Date, string ID, string file)
        {
            try
            {
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_ITEMS_FOR_STKMVT", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                string ItemNumber = "";
                string Quantity = "";
                string StockMovementType = "";
                string TypeOfOperation = "";
                string Reason = "";

                foreach (string[] s in Document)
                {
                    if ((ItemNumber != "") && (Quantity != "") && (Reason != ""))
                    {
                        storedProcedure.Parameters.AddWithValue("ID", ID);
                        storedProcedure.Parameters.AddWithValue("ItemNumber", ItemNumber);
                        storedProcedure.Parameters.AddWithValue("Quantity", Quantity);
                        storedProcedure.Parameters.AddWithValue("StockMovementType", StockMovementType);
                        storedProcedure.Parameters.AddWithValue("TypeOfOperation", TypeOfOperation);
                        storedProcedure.Parameters.AddWithValue("Reason", Reason);
                        storedProcedure.Parameters.AddWithValue("DateOfMovement", Date);

                        storedProcedure.ExecuteNonQuery();
                        storedProcedure.Parameters.Clear();

                        ItemNumber = "";
                        Quantity = "";
                        StockMovementType = "";
                        TypeOfOperation = "";
                        Reason = "";
                    }

                    /**
                     * Gathering relevant variable information.
                     */
                    switch (s[0].Trim())
                    {
                        case "W19":
                            ItemNumber = s[6];
                            Quantity = s[2];
                            TypeOfOperation = s[1];
                            break;
                        case "N9":
                            switch (s[1].Trim())
                            {
                                case "ZZ":
                                    StockMovementType = s[2];
                                    break;
                                case "TD":
                                    Console.WriteLine(s[2]);
                                    Reason = s[2];
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "PPLCON" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert("Error in STKMVTItems, " + file, ex);
            }
        }

        #endregion

        #region Write RECCON
        /**
         * This is to read a 944 file or an RECCON file from GB,
         * This utilizes the same SP's as the sister function in the KTN class,
         * This takes a Document[][] parameter witch is the split up file and uses the maped locations to fill in the variable information.
         */
        public static void WriteRECCON(SqlConnection con, string[][] Document, string file)
        {
            try
            {
                con.Open();
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_RECCON", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                string name = Path.GetFileName(file);
                string Warehouse = Document[0][6];
                Console.WriteLine(Warehouse);
                if (Warehouse.Equals(" GreatBear"))
                {
                    Warehouse = "GBD";
                }
                string DateRecieved = Document[0][9];
                string MessageType = Document[2][1];
                string CustomerReferenceTransport = Document[3][4];
                string ID = Document[3][4];
                string FileAction = Document[3][3];
                string InboundDeliveryType = Document[3][3].Substring(1, 2);

                if (InboundDeliveryType == "PO")
                {
                    InboundDeliveryType = "NORMAL";
                }
                else if (InboundDeliveryType == "RM")
                {
                    InboundDeliveryType = "RETURN";
                }

                string TransportInbound = ""; //Document[0][13];
                string TransporterName = "FlexPort";
                string SupplierName = Document[4][4];
                FileAction = FileAction.Substring(0, 2);
                
                storedProcedure.Parameters.AddWithValue("ID", ID);
                storedProcedure.Parameters.AddWithValue("MessageType", MessageType);
                storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);
                storedProcedure.Parameters.AddWithValue("DateReceived", "20" + DateRecieved.Trim());
                storedProcedure.Parameters.AddWithValue("OriginalFileName", name);
                storedProcedure.Parameters.AddWithValue("FileAction", FileAction);
                storedProcedure.Parameters.AddWithValue("CustomerReferenceInbound", ID);
                storedProcedure.Parameters.AddWithValue("TransportInbound", TransportInbound);
                storedProcedure.Parameters.AddWithValue("InboundDeliveryType", InboundDeliveryType);
                storedProcedure.Parameters.AddWithValue("TransporterName", TransporterName);
                storedProcedure.Parameters.AddWithValue("SupplierName", SupplierName);
                
                storedProcedure.ExecuteNonQuery();
                storedProcedure.Parameters.Clear();

                WriteRECCONITEMS(con, Document, ID, file, DateRecieved);
                Console.WriteLine("File Written");
                File.Move(file, ConfigurationManager.AppSettings["GBProcessed"] + "/RECCON" + name);
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "RECCON" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert(file + " moved to Quarantine", ex);
            }
        }
        #endregion

        #region RECCON Items
        /**
         * This function writes the item infrmation into a different table to the RECCON details,
         * it i passed a few variables as to be able to link the item to the correct reccon data.
         */
        public static void WriteRECCONITEMS(SqlConnection con, string[][] Document, string ID, string file, string DateRecieved)
        {
            try
            {
                SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_ITEMS_FOR_RECCON", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                string ItemNumber = "";
                //string ItemDescription = "";
                string RecievedQuantity = "";

                foreach (string[] s in Document)
                {
                    Console.WriteLine(s[0]);
                    switch (s[0].Trim())
                    {
                        case "W07":
                            ItemNumber = s[5];
                            RecievedQuantity = s[1];
                            Console.WriteLine(ItemNumber);
                            break;
                    }

                    Console.WriteLine("If check for vals");
                    if ((ItemNumber != "") && (RecievedQuantity != ""))
                    {
                        storedProcedure.Parameters.AddWithValue("ID", ID);
                        storedProcedure.Parameters.AddWithValue("ItemNumber", ItemNumber);
                        storedProcedure.Parameters.AddWithValue("RecievedQuantity", RecievedQuantity);
                        storedProcedure.Parameters.AddWithValue("DateReceived", "20" + DateRecieved.Trim());

                        storedProcedure.ExecuteNonQuery();
                        storedProcedure.Parameters.Clear();

                        ItemNumber = "";
                        //ItemDescription = "";
                        RecievedQuantity = "";
                    }
                    Console.WriteLine("End of foreach pass");
                }
                con.Close();
            }
            catch (Exception ex)
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + "RECCON" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "Document Moved to Quarantine");
                SharedFunctions.ErrorAlert("Write RECCONItems failed for file, " + file, ex);
            }
        }
        #endregion
    }
}
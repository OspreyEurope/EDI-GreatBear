using System;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Configuration;
using Aspose.Pdf.Drawing;
using Path = System.IO.Path;
using Aspose.Pdf;
using Aspose.Pdf.Operators;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Reflection;
using System.Diagnostics;
using System.IO.Packaging;

namespace EDI_Orders
{
    internal class KTN
    {
        #region ProcessKTN
        public static void ProcessKTN (string file,SqlConnection con)
        {
            
            string SP = "";
            string OrderType = File.ReadAllLines(file)[0];
            string Decision = OrderType.Substring(29,20);
            string type = "";

            /**
             * This switch case is used to decide which file has been recieved and as a result how to handle the file.
             * This may be improved however revist due to time constraints.
             * STKMVT and RECCON can be merged to be handled as a '2 table' method and then PLCON as a '1 table' method and then used again in future.
             */
            try
            {
                switch (Decision)
                {
                    /**
                     * Use the STKMVT and RECCON approach if you are splitting the file into a header ad lines approach,
                     * This way writes the item and item details into a seperate table to the header details to avoid repeating unneccessary data.
                     */
                    #region STKMVT
                    case "STKMVT              ":
                        type = "STKMVT";
                        SP = "OSP_INSERT_STOCK_MOVEMENT";
                        SqlCommand storedProcedure = new SqlCommand(SP, con);
                        storedProcedure.CommandType = CommandType.StoredProcedure;
                        int lineCount = File.ReadLines(file).Count();
                        //Add Try catch on this block to check it can be read and is not corrupted to prevent a crash sooner rather than later
                        string[] lines = File.ReadAllLines(file);
                        string row = lines[0];
                        string[][] result = new string[15][];
                        string ID = row.Substring(9, 20);
                        string STKMVTType = OrderType.Substring(104, 6);
                        switch (STKMVTType)
                        {
                            #region STKADJ
                            case "STKADJ":
                                con.Open();
                                /**
                                 * Write the header table values
                                 */
                                ID = row.Substring(9, 20);
                                result[6] = new string[] { "ID", ID };
                                result[0] = new string[] { "MessageType", row.Substring(29, 20) };
                                result[1] = new string[] { "Warehouse", row.Substring(59, 10) };
                                result[2] = new string[] { "DateReceived", row.Substring(201, 35) };
                                result[3] = new string[] { "OriginalFileName", row.Substring(239, 50) };
                                row = lines[1];
                                result[4] = new string[] { "FileAction", row.Substring(12, 35) };
                                row = lines[2];
                                result[5] = new string[] { "DatePeriod", row.Substring(12, 35) };

                                /**
                                * This section cycles through the array and adds the values to the stored procedure.
                                * It then inserts the data into the headers table in the database.
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

                                for (int i = 0; i < vals.Length; i++)
                                {
                                    storedProcedure.Parameters.AddWithValue(vals[i][0], vals[i][1]);
                                }

                                storedProcedure.ExecuteNonQuery();
                                storedProcedure.Parameters.Clear();
                                con.Close();

                                /**
                                 * This is the start of the stored procedure to add the items to their table.
                                 * It runs through a loop that reads from where the items start and then adds the values to the stored procedure,
                                 * It finds the header value and the data value by using the method 'ReadKTN' due to the style of file.
                                 */
                                con.Open();
                                storedProcedure = new SqlCommand("OSP_INSERT_ITEMS_FOR_STKMVT", con);
                                storedProcedure.CommandType = CommandType.StoredProcedure;
                                /**
                                 * Write the line items to a seperate table
                                 */
                                lineCount = File.ReadLines(file).Count();
                                for (int i = 3; i < (lineCount - 2); i++)
                                {
                                    string[][] t = ReadKTN(lines[i]);
                                    if (t != null)
                                    {
                                        for (int j = 0; j < t.Length; j++)
                                        {
                                            storedProcedure.Parameters.AddWithValue(t[j][0], t[j][1]);
                                        }
                                        /**
                                         * Ths dictates when the write is used and then clears the parameters to be used again after.
                                         */
                                        if (lines[i + 1].Substring(6, 3) == "LIN" || lines[i + 1].Substring(6, 3) == "UNT")
                                        {
                                            storedProcedure.Parameters.AddWithValue("ID", ID);
                                            storedProcedure.ExecuteNonQuery();
                                            storedProcedure.Parameters.Clear();
                                        }
                                    }
                                }
                                storedProcedure.Parameters.AddWithValue("ID", ID);
                                storedProcedure.ExecuteNonQuery();
                                storedProcedure.Parameters.Clear();
                                con.Close();
                                break;
                            #endregion
                            #region STKBAL
                            case "STKBAL":
                                lines = File.ReadAllLines(file);
                                row = lines[0];
                                string date = row.Substring(201, 35);

                                con.Open();
                                storedProcedure = new SqlCommand("OSP_INSERT_STKBAL", con);
                                storedProcedure.CommandType = CommandType.StoredProcedure;
                                /**
                                 * Write the line items to a seperate table
                                 */
                                lineCount = File.ReadLines(file).Count();
                                for (int i = 3; i < (lineCount - 1); i++)
                                {
                                    string[][] t = ReadKTN(lines[i]);
                                    if (t != null)
                                    {
                                        for (int j = 0; j < t.Length; j++)
                                        {
                                            Console.WriteLine(i);
                                            Console.WriteLine(t[j][0]);
                                            Console.WriteLine(t[j][1]);
                                            storedProcedure.Parameters.AddWithValue(t[j][0], t[j][1]);
                                        }
                                        /**
                                         * Ths dictates when the write is used and then clears the parameters to be used again after.
                                         */
                                        if (lines[i + 1].Substring(6, 3) == "LIN" || lines[i + 1].Substring(6, 3) == "UNT")
                                        {
                                            storedProcedure.Parameters.AddWithValue("DateOfMovement", date);
                                            storedProcedure.ExecuteNonQuery();
                                            storedProcedure.Parameters.Clear();
                                        }
                                    }
                                }
                                //storedProcedure.Parameters.AddWithValue("DateOfMovement", date);
                                //storedProcedure.ExecuteNonQuery();
                                //storedProcedure.Parameters.Clear();
                                con.Close();
                                break;
                            #endregion
                        }
                        break;
                    #endregion
                    #region RECCON
                    case "RECCON              ":
                        type = "RECCON";
                        SP = "OSP_INSERT_RECCON";
                        storedProcedure = new SqlCommand(SP, con);
                        storedProcedure.CommandType = CommandType.StoredProcedure;
                        lineCount = File.ReadLines(file).Count();
                        lines = File.ReadAllLines(file);
                        ID = lines[0].Substring(9, 20);
                        int linePos = 0;
                        string temp = "";
                        while (linePos < lineCount)
                        {
                            row = lines[linePos];
                            string segment = row.Substring(6, 3);
                            switch (segment)
                            {
                                case "FAC":
                                    temp = WriteRECCONHeader(con, storedProcedure,lines,linePos,file,SP);
                                    break;
                                case "LIN":
                                    WriteRECCONItems(con,lines,linePos,temp);;
                                    break;
                                case "UNT":
                                    Console.WriteLine("Made it to the end of the file");
                                        break;
                                default:
                                    
                                    break;
                            }
                            linePos++;
                        }

                        //lineCount = File.ReadLines(file).Count();
                        //here write the run through and write items section

                        break;
                    #endregion
                    /**
                     * Use the PPLCON approach if all data is being put in the same table,
                     * This means if the item details and the header details are all being compiled and not seperated out.
                     */
                    #region PPLCON
                    case "PPLCON              ":
                        /**
                         * Reads the header values from the database,
                         * Assigns the correct stored procedure for the file,
                         * Creates the variables needed to temporarily store the data needed for each row of the database.
                         */
                        type = "PPLCON";
                        SP = "OSP_INSERT_PPLCON";
                        var headers = SharedFunctions.QueryDB(con, "OSP_GETHEADERS", "PPLCON");

                        con.Open();
                        storedProcedure = new SqlCommand(SP, con);
                        storedProcedure.CommandType = CommandType.StoredProcedure;
                        lineCount = File.ReadLines(file).Count();
                        lines = File.ReadAllLines(file);
                        result = new string[15][];
                        ID = "";
                        string MessageType = "";
                        string Warehouse = "";
                        string DateReceived = "";
                        string OriginalFileName = "";
                        string FileAction = "";
                        string DateShipped = "";
                        string KTNOutBound = "";
                        string Transporter = "";
                        string OrderNumber = "";
                        string ItemNumber = "";
                        string ItemDescrtiption = "";
                        string PQty = "";
                        string ConNumber = "";
                        string TrackingNo2 = "";
                        string BarcodeOnLabel = "";
                        string Carrier = "";
                        string PL = "";
                        int p = 0;

                        /**
                         * This gets the length of the file and then cycles through the file reading each line.
                         */
                        
                        for (int i = 0; i < (lineCount - 1); i++)
                        { 
                            string[][] t = ReadKTN(lines[i]);
                            //Console.WriteLine(lines[i].Substring(0, 6));         //This will print the line number which is being processed, if there is a crash this is sueful to figure which is the line it is crashing on, this is usually the ALI line due to length issues
                            if (t != null)
                            {
                                /**
                                 * This section of if statments is used to build the header data that is only present once but is repeated in the database.
                                 */
                                Console.WriteLine("Line: " + i);
                                if (lines[i].Substring(6, 3) == "UNH")
                                {
                                    row = lines[i];
                                    ID = row.Substring(9, 20);
                                    MessageType = row.Substring(29, 20);
                                    Warehouse = row.Substring(59, 10);
                                    DateReceived = row.Substring(201, 35);
                                    OriginalFileName = row.Substring(239, 50);
                                    PL = row.Substring(69, 5);
                                }
                                else if (lines[i].Substring(6, 3) == "LIN")
                                {
                                    ItemNumber = lines[i].Substring(39, 25);
                                    ItemDescrtiption = lines[i].Substring(64, 80);
                                    p++;
                                }
                                else if (lines[i].Substring(6, 3) == "ALI")
                                {
                                    //This is an awkard line, this jsut needs to be here or it fails as this line is the only line in KTN that has a variable length
                                }
                                else if (lines[i].Substring(6, 3) == "FAC")
                                {

                                    row = lines[i];
                                    FileAction = row.Substring(12, 35);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "DTMDEL")
                                {
                                    row = lines[i];
                                    DateShipped = row.Substring(12, 35);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "RFFOUT")
                                {
                                    row = lines[i];
                                    KTNOutBound = row.Substring(12, 80);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "RFFCR2")
                                {
                                    row = lines[i];
                                    OrderNumber = row.Substring(12, 80);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "NADTRO")
                                {
                                    row = lines[i];
                                    Transporter = row.Substring(9, 3);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "QTYTPA")
                                {
                                    PQty = lines[i].Substring(12, 15);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "QTYTLO")
                                {
                                    PQty = lines[i].Substring(12, 15);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "TRATRN")
                                {
                                    ConNumber = lines[i].Substring(12, 35);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "TRATR2")
                                {
                                    TrackingNo2 = lines[i].Substring(12, 35);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "TRABAC")
                                {
                                    BarcodeOnLabel = lines[i].Substring(12, 35);
                                }
                                else if (lines[i].PadRight(13).Substring(6, 6) == "TRACTR")
                                {
                                    Carrier = lines[i].Substring(12, 35);
                                }
                                else
                                {
                                    
                                }
                                
                                
                                    //for (int j = 0; j < t.Length; j++)
                                    //{
                                    //    if (headers.AsEnumerable().Any(a => t[j][0].ToString() == a.Field<string>("Header")))
                                    //    {
                                    //        storedProcedure.Parameters.AddWithValue(t[j][0], t[j][1]);
                                    //    }
                                    //}

                                    /**
                                     * This is when the data is inserted, this only happens when the data is to be repeated.
                                     * It adds all the header style information previously gathered and adds it to the stored procedure.
                                     * It then clears the values ready for the next section of data.
                                     */
                                if ((lines[i+1].PadRight(13).Substring(6, 6) == "RFFCR2" && p > 0) || lines[i+1].Substring(6, 3) == "UNT" || (lines[i+1].Substring(6,3) == "LIN"))
                                {
                                    storedProcedure.Parameters.AddWithValue("ID", ID);
                                    storedProcedure.Parameters.AddWithValue("ItemNumber",ItemNumber);
                                    storedProcedure.Parameters.AddWithValue("ItemDescrtiption", ItemDescrtiption);
                                    storedProcedure.Parameters.AddWithValue("MessageType", MessageType);
                                    storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);
                                    storedProcedure.Parameters.AddWithValue("DateReceived", DateReceived);
                                    storedProcedure.Parameters.AddWithValue("OriginalFileName", OriginalFileName);
                                    storedProcedure.Parameters.AddWithValue("FileAction", FileAction);
                                    storedProcedure.Parameters.AddWithValue("DateShipped", DateShipped);
                                    storedProcedure.Parameters.AddWithValue("KTNOutBoundCode", KTNOutBound);
                                    storedProcedure.Parameters.AddWithValue("Transporter", Transporter);
                                    storedProcedure.Parameters.AddWithValue("OrderNumber", OrderNumber);
                                    storedProcedure.Parameters.AddWithValue("PackedQuantity", PQty);
                                    storedProcedure.Parameters.AddWithValue("ConNumber", ConNumber);
                                    storedProcedure.Parameters.AddWithValue("TrackingNo2", TrackingNo2);
                                    storedProcedure.Parameters.AddWithValue("BarcodeOnLabel", BarcodeOnLabel);
                                    storedProcedure.Parameters.AddWithValue("CarrierTrackingNo", Carrier);
                                    storedProcedure.Parameters.AddWithValue("PL",PL);


                                    storedProcedure.ExecuteNonQuery();
                                    storedProcedure.Parameters.Clear();

                                    
                                    SqlCommand UpdateTracker = new SqlCommand("OSP_UPDATE_TRACKER",con);
                                    UpdateTracker.Parameters.AddWithValue("OrderNumber", OrderNumber);
                                    UpdateTracker.Parameters.AddWithValue("ConNumber", ConNumber);
                                    UpdateTracker.Parameters.AddWithValue("PackedQty", PQty);
                                    UpdateTracker.Parameters.AddWithValue("transport", Transporter);
                                    UpdateTracker.Parameters.AddWithValue("DateShipped", DateShipped);
                                    UpdateTracker.Parameters.AddWithValue("FileName",OriginalFileName );

                                    storedProcedure.ExecuteNonQuery();
                                    storedProcedure.Parameters.Clear();     
                                }
                            }
                        }
                        con.Close();
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                con.Close();
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["PKTN"+ type + "Quarantined"] + "/" + name);
                SharedFunctions.Writefile("There was an issue: " + ex.Message, "File Moved to PKTN" + type + "Quarantined");
            }
        }
        #endregion

        #region Write RECCON Header
        public static string WriteRECCONHeader (SqlConnection con, SqlCommand storedProcedure, string[] lines, int linePos, string file, string SP)
        {
            con.Open();
            /**
             * Write the header table values
             */
            storedProcedure = new SqlCommand(SP, con);
            storedProcedure.CommandType = CommandType.StoredProcedure;
            //Add Try catch on this block to check it can be read and is not corrupted to prevent a crash sooner rather than later
            lines = File.ReadAllLines(file);
            string row = lines[0];
            string[][] result = new string[20][];
            string ID = row.Substring(9, 20);
            result[5] = new string[] { "ID", ID };
            result[0] = new string[] { "MessageType", row.Substring(29, 20) };
            result[1] = new string[] { "Warehouse", row.Substring(59, 10) };
            result[2] = new string[] { "DateReceived", row.Substring(201, 35) };
            result[3] = new string[] { "OriginalFileName", row.Substring(239, 50) };
            row = lines[linePos];
            result[4] = new string[] { "FileAction", row.Substring(12, 35) };
            row = lines[linePos + 1];
            result[6] = new string[] { "TransportInbound", row.Substring(12, 80) };
            row = lines[linePos + 2];
            result[7] = new string[] { "CustomerReferenceInbound", row.Substring(12, 80) };
            row = lines[linePos + 3];
            result[8] = new string[] { "InboundDeliveryType", row.Substring(12, 80) };
            row = lines[linePos + 4];
            result[9] = new string[] { "CustomerReferenceTransport", row.Substring(12, 80) };
            int y = 5;
            row = lines[linePos + 5];
            if (row.Substring(6, 6) != "NADTRA")
            {
                y++;
            }
            row = lines[linePos + y];
            result[9] = new string[] { "TransporterName", row.Substring(32, 80) };
            result[10] = new string[] { "TransporterAddress", row.Substring(112, 80) };
            result[11] = new string[] { "TransporterCountry", row.Substring(302, 80) };
            result[12] = new string[] { "TransporterLicensePlate", row.Substring(759, 20) };
            result[13] = new string[] { "TransporterContact", row.Substring(486, 50) };
            y++;
            row = lines[linePos + y];
            result[14] = new string[] { "SupplierName", row.Substring(32, 80) };
            result[15] = new string[] { "SupplierAddress", row.Substring(112, 80) };
            result[16] = new string[] { "SupplierCountry", row.Substring(302, 80) };
            //result[17] = new string[] { "SupplierContact", row.Substring(486, 50) };
            y++;
            row = lines[linePos + y];
            result[18] = new string[] { "ArrivedDate", row.Substring(12, 35) };
            /**
             * This section cycles through the array and adds the values to the stored procedure.
             * It then inserts the data into the headers table in the database.
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
            for (int l = 0; l < counter; l++)
            {
                vals[count] = result[count];
                count++;
            }

            for (int i = 0; i < vals.Length; i++)
            {
                storedProcedure.Parameters.AddWithValue(vals[i][0], vals[i][1]);
            }

            storedProcedure.ExecuteNonQuery();
            storedProcedure.Parameters.Clear();
            con.Close();

            return result[7][1];
        }
        #endregion

        #region Write Items for RECCON
        public static void WriteRECCONItems (SqlConnection con, string[] lines, int lineCount, string ID)
        {
            /**
                         * This is the start of the stored procedure to add the items to their table.
                         * It runs through a loop that reads from where the items start and then adds the values to the stored procedure,
                         * It finds the header value and the data value by using the method 'ReadKTN' due to the style of file.
                         */
            con.Open();
            SqlCommand StoredProcedure2  = new SqlCommand("OSP_INSERT_ITEMS_FOR_RECCON", con);
            StoredProcedure2.CommandType = CommandType.StoredProcedure;
            /**
             * Write the line items to a seperate table
             */
            int i = lineCount;
            while (!((lines[i + 1].Substring(6, 3) == "FAC") || (lines[i + 1].Substring(6,3) == "UNT")))
            { 
                string[][] t = ReadKTN(lines[i]);
                if (t != null)
                {
                    for (int j = 0; j < t.Length; j++)
                    {
                        StoredProcedure2.Parameters.AddWithValue(t[j][0], t[j][1]);
                    }
                    /**
                     * Ths dictates when the write is used and then clears the parameters to be used again after.
                     */
                    if (lines[i + 1].Substring(6, 3) == "LIN" || lines[i + 1].Substring(6, 3) == "UNT" || lines[i + 1].Substring(6, 3) == "FAC")
                    {
                        StoredProcedure2.Parameters.AddWithValue("ID", ID);
                        StoredProcedure2.ExecuteNonQuery();
                        StoredProcedure2.Parameters.Clear();
                    }
                }
                i++;
            }
            con.Close();
        }
        #endregion

        #region Read Line For KTN
        public static string[][] ReadKTN(string row)
        {
            /**
             * This created the header variable to check against whitch is positions 7, 8 and 9 in the lie,
             * It then creates a result array which will hold the header for the stored procedure to use and the value.
             */
            string header = row.Substring(6, 3);
            string[][] result = new string[15][];

            /**
             * This switch case handles what is done to the line of the message,
             * This is done through checking the value of the edi header which is located at charcter position 7-9.
             */
            switch (header)
            {
                case "UNH":
                    result[0] = new string[] { "MessageType", row.Substring(29, 20) };
                    result[1] = new string[] { "Warehouse", row.Substring(59, 10) };
                    result[2] = new string[] { "DateReceived", row.Substring(201, 35) };
                    result[3] = new string[] { "OriginalFileName", row.Substring(239, 50) };
                    break;
                case "FAC":
                    result[0] = new string[] { "FileAction", row.Substring(12, 35) };
                    break;
                case "TDT":
                    result[0] = new string[] { "OrderType", row.Substring(92, 10) };
                    break;
                case "RFF":
                    string RFFSeg =  row.Substring(9, 3);
                    switch (RFFSeg)
                    {
                        case "CR1":
                            result[0] = new string[] { "CustomerReference1", row.Substring(12, 80) };
                            break;
                        case "CR2":
                            result[0] = new string[] { "CustomerReference2", row.Substring(12, 80) };
                            break;
                        case "CR3":
                            result[0] = new string[] { "CustomerReference3", row.Substring(12, 80) };
                            break;
                        case "CR4":
                            result[0] = new string[] { "CustomerReference4", row.Substring(12, 80) };
                            break;
                        case "CR5":
                            result[0] = new string[] { "CustomerReference5", row.Substring(12, 80) };
                            break;
                        case "CRI":
                            result[0] = new string[] { "CustomerReferenceInbound", row.Substring(12, 80) };
                            break;
                        case "CRT":
                            result[0] = new string[] { "CustomerReferenceTransport", row.Substring(12, 80) };
                            break;
                        case "DEL":
                            result[0] = new string[] { "DeliveryNumber", row.Substring(12, 80) };
                            break;
                        case "TRI":
                            result[0] = new string[] { "TransportInbound", row.Substring(12, 80) };
                            break;
                        case "IDT":
                            result[0] = new string[] { "InboundDeliveryType", row.Substring(12, 80) };
                            break;
                        case "TRR":
                            result[0] = new string[] { "TransportReference", row.Substring(12, 80) };
                            break;
                    }
                    break;
                case "DTM":
                    string DateSeg = row.Substring(9, 3);
                    string DateParty = "";
                    switch (DateSeg)
                    {
                        case "DEL":
                            DateParty = "Delivery";
                            break;
                        case "LOA":
                            DateParty = "Load";
                            break;
                        case "ARR":
                            DateParty = "";
                            break;
                        case "PER":
                            DateParty = "Period";
                            break;
                        default:
                            DateParty = "";
                            break;
                    }
                    if (DateParty != "")
                    {
                        result[0] = new string[] { "Date" + DateParty, row.Substring(12, 35) };
                    }
                    break;
                case "NAD":
                    string NADSeg = row.Substring(9, 3);
                    string NADParty = "";
                    switch (NADSeg)
                    {
                        case "BY":
                            NADParty = "Buyer";
                            break;
                        case "DES":
                            NADParty = "Destination";
                            break;
                        case "CZ":
                            NADParty = "Shipper";
                            result[4] = new string[] { NADParty + "ID", row.Substring(12, 20) };
                            break;
                        case "SE":
                            NADParty = "Seller";
                            break;
                        case "TRA":
                            NADParty = "Transporter";
                            break;
                        case "INV":
                            NADParty = "Invoice";
                            break;
                        case "Supplier":
                            NADParty = "Supplier";
                            break;
                    }
                    if (NADParty != "")
                    {
                        result[0] = new string[] { NADParty + "Name", row.Substring(32, 80) };
                        result[1] = new string[] { NADParty + "Address", row.Substring(112, 80) };
                        result[2] = new string[] { NADParty + "Country", row.Substring(302, 80) };
                        if (NADParty == "Transporter")
                        {
                            result[3] = new string[] { NADParty + "LicensePlate", row.Substring(759, 20) };
                            result[4] = new string[] { NADParty + "Contact", row.Substring(486, 50) };
                        }
                    }
                    break;
                case "FTX":
                    string FTXSeg = row.Substring(9, 3);
                    string FTXParty = "";
                    switch (FTXSeg)
                    {
                        case "PIC":
                            FTXParty = "Picking";
                            break;
                        case "PAC":
                            FTXParty = "Packing";
                            break;
                        case "DEL":
                            FTXParty = "Delivery";
                            break;
                        case "PCP":
                            FTXParty = "PrincipalCom";
                            break;
                        case "ASS":
                            FTXParty = "Assembly";
                            break;
                        case "VSC":
                            FTXParty = "VAS";
                            break;
                    }
                    result[0] = new string[] { FTXParty + "Comments", row.Substring(12, 120) };
                    break;
                case "ALI":
                    if (row.Length > 133)
                    {
                        result[0] = new string[] { "Cost", row.Substring(131, 12) };
                        result[1] = new string[] { "Crrency", row.Substring(143, 3) };
                    }
                    break;
                case "LIN":
                    result[0] = new string[] { "ItemNumber", row.Substring(39, 25) };
                    result[1] = new string[] { "ItemDescription", row.Substring(64, 80) };
                    break;
                case "PIA":
                    string PIASeg = row.Substring(9, 3);
                    string PIAParty = "";
                    switch (PIASeg)
                    {
                        case "SUP":
                            PIAParty = "Supplier";
                            break;
                        case "DES":
                            PIAParty = "Destination";
                            break;
                        case "OL1":
                            PIAParty = "OrderLine1";
                            break;
                        case "OL2":
                            PIAParty = "OrderLine2";
                            break;
                        case "OL3":
                            PIAParty = "OrderLine3";
                            break;
                        case "OL4":
                            PIAParty = "OrderLine4";
                            break;
                        case "CAM":
                            PIAParty = "Campaign";
                            break;
                    }
                    result[0] = new string[] { PIAParty + "ItemNumber", row.Substring(12, 25 ) };
                    result[1] = new string[] { PIAParty + "ItemDescripin", row.Substring(37, 80) };
                    break;
                case "PAC":
                    string PACSeg = row.Substring(9, 3);
                    string PACParty = "";
                    if (PACSeg == "CUS")
                    { 
                        PACParty = "Customer";
                    }
                    result[0] = new string[] { PACParty + "PackagingCoded", row.Substring(40, 25) };
                    break;
                case "QTY":
                    string QTYSeg = row.Substring(9, 3);
                    string QTYParty = "";
                    switch (QTYSeg)
                    {
                        case "DEL":
                            QTYParty = "Deliver";
                            break;
                        case "ASS":
                            QTYParty = "Assemble";
                            break;
                        case "PRC":
                            QTYParty = "PerUnit";
                            break;
                        case "PAC":
                            QTYParty = "PickList";
                            break;
                        case "TRE":
                            QTYParty = "Recieved";
                            break;
                        case "TLO":
                            QTYParty = "TotalLoaded";
                            break;
                        case "TRB":
                            QTYParty = "Blocked";
                            break;
                        case "TPA":
                            QTYParty = "InOrder";
                            break;
                    }
                    if (QTYParty != "")
                    {
                        result[0] = new string[] { QTYParty + "Quantity", row.Substring(12, 15) };
                    }
                    break;
                case "TRA":
                    string TRASeg = row.Substring(19, 3);
                    string TRAParty = "";
                    switch (TRASeg)
                    {
                        case "SNO":
                            TRAParty = "SerialNo";
                            break;
                        case "LNO":
                            TRAParty = "LotNo";
                            break;
                        case "CFG":
                            TRAParty = "Configuration";
                            break;
                        case "BBD":
                            TRAParty = "BestBeforeDate";
                            break;
                        case "SHU":
                            TRAParty = "SHUID";
                            break;
                        case "UID":
                            TRAParty = "UID";
                            break;
                    }
                    result[0] = new string[] { TRAParty + "TrackingCode", row.Substring(12, 35) };
                    break;
                case "LOC":
                    string LOCSeg = row.Substring(9, 3);
                    string LOCParty = "";
                    switch (LOCSeg)
                    {
                        case "WP":
                            LOCParty = "WorkPost";
                            break;
                        case "WPZ":
                            LOCParty = "WorkPostZone";
                            break;
                    }
                    result[0] = new string[] { LOCParty + "Location", row.Substring(12, 25) };
                    break;
                case "SBD":
                    result[0] = new string[] { "Quantity", row.Substring(47, 15) };
                    //result[1] = new string[] { "BlockedQuantity", row.Substring(62, 15) };
                    //result[2] = new string[] { "InOrderQuantity", row.Substring(77, 15) };
                    //result[3] = new string[] { "ReservedQuantity", row.Substring(92, 15) };
                    //result[4] = new string[] { "PickedQuantity", row.Substring(107, 15) };
                    //result[5] = new string[] { "AvalibleQuantity", row.Substring(122, 15) };
                    break;
                case "SMD":
                    result[0] = new string[] { "StockMovementType", row.Substring(9, 3) };
                    result[4] = new string[] { "Quantity", row.Substring(15, 16) };
                    result[1] = new string[] { "TypeOfOperation", row.Substring(12, 3) };
                    result[2] = new string[] { "Reason", row.Substring(33, 20) };
                    result[3] = new string[] { "DateOfMovement", row.Substring(123, 35) };
                    break;
                case "MEA":
                    string MEASeg = row.Substring(9, 3);
                    string MEAParty = "";
                    switch (MEASeg)
                    {
                        case "GRW":
                            MEAParty = "GrossWeight";
                            break;
                        case "TW ":
                            MEAParty = "";
                            break;
                        case "NW ":
                            MEAParty = "";
                            break;
                        case "TS ":
                            MEAParty = "";
                            break;
                        case "ABJ":
                            MEAParty = "";
                            break;
                    }
                    if (MEAParty != "")
                    {
                        result[0] = new string[] { "Total" + MEAParty, row.Substring(12, 12) };
                        result[1] = new string[] { "MeasurementUnit", row.Substring(48, 3) };
                    }
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
            for(int y = 0; y < counter; y++)
            {
                vals[count] = result[count];
                count++;
            }
            return vals;
        }
        #endregion
    }
}

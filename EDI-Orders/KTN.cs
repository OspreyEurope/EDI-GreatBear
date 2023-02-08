using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    internal class KTN
    {
        public static void ProcessKTN (string file,SqlConnection con)
        {
            string SP = "";


            string OrderType = File.ReadAllLines(file)[0];
            string Decision = OrderType.Substring(30,20);
            switch (Decision)
            {
                case "STKMVT":
                    SP = "";
                    break;
                case "RECCON":
                    SP = "";
                    break;
                case "PPLCON":
                    SP = "";
                    break;
            }

            SqlCommand storedProcedure = new SqlCommand(SP, con);
            storedProcedure.CommandType = CommandType.StoredProcedure;

            //Add Try catch on this block to check it can be read and is not corrupted to prevent a crash sooner rather than later
            using (StreamReader streamReader = File.OpenText(file))
            {
                file = streamReader.ReadToEnd();
            }
            int lines = File.ReadLines(file).Count();
            for (int i = 0; i < lines+1; i++)
            {
                string line = File.ReadLines (file).ElementAt(i);
                var t = ReadKTN(line);




                for (int j = 0; j < t.Length; j++)
                {
                    storedProcedure.Parameters.AddWithValue(t[i][0], t[i][1]);
                }
            }

            storedProcedure.ExecuteNonQuery();
            storedProcedure.Parameters.Clear();
            Console.WriteLine("Message Entered Successfully.");
        }

        #region Read Line For KTN
        public static string[][] ReadKTN(string row)
        {
            /**
             * This created the header variable to check against whitch is positions 7, 8 and 9 in the lie,
             * It then creates a result array which will hold the header for the stored procedure to use and the value.
             */
            string header = row.Substring(7, 3);
            string[][] result = new string[15][];

            /**
             * This switch case handles what is done to the line of the message,
             * This is done through checking the value of the edi header which is located at charcter position 7-9.
             */
            switch (header)
            {
                case "UNH":
                    result[0] = new string[] { "MessageType", row.Substring(30, 20) };
                    result[1] = new string[] { "Warehouse", row.Substring(60, 20) };
                    result[2] = new string[] { "FileRecievedDate", row.Substring(202, 35) };
                    result[3] = new string[] { "File", row.Substring(240, 50) };
                    return result;
                case "FAC":
                    result[0] = new string[] { "FileActionDescription", row.Substring(13, 35) };
                    return result;
                case "TDT":
                    result[0] = new string[] { "TransportReferenceNumber", row.Substring(10, 80) };
                    return result;
                case "RFF":
                    string RFFSeg =  row.Substring(10, 3);
                    switch (RFFSeg)
                    {
                        case "CR1":
                            result[0] = new string[] { "CustomerReference1", row.Substring(13, 35) };
                            break;
                        case "CR2":
                            result[0] = new string[] { "CustomerReference2", row.Substring(13, 35) };
                            break;
                        case "CR3":
                            result[0] = new string[] { "CustomerReference3", row.Substring(13, 35) };
                            break;
                        case "CR4":
                            result[0] = new string[] { "CustomerReference4", row.Substring(13, 35) };
                            break;
                        case "CR5":
                            result[0] = new string[] { "CustomerReference5", row.Substring(13, 35) };
                            break;
                    }
                    return result;
                case "DTM":
                    string DateSeg = row.Substring(10, 3);
                    switch (DateSeg)
                    {
                        case "DEL":
                            result[0] = new string[] { "DeliveryDate", row.Substring(13, 35) };
                            break;
                        case "LOA":
                            result[0] = new string[] { "LoadDate", row.Substring(13, 35) };
                            break;
                    }
                    return result;
                case "NAD":
                    string NADSeg = row.Substring(10, 3);
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
                    }
                    result[0] = new string[] { NADParty + "Name", row.Substring(33, 80) };
                    result[1] = new string[] { NADParty + "Address", row.Substring(113, 80) };
                    result[2] = new string[] { NADParty + "City", row.Substring(193, 20) };
                    result[3] = new string[] { NADParty + "NCountry", row.Substring(303, 80) };
                    result[4] = new string[] { NADParty + "Phone", row.Substring(383, 50) };
                    result[5] = new string[] { NADParty + "Contact", row.Substring(486, 50) };
                    result[6] = new string[] { NADParty + "Email", row.Substring(536, 50) };
                    result[7] = new string[] { NADParty + "VAT", row.Substring(666, 20) };
                    result[8] = new string[] { NADParty + "Currency", row.Substring(756, 3) };
                    result[9] = new string[] { NADParty + "Name2", row.Substring(779, 80) };
                    result[10] = new string[] { NADParty + "Address2", row.Substring(859, 80) };
                    return result;
                case "FTX":
                    string FTXSeg = row.Substring(10, 3);
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
                    result[0] = new string[] { FTXParty + "Comments", row.Substring(13, 120) };
                    return result;
                case "ALI":
                    result[0] = new string[] { "Amount", row.Substring(131, 12) };
                    result[1] = new string[] { "Incoterm", row.Substring(147, 10) };
                    return result;
                case "LIN":
                    result[0] = new string[] { "ItemNumber", row.Substring(40, 25) };
                    result[1] = new string[] { "ItemDescription", row.Substring(65, 80) };
                    return result;
                case "PIA":
                    string PIASeg = row.Substring(10, 3);
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
                    result[0] = new string[] { PIAParty + "ItemNumber", row.Substring(13, 25 ) };
                    result[1] = new string[] { PIAParty + "ItemDescripin", row.Substring(38, 80) };
                    return result;
                case "PAC":
                    string PACSeg = row.Substring(10, 3);
                    string PACParty = "";
                    if (PACSeg == "CUS")
                    { 
                        PACParty = "Customer";
                    }
                    result[0] = new string[] { PACParty + "PackagingCoded", row.Substring(41, 25) };
                    return result;
                case "QTY":
                    string QTYSeg = row.Substring(10, 3);
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
                    }
                    result[0] = new string[] { QTYParty + "Quantity", row.Substring(13, 15) };
                    return result;
                case "TRA":
                    string TRASeg = row.Substring(10, 3);
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
                    result[0] = new string[] { TRAParty + "TrackingCode", row.Substring(13, 35) };
                    return result;
                case "LOC":
                    string LOCSeg = row.Substring(10, 3);
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
                    result[0] = new string[] { LOCParty + "Location", row.Substring(13, 25) };
                    return result;
            }
            return null;
        }
        #endregion
    }
}

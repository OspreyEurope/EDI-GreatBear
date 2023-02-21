﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EDI_Orders
{
    internal class KTN
    {
        public static void ProcessKTN (string file,SqlConnection con)
        {
            con.Open();
            string SP = "";
            string OrderType = File.ReadAllLines(file)[0];
            string Decision = OrderType.Substring(29,20);
            switch (Decision)
            {
                case "STKMVT              ":
                    SP = "OSP_Insert_Stock_Movement";
                    break;
                case "RECCON              ":
                    SP = "OSP_Insert_Reccon";
                    break;
                case "PPLCON              ":
                    SP = "OSP_Insert_Pplcon";
                    break;
            }
            Console.WriteLine(SP);
            SqlCommand storedProcedure = new SqlCommand(SP, con);
            storedProcedure.CommandType = CommandType.StoredProcedure;
            int lineCount = File.ReadLines(file).Count();
            //Add Try catch on this block to check it can be read and is not corrupted to prevent a crash sooner rather than later
            string[] lines = File.ReadAllLines(file);
            Console.WriteLine(file);
            for (int i = 0; i < lineCount; i++)
            {
                string[][] t = ReadKTN(lines[i]);
                if (t != null)
                {
                    for (int j = 0; j < t.Length; j++)
                    {
                        storedProcedure.Parameters.AddWithValue(t[j][0], t[j][1]);
                        Console.WriteLine(t[j][0]);
                        Console.WriteLine(t[j][1]);
                    }
                }
            }
            Console.WriteLine(storedProcedure.Parameters.Count);
            storedProcedure.ExecuteNonQuery();
            storedProcedure.Parameters.Clear();
            con.Close();
            Console.WriteLine("Message Entered Successfully.");
        }

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
                    result[0] = new string[] { "FileAction", row.Substring(12, 35) }; Console.WriteLine(result[0][1]);
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
                    if (row.Length > 131)
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
                        case "REC":
                            QTYParty = "";
                            break;
                        case "TLO":
                            QTYParty = "TotalLoaded";
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
                    result[0] = new string[] { "TotalQuantity", row.Substring(47, 15) };
                    result[1] = new string[] { "BlockedQuantity", row.Substring(62, 15) };
                    result[2] = new string[] { "InOrderQuantity", row.Substring(77, 15) };
                    result[3] = new string[] { "ReservedQuantity", row.Substring(92, 15) };
                    result[4] = new string[] { "PickedQuantity", row.Substring(107, 15) };
                    result[5] = new string[] { "AvilibleQuantity", row.Substring(122, 15) };
                    break;
                case "SMD":
                    result[0] = new string[] { "StockMovementType", row.Substring(9, 3) };
                    result[1] = new string[] { "TypeOfOperation", row.Substring(12, 3) };
                    result[2] = new string[] { "Reason", row.Substring(33, 20) };
                    result[0] = new string[] { "DateOfMovement", row.Substring(123, 35) };
                    break;
                case "MEA":
                    result[0] = new string[] { "TotalGrossWeight", row.Substring(12, 12) };
                    result[1] = new string[] { "MeasurementUnit", row.Substring(48, 25) };
                    break;
            }
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

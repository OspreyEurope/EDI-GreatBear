using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace EDI_Orders
{
    public class SharedFunctions
    {
        #region Header Checks
        public static string HeaderChecks(string header, SqlCommand storedProcedure, string[] order)
        {
            if (header.Equals("dates"))
            {
                switch (order[1])
                {
                    case "137":
                        header = "@RequestedDate";  //Document date
                        break;
                    case "64":
                        header = ""; //Earliest Del date;
                        break;
                    case "63":
                        header = "@PromisedDate"; //Del date latest
                        break;
                    case "2":
                        header = "@PromisedDate";  //Date requested
                        break;
                    case "194":
                        header = ""; //No value found but seen in orders
                        break;
                    case "206":
                        header = ""; //No value found but seen in orders
                        break;
                    case "938":
                        header = "@RequestedDate"; //X12
                        break;
                    case "373":
                        header = ""; //X12
                        break;
                    case "374":
                        header = "@PromisedDate"; //X12
                        break;
                    case "011":
                        header = ""; //
                        break;
                    case "017":
                        header = ""; //
                        break;
                    default:
                        header = null;
                        break;
                }
            }
            if (header == null)
            {
                header = "";
            }
            if (header.Equals("N1"))
            {
                if (!storedProcedure.Parameters.Contains("@Customer"))
                {
                    header = "@Customer";
                }
                else if (!storedProcedure.Parameters.Contains("@Supplier"))
                {
                    header = "@Supplier";
                }
                else if (!storedProcedure.Parameters.Contains("@Warehouse"))
                {
                    header = "@Warehouse";
                }
            }
            if (header.Equals("N2"))
            {
                if (!storedProcedure.Parameters.Contains("@CustomerAddressLine2"))
                {
                    header = "@CustomerAddressLine2";
                }
                else if (!storedProcedure.Parameters.Contains("@SupplierAddressLine2"))
                {
                    header = "@SupplierAddressLine2";
                }
                else if (!storedProcedure.Parameters.Contains("@WarehouseAddressLine2"))
                {
                    header = "@WarehouseAddressLine2";
                }
            }
            if (header.Equals("N3"))
            {
                if (!storedProcedure.Parameters.Contains("@CustomerAddressLine3"))
                {
                    header = "@CustomerAddressLine3";
                }
                else if (!storedProcedure.Parameters.Contains("@SupplierAddressLine3"))
                {
                    header = "@SupplierAddressLine3";
                }
                else if (!storedProcedure.Parameters.Contains("@WarehouseAddressLine3"))
                {
                    header = "@WarehouseAddressLine3";
                }
            }
            if (header.Equals("N4"))
            {
                if (!storedProcedure.Parameters.Contains("@CustomerAddressLine4"))
                {
                    header = "@CustomerAddressLine4";
                }
                else if (!storedProcedure.Parameters.Contains("@SupplierAddressLine4"))
                {
                    header = "@SupplierAddressLine4";
                }
                else if (!storedProcedure.Parameters.Contains("@WarehouseAddressLine4"))
                {
                    header = "@WarehouseAddressLine4";
                }
            }
            if (header.Equals("addressIDs"))
            {
                switch (order[1])
                {
                    case "BY":
                        header = "@CustomerID";
                        break;
                    case "SU":
                        header = "@SupplierID";
                        break;
                    case "DP":
                        header = "@BillingID";
                        break;
                    case "IV":
                        header = "@DeliveryAddress";
                        break;
                    case "GY":
                        header = "";
                        break;
                }
            }
            if (storedProcedure.Parameters.Contains(header) || header.Equals("itemDescription") || header.Equals("additionalProductID") || header.Equals("reference"))
            {
                header = "";
            }

            return header;
        }
        #endregion  

        #region Get Data
        public static string getData(string[] values)
        {
            string a = new string(values[0].Where(c => char.IsLetterOrDigit(c)).ToArray());
            switch (a)
            {
                case "DTM":
                    for (int y = 1; y < values.Length; y++)
                    {
                        if (values[y].Length == 8)
                        {
                            return values[y];
                        }
                    };
                    break;
                case "FTX":
                    return values[2];
                case "NAD":
                    switch (values[1])
                    {
                        case "BY":
                            for (int y = 2; y < values.Length; y++)
                            {
                                if (values[y].Length == 13)
                                {
                                    return values[y];
                                }
                            }
                            break;
                        case "SU":
                            for (int y = 2; y < values.Length; y++)
                            {
                                if (values[y].Length == 13)
                                {
                                    return values[y];
                                }
                            }
                            break;
                        case "DP":
                            for (int y = 2; y < values.Length; y++)
                            {
                                if (values[y].Length == 13)
                                {
                                    return values[y];
                                }
                            }
                            break;
                        case "IV":
                            for (int y = 2; y < values.Length; y++)
                            {
                                if (values[y].Length == 13)
                                {
                                    return values[y];
                                }
                            }
                            break;
                    }
                    break;
                case "MSG":
                    return values[2];
                case "CUX":
                    return values[1];
                case "LIN":
                    for (int y = 1; y < values.Length; y++)
                    {
                        if (values[y].Length > 4)
                        {
                            string[] s = values[y].Split('/');
                            string check = s[0].Substring(0, 1);
                            char c = check.First();
                            if (char.IsDigit(c))
                            {
                                return s[0];
                            }
                        }
                    };
                    break;
                case "N":
                    break;
                case "QTY":
                    return values[1];
                case "SN1":
                    return values[1];
                case "N1":
                    string N1 = "";
                    for (int y = 0; y < values.Length; y++)
                    {
                        if (N1.Equals(""))
                        {
                            N1 = values[y];
                        }
                        else
                        {
                            N1 = N1 + " " + values[y];
                        }
                    }
                    return N1;
                case "N2":
                    string N2 = "";
                    for (int y = 1; y < values.Length; y++)
                    {
                        if (N2.Equals(""))
                        {
                            N2 = values[y];
                        }
                        else
                        {
                            N2 = N2 + " " + values[y];
                        }
                    }
                    return N2;
                case "N3":
                    string N3 = "";
                    for (int y = 1; y < values.Length; y++)
                    {
                        if (N3.Equals(""))
                        {
                            N3 = values[y];
                        }
                        else
                        {
                            N3 = N3 + " " + values[y];
                        }
                    }
                    return N3;
                case "N4":
                    string N4 = "";
                    for (int y = 1; y < values.Length; y++)
                    {
                        if (N4.Equals(""))
                        {
                            N4 = values[y];
                        }
                        else
                        {
                            N4 = N4 + " " + values[y];
                        }
                    }
                    return N4;
                case "SE":
                    Console.WriteLine("Total number of lines in file is: " + values[1]);
                    Console.WriteLine("Order added!");
                    break;
                case "UNT":
                    Console.WriteLine("Total number of lines in file is: " + values[1]);
                    Console.WriteLine("Expected is 11.");
                    Console.WriteLine("Order added!");
                    break;
                default:
                    Console.WriteLine("Yall messed up");
                    break;
            }
            return null;
        }

        #endregion

        #region EDI Type Decision
        /**
         * This function is used to determine between the EDI Fact files and the EDI X12 files.
         * This checks the first line of the file so that it can see which format and then direct to the correct
         * processing function afterwards as both are handled differently.
         */
        public static void EDIDecision(string[][] order)
        {
            SqlConnection conOE = new SqlConnection();
            conOE.ConnectionString = ConfigurationManager.ConnectionStrings["OERADEV"].ConnectionString;
            if (order[0][0].Equals("UNH"))
            {
                EDIFact.BuilRecordEDIFact(order, conOE);
            }
            else
            {
                EDIX12.BuildRecordEDIX12(order, conOE);
            }
        }
        #endregion

        #region Queries DB
        /**
         * This function runs the stored procedure to query the database for the information needed to write the edi file
         */
        public static DataTable QueryDB(SqlConnection con, string SP, string id = null, string id2 = null)
        {
            con.Open();
            SqlCommand dataQuery = new SqlCommand(SP, con);
            dataQuery.CommandType = CommandType.StoredProcedure;
            if (id != null)
            {
                dataQuery.Parameters.AddWithValue("@id", id);
            }
            if (id2 != null)
            {
                dataQuery.Parameters.AddWithValue("@id2", id2);
            }
            DataTable data = new DataTable();
            dataQuery.ExecuteNonQuery();
            SqlDataAdapter adapter = new SqlDataAdapter(dataQuery);
            adapter.Fill(data);
            con.Close();
            return data;
        }
        #endregion

        #region Runs Udpate Query
        /**
         * This function runs the stored procedure thatwil pdate the DB
         */
        public static void UpdateRecords(SqlConnection con, string SP, string id)
        {
            con.Open();
            SqlDataAdapter dataQuery = new SqlDataAdapter(SP, con);
            dataQuery.SelectCommand.CommandType = CommandType.StoredProcedure;
            DataTable data = new DataTable();
            dataQuery.Fill(data);
            con.Close();
        }
        #endregion

        #region Folder Checker
        /**
         * This function takes whichever vender the message is coming from adn will handle the information as required.
         * This will read the correct file location, gather an array of files and then process all the gathered files.
         */
        public static void FileCheck (string A)
        {
            #region Dev Connections
            SqlConnection OERADev = new SqlConnection();
            OERADev.ConnectionString = ConfigurationManager.ConnectionStrings["OERADEV"].ConnectionString;
            SqlConnection OrbisDev = new SqlConnection();
            OrbisDev.ConnectionString = ConfigurationManager.ConnectionStrings["Orbis_Interface"].ConnectionString;
            #endregion

            #region Live Connections
            SqlConnection OERALive = new SqlConnection();
            OERALive.ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString;
            SqlConnection OrbisLive = new SqlConnection();
            OrbisLive.ConnectionString = ConfigurationManager.ConnectionStrings["LiveOrbis"].ConnectionString;
            #endregion


            switch (A)
            {
                case "STKMVT":
                    string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["KTNSTKMVTHolding"]); //Temp for testing
                    foreach (var file in files)
                    {
                        string name = Path.GetFileName(file);
                        try
                        {
                            if (file.Substring(3, 0) == "WEB")
                            {
                                File.Move(file, ConfigurationManager.AppSettings["KTNSTKMVTProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                            }
                            else
                            {
                                Console.WriteLine(file);
                                KTN.ProcessKTN(file, OrbisDev);
                                File.Move(file, ConfigurationManager.AppSettings["KTNSTKMVTProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed Successfully.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Writefile("File Quarantined: " + name, ex.Message);
                        }
                    }
                    break;

                case "PPLCON":
                    files = Directory.GetFiles(ConfigurationManager.AppSettings["KTNPPLCONHolding"]); //Temp for testing
                    Console.WriteLine(files.Length);
                    foreach (var file in files)
                    {
                        string name = Path.GetFileName(file);
                        try
                        {
                            if (file.Substring(3, 0) == "WEB")
                            {
                                File.Move(file, ConfigurationManager.AppSettings["KTNPPLCONProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                            }
                            else
                            {
                                Console.WriteLine(file);
                                KTN.ProcessKTN(file, OrbisDev);
                                File.Move(file, ConfigurationManager.AppSettings["KTNPPLCONProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed Successfully.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Writefile("File Quarantined: " + name, ex.Message);
                        }
                    }
                    break;

                case "RECCON":
                    files = Directory.GetFiles(ConfigurationManager.AppSettings["PKTNRECCONHolding"]); //Temp for testing

                    foreach (var file in files)
                    {
                        string name = Path.GetFileName(file);
                        try
                        {
                            if (file.Substring(3, 0) == "WEB")
                            {
                                File.Move(file, ConfigurationManager.AppSettings["PKTNRECCONProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                            }
                            else
                            {
                                Console.WriteLine(file);
                                KTN.ProcessKTN(file, OrbisLive);
                                File.Move(file, ConfigurationManager.AppSettings["PKTNRECCONProcessed"] + "/" + name);
                                Console.WriteLine(file + " Was Processed Successfully.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Writefile("File Quarantined: " + name, ex.Message);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Log Writing
        public static void Writefile(string v, string z)
        {
            string strPath = @"C:\Temp\KTNOrder.txt";
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine(DateTime.Now + v + " - " + z);
            }
            //Add emailing here
        }
        #endregion
    }
}

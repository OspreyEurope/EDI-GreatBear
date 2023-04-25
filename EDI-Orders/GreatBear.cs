using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    internal class GreatBear
    {
        #region Process GreatBear
        public static void ProcessGreatBear (string file, SqlConnection con)
        {
            try
            {
                string[][] File = FileManipulation.readEDIFile(file);
                string Decision = File[2][1].Trim();

                switch (Decision)
                {
                    #region 846 - STKBAL
                    case "846":    //Items Message
                        foreach (string[] s in File)
                        {
                            foreach (string l in s)
                            {
                                Console.WriteLine(l);
                            }
                            Console.WriteLine("Next Line");
                        }
                        break;
                    #endregion
                    #region 944 - RECCON
                    case "944":    //Inbound Receipt Confirmation Message
                        foreach (string[] s in File)
                        {
                            foreach (string l in s)
                            {
                                Console.WriteLine(l);
                            }
                            Console.WriteLine("Next Line");
                        }
                        break;
                    #endregion
                    #region 945 - PPLCON
                    case "945":    //Assembly Order Message
                        con.Open();
                        SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_PPLCON", con);
                        storedProcedure.CommandType = CommandType.StoredProcedure;
                        int p = 0;
                        string Warehouse = "";
                        string DateRecieved = "";
                        string OrderNumber = "";
                        string Dateshipped = "";
                        string CustomerOrderNumber = "";
                        string transporter = "";
                        string ID = "";

                        foreach (string[] s in File)
                        {
                            string header = s[0].Trim();
                            switch (header)
                            {
                                case "ISA":
                                    Warehouse = s[6];
                                    DateRecieved = s[9];
                                    ID = s[10];
                                    break;
                                case "W27":
                                    transporter = s[1];
                                    break;
                                case "W06":
                                    OrderNumber = s[2];
                                    Dateshipped = s[3];
                                    CustomerOrderNumber = s[6];
                                    break;
                            }

                            if ((header == "LX" && p > 0) || header == "W03")
                            {
                                Console.WriteLine("Now hits insert");
                                storedProcedure.Parameters.AddWithValue("Warehouse", Warehouse);
                                storedProcedure.Parameters.AddWithValue("DateReceived", DateRecieved);
                                storedProcedure.Parameters.AddWithValue("OrderNumber", OrderNumber);
                                storedProcedure.Parameters.AddWithValue("Dateshipped", Dateshipped);
                                //storedProcedure.Parameters.AddWithValue("CustomerOrderNumber", CustomerOrderNumber);
                                storedProcedure.Parameters.AddWithValue("transporter", transporter);
                                storedProcedure.Parameters.AddWithValue("OriginalFileName", file);
                                storedProcedure.Parameters.AddWithValue("PL", "PACK");
                                storedProcedure.Parameters.AddWithValue("ID", ID);
                                storedProcedure.Parameters.AddWithValue("MessageType", "PPLCON");

                                storedProcedure.ExecuteNonQuery();
                                storedProcedure.Parameters.Clear();
                                p++;
                            }
                            if (header == "LX")
                            {
                                p++;
                            }
                            if (header == "N9")
                            {
                                if (storedProcedure.Parameters.Contains("CarriertrackingNo"))
                                {

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
                            else if (header == "W12")
                            {
                                if (storedProcedure.Parameters.Contains("PackedQuantity"))
                                {

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
                        break;
                    #endregion
                    #region 947 - STKMVT
                    case "947":     //Stock adjust and status checks
                        foreach (string[] s in File)
                        {
                            foreach (string l in s)
                            {
                                Console.WriteLine(l);
                            }
                            Console.WriteLine("Next Line");
                        }
                        break;
                    #endregion
                    #region 997
                    case "997":     //Functional acknowledgment message
                        foreach (string[] s in File)
                        {
                            foreach (string l in s)
                            {
                                Console.WriteLine(l);
                            }
                            Console.WriteLine("Next Line");
                        }
                        break;
                    #endregion
                    default:
                        break;

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        #endregion

        #region Write Recon Header

        #endregion

        #region Write Reccon Lines

        #endregion

        #region Handle Line
        public static string[][] HandleLine(string[] line)
        {
            string[][] result = new string[15][];
            Console.WriteLine(line[0].Trim());
            string header = line[0].Trim();
            switch(header)
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
                    result[0] = new string[] {"QuantityReceived", line[1] };
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
    }
}

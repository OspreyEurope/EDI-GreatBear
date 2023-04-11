using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    public class EDIFact
    {
        #region Build Record EDIFact
        /**
        * Using the read file method, it builds the record that will be inserted into the database,
        * It reads through each heading in the file and breaks down the values needed from there,
        * When it has found the value it needs, it then adds that value a a parameter to the stored procedure.
        */
        public static void BuilRecordEDIFact(string[][] order, SqlConnection conOE)
        {

            string result = "";
            string prevHeader = "";
            string DelAddrressBackup = "";
            string RequestedDateBackup = "";

            List<List<string>> temps = new List<List<string>>();
            conOE.Open();
            SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_EDI", conOE);
            storedProcedure.CommandType = CommandType.StoredProcedure;

            /**
             * This is adding values to the stored procedure with a built in check for the dates
             */
            for (int x = 0; x < order.Length - 1; x++)
            {
                string a = new string(order[x][0].Where(c => char.IsLetterOrDigit(c)).ToArray());
                string header = Lookups.ReadLookup(a);
                if (header != null)
                {
                    header = SharedFunctions.HeaderChecks(header, storedProcedure, order[x]);
                    if (!header.Equals("") && !header.Equals("orderEnd") && !header.Equals("priceDetails") && !header.Equals("X12OrderEnd"))
                    {
                        result = SharedFunctions.getData(order[x]);
                        prevHeader = header;
                        storedProcedure.Parameters.AddWithValue(header, result);
                        if (header.Equals("@CustomerID"))
                        {
                            DelAddrressBackup = result;
                        }
                    }

                    /**
                     * This builds up tempdata for if the EDI is not following a UNH and UNT per order e.g. intersport EDIs
                     */
                    List<string> tempData = new List<string>();
                    switch (header)
                    {
                        case "@StockCode":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "@CustomerID":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "@BuyerID":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "@BillingID":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "@SupplierID":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "@DeliveryAddress":
                            tempData.Add(header);
                            tempData.Add(result);
                            temps.Add(tempData);
                            break;
                        case "priceDetails":
                            if ((storedProcedure.Parameters.Count) >= 6 && (storedProcedure.Parameters.Count < 12))
                            {
                                if (!storedProcedure.Parameters.Contains("@DeliveryAddress"))
                                {
                                    storedProcedure.Parameters.AddWithValue("@DeliveryAddress", DelAddrressBackup);
                                }
                                if (!storedProcedure.Parameters.Contains("@RequestedDate"))
                                {
                                    storedProcedure.Parameters.AddWithValue("@RequestedDate", RequestedDateBackup);
                                }

                                storedProcedure.ExecuteNonQuery();
                                storedProcedure.Parameters.Clear();
                                //Run through the temp list and add extra new parameters that are needed
                                for (int y = 0; y < temps.Count; y++)
                                {
                                    storedProcedure.Parameters.AddWithValue(temps[y][0].ToString(), temps[y][1]);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Not enough Parms. " + storedProcedure.Parameters.Count + " parameters added.");
                            }
                            break;
                        case "orderEnd":
                            if (!storedProcedure.Parameters.Contains("@DeliveryAddress"))
                            {
                                storedProcedure.Parameters.AddWithValue("@DeliveryAddress", DelAddrressBackup);
                            }
                            if (!storedProcedure.Parameters.Contains("@RequestedDate"))
                            {
                                storedProcedure.Parameters.AddWithValue("@RequestedDate", RequestedDateBackup);
                            }
                            Console.WriteLine(storedProcedure.Parameters.Count);

                            if ((storedProcedure.Parameters.Count > 6) && (storedProcedure.Parameters.Count < 9))
                            {
                                storedProcedure.ExecuteNonQuery();
                                storedProcedure.Parameters.Clear();
                                Console.WriteLine("Order entered.");
                            }
                            else
                            {
                                Console.WriteLine("File End for intersport.");
                            }
                            break;
                    }
                }
            }
            conOE.Close();
        }
        #endregion

        #region Read KTN
        public static void ReadKTN(string[][] order, SqlConnection conOE)
        {
            for (int x = 0; x < order.Length - 1; x++)
            {
                string[] line = order[x];
                switch (line[0])
                {
                    case "":

                        break;

                }
            }
        }
        #endregion
    }
}

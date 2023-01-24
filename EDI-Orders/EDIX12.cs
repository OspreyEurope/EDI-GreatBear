using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    public class EDIX12
    {
        #region Build Record EDI X12
        /**
         * This function handles any EDI files that are in the X12 format
         */
        public static void BuildRecordEDIX12(string[][] order, SqlConnection conOE)
        {
            string result = "";
            string prevHeader = "";
            List<List<string>> temps = new List<List<string>>();
            conOE.Open();
            SqlCommand storedProcedure = new SqlCommand("OSP_INSERT_EDI_X12", conOE);
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
                    if (!header.Equals("") && !header.Equals("OrderEnd") && !header.Equals("priceDetails") && !header.Equals("X12OrderEnd"))
                    {
                        Console.WriteLine(header);
                        result =  SharedFunctions.getData(order[x]);
                        prevHeader = header;
                        Console.WriteLine(result);
                        storedProcedure.Parameters.AddWithValue(header, result);
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
                        case "OrderEnd":
                            Console.WriteLine(storedProcedure.Parameters.Count);
                            storedProcedure.ExecuteNonQuery();
                            storedProcedure.Parameters.Clear();
                            Console.WriteLine("Order entered.");
                            break;
                    }
                }
            }
            conOE.Close();
        }
        #endregion  
    }
}

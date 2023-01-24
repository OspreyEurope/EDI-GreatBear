using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    public class Lookups
    {
        #region Read Lookup
        public static string ReadLookup(string EDI)
        {
            switch (EDI)
            {
                case "DTM":
                    return "dates";
                case "NAD":
                    return "addressIDs";
                case "LIN":
                    return "@StockCode";
                case "QTY":
                    return "@Qty";
                case "UNT":
                    return "orderEnd";
                case "IEA":
                    return "OrderEnd";
                case "PRI":
                    return "priceDetails";
                case "BY":
                    return "@CustomerID";
                case "SU":
                    return "@SupplierID";
                case "DP":
                    return "@BillingID";
                case "IV":
                    return "@DeliveryID";
                case "N1":
                    return "N1";
                case "N2":
                    return "N2";
                case "N3":
                    return "N3";
                case "N4":
                    return "N4";
                case "SN1":
                    return "@Qty";
                default:
                    return null;
            }
        }
        #endregion

        #region Write Lookup
        public static string WriteLookUp(string Header)
        {
            switch (Header)
            {
                //Addresses
                case "DeliveryDate":
                    return "DTM+DEL";
                case "LoadDate":
                    return "DTM+LOA";


                //Dates
                case "ShippingAddress":
                    return "NAD+CZ";
                case "BuyerAddress":
                    return "NAD+BY";


                //Think i need?
                case "Reference":
                    return "RFF";
                case "PackingUnit":
                    return "PAC";



                case "StockCode":
                    return "LIN";
                case "Quantity":
                    return "Qty";
                case "CustomerID":
                    return "BY";
                case "SupplierID":
                    return "SU";
                case "BillingID":
                    return "DP";
                case "DeliveryID":
                    return "IV";
                default:
                    return null;
            }
        }
        #endregion
    }
}

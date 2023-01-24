﻿using System;
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
                case "Ordernumber":
                    return "";
                case "OrderDate":
                    return "DTM+DEL";
                case "RequestedDate":
                    return "DTM+LOA";
                case "CustomerName":
                    return "NAD+BY+3124";
                case "OrderImportType":
                    return "";
                case "OrderReference":
                    return "RFF+1154";
                case "Priority":
                    return "";
                case "CustomerVATCode":
                    return "NAD+BY+K205";
                case "WareHouse":
                    return "NAD+TRA+3124";
                case "DelPostalName":
                    return "NAD+TRA+3124";
                case "DelCity":
                    return "NAD+TRA+3164";
                case "DelPostCode":
                    return "NAD+TRA+3251";
                case "DelCountry":
                    return "NAD+TRA+K485";
                case "ProductCode":
                    return "LIN+7140";
                case "ProductDescription":
                    return "LIN+7009";
                case "Quantity":
                    return "QTY";
                case "UnitPrice":
                    return "ALI+3203";
                case "Currency":
                    return "ALI+6345";
                case "Incoterms":
                    return "ALI+K301";
                case "LanguageCode":
                    return "FTX+3453";


                //Dates
                case "ShippingAddress":
                    return "NAD+CZ";
                case "BuyerAddress":
                    return "NAD+BY";
                default:
                    return null;
            }
        }
        #endregion
    }
}

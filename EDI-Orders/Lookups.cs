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

        #region ASN Lookup
        public static string ASNLookup (string Header)
        {
            switch (Header)
            {
                case "PurchaseOrderNumber":
                    return "DOC||TRA+UID"; //??
                case "SupplierDocumentNumber":
                    return "NAD+SUP";
                case "Warehouse":
                    return "NAD+DP";
                case "SuppAccRef":
                    return "RFF+SUP"; //??
                case "OrderDate":
                    return ""; //Not sure if needed or wanted and if so which DTM+ value to use
                case "OrderRequestedDate":
                    return "DTM+PLA";
                case "LotCode":
                    return "TRA+LNO";
                case "ProductDescription":
                    return "LIN"; //This needs to be paired with the stockitemcode, this cna be done in the SP
                case "Quantity":
                    return "QTY";
                case "UnitPrice":
                    return "MOA";
                case "Currency":
                    return "MOA"; //This needs to be added on same line as unit price as well
                case "Quality":
                    return ""; //Not sure if this is needed/wanted and how to map it if so
                case "StockItemCode":
                    return "LIN";
                default :
                    return null;
            }
        }
        #endregion

        #region Write Lookup
        public static string WriteLookUp(string Header)
        {
            switch (Header)
            {
                case "OrderNumber":
                    return "RFF+ON";
                case "OrderDate":
                    return "DTM+DEL";
                case "OrderRequestedDate":
                    return "DTM+LOA";
                case "CustomerAccountName":
                    return "NAD+BY";
                case "OrderType":
                    return "TDT";
                case "OrderReference":
                    return "RFF+ON";
                case "Priority":
                    return "TDT";
                case "CustomerVATCode":
                    return "NAD+BY";
                case "Warehouse":
                    return "NAD+DP";
                case "DelPostalName":
                    return "NAD+DP";
                case "DelCity":
                    return "NAD+DP";
                case "DelPostCode":
                    return "NAD+DP";
                case "DelCountry":
                    return "NAD+DP";
                case "ProductCode":
                    return "LIN";
                case "ProductDescription":
                    return "IMD";
                case "Quantity":
                    return "QTY";
                case "UnitPrice":
                    return "ALI+3203";
                case "Currency":
                    return "CUX";
                case "Incoterms":
                    return "ALI";
                case "LanguageCode":
                    return "FTX";
                case "DeliveryRequirements":
                    return "TDT";
                case "DelTelephone":
                    return "NAD+DP";
                case "DelEmail":
                    return "NAD+DP";
                case "DelAddressLine1":
                    return "NAD+DP";
                case "DelAddressLine2":
                    return "NAD+DP";
                case "DelAddressLine3":
                    return "NAD+DP";
                case "DelAddressLine4":
                    return "NAD+DP";
                case "DelCountryCode":
                    return "NAD+DP";
                case "InvoiceAddressLine1":
                    return "NAD+IV";
                case "InvoiceAddressLine2":
                    return "NAD+IV";
                case "InvoiceAddressLine3":
                    return "NAD+IV";
                case "InvoiceAddressLine4":
                    return "NAD+IV";
                case "InvoiceCountry":
                    return "NAD+IV";
                case "InvoiceCity":
                    return "NAD+IV";
                case "InvoicePostCode":
                    return "NAD+IV";
                case "InvoicePostalAddress":
                    return "NAD+IV";
                case "StockCode":
                    return "LIN";
                case "Name":
                    return "IMD";
                case "PartNumber":
                    return "TRA";     //I think
                case "ProductGroup":
                    return "GRI";


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

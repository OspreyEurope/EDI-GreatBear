using System;
using System.Configuration;
using System.Data.SqlClient;

//TODO: Swap Connection strings to move to live,
//TODO: Swap the file locations in EDIWrite to production

namespace EDI_Orders
{
    internal class Program
    {
        #region Run Program Reading Incoming
        static void Main(string[] args)
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

            if (args.Length >= 1)
            {
                string choice = args[0];
                switch (choice)
                {
                    #region KTN Wite File
                    case "WriteOrder":
                        EDIWrite.WriteOrder(OrbisLive, args[1]);
                        break;
                    case "WriteProductList":
                        EDIWrite.WriteProductList(OERALive, "100994002");
                        break;
                    case "PO":
                        EDIWrite.WriteASNFile(OrbisDev, args[1]);
                        break;
                    case "TruckDels":
                        EDIWrite.WriteTruckDelsFile(OrbisDev, args[1]);
                        break;
                    case "Return":
                        EDIWrite.WriteReturnResponce(OrbisDev, args[1]);
                        break;
                    #endregion
                    #region KTN Read
                    case "STKMVT":
                        SharedFunctions.FileCheck(choice);
                        break;
                    case "PPLCON":
                        SharedFunctions.FileCheck(choice);
                        break;
                    case "RECCON":
                        SharedFunctions.FileCheck(choice);
                        break;
                    case "R":
                        SharedFunctions.FileCheck(choice);
                        break;
                        #endregion
                }
            }
            else
            {
                while (true)
                {
                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        #region Testing Wite File
                        case "WriteOrder":
                            EDIWrite.WriteOrder(OrbisDev, "0000467716"); 
                            break;
                        case "WriteProductList":
                            EDIWrite.WriteProductList(OERALive, "100994002");
                            break;
                        case "PO":
                            EDIWrite.WriteASNFile(OrbisDev, "0000023558");
                            EDIWrite.WriteASNFile(OrbisDev, "0000023557");
                            break;
                        case "TruckDels":
                            EDIWrite.WriteTruckDelsFile(OrbisDev, "TRUCK118-2");
                            break;
                        case "Returns":
                            EDIWrite.WriteReturnResponce(OrbisDev, "0000058894");
                            break;
                        #endregion
                        #region KTN Read
                        case "STKMVT":
                            SharedFunctions.FileCheck(choice);
                            break;
                        case "PPLCON":
                            SharedFunctions.FileCheck(choice);
                            break;
                        case "RECCON":
                            SharedFunctions.FileCheck(choice);
                            break;
                        case "R":
                            SharedFunctions.FileCheck(choice);
                            break;
                        #endregion
                        default:
                            SharedFunctions.Writefile("Error with the manual functions.", "Possible an incorrect input: " + choice);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
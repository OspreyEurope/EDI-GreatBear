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
            #region SQL Connections
            SqlConnection OERA = new SqlConnection();
            OERA.ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString;
            SqlConnection Orbis = new SqlConnection();
            Orbis.ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString;
            #endregion

            if (args.Length >= 1)
            {
                string choice = args[0];
                switch (choice)
                {
                    #region KTN Wite File
                    case "WriteOrder":
                        EDIWrite.WriteOrder(Orbis, args[1]);
                        break;
                    case "WriteProductList":
                        EDIWrite.WriteProductList(OERA, "100994002");
                        break;
                    case "PO":
                        EDIWrite.WriteASNFile(Orbis, args[1]);
                        break;
                    case "TruckDels":
                        EDIWrite.WriteTruckDelsFile(Orbis, args[1]);
                        break;
                    case "Return":
                        EDIWrite.WriteReturnResponce(Orbis, args[1]);
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
                            EDIWrite.WriteOrder(Orbis, "0000467716"); 
                            break;
                        case "WriteProductList":
                            EDIWrite.WriteProductList(OERA, "100994002");
                            break;
                        case "PO":
                            EDIWrite.WriteASNFile(Orbis, "0000023558");
                            EDIWrite.WriteASNFile(Orbis, "0000023557");
                            break;
                        case "TruckDels":
                            EDIWrite.WriteTruckDelsFile(Orbis, "TRUCK118-2");
                            break;
                        case "Returns":
                            EDIWrite.WriteReturnResponce(Orbis, "0000058894");
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

                        #region GBD
                        case "944":
                            GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\944.txt", Orbis);
                            break;
                        case "945":
                            GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\945.txt", Orbis);
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
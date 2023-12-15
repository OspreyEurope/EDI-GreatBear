using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

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
            SqlConnection OERA = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString
            };
            SqlConnection Orbis = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
            };
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
                        EDIWrite.WriteReturnResponse(Orbis, args[1]);
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
                    #region GB Write File
                    case "WriteOrderGB":
                        EDIWrite.WriteOrderGB(Orbis, args[1]);
                        break;
                    case "WriteProductListGB":
                        EDIWrite.WriteProductListGBItems(OERA, "108016515");
                        break;
                    case "POGB":
                        EDIWrite.WritePOGB(Orbis, "0000099999");
                        break;
                    case "ReturnGB":
                        EDIWrite.ReturnToGB(Orbis, args[1]);
                        break;
                    #endregion
                    #region GB Read
                    case "ReadGB":
                        GreatBear.ProcessGreatBear(Orbis);
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
                        #region KTN Write
                        case "WriteOrder":
                            EDIWrite.WriteOrder(Orbis, "0000419040");
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
                            EDIWrite.WriteReturnResponse(Orbis, "0000060553");
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
                        #region GBD Writes
                        case "940":
                            EDIWrite.WriteOrderGB(Orbis, "0000435970");
                            //EDIWrite.WriteOrderGB(Orbis, "0000544492");
                            //EDIWrite.WriteOrderGB(Orbis, "0000485292");
                            //EDIWrite.WriteOrderGB(Orbis, "0000529087");
                            //EDIWrite.WriteOrderGB(Orbis, "0000501495");
                            //EDIWrite.WriteOrderGB(Orbis, "0000501497");
                            //EDIWrite.WriteOrderGB(Orbis, "0000501499");
                            //EDIWrite.WriteOrderGB(Orbis, "0000460384");
                            break;
                        case "846":
                            EDIWrite.WriteProductListGBItems(OERA, "108016515");
                            break;
                        case "856":
                            EDIWrite.WritePOGB(Orbis, "0000027186");
                            EDIWrite.WritePOGB(Orbis, "0000027187");
                            EDIWrite.WritePOGB(Orbis, "0000027226");
                            EDIWrite.WritePOGB(Orbis, "0000026800");
                            EDIWrite.WritePOGB(Orbis, "0000024548");
                            EDIWrite.WritePOGB(Orbis, "0000026814");
                            EDIWrite.WritePOGB(Orbis, "0000026824");
                            //EDIWrite.WritePOGB(Orbis, "0000026864");
                            //EDIWrite.WritePOGB(Orbis, "0000026814");
                            //EDIWrite.WritePOGB(Orbis, "0000026800");
                            //EDIWrite.WritePOGB(Orbis, "0000024548");
                            //EDIWrite.WritePOGB(Orbis, "0000026824");
                            break;
                        case "returnGB":
                            EDIWrite.ReturnToGB(Orbis, "0000066986");
                            break;
                        #endregion
                        #region GBD Read
                        case "944":
                            //GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\944.txt.edi", Orbis);
                            GreatBear.ProcessGreatBear(Orbis);
                            break;
                        //case "947":
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\947.txt.edi", Orbis);
                        //    break;
                        //case "9472":
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\9472.txt.edi", Orbis);
                        //    break;
                        //case "8462":
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\846.txt.edi", Orbis);
                        //    break;
                        //case "945":
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\945-4.txt", Orbis);
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\945-3.txt", Orbis);
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\945-2.txt", Orbis);
                        //    GreatBear.ProcessGreatBear("C:\\Bespoke\\EDI\\GreatBearSamples\\945-1.txt", Orbis);
                            //break;
                        #endregion
                        case "GUI":
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new GUI());
                            break;
                        default:
                            SharedFunctions.Writefile("Error with the manual functions.", "Possibly an incorrect input: " + choice);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
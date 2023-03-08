using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Messaging;
using System.Web;
using static System.Net.WebRequestMethods;
using File = System.IO.File;




//TODO: Extra info passing???
//TODO: Swap the run program to utilize the EDI Type Decision function,
//            string[][] file = readEDIFile("C://Bespoke/EDITest.edi");
//            EDIDecision(file);
//GitHub created and all files pushed



namespace EDI_Orders
{
    internal class Program
    {
        #region Run Program Reading Incoming
        static void Main(string[] args)
        {
            SqlConnection conOE = new SqlConnection();
            conOE.ConnectionString = ConfigurationManager.ConnectionStrings["OERADEV"].ConnectionString;
            SqlConnection conDev = new SqlConnection();
            conDev.ConnectionString = ConfigurationManager.ConnectionStrings["OERADEVORBIS"].ConnectionString;
            SqlConnection conLive = new SqlConnection();
            conLive.ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString;

            /**
             * These are the current files that can be used for testing
             */
            //string choice = Console.ReadLine();  //Infinite loop
            while (true)
            {
                SqlConnection conKTN = new SqlConnection();
                conKTN.ConnectionString = ConfigurationManager.ConnectionStrings["Orbis_Interface"].ConnectionString;
                string choice = Console.ReadLine();
                switch (choice)
                {
                    #region OG EDIS
                    //OG EDIS
                    case "1":
                        string[][] file = FileManipulation.readEDIFile("C://Bespoke/EDITest.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "2":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/226523-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "3":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233028-1.edi");
                        SharedFunctions.EDIDecision(file); 
                        break;
                    case "4":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233043-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "5":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233076-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "6":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233112-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "7":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233149-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "8":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233188-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "9":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268574-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "10":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268645-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "11":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268715-1.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "12":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233112-2.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region Karstad EDIS
                    case "13":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078555532.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "14":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078725615.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "15":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078735991.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "16":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078744400.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "17":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078744400.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "18":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078909416.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "19":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078919654.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "20":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079061953.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "21":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079081821.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "22":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079162500.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "23":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079315138.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "24":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079492636.edi");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region Intersport EDIS
                    case "25":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229800.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "26":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229801.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "27":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229802.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "28":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229803.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "29":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229804.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "30":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229805.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "31":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229806.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "32":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229807.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "33":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229808.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "34":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229809.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "35":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229810.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "36":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229811.wfa");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region EIUK
                    case "37":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02677841_COB_20221020092057.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "38":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02677843_COB_20221020092108.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "39":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678031_COB_20221020092109.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "40":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678032_COB_20221020092110.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "41":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678033_COB_20221020092110.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "42":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678034_COB_20221020092111.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "43":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678035_COB_20221020092112.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "44":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678036_COB_20221020092112.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "45":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678037_COB_20221020092113.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "46":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678038_COB_20221020092113.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "47":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678039_COB_20221020092114.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region EIUK P2
                    case "48":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779543_COB_20230110033050.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "49":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779544_COB_20230110033051.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "50":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779545_COB_20230110033051.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "51":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779546_COB_20230110033052.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "52":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779547_COB_20230110033052.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "53":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779548_COB_20230110033054.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "54":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779550_COB_20230110033055.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "55":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779552_COB_20230110033055.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "56":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779560_COB_20230110033057.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "57":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779561_COB_20230110033058.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "58":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779569_COB_20230110033059.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "59":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779570_COB_20230110033059.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    case "60":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779576_COB_20230110033100.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region Tests all files at once
                    case "404":
                        file = FileManipulation.readEDIFile("C://Bespoke/EDITest.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/226523-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233028-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233043-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233076-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233112-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233149-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233188-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268574-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268645-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/268715-1.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/233112-2.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078555532.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078725615.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078735991.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078744400.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078744400.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078909416.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00078919654.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079061953.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079081821.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079162500.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079315138.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/00079492636.edi");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229800.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229801.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229802.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229803.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229804.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229805.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229806.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229807.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229808.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229809.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229810.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/P_5056302200001_ORDERS_1154229811.wfa");
                        EDIFact.BuilRecordEDIFact(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02677841_COB_20221020092057.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02677843_COB_20221020092108.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678031_COB_20221020092109.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678032_COB_20221020092110.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678033_COB_20221020092110.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678034_COB_20221020092111.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678035_COB_20221020092112.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678036_COB_20221020092112.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678037_COB_20221020092113.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678038_COB_20221020092113.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02678039_COB_20221020092114.856");
                        EDIX12.BuildRecordEDIX12(file, conOE);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779543_COB_20230110033050.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779544_COB_20230110033051.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779545_COB_20230110033051.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779546_COB_20230110033052.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779547_COB_20230110033052.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779548_COB_20230110033054.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779550_COB_20230110033055.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779552_COB_20230110033055.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779560_COB_20230110033057.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779561_COB_20230110033058.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779569_COB_20230110033059.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779570_COB_20230110033059.856");
                        SharedFunctions.EDIDecision(file);
                        file = FileManipulation.readEDIFile("C://Bespoke/EDI/EXPD856_02779576_COB_20230110033100.856");
                        SharedFunctions.EDIDecision(file);
                        break;
                    #endregion
                    #region Testing Wite File
                    case "A":
                        EDIWrite.WriteOrder(conKTN);
                        break;
                    case "Products":
                        EDIWrite.WriteProductList(conLive, "100994002");
                        break;
                    case "PO":
                        EDIWrite.WriteASNFile(conDev, "0000021672");
                        break;
                    case "B":
                        EDIWrite.WriteTruckDelsFile(conDev, "TRUCK103");
                        break;
                    #endregion
                    #region KTN Read
                    case "KTN":
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/STKADJ00000005.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/STKADJ00000008.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/STKADJ00000009.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/STKADJ00000010.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/STKADJ00000011.txt", conDev);         //Seems to work

                        KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/RECCON00000003.txt", conDev);         //Seems to work
                        KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/RECCON00000005.txt", conDev);         //Seems to work
                        KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/RECCON00000006.txt", conDev);         //Seems to work
                        KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/RECCON00000007.txt", conDev);         //Seems to work
                        KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/RECCON00000008.txt", conDev);         //Seems to work

                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/PPLCON00000018.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/PPLCON00000017.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/PPLCON00000012.txt", conDev);         //Seems to work
                        //KTN.ProcessKTN("C://Bespoke/EDI/KTNSamples/PPLCON00000011.txt", conDev);         //Seems to work
                        break;
                    #endregion
                    default:
                        Console.WriteLine("Please input a number between 1 and 60.");
                        break;
                }
            }
        }
        #endregion
    }
}
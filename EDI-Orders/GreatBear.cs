using System;
using System.Collections.Generic;
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
            string[][] File = FileManipulation.readEDIFile(file);
            string Decision = File[2][1];

            switch (Decision)
            {
                case "846":    //Items Message

                    break;
                case "944":    //Inbound Receipt Confirmation Message

                    break;
                case "945":    //Assembly Order Message

                    break;

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

            switch(line[0])
            {
                case "ISA":
                    result[0] = new string[] { "Warehouse", line[6]};
                    result[1] = new string[] { "DateRecieved", line[9] };
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

                    break;
                case "N1":

                    break;
                case "N9":

                    break;
                case "WO6":

                    break;
                case "W27":

                    break;
                case "LX":

                    break;
                case "W12":

                    break;
                case "WO3":

                    break;
                default:
                    Console.WriteLine("Error");
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

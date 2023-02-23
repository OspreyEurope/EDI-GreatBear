using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Orders
{
    public class FileManipulation
    {
        #region Read File
        /**
         * This method reads from the edi file into an array of each line,
         * Then it reads each line split into each value splitting on the + and : sectio,
         * It outputs the values whilst omitting the blanks,
         * Then reurns the order as the multidimensional array to be used.
         */
        public static string[][] readEDIFile(string EDIFileName)
        {
            string[] EDIOrder = new string[1];
            string EDIFileTxt;
            using (StreamReader streamReader = File.OpenText(EDIFileName))
            {
                EDIFileTxt = streamReader.ReadToEnd();
            }
            string[] lines = EDIFileTxt.Split('\'', '~');
            string[][] order = new string[lines.Length][];
            int countLine = 0;
            foreach (string line in lines)
            {
                char[] spliters = { '+', ':', '*' };
                string[] seperateData = line.Split(spliters);
                order[countLine] = seperateData.Where((c => !string.IsNullOrEmpty(c))).ToArray();
                countLine++;
            }
            return order;
        }
        #endregion

        #region Write File
        public static void writeEDIFile()
        {

        }
        #endregion
    }
}

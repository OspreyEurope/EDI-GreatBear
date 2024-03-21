using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EDI_Orders
{
    internal class FileProcess
    {
        #region This gathers all the files in the target location
        public static string[] GatherFiles(string directory)
        {
            MessageBox.Show("Please Select a file location.");

            string[] files = Directory.GetFiles(directory);

            return files;
        }
        #endregion
    }
}

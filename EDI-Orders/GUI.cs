using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EDI_Orders
{
    public partial class GUI : Form
    {
        #region Generic GUI creation
        public GUI()
        {
            InitializeComponent();
        }

        private void GUI_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region Buttons for KTN
        private void Create_Orde_KTNbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connections
            SqlConnection Orbis = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
            };
            #endregion
            string orderno = this.Order_Number_KTNtxt.Text;
            if (orderno.Length == 10)
            {
                EDIWrite.WriteOrder(Orbis, orderno);
                MessageBox.Show("Order has been created");
            }
            else
            {
                MessageBox.Show("Please insert an order number, this should be 10 digits long.");
            }
        }

        private void Create_PO_KTNbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connections
            SqlConnection Orbis = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
            };
            #endregion
            string PONo = this.PO_Number_KTNtxt.Text;
            if (PONo.Length == 10)
            {
                EDIWrite.WriteASNFile(Orbis, PONo);
                MessageBox.Show("PO has been created");
            }
            else
            {
                MessageBox.Show("Please insert a purchase order number, this should be 10 digits long.");
            }
        }

        private void Product_List_KTNbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connection
            SqlConnection OERA = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString
            };
            #endregion
            EDIWrite.WriteProductList(OERA, "100994002");
            MessageBox.Show("Product List has been created");
        }

        private void Read_KTNbtn_Click(object sender, EventArgs e)
        {
            if (this.KTNFileTypelist.SelectedItems.Count == 1)
            {
                string Choice = this.KTNFileTypelist.SelectedItems[0].Text;
                string file = this.File_Pathtxt.Text;
                Console.WriteLine(Choice + "      " +  file);
                FileCheckKTN(file, Choice);
            }
            
        }
        #endregion

        #region Buttons for GBD
        private void Create_Order_GBDbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connections
            SqlConnection Orbis = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
            };
            #endregion
            string orderno = this.Order_Number_GBDtxt.Text;
            if (orderno.Length == 10)
            {
                EDIWrite.WriteOrderGB(Orbis, orderno);
                MessageBox.Show("Order has been created");
            }
            else
            {
                MessageBox.Show("Please insert an order number, this should be 10 digits long.");
            }
        }

        private void Create_PO_GBDbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connections
            SqlConnection Orbis = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
            };
            #endregion
            string PONo = this.PO_Number_KTNtxt.Text;
            if (PONo.Length == 10)
            {
                EDIWrite.WritePOGB(Orbis, PONo);
                MessageBox.Show("PO has been created");
            }
            else
            {
                MessageBox.Show("Please insert a purchase order number, this should be 10 digits long.");
            }
        }

        private void Product_List_GBDbtn_Click(object sender, EventArgs e)
        {
            #region SQL Connection
            SqlConnection OERA = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString
            };
            #endregion
            EDIWrite.WriteProductListGBItems(OERA, "108016515");
            MessageBox.Show("Product List has been created");
        }

        private void READ_GBDbtn_Click(object sender, EventArgs e)
        {
            string file = this.File_Pathtxt.Text;
            FileCheckGBD(file);
        }
        #endregion

        #region File Handling
        private void Select_Filebtn_Click(object sender, EventArgs e)
        {
            string Path = "";
            var T = new Thread((ThreadStart)(() =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = "c:\\";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var path = ofd.FileName;
                }
                Path = ofd.FileName;
            }));

            T.SetApartmentState(ApartmentState.STA);
            T.Start();
            T.Join();

            this.File_Pathtxt.Text = Path;

            MessageBox.Show("File selected, please see check the textbox to see if the file is correct");
        }

        public static void FileCheckKTN(string file, string Type)
        {
            switch (Type)
            {
                case "STKMVT":
                    string name = Path.GetFileName(file);
                    try
                    {
                        if (file.Substring(3, 0) == "WEB")
                        {
                            File.Move(file, ConfigurationManager.AppSettings["KTNSTKMVTProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                        }
                        else
                        {
                            Console.WriteLine(file);
                            KTN.ProcessKTN(file, Orbis);
                            File.Move(file, ConfigurationManager.AppSettings["KTNSTKMVTProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed Successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("File Quarantined: " + name, ex.Message);
                        SharedFunctions.ErrorAlert("Read STKMVT", ex);
                        File.Move(file, ConfigurationManager.AppSettings["KTNSTKMVTQuarantined"] + "/" + name + "&" + DateTime.Now);
                    }
                    break;

                case "PPLCON":

                    name = Path.GetFileName(file);
                    try
                    {
                        if (file.Substring(3, 0) == "WEB")
                        {
                            File.Move(file, ConfigurationManager.AppSettings["KTNPPLCONProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                        }
                        else
                        {
                            Console.WriteLine(file);
                            KTN.ProcessKTN(file, Orbis);
                            File.Move(file, ConfigurationManager.AppSettings["KTNPPLCONProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed Successfully.");
                        }

                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("File Quarantined: " + name, ex.Message);
                        SharedFunctions.ErrorAlert("Read PPLCON", ex);
                        File.Move(file, ConfigurationManager.AppSettings["KTNPPLCONQuarantined"] + "/" + name + "&" + DateTime.Now);
                    }

                    break;

                case "RECCON":
                    name = Path.GetFileName(file);
                    try
                    {
                        if (file.Substring(3, 0) == "WEB")
                        {
                            File.Move(file, ConfigurationManager.AppSettings["KTNRECCONProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed and moved to EU network Successfully.");
                        }
                        else
                        {
                            Console.WriteLine(file);
                            KTN.ProcessKTN(file, Orbis);
                            File.Move(file, ConfigurationManager.AppSettings["KTNRECCONProcessed"] + "/" + name);
                            Console.WriteLine(file + " Was Processed Successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.Writefile("File Quarantined: " + name, ex.Message);
                        SharedFunctions.ErrorAlert("Read RECCON", ex);
                        File.Move(file, ConfigurationManager.AppSettings["KTNRECCONQuarantined"] + "/" + name + "&" + DateTime.Now);
                    }

                    break;
                default:
                    break;
            }
        }

        public static void FileCheckGBD(string file)
        {
            SqlConnection con = Orbis;
            string[] exists = Directory.GetFiles(ConfigurationManager.AppSettings["GBProcessed"]);
            if ((exists.Contains("RECCON" + file)) || (exists.Contains("PPLCON" + file)) || (exists.Contains("STKMVT" + file)))
            {
                string name = Path.GetFileName(file);
                File.Move(file, ConfigurationManager.AppSettings["GBQuarantined"] + "/" + name);
                SharedFunctions.Writefile("Write Product List Failed to process, error message is: The file already exists.", "");
                SharedFunctions.ErrorAlert("Write Product List", new Exception("Repeat File"));
            }
            else
            {
                try
                {
                    string[][] Document = FileManipulation.readEDIFile(file);
                    string Decision = Document[2][1].Trim();
                    Console.WriteLine(Decision);
                    switch (Decision)
                    {
                        #region 846 - STKBAL
                        case "846":    //Items Message
                            GreatBear.WriteSTKBAL(con, Document, file);
                            break;
                        #endregion
                        #region 944 - RECCON
                        case "944":    //Inbound Receipt Confirmation Message
                            Console.WriteLine("WriteReccon");
                            GreatBear.WriteRECCON(con, Document, file);
                            break;
                        #endregion
                        #region 945 - PPLCON
                        case "945":    //Assembly Order Message
                            GreatBear.WritePPLCON(con, Document, file);
                            break;
                        #endregion
                        #region 947 - STKMVT
                        case "947":     //Stock adjust and status checks
                            GreatBear.WriteSTKMVT(con, Document, file);
                            break;
                        #endregion
                        #region 997 - File Acknowledgment
                        case "997":     //Functional acknowledgment message
                            GreatBear.Handle997(con, Document, file);
                            break;
                        #endregion
                        default:
                            break;

                    }
                }
                catch (Exception ex)
                {
                    SharedFunctions.Writefile("File Quarantined: " + file + " ", ex.Message);
                    SharedFunctions.ErrorAlert("Read GBD file on button from GUI: ", ex);
                }
            }


        }
        #endregion
    }
}

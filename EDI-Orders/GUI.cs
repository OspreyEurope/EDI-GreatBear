using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDI_Orders
{
    public partial class GUI : Form
    {
        #region SQL Connections
        private SqlConnection OERA = new SqlConnection
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["OERA"].ConnectionString
        };
        private SqlConnection Orbis = new SqlConnection
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["Orbis"].ConnectionString
        };
        #endregion

        public GUI()
        {
            InitializeComponent();
        }

        private void GUI_Load(object sender, EventArgs e)
        {
            
        }

        private void Create_Orde_KTNbtn_Click(object sender, EventArgs e)
        {
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
            EDIWrite.WriteProductList(OERA, "100994002");
            MessageBox.Show("Product List has been created");
        }

        private void Create_Order_GBDbtn_Click(object sender, EventArgs e)
        {
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
            EDIWrite.WriteProductListGBItems(OERA, "108016515");
            MessageBox.Show("Product List has been created");
        }

        private void Read_KTNbtn_Click(object sender, EventArgs e)
        {
            
        }

        private void READ_GBDbtn_Click(object sender, EventArgs e)
        {

        }

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

    }
}

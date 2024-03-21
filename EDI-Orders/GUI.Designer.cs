namespace EDI_Orders
{
    partial class GUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "STKMVT"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))));
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "RECCON"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))));
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "PPLCON"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))));
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI));
            this.Create_Orde_KTNbtn = new System.Windows.Forms.Button();
            this.Create_PO_KTNbtn = new System.Windows.Forms.Button();
            this.KTNl = new System.Windows.Forms.Label();
            this.Create_Order_GBDbtn = new System.Windows.Forms.Button();
            this.Create_PO_GBDbtn = new System.Windows.Forms.Button();
            this.Product_List_KTNbtn = new System.Windows.Forms.Button();
            this.Product_List_GBDbtn = new System.Windows.Forms.Button();
            this.GBDl = new System.Windows.Forms.Label();
            this.Order_Number_KTNtxt = new System.Windows.Forms.TextBox();
            this.PO_Number_KTNtxt = new System.Windows.Forms.TextBox();
            this.Order_Number_GBDtxt = new System.Windows.Forms.TextBox();
            this.PO_Number_GBDtxt = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Read_KTNbtn = new System.Windows.Forms.Button();
            this.READ_GBDbtn = new System.Windows.Forms.Button();
            this.File_Pathtxt = new System.Windows.Forms.TextBox();
            this.Select_Filebtn = new System.Windows.Forms.Button();
            this.KTNFileTypelist = new System.Windows.Forms.ListView();
            this.SelectLocationBtn = new System.Windows.Forms.Button();
            this.FileLocationtxt = new System.Windows.Forms.TextBox();
            this.SearchPhraseLbl = new System.Windows.Forms.Label();
            this.SearchPhrasetxt = new System.Windows.Forms.TextBox();
            this.DateSelectorLbl = new System.Windows.Forms.Label();
            this.UseDatesCheck = new System.Windows.Forms.CheckBox();
            this.SearchFilesLbl = new System.Windows.Forms.Label();
            this.SearchFilesbtn = new System.Windows.Forms.Button();
            this.ResultsLV = new System.Windows.Forms.ListView();
            this.DateRangeSelectorTbl = new System.Windows.Forms.MonthCalendar();
            this.FileSearchLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Create_Orde_KTNbtn
            // 
            this.Create_Orde_KTNbtn.Location = new System.Drawing.Point(15, 63);
            this.Create_Orde_KTNbtn.Name = "Create_Orde_KTNbtn";
            this.Create_Orde_KTNbtn.Size = new System.Drawing.Size(75, 23);
            this.Create_Orde_KTNbtn.TabIndex = 0;
            this.Create_Orde_KTNbtn.Text = "Create Order";
            this.Create_Orde_KTNbtn.UseVisualStyleBackColor = true;
            this.Create_Orde_KTNbtn.Click += new System.EventHandler(this.Create_Orde_KTNbtn_Click);
            // 
            // Create_PO_KTNbtn
            // 
            this.Create_PO_KTNbtn.Location = new System.Drawing.Point(15, 113);
            this.Create_PO_KTNbtn.Name = "Create_PO_KTNbtn";
            this.Create_PO_KTNbtn.Size = new System.Drawing.Size(75, 23);
            this.Create_PO_KTNbtn.TabIndex = 1;
            this.Create_PO_KTNbtn.Text = "Create PO";
            this.Create_PO_KTNbtn.UseVisualStyleBackColor = true;
            this.Create_PO_KTNbtn.Click += new System.EventHandler(this.Create_PO_KTNbtn_Click);
            // 
            // KTNl
            // 
            this.KTNl.AutoSize = true;
            this.KTNl.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KTNl.Location = new System.Drawing.Point(65, 9);
            this.KTNl.Name = "KTNl";
            this.KTNl.Size = new System.Drawing.Size(72, 31);
            this.KTNl.TabIndex = 2;
            this.KTNl.Text = "KTN";
            // 
            // Create_Order_GBDbtn
            // 
            this.Create_Order_GBDbtn.Location = new System.Drawing.Point(12, 283);
            this.Create_Order_GBDbtn.Name = "Create_Order_GBDbtn";
            this.Create_Order_GBDbtn.Size = new System.Drawing.Size(75, 23);
            this.Create_Order_GBDbtn.TabIndex = 3;
            this.Create_Order_GBDbtn.Text = "Create Order";
            this.Create_Order_GBDbtn.UseVisualStyleBackColor = true;
            this.Create_Order_GBDbtn.Click += new System.EventHandler(this.Create_Order_GBDbtn_Click);
            // 
            // Create_PO_GBDbtn
            // 
            this.Create_PO_GBDbtn.Location = new System.Drawing.Point(12, 325);
            this.Create_PO_GBDbtn.Name = "Create_PO_GBDbtn";
            this.Create_PO_GBDbtn.Size = new System.Drawing.Size(75, 23);
            this.Create_PO_GBDbtn.TabIndex = 4;
            this.Create_PO_GBDbtn.Text = "Create PO";
            this.Create_PO_GBDbtn.UseVisualStyleBackColor = true;
            this.Create_PO_GBDbtn.Click += new System.EventHandler(this.Create_PO_GBDbtn_Click);
            // 
            // Product_List_KTNbtn
            // 
            this.Product_List_KTNbtn.Location = new System.Drawing.Point(15, 175);
            this.Product_List_KTNbtn.Name = "Product_List_KTNbtn";
            this.Product_List_KTNbtn.Size = new System.Drawing.Size(226, 23);
            this.Product_List_KTNbtn.TabIndex = 5;
            this.Product_List_KTNbtn.Text = "Generate Product List KTN";
            this.Product_List_KTNbtn.UseVisualStyleBackColor = true;
            this.Product_List_KTNbtn.Click += new System.EventHandler(this.Product_List_KTNbtn_Click);
            // 
            // Product_List_GBDbtn
            // 
            this.Product_List_GBDbtn.Location = new System.Drawing.Point(12, 368);
            this.Product_List_GBDbtn.Name = "Product_List_GBDbtn";
            this.Product_List_GBDbtn.Size = new System.Drawing.Size(226, 23);
            this.Product_List_GBDbtn.TabIndex = 6;
            this.Product_List_GBDbtn.Text = "Generate Great Bear Product List";
            this.Product_List_GBDbtn.UseVisualStyleBackColor = true;
            this.Product_List_GBDbtn.Click += new System.EventHandler(this.Product_List_GBDbtn_Click);
            // 
            // GBDl
            // 
            this.GBDl.AutoSize = true;
            this.GBDl.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GBDl.Location = new System.Drawing.Point(38, 227);
            this.GBDl.Name = "GBDl";
            this.GBDl.Size = new System.Drawing.Size(156, 31);
            this.GBDl.TabIndex = 7;
            this.GBDl.Text = "Great Bear";
            // 
            // Order_Number_KTNtxt
            // 
            this.Order_Number_KTNtxt.Location = new System.Drawing.Point(121, 66);
            this.Order_Number_KTNtxt.Name = "Order_Number_KTNtxt";
            this.Order_Number_KTNtxt.Size = new System.Drawing.Size(119, 20);
            this.Order_Number_KTNtxt.TabIndex = 8;
            // 
            // PO_Number_KTNtxt
            // 
            this.PO_Number_KTNtxt.Location = new System.Drawing.Point(121, 113);
            this.PO_Number_KTNtxt.Name = "PO_Number_KTNtxt";
            this.PO_Number_KTNtxt.Size = new System.Drawing.Size(119, 20);
            this.PO_Number_KTNtxt.TabIndex = 9;
            // 
            // Order_Number_GBDtxt
            // 
            this.Order_Number_GBDtxt.Location = new System.Drawing.Point(118, 283);
            this.Order_Number_GBDtxt.Name = "Order_Number_GBDtxt";
            this.Order_Number_GBDtxt.Size = new System.Drawing.Size(119, 20);
            this.Order_Number_GBDtxt.TabIndex = 10;
            // 
            // PO_Number_GBDtxt
            // 
            this.PO_Number_GBDtxt.Location = new System.Drawing.Point(118, 325);
            this.PO_Number_GBDtxt.Name = "PO_Number_GBDtxt";
            this.PO_Number_GBDtxt.Size = new System.Drawing.Size(119, 20);
            this.PO_Number_GBDtxt.TabIndex = 11;
            // 
            // Read_KTNbtn
            // 
            this.Read_KTNbtn.Location = new System.Drawing.Point(275, 46);
            this.Read_KTNbtn.Name = "Read_KTNbtn";
            this.Read_KTNbtn.Size = new System.Drawing.Size(190, 90);
            this.Read_KTNbtn.TabIndex = 12;
            this.Read_KTNbtn.Text = "Read KTN File";
            this.Read_KTNbtn.UseVisualStyleBackColor = true;
            this.Read_KTNbtn.Click += new System.EventHandler(this.Read_KTNbtn_Click);
            // 
            // READ_GBDbtn
            // 
            this.READ_GBDbtn.Location = new System.Drawing.Point(364, 283);
            this.READ_GBDbtn.Name = "READ_GBDbtn";
            this.READ_GBDbtn.Size = new System.Drawing.Size(190, 90);
            this.READ_GBDbtn.TabIndex = 13;
            this.READ_GBDbtn.Text = "Read GBD File";
            this.READ_GBDbtn.UseVisualStyleBackColor = true;
            this.READ_GBDbtn.Click += new System.EventHandler(this.READ_GBDbtn_Click);
            // 
            // File_Pathtxt
            // 
            this.File_Pathtxt.Location = new System.Drawing.Point(305, 177);
            this.File_Pathtxt.Name = "File_Pathtxt";
            this.File_Pathtxt.Size = new System.Drawing.Size(287, 20);
            this.File_Pathtxt.TabIndex = 14;
            // 
            // Select_Filebtn
            // 
            this.Select_Filebtn.Location = new System.Drawing.Point(407, 223);
            this.Select_Filebtn.Name = "Select_Filebtn";
            this.Select_Filebtn.Size = new System.Drawing.Size(116, 35);
            this.Select_Filebtn.TabIndex = 15;
            this.Select_Filebtn.Text = "Select File";
            this.Select_Filebtn.UseVisualStyleBackColor = true;
            this.Select_Filebtn.Click += new System.EventHandler(this.Select_Filebtn_Click);
            // 
            // KTNFileTypelist
            // 
            this.KTNFileTypelist.GridLines = true;
            this.KTNFileTypelist.HideSelection = false;
            this.KTNFileTypelist.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.KTNFileTypelist.Location = new System.Drawing.Point(487, 43);
            this.KTNFileTypelist.Name = "KTNFileTypelist";
            this.KTNFileTypelist.Size = new System.Drawing.Size(105, 97);
            this.KTNFileTypelist.TabIndex = 17;
            this.KTNFileTypelist.UseCompatibleStateImageBehavior = false;
            this.KTNFileTypelist.View = System.Windows.Forms.View.List;
            // 
            // SelectLocationBtn
            // 
            this.SelectLocationBtn.Location = new System.Drawing.Point(619, 57);
            this.SelectLocationBtn.Name = "SelectLocationBtn";
            this.SelectLocationBtn.Size = new System.Drawing.Size(227, 36);
            this.SelectLocationBtn.TabIndex = 18;
            this.SelectLocationBtn.Text = "Select Location";
            this.SelectLocationBtn.UseVisualStyleBackColor = true;
            this.SelectLocationBtn.Click += new System.EventHandler(this.SelectLocationBtn_Click);
            // 
            // FileLocationtxt
            // 
            this.FileLocationtxt.Location = new System.Drawing.Point(619, 103);
            this.FileLocationtxt.Name = "FileLocationtxt";
            this.FileLocationtxt.ReadOnly = true;
            this.FileLocationtxt.Size = new System.Drawing.Size(227, 20);
            this.FileLocationtxt.TabIndex = 19;
            // 
            // SearchPhraseLbl
            // 
            this.SearchPhraseLbl.AutoSize = true;
            this.SearchPhraseLbl.Location = new System.Drawing.Point(695, 132);
            this.SearchPhraseLbl.Name = "SearchPhraseLbl";
            this.SearchPhraseLbl.Size = new System.Drawing.Size(77, 13);
            this.SearchPhraseLbl.TabIndex = 20;
            this.SearchPhraseLbl.Text = "Search Phrase";
            // 
            // SearchPhrasetxt
            // 
            this.SearchPhrasetxt.Location = new System.Drawing.Point(619, 150);
            this.SearchPhrasetxt.Name = "SearchPhrasetxt";
            this.SearchPhrasetxt.Size = new System.Drawing.Size(227, 20);
            this.SearchPhrasetxt.TabIndex = 21;
            // 
            // DateSelectorLbl
            // 
            this.DateSelectorLbl.AutoSize = true;
            this.DateSelectorLbl.Location = new System.Drawing.Point(695, 183);
            this.DateSelectorLbl.Name = "DateSelectorLbl";
            this.DateSelectorLbl.Size = new System.Drawing.Size(72, 13);
            this.DateSelectorLbl.TabIndex = 22;
            this.DateSelectorLbl.Text = "Date Selector";
            // 
            // UseDatesCheck
            // 
            this.UseDatesCheck.AutoSize = true;
            this.UseDatesCheck.Location = new System.Drawing.Point(686, 379);
            this.UseDatesCheck.Name = "UseDatesCheck";
            this.UseDatesCheck.Size = new System.Drawing.Size(106, 17);
            this.UseDatesCheck.TabIndex = 23;
            this.UseDatesCheck.Text = "Use Date Range";
            this.UseDatesCheck.UseVisualStyleBackColor = true;
            // 
            // SearchFilesLbl
            // 
            this.SearchFilesLbl.AutoSize = true;
            this.SearchFilesLbl.Location = new System.Drawing.Point(708, 399);
            this.SearchFilesLbl.Name = "SearchFilesLbl";
            this.SearchFilesLbl.Size = new System.Drawing.Size(65, 13);
            this.SearchFilesLbl.TabIndex = 24;
            this.SearchFilesLbl.Text = "Search Files";
            // 
            // SearchFilesbtn
            // 
            this.SearchFilesbtn.Location = new System.Drawing.Point(619, 415);
            this.SearchFilesbtn.Name = "SearchFilesbtn";
            this.SearchFilesbtn.Size = new System.Drawing.Size(227, 23);
            this.SearchFilesbtn.TabIndex = 25;
            this.SearchFilesbtn.Text = "Search Files";
            this.SearchFilesbtn.UseVisualStyleBackColor = true;
            this.SearchFilesbtn.Click += new System.EventHandler(this.SearchFilesbtn_Click);
            // 
            // ResultsLV
            // 
            this.ResultsLV.HideSelection = false;
            this.ResultsLV.Location = new System.Drawing.Point(871, 26);
            this.ResultsLV.Name = "ResultsLV";
            this.ResultsLV.Size = new System.Drawing.Size(318, 396);
            this.ResultsLV.TabIndex = 26;
            this.ResultsLV.UseCompatibleStateImageBehavior = false;
            // 
            // DateRangeSelectorTbl
            // 
            this.DateRangeSelectorTbl.Location = new System.Drawing.Point(619, 205);
            this.DateRangeSelectorTbl.Name = "DateRangeSelectorTbl";
            this.DateRangeSelectorTbl.TabIndex = 27;
            // 
            // FileSearchLbl
            // 
            this.FileSearchLbl.AutoSize = true;
            this.FileSearchLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileSearchLbl.Location = new System.Drawing.Point(634, 9);
            this.FileSearchLbl.Name = "FileSearchLbl";
            this.FileSearchLbl.Size = new System.Drawing.Size(201, 31);
            this.FileSearchLbl.TabIndex = 28;
            this.FileSearchLbl.Text = "File Searching";
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1207, 450);
            this.Controls.Add(this.FileSearchLbl);
            this.Controls.Add(this.DateRangeSelectorTbl);
            this.Controls.Add(this.ResultsLV);
            this.Controls.Add(this.SearchFilesbtn);
            this.Controls.Add(this.SearchFilesLbl);
            this.Controls.Add(this.UseDatesCheck);
            this.Controls.Add(this.DateSelectorLbl);
            this.Controls.Add(this.SearchPhrasetxt);
            this.Controls.Add(this.SearchPhraseLbl);
            this.Controls.Add(this.FileLocationtxt);
            this.Controls.Add(this.SelectLocationBtn);
            this.Controls.Add(this.KTNFileTypelist);
            this.Controls.Add(this.Select_Filebtn);
            this.Controls.Add(this.File_Pathtxt);
            this.Controls.Add(this.READ_GBDbtn);
            this.Controls.Add(this.Read_KTNbtn);
            this.Controls.Add(this.PO_Number_GBDtxt);
            this.Controls.Add(this.Order_Number_GBDtxt);
            this.Controls.Add(this.PO_Number_KTNtxt);
            this.Controls.Add(this.Order_Number_KTNtxt);
            this.Controls.Add(this.GBDl);
            this.Controls.Add(this.Product_List_GBDbtn);
            this.Controls.Add(this.Product_List_KTNbtn);
            this.Controls.Add(this.Create_PO_GBDbtn);
            this.Controls.Add(this.Create_Order_GBDbtn);
            this.Controls.Add(this.KTNl);
            this.Controls.Add(this.Create_PO_KTNbtn);
            this.Controls.Add(this.Create_Orde_KTNbtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EDI Application Manual Interface";
            this.Load += new System.EventHandler(this.GUI_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Create_Orde_KTNbtn;
        private System.Windows.Forms.Button Create_PO_KTNbtn;
        private System.Windows.Forms.Label KTNl;
        private System.Windows.Forms.Button Create_Order_GBDbtn;
        private System.Windows.Forms.Button Create_PO_GBDbtn;
        private System.Windows.Forms.Button Product_List_KTNbtn;
        private System.Windows.Forms.Button Product_List_GBDbtn;
        private System.Windows.Forms.Label GBDl;
        private System.Windows.Forms.TextBox Order_Number_KTNtxt;
        private System.Windows.Forms.TextBox PO_Number_KTNtxt;
        private System.Windows.Forms.TextBox Order_Number_GBDtxt;
        private System.Windows.Forms.TextBox PO_Number_GBDtxt;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button Read_KTNbtn;
        private System.Windows.Forms.Button READ_GBDbtn;
        private System.Windows.Forms.TextBox File_Pathtxt;
        private System.Windows.Forms.Button Select_Filebtn;
        private System.Windows.Forms.ListView KTNFileTypelist;
        private System.Windows.Forms.Button SelectLocationBtn;
        private System.Windows.Forms.TextBox FileLocationtxt;
        private System.Windows.Forms.Label SearchPhraseLbl;
        private System.Windows.Forms.TextBox SearchPhrasetxt;
        private System.Windows.Forms.Label DateSelectorLbl;
        private System.Windows.Forms.CheckBox UseDatesCheck;
        private System.Windows.Forms.Label SearchFilesLbl;
        private System.Windows.Forms.Button SearchFilesbtn;
        private System.Windows.Forms.ListView ResultsLV;
        private System.Windows.Forms.MonthCalendar DateRangeSelectorTbl;
        private System.Windows.Forms.Label FileSearchLbl;
    }
}

namespace DoorsApp
{
    partial class Settings
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbDirSave = new System.Windows.Forms.TextBox();
            this.tbDirDxf = new System.Windows.Forms.TextBox();
            this.tbDirMaket = new System.Windows.Forms.TextBox();
            this.tbDirConstr = new System.Windows.Forms.TextBox();
            this.tbDirLib = new System.Windows.Forms.TextBox();
            this.btnDirSaveBrowse = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.btnDirDxfBrowse = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDirMaketBrowse = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnDirConstrBrowse = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDirLibBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.fbdBrowse = new System.Windows.Forms.FolderBrowserDialog();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbMaketName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbMaketChanger = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbDirKVD = new System.Windows.Forms.TextBox();
            this.btnDirKVDBrowse = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDirKVDBrowse);
            this.groupBox1.Controls.Add(this.tbDirKVD);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.tbDirSave);
            this.groupBox1.Controls.Add(this.tbDirDxf);
            this.groupBox1.Controls.Add(this.tbDirMaket);
            this.groupBox1.Controls.Add(this.tbDirConstr);
            this.groupBox1.Controls.Add(this.tbDirLib);
            this.groupBox1.Controls.Add(this.btnDirSaveBrowse);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.btnDirDxfBrowse);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnDirMaketBrowse);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.btnDirConstrBrowse);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnDirLibBrowse);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(500, 424);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки рабочих директорий: ";
            // 
            // tbDirSave
            // 
            this.tbDirSave.Location = new System.Drawing.Point(10, 316);
            this.tbDirSave.MaximumSize = new System.Drawing.Size(402, 60);
            this.tbDirSave.MinimumSize = new System.Drawing.Size(402, 45);
            this.tbDirSave.Name = "tbDirSave";
            this.tbDirSave.Size = new System.Drawing.Size(402, 20);
            this.tbDirSave.TabIndex = 19;
            this.tbDirSave.Text = "DIR_SAVE";
            this.tbDirSave.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // tbDirDxf
            // 
            this.tbDirDxf.Location = new System.Drawing.Point(10, 246);
            this.tbDirDxf.MaximumSize = new System.Drawing.Size(402, 60);
            this.tbDirDxf.MinimumSize = new System.Drawing.Size(402, 45);
            this.tbDirDxf.Name = "tbDirDxf";
            this.tbDirDxf.Size = new System.Drawing.Size(402, 20);
            this.tbDirDxf.TabIndex = 18;
            this.tbDirDxf.Text = "DIR_DXF";
            this.tbDirDxf.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // tbDirMaket
            // 
            this.tbDirMaket.Location = new System.Drawing.Point(10, 176);
            this.tbDirMaket.MaximumSize = new System.Drawing.Size(402, 60);
            this.tbDirMaket.MinimumSize = new System.Drawing.Size(402, 45);
            this.tbDirMaket.Name = "tbDirMaket";
            this.tbDirMaket.Size = new System.Drawing.Size(402, 20);
            this.tbDirMaket.TabIndex = 17;
            this.tbDirMaket.Text = "DIR_MAKET";
            this.tbDirMaket.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // tbDirConstr
            // 
            this.tbDirConstr.Location = new System.Drawing.Point(10, 106);
            this.tbDirConstr.MaximumSize = new System.Drawing.Size(402, 60);
            this.tbDirConstr.MinimumSize = new System.Drawing.Size(402, 45);
            this.tbDirConstr.Name = "tbDirConstr";
            this.tbDirConstr.Size = new System.Drawing.Size(402, 20);
            this.tbDirConstr.TabIndex = 16;
            this.tbDirConstr.Text = "DIR_CONSTR";
            this.tbDirConstr.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // tbDirLib
            // 
            this.tbDirLib.Location = new System.Drawing.Point(10, 37);
            this.tbDirLib.MaximumSize = new System.Drawing.Size(402, 60);
            this.tbDirLib.MinimumSize = new System.Drawing.Size(402, 45);
            this.tbDirLib.Name = "tbDirLib";
            this.tbDirLib.Size = new System.Drawing.Size(402, 20);
            this.tbDirLib.TabIndex = 0;
            this.tbDirLib.Text = "DIR_LIB";
            this.tbDirLib.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // btnDirSaveBrowse
            // 
            this.btnDirSaveBrowse.Location = new System.Drawing.Point(418, 314);
            this.btnDirSaveBrowse.Name = "btnDirSaveBrowse";
            this.btnDirSaveBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirSaveBrowse.TabIndex = 14;
            this.btnDirSaveBrowse.Text = "Обзор...";
            this.btnDirSaveBrowse.UseVisualStyleBackColor = true;
            this.btnDirSaveBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 300);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(259, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "Директория для сохранения рабочих INI-файлов: ";
            // 
            // btnDirDxfBrowse
            // 
            this.btnDirDxfBrowse.Location = new System.Drawing.Point(418, 244);
            this.btnDirDxfBrowse.Name = "btnDirDxfBrowse";
            this.btnDirDxfBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirDxfBrowse.TabIndex = 11;
            this.btnDirDxfBrowse.Text = "Обзор...";
            this.btnDirDxfBrowse.UseVisualStyleBackColor = true;
            this.btnDirDxfBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 230);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(223, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Директория для сохранения DXF-файлов: ";
            // 
            // btnDirMaketBrowse
            // 
            this.btnDirMaketBrowse.Location = new System.Drawing.Point(419, 174);
            this.btnDirMaketBrowse.Name = "btnDirMaketBrowse";
            this.btnDirMaketBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirMaketBrowse.TabIndex = 8;
            this.btnDirMaketBrowse.Text = "Обзор...";
            this.btnDirMaketBrowse.UseVisualStyleBackColor = true;
            this.btnDirMaketBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 160);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(167, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Директория с файлом макета: ";
            // 
            // btnDirConstrBrowse
            // 
            this.btnDirConstrBrowse.Location = new System.Drawing.Point(418, 104);
            this.btnDirConstrBrowse.Name = "btnDirConstrBrowse";
            this.btnDirConstrBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirConstrBrowse.TabIndex = 5;
            this.btnDirConstrBrowse.Text = "Обзор...";
            this.btnDirConstrBrowse.UseVisualStyleBackColor = true;
            this.btnDirConstrBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(233, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Директория с таблицами конструирования: ";
            // 
            // btnDirLibBrowse
            // 
            this.btnDirLibBrowse.Location = new System.Drawing.Point(418, 35);
            this.btnDirLibBrowse.Name = "btnDirLibBrowse";
            this.btnDirLibBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirLibBrowse.TabIndex = 2;
            this.btnDirLibBrowse.Text = "Обзор...";
            this.btnDirLibBrowse.UseVisualStyleBackColor = true;
            this.btnDirLibBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(235, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Корневая директория библиотеки констант: ";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(20, 523);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(430, 523);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbMaketName);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cbMaketChanger);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 442);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(499, 75);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Настройки редактора макета: ";
            // 
            // tbMaketName
            // 
            this.tbMaketName.Location = new System.Drawing.Point(211, 46);
            this.tbMaketName.Name = "tbMaketName";
            this.tbMaketName.Size = new System.Drawing.Size(282, 20);
            this.tbMaketName.TabIndex = 3;
            this.tbMaketName.TextChanged += new System.EventHandler(this.tbMaketName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Полное имя файла макета: ";
            // 
            // cbMaketChanger
            // 
            this.cbMaketChanger.FormattingEnabled = true;
            this.cbMaketChanger.Location = new System.Drawing.Point(211, 19);
            this.cbMaketChanger.Name = "cbMaketChanger";
            this.cbMaketChanger.Size = new System.Drawing.Size(282, 21);
            this.cbMaketChanger.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Редактор макета: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 370);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(244, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Директория базы файлов квартирных дверей:";
            // 
            // tbDirKVD
            // 
            this.tbDirKVD.Location = new System.Drawing.Point(13, 387);
            this.tbDirKVD.Name = "tbDirKVD";
            this.tbDirKVD.Size = new System.Drawing.Size(399, 20);
            this.tbDirKVD.TabIndex = 21;
            this.tbDirKVD.Text = "DIR_KVD";
            // 
            // btnDirKVDBrowse
            // 
            this.btnDirKVDBrowse.Location = new System.Drawing.Point(419, 385);
            this.btnDirKVDBrowse.Name = "btnDirKVDBrowse";
            this.btnDirKVDBrowse.Size = new System.Drawing.Size(74, 23);
            this.btnDirKVDBrowse.TabIndex = 22;
            this.btnDirKVDBrowse.Text = "Обзор...";
            this.btnDirKVDBrowse.UseVisualStyleBackColor = true;
            this.btnDirKVDBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 551);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.Name = "Settings";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDirLibBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog fbdBrowse;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDirConstrBrowse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDirDxfBrowse;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnDirMaketBrowse;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDirSaveBrowse;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cbMaketChanger;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMaketName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbDirSave;
        private System.Windows.Forms.TextBox tbDirDxf;
        private System.Windows.Forms.TextBox tbDirMaket;
        private System.Windows.Forms.TextBox tbDirConstr;
        private System.Windows.Forms.TextBox tbDirLib;
        private System.Windows.Forms.Button btnDirKVDBrowse;
        private System.Windows.Forms.TextBox tbDirKVD;
        private System.Windows.Forms.Label label7;
    }
}
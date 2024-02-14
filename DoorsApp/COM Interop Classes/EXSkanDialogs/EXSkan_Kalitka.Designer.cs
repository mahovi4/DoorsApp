
partial class EXSkan_Kalitka
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
            this.lCaption = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbW = new System.Windows.Forms.TextBox();
            this.tbH = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chbChange = new System.Windows.Forms.CheckBox();
            this.frRaspKalitki = new System.Windows.Forms.GroupBox();
            this.tbOtZamka = new System.Windows.Forms.TextBox();
            this.tbOtPola = new System.Windows.Forms.TextBox();
            this.lOtZamka = new System.Windows.Forms.Label();
            this.lOtPola = new System.Windows.Forms.Label();
            this.btApp = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.frRaspKalitki.SuspendLayout();
            this.SuspendLayout();
            // 
            // lCaption
            // 
            this.lCaption.Location = new System.Drawing.Point(12, 9);
            this.lCaption.Name = "lCaption";
            this.lCaption.Size = new System.Drawing.Size(200, 29);
            this.lCaption.TabIndex = 0;
            this.lCaption.Text = "label1";
            this.lCaption.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbW);
            this.groupBox1.Controls.Add(this.tbH);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 79);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Габариты калитки: ";
            // 
            // tbW
            // 
            this.tbW.Location = new System.Drawing.Point(105, 47);
            this.tbW.Name = "tbW";
            this.tbW.Size = new System.Drawing.Size(89, 20);
            this.tbW.TabIndex = 3;
            this.tbW.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.tbW.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // tbH
            // 
            this.tbH.Location = new System.Drawing.Point(105, 20);
            this.tbH.Name = "tbH";
            this.tbH.Size = new System.Drawing.Size(89, 20);
            this.tbH.TabIndex = 2;
            this.tbH.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.tbH.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Ширина калитки:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Высота калитки:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(196, 39);
            this.label4.TabIndex = 2;
            this.label4.Text = "Стандартное расположение калитки:\nот пола = 100 мм;\nот замкового края = 100 мм.";
            // 
            // chbChange
            // 
            this.chbChange.AutoSize = true;
            this.chbChange.Location = new System.Drawing.Point(12, 174);
            this.chbChange.Name = "chbChange";
            this.chbChange.Size = new System.Drawing.Size(198, 17);
            this.chbChange.TabIndex = 3;
            this.chbChange.Text = "Изменить расположение калитки";
            this.chbChange.UseVisualStyleBackColor = true;
            this.chbChange.CheckedChanged += new System.EventHandler(this.chbChange_CheckedChanged);
            // 
            // frRaspKalitki
            // 
            this.frRaspKalitki.Controls.Add(this.tbOtZamka);
            this.frRaspKalitki.Controls.Add(this.tbOtPola);
            this.frRaspKalitki.Controls.Add(this.lOtZamka);
            this.frRaspKalitki.Controls.Add(this.lOtPola);
            this.frRaspKalitki.Location = new System.Drawing.Point(12, 197);
            this.frRaspKalitki.Name = "frRaspKalitki";
            this.frRaspKalitki.Size = new System.Drawing.Size(200, 79);
            this.frRaspKalitki.TabIndex = 4;
            this.frRaspKalitki.TabStop = false;
            this.frRaspKalitki.Text = "Расположение калитки: ";
            // 
            // tbOtZamka
            // 
            this.tbOtZamka.Location = new System.Drawing.Point(105, 47);
            this.tbOtZamka.Name = "tbOtZamka";
            this.tbOtZamka.Size = new System.Drawing.Size(86, 20);
            this.tbOtZamka.TabIndex = 3;
            this.tbOtZamka.Text = "100";
            this.tbOtZamka.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.tbOtZamka.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // tbOtPola
            // 
            this.tbOtPola.Location = new System.Drawing.Point(105, 20);
            this.tbOtPola.Name = "tbOtPola";
            this.tbOtPola.Size = new System.Drawing.Size(87, 20);
            this.tbOtPola.TabIndex = 2;
            this.tbOtPola.Text = "100";
            this.tbOtPola.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.tbOtPola.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // lOtZamka
            // 
            this.lOtZamka.AutoSize = true;
            this.lOtZamka.Location = new System.Drawing.Point(10, 54);
            this.lOtZamka.Name = "lOtZamka";
            this.lOtZamka.Size = new System.Drawing.Size(81, 13);
            this.lOtZamka.TabIndex = 1;
            this.lOtZamka.Text = "От замкового:";
            // 
            // lOtPola
            // 
            this.lOtPola.AutoSize = true;
            this.lOtPola.Location = new System.Drawing.Point(10, 26);
            this.lOtPola.Name = "lOtPola";
            this.lOtPola.Size = new System.Drawing.Size(50, 13);
            this.lOtPola.TabIndex = 0;
            this.lOtPola.Text = "От пола:";
            // 
            // btApp
            // 
            this.btApp.Location = new System.Drawing.Point(15, 282);
            this.btApp.Name = "btApp";
            this.btApp.Size = new System.Drawing.Size(75, 23);
            this.btApp.TabIndex = 5;
            this.btApp.Text = "Применить";
            this.btApp.UseVisualStyleBackColor = true;
            this.btApp.Click += new System.EventHandler(this.btApp_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(131, 282);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 6;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // EXSkan_Kalitka
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 316);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btApp);
            this.Controls.Add(this.frRaspKalitki);
            this.Controls.Add(this.chbChange);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lCaption);
            this.Name = "EXSkan_Kalitka";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.frRaspKalitki.ResumeLayout(false);
            this.frRaspKalitki.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lCaption;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TextBox tbW;
    private System.Windows.Forms.TextBox tbH;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox chbChange;
    private System.Windows.Forms.GroupBox frRaspKalitki;
    private System.Windows.Forms.TextBox tbOtZamka;
    private System.Windows.Forms.TextBox tbOtPola;
    private System.Windows.Forms.Label lOtZamka;
    private System.Windows.Forms.Label lOtPola;
    private System.Windows.Forms.Button btApp;
    private System.Windows.Forms.Button btCancel;
}
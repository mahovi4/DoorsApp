
partial class Main
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
        this.label1 = new System.Windows.Forms.Label();
        this.btnKonstructor = new System.Windows.Forms.Button();
        this.btnFiles = new System.Windows.Forms.Button();
        this.btnScanTable = new System.Windows.Forms.Button();
        this.btnTable = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 9);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(240, 13);
        this.label1.TabIndex = 0;
        this.label1.Text = "Выберите раздел, в котором будете работать";
        // 
        // btnKonstructor
        // 
        this.btnKonstructor.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
        this.btnKonstructor.Location = new System.Drawing.Point(29, 38);
        this.btnKonstructor.Name = "btnKonstructor";
        this.btnKonstructor.Size = new System.Drawing.Size(194, 28);
        this.btnKonstructor.TabIndex = 1;
        this.btnKonstructor.Text = "Конструктор";
        this.btnKonstructor.UseVisualStyleBackColor = true;
        this.btnKonstructor.Click += new System.EventHandler(this.btnKonstructor_Click);
        // 
        // btnFiles
        // 
        this.btnFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
        this.btnFiles.Location = new System.Drawing.Point(29, 72);
        this.btnFiles.Name = "btnFiles";
        this.btnFiles.Size = new System.Drawing.Size(194, 28);
        this.btnFiles.TabIndex = 2;
        this.btnFiles.Text = "Работа по файлам";
        this.btnFiles.UseVisualStyleBackColor = true;
        this.btnFiles.Click += new System.EventHandler(this.btnFiles_Click);
        // 
        // btnScanTable
        // 
        this.btnScanTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
        this.btnScanTable.Location = new System.Drawing.Point(29, 106);
        this.btnScanTable.Name = "btnScanTable";
        this.btnScanTable.Size = new System.Drawing.Size(194, 28);
        this.btnScanTable.TabIndex = 3;
        this.btnScanTable.Text = "Сканировать таблицу";
        this.btnScanTable.UseVisualStyleBackColor = true;
        this.btnScanTable.Click += new System.EventHandler(this.btnScanTable_Click);
        // 
        // btnTable
        // 
        this.btnTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
        this.btnTable.Location = new System.Drawing.Point(29, 140);
        this.btnTable.Name = "btnTable";
        this.btnTable.Size = new System.Drawing.Size(194, 28);
        this.btnTable.TabIndex = 4;
        this.btnTable.Text = "Загрузить таблицу";
        this.btnTable.UseVisualStyleBackColor = true;
        this.btnTable.Click += new System.EventHandler(this.btnTable_Click);
        // 
        // Main
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(256, 196);
        this.Controls.Add(this.btnTable);
        this.Controls.Add(this.btnScanTable);
        this.Controls.Add(this.btnFiles);
        this.Controls.Add(this.btnKonstructor);
        this.Controls.Add(this.label1);
        this.Name = "Main";
        this.Text = "Main";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnKonstructor;
    private System.Windows.Forms.Button btnFiles;
    private System.Windows.Forms.Button btnScanTable;
    private System.Windows.Forms.Button btnTable;
}
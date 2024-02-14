
partial class WorkFiles
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
            this.lFindFiles = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.Sel = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateCreate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnBrowsSel = new System.Windows.Forms.Button();
            this.btnBrowsAll = new System.Windows.Forms.Button();
            this.btnOkSel = new System.Windows.Forms.Button();
            this.btnOkAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Обнаружено:";
            // 
            // lFindFiles
            // 
            this.lFindFiles.AutoSize = true;
            this.lFindFiles.Location = new System.Drawing.Point(92, 13);
            this.lFindFiles.Name = "lFindFiles";
            this.lFindFiles.Size = new System.Drawing.Size(19, 13);
            this.lFindFiles.TabIndex = 1;
            this.lFindFiles.Text = "00";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(117, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "рабочих файлов";
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToOrderColumns = true;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sel,
            this.FileName,
            this.DateCreate,
            this.Count});
            this.dgvFiles.Location = new System.Drawing.Point(16, 38);
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.Size = new System.Drawing.Size(509, 224);
            this.dgvFiles.TabIndex = 3;
            // 
            // Sel
            // 
            this.Sel.HeaderText = "Выполнить";
            this.Sel.Name = "Sel";
            this.Sel.Width = 70;
            // 
            // FileName
            // 
            this.FileName.HeaderText = "Имя файла";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            // 
            // DateCreate
            // 
            this.DateCreate.HeaderText = "Дата создания";
            this.DateCreate.Name = "DateCreate";
            this.DateCreate.ReadOnly = true;
            this.DateCreate.Width = 200;
            // 
            // Count
            // 
            this.Count.HeaderText = "Количество изделий";
            this.Count.Name = "Count";
            this.Count.ReadOnly = true;
            // 
            // btnBrowsSel
            // 
            this.btnBrowsSel.Location = new System.Drawing.Point(544, 38);
            this.btnBrowsSel.Name = "btnBrowsSel";
            this.btnBrowsSel.Size = new System.Drawing.Size(110, 40);
            this.btnBrowsSel.TabIndex = 4;
            this.btnBrowsSel.Text = " Обзор выбранных файлов";
            this.btnBrowsSel.UseVisualStyleBackColor = true;
            this.btnBrowsSel.Click += new System.EventHandler(this.btnBrowsSel_Click);
            // 
            // btnBrowsAll
            // 
            this.btnBrowsAll.Location = new System.Drawing.Point(545, 84);
            this.btnBrowsAll.Name = "btnBrowsAll";
            this.btnBrowsAll.Size = new System.Drawing.Size(110, 40);
            this.btnBrowsAll.TabIndex = 5;
            this.btnBrowsAll.Text = "Обзор всех файлов";
            this.btnBrowsAll.UseVisualStyleBackColor = true;
            this.btnBrowsAll.Click += new System.EventHandler(this.btnBrowsAll_Click);
            // 
            // btnOkSel
            // 
            this.btnOkSel.Location = new System.Drawing.Point(545, 130);
            this.btnOkSel.Name = "btnOkSel";
            this.btnOkSel.Size = new System.Drawing.Size(110, 40);
            this.btnOkSel.TabIndex = 6;
            this.btnOkSel.Text = "Обработать выбранные";
            this.btnOkSel.UseVisualStyleBackColor = true;
            this.btnOkSel.Click += new System.EventHandler(this.btnOkSel_Click);
            // 
            // btnOkAll
            // 
            this.btnOkAll.Location = new System.Drawing.Point(545, 176);
            this.btnOkAll.Name = "btnOkAll";
            this.btnOkAll.Size = new System.Drawing.Size(110, 40);
            this.btnOkAll.TabIndex = 7;
            this.btnOkAll.Text = "Обработать все";
            this.btnOkAll.UseVisualStyleBackColor = true;
            this.btnOkAll.Click += new System.EventHandler(this.btnOkAll_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(545, 222);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 40);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // WorkFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 277);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkAll);
            this.Controls.Add(this.btnOkSel);
            this.Controls.Add(this.btnBrowsAll);
            this.Controls.Add(this.btnBrowsSel);
            this.Controls.Add(this.dgvFiles);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lFindFiles);
            this.Controls.Add(this.label1);
            this.Name = "WorkFiles";
            this.Text = "Работа с файлами";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WorkFiles_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lFindFiles;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.DataGridView dgvFiles;
    private System.Windows.Forms.Button btnBrowsSel;
    private System.Windows.Forms.Button btnBrowsAll;
    private System.Windows.Forms.Button btnOkSel;
    private System.Windows.Forms.Button btnOkAll;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.DataGridViewCheckBoxColumn Sel;
    private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
    private System.Windows.Forms.DataGridViewTextBoxColumn DateCreate;
    private System.Windows.Forms.DataGridViewTextBoxColumn Count;
}
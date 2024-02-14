
namespace DoorsApp
{
    partial class FilterList
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnApp = new System.Windows.Forms.Button();
            this.btnDrop = new System.Windows.Forms.Button();
            this.dgvFilterList = new System.Windows.Forms.DataGridView();
            this.cb = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.text = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterList)).BeginInit();
            this.SuspendLayout();
            // 
            // btnApp
            // 
            this.btnApp.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnApp.Location = new System.Drawing.Point(0, 269);
            this.btnApp.Name = "btnApp";
            this.btnApp.Size = new System.Drawing.Size(211, 23);
            this.btnApp.TabIndex = 1;
            this.btnApp.Text = "Применить";
            this.btnApp.UseVisualStyleBackColor = true;
            this.btnApp.Click += new System.EventHandler(this.btnApp_Click);
            // 
            // btnDrop
            // 
            this.btnDrop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnDrop.Location = new System.Drawing.Point(0, 292);
            this.btnDrop.Name = "btnDrop";
            this.btnDrop.Size = new System.Drawing.Size(211, 23);
            this.btnDrop.TabIndex = 2;
            this.btnDrop.Text = "Сбросить";
            this.btnDrop.UseVisualStyleBackColor = true;
            this.btnDrop.Click += new System.EventHandler(this.btnDrop_Click);
            // 
            // dgvFilterList
            // 
            this.dgvFilterList.AllowUserToAddRows = false;
            this.dgvFilterList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFilterList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvFilterList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvFilterList.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dgvFilterList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvFilterList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFilterList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvFilterList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvFilterList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterList.ColumnHeadersVisible = false;
            this.dgvFilterList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cb,
            this.text});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFilterList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvFilterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFilterList.Location = new System.Drawing.Point(0, 0);
            this.dgvFilterList.MultiSelect = false;
            this.dgvFilterList.Name = "dgvFilterList";
            this.dgvFilterList.ReadOnly = true;
            this.dgvFilterList.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvFilterList.RowHeadersVisible = false;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFilterList.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvFilterList.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvFilterList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFilterList.Size = new System.Drawing.Size(211, 269);
            this.dgvFilterList.TabIndex = 3;
            this.dgvFilterList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterList_CellClick);
            // 
            // cb
            // 
            this.cb.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.cb.Frozen = true;
            this.cb.HeaderText = "CheckBox";
            this.cb.Name = "cb";
            this.cb.ReadOnly = true;
            this.cb.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.cb.Width = 30;
            // 
            // text
            // 
            this.text.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.text.Frozen = true;
            this.text.HeaderText = "TextBox";
            this.text.Name = "text";
            this.text.ReadOnly = true;
            this.text.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.text.Width = 5;
            // 
            // FilterList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.dgvFilterList);
            this.Controls.Add(this.btnApp);
            this.Controls.Add(this.btnDrop);
            this.Name = "FilterList";
            this.Size = new System.Drawing.Size(211, 315);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnApp;
        private System.Windows.Forms.Button btnDrop;
        private System.Windows.Forms.DataGridView dgvFilterList;
        private System.Windows.Forms.DataGridViewCheckBoxColumn cb;
        private System.Windows.Forms.DataGridViewTextBoxColumn text;
    }
}

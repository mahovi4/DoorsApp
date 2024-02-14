using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DoorsApp
{
    public delegate void FilterAppHandler(int owner, Dictionary<string, bool> DataSource);
    public delegate void FilterDropHandler();

    public partial class FilterList : UserControl
    {
        public int Owner { get; private set; }
        public Dictionary<string, bool> DataSource { get; private set; }

        public event FilterAppHandler FilterApp;
        public event FilterDropHandler FilterDrop;

        public FilterList(int own, Dictionary<string, bool> data)
        {
            InitializeComponent();
            Owner = own;
            DataSource = data;
            dgvFilterList.Columns[0].Width = 30;
            dgvFilterList.Columns[1].Width = dgvFilterList.Width - dgvFilterList.Columns[0].Width;
            Fill();
        }

        private void Fill()
        {
            dgvFilterList.Rows.Clear();
            dgvFilterList.Rows.Add(true, "Все");
            foreach(var item in DataSource)
                dgvFilterList.Rows.Add(item.Value, item.Key);
            dgvFilterList.Rows[0].Cells[0].Value = IsAllChacked();
        }
        private bool IsAllChacked()
        {
            int tmp = 0;
            for(int i=1; i<dgvFilterList.Rows.Count; i++) 
                if ((bool)dgvFilterList.Rows[i].Cells[0].Value) tmp++;
            if (tmp == dgvFilterList.Rows.Count - 1) return true;
            else return false;
        }

        protected virtual void OnFilterApp()
        {
            FilterApp?.Invoke(Owner, DataSource);
        }
        protected virtual void OnFilterDrop()
        {
            FilterDrop?.Invoke();
        }

        private void btnApp_Click(object sender, EventArgs e)
        {
            OnFilterApp();
        }
        private void btnDrop_Click(object sender, EventArgs e)
        {
            OnFilterDrop();
        }

        private void dgvFilterList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 0)
            {
                dgvFilterList.Rows[e.RowIndex].Cells[0].Value = !(bool)dgvFilterList.Rows[e.RowIndex].Cells[0].Value;
                if (e.RowIndex == 0)
                {
                    Dictionary<string, bool> tmp = new Dictionary<string, bool>();
                    for (int i = 1; i < dgvFilterList.Rows.Count; i++)
                    {
                        dgvFilterList.Rows[i].Cells[0].Value = dgvFilterList.Rows[0].Cells[0].Value;
                        tmp.Add(dgvFilterList.Rows[i].Cells[1].Value.ToString(), (bool)dgvFilterList.Rows[i].Cells[0].Value);
                    }
                    foreach (var item in tmp)
                        DataSource[item.Key] = item.Value;
                }
                else
                    DataSource[dgvFilterList.Rows[e.RowIndex].Cells[1].Value.ToString()] =
                        (bool)dgvFilterList.Rows[e.RowIndex].Cells[0].Value;
            }
        }
    }
}

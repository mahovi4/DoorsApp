using DoorsMaketChangers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoorsApp
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            btnOk.Enabled = false;
            cbMaketChanger.DataSource = Enum.GetNames(typeof(MaketChangerTypes));
            if (int.TryParse(Program.ini.ReadKey("Global", "MAKET_CHANGER"), out int index))
                cbMaketChanger.SelectedIndex = index;
            else
            {
                cbMaketChanger.SelectedIndex = 0;
                Change();
            }
            tbDirLib.Text = Program.ini.ReadKey("Directoryes", "DIR_LIB");
            tbDirConstr.Text = Program.ini.ReadKey("Directoryes", "DIR_CONSTR");
            tbDirMaket.Text = Program.ini.ReadKey("Directoryes", "DIR_MAKET");
            tbDirDxf.Text = Program.ini.ReadKey("Directoryes", "DIR_DXF");
            tbDirSave.Text = Program.ini.ReadKey("Directoryes", "DIR_SAVE");
            tbDirKVD.Text = Program.ini.ReadKey("Directoryes", "DIR_KVD");
            tbMaketName.Text = Program.ini.ReadKey("Global", "G_MAKET");
        }

        private void Change()
        {
            btnOk.Enabled = true;
        }

        private void tbPath_TextChanged(object sender, EventArgs e)
        {
            var newPath = "";
            var oldPath = "";
            var tb = (TextBox)sender;
            switch (tb.Name)
            {
                case "tbDirLib":
                    newPath = tbDirLib.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_LIB");
                    break;
                case "tbDirConstr":
                    newPath = tbDirConstr.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_CONSTR");
                    break;
                case "tbDirMaket":
                    newPath = tbDirMaket.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_MAKET");
                    break;
                case "tbDirDxf":
                    newPath = tbDirDxf.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_DXF");
                    break;
                case "tbDirSave":
                    newPath = tbDirSave.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_SAVE");
                    break;
                case "tbDirKVD":
                    newPath = tbDirKVD.Text;
                    oldPath = Program.ini.ReadKey("Directoryes", "DIR_KVD");
                    break;
            }
            if (Directory.Exists(newPath))
                Change();
            else
            {
                MessageBox.Show("Папка по пути " + newPath + " не существует!");
                tb.Text = oldPath;
            }
        }
        private void tbMaketName_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(tbDirMaket.Text + @"\" + tbMaketName.Text))
                Change();
            else
            {
                MessageBox.Show("Файл с именем " + tbMaketName.Text + " в папке:" + '\n' + tbDirMaket.Text + '\n' + "не найден!");
                tbMaketName.Text = Program.ini.ReadKey("Global", "G_MAKET");
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            fbdBrowse.ShowNewFolderButton = false;

            if (fbdBrowse.ShowDialog() != DialogResult.OK) return;

            var newPath = fbdBrowse.SelectedPath;

            switch (button.Name)
            {
                case "btnDirLibBrowse":
                    tbDirLib.Text = newPath;
                    break;
                case "btnDirConstrBrowse":
                    tbDirConstr.Text = newPath;
                    break;
                case "btnDirMaketBrowse":
                    tbDirMaket.Text = newPath;
                    break;
                case "btnDirDxfBrowse":
                    tbDirDxf.Text = newPath;
                    break;
                case "btnDirSaveBrowse":
                    tbDirSave.Text = newPath;
                    break;
                case "btnDirKVDBrowse":
                    tbDirKVD.Text = newPath;
                    break;
            }

            Change();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Program.ini.WriteKey("Global", "G_MAKET", tbMaketName.Text);
            Program.ini.WriteKey("Global", "MAKET_CHANGER", cbMaketChanger.SelectedIndex.ToString());

            Program.ini.WriteKey("Directoryes", "DIR_LIB", tbDirLib.Text);
            Program.ini.WriteKey("Directoryes", "DIR_CONSTR", tbDirConstr.Text);
            Program.ini.WriteKey("Directoryes", "DIR_MAKET", tbDirMaket.Text);
            Program.ini.WriteKey("Directoryes", "DIR_DXF", tbDirDxf.Text);
            Program.ini.WriteKey("Directoryes", "DIR_SAVE", tbDirSave.Text);
            Program.ini.WriteKey("Directoryes", "DIR_KVD", tbDirKVD.Text);
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

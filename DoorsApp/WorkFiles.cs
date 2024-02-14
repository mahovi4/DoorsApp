using System;
using System.IO;
using System.Windows.Forms;

public partial class WorkFiles : Form
{
    private string[] files;

    public WorkFiles()
    {
        InitializeComponent();
    }

    internal void LoadTable()
    {
        Constructor constructor = new Constructor();
        TableData[] pars;

        dgvFiles.Rows.Clear();
        string path = Program.ini.ReadKey("Directoryes", "DIR_SAVE");
        files = Directory.GetFiles(path, "*.ini");
        lFindFiles.Text = files.Length.ToString();
        for (short i = 0; i < files.Length; i++)
        {
            FileInfo filesInfo = new FileInfo(files[i]);
            pars = constructor.TDatasInFile(files[i]);
            dgvFiles.Rows.Add(true, filesInfo.Name, File.GetCreationTime(files[i]).ToString(), pars.Length);
        }
    }

    private void WorkFiles_FormClosing(object sender, FormClosingEventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
        e.Cancel = true;
    }
    private void btnOkSel_Click(object sender, EventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
    }
    private void btnOkAll_Click(object sender, EventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
    }
    private void btnCancel_Click(object sender, EventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
    }
    private void btnBrowsSel_Click(object sender, EventArgs e)
    {
        if (files.Length > 0) {
            Hide();
            Program.inWorkForm.Show();
            string[] f = new string[] { };
            for (short i = 0; i < files.Length; i++) {
                if ((bool)dgvFiles.Rows[i].Cells[0].Value) {
                    Array.Resize(ref f, i + 1);
                    f[i] = files[i];
                }
            }
            if (f.Length > 0) {
                Program.inWorkForm.Brows(f);
            }
            else {
                return;
            }
        }
        else {
            return;
        }
    }
    private void btnBrowsAll_Click(object sender, EventArgs e)
    {
        if (files.Length > 0) {
            Hide();
            Program.inWorkForm.Show();
            Program.inWorkForm.Brows(files);
        }
        else {
            return;
        }
    }
}

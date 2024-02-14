using System;
using System.Windows.Forms;

public partial class Main : Form
{
    public Main()
    {
        InitializeComponent();
    }

    private void btnKonstructor_Click(object sender, EventArgs e)
    {
        Hide();
        Program.konstruktorForm.Show();
    }
    private void btnFiles_Click(object sender, EventArgs e)
    {
        Hide();
        Program.workFilesForm.LoadTable();
        Program.workFilesForm.Show();
    }
    private void btnScanTable_Click(object sender, EventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
        Program.inWorkForm.ScanTable();
    }
    private void btnTable_Click(object sender, EventArgs e)
    {
        Hide();
        Program.inWorkForm.Show();
        Program.inWorkForm.ScanFile();
    }
}

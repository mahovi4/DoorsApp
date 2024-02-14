using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

public partial class EXSkan_Kalitka : Form
{
    private TableData _param;
    private bool nonNumberEntered = false;

    public EXSkan_Kalitka()
    {
        InitializeComponent();
    }

    internal void Init(ref TableData data)
    {
        _param = data;
        Text = _param.Num;
        lCaption.Text = "В воротах " + _param.Num + " есть калитка. Введите её параметры.";
        Show();
    }
    public TableData Param
    {
        get { return _param; }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        nonNumberEntered = false;
        if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
        {
            if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
            {
                if (e.KeyCode != Keys.Back)
                {
                    nonNumberEntered = true;
                }
            }
        }

        if (ModifierKeys == Keys.Shift)
        {
            nonNumberEntered = true;
        }
    }
    private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (nonNumberEntered)
        {
            e.Handled = true;
        }
    }

    private void btApp_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tbH.Text))
        {
            Interaction.MsgBox("Поле 'Высота калитки' не может быть пустым");
            return;
        }
        if (string.IsNullOrEmpty(tbW.Text)) {
            Interaction.MsgBox("Поле 'Ширина калитки' не может быть пустым");
            return;
        }
        if (chbChange.Checked) {
            if (string.IsNullOrEmpty(tbOtPola.Text)) {
                Interaction.MsgBox("Поле 'Калитка от пола' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbOtZamka.Text)) {
                Interaction.MsgBox("Поле 'Калитка от замкового' не может быть пустым");
                return;
            }
        }

        _param.Kalit = true;
        if (tbH.Text.Equals("1650") | tbH.Text.Equals("1800") | tbH.Text.Equals("1850") | tbH.Text.Equals("1950") | tbH.Text.Equals("2000") | tbH.Text.Equals("2100")) 
            _param.HKalit = short.Parse(tbH.Text);
        else {
            short a = (short)Interaction.MsgBox("Введено нестандартное значение высоты калитки." + '\n' + 
                                                tbH.Text + " mm" + '\n' + 
                                                "Это верный размер?", (MsgBoxStyle)36, _param.Num);
            if (a == 6) _param.HKalit = short.Parse(tbH.Text);
            else return;
        }

        if (tbW.Text.Equals("700") | tbW.Text.Equals("800") | tbW.Text.Equals("850") | tbW.Text.Equals("900")) 
            _param.WKalit = short.Parse(tbW.Text);
        else {
            short b = (short)Interaction.MsgBox("Введено нестандартное значение ширины калитки." + '\n' + 
                                                tbW.Text + " mm" + '\n' + 
                                                "Это верный размер?", (MsgBoxStyle)36, _param.Num);
            if (b == 6) _param.WKalit = short.Parse(tbW.Text);
            else return;
        }
        if (chbChange.Checked) {
            _param.KalitOtPola = short.Parse(tbOtPola.Text);
            _param.KalitOtZamka = short.Parse(tbOtZamka.Text);
        }
        else {
            _param.KalitOtPola = 100;
            _param.KalitOtZamka = 100;
        }
        // Unload
        Hide();
    }
    private void btCancel_Click(object sender, EventArgs e) {
        _param.Kalit = false;
        Hide();
    }
    private void chbChange_CheckedChanged(object sender, EventArgs e) {
        frRaspKalitki.Enabled = chbChange.Checked;
        lOtPola.Enabled = chbChange.Checked;
        lOtZamka.Enabled = chbChange.Checked;
        tbOtPola.Enabled = chbChange.Checked;
        tbOtZamka.Enabled = chbChange.Checked;
    }

}

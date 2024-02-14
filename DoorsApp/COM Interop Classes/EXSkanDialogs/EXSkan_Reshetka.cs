using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

public partial class EXSkan_Reshetka : Form
{
    private TableData _param;
    private bool _ok;
    private bool nonNumberEntered = false;

    public EXSkan_Reshetka()
    {
        InitializeComponent();
    }

    public void Init(ref TableData Data)
    {
        _param = Data;
        if (_param.WAktiv > 0d) {
            gbPS.Enabled = true;
            lCaption.Text = _param.Num + " - ДМ-2. Параметры решеток:";
        }
        else {
            gbPS.Enabled = false;
            lCaption.Text = _param.Num + " - ДМ-1. Параметры решеток:";
        }
        Text = _param.Num;
        Show();
    }

    public TableData Param
    {
        get { return _param; }
    }
    public bool Ok
    {
        get { return _ok; }
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
    
    private void btApp_Click(object sender, EventArgs e)
    {
        _ok = true;
        if (chRes1.Checked) {
            if (string.IsNullOrEmpty(cbResh1.Text)) {
                Interaction.MsgBox("Поле 'Тип решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbOtPola1.Text)) {
                Interaction.MsgBox("Поле 'Расстояние от пола' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbH1.Text)) {
                Interaction.MsgBox("Поле 'Высота решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbW1.Text)) {
                Interaction.MsgBox("Поле 'Ширина решетки' не может быть пустым");
                return;
            }
        }

        if (chRes2.Checked) {
            if (string.IsNullOrEmpty(cbResh2.Text)) {
                Interaction.MsgBox("Поле 'Тип решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbOtPola2.Text)) {
                Interaction.MsgBox("Поле 'Расстояние от верха' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbH2.Text)) {
                Interaction.MsgBox("Поле 'Высота решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbW2.Text)) {
                Interaction.MsgBox("Поле 'Ширина решетки' не может быть пустым");
                return;
            }
        }

        if (chRes3.Checked) {
            if (string.IsNullOrEmpty(cbResh3.Text)) {
                Interaction.MsgBox("Поле 'Тип решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbOtPola3.Text)) {
                Interaction.MsgBox("Поле 'Расстояние от пола' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbH3.Text)) {
                Interaction.MsgBox("Поле 'Высота решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbW3.Text)) {
                Interaction.MsgBox("Поле 'Ширина решетки' не может быть пустым");
                return;
            }
        }

        if (chRes4.Checked) {
            if (string.IsNullOrEmpty(cbResh4.Text)) {
                Interaction.MsgBox("Поле 'Тип решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbOtPola4.Text)) {
                Interaction.MsgBox("Поле 'Расстояние от верха' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbH4.Text)) {
                Interaction.MsgBox("Поле 'Высота решетки' не может быть пустым");
                return;
            }
            if (string.IsNullOrEmpty(tbW4.Text)) {
                Interaction.MsgBox("Поле 'Ширина решетки' не может быть пустым");
                return;
            }
        }

        if (!chRes1.Checked & !chRes2.Checked & !chRes3.Checked & !chRes4.Checked) {
            Interaction.MsgBox("Не выбрана ни одна решетка!");
            return;
        }

        if (chRes1.Checked)
        {
            eReshetka type;
            if (cbResh1.Text.Equals("Противопожарная")) {
                type = eReshetka.ПП_решетка;
            }
            else if (cbResh1.Text.Equals("Вентиляционная")) {
                type = eReshetka.Вент_решетка;
            }
            else {
                type = eReshetka.нет;
            }

            _param.SetReshPar(0, new ReshParam()
            {
                Height = short.Parse(tbH1.Text),
                Width = short.Parse(tbW1.Text),
                OtPola = short.Parse(tbOtPola1.Text),
                Type = type,
                FromTable = ""
            });
        }
        else {
            _param.SetReshPar(0, new ReshParam()
            {
                Height = 0,
                Width = 0,
                OtPola = 0,
                Type = eReshetka.нет,
                FromTable = ""
            });
        }

        if (chRes2.Checked) {
            eReshetka type;
            if (cbResh2.Text.Equals("Противопожарная")) {
                type = eReshetka.ПП_решетка;
            }
            else if (cbResh2.Text.Equals("Вентиляционная")) {
                type = eReshetka.Вент_решетка;
            }
            else {
                type = eReshetka.нет;
            }

            _param.SetReshPar(1, new ReshParam()
            {
                Height = short.Parse(tbH2.Text),
                Width = short.Parse(tbW2.Text),
                OtPola = short.Parse(tbOtPola2.Text),
                Type = type,
                FromTable = ""
            });
        }
        else {
            _param.SetReshPar(1, new ReshParam()
            {
                Height = 0,
                Width = 0,
                OtPola = 0,
                Type = eReshetka.нет,
                FromTable = ""
            });
        }

        if (chRes3.Checked) {
            eReshetka type;
            if (cbResh3.Text.Equals("Противопожарная")) {
                type = eReshetka.ПП_решетка;
            }
            else if (cbResh3.Text.Equals("Вентиляционная")) {
                type = eReshetka.Вент_решетка;
            }
            else {
                type = eReshetka.нет;
            }

            _param.SetReshPar(2, new ReshParam()
            {
                Height = short.Parse(tbH3.Text),
                Width = short.Parse(tbW3.Text),
                OtPola = short.Parse(tbOtPola3.Text),
                Type = type,
                FromTable = ""
            });
        }
        else {
            _param.SetReshPar(2, new ReshParam()
            {
                Height = 0,
                Width = 0,
                OtPola = 0,
                Type = eReshetka.нет,
                FromTable = ""
            });
        }

        if (chRes4.Checked) {
            eReshetka type;
            if (cbResh4.Text.Equals("Противопожарная")) {
                type = eReshetka.ПП_решетка;
            }
            else if (cbResh4.Text.Equals("Вентиляционная")) {
                type = eReshetka.Вент_решетка;
            }
            else {
                type = eReshetka.нет;
            }

            _param.SetReshPar(3, new ReshParam()
            {
                Height = short.Parse(tbH4.Text),
                Width = short.Parse(tbW4.Text),
                OtPola = short.Parse(tbOtPola4.Text),
                Type = type,
                FromTable = ""
            });
        }
        else {
            _param.SetReshPar(3, new ReshParam()
            {
                Height = 0,
                Width = 0,
                OtPola = 0,
                Type = eReshetka.нет,
                FromTable = ""
            });
        }
        // Unload
        Hide();
    }
    private void btCancel_Click(object sender, EventArgs e)
    {
        _ok = false;
        // Unload
        Hide();
    }
    private void chRes1_CheckedChanged(object sender, EventArgs e) {
        lType1.Enabled = chRes1.Checked;
        cbResh1.Enabled = chRes1.Checked;
        lOtPola.Enabled = chRes1.Checked;
        lH1.Enabled = chRes1.Checked;
        lW1.Enabled = chRes1.Checked;
        tbOtPola1.Enabled = chRes1.Checked;
        tbH1.Enabled = chRes1.Checked;
        tbW1.Enabled = chRes1.Checked;
    }
    private void chRes2_CheckedChanged(object sender, EventArgs e) {
        lType2.Enabled = chRes2.Checked;
        cbResh2.Enabled = chRes2.Checked;
        lOtPola2.Enabled = chRes2.Checked;
        lH1.Enabled = chRes2.Checked;
        lW1.Enabled = chRes2.Checked;
        tbOtPola2.Enabled = chRes2.Checked;
        tbH2.Enabled = chRes2.Checked;
        tbW2.Enabled = chRes2.Checked;
    }
    private void chRes3_CheckedChanged(object sender, EventArgs e) {
        lType3.Enabled = chRes3.Checked;
        cbResh3.Enabled = chRes3.Checked;
        lOtPola3.Enabled = chRes3.Checked;
        lH3.Enabled = chRes3.Checked;
        lW3.Enabled = chRes3.Checked;
        tbOtPola3.Enabled = chRes3.Checked;
        tbH3.Enabled = chRes3.Checked;
        tbW3.Enabled = chRes3.Checked;
    }
    private void chRes4_CheckedChanged(object sender, EventArgs e) {
        lType4.Enabled = chRes4.Checked;
        cbResh4.Enabled = chRes4.Checked;
        lOtPola4.Enabled = chRes4.Checked;
        lH4.Enabled = chRes4.Checked;
        lW4.Enabled = chRes4.Checked;
        tbOtPola4.Enabled = chRes4.Checked;
        tbH4.Enabled = chRes4.Checked;
        tbW4.Enabled = chRes4.Checked;
    }
}

using COM_DoorsLibrary.Формы;
using DoorsApp;
using DoorsMaketChangers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using COM_DoorsLibrary;

public partial class InWork : Form
{
    private readonly Constants cons = new Constants();
    private List<TableData> tDatas = new List<TableData>();
    private string preChangeValue;
    private string postChangeValue;
    private bool complite;
    private bool cancel;
    private MessForm mf;
    private string editNum = "";
    private Color defColor;
    private readonly Color warColor = Color.Crimson;
    private readonly Color atColor = Color.DarkOrange;
    private readonly Color goodColor = Color.LightGreen;
    private readonly Color fColor = Color.DodgerBlue;
    private bool filterApp = false;
    private int compltDXFcount = 0;
    private int addNum = 0;
    private readonly string year;

    public InWork()
    {
        InitializeComponent();
        var date = DateTime.Now;
        year = date.Year.ToString();
        year = year.Substring(year.Length - 2, 2);
    }

    private enum runCommand
    {
        Общий,
        Стойки,
        Стойки_Полотна
    }

    //Загрузка таблицы из файлов
    internal void Brows(string[] files)
    {
        Constructor constructor = new Constructor();
        if (tDatas != null) tDatas.Clear();

        foreach (string file in files)
        {
            TableData[] dates;
            dates = constructor.TDatasInFile(file);
            for (short i = 0; i < dates.Length; i++)
                tDatas.Add(dates[i]);
        }
        LoadTable(true);
    }
    //Сканировать открытую таблицу конструирования в окне Excel
    internal void ScanTable()
    {
        ExcelSkaner eScan = new ExcelSkaner();
        if (tDatas != null) tDatas.Clear();

        Hide();
        tDatas = eScan.Scan;
        if (tDatas == null)
        {
            Program.mainForm.Show();
            return;
        }
        else LoadTable(true);
        Show();
    }
    //Загрузить таблицу конструирования (сканирование в скрытом окне Excel)
    internal void ScanFile()
    {
        ExcelSkaner eScan = new ExcelSkaner();
        bool complite = false;

        OpenTableDialog.Title = "Выберите файл конструирования";
        OpenTableDialog.Filter = "Excel files(*.xlsx)|*.xlsx|Excel files(*.xls)|*.xls";
        OpenTableDialog.InitialDirectory = Program.ini.ReadKey("Directoryes", "DIR_CONSTR");
        do
        {
            switch (OpenTableDialog.ShowDialog())
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    var fp = OpenTableDialog.FileName;
                    if (!string.IsNullOrEmpty(fp))
                    {
                        tDatas = eScan.Load(fp);
                        complite = true;
                    }
                    break;
                case DialogResult.Cancel:
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.No:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } while (!complite);
        LoadTable(true);
    }
    //Загрузить сохраненный файла
    private void LoadSaveFile()
    {
        var constr = new Constructor();
        var complite = false;

        OpenTableDialog.Title = "Выберите файл сохранения";
        OpenTableDialog.Filter = "Save files(*.ini)|*.ini";
        OpenTableDialog.InitialDirectory = Program.ini.ReadKey("Directoryes", "DIR_SAVE");
        do
        {
            switch (OpenTableDialog.ShowDialog())
            {
                case DialogResult.OK:
                    string fp = OpenTableDialog.FileName;
                    if (!string.IsNullOrEmpty(fp))
                    {
                        tDatas = constr.TDatasInFile(fp).ToList();
                        complite = true;
                    }
                    break;
                case DialogResult.Cancel:
                    return;
            }
        } while (!complite);
        LoadTable(true);
    }

    //Загрузка таблицы
    private void LoadTable(bool add)
    {
        if(!add) dgvInFiles.Rows.Clear();
        Constants cons = new Constants();

        type.DataSource = cons.GetKodNames();
        otkr.DataSource = Enum.GetNames(typeof(Otkrivanie));
        sdvigoviy.DataSource = Enum.GetNames(typeof(SdvigoviyNames));
        thickLL.DataSource = cons.ListThikcsAsStr();
        thickVL.DataSource = cons.ListThikcsAsStr();
        teh.DataSource = Enum.GetNames(typeof(Tehnology));
        kabel.DataSource = Enum.GetNames(typeof(KabelRaspolozhenie));
        gnd.DataSource = Enum.GetNames(typeof(GNDRaspolozhenie));
        usileniePetel.DataSource = Enum.GetNames(typeof(VirezKvadrat));
        lFind.Text = tDatas.Count.ToString();
        FillTable();
    }
    //Заполнение таблицы
    private void FillTable()
    {
        foreach (var tData in tDatas)
        {
            int y = dgvInFiles.Rows.Add(new DataGridViewRow());

            FillRow(y, tData);
        }
    }
    //Заполнение строки таблицы
    private void FillRow(int numRow, TableData td)
    {
        var row = dgvInFiles.Rows[numRow];
        var err = td.AppMemory;

        if (string.IsNullOrEmpty(err)) err = "";

        //Номер изделия
        row.Cells[num.Index].Value = td.Num;
        //Тип изделия
        row.Cells[type.Index].Value = type.Items[type.Items.IndexOf(td.TypeConstruct)];
        //Высота изделия
        row.Cells[height.Index].Value = td.Height;
        //Ширина изделия
        row.Cells[width.Index].Value = td.Width;

        //Сторона открывания изделия
        row.Cells[otkr.Index].Value = otkr.Items[(int)td.Otkrivanie];
        if (!string.IsNullOrEmpty(td.OtkrFromTable))
        {
            if (!td.OtkrFromTable.Equals("_"))
            {
                row.Cells[otkr.Index].Style.BackColor = warColor;
                row.Cells[otkr.Index].ToolTipText = td.OtkrFromTable;
            }
        }

        //Порог коробки изделия
        if (cons.CompareKod(td.Kod, "ОДЛ"))
            row.Cells[porog.Index].Value = Enum.GetName(typeof(ODL_PorogNames), td.Porog);
        else
            row.Cells[porog.Index].Value = Enum.GetName(typeof(DM_PorogNames), td.Porog);
        if (!string.IsNullOrEmpty(td.PorogFromTable))
        {
            if (!td.PorogFromTable.Equals("_"))
            {
                row.Cells[porog.Index].Style.BackColor = warColor;
                row.Cells[porog.Index].ToolTipText = td.PorogFromTable;
            }
        }

        //Наличники коробки изделия
        row.Cells[nalicniki.Index].Value = td.NalichnikiAsStr;

        //Ширина активки изделия
        row.Cells[aktiv.Index].Value = td.WAktivForTable;
        if (!string.IsNullOrEmpty(td.WAktivFromTable))
        {
            if (!td.WAktivFromTable.Equals("_"))
            {
                row.Cells[aktiv.Index].Style.BackColor = warColor;
                row.Cells[aktiv.Index].ToolTipText = td.WAktivFromTable;
            }
        }

        //Замок 1 (нижний) изделия
        if (td.Zamok[0].Kod > 0)
        {
            if (td.Zamok[0].Kod > 100 & td.Zamok[0].Kod < 200)
                row.Cells[zamok_1.Index].Value = Enum.GetName(typeof(AntipanikaNames), td.Zamok[0].Kod);
            else
                row.Cells[zamok_1.Index].Value = Enum.GetName(typeof(ZamokNames), td.Zamok[0].Kod);
        }
        else {
            if (td.Kodoviy.Type == Kodoviy.Нет)
                row.Cells[zamok_1.Index].Value = Enum.GetName(typeof(ZamokNames), td.Zamok[0].Kod);
            else
                row.Cells[zamok_1.Index].Value = Enum.GetName(typeof(Kodoviy), (int)td.Kodoviy.Type);
        }
        if (!string.IsNullOrEmpty(td.Zamok[0].FromTable))
        {
            if (!td.Zamok[0].FromTable.Equals("_"))
            {
                row.Cells[zamok_1.Index].Style.BackColor = warColor;
                row.Cells[zamok_1.Index].ToolTipText = td.Zamok[0].FromTable;
            }
        }

        //Цилиндр замка 1
        row.Cells[z1CM.Index].Value = td.Zamok[0].Cilinder;

        //Ручка-1 изделия
        row.Cells[ruchka_1.Index].Value = Enum.GetName(typeof(RuchkaNames), td.GetRuchkaKod((int)Stvorka.Активная, 0));
        if (!string.IsNullOrEmpty(td.GetRuchkaFromTable((int)Stvorka.Активная, 0)))
        { 
            if (!td.GetRuchkaFromTable((int)Stvorka.Активная, 0).Equals("_"))
            {
                row.Cells[ruchka_1.Index].Style.BackColor = warColor;
                row.Cells[ruchka_1.Index].ToolTipText = td.GetRuchkaFromTable((int)Stvorka.Активная, 0);
            }
        }

        //Замок 2 (верхний) изделия
        if (td.Zamok[1].Kod > 0)
            row.Cells[zamok_2.Index].Value = Enum.GetName(typeof(ZamokNames), td.Zamok[1].Kod);
        else
        {
            if (td.Kodoviy.Type == Kodoviy.Нет)
                row.Cells[zamok_2.Index].Value = Enum.GetName(typeof(ZamokNames), td.Zamok[1].Kod);
            else
            {
                if (td.Zamok[0].Kod > 0)
                    row.Cells[zamok_2.Index].Value = Enum.GetName(typeof(Kodoviy), td.Kodoviy.Type);
                else
                    row.Cells[zamok_2.Index].Value = Enum.GetName(typeof(ZamokNames), td.Zamok[1].Kod);
            }
        }
        if (!string.IsNullOrEmpty(td.Zamok[1].FromTable))
        {
            if (!td.Zamok[1].FromTable.Equals("_"))
            {
                row.Cells[zamok_2.Index].Style.BackColor = warColor;
                row.Cells[zamok_2.Index].ToolTipText = td.Zamok[1].FromTable;
            }
        }

        //Ручка-2 изделия
        row.Cells[ruchka_2.Index].Value = Enum.GetName(typeof(RuchkaNames), td.GetRuchkaKod((short)Stvorka.Активная, 1));
        if (!string.IsNullOrEmpty(td.GetRuchkaFromTable((short)Stvorka.Активная, 1)))
        {
            if (!td.GetRuchkaFromTable((short)Stvorka.Активная, 1).Equals("_"))
            {
                row.Cells[ruchka_2.Index].Style.BackColor = warColor;
                row.Cells[ruchka_2.Index].ToolTipText = td.GetRuchkaFromTable((short)Stvorka.Активная, 1);
            }
        }

        //Ручка-3 изделия
        row.Cells[ruchka_3.Index].Value = Enum.GetName(typeof(RuchkaNames), td.GetRuchkaKod((short)Stvorka.Пассивная, 0));
        if (!string.IsNullOrEmpty(td.GetRuchkaFromTable((short)Stvorka.Пассивная, 0)))
        {
            if (!td.GetRuchkaFromTable((short)Stvorka.Пассивная, 0).Equals("_"))
            {
                row.Cells[ruchka_3.Index].Style.BackColor = warColor;
                row.Cells[ruchka_3.Index].ToolTipText = td.GetRuchkaFromTable((short)Stvorka.Пассивная, 0);
            }
        }

        //Ручка-4 изделия
        row.Cells[ruchka_4.Index].Value = Enum.GetName(typeof(RuchkaNames), td.GetRuchkaKod((short)Stvorka.Пассивная, 1));
        if (!string.IsNullOrEmpty(td.GetRuchkaFromTable((short)Stvorka.Пассивная, 1)))
        {
            if (!td.GetRuchkaFromTable((short)Stvorka.Пассивная, 1).Equals("_"))
            {
                row.Cells[ruchka_4.Index].Style.BackColor = warColor;
                row.Cells[ruchka_4.Index].ToolTipText = td.GetRuchkaFromTable((short)Stvorka.Пассивная, 1);
            }
        }

        //Задвижка изделия
        row.Cells[zadvizhka.Index].Value = Enum.GetName(typeof(ZadvizhkaNames), td.Zadvizhka.Kod);

        //Сдвиговый Замок
        if (!cons.CompareKod(td.Kod, "ДМ"))
        {
            row.Cells[sdvigoviy.Index].Value = sdvigoviy.Items[0];
            row.Cells[sdvigoviy.Index].ReadOnly = true;
        }
        else
            row.Cells[sdvigoviy.Index].Value = Enum.GetName(typeof(SdvigoviyNames), td.SdvigoviyZamok.Kod);

        //Наличие торцевых шпингалетов
        row.Cells[tShpin.Index].Value = td.TSpingalet;

        //Глазок изделия
        row.Cells[glazok.Index].Value = td.GlazokAsString();

        //Макушка Интек
        row.Cells[intek.Index].Value = td.Intek;

        //Толщина лицевого листа изделия
        if(td.Thick_LL > 2) 
            row.Cells[thickLL.Index].Value = thickLL.Items[0];
        else
            row.Cells[thickLL.Index].Value = thickLL.Items[thickLL.Items.IndexOf(Math.Round(td.Thick_LL, 1).ToString())];
        //Толщина внутреннего листа изделия
        if (td.Thick_VL == 0 | td.Thick_VL > 2)
            row.Cells[thickVL.Index].Value = thickVL.Items[0];
        else
            row.Cells[thickVL.Index].Value = thickVL.Items[thickVL.Items.IndexOf(Math.Round(td.Thick_VL, 1).ToString())];

        //Технология изделия (техничка, EI, EIS)
        if (td.EI)
            row.Cells[teh.Index].Value = teh.Items[1];
        else if (td.EIS)
            row.Cells[teh.Index].Value = teh.Items[2];
        else if(td.EIW)
            row.Cells[teh.Index].Value = teh.Items[3];
        else if (td.EIWS)
            row.Cells[teh.Index].Value = teh.Items[4];
        else
            row.Cells[teh.Index].Value = teh.Items[0];

        //Термоблокератор
        row.Cells[term.Index].Value = td.Termoblock;

        //Кабельканал
        row.Cells[kabel.Index].Value = kabel.Items[(int)td.Kabel];

        //GND
        row.Cells[gnd.Index].Value = gnd.Items[(int)td.GND];

        //Усиливающий квадрат под петли
        row.Cells[usileniePetel.Index].Value = usileniePetel.Items[(int)td.Kvadrat];

        //Доборы
        row.Cells[dobor.Index].Value = td.DoborParam.AsString();

        //Окна изделия
        for (int i=0; i<4; i++)
            row.Cells["okno_" + (i + 1)].Value = td.OknoArr[i].AsString();

        //Решетки изделия
        for(int i=0; i<4; i++)
        {
            row.Cells["resh_" + (i + 1)].Value = td.ReshArr[i].AsString();
            if (!string.IsNullOrEmpty(td.ReshFromTable))
            {
                if (!td.ReshFromTable.Equals("_"))
                {
                    row.Cells["resh_" + (i + 1)].Style.BackColor = warColor;
                    row.Cells["resh_" + (i + 1)].ToolTipText = td.ReshFromTable;
                }
            }
        }

        //Калитка изделия
        if (td.Kalit)
        {
            row.Cells[kalit.Index].Value = td.KalitAsString;
            if (!string.IsNullOrEmpty(td.KabelFromTable))
            {
                if (!td.KabelFromTable.Equals("_"))
                {
                    row.Cells[kalit.Index].Style.BackColor = warColor;
                    row.Cells[kalit.Index].ToolTipText = td.KalitFromTable;
                }
            }
        }
        else 
            row.Cells[kalit.Index].Value = "Нет";

        //Фрамуги изделия
        for(int i=0; i<4; i++)
        {
            row.Cells["fr_" + i].Value = td.FramugaParArr[i].AsString();
            if (!string.IsNullOrEmpty(td.FramugaParArr[i].FromTable))
            {
                if (!td.FramugaParArr[i].FromTable.Equals("_"))
                {
                    row.Cells["fr_" + i].Style.BackColor = warColor;
                    row.Cells["fr_" + i].ToolTipText = td.FramugaParArr[i].FromTable;
                }
            }
        }

        //Комментарии к изделию
        row.Cells[comments.Index].Value = td.Comments;

        //Ошибки расчетов изделия
        if (!err.Equals(""))
        {
            if (td.AppError)
            {
                MessageToRow(numRow, warColor, err);
            }
            else if (td.AppProblem)
            {
                MessageToRow(numRow, atColor, err);
            }
        }
    }

    //Найти TableData по номеру изделия
    private TableData SearchByNum(List<TableData> tDatas, string num)
    {
        foreach (var td in tDatas)
        {
            if (td.Num.Equals(num)) return td;
        }
        return null;
    }
    //Получить индекс в коллекции TableData по номеру изделия
    private int GetDataIndexByNum(List<TableData> tDatas, string num)
    {
        return tDatas.IndexOf(SearchByNum(tDatas, num));
    }
    //Установить видимость кнопок
    private void SetBtnVisible(bool visible)
    {
        pbProgress.Visible = !visible;
        btnCancel.Visible = !visible;
        lDone.Visible = !visible;
        lWorksComment.Visible = !visible;
        lAllToRun.Visible = !visible;
    }
    //Подкрашивание строки и вывод сообщений в колонку "Ошибки"
    private void MessageToRow(int rowNum, Color color, string message)
    {
        foreach (DataGridViewCell Cell in dgvInFiles.Rows[rowNum].Cells)
        {
            if (Cell.Style.BackColor != warColor | Cell.Style.BackColor != atColor)
            {
                Cell.Style.BackColor = color;
                dgvInFiles.Rows[rowNum].Cells[err.Index].Value = message;
            }
        }
    }
    //Подсчет общего количества выходных DXF файлов для расчетных изделий
    private short CalculateDXF(ref Constructor constr, runCommand com)
    {
        short count = 0;
        if (constr.DMs.Length > 0)
        {
            var dms = constr.DMs;
            if (com == runCommand.Общий)
            {
                foreach (DM dm in dms)
                {
                    count += 1;
                    if (cons.CompareKod(dm.Kod, "ДМ-2"))
                    {
                        if (dm.IsTorcShpingalet((short)Raspolozhenie.Верх) | dm.IsOtvAntipan((short)Raspolozhenie.Верх))
                            count += 1;
                        if (dm.IsTorcShpingalet((short)Raspolozhenie.Ниж) | dm.IsOtvAntipan((short)Raspolozhenie.Ниж))
                            count += 1;
                    }
                }
            }
            else if (com == runCommand.Стойки_Полотна)
            {
                foreach (DM dm in dms)
                {
                    count += 4;
                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) > 0)
                        count += 1;
                }
            }
            else
            {
                foreach (DM dm in dms)
                {
                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) > 0)
                        count += 4;
                    else
                        count += 3;
                }
            }
        }
        if (constr.LMs.Length > 0)
        {
            if (com == runCommand.Общий)
                count += (short)(constr.LMs.Length * 2);
        }
        if (constr.VMs.Length > 0)
        {
            if (com == runCommand.Общий)
            {
                var vms = constr.VMs;
                count += (short)(constr.VMs.Length * 2);
                foreach (var vm in vms)
                {
                    if (vm.IsEnyDobor)
                        count += 1;
                }
            }
        }
        if (constr.ODLs.Length > 0)
        {
            if (com == runCommand.Общий)
            {
                foreach (ODL odl in constr.ODLs)
                {
                    if(odl.IsPassivka)
                        count += 7;
                    else
                        count += 6;
                    if (odl.Nalichnik(Raspolozhenie.Ниж) > 0)
                        count ++;
                    if ((odl.Otkrivanie == Otkrivanie.ЛевоеВО || odl.Otkrivanie == Otkrivanie.ПравоеВО) && odl.Porog > 0)
                        count ++;
                }
                
            }
        }

        return count;
    }
    //Подсчет общего количества выходных DXF файлов для не расчетных изделий (КВ..)
    private static short CalculateDXF(IEnumerable<KVD> list)
    {
        return (short)list.Sum(kvd => kvd.Parts.Count);
    }

    private void CompliteDxf()
    {
        compltDXFcount++;
        pbProgress.PerformStep();
        lDone.Text = compltDXFcount.ToString();
    }
    //Найти строку в таблице по номеру изделия
    private short SearchRowByNum(string number)
    {
        for (var i = 0; i < dgvInFiles.Rows.Count; i++)
        {
            if (dgvInFiles.Rows[i].Cells[num.Index].Value.Equals(number))
            {
                return (short)i;
            }
        }
        return -1;
    }
    //Действия по окончанию выполнения операций
    private void CompliteSub(bool complite)
    {
        if (complite)
        {
            btnCancel.Text = "Назад";
            dgvInFiles.ClearSelection();
        }
        else
            btnCancel.Text = "Отмена";
        this.complite = complite;
    }
    //Выделение строки
    private void SelectRow(short rowNum)
    {
        dgvInFiles.ClearSelection();
        if(dgvInFiles.CurrentCell != null)
            dgvInFiles.CurrentCell.Selected = false;
        dgvInFiles.Rows[rowNum].Selected = true;
        dgvInFiles.FirstDisplayedScrollingRowIndex = rowNum;
    }

    //Сохранение таблицы в файл
    private void Save()
    {
        bool complite = false;
        string fPath = Program.ini.ReadKey("Directoryes", "DIR_SAVE");
        do
        {
            string fName = Interaction.InputBox("Введите название файла", "Имя файла");
            if (!Directory.Exists(fPath)) Directory.CreateDirectory(fPath);
            if (File.Exists(fPath + @"\" + fName + ".ini"))
            {
                switch (Interaction.MsgBox("Файл с таким именем уже существует!" + '\n' + "Заменить файл?", (MsgBoxStyle)35, "Сохранить файл"))
                {
                    case MsgBoxResult.Yes: // Да
                        {
                            File.Delete(fPath + @"\" + fName + ".ini");
                            Save(fPath + @"\" + fName + ".ini");
                            complite = true;
                            break;
                        }

                    case MsgBoxResult.No: // Нет
                        {
                            complite = false;
                            break;
                        }

                    case MsgBoxResult.Cancel: // Отмена
                        return;
                }
            }
            else
            {
                Save(fPath + @"\" + fName + ".ini");
                complite = true;
            }
        } while (!complite);
    }
    private void Save(string fName)
    {
        for (int i = 0; i < dgvInFiles.Rows.Count; i++)
        {
            var data = SearchByNum(tDatas, dgvInFiles.Rows[i].Cells[num.Index].Value.ToString());
            if (dgvInFiles.Rows[i].Cells[num.Index].Style.BackColor == warColor)
            {
                data.AppError = true;
                data.AppProblem = false;
                data.AppMemory =dgvInFiles.Rows[i].Cells[err.Index].Value.ToString();
            }
            else if (dgvInFiles.Rows[i].Cells[num.Index].Style.BackColor == atColor)
            {
                data.AppError = false;
                data.AppProblem = true;
                data.AppMemory = dgvInFiles.Rows[i].Cells[err.Index].Value.ToString();
            }
            else
            {
                data.AppError = false;
                data.AppProblem = false;
                data.AppMemory = "";
            }
            data.SaveData(fName);
        }
    }

    //Расчет и построение изделий из таблицы
    private async void Run(runCommand com)
    {
        compltDXFcount = 0;
        complite = false;
        Application.DoEvents();
        if (tDatas == null) return;

        var ctor = new Constructor();
        MaketChanger mCh;
        var mChType = int.Parse(Program.ini.ReadKey("Global", "MAKET_CHANGER"));
        if (mChType == 0)
            mCh = new SW_MaketChanger(Program.ini.ReadKey("Directoryes", "DIR_MAKET"), 
                Program.ini.ReadKey("Global", "G_MAKET"), 
                Program.ini.ReadKey("Directoryes", "DIR_DXF"));
        else
            mCh = new Kompas_MaketChanger(Program.ini.ReadKey("Directoryes", "DIR_MAKET"),
                Program.ini.ReadKey("Directoryes", "DIR_DXF"));
        short fullDXFcount;

        SetBtnVisible(false);
        CompliteSub(false);

        //Расчет изделий
        for (var i = 0; i < dgvInFiles.Rows.Count; i++)
        {
            if (!cancel) //если не нажата кнопка отмены
            {
                if (!string.IsNullOrEmpty(dgvInFiles.Rows[i].Cells[num.Index].Value.ToString())) //если колонка с номером изелия не пуста
                {
                    if (dgvInFiles.Rows[i].Visible & !dgvInFiles.Rows[i].Cells[num.Index].Style.BackColor.Equals(warColor)) //строка не скрыта и не красная
                    {
                        TableData tData = new TableData();
                        bool ok = false;
                        if (com == runCommand.Общий)
                        {
                            ok = true;
                            tData = SearchByNum(tDatas, dgvInFiles.Rows[i].Cells[num.Index].Value.ToString()); //получаем TableData
                        }
                        else
                        {
                            if (dgvInFiles.Rows[i].Cells[type.Index].Value.ToString().IndexOf("ДМ") >= 0 |
                                dgvInFiles.Rows[i].Cells[type.Index].Value.ToString().IndexOf("MAX") >= 0)
                            {
                                ok = true;
                                tData = SearchByNum(tDatas, dgvInFiles.Rows[i].Cells[num.Index].Value.ToString());
                            }
                        }
                        if (ok)
                        {
                            string result = ctor.Add(tData);
                            if (!result.Equals("OK"))
                                MessageToRow(i, warColor, result);
                        }
                    }
                }
            }
            else
            {
                CompliteSub(true);
                return;
            }
        }

        fullDXFcount = CalculateDXF(ref ctor, com);
        pbProgress.Minimum = 0;
        pbProgress.Maximum = fullDXFcount;
        pbProgress.Step = 1;
        lAllToRun.Text = fullDXFcount.ToString();
        if (ctor.DMs.Length > 0)
        {
            var dms = ctor.DMs;
            foreach (var dm in dms)
            {
                lDone.Text = compltDXFcount.ToString();
                if (!cancel)
                {
                    SelectRow(SearchRowByNum(dm.Num));
                    if (com == runCommand.Общий | com == runCommand.Стойки_Полотна)
                    {
                        if (cons.CompareKod(dm.Kod, "ДОБОР"))
                        {
                            await Task.Run(() => mCh.Build_DM(dm, Command_DM.Добор));
                            CompliteDxf();
                        }
                        else
                        {
                            await Task.Run(() => mCh.Build_DM(dm, Command_DM.Полотна));
                            CompliteDxf();
                            if (com == runCommand.Общий)
                            {
                                if (cons.CompareKod(dm.Kod, "ДМ-2"))
                                {
                                    if (dm.IsTorcShpingalet((int)Raspolozhenie.Верх) | 
                                        dm.IsAnkerInPritoloka | 
                                        dm.IsOtvAntipan((int)Raspolozhenie.Верх))
                                    {
                                        await Task.Run(() => mCh.Build_DM(dm, Command_DM.Притолока));
                                        CompliteDxf();
                                    }
                                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) > 0 & 
                                        (dm.IsTorcShpingalet((int)Raspolozhenie.Ниж) | 
                                         dm.IsOtvAntipan((int)Raspolozhenie.Ниж)))
                                    {
                                        await Task.Run(() => mCh.Build_DM(dm, Command_DM.Порог));
                                        CompliteDxf();
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(dm.Problems))
                                MessageToRow(SearchRowByNum(dm.Num), goodColor, "");
                            else
                                MessageToRow(SearchRowByNum(dm.Num), atColor, dm.Problems);
                        }
                    }
                    if(com == runCommand.Стойки | com == runCommand.Стойки_Полотна)
                    {
                        await Task.Run(() => mCh.Build_DM(dm, Command_DM.Левая_стойка));
                        CompliteDxf();
                        await Task.Run(() => mCh.Build_DM(dm, Command_DM.Правя_стойка));
                        CompliteDxf();
                        await Task.Run(() => mCh.Build_DM(dm, Command_DM.Притолока));
                        CompliteDxf();
                        if(dm.Stoyka_Type(Raspolozhenie.Ниж) > 0)
                        {
                            await Task.Run(() => mCh.Build_DM(dm, Command_DM.Порог));
                            CompliteDxf();
                        }

                    }
                }
                else
                {
                    CompliteSub(true);
                    return;
                }
            }
        }
        if (com == runCommand.Общий)
        {
            if (ctor.LMs.Length > 0)
            {
                var lms = ctor.LMs;
                foreach (var lm in lms)
                {
                    if (!cancel)
                    {
                        SelectRow(SearchRowByNum(lm.Num));
                        for (short i = 0; i <= 1; i++)
                        {
                            lDone.Text = compltDXFcount.ToString();
                            if (!cancel)
                            {
                                if (i == 0)
                                {
                                    await Task.Run(() => mCh.Build_LM(lm, Command_LM.Полотна));
                                }
                                else
                                {
                                    await Task.Run(() => mCh.Build_LM(lm, Command_LM.Коробка));
                                }
                                CompliteDxf();
                            }
                            else
                            {
                                CompliteSub(true);
                                return;
                            }
                        }
                        if (string.IsNullOrEmpty(lm.Problems))
                            MessageToRow(SearchRowByNum(lm.Num), goodColor, "");
                        else
                            MessageToRow(SearchRowByNum(lm.Num), atColor, lm.Problems);
                    }
                    else
                    {
                        CompliteSub(true);
                        return;
                    }
                }
            }
            if (ctor.VMs.Length > 0)
            {
                var vms = ctor.VMs;
                foreach (var vm in vms)
                {
                    if (!cancel)
                    {
                        SelectRow(SearchRowByNum(vm.Num));
                        short c;
                        if (vm.IsEnyDobor)
                            c = 2;
                        else
                            c = 1;

                        for (int i = 0; i < c; i++)
                        {
                            lDone.Text = compltDXFcount.ToString();
                            if (!cancel)
                            {
                                if (i == 0)
                                    await Task.Run(() => mCh.Build_VM(vm, Command_VM.Полотна));
                                else if (i == 1)
                                    await Task.Run(() => mCh.Build_VM(vm, Command_VM.Коробка));
                                else
                                    await Task.Run(() => mCh.Build_VM(vm, Command_VM.Доборы));

                                CompliteDxf();
                            }
                            else
                            {
                                CompliteSub(true);
                                return;
                            }
                        }

                        if (string.IsNullOrEmpty(vm.Problems))
                            MessageToRow(SearchRowByNum(vm.Num), goodColor, "");
                        else
                            MessageToRow(SearchRowByNum(vm.Num), atColor, vm.Problems);
                    }
                    else
                    {
                        CompliteSub(true);
                        return;
                    }
                }
            }
            if (ctor.ODLs.Length > 0)
            {
                var odls = ctor.ODLs;
                foreach (var odl in odls)
                {
                    if (cancel) 
                    {
                        CompliteSub(true);
                        return;
                    }
                    
                    SelectRow(SearchRowByNum(odl.Num));
                    for (int i = 0; i <= 8; i++)
                    {
                        lDone.Text = compltDXFcount.ToString();
                        if (cancel)
                        {
                            CompliteSub(true);
                            return;
                        }
                        
                        if (i == 1)
                        {
                            if (odl.IsPassivka)
                            {
                                await Task.Run(() => mCh.Build_ODL(odl, Command_ODL.Полотно_пассивки));
                                CompliteDxf();
                            }
                        }
                        else if (i == 2)
                        {
                            if (!odl.IsPassivka)
                            {
                                await Task.Run(() => mCh.Build_ODL(odl, Command_ODL.Замковая_стойка));
                                CompliteDxf();
                            }
                        }
                        else if (i == 7)
                        {
                            if (odl.IsPassivka)
                            {
                                await Task.Run(() => mCh.Build_ODL(odl, Command_ODL.Замковой_профиль_пассивки));
                                CompliteDxf();
                            }
                        }
                        else if(i == 8)
                        {
                            if(odl.Nalichnik(Raspolozhenie.Ниж) > 0)
                            {
                                await Task.Run(() => mCh.Build_ODL(odl, Command_ODL.Нижняя_стойка));
                                CompliteDxf();
                            }
                            else if((odl.Otkrivanie == Otkrivanie.ЛевоеВО || odl.Otkrivanie == Otkrivanie.ПравоеВО) && odl.Porog > 0)
                            {
                                await Task.Run(() => mCh.Build_ODL(odl, Command_ODL.Порог));
                                CompliteDxf();
                            }
                        }
                        else
                        {
                            await Task.Run(() => mCh.Build_ODL(odl, (Command_ODL)i));
                            CompliteDxf();
                        }
                    }

                    if (string.IsNullOrEmpty(odl.Problems))
                        MessageToRow(SearchRowByNum(odl.Num), goodColor, "");
                    else
                        MessageToRow(SearchRowByNum(odl.Num), atColor, odl.Problems);
                }

                lDone.Text = compltDXFcount.ToString();
            }
        }
        CompliteSub(true);
        mCh.Exit();
        
        var goodsCount = 0;
        var errDic = new Dictionary<string, string>();
        var probDic = new Dictionary<string, string>();
        foreach (DataGridViewRow row in dgvInFiles.Rows)
        {
            if (row.Cells[0].Style.BackColor == warColor)
                errDic.Add(row.Cells[num.Index].Value.ToString(), row.Cells[err.Index].Value.ToString());

            if (row.Cells[0].Style.BackColor == atColor)
                probDic.Add(row.Cells[num.Index].Value.ToString(), row.Cells[err.Index].Value.ToString());

            if (row.Cells[0].Style.BackColor == goodColor)
                goodsCount++;
        }

        if (errDic.Count > 0 || probDic.Count > 0)
        {
            var allCount = errDic.Count + probDic.Count + goodsCount;
            var message = $"Из {allCount} изделий выполнено: \n    Без ошибок - {goodsCount};\n    С проблемами - {probDic.Count};\n    Не выполнено из-за ошибок - {errDic.Count}\n\n=====Сводка=====\n";
            if (probDic.Count > 0)
            {
                message += "Проблемы:\n";
                foreach (var problem in probDic)
                    message += $"{problem.Key} - {problem.Value}\n";
            }

            if (errDic.Count > 0)
            {
                message += "Ошибки:\n";
                foreach(var error in errDic)
                    message += $"{error.Key} - {error.Value}\n";
            }

            MessageBox.Show(message);
        }
    }
    //Компановка крартирных дверей из базы
    private async void LayoutFiles()
    {
        if (tDatas == null || tDatas.Count == 0)
            return;

        Application.DoEvents();

        var kvds = new List<KVD>();

        foreach (var data in GetVisibleDatas())
        {
            var kod = cons.GetKOD(data.Kod);
            switch (kod.FilePref)
            {
                case "КВ01":
                    if (data.Otkrivanie == Otkrivanie.Левое || data.Otkrivanie == Otkrivanie.Правое)
                    {
                        if(data.LicPanel)
                            kvds.Add(new KV01c(data, cons));
                        else
                            kvds.Add(new KV01a(data, cons));
                    }
                    else
                        kvds.Add(new KV01b(data, cons));
                    break;
                case "КВ02":
                case "КВ03":
                case "КВ04":
                    break;
                case "КВ05":
                    kvds.Add(new KV05(data, cons));
                    break;
                case "КВ06":
                    if(kod.Name.Equals("РИО", StringComparison.InvariantCultureIgnoreCase))
                        kvds.Add(new KV06a(data, cons));
                    else
                        kvds.Add(new KV06b(data, cons));
                    break;
                case "КВ07":
                    kvds.Add(new KV07(data, cons));
                    break;
                case "КВ08":
                    kvds.Add(new KV08(data, cons));
                    break;
                case "КВ09":
                    break;
            }
        }

        if(kvds.Count == 0)
            return;

        compltDXFcount = 0;
        complite = false;

        SetBtnVisible(false);
        CompliteSub(false);

        var fullDXFcount = CalculateDXF(kvds);
        pbProgress.Minimum = 0;
        pbProgress.Maximum = fullDXFcount;
        pbProgress.Step = 1;
        lAllToRun.Text = fullDXFcount.ToString();
        lDone.Text = compltDXFcount.ToString();

        foreach (var kvd in kvds)
        {
            SelectRow(SearchRowByNum(kvd.Num));

            foreach (var part in kvd.Parts)
            {
                var mCh = new SW_MaketChanger(kvd.MaketDir, part.MaketName,
                    Program.ini.ReadKey("Directoryes", "DIR_DXF") + $@"\{year}.{kvd.Data.ER}\{kvd.Data.Num}");

                await Task.Run(() => mCh.Build_KVD(kvd, part.Command, part.FileName));
                CompliteDxf();
                mCh.Exit();
            }

            MessageToRow(SearchRowByNum(kvd.Num), goodColor, "");
        }

        //foreach (var mapper in kvds.Select(kvd => 
        //             new KVDFileMapper(kvd, 
        //             Program.ini.ReadKey("Directoryes", "DIR_KVD"),
        //             Program.ini.ReadKey("Directoryes", "DIR_DXF"))))
        //{
        //    mapper.StepComplete += FileComplete;
        //    mapper.Run();
        //    if(mapper.Errors.Count == 0)
        //        MessageToRow(SearchRowByNum(mapper.KVDNum), goodColor, "");
        //}

        CompliteSub(true);

        void FileComplete(KVDFileMapper sender, bool complete)
        {
            if(complete)
                CompliteDxf();
            else
                foreach(var error in sender.Errors)
                    MessageToRow(SearchRowByNum(sender.KVDNum), warColor, error);
        }
    }

    //Получить все видимые (после фильтрации) данные
    private TableData[] GetVisibleDatas()
    {
        var datas = new List<TableData>();
        foreach(DataGridViewRow row in dgvInFiles.Rows)
            if(row.Visible)
                datas.Add(SearchByNum(tDatas, row.Cells[num.Index].Value.ToString()));
        return datas.ToArray();
    }

    //События нажатия кнопок
    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (complite)
            SetBtnVisible(true);
        else
            cancel = true;
    }

    //События таблицы
    private void dgvInFiles_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
    {
        if (!dgvInFiles.Columns[e.ColumnIndex].ReadOnly)
        {
            editNum = dgvInFiles.Rows[e.RowIndex].Cells["num"].Value.ToString();
            preChangeValue = dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }
    }
    private void dgvInFiles_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        var collName = dgvInFiles.Columns[e.ColumnIndex].Name;
        if (!dgvInFiles.Columns[e.ColumnIndex].ReadOnly)
        {
            switch (collName)
            {
                case "num":
                    postChangeValue = dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    if (!postChangeValue.Equals(preChangeValue))
                    {
                        if (int.TryParse(postChangeValue, out _))
                            SearchByNum(tDatas, editNum).Num = postChangeValue;
                        else
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = preChangeValue;
                    }
                    break;

                case "type":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).Kod = cons.KodFromString(postChangeValue);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = type.Items[type.Items.IndexOf(SearchByNum(tDatas, editNum).TypeConstruct)];
                    break;

                case "height":
                    postChangeValue = dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    if (!postChangeValue.Equals(preChangeValue))
                    {
                        if (short.TryParse(postChangeValue, out short h))
                            SearchByNum(tDatas, editNum).Height = h;
                        else
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = preChangeValue;
                    }
                    break;

                case "width":
                    postChangeValue = dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    if (!postChangeValue.Equals(preChangeValue))
                    {
                        if (short.TryParse(postChangeValue, out short w))
                            SearchByNum(tDatas, editNum).Width = w;
                        else
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = preChangeValue;
                    }
                    break;

                case "otkr":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).Otkrivanie = (Otkrivanie)Enum.Parse(typeof(Otkrivanie), postChangeValue);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 
                            otkr.Items[otkr.Items.IndexOf(SearchByNum(tDatas, editNum).Otkrivanie)];
                    break;

                case "intek":
                    SearchByNum(tDatas, editNum).Intek = (bool)dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    break;

                case "thickLL":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).Thick_LL = Convert.ToDouble(dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 
                            thickLL.Items[thickLL.Items.IndexOf(Math.Round(SearchByNum(tDatas, editNum).Thick_LL, 1).ToString())];
                    break;

                case "thickVL":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).Thick_VL = Convert.ToDouble(dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value =
                            thickLL.Items[thickLL.Items.IndexOf(Math.Round(SearchByNum(tDatas, editNum).Thick_VL, 1).ToString())];
                    break;
                case "teh":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                    {
                        switch (dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString())
                        {
                            case "EI":
                                SearchByNum(tDatas, editNum).DPM = true;
                                SearchByNum(tDatas, editNum).EI = true;
                                SearchByNum(tDatas, editNum).EIS = false;
                                SearchByNum(tDatas, editNum).EIW = false;
                                SearchByNum(tDatas, editNum).EIWS = false;
                                SearchByNum(tDatas, editNum).Termoblock = false;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = false;
                                break;

                            case "EIS":
                                SearchByNum(tDatas, editNum).DPM = true;
                                SearchByNum(tDatas, editNum).EI = false;
                                SearchByNum(tDatas, editNum).EIS = true;
                                SearchByNum(tDatas, editNum).EIW = false;
                                SearchByNum(tDatas, editNum).EIWS = false;
                                SearchByNum(tDatas, editNum).Termoblock = false;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = false;
                                break;

                            case "Техничка_EIS":
                                SearchByNum(tDatas, editNum).DPM = false;
                                SearchByNum(tDatas, editNum).EI = false;
                                SearchByNum(tDatas, editNum).EIS = true;
                                SearchByNum(tDatas, editNum).EIW = false;
                                SearchByNum(tDatas, editNum).EIWS = false;
                                SearchByNum(tDatas, editNum).Termoblock = false;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = false;
                                break;

                            case "EIW":
                                SearchByNum(tDatas, editNum).DPM = false;
                                SearchByNum(tDatas, editNum).EI = false;
                                SearchByNum(tDatas, editNum).EIS = false;
                                SearchByNum(tDatas, editNum).EIW = true;
                                SearchByNum(tDatas, editNum).EIWS = false;
                                SearchByNum(tDatas, editNum).Termoblock = true;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = true;
                                break;

                            case "EIWS":
                                SearchByNum(tDatas, editNum).DPM = false;
                                SearchByNum(tDatas, editNum).EI = false;
                                SearchByNum(tDatas, editNum).EIS = false;
                                SearchByNum(tDatas, editNum).EIW = false;
                                SearchByNum(tDatas, editNum).EIWS = true;
                                SearchByNum(tDatas, editNum).Termoblock = true;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = true;
                                break;

                            default:
                                SearchByNum(tDatas, editNum).DPM = false;
                                SearchByNum(tDatas, editNum).EI = false;
                                SearchByNum(tDatas, editNum).EIS = false;
                                SearchByNum(tDatas, editNum).EIW = false;
                                SearchByNum(tDatas, editNum).EIWS = false;
                                SearchByNum(tDatas, editNum).Termoblock = false;
                                dgvInFiles.Rows[e.RowIndex].Cells[term.Index].Value = false;
                                break;
                        }
                    }
                    else
                    {
                        if (SearchByNum(tDatas, editNum).EI)
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = teh.Items[1];
                        else if(SearchByNum(tDatas, editNum).EIS)
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = teh.Items[2];
                        else if(SearchByNum(tDatas, editNum).EIW)
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = teh.Items[3];
                        else if(SearchByNum(tDatas, editNum).EIWS)
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = teh.Items[4];
                        else
                            dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = teh.Items[0];
                    }
                    break;
                case "term":
                    SearchByNum(tDatas, editNum).Termoblock =
                        (bool) dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    break;
                case "kabel":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                    {
                        TableData changedData = SearchByNum(tDatas, editNum);
                        if (changedData.Zamok[0].ElMehZashelka)
                        {
                            if(cons.CompareKod(changedData.Kod, "ДМ-2"))
                                SearchByNum(tDatas, editNum).Kabel = (KabelRaspolozhenie)Enum.Parse(typeof(KabelRaspolozhenie), postChangeValue);
                            else
                                dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = kabel.Items[(int)SearchByNum(tDatas, editNum).Kabel];
                        }
                        else
                            SearchByNum(tDatas, editNum).Kabel = (KabelRaspolozhenie)Enum.Parse(typeof(KabelRaspolozhenie), postChangeValue);
                    }
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = kabel.Items[(int)SearchByNum(tDatas, editNum).Kabel];
                    break;
                case "gnd":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).GND = (GNDRaspolozhenie)Enum.Parse(typeof(GNDRaspolozhenie), postChangeValue);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = gnd.Items[(int)SearchByNum(tDatas, editNum).GND];
                    break;
                case "usileniePetel":
                    postChangeValue = Convert.ToString((dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell).FormattedValue.ToString());
                    if (!string.IsNullOrEmpty(postChangeValue))
                        SearchByNum(tDatas, editNum).Kvadrat = (VirezKvadrat)Enum.Parse(typeof(VirezKvadrat), postChangeValue);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = usileniePetel.Items[(int)SearchByNum(tDatas, editNum).Kvadrat];
                    break;
                case "zamok_1":
                    if (SearchByNum(tDatas, editNum).Kodoviy.Type == Kodoviy.Нет)
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Enum.GetName(typeof(DM_Zamok1Names), SearchByNum(tDatas, editNum).Zamok[0].Kod);
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Enum.GetName(typeof(Kodoviy), SearchByNum(tDatas, editNum).Kodoviy.Type);
                    break;
                case "z1CM":
                    SearchByNum(tDatas, editNum).Zamok[0].Cilinder = (bool)dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    break;
                case "tShpin":
                    SearchByNum(tDatas, editNum).TSpingalet = (bool)dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    break;
                case "sdvigoviy":
                    var val = ((DataGridViewComboBoxCell) dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex]).Value.ToString();
                    if (!string.IsNullOrEmpty(val) && !val.Equals(SdvigoviyNames.Нет.ToString()))
                        SearchByNum(tDatas, editNum).SetSdvigoviy((SdvigoviyNames)Enum.Parse(typeof(SdvigoviyNames), val));
                    else
                        dgvInFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = SearchByNum(tDatas, editNum).SdvigoviyZamok.Name;
                    break;
            }
        }

    }
    private void dgvInFiles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if(e.ColumnIndex < 0) return;
        if (dgvInFiles.Columns[e.ColumnIndex].ReadOnly)
        {
            var nums = new List<string>();
            if (e.RowIndex < 0)
            {
                if(dgvInFiles.SelectedRows.Count == 0) return;
                nums.AddRange(from DataGridViewRow row in dgvInFiles.SelectedRows 
                    select row.Cells["num"].Value.ToString());
            }
            else
                nums.Add(dgvInFiles.Rows[e.RowIndex].Cells["num"].Value.ToString());

            var collName = dgvInFiles.Columns[e.ColumnIndex].Name;
            TableData firstTD = null;

            for (var i=0; i<nums.Count; i++)
            {
                var td = SearchByNum(tDatas, nums[i]);

                switch (collName)
                {
                    case "porog":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Порог, ref td);
                        else
                        {
                            if(firstTD is null) break;
                            td.Porog = firstTD.Porog;
                        }
                        break;
                    case "nalicniki":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Наличники, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Nalichniki = firstTD.Nalichniki;
                        }
                        break;
                    case "aktiv":
                        if (cons.CompareKod(td.Kod, "ДМ-2") | cons.CompareKod(td.Kod, "MAX") | cons.CompareKod(td.Kod, "ЛМ-2") |
                            cons.CompareKod(td.Kod, "ОДЛ-2") | cons.CompareKod(td.Kod, "ВМ-2") | cons.CompareKod(td.Kod, "ВМк"))
                        {
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Створки, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.Nalichniki = firstTD.Nalichniki;
                            }
                        }
                        else
                            return;
                        break;
                    case "zamok_1":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Замок_1, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Zamok[0] = firstTD.Zamok[0];
                        }
                        break;
                    case "ruchka_1":
                        if (cons.CompareKod(td.Kod, "ДМ") | cons.CompareKod(td.Kod, "ЛМ") | cons.CompareKod(td.Kod, "ОДЛ"))
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Ручка_1, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.SetRuchka((short)Stvorka.Активная, 0, firstTD.GetRuchka((short)Stvorka.Активная, 0));
                            }
                        else return;
                        break;
                    case "zamok_2":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Замок_2, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Zamok[1] = firstTD.Zamok[1];
                        }
                        break;
                    case "ruchka_2":
                        if (cons.CompareKod(td.Kod, "ДМ") | cons.CompareKod(td.Kod, "ЛМ") | cons.CompareKod(td.Kod, "ОДЛ"))
                        {
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Ручка_2, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.SetRuchka((short)Stvorka.Активная, 1, firstTD.GetRuchka((short)Stvorka.Активная, 1));
                            }
                        }
                        else return;
                        break;
                    case "ruchka_3":
                        if (cons.CompareKod(td.Kod, "ДМ") | cons.CompareKod(td.Kod, "ЛМ") | cons.CompareKod(td.Kod, "ОДЛ"))
                        {
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Ручка_3, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.SetRuchka((short)Stvorka.Пассивная, 0, firstTD.GetRuchka((short)Stvorka.Пассивная, 0));
                            }
                        }
                        else return;
                        break;
                    case "ruchka_4":
                        if (cons.CompareKod(td.Kod, "ДМ") | cons.CompareKod(td.Kod, "ЛМ") | cons.CompareKod(td.Kod, "ОДЛ"))
                        {
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Ручка_4, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.SetRuchka((short)Stvorka.Пассивная, 1, firstTD.GetRuchka((short)Stvorka.Пассивная, 1));
                            }
                        }
                        else return;
                        break;
                    case "zadvizhka":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Задвижка, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Zadvizhka = firstTD.Zadvizhka;
                        }
                        break;
                    case "glazok":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Глазок, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Glazok = firstTD.Glazok;
                        }
                        break;
                    case "dobor":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Добор, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.Dobor = firstTD.Dobor;
                        }
                        break;
                    case "okno_1":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Окно_1, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetOkno(0, firstTD.GetOkno(0));
                        }
                        break;
                    case "okno_2":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Окно_2, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetOkno(1, firstTD.GetOkno(1));
                        }
                        break;
                    case "okno_3":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Окно_3, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetOkno(2, firstTD.GetOkno(2));
                        }
                        break;
                    case "okno_4":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Окно_4, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetOkno(3, firstTD.GetOkno(3));
                        }
                        break;
                    case "resh_1":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Решетка_1, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetResh(0, firstTD.GetResh(0));
                        }
                        break;
                    case "resh_2":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Решетка_2, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetResh(1, firstTD.GetResh(1));
                        }
                        break;
                    case "resh_3":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Решетка_3, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetResh(2, firstTD.GetResh(2));
                        }
                        break;
                    case "resh_4":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Решетка_4, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetResh(3, firstTD.GetResh(3));
                        }
                        break;
                    case "kalit":
                        if (cons.CompareKod(td.Kod, "ВМк"))
                        {
                            if (i == 0)
                                mf = new MessForm(MessFormVar.Калитка, ref td);
                            else
                            {
                                if (firstTD is null) break;
                                td.KalitParam = firstTD.KalitParam;
                            }
                        }
                        else
                            return;
                        break;
                    case "fr_0":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Фрамуга_верх, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetFramuga(0, firstTD.GetFramuga(0));
                        }
                        break;
                    case "fr_3":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Фрамуга_лев, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetFramuga(3, firstTD.GetFramuga(3));
                        }
                        break;
                    case "fr_2":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Фрамуга_прав, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetFramuga(2, firstTD.GetFramuga(2));
                        }
                        break;
                    case "fr_1":
                        if (i == 0)
                            mf = new MessForm(MessFormVar.Фрамуга_низ, ref td);
                        else
                        {
                            if (firstTD is null) break;
                            td.SetFramuga(1, firstTD.GetFramuga(1));
                        }
                        break;
                    case "comments":
                        return;
                    case "err":
                        return;
                }

                if (i == 0)
                {
                    if (mf.ShowDialog() == DialogResult.Yes)
                    {
                        tDatas[GetDataIndexByNum(tDatas, nums[i])] = td;
                        firstTD = td;
                    }
                }
                else
                    tDatas[GetDataIndexByNum(tDatas, nums[i])] = td;

                FillRow(SearchRowByNum(nums[i]), td);
            }
        }
    }

    //События фильтра
    private void dgvInFiles_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        Dictionary<string, bool> datas = new Dictionary<string, bool>();
        gbFiltres.Controls.Clear();
        foreach (DataGridViewRow row in dgvInFiles.Rows) 
            if (!datas.ContainsKey(row.Cells[e.ColumnIndex].Value.ToString())) 
                datas.Add(row.Cells[e.ColumnIndex].Value.ToString(), row.Visible);
        FilterList filter = new FilterList(e.ColumnIndex, datas);
        filter.FilterApp += new FilterAppHandler(FilterApp);
        filter.FilterDrop += new FilterDropHandler(FilterDrop);
        filter.Width = gbFiltres.Width - 12;
        filter.Height = gbFiltres.Height - 26;
        filter.Top = 20;
        filter.Left = 6;
        gbFiltres.Controls.Add(filter);
    }
    private void FilterApp(int owner, Dictionary<string, bool> DataSource)
    {
        gbFiltres.Controls.Clear();
        dgvInFiles.EnableHeadersVisualStyles = false;
        defColor = dgvInFiles.Columns[owner].HeaderCell.Style.BackColor;
        dgvInFiles.Columns[owner].HeaderCell.Style.BackColor = fColor;
        for(int i=0; i<dgvInFiles.Rows.Count; i++)
        {
            if (DataSource.TryGetValue(dgvInFiles.Rows[i].Cells[owner].Value.ToString(), out bool visible))
                dgvInFiles.Rows[i].Visible = visible;
        }
        if (!filterApp) filterApp = true;
    }
    private void FilterDrop()
    {
        if (filterApp)
        {
            filterApp = false;
            for (int i = 0; i < dgvInFiles.Columns.Count; i++)
                dgvInFiles.Columns[i].HeaderCell.Style.BackColor = defColor;
            for (int i = 0; i < dgvInFiles.Rows.Count; i++)
                dgvInFiles.Rows[i].Visible = true;
        }
    }

    //События меню (Файл)
    private void loadKonstr_Click(object sender, EventArgs e)
    {
        ScanFile(); 
    }
    private void scanKonstr_Click(object sender, EventArgs e)
    {
        ScanTable();
    }
    //---------------------
    private void saveInFile_Click(object sender, EventArgs e)
    {
        Save();
    }
    private void dosaveInFile_Click(object sender, EventArgs e)
    {

    }
    private void loadFromFile_Click(object sender, EventArgs e)
    {
        dgvInFiles.Rows.Clear();
        tDatas.Clear();
        LoadSaveFile();
    }
    private void doloadFromFile_Click(object sender, EventArgs e)
    {
        LoadSaveFile();
    }
    //---------------------
    private void openFileWindow_Click(object sender, EventArgs e)
    {
        Hide();
        Program.workFilesForm.LoadTable();
        Program.workFilesForm.Show();
    }

    //События меню (Выполнить)
    private void mmRun_Click(object sender, EventArgs e)
    {
        Run(runCommand.Общий);
    }
    private void mmAllDM_Click(object sender, EventArgs e)
    {
        Run(runCommand.Стойки_Полотна);
    }
    private void mmStoykiDM_Click(object sender, EventArgs e)
    {
        Run(runCommand.Стойки);
    }
    //---------------------
    private void mmCompKV_Click(object sender, EventArgs e)
    {
        LayoutFiles();
    }

    //События меню (Добавить)
    private void mmAddCtion_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem item = (ToolStripMenuItem)sender;
        TableData tData = new TableData();
        addNum++;
        switch (item.Text)
        {
            case "ДМ-1":
                tData.ReadDef("DM1_Default");
                tData.Num = addNum.ToString();
                break;
            case "ДМ-2":
                tData.ReadDef("DM2_Default");
                tData.Num = addNum.ToString();
                break;
            case "ЛМ-1":
                tData.ReadDef("LM1_Default");
                tData.Num = addNum.ToString();
                break;
            case "ЛМ-2":
                tData.ReadDef("LM2_Default");
                tData.Num = addNum.ToString();
                break;
            case "ВМ-1":
                tData.ReadDef("VM1_Default");
                tData.Num = addNum.ToString();
                break;
            case "ВМ-2":
                tData.ReadDef("VM2_Default");
                tData.Num = addNum.ToString();
                break;
            case "ВМк":
                tData.ReadDef("VMk_Default");
                tData.Num = addNum.ToString();
                break;
            case "ОДЛ-1":
                tData.ReadDef("ODL1_Default");
                tData.Num = addNum.ToString();
                break;
            case "ОДЛ-2":
                tData.ReadDef("ODL2_Default");
                tData.Num = addNum.ToString();
                break;
        }
        tDatas.Add(tData);
        LoadTable(false);
    }
    //---------------------
    private void mmDel_Click(object sender, EventArgs e)
    {
        foreach(DataGridViewRow row in dgvInFiles.SelectedRows)
        {
            tDatas.Remove(SearchByNum(tDatas, row.Cells[num.Index].Value.ToString()));
            dgvInFiles.Rows.Remove(row);
            lFind.Text = tDatas.Count.ToString();
            dgvInFiles.Refresh();
        }
    }

    //События меню (Настройки)
    private void mmSettings_Click(object sender, EventArgs e)
    {
        Settings settings = new Settings();
        settings.Show();
    }
    //---------------------

}

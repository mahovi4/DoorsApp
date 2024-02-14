using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

public class ExcelSkaner
{
    private Excel.Application Ex;
    private Excel.Workbook Kons;
    private Excel.Worksheet WSK;
    private int LastRow;
    private readonly Constants cons = new Constants();

    public List<TableData> Scan
    {
        get
        {
            if (Interaction.GetObject(Class: "Excel.Application") == null)
            {
                Interaction.MsgBox("Excel не запущен!");
                return null;
            }
            else
            {
                Ex = (Excel.Application)Interaction.GetObject(Class: "Excel.Application");
                Ex.ScreenUpdating = false;
            }

            Kons = SearchBook();
            if (Kons == null)
            {
                Interaction.MsgBox("Таблица конструирования не открыта!");
                return null;
            }
            else
            {
                WSK = SearchSheet();
            }

            return SkanTable();
        }
    }
    public List<TableData> Load(string filePath)
    {
        List<TableData> tds;
        Ex = new Excel.Application
        { 
            ScreenUpdating = false,
            Visible = false
        };
        Kons = Ex.Workbooks.Open(filePath);
        //Kons = Ex.Workbooks.Add(filePath);
        WSK = SearchSheet();
        tds = SkanTable();
        // Kons.Save()
        Kons.Close(false);
        Ex.Quit();
        WSK = null;
        Kons = null;
        Ex = null;
        return tds;
    }

    public TableData ScanRow(int row)
    {
        var dataColumns = new string[Enum.GetNames(typeof(CollNames)).Length];

        var param = new TableData();

        // Заполнение массива значений ячеек сканируемой строки по столбцам ---------------------------------------------------------------------
        for (int i = 0; i < dataColumns.Length; i++) dataColumns[i] = CellValue(row, i + 1);

        // Запись номера строки таблицы----------------------------------------------------------------------------------------------------------
        param.Row = row;

        // Запись номера изделия-----------------------------------------------------------------------------------------------------------------
        param.Num = dataColumns[(int)CollNames.Номер_Изделия];

        // Запись номера заказа------------------------------------------------------------------------------------------------------------------
        var er = dataColumns[(int)CollNames.Номер_Заказа];
        if (string.IsNullOrEmpty(er))
            param.ER = "ER-0";
        else
        {
            var erArr = er.Split('-');
            er = "ER-" + erArr[1];
            param.ER = er;
        }

        // Запись высоты изделия-----------------------------------------------------------------------------------------------------------------
        param.Height = short.TryParse(dataColumns[(int)CollNames.Высота], out var height) 
            ? height 
            : (short) 0;

        // Запись ширины изделия-----------------------------------------------------------------------------------------------------------------
        param.Width = short.TryParse(dataColumns[(int)CollNames.Ширина], out var width) 
            ? width 
            : (short) 0;

        // Запись кода изделия-------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Код_Изделия].Equals("") | dataColumns[(int)CollNames.Код_Изделия].Equals("-")) param.Kod = 0;
        else if (short.TryParse(dataColumns[(int)CollNames.Код_Изделия], out var kod)) param.Kod = kod;
        else param.Kod = 0;

        //Проверка на толщину полотна 62 мм------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("ТОЛЩИНА ПОЛОТНА 62") >= 0 |
            dataColumns[(int)CollNames.Комментарий].IndexOf("ПОЛОТНА 62 мм") >= 0 |
            dataColumns[(int)CollNames.Комментарий].IndexOf("ПОЛОТНО 62 мм") >= 0)
        {
            if (param.Kod == 70) param.Kod = 72;
            else if (param.Kod == 73) param.Kod = 75;
            else if (param.Kod == 16) param.Kod = 18;
            else if (param.Kod == 17) param.Kod = 19;
        }

        // ST-------------------------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Атрибут].IndexOf("(ST)") >= 0) 
            param.ST = true;

        // ДПМ------------------------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Атрибут].IndexOf("ДПМ") >= 0 |
            dataColumns[(int)CollNames.Атрибут].IndexOf("ДПМО") >= 0) 
            param.DPM = true;

        // EIW или EIWS--------------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int) CollNames.Атрибут].IndexOf("EIWS") >= 0)
        {
            param.EIWS = true;
            param.Termoblock = true;
        }
        else if (dataColumns[(int) CollNames.Атрибут].IndexOf("EIW") >= 0)
        {
            param.EIW = true;
            param.Termoblock = true;
        }
        // EIS или EI----------------------------------------------------------------------------------------------------------------------------
        else if (dataColumns[(int)CollNames.Атрибут].IndexOf("EIS") >= 0 |
                 dataColumns[(int)CollNames.Комментарий].IndexOf("Дополнительный контур уплотнителя") >= 0)
        {
            param.EIS = true;
            param.EI = false;
        }
        else if(dataColumns[(int)CollNames.Атрибут].IndexOf("EI") > 0)
        {
            if (param.DPM)
            {
                param.EI = true;
                param.EIS = false;
            }
        }
        

        // Обработка стороны открывания----------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Сторона_Открывания].IndexOf("Левое") >= 0)
        {
            param.Otkrivanie = Otkrivanie.Левое;
            param.OtkrFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Сторона_Открывания].IndexOf("Правое") >= 0)
        {
            param.Otkrivanie = Otkrivanie.Правое;
            param.OtkrFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Сторона_Открывания].IndexOf("Лево вовнутрь") >= 0)
        {
            param.Otkrivanie = Otkrivanie.ЛевоеВО;
            param.OtkrFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Сторона_Открывания].IndexOf("Право вовнутрь") >= 0)
        {
            param.Otkrivanie = Otkrivanie.ПравоеВО;
            param.OtkrFromTable = "_";
        }
        else
        {
            param.Otkrivanie = Otkrivanie.Левое;
            param.OtkrFromTable = dataColumns[(int)CollNames.Сторона_Открывания];
        }

        // Обработка вариантов порога------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Порог].IndexOf("Порог высотой 14 мм") >= 0 |
            dataColumns[(int)CollNames.Порог].IndexOf("14") >= 0 |
            dataColumns[(int)CollNames.Порог].IndexOf("Порог 14") >= 0)
        {
            param.Porog = (short)PorogNames.Порог_14;
            param.PorogFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Выпадающий") >= 0 | dataColumns[(int)CollNames.Порог].IndexOf("выпадающий") >= 0)
        {
            if (dataColumns[(int)CollNames.Порог].IndexOf("Накладной") > 0 | dataColumns[(int)CollNames.Порог].IndexOf("НАКЛАДНОЙ") > 0)
            {
                param.Porog = (short)PorogNames.Нет;
                param.PorogFromTable = "_";
            }
            else
            {
                param.Porog = (short)PorogNames.Выподающий;
                param.PorogFromTable = "_";
            }
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Порог высотой 25 мм (со скосом)") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("25 (со скосом)") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("Порог 25 (со скосом)") >= 0)
        {
            param.Porog = (short)PorogNames.Порог_25_скос;
            param.PorogFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Порог высотой 25 мм") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("25") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("Порог 25") >= 0)
        {
            param.Porog = (short)PorogNames.Порог_25;
            param.PorogFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Съемный порог") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("Съёмный порог") >= 0)
        {
            param.Porog = (short)PorogNames.Порог_40_анкер;
            param.PorogFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Порог высотой 100 мм") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("100") >= 0)
        {
            param.Porog = (short)PorogNames.Порог_100;
            param.PorogFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Нет") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("монтажный порог") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("Плоский порог") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("0") >= 0 |
                 dataColumns[(int)CollNames.Порог].Equals("-") |
                 dataColumns[(int)CollNames.Порог].Equals("-----") |
                 dataColumns[(int)CollNames.Порог].Equals(""))
        {
            param.Porog = (short)PorogNames.Нет;
            param.PorogFromTable = "_";
        }
        else if(dataColumns[(int)CollNames.Порог].Equals("Порог нестандартный 1.2 (смотри чертёж)") | 
                dataColumns[(int)CollNames.Порог].Equals("Порог нестандартный 1.4 (смотри чертёж)"))
        {
            param.Porog = (short)PorogNames.Порог_30_шип_паз;
            param.PorogFromTable = dataColumns[(int)CollNames.Порог];
        }
        else if (dataColumns[(int)CollNames.Порог].Equals("1") |
                 dataColumns[(int)CollNames.Порог].IndexOf("40") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("стандарт") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("Стандарт") >= 0 |
                 dataColumns[(int)CollNames.Порог].IndexOf("НАКЛАДНОЙ порог") >= 0)
        {
            if (param.Kod == 8 | param.Kod == 9)
            {
                if(param.Otkrivanie == Otkrivanie.Левое || param.Otkrivanie == Otkrivanie.Правое)
                    param.Porog = (short)PorogNames.Порог_20_мм;
                else
                    param.Porog = (short)PorogNames.Порог_25;
                param.PorogFromTable = "_";
            }
            else
            {
                param.Porog = (short)PorogNames.Порог_40;
                param.PorogFromTable = "_";
            }
        }
        else if (dataColumns[(int)CollNames.Порог].IndexOf("Порог не стандартный") >= 0)
        {
            param.Porog = 1000;
            param.PorogFromTable = "_";
        }
        else
        {
            param.Porog = (short)PorogNames.Нет;
            param.PorogFromTable = dataColumns[(int)CollNames.Порог];
        }

        // Запись наличников---------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Наличники].IndexOf(":") < 0) {
            if (dataColumns[(int)CollNames.Наличники].Equals("Есть")) {
                if (param.Kod == 16 | param.Kod == 17 | param.Kod == 18 | param.Kod == 19) {
                    param.Nalichniki[(int)Raspolozhenie.Верх] = 60;
                    param.Nalichniki[(int)Raspolozhenie.Ниж] = 60;
                    param.Nalichniki[(int)Raspolozhenie.Прав] = 60;
                    param.Nalichniki[(int)Raspolozhenie.Лев] = 60;
                }
                else if (param.Otkrivanie == Otkrivanie.Левое | param.Otkrivanie == Otkrivanie.Правое) {
                    param.Nalichniki[(int)Raspolozhenie.Верх] = 60;
                    param.Nalichniki[(int)Raspolozhenie.Ниж] = 0;
                    param.Nalichniki[(int)Raspolozhenie.Прав] = 60;
                    param.Nalichniki[(int)Raspolozhenie.Лев] = 60;
                }
                else if (param.Otkrivanie == Otkrivanie.ЛевоеВО | param.Otkrivanie == Otkrivanie.ПравоеВО) {
                    param.Nalichniki[(int)Raspolozhenie.Верх] = 50;
                    param.Nalichniki[(int)Raspolozhenie.Ниж] = 0;
                    param.Nalichniki[(int)Raspolozhenie.Прав] = 50;
                    param.Nalichniki[(int)Raspolozhenie.Лев] = 50;
                }
            }
            else if (dataColumns[(int)CollNames.Наличники].Equals("нет")) {
                param.Nalichniki[(int)Raspolozhenie.Верх] = 0;
                param.Nalichniki[(int)Raspolozhenie.Ниж] = 0;
                param.Nalichniki[(int)Raspolozhenie.Прав] = 0;
                param.Nalichniki[(int)Raspolozhenie.Лев] = 0;
            }
        }
        else
        {
            string Str;
            string[] NalSubStrings;
            Str = dataColumns[(int)CollNames.Наличники].Replace('\n', ';');
            Str = Str.Replace("Сверху: ", "");
            Str = Str.Replace("Снизу: ", "");
            Str = Str.Replace("Правый: ", "");
            Str = Str.Replace("Левый: ", "");
            NalSubStrings = Str.Split(';');
            for (int i = 0; i < 4; i++)
            {
                if (NalSubStrings[i].Equals("нет")) {
                    param.Nalichniki[i] = 0;
                }
                else {
                    param.Nalichniki[i] = short.Parse(NalSubStrings[i]);
                }
            }
        }

        // Параметры активной створки------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Ширина_Активки].Equals("") | dataColumns[(int)CollNames.Ширина_Активки].Equals("-"))
        {
            param.WAktiv = 0d;
            param.WAktivFromTable = "_";
        }
        else if (dataColumns[(int)CollNames.Ширина_Активки].IndexOf("Равно") >= 0)
        {
            param.WAktiv = 1d;
            param.WAktivFromTable = "_";
        }
        else if (double.TryParse(dataColumns[(int)CollNames.Ширина_Активки], out var wa))
        {
            param.WAktiv = wa;
            param.WAktivFromTable = "_";
        }
        else
        {
            param.WAktiv = 0d;
            param.WAktivFromTable = dataColumns[(int)CollNames.Ширина_Активки];
        }

        // Параметры калитки--------------------------------------------------------------------------------------------------------------------
        if (param.Kod == 42 | dataColumns[(int)CollNames.Комментарий].IndexOf("калитка") >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Калитка") >= 0) {
            param.Kalit = true;
            param.KalitFromTable = dataColumns[(int)CollNames.Комментарий] + '\n' + dataColumns[(int)CollNames.Атрибут];
        }
        else param.Kalit = false;

        // Разборная коробка--------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("РАЗБОРНАЯ КОРОБКА") >= 0) param.Razbor = true;
        else param.Razbor = false;

        // Наличие анкеров в макушке-------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Сделать монтажные отверстия в притолоке") >= 0) param.AMak = true;
        else param.AMak = false;

        // Замки и др. фурнитура-----------------------------------------------------------------------------------------------------------------
        // Обработка вариантов нижнего замка
        if (string.IsNullOrEmpty(dataColumns[(int)CollNames.Замок_1_АС])) param.Zamok[0].Kod = (int)ZamokNames.Нет;
        else {
            if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Без пробивки под замок") >= 0) param.Zamok[0].Kod = (int)ZamokNames.Нет;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Пробивка под противопожарный замок") >= 0 |
                        dataColumns[(int)CollNames.Замок_1_АС].Equals("Замок противопожарный") |
                        dataColumns[(int)CollNames.Замок_1_АС].Equals("Замок врезной цилиндровый FL-0433 ANTI-PANIC Fuaro") |
                        dataColumns[(int)CollNames.Замок_1_АС].IndexOf("антипаник 1901/65mm") >= 0 |
                        dataColumns[(int)CollNames.Замок_1_АС].Equals("Замок врезной цилиндровый FL-0432 Fuaro") |
                        dataColumns[(int)CollNames.Замок_1_АС].IndexOf("LB-72 Panic") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.ПП;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ4-31/55") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Просам_ЗВ_4;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("900 3MR") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Фуаро_900;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ8-8У/13") >= 0 |
                        dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ8-4/13") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Просам_ЗВ_8;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("842") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Меттем_842;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("12.11 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_12_11;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("30.01 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_30_01;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Роликовый фиксатор R-0002 CR Apecs") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Apecs_R_0002_CR;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Apecs T-52") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Apecs_T_52;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ 8-6 ЛК5/15") >= 0 |
                     dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ 8-6 ПК5/15") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Бордер_ЗВ8_6;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("32.11 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_32_11;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("10.01 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_10_01;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("12.01 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_12_01;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("ЗВ Гардиан 35.11Р.24") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_35_11;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("30.11 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_30_11;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Замок Кодовый") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Нет;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("32.01 Гардиан") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Гардиан_32_01;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Apecs 30-R") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Apecs_30_R;
            else if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("КРИТ ЗВ - 7 РМП") >= 0)
                param.Zamok[0].Kod = (int)ZamokNames.Crit_7_RPM;
            else if (dataColumns[(int) CollNames.Замок_1_АС].IndexOf("ECO GBS 81") >= 0)
                param.Zamok[0].Kod = (int) ZamokNames.ECO_GBS_81;
            else if (dataColumns[(int) CollNames.Замок_1_АС].IndexOf("Пробивка под замок клиента (смотри примечание)") >= 0 &&
                     dataColumns[(int) CollNames.Замок_1_АС].IndexOf("Пробивка под замок клиента (смотри вложение)") >= 0)
                param.Zamok[0].Kod = (int) ZamokNames.Почтовый;
            else
            {
                param.Zamok[0].Kod = (int)ZamokNames.Нет;
                param.Zamok[0].FromTable = dataColumns[(int)CollNames.Замок_1_АС] + '\n' +
                                            dataColumns[(int)CollNames.Цилиндр_1_АС] + '\n' +
                                            dataColumns[(int)CollNames.Ручка_1_АС] + '\n' +
                                            dataColumns[(int)CollNames.Атрибут] + '\n' +
                                            dataColumns[(int)CollNames.Комментарий];
            }
            if (string.IsNullOrEmpty(dataColumns[(int)CollNames.Цилиндр_1_АС]) | dataColumns[(int)CollNames.Цилиндр_1_АС].IndexOf("Без пробивки под цилиндр") >= 0)
                param.Zamok[0].Cilinder = false;
            else
                param.Zamok[0].Cilinder = true;
            //if (string.IsNullOrEmpty(dataColumns[(int)CollNames.Ручка_1_АС]) | dataColumns[(int)CollNames.Ручка_1_АС].IndexOf("Без пробивки под ручку") >= 0)
            //    param.Zamok[0].Ruchka = false;
            if (dataColumns[(int)CollNames.Комментарий].IndexOf("Электромеханическая защелка") >= 0) 
            {
                param.Zamok[0].FromTable += '\n' + dataColumns[(int)CollNames.Комментарий];
                param.Zamok[0].ElMehZashelka = true;
            }
        }

        // Обработка вариантов ручек
        for(int i=0; i<2; i++)
        {
            for(int y=0; y<2; y++)
            {
                string ruchka;
                if(i == (int)Stvorka.Активная)
                {
                    if (y == 0)
                        ruchka = dataColumns[(int)CollNames.Ручка_1_АС];
                    else
                        ruchka = dataColumns[(int)CollNames.Ручка_2_АС];
                }
                else
                {
                    if (y == 0)
                        ruchka = dataColumns[(int)CollNames.Ручка_1_ПС];
                    else
                        ruchka = dataColumns[(int)CollNames.Ручка_2_ПС];
                }
                // Скоба
                if (ruchka.IndexOf("Ручка-скоба") >= 0 | ruchka.IndexOf("Ручку-скобу") >= 0 |
                   ruchka.IndexOf("ручку-скобу") >= 0 | ruchka.IndexOf("Ручку скобу") >= 0)
                {
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_скоба);
                    RuchkaParam rp = param.GetRuchka((short)i, (short)y);
                    if (ruchka.IndexOf("1300") >= 0)
                    {
                        rp.Mezhosevoe = 1300;
                        rp.FromTable = "_";
                    }
                    else if (ruchka.IndexOf("300") >= 0)
                    {
                        rp.Mezhosevoe = 300;
                        rp.FromTable = "_";
                    }
                    else
                    {
                        rp.Mezhosevoe = 0;
                        rp.FromTable = ruchka;
                    }
                    param.SetRuchka((short)i, (short)y, rp);
                }
                // Кнопка
                else if (ruchka.IndexOf("Ручка кнопка") >= 0 | ruchka.IndexOf("РДК-110") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_кнопка);
                // РЯ-108
                else if (ruchka.IndexOf("РЯ-108") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_РЯ_180);
                // Ручка потайная 70х120
                else if (ruchka.IndexOf("Ручка потайная 70х120") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_Потайная);
                // Pучка "Вега"
                else if (ruchka.IndexOf("Вега") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_Вега);
                // Pучка на планке Просам
                else if ((ruchka.IndexOf("Ручка в комплекте с замком") >= 0 & param.Zamok[0].Kod == (int)ZamokNames.Просам_ЗВ_4) ||
                    ruchka.IndexOf("Ручка на планке Просам") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_планка_Просам);
                // Pучка на планке
                else if (ruchka.IndexOf("на планке") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_черная_планка);
                // Ручка-Уголок (самодельная)
                else if (ruchka.IndexOf("Ручка-уголок") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_уголок);
                // Pучка на фланце
                else if (ruchka.IndexOf("на фланце") >= 0)
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Ручка_фланец);
                else
                {
                    param.SetRuchkaKod((short)i, (short)y, (short)RuchkaNames.Нет);
                    param.SetRuchkaFromTable((short)i, (short)y, ruchka);
                }
            }
            string ap;
            if (i == (int)Stvorka.Активная)
                ap = dataColumns[(int)CollNames.Антипаника_АС];
            else
                ap = dataColumns[(int)CollNames.Антипаника_ПС];
            if (!string.IsNullOrEmpty(ap))
            {
                if (ap.IndexOf("PB-1300") >= 0)
                    param.SetRuchkaKod((short)i, 0, (short)RuchkaNames.PB_1300);
                else if (ap.IndexOf("PB-1700 C") >= 0 || ap.IndexOf("PB-1700 с тягами") >= 0)
                    param.SetRuchkaKod((short)i, 0, (short)RuchkaNames.PB_1700C);
                else if (ap.IndexOf("PB-1700") >= 0)
                    param.SetRuchkaKod((short)i, 0, (short)RuchkaNames.PB_1700A);
                else if (ap.IndexOf("DL") >= 0)
                    param.SetRuchkaKod((short)i, 0, (short)RuchkaNames.АП_DoorLock);
            }
        }
        

        // Обработка вариантов верхнего замка
        if (!dataColumns[(int)CollNames.Замок_2_АС].Equals(""))
        {
            if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("ЗВ8-8У/13") >= 0 | 
                dataColumns[(int)CollNames.Замок_2_АС].IndexOf("ЗВ8-4/13") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Просам_ЗВ_8;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("ЗВ8 842") >= 0 | 
                     dataColumns[(int)CollNames.Замок_2_АС].IndexOf("842.0.0") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Меттем_842;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("30.01 Гардиан") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Гардиан_30_01;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("10.01 Гардиан") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Гардиан_10_01;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("ЗВ 8-6 ЛК5/15") >= 0 | 
                     dataColumns[(int)CollNames.Замок_2_АС].IndexOf("ЗВ 8-6 ПК5/15") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Бордер_ЗВ8_6;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("12.01 Гардиан") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Гардиан_12_01;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("50.01 Гардиан") >= 0)
                param.Zamok[1].Kod = (int)ZamokNames.Гардиан_50_01;
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("Замок Кодовый") >= 0) 
                param.Zamok[1].Kod = (int)ZamokNames.Нет;
            else 
            {
                param.Zamok[1].Kod = (int)ZamokNames.Нет;
                param.Zamok[1].FromTable = dataColumns[(int)CollNames.Замок_2_АС] + '\n' + WSK.Cells[row, 16].value;
            }
            if (string.IsNullOrEmpty(dataColumns[(int)CollNames.Цилиндр_2_АС]) | dataColumns[(int)CollNames.Цилиндр_2_АС].IndexOf("Без пробивки под цилиндр") >= 0)
                param.Zamok[1].Cilinder = false;
            else
                param.Zamok[1].Cilinder = true;
            //if (string.IsNullOrEmpty(dataColumns[(int)CollNames.Ручка_2_АС]) | dataColumns[(int)CollNames.Ручка_2_АС].IndexOf("Без пробивки под ручку") >= 0)
            //    param.Zamok[1].Ruchka = false;
        }
        else param.Zamok[1].Kod = (int)ZamokNames.Нет;

        // Обработка вариантов кодового замка
        KodoviyParam kp = param.Kodoviy;
        if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Замок Кодовый") >= 0 | 
            dataColumns[(int)CollNames.Замок_2_АС].IndexOf("Замок Кодовый") >= 0 | 
            dataColumns[(int)CollNames.Замок_1_ПС].IndexOf("Замок Кодовый") >= 0)
        {
            kp.Type = Kodoviy.Врезной_кнопки_на_лице;
            if (dataColumns[(int)CollNames.Замок_1_АС].IndexOf("Замок Кодовый") >= 0) kp.FromTable = dataColumns[(int)CollNames.Замок_1_АС];
            else if (dataColumns[(int)CollNames.Замок_2_АС].IndexOf("Замок Кодовый") >= 0) kp.FromTable = dataColumns[(int)CollNames.Замок_2_АС];
            else if (dataColumns[(int)CollNames.Замок_1_ПС].IndexOf("Замок Кодовый") >= 0) kp.FromTable = dataColumns[(int)CollNames.Замок_1_ПС];
            param.Kodoviy = kp;
        }

        // Наличие глазка
        if (dataColumns[(int)CollNames.Глазок].IndexOf("Глазок") >= 0) 
            param.AddGlazok = new GlazokParam{ OtPola = 0, Raspolozhenie = GlazokRaspolozhenie.По_центру };

        // Обработка вариантов задвижки
        ZadvizhkaParam zad = param.Zadvizhka;
        if (dataColumns[(int)CollNames.Задвижки].IndexOf("Ночной сторож") >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Ночной сторож") >= 0) 
            zad.Kod = (int)ZadvizhkaNames.Ночной_сторож;
        else if (dataColumns[(int)CollNames.Задвижки].IndexOf("ЗД-01") >= 0 | 
                 dataColumns[(int)CollNames.Комментарий].IndexOf("ЗД-01") >= 0) 
            zad.Kod = (int)ZadvizhkaNames.ЗД_01;
        else if (dataColumns[(int) CollNames.Задвижки].IndexOf("ЗТ-150") >= 0)
        {
            var strCol = dataColumns[(int) CollNames.Задвижки];
            var strArr = strCol.Split('\n');
            if (strArr.Length > 0)
            {
                var count = strArr
                    .Count(zash => zash.IndexOf("ЗТ-150") >= 0);

                if(count == 1 && param.Porog != (short)PorogNames.Выподающий)
                    zad.Kod = (int)ZadvizhkaNames.ЗТ_150;
                else if (count > 1 && param.Porog == (short) PorogNames.Выподающий)
                    zad.Kod = (int) ZadvizhkaNames.ЗТ_150;
                else
                    zad.FromTable = $"Ни хрена не понял!!!\n{strCol}";
            }
        }
        //else if (dataColls[(int)CollNames.Задвижки].IndexOf("Торцевой шпингалет") >= 0) 
        //    zad.Kod = (int)ZadvizhkaNames.Торцевой_шпингалет;
        else {
            zad.Kod = (int)ZadvizhkaNames.Нет;
            zad.FromTable = dataColumns[(int)CollNames.Задвижки] + '\n' + dataColumns[(int)CollNames.Комментарий];
        }
        param.Zadvizhka = zad;

        //Наличие сдвигового замка
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Aler Al250") >= 0)
            param.SdvigoviyZamok = new SdvigoviyParam
            {
                Kod = (short)SdvigoviyNames.Aler_AL250,
                Name = SdvigoviyNames.Aler_AL250.ToString()
            };

        //Наличие броненакладки
        if (dataColumns[(int) CollNames.Комментарий].IndexOf("Броненакладка накладная DOORLOCK DEF5512") >= 0 || 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Броненакладка врез.Fuaro с чашкой DEF 9726") >= 0 ||
            dataColumns[(int)CollNames.Комментарий].IndexOf("Броненакладка врезная APECS PRO 50/27-DP-15-BLM") >= 0)
            param.Bronya = true;

        //Наличие лицевой МДФ панели
        if (dataColumns[(int) CollNames.Атрибут].IndexOf("ДМ-1 Комфорт Р (Премиум)") >= 0 ||
            cons.CompareKod(param.Kod, "КВ06") || cons.CompareKod(param.Kod, "Жардин"))
            param.LicPanel = true;

        //Наличие зеркала во внутренней панели
        if (cons.CompareKod(param.Kod, "КВ06") &&
            (dataColumns[(int) CollNames.Атрибут].IndexOf("ДК-1 ЕЛЕНА Зеркало") >= 0 ||
             dataColumns[(int) CollNames.Атрибут].IndexOf("ДК-1 АЙСБЕРГ Зеркало") >= 0))
            param.Zerkalo = true;

        // Количество противосъемников
        if (param.ST | dataColumns[(int)CollNames.Комментарий].IndexOf("2 противосъемных ригеля") >= 0 | 
                       dataColumns[(int)CollNames.Комментарий].IndexOf("Дополнительный противосъемный штырь") >= 0)  
            param.Protivos = 2;
        else param.Protivos = 1;

        // Наличие торцевых шпингалетов
        if (dataColumns[(int)CollNames.Задвижки].IndexOf("БЕЗ ПРОБИВКИ ПОД ТОРЦЕВОЙ ШПИНГАЛЕТ") >= 0) 
            param.TSpingalet = false;
        else if (dataColumns[(int)CollNames.Задвижки].IndexOf("Торцевой шпингалет") >= 0) 
            param.TSpingalet = true;
        else param.TSpingalet = false;
        if (param.TSpingalet == false && dataColumns[(int) CollNames.Комментарий].IndexOf("по бланку согласования № 866") >= 0)
            param.NSasTS = true;

        // Вырезы под квадрат 8х8
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Усиливающий квадрат") >= 0)
        {
            if ((param.Kod == 2 | param.Kod == 70) & (param.Height > 2249 | param.Width > 1149))
                param.Kvadrat = VirezKvadrat._3_выреза;
            else if ((param.Kod == 4 | param.Kod == 73) & (param.Height > 2249 | param.Width > 1949))
                param.Kvadrat = VirezKvadrat._3_выреза;
            else 
                param.Kvadrat = VirezKvadrat._2_выреза;
        }
        else param.Kvadrat = VirezKvadrat.нет;

        // Наличие кабельперехода
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Кабельный переход EA281 Abloy", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Кабельпереход  EA281 Abloy", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("EA281 Abloy", StringComparison.Ordinal) >= 0) {
            if (param.WAktiv != 0) 
            {
                param.Kabel = KabelRaspolozhenie.в_пассивке;
                param.KabelFromTable = dataColumns[(int)CollNames.Замок_1_АС] + '\n' + dataColumns[(int)CollNames.Комментарий];
            }
            else param.Kabel = KabelRaspolozhenie.в_активке;
        }
        else param.Kabel = KabelRaspolozhenie.нет;

        // Наличие заземления
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Устройство заземления", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("заземления", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("заземление", StringComparison.Ordinal) >= 0) 
        {
            param.GND = GNDRaspolozhenie.только_на_стойке;
            param.GNDFromTable = dataColumns[(int)CollNames.Комментарий];
        }
        else param.GND = GNDRaspolozhenie.нет;

        // Опредиление толщин листов
        var str = dataColumns[(int)CollNames.Толщина_ЛЛ];

        if (double.TryParse(str, out var tll))
            param.Thick_LL = cons.IsValidListThick(tll) 
                ? tll 
                : 1.2d;
        else
        {
            if (str.IndexOf(".", StringComparison.Ordinal) > 0)
                str.Replace(".", ",");
            else
                str.Replace(",", ".");
            if(double.TryParse(str, out tll))
            {
                param.Thick_LL = cons.IsValidListThick(tll) 
                    ? tll 
                    : 1.2d;
            }
            else param.Thick_LL = 1.2d;
        }

        str = dataColumns[(int)CollNames.Толщина_ВЛ];
        if(dataColumns[(int)CollNames.Толщина_ВЛ].IndexOf("МДФ", StringComparison.Ordinal) >= 0)
            param.Thick_VL = 0;
        else
        {
            if (double.TryParse(str, out var tvl))
            {
                param.Thick_VL = cons.IsValidListThick(tvl) 
                    ? tvl 
                    : 1.2d;
            }
            else
            {
                if (str.IndexOf(".", StringComparison.Ordinal) > 0)
                    str.Replace(".", ",");
                else
                    str.Replace(",", ".");
                if (double.TryParse(str, out tvl))
                    param.Thick_VL = cons.IsValidListThick(tvl) 
                        ? tvl 
                        : 1.2d;
            }
        }

        // Нижний притвор по чертежу
        param.Pritvor = dataColumns[(int)CollNames.Комментарий]
            .IndexOf("Нижний притвор по чертежу", StringComparison.Ordinal) < 0;

        // Обработка параметров окон-------------------------------------------------------------------------------------------------------------
        for (var i = 0; i < 4; i++)
        {
            var sh = "";
            var sw = "";
            var spg = "";
            var spv = "";
            var sgr = "";
            var svr = "";
            switch (i)
            {
                case 0:
                    sh = dataColumns[(int)CollNames.Окно_1_Высота];
                    sw = dataColumns[(int)CollNames.Окно_1_Ширина];
                    spg = dataColumns[(int)CollNames.Окно_1_По_горизонтали];
                    spv = dataColumns[(int)CollNames.Окно_1_По_вертикали];
                    sgr = dataColumns[(int)CollNames.Окно_1_Гор_разположение];
                    svr = dataColumns[(int)CollNames.Окно_1_Верт_разположение];
                    break;
                case 1:
                    sh = dataColumns[(int)CollNames.Окно_2_Высота];
                    sw = dataColumns[(int)CollNames.Окно_2_Ширина];
                    spg = dataColumns[(int)CollNames.Окно_2_По_горизонтали];
                    spv = dataColumns[(int)CollNames.Окно_2_По_вертикали];
                    sgr = dataColumns[(int)CollNames.Окно_2_Гор_разположение];
                    svr = dataColumns[(int)CollNames.Окно_2_Верт_разположение];
                    break;
                case 2:
                    sh = dataColumns[(int)CollNames.Окно_3_Высота];
                    sw = dataColumns[(int)CollNames.Окно_3_Ширина];
                    spg = dataColumns[(int)CollNames.Окно_3_По_горизонтали];
                    spv = dataColumns[(int)CollNames.Окно_3_По_вертикали];
                    sgr = dataColumns[(int)CollNames.Окно_3_Гор_разположение];
                    svr = dataColumns[(int)CollNames.Окно_3_Верт_разположение];
                    break;
                case 3:
                    sh = dataColumns[(int)CollNames.Окно_4_Высота];
                    sw = dataColumns[(int)CollNames.Окно_4_Ширина];
                    spg = dataColumns[(int)CollNames.Окно_4_По_горизонтали];
                    spv = dataColumns[(int)CollNames.Окно_4_По_вертикали];
                    sgr = dataColumns[(int)CollNames.Окно_4_Гор_разположение];
                    svr = dataColumns[(int)CollNames.Окно_4_Верт_разположение];
                    break;
            }
            if (!string.IsNullOrEmpty(sh))
            {
                var op = param.GetOkno((short)i);
                op.Steklo.Height = short.TryParse(sh, out var h) 
                    ? h 
                    : (short) 0;
                op.Steklo.Width = short.TryParse(sw, out var w) 
                    ? w 
                    : (short) 0;
                op.Steklo.Thick = StekloThicks._24_мм;
                op.PoGorizontali = short.TryParse(spg, out var pg) 
                    ? pg 
                    : (short) 0;
                op.PoVertikali = short.TryParse(spv, out var pv) 
                    ? pv 
                    : (short) 0;
                if (sgr.IndexOf("От замкового края", StringComparison.Ordinal) >= 0)
                    op.GorRaspol = GorRaspolozhenie.от_замкового;
                else if (sgr.IndexOf("От петлевого края", StringComparison.Ordinal) >= 0)
                    op.GorRaspol = GorRaspolozhenie.от_петлевого;
                else
                    op.GorRaspol = GorRaspolozhenie.по_центру;
                op.VertRaspol = svr.IndexOf("От верха", StringComparison.Ordinal) >= 0 
                    ? VertRaspolozhenie.от_верха 
                    : VertRaspolozhenie.от_пола;
                param.SetOkno((short)i, op);
            }
        }
        
        // Обработка параметров Решеток----------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("решетку", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("решетка", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("решётка", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("решётку", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Решётка", StringComparison.Ordinal) >= 0) 
        {
            param.ReshFromTable = dataColumns[(int)CollNames.Комментарий];
        }

        param.ZResh = ZashResh.нет;
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Решётка оконная защитная на обе стороны", StringComparison.Ordinal) >= 0 || 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Решетка оконная защитная на обе стороны", StringComparison.Ordinal) >= 0) 
            param.ZResh = ZashResh.решетка_на_2стороны;

        // Обработка параметров добора-----------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("Добор", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Добор].IndexOf("Добор", StringComparison.Ordinal) >= 0)
        {
            param.Dobor = true;
            string strD;
            param.Dobor = true;
            param.DoborFromTable = dataColumns[(int)CollNames.Комментарий];
            if (dataColumns[(int)CollNames.Добор].IndexOf("Добор", StringComparison.Ordinal) >= 0) 
                param.DoborFromTable += "\n" + dataColumns[(int)CollNames.Добор];
            if (param.Kod == 21)
                param.Dobor_Nal = param.Nalichniki;
            else
            {
                var nal = new short[4];
                for(var i=0; i<4; i++)
                {
                    if (i == (int)Raspolozhenie.Ниж)
                        nal[i] = 0;
                    else
                        nal[i] = 60;
                }
                param.Dobor_Nal = nal;
            }
            if (dataColumns[(int)CollNames.Добор].IndexOf("Глубина", StringComparison.Ordinal) >= 0) {
                strD = Strings.Mid(
                    dataColumns[(int)CollNames.Добор], 
                    dataColumns[(int)CollNames.Добор].IndexOf("Глубина", StringComparison.Ordinal) + 9, 
                    4);
                if(short.TryParse(strD.Trim(), out var strd)) param.Dobor_Glub = strd;
            }
            else if (dataColumns[(int)CollNames.Комментарий].IndexOf("глубина добора", StringComparison.Ordinal) >= 0)
            {
                strD = Strings.Mid(
                    dataColumns[(int)CollNames.Комментарий], 
                    dataColumns[(int)CollNames.Комментарий].IndexOf("глубина добора", StringComparison.Ordinal) + 15, 
                    4);
                if(short.TryParse(strD.Trim(), out var strd)) param.Dobor_Glub = strd;
            }
        }
        else {
            param.Dobor = false;
            param.DoborParam = new DoborParam() { Glubina = 0, Nalicnik = new short[] { 0,0,0,0}, FromTable = "_" };
        }

        // Обработка параметров фрамуги----------------------------------------------------------------------------------------------------------
        // Верхняя
        FramugaParam fp;
        if (!string.IsNullOrEmpty(dataColumns[(int)CollNames.Фрамуга_Верх]) & !dataColumns[(int)CollNames.Фрамуга_Верх].Equals("-")) 
        {
            fp = param.GetFramugaPar((short)Raspolozhenie.Верх);
            if (dataColumns[(int)CollNames.Фрамуга_Верх].IndexOf("глухая", StringComparison.Ordinal) >= 0) 
                fp.Type = FramugaType.глухая;
            else if (dataColumns[(int)CollNames.Фрамуга_Верх].IndexOf("частично остеклённая", StringComparison.Ordinal) >= 0) 
                fp.Type = FramugaType.частичного_остекления;
            else if (dataColumns[(int)CollNames.Фрамуга_Верх].IndexOf("полностью остеклённая", StringComparison.Ordinal) >= 0) 
                fp.Type = FramugaType.полного_остекления;
            else 
                fp.Type = FramugaType.нет;
            // Dim StrF$
            // Dim FramSubStrings() As String
            // StrF = Mid(WSK.Cells(Row, 59), InStr(1, WSK.Cells(Row, 59).Value, "глухая", 1) + 7, 11)
            // StrF = Replace(StrF, Chr(32), "")
            // StrF = Replace(StrF, "С", "", vbTextCompare)
            // StrF = Replace(StrF, "т", "", vbTextCompare)
            // FramSubStrings = Split(StrF, "x")
            // Param.HFramugi = FramSubStrings(0)
            // Param.WFramugi = FramSubStrings(1)
            fp.Height = 0;
            fp.Width = 0;
            fp.FromTable = dataColumns[(int)CollNames.Фрамуга_Верх];
            param.SetFramugaPar((short)Raspolozhenie.Верх, fp);
        }
        else param.SetFramugaPar((short)Raspolozhenie.Верх, new FramugaParam() { Type = FramugaType.нет });

        // Левая
        if (!string.IsNullOrEmpty(dataColumns[(int)CollNames.Фрамуга_Лев]) & !dataColumns[(int)CollNames.Фрамуга_Лев].Equals("-")) {
            fp = param.GetFramugaPar((short)Raspolozhenie.Лев);
            var strTmp = "";
            if (dataColumns[(int)CollNames.Фрамуга_Лев].IndexOf("глухая", StringComparison.Ordinal) >= 0) 
            {
                fp.Type = FramugaType.глухая;
                strTmp = "глухая";
            }
            else if (dataColumns[(int)CollNames.Фрамуга_Лев].IndexOf("частично остеклённая", StringComparison.Ordinal) >= 0) 
            {
                fp.Type = FramugaType.частичного_остекления;
                strTmp = "частично остеклённая";
            }
            else if (dataColumns[(int)CollNames.Фрамуга_Лев].IndexOf("полностью остеклённая", StringComparison.Ordinal) >= 0) 
            {
                fp.Type = FramugaType.полного_остекления;
                strTmp = "полностью остеклённая";
            }
            else fp.Type = FramugaType.нет;

            if (fp.Type != FramugaType.нет) 
            {
                string StrVL;
                string[] lVstavkaSubStrings;
                StrVL = Strings.Mid(
                    dataColumns[(int)CollNames.Фрамуга_Лев], 
                    dataColumns[(int)CollNames.Фрамуга_Лев].IndexOf(strTmp, StringComparison.Ordinal) + strTmp.Length+1, 
                    11);
                StrVL = Strings.Replace(StrVL.Trim(), "С", "", (int)CompareMethod.Text);
                StrVL = Strings.Replace(StrVL.Trim(), "т", "", (int)CompareMethod.Text);
                StrVL = Strings.Replace(StrVL.Trim(), "\n", "", (int)CompareMethod.Text);
                StrVL = Strings.Replace(StrVL.Trim(), " ", "", (int)CompareMethod.Text);
                lVstavkaSubStrings = Strings.Split(StrVL.Trim(), "x");
                fp.Height = short.TryParse(lVstavkaSubStrings[0], out var frh) 
                    ? frh 
                    : (short) 0;
                fp.Width = short.TryParse(lVstavkaSubStrings[1], out var frw) 
                    ? frw 
                    : (short) 0;
                fp.FromTable = dataColumns[(int)CollNames.Фрамуга_Лев];
            }
            param.SetFramugaPar((short)Raspolozhenie.Лев, fp);
        }
        else param.SetFramugaPar((short)Raspolozhenie.Лев, new FramugaParam() { Type = FramugaType.нет });

        // Правая
        if (!string.IsNullOrEmpty(dataColumns[(int)CollNames.Фрамуга_Прав]) & !dataColumns[(int)CollNames.Фрамуга_Прав].Equals("-"))
        {
            fp = param.GetFramugaPar((short)Raspolozhenie.Прав);
            var strTmp = "";
            if (dataColumns[(int)CollNames.Фрамуга_Прав].IndexOf("глухая", StringComparison.Ordinal) >= 0) {
                fp.Type = FramugaType.глухая;
                strTmp = "глухая";
            }
            else if (dataColumns[(int)CollNames.Фрамуга_Прав].IndexOf("частично остеклённая", StringComparison.Ordinal) >= 0) {
                fp.Type = FramugaType.частичного_остекления;
                strTmp = "частично остеклённая";
            }
            else if (dataColumns[(int)CollNames.Фрамуга_Прав].IndexOf("полностью остеклённая", StringComparison.Ordinal) >= 0)
            {
                fp.Type = FramugaType.полного_остекления;
                strTmp = "полностью остеклённая";
            }
            else fp.Type = FramugaType.нет; 
            //if (fp.Type != FramugaType.нет) 
            //{
            //    string StrVR;
            //    string[] rVstavkaSubStrings;
            //    StrVR = Strings.Mid(
            //        dataColumns[(int)CollNames.Фрамуга_Прав], 
            //        dataColumns[(int)CollNames.Фрамуга_Прав].IndexOf(strTmp, StringComparison.Ordinal) + 7, 
            //        11);
            //    StrVR = Strings.Replace(StrVR.Trim(), "С", "", (int)CompareMethod.Text);
            //    StrVR = Strings.Replace(StrVR.Trim(), "т", "", (int)CompareMethod.Text);
            //    rVstavkaSubStrings = Strings.Split(StrVR.Trim(), "x");
            //    fp.Height = short.TryParse(rVstavkaSubStrings[0], out var frh) 
            //        ? frh 
            //        : (short) 0;
            //    fp.Width = short.TryParse(rVstavkaSubStrings[1], out var frw) 
            //        ? frw 
            //        : (short) 0;
            //    fp.FromTable = dataColumns[(int)CollNames.Фрамуга_Прав];
            //}
            param.SetFramugaPar((short)Raspolozhenie.Прав, fp);
        }
        else 
        {
            param.SetFramugaPar((short)Raspolozhenie.Прав, new FramugaParam() { Type = FramugaType.нет });
        }

        // Макушка "Интек"---------------------------------------------------------------------------------------------------------------------
        if (dataColumns[(int)CollNames.Комментарий].IndexOf("под доводчик не должно торчать", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("под доводчик не должно выступать", StringComparison.Ordinal) >= 0 | 
            dataColumns[(int)CollNames.Комментарий].IndexOf("Интек", StringComparison.Ordinal) >= 0) {
            if ((param.Nalichniki[(int)Raspolozhenie.Верх] == 0 & 
                 param.GetFramuga((int)Raspolozhenie.Верх) != FramugaType.нет) | 
                 param.Nalichniki[(int)Raspolozhenie.Верх] > 0) param.Intek = true;
        }

        // Запись комментария к изделию-------------------------------------------------------------------------------------------------------------------------------
        param.Comments = string.IsNullOrEmpty(dataColumns[(int)CollNames.Комментарий]) 
            ? "" 
            : dataColumns[(int)CollNames.Комментарий];

        return param;
    }
    public List<TableData> SkanTable()
    {
        var tDates = new List<TableData>();

        LastRow = WSK.Cells[5, 1].CurrentRegion.Cells(WSK.Cells[5, 1].CurrentRegion.Cells.Count).Row;

        for (var i = 7; i <= LastRow; i++) {
            if (!WSK.Rows[i].Hidden) {
                var td = ScanRow(i);
                tDates.Add(td);
            }
        }
        return tDates;
    }

    private Excel.Workbook SearchBook()
    {
        for (short i = 1; i <= (short)Ex.Workbooks.Count; i++)
        {
            if (Ex.Workbooks.Item[i] == null) continue;

            var name = Ex.Workbooks.Item[i].Name;

            if (name.IndexOf("Конструирование №", StringComparison.Ordinal) >= 0) 
                return Ex.Workbooks.Item[i];
        }
        return null;
    }

    private Excel.Worksheet SearchSheet()
    {
        for (short i = 1; i <= (short)Kons.Worksheets.Count; i++)
        {
            if (Kons.Worksheets.Item[i] == null) continue;

            string name = Kons.Worksheets.Item[i].Name;

            if (name.IndexOf("TDSheet", StringComparison.Ordinal) >= 0) 
                return Kons.Worksheets.Item[i];
        }
        return null;
    }

    private string CellValue(int row, int coll)
    {
        string str = Convert.ToString(WSK.Cells[row, coll].Value);

        return string.IsNullOrEmpty(str) ? "" : str;
    }
}

public enum CollNames
{
    Дата_Готовности,
    Комментарий,
    Номер_Изделия,
    Высота,
    Ширина,
    Атрибут,
    Сторона_Открывания,
    Порог,
    Наличники,
    Ширина_Активки,
    Замок_1_АС,
    Цилиндр_1_АС,
    Ручка_1_АС,
    Замок_2_АС,
    Цилиндр_2_АС,
    Ручка_2_АС,
    Замок_3_АС,
    Цилиндр_3_АС,
    Ручка_3_АС,
    Замок_1_ПС,
    Цилиндр_1_ПС,
    Ручка_1_ПС,
    Замок_2_ПС,
    Цилиндр_2_ПС,
    Ручка_2_ПС,
    Замок_3_ПС,
    Цилиндр_3_ПС,
    Ручка_3_ПС,
    Глазок,
    Задвижки,
    Толщина_ЛЛ,
    Толщина_ВЛ,
    Окно_1_Высота,
    Окно_1_Ширина,
    Окно_1_Гор_разположение,
    Окно_1_По_горизонтали,
    Окно_1_Верт_разположение,
    Окно_1_По_вертикали,
    Окно_2_Высота,
    Окно_2_Ширина,
    Окно_2_Гор_разположение,
    Окно_2_По_горизонтали,
    Окно_2_Верт_разположение,
    Окно_2_По_вертикали,
    Окно_3_Высота,
    Окно_3_Ширина,
    Окно_3_Гор_разположение,
    Окно_3_По_горизонтали,
    Окно_3_Верт_разположение,
    Окно_3_По_вертикали,
    Окно_4_Высота,
    Окно_4_Ширина,
    Окно_4_Гор_разположение,
    Окно_4_По_горизонтали,
    Окно_4_Верт_разположение,
    Окно_4_По_вертикали,
    Добор,
    Код_Изделия,
    Фрамуга_Верх,
    Фрамуга_Лев,
    Фрамуга_Прав,
    Антипаника_АС,
    Антипаника_ПС,
    Окно_5_Высота,
    Окно_5_Ширина,
    Окно_5_Гор_разположение,
    Окно_5_По_горизонтали,
    Окно_5_Верт_разположение,
    Окно_5_По_вертикали,
    Окно_6_Высота,
    Окно_6_Ширина,
    Окно_6_Гор_разположение,
    Окно_6_По_горизонтали,
    Окно_6_Верт_разположение,
    Окно_6_По_вертикали,
    Номер_Заказа,
    Вывод_Ошибок
}

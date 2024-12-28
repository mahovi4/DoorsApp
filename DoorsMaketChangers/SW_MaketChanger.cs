using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using COM_DoorsLibrary;
using SldWorks;

namespace DoorsMaketChangers
{
    public class SW_MaketChanger : MaketChanger
    {
        private readonly string MaketName;
        private SldWorks.SldWorks swApp = new SldWorks.SldWorks();
        private SldWorks.ModelDoc2 Maket;
        private int longstatus, longwarnings;

        public SW_MaketChanger(string maketRootFolder, string maketName, string dxfFolder)
            : base(maketRootFolder, dxfFolder)
        {
            MaketName = maketName;
            swApp.Visible = false;
        }

        public override void Exit()
        {
            if (Maket != null)
            {
                Maket.Close();
                Maket = null;
            }
            swApp.ExitApp();
            swApp = null;
        }

        public override void Build_DM(DM dm, Command_DM com)
        {
            OpenMaket();
            if ((int)com > 0 & (int)com <= 3)
            {
                ВысветитьПолотнаDM(ref dm, com);
                РедактироватьПолотнаDM(ref dm, com);
            }
            else if ((int)com >= 4 & (int)com <= 7)
            {
                ВысветитьСтойкиДМ(ref dm, true, com);
                РедактироватьСтойкиДМ(ref dm, true, com);
            }
            else
            {
                CloseMaket();
                return;
            }

            SaveDXF(dm.Name((short)com));
            CloseMaket();
        }
        public override void Build_LM(LM lm, Command_LM com)
        {
            OpenMaket();
            ВысветитьLM(ref lm, com);
            РедактироватьLM(ref lm, com);
            SaveDXF(lm.Name((short)com));
            CloseMaket();
        }
        public override void Build_VM(DVM vm, Command_VM com)
        {
            OpenMaket();
            ВысветитьDVM(ref vm, com);
            РедактироватьDVM(ref vm, com);
            SaveDXF(vm.Name((short)com));
            if (com == Command_VM.Доборы & (vm.LicList_IsDopDobor(Stvorka.Активная) | vm.VnutList_IsDopDobor(Stvorka.Активная) | vm.LicList_IsDopDobor(Stvorka.Пассивная) | vm.VnutList_IsDopDobor(Stvorka.Пассивная)))
            {
                РедактироватьПодписиДоборов(ref vm);
                SavePDF(vm.Name((short)com));
            }

            CloseMaket();
        }
        public override void Build_ODL(ODL odl, Command_ODL com)
        {
            OpenMaket();
            ВысветитьODL(ref odl, com);
            РедактироватьODL(ref odl, com);
            SaveDXF(odl.Name((short)com));
            CloseMaket();
        }
        public override void Build_KVD(KVD kvd, Command_KVD com, string name)
        {
            OpenMaket();
            ВысветитьKVD(ref kvd, com);
            РедактироватьKVD(ref kvd, com);
            SaveDXF(name);
            CloseMaket();
        }
        public override void Build_Otboynik(OtboynayaPlastina plastina, string num)
        {
            OpenMaket("Отбойная пластина.SLDPRT");
            РедактироватьОтбойник(plastina);
            SaveDXF($"{plastina.Name}_{num}");
        }

        private void OpenMaket(string maketName = null)
        {
            var mName = maketName ?? MaketName;

            if(File.Exists($"{RootPath}/{mName}"))
                Maket = swApp.OpenDoc6($"{RootPath}/{mName}", 1, 0, "", ref longstatus, ref longwarnings);
            else
            {
                Interaction.MsgBox($"Файл SolidWorks-макета '{mName}' не найден");
                Maket = null;
            }
        }
        private void CloseMaket()
        {
            if (Maket == null) return;

            swApp.CloseDoc(MaketName);
            Maket = null;
        }

        private void SaveDXF(string name)
        {
            _ = Maket.ForceRebuild3(false);
            SldWorks.PartDoc maketDoc = (SldWorks.PartDoc)Maket;
            _ = maketDoc.ExportFlatPatternView(DxfPath + "\\" + name + ".DXF", 1);
        }
        private void SavePDF(string name)
        {
            Maket.ViewZoomtofit2();
            longstatus = Maket.SaveAs3(DxfPath + name + ".PDF", 0, 2);
            ПогаситьЭлемент("DVM_Подпись доборов");
        }

        private void ВысветитьПолотнаDM(ref DM dm, Command_DM com)
        {
            // ---Активная створка---------------------------------------------
            if (com == Command_DM.Полотна | com == Command_DM.Полотна2)
            {
                ВысветитьЭлемент("DM_Лицевой лист (активной створки)");
                ВысветитьЭлемент("DM_Верхний отгиб лицевого (активной створки)");
                ВысветитьЭлемент("DM_Внутренний лист (активной створки)");
                ВысветитьЭлемент("DM_Просечки на активке");
                if (dm.Protivos_Count > 0)
                    ВысветитьЭлемент("DM_Противосъемник 1");
                if (dm.Protivos_Count > 1)
                    ВысветитьЭлемент("DM_Противосъемник 2");
                if (dm.IsDownGib)
                {
                    if(dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                        ВысветитьЭлемент("DM_Нижний отгиб лицевого порог 30 (активной створки)");
                    else
                        ВысветитьЭлемент("DM_Нижний отгиб лицевого (активной створки)");
                }

                if (dm.IsTorcevayaPlastina(0))
                {
                    ВысветитьЭлемент("DM_Торцевая пластина_верх (АС)");
                    ВысветитьЭлемент("DM_Просечки торцевой пластины верх (АС)");
                    ВысветитьЭлемент("DM_Молярное отверстие на торцевой верх (АС)");

                    if(dm.IsTermoblock(0))
                        ВысветитьЭлемент("DM_Термоблокератор (верх) в торцевой пластине");
                }

                if (dm.IsTorcevayaPlastina(1))
                {
                    ВысветитьЭлемент("DM_Торцевая пластина_низ (АС)");
                    ВысветитьЭлемент("DM_Просечки торцевой пластины низ (АС)");
                    ВысветитьЭлемент("DM_Молярное отверстие на торцевой низ (АС)");

                    if (dm.IsTermoblock(1))
                        ВысветитьЭлемент("DM_Термоблокератор (низ) в торцевой пластине");
                }

                if (dm.IsOkno(0))
                {
                    ВысветитьЭлемент("DM_Окно1 (АС)");

                    if (dm.IsOtsechka(0, 0))
                    {
                        ВысветитьЭлемент("DM_Отсечки_Окно1 (ЛЛ)");
                        ВысветитьЭлемент("DM_Просечки отсечек_Окно1 (ЛЛ)");
                    }

                    if (dm.IsOtsechka(0, 1))
                    {
                        ВысветитьЭлемент("DM_Отсечки_Окно1 (ВЛ)");
                        ВысветитьЭлемент("DM_Просечки отсечек_Окно1 (ВЛ)");
                    }

                    if(dm.IsRamka(0, 0))
                        ВысветитьЭлемент("DM_ППР_Окно1 (ЛЛ)");

                    if (dm.IsRamka(0, 1))
                        ВысветитьЭлемент("DM_ППР_Окно1 (ВЛ)");
                }

                if (dm.IsOkno(1))
                {
                    ВысветитьЭлемент("DM_Окно2 (АС)");

                    if (dm.IsOtsechka(1, 0))
                    {
                        ВысветитьЭлемент("DM_Отсечки_Окно2 (ЛЛ)");
                        ВысветитьЭлемент("DM_Просечки отсечек_Окно2 (ЛЛ)");
                    }

                    if (dm.IsOtsechka(1, 1))
                    {
                        ВысветитьЭлемент("DM_Отсечки_Окно2 (ВЛ)");
                        ВысветитьЭлемент("DM_Просечки отсечек_Окно2 (ВЛ)");
                    }

                    if (dm.IsRamka(1, 0))
                        ВысветитьЭлемент("DM_ППР_Окно2 (ЛЛ)");

                    if (dm.IsRamka(1, 1))
                        ВысветитьЭлемент("DM_ППР_Окно2 (ВЛ)");
                }

                if (dm.VirezPodKvadrat_Count >= (short)VirezKvadrat._2_выреза)
                    ВысветитьЭлемент("DM_Вырезы под 2 квадрата в активке");
                if (dm.VirezPodKvadrat_Count == (short)VirezKvadrat._3_выреза)
                    ВысветитьЭлемент("DM_Вырез под 3ий квадрат в активке");
                if (dm.IsGND(1))
                    ВысветитьЭлемент("DM_Заземление в полотне");
                if (dm.IsKabelKanal(1))
                    ВысветитьЭлемент("DM_Кабельканал в активке");
                if (dm.Reshetka_Type(0) == eReshetka.ПП_решетка)
                {
                    ВысветитьЭлемент("DM_ПП решетка 1");
                    ВысветитьЭлемент("DM_Крышка ПП решетки 1");
                }
                if (dm.Reshetka_Type(1) == eReshetka.ПП_решетка)
                {
                    ВысветитьЭлемент("DM_ПП решетка 2");
                    ВысветитьЭлемент("DM_Крышка ПП решетки 2");
                }
                if (dm.Reshetka_Type(0) == eReshetka.Вент_решетка)
                    ВысветитьЭлемент("DM_Вентрешетка 1");
                if (dm.Reshetka_Type(1) == eReshetka.Вент_решетка)
                    ВысветитьЭлемент("DM_Вентрешетка 2");
                if (dm.IsGlazok(0))
                    ВысветитьЭлемент("DM_Вырез под Глазок");
                if (!dm.IsDownPritvor)
                    ВысветитьЭлемент("DM_Обрезка нижнего притвора активки");
                // ---Пассивная створка------------------------------------------
                if (dm.IsPassivka)
                {
                    ВысветитьЭлемент("DM_Лицевой лист (пассивной створки)");
                    ВысветитьЭлемент("DM_Внутренний лист (пассивной створки)");
                    ВысветитьЭлемент("DM_Верхний отгиб лицевого пассивной");
                    ВысветитьЭлемент("DM_Вырез по притвору на лицевом пассивной");
                    ВысветитьЭлемент("DM_Просечки на пассивке");
                    if (dm.Protivos_Count > 0)
                        ВысветитьЭлемент("DM_Противосъемник 1 на пассивке");
                    if (dm.Protivos_Count > 1)
                        ВысветитьЭлемент("DM_Противосъемник 2 на пассивке");
                    if (dm.IsDownGib)
                    {
                        if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                            ВысветитьЭлемент("DM_Нижний отгиб лицевого порог 30 (пассивная створки)");
                        else
                            ВысветитьЭлемент("DM_Нижний отгиб лицевого (пассивной створки)");
                    }
                    if (dm.IsOkno(2))
                    {
                        ВысветитьЭлемент("DM_Окно3 (ПС)");

                        if (dm.IsOtsechka(2, 0))
                        {
                            ВысветитьЭлемент("DM_Отсечки_Окно3 (ЛЛ)");
                            ВысветитьЭлемент("DM_Просечки отсечек_Окно3 (ЛЛ)");
                        }

                        if (dm.IsOtsechka(2, 1))
                        {
                            ВысветитьЭлемент("DM_Отсечки_Окно3 (ВЛ)");
                            ВысветитьЭлемент("DM_Просечки отсечек_Окно3 (ВЛ)");
                        }

                        if (dm.IsRamka(2, 0))
                            ВысветитьЭлемент("DM_ППР_Окно3 (ЛЛ)");

                        if (dm.IsRamka(2, 1))
                            ВысветитьЭлемент("DM_ППР_Окно3 (ВЛ)");
                    }

                    if (dm.IsOkno(3))
                    {
                        ВысветитьЭлемент("DM_Окно4 (ПС)");

                        if (dm.IsOtsechka(3, 0))
                        {
                            ВысветитьЭлемент("DM_Отсечки_Окно4 (ЛЛ)");
                            ВысветитьЭлемент("DM_Просечки отсечек_Окно4 (ЛЛ)");
                        }

                        if (dm.IsOtsechka(3, 1))
                        {
                            ВысветитьЭлемент("DM_Отсечки_Окно4 (ВЛ)");
                            ВысветитьЭлемент("DM_Просечки отсечек_Окно4 (ВЛ)");
                        }

                        if (dm.IsRamka(3, 0))
                            ВысветитьЭлемент("DM_ППР_Окно4 (ЛЛ)");

                        if (dm.IsRamka(3, 1))
                            ВысветитьЭлемент("DM_ППР_Окно4 (ВЛ)");
                    }

                    if (dm.IsTorcShpingalet(0))
                        ВысветитьЭлемент("DM_Верхний шпингалет");
                    if (dm.IsTorcShpingalet(1))
                        ВысветитьЭлемент("DM_Нижний шпингалет");
                    if (dm.NSasTS)
                    {
                        ВысветитьЭлемент("DM_Ночник вместо торцевых");
                        ВысветитьЭлемент("DM_Торцевая пластина с ночником");
                    }
                    if (dm.VirezPodKvadrat_Count >= 2)
                        ВысветитьЭлемент("DM_Вырезы под 2 квадрата в пассивке");
                    if (dm.VirezPodKvadrat_Count > 2)
                        ВысветитьЭлемент("DM_Вырез под 3ий квадрат в пассивке");
                    if (dm.IsKabelKanal(2))
                        ВысветитьЭлемент("DM_Кабельканал в пассивке");
                    if (dm.Reshetka_Type(2) == eReshetka.ПП_решетка)
                    {
                        ВысветитьЭлемент("DM_ПП решетка 3");
                        ВысветитьЭлемент("DM_Крышка ПП решетки 3");
                    }
                    if (dm.Reshetka_Type(3) == eReshetka.ПП_решетка)
                    {
                        ВысветитьЭлемент("DM_ПП решетка 4");
                        ВысветитьЭлемент("DM_Крышка ПП решетки 4");
                    }
                    if (dm.Reshetka_Type(2) == eReshetka.Вент_решетка)
                        ВысветитьЭлемент("DM_Вентрешетка 3");
                    if (dm.Reshetka_Type(3) == eReshetka.Вент_решетка)
                        ВысветитьЭлемент("DM_Вентрешетка 4");
                    if (!dm.IsDownPritvor)
                        ВысветитьЭлемент("DM_Обрезка нижнего притвора пассивки");
                }
                // ---Нижний замок--------------------------------------
                if (dm.Zamok(0).Kod > 0)
                {
                    ZamokDatas zd = dm.Zamok(0);
                    if (!zd.VirezNames[(int)ZamokSketchTypes.На_Активке].Equals("_"))
                        ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.На_Активке]);
                    if (dm.IsPassivka)
                    {
                        if (dm.IsAPanOnPassiv)
                        {
                            if (!zd.VirezNames[(int)ZamokSketchTypes.Ответный_замок].Equals("_"))
                                ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.Ответный_замок]);
                        }
                        else
                        {
                            if (!zd.VirezNames[(int)ZamokSketchTypes.Ответка_Пассивка].Equals("_"))
                                ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.Ответка_Пассивка]);
                        }
                    }
                    if (zd.CM) ВысветитьЭлемент("DM_Цилиндр нижнего");
                }
                // ---Верхний замок-------------------------------------
                if (dm.Zamok(1).Kod > 0)
                {
                    ZamokDatas zd = dm.Zamok(1);
                    if (!zd.VirezNames[(int)ZamokSketchTypes.На_Активке].Equals("_"))
                        ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.На_Активке] + zd.SketchSufName);
                    if (dm.IsPassivka)
                    {
                        if (dm.IsAPanOnPassiv)
                        {
                            if (!zd.VirezNames[(int)ZamokSketchTypes.Ответный_замок].Equals("_"))
                                ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.Ответный_замок] + zd.SketchSufName);
                        }
                        else
                        {
                            if (!zd.VirezNames[(int)ZamokSketchTypes.Ответка_Пассивка].Equals("_"))
                                ВысветитьЭлемент(zd.VirezNames[(int)ZamokSketchTypes.Ответка_Пассивка] + zd.SketchSufName);
                        }
                    }
                }
                // ---Кодовый замок-------------------------------------
                switch (dm.Kodoviy_Kod(1))
                {
                    case 1:
                        {
                            ВысветитьЭлемент("DM_Кодовый (1) на активке");
                            if (dm.IsPassivka)
                                ВысветитьЭлемент("DM_Кодовый (1) ответка на пассивке");
                            break;
                        }

                    case 2:
                        {
                            ВысветитьЭлемент("DM_Кодовый (2) на активке");
                            if (dm.IsPassivka)
                                ВысветитьЭлемент("DM_Кодовый (2) ответка на пассивке");
                            break;
                        }

                    case 3:
                        {
                            ВысветитьЭлемент("DM_Кодовый (3) на пассивке");
                            ВысветитьЭлемент("DM_Кодовый (3) ответка на активке");
                            break;
                        }

                    case 4:
                        {
                            ВысветитьЭлемент("DM_Кодовый (4) на пассивке");
                            ВысветитьЭлемент("DM_Кодовый (4) ответка на активке");
                            break;
                        }

                    case 5:
                        {
                            ВысветитьЭлемент("DM_Кодовый (5) на пассивке");
                            ВысветитьЭлемент("DM_Кодовый (5) на активке");
                            break;
                        }

                    case 6:
                        {
                            ВысветитьЭлемент("DM_Кодовый (6) на пассивке");
                            ВысветитьЭлемент("DM_Кодовый (6) на активке");
                            break;
                        }
                }
                // ---Ручки---------------------------------------------
                var sl = dm.IsPassivka ? 2 : 1;
                for (var i = 0; i < sl; i++)
                {
                    for (var y = 0; y < 2; y++)
                    {
                        if (dm.Ruchka_Kod((short) i, (short) y) <= 0) 
                            continue;

                        if (!dm.Ruchka_VirezName((short)i, (short)y).Equals("_"))
                            ВысветитьЭлемент(dm.Ruchka_VirezName((short)i, (short)y));

                        var r = dm.Ruchka((short) i, (short) y);
                        if (r.Kod == (short) RuchkaNames.PB_1700A && r.KodRuchkaLL == (short)RuchkaNames.Ручка_черная_планка)
                            ВысветитьЭлемент("DM_1700A_стяжки ручки на планке");
                    }
                }
                // ---Задвижки------------------------------------------
                switch (dm.Zadvizhka_Kod(1))
                {
                    case (short)ZadvizhkaNames.Ночной_сторож:
                        ВысветитьЭлемент("DM_Под вертушок");
                        ВысветитьЭлемент("DM_Под тело ночного сторожа в активке");
                        if (dm.IsPassivka)
                            ВысветитьЭлемент("DM_Ночной сторож ответка в пассивке");
                        break;

                    case (short)ZadvizhkaNames.ЗД_01:
                        ВысветитьЭлемент("DM_ЗД-01 в активке");
                        if (dm.IsPassivka)
                            ВысветитьЭлемент("DM_ЗД-01 ответка в пассивке");
                        break;

                    case (short)ZadvizhkaNames.ЗТ_150:
                        ВысветитьЭлемент("DM_ЗТ-150 в активке");
                        break;
                }
                // ---Выподающий порог----------------------------------
                if (dm.IsVipadPorog)
                {
                    ВысветитьЭлемент("DM_Выпадающий порог на внутреннем (активной створки)");
                    if (dm.IsPassivka)
                    {
                        ВысветитьЭлемент("DM_Выпадающий порог на внутреннем пассивной створки");
                        if (dm.IsTorcShpingalet((short)Raspolozhenie.Верх))
                        {
                            //ВысветитьЭлемент("DM_ЗД-01 на внутреннем пассивки при выпадающем пороге");
                            ВысветитьЭлемент($"{dm.GetZadvizhkaNizName()} на внутреннем пассивки при выпадающем пороге");
                            //ВысветитьЭлемент("DM_ЗД-10 на внутреннем пассивки при выпадающем пороге");
                        }
                    }
                }
                // ---Секущая плоскость---------------------------------
                if (dm.IsSekPloskost(Stvorka.Активная))
                    ВысветитьЭлемент("DM_Рассечение ВЛАС");
                if (dm.IsSekPloskost(Stvorka.Пассивная))
                    ВысветитьЭлемент("DM_Рассечение ЛЛПС");
                if (dm.Name(1).IndexOf("MAX") >= 0 || dm.IsTermoblock(0))
                {
                    ВысветитьЭлемент("DM_Вырезы под усиление петель в активке");
                    if(dm.IsPassivka)
                        ВысветитьЭлемент("DM_Вырезы под усиление петель в пассивке");
                }
            }
            // ---Добор------------------------------------------------
            if (com == Command_DM.Добор | ((com == Command_DM.Полотна | com == Command_DM.Полотна2) & dm.IsDobor))
            {
                ВысветитьЭлемент("DM_Левый добор");
                ВысветитьЭлемент("DM_Верхний доор");
                ВысветитьЭлемент("DM_Анкера в доборах");
                if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Прав] != dm.Dobor_Nalichnik[(int)Raspolozhenie.Лев])
                {
                    ВысветитьЭлемент("DM_Правый добор");
                    ВысветитьЭлемент("DM_Анкера в правом");
                }
                if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] > 0 & dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] != dm.Dobor_Nalichnik[(int)Raspolozhenie.Верх])
                {
                    ВысветитьЭлемент("DM_Нижний доор");
                    ВысветитьЭлемент("DM_Анкера в нижнем");
                }
            }
        }
        private void РедактироватьПолотнаDM(ref DM dm, Command_DM com)
        {
            if (com == Command_DM.Полотна | com == Command_DM.Полотна2)
            {
                // ---Активная створка----------------------------------
                РедактироватьЭскиз("DM_ЛЛАС");
                ИзменитьРазмер("DM_ЛЛАС", "отПола", dm.LicevoyList_OtPola);
                ИзменитьРазмер("DM_ЛЛАС", "Ширина", (float)dm.LicevoyList_Width(Stvorka.Активная));
                if (dm.IsDownGib)
                {
                    if(dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                        ИзменитьРазмер("DM_ЛЛАС", "Высота", (float)(dm.LicevoyList_Height(Stvorka.Активная) - 6));
                    else
                        ИзменитьРазмер("DM_ЛЛАС", "Высота", (float)(dm.LicevoyList_Height(Stvorka.Активная) - dm.UpGib(Stvorka.Активная)));
                }
                else
                    ИзменитьРазмер("DM_ЛЛАС", "Высота", (float)dm.LicevoyList_Height(Stvorka.Активная));
                ИзменитьРазмер("DM_Верхний отгиб ЛЛАС", "Отступ", dm.LeftGib(Stvorka.Активная));
                ИзменитьРазмер("DM_Верхний отгиб ЛЛАС", "Высота", dm.UpGib(Stvorka.Активная));
                ИзменитьРазмер("DM_Нижний отгиб ЛЛАС", "Отступ левый", dm.KompensVirez);
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DM_ВЛАС");
                ИзменитьРазмер("DM_ВЛАС", "отПола", dm.VnutrenniyList_OtPola);
                ИзменитьРазмер("DM_ВЛАС", "Ширина", (float)dm.VnutrenniyList_Width(Stvorka.Активная));
                ИзменитьРазмер("DM_ВЛАС", "Высота", (float)dm.VnutrenniyList_Height(Stvorka.Активная));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DM_Верхний отгиб ЛЛАС");
                ИзменитьРазмер("DM_Верхний отгиб ЛЛАС", "Отступ", dm.LeftGib(Stvorka.Активная));
                ИзменитьРазмер("DM_Верхний отгиб ЛЛАС", "Высота", dm.UpGib(Stvorka.Активная));
                ЗакрытьЭскиз();
                if (dm.IsDownGib)
                {
                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                    {
                        РедактироватьЭскиз("DM_Нижний отгиб ЛЛАС_30");
                        ИзменитьРазмер("DM_Нижний отгиб ЛЛАС_30", "Отступ левый", dm.LeftGib(Stvorka.Активная));
                        ИзменитьРазмер("DM_Нижний отгиб ЛЛАС_30", "Отступ правый", dm.RightGib(Stvorka.Активная));
                        ЗакрытьЭскиз();
                    }
                    else
                    {
                        РедактироватьЭскиз("DM_Нижний отгиб ЛЛАС");
                        ИзменитьРазмер("DM_Нижний отгиб ЛЛАС", "Отступ левый", dm.KompensVirez);
                        ИзменитьРазмер("DM_Нижний отгиб ЛЛАС", "Отступ правый", dm.RightGib(Stvorka.Активная));
                        ИзменитьРазмер("DM_Нижний отгиб ЛЛАС", "Высота", dm.UpGib(Stvorka.Активная));
                        ЗакрытьЭскиз();
                    }
                }

                if (dm.IsTorcevayaPlastina(0))
                {
                    РедактироватьЭскиз("DM_ТП_В_АС");
                    ИзменитьРазмер("DM_ТП_В_АС", "Ширина", (float)dm.TorcevayaPlastina(0).Width);
                    ИзменитьРазмер("DM_ТП_В_АС", "ОтступПетлевой", (float)dm.TorcevayaPlastina(0).OtstupPetlya);
                    ИзменитьРазмер("DM_ТП_В_АС", "ОтступЗамковой", (float)dm.TorcevayaPlastina(0).OtstupZamok);
                    ЗакрытьЭскиз();
                }

                if (dm.IsSekPloskost(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DM_Вставка ВЛАС");
                    ИзменитьРазмер("DM_Вставка ВЛАС", "Ширина", dm.Vstavka_Width(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }
                if (dm.Protivos_Count > 0)
                {
                    switch (dm.Protivos_Count)
                    {
                        case 1:
                            РедактироватьЭскиз("DM_Противосъемник1 АС");
                            ИзменитьРазмер("DM_Противосъемник1 АС", "ОтПола", dm.Protivos_OtPola(0));
                            ИзменитьРазмер("DM_Противосъемник1 АС", "От края", (float)dm.Protivos_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                        case 2:
                            РедактироватьЭскиз("DM_Противосъемник1 АС");
                            ИзменитьРазмер("DM_Противосъемник1 АС", "ОтПола", dm.AnkerOtPola(1));
                            ИзменитьРазмер("DM_Противосъемник1 АС", "От края", (float)dm.Protivos_OtKraya);
                            ЗакрытьЭскиз();
                            РедактироватьЭскиз("DM_Противосъемник2 АС");
                            ИзменитьРазмер("DM_Противосъемник2 АС", "ОтПола", dm.AnkerOtPola(3));
                            ИзменитьРазмер("DM_Противосъемник2 АС", "От края", (float)dm.Protivos_OtKraya);
                            ЗакрытьЭскиз();
                            break;

                    }
                }
                if (dm.IsVipadPorog)
                {
                    РедактироватьЭскиз("DM_Выпадающий порог АС");
                    ИзменитьРазмер("DM_Выпадающий порог АС", "От края", dm.SPorog_OtKraya);
                    ЗакрытьЭскиз();
                }
                // ---Пассивная створка--------------------------------
                if (dm.IsPassivka)
                {
                    РедактироватьЭскиз("DM_ЛЛПС");
                    ИзменитьРазмер("DM_ЛЛПС", "Отступ", (float)(dm.LicevoyList_Width(Stvorka.Активная) + dm.VnutrenniyList_Width(Stvorka.Активная) + 500d));
                    ИзменитьРазмер("DM_ЛЛПС", "Ширина", (float)dm.LicevoyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DM_ВЛПС");
                    ИзменитьРазмер("DM_ВЛПС", "Ширина", (float)dm.VnutrenniyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DM_Вырез Притвор");
                    ИзменитьРазмер("DM_Вырез Притвор", "По порогу", (float)dm.VirezPoPorogu_Height);
                    ИзменитьРазмер("DM_Вырез Притвор", "Ширина верх", (float)dm.VirezPoPorogu_Width);
                    ИзменитьРазмер("DM_Вырез Притвор", "Ширина низ", (float)dm.VirezPoPorogu_Width);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DM_Верхний отгиб ЛЛПС");
                    ИзменитьРазмер("DM_Верхний отгиб ЛЛПС", "Отступ левый", dm.LeftGib(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    if (dm.IsDownGib)
                    {
                        if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                        {
                            РедактироватьЭскиз("DM_Нижний отгиб ЛЛПС_30");
                            ИзменитьРазмер("DM_Нижний отгиб ЛЛПС_30", "Отступ левый", dm.LeftGib(Stvorka.Пассивная));
                            ИзменитьРазмер("DM_Нижний отгиб ЛЛПС_30", "Отступ правый", dm.RightGib(Stvorka.Пассивная));
                            ЗакрытьЭскиз();
                        }
                        else
                        {
                            РедактироватьЭскиз("DM_Нижний отгиб ЛЛПС");
                            ИзменитьРазмер("DM_Нижний отгиб ЛЛПС", "Отступ левый", dm.LeftGib(Stvorka.Пассивная));
                            ИзменитьРазмер("DM_Нижний отгиб ЛЛПС", "Отступ правый", dm.KompensVirez);
                            ИзменитьРазмер("DM_Нижний отгиб ЛЛПС", "Высота", dm.UpGib(Stvorka.Активная));
                            ЗакрытьЭскиз();
                        }
                    }
                    if (dm.IsTorcShpingalet((int)Raspolozhenie.Верх))
                    {
                        РедактироватьЭскиз("DM_ВерхнийШпингалет");
                        ИзменитьРазмер("DM_ВерхнийШпингалет", "От края ЛП", (float)dm.TorcShpingalet_OtKraya);
                        ЗакрытьЭскиз();
                    }

                    if (dm.IsTorcShpingalet((int)Raspolozhenie.Ниж))
                    {
                        РедактироватьЭскиз("DM_НижнийШпингалет");
                        ИзменитьРазмер("DM_НижнийШпингалет", "От края ЛП", (float)dm.TorcShpingalet_OtKraya);
                        ЗакрытьЭскиз();
                    }

                    if (dm.NSasTS)
                    {
                        РедактироватьЭскиз("DM_ТП");
                        ИзменитьРазмер("DM_ТП", "Ширина", (float)dm.TPWidth);
                        ИзменитьРазмер("DM_ТП", "ОтКрая", (float)dm.TPOtKraya);
                        ЗакрытьЭскиз();
                    }
                    if (dm.IsSekPloskost(Stvorka.Пассивная))
                    {
                        РедактироватьЭскиз("DM_Вставка ЛЛПС");
                        ИзменитьРазмер("DM_Вставка ЛЛПС", "Ширина", dm.Vstavka_Width(Stvorka.Пассивная));
                        ЗакрытьЭскиз();
                    }
                }
                
                //Редактирование привязок глазка
                if (dm.IsGlazok(0))
                {
                    РедактироватьЭскиз("DM_Глазок");
                    ИзменитьРазмер("DM_Глазок", "От края", (float)dm.Glazok_OtKraya(0, 1));
                    ЗакрытьЭскиз();
                }
                //Редактирование привязок замка
                for (short i = 0; i < 2; i++)
                {
                    if (dm.Zamok_Kod(i, 1) > 0)
                    {
                        ПозиционированиеЗамка(i, ref dm);
                        int ski;
                        if (i == 0 & dm.IsAPanOnPassiv)
                            ski = (int)ZamokSketchTypes.Ответный_замок;
                        else
                            ski = (int)ZamokSketchTypes.Ответка_Пассивка;
                        РазмерыЗамка(i, dm.Zamok_SketchNames(i)[(int)ZamokSketchTypes.На_Активке],
                                        dm.Zamok_SketchNames(i)[ski],
                                        ref dm);
                    }
                }
                //Редактирование привязок кодового замка
                switch (dm.Kodoviy_Kod(1))
                {
                    case 1:
                        {
                            РедактироватьЭскиз("DM_Кодовый1АС");
                            ИзменитьРазмер("DM_Кодовый1АС", "Ширина", (float)dm.Kodoviy_OtShir);
                            ИзменитьРазмер("DM_Кодовый1АС", "От края ЛА", (float)(dm.LicevoyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ИзменитьРазмер("DM_Кодовый1АС", "От края ВА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(1)));
                            ИзменитьРазмер("DM_Кодовый1АС", "До тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый1АС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            if (dm.IsPassivka)
                            {
                                РедактироватьЭскиз("DM_Кодовый1ПС");
                                ИзменитьРазмер("DM_Кодовый1ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                                ЗакрытьЭскиз();
                            }

                            break;
                        }

                    case 2:
                        {
                            РедактироватьЭскиз("DM_Кодовый2АС");
                            ИзменитьРазмер("DM_Кодовый2АС", "От края ЛА", (float)(dm.LicevoyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ИзменитьРазмер("DM_Кодовый2АС", "От края ВА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(1)));
                            ИзменитьРазмер("DM_Кодовый2АС", "От тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый2АС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            if (dm.IsPassivka)
                            {
                                РедактироватьЭскиз("DM_Кодовый2ПС");
                                ИзменитьРазмер("DM_Кодовый2ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                                ЗакрытьЭскиз();
                            }

                            break;
                        }

                    case 3:
                        {
                            РедактироватьЭскиз("DM_Кодовый3ПС");
                            ИзменитьРазмер("DM_Кодовый3ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                            ИзменитьРазмер("DM_Кодовый3ПС", "От тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый3ПС", "От края ВА", (float)dm.Kodoviy_OtKraya(1));
                            ИзменитьРазмер("DM_Кодовый3ПС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            РедактироватьЭскиз("DM_Кодовый3АС");
                            ИзменитьРазмер("DM_Кодовый3АС", "От края ЛА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ЗакрытьЭскиз();
                            break;
                        }

                    case 4:
                        {
                            РедактироватьЭскиз("DM_Кодовый4ПС");
                            ИзменитьРазмер("DM_Кодовый4ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                            ИзменитьРазмер("DM_Кодовый4ПС", "От тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый4ПС", "От края ВА", (float)dm.Kodoviy_OtKraya(1));
                            ИзменитьРазмер("DM_Кодовый4ПС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            РедактироватьЭскиз("DM_Кодовый4АС");
                            ИзменитьРазмер("DM_Кодовый4АС", "От края ЛА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ЗакрытьЭскиз();
                            break;
                        }

                    case 5:
                        {
                            РедактироватьЭскиз("DM_Кодовый5ПС");
                            ИзменитьРазмер("DM_Кодовый5ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                            ИзменитьРазмер("DM_Кодовый5ПС", "От тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый5ПС", "От края ВА", (float)dm.Kodoviy_OtKraya(1));
                            ИзменитьРазмер("DM_Кодовый5ПС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            РедактироватьЭскиз("DM_Кодовый5АС");
                            ИзменитьРазмер("DM_Кодовый5АС", "От края ЛА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ЗакрытьЭскиз();
                            break;
                        }

                    case 6:
                        {
                            РедактироватьЭскиз("DM_Кодовый6ПС");
                            ИзменитьРазмер("DM_Кодовый6ПС", "От края ЛП", (float)dm.Kodoviy_OtKraya(2));
                            ИзменитьРазмер("DM_Кодовый6ПС", "От тела", (float)dm.Kodoviy_OtTela);
                            ИзменитьРазмер("DM_Кодовый6ПС", "От края ВА", (float)dm.Kodoviy_OtKraya(1));
                            ИзменитьРазмер("DM_Кодовый6ПС", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                            РедактироватьЭскиз("DM_Кодовый6АС");
                            ИзменитьРазмер("DM_Кодовый6АС", "От края ЛА", (float)(dm.LicevoyList_Width(Stvorka.Активная) - dm.Kodoviy_OtKraya(0)));
                            ЗакрытьЭскиз();
                            break;
                        }
                }
                //Редактирование привязок ручек
                int sl = dm.IsPassivka ? 2 : 1;
                for (short i = 0; i < sl; i++)
                {
                    for (short y = 0; y < 2; y++)
                    {
                        short kod = dm.Ruchka_Kod(i, y);
                        if (kod > 0)
                        {
                            if (i == (int)Stvorka.Активная &
                                (kod == (int)RuchkaNames.Ручка_черная_планка |
                                kod == (int)RuchkaNames.Ручка_планка_Просам |
                                kod == (int)RuchkaNames.Ручка_фланец |
                                kod == (int)RuchkaNames.Ручка_Вега))
                            {
                                if (dm.Zamok_Kod(0, 1) == 0)
                                {
                                    РедактироватьЭскиз("DM_ЯЗН");
                                    ИзменитьРазмер("DM_ЯЗН", "ОтПола", dm.Ruchka_OtPola(i, y));
                                    ИзменитьРазмер("DM_ЯЗН", "ОтКраяЛА", (float)(dm.LicevoyList_Width(Stvorka.Активная) - dm.Ruchka_OtKraya(i, y, 0)));
                                    ИзменитьРазмер("DM_ЯЗН", "ОтКраяВА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Ruchka_OtKraya(i, y, 1)));
                                    ЗакрытьЭскиз();
                                }
                            }
                            else
                            {
                                if (kod > 10)
                                    РамерыАнтипаники(i, y, ref dm);
                                else
                                {
                                    string sketch = dm.Ruchka_SketchName(i, y);
                                    if (!string.IsNullOrEmpty(sketch) & !sketch.Equals("_"))
                                    {
                                        РедактироватьЭскиз(sketch);
                                        ИзменитьРазмер(sketch, "ОтКраяЛА", (float)dm.Ruchka_OtKraya(i, y, 0));
                                        ИзменитьРазмер(sketch, "От края ВЛАС", (float)dm.Ruchka_OtKraya(i, y, 1));
                                        ИзменитьРазмер(sketch, "ОтПола", (float)dm.Ruchka_OtPola(i, y));
                                        if (kod == (int)RuchkaNames.Ручка_скоба)
                                            ИзменитьРазмер(sketch, "Межосевое", dm.Ruchka_Mezhosevoe(i, y));
                                        ЗакрытьЭскиз();
                                    }
                                }
                            }
                        }
                    }
                }
                //Редактирование привязок задвижек
                switch (dm.Zadvizhka_Kod(1))
                {
                    case (short)ZadvizhkaNames.Ночной_сторож:
                        РедактироватьЭскиз("DM_Вертушок");
                        ИзменитьРазмер("DM_Вертушок", "Отступ", (float)dm.Zadvizhka_Vertushok_OtKraya);
                        ИзменитьРазмер("DM_Вертушок", "ЗадвижкаОтПола", dm.Zadvizhka_OtPola);
                        ЗакрытьЭскиз();
                        РедактироватьЭскиз("DM_Ночник АС");
                        ИзменитьРазмер("DM_Ночник АС", "ЗадвижкаОтКраяВА", (float)(dm.Zadvizhka_OtKrayaVA));
                        ИзменитьРазмер("DM_Ночник АС", "ЗадвижкаОтПола", dm.Zadvizhka_OtPola);
                        ЗакрытьЭскиз();
                        if (dm.IsPassivka)
                        {
                            РедактироватьЭскиз("DM_Ночник ПС");
                            ИзменитьРазмер("DM_Ночник ПС", "ЗадвижкаОтПола", dm.Zadvizhka_OtPola);
                            ЗакрытьЭскиз();
                        }
                        break;

                    case (short)ZadvizhkaNames.ЗД_01:
                        РедактироватьЭскиз("DM_ЗД-01 АС");
                        ИзменитьРазмер("DM_ЗД-01 АС", "ЗадвижкаОтКраяВА", (float)dm.Zadvizhka_OtKrayaVA);
                        ИзменитьРазмер("DM_ЗД-01 АС", "ЗадвижкаОтПола", dm.Zadvizhka_OtPola);
                        ЗакрытьЭскиз();
                        break;

                    case (short)ZadvizhkaNames.ЗТ_150:
                        РедактироватьЭскиз("DM_ЗТ-150 АС");
                        ИзменитьРазмер("DM_ЗТ-150 АС", "ЗадвижкаОтКраяВА", (float)dm.Zadvizhka_OtKrayaVA);
                        ИзменитьРазмер("DM_ЗТ-150 АС", "ЗадвижкаОтПола", dm.Zadvizhka_OtPola);
                        ЗакрытьЭскиз();
                        break;
                }
                //Редактироване привязок противосъемников
                if (dm.Protivos_Count > 1)
                {
                    РедактироватьЭскиз("DM_Противосъем2 АС");
                    ИзменитьРазмер("DM_Противосъем2 АС", "Между противос", dm.Protivos_OtPola(1));
                    ЗакрытьЭскиз();
                }
                //Редактирование привязок окон и решеток
                for (short i = 0; i < 4; i++)
                {
                    if (dm.IsOkno(i))
                    {
                        РедактироватьЭскиз($"DM_Окно{i + 1}");
                        ИзменитьРазмер($"DM_Окно{i + 1}", "От пола", (float)dm.Okno_OtPola(i, 0));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "От края ЛЛ", (float)dm.Okno_OtKraya(i, 0));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "От края ВЛ", (float)dm.Okno_OtKraya(i, 1));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "Ширина ЛЛ", dm.Okno_Width(i, 0));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "Ширина ВЛ", dm.Okno_Width(i, 1));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "Высота ЛЛ", dm.Okno_Height(i, 0));
                        ИзменитьРазмер($"DM_Окно{i + 1}", "Высота ВЛ", dm.Okno_Height(i, 1));
                        ЗакрытьЭскиз();

                        if (dm.IsOtsechka(i, 0))
                        {
                            РедактироватьЭскиз($"DM_Отс_Окно{i + 1}_ЛЛ");
                            ИзменитьРазмер($"DM_Отс_Окно{i + 1}_ЛЛ", "ШиринаОтсечки", (float)dm.Otsechka(i).Width);
                            ЗакрытьЭскиз();
                        }

                        if (dm.IsOtsechka(i, 1))
                        {
                            РедактироватьЭскиз($"DM_Отс_Окно{i + 1}_ВЛ");
                            ИзменитьРазмер($"DM_Отс_Окно{i + 1}_ВЛ", "ШиринаОтсечки", (float)dm.Otsechka(i).Width);
                            ЗакрытьЭскиз();
                        }
                    }
                    if (dm.Reshetka_Type(i) == eReshetka.ПП_решетка)
                    {
                        РедактироватьЭскиз("DM_ППРешетка" + (i + 1));
                        ИзменитьКоличество("DM_ППРешетка" + (i + 1), "Количество вырезов", dm.Reshetka_Virezi_Count(i));
                        ИзменитьРазмер("DM_ППРешетка" + (i + 1), "От пола", dm.Reshetka_OtPola(i));
                        ИзменитьРазмер("DM_ППРешетка" + (i + 1), "Ширина", dm.Reshetka_Width(i));
                        ИзменитьРазмер("DM_ППРешетка" + (i + 1), "Высота", dm.Reshetka_Height(i));
                        ИзменитьРазмер("DM_ППРешетка" + (i + 1), "Отступ", (float)dm.Reshetka_Virezi_Otstup(i));
                        ЗакрытьЭскиз();

                        РедактироватьЭскиз("DM_КрышкаППР" + (i + 1));
                        ИзменитьРазмер("DM_КрышкаППР" + (i + 1), "От края", (float)dm.Reshetka_OtKraya(i));
                        ЗакрытьЭскиз();
                    }
                    else if (dm.Reshetka_Type(i) == eReshetka.Вент_решетка)
                    {
                        РедактироватьЭскиз("DM_Вентрешетка" + (i + 1));
                        ИзменитьРазмер("DM_Вентрешетка" + (i + 1), "От пола", dm.Reshetka_OtPola(i));
                        ИзменитьРазмер("DM_Вентрешетка" + (i + 1), "Ширина", dm.Reshetka_Width(i));
                        ИзменитьРазмер("DM_Вентрешетка" + (i + 1), "Высота", dm.Reshetka_Height(i));
                        ЗакрытьЭскиз();
                    }
                }
            }
            //Редактирование параметров доборов
            if (com == Command_DM.Добор | ((com == Command_DM.Полотна | com == Command_DM.Полотна2) & dm.IsDobor))
            {
                РедактироватьЭскиз("DM_Добор левый");
                ИзменитьРазмер("DM_Добор левый", "Высота", dm.Dobor_Length(Raspolozhenie.Лев));
                ИзменитьРазмер("DM_Добор левый", "Глубина", dm.Dobor_Glubina);
                ИзменитьРазмер("DM_Добор левый", "Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Лев]);
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DM_Добор верхний");
                ИзменитьРазмер("DM_Добор верхний", "Ширина", dm.Dobor_Length(Raspolozhenie.Верх));
                ИзменитьРазмер("DM_Добор верхний", "Глубина", dm.Dobor_Glubina);
                ИзменитьРазмер("DM_Добор верхний", "Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Верх]);
                ЗакрытьЭскиз();
                if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Прав] != dm.Dobor_Nalichnik[(int)Raspolozhenie.Лев])
                {
                    РедактироватьЭскиз("DM_Добор правый");
                    ИзменитьРазмер("DM_Добор правый", "Высота", dm.Dobor_Length(Raspolozhenie.Прав));
                    ИзменитьРазмер("DM_Добор правый", "Глубина", dm.Dobor_Glubina);
                    ИзменитьРазмер("DM_Добор правый", "Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Прав]);
                }
                if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] > 0 & dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] != dm.Dobor_Nalichnik[(int)Raspolozhenie.Верх])
                {
                    РедактироватьЭскиз("DM_Доор нижний");
                    ИзменитьРазмер("DM_Доор нижний", "Ширина", dm.Dobor_Length(Raspolozhenie.Ниж));
                    ИзменитьРазмер("DM_Доор нижний", "Глубина", dm.Dobor_Glubina);
                    ИзменитьРазмер("DM_Доор нижний", "Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Ниж]);
                    ЗакрытьЭскиз();
                }
            }
        }
        private void ВысветитьСтойкиДМ(ref DM dm, bool Otvetki, Command_DM com)
        {
            short conf;
            Raspolozhenie pos;
            if (com == Command_DM.Левая_стойка | com == Command_DM.Правя_стойка | com == Command_DM.Притолока)
            {
                if (dm.Otkrivanie == Otkrivanie.ЛевоеВО | dm.Otkrivanie == Otkrivanie.ПравоеВО) conf = (short)((short)com + 4);
                else conf = (short)com;
            }
            else if (com == Command_DM.Порог)
            {
                if (dm.Nalichnik(Raspolozhenie.Ниж) > 0)
                {
                    if (dm.Otkrivanie == Otkrivanie.ЛевоеВО | dm.Otkrivanie == Otkrivanie.ПравоеВО) conf = 10;
                    else conf = 6;
                }
                else conf = (short)com;
            }
            else return;

            switch (com)
            {
                case Command_DM.Притолока:
                    pos = Raspolozhenie.Верх;
                    break;
                case Command_DM.Порог:
                    pos = Raspolozhenie.Ниж;
                    break;
                case Command_DM.Правя_стойка:
                    pos = Raspolozhenie.Прав;
                    break;
                default:
                    pos = Raspolozhenie.Лев;
                    break;
            }
            //Стойки К1/К2
            if (conf == 4 | conf == 5)
            {
                ВысветитьЭлемент("DM_К1/К2 стойка двери");
                ВысветитьЭлемент("DM_Анкерные отверстия К1/К2");
                //ВысветитьЭлемент("DM_РЖК стойки-низ");
                //if (dm.Height > 1290)
                //{
                //    ВысветитьЭлемент("DM_РЖК стойки-верх");
                //    ВысветитьЭлемент("Технологические Вырезы в РЖКС-верх");
                //}
                //ВысветитьЭлемент("DM_Анкерные отверстия РЖК");

                if (dm.DownZanizhenieStoyki(pos) > 0d)
                    ВысветитьЭлемент("DM_Удлинение стойки К1/К2 при наличнике снизу");
                if (dm.UpZaglushkaNSStoyki(pos) > 0)
                    ВысветитьЭлемент("DM_Верхняя ЗаглушкаNS на стойке К1/К2");
                if (dm.DownZaglushkaNSStoyki(pos) > 0)
                    ВысветитьЭлемент("DM_Нижняя заглушкаNS на стойке К1/К2");
                if (dm.IsObrezkaNalichnika(pos))
                    ВысветитьЭлемент("DM_Обрезка стойки К1/К2");
                if (dm.IsGND((short)com))
                    ВысветитьЭлемент("DM_Заземление в стойке К1");
                if (dm.IsKabelKanal((short)com))
                    ВысветитьЭлемент("DM_Кабельканал в стойке К1");
            }
            //Макушка К1/K2
            if (conf == 6)
            {
                ВысветитьЭлемент("DM_К1/К2 макушка двери");
                if (Otvetki & dm.IsTorcShpingalet((int)Raspolozhenie.Верх))
                {
                    ВысветитьЭлемент("DM_Шпингалет в макушке К1");
                    ВысветитьЭлемент("DM_Маркер левой  К1");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К1");
                }

                if (Otvetki & dm.IsOtvAntipan((short)Raspolozhenie.Верх))
                {
                    ВысветитьЭлемент("DM_Ответка антипаники в макушке К1");
                    ВысветитьЭлемент("DM_Маркер левой  К1");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К1");
                }

                if (dm.IsTermoblock(0))
                {
                    ВысветитьЭлемент("DM_Термоблокератор в К1");
                    ВысветитьЭлемент("DM_Маркер левой  К1");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К1");
                }

                if (dm.IsSdvigoviy)
                {
                    ВысветитьЭлемент(dm.Sdvigoviy.NameVirez);
                    ВысветитьЭлемент("DM_Маркер левой  К1");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К1");
                }

                if (dm.IsAnkerInPritoloka)
                    ВысветитьЭлемент("DM_Анкерные отверстия в макушке К1/К2");
                if (dm.IsObrezkaNalichnika(Raspolozhenie.Верх))
                    ВысветитьЭлемент("DM_Обрезка макушки К1/К2");
            }
            //Стойки К3
            if (conf == 8 | conf == 9)
            {
                ВысветитьЭлемент("DM_К3 стойка двери");
                ВысветитьЭлемент("DM_Анкерные отверстия К3");
                //ВысветитьЭлемент("DM_РЖК стойки-низ");
                //if (dm.Height > 1290)
                //{
                //    ВысветитьЭлемент("DM_РЖК стойки-верх");
                //    ВысветитьЭлемент("Технологические Вырезы в РЖКС-верх");
                //}
                //ВысветитьЭлемент("DM_Анкерные отверстия РЖК");

                if (dm.DownZanizhenieStoyki(pos) > 0d)
                    ВысветитьЭлемент("DM_Удлинение стойки К3 при наличнике снизу");
                if (dm.UpZaglushkaNSStoyki(pos) > 0)
                    ВысветитьЭлемент("DM_Верхняя ЗаглушкаNS на стойке К3");
                if (dm.DownZaglushkaNSStoyki(pos) > 0)
                    ВысветитьЭлемент("DM_Нижняя заглушкаNS на стойке К3");
                if (dm.IsObrezkaNalichnika(pos))
                    ВысветитьЭлемент("DM_Обрезка стойки К3");
            }
            //Макушка К3
            if (conf == 10)
            {
                ВысветитьЭлемент("DM_К3 макушка двери");
                if (dm.IsObrezkaNalichnika(Raspolozhenie.Верх))
                    ВысветитьЭлемент("DM_Обрезка макушки К3");
                if (Otvetki & dm.IsTorcShpingalet((int)Raspolozhenie.Верх))
                {
                    ВысветитьЭлемент("DM_Шпингалет в макушке К3");
                    ВысветитьЭлемент("DM_Маркер левой  К3");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К3");
                }

                if (Otvetki & dm.IsOtvAntipan((short)Raspolozhenie.Верх))
                {
                    ВысветитьЭлемент("DM_Ответка антипаники в макушке К3");
                    ВысветитьЭлемент("DM_Маркер левой  К3");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К3");
                }

                if (dm.IsTermoblock(0))
                {
                    ВысветитьЭлемент("DM_Термоблокератор в К3");
                    ВысветитьЭлемент("DM_Маркер левой  К3");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К3");
                }

                if (dm.IsSdvigoviy)
                {
                    ВысветитьЭлемент(dm.Sdvigoviy.NameVirez);
                    ВысветитьЭлемент("DM_Маркер левой  К3");
                    if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                        ВысветитьЭлемент("DM_Маркер правой  К3");
                }

                if (dm.IsAnkerInPritoloka)
                    ВысветитьЭлемент("DM_Анкерные отверстия в макушке К3");
            }
            //Порог
            if (conf == 7)
            {
                if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                {
                    ВысветитьЭлемент("DM_Порог 30 мм");
                    if (Otvetki & dm.IsTorcShpingalet((int)Raspolozhenie.Ниж))
                    {
                        ВысветитьЭлемент("DM_Шпингалет в пороге 30 мм");
                        ВысветитьЭлемент("DM_Маркер левого порога 30 мм");
                        if (dm.Name((short)com).IndexOf("_R_") >= 0)
                            ВысветитьЭлемент("DM_Маркер правого  порога 30 мм");
                    }
                    if (dm.IsTermoblock(1))
                    {
                        ВысветитьЭлемент("DM_Термоблокератор в пороге 30");
                        ВысветитьЭлемент("DM_Маркер левого порога 30 мм");
                        if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                            ВысветитьЭлемент("DM_Маркер правого  порога 30 мм");
                    }
                }
                else
                {
                    ВысветитьЭлемент("DM_Порог");
                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 41)
                        ВысветитьЭлемент("DM_Анкеры в съемном пороге");
                    if (Otvetki & dm.IsTorcShpingalet((int)Raspolozhenie.Ниж))
                    {
                        ВысветитьЭлемент("DM_Шпингалет в пороге");
                        ВысветитьЭлемент("DM_Маркер левого  порога");
                        if (dm.Name((short)com).IndexOf("_R_") >= 0)
                            ВысветитьЭлемент("DM_Маркер правого  порога");
                    }

                    if (Otvetki & dm.IsOtvAntipan((int)Raspolozhenie.Ниж))
                    {
                        ВысветитьЭлемент("DM_Ответка антипаники в пороге");
                        ВысветитьЭлемент("DM_Маркер левого  порога");
                        if (dm.Name((short)com).IndexOf("_R_") >= 0)
                            ВысветитьЭлемент("DM_Маркер правого  порога");
                    }

                    if (dm.IsTermoblock(1))
                    {
                        ВысветитьЭлемент("DM_Термоблокератор в пороге");
                        ВысветитьЭлемент("DM_Маркер левого  порога");
                        if (dm.Name((short)com).IndexOf("_R_", StringComparison.Ordinal) >= 0)
                            ВысветитьЭлемент("DM_Маркер правого  порога");
                    }
                }
            }
            //Ответки фурнитуры
            //Замки
            for (short i = 0; i < dm.Zamki.Length; i++)
            {
                if (dm.Zamok_Kod(i, (short)com) > 0)
                {
                    string vname = "_";
                    if (conf == 4 | conf == 5)
                        vname = dm.Zamok_VirezNames(i)[(int)ZamokSketchTypes.Ответка_К1];
                    else if (conf == 8 | conf == 9)
                        vname = dm.Zamok_VirezNames(i)[(int)ZamokSketchTypes.Ответка_К3];
                    if (!vname.Equals("_"))
                        ВысветитьЭлемент(vname);
                }
            }
            //if (dm.Zadvizhka_Kod((short)com) == (short)ZadvizhkaNames.Ночной_сторож)
            //{
            //    if (conf == 4 | conf == 5)
            //        ВысветитьЭлемент("DM_Ночной сторож ответка в стойке К1/К2");
            //    else if (conf == 8 | conf == 9)
            //        ВысветитьЭлемент("DM_Ночной сторож ответка в стойке К3");
            //}
            //Кодовый
            switch (dm.Kodoviy_Kod((short)com))
            {
                case 1:
                    {
                        if (conf == 4 | conf == 5)
                            ВысветитьЭлемент("DM_Кодовый (1) ответка на стойке К1/К2");
                        if (conf == 8 | conf == 9)
                            ВысветитьЭлемент("DM_Кодовый (1) ответка на стойке К3");
                        break;
                    }
                case 2:
                    {
                        if (conf == 4 | conf == 5)
                            ВысветитьЭлемент("DM_Кодовый (2) ответка на стойке К1/К2");
                        if (conf == 8 | conf == 9)
                            ВысветитьЭлемент("DM_Кодовый (2) ответка на стойке К3");
                        break;
                    }
            }
            //Задвижки
            switch (dm.Zadvizhka_Kod((short)com))
            {
                case (short)ZadvizhkaNames.Ночной_сторож:
                    {
                        if (conf == 4 | conf == 5)
                            ВысветитьЭлемент("DM_Ночной сторож ответка в стойке К1/К2");
                        if (conf == 8 | conf == 9)
                            ВысветитьЭлемент("DM_Ночной сторож ответка в стойке К3");
                        break;
                    }
            }
        }
        private void РедактироватьСтойкиДМ(ref DM dm, bool Otvetki, Command_DM com)
        {
            Raspolozhenie pos;
            short conf;
            if (com == Command_DM.Левая_стойка | com == Command_DM.Правя_стойка | com == Command_DM.Притолока)
            {
                if (dm.Otkrivanie == Otkrivanie.ЛевоеВО | dm.Otkrivanie == Otkrivanie.ПравоеВО)
                {
                    conf = (short)((short)com + 4);
                }
                else
                {
                    conf = (short)com;
                }
            }
            else if (com == Command_DM.Порог)
            {
                if (dm.Nalichnik(Raspolozhenie.Ниж) > 0)
                {
                    if (dm.Otkrivanie == Otkrivanie.ЛевоеВО | dm.Otkrivanie == Otkrivanie.ПравоеВО) conf = 10;
                    else conf = 6;
                }
                else conf = (short)com;
            }
            else conf = (short)com;

            if (com == Command_DM.Притолока) pos = Raspolozhenie.Верх;
            else if (com == Command_DM.Порог) pos = Raspolozhenie.Ниж;
            else if (com == Command_DM.Правя_стойка) pos = Raspolozhenie.Прав;
            else pos = Raspolozhenie.Лев;
            //Стойки К1/К2
            if (conf == 4 | conf == 5)
            {
                РедактироватьЭскиз("DM_ВС_К1/К2");
                ИзменитьРазмер("DM_ВС_К1/К2", "Высота", dm.HeightStoyki(pos));
                ИзменитьРазмер("DM_ВС_К1/К2", "Габарит", (float)dm.GabaritStoyki(pos));
                ИзменитьРазмер("DM_ВС_К1/К2", "Глубина", (float)dm.GlubinaStoyki(pos));
                ИзменитьРазмер("DM_ВС_К1/К2", "Стыковка", (float)dm.StikovkaStoyki(pos));
                ИзменитьРазмер("DM_ВС_К1/К2", "Занижение", (float)dm.ZanizhenieStoyki(pos));
                ИзменитьРазмер("DM_ВС_К1/К2", "Наличник", (float)dm.NalichnikStoyki(pos));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DM_Анкера К1/К2");
                ИзменитьРазмер("DM_Анкера К1/К2", "От края", (float)dm.AnkerOtKraya(pos));
                ИзменитьРазмер("DM_Анкера К1/К2", "Диаметр1", (float)dm.DAnker1Stoyki(pos));
                ИзменитьРазмер("DM_Анкера К1/К2", "Диаметр2", (float)dm.DAnker2Stoyki(pos));
                ИзменитьРазмер("DM_Анкера К1/К2", "Диаметр3", (float)dm.DAnker1Stoyki(pos));
                if (dm.DownZanizhenieStoyki(pos) > 0)
                {
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 1", dm.AnkerOtPola(1) - 100);
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 2", dm.AnkerOtPola(2) - 100);
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 3", dm.AnkerOtPola(3) - 100);
                }
                else
                {
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 1", dm.AnkerOtPola(1));
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 2", dm.AnkerOtPola(2));
                    ИзменитьРазмер("DM_Анкера К1/К2", "До 3", dm.AnkerOtPola(3));
                }
                ЗакрытьЭскиз();
                if (dm.DownZanizhenieStoyki(pos) > 0d)
                {
                    РедактироватьЭскиз("DM_Удлинение_К1/К2");
                    ИзменитьРазмер("DM_Удлинение_К1/К2", "Занижение", (float)dm.DownZanizhenieStoyki(pos));
                    ЗакрытьЭскиз();
                }

                if (dm.UpZaglushkaNSStoyki(pos) > 0)
                {
                    РедактироватьЭскиз("DM_Заглушка К1 верхняя");
                    ИзменитьРазмер("DM_Заглушка К1 верхняя", "Высота", dm.UpZaglushkaNSStoyki(pos));
                    ЗакрытьЭскиз();
                }

                if (dm.DownZaglushkaNSStoyki(pos) > 0)
                {
                    РедактироватьЭскиз("DM_Заглушка К1 нижняя");
                    ИзменитьРазмер("DM_Заглушка К1 нижняя", "Высота", dm.DownZaglushkaNSStoyki(pos));
                    ЗакрытьЭскиз();
                }

                //РедактироватьЭскиз("РЖКС-низ");
                //ИзменитьРазмер("РЖКС-низ", "ШиринаРЖК", dm.RzkWidth(pos));
                //ИзменитьРазмер("РЖКС-низ", "Высота", dm.Height > 1290 ? 1090 : dm.HeightStoyki(pos));
                //ЗакрытьЭскиз();
                //if (dm.Height > 1290)
                //{
                //    РедактироватьЭскиз("РЖКС-верх");
                //    ИзменитьРазмер("РЖКС-верх", "ШиринаРЖК", dm.RzkWidth(pos));
                //    ИзменитьРазмер("РЖКС-верх", "ВысотаРЖК", dm.RzkHeight(pos));
                //    ЗакрытьЭскиз();
                //    РедактироватьЭскиз("ТВ_РЖКС-верх");
                //    ИзменитьРазмер("ТВ_РЖКС-верх", "Отступ", 100);
                //    ЗакрытьЭскиз();
                //}

                //РедактироватьЭскиз("АнкерРЖКС");
                //ИзменитьРазмер("АнкерРЖКС", "ОтКрая", dm.RzkWidth(pos)/2);
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера1", dm.AnkerOtPola(1));
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера2", dm.AnkerOtPola(2));
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера3", dm.AnkerOtPola(3));
                //ЗакрытьЭскиз();
                
            }
            //Макушка К1
            if (conf == 6)
            {
                РедактироватьЭскиз("DM_ГС_К1/К2");
                ИзменитьРазмер("DM_ГС_К1/К2", "Ширина", dm.HeightStoyki(pos));
                ИзменитьРазмер("DM_ГС_К1/К2", "Габарит", (float)dm.GabaritStoyki(pos));
                ИзменитьРазмер("DM_ГС_К1/К2", "Глубина", (float)dm.GlubinaStoyki(pos));
                ИзменитьРазмер("DM_ГС_К1/К2", "Наличник", (float)dm.NalichnikStoyki(pos));
                ЗакрытьЭскиз();
                if (Otvetki & dm.IsTorcShpingalet((int)Raspolozhenie.Верх))
                {
                    РедактироватьЭскиз("DM_ТШМК1");
                    ИзменитьРазмер("DM_ТШМК1", "От края", (float)dm.TorcShpingalet_Otvetka_OtKraya);
                    ИзменитьРазмер("DM_ТШМК1", "Отступ", (float)dm.TorcShpingalet_Otvrtka_Otstup((int)Raspolozhenie.Верх));
                    ЗакрытьЭскиз();
                }

                if (Otvetki & dm.IsOtvAntipan((short)Raspolozhenie.Верх))
                {
                    РедактироватьЭскиз("DM_ОАМ_К1");
                    ИзменитьРазмер("DM_ОАМ_К1", "От края", (float)dm.OtvAntipan_OtKraya(0));
                    ИзменитьРазмер("DM_ОАМ_К1", "Отступ", (float)dm.OtvAntipan_Otstup(0));
                    ЗакрытьЭскиз();
                }

                if (dm.IsTermoblock(0))
                {
                    РедактироватьЭскиз("DM_ТБ_К1");
                    ИзменитьРазмер("DM_ТБ_К1", "ОтКрая", (float)dm.GetTermoblock(0).OtKraya);
                    ИзменитьРазмер("DM_ТБ_К1", "Отступ", (float)dm.GetTermoblock(0).Otstup);
                    ЗакрытьЭскиз();
                }

                if (dm.IsSdvigoviy)
                {
                    var sd = dm.Sdvigoviy;
                    РедактироватьЭскиз(sd.NameSkatch);
                    ИзменитьРазмер(sd.NameSkatch, "ОтКрая", (float)sd.OtKraya);
                    ИзменитьРазмер(sd.NameSkatch, "Отступ", (float)sd.Otstup);
                    ЗакрытьЭскиз();
                }
            }
            //Стойки К3
            if (conf == 8 | conf == 9)
            {
                РедактироватьЭскиз("DM_ВС_К3");
                ИзменитьРазмер("DM_ВС_К3", "Высота", dm.HeightStoyki(pos));
                ИзменитьРазмер("DM_ВС_К3", "Глубина", (float)dm.GlubinaStoyki(pos));
                ИзменитьРазмер("DM_ВС_К3", "Наличник", (float)dm.NalichnikStoyki(pos));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DM_Анкера К3");
                ИзменитьРазмер("DM_Анкера К3", "От края", (float)dm.AnkerOtKraya(pos));
                ИзменитьРазмер("DM_Анкера К3", "Диаметр1", (float)dm.DAnker1Stoyki(pos));
                ИзменитьРазмер("DM_Анкера К3", "Диаметр2", (float)dm.DAnker2Stoyki(pos));
                ИзменитьРазмер("DM_Анкера К3", "Диаметр3", (float)dm.DAnker1Stoyki(pos));
                if (dm.DownZanizhenieStoyki(pos) > 0)
                {
                    ИзменитьРазмер("DM_Анкера К3", "До 1", dm.AnkerOtPola(1) - 100);
                    ИзменитьРазмер("DM_Анкера К3", "До 2", dm.AnkerOtPola(2) - 100);
                    ИзменитьРазмер("DM_Анкера К3", "До 3", dm.AnkerOtPola(3) - 100);
                }
                else
                {
                    ИзменитьРазмер("DM_Анкера К3", "До 1", dm.AnkerOtPola(1));
                    ИзменитьРазмер("DM_Анкера К3", "До 2", dm.AnkerOtPola(2));
                    ИзменитьРазмер("DM_Анкера К3", "До 3", dm.AnkerOtPola(3));
                }
                ЗакрытьЭскиз();
                if (dm.UpZaglushkaNSStoyki(pos) > 0)
                {
                    РедактироватьЭскиз("DM_Заглушка К3 верхняя");
                    ИзменитьРазмер("DM_Заглушка К3 верхняя", "Высота", dm.UpZaglushkaNSStoyki(pos));
                    ЗакрытьЭскиз();
                }

                if (dm.DownZaglushkaNSStoyki(pos) > 0)
                {
                    РедактироватьЭскиз("DM_Заглушка К3 нижняя");
                    ИзменитьРазмер("DM_Заглушка К3 нижняя", "Высота", dm.DownZaglushkaNSStoyki(pos));
                    ЗакрытьЭскиз();
                }

                //РедактироватьЭскиз("РЖКС-низ");
                //ИзменитьРазмер("РЖКС-низ", "ШиринаРЖК", dm.RzkWidth(pos));
                //ИзменитьРазмер("РЖКС-низ", "Высота", dm.Height > 1290 ? 1090 : dm.HeightStoyki(pos));
                //ЗакрытьЭскиз();
                //if (dm.Height > 1290)
                //{
                //    РедактироватьЭскиз("РЖКС-верх");
                //    ИзменитьРазмер("РЖКС-верх", "ШиринаРЖК", dm.RzkWidth(pos));
                //    ИзменитьРазмер("РЖКС-верх", "ВысотаРЖК", dm.RzkHeight(pos));
                //    ЗакрытьЭскиз();
                //    РедактироватьЭскиз("ТВ_РЖКС-верх");
                //    ИзменитьРазмер("ТВ_РЖКС-верх", "Отступ", 100);
                //    ЗакрытьЭскиз();
                //}
                //РедактироватьЭскиз("АнкерРЖКС");
                //ИзменитьРазмер("АнкерРЖКС", "ОтКрая", dm.RzkWidth(pos) / 2);
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера1", dm.AnkerOtPola(1));
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера2", dm.AnkerOtPola(2));
                //ИзменитьРазмер("АнкерРЖКС", "ДоАнкера3", dm.AnkerOtPola(3));
                //ЗакрытьЭскиз();
            }
            //Макушка К3
            if (conf == 10)
            {
                РедактироватьЭскиз("DM_ГС_К3");
                ИзменитьРазмер("DM_ГС_К3", "Ширина", dm.HeightStoyki(pos));
                ИзменитьРазмер("DM_ГС_К3", "Глубина", (float)dm.GlubinaStoyki(pos));
                ИзменитьРазмер("DM_ГС_К3", "Наличник", (float)dm.NalichnikStoyki(pos));
                ЗакрытьЭскиз();
                if (Otvetki & dm.IsTorcShpingalet((short)Raspolozhenie.Верх))
                {
                    РедактироватьЭскиз("DM_ТШМК3");
                    ИзменитьРазмер("DM_ТШМК3", "От края", (float)dm.TorcShpingalet_Otvetka_OtKraya);
                    ИзменитьРазмер("DM_ТШМК3", "Отступ", (float)dm.TorcShpingalet_Otvrtka_Otstup((int)Raspolozhenie.Верх));
                    ЗакрытьЭскиз();
                }
                if (Otvetki & dm.IsOtvAntipan((short)Raspolozhenie.Верх))
                {
                    РедактироватьЭскиз("DM_ОАМ_К3");
                    ИзменитьРазмер("DM_ОАМ_К3", "От края", (float)dm.OtvAntipan_OtKraya(0));
                    ИзменитьРазмер("DM_ОАМ_К3", "Отступ", (float)dm.OtvAntipan_Otstup(0));
                    ЗакрытьЭскиз();
                }
                if (dm.IsTermoblock(0))
                {
                    РедактироватьЭскиз("DM_ТБ_К3");
                    ИзменитьРазмер("DM_ТБ_К3", "ОтКрая", (float)dm.GetTermoblock(0).OtKraya);
                    ИзменитьРазмер("DM_ТБ_К3", "Отступ", (float)dm.GetTermoblock(0).Otstup);
                    ЗакрытьЭскиз();
                }
                if (dm.IsSdvigoviy)
                {
                    var sd = dm.Sdvigoviy;
                    РедактироватьЭскиз(sd.NameSkatch);
                    ИзменитьРазмер(sd.NameSkatch, "ОтКрая", (float)sd.OtKraya);
                    ИзменитьРазмер(sd.NameSkatch, "Отступ", (float)sd.Otstup);
                    ЗакрытьЭскиз();
                }
            }
            //Порог
            if (conf == 7)
            {
                string skPor, skPorTS, skPorOAP, skPorTerm;
                if(dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                {
                    skPor = "DM_Порог_30";
                    skPorTS = "DM_ТШП_30";
                    skPorOAP = "DM_ОАП";
                    skPorTerm = "DM_ТБ_ПОР_30";
                }
                else
                {
                    skPor = "DM_Порог_ДМ";
                    skPorTS = "DM_ТШП";
                    skPorOAP = "DM_ОАП";
                    skPorTerm = "DM_ТБ_ПОР";
                }
                РедактироватьЭскиз(skPor);
                ИзменитьРазмер(skPor, "Ширина", dm.HeightStoyki(Raspolozhenie.Ниж));
                ИзменитьРазмер(skPor, "Глубина", (float)dm.GlubinaStoyki(Raspolozhenie.Ниж));
                ИзменитьРазмер(skPor, "Стыковка", (float)dm.GabaritStoyki(Raspolozhenie.Ниж));
                ЗакрытьЭскиз();
                if (Otvetki & dm.IsTorcShpingalet((short)Raspolozhenie.Ниж))
                {
                    РедактироватьЭскиз(skPorTS);
                    ИзменитьРазмер(skPorTS, "От края", (float)dm.TorcShpingalet_Otvetka_OtKraya);
                    ИзменитьРазмер(skPorTS, "Отступ", (float)dm.TorcShpingalet_Otvrtka_Otstup(1));
                    ЗакрытьЭскиз();
                }
                if (Otvetki & (dm.Zamok_Kod(0, (short)com) >= 104 | dm.Zamok_Kod(0, (short)com) <= 106))
                {
                    РедактироватьЭскиз(skPorOAP);
                    ИзменитьРазмер(skPorOAP, "Диаметр", (float)dm.OtvAntipan_Diam(1));
                    ИзменитьРазмер(skPorOAP, "От края", (float)dm.OtvAntipan_OtKraya(1));
                    ИзменитьРазмер(skPorOAP, "Отступ", (float)(dm.OtvAntipan_Otstup(1) - 3.6d));
                    ЗакрытьЭскиз();
                }
                if (dm.IsTermoblock(1))
                {
                    РедактироватьЭскиз(skPorTerm);
                    ИзменитьРазмер(skPorTerm, "ОтКрая", (float)dm.GetTermoblock(1).OtKraya);
                    ИзменитьРазмер(skPorTerm, "Отступ", (float)dm.GetTermoblock(1).Otstup);
                    ЗакрытьЭскиз();
                }
            }
            //Ответки фурнитуры
            for (short i = 0; i < dm.Zamki.Length; i++)
            {
                if (dm.Zamok_Kod(i, (short)com) > 0)
                {
                    string sname = "_";
                    if (conf == 4 | conf == 5)
                        sname = dm.Zamok_SketchNames(i)[(int)ZamokSketchTypes.Ответка_К1];
                    else if (conf == 8 | conf == 9)
                        sname = dm.Zamok_SketchNames(i)[(int)ZamokSketchTypes.Ответка_К3];
                    if (!sname.Equals("_"))
                    {
                        РедактироватьЭскиз(sname);
                        if(dm.DownZanizhenieStoyki(pos) > 0)
                            ИзменитьРазмер(sname, "От пола", (float)dm.Zamok_Otvetka_OtPola(i) - 100);
                        else
                            ИзменитьРазмер(sname, "От пола", (float)dm.Zamok_Otvetka_OtPola(i));
                        ИзменитьРазмер(sname, "Отступ", (float)dm.Zamok_Otstup(i));
                        ЗакрытьЭскиз();
                    }
                }
            }
            if (dm.Zadvizhka_Kod((short)com) == (short)ZadvizhkaNames.Ночной_сторож)
            {
                string sname = "_";
                if (conf == 4 | conf == 5)
                    sname = "DM_Ночник К1";
                else if (conf == 8 | conf == 9)
                    sname = "DM_Ночник К3";
                if (!sname.Equals("_"))
                {
                    РедактироватьЭскиз(sname);
                    if (dm.DownZanizhenieStoyki(pos) > 0d)
                        ИзменитьРазмер(sname, "От пола", dm.Zadvizhka_OtPola - 100);
                    else
                        ИзменитьРазмер(sname, "От пола", dm.Zadvizhka_OtPola);
                    ИзменитьРазмер(sname, "Отступ", (float)dm.Zadvizhka_Otstup(dm.Stoyka_Type(pos)));
                    ЗакрытьЭскиз();
                }
            }
            switch (dm.Kodoviy_Kod((short)com))
            {
                case 1:
                    {
                        if (conf == 4 | conf == 5)
                        {
                            РедактироватьЭскиз("DM_Кодовый1К1");
                            ИзменитьРазмер("DM_Кодовый1К1", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                        }
                        if (conf == 8 | conf == 9)
                        {
                            РедактироватьЭскиз("DM_Кодовый1К3");
                            ИзменитьРазмер("DM_Кодовый1К3", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                        }
                        break;
                    }
                case 2:
                    {
                        if (conf == 4 | conf == 5)
                        {
                            РедактироватьЭскиз("DM_Кодовый1К1");
                            ИзменитьРазмер("DM_Кодовый1К1", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                        }
                        if (conf == 8 | conf == 9)
                        {
                            РедактироватьЭскиз("DM_Кодовый1К3");
                            ИзменитьРазмер("DM_Кодовый1К3", "От пола", dm.Kodoviy_OtPola);
                            ЗакрытьЭскиз();
                        }

                        break;
                    }
            }
        }

        private void ВысветитьLM(ref LM lm, Command_LM com)
        {
            if (com == Command_LM.Полотна)
            {
                ВысветитьЭлемент("LM_Лицевой лист активки");
                ВысветитьЭлемент("LM_Гибы лицевого активки");
                ВысветитьЭлемент("LM_Внутренний лист активки");
                ВысветитьЭлемент("LM_Противосъем на активке");
                if (lm.IsPassivka)
                {
                    ВысветитьЭлемент("LM_Лицевой лист пассивки");
                    ВысветитьЭлемент("LM_Гибы лицевого пассивки");
                    ВысветитьЭлемент("LM_Вырезы по притвору");
                    ВысветитьЭлемент("LM_Торцевые шпингалеты");
                    ВысветитьЭлемент("LM_Внутренний лист пассивки");
                    ВысветитьЭлемент("LM_Противосъем на пассивке");
                }

                switch (lm.Zamok_Kod)
                {
                    case 1:
                    {
                        ВысветитьЭлемент("LM_ПП замок в активке");
                        if (lm.IsPassivka)
                            ВысветитьЭлемент("LM_ПП замок в пассивке");
                        break;
                    }
                    case 2:
                    {
                        ВысветитьЭлемент("LM_ПП_БЦ замок в активке");
                        if (lm.IsPassivka)
                            ВысветитьЭлемент("LM_ПП_БЦ замок в пассивке");
                        break;
                    }
                    case 5:
                    {
                        ВысветитьЭлемент("LM_ЗВ8 в активке");
                        if (lm.IsPassivka)
                            ВысветитьЭлемент("LM_ЗВ8 в пассивке");
                        break;
                    }

                    case 6:
                    {
                        ВысветитьЭлемент("LM_М842 в активке");
                        if (lm.IsPassivka)
                            ВысветитьЭлемент("LM_М842 в пасивке");
                        break;
                    }

                    case 20:
                    {
                        ВысветитьЭлемент("LM_ПП без ручки в активке");
                        if (lm.IsPassivka)
                            ВысветитьЭлемент("LM_ПП без ручки в пассивке");
                        break;
                    }
                }

                switch (lm.Ruchka_Kod)
                {
                    case (short)RuchkaNames.Ручка_РЯ_180:
                        ВысветитьЭлемент("LM_Ручка РЯ-180");
                        break;
                }

                switch (lm.Zadvizhka_Kod)
                {
                    case (short) ZadvizhkaNames.Ночной_сторож:
                        ВысветитьЭлемент("LM_Под_вертушок");
                        ВысветитьЭлемент("LM_Тело_ночного_сторожа");
                        if(lm.IsPassivka)
                            ВысветитьЭлемент("LM_Ответка_в_пассивке");
                        break;
                    case (short) ZadvizhkaNames.ЗТ_150:
                        ВысветитьЭлемент("LM_ЗТ-150_Хрущево");
                        break;
                }
            }
            else
            {
                ВысветитьЭлемент("LM_Стойка вертикальная левая");
                if (lm.IsPassivka)
                    ВысветитьЭлемент("LM_Противосъем в замковой");
                ВысветитьЭлемент("LM_Стойка вертикальная правая");
                ВысветитьЭлемент("LM_Анкеры");
                ВысветитьЭлемент("LM_Стойка горизонтальная верхняя");
                ВысветитьЭлемент("LM_Стойка горизонтальная нижняя");
                if (!lm.IsPassivka)
                {
                    switch (lm.Zamok_Kod)
                    {
                        case 1:
                            {
                                ВысветитьЭлемент("LM_ПП замок в стойке");
                                break;
                            }
                        case 2:
                            {
                                ВысветитьЭлемент("LM_ПП_БЦ замок в стойке");
                                break;
                            }

                        case 5:
                            {
                                ВысветитьЭлемент("LM_ЗВ8 в замковой");
                                break;
                            }

                        case 6:
                            {
                                ВысветитьЭлемент("LM_М842 в замковой");
                                break;
                            }

                        case 20:
                            {
                                ВысветитьЭлемент("LM_ПП без ручки в стойке");
                                break;
                            }
                    }

                    switch (lm.Zadvizhka_Kod)
                    {
                        case (short)ZadvizhkaNames.Ночной_сторож:
                            ВысветитьЭлемент("LM_Ответка_в_стойке");
                            break;
                    }
                }
            }
        }
        private void РедактироватьLM(ref LM lm, Command_LM com)
        {
            if (com == Command_LM.Полотна)
            {
                РедактироватьЭскиз("LM_ЛЛАС");
                ИзменитьРазмер("LM_ЛЛАС", "Ширина", lm.LicevoyList_Width(Stvorka.Активная));
                ИзменитьРазмер("LM_ЛЛАС", "Высота", lm.LicevoyList_Height);
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_ВЛАС");
                ИзменитьРазмер("LM_ВЛАС", "Ширина", lm.VnutrenniyList_Width(Stvorka.Активная));
                ИзменитьРазмер("LM_ВЛАС", "Высота", lm.VnutrenniyList_Height);
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_Гибы_ЛЛАС");
                ИзменитьРазмер("LM_Гибы_ЛЛАС", "Левый гиб", lm.GetGib(Raspolozhenie.Лев));
                ИзменитьРазмер("LM_Гибы_ЛЛАС", "Правый гиб", lm.GetGib(Raspolozhenie.Прав));
                ИзменитьРазмер("LM_Гибы_ЛЛАС", "Верхний гиб", lm.GetGib(Raspolozhenie.Верх));
                ИзменитьРазмер("LM_Гибы_ЛЛАС", "Нижний гиб", lm.GetGib(Raspolozhenie.Ниж));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_Противосъем АС");
                ИзменитьРазмер("LM_Противосъем АС", "От края", lm.Protivos_OtKraya);
                ЗакрытьЭскиз();
                if (lm.IsPassivka)
                {
                    РедактироватьЭскиз("LM_ЛЛПС");
                    ИзменитьРазмер("LM_ЛЛПС", "Ширина", lm.LicevoyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("LM_ВЛПС");
                    ИзменитьРазмер("LM_ВЛПС", "Ширина", lm.VnutrenniyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }

                switch (lm.Zamok_Kod)
                {
                    case (int)ZamokNames.ПП:
                    case 7:
                    case 8:
                    case (int)LM_ZamokNames.ПП_без_ручки:
                        РедактироватьЭскиз("LM_ПП АС");
                        ИзменитьРазмер("LM_ПП АС", "От края ЛА", (float)lm.Zamok.OtKrayaLA);
                        ИзменитьРазмер("LM_ПП АС", "От края ВА", (float)lm.Zamok.OtKrayaVA);
                        ИзменитьРазмер("LM_ПП АС", "До тела ВА", (float)lm.Zamok.OtTelaVA);
                        ЗакрытьЭскиз();
                        if (lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_ПП ПС");
                            ИзменитьРазмер("LM_ПП ПС", "От края ЛП", (float)lm.Zamok.OtKrayaLP);
                            ЗакрытьЭскиз();
                        }
                        break;
                    case (int)LM_ZamokNames.ПП_без_цилиндра:
                        РедактироватьЭскиз("LM_ПП_БЦ АС");
                        ИзменитьРазмер("LM_ПП_БЦ АС", "От края ЛА", (float)lm.Zamok.OtKrayaLA);
                        ИзменитьРазмер("LM_ПП_БЦ АС", "От края ВА", (float)lm.Zamok.OtKrayaVA);
                        ИзменитьРазмер("LM_ПП_БЦ АС", "До тела ВА", (float)lm.Zamok.OtTelaVA);
                        ЗакрытьЭскиз();
                        if (lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_ПП_БЦ ПС");
                            ИзменитьРазмер("LM_ПП_БЦ ПС", "От края ЛП", (float)lm.Zamok.OtKrayaLP);
                            ЗакрытьЭскиз();
                        }
                        break;
                    case (int)LM_ZamokNames.Просам_ЗВ_8:
                        РедактироватьЭскиз("LM_ЗВ8 АС");
                        ИзменитьРазмер("LM_ЗВ8 АС", "От края ЛА", (float)lm.Zamok.OtKrayaLA);
                        ИзменитьРазмер("LM_ЗВ8 АС", "От края ВА", (float)lm.Zamok.OtKrayaVA);
                        ИзменитьРазмер("LM_ЗВ8 АС", "До тела ВА", (float)lm.Zamok.OtTelaVA);
                        ЗакрытьЭскиз();
                        if (lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_ЗВ8 ПС");
                            ИзменитьРазмер("LM_ЗВ8 ПС", "От края ЛП", (float)lm.Zamok.OtKrayaLP);
                            ЗакрытьЭскиз();
                        }
                        break;

                    case (int)LM_ZamokNames.Меттем_842:
                        РедактироватьЭскиз("LM_М842 ЛА");
                        ИзменитьРазмер("LM_М842 ЛА", "От края ЛА", (float)lm.Zamok.OtKrayaLA);
                        ИзменитьРазмер("LM_М842 ЛА", "От края ВА", (float)lm.Zamok.OtKrayaVA);
                        ИзменитьРазмер("LM_М842 ЛА", "До тела ВА", (float)lm.Zamok.OtTelaVA);
                        ЗакрытьЭскиз();
                        if (lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_М842 ЛП");
                            ИзменитьРазмер("LM_М842 ЛП", "От края ЛП", (float)lm.Zamok.OtKrayaLP);
                            ЗакрытьЭскиз();
                        }
                        if (lm.Ruchka_Kod == (short)RuchkaNames.Ручка_РЯ_180)
                        {
                            РедактироватьЭскиз("LM_РЯ-180");
                            ИзменитьРазмер("LM_РЯ-180", "От края", 170f);
                            ИзменитьРазмер("LM_РЯ-180", "От центра", 100f);
                            ЗакрытьЭскиз();
                        }
                        break;
                }

                switch (lm.Zadvizhka_Kod)
                {
                    case (short)ZadvizhkaNames.Ночной_сторож:
                        РедактироватьЭскиз("LM_НС_Вертошок");
                        ИзменитьРазмер("LM_НС_Вертошок", "Отступ", (float)lm.Zadvizhka_Vertushok_OtKraya);
                        ИзменитьРазмер("LM_НС_Вертошок", "От центра", (float)lm.Zadvizhka_OtCentra);
                        ЗакрытьЭскиз();
                        РедактироватьЭскиз("LM_НС_ВЛАС");
                        ИзменитьРазмер("LM_НС_ВЛАС", "ЗадвижкаОтКрая", (float)lm.Zadvizhka.OtKrayaVA);
                        ИзменитьРазмер("LM_НС_ВЛАС", "От центра", (float)lm.Zadvizhka_OtCentra);
                        ЗакрытьЭскиз();
                        if (lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_НС_ЛЛПС");
                            ИзменитьРазмер("LM_НС_ЛЛПС", "От центра", (float) lm.Zadvizhka_OtCentra);
                            ЗакрытьЭскиз();
                        }
                        break;
                }
            }
            else
            {
                РедактироватьЭскиз("LM_Стойка левая");
                ИзменитьРазмер("LM_Стойка левая", "Высота", lm.Stoyka_Height(Raspolozhenie.Лев));
                ИзменитьРазмер("LM_Стойка левая", "Развертка", (float)lm.Stoyka_Razvertka(Raspolozhenie.Лев));
                ИзменитьРазмер("LM_Стойка левая", "Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Лев));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_Стойка правая");
                ИзменитьРазмер("LM_Стойка правая", "Развертка", (float)lm.Stoyka_Razvertka(Raspolozhenie.Прав));
                ИзменитьРазмер("LM_Стойка правая", "Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Прав));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_Стойка верхняя");
                ИзменитьРазмер("LM_Стойка верхняя", "Ширина", lm.Stoyka_Height(Raspolozhenie.Верх));
                ИзменитьРазмер("LM_Стойка верхняя", "Развертка", (float)lm.Stoyka_Razvertka(Raspolozhenie.Верх));
                ИзменитьРазмер("LM_Стойка верхняя", "Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Верх));
                ЗакрытьЭскиз();
                РедактироватьЭскиз("LM_Стойка нижняя");
                ИзменитьРазмер("LM_Стойка нижняя", "Развертка", (float)lm.Stoyka_Razvertka(Raspolozhenie.Ниж));
                ИзменитьРазмер("LM_Стойка нижняя", "Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Ниж));
                ЗакрытьЭскиз();

                switch (lm.Zadvizhka_Kod)
                {
                    case (short)ZadvizhkaNames.Ночной_сторож:
                        if (!lm.IsPassivka)
                        {
                            РедактироватьЭскиз("LM_НС_К1");
                            ИзменитьРазмер("LM_НС_К1", "Отступ", (float)lm.Zadvizhka_Otstup);
                            ИзменитьРазмер("LM_НС_К1", "От центра", (float)lm.Zadvizhka_OtCentra);
                            ЗакрытьЭскиз();
                        }
                        break;
                }
            }
        }

        private void ВысветитьDVM(ref DVM dvm, Command_VM com)
        {
            if (com == Command_VM.Полотна)
            {
                ВысветитьЭлемент("DVM_Лицевой лист активки");
                ВысветитьЭлемент("DVM_Внутренний лист активки");
                if (dvm.IsKalitka)
                {
                    ВысветитьЭлемент("DVM_Калитка в лицевом");
                    ВысветитьЭлемент("DVM_Калитка во внутреннем");
                    ВысветитьЭлемент("DVM_Лицевой лист калитки");
                    ВысветитьЭлемент("DVM_Внутренний лист калитки");
                }

                if (dvm.IsPassivka)
                {
                    ВысветитьЭлемент("DVM_Лицевой лист пассивки");
                    ВысветитьЭлемент("DVM_Внутренний лист пассивки");
                }
            }
            else if (com == Command_VM.Коробка)
            {
                ВысветитьЭлемент("DVM_Стойка ворот левая");
                ВысветитьЭлемент("DVM_Стойка ворот правая");
                ВысветитьЭлемент("DVM_Анкеры в стойках");
                ВысветитьЭлемент("DVM_Притолока ворот");
                ВысветитьЭлемент("DVM_Анкер в макушке");
                ВысветитьЭлемент("DVM_Вертикальная стойка активной (петлевая)");
                ВысветитьЭлемент("DVM_Горизонтальная стойка активки");
                ВысветитьЭлемент("DVM_В профиле активки");
                ВысветитьЭлемент("DVM_Противосъем в петлевом профиле");
                if (dvm.Porog_Num > 0)
                {
                    ВысветитьЭлемент("DVM_Порог ворот");
                    ВысветитьЭлемент("DVM_Анкер в пороге");
                    if (dvm.Porog_Dobor_Height == 0)
                        ВысветитьЭлемент("DVM_Анкер в пороге без добора");
                }

                if (dvm.Stoyka_Dobor_Height > 0)
                {
                    ВысветитьЭлемент("DVM_Добор стойки ворот левой");
                    ВысветитьЭлемент("DVM_Добор стойки ворот правой");
                    ВысветитьЭлемент("DVM_Анкеры в доборах стоек");
                    if (dvm.Korobka_IsRazbornaya)
                    {
                        ВысветитьЭлемент("DVM_Разборная в доборе под макушку");
                    }
                    else
                    {
                        ВысветитьЭлемент("DVM_Неразборная в доборах по макушке");
                    }
                }

                if (dvm.Pritoloka_Dobor_Height > 0)
                {
                    ВысветитьЭлемент("DVM_Добор притолоки ворот");
                    ВысветитьЭлемент("DVM_Анкер в доборе макушки");
                }

                if (dvm.Pritoloka_Dobor_Height > 0 & dvm.Porog_Num > 0)
                {
                    ВысветитьЭлемент("DVM_Добор порога ворот");
                    ВысветитьЭлемент("DVM_Анкер в доборе порога");
                }

                if (dvm.Stoyka_Dobor_Height == 0)
                    ВысветитьЭлемент("DVM_Анкеры в стойках без доборов");
                if (dvm.Pritoloka_Dobor_Height == 0)
                    ВысветитьЭлемент("DVM_Анкер в макушке без добора");
                if (dvm.Korobka_IsRazbornaya)
                {
                    if (dvm.Stoyka_Dobor_Height == 0)
                        ВысветитьЭлемент("DVM_Разборная под макушку");
                    if (dvm.Porog_Num == 40)
                        ВысветитьЭлемент("DVM_Разборная под порог 40");
                    if (dvm.Porog_Num == 25)
                        ВысветитьЭлемент("DVM_Разборная под порог 25");
                    if (dvm.Porog_Num == 14)
                        ВысветитьЭлемент("DVM_Разборная под порог 14");
                }
                else
                {
                    if (dvm.Porog_Num > 2)
                        ВысветитьЭлемент("DVM_Неразборная по порогу");
                    if (dvm.Stoyka_Dobor_Height == 0)
                        ВысветитьЭлемент("DVM_Неразборная по макушке");
                }

                if (dvm.VertProf_Dobor_Height > 0)
                    ВысветитьЭлемент("DVM_Добор верикальной стойки активной (петлевой)");
                if (dvm.GorProf_Dobor_Height(Stvorka.Активная) > 0)
                {
                    ВысветитьЭлемент("DVM_Добор горизонтальной стойки активной");
                    ВысветитьЭлемент("DVM_В доборе профиля активки");
                }
                else
                {
                    ВысветитьЭлемент("DVM_В профиле активки без добора");
                }

                if (dvm.IsPassivka)
                {
                    ВысветитьЭлемент("DVM_Горизонтальная стойка пассивной");
                    ВысветитьЭлемент("DVM_В профиле пассивки");
                    if (dvm.GorProf_Dobor_Height(Stvorka.Пассивная) > 0)
                    {
                        ВысветитьЭлемент("DVM_Добор горизонтальной стойки пассивной");
                        ВысветитьЭлемент("DVM_В доборе профиля пассивки");
                    }
                    else
                    {
                        ВысветитьЭлемент("DVM_В профиле пассивки без добора");
                    }

                    if (!dvm.IsKalitka | dvm.Zadvizhka == 404)
                    {
                        ВысветитьЭлемент("DVM_Вертикальная стойка пассивной (замковая)");
                        ВысветитьЭлемент("DVM_Нижний торцевой шпингалет");
                        if (dvm.VertProf_Dobor_Height > 0)
                        {
                            ВысветитьЭлемент("DVM_Добор вертикальной стойки пассивной (замковая)");
                            ВысветитьЭлемент("DVM_Верхний торцевой шпингалет в доборе");
                        }
                        else
                        {
                            ВысветитьЭлемент("DVM_Верхний торцевой шпингалет");
                        }
                    }
                }
                else
                {
                    ВысветитьЭлемент("DVM_Противосъем в петлевой стойке");
                    ВысветитьЭлемент("DVM_Замок ответка в стойке ворот");
                }

                if (dvm.IsKalitka)
                {
                    ВысветитьЭлемент("DVM_Вертикальная профиль обрамления калитки (левая)");
                    ВысветитьЭлемент("DVM_Вертикальная профиль обрамления калитки (правая)");
                    ВысветитьЭлемент("DVM_Противосъем в обрамлении");
                    ВысветитьЭлемент("DVM_Горизонтальная профиль обрамления калитки");
                    ВысветитьЭлемент("DVM_Вертикальная профиль калитки (левая)");
                    ВысветитьЭлемент("DVM_Вертикальная профиль калитки (правая)");
                    ВысветитьЭлемент("DVM_Противосъем в калитке");
                    ВысветитьЭлемент("DVM_Горизонтальный профиль калитки");
                    ВысветитьЭлемент("DVM_В профиле калитки");
                }
                else
                {
                    ВысветитьЭлемент("DVM_Вертикальная стойка активной (замковая)");
                    if (!dvm.IsPassivka)
                        ВысветитьЭлемент("DVM_Замок в профиле ворот");
                    if (dvm.VertProf_Dobor_Height > 0)
                        ВысветитьЭлемент("DVM_Добор верикальной стойки активной (замковой)");
                }
            }
            else
            {
                if (dvm.LicList_IsVertDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Вертикальный добор лицевого активной");
                if (dvm.LicList_IsGorDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Горизонтальный добор лицевого активной");
                if (dvm.LicList_IsDopDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Дополнительный горизонтальный добор лицевого активной");
                if (dvm.VnutList_IsVertDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Вертикальный добор внутреннего активной");
                if (dvm.VnutList_IsGorDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Горизонтальный добор внутреннего активной");
                if (dvm.VnutList_IsDopDobor(Stvorka.Активная))
                    ВысветитьЭлемент("DVM_Дополнительный горизонтальный добор внутреннего активной");
                if (dvm.LicList_IsVertDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Вертикальный добор лицевого пассивной");
                if (dvm.LicList_IsGorDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Горизонтальный добор лицевого пассивной");
                if (dvm.LicList_IsDopDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Дополнительный горизонтальный добор лицевого пассивной");
                if (dvm.VnutList_IsVertDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Вертикальный добор внутреннего пассивной");
                if (dvm.VnutList_IsGorDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Горизонтальный добор внутреннего пассивной");
                if (dvm.VnutList_IsDopDobor(Stvorka.Пассивная))
                    ВысветитьЭлемент("DVM_Дополнительный горизонтальный добор внутреннего пассивной");
                if (dvm.LicList_IsDopDobor(Stvorka.Активная) | dvm.VnutList_IsDopDobor(Stvorka.Активная) | dvm.LicList_IsDopDobor(Stvorka.Пассивная) | dvm.VnutList_IsDopDobor(Stvorka.Пассивная))
                {
                    ВысветитьЭлемент("DVM_Подпись доборов");
                }
            }

            switch (dvm.Zamok)
            {
                case 1:
                    {
                        if (com == Command_VM.Полотна)
                        {
                            if (dvm.IsKalitka)
                            {
                                ВысветитьЭлемент("DVM_Замок в калитке");
                            }
                            else
                            {
                                ВысветитьЭлемент("DVM_Замок в активке");
                            }
                        }
                        else if (com == Command_VM.Коробка)
                        {
                            if (dvm.IsKalitka)
                            {
                                ВысветитьЭлемент("DVM_Замок в профиле калитки");
                                ВысветитьЭлемент("DVM_Замок в обрамлении калитки");
                            }
                            else
                            {
                                ВысветитьЭлемент("DVM_Замок в профиле ворот");
                            }
                        }

                        if (dvm.IsPassivka)
                        {
                            if (!dvm.IsKalitka | dvm.Zadvizhka == 404)
                                ВысветитьЭлемент("DVM_Замок ответка в профиле ворот");
                        }
                        else
                        {
                            ВысветитьЭлемент("DVM_Замок ответка в стойке ворот");
                        }

                        break;
                    }
            }
        }
        private void РедактироватьDVM(ref DVM dvm, Command_VM com)
        {
            if (com == Command_VM.Полотна)
            {
                РедактироватьЭскиз("DVM_ЛЛАС");
                ИзменитьРазмер("DVM_ЛЛАС", "Высота", dvm.LicevoyList_Height(Stvorka.Активная));
                ИзменитьРазмер("DVM_ЛЛАС", "Ширина", dvm.LicevoyList_Width(Stvorka.Активная));
                ИзменитьРазмер("DVM_ЛЛАС", "От пола", dvm.LicevoyList_OtPola);
                ЗакрытьЭскиз();
                РедактироватьЭскиз("DVM_ВЛАС");
                ИзменитьРазмер("DVM_ВЛАС", "Высота", dvm.VnutrenniyList_Height(Stvorka.Активная));
                ИзменитьРазмер("DVM_ВЛАС", "Ширина", dvm.VnutrenniyList_Width(Stvorka.Активная));
                ИзменитьРазмер("DVM_ВЛАС", "От пола", dvm.VnutrenniyList_OtPola);
                ЗакрытьЭскиз();
                if (dvm.IsPassivka)
                {
                    РедактироватьЭскиз("DVM_ЛЛПС");
                    ИзменитьРазмер("DVM_ЛЛПС", "Ширина", dvm.LicevoyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ВЛПС");
                    ИзменитьРазмер("DVM_ВЛПС", "Ширина", dvm.VnutrenniyList_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }
                else
                {
                    РедактироватьЭскиз("DVM_ЗамокАктивки");
                    ИзменитьРазмер("DVM_ЗамокАктивки", "От края ВЛ", 62f);
                    ЗакрытьЭскиз();
                }

                if (dvm.IsKalitka)
                {
                    РедактироватьЭскиз("DVM_ВырезКалиткиЛЛ");
                    ИзменитьРазмер("DVM_ВырезКалиткиЛЛ", "Высота", dvm.Kalitka_LicList_Virez_Height);
                    ИзменитьРазмер("DVM_ВырезКалиткиЛЛ", "Ширина", dvm.Kalitka_LicList_Virez_Width);
                    ИзменитьРазмер("DVM_ВырезКалиткиЛЛ", "От пола", dvm.Kalitka_LicList_VIrez_OtPola);
                    ИзменитьРазмер("DVM_ВырезКалиткиЛЛ", "От замка", dvm.Kalitka_LicList_VIrez_OtZamkProf);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ВырезКалиткиВЛ");
                    ИзменитьРазмер("DVM_ВырезКалиткиВЛ", "Высота", dvm.Kalitka_VnutList_Virez_Height);
                    ИзменитьРазмер("DVM_ВырезКалиткиВЛ", "Ширина", dvm.Kalitka_VnutList_Virez_Width);
                    ИзменитьРазмер("DVM_ВырезКалиткиВЛ", "От пола", dvm.Kalitka_VnutList_VIrez_OtPola);
                    ИзменитьРазмер("DVM_ВырезКалиткиВЛ", "От замка", dvm.Kalitka_VnutList_VIrez_OtZamkProf);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ЛЛК");
                    ИзменитьРазмер("DVM_ЛЛК", "Высота", dvm.Kalitka_LicList_Hight);
                    ИзменитьРазмер("DVM_ЛЛК", "Ширина", dvm.Kalitka_LicList_Width);
                    ИзменитьРазмер("DVM_ЛЛК", "От пола", dvm.Kalitka_LicList_OtPola);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ВЛК");
                    ИзменитьРазмер("DVM_ВЛК", "Высота", dvm.Kalitka_VnutList_Hight);
                    ИзменитьРазмер("DVM_ВЛК", "Ширина", dvm.Kalitka_VnutList_Width);
                    ИзменитьРазмер("DVM_ВЛК", "От пола", dvm.Kalitka_VnutList_OtPola);
                    ЗакрытьЭскиз();
                }
            }
            else if (com == Command_VM.Коробка)
            {
                РедактироватьЭскиз("DVM_Стойка");
                ИзменитьРазмер("DVM_Стойка", "Высота", dvm.Stoyka_Height);
                ЗакрытьЭскиз();
                if (dvm.Stoyka_Dobor_Height != 0)
                {
                    РедактироватьЭскиз("DVM_Добор стойки");
                    ИзменитьРазмер("DVM_Добор стойки", "Высота", dvm.Stoyka_Dobor_Height);
                    ЗакрытьЭскиз();
                }

                РедактироватьЭскиз("DVM_Притолока");
                ИзменитьРазмер("DVM_Притолока", "Ширина", dvm.Pritoloka_Height);
                ЗакрытьЭскиз();
                if (dvm.Pritoloka_Dobor_Height > 0)
                {
                    РедактироватьЭскиз("DVM_Добор притолоки");
                    ИзменитьРазмер("DVM_Добор притолоки", "Ширина", dvm.Pritoloka_Dobor_Height);
                    ЗакрытьЭскиз();
                }

                if (dvm.Porog_Num > 0)
                {
                    РедактироватьЭскиз("DVM_Порог");
                    ИзменитьРазмер("DVM_Порог", "Ширина", dvm.Porog_Height);
                    ИзменитьРазмер("DVM_Порог", "Развертка", dvm.Porog_Razvertka);
                    ЗакрытьЭскиз();
                    if (dvm.Pritoloka_Dobor_Height > 0)
                    {
                        РедактироватьЭскиз("DVM_Добор порога");
                        ИзменитьРазмер("DVM_Добор порога", "Ширина", dvm.Porog_Dobor_Height);
                        ЗакрытьЭскиз();
                    }

                    РедактироватьЭскиз("DVM_АнкерПорог");
                    ИзменитьРазмер("DVM_АнкерПорог", "От края", (float)dvm.Porog_Anker);
                    ЗакрытьЭскиз();
                    if (!dvm.Korobka_IsRazbornaya)
                    {
                        РедактироватьЭскиз("DVM_НеразборнаяПоПорогу");
                        ИзменитьРазмер("DVM_НеразборнаяПоПорогу", "Высота", dvm.Porog_Virez_Height);
                        ЗакрытьЭскиз();
                    }
                }

                РедактироватьЭскиз("DVM_ВСК");
                ИзменитьРазмер("DVM_ВСК", "Высота", dvm.VertProf_Height);
                ИзменитьРазмер("DVM_ВСК", "От пола", dvm.VertProf_OtPola);
                if (dvm.IsPorog)
                {
                    ИзменитьРазмер("DVM_ВСК", "Отступ", 100 + dvm.Porog_Razvertka + 300);
                }
                else
                {
                    ИзменитьРазмер("DVM_ВСК", "Отступ", 300f);
                }

                ЗакрытьЭскиз();
                if (dvm.VertProf_Dobor_Height > 0)
                {
                    РедактироватьЭскиз("DVM_ДВП активки");
                    ИзменитьРазмер("DVM_ДВП активки", "Высота", dvm.VertProf_Dobor_Height);
                    ЗакрытьЭскиз();
                }

                РедактироватьЭскиз("DVM_ГСК активки");
                ИзменитьРазмер("DVM_ГСК активки", "Ширина", dvm.GorProf_Height(Stvorka.Активная));
                if (dvm.IsKalitka)
                {
                    ИзменитьРазмер("DVM_ГСК активки", "Отступ", 100f);
                }
                else
                {
                    ИзменитьРазмер("DVM_ГСК активки", "Отступ", 310f);
                }

                ЗакрытьЭскиз();
                if (dvm.GorProf_Dobor_Height(Stvorka.Активная) > 0)
                {
                    РедактироватьЭскиз("DVM_Добор ГСК активки");
                    ИзменитьРазмер("DVM_Добор ГСК активки", "Ширина", dvm.GorProf_Dobor_Height(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }

                if (dvm.IsPassivka)
                {
                    РедактироватьЭскиз("DVM_ГСК пассивки");
                    ИзменитьРазмер("DVM_ГСК пассивки", "Ширина", dvm.GorProf_Dobor_Height(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ЗамокВПрофиле");
                    ИзменитьРазмер("DVM_ЗамокВПрофиле", "От края профиля", 55f);
                    ЗакрытьЭскиз();
                    if (dvm.Zadvizhka == 404)
                    {
                        РедактироватьЭскиз("DVM_НТШ");
                        ИзменитьРазмер("DVM_НТШ", "От края", dvm.TorcSpingalet_OtKpaya);
                        ЗакрытьЭскиз();
                    }
                }
                else
                {
                    РедактироватьЭскиз("DVM_ЗамокВСтойке");
                    ИзменитьРазмер("DVM_ЗамокВСтойке", "От края стойки", 54.2f);
                    ЗакрытьЭскиз();
                }

                if (dvm.IsKalitka)
                {
                    РедактироватьЭскиз("DVM_ВПОК");
                    ИзменитьРазмер("DVM_ВПОК", "Высота", dvm.Kalitka_Obramlenie_VertProf);
                    ИзменитьРазмер("DVM_ВПОК", "От пола", dvm.Kalitka_OtPola);
                    ИзменитьРазмер("DVM_ВПОК", "Отступ", 720f);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ГПОК");
                    ИзменитьРазмер("DVM_ГПОК", "Ширина", dvm.Kalitka_Obramlenie_GorProf);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ВПК");
                    ИзменитьРазмер("DVM_ВПК", "Высота", dvm.Kalitka_VertProf);
                    ИзменитьРазмер("DVM_ВПК", "От пола", dvm.Kalitka_VertProf_OtPola);
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("DVM_ГПК");
                    ИзменитьРазмер("DVM_ГПК", "Ширина", dvm.Kalitka_GorProf);
                    ЗакрытьЭскиз();
                }
            }
            else
            {
                if (dvm.LicList_IsVertDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ВДЛА");
                    ИзменитьРазмер("DVM_ВДЛА", "Высота", dvm.LicList_VertDobor_Height(Stvorka.Активная));
                    ИзменитьРазмер("DVM_ВДЛА", "Ширина", dvm.LicList_VertDobor_Width(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }

                if (dvm.LicList_IsGorDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ГДЛА");
                    ИзменитьРазмер("DVM_ГДЛА", "Высота", dvm.LicList_GorDobor_Height(Stvorka.Активная));
                    ИзменитьРазмер("DVM_ГДЛА", "Ширина", dvm.LicList_GorDobor_Width(Stvorka.Активная));
                    if (dvm.LicList_IsVertDobor(Stvorka.Активная))
                    {
                        ИзменитьРазмер("DVM_ГДЛА", "Отступ", 150 + dvm.LicList_VertDobor_Width(Stvorka.Активная) + 100);
                    }
                    else
                    {
                        ИзменитьРазмер("DVM_ГДЛА", "Отступ", 150f);
                    }

                    ЗакрытьЭскиз();
                }

                if (dvm.LicList_IsDopDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ДГДЛА");
                    ИзменитьРазмер("DVM_ДГДЛА", "Ширина", dvm.LicList_DopDobor_Width(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsVertDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ВДВА");
                    ИзменитьРазмер("DVM_ВДВА", "Высота", dvm.VnutList_VertDobor_Height(Stvorka.Активная));
                    ИзменитьРазмер("DVM_ВДВА", "Ширина", dvm.VnutList_VertDobor_Width(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsGorDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ГДВА");
                    ИзменитьРазмер("DVM_ГДВА", "Высота", dvm.VnutList_GorDobor_Height(Stvorka.Активная));
                    ИзменитьРазмер("DVM_ГДВА", "Ширина", dvm.VnutList_GorDobor_Width(Stvorka.Активная));
                    if (dvm.VnutList_IsVertDobor(Stvorka.Активная))
                    {
                        ИзменитьРазмер("DVM_ГДВА", "Отступ", 150 + dvm.VnutList_VertDobor_Width(Stvorka.Активная) + 100);
                    }
                    else
                    {
                        ИзменитьРазмер("DVM_ГДВА", "Отступ", 150f);
                    }

                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsDopDobor(Stvorka.Активная))
                {
                    РедактироватьЭскиз("DVM_ДГДВА");
                    ИзменитьРазмер("DVM_ДГДВА", "Ширина", dvm.VnutList_DopDobor_Width(Stvorka.Активная));
                    ЗакрытьЭскиз();
                }

                if (dvm.LicList_IsVertDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ВДЛП");
                    ИзменитьРазмер("DVM_ВДЛП", "Высота", dvm.LicList_VertDobor_Height(Stvorka.Пассивная));
                    ИзменитьРазмер("DVM_ВДЛП", "Ширина", dvm.LicList_VertDobor_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }

                if (dvm.LicList_IsGorDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ГДЛП");
                    ИзменитьРазмер("DVM_ГДЛП", "Высота", dvm.LicList_GorDobor_Height(Stvorka.Пассивная));
                    ИзменитьРазмер("DVM_ГДЛП", "Ширина", dvm.LicList_GorDobor_Width(Stvorka.Пассивная));
                    if (dvm.LicList_IsVertDobor(Stvorka.Пассивная))
                    {
                        ИзменитьРазмер("DVM_ГДЛП", "Отступ", 150 + dvm.LicList_VertDobor_Width(Stvorka.Пассивная) + 100);
                    }
                    else
                    {
                        ИзменитьРазмер("DVM_ГДЛП", "Отступ", 150f);
                    }

                    ЗакрытьЭскиз();
                }

                if (dvm.LicList_IsDopDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ДГДЛП");
                    ИзменитьРазмер("DVM_ДГДЛП", "Ширина", dvm.LicList_DopDobor_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsVertDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ВДВП");
                    ИзменитьРазмер("DVM_ВДВП", "Высота", dvm.VnutList_VertDobor_Height(Stvorka.Пассивная));
                    ИзменитьРазмер("DVM_ВДВП", "Ширина", dvm.VnutList_VertDobor_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsGorDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ГДВП");
                    ИзменитьРазмер("DVM_ГДВП", "Высота", dvm.VnutList_GorDobor_Height(Stvorka.Пассивная));
                    ИзменитьРазмер("DVM_ГДВП", "Ширина", dvm.VnutList_GorDobor_Width(Stvorka.Пассивная));
                    if (dvm.VnutList_IsVertDobor(Stvorka.Пассивная))
                    {
                        ИзменитьРазмер("DVM_ГДВП", "Отступ", 150 + dvm.VnutList_VertDobor_Width(Stvorka.Пассивная) + 100);
                    }
                    else
                    {
                        ИзменитьРазмер("DVM_ГДВП", "Отступ", 150f);
                    }

                    ЗакрытьЭскиз();
                }

                if (dvm.VnutList_IsDopDobor(Stvorka.Пассивная))
                {
                    РедактироватьЭскиз("DVM_ДГДВП");
                    ИзменитьРазмер("DVM_ДГДВП", "Ширина", dvm.VnutList_DopDobor_Width(Stvorka.Пассивная));
                    ЗакрытьЭскиз();
                }
            }
        }
        private void РедактироватьПодписиДоборов(ref DVM dvm)
        {
            SldWorks.Configuration config;
            SldWorks.CustomPropertyManager cusPropMgr;
            config = (SldWorks.Configuration)Maket.GetActiveConfiguration();
            cusPropMgr = config.CustomPropertyManager;
            РедактироватьЭскиз("DVM_Подпись_доборов");
            _ = cusPropMgr.Set2("ВысотаВДАЛ", dvm.LicList_VertDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаВДАЛ", dvm.LicList_VertDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаГДАЛ", dvm.LicList_GorDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаГДАЛ", dvm.LicList_GorDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаДДАЛ", dvm.LicList_DopDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаДДАЛ", dvm.LicList_DopDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаВДАВ", dvm.VnutList_VertDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаВДАВ", dvm.VnutList_VertDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаГДАВ", dvm.VnutList_GorDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаГДАВ", dvm.VnutList_GorDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаДДАВ", dvm.VnutList_DopDobor_Height(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ШиринаДДАВ", dvm.VnutList_DopDobor_Width(Stvorka.Активная).ToString());
            _ = cusPropMgr.Set2("ВысотаВДПЛ", dvm.LicList_VertDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаВДПЛ", dvm.LicList_VertDobor_Width(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ВысотаГДПЛ", dvm.LicList_GorDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаГДПЛ", dvm.LicList_GorDobor_Width(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ВысотаДДПЛ", dvm.LicList_DopDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаДДПЛ", dvm.LicList_DopDobor_Width(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ВысотаВДПВ", dvm.VnutList_VertDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаВДПВ", dvm.VnutList_VertDobor_Width(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ВысотаГДПВ", dvm.VnutList_GorDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаГДПВ", dvm.VnutList_GorDobor_Width(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ВысотаДДПВ", dvm.VnutList_DopDobor_Height(Stvorka.Пассивная).ToString());
            _ = cusPropMgr.Set2("ШиринаДДПВ", dvm.VnutList_DopDobor_Width(Stvorka.Пассивная).ToString());
            ЗакрытьЭскиз();
            _ = Maket.ForceRebuild3(false);
        }

        private static short GetConf(Command_ODL com, Otkrivanie otkrivanie)
        {
            switch (com)
            {
                case Command_ODL.Полотно_активки:
                    return 0;
                case Command_ODL.Полотно_пассивки:
                    return 1;
                case Command_ODL.Замковая_стойка:
                    {
                        if (otkrivanie == Otkrivanie.Левое | otkrivanie == Otkrivanie.Правое) return 2;
                        return 9;
                    }
                case Command_ODL.Петлевая_стойка:
                    {
                        if (otkrivanie == Otkrivanie.Левое | otkrivanie == Otkrivanie.Правое) return 3;
                        return 10;
                    }
                case Command_ODL.Притолока:
                    {
                        if (otkrivanie == Otkrivanie.Левое | otkrivanie == Otkrivanie.Правое) return 4;
                        return 11;
                    }
                case Command_ODL.Замковой_профиль_активки:
                    return 5;
                case Command_ODL.Замковой_профиль_пассивки:
                    return 7;
                case Command_ODL.Петлевой_профиль:
                    return 6;
                case Command_ODL.Порог:
                    return 13;

            }
            if (otkrivanie == Otkrivanie.Левое | otkrivanie == Otkrivanie.Правое) return 8;
            else return 12;
        }
        private void ВысветитьODL(ref ODL odl, Command_ODL com)
        {
            var conf = GetConf(com, odl.Otkrivanie);

            switch (conf)
            {
                case 0:
                    ВысветитьЭлемент("ODL_Лист активки");
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            ВысветитьЭлемент("ODL_ПП(стандарт) в активке");
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:
                            ВысветитьЭлемент("ODL_ЗВ8 в активке");
                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            ВысветитьЭлемент("ODL_Гардиан 12.11+Вега в активке");
                            break;
                        case (int)ZamokNames.Почтовый:
                            ВысветитьЭлемент("ODL_Почтовый в активке");
                            break;
                        case (int)ZamokNames.Гардиан_10_01:
                            ВысветитьЭлемент("ODL_Гардиан 10.01 в активке");
                            break;
                    }
                    switch (odl.Ruchka)
                    {
                        case (int)RuchkaNames.Ручка_кнопка:
                            ВысветитьЭлемент("ODL_РДК-110 в активке");
                            break;
                        case (int)RuchkaNames.Ручка_Потайная:
                            ВысветитьЭлемент("ODL_Потайная в активке");
                            break;
                        case (int)RuchkaNames.Ручка_РЯ_180:
                            ВысветитьЭлемент("ODL_Ручка РЯ в активке");
                            break;
                    }
                    break;
                case 1:
                    ВысветитьЭлемент("ODL_Лист пассивки");
                    break;
                case 2:
                    ВысветитьЭлемент("ODL_Стойка С1");
                    ВысветитьЭлемент("ODL_Анкеры в С1");
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            ВысветитьЭлемент("ODL_ПП(стандарт) в стойке С1");
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:
                            ВысветитьЭлемент("ODL_ЗВ8 в стойке С1");
                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            ВысветитьЭлемент("ODL_Гардиан 12.11+Вега в стойке С1");
                            break;
                        case (int)ZamokNames.Почтовый:
                            ВысветитьЭлемент("ODL_Почтовый в стойке С1");
                            break;
                        case (int)ZamokNames.Гардиан_10_01:
                            ВысветитьЭлемент("ODL_Гардиан 10.01 в стойке С1");
                            break;
                    }
                    break;
                case 3:
                    ВысветитьЭлемент("ODL_Стойка С1");
                    ВысветитьЭлемент("ODL_Анкеры в С1");
                    break;
                case 4:
                    ВысветитьЭлемент("ODL_Притолока С1");
                    break;
                case 5:
                    ВысветитьЭлемент("ODL_Замковой профиль активки");
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            ВысветитьЭлемент("ODL_ПП(стандарт) в профиле активки");
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:
                            ВысветитьЭлемент("ODL_ЗВ8 в профиле активки");
                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            ВысветитьЭлемент("ODL_Гардиан 12.11+Вега в профиле активки");
                            break;
                        case (int)ZamokNames.Почтовый:
                            ВысветитьЭлемент("ODL_Почтовый в профиле активки");
                            break;
                        case (int)ZamokNames.Гардиан_10_01:
                            ВысветитьЭлемент("ODL_Гардиан 10.01 в профиле активки");
                            break;
                    }
                    switch (odl.Ruchka)
                    {
                        case (int)RuchkaNames.Ручка_кнопка:
                            ВысветитьЭлемент("ODL_РДК-110 в профиле");
                            break;
                    }
                    break;
                case 6:
                    ВысветитьЭлемент("ODL_Петлевой_профиль");
                    ВысветитьЭлемент("ODL_Анкеры в профиле");
                    break;
                case 7:
                    ВысветитьЭлемент("ODL_Замковой профиль пассивки");
                    ВысветитьЭлемент("ODL_Делга ZPN");
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            ВысветитьЭлемент("ODL_ПП(стандарт) в профиле пассивки");
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:
                            ВысветитьЭлемент("ODL_ЗВ8 в профиле пассивки");
                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            ВысветитьЭлемент("ODL_Гардиан 12.11+Вега в профиле пассивки");
                            break;
                        case (int)ZamokNames.Гардиан_10_01:
                            ВысветитьЭлемент("ODL_Гардиан 10.01 в профиле пассивки");
                            break;
                    }
                    break;
                case 8:
                    ВысветитьЭлемент("ODL_Притолока С1");
                    break;
                case 9:
                    ВысветитьЭлемент("ODL_Стойка С3");
                    ВысветитьЭлемент("ODL_Анкеры в С3");
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            ВысветитьЭлемент("ODL_ПП(стандарт) в стойке С3");
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:

                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            ВысветитьЭлемент("ODL_Гардиан 12.11+Вега в стойке С3");
                            break;
                        case (int)ZamokNames.Почтовый:
                            ВысветитьЭлемент("ODL_Почтовый в стойке С3");
                            break;
                    }
                    break;
                case 10:
                    ВысветитьЭлемент("ODL_Стойка С3");
                    ВысветитьЭлемент("ODL_Анкеры в С3");
                    ВысветитьЭлемент("ODL_Противосъем в С3");
                    break;
                case 11:
                    ВысветитьЭлемент("ODL_Притолока С3");
                    break;
                case 12:
                    ВысветитьЭлемент("ODL_Притолока С3");
                    break;
                case 13:
                    ВысветитьЭлемент("ODL_Порог 25 ВО");
                    break;
            }
        }
        private void РедактироватьODL(ref ODL odl, Command_ODL com)
        {
            var conf = GetConf(com, odl.Otkrivanie);
            switch (conf)
            {
                case 0:
                    РедактироватьЭскиз("ODL_ЛА");
                    ИзменитьРазмер("ODL_ЛА", "Высота", odl.LicevoyList_Height);
                    ИзменитьРазмер("ODL_ЛА", "Ширина", odl.LicevoyList_Width(Stvorka.Активная));
                    ИзменитьРазмер("ODL_ЛА", "От пола", odl.LicevoyList_OtPola);
                    ЗакрытьЭскиз();
                    switch (odl.Zamok((short)com))
                    {
                        case (int)ZamokNames.ПП:
                            РедактироватьЭскиз("ODL_ПП(стандарт)_АС");
                            ИзменитьРазмер("ODL_ПП(стандарт)_АС", "ОтКрая", (float)odl.Zamok_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                        case (int)ZamokNames.Просам_ЗВ_8:
                            РедактироватьЭскиз("ODL_ЗВ8_АС");
                            ИзменитьРазмер("ODL_ЗВ8_АС", "ОтКрая", (float)odl.Zamok_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                        case (int)ZamokNames.Гардиан_12_11:
                            РедактироватьЭскиз("ODL_Гардиан 12.11+Вега_АС");
                            ИзменитьРазмер("ODL_Гардиан 12.11+Вега_АС", "ОтКрая", (float)odl.Zamok_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                        case (int)ZamokNames.Почтовый:
                            РедактироватьЭскиз("ODL_П_АС");
                            ИзменитьРазмер("ODL_П_АС", "ОтКрая", (float)odl.Zamok_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                        case (int)ZamokNames.Гардиан_10_01:
                            РедактироватьЭскиз("ODL_Гардиан 10.01_АС");
                            ИзменитьРазмер("ODL_Гардиан 10.01_АС", "ОтКрая", (float)odl.Zamok_OtKraya);
                            ЗакрытьЭскиз();
                            break;
                    }
                    break;
                case 1:
                    РедактироватьЭскиз("ODL_ЛП");
                    ИзменитьРазмер("ODL_ЛП", "Высота", odl.LicevoyList_Height);
                    ИзменитьРазмер("ODL_ЛП", "Ширина", odl.LicevoyList_Width(Stvorka.Пассивная));
                    ИзменитьРазмер("ODL_ЛП", "От пола", odl.LicevoyList_OtPola);
                    ЗакрытьЭскиз();
                    break;
                case 2:
                case 3:
                    РедактироватьЭскиз("ODL_ЗПС1");
                    ИзменитьРазмер("ODL_ЗПС1", "Высота", odl.VertStoyka_Height);
                    ИзменитьРазмер("ODL_ЗПС1", "Наличник", (float)odl.Nalichnik_Raz((short)com));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("ODL_АнкерыЗПС1");
                    ИзменитьРазмер("ODL_АнкерыЗПС1", "До верхнего", odl.RastDo3Ankera);
                    if (com == Command_ODL.Петлевая_стойка | com == Command_ODL.Замковая_стойка & odl.IsPassivka)
                    {
                        ИзменитьРазмер("ODL_АнкерыЗПС1", "Диаметр анкера2", 25f);
                    }
                    else
                    {
                        ИзменитьРазмер("ODL_АнкерыЗПС1", "Диаметр анкера2", 16f);
                    }
                    ЗакрытьЭскиз();
                    break;
                case 4:
                    РедактироватьЭскиз("ODL_ГС1");
                    ИзменитьРазмер("ODL_ГС1", "Ширина", (float)odl.GorStoyka_Height);
                    ИзменитьРазмер("ODL_ГС1", "Наличник", (float)odl.Nalichnik_Raz((short)com));
                    ЗакрытьЭскиз();
                    break;
                case 5:
                    РедактироватьЭскиз("ODL_ЗПА");
                    ИзменитьРазмер("ODL_ЗПА", "Высота", odl.ZamkovoyProfil_Height(Stvorka.Активная));
                    ИзменитьРазмер("ODL_ЗПА", "Ширина", odl.ZamkovoyProfil_Razvertka(Stvorka.Активная));
                    ИзменитьРазмер("ODL_ЗПА", "От пола", odl.ZamkovoyProfil_OtPola);
                    ЗакрытьЭскиз();
                    break;
                case 6:
                    РедактироватьЭскиз("ODL_ППА");
                    ИзменитьРазмер("ODL_ППА", "Высота", odl.ZamkovoyProfil_Height(Stvorka.Активная));
                    ИзменитьРазмер("ODL_ППА", "От пола", odl.ZamkovoyProfil_OtPola);
                    ЗакрытьЭскиз();
                    break;
                case 7:
                    РедактироватьЭскиз("ODL_ЗПП");
                    ИзменитьРазмер("ODL_ЗПП", "Высота", odl.ZamkovoyProfil_Height(Stvorka.Пассивная));
                    ИзменитьРазмер("ODL_ЗПП", "Ширина", odl.ZamkovoyProfil_Razvertka(Stvorka.Пассивная));
                    ИзменитьРазмер("ODL_ЗПП", "От пола", odl.ZamkovoyProfil_OtPola + 5);
                    ЗакрытьЭскиз();
                    break;
                case 8:
                    РедактироватьЭскиз("ODL_ГС1");
                    ИзменитьРазмер("ODL_ГС1", "Ширина", (float)odl.GorStoyka_Height);
                    ИзменитьРазмер("ODL_ГС1", "Наличник", (float)odl.Nalichnik_Raz((short)com));
                    ЗакрытьЭскиз();
                    break;
                case 9:
                case 10:
                    РедактироватьЭскиз("ODL_ЗПС3");
                    ИзменитьРазмер("ODL_ЗПС3", "Высота", odl.VertStoyka_Height);
                    ИзменитьРазмер("ODL_ЗПС3", "Наличник", (float)odl.Nalichnik_Raz((short)com));
                    ЗакрытьЭскиз();
                    РедактироватьЭскиз("ODL_АнкерыЗПС3");
                    ИзменитьРазмер("ODL_АнкерыЗПС3", "До верхнего", odl.RastDo3Ankera);
                    //if (com == Command_ODL.Петлевая_стойка | com == Command_ODL.Замковая_стойка & odl.IsPassivka)
                    //{
                    //    ИзменитьРазмер("ODL_АнкерыЗПС3", "Диаметр анкера2", 25f);
                    //}
                    //else
                    //{
                    //    ИзменитьРазмер("ODL_АнкерыЗПС3", "Диаметр анкера2", 16f);
                    //}
                    ЗакрытьЭскиз();
                    break;
                case 11:
                case 12:
                    РедактироватьЭскиз("ODL_ГС3");
                    ИзменитьРазмер("ODL_ГС3", "Ширина", (float)odl.GorStoyka_Height);
                    ИзменитьРазмер("ODL_ГС3", "Наличник", (float)odl.Nalichnik_Raz((short)com));
                    ЗакрытьЭскиз();
                    break;
                case 13:
                    РедактироватьЭскиз("ODL_Порог25-ВО");
                    ИзменитьРазмер("ODL_Порог25-ВО", "Высота", (float)odl.Porog_Length);
                    ЗакрытьЭскиз();
                    break;
            }
        }

        private void ВысветитьKVD(ref KVD kvd,  Command_KVD com)
        {
            switch (com)
            {
                case Command_KVD.Лицевой_лист:
                {
                    if (kvd.Data.Glazok.Length > 0)
                        ВысветитьЭлемент(kvd.Data.Zerkalo ? "Глазок_край" : "Глазок_центр");

                    if(kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_32_11)
                        ВысветитьЭлемент("Гардиан_32.11");
                    if (kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_12_11)
                        ВысветитьЭлемент("Гардиан_12.11");

                    if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_30_01)
                        ВысветитьЭлемент("Гардиан_30.01");
                    else if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_10_01)
                        ВысветитьЭлемент("Гардиан_10.01");
                    else if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_50_01)
                        ВысветитьЭлемент("Гардиан_50.01");
                    else if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_12_01)
                        ВысветитьЭлемент("Гардиан_12.01");

                    if (kvd.Data.Bronya)
                    {
                        if (kvd.Data.Kod == 52 || kvd.Data.Kod == 53 || kvd.Name == "КВ01c")
                            ВысветитьЭлемент("Броненакладка DEF-9726");
                        else
                        {
                            if (kvd.Data.Kod == 1 && (kvd.Data.Otkrivanie == Otkrivanie.ЛевоеВО ||
                                                      kvd.Data.Otkrivanie == Otkrivanie.ПравоеВО))
                                ВысветитьЭлемент(kvd.Data.GetZamok(0) == (short) ZamokNames.Гардиан_32_11
                                    ? "Накладная_броня_32.11"
                                    : "Накладная_броня_12.11");
                            else
                                ВысветитьЭлемент("Накладная_броня");
                        }
                    }

                    if((kvd.Data.Otkrivanie == Otkrivanie.ЛевоеВО || kvd.Data.Otkrivanie == Otkrivanie.ПравоеВО) 
                       && kvd.Data.Zadvizhka.Kod == (short)ZadvizhkaNames.Ночной_сторож)
                            ВысветитьЭлемент("Ночной_сторож");
                    if (kvd.Data.Kod != 1 && kvd.Data.Kod != 50 && kvd.Data.Kod != 54 && kvd.Data.Kod != 55 && kvd.Data.Zadvizhka.Kod == (short)ZadvizhkaNames.Ночной_сторож)
                        ВысветитьЭлемент("Ночной_сторож");
                    if(kvd.Data.Kod == 50 && (kvd.Data.Otkrivanie == Otkrivanie.ЛевоеВО || kvd.Data.Otkrivanie == Otkrivanie.ПравоеВО))
                        ВысветитьЭлемент("Ночной_сторож");

                    if(kvd.Data.Kod == 1 && kvd.Data.LicPanel)
                        ВысветитьЭлемент("Маркер третьей петли");

                    if (kvd.Data.LicPanel)
                        ВысветитьЭлемент("Крепление_лицевой_МДФ");
                    break;
                }
                case Command_KVD.Внутренний_лист:
                {
                    if (kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_32_11)
                        ВысветитьЭлемент("Гардиан_32.11");

                    if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_30_01)
                        ВысветитьЭлемент("Гардиан_30.01");
                    else if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_50_01)
                        ВысветитьЭлемент("Гардиан_50.01");

                    if (kvd.Data.Kod != 50 && kvd.Data.Zadvizhka.Kod == (short)ZadvizhkaNames.Ночной_сторож)
                        ВысветитьЭлемент("Ночной_сторож");
                    if (kvd.Data.Kod == 50 && kvd.Data.Zadvizhka.Kod == (short) ZadvizhkaNames.Ночной_сторож)
                    {
                        ВысветитьЭлемент("Ночной_сторож_тело");

                        if(kvd.Data.Otkrivanie == Otkrivanie.Левое || kvd.Data.Otkrivanie == Otkrivanie.Правое)
                            ВысветитьЭлемент("Ночной_сторож_вертушек");
                    }
                    break;
                }
                case Command_KVD.Замковой_профиль:
                {
                    if (kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_32_11)
                        ВысветитьЭлемент("32.11");
                    if (kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_12_11)
                        ВысветитьЭлемент("12.11");

                    if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_30_01)
                        ВысветитьЭлемент("30.01");
                    else if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_10_01)
                        ВысветитьЭлемент("10.01");
                    else if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_12_01)
                        ВысветитьЭлемент("10.01");
                    else if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_50_01)
                        ВысветитьЭлемент("50.01");

                    if (kvd.Data.Zadvizhka.Kod == (short) ZadvizhkaNames.Ночной_сторож)
                        ВысветитьЭлемент("Ночной_сторож");

                    if(kvd.Name == "КВ01c")
                        ВысветитьЭлемент("Просечки Комфорт-Р");
                    break;
                }
                case Command_KVD.Петлевой_профиль:
                {
                    ВысветитьЭлемент("Противосъемники");
                    if((kvd.Data.Kod == 1 && kvd.Data.Protivos == 2) || kvd.Data.Kod == 54 || kvd.Data.Kod == 55)
                        ВысветитьЭлемент("3-ий противосъем");

                    ВысветитьЭлемент("Петли");

                    if (kvd.Name == "КВ01c")
                    {
                        ВысветитьЭлемент("3-я петля");
                        ВысветитьЭлемент("Вырезы Комфорт-Р");
                        ВысветитьЭлемент("Отверстия Комфорт-Р");
                    }

                    break;
                }
                case Command_KVD.Верхний_профиль:
                case Command_KVD.Нижний_профиль:
                    if (kvd.Data.Kod == 52 || kvd.Data.Kod == 53)
                        ВысветитьЭлемент("Для_крепления_лицевого_МДФ");
                    break;
                case Command_KVD.Монтажный_профиль_нижний:
                    break;
                case Command_KVD.Монтажный_профиль_петлевой:
                    if(kvd.Data.Kod == 1 && kvd.Data.Protivos == 2)
                        ВысветитьЭлемент("3-ий противосъем");
                    break;
                case Command_KVD.Замковая_стойка:
                {
                    if (kvd.Data.GetZamok(0) == (short) ZamokNames.Гардиан_32_11)
                        ВысветитьЭлемент("Гардиан_32.11");
                    else if (kvd.Data.GetZamok(0) == (short) ZamokNames.Гардиан_32_01)
                        ВысветитьЭлемент("Гардиан_32.01");
                    else if (kvd.Data.GetZamok(0) == (short)ZamokNames.Гардиан_12_11)
                        ВысветитьЭлемент("Гардиан_12.11");

                    if (kvd.Data.GetZamok(1) == (short) ZamokNames.Гардиан_10_01)
                        ВысветитьЭлемент("Гардиан_10.01");
                    else if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_12_01)
                        ВысветитьЭлемент("Гардиан_12.01");
                    else if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_30_01)
                        ВысветитьЭлемент("Гардиан_30.01");
                    else if (kvd.Data.GetZamok(1) == (short)ZamokNames.Гардиан_50_01)
                        ВысветитьЭлемент("Гардиан_50.01");

                    if (kvd.Data.Zadvizhka.Kod == (short) ZadvizhkaNames.Ночной_сторож)
                        ВысветитьЭлемент("Ночной сторож");

                    if(kvd.Data.LicPanel && kvd.GetNalichnik() > 20)
                        ВысветитьЭлемент("Прорези_под_наличник");
                    break;
                }
                case Command_KVD.Петлевая_стойка:
                    if(kvd.Data.Kod == 1 || kvd.Data.Kod == 54 || kvd.Data.Kod == 55)
                        ВысветитьЭлемент("Противосъемники");
                    if((kvd.Data.Kod == 1 && kvd.Data.Protivos == 2) || kvd.Data.Kod == 54 || kvd.Data.Kod == 55)
                        ВысветитьЭлемент("3-ий противосъем");

                    if (kvd.Data.LicPanel && kvd.GetNalichnik(false) > 20)
                        ВысветитьЭлемент("Прорези_под_наличник");
                    break;
                case Command_KVD.Притолока:
                    if (kvd.Data.LicPanel)
                        ВысветитьЭлемент("Вырезы_под_крепление_наличника.");
                    break;
                case Command_KVD.Порог:
                    break;
                case Command_KVD.РЖК_Замковая:
                    break;
                case Command_KVD.РЖК_Петлевая:
                    ВысветитьЭлемент("Петли");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(com), com, null);
            }
        }
        private void РедактироватьKVD(ref KVD kvd, Command_KVD com)
        {
            switch (com)
            {
                case Command_KVD.Лицевой_лист:
                    РедактироватьЭскиз("ЛЛ");
                    ИзменитьРазмер("ЛЛ", "Высота", (float)kvd.LL_Height);
                    ИзменитьРазмер("ЛЛ", "Ширина", (float)kvd.LL_Width);
                    if(kvd.Name.IndexOf("КВ01", StringComparison.Ordinal) >= 0 
                       || kvd.Name.IndexOf("КВ10", StringComparison.Ordinal) >= 0
                       || kvd.Name.IndexOf("КВ11", StringComparison.Ordinal) >= 0)
                        ИзменитьРазмер("ЛЛ", "ОтПола", (float)kvd.LL_OtPola);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Внутренний_лист:
                    РедактироватьЭскиз("ВЛ");
                    ИзменитьРазмер("ВЛ", "Высота", (float)kvd.VL_Height);
                    ИзменитьРазмер("ВЛ", "Ширина", (float)kvd.VL_Width);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Замковой_профиль:
                case Command_KVD.Петлевой_профиль:
                    РедактироватьЭскиз("ЗПП");
                    ИзменитьРазмер("ЗПП", "Длина", (float)kvd.VP_Length);
                    if (kvd.Name.IndexOf("КВ01", StringComparison.Ordinal) >= 0 
                        || kvd.Name.IndexOf("КВ10", StringComparison.Ordinal) >= 0)
                    {
                        ИзменитьРазмер("ЗПП", "ЛЛ_ОтПола", (float) kvd.LL_OtPola);
                        if(com == Command_KVD.Петлевой_профиль)
                            ИзменитьРазмер("Противосъем", "ОтНиза", (float)kvd.ProtivosOtstup);
                    }

                    if(com == Command_KVD.Петлевой_профиль && kvd.Name == "КВ01c")
                        ИзменитьРазмер("ЗПП", "Ширина", (float)94.08);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Верхний_профиль:
                case Command_KVD.Нижний_профиль:
                    РедактироватьЭскиз("ВНП");
                    ИзменитьРазмер("ВНП", "Длина", (float)kvd.GP_Length);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Монтажный_профиль_нижний:
                    РедактироватьЭскиз("МП");
                    ИзменитьРазмер("МП", "Длина", (float)kvd.MP_Length);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Монтажный_профиль_петлевой:
                    РедактироватьЭскиз("BZU");
                    ИзменитьРазмер("BZU", "Длина", (float)kvd.VP_Length);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Замковая_стойка:
                    РедактироватьЭскиз("ЗПС");
                    ИзменитьРазмер("ЗПС", "Длина", (float)kvd.VS_Length);
                    ИзменитьРазмер("ЗПС", "Наличник", GetNalichnik(kvd, true));
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Петлевая_стойка:
                    РедактироватьЭскиз("ЗПС");
                    ИзменитьРазмер("ЗПС", "Длина", (float)kvd.VS_Length);
                    ИзменитьРазмер("ЗПС", "Наличник", GetNalichnik(kvd));
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Притолока:
                    РедактироватьЭскиз("ВНС");
                    ИзменитьРазмер("ВНС", "Длина", (float)kvd.GS_Length);
                    ИзменитьРазмер("ВНС", "Наличник", (float)kvd.Nalichnik(Raspolozhenie.Верх));
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.Порог:
                    РедактироватьЭскиз("ВНС");
                    ИзменитьРазмер("ВНС", "Длина", (float)kvd.GS_Length);
                    if (kvd.Name.IndexOf("КВ01", StringComparison.Ordinal) >= 0
                        || kvd.Name.IndexOf("КВ10", StringComparison.Ordinal) >= 0
                        || kvd.Name.IndexOf("КВ11", StringComparison.Ordinal) >= 0)
                    {
                        ИзменитьРазмер("ВНС", "Зад", (float)kvd.POR_Zad);
                        ИзменитьРазмер("ВНС", "Перед", (float)kvd.POR_Pered);
                    }
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.РЖК_Замковая:
                case Command_KVD.РЖК_Петлевая:
                    РедактироватьЭскиз("РЖК");
                    ИзменитьРазмер("РЖК", "Длина", (float)kvd.RZK_Length);
                    ЗакрытьЭскиз();
                    break;
                case Command_KVD.РЖ_полотна:
                    РедактироватьЭскиз("РЖП");
                    ИзменитьРазмер("РЖП", "Длина", (float)kvd.RZP_Lengnth);
                    ЗакрытьЭскиз();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(com), com, null);
            }
        }

        private float GetNalichnik(KVD kvd, bool zamkovoy = false)
        {
            if (zamkovoy)
            {
                if (kvd.Data.Otkrivanie == Otkrivanie.Левое || kvd.Data.Otkrivanie == Otkrivanie.ЛевоеВО)
                    return (float) kvd.Nalichnik(Raspolozhenie.Прав);
                return (float) kvd.Nalichnik(Raspolozhenie.Лев);
            }
            if (kvd.Data.Otkrivanie == Otkrivanie.Левое || kvd.Data.Otkrivanie == Otkrivanie.ЛевоеВО)
                return (float)kvd.Nalichnik(Raspolozhenie.Лев);
            return (float)kvd.Nalichnik(Raspolozhenie.Прав);
        }

        private void ВысветитьЭлемент(string Name)
        {
            bool status;
            int iter;
            status = false;
            iter = 0;
            while (!status)
            {
                iter++;
                status = Maket.Extension.SelectByID2(Name, "BODYFEATURE", 0d, 0d, 0d, false, 0, null, 0);
                if (iter > 5 && !status)
                {
                    Interaction.MsgBox("Не могу найти элемент - " + Name, (MsgBoxStyle)16, "!!!Не могу найти элемент!!!");
                    return;
                }
            }
            Maket.EditUnsuppress2();
            Maket.ClearSelection2(true);
        }
        private void ПогаситьЭлемент(string Name)
        {
            bool status;
            int iter;
            status = false;
            iter = 0;
            while (!status)
            {
                iter += 1;
                status = Maket.Extension.SelectByID2(Name, "BODYFEATURE", 0d, 0d, 0d, false, 0, null, 0);
                if (iter > 5)
                {
                    Interaction.MsgBox("Не могу найти элемент - " + Name, (MsgBoxStyle)16, "!!!Не могу найти элемент!!!");
                    return;
                }
            }

            Maket.EditSuppress2();
            Maket.ClearSelection2(true);
        }
        private void РедактироватьЭскиз(string Name)
        {
            bool status;
            int iter;
            status = false;
            iter = 0;
            while (!status)
            {
                iter += 1;
                status = Maket.Extension.SelectByID2(Name, "SKETCH", 0d, 0d, 0d, false, 0, null, 0);
                if (iter > 5)
                {
                    Interaction.MsgBox("Не могу найти эскиз - " + Name, (MsgBoxStyle)16, "!!!Не могу найти эскиз!!!");
                    return;
                }
            }

            Maket.EditSketch();
            Maket.ClearSelection2(true);
        }
        private void ЗакрытьЭскиз()
        {
            Maket.ClearSelection2(true);
            Maket.SketchManager.InsertSketch(true);
        }
        private void ИзменитьРазмер(string nameS, string nameD, float value)
        {
            bool status;
            int iter;
            status = false;
            iter = 0;
            while (!status)
            {
                iter += 1;
                status = Maket.Extension.SelectByID2(nameD + "@" + nameS + "@" + MaketName, "DIMENSION", 0d, 0d, 0d, false, 0, null, 0);
                if (iter > 5)
                {
                    Interaction.MsgBox("Не могу найти размер - " + nameD, (MsgBoxStyle)16, "!!!Не могу найти размер!!!");
                    return;
                }
            }
            Maket.Parameter(nameD + "@" + nameS).SystemValue = (float)(value / 1000f);
        }
        private void ИзменитьКоличество(string nameS, string nameD, short value)
        {
            bool status;
            int iter;
            status = false;
            iter = 0;
            while (!status)
            {
                iter += 1;
                status = Maket.Extension.SelectByID2(nameD + "@" + nameS + "@" + MaketName, "DIMENSION", 0d, 0d, 0d, false, 0, null, 0);
                if (iter > 5)
                {
                    Interaction.MsgBox("Не могу найти размер - " + nameD, (MsgBoxStyle)16, "!!!Не могу найти размер!!!");
                    return;
                }
            }
            Maket.Parameter(nameD + "@" + nameS).SystemValue = (short)value;
        }
        private void ПозиционированиеЗамка(short num, ref DM dm)
        {
            string sketch = num == 0 ? "DM_ЯЗН" : "DM_ЯЗВ";
            РедактироватьЭскиз(sketch);
            ИзменитьРазмер(sketch, "ОтПола", (float)dm.Zamok_OtPola(num));
            ИзменитьРазмер(sketch, "ОтКраяЛА", (float)(dm.LicevoyList_Width(Stvorka.Активная) - dm.Zamok_OtKraya(num, 0)));
            ИзменитьРазмер(sketch, "ОтКраяВА", (float)(dm.VnutrenniyList_Width(Stvorka.Активная) - dm.Zamok_OtKraya(num, 1)));
            ЗакрытьЭскиз();
        }
        private void РазмерыЗамка(short num, string nameA, string nameP, ref DM dm)
        {
            if (dm.Zamok_OtTela(num) > 0)
            {
                РедактироватьЭскиз(nameA);
                ИзменитьРазмер(nameA, "ДоТела", (float)dm.Zamok_OtTela(num));
                ЗакрытьЭскиз();
            }
            if (dm.IsPassivka & !nameP.Equals("_"))
            {
                РедактироватьЭскиз(nameP);
                ИзменитьРазмер(nameP, "ОтКраяЛП", (float)dm.Zamok_OtKraya(num, 2));
                ИзменитьРазмер(nameP, "ОтПола", (float)dm.Zamok_Otvetka_OtPola(num));
                ЗакрытьЭскиз();
            }
            if (num == 0 & dm.Zamok(num).CM)
            {
                РедактироватьЭскиз("DM_ЦилиндрН");
                ИзменитьРазмер("DM_ЦилиндрН", "Межосевое", dm.Zamok(num).Mezhosevoe);
                ЗакрытьЭскиз();
            }
        }
        private void РамерыАнтипаники(short stvorka, short num, ref DM dm)
        {
            string nameSk = dm.Ruchka_SketchName(stvorka, num);
            if (!string.IsNullOrEmpty(nameSk) & !nameSk.Equals("_"))
            {
                РедактироватьЭскиз(nameSk);
                if (stvorka == (short)Stvorka.Активная)
                    ИзменитьРазмер(nameSk, "АПотКраяВА", (float)dm.Ruchka_OtLeftKraya(stvorka, num, 0));
                else
                {
                    ИзменитьРазмер(nameSk, "АПотКраяВП", (float)dm.Ruchka_OtLeftKraya(stvorka, num, 1));
                    ИзменитьРазмер(nameSk, "АПотПКраяВП", (float)(dm.VnutrenniyList_Width(Stvorka.Пассивная) - dm.Ruchka_OtKraya(stvorka, num, 2)));
                    if (dm.Ruchka_Kod(stvorka, 0) == (int)RuchkaNames.PB_1700C)
                        ИзменитьРазмер(nameSk, "От низа", dm.APanOtNiza);
                }
                ЗакрытьЭскиз();
            }
        }
        private void РедактироватьОтбойник(OtboynayaPlastina plastina)
        {
            РедактироватьЭскиз("Пластина");
            ИзменитьРазмер("Пластина", "Высота", plastina.Height);
            ИзменитьРазмер("Пластина", "Ширина", plastina.Width);
            ЗакрытьЭскиз();
        }
    }
}
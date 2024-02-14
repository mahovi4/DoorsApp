using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COM_DoorsLibrary;

namespace DoorsMaketChangers
{
    public class Kompas_MaketChanger : MaketChanger
    {
        private const bool KOMPAS_VISIBLE = true;

        private readonly Application kompas5;
        private readonly IApplication kompas7;
        private ksDocument2D maket;
        private IKompasDocument2D maket7;
        private IKompasDocument maketDok;

        private readonly string kompasPath;
        private readonly string maketPath;
        private readonly string furnituraPath;

        public Kompas_MaketChanger(string maketRootFolder, string dxfFolder)
            : base(maketRootFolder, dxfFolder)
        {
            kompas5 = (Application)Interaction.CreateObject("KOMPAS.Application.5");
            kompas5.Visible = KOMPAS_VISIBLE;
            kompas7 = (IApplication)kompas5.ksGetApplication7();

            kompasPath = RootPath + @"\Компас";
            maketPath = kompasPath + @"\Макеты";
            furnituraPath = kompasPath + @"\Фурнитура";
        }

        public override void Build_KVD(KVD kvd, Command_KVD com, string name)
        {
            throw new NotImplementedException();
        }

        public override void Exit()
        {
            kompas5.Quit();
        }

        public override void Build_DM(DM dm, Command_DM com)
        {
            switch (com)
            {
                case Command_DM.Полотна:
                case Command_DM.Полотна2:
                    Build_StvorkaDM(Stvorka.Активная, ref dm);
                    if (dm.IsPassivka)
                        Build_StvorkaDM(Stvorka.Пассивная, ref dm);
                    break;
                case Command_DM.Левая_стойка:
                case Command_DM.Правя_стойка:
                    Raspolozhenie pos = com == Command_DM.Левая_стойка ? Raspolozhenie.Лев : Raspolozhenie.Прав;
                    Build_StoykaDM(pos, ref dm);
                    break;
                case Command_DM.Притолока:
                    Build_PritolokaDM(Raspolozhenie.Верх, ref dm);
                    break;
                case Command_DM.Порог:
                    if (dm.Stoyka_Type(Raspolozhenie.Ниж) > 0 & dm.Stoyka_Type(Raspolozhenie.Ниж) < 5)
                        Build_PritolokaDM(Raspolozhenie.Ниж, ref dm);
                    else if (dm.Stoyka_Type(Raspolozhenie.Ниж) > 5)
                        Build_PorogDM(ref dm);
                    else
                        return;
                    break;
                default:
                    return;
            }
            ClearMaket();
            SaveMaket_DXF(dm.Name((short)com));
            System.Threading.Thread.Sleep(1000);
            CloseMaket();
        }
        public override void Build_LM(LM lm, Command_LM com) { }
        public override void Build_VM(DVM vm, Command_VM com) { }
        public override void Build_ODL(ODL odl, Command_ODL com) { }

        private void Build_StvorkaDM(Stvorka stvorka, ref DM dm)
        {
            if(stvorka == Stvorka.Активная)
            {
                //Открыть макет активки
                OpenMaket(MaketTypes.DM, "DM_Активка"); 
                //Сформировать коллекцию переменных для редактирования макета активки
                Dictionary<string, float> varsAktive = new Dictionary<string, float>
                {
                    {"LL_Height", (float)dm.LicevoyList_Height(Stvorka.Активная) },
                    {"LL_Width", (float)dm.LicevoyList_Width(Stvorka.Активная) },
                    {"LL_OtPola", dm.LicevoyList_OtPola },
                    {"VL_Height", (float)dm.VnutrenniyList_Height(Stvorka.Активная) },
                    {"VL_Width", (float)dm.VnutrenniyList_Width(Stvorka.Активная) },
                    {"VL_OtPola", dm.VnutrenniyList_OtPola },
                    {"UpGib", dm.UpGib(Stvorka.Активная) },
                    {"DownGib", dm.IsDownGib? dm.UpGib(Stvorka.Активная):0 }
                };
                //Редактировать переменные макета активки
                ChangeVariables(maketDok.Reference, varsAktive);
                //Вставить фрагмент верхнего притвора активки
                int insUP = InsertFragment(maketPath + @"\DM\DM_Stvorka\Притворы\Верхний гиб ЛЛАС.frw", 0, GetVariable(maketDok.Reference, "UpGib_Y"), 0);
                //Сформировать коллекцию переменных для редактирования фрагмента верхнего притвора активки
                Dictionary<string, float> varsUP = new Dictionary<string, float>
                {
                    {"LeftGib", dm.LeftGib(Stvorka.Активная)},
                    {"RightGib", dm.RightGib(Stvorka.Активная)},
                    {"TopGib", dm.UpGib(Stvorka.Активная)},
                    {"Width", (float)dm.LicevoyList_Width(Stvorka.Активная)}
                };
                //Редактировать переменные фрагмента верхнего притвора активки
                ChangeVariables(insUP, varsUP);
                //Если есть нижний притвор...
                if (dm.IsDownGib)
                {
                    //Вставить фрагмент нижнего притвора
                    int insDP = InsertFragment(maketPath + @"\DM\DM_Stvorka\Притворы\Нижний гиб ЛЛАС.frw", 0, dm.LicevoyList_OtPola, 0);
                    //Сформировать коллекцию переменных для редактирования фрагмента нижнего притвора
                    Dictionary<string, float> varsDP = new Dictionary<string, float>
                    {
                        {"LeftGib", dm.KompensVirez},
                        {"RightGib", dm.RightGib(Stvorka.Активная)},
                        {"DownGib", dm.UpGib(Stvorka.Активная)},
                        {"Width", (float)dm.LicevoyList_Width(Stvorka.Активная)}
                    };
                    //Редактировать переменные фрагмента нижнего притвора
                    ChangeVariables(insDP, varsDP);
                }
                else //...иначе нарисовать линию
                    DrawLine(0, dm.LicevoyList_OtPola, (float)dm.LicevoyList_Width(Stvorka.Активная), dm.LicevoyList_OtPola);
            }
            else
            {
                //Вставить фрагмент пассивки
                int insPas = InsertFragment(maketPath + @"\DM\DM_Пассивка.frw", GetVariable(maketDok.Reference, "Passivka_X"), 0, 0);
                //Сформировать коллекцию переменных для редактирования фрагмента пассивки
                Dictionary<string, float> varsPassive = new Dictionary<string, float>
                {
                    {"LL_Height", (float)dm.LicevoyList_Height(Stvorka.Пассивная)},
                    {"LL_Width", (float)dm.LicevoyList_Width(Stvorka.Пассивная)},
                    {"LL_OtPola", dm.LicevoyList_OtPola},
                    {"VL_Height", (float)dm.VnutrenniyList_Height(Stvorka.Пассивная)},
                    {"VL_Width", (float)dm.VnutrenniyList_Width(Stvorka.Пассивная)},
                    {"VL_OtPola", dm.VnutrenniyList_OtPola},
                    {"VirezPritvorDw_Height", (float)dm.VirezPoPorogu_Height},
                    {"UpGib", dm.UpGib(Stvorka.Пассивная)},
                    {"DownGib", dm.IsDownGib? dm.UpGib(Stvorka.Пассивная):0}
                };
                //Редактировать переменные фрагмента пассивки
                ChangeVariables(insPas, varsPassive);
                //Вставить фрагмент верхнего притвора пассивки
                int insUPP = InsertFragment(maketPath + @"\DM\DM_Stvorka\Притворы\Верхний гиб ЛЛПС.frw",
                                            GetVariable(maketDok.Reference, "Passivka_X"),
                                            (float)(dm.LicevoyList_OtPola + (dm.LicevoyList_Height(Stvorka.Пассивная) -
                                            (dm.IsDownGib ? dm.UpGib(Stvorka.Пассивная) : 0))), 0);
                //Сформировать коллекцию переменных для редактирования фрагмента верхнего притвора пассивки
                Dictionary<string, float> varsUPP = new Dictionary<string, float>
                {
                    {"UpGib", dm.UpGib(Stvorka.Пассивная)},
                    {"LeftGib", dm.LeftGib(Stvorka.Пассивная)},
                    {"RightGib", dm.RightGib(Stvorka.Пассивная)},
                    {"Width", (float)dm.LicevoyList_Width(Stvorka.Пассивная)},
                    {"VirezPritvor_Width", (float)dm.VirezPoPorogu_Width}
                };
                //Редактировать переменные фрагмента верхнего притвора пассивки
                ChangeVariables(insUPP, varsUPP);
                //Если есть нижний притвор...
                if (dm.IsDownGib)
                {
                    //Вставить фрагмент нижнего притвора пассивки
                    int insDPP = InsertFragment(maketPath + @"\DM\DM_Stvorka\Притворы\Нижний гиб ЛЛПС.frw",
                                                GetVariable(maketDok.Reference, "Passivka_X"), dm.LicevoyList_OtPola, 0);
                    //Сформировать коллекцию переменных для редактирования фрагмента нижнего притвора пассивки
                    Dictionary<string, float> varsDPP = new Dictionary<string, float>
                    {
                        {"DownGib", dm.UpGib(Stvorka.Пассивная)},
                        {"LeftGib", dm.LeftGib(Stvorka.Пассивная)},
                        {"RightGib", dm.KompensVirez},
                        {"Width", (float)dm.LicevoyList_Width(Stvorka.Пассивная)},
                        {"VirezPritvor_Width", (float)dm.VirezPoPorogu_Width},
                        {"VirezPritvor_Height", (float)dm.VirezPoPorogu_Height}
                    };
                    //Редактировать переменные фрагмента нижнего притвора пассивки
                    ChangeVariables(insDPP, varsDPP);
                }
                else //...иначе...
                {
                    //Вставить фрагмент нижней части пассивки без притвора
                    int insDPP = InsertFragment(maketPath + @"\DM\DM_Stvorka\Притворы\Нижний гиб ЛЛПС_без притвора.frw",
                                                GetVariable(maketDok.Reference, "Passivka_X"), dm.LicevoyList_OtPola, 0);
                    //Сформировать коллекцию переменных для редактирования фрагмента нижней части пассивки без притвора
                    Dictionary<string, float> varsDPP = new Dictionary<string, float>
                            {
                                {"Width", (float)dm.LicevoyList_Width(Stvorka.Пассивная)},
                                {"VirezPritvor_Width", (float)dm.VirezPoPorogu_Width},
                                {"VirezPritvor_Height", (float)dm.VirezPoPorogu_Height}
                            };
                    //Редактировать переменные фрагмента нижней части пассивки без притвора
                    ChangeVariables(insDPP, varsDPP);
                }
            }
        }
        private void Build_StoykaDM(Raspolozhenie pos, ref DM dm)
        {
            if(dm.Otkrivanie == Otkrivanie.Левое || dm.Otkrivanie == Otkrivanie.Правое) // Если наружнее открывание
            {
                //Открыть макет стойки К1
                OpenMaket(MaketTypes.DM, "DM_Стойка К1");
                //Сформировать коллекцию переменных для редактирования макета стойки К1
                Dictionary<string, float> varsK1 = new Dictionary<string, float>
                {
                    {"Height", dm.HeightStoyki(pos)},
                    {"Width", (float)dm.RazvertkaStoyki(pos)}
                };
                //Редактировать переменные макета стойки К1
                ChangeVariables(maketDok.Reference, varsK1);
                //Сформировать коллекцию переменных для редактирования фрагмента верхней стыковки
                Dictionary<string, float> varsUS = new Dictionary<string, float>
                {
                    {"Gabarit", (float)dm.GabaritStoyki(pos)},
                    {"Stikovka", (float)dm.StikovkaStoyki(pos)},
                    {"Glubina", (float)dm.GlubinaStoyki(pos)},
                    {"Zanizhenie", (float)dm.ZanizhenieStoyki(pos)},
                    {"Nalichnik", (float)dm.NalichnikStoyki(pos)}
                };
                int insUS;
                if (dm.UpZaglushkaNSStoyki(pos) > 0) //если есть заглушка разности наличников (писька)))), то...
                {
                    //вставить фрагмент верхней стыковки с заглушкой
                    insUS = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\СтойкаК1_Верх с заглушкой.frw", 0, dm.HeightStoyki(pos), 0);
                    //добавить в коллекцию переменных высоту заглушки
                    varsUS.Add("Zaglushka", dm.UpZaglushkaNSStoyki(pos));
                }
                else //...иначе вставить фрагмент верхней стыковки без заглушки
                    insUS = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\СтойкаК1_Верх.frw", 0, dm.HeightStoyki(pos), 0);
                //Редактировать переменные стыковки стойки К1
                ChangeVariables(insUS, varsUS);

                if (dm.DownZanizhenieStoyki(pos) > 0) //Если нижнее занижение стыковки больше 0 (т.е. есть стыковка снизу), то...
                {
                    //Сформировать коллекцию переменных для редактирования фрагмента нижней стыковки
                    Dictionary<string, float> varsDS = new Dictionary<string, float>
                    {
                        {"Gabarit", (float)dm.GabaritStoyki(pos)},
                        {"Stikovka", (float)dm.StikovkaStoyki(pos)},
                        {"Glubina", (float)dm.GlubinaStoyki(pos)},
                        {"Zanizhenie", (float)dm.DownZanizhenieStoyki(pos)},
                        {"Nalichnik", (float)dm.NalichnikStoyki(pos)}
                    };
                    int insDS;
                    if (dm.DownZaglushkaNSStoyki(pos) > 0) //если снизу есть заглушка разницы наличников, то...
                    {
                        //вставить фрагмент нижней стыковки с заглушкой
                        insDS = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\СтойкаК1_Низ с заглушкой.frw", 0, 0, 0);
                        //добавить в коллекцию переменных высоту заглушки
                        varsDS.Add("Zaglushka", dm.DownZaglushkaNSStoyki(pos));
                    }
                    else //...иначе вставить фрагмент нижней стыковки без заглушки
                        insDS = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\СтойкаК1_Низ.frw", 0, 0, 0);
                    //Редактировать переменные нижней стыковки стойки К1
                    ChangeVariables(insDS, varsDS);
                }
                else //...иначе...
                {
                    //нарисовать нижнюю часть стойки без стыковки
                    float w = (float)dm.RazvertkaStoyki(pos);
                    DrawLine(0, 0, w, 0);
                    DrawLine(w, 0, w, 30);
                }
            }
            else
            {

            }
        }
        private void Build_PritolokaDM(Raspolozhenie pos, ref DM dm)
        {
            if(dm.Otkrivanie == Otkrivanie.Левое | dm.Otkrivanie == Otkrivanie.Правое) //Если наружнее открывание
            {
                //Открыть макет притолоки К1
                OpenMaket(MaketTypes.DM, "DM_Притолока К1"); 
                //Сформировать коллекцию переменных для редактирования макета притолоки К1
                Dictionary<string, float> varsP = new Dictionary<string, float>
                {
                    {"Height", dm.HeightStoyki(pos)},
                    {"Width", (float)dm.RazvertkaStoyki(pos)}
                };
                //Редактировать переменные макета притолоки К1
                ChangeVariables(maketDok.Reference, varsP);
                //Вставить фрагмент верхней стыковки притолоки
                int insUSP = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\ПритолокаК1_Верх.frw", 0, dm.HeightStoyki(pos), 0);
                //Сформировать коллекцию переменных для редактирования фрагментов стыковок притолоки К1
                Dictionary<string, float> varsSP = new Dictionary<string, float>
                {
                    {"Gabarit", (float)dm.GabaritStoyki(pos)},
                    {"Glubina", (float)dm.GlubinaStoyki(pos)},
                    {"Nalichnik", (float)dm.NalichnikStoyki(pos)}
                };
                //Редактировать переменные фрагмента верхней стыковки притолоки К1
                ChangeVariables(insUSP, varsSP);
                //Вставить фрагмент нижней стыковки притолоки К1
                int insDSP = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\ПритолокаК1_Низ.frw", 0, 0, 0);
                //Редактировать переменные фрагмента нижней стыковки притолоки К1
                ChangeVariables(insDSP, varsSP);
            }
            else
            {

            }
        }
        private void Build_PorogDM(ref DM dm)
        {
            //Открыть макет порога
            OpenMaket(MaketTypes.DM, "DM_Порог");
            //Сформировать коллекцию переменных редактирования макета порога
            Dictionary<string, float> varsP = new Dictionary<string, float>
            {
                {"Height", dm.HeightStoyki(Raspolozhenie.Ниж)},
                {"Width", (float)dm.RazvertkaStoyki(Raspolozhenie.Ниж)}
            };
            //Редактировать макет порога
            ChangeVariables(maketDok.Reference, varsP);
            //Вставить фрагмент верхней стыковки порога
            int insUSP = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\Порог_Верх.frw", 0, dm.HeightStoyki(Raspolozhenie.Ниж), 0);
            //Сформировать коллекцию переменных редактирования фрагментов стыковок порога
            Dictionary<string, float> varsSP = new Dictionary<string, float>
            {
                {"Glubina", (float)dm.GlubinaStoyki(Raspolozhenie.Ниж)},
                {"Stikovka", (float)dm.GabaritStoyki(Raspolozhenie.Ниж)}
            };
            //Редактировать фрагмент верхней стыковки порога
            ChangeVariables(insUSP, varsSP);
            //Вставить фрагмент нижней стыковки порога
            int insDSP = InsertFragment(maketPath + @"\DM\DM_Korobka\Стыковки\Порог_Низ.frw", 0, 0, 0);
            //Редактировать фрагмент нижней стыковки порога
            ChangeVariables(insDSP, varsSP);
        }

        private void CreateMaket()
        {
            Documents docs = kompas7.Documents;
            maketDok = docs.AddWithDefaultSettings(DocumentTypeEnum.ksDocumentFragment);
            maket = (ksDocument2D)kompas5.ActiveDocument2D();
            maket7 = (IKompasDocument2D)kompas7.ActiveDocument;
        }
        private void OpenMaket(MaketTypes type, string name)
        {
            string typeFolder = "";
            switch (type)
            {
                case MaketTypes.DM:
                    typeFolder = @"\DM\";
                    break;
                case MaketTypes.LM:
                    typeFolder = @"\LM\";
                    break;
                case MaketTypes.DVM:
                    typeFolder = @"\DVM\";
                    break;
                case MaketTypes.ODL:
                    typeFolder = @"\ODL\";
                    break;
            }
            Documents docs = kompas7.Documents;
            maketDok = docs.Open(maketPath + typeFolder + name + ".frw");
            maket = (ksDocument2D)kompas5.ActiveDocument2D();
            maket7 = (IKompasDocument2D)kompas7.ActiveDocument;
        }
        private void ClearMaket() //Очистить текущий макет от всей лишней информации (на выходе исключительно контуры и ничего более)
        {
            //Получить контейнер всей графики в текущем макете
            IDrawingContainer container = (IDrawingContainer)maket7.ViewsAndLayersManager.Views.ActiveView;
            //Перебрать все вставки фрагментов в контейнере
            foreach (IInsertionObject ins in container.InsertionObjects)
                maket.ksDestroyObjects(ins.Reference); //...и разрушить их
            //Удалить слой "Размеры" (иначе все параметрические размеры вылезут в макете после импорта dxf-ки обратно в компас)
            maket7.ViewsAndLayersManager.Views.ActiveView.Layers.Layer["Размеры"].Delete();
        }
        private void SaveMaket_DXF(string maketName)
        {
            //перестроить макет
            maket.ksRebuildDocument();
            //сохранить макет в папку DxfPath (адрес получаем из Settings.ini к проге) в формате DXF
            maket.ksSaveToDXF(DxfPath + "\\" + maketName + ".DXF");
        }
        private void CloseMaket()
        {
            try
            {
                maketDok.Close(DocumentCloseOptions.kdDoNotSaveChanges);
            }
            catch(Exception ex)
            {
                Interaction.MsgBox(ex.Message);
            }
            maketDok = null;
            maket = null;
            maket7 = null;
        }
        private int InsertFragment(string fragmentFullName, float insX, float insY, float rotAngle)
        {
            ksFragment fr = (ksFragment)maket.GetFragment();
            int def = fr.ksFragmentDefinition(fragmentFullName, fragmentFullName, 0);
            PlacementParam param = (PlacementParam)kompas5.GetParamStruct((short)StructType2DEnum.ko_PlacementParam);
            param.Init();
            param.angle = rotAngle;
            param.scale_ = 1;
            param.xBase = insX;
            param.yBase = insY;
            return fr.ksInsertFragment(def, false, param);
        }
        private void ChangeVariables(int insertfr, Dictionary<string, float> variables)
        {
            //Получаем массив переменных, находящихся в макете
            DynamicArray array = (DynamicArray)maket.ksGetDocVariableArray(insertfr);
            if (array.ksGetArrayCount() > 0) //если он не пуст, то...
            {
                foreach (var variable in variables)//...для каждой переменной во входной коллекции...
                {
                    for (int i = 0; i < array.ksGetArrayCount(); i++)//...обходим весь массив переменных макета...
                    {
                        //создаем локальную переменную, которая будет хранить данные одной переменной макета из массива
                        ksVariable var = (ksVariable)kompas5.GetParamStruct((short)StructType2DEnum.ko_VariableParam);
                        //записываем в локальную переменную данные i-ой переменной в массиве переменных макета
                        array.ksGetArrayItem(i, var);
                        if (var.name.Equals(variable.Key)) //если имя переменной макета совподает с именем переменной из входной коллекции, то...
                        {
                            //...заменяем значение переменной на требуемое
                            var.value = variable.Value;
                            //...заменяем переменную в массиве переменных макета на отредактированную
                            array.ksSetArrayItem(i, var);
                            //выход из цикла for
                            break;
                        }
                    }
                }
                //применяем отредактированный массив переменных макета
                maket.ksSetDocVariableArray(insertfr, array, false);
                //обновляем макет
                maket7.ViewsAndLayersManager.Views.ActiveView.Update();
            }
        }
        private float GetVariable(int insertfr, string varName)
        {
            //получить массив переменных из макета
            DynamicArray array = (DynamicArray)maket.ksGetDocVariableArray(insertfr);
            if (array.ksGetArrayCount() > 0) //если массив не пустой
            {
                for (int i = 0; i < array.ksGetArrayCount(); i++) //обходим массив
                {
                    //создаем локальную переменную, в которой будут храниться данные одной переменной из массива
                    ksVariable var = (ksVariable)kompas5.GetParamStruct((short)StructType2DEnum.ko_VariableParam);
                    //записываем данные i-той переменной массива в локальную
                    array.ksGetArrayItem(i, var);
                    if (var.name.Equals(varName)) //если имя переменной совподает с запрашиваемым
                        return (float)var.value;//возвращаем значение переменной
                }
                return 0; //если переменная с запрашиваемым именем в массиве не найдена, возвращаем 0
            }
            else return 0; //если массив переменных пуст, возвращаем 0
        }
        private int DrawLine(float x1, float y1, float x2, float y2)
        {
            return maket.ksLineSeg(x1, y1, x2, y2, 2);
        }
    }
}

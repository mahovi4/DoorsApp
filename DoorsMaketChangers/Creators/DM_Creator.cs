using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COM_DoorsLibrary;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal static class DM_Creator
    {
        public static ModelDoc2 CreateDm(this SW_Model model, DM dm, Command_DM com, string rootPath)
        {
            switch (com)
            {
                case Command_DM.Полотна:
                case Command_DM.Полотна2: {

                    var isLeft =
                        dm.Otkrivanie == Otkrivanie.Левое ||
                        dm.Otkrivanie == Otkrivanie.ЛевоеВО;

                    SW_TemplateModel lla;
                    SW_TemplateModel vla;
                    SW_TemplateModel llp = null;
                    SW_TemplateModel vlp = null;

                    var llaTransform = new List<ElementTransform>();
                    var llpTransform = new List<ElementTransform>();

                    var offseter = 100.0;

                    //Создание трансформирования развертки лицевого листа активки и пассивки
                    var sweepLlaTpansform = new Dictionary<string, float>
                    {
                        {"Ширина", (float)dm.LicevoyList_Width(Stvorka.Активная)}
                    };
                    var sweepLlpTpansform = new Dictionary<string, float>();
                    if (dm.IsPassivka)
                        sweepLlpTpansform.Add("Ширина", (float)dm.LicevoyList_Width(Stvorka.Пассивная));
                    if (dm.IsDownGib)
                    {
                        if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                        {
                            sweepLlaTpansform.Add("Высота", (float)dm.LicevoyList_Height(Stvorka.Активная) - 6);
                            if (dm.IsPassivka)
                                sweepLlpTpansform.Add("Высота", (float)dm.LicevoyList_Height(Stvorka.Активная) - 6);
                        }
                        else
                        {
                            sweepLlaTpansform.Add("Высота", (float)dm.LicevoyList_Height(Stvorka.Активная) - dm.UpGib(Stvorka.Активная));
                            if (dm.IsPassivka)
                                sweepLlpTpansform.Add("Высота", (float)dm.LicevoyList_Height(Stvorka.Активная) - dm.UpGib(Stvorka.Активная));
                        }
                    }

                    //Создание трансформирования лицевого листа активной створки
                    llaTransform.Add(new ElementTransform
                    {
                        Name = "Верхний гиб",
                        Suppress = true,
                        Dimensions = new Dictionary<string, float>
                        {
                            {"Величина", dm.UpGib(Stvorka.Активная)},
                            {"ОтступЛевый", isLeft ? dm.LeftGib(Stvorka.Активная) : dm.RightGib(Stvorka.Активная)},
                            {"ОтступПравый", isLeft ? dm.RightGib(Stvorka.Активная) : dm.LeftGib(Stvorka.Активная)}
                        }
                    });

                    //Создание трансформирования лицевого листа пассивной створки
                    if (dm.IsPassivka)
                        llpTransform.Add(new ElementTransform
                        {
                            Name = "Верхний гиб",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                        {
                            {"Величина", dm.UpGib(Stvorka.Пассивная)},
                            {"ОтступЛевый", isLeft ? dm.LeftGib(Stvorka.Пассивная) : dm.RightGib(Stvorka.Пассивная)},
                            {"ОтступПравый", isLeft ? dm.RightGib(Stvorka.Пассивная) : dm.LeftGib(Stvorka.Пассивная)}
                        }
                        });

                    //Определение трансформирования нижней части лицевых листов (по порогу)
                    if (dm.IsDownGib)
                    {
                        if (dm.Stoyka_Type(Raspolozhenie.Ниж) == 30)
                        {
                            llaTransform.Add(new ElementTransform
                            {
                                Name = "Нижняя юбка (порог 30)",
                                Suppress = true,
                                Dimensions = new Dictionary<string, float>
                            {
                                {"ОтступЛевый", isLeft ? dm.LeftGib(Stvorka.Активная) : dm.RightGib(Stvorka.Активная)},
                                {"ОтступПравый", isLeft ? dm.RightGib(Stvorka.Активная) : dm.LeftGib(Stvorka.Активная)}
                            }
                            });

                            if (dm.IsPassivka)
                                llpTransform.Add(new ElementTransform
                                {
                                    Name = "Нижняя юбка (порог 30)",
                                    Suppress = true,
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтступЛевый", isLeft ? dm.LeftGib(Stvorka.Пассивная) : dm.RightGib(Stvorka.Пассивная)},
                                        {"ОтступПравый", isLeft ? dm.RightGib(Stvorka.Пассивная) : dm.LeftGib(Stvorka.Пассивная)}
                                    }
                                });
                        }
                        else
                        {
                            llaTransform.Add(new ElementTransform
                            {
                                Name = "Нижний гиб",
                                Suppress = true,
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Величина", dm.UpGib(Stvorka.Активная)},
                                    {"ОтступЛевый", isLeft ? dm.KompensVirez : dm.RightGib(Stvorka.Активная)},
                                    {"ОтступПравый", isLeft ? dm.RightGib(Stvorka.Активная) : dm.KompensVirez}
                                }
                            });

                            llpTransform.Add(new ElementTransform
                            {
                                Name = "Нижний гиб",
                                Suppress = true,
                                Dimensions = new Dictionary<string, float>
                            {
                                {"Величина", dm.UpGib(Stvorka.Активная)},
                                {"ОтступЛевый", isLeft ? dm.LeftGib(Stvorka.Пассивная) : dm.KompensVirez},
                                {"ОтступПравый", isLeft ? dm.KompensVirez : dm.LeftGib(Stvorka.Пассивная)}
                            }
                            });
                        }
                    }

                    //Добавление в трансформирование лиц. листа пассивки притворных вырезов
                    //if (dm.IsPassivka)
                    //{
                    //    llpTransform.Add(new ElementTransform
                    //    {
                    //        Name = "Вырез по верхнему притвору",
                    //        Suppress = true,
                    //        Dimensions = new Dictionary<string, float>
                    //        {
                    //            {"Величина", (float)dm.VirezPoPritvoru_Height},
                    //            {"Ширина", (float)dm.VirezPoPorogu_Width}
                    //        }
                    //    });

                    //    llpTransform.Add(new ElementTransform
                    //    {
                    //        Name = "Вырез по нижнему притвору",
                    //        Suppress = true,
                    //        Dimensions = new Dictionary<string, float>
                    //        {
                    //            {"Величина", (float)dm.VirezPoPorogu_Height},
                    //            {"Ширина", (float)dm.VirezPoPorogu_Width}
                    //        }
                    //    });
                    //}

                    //Добавление лицевого листа активки в файл
                    lla = new SW_TemplateModel(new PositionCoordinates(offseter, dm.LicevoyList_OtPola), model,
                    new SweepTemplatePath("Лицевой Лист Активки", "Лист полотна", $@"{rootPath}\SW"),
                    new TemplateTransform
                    {
                        SweepTransform = sweepLlaTpansform,
                        ElementsTransform = llaTransform
                    });

                    //Смещение точки вставки в файл
                    offseter += dm.LicevoyList_Width(Stvorka.Активная) + 100;

                    //Добавление внутреннего листа активки в файл
                    vla = new SW_TemplateModel(new PositionCoordinates(offseter, dm.VnutrenniyList_OtPola), model,
                        new SweepTemplatePath("Внутренний Лист Активки", "Лист полотна", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Ширина", (float)dm.VnutrenniyList_Width(Stvorka.Активная)},
                                {"Высота", (float)dm.VnutrenniyList_Height(Stvorka.Активная)}
                            }
                        });

                    //Добавление пассивной створки в файл
                    if (dm.IsPassivka)
                    {
                        //Смещение точки вставки
                        offseter += dm.VnutrenniyList_Width(Stvorka.Активная) + 100;

                        //Добавление лицевого листа
                        llp = new SW_TemplateModel(new PositionCoordinates(offseter, dm.LicevoyList_OtPola), model,
                            new SweepTemplatePath("Лицевой Лист Пассивки", "Лист полотна", $@"{rootPath}\SW"),
                            new TemplateTransform
                            {
                                SweepTransform = sweepLlpTpansform,
                                ElementsTransform = llpTransform
                            });

                        //Смещение точки вставки
                        offseter += dm.LicevoyList_Width(Stvorka.Пассивная) + 100;

                        //Добавление внутреннего листа 
                        vlp = new SW_TemplateModel(new PositionCoordinates(offseter, dm.VnutrenniyList_OtPola), model,
                            new SweepTemplatePath("Внутренний Лист Пассивки", "Лист полотна", $@"{rootPath}\SW"),
                            new TemplateTransform
                            {
                                SweepTransform = new Dictionary<string, float>
                                {
                                    {"Ширина", (float)dm.VnutrenniyList_Width(Stvorka.Пассивная)},
                                    {"Высота", (float)dm.VnutrenniyList_Height(Stvorka.Пассивная)}
                                }
                            });
                    }

                    //Добавление маркеров низа на детали полотна
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер низа ВЛАС", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Маркер низа ВЛАС",
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 5},
                                {"ОтНижКрая", 5}
                            }
                        });
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер низа ЛЛПС", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Маркер низа ЛЛПС",
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 5},
                                {"ОтНижКрая", (float)(5 + dm.VirezPoPorogu_Height)}
                            }
                        });
                    vlp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер низа ВЛПС", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Маркер низа ВЛПС",
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 5},
                                {"ОтНижКрая", 5}
                            }
                        });

                    //Добавление притворных вырезов на лицевой лист пассивки
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Вырез по верхнему притвору", "Прямоугольный вырез", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Вырез по верхнему притвору",
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", isLeft ? 0 : (float)dm.VirezPoPorogu_Width},
                                {"ОтВерхКрая", (float)dm.VirezPoPritvoru_Height},
                                {"Высота", (float)dm.VirezPoPritvoru_Height},
                                {"Ширина", (float)dm.VirezPoPorogu_Width}
                            }
                        });
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Вырез по нижнему притвору", "Прямоугольный вырез", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Вырез по нижнему притвору",
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", isLeft ? 0 : (float)dm.VirezPoPorogu_Width},
                                {"ОтНижКрая", 0},
                                {"Высота", (float)dm.VirezPoPorogu_Height},
                                {"Ширина", (float)dm.VirezPoPorogu_Width}
                            }
                        });

                    //Добавление противосъемов
                    if (dm.Protivos_Count > 0)
                    {
                        for (var i = 0; i < dm.Protivos_Count; i++)
                        {
                            vla.InsertLibElement(
                                new FurnitureCutoutTemplatePath($"Противосъм АС - {i + 1}",
                                    "Противосъем на полотне", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = $"Противосъм АС - {i + 1}",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float) dm.Protivos_OtKraya},
                                        {"ОтПола", dm.Protivos_OtPola((short) i)}
                                    }
                                });

                            vlp?.InsertLibElement(
                                new FurnitureCutoutTemplatePath($"Противосъм ПС - {i + 1}",
                                    "Противосъем на полотне", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = $"Противосъм ПС - {i + 1}",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Protivos_OtKraya},
                                        {"ОтПола", dm.Protivos_OtPola((short) i)}
                                    }
                                });
                        }
                    }

                    //Добавление сварных просечек
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Левые просечки АС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Левые просечки АС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)dm.VnutrenniyList_Height(Stvorka.Активная)}
                            }
                        });
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Правые просечки АС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Правые просечки АС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПравКрая", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)dm.VnutrenniyList_Height(Stvorka.Активная)}
                            }
                        });
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Просечки ЛЛПС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Просечки ЛЛПС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 0},
                                {"ОтНижКрая", (float)dm.VirezPoPorogu_Height},
                                {"Длина", (float)(dm.LicevoyList_Height(Stvorka.Активная) - dm.VirezPoPorogu_Height - dm.VirezPoPritvoru_Height)}
                            }
                        });
                    vlp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Просечки ВЛПС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Просечки ВЛПС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)dm.VnutrenniyList_Height(Stvorka.Пассивная)}
                            }
                        });

                    //Добавление торцевых пластин
                    if (dm.IsTorcevayaPlastina(0))
                    {
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижняя торцевая пластина АС",
                                "Торцевая пластина нижняя", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Нижняя торцевая пластина АС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(0).OtstupPetlya
                                    : dm.TorcevayaPlastina(0).OtstupZamok)},
                                    {"ОтНижКрая", 0},
                                    {"Ширина", (float) dm.TorcevayaPlastina(0).Length},
                                    {"Высота", (float) dm.TorcevayaPlastina(0).Width},
                                    {"Зазор", (float) dm.TorcevayaPlastina(0).Gap},
                                    {"Паз", (float) dm.TorcevayaPlastina(0).Groove}
                                }
                            });
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижнее малярное АС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Нижнее малярное АС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft 
                                    ? dm.TorcevayaPlastina(0).OtstupPetlya 
                                    : dm.TorcevayaPlastina(0).OtstupZamok) 
                                    + 30},
                                    {"ОтНижКрая", (float)(dm.TorcevayaPlastina(0).Width/2 * -1)}
                                }
                            });

                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижняя торцевая пластина ПС",
                                "Торцевая пластина нижняя", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Нижняя торцевая пластина ПС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(0).OtstupPetlya
                                    : dm.TorcevayaPlastina(0).OtstupZamok)},
                                    {"ОтНижКрая", 0},
                                    {"Ширина", (float) dm.TorcevayaPlastina(0).Length},
                                    {"Высота", (float) dm.TorcevayaPlastina(0).Width},
                                    {"Зазор", (float) dm.TorcevayaPlastina(0).Gap},
                                    {"Паз", (float) dm.TorcevayaPlastina(0).Groove}
                                }
                            });
                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижнее малярное ПС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Нижнее малярное ПС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(0).OtstupPetlya
                                    : dm.TorcevayaPlastina(0).OtstupZamok)
                                    + 30},
                                    {"ОтНижКрая", (float)(dm.TorcevayaPlastina(0).Width/2 * -1)}
                                }
                            });
                    } //нижняя
                    if (dm.IsTorcevayaPlastina(1))
                    {
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхняя торцевая АС", "Торцевая пластина верхняя",
                                $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Верхняя торцевая АС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(1).OtstupPetlya
                                    : dm.TorcevayaPlastina(1).OtstupZamok)},
                                    {"ОтВерхКрая", 0},
                                    {"Ширина", (float) dm.TorcevayaPlastina(1).Length},
                                    {"Высота", (float) dm.TorcevayaPlastina(1).Width},
                                    {"Зазор", (float) dm.TorcevayaPlastina(1).Gap},
                                    {"Паз", (float) dm.TorcevayaPlastina(1).Groove}
                                }
                            });
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхнее малярное АС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Верхнее малярное АС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(1).OtstupPetlya
                                    : dm.TorcevayaPlastina(1).OtstupZamok)
                                    + 30},
                                    {"ОтВерхКрая", (float)dm.TorcevayaPlastina(1).Width/2}
                                }
                            });

                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхняя торцевая ПС", "Торцевая пластина верхняя",
                                $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Верхняя торцевая ПС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(1).OtstupPetlya
                                    : dm.TorcevayaPlastina(1).OtstupZamok)},
                                    {"ОтВерхКрая", 0},
                                    {"Ширина", (float) dm.TorcevayaPlastina(1).Length},
                                    {"Высота", (float) dm.TorcevayaPlastina(1).Width},
                                    {"Зазор", (float) dm.TorcevayaPlastina(1).Gap},
                                    {"Паз", (float) dm.TorcevayaPlastina(1).Groove}
                                }
                            });
                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхнее малярное ПС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Верхнее малярное ПС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                    ? dm.TorcevayaPlastina(1).OtstupPetlya
                                    : dm.TorcevayaPlastina(1).OtstupZamok)
                                    + 30},
                                    {"ОтВерхКрая", (float)dm.TorcevayaPlastina(1).Width/2}
                                }
                            });
                    } //верхняя

                    //Добавление замков
                    if (dm.Zamki.Length > 0)
                    {
                        for (var i=0;  i < dm.Zamki.Length; i++)
                        {
                            if (dm.Zamki[i].Ruchka)
                                vla.InsertLibElement(
                                    new LockCutoutTemplatePath($"{dm.Zamki[i].Name} на ВЛАС_Замок-{i + 1}", dm.Zamki[i].Name, $@"{rootPath}\SW"),
                                    new ElementTransform
                                    {
                                        Name = $"{dm.Zamki[i].Name} на ВЛАС_Замок-{i + 1}",
                                        Dimensions = new Dictionary<string, float>
                                        {
                                            {"ОтПола", (float)(dm.Zamki[i].OtPola + dm.Zamki[i].OtRuchkiDoTela)},
                                            {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zamki[i].TeloOtKraya}
                                        }
                                    });
                            else
                                vla.InsertLibElement(
                                    new LockCutoutTemplatePath($"{dm.Zamki[i].Name} на ВЛАС_Замок-{i + 1}", dm.Zamki[i].Name, $@"{rootPath}\SW"),
                                    new ElementTransform
                                    {
                                        Dimensions = new Dictionary<string, float>
                                        {
                                            {"ОтПола", dm.Zamki[i].OtPola},
                                            {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zamki[i].OtKrayaVA}
                                        }
                                    });

                            llp?.InsertLibElement(
                                new LockCutoutTemplatePath($"Ответка-{i + 1} на ЛЛПС", $"{dm.Zamki[i].Name} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)dm.Zamki[i].OtvOtPola},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zamki[i].OtKrayaLP}
                                    }
                                });

                            if (dm.Zamki[i].CM)
                            {
                                lla.InsertLibElement(
                                    new LockCutoutTemplatePath($"ЦМ-{i+1} на ЛЛАС", "ЦМ", $@"{rootPath}\SW"),
                                    new ElementTransform
                                    {
                                        Name = $"ЦМ-{i + 1} на ЛЛАС",
                                        Dimensions = new Dictionary<string, float>
                                        {
                                            {"ОтПола", dm.Zamki[i].OtPola - dm.Zamki[i].Mezhosevoe},
                                            {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Zamki[i].OtKrayaLA}
                                        }
                                    });

                                vla.InsertLibElement(
                                    new LockCutoutTemplatePath($"ЦМ-{i+1} на ВЛАС", "ЦМ", $@"{rootPath}\SW"),
                                    new ElementTransform
                                    {
                                        Name = $"ЦМ-{i + 1} на ВЛАС",
                                        Dimensions = new Dictionary<string, float>
                                        {
                                            {"ОтПола", dm.Zamki[i].OtPola - dm.Zamki[i].Mezhosevoe},
                                            {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zamki[i].OtKrayaVA}
                                        }
                                    });
                            }
                        }
                    }

                    //Добавление ручек
                    for (short r = 0; r < 2; r++)
                    {
                        var sCount = dm.IsPassivka ? (short)2 : (short)1;

                        for (short s = 0; s < sCount; s++)
                        {
                            if (!dm.IsRuchka(s, r)) continue;

                            var ruchka = dm.Ruchka(s, r);

                            if (s == (int)Stvorka.Активная)
                            {
                                if (!ruchka.IsZamkovaya) continue;

                                foreach (var zamok in dm.Zamki)
                                {
                                    if (!zamok.Ruchka) continue;

                                    if (zamok.IsSovmestima(ruchka.Kod))
                                    {
                                        var rName = ruchka.Kod > 10 ? "Ручка на планке" : ruchka.Name;

                                        lla.InsertLibElement(
                                            new HandleCutoutTemplatePath($"{rName}_ЛЛАС", rName, $@"{rootPath}\SW"),
                                            new ElementTransform
                                            {
                                                Name = $"{rName}_ЛЛАС",
                                                Dimensions = new Dictionary<string, float>
                                                {
                                                    {"ОтПола", zamok.OtPola},
                                                    {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)zamok.OtKrayaLA}
                                                }
                                            });

                                        var dim = new Dictionary<string, float>
                                        {
                                            {"ОтПола", zamok.OtPola},
                                            {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)zamok.OtKrayaVA}
                                        };

                                        if (ruchka.Kod > 10)
                                            dim.Add("Штанга", 
                                                (float)(dm.VnutrenniyList_Width(Stvorka.Активная) 
                                                - zamok.OtKrayaVA 
                                                - dm.Ruchka_OtLeftKraya(s, r, 0)));

                                        vla.InsertLibElement(
                                            new HandleCutoutTemplatePath($"{ruchka.Name}_ВЛАС", ruchka.Name, $@"{rootPath}\SW"),
                                            new ElementTransform
                                            {
                                                Dimensions = dim
                                            });
                                    }
                                }
                            }
                        }
                    }

                    //Добавление задвижек
                    switch (dm.Zadvizhka_Kod(1))
                    {
                        case (short)ZadvizhkaNames.Ночной_сторож:
                            vla.InsertLibElement(
                                new ValveCutoutTemplatePath("Ночной сторож", "Ночной сторож", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "Ночной сторож",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", dm.Zadvizhka_OtPola},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zadvizhka_OtKrayaVA}
                                    }
                                });

                            var vert = new ValveCutoutTemplatePath("Вертушек НС", "Ночной сторож (вертушек)", $@"{rootPath}\SW");
                            var side = dm.Zadvizhka_OnList == 0 
                                    ? (isLeft ? "ОтПравКрая" : "ОтЛевКрая") 
                                    : (isLeft ? "ОтЛевКрая" : "ОтПравКрая");

                            var vertTransform = new ElementTransform
                            {
                                Name = "Вертушек НС",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтПола", dm.Zadvizhka_OtPola},
                                    {side, (float)dm.Zadvizhka_Vertushok_OtKraya}
                                }
                            };

                            if (dm.Zadvizhka_OnList == 0)
                                lla.InsertLibElement(vert, vertTransform);
                            else
                                vla.InsertLibElement(vert, vertTransform);

                            llp?.InsertLibElement(
                                new ValveCutoutTemplatePath("Ответка НС", "Ночной сторож ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", dm.Zadvizhka_OtPola},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Zadvizhka_OtKrayaLP}
                                    }
                                });

                            break;

                        case (short)ZadvizhkaNames.ЗД_01:
                            vla.InsertLibElement(
                                new ValveCutoutTemplatePath("ЗД-01", "ЗД-01", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "ЗД-01",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", dm.Zadvizhka_OtPola},
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Zadvizhka_OtKrayaVA}
                                    }
                                });

                            break;

                        case (short)ZadvizhkaNames.ЗТ_150:
                            vla.InsertLibElement(
                                new ValveCutoutTemplatePath("ЗТ-150", "ЗТ-150", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "ЗТ-150",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", dm.Zadvizhka_OtPola},
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Zadvizhka_OtKrayaVA}
                                    }
                                });

                            break;
                    }

                    //Добавление окон
                    for (short o = 0; o < 4; o++)
                    {
                        if (!dm.IsOkno(o)) continue;

                        var oknoLName = $"Окно {o + 1}_ЛЛ{(o < 2 ? "АС" : "ПС")}";
                        var oknoVName = $"Окно {o + 1}_ВЛ{(o < 2 ? "АС" : "ПС")}";

                        var oknoLTemplate = dm.IsOtsechka(o, 0)
                            ? "Прямоугольный вырез (С торцевыми)"
                            : "Прямоугольный вырез";
                        var oknoVTemplate = dm.IsOtsechka(o, 1)
                            ? "Прямоугольный вырез (С торцевыми)"
                            : "Прямоугольный вырез";

                        var oknoLTransform = new ElementTransform
                        {
                            Name = oknoLName,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПола", (float)dm.Okno_OtPola(o, 0)},
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Okno_OtKraya(o, 0)},
                                {"Ширина", dm.Okno_Width(o, 0)},
                                {"Высота", dm.Okno_Height(o, 0)}
                            }
                        };
                        var oknoVTransform = new ElementTransform
                        {
                            Name = oknoVName,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПола", (float)dm.Okno_OtPola(o, 1)},
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Okno_OtKraya(o, 1)},
                                {"Ширина", dm.Okno_Width(o, 1)},
                                {"Высота", dm.Okno_Height(o, 1)}
                            }
                        };

                        if (dm.IsOtsechka(o, 0))
                            oknoLTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(o).Width);

                        if (dm.IsOtsechka(o, 1))
                            oknoVTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(o).Width);

                        if (o < 2)
                        {
                            lla.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(oknoLName, oknoLTemplate, $@"{rootPath}\SW"),
                                oknoLTransform);
                            vla.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(oknoVName, oknoVTemplate, $@"{rootPath}\SW"),
                                oknoVTransform);
                        }
                        else
                        {
                            llp?.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(oknoLName, oknoLTemplate, $@"{rootPath}\SW"),
                                oknoLTransform);
                            vlp?.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(oknoVName, oknoVTemplate, $@"{rootPath}\SW"),
                                oknoVTransform);
                        }
                    }

                    //Добавление вентрешеток
                    for (short r = 0; r < 4; r++)
                    {
                        if (!dm.IsReshetka(r)) continue;

                        var reshLName = $"Решетка {r + 1}_ЛЛ{(r < 2 ? "АС" : "ПС")}";
                        var reshVName = $"Решетка {r + 1}_ВЛ{(r < 2 ? "АС" : "ПС")}";

                        var reshLTempate = "Прямоугольный вырез";
                        var reshVTempate = "Прямоугольный вырез (С торцевыми)";

                        var reshLTransform = new ElementTransform
                        {
                            Name = reshLName,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПола", dm.Reshetka_OtPola(r)},
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Reshetka_OtCrayaLista(r, 0)},
                                {"Ширина", dm.Reshetka_Width(r)},
                                {"Высота", dm.Reshetka_Height(r)}
                            }
                        };
                        var reshVTransform = new ElementTransform
                        {
                            Name = reshVName,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПола", dm.Reshetka_OtPola(r)},
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Reshetka_OtCrayaLista(r, 1)},
                                {"Ширина", dm.Reshetka_Width(r)},
                                {"Высота", dm.Reshetka_Height(r)}
                            }
                        };

                        if (dm.Reshetka_Type(r) == eReshetka.ПП_решетка)
                        {
                            reshLName = $"ПП-{reshLName}";
                            reshVName = $"ПП-{reshVName}";


                        }

                        if (dm.IsOtsechka(r, 0) || dm.IsOtsechka(r, 1))
                            reshVTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(r).Width);

                        if (r < 2)
                        {
                            lla.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(reshLName, "Прямоугольный вырез", $@"{rootPath}\SW"),
                                reshLTransform);
                            vla.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(reshVName, "Прямоугольный вырез (С торцевыми)", $@"{rootPath}\SW"),
                                reshVTransform);
                        }
                        else
                        {
                            llp?.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(reshLName, "Прямоугольный вырез", $@"{rootPath}\SW"),
                                reshLTransform);
                            vlp?.InsertLibElement(
                                new TechnologicalCutoutTemplatePath(reshVName, "Прямоугольный вырез (С торцевыми)", $@"{rootPath}\SW"),
                                reshVTransform);
                        }
                    }

                    break;
                }
                
                case Command_DM.Добор: {
                    if (!dm.IsDobor) throw new Exception();

                    var offseter = 100;

                    var dSweepTransform = new Dictionary<string, float>(); //Трансформация развертки
                    var dTransform = new List<ElementTransform>(); //Трансформация элементов детали

                    //-------------------------Добор левый
                    //Определение трансформации левого добора
                    dSweepTransform.Add("Высота", dm.Dobor_Length(Raspolozhenie.Лев));
                    dSweepTransform.Add("Ширина", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Лев] + dm.Dobor_Glubina);
                    dSweepTransform.Add("Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Лев]);
                    dSweepTransform.Add("Компенсация", (float)1.35);
                    dSweepTransform.Add("Глубина", dm.Dobor_Glubina);

                    if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] > 0)
                        dTransform.Add(new ElementTransform
                        {
                            Name = "Второй наличник",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>()
                        });

                    //Добавление левого добора
                    var dL = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                    new SweepTemplatePath("Добор левый", "Добор", $@"{rootPath}\SW"),
                    new TemplateTransform
                    {
                        SweepTransform = dSweepTransform,
                        ElementsTransform = dTransform
                    });

                    dL.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер нижний", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Анкер нижний",
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.Dobor_Glubina/2},
                                {"ОтПола", 250},
                                {"Диаметр", 16}
                            }
                        });

                    dL.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер средний", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Анкер средний",
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.Dobor_Glubina/2},
                                {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Лев)/2},
                                {"Диаметр", 16}
                            }
                        });

                    dL.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер верхний", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Анкер верхний",
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.Dobor_Glubina/2},
                                {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Лев) - 250},
                                {"Диаметр", 16}
                            }
                        });

                    //-------------------------Добор верхний
                    offseter += 250;
                    //Определение трансформации верхнего добора
                    dSweepTransform.Add("Длина", dm.Dobor_Length(Raspolozhenie.Верх));
                    dSweepTransform.Add("Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Верх]);
                    dSweepTransform.Add("Компенсация", (float)0.001);
                    dSweepTransform.Add("Глубина", dm.Dobor_Glubina);

                    dTransform.Add(new ElementTransform
                    {
                        Name = "Второй наличник",
                        Suppress = true,
                        Dimensions = new Dictionary<string, float>()
                    });

                    //Добавление верхнего добора
                    var dU = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                    new SweepTemplatePath("Добор верхний", "Добор", $@"{rootPath}\SW"),
                    new TemplateTransform
                    {
                        SweepTransform = dSweepTransform,
                        ElementsTransform = dTransform
                    });

                    dL.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер нижний", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                    {
                        Name = "Анкер нижний",
                        Dimensions = new Dictionary<string, float>
                        {
                            {"Отступ", (float)dm.Dobor_Glubina/2},
                            {"ОтПола", 250},
                            {"Диаметр", 16}
                        }
                    });

                    dL.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер верхний", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Name = "Анкер верхний",
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.Dobor_Glubina/2},
                                {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Верх) - 250},
                                {"Диаметр", 16}
                            }
                        });

                    //-------------------------Добор правый
                    if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Лев] != dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Прав])
                    {
                        offseter += 250;
                        //Определение трансформации правого добора
                        dSweepTransform.Add("Длина", dm.Dobor_Length(Raspolozhenie.Прав));
                        dSweepTransform.Add("Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Прав]);
                        dSweepTransform.Add("Компенсация", (float)1.35);
                        dSweepTransform.Add("Глубина", dm.Dobor_Glubina);

                        if (dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] > 0)
                            dTransform.Add(new ElementTransform
                            {
                                Name = "Второй наличник",
                                Suppress = true,
                                Dimensions = new Dictionary<string, float>()
                            });

                        //Добавление правого добора
                        var dR = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                        new SweepTemplatePath("Добор правый", "Добор", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = dSweepTransform,
                            ElementsTransform = dTransform
                        });

                        dL.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер нижний", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                        {
                            Name = "Анкер нижний",
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.Dobor_Glubina/2},
                                {"ОтПола", 250},
                                {"Диаметр", 16}
                            }
                        });

                        dL.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер средний", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Анкер средний",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.Dobor_Glubina/2},
                                    {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Прав)/2},
                                    {"Диаметр", 16}
                                }
                            });

                        dL.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер верхний", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Анкер верхний",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.Dobor_Glubina/2},
                                    {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Прав) - 250},
                                    {"Диаметр", 16}
                                }
                            });
                    }

                    if(dm.Dobor_Nalichnik[(int)Raspolozhenie.Ниж] > 0)
                    {
                        //-------------------------Добор нижний
                        offseter += 250;
                        //Определение трансформации нижнего добора
                        dSweepTransform.Add("Длина", dm.Dobor_Length(Raspolozhenie.Ниж));
                        dSweepTransform.Add("Наличник", (float)dm.Dobor_Nalichnik_Razv[(int)Raspolozhenie.Ниж]);
                        dSweepTransform.Add("Компенсация", (float)0.001);
                        dSweepTransform.Add("Глубина", dm.Dobor_Glubina);

                        dTransform.Add(new ElementTransform
                        {
                            Name = "Второй наличник",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>()
                        });

                        //Добавление нижнего добора
                        var dD = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                        new SweepTemplatePath("Добор нижний", "Добор", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = dSweepTransform,
                            ElementsTransform = dTransform
                        });

                        dL.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер нижний", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Анкер нижний",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.Dobor_Glubina/2},
                                    {"ОтПола", 250},
                                    {"Диаметр", 16}
                                }
                            });

                        dL.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер верхний", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Анкер верхний",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.Dobor_Glubina/2},
                                    {"ОтПола", (float)dm.Dobor_Length(Raspolozhenie.Ниж) - 250},
                                    {"Диаметр", 16}
                                }
                            });
                    }

                    break;
                }
                
                case Command_DM.Левая_стойка:
                case Command_DM.Правя_стойка: {

                    var pos = com == Command_DM.Левая_стойка 
                            ? Raspolozhenie.Лев 
                            : Raspolozhenie.Прав;

                    var template = (dm.Otkrivanie == Otkrivanie.ЛевоеВО || dm.Otkrivanie == Otkrivanie.ПравоеВО) 
                            ? "Стойка К3" 
                            : "Стойка К1_К2";

                    var offseter = 100;

                    var sSweepTransform = new Dictionary<string, float>(); //Трансформация развертки
                    var sTransform = new List<ElementTransform>(); //Трансформация элементов детали
                    var k = (dm.Otkrivanie == Otkrivanie.ЛевоеВО || dm.Otkrivanie == Otkrivanie.ПравоеВО) ? 31.5 + 39.73 : 4.6;

                    sSweepTransform.Add("Высота", dm.HeightStoyki(pos));
                    sSweepTransform.Add("Ширина", (float)(dm.GabaritStoyki(pos) + dm.GlubinaStoyki(pos) + dm.NalichnikStoyki(pos) + k));
                    sSweepTransform.Add("Габарит", (float)dm.GabaritStoyki(pos));
                    sSweepTransform.Add("Глубина", (float)dm.GlubinaStoyki(pos));
                    sSweepTransform.Add("Стыковка", (float)dm.StikovkaStoyki(pos));
                    sSweepTransform.Add("Занижение", (float)dm.ZanizhenieStoyki(pos));
                    sSweepTransform.Add("Наличник", (float)dm.NalichnikStoyki(pos));

                    if (dm.IsObrezkaNalichnika(pos))
                        sTransform.Add(new ElementTransform
                        {
                            Name = "Обрезка по вставке ПО",
                            Suppress = true
                        });

                    if (dm.UpZaglushkaNSStoyki(pos) > 0)
                        sTransform.Add(new ElementTransform
                        {
                            Name = "Верхняя заглушка НС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Высота", dm.UpZaglushkaNSStoyki(pos)}
                            }
                        });

                    if (dm.DownZanizhenieStoyki(pos) > 0)
                    {
                        sTransform.Add(new ElementTransform
                        {
                            Name = "Наличник снизу",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>()
                        });

                        sTransform.Add(new ElementTransform
                        {
                            Name = "Нижняя стыковка",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float> 
                            {
                                {"Занижение", (float)dm.DownZanizhenieStoyki(pos)}
                            }
                        });
                    }

                    if(dm.DownZaglushkaNSStoyki(pos) > 0)
                        sTransform.Add(new ElementTransform
                        {
                            Name = "Нижняя заглушка НС",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Высота", dm.UpZaglushkaNSStoyki(pos)}
                            }
                        });

                    var s = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                        new SweepTemplatePath("Стойка левая", template, $@"{rootPath}\SW"), 
                        new TemplateTransform
                        {
                            Name = "Стойка левая",
                            SweepTransform = sSweepTransform,
                            ElementsTransform = sTransform
                        });

                    for(var i = 0; i<3; i++)
                    {
                        s.InsertLibElement(
                            new TechnologicalCutoutTemplatePath($"Анкер {i+1}", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = $"Анкер {i+1}",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.AnkerOtKraya(pos)},
                                    {"ОтПола", dm.AnkerOtPola((short)(i+1))},
                                    {"Диаметр", (float)dm.DAnkerStoyki(i+1, pos)}
                                }
                            });
                    }

                    if (dm.IsStoykaZamkovaya(pos))
                    {
                        for (short i = 0; i < dm.Zamki.Length; i++)
                        {
                            if (dm.Zamok_Kod(i, (short)com) <= 0) continue;

                            var zamok = dm.Zamok(i);
                            s.InsertLibElement(
                                new LockCutoutTemplatePath($"Ответка {i+1} - {zamok.Name}", zamok.OtvName, $@"{rootPath}\SW"), 
                                new ElementTransform
                                {
                                    Name = $"Ответка {i+1} - {zamok.Name}",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"Отступ", (float)(dm.Zamok_Otstup(i) + dm.AnkerOtKraya(pos))},
                                        {"ОтПола", (float)dm.Zamok_Otvetka_OtPola(i)}
                                    }
                                });
                        }
                    }

                    break;
                }
                
                case Command_DM.Притолока: {
                    var pos = Raspolozhenie.Верх;

                    var no = dm.Otkrivanie == Otkrivanie.Левое || dm.Otkrivanie == Otkrivanie.Правое;

                    var template = no
                        ? "Притолока К1_К2"
                        : "Притолока К3";

                    var offseter = 100;

                    var sSweepTransform = new Dictionary<string, float>(); //Трансформация развертки
                    var sTransform = new List<ElementTransform>(); //Трансформация элементов развертки
                    var k = no ? 25.95 : 39.73 + 31.5;

                    sSweepTransform.Add("Высота", dm.HeightStoyki(pos));
                    sSweepTransform.Add("Ширина", (float)(dm.GlubinaStoyki(pos) + dm.NalichnikStoyki(pos) + dm.GabaritStoyki(pos) + k));
                    sSweepTransform.Add("Глубина", (float)dm.GlubinaStoyki(pos));
                    sSweepTransform.Add("Наличник", (float)dm.NalichnikStoyki(pos));

                    if(no)
                        sSweepTransform.Add("Габарит", (float)dm.GabaritStoyki(pos));

                    if (dm.IsObrezkaNalichnika(pos))
                        sTransform.Add(new ElementTransform
                        {
                            Name = "Обрезка по фрамуге ПО",
                            Suppress = true
                        });

                    var s = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model, 
                        new SweepTemplatePath("Притолока", template, $@"{rootPath}\SW"), 
                        new TemplateTransform
                        {
                            Name = "Притолока",
                            SweepTransform = sSweepTransform,
                            ElementsTransform = sTransform
                        });

                    if (dm.IsTermoblock(0) || dm.IsPassivka)
                    {
                        var offset = no ? 18 : 3;

                        s.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Маркер 1",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", 3 },
                                    {"ОтНижКрая", offset }
                                }
                            });

                        if (dm.Otkrivanie == Otkrivanie.Правое || dm.Otkrivanie == Otkrivanie.ПравоеВО)
                            s.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "Маркер 2",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"Отступ", 3 },
                                        {"ОтНижКрая", offset + 6 }
                                    }
                                });
                    }

                    if (dm.IsTorcShpingalet((short)pos))
                    {
                        s.InsertLibElement(
                            new ValveCutoutTemplatePath("ТШ ответка в притолоке", "Торцевой шпингалет ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.TorcShpingalet_Otvrtka_Otstup(0) 
                                        + (float)(!no 
                                            ? 0 
                                            : (dm.Stoyka_Type(pos) == 1 
                                                ? 96.18 
                                                : 96.18 + 15))
                                    },
                                    {"ОтНижКрая", (float)dm.TorcShpingalet_Otvetka_OtKraya + (float)(no ? 0 : 34.82)}
                                }
                            });
                    }

                    if (dm.IsTermoblock((short)pos))
                    {
                        s.InsertLibElement(
                            new FurnitureCutoutTemplatePath("ТБ ответка в притолоке", "Термоблокератор ответка", $@"{rootPath}\SW"), 
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.GetTermoblock(0).Otstup},
                                    {"ОтНижКрая", (float)dm.GetTermoblock(0).OtKraya + (float)(no ? 0 : 34.82)}
                                }
                            });
                    }

                    if (dm.IsOtvAntipan((short)pos))
                    {
                        s.InsertLibElement(
                            new HandleCutoutTemplatePath("PB-DL ответка в притолоке", "DoorLock (Антипаника) ответка в притолоке", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)dm.OtvAntipan_Otstup((short)pos)
                                        + (float)(!no
                                            ? 0
                                            : (dm.Stoyka_Type(pos) == 1
                                                ? 96.18
                                                : 96.18 + 15))
                                    },
                                    {"ОтНижКрая", (float)dm.OtvAntipan_OtKraya((short)pos) + (float)(no ? 0 : 34.82)}
                                }
                            });
                    }

                    if (dm.IsSdvigoviy)
                    {
                        s.InsertLibElement(
                            new LockCutoutTemplatePath("AL250 ответка в притолоке", "Aler AL250 ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)(dm.Sdvigoviy.Otstup
                                        + (!no
                                            ? 92.46
                                            : (dm.Stoyka_Type(pos) == 1
                                                ? 96.18
                                                : 96.18 + 15)))
                                    },
                                    {"ОтНижКрая", (float)(dm.Sdvigoviy.OtKraya + (no ? 0 : 34.82))}
                                }
                            });
                    }

                    if (dm.IsAnkerInPritoloka)
                    {
                        s.InsertLibElement(
                            new TechnologicalCutoutTemplatePath($"Анкер 1 в притолоке", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)(!no
                                                        ? 92.46
                                                        : (dm.Stoyka_Type(pos) == 1
                                                            ? 96.18
                                                            : 96.18 + 15))
                                    },
                                    {"ОтВерхКрая", 250 + (float)(no ? 0 : 34.82)},
                                    {"Диаметр", 16}
                                }
                            });

                        s.InsertLibElement(
                            new TechnologicalCutoutTemplatePath($"Анкер 2 в притолоке", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"Отступ", (float)(!no
                                                        ? 92.46
                                                        : (dm.Stoyka_Type(pos) == 1
                                                            ? 96.18
                                                            : 96.18 + 15))
                                    },
                                    {"ОтНижКрая", 250 + (float)(no ? 0 : 34.82)},
                                    {"Диаметр", 16}
                                }
                            });
                    }

                    break;
                }

                case Command_DM.Порог: {
                    var offseter = 100;
                    var pos = Raspolozhenie.Ниж;
                    var no = dm.Otkrivanie == Otkrivanie.Левое || dm.Otkrivanie == Otkrivanie.Правое;

                    var p30 = dm.Stoyka_Type(pos) == 30;

                    var pSweepTransform = new Dictionary<string, float>(); //Трансформация развертки

                    var template = "Порог";

                    if (p30)
                        template = "Порог 30";

                    var k = dm.Stoyka_Type(pos) == 30 ? 100 : 1.9;
                    var offset = p30 ? "ОтПравКрая" : "ОтЛевКрая";

                    pSweepTransform.Add("Высота", dm.HeightStoyki(pos));
                    pSweepTransform.Add("Ширина", (float)(dm.GlubinaStoyki(pos) + dm.StikovkaStoyki(pos) + k));
                    pSweepTransform.Add("Глубина", (float)dm.GlubinaStoyki(pos));
                    pSweepTransform.Add("Стыковка", (float)dm.GabaritStoyki(pos));

                    var p = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model, 
                        new SweepTemplatePath($"Порог {dm.Stoyka_Type(pos)}", template, $@"{rootPath}\SW"),  
                        new TemplateTransform
                        {
                            SweepTransform = pSweepTransform,
                        });

                    if(dm.Stoyka_Type(pos) == 41)
                    {

                    }

                    if (dm.IsTermoblock(0) || dm.IsPassivka)
                    {
                        p.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Name = "Маркер 1",
                                Dimensions = new Dictionary<string, float>
                                {
                                    {offset, 3 },
                                    {"ОтНижКрая", 3 }
                                }
                            });

                        if (dm.Otkrivanie == Otkrivanie.Правое || dm.Otkrivanie == Otkrivanie.ПравоеВО)
                            p.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "Маркер 2",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {offset, 3 },
                                        {"ОтНижКрая", 9 }
                                    }
                                });
                    }

                    if (dm.IsTorcShpingalet((short)pos))
                    {
                        var val = (float)(p30 ? dm.GlubinaStoyki(pos) - 23.5 : dm.TorcShpingalet_Otvrtka_Otstup((short)pos));

                        p.InsertLibElement(
                        new ValveCutoutTemplatePath("ТШ ответка в пороге", "Торцевой шпингалет ответка", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {offset, val},
                                {"ОтНижКрая", (float)dm.TorcShpingalet_Otvetka_OtKraya}
                            }
                        });
                    }

                    if (dm.IsOtvAntipan((short)pos))
                    {
                        p.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("PB-DL ответка в пороге", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.OtvAntipan_OtKraya((short)pos)},
                                {"ОтНижКрая", (float)(dm.OtvAntipan_Otstup((short)pos) - 3.6d)},
                                {"Диаметр", (float)dm.OtvAntipan_Diam((short)pos)}
                            }
                        });
                    }

                    if (dm.IsTermoblock((short)pos))
                    {
                        p.InsertLibElement(
                        new FurnitureCutoutTemplatePath("ТБ ответка в пороге", "Термоблокератор ответка", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)dm.GetTermoblock((short)pos).Otstup},
                                {"ОтНижКрая", (float)dm.GetTermoblock((short)pos).OtKraya}
                            }
                        });
                    }

                    break;
                }
            }

            return model.Document;
        }
    }
}

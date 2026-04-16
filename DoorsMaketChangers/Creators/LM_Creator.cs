using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal static class LM_Creator
    {
        public static ModelDoc2 CreateLm(this SW_Model model, LM lm, Command_LM com, string rootPath)
        {
            var isLeft =
                lm.Otkrivanie == Otkrivanie.Левое ||
                lm.Otkrivanie == Otkrivanie.ЛевоеВО;

            var isNo = 
                lm.Otkrivanie == Otkrivanie.Левое || 
                lm.Otkrivanie == Otkrivanie.Правое;

            var offseter = 100.0;

            switch (com)
            {
                case Command_LM.Полотна:
                {
                    SW_TemplateModel lla;
                    SW_TemplateModel vla;
                    SW_TemplateModel llp = null;
                    SW_TemplateModel vlp = null;

                    var llaTransform = new List<ElementTransform>();
                    var llpTransform = new List<ElementTransform>();

                    var sweepLlaTpansform = new Dictionary<string, float>();
                    var sweepLlpTpansform = new Dictionary<string, float>();
                        

                    //Создание трансформирования развертки лицевого листа активки и пассивки
                    sweepLlaTpansform.Add("Ширина", (float)lm.LicevoyList_Width(Stvorka.Активная));
                    sweepLlaTpansform.Add("Высота", (float)lm.LicevoyList_Height - 9);

                    if (lm.IsPassivka)
                    {
                        sweepLlpTpansform.Add("Ширина", (float)lm.LicevoyList_Width(Stvorka.Пассивная));
                        sweepLlpTpansform.Add("Высота", (float)lm.LicevoyList_Height - 9);
                    }

                    //Создание трансформирования лицевого листа активной створки
                    llaTransform.Add(new ElementTransform
                    {
                        Name = "Верхний гиб",
                        Suppress = true,
                        Dimensions = new Dictionary<string, float>
                        {
                            {"Величина", (float)lm.GetGib(Raspolozhenie.Верх, Stvorka.Активная)},
                            {"ОтступЛевый", (float)(isLeft 
                                ? lm.GetGib(Raspolozhenie.Лев, Stvorka.Активная) 
                                : lm.GetGib(Raspolozhenie.Прав, Stvorka.Активная))
                            },
                            {"ОтступПравый", (float)(isLeft 
                                ? lm.GetGib(Raspolozhenie.Прав, Stvorka.Активная) 
                                : lm.GetGib(Raspolozhenie.Лев, Stvorka.Активная))
                            }
                        }
                    });

                    //Создание трансформирования лицевого листа пассивной створки
                    if (lm.IsPassivka)
                        llpTransform.Add(new ElementTransform
                        {
                            Name = "Верхний гиб",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Величина", (float)lm.GetGib(Raspolozhenie.Верх, Stvorka.Пассивная)},
                                {"ОтступЛевый", (float)(isLeft
                                        ? lm.GetGib(Raspolozhenie.Лев, Stvorka.Пассивная)
                                        : lm.GetGib(Raspolozhenie.Прав, Stvorka.Пассивная))
                                },
                                {"ОтступПравый", (float)(isLeft
                                        ? lm.GetGib(Raspolozhenie.Прав, Stvorka.Пассивная)
                                        : lm.GetGib(Raspolozhenie.Лев, Stvorka.Пассивная))
                                }
                            }
                        });

                    //Определение трансформирования нижней части лицевых листов (по порогу)
                    llaTransform.Add(new ElementTransform
                    {
                        Name = "Нижний гиб",
                        Suppress = true,
                        Dimensions = new Dictionary<string, float>
                        {
                            {"Величина", (float)lm.GetGib(Raspolozhenie.Ниж, Stvorka.Активная)},
                            {"ОтступЛевый", (float)(isLeft ? 50 : lm.GetGib(Raspolozhenie.Прав, Stvorka.Активная))},
                            {"ОтступПравый", (float)(isLeft ? lm.GetGib(Raspolozhenie.Прав, Stvorka.Активная) : 50)}
                        }
                    });

                    if(lm.IsPassivka)
                        llpTransform.Add(new ElementTransform
                        {
                            Name = "Нижний гиб",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Величина", (float)lm.GetGib(Raspolozhenie.Ниж, Stvorka.Пассивная)},
                                {"ОтступЛевый", (float)(isLeft ? lm.GetGib(Raspolozhenie.Лев, Stvorka.Пассивная) : 50)},
                                {"ОтступПравый", (float)(isLeft ? 50 : lm.GetGib(Raspolozhenie.Лев, Stvorka.Пассивная))}
                            }
                        });

                    //Добавление лицевого листа активки в файл
                    lla = new SW_TemplateModel(new PositionCoordinates(offseter, 19), model,
                    new SweepTemplatePath("Лицевой Лист Активки", "Лист полотна", $@"{rootPath}\SW"),
                    new TemplateTransform
                    {
                        SweepTransform = sweepLlaTpansform,
                        ElementsTransform = llaTransform
                    });

                    //Смещение точки вставки в файл
                    offseter += lm.LicevoyList_Width(Stvorka.Активная) + 100;

                    //Добавление внутреннего листа активки в файл
                    vla = new SW_TemplateModel(new PositionCoordinates(offseter, 46), model,
                        new SweepTemplatePath("Внутренний Лист Активки", "Лист полотна", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Ширина", (float)lm.VnutrenniyList_Width(Stvorka.Активная)},
                                {"Высота", (float)lm.VnutrenniyList_Height}
                            }
                        });

                    //Добавление пассивной створки в файл
                    if (lm.IsPassivka)
                    {
                        //Смещение точки вставки
                        offseter += lm.VnutrenniyList_Width(Stvorka.Активная) + 100;

                        //Добавление лицевого листа
                        llp = new SW_TemplateModel(new PositionCoordinates(offseter, 19), model,
                            new SweepTemplatePath("Лицевой Лист Пассивки", "Лист полотна", $@"{rootPath}\SW"),
                            new TemplateTransform
                            {
                                SweepTransform = sweepLlpTpansform,
                                ElementsTransform = llpTransform
                            });

                        //Смещение точки вставки
                        offseter += lm.LicevoyList_Width(Stvorka.Пассивная) + 100;

                        //Добавление внутреннего листа 
                        vlp = new SW_TemplateModel(new PositionCoordinates(offseter, 46), model,
                            new SweepTemplatePath("Внутренний Лист Пассивки", "Лист полотна", $@"{rootPath}\SW"),
                            new TemplateTransform
                            {
                                SweepTransform = new Dictionary<string, float>
                                {
                                    {"Ширина", (float)lm.VnutrenniyList_Width(Stvorka.Пассивная)},
                                    {"Высота", (float)lm.VnutrenniyList_Height}
                                }
                            });
                    }

                    //Добавление маркеров низа на детали полотна
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер низа ВЛАС", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
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
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 5},
                                {"ОтНижКрая", (float)(5 + 19)}
                            }
                        });
                    vlp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер низа ВЛПС", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
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
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", isLeft ? 0 : (float)lm.VirezPritvorWidth},
                                {"ОтВерхКрая", 19},
                                {"Высота", 19},
                                {"Ширина", (float)lm.VirezPritvorWidth}
                            }
                        });
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Вырез по нижнему притвору", "Прямоугольный вырез", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", isLeft ? 0 : (float)lm.VirezPritvorWidth},
                                {"ОтНижКрая", 0},
                                {"Высота", 19},
                                {"Ширина", (float)lm.VirezPritvorWidth}
                            }
                        });

                    //Добавление противосъемов
                    vla.InsertLibElement(
                        new FurnitureCutoutTemplatePath($"Противосъм АС", "Противосъем на полотне", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)lm.Protivos_OtKraya},
                                {"ОтНижКрая", (float)lm.VnutrenniyList_Height/2}
                            }
                        });

                    vlp?.InsertLibElement(
                        new FurnitureCutoutTemplatePath($"Противосъм ПС", "Противосъем на полотне", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Protivos_OtKraya},
                                {"ОтНижКрая", (float)lm.VnutrenniyList_Height/2}
                            }
                        });

                    //Добавление сварных просечек
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Левые просечки АС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)lm.VnutrenniyList_Height}
                            }
                        });
                    vla.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Правые просечки АС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтПравКрая", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)lm.VnutrenniyList_Height}
                            }
                        });
                    llp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Просечки ЛЛПС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 0},
                                {"ОтНижКрая", 19},
                                {"Длина", (float)(lm.LicevoyList_Height - 38)}
                            }
                        });
                    vlp?.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Просечки ВЛПС", "Сварные просечки", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 0},
                                {"ОтНижКрая", 0},
                                {"Длина", (float)lm.VnutrenniyList_Height}
                            }
                        });

                    //Добавление торцевых пластин
                    if (lm.IsTorcevayaPlastina(0))
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
                                        ? lm.TorcevayaPlastina(0).OtstupPetlya
                                        : lm.TorcevayaPlastina(0).OtstupZamok)
                                    },
                                    {"ОтНижКрая", 0},
                                    {"Ширина", (float) lm.TorcevayaPlastina(0).Length},
                                    {"Высота", (float) lm.TorcevayaPlastina(0).Width},
                                    {"Зазор", (float) lm.TorcevayaPlastina(0).Gap},
                                    {"Паз", (float) lm.TorcevayaPlastina(0).Groove}
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
                                        ? lm.TorcevayaPlastina(0).OtstupPetlya
                                        : lm.TorcevayaPlastina(0).OtstupZamok)
                                        + 30
                                    },
                                    {"ОтНижКрая", (float)(lm.TorcevayaPlastina(0).Width/2 * -1)}
                                }
                            });

                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижняя торцевая пластина ПС",
                                "Торцевая пластина нижняя", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(0).OtstupPetlya
                                        : lm.TorcevayaPlastina(0).OtstupZamok)
                                    },
                                    {"ОтНижКрая", 0},
                                    {"Ширина", (float) lm.TorcevayaPlastina(0).Length},
                                    {"Высота", (float) lm.TorcevayaPlastina(0).Width},
                                    {"Зазор", (float) lm.TorcevayaPlastina(0).Gap},
                                    {"Паз", (float) lm.TorcevayaPlastina(0).Groove}
                                }
                            });
                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Нижнее малярное ПС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(0).OtstupPetlya
                                        : lm.TorcevayaPlastina(0).OtstupZamok)
                                        + 30
                                    },
                                    {"ОтНижКрая", (float)(lm.TorcevayaPlastina(0).Width/2 * -1)}
                                }
                            });
                    } //нижняя
                    if (lm.IsTorcevayaPlastina(1))
                    {
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхняя торцевая АС", "Торцевая пластина верхняя",
                                $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(1).OtstupPetlya
                                        : lm.TorcevayaPlastina(1).OtstupZamok)},
                                    {"ОтВерхКрая", 0},
                                    {"Ширина", (float) lm.TorcevayaPlastina(1).Length},
                                    {"Высота", (float) lm.TorcevayaPlastina(1).Width},
                                    {"Зазор", (float) lm.TorcevayaPlastina(1).Gap},
                                    {"Паз", (float) lm.TorcevayaPlastina(1).Groove}
                                }
                            });
                        vla.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхнее малярное АС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(1).OtstupPetlya
                                        : lm.TorcevayaPlastina(1).OtstupZamok)
                                        + 30
                                    },
                                    {"ОтВерхКрая", (float)lm.TorcevayaPlastina(1).Width/2}
                                }
                            });

                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхняя торцевая ПС", "Торцевая пластина верхняя",
                                $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(1).OtstupPetlya
                                        : lm.TorcevayaPlastina(1).OtstupZamok)},
                                    {"ОтВерхКрая", 0},
                                    {"Ширина", (float) lm.TorcevayaPlastina(1).Length},
                                    {"Высота", (float) lm.TorcevayaPlastina(1).Width},
                                    {"Зазор", (float) lm.TorcevayaPlastina(1).Gap},
                                    {"Паз", (float) lm.TorcevayaPlastina(1).Groove}
                                }
                            });
                        vlp?.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Верхнее малярное ПС", "Малярное отверстие", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)(isLeft
                                        ? lm.TorcevayaPlastina(1).OtstupPetlya
                                        : lm.TorcevayaPlastina(1).OtstupZamok)
                                        + 30
                                    },
                                    {"ОтВерхКрая", (float)lm.TorcevayaPlastina(1).Width/2}
                                }
                            });
                    } //верхняя

                    //Добавление замков
                    if (lm.Zamok.Kod > 0)
                    {
                        if (lm.Zamok.Ruchka)
                            vla.InsertLibElement(
                                new LockCutoutTemplatePath($"{lm.Zamok.Name} на ВЛАС", lm.Zamok.Name, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2 + lm.Zamok.OtRuchkiDoTela)},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zamok.TeloOtKraya}
                                    }
                                });
                        else
                            vla.InsertLibElement(
                                new LockCutoutTemplatePath($"{lm.Zamok.Name} на ВЛАС", lm.Zamok.Name, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2)},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zamok.OtKrayaVA}
                                    }
                                });

                        llp?.InsertLibElement(
                            new LockCutoutTemplatePath($"Ответка на ЛЛПС", $"{lm.Zamok.Name} ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2)},
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zamok.OtKrayaLP}
                                }
                            });

                        if (lm.Zamok.CM)
                        {
                            lla.InsertLibElement(
                                new LockCutoutTemplatePath($"ЦМ на ЛЛАС", "ЦМ", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = "ЦМ на ЛЛАС",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2 - lm.Zamok.Mezhosevoe)},
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)lm.Zamok.OtKrayaLA}
                                    }
                                });

                            vla.InsertLibElement(
                                new LockCutoutTemplatePath($"ЦМ на ВЛАС", "ЦМ", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Name = $"ЦМ на ВЛАС",
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2 - lm.Zamok.Mezhosevoe)},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zamok.OtKrayaVA}
                                    }
                                });
                        }
                        
                    }

                    //Добавление ручек
                    if (lm.Ruchka_Kod > 0)
                    {
                        var rName = lm.Ruchka_Kod > 10 ? "Ручка на планке" : lm.Ruchka.Name;
                        
                        if (lm.Ruchka.IsZamkovaya && lm.Zamok.IsSovmestima(lm.Ruchka_Kod))
                        {
                            lla.InsertLibElement(
                                new HandleCutoutTemplatePath($"{rName}_ЛЛАС", rName, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2)},
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float) lm.Zamok.OtKrayaLA}
                                    }
                                });

                            vla.InsertLibElement(
                                new HandleCutoutTemplatePath($"{lm.Ruchka.Name}_ВЛАС", lm.Ruchka.Name, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", (float)(46 + lm.VnutrenniyList_Height / 2)},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float) lm.Zamok.OtKrayaVA}
                                    }
                                });
                            
                        }
                        else if(!lm.Ruchka.IsZamkovaya)
                        {
                            switch (lm.Ruchka_Kod)
                            {
                                case (int) LM_RuchkaNames.РЯ_180:
                                {
                                    lla.InsertLibElement(
                                        new HandleCutoutTemplatePath($"РЯ-180_ЛЛАС", "Ручка РЯ-180", $@"{rootPath}\SW"),
                                        new ElementTransform
                                        {
                                            Dimensions = new Dictionary<string, float>
                                            {
                                                {"ОтПола", (float)(19 + lm.LicevoyList_Height / 2 + 39)},
                                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 150}
                                            }
                                        });

                                    break;
                                }
                                case (int) LM_RuchkaNames.Ручка_Потайная:
                                {
                                    lla.InsertLibElement(
                                        new HandleCutoutTemplatePath($"Ручка потайная_ЛЛАС", "Ручка потайная", $@"{rootPath}\SW"),
                                        new ElementTransform
                                        {
                                            Dimensions = new Dictionary<string, float>
                                            {
                                                {"ОтПола", (float)(19 + lm.LicevoyList_Height / 2 + 76)},
                                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 159}
                                            }
                                        });

                                    break;
                                }
                            }
                        }
                    }

                    //Добавление задвижек
                    ValveCutoutTemplatePath template = null;

                    var side = lm.Zadvizhka_OnList == 0
                        ? isLeft ? "ОтПравКрая" : "ОтЛевКрая"
                        : isLeft ? "ОтЛевКрая" : "ОтПравКрая";

                    ElementTransform transform = null;

                    var otPola = (float)(46 + lm.VnutrenniyList_Height / 2 + lm.Zadvizhka_OtCentra);

                    var b = false;

                    switch (lm.Zadvizhka_Kod)
                    {
                        case (short)ZadvizhkaNames.Ночной_сторож:
                            b = true;
                            vla.InsertLibElement(
                                new ValveCutoutTemplatePath("Ночной сторож", "Ночной сторож", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", otPola},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zadvizhka.OtKrayaVA}
                                    }
                                });

                            llp?.InsertLibElement(
                                new ValveCutoutTemplatePath("Ответка НС", "Ночной сторож ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтПола", otPola},
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)lm.Zadvizhka.OtKrayaLP}
                                    }
                                });

                            template = new ValveCutoutTemplatePath("Вертушек НС", "Ночной сторож (вертушек)", $@"{rootPath}\SW");

                            transform = new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтПола", otPola},
                                    {side, (float)lm.Zadvizhka.VertOtKraya}
                                }
                            };

                            break;

                        case (short)ZadvizhkaNames.ЗД_01:
                            b = true;

                            template = new ValveCutoutTemplatePath("ЗД-01", "ЗД-01", $@"{rootPath}\SW");
                                
                            transform = new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтПола", otPola},
                                    {side, (float)lm.Zadvizhka.OtKrayaVA}
                                }
                            };

                            break;

                        case (short)ZadvizhkaNames.ЗТ_150:
                            b = true;

                            template = new ValveCutoutTemplatePath("ЗТ-150", "ЗТ-150", $@"{rootPath}\SW");

                            transform = new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтПола", otPola},
                                    {side, (float)lm.Zadvizhka.OtKrayaVA}
                                }
                            };

                            break;
                    }

                    if (b)
                    {
                        if (lm.Zadvizhka_OnList == 0)
                            lla.InsertLibElement(template, transform);
                        else
                            vla.InsertLibElement(template, transform);
                    }

                    /** на развитие
                    ////Добавление окон
                    //for (short o = 0; o < 4; o++)
                    //{
                    //    if (!dm.IsOkno(o)) continue;

                    //    var oknoLName = $"Окно {o + 1}_ЛЛ{(o < 2 ? "АС" : "ПС")}";
                    //    var oknoVName = $"Окно {o + 1}_ВЛ{(o < 2 ? "АС" : "ПС")}";

                    //    var oknoLTemplate = dm.IsOtsechka(o, 0)
                    //        ? "Прямоугольный вырез (С торцевыми)"
                    //        : "Прямоугольный вырез";
                    //    var oknoVTemplate = dm.IsOtsechka(o, 1)
                    //        ? "Прямоугольный вырез (С торцевыми)"
                    //        : "Прямоугольный вырез";

                    //    var oknoLTransform = new ElementTransform
                    //    {
                    //        Name = oknoLName,
                    //        Dimensions = new Dictionary<string, float>
                    //    {
                    //        {"ОтПола", (float)dm.Okno_OtPola(o, 0)},
                    //        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Okno_OtKraya(o, 0)},
                    //        {"Ширина", dm.Okno_Width(o, 0)},
                    //        {"Высота", dm.Okno_Height(o, 0)}
                    //    }
                    //    };
                    //    var oknoVTransform = new ElementTransform
                    //    {
                    //        Name = oknoVName,
                    //        Dimensions = new Dictionary<string, float>
                    //    {
                    //        {"ОтПола", (float)dm.Okno_OtPola(o, 1)},
                    //        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Okno_OtKraya(o, 1)},
                    //        {"Ширина", dm.Okno_Width(o, 1)},
                    //        {"Высота", dm.Okno_Height(o, 1)}
                    //    }
                    //    };

                    //    if (dm.IsOtsechka(o, 0))
                    //        oknoLTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(o).Width);

                    //    if (dm.IsOtsechka(o, 1))
                    //        oknoVTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(o).Width);

                    //    if (o < 2)
                    //    {
                    //        lla.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(oknoLName, oknoLTemplate, $@"{rootPath}\SW"),
                    //            oknoLTransform);
                    //        vla.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(oknoVName, oknoVTemplate, $@"{rootPath}\SW"),
                    //            oknoVTransform);
                    //    }
                    //    else
                    //    {
                    //        llp?.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(oknoLName, oknoLTemplate, $@"{rootPath}\SW"),
                    //            oknoLTransform);
                    //        vlp?.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(oknoVName, oknoVTemplate, $@"{rootPath}\SW"),
                    //            oknoVTransform);
                    //    }
                    //}

                    ////Добавление вентрешеток
                    //for (short r = 0; r < 4; r++)
                    //{
                    //    if (!dm.IsReshetka(r)) continue;

                    //    var reshLName = $"Решетка {r + 1}_ЛЛ{(r < 2 ? "АС" : "ПС")}";
                    //    var reshVName = $"Решетка {r + 1}_ВЛ{(r < 2 ? "АС" : "ПС")}";

                    //    var reshLTempate = "Прямоугольный вырез";
                    //    var reshVTempate = "Прямоугольный вырез (С торцевыми)";

                    //    var reshLTransform = new ElementTransform
                    //    {
                    //        Name = reshLName,
                    //        Dimensions = new Dictionary<string, float>
                    //    {
                    //        {"ОтПола", dm.Reshetka_OtPola(r)},
                    //        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)dm.Reshetka_OtCrayaLista(r, 0)},
                    //        {"Ширина", dm.Reshetka_Width(r)},
                    //        {"Высота", dm.Reshetka_Height(r)}
                    //    }
                    //    };
                    //    var reshVTransform = new ElementTransform
                    //    {
                    //        Name = reshVName,
                    //        Dimensions = new Dictionary<string, float>
                    //    {
                    //        {"ОтПола", dm.Reshetka_OtPola(r)},
                    //        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)dm.Reshetka_OtCrayaLista(r, 1)},
                    //        {"Ширина", dm.Reshetka_Width(r)},
                    //        {"Высота", dm.Reshetka_Height(r)}
                    //    }
                    //    };

                    //    if (dm.Reshetka_Type(r) == eReshetka.ПП_решетка)
                    //    {
                    //        reshLName = $"ПП-{reshLName}";
                    //        reshVName = $"ПП-{reshVName}";


                    //    }

                    //    if (dm.IsOtsechka(r, 0) || dm.IsOtsechka(r, 1))
                    //        reshVTransform.Dimensions.Add("Пластина", (float)dm.Otsechka(r).Width);

                    //    if (r < 2)
                    //    {
                    //        lla.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(reshLName, "Прямоугольный вырез", $@"{rootPath}\SW"),
                    //            reshLTransform);
                    //        vla.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(reshVName, "Прямоугольный вырез (С торцевыми)", $@"{rootPath}\SW"),
                    //            reshVTransform);
                    //    }
                    //    else
                    //    {
                    //        llp?.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(reshLName, "Прямоугольный вырез", $@"{rootPath}\SW"),
                    //            reshLTransform);
                    //        vlp?.InsertLibElement(
                    //            new TechnologicalCutoutTemplatePath(reshVName, "Прямоугольный вырез (С торцевыми)", $@"{rootPath}\SW"),
                    //            reshVTransform);
                    //    }
                    //}
                    **/
                    
                    break;
                }

                case Command_LM.Коробка:
                {
                    var sTemplate = isNo ? "Стойка М1_М2" : "Стойка М3";
                    var pTemplate = isNo ? "Притолока М1_М2" : "Притолока М3";
                    var sOtPola = isNo ? 0 : 20;

                    var ls = new SW_TemplateModel(new PositionCoordinates(offseter, sOtPola), model, 
                        new SweepTemplatePath("Левая стойка", sTemplate, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", lm.Stoyka_Height(Raspolozhenie.Лев)},
                                {"Ширина", (float)lm.Stoyka_Razvertka(Raspolozhenie.Лев)},
                                {"Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Лев)}
                            }
                        });

                    offseter += lm.Stoyka_Razvertka(Raspolozhenie.Лев) + 100;

                    ls.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 3},
                                {"ОтНижКрая", 3}
                            }
                        });

                    ls.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер 1", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                {"ОтПола", 200},
                                {"Диаметр", 16}
                            }
                        });

                    ls.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер 3", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                {"ОтВерхКрая", 200 - sOtPola},
                                {"Диаметр", 16}
                            }
                        });

                    if (lm.IsZamkovayaStoyka(Raspolozhenie.Лев))
                        ls.InsertLibElement(
                            new LockCutoutTemplatePath("Ответка", $"{lm.Zamok.Name} ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)lm.Zamok_OtvOtstup},
                                    {"ОтПола", sOtPola + lm.Stoyka_Height(Raspolozhenie.Лев)/2}
                                }
                            });
                    else
                        ls.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер 2", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                    {"ОтВерхКрая", sOtPola + lm.Stoyka_Height(Raspolozhenie.Лев)/2},
                                    {"Диаметр", (float)25.4}
                                }
                            });
                    

                    var rs = new SW_TemplateModel(new PositionCoordinates(offseter, sOtPola), model,
                        new SweepTemplatePath("Правая стойка", sTemplate, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", lm.Stoyka_Height(Raspolozhenie.Прав)},
                                {"Ширина", (float)lm.Stoyka_Razvertka(Raspolozhenie.Прав)},
                                {"Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Прав)}
                            }
                        });

                    offseter += lm.Stoyka_Razvertka(Raspolozhenie.Прав) + 100;

                    rs.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 3},
                                {"ОтНижКрая", 3}
                            }
                        });
                    rs.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 3},
                                {"ОтНижКрая", 9}
                            }
                        });

                    rs.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер 1", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                {"ОтПола", 200},
                                {"Диаметр", 16}
                            }
                        });

                    rs.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Анкер 3", "Вырез круглый", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                {"ОтВерхКрая", 200 - sOtPola},
                                {"Диаметр", 16}
                            }
                        });

                    if (lm.IsZamkovayaStoyka(Raspolozhenie.Прав))
                        rs.InsertLibElement(
                            new LockCutoutTemplatePath("Ответка", $"{lm.Zamok.Name} ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)lm.Zamok_OtvOtstup},
                                    {"ОтПола", sOtPola + lm.Stoyka_Height(Raspolozhenie.Прав)/2}
                                }
                            });
                    else
                        rs.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Анкер 2", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)lm.Anker_Otstup},
                                    {"ОтВерхКрая", sOtPola + lm.Stoyka_Height(Raspolozhenie.Прав)/2},
                                    {"Диаметр", (float)25.4}
                                }
                            });

                    var us = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                        new SweepTemplatePath("Притолока", pTemplate, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", lm.Stoyka_Height(Raspolozhenie.Верх)},
                                {"Ширина", (float)lm.Stoyka_Razvertka(Raspolozhenie.Верх)},
                                {"Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Верх)}
                            }
                        });

                    offseter += (float) lm.Stoyka_Razvertka(Raspolozhenie.Верх) + 100;

                    us.InsertLibElement(
                        new ValveCutoutTemplatePath("ТШ ответка верх", "Торцевой шпингалет ответка", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)lm.TorcShpingalet_OtvOtstup},
                                {"ОтНижКрая", (float)lm.TorcShpingalet_OtvOtKraya}
                            }
                        });

                    var ds = new SW_TemplateModel(new PositionCoordinates(offseter, 0), model,
                        new SweepTemplatePath("Порог", pTemplate, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", lm.Stoyka_Height(Raspolozhenie.Ниж)},
                                {"Ширина", (float)lm.Stoyka_Razvertka(Raspolozhenie.Ниж)},
                                {"Глубина", (float)lm.Stoyka_Glubina(Raspolozhenie.Ниж)}
                            }
                        });

                    ds.InsertLibElement(
                        new ValveCutoutTemplatePath("ТШ ответка низ", "Торцевой шпингалет ответка", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"Отступ", (float)lm.TorcShpingalet_OtvOtstup},
                                {"ОтНижКрая", (float)lm.TorcShpingalet_OtvOtKraya}
                            }
                        });

                        break;
                }
            }

            return model.Document;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal static class ODL_Creator
    {
        internal static ModelDoc2 CreateOdl(this SW_Model model, ODL odl, Command_ODL com, string rootPath)
        {
            var isLeft = odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.ЛевоеВО;

            switch (com)
            {
                case Command_ODL.Полотно_активки:
                {
                    var la = new SW_TemplateModel(new PositionCoordinates(100, odl.LicevoyList_OtPola), model, 
                        new SweepTemplatePath("Лист активки", "Лист полотна", $@"{rootPath}\SW"), 
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.LicevoyList_Height},
                                {"Ширина", odl.LicevoyList_Width(Stvorka.Активная)}
                            }
                        });

                    var side = isLeft ? "ОтПравКрая" : "ОтЛевКрая";

                    switch (odl.Ruchka)
                    {
                        case (int)ODL_RuchkaNames.Ручка_черная_планка:
                        case (int)ODL_RuchkaNames.Ручка_Вега:
                        case (int)ODL_RuchkaNames.Ручка_фланец:
                            la.InsertLibElement(
                                new LockCutoutTemplatePath($"{odl.RuchkaName} на лицевом", odl.RuchkaName, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {side, (float)odl.Zamok_OtKraya},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });
                            break;

                        case (int)ODL_RuchkaNames.Ручка_Потайная:
                            la.InsertLibElement(
                                new LockCutoutTemplatePath($"{odl.RuchkaName} на лицевом", odl.RuchkaName, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {side, 81},
                                        {"ОтПола", odl.Ruchka_OtPola}
                                    }
                                });
                            break;

                        case (int)ODL_RuchkaNames.Ручка_РЯ_180:
                            la.InsertLibElement(
                                new LockCutoutTemplatePath($"{odl.RuchkaName} на лицевом", odl.RuchkaName, $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {side, 76},
                                        {"ОтПола", odl.Ruchka_OtPola}
                                    }
                                });
                            break;
                    }

                    if (odl.CM)
                    {
                        var tempName = odl.ZamokName.Equals("Почтовый") 
                            ? "Почтовый ключевина" 
                            : "ЦМ";

                        var k = odl.Zamok(0) == (int) ODL_ZamokNames.ПП 
                            ? 80 
                            : odl.Zamok(0) == (int) ODL_ZamokNames.Г_12_11_ручка_фланец 
                                ? 93 
                                : 0;

                        la.InsertLibElement(
                            new LockCutoutTemplatePath("ЦМ", tempName, $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {side, (float) odl.Zamok_OtKraya},
                                    {"ОтПола", odl.Zamok_OtPola - k}
                                }
                            });
                    }

                    if (odl.Suv)
                        la.InsertLibElement(
                            new LockCutoutTemplatePath("Ключ", "Сувальд", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {side, (float) odl.Zamok_OtKraya},
                                    {"ОтПола", odl.Zamok_OtPola}
                                }
                            });

                    break;
                }

                case Command_ODL.Полотно_пассивки:
                {
                    var lp = new SW_TemplateModel(new PositionCoordinates(100, odl.LicevoyList_OtPola), model, 
                        new SweepTemplatePath("Лист пассивки", "Лист полотна", $@"{rootPath}\SW"), 
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.LicevoyList_Height},
                                {"Ширина", odl.LicevoyList_Width(Stvorka.Пассивная)}
                            }
                        });

                    break;
                }

                case Command_ODL.Замковой_профиль_активки:
                {
                    var zpa = new SW_TemplateModel(new PositionCoordinates(100, odl.ZamkovoyProfil_OtPola), model, 
                        new SweepTemplatePath("Замковой профиль активки", "Лист полотна", $@"{rootPath}\SW"), 
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.ZamkovoyProfil_Height(Stvorka.Активная)},
                                {"Ширина", odl.ZamkovoyProfil_Razvertka(Stvorka.Активная)}
                            }
                        });

                    zpa.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"), 
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 3},
                                {"ОтНижКрая", 3}
                            }
                        });

                    if(!isLeft)
                        zpa.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 3},
                                    {"ОтНижКрая", 9}
                                }
                            });
                    
                    switch (odl.Zamok(0))
                    {
                        case (int)ODL_ZamokNames.ПП:
                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Замок", "Противопожарный", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)19.5},
                                        {"ОтПола", odl.Zamok_OtPola - (float)55.5}
                                    }
                                });

                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("ЦМ", "ЦМ", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)99.5},
                                        {"ОтПола", odl.Zamok_OtPola - (float)80}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Г_12_11_ручка_фланец:
                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Замок", "ГАРДИАН 12.11", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)19.4},
                                        {"ОтПола", odl.Zamok_OtPola - (float)36.9}
                                    }
                                });

                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("ЦМ", "ЦМ", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)87.7},
                                        {"ОтПола", odl.Zamok_OtPola - (float)93}
                                    }
                                });

                                break;

                        case (int)ODL_ZamokNames.Гардиан_10_01:
                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Замок", "ГАРДИАН 10.01", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)20.05},
                                        {"ОтПола", odl.Zamok_OtPola + (float)26}
                                    }
                                });

                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Ключевина", "Сувальд", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)99.5},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Просам_ЗВ_8:
                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Замок", "ПРОСАМ ЗВ-8", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)20.05},
                                        {"ОтПола", odl.Zamok_OtPola + (float)18}
                                    }
                                });

                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Ключевина", "Сувальд", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)87.3},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Почтовый:
                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Замок", "Почтовый", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 17},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            zpa.InsertLibElement(
                                new LockCutoutTemplatePath("Лючок", "Почтовый лючок", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)90.2},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        }

                    switch (odl.Ruchka)
                    {
                        case (int)ODL_RuchkaNames.Ручка_черная_планка:
                            zpa.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Ручка", "Ручка черная на планке", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)99.5},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        case (int)ODL_RuchkaNames.Ручка_Вега:
                            zpa.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Ручка", "Ручка-Вега", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)87.7},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        case (int)ODL_RuchkaNames.Ручка_фланец:
                            zpa.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Ручка", "Ручка на фланце (стяжки)", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)87.7},
                                        {"ОтПола", odl.Zamok_OtPola}
                                    }
                                });

                            break;

                        case (int)ODL_RuchkaNames.Ручка_кнопка:
                            zpa.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Ручка", "Ручка-Кнопка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 88},
                                        {"ОтПола", odl.Ruchka_OtPola}
                                    }
                                });

                            break;
                        }

                    break;
                }

                case Command_ODL.Петлевой_профиль:
                {
                    var pp = new SW_TemplateModel(
                        new PositionCoordinates(100, odl.ZamkovoyProfil_OtPola), model,
                        new SweepTemplatePath("Петлевой профиль", "Лист полотна", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.ZamkovoyProfil_Height(Stvorka.Активная)},
                                {"Ширина", 126}
                            }
                        });

                    pp.InsertLibElement(
                        new FurnitureCutoutTemplatePath("Противосъем", "противосъем на полотне", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтПравКрая" : "ОтЛевКрая", 23},
                                {"ОтПола", 1070}
                            }
                        });

                    pp.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Заглушка низ", "Заглушка с малярным (низ)", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 63},
                                {"ОтНижКрая", 0}
                            }
                        });

                    pp.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Заглушка верх", "Заглушка с малярным (верх)", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 63},
                                {"ОтВерхКрая", 0}
                            }
                        });

                        break;
                }

                case Command_ODL.Замковой_профиль_пассивки:
                {
                    var zpp = new SW_TemplateModel(
                        new PositionCoordinates(100, odl.ZamkovoyProfil_OtPola + 5), model,
                        new SweepTemplatePath("Замковой профиль пассивки", "Лист полотна", $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.ZamkovoyProfil_Height(Stvorka.Пассивная)},
                                {"Ширина", odl.ZamkovoyProfil_Razvertka(Stvorka.Пассивная)}
                            }
                        });

                    zpp.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 3},
                                {"ОтНижКрая", 3}
                            }
                        });

                    if (!isLeft)
                        zpp.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {isLeft ? "ОтЛевКрая" : "ОтПравКрая", 3},
                                    {"ОтНижКрая", 9}
                                }
                            });

                    zpp.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Делга низ", "Делга вертушек", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)46.16},
                                {"ОтНижКрая", 42}
                            }
                        });

                    zpp.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Делга верх", "Делга вертушек", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {isLeft ? "ОтЛевКрая" : "ОтПравКрая", (float)46.16},
                                {"ОтВерхКрая", 42}
                            }
                        });

                        switch (odl.Zamok(0))
                    {
                        case (int)ODL_ZamokNames.ПП:

                            zpp.InsertLibElement(
                                new TechnologicalCutoutTemplatePath($"{odl.ZamokName} ответка", $"{odl.ZamokName} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)42.8},
                                        {"ОтПола", odl.Zamok_OtPola - (float)55.5}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Г_12_11_ручка_фланец:

                            zpp.InsertLibElement(
                                new TechnologicalCutoutTemplatePath($"{odl.ZamokName} ответка", $"{odl.ZamokName} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)42.8},
                                        {"ОтПола", odl.Zamok_OtPola + (float)1}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Гардиан_10_01:

                            zpp.InsertLibElement(
                                new TechnologicalCutoutTemplatePath($"{odl.ZamokName} ответка", $"{odl.ZamokName} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)40.3},
                                        {"ОтПола", odl.Zamok_OtPola - (float)74}
                                    }
                                });

                            break;

                        case (int)ODL_ZamokNames.Просам_ЗВ_8:

                            zpp.InsertLibElement(
                                new TechnologicalCutoutTemplatePath($"{odl.ZamokName} ответка", $"{odl.ZamokName} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {isLeft ? "ОтПравКрая" : "ОтЛевКрая", (float)40.3},
                                        {"ОтПола", odl.Zamok_OtPola - (float)82.5}
                                    }
                                });

                            break;

                        }

                    break;
                }

                case Command_ODL.Замковая_стойка:
                case Command_ODL.Петлевая_стойка:
                {
                    var template = 
                        odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое
                            ? "Стойка С1_С2"
                            : "Стойка С3";

                    var k =  
                        odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое 
                            ? 94.45 
                            : 109.32;

                    var y = odl.Nalichnik(Raspolozhenie.Ниж) > 0 ? 20 : 0;

                    var et = new List<ElementTransform>();

                    if (odl.Nalichnik(Raspolozhenie.Ниж) > 0)
                    {
                        et.Add(new ElementTransform
                        {
                            Name = "Наличник снизу",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>()
                        });
                        et.Add(new ElementTransform
                        {
                            Name = "Стыковка снизу",
                            Suppress = true,
                            Dimensions = new Dictionary<string, float>()
                        });
                    }

                    var zs = new SW_TemplateModel(
                        new PositionCoordinates(100, y), 
                        model, 
                        new SweepTemplatePath("Стойка", template, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", odl.VertStoyka_Height},
                                {"Ширина", (float)(k + odl.Nalichnik_Raz((short) com))},
                                {"Наличник", (float) odl.Nalichnik_Raz((short) com)}
                            },
                            ElementsTransform = et
                        });

                    zs.InsertLibElement(
                        new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                        new ElementTransform
                        {
                            Dimensions = new Dictionary<string, float>
                            {
                                {"ОтЛевКрая", 3},
                                {"ОтНижКрая", 3}
                            }
                        });
                    if (!isLeft)
                        zs.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", 3},
                                    {"ОтНижКрая", 9}
                                }
                            });

                    var pos = com == Command_ODL.Замковая_стойка
                        ? odl.Otkrivanie == Otkrivanie.ЛевоеВО || odl.Otkrivanie == Otkrivanie.ЛевоеВО
                            ? Raspolozhenie.Прав
                            : Raspolozhenie.Лев
                        : odl.Otkrivanie == Otkrivanie.ЛевоеВО || odl.Otkrivanie == Otkrivanie.ЛевоеВО
                            ? Raspolozhenie.Лев
                            : Raspolozhenie.Прав;

                    var offset = 
                        odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое
                            ? 55.47
                            : 57.55;

                    for (var i = 0; i < 3; i++)
                    {
                        zs.InsertLibElement(
                            new TechnologicalCutoutTemplatePath($"Анкер {i + 1}", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)offset},
                                    {"ОтПола", i==0 ? 200 : i==1 ? 1070 : odl.RastDo3Ankera},
                                    {"Диаметр", (float)(i!=1 ? odl.Anker_Diametr : odl.Anker2_Diametr(pos))}
                                }
                            });

                        zs.InsertLibElement(
                            new TechnologicalCutoutTemplatePath($"Прорезь анкер {i + 1}", "Вырез круглый", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", -2},
                                    {"ОтПола", i==0 ? 200 : i==1 ? 1070 : odl.RastDo3Ankera},
                                    {"Диаметр", (float)odl.Anker_Diametr}
                                }
                            });
                    }

                    if (com == Command_ODL.Замковая_стойка)
                    {
                        var c1 = odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое;
                        var otv = false;
                        float otstup = 0, otPola = 0;

                        switch (odl.Zamok(0))
                        {
                            case (int)ODL_ZamokNames.ПП:
                                otv = true;
                                otstup = (float) (c1 ? 59.48 : 55.5);
                                otPola = (float) 944.5;
                                break;

                            case (int)ODL_ZamokNames.Г_12_11_ручка_фланец:
                                otv = true;
                                otstup = (float)(c1 ? 57.47 : 57.67);
                                otPola = (float)1018.5;
                                break;

                            case (int)ODL_ZamokNames.Просам_ЗВ_8:
                                otv = true;
                                otstup = (float)58.3;
                                otPola = (float)917.5;
                                break;

                            case (int)ODL_ZamokNames.Гардиан_10_01:
                                otv = true;
                                otstup = (float)58.3;
                                otPola = (float)921.5;
                                break;

                            case (int)ODL_ZamokNames.Почтовый:
                                otv = true;
                                otstup = (float)(c1 ? 58.25 : 56);
                                otPola = 1000;
                                break;
                        }

                        if(otv)
                            zs.InsertLibElement(
                                new LockCutoutTemplatePath($"Ответка {odl.ZamokName}", $"{odl.ZamokName} ответка", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтЛевКрая", otstup},
                                        {"ОтПола", otPola}
                                    }
                                });
                    }

                    break;
                }

                case Command_ODL.Притолока:
                case Command_ODL.Нижняя_стойка:
                {
                    var isNo = 
                        odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое;

                    var template = isNo
                            ? "Стойка С1_С2"
                            : "Стойка С3";

                    var name = "Притолока";

                    if (com == Command_ODL.Нижняя_стойка)
                        name += " нижняя";

                    var k = isNo
                            ? 94.45
                            : 109.32;

                    var gs = new SW_TemplateModel(new PositionCoordinates(100, 0), model, 
                        new SweepTemplatePath(name, template, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = new Dictionary<string, float>
                            {
                                {"Высота", (float)odl.GorStoyka_Height},
                                {"Ширина", (float)(k + odl.Nalichnik_Raz((short) com))},
                                {"Наличник", (float) odl.Nalichnik_Raz((short) com)}
                            },
                            ElementsTransform = new List<ElementTransform>
                            {
                                new ElementTransform
                                {
                                    Name = "Наличник снизу",
                                    Suppress = true,
                                    Dimensions = new Dictionary<string, float>()
                                },
                                new ElementTransform
                                {
                                    Name = "Стыковка снизу",
                                    Suppress = true,
                                    Dimensions = new Dictionary<string, float>()
                                }
                            }
                        });

                    var otv = com == Command_ODL.Притолока 
                        ? odl.IsTorcevoyShpingalet(0) 
                        : odl.IsTorcevoyShpingalet(1);

                    if (otv)
                    {
                        var offset = isNo ? 28.79 : 29.32;

                        gs.InsertLibElement(
                            new ValveCutoutTemplatePath("Ответка делги", "Делга ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)odl.TorcevoySpingaletOtstup(0)},
                                    {"ОтВерхКрая", (float)(odl.TorcevoySpingaletOtKraya + offset)}
                                }
                            });

                        gs.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", 3},
                                    {"ОтНижКрая", 3}
                                }
                            });
                        if (!isLeft)
                            gs.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтЛевКрая", 3},
                                        {"ОтНижКрая", 9}
                                    }
                                });
                    }

                    break;
                }

                case Command_ODL.Порог:
                {
                    var isNo =
                        odl.Otkrivanie == Otkrivanie.Левое || odl.Otkrivanie == Otkrivanie.Правое;

                    var template = isNo ? "Лист полотна" : "Порог";

                    var st = new Dictionary<string, float>
                    {
                        {"Высота", (float)odl.Porog_Length},
                        {"Ширина", (float)(isNo ? odl.Porog_Width : 122.35)}
                    };

                    if (!isNo)
                    {
                        st.Add("Глубина", (float)59.85);
                        st.Add("Стыковка", (float)60.6);
                    }

                    var p = new SW_TemplateModel(new PositionCoordinates(100, 0), model,
                        new SweepTemplatePath("Порог", template, $@"{rootPath}\SW"),
                        new TemplateTransform
                        {
                            SweepTransform = st
                        });

                    if (odl.IsTorcevoyShpingalet(1))
                    {
                        p.InsertLibElement(
                            new ValveCutoutTemplatePath("Ответка делги", "Делга ответка", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", (float)odl.TorcevoySpingaletOtstup(1)},
                                    {"ОтВерхКрая", (float)odl.TorcevoySpingaletOtKraya}
                                }
                            });

                        p.InsertLibElement(
                            new TechnologicalCutoutTemplatePath("Маркер 1", "Маркер", $@"{rootPath}\SW"),
                            new ElementTransform
                            {
                                Dimensions = new Dictionary<string, float>
                                {
                                    {"ОтЛевКрая", 3},
                                    {"ОтНижКрая", 3}
                                }
                            });

                        if (!isLeft)
                            p.InsertLibElement(
                                new TechnologicalCutoutTemplatePath("Маркер 2", "Маркер", $@"{rootPath}\SW"),
                                new ElementTransform
                                {
                                    Dimensions = new Dictionary<string, float>
                                    {
                                        {"ОтЛевКрая", 3},
                                        {"ОтНижКрая", 9}
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

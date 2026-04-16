using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal static class SW_ModelManager
    {
        public static SW_TemplateLibFeature InsertLibTemplate(this SW_Model model, TemplatePath template)
        {
            model.Document.ClearSelection2(true);
            
            model.Document.Extension.SelectByID2(model.Planes[SW_PlanesName.Спереди].Name, "PLANE", 0, 0, 0, true, 0, null, 0);
            
            model.Document.Extension.SelectByID2($"Point1@{model.StartingPoint.Name}", "EXTSKETCHPOINT", 0, 0, 0, true, 1, null, 0);

            model.Document.InsertLibraryFeature(template.Path);

            model.Document.ClearSelection2(true);

            return (SW_TemplateLibFeature)model.RecordFeatures(template.Name);
        }

        public static SW_TemplateSweepFeature InsertSweepTemplate(this SW_Model model, TemplatePath template)
        {
            var part = (PartDoc)model.Document;

            var ins = model.PartDoc.InsertPart2(template.Path, 515);

            return ins == null ? null : (SW_TemplateSweepFeature)model.RecordFeatures(template.Name);
        }
        
        private static SW_ITemplateFeature RecordFeatures(this SW_Model model, string templateName)
        {
            var features = (object[])model.Document.FeatureManager.GetFeatures(true);
            
            Feature rootFeature = null;
            var elements = new List<Feature>();

            var isSweep = false;

            var dic = new Dictionary<string, string>();

            var b = false;
            var ext = false;

            for (var i = features.Length - 1; i >= 0; i--)
            {
                var feature = (Feature)features[i];

                var tName = feature.GetTypeName2();

                switch (tName)
                {
                    case "Extrusion":
                    case "ICE":
                        elements.Add(feature);
                        break;
                    case "BrokenDerivedPartFolder":
                    case "LibraryFeature":
                        if (b)
                            ext = true;
                        else
                        {
                            if (tName.Equals("BrokenDerivedPartFolder"))
                                isSweep = true;

                            rootFeature = feature;
                            b = true;
                        }
                        break;
                    default:
                        continue;
                }

                if (ext) break;
            }

            var ins = new SW_Insertion(templateName, rootFeature, elements);

            if (isSweep) return new SW_TemplateSweepFeature(ins);

            return new SW_TemplateLibFeature(ins);
        }
    }
}

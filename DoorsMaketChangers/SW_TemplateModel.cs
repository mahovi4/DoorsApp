using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace DoorsMaketChangers
{
    internal class SW_TemplateModel
    {
        public static int DimType = 
            (int)swLibFeatDimensionType_e.swLibFeatSizeDimension;

        private readonly SW_Model root;
        private readonly PositionCoordinates position;

        private float height = 0;
        private float width = 0;

        private readonly SW_TemplateSweepFeature based;
        private readonly List<SW_TemplateLibFeature> insertions 
            = new List<SW_TemplateLibFeature>();

        public string Name { get; }

        public SW_TemplateModel(PositionCoordinates offset, SW_Model rootModel, 
            TemplatePath template, TemplateTransform transform = null)
        {
            position = offset;
            Name = template.Name;
            root = rootModel;

            var ins = root.InsertSweepTemplate(template);

            based = ins ?? throw new Exception();

            if (transform != null)
            {
                transform.Name = Name;
                Transform(transform);
            }
        }

        public bool Transform(TemplateTransform transform)
        {
            if (!transform.SweepTransform.ContainsKey("Высота"))
                throw new Exception("Не найден размер 'Высота'");

            if (!transform.SweepTransform.ContainsKey("Ширина"))
                throw new Exception("Не найден размер 'Ширина'");

            height = transform.SweepTransform["Высота"];
            width = transform.SweepTransform["Ширина"];

            transform.SweepTransform.Add("Отступ", (float)position.X);
            transform.SweepTransform.Add("ОтПола", (float)position.Y);

            return root.TransformTemplate(based, transform);
        }

        public bool InsertLibElement(TemplatePath template, ElementTransform transform = null)
        {
            var ins = root.InsertLibTemplate(template);

            if(ins == null) return false;

            insertions.Add(ins);

            if (transform != null)
            {
                transform.Name = template.Name;
                return TransformLibElement(transform);
            }

            return true;
        }

        public bool TransformLibElement(ElementTransform transform)
        {
            var insertion = insertions.
                FirstOrDefault(ins => ins.Name.IndexOf(transform.Name) >= 0);

            if (insertion == null) return false;

            if (transform.Dimensions.ContainsKey("Отступ"))
                transform.Dimensions["Отступ"] += (float)position.X;

            if (transform.Dimensions.ContainsKey("ОтЛевКрая")) 
            {
                var dim = transform.Dimensions["ОтЛевКрая"];
                transform.Dimensions.Remove("ОтЛевКрая");
                transform.Dimensions.Add("Отступ", dim + (float)position.X); 
            }

            if (transform.Dimensions.ContainsKey("ОтПравКрая"))
            {
                var dim = transform.Dimensions["ОтПравКрая"];
                transform.Dimensions.Remove("ОтПравКрая");
                transform.Dimensions.Add("Отступ", (float)position.X + width - dim);
            }

            if (transform.Dimensions.ContainsKey("ОтНижКрая"))
            {
                var dim = transform.Dimensions["ОтНижКрая"];
                transform.Dimensions.Remove("ОтНижКрая");
                transform.Dimensions.Add("ОтПола", dim + (float)position.Y);
            }

            if (transform.Dimensions.ContainsKey("ОтВерхКрая"))
            {
                var dim = transform.Dimensions["ОтВерхКрая"];
                transform.Dimensions.Remove("ОтВерхКрая");
                transform.Dimensions.Add("ОтПола", (float)position.Y + height - dim);
            }

            var b = insertion.SetDimensions(transform.Dimensions);

            _ = root.Rebuild;

            var def = insertion.Element.GetDefinition();

            var ex = def as ExtrudeFeatureData2;

            ex.AutoSelect = true;
            ex.Merge = true;

            insertion.Element.ModifyDefinition(ex, root.Document, null);

            return b;
        }
    }
}

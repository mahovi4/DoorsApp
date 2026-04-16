using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal static class SW_ModelTransformer
    {
        private static bool UnsuppressElement(this SW_Model model, string elementName, bool unsuppress = true)
        {
            model.Document.ClearSelection2(true);

            var b = model.Document.Extension.SelectByID2(elementName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);

            if (unsuppress)
                model.Document.EditUnsuppress2();
            else
                model.Document.EditSuppress2();

            model.Document.ClearSelection2(true);

            return b;
        }

        private static bool EditDimension(this SW_Model model, string sketchName, string dimName, float dimValue)
        {
            var b = model.Document.Extension.SelectByID2($"{dimName}@{sketchName}@{model.Document.GetTitle()}",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);

            if(b)
                model.Document.Parameter($"{dimName}@{sketchName}").SystemValue = (dimValue / 1000f);

            return b;
        }

        private static bool EditSketch(this SW_Model model, string sketchName)
        {
            var b = model.Document.Extension
                .SelectByID2(sketchName, "SKETCH", 0, 0, 0, false, 0, null, 0);
            model.Document.EditSketch();

            return b;
        }

        private static void InsertSketch(this SW_Model model)
        {
            model.Document.SketchManager.InsertSketch(true);
        }

        private static bool EditSketchDimensions(this SW_Model model, string sketchName, Dictionary<string, float> dimensions)
        {
            var b = model.EditSketch(sketchName);

            foreach (var dim in dimensions)
            {
                var res = model.EditDimension(sketchName, dim.Key, dim.Value);
                if(b) b=res;
            }

            model.InsertSketch();

            return b;
        }

        public static bool TransformElement(this SW_Model model, 
            SW_TemplateSweepFeature template, ElementTransform transform, 
            bool sweepTransform = false)
        {
            var element = sweepTransform ? template.Sweep : template.GetElement(transform.Name);

            if(element == null) return false;

            var b = model.UnsuppressElement(element.Name, transform.Suppress);

            if (transform.Dimensions == null || transform.Dimensions.Count <= 0) return b;

            var elementSketchName = element.GetFirstSubFeature().Name;

            if (string.IsNullOrEmpty(elementSketchName)) return false;

            var res = EditSketchDimensions(model, elementSketchName, transform.Dimensions);

            if (b) b = res;

            return b;
        }

        public static bool TransformTemplate(this SW_Model model, 
            SW_TemplateSweepFeature template, TemplateTransform transform)
        {
            bool b;

            if (transform.SweepTransform == null) return false;

            b = TransformElement(model, template, 
                    new ElementTransform { 
                        Name = transform.Name, 
                        Suppress = true, 
                        Dimensions = transform.SweepTransform,
                    },
                    true
                );

            if (transform.ElementsTransform == null || transform.ElementsTransform.Count == 0) return b;

            foreach (var res in transform.ElementsTransform
                         .SelectMany(elTransform => template.Elements
                                 .Where(element => element.Name.IndexOf(elTransform.Name) >= 0),
                                 (elTransform, element) =>
                                 model.TransformElement(template, elTransform))
                         .Where(res => b)) b = res;

            return b;
        }
    }
}

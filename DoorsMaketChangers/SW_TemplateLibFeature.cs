using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal class SW_TemplateLibFeature : SW_ITemplateFeature
    {
        public static string FeatureName = "LibraryFeature";

        private readonly LibraryFeatureData rootFeature;
        private readonly SW_Insertion block;

        public Dictionary<string, double> Dimensions { get; } 
            = new Dictionary<string, double>();

        public string Name => 
            block.Name;

        public Feature Element => 
            block.Elements[0];

        public SW_TemplateLibFeature(SW_Insertion insertion)
        {
            block = insertion;

            if (!block.RootFeature.GetTypeName2().Equals(FeatureName)) 
                throw new Exception();

            try
            {
                rootFeature = (LibraryFeatureData)block.RootFeature.GetDefinition();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            rootFeature.OverrideDimension = true;

            object obj = Array.Empty<string>();
            var dims = rootFeature.GetDimensions(SW_TemplateModel.DimType, out obj);

            if (!(obj is string[] names)) throw new Exception();

            if(!(dims is double[] values)) throw new Exception();

            if (names.Length == 0 || values.Length == 0 || values.Length != names.Length) throw new Exception();

            for (var i = 0; i < names.Length; i++)
                Dimensions.Add(names[i], values[i]);
        }

        public bool SetDimensions(Dictionary<string, float> dimensions)
        {
            if (dimensions.Count == 0) return false;

            var b = true;

            foreach (var dimension in dimensions)
            {
                foreach (var dim in Dimensions)
                {
                    if (dim.Key.IndexOf(dimension.Key, StringComparison.Ordinal) < 0) continue;

                    var res = rootFeature.SetDimension(SW_TemplateModel.DimType, dim.Key, dimension.Value/1000);

                    if (b) b = res;
                }
            }

            return b;
        }
    }
}

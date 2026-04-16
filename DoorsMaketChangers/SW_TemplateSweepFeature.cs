using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal class SW_TemplateSweepFeature : SW_ITemplateFeature
    {
        public static string FeatureType = "BrokenDerivedPartFolder";
        public static string SweepType = "Extrusion";
        public static string ElementType = "ICE";

        private readonly SW_Insertion block;

        public string Name => 
            block.Name;

        public Feature Sweep { get; }

        public List<Feature> Elements { get; }
            = new List<Feature>();

        public SW_TemplateSweepFeature(SW_Insertion insertion)
        {
            block = insertion;

            foreach (var feature in block.Elements)
            {
                if(feature.GetTypeName2().Equals(SweepType))
                    Sweep = feature;
                if(feature.GetTypeName2().Equals(ElementType))
                    Elements.Add(feature);
            }
        }

        public Feature GetElement(string name)
        {
            foreach(var element in Elements)
                if(element.Name.IndexOf(name) >= 0)
                    return element;

            return null;
        }
    }
}

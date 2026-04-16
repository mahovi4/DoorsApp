using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal class SW_Insertion
    {
        public string Name { get; }

        public Feature RootFeature { get; }

        public List<Feature> Elements { get; }

        public SW_Insertion(string name, Feature feature, List<Feature> elements)
        {
            Name = name;
            RootFeature = feature;
            Elements = elements;

            RootFeature.Name = name;
        }
    }
}

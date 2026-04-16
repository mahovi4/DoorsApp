using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal class TemplateTransform
    {
        public string Name { get; set; }

        public Dictionary<string, float> SweepTransform { get; set; }

        public List<ElementTransform> ElementsTransform { get; set; }
    }
}

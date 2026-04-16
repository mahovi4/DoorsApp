using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoorsMaketChangers
{
    internal class ElementTransform
    {
        public string Name { get; set; }

        public bool Suppress { get; set; }

        public Dictionary<string, float> Dimensions { get; set; }
    }
}

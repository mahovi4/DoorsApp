using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal sealed class PartSweepTemplateSection : TemplateSubSection
    {
        internal override TemplateSection Section => new SweepTemplateSection();

        internal override string Name => null;
    }
}

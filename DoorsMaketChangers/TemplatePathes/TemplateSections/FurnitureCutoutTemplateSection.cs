using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal sealed class FurnitureCutoutTemplateSection : TemplateSubSection
    {
        internal override TemplateSection Section => new CutoutTemplateSection();

        internal override string Name => "Фурнитура";
    }
}

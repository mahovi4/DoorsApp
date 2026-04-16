using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal class HandleCutoutTemplateFile : CutoutTemplateFile
    {
        public HandleCutoutTemplateFile(string name, string fileName, string rootPath, string folderName = null) 
            : base(name, fileName, rootPath, folderName)
        {
        }

        public override TemplateSubSection Section => new FurnitureCutoutTemplateSection();

        public override FurnitureTemplateType FType => new HandleFurnitureTemplateType();
    }
}

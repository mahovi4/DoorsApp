using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal class LockCutoutTemplatePath : TemplatePath
    {
        public LockCutoutTemplatePath(string name, string fileName, string rootPath) 
            : base(name, fileName, rootPath, "Замки")
        {
        }

        public override TemplateSubSection Section => 
            new FurnitureCutoutTemplateSection();

        public override string Path => 
            base.Path + FileCreator.CutTemplateFileExt;
    }
}

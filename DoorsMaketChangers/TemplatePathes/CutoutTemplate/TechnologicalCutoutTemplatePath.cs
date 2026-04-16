using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal class TechnologicalCutoutTemplatePath : TemplatePath
    {
        public TechnologicalCutoutTemplatePath(string name, string fileName, string rootPath, string folderName = null) 
            : base(name, fileName, rootPath, folderName)
        {
        }

        public override TemplateSubSection Section => 
            new TechnologicalCutoutTemplateSection();

        public override string Path =>
            base.Path + FileCreator.CutTemplateFileExt;
    }
}

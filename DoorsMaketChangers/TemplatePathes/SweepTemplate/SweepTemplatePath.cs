using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal class SweepTemplatePath : TemplatePath
    {
        public SweepTemplatePath(string name, string fileName, string rootPath, string folderName = null)
            : base(name, fileName, rootPath, folderName)
        {
        }

        public override TemplateSubSection Section => 
            new PartSweepTemplateSection();

        public override string Path => base.Path + FileCreator.SweepTemplateFileExt;
    }
}
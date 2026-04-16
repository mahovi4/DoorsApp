using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal abstract class TemplatePath
    {
        private readonly string rootPath;
        
        public abstract TemplateSubSection Section { get; }

        public virtual string Path => 
            $@"{rootPath}\{Section.Path}\{(FolderName == null ? "" : $@"{FolderName}\")}{FileName}";

        public string Name { get; }

        public string FileName { get; }

        public string FolderName { get; }

        protected TemplatePath(string name, string fileName, string rootPath, string folderName = null)
        {
            FolderName = folderName;
            FileName = fileName;
            this.rootPath = rootPath;
            Name = name;
        }
    }
}
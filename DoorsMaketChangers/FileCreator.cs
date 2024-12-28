using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorsMaketChangers
{
    internal abstract class FileCreator
    {
        private protected readonly string RootPath;
        private protected readonly string DxfPath;

        protected FileCreator(string maketRootFolder, string dxfFolder)
        {
            RootPath = maketRootFolder;
            DxfPath = dxfFolder;
            if (!Directory.Exists(DxfPath)) Directory.CreateDirectory(DxfPath);
        }


    }
}
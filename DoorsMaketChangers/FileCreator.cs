using COM_DoorsLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dna;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    public class FileCreator : MaketChanger
    {
        public const string CutTemplateFileExt = ".SLDLFP";
        public const string SweepTemplateFileExt = ".SLDPRT";
        

        private SldWorks.SldWorks swApp = new SldWorks.SldWorks();
        private int longstatus, longwarnings;
        
        private List<string> sd_featuresName;

        private SW_Model doc;

        private const bool SaveFile = true;

        public FileCreator(string maketRootFolder, string dxfFolder) 
            : base(maketRootFolder, dxfFolder)
        {
            swApp.Visible = true;
        }

        public override void Build_DM(DM dm, Command_DM com)
        {
            doc = new SW_Model(swApp, RootPath);

            var model = doc.CreateDm(dm, com, RootPath);

            if(SaveFile)
                doc.Save(DxfPath, dm.Name((short)com));

            doc.SaveAsDXF(DxfPath, dm.Name((short)com));

            CloseDoc();
        }

        public override void Build_KVD(KVD kvd, Command_KVD com, string name)
        {
            throw new NotImplementedException();
        }

        public override void Build_LM(LM lm, Command_LM com)
        {
            throw new NotImplementedException();
        }

        public override void Build_ODL(ODL odl, Command_ODL com)
        {
            doc = new SW_Model(swApp, RootPath);

            var model = doc.CreateOdl(odl, com, RootPath);

            if(SaveFile)
                doc.Save(DxfPath, odl.Name((short)com));

            doc.SaveAsDXF(DxfPath, odl.Name((short)com));

            CloseDoc();
        }

        public override void Build_Otboynik(OtboynayaPlastina plastina, string num, int count)
        {
            throw new NotImplementedException();
        }

        public override void Build_VM(DVM vm, Command_VM com)
        {
            throw new NotImplementedException();
        }

        public void CloseDoc()
        {
            if(doc.Document == null) 
                return;

            swApp.CloseDoc("");

            if (doc == null) return;
            
            doc.Close();

            doc = null;
        }

        public override void Exit()
        {
            swApp.ExitApp();
            swApp = null;
        }
    }
}
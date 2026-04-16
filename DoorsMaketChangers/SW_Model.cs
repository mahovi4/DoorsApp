using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace DoorsMaketChangers
{
    internal class SW_Model
    {
        private const string FileTemplate = @"C:\ProgramData\SolidWorks\SOLIDWORKS 2016\templates\gost-part.prtdot";
        private const string PartName = "SW_Макет.SLDPRT";
        private const string FileExt = ".SLDPRT";

        public ModelDoc2 Document { get; private set; }

        public PartDoc PartDoc => (PartDoc)Document;

        public Dictionary<SW_PlanesName, Feature> Planes { get; }
            = new Dictionary<SW_PlanesName, Feature>();

        public Feature StartingPoint { get; }

        public SW_Model(SldWorks.SldWorks app, string rootPath)
        {
            int longstatus = 0, longwarnings = 0;

            //app.NewDocument(FileTemplate, 0, 0, 0);
            //Document = app.OpenDoc6($"{rootPath}/{PartName}", 1, 0, "", ref longstatus, ref longwarnings);

            Document = (ModelDoc2)app.ActiveDoc;

            var features = Document.FeatureManager.GetFeatures(true);

            foreach (var f in features)
            {
                var feature = (Feature)f;

                if (feature.GetTypeName2().Equals("RefPlane"))
                {
                    switch (feature.Name)
                    {
                        case "Спереди":
                            Planes.Add(SW_PlanesName.Спереди, feature);
                            break;
                        case "Сверху":
                            Planes.Add(SW_PlanesName.Сверху, feature);
                            break;
                        case "Справа":
                            Planes.Add(SW_PlanesName.Справа, feature);
                            break;
                    }
                }

                if (feature.GetTypeName2().Equals("OriginProfileFeature"))
                    StartingPoint = feature;
            }
        }

        public bool Rebuild => 
            Document.ForceRebuild3(false);

        public void SaveAsDXF(string dxfPath, string name)
        {
            _ = Document.ForceRebuild3(false);
            var b = PartDoc.ExportFlatPatternView(dxfPath + "\\" + name + ".DXF", 1);
            //var b = Document.SaveAs3($@"{dxfPath}\{name}.DXF", 0,  2);
        }

        public void Save(string path, string name)
        {
            _ = Document.ForceRebuild3(false);
            var p = Document.SaveAs($@"{path}\{name}{FileExt}");
        }

        public void Close()
        {
            Document = null;
        }
    }
}

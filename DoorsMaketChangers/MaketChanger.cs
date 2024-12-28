using System;
using System.IO;
using COM_DoorsLibrary;

namespace DoorsMaketChangers
{
    public abstract class MaketChanger
    {
        private protected readonly string RootPath;
        private protected readonly string DxfPath;

        protected MaketChanger(string maketRootFolder, string dxfFolder)
        {
            RootPath = maketRootFolder;
            DxfPath = dxfFolder;
            if (!Directory.Exists(DxfPath)) Directory.CreateDirectory(DxfPath);
        }

        public abstract void Build_DM(DM dm, Command_DM com);
        public abstract void Build_LM(LM lm, Command_LM com);
        public abstract void Build_VM(DVM vm, Command_VM com);
        public abstract void Build_ODL(ODL odl, Command_ODL com);
        public abstract void Build_KVD(KVD kvd, Command_KVD com, string name);
        public abstract void Build_Otboynik(OtboynayaPlastina plastina, string num);
        public abstract void Exit();
    }

    public enum MaketChangerTypes
    {
        SolidWorks,
        Kompas
    }
    public enum MaketTypes
    {
        DM,
        LM,
        DVM,
        ODL
    }
    public enum Command_DM
    {
        Нет,
        Полотна,
        Полотна2,
        Добор,
        Левая_стойка,
        Правя_стойка,
        Притолока,
        Порог,
        Отбойники
    }
    public enum Command_LM
    {
        Нет,
        Полотна,
        Коробка
    }
    public enum Command_VM
    {
        Нет,
        Полотна,
        Коробка,
        Доборы
    }
    public enum Command_ODL
    {
        Полотно_активки,
        Полотно_пассивки,
        Замковая_стойка,
        Петлевая_стойка,
        Притолока,
        Замковой_профиль_активки,
        Петлевой_профиль,
        Замковой_профиль_пассивки,
        Нижняя_стойка,
        Порог
    }
}

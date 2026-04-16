namespace DoorsMaketChangers
{
    internal sealed class ConstructCutoutTemplateSection : TemplateSubSection
    {
        internal override TemplateSection Section => new CutoutTemplateSection();

        internal override string Name => "Конструктивы";
    }
}
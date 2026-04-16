namespace DoorsMaketChangers
{
    internal abstract class TemplateSubSection
    {
        internal abstract TemplateSection Section { get; }
        internal abstract string Name { get; }

        internal string Path => 
            $@"{Section.SectionName}{(Name != null ? $@"\{Name}" : "")}";
    }
}
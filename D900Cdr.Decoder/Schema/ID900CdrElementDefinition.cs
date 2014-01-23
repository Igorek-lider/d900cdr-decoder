namespace D900Cdr.Schema
{
    public interface ID900CdrElementDefinition
    {
        string Path { get; }
        string Name { get; }
        string Parselet { get; }
        string ValueType { get; }
    }

}

namespace ObjectRepositoryContract
{
    public interface IObject
    {
        object Id { get; }
        string Type { get; }
    }
}
namespace ObjectRepositoryContract
{
    public interface ICollection
    {
        object GetObject(object id);
        void InsertObject(object obj);
        void UpdateObject(object obj);
        void DeleteObject(object obj);
        void RegisterReference(IReference reference);
    }
}
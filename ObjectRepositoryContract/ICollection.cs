namespace ObjectRepositoryContract
{
    public interface ICollection
    {
        IObject GetObject(object id);
        IObject CreateObject(string type);
        void UpdateObject(IObject obj);
        void RemoveObject(IObject obj);
        void RegisterReference(IReference reference);
    }
}
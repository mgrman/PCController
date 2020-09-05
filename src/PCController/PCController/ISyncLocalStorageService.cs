namespace PCController
{
    public interface ISyncLocalStorageService
    {
        bool ContainKey(string key);
        string GetItemAsString(string key);
        void SetItem(string key, string value);
        void Remove(string key);
    }
}
using Plugin.Settings;

namespace PCController
{
    public class SyncLocalStorageService : ISyncLocalStorageService
    {
        public SyncLocalStorageService(int index)
        {
            Index = index;
        }

        public int Index { get; }

        public bool ContainKey(string key)
        {
            return CrossSettings.Current.Contains(GetIndexedKey(key));
        }

        public string GetItemAsString(string key)
        {
            return CrossSettings.Current.GetValueOrDefault(GetIndexedKey(key), string.Empty);
        }

        public void Remove(string key)
        {
            CrossSettings.Current.Remove(GetIndexedKey(key));
        }

        public void SetItem(string key, string value)
        {
            CrossSettings.Current.AddOrUpdateValue(GetIndexedKey(key), value);
        }

        private string GetIndexedKey(string key)
        {
            return $"{key}[{Index}]";
        }
    }
}
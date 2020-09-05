using Plugin.Settings;

namespace PCController
{
    public class SyncLocalStorageService : ISyncLocalStorageService
    {
        public bool ContainKey(string key)
        {
            return CrossSettings.Current.Contains(key);
        }

        public string GetItemAsString(string key)
        {
            return CrossSettings.Current.GetValueOrDefault(key, string.Empty);
        }

        public void Remove(string key)
        {
            CrossSettings.Current.Remove(key);
        }

        public void SetItem(string key, string value)
        {
            CrossSettings.Current.AddOrUpdateValue(key, value);
        }
    }
}
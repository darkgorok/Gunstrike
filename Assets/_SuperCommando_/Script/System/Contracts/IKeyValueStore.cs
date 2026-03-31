public interface IKeyValueStore
{
    bool HasKey(string key);

    int GetInt(string key, int defaultValue = 0);
    void SetInt(string key, int value);

    float GetFloat(string key, float defaultValue = 0f);
    void SetFloat(string key, float value);

    string GetString(string key, string defaultValue = "");
    void SetString(string key, string value);

    void DeleteKey(string key);
    void DeleteAll();
    void Save();
}

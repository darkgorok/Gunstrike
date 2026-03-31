using UnityEngine;

public interface ILevelCatalogService
{
    string GetLevelSceneName(int levelNumber);
    string GetLevelMapResourcePath(int levelNumber);
    GameObject LoadLevelMap(int levelNumber);
    bool HasLevelMap(int levelNumber);
}

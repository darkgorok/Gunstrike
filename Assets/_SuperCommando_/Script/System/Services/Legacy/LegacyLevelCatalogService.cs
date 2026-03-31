using UnityEngine;

public sealed class LegacyLevelCatalogService : ILevelCatalogService
{
    public string GetLevelSceneName(int levelNumber)
    {
        return "Level " + levelNumber;
    }

    public string GetLevelMapResourcePath(int levelNumber)
    {
        return "LevelMap/Final Level/Level Map " + levelNumber;
    }

    public GameObject LoadLevelMap(int levelNumber)
    {
        return Resources.Load(GetLevelMapResourcePath(levelNumber)) as GameObject;
    }

    public bool HasLevelMap(int levelNumber)
    {
        return LoadLevelMap(levelNumber) != null;
    }
}

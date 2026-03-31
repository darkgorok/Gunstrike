using UnityEngine;
using VContainer.Unity;

public static class ProjectLifetimeScopeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (ProjectScope.IsInitialized)
            return;

        var existingScope = LifetimeScope.Find<ProjectLifetimeScope>() as ProjectLifetimeScope;
        if (existingScope != null)
        {
            if (existingScope.Container == null)
                existingScope.Build();

            ProjectScope.SetResolver(existingScope.Container);
            return;
        }

        var scopeObject = new GameObject("[ProjectLifetimeScope]");
        scopeObject.AddComponent<ProjectLifetimeScope>();
    }
}

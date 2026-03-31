using VContainer;

public static class ProjectScope
{
    public static IObjectResolver Resolver { get; private set; }

    public static bool IsInitialized => Resolver != null;

    internal static void SetResolver(IObjectResolver resolver)
    {
        Resolver = resolver;
    }

    public static void EnsureInitialized()
    {
        if (IsInitialized)
            return;

        ProjectLifetimeScopeBootstrap.EnsureInitialized();
    }

    public static void Inject(object target)
    {
        EnsureInitialized();
        Resolver?.Inject(target);
    }

    public static T Resolve<T>() where T : class
    {
        EnsureInitialized();
        return Resolver?.Resolve<T>();
    }
}

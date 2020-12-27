using System.Reflection;
using DumpOnException.StartupHook;

// ReSharper disable once CheckNamespace
internal class StartupHook
{
    // ReSharper disable once NotAccessedField.Local
#pragma warning disable IDE0052 // Remove unread private members
    private static object Listener;
#pragma warning restore IDE0052 // Remove unread private members

    public static void Initialize()
    {
        var loadContext = new StartupAssemblyLoadContext();

        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName("DumpOnException.Dumpster"));

        Listener = assembly.CreateInstance("DumpOnException.Dumpster.Listener");
    }
}
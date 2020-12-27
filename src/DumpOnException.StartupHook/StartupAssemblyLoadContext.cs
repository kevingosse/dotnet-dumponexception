using System.Reflection;
using System.Runtime.Loader;

namespace DumpOnException.StartupHook
{
    internal class StartupAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public StartupAssemblyLoadContext()
        {
            _resolver = new AssemblyDependencyResolver(Assembly.GetExecutingAssembly().Location);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
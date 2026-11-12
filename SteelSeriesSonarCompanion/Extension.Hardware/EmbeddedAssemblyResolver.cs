using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SteelSeriesSonarCompanion.Extension.Hardware
{
	internal static class EmbeddedAssemblyResolver
	{
		private static readonly Dictionary<string, Assembly> LoadedAssemblyMap = new();
		private static bool isRegistered;

		public static void Register ()
		{
			if (isRegistered == true)
			{
				return;
			}

			isRegistered = true;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
		}

		private static Assembly? ResolveEmbeddedAssembly (object? sender, ResolveEventArgs args)
		{
			string? assemblyName = new AssemblyName(args.Name).Name;

			if (assemblyName == null)
			{
				return null;
			}

			lock (LoadedAssemblyMap)
			{
				if (LoadedAssemblyMap.TryGetValue(assemblyName, out Assembly? loadedAssembly) == true)
				{
					return loadedAssembly;
				}

				using Stream? stream = typeof(EmbeddedAssemblyResolver).Assembly.GetManifestResourceStream(assemblyName + ".dll");

				if (stream == null)
				{
					return null;
				}

				byte[] assemblyData = new byte[stream.Length];
				stream.ReadExactly(assemblyData);

				Assembly assembly = Assembly.Load(assemblyData);
				LoadedAssemblyMap[assemblyName] = assembly;
				Trace.WriteLine($"Loaded embedded {assemblyName}");
				return assembly;
			}
		}
	}
}

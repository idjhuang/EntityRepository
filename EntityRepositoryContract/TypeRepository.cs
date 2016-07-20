using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EntityRepositoryContract
{
    public static class TypeRepository
    {
        private static readonly Dictionary<string, Type> TypeTbl = new Dictionary<string, Type>();
        private static readonly List<Assembly> Assemblies = new List<Assembly>();

        static TypeRepository()
        {
        }

        public static void Init()
        {
            var log = new EventLog("Application", ".", "Object Repository");
            var assemblies = Properties.Settings.Default.ModuleList;
            foreach (var name in assemblies)
            {
                try
                {
                    var assembly = Assembly.Load(name);
                    if (assembly != null) Assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    log.WriteEntry($"Load assembly ({name}) failure: {e.Message}", EventLogEntryType.Error);
                }
            }
        }

        public static IList<Type> LoadModules(Type type)
        {
            var modules = new List<Type>();
            foreach (var t in Assemblies.SelectMany(assembly => assembly.GetTypes().Where(type.IsAssignableFrom))
                .Where(t => t.IsClass && !TypeTbl.ContainsKey(t.FullName)))
            {
                modules.Add(t);
                TypeTbl.Add(t.FullName, t);
            }
            return modules;
        }

        public static Type GetType(string typeName)
        {
            return TypeTbl.ContainsKey(typeName) ? TypeTbl[typeName] : null;
        }
    }
}
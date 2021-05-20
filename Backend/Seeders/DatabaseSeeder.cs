using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Server.Seeders
{
    public class DatabaseSeeder
    {
        public static void Run()
        {
            Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Server.Seeders");

            List<string> types = new();

            for (int i = 0; i < typelist.Length; i++)
                types.Add(typelist[i].Name);
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.OrdinalIgnoreCase))
                      .ToArray();
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFisobs.Core
{
    public static class Content
    {
        public static bool IsValidID(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));
            }

            bool validFirstChar = id[0] >= 'a' && id[0] <= 'z' || id[0] >= 'A' && id[0] <= 'Z' || id[0] == '_';
            if (!validFirstChar) {
                return false;
            }
            for (int i = 1; i < id.Length; i++) {
                bool validChar = id[i] >= 'a' && id[i] <= 'z' || id[i] >= 'A' && id[i] <= 'Z' || id[i] >= '0' && id[i] <= '9' || id[i] == '_';
                if (!validChar) {
                    return false;
                }
            }
            return true;
        }

        public static void Register(params IContent[] entries)
        {
            try {
                RegisterInner(entries);
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
                Console.WriteLine(e);
                throw;
            }
        }

        private static void RegisterInner(IContent[] entries)
        {
            // Includes duplicate registries
            var regsDirty = entries.SelectMany(r => r.GetRegistries());
            var regComparer = new RegistryEqualityComparer();

            // Does not include duplicate registries
            var registries = new HashSet<Registry>(regsDirty, regComparer);

            foreach (var entry in entries) {
                foreach (var registry in entry.GetRegistries()) {
                    registry.Process(entry);
                }
            }

            foreach (var registry in registries) {
                registry.Apply();
            }
        }

        private struct RegistryEqualityComparer : IEqualityComparer<Registry>
        {
            public bool Equals(Registry x, Registry y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(Registry obj)
            {
                return obj.ID;
            }
        }
    }
}
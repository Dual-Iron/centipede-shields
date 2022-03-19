#nullable enable
namespace CFisobs.Core
{
    public abstract class Registry
    {
        // Tracking IDs is done just to provide an easy way to check
        // if two IRegistryEntry objects are part of the same registry.
        private static int globalID;
        
        public int ID { get; }

        protected Registry()
        {
            ID = globalID++;
        }

        // Should throw exceptions for invalid entries and process entries.
        protected internal abstract void Process(IContent entry);
        // Should apply changes like MonoMod hooks.
        protected internal abstract void Apply();
    }
}
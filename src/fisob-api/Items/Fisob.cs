#nullable enable
using CFisobs.Common;
using CFisobs.Core;
using System.Collections.Generic;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace CFisobs.Items
{
    public abstract class Fisob : IContent, ICommon
    {
        private readonly List<SandboxUnlock> sandboxUnlocks = new();

        protected Fisob(ObjectType type)
        {
            Type = type;
        }

        public ObjectType Type { get; }

        public Icon Icon { get; set; } = new NoIcon();

        public abstract AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock);

        public virtual ItemProperties? Properties(PhysicalObject forObject) => null;

        public virtual void LoadResources(RainWorld rainWorld)
        {
            string iconName = Ext.LoadAtlasFromEmbeddedResource($"icon_{Type}") ? $"icon_{Type}" : "Futile_White";

            if (Icon is NoIcon) {
                Icon = new SimpleIcon(iconName, Ext.DefaultIconColor);
            }
        }

        public void RegisterUnlock(SandboxUnlock unlock)
        {
            sandboxUnlocks.Add(unlock);
        }

        PhysobType ICommon.Type => Type;

        IList<SandboxUnlock> ICommon.SandboxUnlocks => sandboxUnlocks;

        IEnumerable<Registry> IContent.GetRegistries()
        {
            yield return FisobRegistry.Instance;
            yield return CommonRegistry.Instance;
        }

        AbstractWorldEntity ICommon.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
        {
            return Parse(world, data, unlock);
        }
    }
}

#nullable enable
using CFisobs.Core;
using System.Collections.Generic;

namespace CFisobs.Common
{
    public interface ICommon
    {
        Either<AbstractPhysicalObject.AbstractObjectType, CreatureTemplate.Type> Type { get; }
        IList<SandboxUnlock> SandboxUnlocks { get; }

        ItemProperties? Properties(PhysicalObject forObject);
        AbstractWorldEntity ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock);
        void LoadResources(RainWorld rainWorld);
    }
}

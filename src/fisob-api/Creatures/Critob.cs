#nullable enable
using CFisobs.Common;
using CFisobs.Core;
using System.Collections.Generic;
using CreatureType = CreatureTemplate.Type;

namespace CFisobs.Creatures
{
    public abstract class Critob : IContent, ICommon
    {
        private readonly List<SandboxUnlock> sandboxUnlocks = new();

        protected Critob(CreatureType type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Name { get; }
        public CreatureType Type { get; }
        public Icon Icon { get; set; } = new NoIcon();

        // TODO: Remark that SandboxData will be added if coming from sandbox mode.
        public virtual CreatureState GetState(AbstractCreature acrit) => new HealthState(acrit);
        public virtual void Init(AbstractCreature acrit, World world, WorldCoordinate pos, EntityID id) { }
        public virtual bool GraspParalyzesPlayer(Creature.Grasp grasp) => false;
        public virtual void KillsMatter(CreatureType type, ref bool ret) { }
        public virtual ItemProperties? Properties(PhysicalObject forObject) => null;

        // Must be non-null if AI is true; should be null if AI is false
        public abstract AbstractCreatureAI? GetAbstractAI(AbstractCreature acrit, World world);
        // Must be non-null if AI is true; should be null if AI is false
        public abstract Creature GetRealizedCreature(AbstractCreature acrit);
        public abstract IEnumerable<CreatureTemplate> GetTemplates();
        public abstract void EstablishRelationships();
        public abstract ArtificialIntelligence? GetRealizedAI(AbstractCreature acrit);

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

        Either<AbstractPhysicalObject.AbstractObjectType, CreatureType> ICommon.Type => Type;

        IList<SandboxUnlock> ICommon.SandboxUnlocks => sandboxUnlocks;

        IEnumerable<Registry> IContent.GetRegistries()
        {
            yield return CritobRegistry.Instance;
            yield return CommonRegistry.Instance;
        }

        AbstractWorldEntity ICommon.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
        {
            string creatureString = $"{data}<cB>SandboxData<cC>{unlock.Data}";

            return SaveState.AbstractCreatureFromString(world, creatureString, false);
        }
    }
}

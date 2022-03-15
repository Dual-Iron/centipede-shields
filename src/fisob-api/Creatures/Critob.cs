using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFisobs.Creatures
{
    public abstract class Critob : Fisob
    {
        private readonly List<CreatureTemplate.Type> childTypes = new List<CreatureTemplate.Type>();

        protected Critob(string id, string name) : base(id)
        {
            Type = new LazyEnum<CreatureTemplate.Type>(id);
            Name = name;

            ChildTypes = new ReadOnlyCollection<CreatureTemplate.Type>(childTypes);
        }

        internal void AddChildType(CreatureTemplate.Type childType) => childTypes.Add(childType);

        public string Name { get; }

        new public LazyEnum<CreatureTemplate.Type> Type { get; }

        public ReadOnlyCollection<CreatureTemplate.Type> ChildTypes { get; }

        public virtual void KillsMatter(CreatureTemplate.Type type, ref bool ret) { }

        public abstract IEnumerable<CreatureTemplate> GetTemplates();

        public abstract void EstablishRelationships();

        public abstract Creature GetRealizedCreature(AbstractCreature acrit);

        public abstract ArtificialIntelligence GetRealizedAI(AbstractCreature acrit);

        public virtual AbstractCreatureAI GetAbstractAI(AbstractCreature acrit, World world) => null;

        public virtual CreatureState GetState(AbstractCreature acrit) => null;

        public virtual void Init(AbstractCreature acrit, World world, WorldCoordinate pos, EntityID id) { }

        public virtual bool GraspParalyzesPlayer(Creature.Grasp grasp)
        {
            return false;
        }
    }
}

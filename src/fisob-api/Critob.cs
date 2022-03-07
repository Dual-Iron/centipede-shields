namespace CFisobs
{
    public abstract class Critob : Fisob
    {
        protected Critob(string id) : base(id)
        {
        }

        private CreatureTemplate.Type? type;

        new public CreatureTemplate.Type Type {
            get {
                if (type == null) {
                    type = RWCustom.Custom.ParseEnum<CreatureTemplate.Type>(ID);
                }
                return type.Value;
            }
        }

        public virtual void KillsMatter(CreatureTemplate.Type type, ref bool ret) { }

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

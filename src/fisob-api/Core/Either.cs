#nullable enable

namespace CFisobs.Core
{
    public readonly struct PhysobType
    {
        public readonly AbstractPhysicalObject.AbstractObjectType ObjectType;
        public readonly CreatureTemplate.Type CritType;

        public PhysobType(AbstractPhysicalObject.AbstractObjectType objectType) : this()
        {
            ObjectType = objectType;
        }

        public PhysobType(CreatureTemplate.Type critType) : this()
        {
            CritType = critType;
        }

        public override readonly string? ToString()
        {
            return ObjectType == 0 ? CritType.ToString() : ObjectType.ToString();
        }

        public static implicit operator PhysobType(AbstractPhysicalObject.AbstractObjectType objectType) => new(objectType);
        public static implicit operator PhysobType(CreatureTemplate.Type critType) => new(critType);
    }
}

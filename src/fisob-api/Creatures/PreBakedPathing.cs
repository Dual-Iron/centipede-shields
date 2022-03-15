namespace CFisobs.Creatures
{
    public struct PreBakedPathing
    {
        private byte discriminant; // 0 for none, 1 for original, 2 for ancestor
        private CreatureTemplate.Type ancestor;

        public static PreBakedPathing None => new PreBakedPathing { discriminant = 0 };
        public static PreBakedPathing Original => new PreBakedPathing { discriminant = 1 };
        public static PreBakedPathing From(CreatureTemplate.Type ancestor) => new PreBakedPathing { discriminant = 2, ancestor = ancestor };

        public bool IsNone => discriminant == 0;
        public bool IsOriginal => discriminant == 1;
        public bool IsFromAncestor(out CreatureTemplate.Type ancestor)
        {
            ancestor = this.ancestor;
            return discriminant == 2;
        }
    }
}

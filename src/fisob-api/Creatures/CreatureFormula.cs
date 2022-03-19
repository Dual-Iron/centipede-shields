#nullable enable
using System.Collections.Generic;

namespace CFisobs.Creatures
{
    using TTR = TileTypeResistance;
    using CR = TileConnectionResistance;
    using static AItile.Accessibility;
    using static MovementConnection.MovementType;
    using static Creature.DamageType;

    public sealed class CreatureFormula
    {
        public readonly CreatureTemplate.Type Type;
        public readonly string Name;
        public readonly CreatureTemplate? Ancestor;

        public PreBakedPathing Pathing;
        public TileResist TileResistances;
        public ConnectionResist ConnectionResistances;
        public AttackResist DamageResistances;
        public AttackResist StunResistances;
        public float InstantDeathDamage;
        public bool HasAI;
        public CreatureTemplate.Relationship DefaultRelationship;

        public CreatureFormula(Critob critob) : this(critob.Type, critob.Name)
        { }

        public CreatureFormula(CreatureTemplate.Type type, string name)
        {
            Type = type;
            Name = name;
        }

        // Reuse these lists because there's no reason not to.
        // Not thread-safe but who cares
        static readonly List<TTR> tRs = new List<TTR>(capacity: 8);
        static readonly List<CR> cRs = new List<CR>(capacity: 24);

        public CreatureTemplate IntoTemplate()
        {
            tRs.Clear();
            cRs.Clear();

            AddTileRes(in TileResistances);
            AddConnRes(in ConnectionResistances);

            CreatureTemplate template = new CreatureTemplate(Type, Ancestor, tRs, cRs, DefaultRelationship) {
                name = Name,
                AI = HasAI,
                instantDeathDamageLimit = InstantDeathDamage,
                baseDamageResistance = DamageResistances.Base,
                baseStunResistance = StunResistances.Base,
            };

            AddResistances(template.damageRestistances, in DamageResistances, in StunResistances);

            // doPreBakedPathing    true iff        this creature defines a new pre-baked pathing type
            // requireAImap         true iff        doPreBakedPathing or preBakedPathingAncestor.doPreBakedPathing is true

            if (Pathing.IsFromAncestor(out var pathAncestor)) {
                template.doPreBakedPathing = false;
                template.requireAImap = true;
                template.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(pathAncestor);
            } else if (Pathing.IsOriginal) {
                template.doPreBakedPathing = true;
                template.requireAImap = true;
            } else {
                template.doPreBakedPathing = false;
                template.requireAImap = false;
            }

            return template;
        }

        private static void AddResistances(float[,] res, in AttackResist dmgRes, in AttackResist stunRes)
        {
            res[(int)Blunt, 0] = dmgRes.Blunt;
            res[(int)Stab, 0] = dmgRes.Stab;
            res[(int)Bite, 0] = dmgRes.Bite;
            res[(int)Water, 0] = dmgRes.Water;
            res[(int)Explosion, 0] = dmgRes.Explosion;
            res[(int)Electric, 0] = dmgRes.Electric;

            res[(int)Blunt, 1] = stunRes.Blunt;
            res[(int)Stab, 1] = stunRes.Stab;
            res[(int)Bite, 1] = stunRes.Bite;
            res[(int)Water, 1] = stunRes.Water;
            res[(int)Explosion, 1] = stunRes.Explosion;
            res[(int)Electric, 1] = stunRes.Electric;
        }

        private static void AddTileRes(in TileResist tR)
        {
            // I really wish I had macros while writing this

            if (tR.OffScreen != default) tRs.Add(new TTR(OffScreen, tR.OffScreen.resistance, tR.OffScreen.legality));
            if (tR.Air != default) tRs.Add(new TTR(Air, tR.Air.resistance, tR.Air.legality));
            if (tR.Ceiling != default) tRs.Add(new TTR(Ceiling, tR.Ceiling.resistance, tR.Ceiling.legality));
            if (tR.Climb != default) tRs.Add(new TTR(Climb, tR.Climb.resistance, tR.Climb.legality));
            if (tR.Corridor != default) tRs.Add(new TTR(Corridor, tR.Corridor.resistance, tR.Corridor.legality));
            if (tR.Floor != default) tRs.Add(new TTR(Floor, tR.Floor.resistance, tR.Floor.legality));
            if (tR.Solid != default) tRs.Add(new TTR(Solid, tR.Solid.resistance, tR.Solid.legality));
            if (tR.Wall != default) tRs.Add(new TTR(Wall, tR.Wall.resistance, tR.Wall.legality));
        }

        private static void AddConnRes(in ConnectionResist cR)
        {
            // Thank Stephen Cole Kleene for RegEx. This is the closest we'll come to C# macros.

            // MATCH    public PathCost (\w+);
            // REPLACE  if (cR.$1 != default) cRs.Add(new CR($1, cR.$1.resistance, cR.$1.legality));

            if (cR.Standard != default) cRs.Add(new CR(Standard, cR.Standard.resistance, cR.Standard.legality));
            if (cR.ReachOverGap != default) cRs.Add(new CR(ReachOverGap, cR.ReachOverGap.resistance, cR.ReachOverGap.legality));
            if (cR.ReachUp != default) cRs.Add(new CR(ReachUp, cR.ReachUp.resistance, cR.ReachUp.legality));
            if (cR.DoubleReachUp != default) cRs.Add(new CR(DoubleReachUp, cR.DoubleReachUp.resistance, cR.DoubleReachUp.legality));
            if (cR.ReachDown != default) cRs.Add(new CR(ReachDown, cR.ReachDown.resistance, cR.ReachDown.legality));
            if (cR.SemiDiagonalReach != default) cRs.Add(new CR(SemiDiagonalReach, cR.SemiDiagonalReach.resistance, cR.SemiDiagonalReach.legality));
            if (cR.DropToFloor != default) cRs.Add(new CR(DropToFloor, cR.DropToFloor.resistance, cR.DropToFloor.legality));
            if (cR.DropToClimb != default) cRs.Add(new CR(DropToClimb, cR.DropToClimb.resistance, cR.DropToClimb.legality));
            if (cR.DropToWater != default) cRs.Add(new CR(DropToWater, cR.DropToWater.resistance, cR.DropToWater.legality));
            if (cR.LizardTurn != default) cRs.Add(new CR(LizardTurn, cR.LizardTurn.resistance, cR.LizardTurn.legality));
            if (cR.OpenDiagonal != default) cRs.Add(new CR(OpenDiagonal, cR.OpenDiagonal.resistance, cR.OpenDiagonal.legality));
            if (cR.Slope != default) cRs.Add(new CR(Slope, cR.Slope.resistance, cR.Slope.legality));
            if (cR.CeilingSlope != default) cRs.Add(new CR(CeilingSlope, cR.CeilingSlope.resistance, cR.CeilingSlope.legality));
            if (cR.ShortCut != default) cRs.Add(new CR(ShortCut, cR.ShortCut.resistance, cR.ShortCut.legality));
            if (cR.NPCTransportation != default) cRs.Add(new CR(NPCTransportation, cR.NPCTransportation.resistance, cR.NPCTransportation.legality));
            if (cR.BigCreatureShortCutSqueeze != default) cRs.Add(new CR(BigCreatureShortCutSqueeze, cR.BigCreatureShortCutSqueeze.resistance, cR.BigCreatureShortCutSqueeze.legality));
            if (cR.OutsideRoom != default) cRs.Add(new CR(OutsideRoom, cR.OutsideRoom.resistance, cR.OutsideRoom.legality));
            if (cR.SideHighway != default) cRs.Add(new CR(SideHighway, cR.SideHighway.resistance, cR.SideHighway.legality));
            if (cR.SkyHighway != default) cRs.Add(new CR(SkyHighway, cR.SkyHighway.resistance, cR.SkyHighway.legality));
            if (cR.SeaHighway != default) cRs.Add(new CR(SeaHighway, cR.SeaHighway.resistance, cR.SeaHighway.legality));
            if (cR.RegionTransportation != default) cRs.Add(new CR(RegionTransportation, cR.RegionTransportation.resistance, cR.RegionTransportation.legality));
            if (cR.BetweenRooms != default) cRs.Add(new CR(BetweenRooms, cR.BetweenRooms.resistance, cR.BetweenRooms.legality));
            if (cR.OffScreenMovement != default) cRs.Add(new CR(OffScreenMovement, cR.OffScreenMovement.resistance, cR.OffScreenMovement.legality));
            if (cR.OffScreenUnallowed != default) cRs.Add(new CR(OffScreenUnallowed, cR.OffScreenUnallowed.resistance, cR.OffScreenUnallowed.legality));
        }
    }

    // Some notes on the fields of CreatureTemplate.

    // requireAImap         should be true iff doPreBakedPathing or preBakedPathingAncestor.doPreBakedPathing
    // offScreenSpeed       how fast the creature moves between abstract rooms
    // abstractLaziness     how long it takes the creature to start migrating
    // smallCreature        determines if rocks instakill, if large predators ignore it, etc
    // dangerToPlayer       DLLs are 0.85, spiders are 0.1, pole plants are 0.5
    // waterVision          0..1 how well the creature can see through water
    // throughSurfaceVision 0..1 how well the creature can see through water surfaces
    // movementBasedVision  0..1 bonus to vision for moving creatures
    // lungCapacity         ticks until the creature falls unconscious from drowning
    // quickDeath           determines if the creature should die in Creature.Violence(). if false, you must define custom death logic
    // saveCreature         determines if the creature is saved after a cycle ends. false for overseers and garbage worms
    // hibernateOffScreen   self-explanatory. true for deer, miros birds, leviathans, vultures, and scavengers
    // bodySize             eggbugs are 0.4, DLLs are 5.5, slugcats are 1
}

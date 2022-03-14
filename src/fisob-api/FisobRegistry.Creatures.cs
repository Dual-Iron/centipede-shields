using UnityEngine;
using System;
using System.Collections.Generic;

namespace CFisobs
{
    public struct PreBakedPathing
    {
        private byte type; // 0 for none, 1 for original, 2 for ancestor
        private CreatureTemplate.Type ancestor;

        public static PreBakedPathing None => new PreBakedPathing { type = 0 };
        public static PreBakedPathing Original => new PreBakedPathing { type = 1 };
        public static PreBakedPathing From(CreatureTemplate.Type ancestor) => new PreBakedPathing { type = 2, ancestor = ancestor };

        public bool IsNone => type == 0;
        public bool IsOriginal => type == 1;
        public bool IsFromAncestor(out CreatureTemplate.Type ancestor)
        {
            ancestor = this.ancestor;
            return type == 2;
        }
    }

    public struct TileResistance
    {
        public PathCost OffScreen; // when abstracted
        public PathCost Floor;
        public PathCost Corridor;
        public PathCost Climb;
        public PathCost Wall;
        public PathCost Ceiling;
        public PathCost Air;
        public PathCost Solid;
    }

    public struct MovementResistance
    {
        public PathCost Standard;
        public PathCost ReachOverGap;
        public PathCost ReachUp;
        public PathCost DoubleReachUp;
        public PathCost ReachDown;
        public PathCost SemiDiagonalReach;
        public PathCost DropToFloor;
        public PathCost DropToClimb;
        public PathCost DropToWater;
        public PathCost LizardTurn;
        public PathCost OpenDiagonal;
        public PathCost Slope;
        public PathCost CeilingSlope;
        public PathCost ShortCut;
        public PathCost NPCTransportation;
        public PathCost BigCreatureShortCutSqueeze;
        public PathCost OutsideRoom;
        public PathCost SideHighway;
        public PathCost SkyHighway;
        public PathCost SeaHighway;
        public PathCost RegionTransportation;
        public PathCost BetweenRooms;
        public PathCost OffScreenMovement;
        public PathCost OffScreenUnallowed;
    }

    public struct AttackResistance
    {
        public float All;
        public float Blunt;
        public float Stab;
        public float Bite;
        public float Water;
        public float Explosion;
        public float Electric;
    }

    public enum KillSignificance : byte
    {
        None,
        CountsAgainstSaint,
        CountsTowardsOutlaw
    }

    public sealed class CreatureTemplateData
    {
        // requireAImap should be true iff doPreBakedPathing or preBakedPathingAncestor.doPreBakedPathing

        public readonly string Name;
        public CreatureTemplate.Type? Ancestor;
        public PreBakedPathing Pathing;
        public TileResistance TileResistances;
        public MovementResistance MovementResistances;
        public AttackResistance DamageResistances;
        public AttackResistance StunResistances;
        public float InstantDeathDamage;
        public bool HasAI;
        public bool IsQuantified;
        public float AbstractSpeed; // how fast the creature moves between abstract rooms
        public int AbstractLaziness; // how long it takes the creature to start migrating
        public BreedParameters BreedParameters;
        public bool CanFly;
        public int Grasps;
        public bool PutsFoodInDen;
        public bool IsSmall; // if rocks instakill, if apex predators ignore it, etc if it's mostly ignored by macro creatures
        public float DangerToPlayer; // 0..1, DLLs are .85, spiders are .1, pole plants are .5
        public float RoamInRoomChance;
        public float RoamBetweenRoomsChance;

        public float VisionRadius;
        public float VisionThruWater = 0.4f; // proficiency at seeing a point that passes through deep water
        public float VisionThruWaterSurface = 0.8f; // proficiency at seeing a point that passes through the surface of water
        public float VisionMovementBonus = 0.2f; // bonus to vision on moving targets

        public CreatureCommunities.CommunityID Community = CreatureCommunities.CommunityID.All;
        public float CommunityInfluence = 0.5f;

        public KillSignificance KillSignificance = KillSignificance.CountsTowardsOutlaw;
        public int MeatPoints;

        public float LungCapacity = 520f; // ticks it takes to fall unconscious from drowning
        public bool QuickDeath = true; // true if the creature should die as defined by Creature.Violence. if false, custom death logic must be used

        // TODO fill out relevant fields for CreatureTemplate

        public CreatureTemplateData(string name)
        {
            Name = name;
        }
    }

    public sealed partial class FisobRegistry
    {
        void ApplyCreatures()
        {
            RegisterCustomCreatures();

            On.Player.Grabbed += PlayerGrabbed;
            On.AbstractCreature.Realize += Realize;
            On.AbstractCreature.InitiateAI += InitiateAI;
            On.AbstractCreature.ctor += Ctor;
            On.CreatureSymbol.DoesCreatureEarnATrophy += KillsMatter;
        }

        private static void RegisterCustomCreatures()
        {
            List<CreatureTemplate> newTemplates = new List<CreatureTemplate>();


        }

        // TODO use the following mosquito example to register custom creatures:
        //public static void AddCustomCreatures()
        //{
        //    List<CreatureTemplate> list = new List<CreatureTemplate>(StaticWorld.creatureTemplates);

        //    var tileRes = new List<TileTypeResistance>();
        //    var connRes = new List<TileConnectionResistance>();

        //    CreatureTemplate preBakedPathingAncestor = list.FirstOrDefault(c => c.type == CreatureTemplate.Type.Fly);

        //    tileRes.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
        //    connRes.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 1f, PathCost.Legality.Allowed));

        //    CreatureTemplate template = new CreatureTemplate(EnumExt_Mosquitoes.Mosquito, null, tileRes, connRes, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)) {
        //        abstractedLaziness = 200,
        //        roamBetweenRoomsChance = 0.07f,
        //        baseDamageResistance = 0.95f,
        //        baseStunResistance = 0.6f,
        //        doPreBakedPathing = false,
        //        offScreenSpeed = 0.1f,
        //        AI = true,
        //        requireAImap = true,
        //        bodySize = 0.5f,
        //        stowFoodInDen = true,
        //        shortcutSegments = 2,
        //        preBakedPathingAncestor = preBakedPathingAncestor,
        //        grasps = 1,
        //        visualRadius = 800f,
        //        movementBasedVision = 0.65f,
        //        communityInfluence = 0.1f,
        //        waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface,
        //        waterPathingResistance = 2f,
        //        canFly = true,
        //        meatPoints = 3,
        //        dangerousToPlayer = 0.4f
        //    };

        //    list.Add(template);



        //    int length = Enum.GetValues(typeof(CreatureTemplate.Type)).Length;

        //    StaticWorld.creatureTemplates = new CreatureTemplate[length];

        //    for (int i = 0; i < list.Count; i++) {
        //        int type = (int)list[i].type;
        //        if (type != -1) {
        //            if (StaticWorld.creatureTemplates.Length <= type) {
        //                Array.Resize(ref StaticWorld.creatureTemplates, type + 1);
        //            }
        //            StaticWorld.creatureTemplates[type] = list[i];
        //        }
        //    }

        //    for (int j = 0; j < StaticWorld.creatureTemplates.Length; j++) {
        //        CreatureTemplate creatureTemplate2 = StaticWorld.creatureTemplates[j];
        //        if (creatureTemplate2 == null) {
        //            creatureTemplate2 = StaticWorld.creatureTemplates[j] = new CreatureTemplate((CreatureTemplate.Type)j, null, new List<TileTypeResistance>(), new List<TileConnectionResistance>(), new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        //        }
        //        if (creatureTemplate2.relationships.Length < length) {
        //            Array.Resize(ref creatureTemplate2.relationships, length);
        //            for (int k = creatureTemplate2.relationships.Length; k < length; k++) {
        //                creatureTemplate2.relationships[k] = creatureTemplate2.relationships[0];
        //            }
        //        }
        //    }

        //    for (int l = 0; l < StaticWorld.creatureTemplates.Length; l++) {
        //        Debug.Log($"{l}: {StaticWorld.creatureTemplates[l].type}, {StaticWorld.creatureTemplates[l].relationships?.Length.ToString() ?? "NULL"} relationships");
        //    }

        //    StaticWorld.EstablishRelationship(EnumExt_Mosquitoes.Mosquito, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
        //}

        private void PlayerGrabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);

            if (grasp?.grabber?.abstractCreature != null && TryGet(grasp.grabber.abstractCreature.creatureTemplate.TopAncestor().type, out var critob) && critob.GraspParalyzesPlayer(grasp)) {
                self.dangerGraspTime = 0;
                self.dangerGrasp = grasp;
            }
        }

        private void InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
        {
            orig(self);

            if (TryGet(self.creatureTemplate.TopAncestor().type, out var crit)) {
                if (self.abstractAI != null && self.creatureTemplate.AI) {
                    self.abstractAI.RealAI = crit.GetRealizedAI(self)
                        ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedAI returned null but template.AI was true!");
                }
            }
        }

        private void Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            if (self.realizedCreature == null && TryGet(self.creatureTemplate.TopAncestor().type, out var crit)) {
                self.realizedObject = crit.GetRealizedCreature(self)
                    ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedCreature returned null!");

                self.InitiateAI();

                foreach (var stuck in self.stuckObjects) {
                    if (stuck.A.realizedObject == null) {
                        stuck.A.Realize();
                    }
                    if (stuck.B.realizedObject == null) {
                        stuck.B.Realize();
                    }
                }
            }

            orig(self);
        }

        private void Ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate template, Creature real, WorldCoordinate pos, EntityID id)
        {
            orig(self, world, template, real, pos, id);

            if (TryGet(template.TopAncestor().type, out var crit)) {
                // Set creature state
                self.state = crit.GetState(self) ?? new HealthState(self);

                // Set creature AI
                AbstractCreatureAI abstractAI = crit.GetAbstractAI(self, world);

                if (template.AI) {
                    self.abstractAI = abstractAI ?? new AbstractCreatureAI(world, self);

                    bool setDenPos = pos.abstractNode > -1 && pos.abstractNode < self.Room.nodes.Length
                        && self.Room.nodes[pos.abstractNode].type == AbstractRoomNode.Type.Den && !pos.TileDefined;

                    if (setDenPos) {
                        abstractAI.denPosition = pos;
                    }
                } else if (abstractAI is object) {
                    Debug.LogError($"{crit.GetType()}::GetAbstractAI returned a non-null object but template.AI was false!");
                }

                // Arbitrary setup
                crit.Init(self, world, pos, id);
            }
        }

        private bool KillsMatter(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureTemplate.Type creature)
        {
            var ret = orig(creature);
            if (TryGet(StaticWorld.GetCreatureTemplate(creature).TopAncestor().type, out var critob)) {
                critob.KillsMatter(creature, ref ret);
            }
            return ret;
        }
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

namespace CFisobs
{
    public sealed class CreatureTemplateData
    {
        public readonly string Name;
        public readonly CreatureTemplate.Type Ancestor;
        public readonly CreatureTemplate.Type PathingAncestor;
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
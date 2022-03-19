using System;
using Noise;
using RWCustom;
using UnityEngine;

namespace CentiShields.Mosquitoes
{
    sealed class MosquitoAI : ArtificialIntelligence, IUseARelationshipTracker
    {
        enum Behavior
        {
            Idle,
            Flee,
            EscapeRain,
            ReturnPrey,
            Hunt
        }

        sealed class MosquitoTrackedState : RelationshipTracker.TrackedCreatureState
        {
            public float bloodTaken;
        }

        public Mosquito bug;
        public float currentUtility;
        public int tiredOfHuntingCounter;
        public AbstractCreature tiredOfHuntingCreature;
        private Behavior behavior;
        private int idlePosCounter;
        private WorldCoordinate tempIdlePos;

        public MosquitoAI(AbstractCreature acrit) : base(acrit, acrit.world)
        {
            bug = acrit.realizedCreature as Mosquito;
            bug.AI = this;
            AddModule(new StandardPather(this, acrit.world, acrit));
            pathFinder.stepsPerFrame = 20;
            AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
            AddModule(new ThreatTracker(this, 3));
            AddModule(new RainTracker(this));
            AddModule(new DenFinder(this, acrit));
            AddModule(new NoiseTracker(this, tracker));
            AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
            AddModule(new UtilityComparer(this));
            AddModule(new RelationshipTracker(this, tracker));
            var smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
            utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
            utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
            utilityComparer.AddComparedModule(preyTracker, null, 0.4f, 1.1f);
            noiseTracker.hearingSkill = 0.5f;
            behavior = Behavior.Idle;
        }

        AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            return relationship.type switch {
                CreatureTemplate.Relationship.Type.Eats => preyTracker,
                CreatureTemplate.Relationship.Type.Afraid => threatTracker,
                _ => null
            };
        }

        RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return new MosquitoTrackedState();
        }

        CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if (dRelation.state is not MosquitoTrackedState state) return default;

            if (dRelation.trackerRep.VisualContact) {
                dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
            }

            if (dRelation.trackerRep.representedCreature.realizedObject is Creature c && c.State.alive && bug.grasps[0]?.grabbed == c) {
                state.bloodTaken += bug.bloat - bug.lastBloat;
            }

            var result = StaticRelationship(dRelation.trackerRep.representedCreature);
            if (!dRelation.state.alive) {
                result.intensity = 0f;
            } else if (result.type == CreatureTemplate.Relationship.Type.Eats) {
                result.intensity = Mathf.Lerp(result.intensity, 0f, Mathf.Sqrt(state.bloodTaken));

                if (result.intensity < 0.1f) {
                    result.intensity = 1f - result.intensity;
                    result.type = CreatureTemplate.Relationship.Type.StayOutOfWay;
                }
            }

            return result;
        }

        public override void Update()
        {
            base.Update();

            if (bug.room == null) {
                return;
            }

            pathFinder.walkPastPointOfNoReturn = stranded 
                || denFinder.GetDenPosition() == null 
                || !pathFinder.CoordinatePossibleToGetBackFrom(denFinder.GetDenPosition().Value) 
                || threatTracker.Utility() > 0.95f;

            utilityComparer.GetUtilityTracker(threatTracker).weight = Custom.LerpMap(threatTracker.ThreatOfTile(creature.pos, true), 0.1f, 2f, 0.1f, 1f, 0.5f);

            currentUtility = utilityComparer.HighestUtility();

            AIModule aimodule = utilityComparer.HighestUtilityModule();
            behavior = aimodule switch {
                ThreatTracker => Behavior.Flee,
                RainTracker => Behavior.EscapeRain,
                PreyTracker => Behavior.Hunt,
                _ => behavior
            };

            if (currentUtility < 0.02f && !(behavior == Behavior.Hunt && preyTracker.MostAttractivePrey != null)) {
                behavior = Behavior.Idle;
            }

            switch (behavior) {
                case Behavior.Idle:
                    bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f + 0.5f * Mathf.Max(threatTracker.Utility(), 1f), 0.01f, 0.016666668f);
                    bool toNewRoom = pathFinder.GetDestination.room != bug.room.abstractRoom.index;
                    if (!toNewRoom && idlePosCounter <= 0) {
                        int abstractNode = bug.room.abstractRoom.RandomNodeInRoom().abstractNode;
                        if (bug.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit) {
                            int num = bug.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, bug.Template);
                            if (num > -1) {
                                int num2 = bug.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(bug.abstractCreature.pos.Tile, num, bug.Template);
                                if (num2 > -1 && num2 < 400 && bug.room.game.world.GetAbstractRoom(bug.room.abstractRoom.connections[abstractNode]) is AbstractRoom room) {
                                    WorldCoordinate worldCoordinate = room.RandomNodeInRoom();
                                    if (pathFinder.CoordinateReachableAndGetbackable(worldCoordinate)) {
                                        Debug.Log("scorpion leaving room");
                                        creature.abstractAI.SetDestination(worldCoordinate);
                                        idlePosCounter = UnityEngine.Random.Range(200, 500);
                                        toNewRoom = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!toNewRoom) {
                        WorldCoordinate coord = new(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
                        if (IdleScore(coord) < IdleScore(tempIdlePos)) {
                            tempIdlePos = coord;
                        }
                        if (IdleScore(tempIdlePos) < IdleScore(pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f)) {
                            SetDestination(tempIdlePos);
                            idlePosCounter = UnityEngine.Random.Range(200, 800);
                            tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
                        }
                    }
                    idlePosCounter--;
                    break;
                case Behavior.Flee:
                    bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
                    creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 10, 30, true));
                    break;
                case Behavior.Hunt:
                    bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, .1f);
                    creature.abstractAI.SetDestination(preyTracker.MostAttractivePrey.BestGuessForPosition());

                    tiredOfHuntingCounter++;
                    if (tiredOfHuntingCounter > 200) {
                        tiredOfHuntingCreature = preyTracker.MostAttractivePrey.representedCreature;
                        tiredOfHuntingCounter = 0;
                        preyTracker.ForgetPrey(tiredOfHuntingCreature);
                        tracker.ForgetCreature(tiredOfHuntingCreature);
                    }
                    break;
                case Behavior.EscapeRain:
                    bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
                    if (denFinder.GetDenPosition() != null) {
                        creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
                    }
                    break;
            }
        }

        private float IdleScore(WorldCoordinate coord)
        {
            if (coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord) || bug.room.aimap.getAItile(coord).acc >= AItile.Accessibility.Wall) {
                return float.MaxValue;
            }
            float result = 1f;
            if (pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1))) {
                result += 10f;
            }
            if (bug.room.aimap.getAItile(coord).narrowSpace) {
                result += 50f;
            }
            result += threatTracker.ThreatOfTile(coord, true) * 1000f;
            result += threatTracker.ThreatOfTile(bug.room.GetWorldCoordinate((bug.room.MiddleOfTile(coord) + bug.room.MiddleOfTile(creature.pos)) / 2f), true) * 1000f;
            for (int i = 0; i < noiseTracker.sources.Count; i++) {
                result += Custom.LerpMap(Vector2.Distance(bug.room.MiddleOfTile(coord), noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
            }
            return result;
        }

        public override bool WantToStayInDenUntilEndOfCycle()
        {
            return rainTracker.Utility() > 0.01f;
        }

        public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
        {
            return otherCreature.creatureTemplate.smallCreature
                ? new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false)
                : new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
        }

        public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
        {
            float val = Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, false) - threatTracker.ThreatOfTile(creature.pos, false));
            return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
        }
    }
}

using CFisobs.Common;
using CFisobs.Creatures;
using System.Collections.Generic;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;

namespace CentiShields.Mosquitoes
{
    sealed class MosquitoCritob : Critob
    {
        public MosquitoCritob() : base(EnumExt_Mosquito.Mosquito, "Mosquito")
        {
            RegisterUnlock(new(EnumExt_Mosquito.MosquitoUnlock, killScore: 3));
        }

        public override IEnumerable<CreatureTemplate> GetTemplates()
        {
            // Mosquito template
            CreatureTemplate t = new CreatureFormula(this) {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 0.25f),
                HasAI = true,
                InstantDeathDamage = 1,
                Pathing = PreBakedPathing.From(CreatureType.Fly),
                TileResistances = new() { Air = new(1, Allowed) },
                ConnectionResistances = new() {
                    Standard = new(1, Allowed),
                    OpenDiagonal = new(1, Allowed),
                    ShortCut = new(1, Allowed),
                    NPCTransportation = new(10, Allowed),
                    OffScreenMovement = new(1, Allowed),
                    BetweenRooms = new(1, Allowed),
                },
                DamageResistances = new() { Base = 0.95f, },
                StunResistances = new() { Base = 0.6f, }
            }.IntoTemplate();

            t.abstractedLaziness = 200;
            t.roamBetweenRoomsChance = 0.07f;
            t.offScreenSpeed = 0.1f;
            t.bodySize = 0.5f;
            t.stowFoodInDen = true;
            t.shortcutSegments = 2;
            t.grasps = 1;
            t.visualRadius = 800f;
            t.movementBasedVision = 0.65f;
            t.communityInfluence = 0.1f;
            t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            t.waterPathingResistance = 2f;
            t.canFly = true;
            t.meatPoints = 3;
            t.dangerousToPlayer = 0.4f;

            yield return t;
        }

        public override void EstablishRelationships()
        {
            Relationships mosquito = new(EnumExt_Mosquito.Mosquito);

            foreach (var template in StaticWorld.creatureTemplates) {
                if (template.quantified) {
                    mosquito.Ignores(template.type);
                    mosquito.IgnoredBy(template.type);
                }
            }

            mosquito.IsInPack(EnumExt_Mosquito.Mosquito, 1f);

            mosquito.Eats(CreatureType.Slugcat, 0.4f);
            mosquito.Eats(CreatureType.Scavenger, 0.6f);
            mosquito.Eats(CreatureType.LizardTemplate, 0.3f);
            mosquito.Eats(CreatureType.CicadaA, 0.4f);

            mosquito.Intimidates(CreatureType.LizardTemplate, 0.35f);
            mosquito.Intimidates(CreatureType.CicadaA, 0.3f);

            mosquito.AttackedBy(CreatureType.Slugcat, 0.2f);
            mosquito.AttackedBy(CreatureType.Scavenger, 0.2f);

            mosquito.EatenBy(CreatureType.BigSpider, 0.35f);

            mosquito.Fears(CreatureType.Spider, 0.2f);
            mosquito.Fears(CreatureType.BigSpider, 0.2f);
            mosquito.Fears(CreatureType.SpitterSpider, 0.6f);
        }

        public override ArtificialIntelligence GetRealizedAI(AbstractCreature acrit) => new MosquitoAI(acrit);
        public override Creature GetRealizedCreature(AbstractCreature acrit) => new Mosquito(acrit);
        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return forObject is Mosquito m ? new MosquitoProperties(m) : null;
        }

        sealed class MosquitoProperties : ItemProperties
        {
            private readonly Mosquito mosquito;

            public MosquitoProperties(Mosquito mosquito)
            {
                this.mosquito = mosquito;
            }

            public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
            {
                if (mosquito.State.alive) {
                    grabability = Player.ObjectGrabability.CantGrab;
                } else {
                    grabability = Player.ObjectGrabability.OneHand;
                }
            }

            public override void Meat(Player player, ref bool meat)
            {
                meat = true;
            }
        }
    }
}

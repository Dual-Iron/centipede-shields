using BepInEx;
using CFisobs.Core;
using System.Linq;

namespace CentiShields
{
    [BepInPlugin("org.dual.centishields", nameof(CentiShields), "0.1.0")]
    sealed class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            Content.Register(new CentiShieldFisob());

            // Create centi shields when centipedes lose their shells
            On.Room.AddObject += Room_AddObject;

            // Protect the player from grabs while holding a shield
            On.Creature.Grab += Creature_Grab;
        }

        private void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            if (obj is CentipedeShell shell && shell.scaleX > 0.9f && shell.scaleY > 0.9f && UnityEngine.Random.value < 0.25f) {
                var tilePos = self.GetTilePosition(shell.pos);
                var pos = new WorldCoordinate(self.abstractRoom.index, tilePos.x, tilePos.y, 0);
                var abstr = new CentiShieldAbstract(self.world, pos, self.game.GetNewID()) {
                    hue = shell.hue,
                    saturation = shell.saturation,
                    scaleX = shell.scaleX,
                    scaleY = shell.scaleY
                };
                obj = new CentiShield(abstr, shell.pos, shell.vel);

                self.abstractRoom.AddEntity(abstr);
            }

            orig(self, obj);
        }

        private bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            const float maxDistance = 5;

            if (obj is Player p && !(self is DropBug)) {
                var shieldGrasp = p.grasps.FirstOrDefault(g => g?.grabbed is CentiShield);
                if (shieldGrasp?.grabbed is CentiShield shield && self.bodyChunks.Any(b => (b.pos - shield.firstChunk.pos).magnitude - b.rad - shield.firstChunk.rad < maxDistance)) {
                    shield.AllGraspsLetGoOfThisObject(true);
                    shield.Forbid();
                    shield.HitEffect((shield.firstChunk.pos - self.firstChunk.pos).normalized);

                    shield.AddDamage(0.5f);

                    return false;
                }
            }

            return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        }
    }
}
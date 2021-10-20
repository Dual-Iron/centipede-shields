using BepInEx;
using CFisobs;
using UnityEngine;

namespace CentiShields;

[BepInPlugin("org.dual.centishields", nameof(CentiShields), "0.1.0")]
public sealed class Plugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        static IEnumerable<Fisob> GetFisobs()
        {
            yield return CentiShieldFisob.Instance;
        }

        FisobRegistry reg = new(GetFisobs());

        reg.ApplyHooks();

        // Creating centi shields
        On.Room.Update += Room_Update;
        On.Room.AddObject += Room_AddObject;

        // Grab/bite protection
        On.Creature.Grab += Creature_Grab;
    }

    private void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)) {
            var pos = self.game.Players[0]?.pos;

            if (pos?.room == self.abstractRoom.index) {
                var abstr = new CentiShieldAbstract(self.world, pos.Value, self.game.GetNewID()) {
                    hue = 0.5f,
                    saturation = 1,
                    scaleX = 1,
                    scaleY = 1
                };

                abstr.Spawn();
            }
        }
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

        if (obj is Player p && self is not DropBug) {
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

using BepInEx;
using Fisobs;

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

        On.Room.AddObject += Room_AddObject;
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
        }

        orig(self, obj);
    }
}

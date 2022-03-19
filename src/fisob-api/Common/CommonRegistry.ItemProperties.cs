using CFisobs.Core;

namespace CFisobs.Common
{
    public sealed partial class CommonRegistry : Registry
    {
        private void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);

            foreach (var common in all) {
                common.LoadResources(self);
            }
        }

        private bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.Throwable(self, ref ret);

            return ret;
        }

        private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            Player.ObjectGrabability ret = (Player.ObjectGrabability)orig(self, obj);

            P(obj)?.Grabability(self, ref ret);

            return (int)ret;
        }

        private bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.LethalWeapon(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
        {
            int ret = orig(self, obj, pickupDropInsteadOfWeaponSelection);

            if (pickupDropInsteadOfWeaponSelection)
                P(obj)?.ScavWeaponPickupScore(self.scavenger, ref ret);
            else
                P(obj)?.ScavWeaponUseScore(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (weaponFiltered) return orig(self, obj, true);

            int ret = orig(self, obj, weaponFiltered);

            P(obj)?.ScavCollectScore(self.scavenger, ref ret);

            return ret;
        }
    }
}

using CFisobs.Common;
using CFisobs.Core;
using CFisobs.Items;
using System.Linq;
using UnityEngine;

namespace CentiShields
{
    /// <inheritdoc/>
    public static class EnumExt_CentiShields
    {
        /// <inheritdoc/>
        public static AbstractPhysicalObject.AbstractObjectType CentiShield;
        /// <inheritdoc/>
        public static MultiplayerUnlocks.SandboxUnlockID RedCentiShield;
        /// <inheritdoc/>
        public static MultiplayerUnlocks.SandboxUnlockID OrangeCentiShield;
    }
    
    sealed class CentiShieldFisob : Fisob
    {
        private static readonly CentiShieldProperties properties = new();

        public CentiShieldFisob() : base(EnumExt_CentiShields.CentiShield)
        {
            // If you don't want to manually implement an icon, omit this line.
            // Fisobs would autoload the `icon_CentiShield` embedded resource for you.
            Icon = new CentiShieldIcon();

            RegisterUnlock(new(EnumExt_CentiShields.RedCentiShield, 0));
            RegisterUnlock(new(EnumExt_CentiShields.OrangeCentiShield, 70));
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5) {
                p = new string[5];
            }

            var result = new CentiShieldAbstract(world, saveData.Pos, saveData.ID) {
                hue = float.TryParse(p[0], out var h) ? h : 0,
                saturation = float.TryParse(p[1], out var s) ? s : 1,
                scaleX = float.TryParse(p[2], out var x) ? x : 1,
                scaleY = float.TryParse(p[3], out var y) ? y : 1,
                damage = float.TryParse(p[4], out var r) ? r : 0
            };

            if (unlock != null) {
                result.hue = unlock.Data / 1000f;

                if (unlock.Data == 0) {
                    result.scaleX += 0.2f;
                    result.scaleY += 0.2f;
                }
            }

            return result;
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return properties;
        }
    }

    sealed class CentiShieldIcon : Icon
    {
        // Vanilla only gives you one int field to store all your custom data.
        // In this case, that int field is used to store the shield's hue (scaled by 1000).
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CentiShieldAbstract shield ? (int)(shield.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the embedded resource named `icon_{Type}` automatically
            // For CentiShields, this is `icon_CentiShield`
            return "icon_CentiShield";
        }
    }

    sealed class CentiShieldProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
            => throwable = false;

        public override void ScavCollectScore(Scavenger scavenger, ref int score)
            => score = 3;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            // The player can only grab one centishield at a time,
            // but that shouldn't prevent them from grabbing a spear,
            // so don't use Player.ObjectGrabability.BigOneHand

            if (player.grasps.Any(g => g?.grabbed is CentiShield)) {
                grabability = Player.ObjectGrabability.CantGrab;
            } else {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }
    }
}

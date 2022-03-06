using CFisobs;
using UnityEngine;

namespace CentiShields
{
    sealed class CentiShieldFisob : Fisob
    {
        public static readonly CentiShieldFisob Instance = new CentiShieldFisob();

        private static readonly CentiShieldProperties properties = new CentiShieldProperties();

        private CentiShieldFisob() : base("centipede_shield")
        {
            Icon = new CentiShieldIcon();

            SandboxUnlocks.Add(new SandboxUnlock("centipede_shield_red", 0));
            SandboxUnlocks.Add(new SandboxUnlock("centipede_shield_orange", 70));
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

        public override FisobProperties GetProperties(PhysicalObject forObject)
        {
            return properties;
        }
    }

    sealed class CentiShieldIcon : IFisobIcon
    {
        int IFisobIcon.Data(AbstractPhysicalObject apo)
        {
            return apo is CentiShieldAbstract shield ? (int)(shield.hue * 1000f) : 0;
        }

        Color IFisobIcon.SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }

        string IFisobIcon.SpriteName(int data)
        {
            return "icon_centipede_shield";
        }
    }
}

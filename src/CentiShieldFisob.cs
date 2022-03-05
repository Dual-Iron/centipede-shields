using CFisobs;
using UnityEngine;

namespace CentiShields
{
    sealed class CentiShieldFisob : Fisob
    {
        public static readonly CentiShieldFisob Instance = new CentiShieldFisob();

        private static readonly CentipedeShieldProperties properties = new CentipedeShieldProperties();

        private CentiShieldFisob() : base("centipede_shield")
        {
            Icon = new CentiShieldIcon();

            SandboxUnlocks.Add(new CentiShieldUnlock(this, 0));
            SandboxUnlocks.Add(new CentiShieldUnlock(this, 70));
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
        {
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5) {
                p = new string[5];
            }

            return new CentiShieldAbstract(world, saveData.Pos, saveData.ID) {
                hue = float.TryParse(p[0], out var h) ? h : 0,
                saturation = float.TryParse(p[1], out var s) ? s : 1,
                scaleX = float.TryParse(p[2], out var x) ? x : 1,
                scaleY = float.TryParse(p[3], out var y) ? y : 1,
                damage = float.TryParse(p[4], out var r) ? r : 0
            };
        }

        public override FisobProperties GetProperties(PhysicalObject forObject)
        {
            return properties;
        }
    }

    sealed class CentiShieldUnlock : SandboxUnlock
    {
        public CentiShieldUnlock(Fisob owner, int data) : base(owner, $"centipede_shield_{data}", data)
        {
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
        {
            var ret = base.Parse(world, saveData);
            if (ret is CentiShieldAbstract centiShield) {
                centiShield.hue = Data / 1000f;

                if (Data == 0) {
                    centiShield.scaleX += .2f;
                    centiShield.scaleY += .2f;
                }
            }
            return ret;
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

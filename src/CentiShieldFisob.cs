using CFisobs;

namespace CentiShields
{
    sealed class CentiShieldFisob : Fisob
    {
        public static readonly CentiShieldFisob Instance = new CentiShieldFisob();

        private static readonly CentipedeShieldProperties properties = new CentipedeShieldProperties();

        private CentiShieldFisob() : base("dual_centi_shield")
        {
            IconColor = new UnityEngine.Color(1f, 0.1f, 0.1f);
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

        public override SandboxState GetSandboxState(MultiplayerUnlocks unlocks)
        {
            return SandboxState.Unlocked;
        }

        public override FisobProperties GetProperties(PhysicalObject forObject)
        {
            return properties;
        }
    }
}

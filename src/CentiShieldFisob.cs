using Fisobs;

namespace CentiShields;

public sealed class CentiShieldFisob : Fisob
{
    public static readonly CentiShieldFisob Instance = new();

    private CentiShieldFisob() : base("dual_centi_shield") { }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
    {
        string[] p = saveData.CustomData.Split(';');
        return new CentiShieldAbstract(world, saveData.Pos, saveData.ID) {
            hue = float.Parse(p[0]),
            saturation = float.Parse(p[1]),
            scaleX = float.Parse(p[2]),
            scaleY = float.Parse(p[3])
        };
    }

    public override FisobProperties GetProperties(PhysicalObject forObject)
    {
        return new CentipedeShieldProperties();
    }

    private sealed class CentipedeShieldProperties : FisobProperties
    {
        public override void CanThrow(Player player, ref bool throwable)
            => throwable = false;

        public override void GetScavCollectibleScore(Scavenger scavenger, ref int score)
            => score = 3;

        public override void GetGrabability(Player player, ref Player.ObjectGrabability grabability)
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

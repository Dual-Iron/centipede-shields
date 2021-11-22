using CFisobs;
using System.Linq;

namespace CentiShields
{
    sealed class CentipedeShieldProperties : FisobProperties
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

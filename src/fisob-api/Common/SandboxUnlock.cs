#nullable enable
namespace CFisobs.Common
{
    public class SandboxUnlock
    {
        public int Data { get; }
        public int? KillScore { get; }
        public MultiplayerUnlocks.SandboxUnlockID Type { get; }

        // kill score < 0 means the creature won't show up
        public SandboxUnlock(MultiplayerUnlocks.SandboxUnlockID type, int data = 0, int? killScore = null)
        {
            Type = type;
            Data = data;
            KillScore = killScore;
        }

        public virtual bool IsUnlocked(MultiplayerUnlocks unlocks) => true;
    }
}

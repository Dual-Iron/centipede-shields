using RWCustom;

namespace CFisobs
{
    public class SandboxUnlock
    {
        public SandboxUnlock(string id, int data)
        {
            ID = id;
            Data = data;
        }

        public readonly Fisob Owner;
        public readonly string ID;
        public readonly int Data;

        private MultiplayerUnlocks.SandboxUnlockID? type;

        public MultiplayerUnlocks.SandboxUnlockID Type {
            get {
                if (type == null) {
                    type = Custom.ParseEnum<MultiplayerUnlocks.SandboxUnlockID>(ID);
                }
                return type.Value;
            }
        }

        public virtual bool IsUnlocked(MultiplayerUnlocks unlocks) => true;
    }
}

#nullable enable
using CFisobs.Core;
using System.Collections.Generic;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using System.Linq;

namespace CFisobs.Common
{
    public sealed partial class CommonRegistry : Registry
    {
        public static CommonRegistry Instance { get; } = new CommonRegistry();

        readonly List<ICommon> all = new();
        readonly Dictionary<ObjectType, ICommon> items = new();
        readonly Dictionary<CreatureType, ICommon> crits = new();

        protected internal override void Process(IContent entry)
        {
            if (entry is ICommon common) {
                all.Add(common);
                if (common.Type.MatchL(out var objectType)) {
                    items[objectType] = common;
                } else if (common.Type.MatchR(out var creatureType)) {
                    crits[creatureType] = common;
                }
            }
        }

        protected internal override void Apply()
        {
            // Items
            On.RainWorld.LoadResources += RainWorld_LoadResources;
            On.Player.IsObjectThrowable += Player_IsObjectThrowable;
            On.Player.Grabability += Player_Grabability;
            On.ScavengerAI.RealWeapon += ScavengerAI_RealWeapon;
            On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;

            // Sandbox
            if (all.Any(c => c.SandboxUnlocks.Count > 0)) {
                IL.Menu.SandboxEditorSelector.ctor += AddCustomFisobs;
                On.Menu.SandboxEditorSelector.ctor += ResetWidthAndHeight;
                On.SandboxGameSession.SpawnEntity += SpawnEntity;
                On.MultiplayerUnlocks.SandboxItemUnlocked += IsUnlocked;
            }

            // Sandbox (creatures specifically)
            if (crits.Values.Any(c => c.SandboxUnlocks.Count > 0)) {

            }
        }

        private ItemProperties? P(PhysicalObject po)
        {
            if (po?.abstractPhysicalObject is AbstractPhysicalObject apo) {
                if (items.TryGetValue(apo.type, out ICommon one)) {
                    return one.Properties(po);
                }
                if (apo is AbstractCreature crit && crits.TryGetValue(crit.creatureTemplate.type, out ICommon two)) {
                    return two.Properties(po);
                }
            }
            return null;
        }
    }
}

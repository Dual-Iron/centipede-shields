#nullable enable
using CFisobs.Core;
using System.Collections.Generic;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using System.Linq;
using System;
using UnityEngine;

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
                if (common.Type.ObjectType != 0) {
                    items[common.Type.ObjectType] = common;
                } else if (common.Type.CritType != 0) {
                    crits[common.Type.CritType] = common;
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
            IL.Player.ObjectEaten += Player_ObjectEaten;

            bool sandboxCritobs = crits.Values.Any(c => c.SandboxUnlocks.Count > 0);
            bool sandboxAny = sandboxCritobs || all.Any(c => c.SandboxUnlocks.Count > 0);

            // Sandbox bits that apply to both items and creatures
            if (sandboxAny) {
                IL.Menu.SandboxEditorSelector.ctor += AddCustomFisobs;
                On.Menu.SandboxEditorSelector.ctor += ResetWidthAndHeight;
                On.SandboxGameSession.SpawnEntity += SpawnEntity;
                On.MultiplayerUnlocks.SandboxItemUnlocked += IsUnlocked;
                On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += FromUnlock;
                On.MultiplayerUnlocks.SandboxUnlockForSymbolData += FromSymbolData;
            }

            // Sandbox bits that apply to creatures only
            if (sandboxCritobs) {
                On.Menu.SandboxSettingsInterface.ctor += AddPages;
                On.Menu.SandboxSettingsInterface.DefaultKillScores += DefaultKillScores;
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

using ObjType = AbstractPhysicalObject.AbstractObjectType;
using CritType = CreatureTemplate.Type;
using UnlockType = MultiplayerUnlocks.SandboxUnlockID;
using PastebinMachine.EnumExtender;
using UnityEngine;
using System.Collections.Generic;
using System;
using CFisobs.Creatures;

namespace CFisobs
{
    /// <summary>
    /// Provides methods to register physical object handlers (fisobs) through the <see cref="Fisob"/> type.
    /// </summary>
    /// <remarks>Users should create one instance of this class and pass it around. After creating a new instance, <see cref="ApplyHooks"/> should be called.</remarks>
    public sealed partial class FisobRegistry
    {
        /// <summary>
        /// Creates a new fisob registry and applies its hooks.
        /// </summary>
        /// <remarks>
        /// This is shorthand for the following: <code>new FisobRegistry(fisobs).ApplyHooks()</code>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when a fisob can't be added to the registry.</exception>
        public static void Register(params Fisob[] fisobs)
        {
            try {
                new FisobRegistry(fisobs).ApplyHooks();
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError(e);
                Console.WriteLine(e);
                throw;
            }
        }

        private readonly List<Fisob> allFisobs = new List<Fisob>();
        private readonly Dictionary<ObjType, Fisob> fisobsByType = new Dictionary<ObjType, Fisob>();
        private readonly Dictionary<CritType, Critob> critobsByType = new Dictionary<CritType, Critob>();

        /// <summary>
        /// Creates a new fisob registry from the provided set of <see cref="Fisob"/> instances.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when a fisob can't be added to this registry.</exception>
        public FisobRegistry(IEnumerable<Fisob> fisobs)
        {
            Verify(fisobs);
            RegisterTypes(fisobs);
        }

        private static void Verify(IEnumerable<Fisob> fisobs)
        {
            var objectIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            objectIDs.UnionWith(Enum.GetNames(typeof(ObjType)));
            objectIDs.UnionWith(Enum.GetNames(typeof(CritType)));

            var sandboxIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            sandboxIDs.UnionWith(Enum.GetNames(typeof(UnlockType)));

            foreach (Fisob fisob in fisobs) {
                if (!FisobExtensions.IsValidID(fisob.ID)) {
                    throw new InvalidOperationException($"The fisob ID \"{fisob.ID}\" is invalid. Valid IDs must consist of a-z and _.");
                }

                if (!objectIDs.Add(fisob.ID)) {
                    throw new InvalidOperationException($"The fisob ID \"{fisob.ID}\" is already in use.");
                }

                foreach (SandboxUnlock unlock in fisob.SandboxUnlocks) {
                    if (!FisobExtensions.IsValidID(unlock.ID)) {
                        throw new InvalidOperationException($"The sandbox unlock ID \"{fisob.ID}\" is invalid. Valid IDs must consist of a-z and _.");
                    }

                    if (!sandboxIDs.Add(unlock.ID)) {
                        throw new InvalidOperationException($"The sandbox unlock ID \"{fisob.ID}\" is already in use.");
                    }
                }
            }
        }

        private void RegisterTypes(IEnumerable<Fisob> fisobs)
        {
            foreach (Fisob fisob in fisobs) {
                if (fisob is Critob) {
                    EnumExtender.AddDeclaration(typeof(CritType), fisob.ID);
                } else {
                    EnumExtender.AddDeclaration(typeof(ObjType), fisob.ID);
                }

                foreach (SandboxUnlock unlock in fisob.SandboxUnlocks) {
                    EnumExtender.AddDeclaration(typeof(UnlockType), unlock.ID);
                }
            }

            EnumExtender.ExtendEnumsAgain();

            foreach (Fisob fisob in fisobs) {
                allFisobs.Add(fisob);
                if (fisob is Critob critob) {
                    critobsByType[critob.Type] = critob;
                } else {
                    fisobsByType[fisob.Type] = fisob;
                }
            }
        }

        /// <summary>
        /// Gets a fisob from an object type.
        /// </summary>
        /// <param name="type">The type of the fisob.</param>
        /// <param name="fisob">If it exists, the fisob; otherwise, <see langword="null"/>.</param>
        /// <returns>If the fisob exists, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGet(ObjType type, out Fisob fisob)
        {
            return fisobsByType.TryGetValue(type, out fisob);
        }

        /// <summary>
        /// Gets a critob from a creature type.
        /// </summary>
        /// <param name="type">The type of the critob.</param>
        /// <param name="critob">If it exists, the critob; otherwise, <see langword="null"/>.</param>
        /// <returns>If the critob exists, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGet(CritType type, out Critob critob)
        {
            return critobsByType.TryGetValue(type, out critob);
        }

        /// <summary>
        /// Applies hooks that enable fisob behavior.
        /// </summary>
        public void ApplyHooks()
        {
            ApplySandbox();
            ApplyIcons();

            On.RainWorld.LoadResources += RainWorld_LoadResources;
            On.Player.IsObjectThrowable += Player_IsObjectThrowable;
            On.Player.Grabability += Player_Grabability;
            On.ScavengerAI.RealWeapon += ScavengerAI_RealWeapon;
            On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
        }

        private FisobProperties P(PhysicalObject po)
        {
            if (po is object) {
                if (TryGet(po.abstractPhysicalObject.type, out Fisob f)) {
                    return f.GetProperties(po);
                }
                if (po.abstractPhysicalObject is AbstractCreature crit && TryGet(crit.creatureTemplate.type, out Critob c)) {
                    return c.GetProperties(po);
                }
            }
            return null;
        }

        private void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);

            foreach (var fisob in allFisobs) {
                fisob.LoadResources(self);
            }
        }

        private bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.CanThrow(self, ref ret);

            return ret;
        }

        private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            Player.ObjectGrabability ret = (Player.ObjectGrabability)orig(self, obj);

            P(obj)?.GetGrabability(self, ref ret);

            return (int)ret;
        }

        private bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.IsLethalWeapon(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
        {
            int ret = orig(self, obj, pickupDropInsteadOfWeaponSelection);

            if (pickupDropInsteadOfWeaponSelection)
                P(obj)?.GetScavWeaponPickupScore(self.scavenger, ref ret);
            else
                P(obj)?.GetScavWeaponUseScore(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (weaponFiltered) return orig(self, obj, true);

            int ret = orig(self, obj, weaponFiltered);

            P(obj)?.GetScavCollectibleScore(self.scavenger, ref ret);

            return ret;
        }

        private AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
            ObjType type = RWCustom.Custom.ParseEnum<ObjType>(array[1]);

            if (TryGet(type, out Fisob o) && array.Length > 2) {
                EntityID id = EntityID.FromString(array[0]);

                string[] coordParts = array[2].Split('.');

                WorldCoordinate coord;

                if (int.TryParse(coordParts[0], out int room) &&
                    int.TryParse(coordParts[1], out int x) &&
                    int.TryParse(coordParts[2], out int y) &&
                    int.TryParse(coordParts[3], out int node)) {
                    coord = new WorldCoordinate(room, x, y, node);
                } else {
                    Debug.LogError($"{nameof(CFisobs)} : Corrupt world coordinate on object \"{id}\", type \"{o.ID}\"");
                    return null;
                }

                string customData = array.Length == 4 ? array[3] : "";

                if (array.Length > 4) {
                    Debug.LogError($"{nameof(CFisobs)} : Save data had more than 4 <oA> sections in \"{o.ID}\". Override `APO.ToString()` to return `this.SaveAsString(...)`.");
                    return null;
                }

                try {
                    return o.Parse(world, new EntitySaveData(o.Type, 0, id, coord, customData), null);
                } catch (Exception e) {
                    Debug.LogError($"{nameof(CFisobs)} : {e}");
                    return null;
                }
            }

            return orig(world, objString);
        }
    }
}
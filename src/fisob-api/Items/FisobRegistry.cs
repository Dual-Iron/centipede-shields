#nullable enable
using CFisobs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace CFisobs.Items
{
    public sealed class FisobRegistry : Registry
    {
        public static FisobRegistry Instance { get; } = new FisobRegistry();

        readonly HashSet<string> physobStrings = new HashSet<string>();
        readonly Dictionary<ObjectType, Fisob> fisobs = new Dictionary<ObjectType, Fisob>();

        FisobRegistry()
        {
            string[] names = Enum.GetNames(typeof(ObjectType));
            string[] namesCulled = new string[31];
            Array.Copy(names, namesCulled, 31);
            physobStrings = new HashSet<string>(namesCulled, StringComparer.OrdinalIgnoreCase);
        }

        protected internal override void Process(IContent entry)
        {
            if (entry is Fisob fisob) {
                if (!physobStrings.Add(fisob.Type.ToString())) {
                    throw RegisterException.DuplicateID(fisob.Type.ToString());
                }
                if (!Content.IsValidID(fisob.Type.ToString())) {
                    throw RegisterException.InvalidID(fisob.Type.ToString());
                }
                fisobs[fisob.Type] = fisob;
            }
        }

        protected internal override void Apply()
        {
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
        }

        private IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (fisobs.TryGetValue(item.type, out var fisob)) {
                return new IconSymbol.IconSymbolData(0, item.type, fisob.Icon.Data(item));
            }
            return orig(item);
        }

        private Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, ObjectType itemType, int intData)
        {
            if (fisobs.TryGetValue(itemType, out var fisob)) {
                return fisob.Icon.SpriteColor(intData);
            }
            return orig(itemType, intData);
        }

        private string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, ObjectType itemType, int intData)
        {
            if (fisobs.TryGetValue(itemType, out var fisob)) {
                return fisob.Icon.SpriteName(intData);
            }
            return orig(itemType, intData);
        }

        private AbstractPhysicalObject? SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            var data = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
            var type = RWCustom.Custom.ParseEnum<ObjectType>(data[1]);

            if (fisobs.TryGetValue(type, out Fisob o) && data.Length > 2) {
                EntityID id = EntityID.FromString(data[0]);

                string[] coordParts = data[2].Split('.');

                WorldCoordinate coord;

                if (int.TryParse(coordParts[0], out int room) &&
                    int.TryParse(coordParts[1], out int x) &&
                    int.TryParse(coordParts[2], out int y) &&
                    int.TryParse(coordParts[3], out int node)) {
                    coord = new WorldCoordinate(room, x, y, node);
                } else {
                    Debug.LogError($"Corrupt world coordinate on object \"{id}\", type \"{o.Type}\".");
                    return null;
                }

                string customData = data.Length == 4 ? data[3] : "";

                if (data.Length > 4) {
                    Debug.LogError($"Save data had more than 4 <oA> sections in fisob \"{o.Type}\". Override `APO.ToString()` to return `this.SaveAsString(...)`.");
                    return null;
                }

                try {
                    return o.Parse(world, new EntitySaveData(o.Type, id, coord, customData), null);
                } catch (Exception e) {
                    Debug.LogException(e);
                    Debug.LogError($"An exception was thrown in {o.GetType().FullName}::Parse: {e.Message}");
                    return null;
                }
            }

            return orig(world, objString);
        }
    }
}

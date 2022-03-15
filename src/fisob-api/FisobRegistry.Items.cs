using ObjType = AbstractPhysicalObject.AbstractObjectType;
using UnityEngine;
using System;

namespace CFisobs
{
    public sealed partial class FisobRegistry
    {
        void ApplyItems()
        {
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;

            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
        }

        private IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (TryGet(item.type, out var fisob)) {
                return new IconSymbol.IconSymbolData(0, item.type, fisob.Icon.Data(item));
            }
            return orig(item);
        }

        private Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, ObjType itemType, int intData)
        {
            if (TryGet(itemType, out var fisob)) {
                return fisob.Icon.SpriteColor(intData);
            }
            return orig(itemType, intData);
        }

        private string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, ObjType itemType, int intData)
        {
            if (TryGet(itemType, out var fisob)) {
                return fisob.Icon.SpriteName(intData);
            }
            return orig(itemType, intData);
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
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using UnityEngine;

namespace CFisobs
{
    public sealed partial class FisobRegistry
    {
        void ApplyIcons()
        {
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
        }

        void UndoIcons()
        {
            On.ItemSymbol.SymbolDataFromItem -= ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.ColorForItem -= ItemSymbol_ColorForItem;
            On.ItemSymbol.SpriteNameForItem -= ItemSymbol_SpriteNameForItem;
        }

        private IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (fisobsByType.TryGetValue(item.type, out var fisob)) {
                return new IconSymbol.IconSymbolData(0, item.type, fisob.Icon.Data(item));
            }
            return orig(item);
        }

        private Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, ObjType itemType, int intData)
        {
            if (fisobsByType.TryGetValue(itemType, out var fisob)) {
                return fisob.Icon.SpriteColor(intData);
            }
            return orig(itemType, intData);
        }

        private string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, ObjType itemType, int intData)
        {
            if (fisobsByType.TryGetValue(itemType, out var fisob)) {
                return fisob.Icon.SpriteName(intData);
            }
            return orig(itemType, intData);
        }
    }
}
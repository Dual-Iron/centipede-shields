using ObjType = AbstractPhysicalObject.AbstractObjectType;
using UnityEngine;

namespace CFisobs
{
    public sealed partial class FisobRegistry
    {
        void ApplyIcons()
        {
            On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
            On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;

            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
        }

        private IconSymbol.IconSymbolData CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
        {
            if (TryGet(creature.creatureTemplate.type, out var critob)) {
                return new IconSymbol.IconSymbolData(creature.creatureTemplate.type, ObjType.Creature, critob.Icon.Data(creature));
            }
            return orig(creature);
        }

        private Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (TryGet(iconData.critType, out var critob)) {
                return critob.Icon.SpriteColor(iconData.intData);
            }
            return orig(iconData);
        }

        private string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (TryGet(iconData.critType, out var critob)) {
                return critob.Icon.SpriteName(iconData.intData);
            }
            return orig(iconData);
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
    }
}
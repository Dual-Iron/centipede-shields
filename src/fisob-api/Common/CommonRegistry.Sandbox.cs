﻿#nullable enable
using static Menu.SandboxEditorSelector;
using UnityEngine;
using System;
using ArenaBehaviors;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Menu;
using System.Linq;
using CFisobs.Creatures;
using CFisobs.Core;
using CFisobs.Items;

namespace CFisobs.Common
{
    public sealed partial class CommonRegistry : Registry
    {
        #region Hooking sandbox select menu
        private void AddCustomFisobs(ILContext il)
        {
            ILCursor cursor = new(il);

            try {
                // Move before creatures are added
                cursor.GotoNext(MoveType.Before, i => i.MatchLdcI4(0) && i.Next.MatchStloc(3));

                // Call `InsertPhysicalObjects` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
                cursor.EmitDelegate(InsertPhysicalObjects);

                // Move after creatures are added, before play button is added
                cursor.GotoNext(MoveType.Before, i => i.MatchLdarg(0) && i.Next.MatchLdcI4(1));

                // Call `InsertCreatures` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
                cursor.EmitDelegate(InsertCreatures);
            } catch (Exception e) {
                Debug.LogException(e);
                Console.WriteLine($"{nameof(CFisobs)} : Couldn't register fisobs because of exception in {nameof(AddCustomFisobs)}: {e.Message}");
            }
        }

        // Must be static to work around a weird Realm bug (see https://github.com/MonoMod/MonoMod/issues/85)
        private static void InsertPhysicalObjects(SandboxEditorSelector self, ref int counter) => Instance?.InsertEntries(false, self, ref counter);
        private static void InsertCreatures(SandboxEditorSelector self, ref int counter) => Instance?.InsertEntries(true, self, ref counter);

        private void InsertEntries(bool creatures, SandboxEditorSelector self, ref int counter)
        {
            foreach (var common in all) {
                if (creatures && common is not Critob || !creatures && common is not Fisob) {
                    continue;
                }

                foreach (var unlock in common.SandboxUnlocks) {
                    // Reserve slots for:
                    int padding = creatures
                        ? 8     // empty space (3) + randomize button (1) + config buttons (3) + play button (1)
                        : 51    // all of the above (8) + creature unlocks (43)
                        ;

                    if (counter >= Width * Height - padding) {
                        GrowEditorSelector(self);
                    }

                    Button button;
                    if (self.unlocks.SandboxItemUnlocked(unlock.Type)) {
                        button = new CreatureOrItemButton(self.menu, self, new IconSymbol.IconSymbolData(common.Type.RightOr(0), common.Type.LeftOr(0), unlock.Data));
                    } else {
                        button = new LockedButton(self.menu, self);
                    }
                    self.AddButton(button, ref counter);
                }
            }
        }

        private static void GrowEditorSelector(SandboxEditorSelector self)
        {
            self.bkgRect.size.y += ButtonSize;
            self.size.y += ButtonSize;
            self.pos.y += ButtonSize;
            Height += 1;

            Button[,] newArr = new Button[Width, Height];

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height - 1; j++) {
                    newArr[i, j + 1] = self.buttons[i, j];
                }
            }

            self.buttons = newArr;
        }
        #endregion

        private void ResetWidthAndHeight(On.Menu.SandboxEditorSelector.orig_ctor orig, SandboxEditorSelector self, Menu.Menu menu, Menu.MenuObject owner, SandboxOverlayOwner overlayOwner)
        {
            Width = 19;
            Height = 4;
            orig(self, menu, owner, overlayOwner);
        }

        private bool IsUnlocked(On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            foreach (var common in all) {
                var unlock = common.SandboxUnlocks.FirstOrDefault(s => s.Type == unlockID);
                if (unlock != null) {
                    return unlock.IsUnlocked(self);
                }
            }
            return orig(self, unlockID);
        }

        private void SpawnEntity(On.SandboxGameSession.orig_SpawnEntity orig, SandboxGameSession self, SandboxEditor.PlacedIconData p)
        {
            EntityID id = self.GameTypeSetup.saveCreatures ? p.ID : self.game.GetNewID();
            WorldCoordinate coord = new(0, Mathf.RoundToInt(p.pos.x / 20f), Mathf.RoundToInt(p.pos.y / 20f), -1);

            if (crits.TryGetValue(p.data.critType, out var critob)) {
                EntitySaveData? data = null;

                if (self.GameTypeSetup.saveCreatures) {
                    var creature = self.arenaSitting.creatures.FirstOrDefault(c => c.creatureTemplate.type == p.data.critType && c.ID == p.ID);
                    if (creature != null) {
                        self.arenaSitting.creatures.Remove(creature);

                        for (int i = 0; i < 2; i++) {
                            creature.state.CycleTick();
                        }

                        data = EntitySaveData.CreateFrom(creature, creature.state.ToString());
                    }
                }

                DoSpawn(self, p, data ?? new(p.data.critType, id, coord, ""), critob);

            } else if (items.TryGetValue(p.data.itemType, out var fisob)) {
                EntitySaveData data = new(p.data.itemType, id, coord, "");

                DoSpawn(self, p, data, fisob);

            } else {
                orig(self, p);
            }
        }

        private static void DoSpawn(SandboxGameSession self, SandboxEditor.PlacedIconData p, EntitySaveData data, ICommon common)
        {
            SandboxUnlock? unlock = common.SandboxUnlocks.FirstOrDefault(u => u.Data == p.data.intData);

            if (unlock == null) {
                Debug.LogError($"The fisob \"{common.Type}\" had no sandbox unlocks where Data={p.data.intData}.");
                return;
            }

            try {
                var entity = common.ParseFromSandbox(self.game.world, data, unlock);
                if (entity != null) {
                    self.game.world.GetAbstractRoom(0).AddEntity(entity);
                } else {
                    Debug.LogError($"The sandbox unlock \"{unlock.Type}\" returned null when being parsed in sandbox mode.");
                }
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"The sandbox unlock \"{unlock.Type}\" threw an exception when being parsed in sandbox mode.");
            }
        }
    }
}

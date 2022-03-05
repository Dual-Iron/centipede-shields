using static Menu.SandboxEditorSelector;
using UnityEngine;
using System;
using ArenaBehaviors;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Menu;
using System.Linq;

namespace CFisobs
{
    public sealed partial class FisobRegistry
    {
        void ApplySandbox()
        {
            IL.Menu.SandboxEditorSelector.ctor += AddCustomFisobs;
            On.Menu.SandboxEditorSelector.ctor += SandboxEditorSelector_ctor;
            On.SandboxGameSession.SpawnEntity += SandboxGameSession_SpawnEntity;
        }

        void UndoSandbox()
        {
            IL.Menu.SandboxEditorSelector.ctor -= AddCustomFisobs;
            On.Menu.SandboxEditorSelector.ctor -= SandboxEditorSelector_ctor;
            On.SandboxGameSession.SpawnEntity -= SandboxGameSession_SpawnEntity;
        }

        #region Hooking sandbox select menu
        delegate void InsertSandboxEditor(SandboxEditorSelector self, ref int counter);

        private void AddCustomFisobs(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            try {
                _this = new WeakReference(this);

                // Move before creatures are added
                cursor.GotoNext(MoveType.Before, i => i.MatchLdcI4(0) && i.Next.MatchStloc(3));

                // Call `InsertPhysicalObjects` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
                cursor.EmitDelegate<InsertSandboxEditor>(InsertPhysicalObjects);

                // Move after creatures are added, before play button is added
                cursor.GotoNext(MoveType.Before, i => i.MatchLdarg(0) && i.Next.MatchLdcI4(1));

                // Call `InsertCreatures` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
                cursor.EmitDelegate<InsertSandboxEditor>(InsertCreatures);
            } catch (Exception e) {
                Debug.LogException(e);
                Console.WriteLine($"{nameof(CFisobs)} : Couldn't register fisobs because of exception in {nameof(AddCustomFisobs)}: {e.Message}");
            }
        }

        // Use WeakReference to prevent memleak on unloaded mod
        private static WeakReference _this;
        private static FisobRegistry This => _this.Target as FisobRegistry;

        // Must be static to work around a weird Realm bug (see https://github.com/MonoMod/MonoMod/issues/85)
        private static void InsertPhysicalObjects(SandboxEditorSelector self, ref int counter) => This?.InsertFisobs(false, self, ref counter);
        private static void InsertCreatures(SandboxEditorSelector self, ref int counter) => This?.InsertFisobs(true, self, ref counter);

        private void InsertFisobs(bool creatures, SandboxEditorSelector self, ref int counter)
        {
            foreach (Fisob fisob in fisobsByType.Values) {
                //if (creatures != fisob is Critob) {
                //    continue;
                //}
                // TODO critobs

                foreach (var unlock in fisob.SandboxUnlocks) {
                    // Reserve slots for:
                    int padding = creatures
                        ? 8     // empty space (3) + randomize button (1) + config buttons (3) + play button (1)
                        : 51    // all of the above (8) + creature unlocks (43)
                        ;

                    if (counter >= Width * Height - padding) {
                        GrowEditorSelector(self);
                    }

                    Button button;
                    if (unlock.IsUnlocked(self.unlocks)) {
                        button = new CreatureOrItemButton(self.menu, self, new IconSymbol.IconSymbolData(0, fisob.Type, unlock.Data));
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

        private void SandboxEditorSelector_ctor(On.Menu.SandboxEditorSelector.orig_ctor orig, SandboxEditorSelector self, Menu.Menu menu, Menu.MenuObject owner, SandboxOverlayOwner overlayOwner)
        {
            Width = 19;
            Height = 4;
            orig(self, menu, owner, overlayOwner);
        }
        #endregion

        private void SandboxGameSession_SpawnEntity(On.SandboxGameSession.orig_SpawnEntity orig, SandboxGameSession self, SandboxEditor.PlacedIconData p)
        {
            if (fisobsByType.TryGetValue(p.data.itemType, out var fisob)) {
                WorldCoordinate coord = new WorldCoordinate(0, Mathf.RoundToInt(p.pos.x / 20f), Mathf.RoundToInt(p.pos.y / 20f), -1);
                EntitySaveData data = new EntitySaveData(p.data.itemType, p.ID, coord, "");
                SandboxUnlock unlock = fisob.SandboxUnlocks.FirstOrDefault(u => u.Data == p.data.intData);

                if (unlock == null) {
                    Debug.LogError($"The fisob \"{fisob.ID}\" had no sandbox unlocks where Data={p.data.intData}.");
                    return;
                }

                try {
                    var entity = unlock.Parse(self.game.world, data);
                    if (entity != null) {
                        self.game.world.GetAbstractRoom(0).AddEntity(entity);
                    } else {
                        Debug.LogError($"The sandbox unlock \"{unlock.ID}\" returned null when being parsed in sandbox mode.");
                    }
                } catch (Exception e) {
                    Debug.LogException(e);
                    Debug.LogError($"The sandbox unlock \"{unlock.ID}\" threw an exception when being parsed in sandbox mode.");
                }
            } else {
                orig(self, p);
            }
        }
    }
}
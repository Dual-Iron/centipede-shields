#nullable enable
using CFisobs.Core;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Menu.SandboxSettingsInterface;

namespace CFisobs.Common
{
    public sealed partial class CommonRegistry : Registry
    {
        // Per-controller offsets. pos = offset * index
        const float xOffset = 88.666f + 0.01f;
        const float yOffset = -30f;

        // Cross-fisobs compatibility
        const int paginatorVersion = 0;
        const string paginatorKey = "paginator";
        const int paginatorKeyLength = 9; // paginatorKey.Length

        struct ScoreElement
        {
            public int I;
            public ScoreController C;
        }

        sealed class PageButton : SymbolButton
        {
            public readonly int dir;

            public PageButton(MenuObject owner, int dir, Vector2 pos) : base(owner.menu, owner, "Menu_Symbol_Arrow", "", pos)
            {
                this.dir = dir;
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                symbolSprite.rotation = dir * 90 + 90;
            }
        }

        sealed class Paginator : PositionedMenuObject
        {
            // VANILLA NOTES
            // There are 36 slots to display entries. 35 of them have ScoreControllers occupying them.
            // 32 of them [0..32)  are page entries. They may move.
            //  1 of them (32..32) is NULL. Exclude it from count.
            //  3 of them [32..36) are food, survival, and spearhit scores. They should never move.

            new private readonly SandboxSettingsInterface owner;
            private readonly PageButton left;
            private readonly PageButton right;

            const int RowMin = 0;
            int RowMax => Mathf.CeilToInt((owner.scoreControllers.Count + 1) / 4f) - 9;

            int rowOffset;
            float rowSmoothed;

            public Paginator(SandboxSettingsInterface owner, Vector2 pos) : base(owner.menu, owner, pos)
            {
                this.owner = owner;

                float xOffset = 88.666f + 0.01f;

                subObjects.Add(left = new PageButton(this, -1, new(xOffset * 1f, -280f)));
                subObjects.Add(right = new PageButton(this, 1, new(xOffset * 2f, -280f)));
            }

            public override string ToString() => $"{paginatorKey}{paginatorVersion}";

            public override void Update()
            {
                left.GetButtonBehavior.greyedOut = rowOffset == RowMin;
                right.GetButtonBehavior.greyedOut = rowOffset == RowMax;

                rowSmoothed = Custom.LerpAndTick(rowSmoothed, rowOffset, 1f / 10f, 1f / 40f);

                base.Update();
            }

            public override void Singal(MenuObject sender, string message)
            {
                if (sender is PageButton pageButton && rowOffset + pageButton.dir >= RowMin && rowOffset + pageButton.dir <= RowMax) {
                    IncrementPage(pageButton.dir);
                }
            }

            void IncrementPage(int dir)
            {
                rowOffset += dir;
            }

            public override void GrafUpdate(float timeStacker)
            {
                int slots = owner.scoreControllers.Count + 1;
                int columns = 4;
                int rows = Mathf.CeilToInt(slots / (float)columns); // vanilla has 9

                for (int index = 0; index < owner.scoreControllers.Count; index++) {
                    // Skip static slots (null, Food, Survive, and Spear hit)
                    if (index > 31 && index < 35) {
                        continue;
                    }

                    int i = index < 35 ? index : index - 3; // account for skipped slots
                    int x = i / rows;
                    int y = i % rows;

                    ScoreController score = owner.scoreControllers[index];
                    score.pos.x = x * xOffset;
                    score.pos.y = y * yOffset;

                    if (x < 3) {
                        score.pos.y -= rowSmoothed * yOffset;

                        float offsetToRow0 = 0 - (y - rowSmoothed);
                        float offsetToRow8 = 8 - (y - rowSmoothed);
                        float fade = 1 - Mathf.Max(offsetToRow0, -offsetToRow8);

                        SetAlpha(score, Mathf.Pow(Mathf.Clamp01(fade), 2));
                    }
                }

                base.GrafUpdate(timeStacker);
            }

            void SetAlpha(ScoreController score, float alpha)
            {
                if (score?.scoreDragger == null) return;

                score.scoreDragger.buttonBehav.greyedOut = alpha < 0.9f;
                score.scoreDragger.label.label.alpha = alpha;
                foreach (var sprite in score.scoreDragger.roundedRect.sprites) {
                    sprite.alpha = alpha;
                }

                if (score is LockedScore locked) {
                    if (locked.shadowSprite1 != null) locked.shadowSprite1.alpha = alpha;
                    if (locked.shadowSprite2 != null) locked.shadowSprite2.alpha = alpha;
                    if (locked.symbolSprite != null) locked.symbolSprite.alpha = alpha;
                } else if (score is KillScore kill && kill.symbol != null) {
                    if (kill.symbol.shadowSprite1 != null) kill.symbol.shadowSprite1.alpha = alpha;
                    if (kill.symbol.shadowSprite2 != null) kill.symbol.shadowSprite2.alpha = alpha;
                    if (kill.symbol.symbolSprite != null) kill.symbol.symbolSprite.alpha = alpha;
                }
            }
        }
    }
}

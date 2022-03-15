using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using CFisobs.Creatures;

namespace CFisobs
{
    public sealed partial class FisobRegistry
    {
        void ApplyCreatures()
        {
            RegisterCustomCreatures();

            On.Player.Grabbed += PlayerGrabbed;
            On.AbstractCreature.Realize += Realize;
            On.AbstractCreature.InitiateAI += InitiateAI;
            On.AbstractCreature.ctor += Ctor;
            On.CreatureSymbol.DoesCreatureEarnATrophy += KillsMatter;

            On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
            On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;
        }

        private void RegisterCustomCreatures()
        {
            var types = (CreatureTemplate.Type[])Enum.GetValues(typeof(CreatureTemplate.Type));
            var oldTemplatesCount = StaticWorld.creatureTemplates.Length;
            var newTemplates = new List<CreatureTemplate>(types.Length - oldTemplatesCount);

            // Get new critob templates
            foreach (Critob critob in critobsByType.Values) {
                var templates = critob.GetTemplates()?.ToList() ?? throw new InvalidOperationException($"Critob \"{critob.ID}\" returned null in GetTemplates().");

                if (!templates.Any(t => t.type == critob.Type)) {
                    throw new InvalidOperationException($"Critob \"{critob.ID}\" does not have a template for its type, \"CreatureTemplate.Type::{critob.Type}\".");
                }
                if (templates.FirstOrDefault(t => t.TopAncestor().type != critob.Type) is CreatureTemplate offender) {
                    throw new InvalidOperationException($"The template with type \"{offender.type}\" from critob \"{critob.ID}\" must have an ancestor of type \"CreatureTemplate.Type::{critob.Type}\".");
                }

                newTemplates.AddRange(templates);

                foreach (var template in newTemplates) {
                    critob.AddChildType(template.type);
                }
            }

            // Add new critob templates to StaticWorld.creatureTemplates
            Array.Resize(ref StaticWorld.creatureTemplates, oldTemplatesCount + newTemplates.Count);

            foreach (CreatureTemplate extraTemplate in newTemplates) {
                // Make sure we're not overwriting vanilla or causing index-out-of-bound errors
                if ((int)extraTemplate.type < 46) {
                    throw new InvalidOperationException($"The CreatureTemplate.Type value {extraTemplate.type} ({(int)extraTemplate.type}) must be greater than 45 to not overwrite vanilla.");
                }
                if ((int)extraTemplate.type >= StaticWorld.creatureTemplates.Length) {
                    throw new InvalidOperationException(
                        $"The CreatureTemplate.Type value {extraTemplate.type} ({(int)extraTemplate.type}) must be less than StaticWorld.creatureTemplates.Length ({StaticWorld.creatureTemplates.Length}).");
                }
                StaticWorld.creatureTemplates[(int)extraTemplate.type] = extraTemplate;
            }

            // Avoid null refs at all costs here
            int nullIndex = StaticWorld.creatureTemplates.IndexOf(null);
            if (nullIndex != -1) {
                throw new InvalidOperationException($"StaticWorld.creatureTemplates has a null value at index {nullIndex}.");
            }

            // Add default relationship to existing creatures
            foreach (CreatureTemplate template in StaticWorld.creatureTemplates) {
                int oldRelationshipsLength = template.relationships.Length;

                Array.Resize(ref template.relationships, StaticWorld.creatureTemplates.Length);

                for (int i = oldRelationshipsLength; i < StaticWorld.creatureTemplates.Length; i++) {
                    template.relationships[i] = template.relationships[0];
                }
            }

            foreach (Critob critob in critobsByType.Values) {
                critob.EstablishRelationships();
            }
        }

        private void PlayerGrabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);

            if (grasp?.grabber?.abstractCreature != null && TryGet(grasp.grabber.abstractCreature.creatureTemplate.TopAncestor().type, out var critob) && critob.GraspParalyzesPlayer(grasp)) {
                self.dangerGraspTime = 0;
                self.dangerGrasp = grasp;
            }
        }

        private void InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
        {
            orig(self);

            if (TryGet(self.creatureTemplate.TopAncestor().type, out var crit)) {
                if (self.abstractAI != null && self.creatureTemplate.AI) {
                    self.abstractAI.RealAI = crit.GetRealizedAI(self)
                        ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedAI returned null but template.AI was true!");
                }
            }
        }

        private void Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            if (self.realizedCreature == null && TryGet(self.creatureTemplate.TopAncestor().type, out var crit)) {
                self.realizedObject = crit.GetRealizedCreature(self)
                    ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedCreature returned null!");

                self.InitiateAI();

                foreach (var stuck in self.stuckObjects) {
                    if (stuck.A.realizedObject == null) {
                        stuck.A.Realize();
                    }
                    if (stuck.B.realizedObject == null) {
                        stuck.B.Realize();
                    }
                }
            }

            orig(self);
        }

        private void Ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate template, Creature real, WorldCoordinate pos, EntityID id)
        {
            orig(self, world, template, real, pos, id);

            if (TryGet(template.TopAncestor().type, out var crit)) {
                // Set creature state
                self.state = crit.GetState(self) ?? new HealthState(self);

                // Set creature AI
                AbstractCreatureAI abstractAI = crit.GetAbstractAI(self, world);

                if (template.AI) {
                    self.abstractAI = abstractAI ?? new AbstractCreatureAI(world, self);

                    bool setDenPos = pos.abstractNode > -1 && pos.abstractNode < self.Room.nodes.Length
                        && self.Room.nodes[pos.abstractNode].type == AbstractRoomNode.Type.Den && !pos.TileDefined;

                    if (setDenPos) {
                        abstractAI.denPosition = pos;
                    }
                } else if (abstractAI is object) {
                    Debug.LogError($"{crit.GetType()}::GetAbstractAI returned a non-null object but template.AI was false!");
                }

                // Arbitrary setup
                crit.Init(self, world, pos, id);
            }
        }

        private bool KillsMatter(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureTemplate.Type creature)
        {
            var ret = orig(creature);
            if (TryGet(StaticWorld.GetCreatureTemplate(creature).TopAncestor().type, out var critob)) {
                critob.KillsMatter(creature, ref ret);
            }
            return ret;
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
    }
}
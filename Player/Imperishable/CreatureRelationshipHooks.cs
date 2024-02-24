using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using MoreSlugcats;
using System.Runtime.CompilerServices;
using SlugBase.SaveData;
using System.Collections.Generic;
using SunriseIdyll;

namespace SunriseIdyll
{
    public static class CreatureRelationshipHooks
    {
        public static CreatureTemplate.Relationship.Type newRelation = CreatureTemplate.Relationship.Type.Afraid;
        public static float intensity = 1f;
        public static bool Condition(Creature? crit)
        {
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (crit != null && crit.Template.type == CreatureTemplate.Type.Slugcat && crit is Player player && player.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 6)

                return true;

            else

                return false;
        }
        public static void ApplyHooks()
        {
            On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.MirosBirdAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.MoreSlugcats.StowawayBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };
        }
    }
}

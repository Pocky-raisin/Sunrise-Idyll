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
    public static class CreatureRelationshipHooks //credit to Bro for original code
    {
        public static CreatureTemplate.Relationship.Type newRelation = CreatureTemplate.Relationship.Type.Afraid;
        public static float intensity = 1f;
        public static bool Condition(Creature? crit)
        {
            if (crit != null && crit.Template.type == CreatureTemplate.Type.Slugcat && crit is Player player && player.slugcatStats.name == ImperishableHooks.ImperishableName && player.KarmaCap >= 7)

                return true;

            else
            {
                return false;
            }
        }
        public static void ApplyHooks()
        {

            On.ArtificialIntelligence.DynamicRelationship_CreatureRepresentation_AbstractCreature += (orig, self, rep, absCrit) => {
                Creature? trackedCreature = null;   // make sure that no null values are returned, and set the trackedCreature appropriatly
                if (rep != null)
                {
                    trackedCreature = rep.representedCreature?.realizedCreature;
                }
                else if (absCrit != null)
                {
                    trackedCreature = absCrit.realizedCreature;
                }
                if (Condition(trackedCreature))
                {    // if a creature tries to access it's dynamic relationship, return a new one instead if consitions are met
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, rep, absCrit);
            };

            On.ArtificialIntelligence.StaticRelationship += (orig, self, otherCreature) => {
                if (Condition(otherCreature.realizedCreature))
                {    // if a creature skips calling DynamicRelationship in favor of using a Static one, catch it here and make sure to still return a new relationship
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, otherCreature);
            };


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

            On.CicadaAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
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

            On.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
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

            On.TempleGuardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
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

            On.MoreSlugcats.InspectorAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
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

            On.MoreSlugcats.YeekAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
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

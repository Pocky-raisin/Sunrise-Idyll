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
    public static class WorldStateHooks
    {
        public static void ApplyHooks()
        {
            
            On.DeathPersistentSaveData.ctor += doInitialSaveStuff;
            On.SlugcatStats.SpearSpawnModifier += extraSpears;
            On.Creature.HypothermiaUpdate += hypothermiaModify;
            On.SlugcatStats.SpearSpawnExplosiveRandomChance += explosiveSpearsNaturalSpawn;
        }

        public static DeathPersistentSaveData data1 = new(ImperishableHooks.ImperishableName);

        public static void doInitialSaveStuff(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcatName) //sets up several savestate flags
        {
            try
            {
                var result = ImperishableHooks.imperishableSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val1);
                if (!val1)
                {
                    ImperishableHooks.imperishableSaveData.Set<int>("minKarma", 9);
                    ImperishableHooks.imperishableSaveData.Set<int>("maxKarma", 9);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK9Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK8Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK7Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK6Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK5Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK4Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK3Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK2Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didK1Miracle", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("alreadyDidSaveSetup", true);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + "WorldStateHooks Error 1");
            }
            try {
                var result = ChandlerHooks.chandlerSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val2);
                if (!val2)
                {
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK4Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK5Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK6Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK7Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK8Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didK9Miracle", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("alreadyDidSaveSetup", true);
                }
            }
            catch (Exception e2)
            {
                Debug.Log(e2 + "WorldStateHooks Error 2");
            }
            orig(self, slugcatName);
        }

        public static float extraSpears(On.SlugcatStats.orig_SpearSpawnModifier orig, SlugcatStats.Name index, float originalChance)
        {
            if (index == ChandlerHooks.ChandlerName || index == TrespasserHooks.TrespasserName)
            {
                return Mathf.Pow(originalChance, 0.75f);
            }
            if (index == ImperishableHooks.ImperishableName || index == LampHooks.LampName)
            {
                return Mathf.Pow(originalChance, 0.7f);
            }

            return orig(index, originalChance);
        }

        static float explosiveSpearsNaturalSpawn(On.SlugcatStats.orig_SpearSpawnExplosiveRandomChance orig, SlugcatStats.Name index)
        {
            if (index == ChandlerHooks.ChandlerName || index == TrespasserHooks.TrespasserName)
            {
                return 0.01f;
            }
            if (index == ImperishableHooks.ImperishableName || index == TrespasserHooks.TrespasserName)
            {
                return 0.015f;
            }
            return orig(index);
        }

        public static void hypothermiaModify(On.Creature.orig_HypothermiaUpdate orig, Creature creature)
        {
            orig(creature);
            
            if (creature.abstractCreature.world.game.IsStorySession && (creature.abstractCreature.world.game.StoryCharacter == ChandlerHooks.ChandlerName || creature.abstractCreature.world.game.StoryCharacter == TrespasserHooks.TrespasserName))
            {
                creature.HypothermiaGain *= 1.1f;
            }
            else if (creature.abstractCreature.world.game.IsStorySession && (creature.abstractCreature.world.game.StoryCharacter == ImperishableHooks.ImperishableName || creature.abstractCreature.world.game.StoryCharacter == LampHooks.LampName))
            {
                creature.HypothermiaGain *= 1.2f;
            }
            if (creature is Player player && (player.slugcatStats.name == ChandlerHooks.ChandlerName && player.KarmaCap >= 3 && player.KarmaCap <= 6))
            {
                creature.HypothermiaGain *= 0.75f;
            }
            if (creature.abstractCreature.world.game.IsStorySession && (creature.abstractCreature.world.game.StoryCharacter == ChandlerHooks.ChandlerName || creature.abstractCreature.world.game.StoryCharacter == ImperishableHooks.ImperishableName || creature.abstractCreature.world.game.StoryCharacter == LampHooks.LampName 
            || creature.abstractCreature.world.game.StoryCharacter == TrespasserHooks.TrespasserName))
            {
                creature.Hypothermia += creature.HypothermiaGain * (creature.HypothermiaGain - 1f);
            }
            if (creature is Player pl && pl.TryGetLamp(out var data))
            {
                data.HypothermiaUpdate();
            }
            
        }


    }
}

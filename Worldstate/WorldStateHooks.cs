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
using System.Reflection;

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

        public static bool ChandTressWorld(this RainWorldGame game)
        {
            return game.IsStorySession && (game.StoryCharacter == ChandlerHooks.ChandlerName || game.StoryCharacter == TrespasserHooks.TrespasserName);
        }

        public static bool LampPerishWorld(this RainWorldGame game)
        {
            return game.IsStorySession && (game.StoryCharacter == LampHooks.LampName || game.StoryCharacter == ImperishableHooks.ImperishableName);
        }

        public static bool SunriseWorld(this RainWorldGame game)
        {
            return game.IsStorySession && (game.StoryCharacter == LampHooks.LampName || game.StoryCharacter == ImperishableHooks.ImperishableName || game.StoryCharacter == ChandlerHooks.ChandlerName || game.StoryCharacter == TrespasserHooks.TrespasserName);
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

        //player.slugcatStats.name == LampHooks.LampName <--less efficient for slugcatname checks, but use for storycharacter checks(if not already checking Chandler as well)
        //player.isLampScug() <------inbuilt function for checking if a scug is lamplighter
        //player.TryGetLamp(out var data) <-----functions as both a slugcat check and returns data
        //btw this goes for Trespasser as well

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
            if (index == ImperishableHooks.ImperishableName || index == LampHooks.LampName)
            {
                return 0.015f;
            }
            return orig(index);
        }

        public static void hypothermiaModify(On.Creature.orig_HypothermiaUpdate orig, Creature creature)
        {
            orig(creature);

            if (!creature.room.game.IsStorySession) return;
            
            if (creature.room.world.game.ChandTressWorld())
            {
                creature.HypothermiaGain *= 1.1f;
            }
            else if (creature.room.world.game.LampPerishWorld())
            {
                creature.HypothermiaGain *= 1.2f;
            }

            if (creature.room.world.game.SunriseWorld())
            {
                creature.Hypothermia += creature.HypothermiaGain * (creature.HypothermiaGain - 1f);
            }

            if (creature is not Player) return;

            Player pl = creature as Player;

            if ((pl.isChandler() && pl.KarmaCap >= 3 && pl.KarmaCap <= 6))
            {
                creature.HypothermiaGain *= 0.75f;
            }
            if (pl.TryGetLamp(out var data))
            {
                if(data.Warm){
                    creature.HypothermiaGain *= 0.8f;
                } else{
                    creature.HypothermiaGain *= 1.35f;
                    if (pl.Hypothermia > 1f)
                    {
                        pl.Die();
                    }
                }
            }            
        }
    }
}

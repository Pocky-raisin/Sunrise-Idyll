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
using RWCustom;
using Menu;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
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
            On.RainWorld.PostModsInit += doHookPostModsInit;
            //On.RainCycle.ctor += RainCycle_ctor;
            //On.RoomRain.Update += RoomRain_Update;
        }

        private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
        {
            orig(self, eu);

            if (self.room.game != null)
            {
                if (self.room.game.ChandTressWorld())
                {
                    if (self.intensity < 0.35f) self.intensity = 0.35f;
                }
                else if (self.room.game.LampPerishWorld())
                {
                    if (self.intensity < 0.5f) self.intensity = 0.5f;
                }
            }
        }

        public static void doHookPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self) //guarantees that these hooks are called after slugbase's
        {
            orig(self);
            On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.ctor += giveUnlockStatus;
            On.SlugcatStats.SlugcatUnlocked += idyllChecker;
            On.Menu.SlugcatSelectMenu.ctor += checkBeaten;
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

        

        public static bool idyllScugUnlocked(SlugcatStats.Name i, RainWorld rainWorld) //same effect as idyllChecker, exists solely so i don't have to use orig a ton in giveUnlockStatus
        {
            bool val4 = rainWorld.progression.miscProgressionData.beaten_Saint;
            TrespasserHooks.trespasserSaveData.TryGet<bool>("beatTrespasser", out bool val1);
            ChandlerHooks.chandlerSaveData.TryGet<bool>("beatChandler", out bool val2);
            LampHooks.lampSaveData.TryGet<bool>("beatLamp", out bool val3);
            if ((i == ChandlerHooks.ChandlerName && val1) || (i == LampHooks.LampName && val2) || (i == ImperishableHooks.ImperishableName && val3) || (ModManager.MSC && MoreSlugcats.MoreSlugcats.chtUnlockCampaigns.Value) || (i == TrespasserHooks.TrespasserName && val4))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static SlugcatStats.Name TresWorld = TrespasserHooks.TrespasserName;
        public static SlugcatStats.Name LampWorld = LampHooks.LampName;
        public static SlugcatStats.Name ChandWorld = ChandlerHooks.ChandlerName;
        public static SlugcatStats.Name PerishWorld = ImperishableHooks.ImperishableName;

        public static bool idyllChecker(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld) //checks if the slugcat is unlocked
        {
            bool val4 = rainWorld.progression.miscProgressionData.beaten_Saint;
            TrespasserHooks.trespasserSaveData.TryGet<bool>("beatTrespasser", out bool val1);
            ChandlerHooks.chandlerSaveData.TryGet<bool>("beatChandler", out bool val2);
            LampHooks.lampSaveData.TryGet<bool>("beatLamp", out bool val3);
            if (ModManager.MSC && MoreSlugcats.MoreSlugcats.chtUnlockCampaigns.Value)
            {
                return true;
            }
            if (i == ChandlerHooks.ChandlerName)
            {
                return val1;
            }
            if (i == LampHooks.LampName)
            {
                return val3;
            }
            if (i == ImperishableHooks.ImperishableName)
            {
                return val3;
            }
            if (i == TrespasserHooks.TrespasserName)
            {
                return val4;
            }
            return orig(i, rainWorld);
        }

        public static void giveUnlockStatus(On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.orig_ctor orig, Menu.SlugcatSelectMenu.SlugcatPageNewGame self, Menu.Menu menu, Menu.MenuObject owner, int pageIndex, SlugcatStats.Name i) //locks the slugcats if they're not intended to be playable yet
        {
            orig(self, menu, owner, pageIndex, i);
            string text1 = "";
            string text2 = "";
            bool flag = false;
            if (i == TrespasserHooks.TrespasserName)
            {
                flag = true;
                text1 = "THE TRESPASSER";
                if (idyllScugUnlocked(i, self.menu.manager.rainWorld))
                {
                    text2 = "TBA";
                }
                else
                {
                    text2 = "Clear the game as Saint to unlock.";
                }

            }
            else if (i == ChandlerHooks.ChandlerName)
            {
                flag = true;
                text1 = "THE CHANDLER";
                if (idyllScugUnlocked(i, self.menu.manager.rainWorld))
                {
                    text2 = "TBA";
                }
                else
                {
                    text2 = "Clear the game as Trespasser to unlock.";
                }
            }
            else if (i == LampHooks.LampName)
            {
                flag = true;
                text1 = "THE LAMPLIGHTER";
                if (idyllScugUnlocked(i, self.menu.manager.rainWorld))
                {
                    text2 = "TBA";
                }
                else
                {
                    text2 = "Clear the game as Chandler to unlock.";
                }
            }
            else if (i == ImperishableHooks.ImperishableName)
            {
                flag = true;
                text1 = "THE IMPERISHABLE";
                if (idyllScugUnlocked(i, self.menu.manager.rainWorld))
                {
                    text2 = "TBA";
                }
                else
                {
                    text2 = "Clear the game as Lamplighter to unlock.";
                }
            }
            if (flag == true)
            {
                text2 = Custom.ReplaceLineDelimeters(text2);
                int num = text2.Count((char f) => f == '\n');
                float num2 = 0f;
                if (num > 1)
                {
                    num2 = 30f;
                }
                self.difficultyLabel = new Menu.MenuLabel(menu, self, text1, new Vector2(-1000f, self.imagePos.y - 249f + num2), new Vector2(200f, 30f), true, null);
                self.difficultyLabel.label.alignment = FLabelAlignment.Center;
                self.subObjects.Add(self.difficultyLabel);
                self.infoLabel = new Menu.MenuLabel(menu, self, text2, new Vector2(-1000f, self.imagePos.y - 249f - 60f + num2 / 2f), new Vector2(400f, 60f), true, null);
                self.infoLabel.label.alignment = FLabelAlignment.Center;
                self.subObjects.Add(self.infoLabel);
                if (num > 1)
                {
                    self.imagePos.y = self.imagePos.y + 30f;
                    self.sceneOffset.y = self.sceneOffset.y + 30f;
                }
                if (idyllScugUnlocked(i, self.menu.manager.rainWorld))
                {
                    self.difficultyLabel.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
                    self.infoLabel.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
                }
                else
                {
                    self.difficultyLabel.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey);
                    self.infoLabel.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey);
                }
            }
        }

        public static void checkBeaten(On.Menu.SlugcatSelectMenu.orig_ctor orig, Menu.SlugcatSelectMenu self, ProcessManager manager) //makes the flags used by the above hooks
        {
            try
            {
                var result1 = TrespasserHooks.trespasserSaveData.TryGet<bool>("beatTrespasser", out bool val1);
                
                if (!val1)
                {
                    TrespasserHooks.trespasserSaveData.Set<bool>("beatTrespasser", false);
                }
                
            }
            catch(Exception e1)
            {
                Debug.Log("Save Setup Error 1 - " + e1);
            }

            try
            {
                var result2 = ChandlerHooks.chandlerSaveData.TryGet<bool>("beatChandler", out bool val2);

                if (!val2)
                {
                    ChandlerHooks.chandlerSaveData.Set<bool>("beatChandler", false);
                }

            }
            catch (Exception e2)
            {
                Debug.Log("Save Setup Error 2 - " + e2);
            }
            try
            {
                var result3 = LampHooks.lampSaveData.TryGet<bool>("beatLamp", out bool val3);

                if (!val3)
                {
                    LampHooks.lampSaveData.Set<bool>("beatLamp", false);
                }

            }
            catch (Exception e3)
            {
                Debug.Log("Save Setup Error 3 - " + e3);
            }
            try
            {
                var result4 = ImperishableHooks.imperishableSaveData.TryGet<bool>("beatImperishable", out bool val4);

                if (!val4)
                {
                    ImperishableHooks.imperishableSaveData.Set<bool>("beatImperishable", false);
                }

            }
            catch (Exception e4)
            {
                Debug.Log("Save Setup Error 4 - " + e4);
            }
            orig(self, manager);
        }

        public static void doInitialSaveStuff(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcatName) //sets up several savestate flags
        {
            try
            {
                var result = ImperishableHooks.imperishableSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val1);
                if (!val1)
                {
                    ImperishableHooks.imperishableSaveData.Set<int>("minKarma", 9);
                    ImperishableHooks.imperishableSaveData.Set<int>("maxKarma", 9);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didCSKYRitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didSLRitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didCLRitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didSURitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didVSRitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didLFRitual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("did[new_region_1_initials]Ritual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("did[new_region_2_initials]Ritual", false);
                    ImperishableHooks.imperishableSaveData.Set<bool>("didUGRitual", false);
                    //tenth ritual is in HR
                    ImperishableHooks.imperishableSaveData.Set<bool>("alreadyDidSaveSetup", true);
                    ImperishableHooks.imperishableSaveData.Set<bool>("beatImperishable", false);
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
                    ChandlerHooks.chandlerSaveData.Set<bool>("didUGRitual", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didCCRitual", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didCLRitual", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("did[new_region_1_initials]Ritual", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didVSRitual", false);
                    ChandlerHooks.chandlerSaveData.Set<bool>("didSIRitual", false);
                    //seventh ritual is in SU
                    ChandlerHooks.chandlerSaveData.Set<bool>("alreadyDidSaveSetup", true);
                    ChandlerHooks.chandlerSaveData.Set<bool>("beatChandler", false);
                }
            }
            catch (Exception e2)
            {
                Debug.Log(e2 + "WorldStateHooks Error 2");
            }
            try
            {
                var result = TrespasserHooks.trespasserSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val3);
                if (!val3)
                {
                    TrespasserHooks.trespasserSaveData.Set<bool>("alreadyDidSaveSetup", true);
                    TrespasserHooks.trespasserSaveData.Set<bool>("beatTrespasser", false);
                    TrespasserHooks.trespasserSaveData.Set<bool>("karmaSpecial", false);
                    TrespasserHooks.trespasserSaveData.Set<string>("fatalShelter", "");
                    TrespasserWorld.foundTokens.Add("SU", false);
                    TrespasserWorld.foundTokens.Add("HI", false);
                    TrespasserWorld.foundTokens.Add("UG", false);
                    TrespasserWorld.foundTokens.Add("CC", false);
                    TrespasserWorld.foundTokens.Add("VS", false);
                    TrespasserWorld.foundTokens.Add("GW", false);
                    TrespasserWorld.foundTokens.Add("SL", false);
                    TrespasserWorld.foundTokens.Add("SI", false);
                    TrespasserWorld.foundTokens.Add("LF", false);
                    TrespasserWorld.foundTokens.Add("LT", false);
                    bool flag = true;
                    for(int i = 0; i < ModManager.InstalledMods.Count; i++)
                    {
                        if (ModManager.InstalledMods[i].id == "propane.begoniaCat")
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        TrespasserWorld.foundTokens.Add("CL", false);
                    }
                    else
                    {
                        TrespasserWorld.foundTokens.Add("RP", false);
                        TrespasserWorld.foundTokens.Add("JW", false);
                        TrespasserWorld.foundTokens.Add("CE", false);
                    }
                }
            }
            catch (Exception e3)
            {
                Debug.Log(e3 + "WorldStateHooks Error 3");
            }
            try
            {
                var result = LampHooks.lampSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val4);
                if (!val4)
                {
                    LampHooks.lampSaveData.Set<bool>("alreadyDidSaveSetup", true);
                    LampHooks.lampSaveData.Set<bool>("beatLamp", false);
                }
            }
            catch (Exception e4)
            {
                Debug.Log(e4 + "WorldStateHooks Error 4");
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
            if (index == ChandlerHooks.ChandlerName|| index == TrespasserHooks.TrespasserName)
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

            if (!creature.room.game.IsStorySession || creature.abstractCreature.HypothermiaImmune || !(creature is Player)) return;

            Player pl = creature as Player;

            if ((pl.isChandler() && pl.KarmaCap >= 3 && pl.KarmaCap <= 6))
            {
                creature.HypothermiaGain *= 0.75f;
            }
            if (pl.TryGetLamp(out var data))
            {
                if(data.Warm){
                    creature.HypothermiaGain *= 0.8f;
                }
                else{
                    creature.HypothermiaGain *= 1.35f;
                }
            }            
        }

        public static void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            if (world.game != null && world.game.SunriseWorld())
            {
                minutes += Random.Range(3, 6);
            }
                orig(self, world, minutes);
        }

    }
}

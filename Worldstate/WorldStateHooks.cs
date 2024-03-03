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
            On.Menu.SlugcatSelectMenu.ctor += checkBeaten;
        }

        public static void doHookPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self) //guarantees that these hooks are called after slugbase's
        {
            On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.ctor += giveUnlockStatus;
            On.Menu.SlugcatSelectMenu.SlugcatUnlocked += idyllChecker;
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

        public static bool idyllChecker(On.Menu.SlugcatSelectMenu.orig_SlugcatUnlocked orig, Menu.SlugcatSelectMenu self, SlugcatStats.Name i) //checks if the slugcat is unlocked
        {
            bool val4 = self.manager.rainWorld.progression.miscProgressionData.beaten_Saint;
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
            return orig(self, i);
        }

        public static void giveUnlockStatus(On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.orig_ctor orig, Menu.SlugcatSelectMenu.SlugcatPageNewGame self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name i) //locks the slugcats if they're not intended to be playable yet
        {
            orig(self, menu, owner, pageIndex, i);
            string text1 = "";
            string text2 = "";
            bool flag = false;
            if ( i == TrespasserHooks.TrespasserName)
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
                self.difficultyLabel = new MenuLabel(menu, self, text1, new Vector2(-1000f, self.imagePos.y - 249f + num2), new Vector2(200f, 30f), true, null);
                self.difficultyLabel.label.alignment = FLabelAlignment.Center;
                self.subObjects.Add(self.difficultyLabel);
                self.infoLabel = new MenuLabel(menu, self, text2, new Vector2(-1000f, self.imagePos.y - 249f - 60f + num2 / 2f), new Vector2(400f, 60f), true, null);
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
                    self.infoLabel.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
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
            if (creature is Player player1 && player1.slugcatStats.name == LampHooks.LampName)
            {
                player1.TryGetLamp(out var data);
                if(data.Warm){
                    creature.HypothermiaGain *= 0.6f;
                } else{
                    creature.HypothermiaGain *= 1.5f;
                }
            }
            if (creature.abstractCreature.world.game.IsStorySession && (creature.abstractCreature.world.game.StoryCharacter == ChandlerHooks.ChandlerName || creature.abstractCreature.world.game.StoryCharacter == ImperishableHooks.ImperishableName || creature.abstractCreature.world.game.StoryCharacter == LampHooks.LampName 
            || creature.abstractCreature.world.game.StoryCharacter == TrespasserHooks.TrespasserName))
            {
                creature.Hypothermia += creature.HypothermiaGain * (creature.HypothermiaGain - 1f);
            }
            if(creature is Player player2 && player2.slugcatStats.name == LampHooks.LampName && creature.Hypothermia > 1f){
                player2.TryGetLamp(out var data1);
                if(!data1.Warm){
                    creature.Die();
                }
            }
            
        }
    }
}

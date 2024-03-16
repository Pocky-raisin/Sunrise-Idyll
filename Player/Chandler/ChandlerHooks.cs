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
    public static class ChandlerHooks
    {
        public static void ApplyHooks()
        {
            On.Spear.HitSomething += spearResist;
            On.SaveState.IncreaseKarmaCapOneStep += increaseKarmaSingleStep;
            On.Player.CanEatMeat += tempEdible;
        }

        public static SlugBaseSaveData chandlerSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(ChandlerName));
        public static readonly SlugcatStats.Name ChandlerName = new SlugcatStats.Name("IDYLL.Candle", false);

        public static bool isChandler(this Player pl)
        {
            return pl.SlugCatClass == ChandlerName;
        }

        public static bool spearResist(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (self.room.world.game.IsStorySession)
            {
                if (result.obj is Player player && player.slugcatStats.name == ChandlerName && player.KarmaCap >= 4 && player.KarmaCap <= 7 && UnityEngine.Random.value <= 0.15) //15% chance to work
                {
                    self.spearDamageBonus *= 0.01f; //effectively nullifies, but lets the spear stick in the player.
                }
            }
            return orig(self, result, eu);
        }

        public static void increaseKarmaSingleStep(On.SaveState.orig_IncreaseKarmaCapOneStep orig, SaveState self) //increases karma by one step each time, instead of 4->6 like the vanilla method does
        {
            if(self.saveStateNumber != null)
            {
                if (self.saveStateNumber == ChandlerName)
                {
                    self.deathPersistentSaveData.karmaCap++;
                }
                else
                {
                    orig(self);
                }
            }
            else
            {
                orig(self);
            }
        }

        public static bool tempEdible(On.Player.orig_CanEatMeat orig, Player self, Creature crit) //does not work at present
        {
            if (self.room.world.game.IsStorySession)
            {
                if (self.slugcatStats.name == ChandlerName && self.KarmaCap >= 5)
                {
                    return crit.dead;
                }
                else
                {
                    return orig(self, crit);
                }
            }
            else
            {
                return orig(self, crit);
            }
        }
    }
}

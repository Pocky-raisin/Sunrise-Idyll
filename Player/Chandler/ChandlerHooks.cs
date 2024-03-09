﻿using System;
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
            if (result.obj is Player && (result.obj as Player).slugcatStats.name == ChandlerName && (result.obj as Player).KarmaCap >= 4 && (result.obj as Player).KarmaCap <= 7 && UnityEngine.Random.value <= 0.15) //15% chance to work
            {
                self.spearDamageBonus *= 0.01f; //effectively nullifies, but lets the spear stick in the player.
            }
            return orig(self, result, eu);
        }

        public static void increaseKarmaSingleStep(On.SaveState.orig_IncreaseKarmaCapOneStep orig, SaveState self) //increases karma by one step each time, instead of 4->6 like the vanilla method does
        {
            if (self.saveStateNumber == ChandlerName)
            {
                if (self.deathPersistentSaveData.karmaCap < 6) //karma 10 will be when the campaign ends, so it isn't required
                {
                    self.deathPersistentSaveData.karmaCap++;
                }
            }
            else
            {
                orig(self);
            }
        }

        public static bool tempEdible(On.Player.orig_CanEatMeat orig, Player self, Creature crit) //does not work at present
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
    }
}

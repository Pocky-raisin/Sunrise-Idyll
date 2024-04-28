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
    public static class FireCatsHooks
    {
        public static void ApplyHooks()
        {
            On.Player.SpearStick += noStickSpears;
            On.Creature.Violence += deflectSpears;
            On.PlayerGraphics.Update += forgeGlowMake;
            On.Creature.Violence += damageImmune;
            On.Player.ctor += foodStuff;
            On.Player.ctor += immunities;
            On.Lizard.Bite += alwaysSurviveLizards;
            On.Player.GrabUpdate += makeFireSpears;
        }

        private static int spearCraftCountUp = 0;

        public static  void deflectSpears(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            int check = 0;
            if (self is Player player && (self.isChandlerWorld() || self.isImperishableWorld()))
            {
                if (player.isImperishableWorld() && player.isPerish())
                {
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                if ((player.isChandler() && player.KarmaCap >= 8) || (player.isImperishableWorld() && check >= 8))
                {
                    if (type == Creature.DamageType.Stab || type == Creature.DamageType.Bite)
                    {
                        if (self.graphicsModule != null && source != null && self.room != null)
                        {
                            self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.mainBodyChunk);
                            self.room.AddObject(new StationaryEffect(source.pos, new Color(1f, 1f, 1f), self.graphicsModule as LizardGraphics, StationaryEffect.EffectType.FlashingOrb));

                        }
                        return;
                    }

                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        public static bool noStickSpears(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos hitAppendage, Vector2 direction)
        {
            int check = 0;
            if (self.room.world.game.IsStorySession && (self.isChandler() || self.isPerish()))
            {
                if (self.isImperishableWorld() && self.isPerish())
                {
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                if ((self.isChandler() && self.KarmaCap >= 8) || (self.isPerish() && check >= 8))
                {
                    return false;
                }
                else
                {
                    return orig(self, source, dmg, chunk, hitAppendage, direction);
                }
            }
            else
            {
                return orig(self, source, dmg, chunk, hitAppendage, direction);
            }
        }

        public static void forgeGlowMake(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            int check = 0;
            if(self.player.room.world.game.IsStorySession && (self.player.isPerish() || self.player.isChandler()))
            {
                if (self.player.isImperishableWorld() && self.player.isPerish())
                {
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                
                if (self.player.slugcatStats.name == ChandlerHooks.ChandlerName && self.player.KarmaCap >= 3)
                {
                    if (self.lightSource != null)
                    {
                        self.lightSource.stayAlive = true;
                        self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);
                    }
                    if (self.lightSource == null)
                    {
                        self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, new Color(1f, 0.56078431372f, 0.2431372549f), self.player);
                        self.lightSource.requireUpKeep = true;
                        self.lightSource.setRad = new float?(300f);
                        self.lightSource.setAlpha = new float?(1f);
                        self.player.room.AddObject(self.lightSource);
                    }
                }
                else if (self.player.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 2)
                {
                    if (self.lightSource != null)
                    {
                        self.lightSource.stayAlive = true;
                        self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);
                    }
                    if (self.lightSource == null)
                    {
                        self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, new Color(1f, 0.76078431372f, 0.4431372549f), self.player);
                        self.lightSource.requireUpKeep = true;
                        self.lightSource.setRad = new float?(500f);
                        self.lightSource.setAlpha = new float?(1f);
                        self.player.room.AddObject(self.lightSource);
                    }
                }
            }
            
        }

        public static void damageImmune(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            int check = 0;
            if (self.isChandlerWorld() || self.isImperishableWorld())
            {
                if (self.isImperishableWorld())
                {
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                if (self is Player player && (type == Creature.DamageType.Explosion || type == WorldThings.Fire) && ((player.isChandler() && player.KarmaCap >= 4) || (player.isPerish() && check >= 3)))
                {
                    return;
                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        public static void immunities(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            int check = 0;
            orig(self, abstractCreature, world);
            if(self.isImperishableWorld() || self.isChandlerWorld())
            {
                if (self.isImperishableWorld() && self.isPerish()){
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                if ((self.isChandler() && self.KarmaCap >= 7) || (self.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 4))
                {
                    self.abstractCreature.HypothermiaImmune = true;
                }
                if (self.isChandler() || (self.isPerish() && check >= 1))
                {
                    self.abstractCreature.lavaImmune = true;
                }
            }
        }

        public static void makeFireSpears(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            float hue = 0f;
            if (!self.room.world.game.IsStorySession || !(self.isPerish() || self.isChandler()))
            {
                return;
            }
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null)
                {
                    if (self.grasps[i].grabbed is Spear spear)
                    {
                        if (!spear.bugSpear && !spear.abstractSpear.electric && !spear.abstractSpear.explosive)
                        {
                            if (spearCraftCountUp <= 200)
                            {
                                if (self.slugcatStats.name == ChandlerHooks.ChandlerName && self.KarmaCap >= 6)
                                {
                                    spearCraftCountUp++;
                                }
                                else if (self.isPerish() && self.isImperishableWorld())
                                {
                                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
                                    if (check >= 5)
                                    {
                                        spearCraftCountUp += 200;
                                    }
                                }
                            }
                            else
                            {
                                spearCraftCountUp = 0;
                                self.ReleaseGrasp(i);
                                spear.abstractPhysicalObject.realizedObject.RemoveFromRoom();
                                if (self.isChandler())
                                {
                                    hue = Mathf.Clamp(UnityEngine.Random.value, 0.1f, 1);
                                }
                                else if(self.isPerish())
                                {
                                    hue = Mathf.Clamp(UnityEngine.Random.value, 0.5f, 1);
                                }
                                AbstractSpear spear1 = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, hue);
                                self.room.abstractRoom.AddEntity(spear1);
                                spear1.RealizeInRoom();
                                if (self.FreeHand() != -1)
                                {
                                    self.SlugcatGrab(spear1.realizedObject, self.FreeHand());
                                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.firstChunk.pos, 1f, UnityEngine.Random.Range(0.8f, 1.2f));
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void alwaysSurviveLizards(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
        {
            int check = 0;
            float origBiteChance = self.lizardParams.biteChance;
            float origBiteDamage = self.lizardParams.biteDamage;
            if (self.isImperishableWorld() || self.isChandlerWorld())
            {
                if (self.isImperishableWorld())
                {
                    ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
                }
                if (chunk.owner is Player player && (player.isChandler() && player.KarmaCap >= 8 || (player.isPerish() && check >= 8)))
                {
                    self.lizardParams.biteDamageChance = 0;
                    self.lizardParams.biteDamage = 0;
                }
            }
            orig(self, chunk);
            self.lizardParams.biteDamageChance = origBiteChance;
            self.lizardParams.biteDamage = origBiteDamage;
        }

        public static void foodStuff(On.Player.orig_ctor orig, Player self, AbstractCreature creature, World world)
        {
            int check = 0;
            orig(self, creature, world);
            if (!self.room.world.game.IsStorySession || !(self.isPerish() || self.isChandler()))
            {
                return;
            }
            if (!self.isImperishableWorld() && self.isPerish())
            {
                ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out check);
            }
            if (self.isChandler() && self.KarmaCap >= 5)
            {
                self.slugcatStats.foodToHibernate = 5;
            }
            else if (self.isPerish() && check <= 4)
            {
                self.slugcatStats.maxFood = 6;
            }
        }
    }
}

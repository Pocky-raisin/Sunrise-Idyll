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
            if (self is Player player)
            {
                ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
                if ((player.slugcatStats.name == ChandlerHooks.ChandlerName && player.KarmaCap >= 8) || (player.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 8))
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
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if ((self.slugcatStats.name == ChandlerHooks.ChandlerName && self.KarmaCap >= 8) || (self.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 8))
            {
                return false;
            }
            else
            {
                return orig(self, source, dmg, chunk, hitAppendage, direction);
            }
        }

        public static void forgeGlowMake(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
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

        public static void damageImmune(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self is Player player && (type == Creature.DamageType.Explosion || type == WorldThings.Fire) && ((player.slugcatStats.name == ChandlerHooks.ChandlerName && player.KarmaCap >= 4) || (player.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 3)))
            {
                return;
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

        }

        public static void immunities(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if ((self.slugcatStats.name == ChandlerHooks.ChandlerName && self.KarmaCap >= 7) || (self.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 4))
            {
                self.abstractCreature.HypothermiaImmune = true;
            }
            if (self.slugcatStats.name == ChandlerHooks.ChandlerName || (self.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 1))
            {
                self.abstractCreature.lavaImmune = true;
            }
        }

        public static void makeFireSpears(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

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
                                ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
                                if (self.slugcatStats.name == ChandlerHooks.ChandlerName && self.KarmaCap >= 6)
                                {
                                    spearCraftCountUp++;
                                }
                                else if (self.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 5)
                                {
                                    spearCraftCountUp += 200;
                                }
                            }
                            else
                            {
                                spearCraftCountUp = 0;
                                self.ReleaseGrasp(i);
                                spear.abstractPhysicalObject.realizedObject.RemoveFromRoom();
                                AbstractSpear spear1 = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, 0.5f);
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
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (chunk.owner is Player player && (player.slugcatStats.name == ChandlerHooks.ChandlerName && player.KarmaCap >= 8 || (player.slugcatStats.name == ImperishableHooks.ImperishableName && check >= 8)))
            {
                self.lizardParams.biteDamageChance = 0;
                self.lizardParams.biteDamage = 0;
            }
            orig(self, chunk);
        }

        public static void foodStuff(On.Player.orig_ctor orig, Player self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            ImperishableHooks.imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self.slugcatStats.name == ChandlerHooks.ChandlerName && self.KarmaCap >= 5)
            {
                self.slugcatStats.foodToHibernate = 5;
            }
            else if (self.slugcatStats.name == ImperishableHooks.ImperishableName && check <= 4)
            {
                self.slugcatStats.maxFood = 6;
            }
        }
    }
}

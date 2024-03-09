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
    public static class ImperishableHooks
    {
        public static void ApplyHooks()
        {
            On.Player.Update += dBreakerMinKarma;
            On.Player.Die += dBreakerMinKarmaOnDeath;
            On.Player.Update += damageGrabber;
            On.Player.Update += explodeJumpImperishable;
        }

        public static SlugBaseSaveData imperishableSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(ImperishableName));
        public static readonly SlugcatStats.Name ImperishableName = new SlugcatStats.Name("IDYLL.Wildfire", false);

        public static bool isPerish(this Player pl)
        {
            return pl.SlugCatClass == ImperishableName;
        }

        public static void dBreakerMinKarma(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
            imperishableSaveData.TryGet<int>("maxKarma", out int maxKarma);
            if (self.slugcatStats.name == ImperishableName)
            {

                if (minKarma <= 6)
                {
                    maxKarma = minKarma + 2;
                }
                else
                {
                    maxKarma = 9;
                }
                imperishableSaveData.Set<int>("maxKarma", maxKarma);
                if (self.KarmaCap > maxKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = maxKarma;
                }
                if (self.Karma > self.KarmaCap)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = self.KarmaCap;
                }
                if (self.Karma < minKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = minKarma;
                }

            }
        }

        public static void decreaseMinKarmaOneStep(Player player)
        {
            if (player.slugcatStats.name == ImperishableName)
            {
                imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
                minKarma--;
                imperishableSaveData.Set<int>("minKarma", minKarma);
            }
        }

        public static void dBreakerMinKarmaOnDeath(On.Player.orig_Die orig, Player self)
        {
            if (self.slugcatStats.name == ImperishableName)
            {
                imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
                if (self.Karma == minKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
                }
            }
            orig(self);
        }

        public static void explodeJumpImperishable(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if(self.slugcatStats.name == ImperishableName && check >= 9)
            {
                if (self.Consious || self.dead)
                {
                    if (self.wantToJump > 0 && self.input[0].pckp && !self.pyroJumpped && self.canJump <= 0 && self.eatMeat < 20 && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f))) && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                        && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.bodyMode != Player.BodyModeIndex.Swimming && self.bodyMode != Player.BodyModeIndex.WallClimb && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab
                        && self.onBack == null)
                    {
                        self.pyroJumpped = true;
                        self.noGrabCounter = 5;
                        Vector2 Pos = self.firstChunk.pos;
                        for (int i = 0; i < 8; i++)
                        {
                            self.room.AddObject(new Explosion.ExplosionSmoke(Pos, RWCustom.Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                        }
                        self.room.AddObject(new Explosion.ExplosionLight(Pos, 160f, 1f, 3, new Color(1f, 0.6392f, 0.1529f)));
                        for (int j = 0; j < 10; j++)
                        {
                            Vector2 a = RWCustom.Custom.RNV();
                            self.room.AddObject(new Spark(Pos + a * UnityEngine.Random.value * 40f, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.black, null, 4, 18));
                        }
                        self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, Pos, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
                        self.room.InGameNoise(new Noise.InGameNoise(Pos, 8000f, self, 1f));
                        if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
                        {
                            float num3 = (float)self.input[0].x;
                            float num4 = (float)self.input[0].y;
                            while (num4 == 0f && num3 == 0f)
                            {
                                num3 = ((UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1));
                                num4 = ((UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1));
                            }
                            self.bodyChunks[0].vel.x = 9f * num3;
                            self.bodyChunks[0].vel.y = 9f * num4;
                            self.bodyChunks[1].vel.x = 8f * num3;
                            self.bodyChunks[1].vel.y = 8f * num4;

                        }
                        else
                        {
                            if (self.input[0].x != 0)
                            {
                                self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f;
                                self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f;
                                self.jumpBoost = 6f;
                            }
                            if (self.input[0].x == 0 || self.input[0].y == 1)
                            {
                                self.bodyChunks[0].vel.y = 16f;
                                self.bodyChunks[1].vel.y = 15f;
                                self.jumpBoost = 10f;
                            }
                            if (self.input[0].y == 1)
                            {
                                self.bodyChunks[0].vel.x = 10f * (float)self.input[0].x;
                                self.bodyChunks[1].vel.x = 8f * (float)self.input[0].x;
                            }
                            else
                            {
                                self.bodyChunks[0].vel.x = 15f * (float)self.input[0].x;
                                self.bodyChunks[1].vel.x = 13f * (float)self.input[0].x;
                            }
                            self.animation = Player.AnimationIndex.Flip;
                            self.bodyMode = Player.BodyModeIndex.Default;


                        }
                    }
                    else if (self.input[0].pckp && self.eatMeat < 20 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped && !self.submerged)
                    {
                        if (self.canJump <= 0)
                        {
                            self.pyroJumpped = true;
                            self.bodyChunks[0].vel.y = 8f;
                            self.bodyChunks[1].vel.y = 6f;
                            self.jumpBoost = 6f;
                            self.forceSleepCounter = 0;
                        }
                        Vector2 pos2 = self.firstChunk.pos;
                        for (int k = 0; k < 8; k++)
                        {
                            self.room.AddObject(new Explosion.ExplosionSmoke(pos2, RWCustom.Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                        }
                        self.room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, new Color(1f, 0.6392f, 0.1529f)));
                        for (int l = 0; l < 10; l++)
                        {
                            Vector2 a2 = RWCustom.Custom.RNV();
                            self.room.AddObject(new Spark(pos2 + a2 * UnityEngine.Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.black, null, 4, 18));
                        }
                        self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
                        self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, pos2, 0.5f + UnityEngine.Random.value * 0.5f, 0.5f + UnityEngine.Random.value * 2f);
                        self.room.InGameNoise(new Noise.InGameNoise(pos2, 8000f, self, 1f));
                        List<Weapon> list = new List<Weapon>();
                        for (int m = 0; m < self.room.physicalObjects.Length; m++)
                        {
                            for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                            {
                                if (self.room.physicalObjects[m][n] is Weapon)
                                {
                                    Weapon weapon = self.room.physicalObjects[m][n] as Weapon;
                                    if (weapon.mode == Weapon.Mode.Thrown && RWCustom.Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                                    {
                                        list.Add(weapon);
                                    }
                                }
                                bool flag3;
                                if (ModManager.CoopAvailable && !RWCustom.Custom.rainWorld.options.friendlyFire)
                                {
                                    Player player = self.room.physicalObjects[m][n] as Player;
                                    flag3 = (player == null || player.isNPC);
                                }
                                else
                                {
                                    flag3 = true;
                                }
                                bool flag4 = flag3;
                                if (self.room.physicalObjects[m][n] is Creature && self.room.physicalObjects[m][n] != self && flag4)
                                {
                                    Creature creature = self.room.physicalObjects[m][n] as Creature;
                                    if (RWCustom.Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (RWCustom.Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                                    {
                                        self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                                        creature.SetKillTag(self.abstractCreature);

                                        creature.Stun(80);
                                    }
                                    creature.firstChunk.vel = RWCustom.Custom.DegToVec(RWCustom.Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                                    if (creature is TentaclePlant)
                                    {
                                        for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                        {
                                            creature.ReleaseGrasp(num5);
                                        }
                                    }
                                }
                            }
                        }
                        if (list.Count > 0 && self.room.game.IsArenaSession)
                        {
                            self.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
                        }
                        for (int num6 = 0; num6 < list.Count; num6++)
                        {
                            list[num6].ChangeMode(Weapon.Mode.Free);
                            list[num6].firstChunk.vel = RWCustom.Custom.DegToVec(RWCustom.Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
                            list[num6].SetRandomSpin();
                        }

                    }
                }
                if (self.canJump > 0 || !self.Consious || self.Stunned || self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.bodyMode == Player.BodyModeIndex.WallClimb || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.VineGrab ||
                    self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.bodyMode == Player.BodyModeIndex.Swimming || ((self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity <= 0.5f || self.gravity <= 0.5f) && (self.wantToJump == 0 || !self.input[0].pckp)))
                {
                    self.pyroJumpped = false;
                }
            }
            
        }

        public static void damageGrabber(On.Player.orig_Update orig, Player self, bool eu)
        {
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            Creature grabber;
            if (self.grabbedBy.Count > 0 && self.slugcatStats.name == ImperishableName && check >= 7)
            {
                
                
                for (int i = self.grabbedBy.Count - 1; i >= 0; i--)
                {
                    grabber = self.grabbedBy[i].grabber;
                    PhysicalObject.Appendage app = new PhysicalObject.Appendage(grabber, 999, grabber.bodyChunks.Length);
                    PhysicalObject.Appendage.Pos pos = new PhysicalObject.Appendage.Pos(app, 0, 0.5f);
                    try
                    {
                        if(grabber is Vulture || grabber is TentaclePlant || grabber is Inspector || grabber is PoleMimic || grabber is DaddyLongLegs)
                        {
                            grabber.Violence(self.mainBodyChunk, new Vector2(0, 0), null, null, MoreSlugcatsEnums.DamageType.None, 0.3f, 1f);
                        }
                        else
                        {
                            grabber.Violence(self.mainBodyChunk, new Vector2(0, 0), grabber.mainBodyChunk, pos, MoreSlugcatsEnums.DamageType.None, 0.3f, 1f);
                        }
                        grabber.Stun(80);
                    }
                    catch(Exception e)
                    {
                        Debug.Log("G" + e);
                    }
                    self.cantBeGrabbedCounter += 30;
                    grabber.LoseAllGrasps();

                }
            }
            orig(self, eu);
        }

        public static PhysicalObject.Appendage grabbingApp(PhysicalObject self)
        {
            
            int appendage = -1;
            if (self.appendages.Count >= 0)
            {
                for (int i = 0; i < self.appendages.Count; i++)
                {
                    if (self is DaddyLongLegs dll)
                    {
                        if (dll.tentacles[i].grabChunk != null)
                        {
                            appendage = i; break;
                        }
                    }
                    else if (self is Inspector insp)
                    {
                        for (int j = 0; j < insp.grasps.Length; j++)
                        {
                            if (insp.grasps[j] != null)
                            {
                                appendage = i; break;
                            }
                        }
                    }
                    else if (self is PoleMimic mimic)
                    {
                        for(int j = 0; j < mimic.grasps.Length; j++)
                        {
                            if (mimic.grasps[j] != null)
                            {
                                appendage = i; break;
                            }
                        }
                    }
                    else if (self is TentaclePlant kelp)
                    {
                        for (int j = 0; j < kelp.grasps.Length; j++)
                        {
                            if (kelp.grasps[j] != null)
                            {
                                appendage = i; break;
                            }
                        }
                    }
                    else if (self is Vulture vulture)
                    {
                        for (int j = 0; j < vulture.grasps.Length; j++)
                        {
                            if (vulture.grasps[j] != null)
                            {
                                appendage = i; break;
                            }
                        }
                    }
                    else if (self is StowawayBug bug)
                    {
                        for (int j = 0; j < bug.grasps.Length; j++)
                        {
                            if (bug.grasps[j] != null)
                            {
                                appendage = i; break;
                            }
                        }
                    }
                }
            }
            if(appendage >= 0)
            {
                return self.appendages[appendage];
            }
            else
            {
                return null;
            }
            
        }
    }
}

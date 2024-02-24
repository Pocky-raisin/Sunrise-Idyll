using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Sunrise;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Random = UnityEngine.Random;
using RWCustom;
using AnimIndex = Player.AnimationIndex;
using MoreSlugcats;

namespace Sunrise
{
    public static class LampHooks
    {
        public static void ApplyHooks()
        {
            On.Player.GraspsCanBeCrafted += GraspsCanBeCrafted;
            IL.Player.GrabUpdate += IL_GrabUpdate;
            On.Player.SpitUpCraftedObject += SpitUpCraftedObject;

            On.Player.TerrainImpact += TerrainImpact;
            On.Player.SlugSlamConditions += SlugSlamConditions;
            On.Player.Update += Update;
        }


        public static bool IsLampScug(this Player self)
        {
            return self.SlugCatClass.value == "LampScug";
        }

        public static void Update(On.Player.orig_Update orig, Player self, bool e)
        {
            orig(self, e);

            //add goolantern check


            if (self.TryGetLamp(out var data))
            {
                //Warmth interactions
                //implement soaking

                if (data.Warm)
                {
                    self.Hypothermia -= 0.0003f;
                    if (self.Hypothermia < 0f)
                    {
                        self.Hypothermia = 0f;
                    }
                }


                if (self.firstChunk.submersion > 0.25f)
                {
                    data.SoakCounter += 7;
                }

                data.DroolMeltCounter = Mathf.Min(100, data.SoakCounter);

                if (data.SoakCounter > 0)
                {
                    data.Warm = false;

                    if (self.HypothermiaGain > 0)
                    {
                        self.HypothermiaGain *= 1.2f;
                    }
                    else
                    {
                        self.HypothermiaGain *= 0.8f;
                    }

                    float shiver = Mathf.Min(2f, data.SoakCounter / 5);

                    if (!self.dead && self.graphicsModule != null)
                    {
                        (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (shiver * 0.75f); // Head shivers
                        self.Blink(5);
                    }

                    if (self.Hypothermia > 1f)
                    {
                        self.Die();
                    }

                    data.SoakCounter--;

                }

                if (data.SoakCounter <= 0)
                {
                    data.Warm = true;
                }

            }
        }

        public static bool GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            var result = orig(self);
            if (self.IsLampScug())
            {
                if (self.FoodInStomach >= 1)
                {
                    Creature.Grasp[] grasps = self.grasps;
                    for (int i = 0; i < grasps.Length; i++)
                    {
                        if (grasps[i] != null && grasps[i].grabbed is IPlayerEdible && (grasps[i].grabbed as IPlayerEdible).Edible)
                        {
                            return false;
                        }
                    }
                    if (grasps[0] != null && grasps[0].grabbed is Spear)// && !(grasps[0].grabbed as Spear).abstractSpear.explosive)//edit to goo spear check
                    {
                        return true;
                    }
                    if (grasps[0] == null && grasps[1] != null && grasps[1].grabbed is Spear && !(grasps[1].grabbed as Spear).abstractSpear.explosive && self.objectInStomach == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return result;
        }

        public static void SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
        {
            if (self.IsLampScug())
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null)
                    {
                        AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                        if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)// && !(abstractPhysicalObject as AbstractSpear).explosive)
                        {
                            if ((abstractPhysicalObject as AbstractSpear).electric && (abstractPhysicalObject as AbstractSpear).electricCharge > 0)
                            {
                                self.room.AddObject(new ZapCoil.ZapFlash(self.firstChunk.pos, 10f));
                                self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);
                                if (self.Submersion > 0.5f)
                                {
                                    self.room.AddObject(new UnderwaterShock(self.room, null, self.firstChunk.pos, 10, 800f, 2f, self, new Color(0.8f, 0.8f, 1f)));
                                }
                                self.Stun(200);
                                self.room.AddObject(new CreatureSpasmer(self, false, 200));
                                return;
                            }
                            else if ((abstractPhysicalObject as AbstractSpear).explosive && (abstractPhysicalObject as AbstractSpear).electricCharge > 0)
                            {
                                self.room.AddObject(new ShockWave(self.firstChunk.pos, 100f, 1f, 10));
                                self.room.AddObject(new SootMark(self.room, self.firstChunk.pos, 300f, false));
                                self.room.AddObject(new Spark(self.firstChunk.pos, Custom.RNV(), Color.white, null, 10, 17));
                                self.room.PlaySound(SoundID.Fire_Spear_Explode, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);

                                self.mainBodyChunk.vel.y = 6f;
                                self.Stun(80);
                                self.LoseAllGrasps();

                                return;
                            }
                            self.ReleaseGrasp(i);
                            abstractPhysicalObject.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                            self.SubtractFood(1);
                            AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), true);//change to goo spear
                            self.room.abstractRoom.AddEntity(abstractSpear);
                            abstractSpear.RealizeInRoom();
                            if (self.FreeHand() != -1)
                            {
                                self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                            }
                            self.room.AddObject(new WaterDrip(self.firstChunk.pos, new Vector2(0f, -5f), Random.value < 0.5f));
                            self.room.PlaySound(SoundID.Slugcat_Regurgitate_Item, self.firstChunk);
                            return;
                        }
                    }
                }
                return;
            }
            orig(self);
        }

        public static void IL_GrabUpdate(ILContext il)
        {
            try
            {
                ILCursor c = new(il);
                ILLabel label = il.DefineLabel();

                if (!c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Player>("FreeHand"), i => i.MatchLdcI4(-1), i => i.Match(OpCodes.Beq_S)))
                {
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Player, bool>>(self =>
                {
                    return self.IsLampScug();
                });
                c.Emit(OpCodes.Brtrue_S, label);

                c.GotoNext(MoveType.Before, i => i.MatchLdarg(0), i => i.MatchCallOrCallvirt<Player>("GraspsCanBeCrafted"));
                c.MarkLabel(label);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void TerrainImpact(On.Player.orig_TerrainImpact orig, Player self, int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            if (self.IsLampScug())
            {
                if (speed > 33.5f)
                {
                    self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Death, self.mainBodyChunk);
                    Debug.Log("Fall damage death");
                    self.Die();
                }
                else if (speed > 16f)
                {
                    self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                    //self.Stun((int)Custom.LerpMap(speed, 30f, 40f, 40f, 150f, 2.5f));
                    self.Stun(Mathf.Max(60, Mathf.RoundToInt(speed * 2.5f)));
                }
            }
            orig.Invoke(self, chunk, direction, speed, firstContact);
        }

        public static bool SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject slamming)
        {
            if (self.IsLampScug())
            {
                if ((slamming as Creature).abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                {
                    return false;
                }
                if (self.gourmandAttackNegateTime > 0)
                {
                    return false;
                }
                if (self.gravity == 0f)
                {
                    return false;
                }
                if (self.cantBeGrabbedCounter > 0)
                {
                    return false;
                }
                if (self.forceSleepCounter > 0)
                {
                    return false;
                }
                if (self.timeSinceInCorridorMode < 5)
                {
                    return false;
                }
                if (self.submerged)
                {
                    return false;
                }
                if (self.enteringShortCut != null || (self.animation != Player.AnimationIndex.BellySlide && self.canJump >= 5))
                {
                    return false;
                }
                if (self.animation == Player.AnimationIndex.CorridorTurn || self.animation == Player.AnimationIndex.CrawlTurn || self.animation == Player.AnimationIndex.ZeroGSwim || self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.animation == Player.AnimationIndex.GetUpOnBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.BeamTip)
                {
                    return false;
                }
                Vector2 vel = self.bodyChunks[0].vel;
                if (self.bodyChunks[1].vel.magnitude < vel.magnitude)
                {
                    vel = self.bodyChunks[1].vel;
                }
                Creature creature = slamming as Creature;
                foreach (Creature.Grasp grasp in self.grabbedBy)
                {
                    if (grasp.pacifying || grasp.grabber == creature)
                    {
                        return false;
                    }
                }
                return !ModManager.CoopAvailable || !(slamming is Player) || Custom.rainWorld.options.friendlyFire;
            }
            else return orig(self, slamming);
        }
    }
}
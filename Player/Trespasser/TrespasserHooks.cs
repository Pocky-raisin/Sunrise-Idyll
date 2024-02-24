using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Sunrise;
using RWCustom;
using MonoMod.Cil;
using MonoMod;
using MoreSlugcats;


namespace Sunrise
{
    public static class TrespasserHooks
    {
        public static void ApplyHooks()
        {
            On.Player.MovementUpdate += MovementUpdate;
            On.Player.UpdateBodyMode += UpdateBodyMode;
            On.Player.ThrowObject += ThrowObject;
            //On.Player.Update += Update;

        }
        
        public static bool IsTrespasser(this Player pl)
        {
            return pl.SlugCatClass.value == "GlideScug";
        }

        public static void ThrowObject(On.Player.orig_ThrowObject orig, Player self, int g, bool e)
        {
            if (self.IsTrespasser())
            {
                self.ReleaseGrasp(g);
                return;
            }
            orig(self, g, e);
        }

        private static void UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);

            //trying to prepare wallclimbing

            bool isTresspasser = self.TryGetTrespasser(out var data);

            if (isTresspasser)
            {
                if (self.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    data.WallClimbing = true;

                    if (self.input[0].jmp && data.WallClimbing)
                    {
                        data.WallClimbing = false;
                    }
                }
            }
        }

        private static void IL_SlugcatHand_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

        }

        private static void MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            bool isGliderscug = self.TryGetTrespasser(out TrespasserModule.TrespasserData data);
            if (!isGliderscug)
            {
                orig(self, eu);
                return;
            }

            const float normalGravity = 0.9f;
            const float boostPower = 3;

            //Initiate double jump 
            if (data.triggerGlide && self.bodyMode != Player.BodyModeIndex.ZeroG && !data.holdingBigItem)
            {
                data.CanInitGlide = false;
                self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk.pos, 1f, 1f);

                foreach (BodyChunk chunk in self.bodyChunks)
                {
                    chunk.vel.y = boostPower;
                }
                //init glide
                data.CanGlide = true;
                data.Gliding = true;
                data.GlideSpeed = 0;
            }

            //runs when gliding
            if (data.holdingGlide && data.CanGlide && data.Gliding && self.mainBodyChunk.vel.y < 0f && !data.touchingTerrain)
            {
                //glide physics
                self.slugOnBack.interactionLocked = true;
                self.eatCounter = 0;
                self.wantToThrow = 0;
                if (self.spearOnBack != null)
                {
                    self.spearOnBack.interactionLocked = true;
                }
                self.swallowAndRegurgitateCounter = 0;
                self.noGrabCounter = 5;
                self.standing = false;
                self.WANTTOSTAND = false;

                

                if (self.slugOnBack != null && self.slugOnBack.slugcat != null && self.slugOnBack.HasASlug)
                {
                    foreach (BodyChunk chunk in self.slugOnBack.slugcat.bodyChunks)
                    {
                        chunk.vel.y = Custom.LerpAndTick(chunk.vel.y, -0.5f, 0.2f, 0.5f);
                    }
                }
                if (self.input[0].x == 0 || self.input[0].y == -1)
                {
                    self.bodyMode = Player.BodyModeIndex.Crawl;
                }
                else
                {
                    self.animation = Player.AnimationIndex.RocketJump;
                }
                foreach (BodyChunk chunk in self.bodyChunks)
                {

                    if (self.input[0].x == 0 || self.input[0].y == -1)
                    {

                    }
                    else
                    {
                        chunk.vel.y = Custom.LerpAndTick(chunk.vel.y, -0.5f, 0.2f, 0.5f);
                    }
                    //Stop velocity when turning
                    if (self.input[0].y + self.input[0].x != 0)
                    {
                        if (self.flipDirection != data.GlideDirection)
                        {
                            self.animation = Player.AnimationIndex.CrawlTurn;
                            data.GlideSpeed = 0;
                            chunk.vel.x = 0;
                            data.GlideDirection = self.flipDirection;
                        }
                        else
                        //Increase speed
                        {
                            if (data.GlideSpeed > 50)
                            {
                                data.GlideSpeed = 50;
                            }
                            data.GlideSpeed++;
                        }
                        //Implement speed increase
                        if (data.CanGlide && Mathf.Abs(chunk.vel.x) < 20)
                        {
                            chunk.vel.x = Mathf.Lerp(chunk.vel.x, chunk.vel.x + (data.GlideSpeed / 20 * self.flipDirection), 0.2f);
                        }
                        else
                        {
                            chunk.vel.x = 20 * self.flipDirection;
                        }
                    }
                    else
                    {
                        //Slow down if not moving forwards
                        chunk.vel.x = Mathf.Lerp(chunk.vel.x, 0, 0.2f);
                        self.animation = Player.AnimationIndex.None;
                        //implement diving
                    }

                }
            }

            //Recharge the glide when on a wall, floor or pole
            if (data.rechargeGlide && self.room.gravity != 0)
            {
                self.gravity = normalGravity;
                self.customPlayerGravity = normalGravity;
                data.CanInitGlide = true;
                data.Gliding = false;
                data.CanGlide = true;
                data.GlideSpeed = 0;
            }

            if (data.GlideCooldown > 0)
            {
                data.GlideCooldown--;
            }
            //Cooldown for how soon gliding can begin after jumping
            if (data.GlideCooldown < 5 && data.rechargeGlide && self.bodyMode == Player.BodyModeIndex.ZeroG && self.animation == Player.AnimationIndex.ZeroGSwim)
            {
                data.GlideCooldown = 5;
            }

            //Logs
            //Debug.Log("Glide Cooldown" + data.glideCooldown);
            orig(self, eu);
        }

    }
}
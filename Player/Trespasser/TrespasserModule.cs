using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using static MonoMod.InlineRT.MonoModRule;


namespace SunriseIdyll
{
    public static class TrespasserModule
    {

        public static TrespasserData GetTresspasserData(this Player self)
        {
            return TrespasserCWT.GetValue(self, (Player _) => new TrespasserData(self));
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000041C4 File Offset: 0x000023C4
        public static bool TryGetTrespasser(this Player self, out TrespasserData corrosiveData)
        {
            bool flag = self.IsTrespasser();
            bool result;
            if (flag)
            {
                corrosiveData = self.GetTresspasserData();
                result = true;
            }
            else
            {
                corrosiveData = null;
                result = false;
            }
            return result;
        }

        private static readonly ConditionalWeakTable<Player, TrespasserData> TrespasserCWT = new ConditionalWeakTable<Player, TrespasserData>();
        public class TrespasserData
        {
            public TrespasserData(Player pl)
            {
                GlideCooldown = 0;
                GlideDirection = 0;
                GlideSpeed = 0;
                Gliding = false;
                CanGlide = false;
                HeavyCarrying = false;
                CanInitGlide = false;
                self = pl;
            }

            public Player self;

            public int GlideCooldown;
            public int GlideDirection;
            public int GlideSpeed;
            public bool Gliding;
            public bool CanGlide;
            public bool HeavyCarrying;
            public bool CanInitGlide;

            public bool WallClimbing;
            public bool CeilingClimbing;
            public int WallClimbDuration;
            public int WallClimbDir;
            public int CeilingClimbDir;

            public int NoFakeDeadCounter;
            public bool FakeDead;
            public int FakeDeadDelay;
            public int FakeDeadCounter;

            public int earsprite;

            public bool holdingGlide
            {
                get
                {
                    return self.room != null && self.input[0].jmp && self.canJump == 0 && self.canWallJump == 0;
                }
            }

            public bool holdingBigItem
            {
                get
                {
                    return self != null && self.grasps[0]?.grabbed is TubeWorm || self.Grabability(self.grasps[0]?.grabbed) is Player.ObjectGrabability.TwoHands || self.Grabability(self.grasps[1]?.grabbed) is Player.ObjectGrabability.TwoHands;
                }
            }

            public bool rechargeGlide
            {
                get
                {
                    string[] bodyModes = { "Stand", "CorridorClimb", "WallClimb", "Swimming", "ClimbingOnBeam" };
                    for (int i = 0; i < bodyModes.Length; i++)
                    {
                        Player.BodyModeIndex bodyRef = new Player.BodyModeIndex(bodyModes[i]);
                        if (self.bodyMode == bodyRef)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            public bool touchingTerrain
            {
                get
                {
                    return self != null && (self.bodyChunks[0].contactPoint != default || self.bodyChunks[1].contactPoint != default || self.bodyMode != Player.BodyModeIndex.Default || self.animation == Player.AnimationIndex.Flip || rechargeGlide);
                }
            }
            public bool triggerGlide
            {
                get
                {
                    return self != null && GlideCooldown <= 0 && !touchingTerrain && CanInitGlide && self.input[0].jmp && !self.input[1].jmp;
                }
            }
        }
    }
}
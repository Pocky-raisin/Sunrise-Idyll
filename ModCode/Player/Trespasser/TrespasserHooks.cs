using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using RWCustom;
using MonoMod.Cil;
using MonoMod;
using MoreSlugcats;
using System.Linq;
using SlugBase.SaveData;


namespace SunriseIdyll
{
    public static class TrespasserHooks
    {
        public static void ApplyHooks()
        {
            On.Player.MovementUpdate += MovementUpdate;
            On.Player.UpdateBodyMode += UpdateBodyMode;
            On.Player.ThrowObject += ThrowObject;
            On.Player.Update += Update;
            //On.Player.UpdateMSC += checkIfEspecialKarmaSituation;
            //On.Player.Die += resetMaxKarmaTrespasser;
            //On.RegionGate.ctor += trespasserGate;
            //the below two don't work and/or cause exceptions
            //On.HUD.KarmaMeter.ctor += karmaMeterCWTEnabler;
            //On.HUD.KarmaMeter.UpdateGraphic += falseUpdateGraphics;
        }
        
        public static bool IsTrespasser(this Player pl)
        {
            return pl.SlugCatClass.value == "IDYLL.GlideScug";
        }

        public static readonly SlugcatStats.Name TrespasserName = new SlugcatStats.Name("IDYLL.GlideScug", false);

        public static SlugBaseSaveData trespasserSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(TrespasserName));

        public static Dictionary<string, bool> foundTokens = new Dictionary<string, bool>();
        public static ConditionalWeakTable<HUD.KarmaMeter, KarmaMeterCWTClass> meterCWT = new();


        public static void ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            PhysicalObject grabbedObj = self.grasps[grasp].grabbed;
            orig(self, grasp, eu);
            if (self.IsTrespasser())
            {
                if (grabbedObj is Weapon && grabbedObj is not Rock)
                {
                    (grabbedObj as Weapon).doNotTumbleAtLowSpeed = true;
                }
                grabbedObj.firstChunk.vel *= 0.2f;
            }
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

        //karma section start
        private static void checkIfEspecialKarmaSituation(On.Player.orig_UpdateMSC orig, Player self)
        {
            trespasserSaveData.TryGet<bool>("karmaSpecial", out bool flag);
            if (flag && self.IsTrespasser())
            {
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap = 1;
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karma = 1;
                (self.playerState as PlayerNPCState).KarmaLevel = 1;
            }
            orig(self);
        }

        private static void trespasserGate(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {
            orig(self, room);
            if (room.world.game.IsStorySession && room.world.game.StoryCharacter == TrespasserName && !self.unlocked)
            {
                self.karmaRequirements[0] = RegionGate.GateRequirement.TwoKarma;
                self.karmaRequirements[1] = RegionGate.GateRequirement.TwoKarma;
                for(int i = 0; i < 2; i++)
                {
                    room.RemoveObject(self.karmaGlyphs[i]);
                    self.karmaGlyphs[i] = new GateKarmaGlyph(i == 1, self, SunriseEnums.RegionGateReq.Nomad);
                    room.AddObject(self.karmaGlyphs[i]);
                }
            }
        }

        private static void resetMaxKarmaTrespasser(On.Player.orig_Die orig, Player self)
        {
            if (!self.KarmaIsReinforced && self.IsTrespasser() && self.room.world.game.IsStorySession)
            {
                trespasserSaveData.Set<bool>("karmaSpecial", false);
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap = 0;
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karma = 0;
            }
            orig(self);
        }

        private static void karmaMeterCWTEnabler(On.HUD.KarmaMeter.orig_ctor orig, HUD.KarmaMeter self, HUD.HUD hud, FContainer fContainer, IntVector2 displayKarma, bool showAsReinforced) //this one causes the crash it seems
        {
            try
            {
                self.displayKarma = displayKarma;
                self.showAsReinforced = showAsReinforced;
                displayKarma.x = RWCustom.Custom.IntClamp(displayKarma.x, 0, displayKarma.y);
                self.pos = new Vector2(Mathf.Max(55.01f, hud.rainWorld.options.SafeScreenOffset.x + 22.51f), Mathf.Max(45.01f, hud.rainWorld.options.SafeScreenOffset.y + 22.51f));
                self.lastPos = self.pos;
                self.rad = 22.5f;
                self.lastRad = self.rad;
                self.darkFade = new FSprite("Futile_White", true);
                self.darkFade.shader = hud.rainWorld.Shaders["FlatLight"];
                self.darkFade.color = new Color(0f, 0f, 0f);
                fContainer.AddChild(self.darkFade);

                if (!meterCWT.TryGetValue(self, out KarmaMeterCWTClass key))
                {
                    KarmaMeterCWTClass meter = new(self);
                    if (meter.isTress)
                    {
                        if (meter.isNomad)
                        {
                            self.karmaSprite = new FSprite("smallKarma2");
                        }
                        else
                        {
                            self.karmaSprite = new FSprite("smallKarma4");
                        }
                        meterCWT.Add(self, meter);
                    }
                    else
                    {
                        self.karmaSprite = new FSprite(KarmaMeter.KarmaSymbolSprite(true, displayKarma), true);
                    }
                }
                else if (meterCWT.TryGetValue(self, out KarmaMeterCWTClass meter))
                {
                    if (meter.isTress)
                    {
                        if (meter.isNomad)
                        {
                            self.karmaSprite = new FSprite("smallKarma2");
                        }
                        else
                        {
                            self.karmaSprite = new FSprite("smallKarma4");
                        }
                    }
                    else
                    {
                        self.karmaSprite = new FSprite(KarmaMeter.KarmaSymbolSprite(true, displayKarma), true);
                    }

                }
                self.karmaSprite.color = new Color(1f, 1f, 1f);
                fContainer.AddChild(self.karmaSprite);
                self.glowSprite = new FSprite("Futile_White", true);
                self.glowSprite.shader = hud.rainWorld.Shaders["FlatLight"];
                fContainer.AddChild(self.glowSprite);
                if (ModManager.MSC)
                {
                    if (self.hud.owner is Player && (self.hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                    {
                        self.notSleptWith = !showAsReinforced;
                        return;
                    }
                    self.notSleptWith = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                orig(self, hud, fContainer, displayKarma, showAsReinforced);
            }
            
        }

        private static void falseUpdateGraphics(On.HUD.KarmaMeter.orig_UpdateGraphic orig, HUD.KarmaMeter self)
        {
            try
            {
                if (self.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && meterCWT.TryGetValue(self, out KarmaMeterCWTClass meter) && meter.isTress)
                {
                    self.displayKarma.x = (self.hud.owner as Player).Karma;
                    self.displayKarma.y = 1;
                    if (meter.isNomad && self.displayKarma.x == 1)
                    {
                        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("smallKarma2");
                    }
                    else
                    {
                        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("smallKarma4");
                    }
                }
                else
                {
                    orig(self);
                }
            }
            catch(Exception e)
            {
                orig(self);
                Debug.LogException(e);
            }
            
        }

        //karma section end

        private static void Update(On.Player.orig_Update orig, Player self, bool e)
        {
            orig(self, e);

            if (self.TryGetTrespasser(out var data))
            {
                if (!self.Consious || self.lungsExhausted || self.dead || self.exhausted || self.room == null)
                {
                    data.NoFakeDeadCounter = 80;
                }
            }
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

            //Initiate double jump 
            if (data.triggerGlide && self.bodyMode != Player.BodyModeIndex.ZeroG && !data.holdingBigItem)
            {
                data.CanInitGlide = false;
                self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk.pos, 1f, 1f);

                //init glide
                data.CanGlide = true;
                data.Gliding = true;

                int speed = 0;

                if (self.input[0].x != 0)
                {
                    speed = 6;
                }

                data.GlideSpeed = speed;
            }

            //runs when gliding
            if (data.holdingGlide && data.CanGlide && data.Gliding && self.mainBodyChunk.vel.y < 0f && !data.touchingTerrain)
            {
                //glide physics
                self.slugOnBack.interactionLocked = true;
                self.eatCounter = 0;
                if (self.spearOnBack != null)
                {
                    self.spearOnBack.interactionLocked = true;
                }
                self.swallowAndRegurgitateCounter = 0;
                self.noGrabCounter = 5;
                self.standing = false;
                self.WANTTOSTAND = false;

                if (self.graphicsModule as PlayerGraphics != null && self.graphicsModule is PlayerGraphics pg)
                {
                    if (self.input[0].y > 0)
                    {
                        pg.head.vel.y += 1f;
                        pg.lookDirection.y += 0.5f;
                    }
                    pg.head.vel.x += 1f * self.flipDirection;
                    pg.lookDirection.x += 0.5f * self.flipDirection;
                }

                if (self.slugOnBack != null && self.slugOnBack.slugcat != null && self.slugOnBack.HasASlug)
                {
                    self.slugOnBack.DropSlug();
                }

                self.animation = Player.AnimationIndex.RocketJump;

                foreach (BodyChunk chunk in self.bodyChunks)
                {
                    /*
                    if (self.input[0].x == 0 || self.input[0].y == -1)
                    {

                    }
                    else
                    {
                        
                    }
                    //Stop velocity when turning
                    if (self.input[0].x != 0)
                    {

                    }
                    else
                    {
                        //Slow down if not moving forwards
                        chunk.vel.x = Mathf.Lerp(chunk.vel.x, 0, 0.2f);
                        self.animation = Player.AnimationIndex.None;
                        //implement diving
                    }
                    */



                    if (self.flipDirection != data.GlideDirection)
                    {
                        self.animation = Player.AnimationIndex.CrawlTurn;
                        data.GlideSpeed = 3;
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
                        if (data.GlideSpeed < 3)
                        {
                            data.GlideSpeed = 3;
                        }
                        if (self.input[0].x != 0)
                        {
                            data.GlideSpeed++;
                        }
                        else data.GlideSpeed-= self.input[0].y == -1? 8 : 2;
                    }

                    float uhh = 0.8f;
                    if (self.input[0].y < 0) uhh = 1.4f;


                    if (data.GlideSpeed <= 20)
                    {
                        float negative = Mathf.InverseLerp(0.5f, uhh, data.GlideSpeed / 20);
                        chunk.vel.y -= negative;
                    }
                    else chunk.vel.y = -0.5f;

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
            if (data.GlideCooldown < 10 && data.rechargeGlide && self.bodyMode == Player.BodyModeIndex.ZeroG && self.animation == Player.AnimationIndex.ZeroGSwim)
            {
                data.GlideCooldown = 10;
            }

            //Logs
            //Debug.Log("Glide Cooldown" + data.glideCooldown);
            orig(self, eu);


            if (data.NoFakeDeadCounter > 0) data.NoFakeDeadCounter--;

            if (!self.input[0].AnyDirectionalInput && self.input[0].thrw && data.NoFakeDeadCounter <= 0 && (!self.standing || self.bodyMode == Player.BodyModeIndex.CorridorClimb) && self.bodyChunks[0].ContactPoint.y < 0)
            {
                data.FakeDeadDelay++;
                self.Blink(5);

                if (data.FakeDeadDelay > 40)
                {
                    data.FakeDead = true;
                }
            }
            else
            {
                data.FakeDead = false;
                data.FakeDeadDelay = 0;
            }

            if (data.FakeDead)
            {
                data.FakeDeadCounter++;
                self.animation = Player.AnimationIndex.None;
                self.bodyMode = Player.BodyModeIndex.Stunned;
                self.LoseAllGrasps();
                self.noGrabCounter = 5;
                self.swallowAndRegurgitateCounter = 0;
                self.standing = false;
                self.feetStuckPos = null;

                //creature AI modification

                var AIlist = self.room.updateList.OfType<Creature>().Select(x => x.abstractCreature.abstractAI?.RealAI).Where(x => x is not null);

                foreach (var AI in AIlist)
                {
                    AI.discomfortTracker?.tracker?.ForgetCreature(self.abstractCreature);
                    AI.threatTracker?.RemoveThreatCreature(self.abstractCreature);
                    AI.preyTracker?.ForgetPrey(self.abstractCreature);
                    AI.agressionTracker?.ForgetCreature(self.abstractCreature);
                }

                if (data.FakeDeadCounter > 300)
                {
                    data.FakeDead = false;
                    data.NoFakeDeadCounter = 180;
                    data.FakeDeadDelay = 0;
                    data.FakeDeadCounter = 0;
                    self.airInLungs = 0f;

                    self.lungsExhausted = true;
                }
            }
        }
    }

    public class KarmaMeterCWTClass
    {
        public bool isTress;
        public bool isNomad;
        public KarmaMeterCWTClass(HUD.KarmaMeter meter)
        {
            isTress = meter.hud.owner is Player && (meter.hud.owner as Player).IsTrespasser();
            isNomad = meter.hud.owner is Player && (meter.hud.owner as Player).Karma >= 1;
        }
    }
}
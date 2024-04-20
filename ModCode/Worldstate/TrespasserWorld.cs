using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;


namespace SunriseIdyll
{
    public static class TrespasserWorld
    {
        public static void ApplyHooks()
        {
            On.Player.UpdateMSC += checkIfEspecialKarmaSituation;
            On.KarmaFlower.Update += KarmaFlower_Update;
            On.Player.Die += resetMaxKarmaTrespasser;
            On.RegionGate.ctor += trespasserGate;
            On.HUD.KarmaMeter.ctor += karmaMeterCWTEnabler;
            On.HUD.KarmaMeter.UpdateGraphic += falseUpdateGraphics;
            On.RegionGate.Update += RegionGate_Update;
            On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
            On.RainWorldGame.Win += RainWorldGame_Win;
            On.HUD.KarmaMeter.Draw += KarmaMeter_Draw;
        }

        private static void KarmaFlower_Update(On.KarmaFlower.orig_Update orig, KarmaFlower self, bool eu)
        {
            orig(self, eu);

            if (self.room.game != null && self.room.game.StoryCharacter == TrespasserHooks.TrespasserName) self.Destroy();
        }

        private static bool ShelterKillTrespasser = false;
        public static Dictionary<string, bool> foundTokens = new Dictionary<string, bool>();
        public static ConditionalWeakTable<HUD.KarmaMeter, KarmaMeterCWTClass> meterCWT = new();


        private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
        {
            orig(self);
            if (self.room.game.IsStorySession && self.room.game.StoryCharacter == TrespasserHooks.TrespasserName)
            {
                TrespasserHooks.trespasserSaveData.TryGet<string>("fatalShelter", out string sheltername);

                if (sheltername == self.room.abstractRoom.name)
                {
                    ShelterKillTrespasser = true;
                }
                else
                {
                    ShelterKillTrespasser = false;
                    TrespasserHooks.trespasserSaveData.Set<string>("fatalShelter", self.room.abstractRoom.name);
                }
            }
        }

        private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
        {
            if (ShelterKillTrespasser)
            {
                self.GoToDeathScreen();
                return;
            }
            orig(self, malnourished);
        }

        private static void checkIfEspecialKarmaSituation(On.Player.orig_UpdateMSC orig, Player self)
        {
            TrespasserHooks.trespasserSaveData.TryGet<bool>("karmaSpecial", out bool flag);
            if (flag && self.IsTrespasser())
            {
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap = 1;
                self.room.world.game.GetStorySession.saveState.deathPersistentSaveData.karma = 1;
                (self.playerState as PlayerNPCState).KarmaLevel = 1;
            }
            orig(self);
        }

        private static void KarmaMeter_Draw(On.HUD.KarmaMeter.orig_Draw orig, KarmaMeter self, float timeStacker)
        {
            orig(self, timeStacker);

            if (self.hud.owner != null && self.hud.owner is Player player && player.room != null)
            {
                if (player.room.game.IsStorySession && player.room.game.StoryCharacter == TrespasserHooks.TrespasserName)
                {
                    TrespasserHooks.trespasserSaveData.TryGet<bool>("karmaSpecial", out bool flag);
                    string karmasprite;

                    if (flag)
                    {
                        karmasprite = "smallKarmaNomad";
                    }
                    else
                    {
                        karmasprite = "smallKarmaEmpty";
                    }

                    self.karmaSprite.element = Futile.atlasManager.GetElementWithName(karmasprite);
                }
            }
        }


        private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate self, bool eu)
        {
            var mode = self.mode;

            orig(self, eu);

            if (mode != self.mode && self.mode == RegionGate.Mode.ClosingAirLock && self.room.game != null && self.room.game.IsStorySession && self.room.game.StoryCharacter == TrespasserHooks.TrespasserName)
            {
                TrespasserHooks.trespasserSaveData.Set<bool>("karmaSpecial", false);
                self.room.PlaySound(SoundID.HUD_Karma_Reinforce_Bump, new Vector2(50f, 50f));
            }
        }

        private static void trespasserGate(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {
            orig(self, room);
            if (room.world.game.IsStorySession && room.world.game.StoryCharacter == TrespasserHooks.TrespasserName && !self.unlocked)
            {
                self.karmaRequirements[0] = RegionGate.GateRequirement.TwoKarma;
                self.karmaRequirements[1] = RegionGate.GateRequirement.TwoKarma;
                for (int i = 0; i < 2; i++)
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
                TrespasserHooks.trespasserSaveData.Set<bool>("karmaSpecial", false);
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
            catch (Exception e)
            {
                orig(self);
                Debug.LogException(e);
            }

        }
    }
}
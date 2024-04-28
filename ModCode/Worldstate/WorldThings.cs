using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using static MonoMod.InlineRT.MonoModRule;

namespace SunriseIdyll
{
    public static class WorldThings
    {
        public static void ApplyHooks()
        {
            On.Spear.HitSomething += SpearsFireDamage;
            On.Spear.Update += doDamageOverTime;
            On.Creature.Violence += creatureFireDamageCheck;
            /*
            BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

            new Hook(
            typeof(Lantern)
            .GetProperty(nameof(Lantern.ge), flags)
            .GetGetMethod(),
            MainMechanicsCloudtail.IsGourmandHook);
            */
            
        }
        public static Creature.DamageType Fire = new Creature.DamageType("Fire", true); //new damage type
        public static Dictionary<Spear, int> FireSpearDamageTracker = new Dictionary<Spear, int>();

        public static void addSpearToDict(On.Spear.orig_ctor orig, Spear self, AbstractPhysicalObject obj, World world)
        {
            orig(self, obj, world);
            if (self.bugSpear)
            {
                FireSpearDamageTracker.Add(self, 40);
            }
        }

        public static void doDamageOverTime(On.Spear.orig_Update orig, Spear self,  bool eu)
        {
            orig(self, eu);
            
            if (self.bugSpear && self.stuckInChunk != null)
            {
                if(self.stuckInChunk.owner is Creature)
                {
                    if (FireSpearDamageTracker.TryGetValue(self, out int timer) && timer == 0f)
                    {
                        (self.stuckInChunk.owner as Creature).takeFireDamage(0.1f, 0f, self.bodyChunks[0]);
                        self.abstractSpear.hue -= 0.1f;
                        if (self.abstractSpear.hue < 0.1f && self.abstractSpear.hue > 0f)
                        {
                            self.abstractSpear.hue = 0f;
                        }
                        FireSpearDamageTracker[self] = 40;
                    }
                    else
                    {
                        FireSpearDamageTracker[self]--;
                    }
                    
                }
            }
        }

        public static bool SpearsFireDamage(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu) //makes bug spears do fire damage
        {
            float num = self.spearDamageBonus;
            if(result.obj == null || !self.abstractPhysicalObject.world.game.SunriseWorld()) //only works in the above campaigns
            {
                return orig(self, result, eu);
            }
            if(result.obj is Creature creature && self.bugSpear) //runs the usual code that's run with a bug spear
            {
                if (ModManager.MSC && result.obj is Player pl && pl.isGourmand && UnityEngine.Random.value < 0.15f)
                {
                    num /= 10f;
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log("GOURMAND SAVE!");
                    }
                }
                if (ModManager.MSC && result.obj is Player player)
                {
                    player.playerState.permanentDamageTracking += (double)(num / player.Template.baseDamageResistance);
                    if (player.playerState.permanentDamageTracking >= 1.0)
                    {
                        player.Die();
                    }
                }
                
                creature.Violence(self.firstChunk, new Vector2?(self.firstChunk.vel * self.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Fire, num, 20f);

                if (creature.SpearStick(self, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, self.firstChunk.vel))
                {
                    self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.firstChunk);
                    self.LodgeInCreature(result, eu);
                    return true;
                }
                else
                {
                    self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.firstChunk);
                    self.vibrate = 20;
                    self.ChangeMode(Weapon.Mode.Free);
                    self.firstChunk.vel = self.firstChunk.vel * -0.5f + RWCustom.Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * self.firstChunk.vel.magnitude;
                    self.SetRandomSpin();
                    return false;
                }
            }
            else
            {
                return orig(self, result, eu); //runs the normal code if the new code isn't run
            }
            
        }

        public static void creatureFireDamageCheck(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) //sets the fire resistance for all creatures
        {
            if(type == Fire)
            {
                if (!(source.owner is Spear))
                {
                    self.takeFireDamage(damage, stunBonus, source);
                    return;
                }
                damage = self.fireSpearDamageMultiplier(damage);
                stunBonus *= 2;
                if(self is MirosBird || (self is Vulture && (self as Vulture).IsMiros))
                {
                    stunBonus *= 3;
                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus); //calls orig with the newly updated damage value
        }

        public static float fireSpearDamageMultiplier(this Creature self, float damage)
        {
            if (self.abstractCreature.lavaImmune || (self is EggBug && (self as EggBug).FireBug)) //affects creatures that don't take damage from acid water
            {
                damage *= 0f;
            }
            else if (self.abstractCreature.Winterized || self is Centipede) //affects creatures that are cold-adapted
            {
                damage *= 6f;
            }
            else if (self is BigSpider) //affects spiders
            {
                (self as BigSpider).spewBabies = true; //makes it so the game thinks the spider has already laid its brood, in case the spider is a mother spider
                damage *= 3.6f;
            }
            else if (self is DropBug || (self is EggBug && !(self as EggBug).FireBug) || self is NeedleWorm || self is Snail || self is Cicada || self is Fly || self is Spider) //affects all varieties of insects
            {
                damage *= 3.6f;
            }
            else if ((self is Vulture || self is VultureGrub) && !(self as Vulture).IsMiros) //affects non-miros vultures
            {
                damage *= 2.7f;
            }
            else if (self is MirosBird || (self is Vulture && (self as Vulture).IsMiros)) //affects miros vultures and birds
            {
                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Volt_Shock, self.firstChunk.pos, 2f, UnityEngine.Random.Range(0.8f, 1.2f)); //plays the volt_shock sound at double volume on hit 
                damage *= 9f;
            }
            else //affects all un-listed creatures
            {
                damage *= 3f;
            }
            return damage;
        }

        public static void takeFireDamage(this Creature self, float damage, float stunBonus, BodyChunk source)
        {
            if (source != null && source.owner is Creature)
            {
                self.SetKillTag((source.owner as Creature).abstractCreature);
            }
            if (self.abstractCreature.lavaImmune || (self is EggBug && (self as EggBug).FireBug)) //affects creatures that don't take damage from acid water
            {
                damage *= 0f;
            }
            else if (self.abstractCreature.Winterized || self is Centipede) //affects creatures that are cold-adapted
            {
                damage *= 6f;
            }
            else if (self is BigSpider) //affects spiders
            {
                (self as BigSpider).spewBabies = true; //makes it so the game thinks the spider has already laid its brood, in case the spider is a mother spider
                damage *= 3.6f;
            }
            else if (self is DropBug || (self is EggBug && !(self as EggBug).FireBug) || self is NeedleWorm || self is Snail || self is Cicada || self is Fly || self is Spider) //affects all varieties of insects
            {
                damage *= 3.6f;
            }
            else if ((self is Vulture || self is VultureGrub) && !(self as Vulture).IsMiros) //affects non-miros vultures
            {
                damage *= 2.7f;
            }
            else if (self is MirosBird || (self is Vulture && (self as Vulture).IsMiros)) //affects miros vultures and birds
            {
                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Volt_Shock, self.firstChunk.pos, 2f, UnityEngine.Random.Range(0.8f, 1.2f)); //plays the volt_shock sound at double volume on hit 
                damage *= 9f;
                stunBonus *= 2f;
            }
            else //affects all un-listed creatures
            {
                damage *= 3f;
            }
            damage /= self.Template.baseDamageResistance;
            float stun = (damage * 30f + stunBonus) / self.Template.baseStunResistance;
            if (self.State is HealthState)
            {
                stun *= 1.5f + Mathf.InverseLerp(0.5f, 0f, (self.State as HealthState).health) * UnityEngine.Random.value;
            }
            if (self.room != null && self.room.game.IsArenaSession && self.room.world.game.GetArenaGameSession.chMeta != null && self.room.world.game.GetArenaGameSession.chMeta.resistMultiplier > 0f && !(self is Player))
            {
                damage /= self.room.world.game.GetArenaGameSession.chMeta.resistMultiplier;
            }
            if (self.room != null && self.room.game.IsArenaSession && self.room.world.game.GetArenaGameSession.chMeta != null && self.room.world.game.GetArenaGameSession.chMeta.invincibleCreatures && !(self is Player))
            {
                damage = 0f;
            }
            self.stunDamageType = Fire;
            self.Stun((int)stun);
            self.stunDamageType = Creature.DamageType.Water;
            if(self.State is HealthState)
            {
                (self.State as HealthState).health -= damage;
                if (self.Template.quickDeath && (UnityEngine.Random.value < -(self.State as HealthState).health || (self.State as HealthState).health < -1f || ((self.State as HealthState).health < 0f && UnityEngine.Random.value < 0.33f)))
                {
                    self.Die();
                }
            }
            if (damage >= self.Template.instantDeathDamageLimit)
            {
                self.Die();
            }
        }
    }
}
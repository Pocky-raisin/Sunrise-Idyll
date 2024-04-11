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
            On.Creature.Violence += CreatureFireResist;
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

        public static void CreatureFireResist(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) //sets the fire resistance for all creatures
        {
            if(type == Fire && self.abstractPhysicalObject.world.game.SunriseWorld())
            {
                //if-elif-else below determines how much damage is dealt
                if (self.abstractCreature.lavaImmune || (self is EggBug && (self as EggBug).FireBug)) //affects creatures that don't take damage from acid water
                {
                    damage *= 0f;
                }
                else if (self.abstractCreature.Winterized || self is Centipede) //affects creatures that are cold-adapted
                {
                    damage *= 6f;
                }
                else if(self is BigSpider) //affects spiders
                {
                    (self as BigSpider).spewBabies = true; //makes it so the game thinks the spider has already laid its brood, in case the spider is a mother spider
                    damage *= 3.6f;
                }
                else if(self is DropBug || (self is EggBug&& !(self as EggBug).FireBug) || self is NeedleWorm || self is Snail || self is Cicada || self is Fly) //affects all varieties of insects
                {
                    damage *= 3.6f;
                }
                else if((self is Vulture || self is VultureGrub) && !(self as Vulture).IsMiros) //affects non-miros vultures
                {
                    damage *= 2.7f;
                }
                else if(self is MirosBird || (self is Vulture && (self as Vulture).IsMiros)) //affects miros vultures and birds
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Volt_Shock, self.firstChunk.pos, 2f, UnityEngine.Random.Range(0.8f, 1.2f)); //plays the volt_shock sound at double volume on hit 
                    damage *= 9f;
                    stunBonus *= 2f;
                    if(hitAppendage != null && hitAppendage.appendage.appIndex > 0 && self is Vulture vul && vul.IsMiros)
                    {
                        damage *= 10f;
                    }
                }
                else //affects all un-listed creatures
                {
                    damage *= 3f;
                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus); //calls orig with the newly updated damage value
        }

        public static void uh()
        {

        }
    }
}
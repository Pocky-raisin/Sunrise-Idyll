using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using MonoMod.RuntimeDetour;
using System.Reflection;


namespace SunriseIdyll
{
    public static class WorldThings
    {
        public static void ApplyHooks()
        {
            On.Lantern.ApplyPalette += Lantern_ApplyPalette;

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

        public static bool LampWorldState(this RainWorldGame game)
        {
            return game.IsStorySession && game.StoryCharacter.value == "LampScug";
        }


        //Lantern cosmetic differences for Lamplighter
        public static void Lantern_ApplyPalette(On.Lantern.orig_ApplyPalette orig, Lantern self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.LampWorldState())
            {
                sLeaser.sprites[1].color = Color.Lerp(sLeaser.sprites[0].color, Color.black, 0.4f);
                sLeaser.sprites[0].color = palette.blackColor;
                sLeaser.sprites[2].color = Color.Lerp(sLeaser.sprites[2].color, Color.gray, 0.4f);
                sLeaser.sprites[3].color = Color.Lerp(sLeaser.sprites[3].color, Color.gray, 04f);
            }
        }
    }
}
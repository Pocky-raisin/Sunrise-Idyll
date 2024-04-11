global using BepInEx;
global using DevInterface;
global using HUD;
global using JollyCoop;
global using JollyCoop.JollyMenu;
global using LizardCosmetics;
global using Menu.Remix.MixedUI;
global using MonoMod.Cil;
global using MonoMod.RuntimeDetour;
global using MoreSlugcats;
global using On.Menu;
global using RWCustom;
global using SlugBase;
global using SlugBase.DataTypes;
global using SlugBase.Features;
global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Security.Permissions;
global using System.Text.RegularExpressions;
global using UnityEngine;
global using static System.Reflection.BindingFlags;
global using OpCodes = Mono.Cecil.Cil.OpCodes;
global using Color = UnityEngine.Color;
global using Random = UnityEngine.Random;
global using SunriseIdyll.Objects;
using Fisobs.Core;
namespace SunriseIdyll
{
    [BepInPlugin(MOD_ID, "Sunrise Idyll", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "idyllTeam.SunriseIdyll";

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            TrespasserHooks.ApplyHooks();
            LampGraphics.ApplyHooks();
            LampHooks.ApplyHooks();
            WorldStateHooks.ApplyHooks();
            FireCatsHooks.ApplyHooks();
            ChandlerHooks.ApplyHooks();
            ImperishableHooks.ApplyHooks();
            WorldThings.ApplyHooks();
            // Put your custom hooks here!

            Content.Register(new WarmSlimeFisob());

        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampArm");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampBody");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampHips");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampLegs");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampMask");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampPaw");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampSocks");
            Futile.atlasManager.LoadAtlas("atlases/LamplighterSprites/LampTail");

            Futile.atlasManager.LoadAtlas("atlases/TrespasserSprites/TresSprites");
            Futile.atlasManager.LoadAtlas("atlases/TrespasserSprites/TresHead");
            Futile.atlasManager.LoadAtlas("atlases/TrespasserSprites/TresEars");
            Futile.atlasManager.LoadAtlas("atlases/TrespasserSprites/TresFace");

            Futile.atlasManager.LoadAtlas("atlases/SlimeSprites");
        }



    }

}
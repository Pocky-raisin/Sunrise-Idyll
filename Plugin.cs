using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

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

        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/LampArm");
            Futile.atlasManager.LoadAtlas("atlases/LampHead");
            Futile.atlasManager.LoadAtlas("atlases/LampLegs");

            Futile.atlasManager.LoadAtlas("atlases/TresSprites");

            Futile.atlasManager.LoadAtlas("atlases/SlimeSprites");
        }



    }

}
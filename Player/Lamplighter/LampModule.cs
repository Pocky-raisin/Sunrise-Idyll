using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using static SunriseIdyll.TrespasserModule;
using Steamworks;


namespace SunriseIdyll
{
    public static class LampModule
    {
        private static readonly ConditionalWeakTable<Player, LampData> LampCWT = new ConditionalWeakTable<Player, LampData>();


        public static LampData GetLampData(this Player player)
        {
            return LampCWT.GetValue(player, (Player _) => new LampData(player));
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000041C4 File Offset: 0x000023C4
        public static bool TryGetLamp(this Player player, out LampData lampData)
        {
            bool flag = player.IsLampScug();
            bool result;
            if (flag)
            {
                lampData = player.GetLampData();
                result = true;
            }
            else
            {
                lampData = null;
                result = false;
            }
            return result;
        }

        public class LampData
        {
            public LampData(Player pl)
            {
                self = pl;
                DroolMeltCounter = 0;
            }

            //public int Capacity = 4;
            //public Stack<Player> Stack = new Stack<Player>();

            public bool Warm;
            public float ChillTimer;
            public int SoakCounter;

            public int MaxSoak
            {
                get
                {
                    return 3000;
                }
            }

            public Player self;
            public int DroolIndex;
            public int startsprite;
            public int endsprite;
            public int pawsprite1;
            public int pawsprite2;
            public int socksprite;
            public int masksprite;
            public bool GraphicsInit;
            public SlimeDrip drool;
            public int DroolMeltCounter;

            public Color Markingcol;
            public Color BrightCol;
            public Color BodyCol;
        }
    }
}
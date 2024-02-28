using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using RWCustom;


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
            public SlimeDrip drool;
            public int DroolMeltCounter;
            public Color LampCol;
            public Color Tailcol;
            public Color DroolCol;
            public Color BrightCol;
            public Color BodyCol;

            public void HypothermiaUpdate()
            {
                if (this.self.firstChunk.submersion > 0.25f)
                {
                    this.SoakCounter += 7;
                }

                this.DroolMeltCounter = Mathf.Min(100, this.SoakCounter);

                if (this.SoakCounter > 0)
                {
                    this.Warm = false;

                    this.self.Hypothermia += 0.0002f;

                    float shiver = Mathf.Min(2f, this.SoakCounter / 5);

                    if (!this.self.dead && this.self.graphicsModule != null)
                    {
                        (this.self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (shiver * 0.75f); // Head shivers
                        this.self.Blink(5);
                    }

                    if (this.self.Hypothermia > 0.8f)
                    {
                        this.self.Die();
                    }

                    this.SoakCounter--;

                }

                if (this.SoakCounter <= 0)
                {
                    this.Warm = true;
                }

                if (this.Warm)
                {
                    this.self.Hypothermia -= Mathf.Lerp(0.00025f, 0f, this.self.HypothermiaExposure);
                }
            }
        }
    }
}
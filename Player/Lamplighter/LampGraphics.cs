using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Sunrise;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;
using IL.Menu.Remix;


namespace Sunrise
{
    public static class LampGraphics
    {
        public static void ApplyHooks()
        {
            On.PlayerGraphics.ctor += PG_Ctor;
            On.PlayerGraphics.InitiateSprites += PG_Init;
            On.PlayerGraphics.AddToContainer += PG_Add;
            On.PlayerGraphics.Reset += PG_Reset;
            On.PlayerGraphics.Update += PG_Update;
            On.PlayerGraphics.DrawSprites += PG_Draw;

            On.Player.ShortCutColor += ShortCutColor;
        }

        private static void PG_Ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (!self.player.IsLampScug()) return;

            self.tail[0] = new TailSegment(self, 9f, 4f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 7f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 5f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 3f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }

        private static void PG_Init(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (self.player.TryGetLamp(out var data))
            {
                data.drool = new SlimeDrip(sLeaser, rCam, sLeaser.sprites.Length, 0.75f, self.player.mainBodyChunk, 9);
                data.DroolIndex = sLeaser.sprites.Length;
                System.Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                data.drool.GenerateMesh(sLeaser, rCam);
                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        public static Color ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            if (self.TryGetLamp(out var data))
            {
                return Color.Lerp(data.BrightCol, Color.gray, data.DroolMeltCounter / 100f);
            }
            return orig(self);
        }




        private static void PG_Add(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (self.player.TryGetLamp(out var data))
            {
                if (sLeaser.sprites.Length > data.DroolIndex && data.drool != null)
                {
                    if (newContatiner == null)
                    {
                        newContatiner = rCam.ReturnFContainer("Midground");
                    }

                    data.drool.AddContainerChild(sLeaser, newContatiner);
                    data.drool.MoveBehind(sLeaser, sLeaser.sprites[10]);
                }
            }
        }

        private static void PG_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            orig(self);
            if (self.player.TryGetLamp(out var data))
            {
                if (data.drool != null)data.drool.Reset();
            }
        }

        private static void PG_Draw(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.TryGetLamp(out var data))
            {
                data.BodyCol = sLeaser.sprites[0].color;
                data.BrightCol = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);

                data.Tailcol = Color.Lerp(data.BodyCol, data.BrightCol, 0.5f);

                void UpdateReplacement(int num, string tofind)
                {
                    if (!sLeaser.sprites[num].element.name.Contains("Lamp") && sLeaser.sprites[num].element.name.StartsWith(tofind)) sLeaser.sprites[num].SetElementByName("Lamp" + sLeaser.sprites[num].element.name);
                }



                if (data.drool != null)
                {
                    data.drool.Draw(sLeaser, timeStacker, camPos);
                    data.drool.SetColor(sLeaser, Color.Lerp(data.BrightCol, Color.gray, data.DroolMeltCounter / 100f));
                }

                UpdateReplacement(3, "HeadA");
                UpdateReplacement(4, "LegsA");
                UpdateReplacement(5, "PlayerArm");
                UpdateReplacement(6, "PlayerArm");

                float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * 3.1415927f * 2f);

                sLeaser.sprites[0].scaleX = 1.25f + self.player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * self.malnourished;
                sLeaser.sprites[1].scaleY = 1.1125f;
                sLeaser.sprites[1].scaleX = 1.325f + self.player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * self.malnourished;



                data.BodyCol = sLeaser.sprites[0].color;
                data.BrightCol = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);

                data.Tailcol = Color.Lerp(data.BodyCol, data.BrightCol, 0.5f);

                var effectcol = Color.Lerp(data.Tailcol, data.BodyCol, data.DroolMeltCounter / 100f);

                if (sLeaser.sprites[2] is TriangleMesh tailMesh)
                {
                    sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
                    if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
                    {
                        tailMesh.verticeColors = new Color[tailMesh.vertices.Length];
                    }
                    tailMesh.customColor = true;

                    var color2 = data.BodyCol; //Base color
                    var color3 = effectcol; //Tip color

                    for (int j = tailMesh.verticeColors.Length - 1; j >= 0; j--)
                    {
                        float num2 = (j / 2f) / (tailMesh.verticeColors.Length - 3 / 2f);
                        if (j > 12)
                            tailMesh.verticeColors[j] = effectcol;
                        else if (j < 9)
                            tailMesh.verticeColors[j] = data.BodyCol;
                        else
                            tailMesh.verticeColors[j] = Color.Lerp(color2, color3, 0.4f);

                    }
                    tailMesh.Refresh();
                }
            }
        }

        private static void PG_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (self.player.TryGetLamp(out var data))
            {
                if (data.drool != null) data.drool.ApplyMovement();
            }
        }
    }
}
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SunriseIdyll;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;
using IL.Menu.Remix;


namespace SunriseIdyll
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

            self.tail[0] = new TailSegment(self, 12f, 5.5f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 9f, 8.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 6f, 8.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 3f, 8.5f, self.tail[2], 0.85f, 1f, 0.5f, true);


            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }

        private static void PG_Init(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
            if (self.player.TryGetLamp(out var lamp))
            {
                lamp.GraphicsInit = false;
            }
            
            orig(self, sLeaser, rCam);

            if (self.player.TryGetLamp(out var data))
            {
                if (sLeaser.sprites[2] is TriangleMesh tail)
                {
                    tail.element = Futile.atlasManager.GetElementWithName("LampTail");
                    for (var i = tail.vertices.Length - 1; i >= 0; i--)
                    {
                        var perc = i / 2 / (float)(tail.vertices.Length / 2);

                        Vector2 uv;
                        if (i % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (i < tail.vertices.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[i] = uv;
                    }
                }
                data.GraphicsInit = true;
                data.startsprite = sLeaser.sprites.Length;

                data.DroolIndex = sLeaser.sprites.Length;
                data.socksprite = sLeaser.sprites.Length + 1;
                data.masksprite = sLeaser.sprites.Length + 2;
                data.pawsprite1 = sLeaser.sprites.Length + 3;
                data.pawsprite2 = sLeaser.sprites.Length + 4;

                data.drool = new SlimeDrip(sLeaser, rCam, data.DroolIndex, 1.35f, self.player.mainBodyChunk, 9);

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 5);
                data.endsprite = sLeaser.sprites.Length;
                data.drool.GenerateMesh(sLeaser, rCam);

                for (int i = data.startsprite; i < data.endsprite; i++)
                {
                    if (i != data.DroolIndex)
                    {
                        sLeaser.sprites[i] = new FSprite("Circle20");
                    }
                }

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
                if (data.GraphicsInit)
                {
                    if (newContatiner == null)
                    {
                        newContatiner = rCam.ReturnFContainer("Midground");
                    }

                    for (int i = data.startsprite; i < data.endsprite; i++)
                    {
                        newContatiner.AddChild(sLeaser.sprites[i]);
                    }

                    data.drool.MoveInFrontOf(sLeaser, sLeaser.sprites[9]);
                    sLeaser.sprites[data.masksprite].MoveBehindOtherNode(sLeaser.sprites[9]);
                    sLeaser.sprites[data.socksprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
                    sLeaser.sprites[data.pawsprite1].MoveToBack();
                    sLeaser.sprites[data.pawsprite2].MoveToBack();
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
                data.BrightCol = PlayerColor.GetCustomColor(self, 2);
                data.Markingcol = PlayerColor.GetCustomColor(self, 3);

                //data.Tailcol = Color.Lerp(data.BodyCol, data.BrightCol, 0.5f);

                if (self.player.flipDirection == 1)
                {
                    sLeaser.sprites[data.pawsprite1].MoveBehindOtherNode(sLeaser.sprites[3]);
                    sLeaser.sprites[data.pawsprite2].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
                else
                {
                    sLeaser.sprites[data.pawsprite1].MoveBehindOtherNode(sLeaser.sprites[0]);
                    sLeaser.sprites[data.pawsprite2].MoveBehindOtherNode(sLeaser.sprites[3]);
                }

                sLeaser.sprites[5].MoveToBack();
                sLeaser.sprites[6].MoveToBack();

                void UpdateReplacement(int num, string tofind)
                {
                    if (num != 3)
                    {
                        if (!sLeaser.sprites[num].element.name.Contains("Lamp") && sLeaser.sprites[num].element.name.StartsWith(tofind)) sLeaser.sprites[num].SetElementByName("Lamp" + sLeaser.sprites[num].element.name);
                    }
                    else
                    {
                        var name = sLeaser.sprites[num].element.name;
                        name = name.Replace("HeadA", "HeadB");
                        sLeaser.sprites[num].SetElementByName(name);
                    }
                }
                void UpdateCustom(int findindex, string find, string replace, int customindex)
                {
                    try
                    {
                        string origelement = sLeaser.sprites[findindex].element.name;

                        origelement = origelement.Replace(find, replace);

                        sLeaser.sprites[customindex].SetElementByName(origelement);
                        sLeaser.sprites[customindex].Follow(sLeaser.sprites[findindex]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }



                if (data.drool != null)
                {
                    data.drool.size = Mathf.Lerp(1.35f, 0.075f, data.DroolMeltCounter / 100f);
                    data.drool.Draw(sLeaser, timeStacker, camPos);
                    data.drool.SetColor(sLeaser, Color.Lerp(data.BrightCol, Color.gray, data.DroolMeltCounter / 100f));
                }

                UpdateReplacement(3, "HeadA");
                UpdateReplacement(5, "PlayerArm");
                UpdateReplacement(6, "PlayerArm");
                UpdateReplacement(0, "BodyA");
                UpdateReplacement(1, "HipsA");

                UpdateCustom(9, "Face", "LampMask", data.masksprite);
                UpdateCustom(4, "LegsA", "LampSocksA", data.socksprite);
                UpdateCustom(5, "PlayerArm", "Paw", data.pawsprite1);
                UpdateCustom(6, "PlayerArm", "Paw", data.pawsprite2);


                var effectcol = Color.Lerp(data.Markingcol, data.BodyCol, data.DroolMeltCounter / 100f);

                sLeaser.sprites[data.pawsprite1].color = effectcol;
                sLeaser.sprites[data.pawsprite2].color = effectcol;
                sLeaser.sprites[data.masksprite].color = effectcol;
                sLeaser.sprites[data.socksprite].color = effectcol;


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
                        if (j > 13)
                            tailMesh.verticeColors[j] = effectcol;
                        else if (j < 2)
                            tailMesh.verticeColors[j] = data.BodyCol;
                        else
                            tailMesh.verticeColors[j] = Color.Lerp(color2, color3, (j * 2f - tailMesh.verticeColors.Length) / tailMesh.verticeColors.Length);

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
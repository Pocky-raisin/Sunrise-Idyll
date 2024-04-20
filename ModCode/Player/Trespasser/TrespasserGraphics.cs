using System.Numerics;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;
using Vector2 = UnityEngine.Vector2;

namespace SunriseIdyll
{
    public static class TrespasserGraphics
    {
        public static void ApplyHooks()
        {
            On.PlayerGraphics.ctor += PG_Ctor;
            On.PlayerGraphics.InitiateSprites += PG_Init;
            On.PlayerGraphics.AddToContainer += PG_Add;
            On.PlayerGraphics.DrawSprites += PG_Draw;
            On.SlugcatHand.EngageInMovement += Hand_Engage;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            //IL.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private static void PlayerGraphics_DrawSprites(ILContext il)
        {
            ILCursor cursor = new(il);

            for (int i = 0; i < 2; i++)
            {
                cursor.GotoNext(MoveType.After,
                i => i.MatchLdfld<Player>(nameof(Player.bodyMode)),
                i => i.MatchLdsfld<Player.BodyModeIndex>(nameof(Player.BodyModeIndex.Crawl)),
                i => i.MatchCallOrCallvirt(typeof(ExtEnum<Player.BodyModeIndex>).GetMethod("op_Equality")));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((PlayerGraphics self) =>
                {
                    return self.player.TryGetTrespasser(out var data) && data.Climbing;
                });
                cursor.Emit(OpCodes.Or);
            }
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (self.player.TryGetTrespasser(out var data))
            {
                if (data.Climbing && self.player.bodyChunks[0].pos.y < self.player.bodyChunks[1].pos.y)
                {
                    self.tail[self.tail.Length - 1].vel.y += 2.15f;
                }
            }
        }

        private static bool Hand_Engage(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
        {
            Player player = (self.owner.owner as Player);

            if (player.TryGetTrespasser(out var data))
            {
                if (data.holdingGlide && data.CanGlide && data.Gliding && player.mainBodyChunk.vel.y < 0f && !data.touchingTerrain)
                {
                    var playerOrientation = player.bodyChunks[0].pos - player.bodyChunks[1].pos;


                    if (data.GlideSpeed <= 10)
                    {
                        Vector2 tposePos =
                                    52 *
                                    (self.limbNumber - 0.5f) *
                                    new Vector2(playerOrientation.y, -playerOrientation.x).normalized;

                        tposePos += -1f * playerOrientation.normalized;

                        self.quickness = 1f;
                        self.huntSpeed = 50f;

                        self.mode = Limb.Mode.HuntAbsolutePosition;
                        self.absoluteHuntPos = player.bodyChunks[0].pos + tposePos;
                    }
                    else
                    {
                        if (self.limbNumber == 0)
                        {
                            Vector2 tposePos =
                            52 *
                            (-0.45f * -player.flipDirection) *
                            new Vector2(playerOrientation.y, -playerOrientation.x).normalized;

                            tposePos += -1f * playerOrientation.normalized;

                            self.quickness = 1f;
                            self.huntSpeed = 50f;

                            self.mode = Limb.Mode.HuntAbsolutePosition;
                            self.absoluteHuntPos = player.bodyChunks[0].pos + tposePos;
                        }
                        else
                        {
                            Vector2 tposePos =
                            52 *
                            (-0.75f * -player.flipDirection) *
                            new Vector2(playerOrientation.y, -playerOrientation.x).normalized;

                            tposePos += -1f * playerOrientation.normalized;

                            self.quickness = 1f;
                            self.huntSpeed = 50f;

                            self.mode = Limb.Mode.HuntAbsolutePosition;
                            self.absoluteHuntPos = player.bodyChunks[0].pos + tposePos;
                        }
                    }
                    return false;
                }

                else if (data.Climbing)
                {
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    float walldir;

                    if ((self.owner.owner as Player).bodyChunks[0].pos.y > (self.owner.owner as Player).bodyChunks[1].pos.y)
                    {
                        walldir = 1;
                    }
                    else walldir = -1;

                    self.huntSpeed = 12f;
                    self.quickness = 0.7f;
                    if ((self.limbNumber == 0 || (Mathf.Abs((self.owner as PlayerGraphics).hands[0].pos.y - self.owner.owner.bodyChunks[0].pos.y) < 10f && (self.owner as PlayerGraphics).hands[0].reachedSnapPosition)) && !Custom.DistLess(self.owner.owner.bodyChunks[0].pos, self.absoluteHuntPos, 29f))
                    {
                        Vector2 absoluteHuntPos = self.absoluteHuntPos;
                        self.FindGrip(self.owner.owner.room, self.connection.pos + new Vector2(0f, walldir * 20f),
                            self.connection.pos + new Vector2(0f, walldir * 20f),
                            100f, new Vector2(self.owner.owner.room.MiddleOfTile(self.owner.owner.bodyChunks[0].pos).y - 10f, self.owner.owner.bodyChunks[0].pos.y + walldir * 28f), 2, 1, false);//
                        if (self.absoluteHuntPos != absoluteHuntPos)
                        {
                        }
                    }
                    return false;
                }
            }

            return orig(self);
        }

        private static void PG_Ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);//thicker + longer + floatier tail to compensate for texture
            if (!self.player.IsTrespasser()) return;

            self.tail[0] = new TailSegment(self, 12f, 8.5f, null, 0.85f, 0.66f, 1f, true);
            self.tail[1] = new TailSegment(self, 11f, 11.5f, self.tail[0], 0.85f, 0.66f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 11f, 11.5f, self.tail[1], 0.85f, 0.66f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 10f, 11.66f, self.tail[2], 0.85f, 0.66f, 0.5f, true);

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }

        private static void PG_Init(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (self.player.TryGetTrespasser(out var trespasser))
            {
                trespasser.graphicsinit = false;
            }
            
            orig(self, sLeaser, rCam);

            if (self.player.TryGetTrespasser(out var data))
            {
                //tail texture
                if (sLeaser.sprites[2] is TriangleMesh tail)
                {
                    tail.element = Futile.atlasManager.GetElementWithName("TresTail");
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
                data.graphicsinit = true;

                //actual earsprie code
                data.earsprite = sLeaser.sprites.Length;
                data.wings = sLeaser.sprites.Length + 1;

                System.Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 2);

                sLeaser.sprites[data.earsprite] = new FSprite("Circle20", true);
                TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                {
                    new (0, 1, 2),
                    new (1, 2, 3)
                };

                var triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
                sLeaser.sprites[data.wings] = triangleMesh;

                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        private static void PG_Add(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (self.player.TryGetTrespasser(out var data))
            {
                if (data.graphicsinit)
                {
                    if (newContatiner == null)
                    {
                        newContatiner = rCam.ReturnFContainer("Midground");
                    }

                    newContatiner.AddChild(sLeaser.sprites[data.earsprite]);
                    newContatiner.AddChild(sLeaser.sprites[data.wings]);
                    sLeaser.sprites[data.earsprite].MoveInFrontOfOtherNode(sLeaser.sprites[3]);//move it just in front of head

                    sLeaser.sprites[data.wings].MoveBehindOtherNode(sLeaser.sprites[5]);


                }
            }
        }

        public static void Follow(this FSprite sprite, FSprite follow)
        {
            sprite.SetPosition(follow.GetPosition());
            sprite.rotation = follow.rotation;
            sprite.scaleX = follow.scaleX;
            sprite.scaleY = follow.scaleY;
            sprite.isVisible = follow.isVisible;
            sprite.alpha = follow.alpha;
            sprite.anchorX = follow.anchorX;
            sprite.anchorY = follow.anchorY;
        }

        private static void PG_Draw(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.TryGetTrespasser(out var data))
            {

                if (sLeaser.sprites[data.wings] is TriangleMesh wing)
                {

                    if (sLeaser.sprites[5].isVisible)
                    {
                        wing.MoveVertice(0, sLeaser.sprites[5].GetPosition());
                    }
                    else
                    {
                        wing.MoveVertice(0, sLeaser.sprites[0].GetPosition());
                    }

                    if (sLeaser.sprites[6].isVisible)
                    {
                        wing.MoveVertice(3, sLeaser.sprites[6].GetPosition());
                    }
                    else
                    {
                        wing.MoveVertice(3, sLeaser.sprites[0].GetPosition());
                    }

                    var bottom = new Vector2(Mathf.Lerp(sLeaser.sprites[1].x, sLeaser.sprites[4].x, 0.75f), Mathf.Lerp(sLeaser.sprites[1].y, sLeaser.sprites[4].y, 0.75f));

                    if (data.CanGlide && data.holdingGlide && data.Gliding)
                    {
                        bottom = sLeaser.sprites[4].GetPosition();
                    }

                    wing.MoveVertice(1, bottom);

                    wing.MoveVertice(2, sLeaser.sprites[0].GetPosition());

                }

                void UpdateReplacement(int num, string tofind)
                {
                    if (!sLeaser.sprites[num].element.name.Contains("Tres") && sLeaser.sprites[num].element.name.StartsWith(tofind)) sLeaser.sprites[num].SetElementByName("Tres" + sLeaser.sprites[num].element.name);
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



                if (data.Climbing)
                {
                    sLeaser.sprites[3].SetElementByName("HeadA7");
                    sLeaser.sprites[9].SetElementByName("FaceA4");

                    Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                    Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);

                    if (self.player.bodyChunks[0].vel.x > 0f)// --}
                    {
                        sLeaser.sprites[3].scaleX = 1f;
                        sLeaser.sprites[3].rotation = 0f;
                        //sLeaser.sprites[9].scaleX = Mathf.Abs(sLeaser.sprites[9].scaleX);
                        sLeaser.sprites[3].scaleY = 1f;
                        sLeaser.sprites[9].rotation = -90f;
                        sLeaser.sprites[4].rotation = -90f;

                        if (self.player.bodyChunks[0].pos.y > self.player.bodyChunks[1].pos.y)//facing up?
                        {
                            sLeaser.sprites[9].scaleX = 1f;// Mathf.Abs(Mathf.Sign(vector.x - vector2.x));
                            sLeaser.sprites[4].scaleX = 1f;
                            sLeaser.sprites[3].scaleY = 1f;
                            self.lookDirection.y += 0.0275f;
                        }
                        else
                        {
                            sLeaser.sprites[9].scaleX = -1f;// Mathf.Abs(Mathf.Sign(vector.x - vector2.x));
                            sLeaser.sprites[4].scaleX = -1f;
                            sLeaser.sprites[3].scaleY = -1f;
                            self.lookDirection.y -= 0.0275f;
                        }

                    }
                    else//{---
                    {
                        sLeaser.sprites[3].scaleX = -1f;
                        sLeaser.sprites[3].rotation = 0f;
                        sLeaser.sprites[3].scaleY = 1f;
                        sLeaser.sprites[9].rotation = 90f;
                        sLeaser.sprites[4].rotation = 90f;

                        if (self.player.bodyChunks[0].pos.y > self.player.bodyChunks[1].pos.y)//facing up?
                        {
                            sLeaser.sprites[3].scaleY = 1f;
                            sLeaser.sprites[9].scaleX = -1f;// Mathf.Abs(Mathf.Sign(vector.x - vector2.x));
                            sLeaser.sprites[4].scaleX = -1f;
                            self.lookDirection.y += 0.0275f;
                        }
                        else
                        {
                            sLeaser.sprites[3].scaleY = -1f;
                            sLeaser.sprites[9].scaleX = 1f;// Mathf.Abs(Mathf.Sign(vector.x - vector2.x));
                            sLeaser.sprites[4].scaleX = 1f;
                            self.lookDirection.y -= 0.0275f;
                        }
                    }
                    self.lookDirection.x = 0f;
                }
                else
                {
                    sLeaser.sprites[3].scaleY = 1f;
                }

                UpdateReplacement(3, "HeadA");//floof head
                UpdateReplacement(9, "FaceA");//sad face
                UpdateCustom(3, "HeadA", "EarsA", data.earsprite);//earsprite

                if (data.Gliding)
                {
                    self.tail[self.tail.Length - 1].vel.x += 0.9f * -self.player.flipDirection;
                }

                sLeaser.sprites[data.earsprite].color = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);//gets custom col of index 2, aka the third col
            }
        }

    }
}
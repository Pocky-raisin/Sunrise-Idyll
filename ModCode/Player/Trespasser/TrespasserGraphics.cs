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
        }

        private static void PG_Ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);//thicker + longer + floatier tail to compensate for texture
            if (!self.player.IsTrespasser()) return;

            self.tail[0] = new TailSegment(self, 10f, 8.5f, null, 0.85f, 0.66f, 1f, true);
            self.tail[1] = new TailSegment(self, 10.5f, 11.5f, self.tail[0], 0.85f, 0.66f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 11.5f, 11.5f, self.tail[1], 0.85f, 0.66f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 12f, 11.66f, self.tail[2], 0.85f, 0.66f, 0.5f, true);

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }

        private static void PG_Init(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
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


                //actual earsprie code
                data.earsprite = sLeaser.sprites.Length;
                System.Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                sLeaser.sprites[data.earsprite] = new FSprite("Circle20", true);//some random fsprite

                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        private static void PG_Add(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (self.player.TryGetTrespasser(out var data))
            {
                if (sLeaser.sprites.Length > data.earsprite)
                {
                    if (newContatiner == null)
                    {
                        newContatiner = rCam.ReturnFContainer("Midground");
                    }

                    newContatiner.AddChild(sLeaser.sprites[data.earsprite]);
                    sLeaser.sprites[data.earsprite].MoveBehindOtherNode(sLeaser.sprites[4]);//move it just in front of head
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

                UpdateReplacement(3, "HeadA");//floof head
                UpdateReplacement(9, "FaceA");//sad face
                UpdateCustom(3, "HeadA", "EarsA", data.earsprite);//earsprite

                if (data.FakeDead)//play dead graphics changes
                {
                    sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("TresFakeFace");

                    Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                    Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                    Vector2 vector3 = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);
                    float num3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);

                    sLeaser.sprites[9].rotation = num3;
                    sLeaser.sprites[3].rotation = num3;
                    sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("TresHeadA0");
                    sLeaser.sprites[data.earsprite].element = Futile.atlasManager.GetElementWithName("TresEarsA0");
                }

                if (data.Gliding)
                {
                    self.tail[self.tail.Length - 1].vel.x += 0.9f * -self.player.flipDirection;
                }

                sLeaser.sprites[data.earsprite].color = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);//gets custom col of index 2, aka the third col
            }
        }

    }
}
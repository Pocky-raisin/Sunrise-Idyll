using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Sunrise
{
    namespace Objects
    {
        public class WarmSlime : PlayerCarryableItem, IDrawable, IProvideWarmth
        {

            public WarmSlimeAbstract Abstr { get; set; }

            public SlimeDrip drip;
            public bool dripVisible;

            public bool InStorage;
            
            public WarmSlime(WarmSlimeAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
            {
                base.bodyChunks = new BodyChunk[1];
                base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
                this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
                base.airFriction = 0.999f;
                base.gravity = 0.9f;
                this.bounce = 0.6f;
                this.surfaceFriction = 0.4f;
                this.collisionLayer = 1;
                base.waterFriction = 0.95f;
                base.buoyancy = 0.8f;
                InStorage = false;

                Abstr = abstr;

                bodyChunks[0].lastPos = bodyChunks[0].pos;
                bodyChunks[0].vel = vel;
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[3];

                sLeaser.sprites[0] = new FSprite("WarmSlimeSprite", true);
                sLeaser.sprites[1] = new FSprite("JetFishEyeA", true);

                this.drip = new SlimeDrip(sLeaser, rCam, 2, 1f, base.firstChunk, 0);
                this.drip.GenerateMesh(sLeaser, rCam);
                this.drip.Reset();

                AddToContainer(sLeaser, rCam, null);

            }

            public override void Update(bool eu)
            {
                bool wasInitiated = drip == null;
                base.Update(eu);
                if (wasInitiated && drip != null)
                {
                    drip.Reset();
                }
                else
                {
                    drip?.ApplyMovement();
                }
                if (firstChunk.submersion > 0f && !(Abstr.Lifetime >= Abstr.MaxLifetime))
                {
                    room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, firstChunk);
                    Abstr.Lifetime = Abstr.MaxLifetime;
                }

                Abstr.Lifetime++;
                if (Abstr.Lifetime > Abstr.MaxLifetime) Abstr.Lifetime = Abstr.MaxLifetime;
            }

            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                if (newContatiner == null)
                {
                    newContatiner = rCam.ReturnFContainer("Items");
                }
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                }

                foreach (FSprite sprite in sLeaser.sprites)
                {
                    newContatiner.AddChild(sprite);
                }

                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[0]);
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {

                Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
                sLeaser.sprites[0].x = vector.x - camPos.x;
                sLeaser.sprites[0].y = vector.y - camPos.y;
                sLeaser.sprites[1].SetPosition(sLeaser.sprites[0].GetPosition());

                if (drip != null)
                {
                    this.drip.Draw(sLeaser, timeStacker, camPos);
                }

                if (InStorage)
                {
                    sLeaser.sprites[0].scale = 0.75f;
                    sLeaser.sprites[1].scale = 0.75f;
                    if (drip != null)
                    {
                        drip.size = 0.25f;
                    }
                }
                else
                {
                    sLeaser.sprites[0].scale = 1f;
                    sLeaser.sprites[1].scale = 1f;
                    if (drip != null)
                    {
                        drip.size = 1f;
                    }
                }

                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = Abstr.colour != null? Abstr.colour : Color.red;
                }

                sLeaser.sprites[1].color = Color.Lerp(Abstr.colour, Color.black, 0.35f);

                float desaturate = 0f;
                
                if (Abstr.Lifetime > Abstr.MaxLifetime / 4)
                {
                    desaturate = 0.25f;
                }
                else if(Abstr.Lifetime > Abstr.MaxLifetime / 2)
                {
                    desaturate = 0.5f;
                }
                else if(Abstr.Lifetime > Abstr.MaxLifetime * 0.75f)
                {
                    desaturate = 0.75f;
                }
                else if(Abstr.Lifetime >= Abstr.MaxLifetime)
                {
                    desaturate = 1f;
                }

                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = Color.Lerp(sLeaser.sprites[i].color, Color.gray, desaturate);

                    if (i == 1)
                    {
                        sLeaser.sprites[i].color = Color.Lerp(sLeaser.sprites[i].color, Color.black, desaturate);
                    }
                }

                if (slatedForDeletetion || room != rCam.room)
                {
                    sLeaser.CleanSpritesAndRemove();
                }
            }

            float IProvideWarmth.range
            {
                get
                {
                    return 300f;
                }
            }

            Room IProvideWarmth.loadedRoom
            {
                get
                {
                    return this.room;
                }
            }

            Vector2 IProvideWarmth.Position()
            {
                return this.firstChunk.pos;
            }

            float IProvideWarmth.warmth
            {
                get
                {
                    if (Abstr.Lifetime >= Abstr.MaxLifetime)
                    {
                        return 0f;
                    }
                    else if (Abstr.Lifetime >= Abstr.MaxLifetime / 4)
                    {
                        return RainWorldGame.DefaultHeatSourceWarmth / 4f;
                    }
                    else if (Abstr.Lifetime >= Abstr.MaxLifetime / 2)
                    {
                        return RainWorldGame.DefaultHeatSourceWarmth * 0.5f;
                    }
                    else if (Abstr.Lifetime >= Abstr.MaxLifetime * 0.75f)
                    {
                        return RainWorldGame.DefaultHeatSourceWarmth * 0.75f;
                    }
                    return RainWorldGame.DefaultHeatSourceWarmth * 1.15f;
                }
            }
        }
    }
}
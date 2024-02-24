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
            public LightSource light;

            public bool InStorage;
            
            public WarmSlime(WarmSlimeAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
            {
                base.bodyChunks = new BodyChunk[1];
                base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
                this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
                base.airFriction = 0.999f;
                base.gravity = 0.9f;
                this.bounce = 0.6f;
                this.surfaceFriction = 0.8f;
                this.collisionLayer = 1;
                base.waterFriction = 0.95f;
                base.buoyancy = 0.8f;

                Abstr = abstr;

                bodyChunks[0].lastPos = bodyChunks[0].pos;
                bodyChunks[0].vel = vel;
            }

            float IProvideWarmth.range
            {
                get
                {
                    return 350f;
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
                    return RainWorldGame.DefaultHeatSourceWarmth;
                }
            }
        }
    }
}
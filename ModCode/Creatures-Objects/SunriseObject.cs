namespace SunriseIdyll
{
    namespace Objects
    {
        public class WarmSlime : PlayerCarryableItem, IDrawable, IProvideWarmth
        {

            public WarmSlimeAbstract Abstr { get; set; }
            public SlimeDrip drip;

            public Vector2 rotation;
            public Vector2 lastRotation;
            public Vector2? setRotation;
            public float prop;
            public float lastProp;
            public float propSpeed;
            public float plop;
            public float lastPlop;

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
                base.buoyancy = 0.95f;

                Abstr = abstr;

                bodyChunks[0].lastPos = bodyChunks[0].pos;
                bodyChunks[0].vel = vel;
            }

            public override void Update(bool eu)
            {
                bool WasNull = drip == null;
                
                base.Update(eu);

                if (WasNull && drip != null)
                {
                    drip.Reset();
                }
                drip?.ApplyMovement();

                this.lastRotation = this.rotation;
                if (this.grabbedBy.Count > 0)
                {
                    this.rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
                    this.rotation.y = Mathf.Abs(this.rotation.y);
                }
                if (this.setRotation != null)
                {
                    this.rotation = this.setRotation.Value;
                    this.setRotation = null;
                }
                if (base.firstChunk.ContactPoint.y < 0)
                {
                    this.rotation = (this.rotation - Custom.PerpendicularVector(this.rotation) * 0.1f * base.firstChunk.vel.x).normalized;
                    BodyChunk firstChunk = base.firstChunk;
                    firstChunk.vel.x = firstChunk.vel.x * 0.8f;
                }
                this.lastProp = this.prop;
                this.prop += this.propSpeed;
                this.propSpeed *= 0.85f;
                this.propSpeed -= this.prop / 10f;
                this.prop = Mathf.Clamp(this.prop, -15f, 15f);
                if (this.grabbedBy.Count == 0)
                {
                    this.prop += (base.firstChunk.lastPos.x - base.firstChunk.pos.x) / 15f;
                    this.prop -= (base.firstChunk.lastPos.y - base.firstChunk.pos.y) / 15f;
                }
                this.lastPlop = this.plop;
                if (this.plop > 0f && this.plop < 1f)
                {
                    this.plop = Mathf.Min(1f, this.plop + 0.1f);
                }

                if (firstChunk.submersion > 0.25f)
                {
                    Abstr.Lifetime += 40;
                }
            }

            public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
            {
                base.TerrainImpact(chunk, direction, speed, firstContact);
                if (direction.y != 0)
                {
                    this.prop += speed;
                    this.propSpeed += speed / 10f;
                }
                else
                {
                    this.prop -= speed;
                    this.propSpeed -= speed / 10f;
                }
                if (speed > 1.2f && firstContact)
                {
                    Vector2 pos = base.firstChunk.pos + direction.ToVector2() * base.firstChunk.rad;
                    for (int i = 0; i < Mathf.RoundToInt(Custom.LerpMap(speed, 1.2f, 6f, 2f, 5f, 1.2f)); i++)
                    {
                        this.room.AddObject(new WaterDrip(pos, Custom.RNV() * (2f + speed) * UnityEngine.Random.value * 0.5f + -direction.ToVector2() * (3f + speed) * 0.35f, true));
                    }
                    this.room.PlaySound(SoundID.Tube_Worm_Detach_Tongue_Terrain, pos, Custom.LerpMap(speed, 1.2f, 6f, 0.2f, 1f), 1f);
                }
            }
            
            float IProvideWarmth.range
            {
                get
                {
                    return Mathf.Lerp(350f, 100f, (Abstr.Lifetime / Abstr.MaxLifetime) - 0.33f);
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
                    return Mathf.Lerp(0.00055f, 0.00005f, (Abstr.Lifetime / Abstr.MaxLifetime));
                }
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[2];

                sLeaser.sprites[0] = new FSprite("WarmSlimeSprite");
                drip = new SlimeDrip(sLeaser, rCam, 1, 1.25f, firstChunk, 0);
                drip.GenerateMesh(sLeaser, rCam);
                AddToContainer(sLeaser, rCam, null);
            }

            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                newContatiner ??= rCam.ReturnFContainer("Items");
                newContatiner.AddChild(sLeaser.sprites[0]);
                newContatiner.AddChild(sLeaser.sprites[1]);
                drip.AddContainerChild(sLeaser, newContatiner);
                drip.MoveBehind(sLeaser, sLeaser.sprites[1]);
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
            {
                Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
                Vector2 v = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);

                sLeaser.sprites[0].color = Color.Lerp(Abstr.colour, rCam.currentPalette.blackColor, (Abstr.Lifetime / Abstr.MaxLifetime) - 0.25f);
                sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(Abstr.colour, rCam.currentPalette.blackColor, 0.25f), rCam.currentPalette.blackColor, (Abstr.Lifetime / Abstr.MaxLifetime) - 0.2f);
                drip?.SetColor(sLeaser, Color.Lerp(Abstr.colour, rCam.currentPalette.blackColor, (Abstr.Lifetime / Abstr.MaxLifetime) - 0.25f));

                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[i].x = vector.x - camPos.x;
                    sLeaser.sprites[i].y = vector.y - camPos.y;
                    sLeaser.sprites[i].rotation = Custom.VecToDeg(v);
                }

                drip?.Draw(sLeaser, timeStacker, camPos);

                for (int wa = 0; wa < 3; wa++)
                {
                    if (this.blink > 0 && UnityEngine.Random.value < 0.5f)
                    {
                        sLeaser.sprites[wa].color = new Color(1f, 1f, 1f);
                    }
                }

                float num = Mathf.Lerp(this.lastPlop, this.plop, timeStacker);
                num = Mathf.Lerp(0f, 1f + Mathf.Sin(num * 3.1415927f), num);

                sLeaser.sprites[0].scaleX = (1.2f * Custom.LerpMap((float)3, 3f, 1f, 1f, 0.2f) * 1f + Mathf.Lerp(this.lastProp, this.prop, timeStacker) / 20f) * num;
                sLeaser.sprites[0].scaleY = (1.2f * Custom.LerpMap((float)3, 3f, 1f, 1f, 0.2f) * 1f - Mathf.Lerp(this.lastProp, this.prop, timeStacker) / 20f) * num;

                sLeaser.sprites[1].scaleX = sLeaser.sprites[0].scaleX - 0.3f;
                sLeaser.sprites[1].scaleY = sLeaser.sprites[0].scaleY - 0.3f;

                if (base.slatedForDeletetion || this.room != rCam.room)
                {
                    sLeaser.CleanSpritesAndRemove();
                }
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
            }
        }

        public class HarpoonSpear : Weapon, IDrawable
        {
            public HarpoonSpearAbstract Abstr { get; set; }

            public float prop;
            public float lastProp;
            public float propSpeed;
            public float plop;
            public float lastPlop;
            public int stuckBodyPart;
            public bool InStorage;
            public int damageTicks;
            public int damageCount;
            public Creature.DamageType damageType = WorldThings.Fire;

            public HarpoonSpear(HarpoonSpearAbstract abstr, Vector2 pos, Vector2 vel, World world, int damageTicks) : base(abstr, world)
            {
                base.bodyChunks = new BodyChunk[1];
                base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
                this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
                base.airFriction = 0.999f;
                base.gravity = 1.7f;
                this.bounce = 0.4f;
                this.surfaceFriction = 0.4f;
                this.collisionLayer = 2;
                base.waterFriction = 0.98f;
                base.buoyancy = 0.4f;
                this.stuckBodyPart = -1;
                this.damageTicks = 40;
                this.damageCount = 3;

                Abstr = abstr;

                bodyChunks[0].lastPos = bodyChunks[0].pos;
                bodyChunks[0].vel = vel;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if(this.stuckBodyPart >= 0)
                {
                    if(this.damageTicks == 0 && this.damageCount > 0)
                    {
                        this.damageCount--;
                        this.damageTicks = 40; 
                        //creature.takeFireDamage(0.5f, 0.1f, this.firstChunk);
                    }
                    else if(this.damageTicks > 0)
                    {
                        this.damageTicks--;
                    }
                }
            }
        }
    }
}
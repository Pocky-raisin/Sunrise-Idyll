using SlugBase.SaveData;
namespace SunriseIdyll
{
    public static class TrespasserHooks
    {
        public static void ApplyHooks()
        {
            On.Player.MovementUpdate += MovementUpdate;
            On.Player.ThrowObject += ThrowObject;
            //On.Player.TerrainImpact += Player_TerrainImpact;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.Player.UpdateMSC += Player_UpdateMSC;
        }

        private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (self.TryGetTrespasser(out var data))
            {
                if (data.Climbing)
                {
                    self.gravity = 0f;
                }
            }
        }

        private static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);

            void FlipBodyChunks()
            {
                var pos0 = self.bodyChunks[0].pos;
                var pos1 = self.bodyChunks[1].pos;

                self.bodyChunks[1].pos = pos0;
                self.bodyChunks[0].pos = pos1;
            }

            if (self.TryGetTrespasser(out var data))
            {
                if (/*self.IsTileSolid(0, self.flipDirection, 0)*/self.bodyChunks[0].ContactPoint.x == self.flipDirection && /*!self.IsTileSolid(1, 0, -1)*/ self.bodyChunks[1].ContactPoint.y > -1 && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.Submersion < 0.5f)
                {
                    self.bodyMode = Player.BodyModeIndex.WallClimb;

                    data.Climbing = true;
                    data.ClimbDirection = new IntVector2(0, 0);

                    self.bodyChunks[0].vel.x = self.flipDirection * 1.5f;
                    self.bodyChunks[1].vel.x = self.flipDirection * 1.5f;

                    if (self.input[0].y == 1)
                    {
                        data.ClimbDirection.y = 1;
                        self.bodyChunks[0].vel.y = 2.75f;
                        self.bodyChunks[1].vel.y = 2.25f;

                        if (self.bodyChunks[0].pos.y < self.bodyChunks[1].pos.y)
                        {
                            FlipBodyChunks();
                        }

                    }
                    else if (self.input[0].y == -1)
                    {
                        data.ClimbDirection.y = -1;
                        self.bodyChunks[0].vel.y = - 2.75f;
                        self.bodyChunks[1].vel.y = -2.25f;

                        if (self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y)
                        {
                            FlipBodyChunks();
                        }
                    }
                    else
                    {
                        self.bodyChunks[0].vel.y = 0;
                        self.bodyChunks[1].vel.y = 0;
                    }

                    self.canJump = Mathf.Max(self.canJump, 5);

                    /*
                    if (data.ClimbDirection.y == 1)
                    {

                        self.bodyChunks[1].pos = self.bodyChunks[0].pos - new Vector2(0f, 4.25f);
                    }
                    else if(data.ClimbDirection.y == -1)
                        self.bodyChunks[1].pos = self.bodyChunks[0].pos + new Vector2(0f, 4.25f);
                    */

                    if (self.input[0].jmp && !self.input[1].jmp)
                    {
                        data.Climbing = false;
                        self.bodyChunks[0].vel = new Vector2(7 * -self.flipDirection, 7 * self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y? 6f : -6f);
                        self.bodyChunks[1].vel = self.bodyChunks[1].vel * 0.75f;

                        self.bodyChunks[0].pos += new Vector2(7 * -self.flipDirection, 7 * self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y ? 6f : -6f);
                        self.bodyChunks[1].pos += new Vector2(5 * -self.flipDirection, 5 * self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y ? 4f : -4f);
                        data.ClimbDirection = new IntVector2(0, 0);
                    }

                }
                else
                {
                    data.Climbing = false;
                    data.ClimbDirection = new IntVector2(0, 0);
                }
            }

        }

        private static void Player_TerrainImpact(On.Player.orig_TerrainImpact orig, Player self, int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            orig(self, chunk, direction, speed, firstContact);
            if (self.TryGetTrespasser(out var data))
            {
                if ((!self.standing && direction.y < 0) || direction.y >= 0)
                {
                    data.Climbing = true;
                    data.ClimbDirection = new IntVector2(0, 0);

                    if (direction.x != 0)
                    {
                        data.ClimbDirection.y = 1;
                    }

                    self.bodyChunks[0].vel = Vector2.zero;
                    self.bodyChunks[1].vel = Vector2.zero;

                    if (self.input[0].y == 1)
                    {
                        data.ClimbDirection.y = 1;
                        self.bodyChunks[0].pos.y += 5f;
                    }
                    else if (self.input[0].y == -1)
                    {
                        data.ClimbDirection.y = -1;
                        self.bodyChunks[0].pos.y -= 5f;
                    }

                    self.canJump = Mathf.Max(self.canJump, 5);
                    self.bodyChunks[1].pos = self.bodyChunks[0].pos - new Vector2(0f, 2.5f * data.ClimbDirection.y);
                }
            }
        }

        public static void ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            PhysicalObject grabbedObj = self.grasps[grasp].grabbed;
            orig(self, grasp, eu);
            if (self.IsTrespasser())
            {
                if (grabbedObj is Weapon && grabbedObj is not Rock)
                {
                    (grabbedObj as Weapon).doNotTumbleAtLowSpeed = true;
                }
                grabbedObj.firstChunk.vel *= 0.2f;
            }
        }

        #region WorldMechanics

        #endregion

        private static void MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            bool isGliderscug = self.TryGetTrespasser(out TrespasserModule.TrespasserData data);
            if (!isGliderscug)
            {
                orig(self, eu);
                return;
            }

            const float normalGravity = 0.9f;

            //Initiate double jump 
            if (data.triggerGlide && self.bodyMode != Player.BodyModeIndex.ZeroG && !data.holdingBigItem && !self.input[0].pckp)
            {
                data.CanInitGlide = false;
                self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk.pos, 1f, 1f);

                //init glide
                data.CanGlide = true;
                data.Gliding = true;

                int speed = 0;

                if (self.input[0].x != 0)
                {
                    speed = 6;
                }

                data.GlideSpeed = speed;
            }

            //runs when gliding
            if (data.holdingGlide && data.CanGlide && data.Gliding && self.mainBodyChunk.vel.y < 0f && !data.touchingTerrain)
            {
                //glide physics
                if (self.slugOnBack != null)
                {
                    self.slugOnBack.interactionLocked = true;
                }
                if (self.spearOnBack != null)
                {
                    self.spearOnBack.interactionLocked = true;
                }
                self.eatCounter = 0;
                self.swallowAndRegurgitateCounter = 0;
                self.noGrabCounter = 5;
                self.standing = false;
                self.WANTTOSTAND = false;

                if (self.input[0].y > 0)
                {
                    self.input[0].y = 0;
                    self.input[0].x = self.flipDirection;
                }

                if (self.graphicsModule as PlayerGraphics != null && self.graphicsModule is PlayerGraphics pg)
                {
                    pg.head.vel.x += 1f * self.flipDirection;
                    pg.lookDirection.x += 0.5f * self.flipDirection;
                }

                if (self.slugOnBack != null && self.slugOnBack.slugcat != null && self.slugOnBack.HasASlug)
                {
                    self.slugOnBack.DropSlug();
                }

                self.animation = Player.AnimationIndex.RocketJump;

                foreach (BodyChunk chunk in self.bodyChunks)
                {

                    if (self.flipDirection != data.GlideDirection)
                    {
                        self.animation = Player.AnimationIndex.CrawlTurn;
                        data.GlideSpeed = 3;
                        chunk.vel.x = 0;
                        data.GlideDirection = self.flipDirection;
                    }
                    else
                    //Increase speed
                    {
                        if (data.GlideSpeed > 50)
                        {
                            data.GlideSpeed = 50;
                        }
                        if (data.GlideSpeed < 3)
                        {
                            data.GlideSpeed = 3;
                        }
                        if (self.input[0].x != 0)
                        {
                            data.GlideSpeed++;
                        }
                        else data.GlideSpeed-= self.input[0].y == -1? 8 : 2;
                    }

                    float uhh = 0.8f;
                    if (self.input[0].y < 0) uhh = 1.4f;


                    if (data.GlideSpeed <= 30)
                    {
                        float negative = Mathf.InverseLerp(0.5f, uhh, data.GlideSpeed / 30);
                        chunk.vel.y -= negative;
                    }
                    else chunk.vel.y = -0.5f;

                    //Implement speed increase
                    if (data.CanGlide && Mathf.Abs(chunk.vel.x) < 20)
                    {
                        chunk.vel.x = Mathf.Lerp(chunk.vel.x, chunk.vel.x + (data.GlideSpeed / 20 * self.flipDirection), 0.2f);
                    }
                    else
                    {
                        chunk.vel.x = 20 * self.flipDirection;
                    }

                }
            }

            //Recharge the glide when on a wall, floor or pole
            if (data.rechargeGlide && self.room.gravity != 0)
            {
                self.gravity = normalGravity;
                self.customPlayerGravity = normalGravity;
                data.CanInitGlide = true;
                data.Gliding = false;
                data.CanGlide = true;
                data.GlideSpeed = 0;
            }

            if (data.GlideCooldown > 0)
            {
                data.GlideCooldown--;
            }
            //Cooldown for how soon gliding can begin after jumping
            if (data.GlideCooldown < 10 && data.rechargeGlide && self.bodyMode == Player.BodyModeIndex.ZeroG && self.animation == Player.AnimationIndex.ZeroGSwim)
            {
                data.GlideCooldown = 10;
            }
            orig(self, eu);
        }

        public static bool IsTrespasser(this Player pl)
        {
            return pl.SlugCatClass.value == "IDYLL.GlideScug";
        }

        public static readonly SlugcatStats.Name TrespasserName = new SlugcatStats.Name("IDYLL.GlideScug", false);

        public static SlugBaseSaveData trespasserSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(TrespasserName));
    }

    public class KarmaMeterCWTClass
    {
        public bool isTress;
        public bool isNomad;
        public KarmaMeterCWTClass(HUD.KarmaMeter meter)
        {
            isTress = meter.hud.owner is Player && (meter.hud.owner as Player).IsTrespasser();
            isNomad = meter.hud.owner is Player && (meter.hud.owner as Player).Karma >= 1;
        }
    }
}
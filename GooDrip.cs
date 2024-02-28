using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using RWCustom;

namespace SunriseIdyll
{
    // Token: 0x0200001F RID: 31
    public class SlimeDrip
    {
        // Token: 0x0600009D RID: 157 RVA: 0x00008670 File Offset: 0x00006870
        public SlimeDrip(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int spriteIndex, float size, BodyChunk parentChunk, int parentSpriteIndex)
        {
            this.spriteIndex = spriteIndex;
            this.size = size;
            this.parentChunk = parentChunk;
            this.parentSpriteIndex = parentSpriteIndex;
            this.scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
            this.prevRotation = default(Vector2);
            this.spriteOffset = default(Vector2);
            this.vertices = new Vector2[6, 6];
        }

        // Token: 0x0600009E RID: 158 RVA: 0x000086D4 File Offset: 0x000068D4
        public void GenerateMesh(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[this.spriteIndex] = TriangleMesh.MakeLongMesh(this.vertices.GetLength(0), false, false);
            sLeaser.sprites[this.spriteIndex].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
            sLeaser.sprites[this.spriteIndex].alpha = 1f;
        }

        // Token: 0x0600009F RID: 159 RVA: 0x00008747 File Offset: 0x00006947
        public void AddContainerChild(RoomCamera.SpriteLeaser sLeaser, FContainer container)
        {
            container.AddChild(sLeaser.sprites[this.spriteIndex]);
        }

        // Token: 0x060000A0 RID: 160 RVA: 0x0000875E File Offset: 0x0000695E
        public void SetColor(RoomCamera.SpriteLeaser sLeaser, Color color)
        {
            sLeaser.sprites[this.spriteIndex].color = color;
        }

        // Token: 0x060000A1 RID: 161 RVA: 0x00008775 File Offset: 0x00006975
        public void MoveBehind(RoomCamera.SpriteLeaser sLeaser, FNode node)
        {
            sLeaser.sprites[this.spriteIndex].MoveBehindOtherNode(node);
        }

        // Token: 0x060000A2 RID: 162 RVA: 0x0000878C File Offset: 0x0000698C
        public void MoveInFrontOf(RoomCamera.SpriteLeaser sLeaser, FNode node)
        {
            sLeaser.sprites[this.spriteIndex].MoveInFrontOfOtherNode(node);
        }

        // Token: 0x060000A3 RID: 163 RVA: 0x000087A4 File Offset: 0x000069A4
        public void Reset()
        {
            Vector2 vector = this.AttachPos(1f);
            for (int i = 0; i < this.vertices.GetLength(0); i++)
            {
                this.vertices[i, 0] = vector;
                this.vertices[i, 1] = vector;
                this.vertices[i, 2] *= 0f;
            }
        }

        // Token: 0x060000A4 RID: 164 RVA: 0x0000881C File Offset: 0x00006A1C
        public void Draw(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
        {
            float num = 0f;
            this.spriteOffset = new Vector2(sLeaser.sprites[this.parentSpriteIndex].x, sLeaser.sprites[this.parentSpriteIndex].y) + camPos - this.parentChunk.pos;
            Vector2 a = this.AttachPos(timeStacker);
            TriangleMesh triangleMesh = sLeaser.sprites[this.spriteIndex] as TriangleMesh;
            bool flag = triangleMesh == null;
            if (!flag)
            {
                for (int i = 0; i < this.vertices.GetLength(0); i++)
                {
                    float f = (float)i / (float)(this.vertices.GetLength(0) - 1);
                    Vector2 vector = Vector2.Lerp(this.vertices[i, 1], this.vertices[i, 0], timeStacker);
                    float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(this.vertices[i, 4], this.vertices[i, 3], timeStacker).x;
                    Vector2 normalized = (a - vector).normalized;
                    Vector2 a2 = Custom.PerpendicularVector(normalized);
                    float d = Vector2.Distance(a, vector) / 5f;
                    triangleMesh.MoveVertice(i * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - camPos);
                    triangleMesh.MoveVertice(i * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - camPos);
                    triangleMesh.MoveVertice(i * 4 + 2, vector + normalized * d - a2 * num2 - camPos);
                    triangleMesh.MoveVertice(i * 4 + 3, vector + normalized * d + a2 * num2 - camPos);
                }
            }
        }

        // Token: 0x060000A5 RID: 165 RVA: 0x00008A5C File Offset: 0x00006C5C
        public void ApplyMovement()
        {
            for (int i = 0; i < this.vertices.GetLength(0); i++)
            {
                float t = (float)i / (float)(this.vertices.GetLength(0) - 1);
                this.vertices[i, 1] = this.vertices[i, 0];
                this.vertices[i, 0] += this.vertices[i, 2];
                this.vertices[i, 2] -= this.parentChunk.Rotation * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
                this.vertices[i, 4] = this.vertices[i, 3];
                this.vertices[i, 3] = (this.vertices[i, 3] + this.vertices[i, 5] * Custom.LerpMap(Vector2.Distance(this.vertices[i, 0], this.vertices[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                this.vertices[i, 5] = (this.vertices[i, 5] + Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(this.vertices[i, 0], this.vertices[i, 1])), 0.3f)).normalized;
                bool flag = this.parentChunk.owner.room.PointSubmerged(this.vertices[i, 0]);
                if (flag)
                {
                    this.vertices[i, 2] *= Custom.LerpMap(this.vertices[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    this.vertices[i, 2].y += 0.05f;
                    this.vertices[i, 2] += Custom.RNV() * 0.1f;
                }
                else
                {
                    this.vertices[i, 2] *= Custom.LerpMap(Vector2.Distance(this.vertices[i, 0], this.vertices[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                    this.vertices[i, 2].y -= this.parentChunk.owner.room.gravity * Custom.LerpMap(Vector2.Distance(this.vertices[i, 0], this.vertices[i, 1]), 1f, 6f, 0.6f, 0f);
                    bool flag2 = i % 3 == 2 || i == this.vertices.GetLength(0) - 1;
                    if (flag2)
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData = this.scratchTerrainCollisionData.Set(this.vertices[i, 0], this.vertices[i, 1], this.vertices[i, 2], 1f, new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(this.parentChunk.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.VerticalCollision(this.parentChunk.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(this.parentChunk.owner.room, terrainCollisionData);
                        this.vertices[i, 0] = terrainCollisionData.pos;
                        this.vertices[i, 2] = terrainCollisionData.vel;
                        bool flag3 = terrainCollisionData.contactPoint.x != 0;
                        if (flag3)
                        {
                            this.vertices[i, 2].y *= 0.6f;
                        }
                        bool flag4 = terrainCollisionData.contactPoint.y != 0;
                        if (flag4)
                        {
                            this.vertices[i, 2].x *= 0.6f;
                        }
                    }
                }
            }
            for (int j = 0; j < this.vertices.GetLength(0); j++)
            {
                bool flag5 = j > 0;
                if (flag5)
                {
                    Vector2 normalized = (this.vertices[j, 0] - this.vertices[j - 1, 0]).normalized;
                    float num = Vector2.Distance(this.vertices[j, 0], this.vertices[j - 1, 0]);
                    float d = (num > this.size) ? 0.5f : 0.25f;
                    this.vertices[j, 0] += normalized * (this.size - num) * d;
                    this.vertices[j, 2] += normalized * (this.size - num) * d;
                    this.vertices[j - 1, 0] -= normalized * (this.size - num) * d;
                    this.vertices[j - 1, 2] -= normalized * (this.size - num) * d;
                    bool flag6 = j > 1;
                    if (flag6)
                    {
                        normalized = (this.vertices[j, 0] - this.vertices[j - 2, 0]).normalized;
                        this.vertices[j, 2] += normalized * 0.2f;
                        this.vertices[j - 2, 2] -= normalized * 0.2f;
                    }
                    bool flag7 = j < this.vertices.GetLength(0) - 1;
                    if (flag7)
                    {
                        this.vertices[j, 3] = Vector3.Slerp(this.vertices[j, 3], (this.vertices[j - 1, 3] * 2f + this.vertices[j + 1, 3]) / 3f, 0.1f);
                        this.vertices[j, 5] = Vector3.Slerp(this.vertices[j, 5], (this.vertices[j - 1, 5] * 2f + this.vertices[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(this.vertices[j, 1], this.vertices[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    this.vertices[j, 0] = this.AttachPos(1f);
                    this.vertices[j, 2] *= 0f;
                }
            }
            this.prevRotation = this.parentChunk.Rotation;
        }

        // Token: 0x060000A6 RID: 166 RVA: 0x0000928C File Offset: 0x0000748C
        private Vector2 AttachPos(float timeStacker)
        {
            return Vector2.Lerp(this.parentChunk.lastPos + this.spriteOffset, this.parentChunk.pos + this.spriteOffset, timeStacker) + Vector3.Slerp(this.prevRotation, this.parentChunk.Rotation, timeStacker).ToVector2InPoints() * 15f;
        }

        // Token: 0x04000037 RID: 55
        private int spriteIndex;

        // Token: 0x04000038 RID: 56
        public float size;

        // Token: 0x04000039 RID: 57
        private BodyChunk parentChunk;

        // Token: 0x0400003A RID: 58
        private int parentSpriteIndex;

        // Token: 0x0400003B RID: 59
        public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

        // Token: 0x0400003C RID: 60
        public Vector2 prevRotation;

        // Token: 0x0400003D RID: 61
        public Vector2 spriteOffset;

        // Token: 0x0400003E RID: 62
        public Vector2[,] vertices;
    }
}
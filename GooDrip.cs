namespace SunriseIdyll
{
    public class SlimeDrip
    {
        public SlimeDrip(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int spriteIndex, float size, BodyChunk parentChunk, int parentSpriteIndex)
        {
            this.spriteIndex = spriteIndex;
            this.size = size;
            this.parentChunk = parentChunk;
            this.parentSpriteIndex = parentSpriteIndex;
            scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
            prevRotation = default(Vector2);
            spriteOffset = default(Vector2);
            vertices = new Vector2[6, 6];
        }
        //
        public void GenerateMesh(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[spriteIndex] = TriangleMesh.MakeLongMesh(vertices.GetLength(0), false, false);
            sLeaser.sprites[spriteIndex].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
            sLeaser.sprites[spriteIndex].alpha = 1f;
        }

        public void AddContainerChild(RoomCamera.SpriteLeaser sLeaser, FContainer container)
        {
            container.AddChild(sLeaser.sprites[spriteIndex]);
        }

        public void SetColor(RoomCamera.SpriteLeaser sLeaser, Color color)
        {
            sLeaser.sprites[spriteIndex].color = color;
        }

        public void MoveBehind(RoomCamera.SpriteLeaser sLeaser, FNode node)
        {
            sLeaser.sprites[spriteIndex].MoveBehindOtherNode(node);
        }

        public void MoveInFrontOf(RoomCamera.SpriteLeaser sLeaser, FNode node)
        {
            sLeaser.sprites[spriteIndex].MoveInFrontOfOtherNode(node);
        }

        public void Reset()
        {
            Vector2 vector = AttachPos(1f);
            for (int i = 0; i < vertices.GetLength(0); i++)
            {
                vertices[i, 0] = vector;
                vertices[i, 1] = vector;
                vertices[i, 2] *= 0f;
            }
        }

        public void Draw(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
        {
            float num = 0f;
            spriteOffset = new Vector2(sLeaser.sprites[parentSpriteIndex].x, sLeaser.sprites[parentSpriteIndex].y) + camPos - parentChunk.pos;
            Vector2 a = AttachPos(timeStacker);
            TriangleMesh triangleMesh = sLeaser.sprites[spriteIndex] as TriangleMesh;
            if (triangleMesh != null)
            {
                for (int i = 0; i < vertices.GetLength(0); i++)
                {
                    float f = (float)i / (float)(vertices.GetLength(0) - 1);
                    Vector2 vector = Vector2.Lerp(vertices[i, 1], vertices[i, 0], timeStacker);
                    float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(vertices[i, 4], vertices[i, 3], timeStacker).x;
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

        public void ApplyMovement()
        {
            for (int i = 0; i < vertices.GetLength(0); i++)
            {
                float t = (float)i / (float)(vertices.GetLength(0) - 1);
                vertices[i, 1] = vertices[i, 0];
                vertices[i, 0] += vertices[i, 2];
                vertices[i, 2] -= parentChunk.Rotation * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
                vertices[i, 4] = vertices[i, 3];
                vertices[i, 3] = (vertices[i, 3] + vertices[i, 5] * Custom.LerpMap(Vector2.Distance(vertices[i, 0], vertices[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                vertices[i, 5] = (vertices[i, 5] + Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(vertices[i, 0], vertices[i, 1])), 0.3f)).normalized;
                if (parentChunk.owner.room.PointSubmerged(vertices[i, 0]))
                {
                    vertices[i, 2] *= Custom.LerpMap(vertices[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    vertices[i, 2].y += 0.05f;
                    vertices[i, 2] += Custom.RNV() * 0.1f;
                }
                else
                {
                    vertices[i, 2] *= Custom.LerpMap(Vector2.Distance(vertices[i, 0], vertices[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                    vertices[i, 2].y -= parentChunk.owner.room.gravity * Custom.LerpMap(Vector2.Distance(vertices[i, 0], vertices[i, 1]), 1f, 6f, 0.6f, 0f);
                    if (i % 3 == 2 || i == vertices.GetLength(0) - 1)
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData = scratchTerrainCollisionData.Set(vertices[i, 0], vertices[i, 1], vertices[i, 2], 1f, new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(parentChunk.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.VerticalCollision(parentChunk.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(parentChunk.owner.room, terrainCollisionData);
                        vertices[i, 0] = terrainCollisionData.pos;
                        vertices[i, 2] = terrainCollisionData.vel;
                        if (terrainCollisionData.contactPoint.x != 0)
                        {
                            vertices[i, 2].y *= 0.6f;
                        }
                        if (terrainCollisionData.contactPoint.y != 0)
                        {
                            vertices[i, 2].x *= 0.6f;
                        }
                    }
                }
            }
            for (int j = 0; j < vertices.GetLength(0); j++)
            {
                if (j > 0)
                {
                    Vector2 normalized = (vertices[j, 0] - vertices[j - 1, 0]).normalized;
                    float num = Vector2.Distance(vertices[j, 0], vertices[j - 1, 0]);
                    float d = (num > size) ? 0.5f : 0.25f;
                    vertices[j, 0] += normalized * (size - num) * d;
                    vertices[j, 2] += normalized * (size - num) * d;
                    vertices[j - 1, 0] -= normalized * (size - num) * d;
                    vertices[j - 1, 2] -= normalized * (size - num) * d;
                    if (j > 1)
                    {
                        normalized = (vertices[j, 0] - vertices[j - 2, 0]).normalized;
                        vertices[j, 2] += normalized * 0.2f;
                        vertices[j - 2, 2] -= normalized * 0.2f;
                    }
                    if (j < vertices.GetLength(0) - 1)
                    {
                        vertices[j, 3] = Vector3.Slerp(vertices[j, 3], (vertices[j - 1, 3] * 2f + vertices[j + 1, 3]) / 3f, 0.1f);
                        vertices[j, 5] = Vector3.Slerp(vertices[j, 5], (vertices[j - 1, 5] * 2f + vertices[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(vertices[j, 1], vertices[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    vertices[j, 0] = AttachPos(1f);
                    vertices[j, 2] *= 0f;
                }
            }
            prevRotation = parentChunk.Rotation;
        }

        private Vector2 AttachPos(float timeStacker)
        {
            return Vector2.Lerp(parentChunk.lastPos + spriteOffset, parentChunk.pos + spriteOffset, timeStacker) + Vector3.Slerp(prevRotation, parentChunk.Rotation, timeStacker).ToVector2InPoints() * 15f;
        }

        private int spriteIndex;
        public float size;
        private BodyChunk parentChunk;
        private int parentSpriteIndex;
        public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;
        public Vector2 prevRotation;
        public Vector2 spriteOffset;
        public Vector2[,] vertices;
    }
}
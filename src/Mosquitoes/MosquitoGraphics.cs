using System;
using RWCustom;
using UnityEngine;
using System.Collections.Generic;

namespace CentiShields.Mosquitoes
{
    sealed class MosquitoGraphics : GraphicsModule
    {
        const int meshSegs = 9;

        const float squeeze = -0.1f;
        const float squirmAdd = 0;
        const float squirmWidth = 0;
        const float squirmAmp = 0;

        //const int UntilSleepDelay = 340;
        //float squirmAddGetTo;
        //float squirmWidthGetTo;
        //float squirmAmpGetTo;

        readonly Mosquito bug;
        readonly Vector2[,] body = new Vector2[2, 3];
        readonly float[,] squirm = new float[meshSegs, 3];
        readonly float sizeFac;

        float squirmOffset;
        float darkness;
        float lastDarkness;
        Color yellow;
        float wingFlap;
        float lastWingFlap;
        RoomPalette roomPalette;

        readonly TriangleMesh[] m = new TriangleMesh[2]; // mesh sprites 0 and 1
        readonly CustomFSprite[] w = new CustomFSprite[2]; // wing sprites 0 and 1

        public MosquitoGraphics(Mosquito bug) : base(bug, false)
        {
            this.bug = bug;

            int seed = UnityEngine.Random.seed;
            UnityEngine.Random.seed = bug.abstractCreature.ID.RandomSeed;

            sizeFac = Custom.ClampedRandomVariation(0.8f, 0.2f, 0.5f);
            body = new Vector2[2, 3];

            UnityEngine.Random.seed = seed;
        }

        public override void Reset()
        {
            base.Reset();
            Vector2 dir = Custom.RNV();
            for (int i = 0; i < body.GetLength(0); i++) {
                body[i, 0] = bug.firstChunk.pos - dir * i;
                body[i, 1] = body[i, 0];
                body[i, 2] *= 0f;
            }
        }

        public override void Update()
        {
            base.Update();
            if (culled) {
                return;
            }

            lastWingFlap = wingFlap;
            wingFlap += (0.4f + UnityEngine.Random.value * 0.05f) * (bug.Consious && bug.grasps[0] == null ? 1f : 0f);

            for (int i = 0; i < body.GetLength(0); i++) {
                body[i, 1] = body[i, 0];
                body[i, 0] += body[i, 2];
                body[i, 2] *= bug.airFriction;
                body[i, 2].y -= bug.gravity;
                body[i, 2] += bug.bloat * -bug.needleDir * 3f;
            }
            for (int j = 0; j < body.GetLength(0); j++) {
                SharedPhysics.TerrainCollisionData terrainCollisionData = new(body[j, 0], body[j, 1], body[j, 2], (2.5f - j * 0.5f) * sizeFac, default, bug.firstChunk.goThroughFloors);
                terrainCollisionData = SharedPhysics.VerticalCollision(bug.room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.HorizontalCollision(bug.room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.SlopesVertically(bug.room, terrainCollisionData);
                body[j, 0] = terrainCollisionData.pos;
                body[j, 2] = terrainCollisionData.vel;
                if (terrainCollisionData.contactPoint.y < 0) {
                    body[j, 2].x *= 0.4f;
                }
                if (j == 0) {
                    Vector2 a = Custom.DirVec(body[j, 0], bug.firstChunk.pos) * (Vector2.Distance(body[j, 0], bug.firstChunk.pos) - 5f * sizeFac);
                    body[j, 0] += a;
                    body[j, 2] += a;
                } else {
                    Vector2 a = Custom.DirVec(body[j, 0], body[j - 1, 0]) * (Vector2.Distance(body[j, 0], body[j - 1, 0]) - 5f * sizeFac);
                    body[j, 0] += a * 0.5f;
                    body[j, 2] += a * 0.5f;
                    body[j - 1, 0] -= a * 0.5f;
                    body[j - 1, 2] -= a * 0.5f;
                }
            }
            float d = Mathf.Pow(Mathf.InverseLerp(0.25f, -0.75f, Vector2.Dot((bug.firstChunk.pos - body[0, 0]).normalized, (body[0, 0] - body[1, 0]).normalized)), 2f);
            body[1, 2] -= Custom.DirVec(body[1, 0], bug.firstChunk.pos) * d * 3f * sizeFac;
            body[1, 0] -= Custom.DirVec(body[1, 0], bug.firstChunk.pos) * d * 3f * sizeFac;

            bug.needleDir = (bug.needleDir + Custom.DirVec(body[0, 0], bug.firstChunk.pos) * 0.2f).normalized;

            squirmOffset += squirmAdd * 0.2f;

            for (int k = 0; k < squirm.GetLength(0); k++) {
                squirm[k, 1] = squirm[k, 0];
                squirm[k, 0] = Mathf.Sin(squirmOffset + k * Mathf.Lerp(0.5f, 2f, squirmWidth)) * squirmAmp * (1f - bug.bloat);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];
            sLeaser.sprites[0] = m[0] = TriangleMesh.MakeLongMesh(meshSegs, false, true);
            sLeaser.sprites[1] = m[1] = TriangleMesh.MakeLongMesh(meshSegs - 3, false, true);
            for (int i = 0; i < 2; i++) {
                sLeaser.sprites[2 + i] = w[i] = new CustomFSprite("CentipedeWing") {
                    shader = rCam.room.game.rainWorld.Shaders["CicadaWing"]
                };
            }

            AddToContainer(sLeaser, rCam, null);

            base.InitiateSprites(sLeaser, rCam);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");

            for (int i = 0; i < sLeaser.sprites.Length; i++) {
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (culled) {
                return;
            }

            Vector2 chk0Pos = Vector2.Lerp(bug.firstChunk.lastPos, bug.firstChunk.pos, timeStacker);
            Vector2 bodyPos = Vector2.Lerp(body[0, 1], body[0, 0], timeStacker);
            Vector2 headPos = Vector2.Lerp(body[1, 1], body[1, 0], timeStacker);
            Vector2 segmentDir = -Vector3.Slerp(bug.lastNeedleDir, bug.needleDir, timeStacker);
            Vector2 chkDir = Custom.DirVec(chk0Pos, bodyPos);
            Vector2 bodyDir = Custom.DirVec(bodyPos, headPos);

            if (bug.room != null) {
                lastDarkness = darkness;
                darkness = bug.room.DarknessOfPoint(rCam, bodyPos);
                if (darkness != lastDarkness) {
                    ApplyPalette(sLeaser, rCam, rCam.currentPalette);
                }
            }

            chk0Pos -= segmentDir * 7f * sizeFac;
            headPos += chkDir * (7f * (1f - bug.bloat)) * sizeFac;
            Vector2 vector4 = chk0Pos - segmentDir * 18f;
            Vector2 v = vector4;
            float num = 0f;
            float num2 = Custom.LerpMap(Vector2.Distance(chk0Pos, bodyPos) + Vector2.Distance(bodyPos, headPos), 20f, 140f, 1f, 0.3f, 2f);
            Vector2 a4 = Custom.DegToVec(-45f);

            for (int i = 0; i < meshSegs; i++) {
                float num4 = Mathf.InverseLerp(1f, meshSegs - 1, i);
                float num5 = i < 2 ? (0.5f + i) : (Custom.LerpMap(num4, 0.5f, 1f, Mathf.Lerp(3f, 2.5f, num4), 1f, 3f) * num2);
                if (bug.bloat > 0f && i > 1) {
                    num5 = Mathf.Lerp(num5 * (1.2f + 0.65f * Mathf.Sin(3.1415927f * num4) * bug.bloat * 2f), 1f, (0.5f + 0.5f * squeeze) * Mathf.InverseLerp(1f - squeeze - 0.1f, 1f - squeeze + 0.1f, num4));
                }
                num5 *= sizeFac;

                Vector2 vector5;
                if (i == 0) {
                    vector5 = chk0Pos - segmentDir * 4f;
                } else if (num4 < 0.5f) {
                    vector5 = Custom.Bezier(chk0Pos, chk0Pos + segmentDir * 2f, bodyPos, bodyPos - chkDir * 4f, Mathf.InverseLerp(0f, 0.5f, num4));
                } else {
                    vector5 = Custom.Bezier(bodyPos, bodyPos + chkDir * 4f, headPos, headPos - bodyDir * 2f, Mathf.InverseLerp(0.5f, 1f, num4));
                }

                Vector2 vector6 = vector5;
                Vector2 a5 = Custom.PerpendicularVector(vector6, v);
                vector5 += a5 * Mathf.Lerp(squirm[i, 1], squirm[i, 0], timeStacker) * num5 * (num4 * 0.3f + Mathf.Sin(num4 * 3.1415927f));
                Vector2 a6 = Custom.PerpendicularVector(vector5, vector4);
                m[0].MoveVertice(i * 4, (vector4 + vector5) / 2f - a6 * (num5 + num) * 0.5f - camPos);
                m[0].MoveVertice(i * 4 + 1, (vector4 + vector5) / 2f + a6 * (num5 + num) * 0.5f - camPos);
                m[0].MoveVertice(i * 4 + 2, vector5 - a6 * num5 - camPos);
                m[0].MoveVertice(i * 4 + 3, vector5 + a6 * num5 - camPos);

                if (i > 1 && i < meshSegs - 1) {
                    float d = Mathf.Lerp(0.2f, 0.5f, Mathf.Sin(3.1415927f * Mathf.Pow(Mathf.InverseLerp(2f, meshSegs - 2, i), 0.5f)));
                    m[1].MoveVertice((i - 2) * 4, (vector4 + a4 * num * d + vector5 + a4 * num5 * d) / 2f - a6 * (num5 + num) * 0.5f * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 1, (vector4 + a4 * num * d + vector5 + a4 * num5 * d) / 2f + a6 * (num5 + num) * 0.5f * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 2, vector5 + a4 * num5 * d - a6 * num5 * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 3, vector5 + a4 * num5 * d + a6 * num5 * d - camPos);
                }

                vector4 = vector5;
                v = vector6;
                num = num5;
            }

            const float wingsSize = .7f;

            for (int m = 0; m < 2; m++) {
                Vector2 firstChunkPos = Vector2.Lerp(bug.firstChunk.lastPos, bug.firstChunk.pos, timeStacker);

                /*
				Vector2 vector10 = this.GraphSegmentPos(this.wings[l, m].connection.index + this.snout.Length, timeStacker);
				if (m == 0)
				{
					vector10 = this.GraphSegmentPos(1 + this.snout.Length, timeStacker);
				}
				
                Vector2 vector11 = Vector2.Lerp(this.wings[l, m].lastPos, this.wings[l, m].pos, timeStacker);
                Vector2 vector11 = vector10 + Custom.PerpendicularVector(a) * (m == 0 ? 1f : -1f) * 1f;
                Vector2 segmentDir = a;
                vector10 -= Custom.PerpendicularVector(segmentDir) * wingsSize * 5f * Mathf.Abs(vector3.y) * ((m != 0) ? 1f : -1f);
                vector11.y -= (18f + 18f * Mathf.Sin((Mathf.Lerp(this.lastWingFlap, this.wingFlap, timeStacker) + ((m != 0) ? 0f : 0.33f)) * 3.1415927f * 2f)) * num * wingsSize;
                vector11 = vector10 + Custom.DirVec(vector10, vector11) * Mathf.Lerp(40f, 60f, num) * wingsSize;
                */

                Vector2 wingVert = firstChunkPos;

                if (bug.Consious && bug.grasps[0] == null) {
                    Vector2 wingVertOff = new(m == 0 ? 1f : -1f, Mathf.Sin((Mathf.Lerp(lastWingFlap, wingFlap, timeStacker) + (m == 0 ? 0.33f : 0f)) * Mathf.PI * 2f) * .8f);

                    wingVert += (wingVertOff + segmentDir * .1f).normalized * wingsSize * 33f;
                } else {
                    wingVert += (segmentDir + Custom.PerpendicularVector(segmentDir) * (m == 0 ? 1f : -1f) * .2f).normalized * wingsSize * 33f;
                }

                Vector2 offset2 = Vector3.Slerp(Custom.PerpendicularVector(segmentDir) * (m == 0 ? -1f : 1f), new Vector2(m == 0 ? -1f : 1f, 0f), num);
                // int num7 = (m == 0 != vector3.x > 0f) ? 1 : 0;

                w[m].MoveVertice(1, wingVert + offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(0, wingVert - offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(2, firstChunkPos + offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(3, firstChunkPos - offset2 * 2f * wingsSize - camPos);

                // sLeaser.sprites[2 + m].isVisible = (!this.small || (this.worm as SmallNeedleWorm).bites > 4);
            }

            ApplyPalette(sLeaser, rCam, roomPalette);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            roomPalette = palette;

            yellow = Color.Lerp(new Color(0.95f, 0.8f, 0.55f), palette.fogColor, 0.2f);
            yellow = Color.Lerp(yellow, palette.blackColor, .8f);

            Color to = new(1f, 0f, 0f);
            Color wColors = new(1f, 1f, 1f);

            if (darkness > 0f) {
                yellow = Color.Lerp(yellow, palette.blackColor, darkness);
                to = Color.Lerp(to, palette.blackColor, darkness);
                wColors = Color.Lerp(wColors, palette.blackColor, Mathf.Pow(darkness, 1.5f));
            }

            for (int i = 0; i < m[0].verticeColors.Length; i++) {
                float value = Mathf.InverseLerp(0f, m[0].verticeColors.Length - 1, i);
                m[0].verticeColors[i] = Color.Lerp(yellow, to, 0.25f + Mathf.InverseLerp(0f, 0.2f + 0.8f, value) * 0.75f * bug.bloat);
            }

            m[0].verticeColors[0] = wColors;
            m[0].verticeColors[1] = wColors;

            for (int j = 0; j < m[1].verticeColors.Length; j++) {
                float value2 = Mathf.InverseLerp(0f, m[1].verticeColors.Length - 1, j);
                m[1].verticeColors[j] = Custom.RGB2RGBA(wColors, Mathf.InverseLerp(0f, 0.2f, value2) * (1f - 0.75f - 0.25f));
            }

            for (int n = 0; n < 2; n++) {
                w[n].verticeColors[2] = Color.Lerp(palette.fogColor, yellow, 0.5f);
                w[n].verticeColors[3] = Color.Lerp(palette.fogColor, yellow, 0.5f);
                w[n].verticeColors[0] = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f);
                w[n].verticeColors[1] = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f);
            }
        }
    }
}

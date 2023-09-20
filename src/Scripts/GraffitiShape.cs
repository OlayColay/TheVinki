using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Vinki;
// Token: 0x0200067D RID: 1661
public partial class GraffitiHolder
{
    public class Shape
    {
        // Token: 0x1700093B RID: 2363
        // (get) Token: 0x060038DA RID: 14554 RVA: 0x0040BB9B File Offset: 0x00409D9B
        public GraffitiHolder.Shape.FloatChanger MainRotation
        {
            get
            {
                return this.floatChangers[0];
            }
        }

        // Token: 0x1700093C RID: 2364
        // (get) Token: 0x060038DB RID: 14555 RVA: 0x0040BBA5 File Offset: 0x00409DA5
        public GraffitiHolder.Shape.FloatChanger Height
        {
            get
            {
                return this.floatChangers[1];
            }
        }

        // Token: 0x1700093D RID: 2365
        // (get) Token: 0x060038DC RID: 14556 RVA: 0x0040BBAF File Offset: 0x00409DAF
        public GraffitiHolder.Shape.FloatChanger ShapeA
        {
            get
            {
                return this.floatChangers[2];
            }
        }

        // Token: 0x1700093E RID: 2366
        // (get) Token: 0x060038DD RID: 14557 RVA: 0x0040BBB9 File Offset: 0x00409DB9
        public GraffitiHolder.Shape.FloatChanger ShapeB
        {
            get
            {
                return this.floatChangers[3];
            }
        }

        // Token: 0x1700093F RID: 2367
        // (get) Token: 0x060038DE RID: 14558 RVA: 0x0040BBC3 File Offset: 0x00409DC3
        public GraffitiHolder.Shape.FloatChanger Errors
        {
            get
            {
                return this.floatChangers[4];
            }
        }

        // Token: 0x17000940 RID: 2368
        // (get) Token: 0x060038DF RID: 14559 RVA: 0x0040BBCD File Offset: 0x00409DCD
        public GraffitiHolder.Shape.FloatChanger Fade
        {
            get
            {
                return this.floatChangers[5];
            }
        }

        // Token: 0x17000941 RID: 2369
        // (get) Token: 0x060038E0 RID: 14560 RVA: 0x0040BBD7 File Offset: 0x00409DD7
        public GraffitiHolder.Shape.FloatChanger DRotA
        {
            get
            {
                return this.floatChangers[6];
            }
        }

        // Token: 0x17000942 RID: 2370
        // (get) Token: 0x060038E1 RID: 14561 RVA: 0x0040BBE1 File Offset: 0x00409DE1
        public GraffitiHolder.Shape.FloatChanger DRotB
        {
            get
            {
                return this.floatChangers[7];
            }
        }

        // Token: 0x17000943 RID: 2371
        // (get) Token: 0x060038E2 RID: 14562 RVA: 0x0040BBEC File Offset: 0x00409DEC
        public int LinesCount
        {
            get
            {
                int num = this.holoLines.Count;
                for (int i = 0; i < this.subShapes.Count; i++)
                {
                    num += this.subShapes[i].LinesCount;
                }
                return num;
            }
        }

        // Token: 0x060038E3 RID: 14563 RVA: 0x0040BC30 File Offset: 0x00409E30
        public Shape(GraffitiHolder.Shape owner, GraffitiHolder.Shape.ShapeType shapeType, Vector3 pos, float width, float height)
        {
            this.owner = owner;
            this.shapeType = shapeType;
            this.pos = pos;
            this.startPos = pos;
            this.lastPos = pos;
            if (shapeType == GraffitiHolder.Shape.ShapeType.Main)
            {
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Shell, pos, 9f, 22f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Belt, pos, 25f, 6f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.DiamondHolder, pos, 35f, 0f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Cube, pos, 27f, 27f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Ribbon, pos, 44f, 7f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Sphere, pos, 36f, 28f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.BigDiamonds, pos, 39f, 11f));
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Shell)
            {
                int num = 5;
                this.verts.Add(new GraffitiHolder.Shape.Vert(0f, 0f, -height));
                this.verts[0].B *= 0.5f;
                this.verts[0].C *= 1.5f;
                this.verts.Add(new GraffitiHolder.Shape.Vert(0f, 0f, height));
                this.verts[1].B *= 0.5f;
                this.verts[1].C *= 1.5f;
                for (int i = 0; i < num; i++)
                {
                    Vector2 vector = Custom.DegToVec((float)i / (float)num * 360f) * width;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector.x, vector.y, -height / 2.2f));
                    this.verts[this.verts.Count - 1].B *= 1.2f;
                    GraffitiHolder.Shape.Vert vert = this.verts[this.verts.Count - 1];
                    vert.B.z = vert.B.z * 0.2f;
                    this.verts[this.verts.Count - 1].C *= 0.8f;
                    GraffitiHolder.Shape.Vert vert2 = this.verts[this.verts.Count - 1];
                    vert2.C.z = vert2.C.z * 2f;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector.x, vector.y, height / 2.2f));
                    this.verts[this.verts.Count - 1].B *= 1.2f;
                    GraffitiHolder.Shape.Vert vert3 = this.verts[this.verts.Count - 1];
                    vert3.B.z = vert3.B.z * 0.2f;
                    this.verts[this.verts.Count - 1].C *= 0.8f;
                    GraffitiHolder.Shape.Vert vert4 = this.verts[this.verts.Count - 1];
                    vert4.C.z = vert4.C.z * 2f;
                }
                for (int j = 0; j < num; j++)
                {
                    int num2 = (j < num - 1) ? (j + 1) : 0;
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[2 + j * 2], this.verts[0]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[2 + j * 2], this.verts[2 + num2 * 2]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[2 + j * 2 + 1], this.verts[2 + num2 * 2 + 1]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[2 + j * 2], this.verts[2 + j * 2 + 1]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[2 + j * 2 + 1], this.verts[1]));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Belt)
            {
                int num = 7;
                for (int k = 0; k < num; k++)
                {
                    Vector2 vector2 = Custom.DegToVec((float)k / (float)num * 360f);
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector2.x * width, vector2.y * width, -height * 1.2f));
                    this.verts[this.verts.Count - 1].B = new Vector3(vector2.x * (width - height * 0.25f), vector2.y * (width - height * 0.25f), 0f);
                    this.verts[this.verts.Count - 1].C = new Vector3(vector2.x * (width + height * 2f), vector2.y * (width + height * 1.5f), 0f);
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector2.x * width, vector2.y * width, height * 1.2f));
                    this.verts[this.verts.Count - 1].B = new Vector3(vector2.x * (width + height * 2f), vector2.y * (width + height * 2f), 0f);
                    this.verts[this.verts.Count - 1].C = new Vector3(vector2.x * (width - height * 0.25f), vector2.y * (width - height * 0.25f), 0f);
                }
                for (int l = 0; l < num; l++)
                {
                    int num3 = (l < num - 1) ? (l + 1) : 0;
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[l * 2], this.verts[num3 * 2]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[l * 2 + 1], this.verts[num3 * 2 + 1]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[l * 2], this.verts[l * 2 + 1]));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.DiamondHolder)
            {
                int num = 5;
                for (int m = 0; m < num; m++)
                {
                    Vector2 vector3 = Custom.DegToVec((float)m / (float)num * 360f) * width;
                    this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Diamond, new Vector3(vector3.x, vector3.y, 0f), 3f, 5f));
                }
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.SmallDiamondHolder, pos + new Vector3(0f, 0f, -22f), 20f, 0f));
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.SmallDiamondHolder, pos + new Vector3(0f, 0f, 22f), 20f, 0f));
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.SmallDiamondHolder)
            {
                int num = 3;
                for (int n = 0; n < num; n++)
                {
                    Vector2 vector4 = Custom.DegToVec((float)n / (float)num * 360f) * width;
                    this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.Diamond, new Vector3(vector4.x, vector4.y, 0f), 3f, 5f));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Diamond)
            {
                this.verts.Add(new GraffitiHolder.Shape.Vert(-width, 0f, 0f));
                this.verts.Add(new GraffitiHolder.Shape.Vert(0f, 0f, height));
                this.verts.Add(new GraffitiHolder.Shape.Vert(width, 0f, 0f));
                this.verts.Add(new GraffitiHolder.Shape.Vert(0f, 0f, -height));
                int num = 4;
                for (int num4 = 0; num4 < num; num4++)
                {
                    int index = (num4 < num - 1) ? (num4 + 1) : 0;
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num4], this.verts[index]));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Cube)
            {
                int num = 4;
                for (int num5 = 0; num5 < num; num5++)
                {
                    Vector2 vector5 = Custom.DegToVec((float)num5 / (float)num * 360f) * width * 1.42f;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector5.x, vector5.y, -height));
                    this.verts[this.verts.Count - 1].B = this.MultVec(this.verts[this.verts.Count - 1].B, new Vector3(1.4f, 1.4f, 0.2f));
                    this.verts[this.verts.Count - 1].C *= 0.2f;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector5.x, vector5.y, height));
                    this.verts[this.verts.Count - 1].B = this.MultVec(this.verts[this.verts.Count - 1].B, new Vector3(1.4f, 1.4f, 0.2f));
                    this.verts[this.verts.Count - 1].C *= 0.2f;
                }
                for (int num6 = 0; num6 < num; num6++)
                {
                    int num7 = (num6 < num - 1) ? (num6 + 1) : 0;
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num6 * 2], this.verts[num7 * 2]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num6 * 2 + 1], this.verts[num7 * 2 + 1]));
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num6 * 2], this.verts[num6 * 2 + 1]));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Ribbon)
            {
                int num = 22;
                for (int num8 = 0; num8 < num; num8++)
                {
                    Vector2 vector6 = Custom.DegToVec((float)num8 / (float)num * 360f) * width;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector6.x, vector6.y, -height));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector6.x, vector6.y, height));
                    vector6 = Custom.DegToVec(((float)num8 + ((num8 % 2 == 0) ? -0.5f : 0.5f)) / (float)num * 360f) * width;
                    this.verts[this.verts.Count - 2].B = new Vector3(vector6.x, vector6.y, -height * 0.75f);
                    this.verts[this.verts.Count - 1].B = new Vector3(vector6.x, vector6.y, height * 0.75f);
                    vector6 = Custom.DegToVec(((float)num8 + ((num8 % 2 == 0) ? 0.5f : -0.5f)) / (float)num * 360f) * width;
                    this.verts[this.verts.Count - 2].C = new Vector3(vector6.x, vector6.y, -height * 1.5f);
                    this.verts[this.verts.Count - 1].C = new Vector3(vector6.x, vector6.y, height * 1.5f);
                }
                for (int num9 = 0; num9 < num; num9++)
                {
                    int num10 = (num9 < num - 1) ? (num9 + 1) : 0;
                    if (num9 % 2 == 0)
                    {
                        this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num9 * 2], this.verts[num10 * 2]));
                        this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num9 * 2 + 1], this.verts[num10 * 2 + 1]));
                    }
                    this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num9 * 2], this.verts[num9 * 2 + 1]));
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.Sphere)
            {
                int num = 18;
                for (int num11 = 0; num11 < 2; num11++)
                {
                    for (int num12 = 0; num12 < num; num12++)
                    {
                        Vector2 vector7 = Custom.DegToVec((float)num12 / (float)num * 360f) * width;
                        this.verts.Add(new GraffitiHolder.Shape.Vert(vector7.x, vector7.y, height * 0.5f * (float)((num11 == 0) ? -1 : 1)));
                        this.verts.Add(new GraffitiHolder.Shape.Vert(vector7.x * 0.72f, vector7.y * 0.75f, height * (float)((num11 == 0) ? -1 : 1)));
                        vector7 = Custom.DegToVec(((float)num12 + ((num12 % 2 == num11) ? -0.5f : 0.5f)) / (float)num * 360f) * width;
                        this.verts[this.verts.Count - 2].B = new Vector3(vector7.x, vector7.y, height * 0.5f * (float)((num11 == 0) ? -1 : 1));
                        this.verts[this.verts.Count - 1].B = new Vector3(vector7.x * 0.72f, vector7.y * 0.72f, height * (float)((num11 == 0) ? -1 : 1));
                        vector7 = Custom.DegToVec(((float)num12 + ((num12 % 2 != num11) ? -0.5f : 0.5f)) / (float)num * 360f) * width;
                        this.verts[this.verts.Count - 2].C = new Vector3(vector7.x, vector7.y, height * 0.5f * (float)((num11 == 0) ? -1 : 1));
                        this.verts[this.verts.Count - 1].C = new Vector3(vector7.x * 0.72f, vector7.y * 0.72f, height * (float)((num11 == 0) ? -1 : 1));
                    }
                }
                for (int num13 = 0; num13 < 2; num13++)
                {
                    for (int num14 = 0; num14 < num; num14++)
                    {
                        int num15 = (num14 < num - 1) ? (num14 + 1) : 0;
                        if (num14 % 2 == num13)
                        {
                            this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num * 2 * num13 + num14 * 2], this.verts[num * 2 * num13 + num15 * 2]));
                            this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num * 2 * num13 + num14 * 2 + 1], this.verts[num * 2 * num13 + num15 * 2 + 1]));
                        }
                        this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num * 2 * num13 + num14 * 2], this.verts[num * 2 * num13 + num14 * 2 + 1]));
                    }
                }
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds)
            {
                int num = 7;
                for (int num16 = 0; num16 < num; num16++)
                {
                    Vector2 vector8 = Custom.DegToVec((float)num16 / (float)num * 360f) * (width - height / 3f);
                    Vector2 vector9 = Custom.DegToVec(((float)num16 - 0.15f) / (float)num * 360f) * width;
                    Vector2 vector10 = Custom.DegToVec(((float)num16 + 0.15f) / (float)num * 360f) * width;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector8.x, vector8.y, -height));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector9.x, vector9.y, 0f));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector8.x, vector8.y, height));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector10.x, vector10.y, 0f));
                    for (int num17 = 0; num17 < 4; num17++)
                    {
                        int num18 = (num17 < 3) ? (num17 + 1) : 0;
                        this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num16 * 4 + num17], this.verts[num16 * 4 + num18]));
                    }
                }
                this.subShapes.Add(new GraffitiHolder.Shape(this, GraffitiHolder.Shape.ShapeType.BigDiamonds2, pos, width, height - 3.5f));
                return;
            }
            if (shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds2)
            {
                int num = 7;
                for (int num19 = 0; num19 < num; num19++)
                {
                    Vector2 vector11 = Custom.DegToVec((float)num19 / (float)num * 360f) * (width - height / 5f);
                    Vector2 vector12 = Custom.DegToVec(((float)num19 - 0.08f) / (float)num * 360f) * width;
                    Vector2 vector13 = Custom.DegToVec(((float)num19 + 0.08f) / (float)num * 360f) * width;
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector11.x, vector11.y, -height));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector12.x, vector12.y, 0f));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector11.x, vector11.y, height));
                    this.verts.Add(new GraffitiHolder.Shape.Vert(vector13.x, vector13.y, 0f));
                    for (int num20 = 0; num20 < 4; num20++)
                    {
                        int num21 = (num20 < 3) ? (num20 + 1) : 0;
                        this.holoLines.Add(new GraffitiHolder.Shape.Line(this.verts[num19 * 4 + num20], this.verts[num19 * 4 + num21]));
                    }
                }
            }
        }

        // Token: 0x060038E4 RID: 14564 RVA: 0x0040D10C File Offset: 0x0040B30C
        public void Update(bool changeLikely, float errors, float fade, Vector2 movement, float upRotat, ref float[,] directionsPower)
        {
            this.lastPos = this.pos;
            fade = Mathf.Min(fade, this.Fade.SmoothValue(1f));
            errors = Mathf.Max(Mathf.Max(errors, this.Errors.SmoothValue(1f)), (1f - fade) * 0.25f);
            if (errors > 0.5f && Random.value < 0.25f)
            {
                fade *= Custom.LerpMap(errors * Random.value, 0.5f, 1f, 1f, 0.3f);
            }
            for (int i = 0; i < this.subShapes.Count; i++)
            {
                this.subShapes[i].Update(changeLikely, errors, fade, movement, upRotat, ref directionsPower);
            }
            for (int j = 0; j < this.floatChangers.Length; j++)
            {
                this.floatChangers[j].Update();
            }
            if (Random.value < 1f / (changeLikely ? 6f : 600f))
            {
                if (this.shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds2 && Random.value < 0.8f)
                {
                    this.MainRotation.NewGoal(0f, 2f, 60f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.6f);
                }
                else
                {
                    this.MainRotation.NewGoal(this.MainRotation.to + Mathf.Lerp(-360f, 360f, Random.value), 2f, 60f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.3f);
                }
            }
            if (this.shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds2)
            {
                if (Random.value < 0.8f)
                {
                    this.dRotA = this.owner.dRotA;
                    this.DRotA.NewGoal((float)this.dRotA * 3.1415927f, 0.033333335f, 30f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                    this.dRotB = this.owner.dRotB;
                    this.DRotB.NewGoal((float)this.dRotB * 3.1415927f, 0.033333335f, 30f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
            }
            else
            {
                this.pos.z = this.startPos.z + Mathf.Lerp(-10f, 10f, this.Height.SmoothValue(1f));
            }
            if (Random.value < 1f / (changeLikely ? 6f : 600f))
            {
                this.Height.NewGoal(Random.value, 0.016666668f, 60f, 0.5f);
            }
            if (Random.value < 1f / (changeLikely ? 6f : 600f))
            {
                this.ShapeA.NewGoal((Random.value < 0.5f) ? 0f : Mathf.Pow(Random.value, 0.6f), 0.016666668f, 60f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
            }
            if (Random.value < 1f / (changeLikely ? 6f : 600f))
            {
                this.ShapeB.NewGoal(Custom.PushFromHalf(Random.value, 2f), 0.016666668f, 60f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
            }
            if (Random.value < 0.016666668f && this.verts.Count > 0)
            {
                this.shakeError = (Random.value < 0.5f);
                this.Errors.NewGoal((Random.value < Mathf.Lerp(0.95f, 0.82f, errors)) ? 0f : Mathf.Pow(Random.value, 0.75f), 0.025f, 60f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
            }
            if (Random.value < 1f / (changeLikely ? 15f : 1500f))
            {
                if (this.owner != null && this.owner.owner != null && this.owner.Fade.to == 0f)
                {
                    this.Fade.NewGoal(0f, 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
                else if (this.shapeType == GraffitiHolder.Shape.ShapeType.Cube || this.shapeType == GraffitiHolder.Shape.ShapeType.Ribbon || this.shapeType == GraffitiHolder.Shape.ShapeType.Sphere || this.shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds)
                {
                    float num = 0.2f;
                    for (int k = 0; k < this.owner.subShapes.Count; k++)
                    {
                        if (this.owner.subShapes[k] != this && (this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.Ribbon || this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.Cube || this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.Sphere || this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.Belt || this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds) && this.owner.subShapes[k].Fade.to != 0f)
                        {
                            num *= ((this.owner.subShapes[k].shapeType == GraffitiHolder.Shape.ShapeType.Belt) ? 0.5f : 0f);
                        }
                    }
                    this.Fade.NewGoal((Random.value < num) ? 1f : 0f, 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
                else
                {
                    this.Fade.NewGoal((Random.value < 0.75f) ? 1f : Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, Random.value), 0.5f), 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
            }
            if (this.owner == null)
            {
                this.Fade.from = 1f;
                this.Fade.to = 1f;
                this.Fade.prog = 1f;
            }
            if (Random.value < 1f / (changeLikely ? 22f : 2200f) && (this.shapeType == GraffitiHolder.Shape.ShapeType.Shell || this.shapeType == GraffitiHolder.Shape.ShapeType.Belt || this.shapeType == GraffitiHolder.Shape.ShapeType.Cube || this.shapeType == GraffitiHolder.Shape.ShapeType.Ribbon || this.shapeType == GraffitiHolder.Shape.ShapeType.Sphere || (Random.value < 0.4f && (this.shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds || this.shapeType == GraffitiHolder.Shape.ShapeType.BigDiamonds2))))
            {
                if (Random.value < 0.5f)
                {
                    this.dRotA += ((Random.value < 0.5f) ? -1 : 1);
                    this.DRotA.NewGoal((float)this.dRotA * 3.1415927f, 0.033333335f, 30f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
                if (Random.value < 0.5f)
                {
                    this.dRotB += ((Random.value < 0.5f) ? -1 : 1);
                    this.DRotB.NewGoal((float)this.dRotB * 3.1415927f, 0.033333335f, 30f * Mathf.Lerp(0.5f, 1.5f, Random.value), 0.5f);
                }
            }
            float num2 = this.SmoothRotat(1f);
            Vector3 b = this.MultVec(this.SmoothPos(1f), new Vector3(Mathf.Pow(fade, 0.3f), Mathf.Pow(fade, 0.3f), fade));
            for (int l = 0; l < this.verts.Count; l++)
            {
                this.verts[l].lastPos = this.verts[l].pos;
                Vector3 vector = this.Rotate(Vector3.Lerp(this.verts[l].A, Vector3.Lerp(this.verts[l].B, this.verts[l].C, this.ShapeB.SmoothValue(1f)), this.ShapeA.SmoothValue(1f)), 1.5707964f + this.DRotA.SmoothValue(1f), 0f + this.DRotB.SmoothValue(1f), num2 * 0.017453292f) + b;
                Vector2 vector2 = Custom.RotateAroundOrigo(new Vector2(vector.x, vector.y), upRotat);
                vector.x = vector2.x;
                vector.y = vector2.y;
                this.verts[l].errors = Mathf.Lerp(this.verts[l].errors, errors, 0.07f);
                if (Random.value < 0.071428575f)
                {
                    this.verts[l].errors = errors;
                }
                if (Random.value < 0.071428575f)
                {
                    if (this.verts[l].errors > 0.1f && Random.value < 0.1f && fade > 0.3f)
                    {
                        this.verts[l].on = !this.verts[l].on;
                    }
                    else if (this.verts[l].errors > 0.45f && Random.value < 0.2f * this.verts[l].errors)
                    {
                        this.verts[l].on = false;
                    }
                    else if (this.verts[l].errors < 0.45f && fade > 0.7f)
                    {
                        this.verts[l].on = true;
                    }
                }
                this.verts[l].errorDrift *= Mathf.Lerp(0.4f, 0.8f, this.verts[l].errors);
                this.verts[l].errorDrift = Vector3.Lerp(this.verts[l].errorDrift, this.verts[l].errorDriftTarget, 0.1f * Random.value * this.verts[l].errors);
                if (Random.value < 0.1f)
                {
                    this.verts[l].errorDriftTarget = Random.insideUnitSphere * 0.2f;
                }
                if (this.verts[l].errors < 0.1f || Random.value < 1f / Mathf.Lerp(10f, 100f, this.verts[l].errors))
                {
                    this.verts[l].pos = vector;
                }
                else
                {
                    if (this.verts[l].errors > 0.75f)
                    {
                        this.verts[l].pos = Vector3.Lerp(this.verts[l].pos, new Vector3(0f, 0f, 0f), 0.5f * Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, this.verts[l].errors), 5f) * Random.value);
                    }
                    this.verts[l].pos += this.verts[l].errorDrift;
                    this.verts[l].pos -= new Vector3(movement.x, movement.y, 0f);
                    this.verts[l].pos = Vector3.Slerp(this.verts[l].pos, vector, Custom.LerpMap(Vector3.Distance(this.verts[l].pos, vector), 20f + 35f * Random.value * errors, 7f, 0.1f, Mathf.Lerp(0.03f, 0.01f, Mathf.Pow(this.verts[l].errors, 0.5f))));
                }
                this.verts[l].pos = Vector3.Lerp(new Vector3(0f, 0f, 0f), this.verts[l].pos, Mathf.Pow(fade, 0.2f));
                if (this.verts[l].on && fade > 0f)
                {
                    int num3 = Mathf.RoundToInt((360f + Custom.VecToDeg(this.verts[l].pos) + upRotat) * ((float)directionsPower.GetLength(0) / 360f)) % directionsPower.GetLength(0);
                    directionsPower[num3, 2] = Mathf.Lerp(directionsPower[num3, 2], 1f, fade * (1f - this.verts[l].errors));
                }
            }
        }

        // Token: 0x060038E5 RID: 14565 RVA: 0x0040DFC4 File Offset: 0x0040C1C4
        public void ResetUpdate(Vector2 v)
        {
            for (int i = 0; i < this.verts.Count; i++)
            {
                this.verts[i].drawPos = v;
            }
            this.Fade.from = 0f;
            this.Fade.to = 0f;
            this.Fade.prog = 1f;
            for (int j = 0; j < this.subShapes.Count; j++)
            {
                this.subShapes[j].ResetUpdate(v);
            }
        }

        // Token: 0x060038E6 RID: 14566 RVA: 0x0040E051 File Offset: 0x0040C251
        public float SmoothRotat(float timeStacker)
        {
            if (this.owner == null)
            {
                return this.MainRotation.SmoothValue(timeStacker);
            }
            return this.MainRotation.SmoothValue(timeStacker) + this.owner.SmoothRotat(timeStacker);
        }

        // Token: 0x060038E7 RID: 14567 RVA: 0x0040E084 File Offset: 0x0040C284
        protected Vector3 SmoothPos(float timeStacker)
        {
            if (this.owner == null)
            {
                return Vector3.Lerp(this.pos, this.lastPos, timeStacker);
            }
            return this.Rotate(Vector3.Lerp(this.pos, this.lastPos, timeStacker), 1.5707964f + this.DRotA.SmoothValue(timeStacker), 0f + this.DRotB.SmoothValue(timeStacker), this.owner.SmoothRotat(timeStacker) * 0.017453292f) + this.owner.SmoothPos(timeStacker);
        }

        // Token: 0x060038E8 RID: 14568 RVA: 0x0040E10B File Offset: 0x0040C30B
        private Vector3 MultVec(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
        }

        // Token: 0x060038E9 RID: 14569 RVA: 0x0040E13C File Offset: 0x0040C33C
        private float DirPow(Vector2 v, float upRotat, float timeStacker, ref float[,] directionsPower)
        {
            float num = (360f + Custom.VecToDeg(v) + upRotat) * ((float)directionsPower.GetLength(0) / 360f);
            int num2 = Mathf.FloorToInt(num);
            return Mathf.Lerp(Mathf.Lerp(directionsPower[num2 % directionsPower.GetLength(0), 1], directionsPower[(num2 + 1) % directionsPower.GetLength(0), 1], Mathf.InverseLerp((float)num2, (float)(num2 + 1), num)), Mathf.Lerp(directionsPower[num2 % directionsPower.GetLength(0), 0], directionsPower[(num2 + 1) % directionsPower.GetLength(0), 0], Mathf.InverseLerp((float)num2, (float)(num2 + 1), num)), timeStacker);
        }

        // Token: 0x060038EA RID: 14570 RVA: 0x0040E1EC File Offset: 0x0040C3EC
        public void Draw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 projPos, Vector2 camPos, ref int sprite, float upRotat, float errors, float fade, bool shakeErr, ref Vector2 pointsVec, ref float pointsWeight, ref float maxDist, ref float[,] directionsPower)
        {
            shakeErr = (shakeErr || this.shakeError);
            fade = Mathf.Min(fade, this.Fade.SmoothValue(timeStacker));
            errors = Mathf.Max(Mathf.Max(errors, this.Errors.SmoothValue(timeStacker)), (1f - fade) * 0.25f);
            if (Random.value < 0.5f && Random.value < Mathf.Pow(this.Errors.SmoothValue(timeStacker), 2f))
            {
                fade *= Random.value;
            }
            if (fade > 0f)
            {
                for (int i = 0; i < this.verts.Count; i++)
                {
                    Vector3 vector = Vector3.Lerp(this.verts[i].lastPos, this.verts[i].pos, timeStacker);
                    vector = this.MultVec(vector, new Vector3(fade, 3f - 2f * Mathf.Pow(fade, 0.4f), Mathf.Pow(fade, 0.3f)));
                    float num = this.DirPow(vector, upRotat, timeStacker, ref directionsPower);
                    vector *= Mathf.Lerp(Mathf.InverseLerp(0.4f, 0.6f, num) * num + Mathf.Sin(Mathf.InverseLerp(0.4f, 0.6f, num) * 3.1415927f), 1f, fade * (1f - errors));
                    vector = rCam.ApplyDepth(new Vector2(vector.x, vector.y) + projPos, vector.z / 4f);
                    this.verts[i].drawPos = vector;
                }
                for (int j = 0; j < this.holoLines.Count; j++)
                {
                    float num2 = (this.holoLines[j].A.errors + this.holoLines[j].B.errors) / 2f;
                    if (Random.value > 0.2f * num2 && this.holoLines[j].A.on && this.holoLines[j].B.on)
                    {
                        Vector2 vector2 = this.holoLines[j].A.drawPos;
                        Vector2 vector3 = this.holoLines[j].B.drawPos;
                        if (shakeErr && Random.value < Mathf.Pow(num2, 3f))
                        {
                            if (Random.value < num2)
                            {
                                vector2 = Vector2.Lerp(vector2, (Random.value < 0.5f) ? this.holoLines[Random.Range(0, this.holoLines.Count)].A.drawPos : this.holoLines[Random.Range(0, this.holoLines.Count)].B.drawPos, Mathf.Pow(Random.value * errors, 4f));
                            }
                            if (Random.value < num2)
                            {
                                vector3 = Vector2.Lerp(vector3, (Random.value < 0.5f) ? this.holoLines[Random.Range(0, this.holoLines.Count)].A.drawPos : this.holoLines[Random.Range(0, this.holoLines.Count)].B.drawPos, Mathf.Pow(Random.value * errors, 4f));
                            }
                        }
                        if (Random.value < num2 * (0.5f + 0.5f * fade))
                        {
                            if (Random.value < 0.5f)
                            {
                                vector2 = Vector2.Lerp(Vector2.Lerp(vector2, vector3, Random.value), projPos, Random.value * Mathf.Max(num2, 1f - fade));
                            }
                            else
                            {
                                vector3 = Vector2.Lerp(Vector2.Lerp(vector2, vector3, Random.value), projPos, Random.value * Mathf.Max(num2, 1f - fade));
                            }
                        }
                        sLeaser.sprites[sprite].x = vector2.x - camPos.x;
                        sLeaser.sprites[sprite].y = vector2.y - camPos.y;
                        sLeaser.sprites[sprite].scaleY = Vector2.Distance(vector2, vector3);
                        sLeaser.sprites[sprite].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
                        sLeaser.sprites[sprite].isVisible = true;
                        pointsVec += (vector2 + vector3) * fade;
                        pointsWeight += 2f * fade;
                        if (!Custom.DistLess(projPos, vector2, maxDist))
                        {
                            maxDist = Vector2.Distance(projPos, vector2);
                        }
                        if (!Custom.DistLess(projPos, vector3, maxDist))
                        {
                            maxDist = Vector2.Distance(projPos, vector3);
                        }
                        sLeaser.sprites[sprite].alpha = (0.9f + 0.1f * Random.value) * Mathf.Pow(fade, 0.2f);
                    }
                    else
                    {
                        sLeaser.sprites[sprite].isVisible = false;
                    }
                    sprite++;
                }
            }
            else
            {
                for (int k = 0; k < this.holoLines.Count; k++)
                {
                    sLeaser.sprites[sprite].isVisible = false;
                    sprite++;
                }
            }
            for (int l = 0; l < this.subShapes.Count; l++)
            {
                this.subShapes[l].Draw(sLeaser, rCam, timeStacker, projPos, camPos, ref sprite, upRotat, errors, fade, shakeErr, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
            }
        }

        // Token: 0x060038EB RID: 14571 RVA: 0x0040E7A4 File Offset: 0x0040C9A4
        private Vector3 Rotate(Vector3 position, float urX, float urY, float urZ)
        {
            float x = position.x * Mathf.Cos(urZ) - position.y * Mathf.Sin(urZ);
            position.y = position.x * Mathf.Sin(urZ) + position.y * Mathf.Cos(urZ);
            position.x = x;
            float y = position.y * Mathf.Cos(urX) - position.z * Mathf.Sin(urX);
            position.z = position.y * Mathf.Sin(urX) + position.z * Mathf.Cos(urX);
            position.y = y;
            float z = position.z * Mathf.Cos(urY) - position.x * Mathf.Sin(urY);
            position.x = position.z * Mathf.Sin(urY) + position.x * Mathf.Cos(urY);
            position.z = z;
            return position;
        }

        // Token: 0x0400379E RID: 14238
        public GraffitiHolder.Shape owner;

        // Token: 0x0400379F RID: 14239
        public Vector3 pos;

        // Token: 0x040037A0 RID: 14240
        public Vector3 lastPos;

        // Token: 0x040037A1 RID: 14241
        public Vector3 startPos;

        // Token: 0x040037A2 RID: 14242
        public List<GraffitiHolder.Shape.Vert> verts = new List<GraffitiHolder.Shape.Vert>();

        // Token: 0x040037A3 RID: 14243
        public List<GraffitiHolder.Shape.Line> holoLines = new List<GraffitiHolder.Shape.Line>();

        // Token: 0x040037A4 RID: 14244
        public List<GraffitiHolder.Shape> subShapes = new List<GraffitiHolder.Shape>();

        // Token: 0x040037A5 RID: 14245
        public GraffitiHolder.Shape.ShapeType shapeType;

        // Token: 0x040037A6 RID: 14246
        public GraffitiHolder.Shape.FloatChanger[] floatChangers = new GraffitiHolder.Shape.FloatChanger[]
        {
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger(),
        new GraffitiHolder.Shape.FloatChanger()
        };

        // Token: 0x040037A7 RID: 14247
        private int dRotA;

        // Token: 0x040037A8 RID: 14248
        private int dRotB;

        // Token: 0x040037A9 RID: 14249
        private bool shakeError;

        // Token: 0x040037AA RID: 14250
        public float[] rotats;

        // Token: 0x02000966 RID: 2406
        public class ShapeType : ExtEnum<GraffitiHolder.Shape.ShapeType>
        {
            // Token: 0x0600444C RID: 17484 RVA: 0x004ABB57 File Offset: 0x004A9D57
            public ShapeType(string value, bool register = false) : base(value, register)
            {
            }

            // Token: 0x04004944 RID: 18756
            public static readonly GraffitiHolder.Shape.ShapeType Main = new GraffitiHolder.Shape.ShapeType("Main", true);

            // Token: 0x04004945 RID: 18757
            public static readonly GraffitiHolder.Shape.ShapeType Shell = new GraffitiHolder.Shape.ShapeType("Shell", true);

            // Token: 0x04004946 RID: 18758
            public static readonly GraffitiHolder.Shape.ShapeType Belt = new GraffitiHolder.Shape.ShapeType("Belt", true);

            // Token: 0x04004947 RID: 18759
            public static readonly GraffitiHolder.Shape.ShapeType DiamondHolder = new GraffitiHolder.Shape.ShapeType("DiamondHolder", true);

            // Token: 0x04004948 RID: 18760
            public static readonly GraffitiHolder.Shape.ShapeType SmallDiamondHolder = new GraffitiHolder.Shape.ShapeType("SmallDiamondHolder", true);

            // Token: 0x04004949 RID: 18761
            public static readonly GraffitiHolder.Shape.ShapeType Diamond = new GraffitiHolder.Shape.ShapeType("Diamond", true);

            // Token: 0x0400494A RID: 18762
            public static readonly GraffitiHolder.Shape.ShapeType Cube = new GraffitiHolder.Shape.ShapeType("Cube", true);

            // Token: 0x0400494B RID: 18763
            public static readonly GraffitiHolder.Shape.ShapeType Ribbon = new GraffitiHolder.Shape.ShapeType("Ribbon", true);

            // Token: 0x0400494C RID: 18764
            public static readonly GraffitiHolder.Shape.ShapeType Sphere = new GraffitiHolder.Shape.ShapeType("Sphere", true);

            // Token: 0x0400494D RID: 18765
            public static readonly GraffitiHolder.Shape.ShapeType BigDiamonds = new GraffitiHolder.Shape.ShapeType("BigDiamonds", true);

            // Token: 0x0400494E RID: 18766
            public static readonly GraffitiHolder.Shape.ShapeType BigDiamonds2 = new GraffitiHolder.Shape.ShapeType("BigDiamonds2", true);
        }

        // Token: 0x02000967 RID: 2407
        public class FloatChanger
        {
            // Token: 0x0600444E RID: 17486 RVA: 0x004ABC21 File Offset: 0x004A9E21
            public float SmoothValue(float timeStacker)
            {
                return Mathf.Lerp(this.from, this.to, Custom.SCurve(Mathf.Lerp(this.lastProg, this.prog, timeStacker), 0.65f));
            }

            // Token: 0x0600444F RID: 17487 RVA: 0x004ABC50 File Offset: 0x004A9E50
            public FloatChanger()
            {
                this.prog = 1f;
                this.lastProg = 1f;
            }

            // Token: 0x06004450 RID: 17488 RVA: 0x004ABC6E File Offset: 0x004A9E6E
            public void Update()
            {
                this.lastProg = this.prog;
                this.prog = Mathf.Min(1f, this.prog + this.speed);
            }

            // Token: 0x06004451 RID: 17489 RVA: 0x004ABC9C File Offset: 0x004A9E9C
            public void NewGoal(float goal, float distanceTravelledInOneFrame, float framesToGetToGoal, float absTimeFac)
            {
                if (this.prog < 1f || this.lastProg < 1f || goal == this.to)
                {
                    return;
                }
                this.from = this.to;
                this.to = goal;
                this.prog = 0f;
                this.lastProg = 0f;
                this.speed = Mathf.Lerp(distanceTravelledInOneFrame / Mathf.Abs(this.from - this.to), 1f / framesToGetToGoal, absTimeFac);
            }

            // Token: 0x0400494F RID: 18767
            public float prog;

            // Token: 0x04004950 RID: 18768
            public float lastProg;

            // Token: 0x04004951 RID: 18769
            public float from;

            // Token: 0x04004952 RID: 18770
            public float to;

            // Token: 0x04004953 RID: 18771
            public float speed;
        }

        // Token: 0x02000968 RID: 2408
        public class Vert
        {
            // Token: 0x06004452 RID: 17490 RVA: 0x004ABD1E File Offset: 0x004A9F1E
            public Vert(float x, float y, float z)
            {
                this.A = new Vector3(x, y, z);
                this.B = this.A;
                this.C = this.A;
            }

            // Token: 0x04004954 RID: 18772
            public Vector3 A;

            // Token: 0x04004955 RID: 18773
            public Vector3 B;

            // Token: 0x04004956 RID: 18774
            public Vector3 C;

            // Token: 0x04004957 RID: 18775
            public Vector3 lastPos;

            // Token: 0x04004958 RID: 18776
            public Vector3 pos;

            // Token: 0x04004959 RID: 18777
            public Vector2 drawPos;

            // Token: 0x0400495A RID: 18778
            public Vector3 errorDrift;

            // Token: 0x0400495B RID: 18779
            public Vector3 errorDriftTarget;

            // Token: 0x0400495C RID: 18780
            public float errors;

            // Token: 0x0400495D RID: 18781
            public bool on;
        }

        // Token: 0x02000969 RID: 2409
        public class Line
        {
            // Token: 0x06004453 RID: 17491 RVA: 0x004ABD4C File Offset: 0x004A9F4C
            public Line(GraffitiHolder.Shape.Vert A, GraffitiHolder.Shape.Vert B)
            {
                this.A = A;
                this.B = B;
            }

            // Token: 0x0400495E RID: 18782
            public GraffitiHolder.Shape.Vert A;

            // Token: 0x0400495F RID: 18783
            public GraffitiHolder.Shape.Vert B;
        }
    }
}

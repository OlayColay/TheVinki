using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Smoke;

public class TagSmoke : PositionedSmokeEmitter
{
    public PhysicalObject source;
    public Creature target;
    public float hue;

    // Token: 0x06002820 RID: 10272 RVA: 0x0030DCD0 File Offset: 0x0030BED0
    public TagSmoke(Room room, PhysicalObject source, Creature target) : base(SmokeSystem.SmokeType.BlackHaze, room, source.firstChunk.pos, 2, 0f, false, -1f, -1)
    {
        this.source = source;
        this.target = target;
        hue = Random.value;

        particles.Clear();
        if (particlePool.GetParticle() != null)
        {
            particlePool.RemoveFromRoom();
        }
    }

    // Token: 0x06002821 RID: 10273 RVA: 0x0030DCF7 File Offset: 0x0030BEF7
    public override SmokeSystemParticle CreateParticle()
    {
        return new SmokeSegment(hue);
    }

    // Token: 0x06002822 RID: 10274 RVA: 0x0030DD00 File Offset: 0x0030BF00
    public override void Update(bool eu)
    {
        base.Update(eu);
        for (int i = 0; i < particles.Count; i++)
        {
            if (PushPow(i) > 0f)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (Custom.DistLess(particles[i].pos, particles[j].pos, 60f))
                    {
                        float num = PushPow(j) / (PushPow(i) + PushPow(j));
                        Vector2 b = (particles[i].vel + particles[j].vel) / 2f;
                        float num2 = Mathf.InverseLerp(60f, 30f, Vector2.Distance(particles[i].pos, particles[j].pos));
                        particles[i].vel = Vector2.Lerp(particles[i].vel, b, num * num2);
                        particles[j].vel = Vector2.Lerp(particles[j].vel, b, (1f - num) * num2);
                    }
                }
            }
        }
        if (source != null && source.firstChunk != null)
        {
            pos = source.firstChunk.pos + new Vector2(0f, 10f);
        }
    }

    // Token: 0x06002823 RID: 10275 RVA: 0x0030DE66 File Offset: 0x0030C066
    private float PushPow(int i)
    {
        return Mathf.InverseLerp(0.65f, 0.85f, particles[i].life) * (particles[i] as SmokeSegment).power;
    }

    // Token: 0x06002824 RID: 10276 RVA: 0x0030DEA0 File Offset: 0x0030C0A0
    public void EmitSmoke(float power)
    {
        if (AddParticle(pos, (target.bodyChunks[1].pos - pos) * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, Random.value), Mathf.Lerp(60f, 100f, Random.value))) is SmokeSegment newVultureSmokeSegment)
        {
            newVultureSmokeSegment.power = power;
        }
    }

    // Token: 0x02000867 RID: 2151
    public new class SmokeSegment(float hue) : HyrbidSmokeSegment()
    {

        // Token: 0x06004078 RID: 16504 RVA: 0x00485191 File Offset: 0x00483391
        public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
        {
            base.Reset(newOwner, pos, vel, lifeTime);
            driftDir = Custom.RNV();
            age = 0;
            power = 0f;
        }

        // Token: 0x06004079 RID: 16505 RVA: 0x004851BC File Offset: 0x004833BC
        public override void Update(bool eu)
        {
            base.Update(eu);
            age++;
            vel += driftDir * Mathf.Sin(Mathf.InverseLerp(0.55f, 0.75f, life) * 3.1415927f) * 0.6f * power;
        }

        // Token: 0x0600407A RID: 16506 RVA: 0x00485230 File Offset: 0x00483430
        public override void WindAndDrag(Room rm, ref Vector2 v, Vector2 p)
        {
            if (v.magnitude > 0f)
            {
                v *= 0.5f + 0.5f / Mathf.Pow(v.magnitude, 0.5f);
            }
            v += SmokeSystem.PerlinWind(p, rm) * 2f;
            if (rm.readyForAI && rm.aimap.getTerrainProximity(p) < 3)
            {
                int terrainProximity = rm.aimap.getTerrainProximity(p);
                Vector2 vector = default;
                for (int i = 0; i < 8; i++)
                {
                    if (rm.aimap.getTerrainProximity(p + Custom.eightDirections[i].ToVector2() * 20f) > terrainProximity)
                    {
                        vector += Custom.eightDirections[i].ToVector2();
                    }
                }
                v += Vector2.ClampMagnitude(vector, 1f) * 0.035f;
            }
        }

        // Token: 0x0600407B RID: 16507 RVA: 0x00485351 File Offset: 0x00483551
        public override float ConDist(float timeStacker)
        {
            return Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) * power;
        }

        // Token: 0x0600407C RID: 16508 RVA: 0x0048538C File Offset: 0x0048358C
        public override float MyRad(float timeStacker)
        {
            return Mathf.Min(Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) + Mathf.Sin(Mathf.InverseLerp(0.7f, 0f, Mathf.Lerp(lastLife, life, timeStacker)) * 3.1415927f) * 8f, 5f + 25f * power) * (2f - MyOpactiy(timeStacker));
        }

        // Token: 0x0600407D RID: 16509 RVA: 0x00485424 File Offset: 0x00483624
        public override float MyOpactiy(float timeStacker)
        {
            if (resting)
            {
                return 0f;
            }
            return Mathf.InverseLerp(0f, 0.7f, Mathf.Lerp(lastLife, life, timeStacker)) * Mathf.Lerp(Custom.LerpMap(Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), base.NextPos(timeStacker)), 20f, 250f, 1f, 0f, 1.5f), 1f, Mathf.InverseLerp(0.9f, 1f, Mathf.Lerp(lastLife, life, timeStacker))) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
        }

        // Token: 0x0600407E RID: 16510 RVA: 0x004854EA File Offset: 0x004836EA
        public override Color MyColor(float timeStacker)
        {
            return VultureSmokeColor(Mathf.InverseLerp(1f, 5f + 15f * power, (float)age + timeStacker));
        }

        // Token: 0x0600407F RID: 16511 RVA: 0x00485518 File Offset: 0x00483718
        public Color VultureSmokeColor(float x)
        {
            return new HSLColor(hue, 0.5f, Mathf.Lerp(0.8f, 0.15f, x)).rgb;
        }

        // Token: 0x06004080 RID: 16512 RVA: 0x004855A8 File Offset: 0x004837A8
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["SmokeTrail"];
            sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
            sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
        }

        // Token: 0x06004081 RID: 16513 RVA: 0x00485644 File Offset: 0x00483844
        public override void HybridDraw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 Apos, Vector2 Bpos, Color Acol, Color Bcol, float Arad, float Brad)
        {
            base.HybridDraw(sLeaser, rCam, timeStacker, camPos, Apos, Bpos, Acol, Bcol, Arad, Brad);
            sLeaser.sprites[1].scale = Arad * (6f - Acol.a) / 8f;
            sLeaser.sprites[1].alpha = Mathf.Pow(Acol.a, 0.6f) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
            Acol.a = 1f;
            sLeaser.sprites[1].color = Acol;
            sLeaser.sprites[2].scale = Brad * (6f - Bcol.a) / 8f;
            sLeaser.sprites[2].alpha = Mathf.Pow(Bcol.a, 0.6f) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
            Bcol.a = 1f;
            sLeaser.sprites[2].color = Bcol;
        }

        // Token: 0x040043CB RID: 17355
        private Vector2 driftDir;

        // Token: 0x040043CC RID: 17356
        public int age;

        // Token: 0x040043CD RID: 17357
        public float power;

        public float hue = hue;
    }
}

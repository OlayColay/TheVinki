using RWCustom;
using UnityEngine;
using MoreSlugcats;

namespace SprayCans;

sealed class SprayCan : Weapon
{
    public override bool HeavyWeapon
    {
        get
        {
            return true;
        }
    }

    private static float Rand => Random.value;

    new public float rotation;
    new public float lastRotation;
    public float rotVel;
    public float lastDarkness = -1f;
    public float darkness;
    public Smoke.BombSmoke smoke;

    private Color blackColor;
    private Color earthColor;
    private bool ignited;
    private float burn = 0f;
    private RoomPalette roomPalette = new RoomPalette();

    public float gamerHue = 0f;

    public SprayCanAbstract Abstr { get; }

    public void InitiateBurn()
    {
        if (Abstr.uses < 1)
        {
            return;
        }

        if (burn == 0f)
        {
            burn = Random.value;
            room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk, false, 0.5f, 1.4f);
            base.firstChunk.vel += Custom.RNV() * Random.value * 6f;
            return;
        }
        burn = Mathf.Min(burn, Random.value);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.sound = SoundID.None;
        if (base.mode == Weapon.Mode.Free && collisionLayer != 1)
        {
            base.ChangeCollisionLayer(1);
        }
        else if (base.mode != Weapon.Mode.Free && collisionLayer != 2)
        {
            base.ChangeCollisionLayer(2);
        }
        if (base.firstChunk.vel.magnitude > 5f)
        {
            if (base.firstChunk.ContactPoint.y < 0)
            {
                soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
            }
            else
            {
                soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
            }
            soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
        }
        soundLoop.Update();
        if (base.firstChunk.ContactPoint.y != 0)
        {
            rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
        }
        if (base.Submersion >= 0.2f && room.waterObject.WaterIsLethal && burn == 0f)
        {
            ignited = (Abstr.uses > 0 && Abstr.uses <= 9000);
            base.buoyancy = 0.9f;
            base.firstChunk.vel *= 0.2f;
            burn = 0.8f + Random.value * 0.2f;
        }
        if (ignited || burn > 0f)
        {
            if (base.Submersion == 1f && !room.waterObject.WaterIsLethal)
            {
                ignited = false;
                burn = 0f;
            }
            if (ignited && burn == 0f && base.mode != Weapon.Mode.Thrown)
            {
                burn = 0.5f + Random.value * 0.5f;
            }
            for (int i = 0; i < 3; i++)
            {
                room.AddObject(new Spark(Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, Random.value), base.firstChunk.vel * 0.1f + Custom.RNV() * 3.2f * Random.value, RandomColor(), null, 7, 30));
            }
            if (smoke == null)
            {
                smoke = new Smoke.BombSmoke(room, base.firstChunk.pos, base.firstChunk, RandomColor());
                room.AddObject(smoke);
            }
        }
        else
        {
            if (smoke != null)
            {
                smoke.Destroy();
            }
            smoke = null;
        }
        if (burn > 0f)
        {
            burn -= 0.033333335f;
            if (burn <= 0f)
            {
                Explode(null);
            }
        }
    }

    // Token: 0x06001A68 RID: 6760 RVA: 0x00205D74 File Offset: 0x00203F74
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (floorBounceFrames > 0 && (direction.x == 0 || room.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Slope))
        {
            return;
        }
        if (ignited)
        {
            Explode(null);
        }
    }

    // Token: 0x06001A69 RID: 6761 RVA: 0x00205DCC File Offset: 0x00203FCC
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj == null)
        {
            return false;
        }

        // If there are no charges left, it should hit something like a Rock would
        if (Abstr.uses == 0 || Abstr.uses > 9000)
        {
            return HitLikeRock(result, eu);
        }

        vibrate = 20;
        ChangeMode(Weapon.Mode.Free);
        if (result.obj is Creature)
        {
            (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Explosion, 0f, 85f);
        }
        else if (result.chunk != null)
        {
            result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos != null)
        {
            (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
        }
        Explode(result.chunk);
        return true;
    }

    // Token: 0x06001A6A RID: 6762 RVA: 0x00205EEA File Offset: 0x002040EA
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        Room room = this.room;
        if (room != null && Abstr.uses > 0 && Abstr.uses <= 9000)
        {
            room.PlaySound(SoundID.Slugcat_Throw_Bomb, base.firstChunk);
        }
        ignited = (Abstr.uses > 0 && Abstr.uses <= 9000);
    }

    // Token: 0x06001A6B RID: 6763 RVA: 0x00205F1F File Offset: 0x0020411F
    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, base.firstChunk);
    }

    // Token: 0x06001A6C RID: 6764 RVA: 0x00205F38 File Offset: 0x00204138
    public override void HitByWeapon(Weapon weapon)
    {
        if (weapon.mode == Weapon.Mode.Thrown && thrownBy == null && weapon.thrownBy != null)
        {
            thrownBy = weapon.thrownBy;
        }
        base.HitByWeapon(weapon);
        InitiateBurn();
    }

    // Token: 0x06001A6D RID: 6765 RVA: 0x00205F75 File Offset: 0x00204175
    public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
    {
        base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
        if (Random.value < 0.5f)
        {
            Explode(null);
            return;
        }
        ignited = (Abstr.uses > 0 && Abstr.uses <= 9000);
        InitiateBurn();
    }

    // Token: 0x06001A6E RID: 6766 RVA: 0x00205FA1 File Offset: 0x002041A1
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        if (Random.value < hitFac)
        {
            if (thrownBy == null)
            {
                thrownBy = explosion.killTagHolder;
            }
            InitiateBurn();
        }
    }
    
    public SprayCan(SprayCanAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr, abstr.world)
    {
        Abstr = abstr;

        UpdateColor();

        bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4 * (Abstr.scaleX + Abstr.scaleY), 0.35f) { goThroughFloors = true } };
        bodyChunks[0].lastPos = bodyChunks[0].pos;
        bodyChunks[0].vel = vel;

        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.6f;
        surfaceFriction = 0.45f;
        collisionLayer = 1;
        waterFriction = 0.92f;
        buoyancy = 0.75f;

        rotation = 0f;
        lastRotation = rotation;

        soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
        ResetVel(vel.magnitude);
    }

    public void HitEffect(Vector2 impactVelocity)
    {
        var num = Random.Range(3, 8);
        for (int k = 0; k < num; k++)
        {
            Vector2 pos = firstChunk.pos + Custom.DegToVec(Rand * 360f) * 5f * Rand;
            Vector2 vel = -impactVelocity * -0.1f + Custom.DegToVec(Rand * 360f) * Mathf.Lerp(0.2f, 0.4f, Rand) * impactVelocity.magnitude;
            room.AddObject(new Spark(pos, vel, new Color(1f, 1f, 1f), null, 10, 170));
        }

        room.AddObject(new StationaryEffect(firstChunk.pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
    }

    public bool TryUse()
    {
        if (Abstr.uses < 1)
        {
            return false;
        }
        else if (Abstr.uses > 9000)
        {
            return true;
        }
        Abstr.uses--;

        UpdateColor();

        return true;
    }

    private void UpdateColor()
    {
        color = Abstr.uses <= SprayCanAbstract.CanColors.Length ? SprayCanAbstract.CanColors[Abstr.uses] : Color.white;
        Color.RGBToHSV(color, out Abstr.hue, out Abstr.saturation, out _);
    }

    private void Explode(BodyChunk hitChunk)
    {
        if (Abstr.uses < 1 || Abstr.uses > 9000 || base.slatedForDeletetion)
        {
            return;
        }

        Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
        room.AddObject(new Explosion(room, this, vector, 7, 250f, 2f * Abstr.uses, 0f, 70f * Abstr.uses, 0f, thrownBy, 0f, 20f, 1f));
        room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, RandomColor()));
        room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, RandomColor()));
        room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, RandomColor()));
        room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
        for (int i = 0; i < 8 * Abstr.uses; i++)
        {
            Vector2 a = Custom.RNV();
            if (room.GetTile(vector + a * 20f).Solid)
            {
                if (!room.GetTile(vector - a * 20f).Solid)
                {
                    a *= -1f;
                }
                else
                {
                    a = Custom.RNV();
                }
            }
            for (int j = 0; j < 3; j++)
            {
                room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(RandomColor(), new Color(1f, 1f, 1f), Random.value), null, 11, 28));
            }
            room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), RandomColor(), Random.Range(3, 11)));
        }
        for (int l = 0; l < 6; l++)
        {
            room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
        }
        room.ScreenMovement(new Vector2?(vector), default(Vector2), 0.3f * Abstr.uses);
        for (int m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++)
        {
            abstractPhysicalObject.stuckObjects[m].Deactivate();
        }
        room.PlaySound(SoundID.Bomb_Explode, vector, 0.2f * Abstr.uses, 1f);
        //this.room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
        bool flag = hitChunk != null;
        for (int n = 0; n < 5; n++)
        {
            if (room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            Color smokeColor = RandomColor();
            if (smoke == null)
            {
                smoke = new Smoke.BombSmoke(room, vector, null, smokeColor);
                roomPalette.blackColor = roomPalette.fogColor = smokeColor;
                room.AddObject(smoke);
                foreach (Smoke.BombSmoke.ThickSmokeSegment particle in smoke.particles)
                {
                    particle.ApplyPalette(null, null, roomPalette);
                }
            }
            if (hitChunk != null)
            {
                smoke.chunk = hitChunk;
            }
            else
            {
                smoke.chunk = null;
                smoke.fadeIn = 1f;
            }
            smoke.pos = vector;
            smoke.stationary = true;
            smoke.DisconnectSmoke();
        }
        else if (smoke != null)
        {
            smoke.Destroy();
        }
        Destroy();
    }

    private void ResetVel(float speed)
    {
        rotVel = Mathf.Lerp(-1f, 1f, Rand) * Custom.LerpMap(speed, 0f, 18f, 5f, 26f);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("SprayCan", true);
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        float num = Mathf.InverseLerp(305f, 380f, timeStacker);
        pos.y -= 20f * Mathf.Pow(num, 3f);
        float num2 = Mathf.Pow(1f - num, 0.25f);
        lastDarkness = darkness;
        darkness = rCam.room.Darkness(pos);
        darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(pos);

        for (int i = 0; i < 1; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[i].scaleY = num2 * Abstr.scaleY;
            sLeaser.sprites[i].scaleX = num2 * Abstr.scaleX;
        }

        if (Abstr.uses > 9000)
        {
            gamerHue += 0.0025f;
            if (gamerHue >= 1f)
            {
                gamerHue -= 1f;
            }
            sLeaser.sprites[0].color = new HSLColor(gamerHue, 0.9f, 0.6f).rgb;
            Abstr.hue = gamerHue;
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), blackColor, darkness);
        }

        if (blink > 0 && Rand < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
        }
        else if (num > 0.3f)
        {
            for (int j = 0; j < 1; j++)
            {
                sLeaser.sprites[j].color = Color.Lerp(sLeaser.sprites[j].color, earthColor, Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, num), 1.6f));
            }
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
        earthColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.5f);
    }

    private Color RandomColor()
    {
        return Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
    }

    private bool HitLikeRock(SharedPhysics.CollisionResult result, bool eu)
    {
        if (thrownBy is Scavenger && (thrownBy as Scavenger).AI != null)
        {
            (thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
        }
        vibrate = 20;
        ChangeMode(Weapon.Mode.Free);
        if (result.obj is Creature)
        {
            float stunBonus = 45f;
            if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || (ModManager.MSC && result.obj is Yeek)))
            {
                stunBonus = 90f;
            }
            if (ModManager.MSC && room.game.IsArenaSession && room.game.GetArenaGameSession.chMeta != null)
            {
                stunBonus = 90f;
            }
            (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.01f, stunBonus);
        }
        else if (result.chunk != null)
        {
            result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos != null)
        {
            (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
        }
        base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * base.firstChunk.vel.magnitude;
        room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
        if (result.chunk != null)
        {
            room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
        }
        SetRandomSpin();
        return true;
    }

#nullable enable
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }
}
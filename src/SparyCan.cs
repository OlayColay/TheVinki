using IL.Smoke;
using RWCustom;
using UnityEngine;
using Smoke;
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

    public SprayCanAbstract Abstr { get; }

    public void InitiateBurn()
    {
        if (Abstr.uses < 1)
        {
            return;
        }

        if (this.burn == 0f)
        {
            this.burn = Random.value;
            this.room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk, false, 0.5f, 1.4f);
            base.firstChunk.vel += Custom.RNV() * Random.value * 6f;
            return;
        }
        this.burn = Mathf.Min(this.burn, Random.value);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.soundLoop.sound = SoundID.None;
        if (base.mode == Weapon.Mode.Free && this.collisionLayer != 1)
        {
            base.ChangeCollisionLayer(1);
        }
        else if (base.mode != Weapon.Mode.Free && this.collisionLayer != 2)
        {
            base.ChangeCollisionLayer(2);
        }
        if (base.firstChunk.vel.magnitude > 5f)
        {
            if (base.firstChunk.ContactPoint.y < 0)
            {
                this.soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
            }
            else
            {
                this.soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
            }
            this.soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
        }
        this.soundLoop.Update();
        if (base.firstChunk.ContactPoint.y != 0)
        {
            this.rotationSpeed = (this.rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
        }
        if (base.Submersion >= 0.2f && this.room.waterObject.WaterIsLethal && this.burn == 0f)
        {
            this.ignited = (Abstr.uses > 0);
            base.buoyancy = 0.9f;
            base.firstChunk.vel *= 0.2f;
            this.burn = 0.8f + Random.value * 0.2f;
        }
        if (this.ignited || this.burn > 0f)
        {
            if (base.Submersion == 1f && !this.room.waterObject.WaterIsLethal)
            {
                this.ignited = false;
                this.burn = 0f;
            }
            if (this.ignited && this.burn == 0f && base.mode != Weapon.Mode.Thrown)
            {
                this.burn = 0.5f + Random.value * 0.5f;
            }
            for (int i = 0; i < 3; i++)
            {
                this.room.AddObject(new Spark(Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, Random.value), base.firstChunk.vel * 0.1f + Custom.RNV() * 3.2f * Random.value, this.RandomColor(), null, 7, 30));
            }
            if (this.smoke == null)
            {
                this.smoke = new Smoke.BombSmoke(this.room, base.firstChunk.pos, base.firstChunk, this.RandomColor());
                this.room.AddObject(this.smoke);
            }
        }
        else
        {
            if (this.smoke != null)
            {
                this.smoke.Destroy();
            }
            this.smoke = null;
        }
        if (this.burn > 0f)
        {
            this.burn -= 0.033333335f;
            if (this.burn <= 0f)
            {
                this.Explode(null);
            }
        }
    }

    // Token: 0x06001A68 RID: 6760 RVA: 0x00205D74 File Offset: 0x00203F74
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (this.floorBounceFrames > 0 && (direction.x == 0 || this.room.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Slope))
        {
            return;
        }
        if (this.ignited)
        {
            this.Explode(null);
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
        if (this.Abstr.uses == 0)
        {
            return HitLikeRock(result, eu);
        }

        this.vibrate = 20;
        this.ChangeMode(Weapon.Mode.Free);
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
            (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
        }
        this.Explode(result.chunk);
        return true;
    }

    // Token: 0x06001A6A RID: 6762 RVA: 0x00205EEA File Offset: 0x002040EA
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        Room room = this.room;
        if (room != null && Abstr.uses > 0)
        {
            room.PlaySound(SoundID.Slugcat_Throw_Bomb, base.firstChunk);
        }
        this.ignited = (Abstr.uses > 0);
    }

    // Token: 0x06001A6B RID: 6763 RVA: 0x00205F1F File Offset: 0x0020411F
    public override void PickedUp(Creature upPicker)
    {
        this.room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, base.firstChunk);
    }

    // Token: 0x06001A6C RID: 6764 RVA: 0x00205F38 File Offset: 0x00204138
    public override void HitByWeapon(Weapon weapon)
    {
        if (weapon.mode == Weapon.Mode.Thrown && this.thrownBy == null && weapon.thrownBy != null)
        {
            this.thrownBy = weapon.thrownBy;
        }
        base.HitByWeapon(weapon);
        this.InitiateBurn();
    }

    // Token: 0x06001A6D RID: 6765 RVA: 0x00205F75 File Offset: 0x00204175
    public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
    {
        base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
        if (Random.value < 0.5f)
        {
            this.Explode(null);
            return;
        }
        this.ignited = (Abstr.uses > 0);
        this.InitiateBurn();
    }

    // Token: 0x06001A6E RID: 6766 RVA: 0x00205FA1 File Offset: 0x002041A1
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        if (Random.value < hitFac)
        {
            if (this.thrownBy == null)
            {
                this.thrownBy = explosion.killTagHolder;
            }
            this.InitiateBurn();
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
        Abstr.uses--;

        UpdateColor();

        return true;
    }

    private void UpdateColor()
    {
        switch (Abstr.uses)
        {
            case 0:
                color = Color.gray;
                Abstr.hue = 0f;
                Abstr.saturation = 0f;
                break;
            case 1:
                color = Color.red;
                Abstr.hue = 0f;
                Abstr.saturation = 1f;
                break;
            case 2:
                color = Color.HSVToRGB(0.07669325f, 1f, 1f);
                Abstr.hue = 0.07669325f;
                Abstr.saturation = 1f;
                break;
            case 3:
                color = Color.yellow;
                Abstr.hue = 0.1533865f;
                Abstr.saturation = 1f;
                break;
            case 4:
                color = Color.HSVToRGB(0.245f, 1f, 1f);
                Abstr.hue = 0.245f;
                Abstr.saturation = 1f;
                break;
            case 5:
                color = Color.green;
                Abstr.hue = 0.3333333f;
                Abstr.saturation = 1f;
                break;
            default:
                color = Color.white;
                Abstr.hue = 0f;
                Abstr.saturation = 0f;
                break;
        }
    }

    private void Explode(BodyChunk hitChunk)
    {
        if (Abstr.uses < 1 || base.slatedForDeletetion)
        {
            return;
        }

        Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
        this.room.AddObject(new Explosion(this.room, this, vector, 7, 250f, 2f * Abstr.uses, 0f, 100f, 0f, this.thrownBy, 0f, 20f, 1f));
        this.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, RandomColor()));
        this.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, RandomColor()));
        this.room.AddObject(new ExplosionSpikes(this.room, vector, 14, 30f, 9f, 7f, 170f, RandomColor()));
        this.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
        for (int i = 0; i < 8 * Abstr.uses; i++)
        {
            Vector2 a = Custom.RNV();
            if (this.room.GetTile(vector + a * 20f).Solid)
            {
                if (!this.room.GetTile(vector - a * 20f).Solid)
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
                this.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(RandomColor(), new Color(1f, 1f, 1f), Random.value), null, 11, 28));
            }
            this.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), RandomColor(), Random.Range(3, 11)));
        }
        for (int l = 0; l < 6; l++)
        {
            this.room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
        }
        this.room.ScreenMovement(new Vector2?(vector), default(Vector2), 0.3f * Abstr.uses);
        for (int m = 0; m < this.abstractPhysicalObject.stuckObjects.Count; m++)
        {
            this.abstractPhysicalObject.stuckObjects[m].Deactivate();
        }
        this.room.PlaySound(SoundID.Bomb_Explode, vector, 0.3f * Abstr.uses, 1f);
        //this.room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
        bool flag = hitChunk != null;
        for (int n = 0; n < 5; n++)
        {
            if (this.room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            Color smokeColor = RandomColor();
            if (this.smoke == null)
            {
                this.smoke = new Smoke.BombSmoke(this.room, vector, null, smokeColor);
                roomPalette.blackColor = roomPalette.fogColor = smokeColor;
                this.room.AddObject(this.smoke);
                foreach (Smoke.BombSmoke.ThickSmokeSegment particle in this.smoke.particles)
                {
                    particle.ApplyPalette(null, null, roomPalette);
                }
            }
            if (hitChunk != null)
            {
                this.smoke.chunk = hitChunk;
            }
            else
            {
                this.smoke.chunk = null;
                this.smoke.fadeIn = 1f;
            }
            this.smoke.pos = vector;
            this.smoke.stationary = true;
            this.smoke.DisconnectSmoke();
        }
        else if (this.smoke != null)
        {
            this.smoke.Destroy();
        }
        this.Destroy();
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

        sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), blackColor, darkness);

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
        if (this.thrownBy is Scavenger && (this.thrownBy as Scavenger).AI != null)
        {
            (this.thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
        }
        this.vibrate = 20;
        this.ChangeMode(Weapon.Mode.Free);
        if (result.obj is Creature)
        {
            float stunBonus = 45f;
            if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || (ModManager.MSC && result.obj is Yeek)))
            {
                stunBonus = 90f;
            }
            if (ModManager.MSC && this.room.game.IsArenaSession && this.room.game.GetArenaGameSession.chMeta != null)
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
            (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
        }
        base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * base.firstChunk.vel.magnitude;
        this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
        if (result.chunk != null)
        {
            this.room.AddObject(new ExplosionSpikes(this.room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
        }
        this.SetRandomSpin();
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
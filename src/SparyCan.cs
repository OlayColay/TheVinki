using IL.Smoke;
using RWCustom;
using UnityEngine;
using Smoke;

namespace SprayCans;

sealed class SprayCan : Weapon
{
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
    private readonly float rotationOffset;
    private RoomPalette roomPalette = new RoomPalette();

    public SprayCanAbstract Abstr { get; }

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

        rotation = Rand * 360f;
        lastRotation = rotation;

        rotationOffset = Rand * 30 - 15;

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
                color = Color.yellow;
                Abstr.hue = 0.1533865f;
                Abstr.saturation = 1f;
                break;
            case 3:
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
        if (base.slatedForDeletetion)
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

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        Room room = this.room;
        if (room != null)
        {
            room.PlaySound(SoundID.Slugcat_Throw_Bomb, base.firstChunk);
        }
        this.ignited = true;
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj == null)
        {
            return false;
        }
        this.vibrate = 20;
        this.ChangeMode(Weapon.Mode.Free);
        if (result.obj is Creature)
        {
            (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Explosion, 0.2f, 85f);
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


    public override void Update(bool eu)
    {
        ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);
        firstChunk.collideWithTerrain = grabbedBy.Count == 0;
        firstChunk.collideWithSlopes = grabbedBy.Count == 0;

        base.Update(eu);

        var chunk = firstChunk;

        lastRotation = rotation;
        rotation += rotVel * Vector2.Distance(chunk.lastPos, chunk.pos);

        rotation %= 360;

        if (grabbedBy.Count == 0)
        {
            if (firstChunk.lastPos == firstChunk.pos)
            {
                rotVel *= 0.9f;
            }
            else if (Mathf.Abs(rotVel) <= 0.01f)
            {
                ResetVel((firstChunk.lastPos - firstChunk.pos).magnitude);
            }
        }
        else
        {
            var grabberChunk = grabbedBy[0].grabber.mainBodyChunk;
            rotVel *= 0.9f;
            rotation = Mathf.Lerp(rotation, grabberChunk.Rotation.GetAngle() + rotationOffset, 0.25f);
        }

        if (!Custom.DistLess(chunk.lastPos, chunk.pos, 3f) && room.GetTile(chunk.pos).Solid && !room.GetTile(chunk.lastPos).Solid)
        {
            var firstSolid = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(chunk.lastPos), room.GetTilePosition(chunk.pos));
            if (firstSolid != null)
            {
                FloatRect floatRect = Custom.RectCollision(chunk.pos, chunk.lastPos, room.TileRect(firstSolid.Value).Grow(2f));
                chunk.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                bool flag = false;
                if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
                {
                    chunk.vel.x = Mathf.Abs(chunk.vel.x) * 0.15f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
                {
                    chunk.vel.x = -Mathf.Abs(chunk.vel.x) * 0.15f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
                {
                    chunk.vel.y = Mathf.Abs(chunk.vel.y) * 0.15f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
                {
                    chunk.vel.y = -Mathf.Abs(chunk.vel.y) * 0.15f;
                    flag = true;
                }
                if (flag)
                {
                    rotVel *= 0.8f;
                }
            }
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);

        if (grabbedBy.Count > 0)
        {
            Creature grabber = grabbedBy[0].grabber;
            Vector2 push = firstChunk.vel * firstChunk.mass / grabber.firstChunk.mass;
            grabber.firstChunk.vel += push;
        }

        firstChunk.vel = Vector2.zero;

        HitEffect(weapon.firstChunk.vel);
    }

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

    private void ResetVel(float speed)
    {
        rotVel = Mathf.Lerp(-1f, 1f, Rand) * Custom.LerpMap(speed, 0f, 18f, 5f, 26f);
    }

    public override void ChangeMode(Mode newMode)
    { }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("CentipedeBackShell", true);
        sLeaser.sprites[1] = new FSprite("CentipedeBackShell", true);
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

        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[i].scaleY = num2 * Abstr.scaleY;
            sLeaser.sprites[i].scaleX = num2 * Abstr.scaleX;
        }

        sLeaser.sprites[0].color = blackColor;

        sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), blackColor, darkness);

        if (blink > 0 && Rand < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
        }
        else if (num > 0.3f)
        {
            for (int j = 0; j < 2; j++)
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
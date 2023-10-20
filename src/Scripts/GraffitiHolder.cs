using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Vinki;

public class GraffitiHolder : UpdatableAndDeletable, IDrawable
{
    public int LightSprite
    {
        get
        {
            return 0;
        }
    }

    public int MainSprite
    {
        get
        {
            return 1;
        }
    }

    public int LineSprite(int line)
    {
        return 2 + line;
    }

    public int GoldSprite
    {
        get
        {
            return 6;
        }
    }

    public int TotalSprites
    {
        get
        {
            return 7;
        }
    }

    public bool sprayable = true;
    public Vector2 hoverPos;
    public Vector2 pos;
    public Vector2 lastPos;
    public Vector2 vel;
    public Vector2[] trail;
    public Vector2[,] lines;
    public Vector2[] glassesPoints = new Vector2[4] { new Vector2(-22f, 22f), new Vector2(10f, -10f), new Vector2(22f, 22f), new Vector2(-10f, -10f) };
    public Vector2[] boundsPoints = new Vector2[4];
    private float glitch;
    private float lastGlitch;
    private float generalGlitch;
    public float sinCounter;
    public float sinCounter2;
    //public PlacedObject placedObj;
    private bool displayBounds;
    private bool contract;
    private float power;
    private float lastPower;
    private float expand;
    private float lastExpand;
    private StaticSoundLoop soundLoop;
    private StaticSoundLoop glitchLoop;
    public Color TokenColor = new Color(1f, 0.5f, 0f);
    public int gNum;

    public GraffitiHolder(PlacedObject.CustomDecalData graffitiData, KeyValuePair<string, Vector2> graffitiPosition, Room room, int gNum)
	{
        this.room = room;
        this.pos = graffitiPosition.Value;
        this.hoverPos = this.pos;
        this.lastPos = this.pos;
        this.lines = new Vector2[4, 4];
        for (int i = 0; i < this.lines.GetLength(0); i++)
        {
            this.lines[i, 0] = this.pos;
            this.lines[i, 1] = this.pos;
        }

        // Initially set vertices of hologram to be glasses
        for (int i = 0; i < this.lines.GetLength(0); i++)
        {
            this.lines[i, 2] = glassesPoints[i];
        }

        // Initialize corners of graffiti bounds
        Vector2 grafRadii = graffitiData.handles[1] / 2f;
        boundsPoints[0] = new Vector2(-grafRadii.x, grafRadii.y);
        boundsPoints[1] = new Vector2(grafRadii.x, grafRadii.y);
        boundsPoints[2] = new Vector2(grafRadii.x, -grafRadii.y);
        boundsPoints[3] = new Vector2(-grafRadii.x, -grafRadii.y);

        this.trail = new Vector2[5];
        for (int j = 0; j < this.trail.Length; j++)
        {
            this.trail[j] = this.pos;
            this.trail[j].y += -11f;
        }
        this.soundLoop = new StaticSoundLoop(SoundID.Token_Idle_LOOP, this.pos, room, 0f, 1f);
        this.glitchLoop = new StaticSoundLoop(SoundID.Token_Upset_LOOP, this.pos, room, 0f, 1f);

        this.gNum = gNum;
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Water");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
        }
        if (this.sprayable)
        {
            newContatiner.AddChild(sLeaser.sprites[this.GoldSprite]);
        }
        for (int j = 0; j < this.GoldSprite; j++)
        {
            bool flag = false;
            if (ModManager.MMF)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (j == this.LineSprite(k))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                newContatiner.AddChild(sLeaser.sprites[j]);
            }
        }
        if (ModManager.MMF)
        {
            for (int l = 0; l < 4; l++)
            {
                rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[this.LineSprite(l)]);
            }
        }
        if (!this.sprayable)
        {
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[this.GoldSprite]);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
        float num = Mathf.Lerp(this.lastGlitch, this.glitch, timeStacker);
        float num2 = Mathf.Lerp(this.lastExpand, this.expand, timeStacker);
        float num3 = Mathf.Lerp(this.lastPower, this.power, timeStacker);
        if (this.room != null && rCam.room.game.GetStorySession.saveStateNumber != Enums.vinki)
        {
            num = Mathf.Lerp(num, 1f, Random.value);
            num3 *= 0.3f + 0.7f * Random.value;
        }
        sLeaser.sprites[this.GoldSprite].x = vector.x - camPos.x;
        sLeaser.sprites[this.GoldSprite].y = vector.y - camPos.y;
        if (this.sprayable)
        {
            sLeaser.sprites[this.GoldSprite].alpha = this.displayBounds ? 0f : 0.75f * Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * Random.value)), 0.7f, num2) * num3;
        }
        else
        {
            sLeaser.sprites[this.GoldSprite].alpha = this.displayBounds ? 0f : Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * Random.value)), 0.7f, num2) * num3;
        }
        sLeaser.sprites[this.GoldSprite].scale = Mathf.Lerp(true ? 110f : 100f, 300f, num2) / 16f;
        sLeaser.sprites[this.GoldSprite].isVisible = !this.displayBounds;
        Color color = this.GoldCol(num);
        sLeaser.sprites[this.MainSprite].color = color;
        sLeaser.sprites[this.MainSprite].x = vector.x - camPos.x;
        sLeaser.sprites[this.MainSprite].y = vector.y - camPos.y - 11f;
        sLeaser.sprites[this.MainSprite].alpha = this.displayBounds ? 0f : (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3;
        sLeaser.sprites[this.MainSprite].scaleY = 0.5f;
        sLeaser.sprites[this.MainSprite].isVisible = !this.displayBounds;
        sLeaser.sprites[this.LightSprite].x = vector.x - camPos.x;
        sLeaser.sprites[this.LightSprite].y = vector.y - camPos.y;
        sLeaser.sprites[this.LightSprite].alpha = 0f;
        sLeaser.sprites[this.LightSprite].scale = Mathf.Lerp(20f, 40f, num) / 16f;
        sLeaser.sprites[this.LightSprite].isVisible = false;
        if (this.sprayable)
        {
            sLeaser.sprites[this.LightSprite].color = Color.Lerp(this.TokenColor, color, 0.4f);
        }
        else
        {
            sLeaser.sprites[this.LightSprite].color = color;
        }
        sLeaser.sprites[this.LightSprite].isVisible = (!this.contract && num3 > 0f);
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            Vector2 vector2 = Vector2.Lerp(this.lines[i, 1], this.lines[i, 0], timeStacker);
            int num4 = (i == lines.GetLength(0)-1) ? 0 : (i + 1);
            Vector2 vector3 = Vector2.Lerp(this.lines[num4, 1], this.lines[num4, 0], timeStacker);
            float num5 = 1f - (1f - Mathf.Max(this.lines[i, 3].x, this.lines[num4, 3].x)) * (1f - num);
            num5 = Mathf.Pow(num5, 2f);
            num5 *= 1f - num2;
            if (Random.value < num5)
            {
                vector3 = Vector2.Lerp(vector2, vector3, Random.value);
            }
            sLeaser.sprites[this.LineSprite(i)].x = vector2.x - camPos.x;
            sLeaser.sprites[this.LineSprite(i)].y = vector2.y - camPos.y;
            sLeaser.sprites[this.LineSprite(i)].scaleY = Vector2.Distance(vector2, vector3);
            sLeaser.sprites[this.LineSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
            sLeaser.sprites[this.LineSprite(i)].alpha = (1f - num5) * num3;
            sLeaser.sprites[this.LineSprite(i)].color = color;
            sLeaser.sprites[this.LineSprite(i)].isVisible = (num3 > 0f);
        }
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[this.TotalSprites];
        sLeaser.sprites[this.LightSprite] = new FSprite("Futile_White", true);
        sLeaser.sprites[this.LightSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        sLeaser.sprites[this.GoldSprite] = new FSprite("Futile_White", true);
        if (this.sprayable)
        {
            sLeaser.sprites[this.GoldSprite].color = Color.Lerp(new Color(0f, 0f, 0f), RainWorld.GoldRGB, 0.2f);
            sLeaser.sprites[this.GoldSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        }
        else
        {
            sLeaser.sprites[this.GoldSprite].shader = rCam.game.rainWorld.Shaders["GoldenGlow"];
        }
        sLeaser.sprites[this.MainSprite] = new FSprite("JetFishEyeA", true);
        sLeaser.sprites[this.MainSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
        for (int i = 0; i < this.lines.GetLength(0); i++)
        {
            sLeaser.sprites[this.LineSprite(i)] = new FSprite("pixel", true);
            sLeaser.sprites[this.LineSprite(i)].anchorY = 0f;
            sLeaser.sprites[this.LineSprite(i)].shader = rCam.game.rainWorld.Shaders["Hologram"];
        }
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void Update(bool eu)
    {
        if (ModManager.MMF && this.room.game.StoryCharacter != Enums.vinki)
        {
            this.Destroy();
        }
        this.sinCounter += Random.value * this.power;
        this.sinCounter2 += (1f + Mathf.Lerp(-10f, 10f, Random.value) * this.glitch) * this.power;
        float num = Mathf.Sin(this.sinCounter2 / 20f);
        num = Mathf.Pow(Mathf.Abs(num), 0.5f) * Mathf.Sign(num);
        this.soundLoop.Update();
        this.soundLoop.pos = this.pos;
        this.soundLoop.pitch = 1f + 0.25f * num * this.glitch;
        this.soundLoop.volume = Mathf.Pow(this.power, 0.5f) * Mathf.Pow(1f - this.glitch, 0.5f);
        this.glitchLoop.Update();
        this.glitchLoop.pos = this.pos;
        this.glitchLoop.pitch = Mathf.Lerp(0.75f, 1.25f, this.glitch) - 0.25f * num * this.glitch;
        this.glitchLoop.volume = Mathf.Pow(Mathf.Sin(Mathf.Clamp(this.glitch, 0f, 1f) * 3.1415927f), 0.1f) * Mathf.Pow(this.power, 0.1f);
        this.lastPos = this.pos;
        for (int i = 0; i < this.lines.GetLength(0); i++)
        {
            this.lines[i, 1] = this.lines[i, 0];
        }
        this.lastGlitch = this.glitch;
        this.lastExpand = this.expand;
        for (int j = this.trail.Length - 1; j >= 1; j--)
        {
            this.trail[j] = this.trail[j - 1];
        }
        this.trail[0] = this.lastPos;
        this.lastPower = this.power;
        this.power = Custom.LerpAndTick(this.power, 1f, 0.07f, 0.025f);
        this.glitch = Mathf.Max(this.glitch, 1f - this.power);
        for (int k = 0; k < this.lines.GetLength(0); k++)
        {
            if (Mathf.Pow(Random.value, 0.1f + this.glitch * 5f) > this.lines[k, 3].x)
            {
                if (this.displayBounds)
                {
                    this.lines[k, 0] = Vector2.Lerp(this.lines[k, 0], this.pos + new Vector2(this.lines[k, 2].x, this.lines[k, 2].y), Mathf.Pow(Random.value, 1f + this.lines[k, 3].x * 8f));
                }
                else
                {
                    this.lines[k, 0] = Vector2.Lerp(this.lines[k, 0], this.pos + new Vector2(this.lines[k, 2].x * num, this.lines[k, 2].y), Mathf.Pow(Random.value, 1f + this.lines[k, 3].x * 17f));
                }
            }
            if (Random.value < Mathf.Pow(this.lines[k, 3].x, 0.2f) && Random.value < Mathf.Pow(this.glitch, 0.8f - 0.4f * this.lines[k, 3].x))
            {
                this.lines[k, 0] += Custom.RNV() * 17f * this.lines[k, 3].x * this.power;
                this.lines[k, 3].y = Mathf.Max(this.lines[k, 3].y, this.glitch);
            }
            this.lines[k, 3].x = Custom.LerpAndTick(this.lines[k, 3].x, this.lines[k, 3].y, 0.01f, 0.033333335f);
            this.lines[k, 3].y = Mathf.Max(0f, this.lines[k, 3].y - 0.014285714f);
            if (Random.value < 1f / Mathf.Lerp(210f, 20f, this.glitch))
            {
                this.lines[k, 3].y = Mathf.Max(this.glitch, (Random.value < 0.5f) ? this.generalGlitch : Random.value);
            }
        }
        this.vel *= 0.995f;
        this.vel += Vector2.ClampMagnitude(this.hoverPos + new Vector2(0f, Mathf.Sin(this.sinCounter / 15f) * 7f) - this.pos, 15f) / 81f;
        this.vel += Custom.RNV() * Random.value * Random.value * Mathf.Lerp(0.06f, 0.4f, this.glitch);
        {
            this.generalGlitch = Mathf.Max(0f, this.generalGlitch - 0.008333334f);
            if (Random.value < 0.0027027028f)
            {
                this.generalGlitch = Random.value;
            }
            float f = Mathf.Sin(Mathf.Clamp(this.glitch, 0f, 1f) * 3.1415927f);
            if (Random.value < 0.05f + 0.35f * Mathf.Pow(f, 0.5f) && Random.value < this.power && !this.displayBounds)
            {
                this.room.AddObject(new CollectToken.TokenSpark(this.pos + Custom.RNV() * 6f * this.glitch, Custom.RNV() * Mathf.Lerp(2f, 9f, Mathf.Pow(f, 2f)) * Random.value, this.GoldCol(this.glitch), false));
            }
            this.glitch = Custom.LerpAndTick(this.glitch, this.generalGlitch / 2f, 0.01f, 0.033333335f);
            if (Random.value < 1f / Mathf.Lerp(360f, 10f, this.generalGlitch))
            {
                this.glitch = Mathf.Pow(Random.value, 1f - 0.85f * this.generalGlitch);
            }
            float num4 = float.MaxValue;
            bool flag = this.room.game.StoryCharacter == Enums.vinki;
            if (RainWorld.lockGameTimer)
            {
                flag = false;
            }
            float num5 = 140f;
            for (int n = 0; n < this.room.game.session.Players.Count; n++)
            {
                if (this.room.game.session.Players[n].realizedCreature != null && this.room.game.session.Players[n].realizedCreature.Consious && (this.room.game.session.Players[n].realizedCreature as Player).dangerGrasp == null && this.room.game.session.Players[n].realizedCreature.room == this.room)
                {
                    num4 = Mathf.Min(num4, Vector2.Distance(this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, this.pos));
                }
            }
            if (!flag && this.displayBounds)
            {
                if (Random.value < 0.14285715f)
                {
                    this.glitch = Mathf.Max(this.glitch, Random.value * Random.value * Random.value);
                }
            }
            if (this.displayBounds && this.expand == 0f && !this.contract && Random.value < Mathf.InverseLerp(num5 + 160f, num5 + 460f, num4))
            {
                // check if a player is within the graffiti bounds, and show the bounds if one is
                foreach (AbstractCreature player in this.room.game.session.Players)
                {
                    if (player.realizedCreature == null)
                    {
                        continue;
                    }

                    Vector2 playerPos = player.realizedCreature.mainBodyChunk.pos;
                    if (playerPos.x <= this.pos.x - this.boundsPoints[1].x || playerPos.x >= this.pos.x + this.boundsPoints[1].x ||
                        playerPos.y <= this.pos.y - this.boundsPoints[1].y || playerPos.y >= this.pos.y + this.boundsPoints[1].y)
                    {
                        this.displayBounds = false;
                        // Set vertices of hologram to be glasses
                        for (int i = 0; i < this.lines.GetLength(0); i++)
                        {
                            this.lines[i, 2] = glassesPoints[i];
                        }
                        this.room.PlaySound(SoundID.Token_Turn_Off, this.pos);
                    }
                }
            }
            else if (!this.displayBounds)
            {
                // check if a player is within the graffiti bounds, and show the bounds if one is
                foreach (AbstractCreature player in this.room.game.session.Players)
                {
                    if (player.realizedCreature == null)
                    {
                        continue;
                    }

                    Vector2 playerPos = player.realizedCreature.mainBodyChunk.pos;
                    if (playerPos.x >= this.pos.x - this.boundsPoints[1].x && playerPos.x <= this.pos.x + this.boundsPoints[1].x &&
                        playerPos.y >= this.pos.y - this.boundsPoints[1].y && playerPos.y <= this.pos.y + this.boundsPoints[1].y)
                    {
                        this.displayBounds = true;
                        // Set vertices of hologram to be bounds of graffiti
                        for (int i = 0; i < this.lines.GetLength(0); i++)
                        {
                            this.lines[i, 2] = boundsPoints[i];
                        }
                        this.room.PlaySound(SoundID.Token_Turn_On, this.pos);
                    }
                }
                
            }
        }
        base.Update(eu);
    }

    public Color GoldCol(float g)
    {
        if (this.sprayable)
        {
            return Color.Lerp(this.TokenColor, new Color(1f, 1f, 1f), 0.4f + 0.4f * Mathf.Max(this.contract ? 0.5f : (this.expand * 0.5f), Mathf.Pow(g, 0.5f)));
        }
        return Color.Lerp(this.TokenColor, new Color(1f, 1f, 1f), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, g), 0.5f));
    }
}

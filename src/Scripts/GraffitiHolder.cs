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
    public Vector2[] glassesPoints = [new Vector2(-22f, 22f), new Vector2(10f, -10f), new Vector2(22f, 22f), new Vector2(-10f, -10f)];
    public Vector2[] boundsPoints = new Vector2[4];
    private float glitch;
    private float lastGlitch;
    private float generalGlitch;
    public float sinCounter;
    public float sinCounter2;
    //public PlacedObject placedObj;
    private bool displayBounds;
    private readonly bool contract = false;
    private float power;
    private float lastPower;
    private readonly float expand = 0;
    private float lastExpand;
    private readonly StaticSoundLoop soundLoop;
    private readonly StaticSoundLoop glitchLoop;
    public Color TokenColor = new(1f, 0.5f, 0f);
    public int gNum;

    public GraffitiHolder(PlacedObject.CustomDecalData graffitiData, KeyValuePair<string, Vector2> graffitiPosition, Room room, int gNum)
	{
        this.room = room;
        pos = graffitiPosition.Value;
        hoverPos = pos;
        lastPos = pos;
        lines = new Vector2[4, 4];
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            lines[i, 0] = pos;
            lines[i, 1] = pos;
        }

        // Initially set vertices of hologram to be glasses
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            lines[i, 2] = glassesPoints[i];
        }

        // Initialize corners of graffiti bounds
        Vector2 grafRadii = graffitiData.handles[1] / 2f;
        boundsPoints[0] = new Vector2(-grafRadii.x, grafRadii.y);
        boundsPoints[1] = new Vector2(grafRadii.x, grafRadii.y);
        boundsPoints[2] = new Vector2(grafRadii.x, -grafRadii.y);
        boundsPoints[3] = new Vector2(-grafRadii.x, -grafRadii.y);

        trail = new Vector2[5];
        for (int j = 0; j < trail.Length; j++)
        {
            trail[j] = pos;
            trail[j].y += -11f;
        }
        soundLoop = new StaticSoundLoop(SoundID.Token_Idle_LOOP, pos, room, 0f, 1f);
        glitchLoop = new StaticSoundLoop(SoundID.Token_Upset_LOOP, pos, room, 0f, 1f);

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
        if (sprayable)
        {
            newContatiner.AddChild(sLeaser.sprites[GoldSprite]);
        }
        for (int j = 0; j < GoldSprite; j++)
        {
            bool flag = false;
            if (ModManager.MMF)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (j == LineSprite(k))
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
                rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[LineSprite(l)]);
            }
        }
        if (!sprayable)
        {
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[GoldSprite]);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
        float num = Mathf.Lerp(lastGlitch, glitch, timeStacker);
        float num2 = Mathf.Lerp(lastExpand, expand, timeStacker);
        float num3 = Mathf.Lerp(lastPower, power, timeStacker);
        if (room != null && rCam.room.game.GetStorySession.saveStateNumber != Enums.vinki)
        {
            num = Mathf.Lerp(num, 1f, Random.value);
            num3 *= 0.3f + 0.7f * Random.value;
        }
        sLeaser.sprites[GoldSprite].x = vector.x - camPos.x;
        sLeaser.sprites[GoldSprite].y = vector.y - camPos.y;
        if (sprayable)
        {
            sLeaser.sprites[GoldSprite].alpha = displayBounds ? 0f : 0.75f * Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * Random.value)), 0.7f, num2) * num3;
        }
        else
        {
            sLeaser.sprites[GoldSprite].alpha = displayBounds ? 0f : Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * Random.value)), 0.7f, num2) * num3;
        }
        sLeaser.sprites[GoldSprite].scale = Mathf.Lerp(true ? 110f : 100f, 300f, num2) / 16f;
        sLeaser.sprites[GoldSprite].isVisible = !displayBounds;
        Color color = GoldCol(num);
        sLeaser.sprites[MainSprite].color = color;
        sLeaser.sprites[MainSprite].x = vector.x - camPos.x;
        sLeaser.sprites[MainSprite].y = vector.y - camPos.y - 11f;
        sLeaser.sprites[MainSprite].alpha = displayBounds ? 0f : (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3;
        sLeaser.sprites[MainSprite].scaleY = 0.5f;
        sLeaser.sprites[MainSprite].isVisible = !displayBounds;
        sLeaser.sprites[LightSprite].x = vector.x - camPos.x;
        sLeaser.sprites[LightSprite].y = vector.y - camPos.y;
        sLeaser.sprites[LightSprite].alpha = 0f;
        sLeaser.sprites[LightSprite].scale = Mathf.Lerp(20f, 40f, num) / 16f;
        sLeaser.sprites[LightSprite].isVisible = false;
        if (sprayable)
        {
            sLeaser.sprites[LightSprite].color = Color.Lerp(TokenColor, color, 0.4f);
        }
        else
        {
            sLeaser.sprites[LightSprite].color = color;
        }
        sLeaser.sprites[LightSprite].isVisible = (!contract && num3 > 0f);
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            Vector2 vector2 = Vector2.Lerp(lines[i, 1], lines[i, 0], timeStacker);
            int num4 = (i == lines.GetLength(0)-1) ? 0 : (i + 1);
            Vector2 vector3 = Vector2.Lerp(lines[num4, 1], lines[num4, 0], timeStacker);
            float num5 = 1f - (1f - Mathf.Max(lines[i, 3].x, lines[num4, 3].x)) * (1f - num);
            num5 = Mathf.Pow(num5, 2f);
            num5 *= 1f - num2;
            if (Random.value < num5)
            {
                vector3 = Vector2.Lerp(vector2, vector3, Random.value);
            }
            sLeaser.sprites[LineSprite(i)].x = vector2.x - camPos.x;
            sLeaser.sprites[LineSprite(i)].y = vector2.y - camPos.y;
            sLeaser.sprites[LineSprite(i)].scaleY = Vector2.Distance(vector2, vector3);
            sLeaser.sprites[LineSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
            sLeaser.sprites[LineSprite(i)].alpha = (1f - num5) * num3;
            sLeaser.sprites[LineSprite(i)].color = color;
            sLeaser.sprites[LineSprite(i)].isVisible = (num3 > 0f);
        }
        if (base.slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[TotalSprites];
        sLeaser.sprites[LightSprite] = new FSprite("Futile_White", true)
        {
            shader = rCam.game.rainWorld.Shaders["FlatLight"]
        };
        sLeaser.sprites[GoldSprite] = new FSprite("Futile_White", true);
        if (sprayable)
        {
            sLeaser.sprites[GoldSprite].color = Color.Lerp(new Color(0f, 0f, 0f), RainWorld.GoldRGB, 0.2f);
            sLeaser.sprites[GoldSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        }
        else
        {
            sLeaser.sprites[GoldSprite].shader = rCam.game.rainWorld.Shaders["GoldenGlow"];
        }
        sLeaser.sprites[MainSprite] = new FSprite("JetFishEyeA", true)
        {
            shader = rCam.game.rainWorld.Shaders["Hologram"]
        };
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            sLeaser.sprites[LineSprite(i)] = new FSprite("pixel", true)
            {
                anchorY = 0f,
                shader = rCam.game.rainWorld.Shaders["Hologram"]
            };
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void Update(bool eu)
    {
        if (ModManager.MMF && room.game.StoryCharacter != Enums.vinki)
        {
            Destroy();
        }
        sinCounter += Random.value * power;
        sinCounter2 += (1f + Mathf.Lerp(-10f, 10f, Random.value) * glitch) * power;
        float num = Mathf.Sin(sinCounter2 / 20f);
        num = Mathf.Pow(Mathf.Abs(num), 0.5f) * Mathf.Sign(num);
        soundLoop.Update();
        soundLoop.pos = pos;
        soundLoop.pitch = 1f + 0.25f * num * glitch;
        soundLoop.volume = Mathf.Pow(power, 0.5f) * Mathf.Pow(1f - glitch, 0.5f);
        glitchLoop.Update();
        glitchLoop.pos = pos;
        glitchLoop.pitch = Mathf.Lerp(0.75f, 1.25f, glitch) - 0.25f * num * glitch;
        glitchLoop.volume = Mathf.Pow(Mathf.Sin(Mathf.Clamp(glitch, 0f, 1f) * 3.1415927f), 0.1f) * Mathf.Pow(power, 0.1f);
        lastPos = pos;
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            lines[i, 1] = lines[i, 0];
        }
        lastGlitch = glitch;
        lastExpand = expand;
        for (int j = trail.Length - 1; j >= 1; j--)
        {
            trail[j] = trail[j - 1];
        }
        trail[0] = lastPos;
        lastPower = power;
        power = Custom.LerpAndTick(power, 1f, 0.07f, 0.025f);
        glitch = Mathf.Max(glitch, 1f - power);
        for (int k = 0; k < lines.GetLength(0); k++)
        {
            if (Mathf.Pow(Random.value, 0.1f + glitch * 5f) > lines[k, 3].x)
            {
                if (displayBounds)
                {
                    lines[k, 0] = Vector2.Lerp(lines[k, 0], pos + new Vector2(lines[k, 2].x, lines[k, 2].y), Mathf.Pow(Random.value, 1f + lines[k, 3].x * 8f));
                }
                else
                {
                    lines[k, 0] = Vector2.Lerp(lines[k, 0], pos + new Vector2(lines[k, 2].x * num, lines[k, 2].y), Mathf.Pow(Random.value, 1f + lines[k, 3].x * 17f));
                }
            }
            if (Random.value < Mathf.Pow(lines[k, 3].x, 0.2f) && Random.value < Mathf.Pow(glitch, 0.8f - 0.4f * lines[k, 3].x))
            {
                lines[k, 0] += Custom.RNV() * 17f * lines[k, 3].x * power;
                lines[k, 3].y = Mathf.Max(lines[k, 3].y, glitch);
            }
            lines[k, 3].x = Custom.LerpAndTick(lines[k, 3].x, lines[k, 3].y, 0.01f, 0.033333335f);
            lines[k, 3].y = Mathf.Max(0f, lines[k, 3].y - 0.014285714f);
            if (Random.value < 1f / Mathf.Lerp(210f, 20f, glitch))
            {
                lines[k, 3].y = Mathf.Max(glitch, (Random.value < 0.5f) ? generalGlitch : Random.value);
            }
        }
        vel *= 0.995f;
        vel += Vector2.ClampMagnitude(hoverPos + new Vector2(0f, Mathf.Sin(sinCounter / 15f) * 7f) - pos, 15f) / 81f;
        vel += Custom.RNV() * Random.value * Random.value * Mathf.Lerp(0.06f, 0.4f, glitch);
        {
            generalGlitch = Mathf.Max(0f, generalGlitch - 0.008333334f);
            if (Random.value < 0.0027027028f)
            {
                generalGlitch = Random.value;
            }
            float f = Mathf.Sin(Mathf.Clamp(glitch, 0f, 1f) * 3.1415927f);
            if (Random.value < 0.05f + 0.35f * Mathf.Pow(f, 0.5f) && Random.value < power && !displayBounds)
            {
                room.AddObject(new CollectToken.TokenSpark(pos + Custom.RNV() * 6f * glitch, Custom.RNV() * Mathf.Lerp(2f, 9f, Mathf.Pow(f, 2f)) * Random.value, GoldCol(glitch), false));
            }
            glitch = Custom.LerpAndTick(glitch, generalGlitch / 2f, 0.01f, 0.033333335f);
            if (Random.value < 1f / Mathf.Lerp(360f, 10f, generalGlitch))
            {
                glitch = Mathf.Pow(Random.value, 1f - 0.85f * generalGlitch);
            }
            float num4 = float.MaxValue;
            bool flag = room.game.StoryCharacter == Enums.vinki;
            if (RainWorld.lockGameTimer)
            {
                flag = false;
            }
            float num5 = 140f;
            for (int n = 0; n < room.game.session.Players.Count; n++)
            {
                if (room.game.session.Players[n].realizedCreature != null && room.game.session.Players[n].realizedCreature.Consious && (room.game.session.Players[n].realizedCreature as Player).dangerGrasp == null && room.game.session.Players[n].realizedCreature.room == room)
                {
                    num4 = Mathf.Min(num4, Vector2.Distance(room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, pos));
                }
            }
            if (!flag && displayBounds)
            {
                if (Random.value < 0.14285715f)
                {
                    glitch = Mathf.Max(glitch, Random.value * Random.value * Random.value);
                }
            }
            if (displayBounds && expand == 0f && !contract && Random.value < Mathf.InverseLerp(num5 + 160f, num5 + 460f, num4))
            {
                // check if a player is within the graffiti bounds, and show the bounds if one is
                foreach (AbstractCreature player in room.game.session.Players)
                {
                    if (player.realizedCreature == null)
                    {
                        continue;
                    }

                    Vector2 playerPos = player.realizedCreature.mainBodyChunk.pos;
                    if (playerPos.x <= pos.x - boundsPoints[1].x || playerPos.x >= pos.x + boundsPoints[1].x ||
                        playerPos.y <= pos.y - boundsPoints[1].y || playerPos.y >= pos.y + boundsPoints[1].y)
                    {
                        displayBounds = false;
                        // Set vertices of hologram to be glasses
                        for (int i = 0; i < lines.GetLength(0); i++)
                        {
                            lines[i, 2] = glassesPoints[i];
                        }
                        room.PlaySound(SoundID.Token_Turn_Off, pos);
                    }
                }
            }
            else if (!displayBounds)
            {
                // check if a player is within the graffiti bounds, and show the bounds if one is
                foreach (AbstractCreature player in room.game.session.Players)
                {
                    if (player.realizedCreature == null)
                    {
                        continue;
                    }

                    Vector2 playerPos = player.realizedCreature.mainBodyChunk.pos;
                    if (playerPos.x >= pos.x - boundsPoints[1].x && playerPos.x <= pos.x + boundsPoints[1].x &&
                        playerPos.y >= pos.y - boundsPoints[1].y && playerPos.y <= pos.y + boundsPoints[1].y)
                    {
                        displayBounds = true;
                        // Set vertices of hologram to be bounds of graffiti
                        for (int i = 0; i < lines.GetLength(0); i++)
                        {
                            lines[i, 2] = boundsPoints[i];
                        }
                        room.PlaySound(SoundID.Token_Turn_On, pos);
                    }
                }
                
            }
        }
        base.Update(eu);
    }

    public Color GoldCol(float g)
    {
        if (sprayable)
        {
            return Color.Lerp(TokenColor, new Color(1f, 1f, 1f), 0.4f + 0.4f * Mathf.Max(contract ? 0.5f : (expand * 0.5f), Mathf.Pow(g, 0.5f)));
        }
        return Color.Lerp(TokenColor, new Color(1f, 1f, 1f), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, g), 0.5f));
    }
}

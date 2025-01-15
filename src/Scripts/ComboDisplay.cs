using System;
using HUD;
using RWCustom;
using UnityEngine;

namespace Vinki;

public class ComboDisplay : HudPart
{
    public Vector2 pos;
    public Vector2 lastPos;
    public int opacityTimer = 0;

    public float trickAndComboOffsetX = 0f;

	public VinkiPlayerData vinki;
	public FLabel currentTrick;
	public FLabel currentCombo;
	public FLabel trickScore;
    public FSprite comboTimer;
	public FLabel totalScore;

    public ComboDisplay(HUD.HUD hud, VinkiPlayerData vinki) : base(hud)
	{
        this.pos = new Vector2(50f, 500f);
        this.vinki = vinki;

        this.vinki.OnNewTrick += OnNewTrick;

        this.currentTrick = new(Custom.GetDisplayFont(), vinki.currentTrickName)
        {
            alignment = FLabelAlignment.Left,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 99f,
            scale = 1.25f
        };
		hud.fContainers[1].AddChild(this.currentTrick);

        this.currentCombo = new(Custom.GetDisplayFont(), "x")
        {
            alignment = FLabelAlignment.Left,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 46f,
            anchorY = 0f
        };
        hud.fContainers[1].AddChild(this.currentCombo);

        this.trickScore = new(Custom.GetFont(), vinki.currentTrickScore.ToString())
        {
            alignment = FLabelAlignment.Right,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 50f,
            anchorY = 0f
        };
        hud.fContainers[1].AddChild(this.trickScore);

        this.comboTimer = new FSprite("Futile_White", true)
        {
            scaleX = 15f,
            scaleY = 0.5f,
            color = new Color(0.6f, 0.6f, 0.6f),
            x = 0f,
            y = this.pos.y + 30f,
            anchorX = 0f,
            anchorY = 0f
        };
        hud.fContainers[1].AddChild(this.comboTimer);

        this.totalScore = new(Custom.GetFont(), $"{400 * vinki.comboSize}")
        {
            alignment = FLabelAlignment.Right,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 5f,
            anchorY = 0f
        };
        hud.fContainers[1].AddChild(this.totalScore);
    }

    public override void Update()
    {
        this.lastPos = this.pos;

        if (vinki.timeLeftInCombo <= 0)
        {
            this.opacityTimer--;
        }
        else
        {
            this.opacityTimer = 200;
        }

        if (this.trickAndComboOffsetX > 0f)
        {
            this.trickAndComboOffsetX -= 38f;
        }
    }

    public override void Draw(float timeStacker)
    {
        this.currentTrick.x = this.DrawPos(timeStacker).x - 25f - trickAndComboOffsetX;
        this.currentCombo.x = this.DrawPos(timeStacker).x + 120f - trickAndComboOffsetX;
        this.trickScore.x = this.DrawPos(timeStacker).x + 90f - trickAndComboOffsetX;
        this.comboTimer.scaleX = Mathf.Lerp(0f, 15f, vinki.timeLeftInCombo / 400f);
        this.totalScore.x = this.DrawPos(timeStacker).x + 120f;

        // Don't update the text if the combo is over, so players have a chance to see their end score and combo
        if (vinki.timeLeftInCombo > 0)
        {
            this.currentTrick.text = vinki.currentTrickName;
            this.currentCombo.text = "x" + vinki.comboSize.ToString();
            this.trickScore.text = vinki.currentTrickScore.ToString();
            this.totalScore.text = vinki.comboTotalScore.ToString();
        }

        float alpha = 1f;
        if (this.opacityTimer <= 120)
        {
            alpha = Mathf.Lerp(0f, 1f, this.opacityTimer / 120f);
        }
        this.currentTrick.alpha = alpha;
        this.currentCombo.alpha = alpha;
        this.trickScore.alpha = alpha;
        this.totalScore.alpha = alpha;
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
    }

    public void OnNewTrick()
    {
        if (vinki.timeLeftInCombo > 0)
        {
            trickAndComboOffsetX = 304f;
        }
    }

    public override void ClearSprites()
    {
        vinki.OnNewTrick -= OnNewTrick;
    }
}

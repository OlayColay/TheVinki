using System;
using HUD;
using RWCustom;
using UnityEngine;

namespace Vinki;

public class ComboDisplay : HudPart
{
    public Vector2 pos;
    public Vector2 lastPos;

	public VinkiPlayerData vinki;
	public FLabel currentTrick;
	public FLabel scoreAndCombo;
	public FSprite comboTimer;
	public FLabel totalScore;

    public ComboDisplay(HUD.HUD hud, VinkiPlayerData vinki) : base(hud)
	{
        this.pos = new Vector2(20.2f, 725.2f);
        this.vinki = vinki;

        this.currentTrick = new(Custom.GetDisplayFont(), "Frontside (New)")
        {
            alignment = FLabelAlignment.Left,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 69f
        };
		hud.fContainers[1].AddChild(this.currentTrick);

        this.scoreAndCombo = new(Custom.GetFont(), "400 x" + vinki.comboSize)
        {
            alignment = FLabelAlignment.Right,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y + 46f
        };
        hud.fContainers[1].AddChild(this.scoreAndCombo);

        this.comboTimer = new FSprite("LinearGradient200", true)
        {
            rotation = 90f,
            scaleY = 2f,
            color = new Color(0.6f, 0.6f, 0.6f),
            y = this.pos.y + 23f
        };
        hud.fContainers[1].AddChild(this.comboTimer);

        this.totalScore = new(Custom.GetFont(), $"400 * vinki.comboSize")
        {
            alignment = FLabelAlignment.Right,
            color = new Color(0.7f, 0.7f, 0.7f),
            y = this.pos.y
        };
        hud.fContainers[1].AddChild(this.totalScore);
    }

    public override void Update()
    {
        this.lastPos = this.pos;
    }

    public override void Draw(float timeStacker)
    {
        
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
    }
}

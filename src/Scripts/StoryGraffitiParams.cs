using System;
using UnityEngine;
using Vinki;

public class StoryGraffitiParams
{
    /// <summary> Room name that this graffiti will be sprayed in (ex. "SU_C01") </summary>
	public string room = "";
    /// <summary> Position of graffiti in room (ex. [100, 200]) </summary>
	public Vector2 position = Vector2.zero;
    /// <summary> Number of circular smoke effects to show before graffiti completes. Higher number means longer time to complete </summary>
	public int numSmokes = 10;
    /// <summary> Size of each smoke effect </summary>
    public float smokeSize = 2f;
    /// <summary> Opacity added to graffiti per smoke effect. Note that 1f isn't actually fully opaque in this case </summary>
	public float alphaPerSmoke = 0.3f;
    /// <summary> After spraying this graffiti as Vinki, show the design in campaigns that take place later in the timeline </summary>
	public bool spawnInFutureCampaigns = true;
    /// <summary> Position determines the position of the center of the graffiti instead of the bottom-left corner </summary>
    public bool anchorToCenter = false;
    /// <summary> Hide the hologram for this graffiti, although it will still be sprayable. </summary>
    public bool hideHologram = false;
    /// <summary> What graffiti to replace when spraying this one. That graffiti will be totally removed if it has numSmokes <= this one.
    /// Note that wis graffiti won't be sprayable until the replacee graffiti is sprayed.
    /// See Qu1a through Qu1c in Farm Arrays as an example</summary>
    public string replaces = "";

	public int replacesGNum = -1;
    public int enableAfterSpray = -1;

    public StoryGraffitiParams(string room, Vector2 position, int numSmokes, float smokeSize, float alphaPerSmoke,
        bool spawnInFutureCampaigns, bool anchorToCenter, bool hideHologram, string replaces)
	{
        this.room = room.ToLower();
        this.position = position;
        this.numSmokes = numSmokes;
        this.smokeSize = smokeSize;
        this.alphaPerSmoke = alphaPerSmoke;
        this.spawnInFutureCampaigns = spawnInFutureCampaigns;
        this.anchorToCenter = anchorToCenter;
        this.hideHologram = hideHologram;

        if (replaces != "" && Plugin.graffitis.ContainsKey("Story"))
        {
            this.replaces = replaces;
            this.replacesGNum = Plugin.graffitis["Story"].FindIndex(graffiti => graffiti.imageName.EndsWith(replaces));

            // Spraying the graffiti this one will replace should enable this graffiti to be sprayed
            Plugin.storyGraffitiParameters[this.replacesGNum].enableAfterSpray = Plugin.graffitis["Story"].Count;
        }

        Plugin.VLogger.LogInfo("Adding story graffiti with parameters: " + string.Join(", ",
        [
            this.room,
            this.position.ToString(),
            this.numSmokes,
            this.alphaPerSmoke,
            this.spawnInFutureCampaigns,
            this.anchorToCenter,
            this.replaces,
            this.replacesGNum
        ]));
    }
}

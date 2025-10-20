using System;
using UnityEngine;
using Vinki;

public class StoryGraffitiParams
{
	public string room = "";
	public Vector2 position = Vector2.zero;
	public int numSmokes = 10;
	public float alphaPerSmoke = 0.3f;
	public bool spawnInFutureCampaigns = true;
    public string replaces = "";
    public bool anchorToCenter = false;

	public int replacesGNum = -1;

    public StoryGraffitiParams(string room, Vector2 position, int numSmokes, float alphaPerSmoke, bool spawnInFutureCampaigns, bool anchorToCenter, string replaces)
	{
        this.room = room;
        this.position = position;
        this.numSmokes = numSmokes;
        this.alphaPerSmoke = alphaPerSmoke;
        this.spawnInFutureCampaigns = spawnInFutureCampaigns;
        this.anchorToCenter = anchorToCenter;

        if (replaces != "" && Plugin.graffitis.ContainsKey("Story"))
        {
            this.replaces = replaces;
            this.replacesGNum = Plugin.graffitis["Story"].FindIndex(graffiti => graffiti.imageName.EndsWith(replaces));
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

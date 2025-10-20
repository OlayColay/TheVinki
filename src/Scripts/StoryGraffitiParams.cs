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
	public int replacesGNum = -1;

    public StoryGraffitiParams(string room, Vector2 position, int numSmokes, float alphaPerSmoke, bool spawnInFutureCampaigns, string replaces)
	{
        this.room = room;
        this.position = position;
        this.numSmokes = numSmokes;
        this.alphaPerSmoke = alphaPerSmoke;
        this.spawnInFutureCampaigns = spawnInFutureCampaigns;

        if (replaces != "" && Plugin.graffitis.ContainsKey("Story"))
        {
            this.replaces = replaces;
            this.replacesGNum = Plugin.graffitis["Story"].FindIndex(graffiti =>
            {
                if (graffiti.imageName.EndsWith(replaces)) 
                { 
                    Plugin.VLogger.LogInfo("Setting " + graffiti.imageName + " as replacement");
                    return true;
                }
                return false;
            });
        }
    }
}

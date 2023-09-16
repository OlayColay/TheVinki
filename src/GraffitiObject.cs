using System;

public class GraffitiObject : CustomDecal
{
	int cyclePlaced = 0;
	bool isStory = false;

	public GraffitiObject(PlacedObject placedObject, int cyclePlaced, bool isStory = false) : base(placedObject)
    {
        this.cyclePlaced = cyclePlaced;
        this.isStory = isStory;
    }
}

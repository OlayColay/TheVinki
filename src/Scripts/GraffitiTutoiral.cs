using System.Linq;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace Vinki;

public class GraffitiTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Init,
        Craft,
        Spray,
        Throw,
        End
    }
    public Phase currentPhase { get; set; } = Phase.Init;
    public Vector2 craftTriggerPos { get; } = new(820.0f, 290.0f);

    public GraffitiTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded || !room.BeingViewed || currentPhase == Phase.End) return;
        var game = room.game;

        if (currentPhase == Phase.Init)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Press (GRAFFITI MODE + CRAFT) while carrying a rock and colorful object to craft a Spray Can."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Different objects produce different levels of Spray Cans. You can combine two Spray Cans, or upgrade one with another object."),
                800, 600, false, false
            );
            currentPhase = Phase.Craft;
        }
    }
}
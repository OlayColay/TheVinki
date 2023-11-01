using ImprovedInput;
using System.Linq;
using UnityEngine;

namespace Vinki;

public class GraffitiTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Craft,
        Spray,
        Throw,
        End
    }
    public Phase nextPhase { get; set; } = Phase.Craft;
    public Vector2 playerPos = Vector2.zero;
    public Vector2 craftTriggerPos { get; } = new(742f, 842f);
    public Vector2 sprayTriggerPos { get; } = new(190f, 1626f);
    public Vector2 throwTriggerPos { get; } = new(742f, 2442f);
    public float triggerReach = 100f;

    public GraffitiTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded || !room.BeingViewed || nextPhase == Phase.End || room.PlayersInRoom.Count < 1)
        {
            return;
        }

        playerPos = room.PlayersInRoom.First().mainBodyChunk.pos;
        //Debug.Log("Current player position: " + playerPos);
        var game = room.game;

        if (Vector2.Distance(playerPos, craftTriggerPos) <= triggerReach && nextPhase == Phase.Craft)
        {
            string graffitiMode = KeyCodeTranslator.Translate(0, PlayerKeybind.Get("thevinki:graffiti").CurrentBinding(0));
            string craft = KeyCodeTranslator.Translate(0, PlayerKeybind.Get("thevinki:craft").CurrentBinding(0));
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Hold (" + graffitiMode + " + " + craft + ") while carrying a rock and colorful object to craft a Spray Can."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Different objects produce different levels of Spray Cans. You can also combine two Spray Cans, or upgrade one with another object."),
                100, 600, false, false
            );
            nextPhase = Phase.Spray;
        }
        else if (Vector2.Distance(playerPos, sprayTriggerPos) <= triggerReach && nextPhase == Phase.Spray)
        {
            string graffitiMode = KeyCodeTranslator.Translate(0, PlayerKeybind.Get("thevinki:graffiti").CurrentBinding(0));
            string spray = KeyCodeTranslator.Translate(0, PlayerKeybind.Get("thevinki:spray").CurrentBinding(0));
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Press (" + graffitiMode + " + " + spray + ") while carrying a Spray Can to spend a charge and spray graffiti."),
                0, 600, false, false
            );
            nextPhase = Phase.Throw;
        }
        else if (Vector2.Distance(playerPos, throwTriggerPos) <= triggerReach && nextPhase == Phase.Throw)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Throwing a Spray Can will create a large, non-lethal explosion of compressed paint."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("The more charages a Spray Can has, the more force the explosion will have. (The max charges of a Spray Can is 5)"),
                100, 600, false, false
            );
            nextPhase = Phase.End;
        }
    }
}
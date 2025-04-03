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
    public Phase nextPhase = Phase.Craft;
    public Vector2 playerPos = Vector2.zero;
    public float craftTriggerPos { get; } = 842f;
    public float sprayTriggerPos { get; } = 1626f;
    public float throwTriggerPos { get; } = 2442f;

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
        //VLogger.LogInfo("Current player position: " + playerPos + "\tnextPhase: " + nextPhase.ToString());
        var game = room.game;

        if (playerPos.y >= craftTriggerPos && nextPhase == Phase.Craft)
        {
            string graffitiMode = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:graffiti") : "Up";
            string craft = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:craft") : "Pickup";
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Hold (" + graffitiMode + " + " + craft + ") while carrying a rock and colorful object (pearls, fruit, etc.) to craft a Spray Can."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Different objects produce different levels of Spray Cans. You can also combine two Spray Cans, or upgrade one with another object."),
                100, 600, false, false
            );
            nextPhase = Phase.Spray;
        }
        else if (playerPos.y >= sprayTriggerPos && nextPhase == Phase.Spray)
        {
            string graffitiMode = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:graffiti") + " + " : "";
            string spray = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:spray") : "Jump + Pickup";
            if (Plugin.improvedInput)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Press (" + graffitiMode + spray + ") while carrying a Spray Can to spend a charge and spray graffiti."),
                    0, 600, false, false
                );
            }
            else
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Press (" + spray + ") while in midair and carrying a Spray Can to spend a charge and spray graffiti."),
                    0, 600, false, false
                );
            }
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("You can also damage some creatures by spraying them."),
                0, 600, false, false
            );
            nextPhase = Phase.Throw;
        }
        else if (playerPos.y >= throwTriggerPos && nextPhase == Phase.Throw)
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
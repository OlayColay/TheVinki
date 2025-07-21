using System.Linq;
using UnityEngine;

namespace Vinki;

public class GraffitiTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Craft,
        Craft2,
        Spray,
        Spray2,
        Throw,
        Throw2,
        End
    }
    public Phase nextPhase = Phase.Craft;
    public Vector2 playerPos = Vector2.zero;
    public float CraftTriggerPos { get; } = 842f;
    public float SprayTriggerPos { get; } = 1626f;
    public float ThrowTriggerPos { get; } = 2442f;

    public GraffitiTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded || !room.BeingViewed || room.PlayersInRoom.Count < 1)
        {
            return;
        }

        playerPos = room.PlayersInRoom.First().mainBodyChunk.pos;
        //Plugin.VLogger.LogInfo("Current player position: " + playerPos + "\tnextPhase: " + nextPhase.ToString());
        var game = room.game;

        if (playerPos.y >= CraftTriggerPos && playerPos.y < SprayTriggerPos)
        {
            if (nextPhase == Phase.Craft)
            {
                string graffitiMode = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:graffiti", Plugin.Graffiti) : "Up";
                string craft = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:craft", Plugin.Craft) : "Pickup";
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Hold (" + graffitiMode + " + " + craft + ") while carrying a rock and colorful object (pearls, fruit, etc.) to craft a Spray Can."),
                    0, 600, false, true
                );
                nextPhase = Phase.Craft2;
            }
            else if (nextPhase == Phase.Craft2)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Objects have varying amount of charges. You can also combine two Spray Cans, or recharge a can with another colorful object."),
                    100, 600, false, true
                );
                nextPhase = Phase.Spray;
            }
        }
        else if (playerPos.y >= SprayTriggerPos && playerPos.y < ThrowTriggerPos)
        {
            if (nextPhase == Phase.Spray)
            {
                string graffitiMode = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:graffiti", Plugin.Graffiti) + " + " : "";
                string spray = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:spray", Plugin.Spray) : "Special";
                if (Plugin.improvedInput)
                {
                    game.cameras.First().hud.textPrompt.AddMessage(
                        game.manager.rainWorld.inGameTranslator.Translate("Press (" + graffitiMode + spray + ") while carrying a Spray Can to spend a charge and spray graffiti."),
                        0, 600, false, true
                    );
                }
                else
                {
                    game.cameras.First().hud.textPrompt.AddMessage(
                        game.manager.rainWorld.inGameTranslator.Translate("Press (" + spray + ") while carrying a Spray Can to spend a charge and spray graffiti."),
                        0, 600, false, true
                    );
                }
                nextPhase = Phase.Spray2;
            }
            else if (nextPhase == Phase.Spray2)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("You can also damage most creatures by \"tagging\" them."),
                    0, 600, false, true
                );
                nextPhase = Phase.Throw;
            }
        }
        else if (playerPos.y >= ThrowTriggerPos)
        {
            if (nextPhase == Phase.Throw)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Throwing a Spray Can will create a large, non-lethal explosion of compressed paint."),
                    0, 600, false, true
                );
                nextPhase = Phase.Throw2;
            }
            else if (nextPhase == Phase.Throw2)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("The more charges a Spray Can has, the more force the explosion will have. (The max charges of a Spray Can is 5)"),
                    100, 600, false, true
                );
                nextPhase = Phase.End;
            }
            else if (nextPhase == Phase.End)
            {
                this.Destroy();
            }
        }
    }
}
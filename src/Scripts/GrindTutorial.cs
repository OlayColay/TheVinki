﻿using SlugBase.SaveData;
using System.Linq;

namespace Vinki;

public class GrindTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Grind,
        Land,
        Coyote,
        End
    }
    public Phase NextPhase { get; set; } = Phase.Grind;
    public float playerPos = float.MaxValue;
    public const float landTriggerPos = 6030f;
    public const float coyoteTriggerPos = 1933f;

    public GrindTutorial(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);        

        if (!room.fullyLoaded || !room.BeingViewed || NextPhase == Phase.End || room.PlayersInRoom.Count < 1)
        {
            return;
        }

        var game = room.game;
        playerPos = room.PlayersInRoom.First().mainBodyChunk.pos.x;

        if (NextPhase == Phase.Grind)
        {
            string grind = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:grind", Plugin.Grind) : "Pickup";
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Hold (" + grind + ") while moving atop a horizontal beam or climbing a vertical pole to grind."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("While grinding atop a horizontal beam, jump to execute a trick jump."),
                100, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("While grinding up a vertical pole, hold jump to trick jump off the top of the pole."),
                100, 600, false, false
            );
            NextPhase = Phase.Land;
        }
        else if (playerPos <= landTriggerPos && playerPos > coyoteTriggerPos && NextPhase == Phase.Land)
        {
            string grind = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:grind", Plugin.Grind) : "Pickup";
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Hold (" + grind + ") while falling to catch a horizontal beam with your feet and continue grinding."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("You can hold down to fall off a beam, then let go to catch another beam."),
                0, 600, false, false
            );
            NextPhase = Phase.Coyote;
        }
        else if (playerPos <= coyoteTriggerPos && NextPhase == Phase.Coyote)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("By jumping at the very end of a beam, you will Coyote Jump. This gives you even more jump height and speed."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("You can execute a similar boost by falling from grinding a beam, and jumping right when you land on the ground."),
                100, 600, false, false
            );
            NextPhase = Phase.End;
            SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(room.game.rainWorld.progression.currentSaveState.miscWorldSaveData);
            miscSave.Set("GrindTutorialCompleted", true);
        }
    }
}
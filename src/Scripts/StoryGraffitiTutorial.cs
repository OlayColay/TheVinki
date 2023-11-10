using ImprovedInput;
using System.Linq;
using UnityEngine;

namespace Vinki;

public class StoryGraffitiTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Spray = 0,
        Explore = 1,
        End = 2
    }
    public Phase nextPhase = Phase.Spray;
    public Vector2 playerPos = Vector2.zero;
    public Vector2 triggerPos { get; } = Vector2.zero;
    public float triggerReach = 1000f;
    public SlugBase.SaveData.SlugBaseSaveData miscSave;

    public StoryGraffitiTutorial(Room room)
    {
        this.room = room;
        miscSave = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(room.game.GetStorySession.saveState.miscWorldSaveData);
        miscSave.TryGet("StoryGraffitiTutorialPhase", out int nextPhaseInt);  // If there's nothing in the save, it should give 0
        nextPhase = (Phase)nextPhaseInt;
        Debug.Log("Created StoryGraffitiTutorial at phase " + nextPhase.ToString());
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

        if (Vector2.Distance(playerPos, triggerPos) <= triggerReach && nextPhase == Phase.Spray)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("The hologram-like visions show where you need to spray in order to pursue your goal."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("Some large pieces may require you to spray from multiple positions."),
                100, 600, false, false
            );
            nextPhase = Phase.End;
        }
        else if (Vector2.Distance(playerPos, triggerPos) <= triggerReach && nextPhase == Phase.Explore)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("It appears that Five Pebbles might not tolerate your graffiti being anywhere near him. Search for better spots elsewhere."),
                0, 600, false, false
            );
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("... in a future update."),
                10, 600, false, false
            );
            //game.cameras.First().hud.textPrompt.AddMessage(
            //    game.manager.rainWorld.inGameTranslator.Translate("Access the Quest Map screen while hibernating to view hints of where to paint next."),
            //    100, 600, false, false
            //);
            miscSave.Set("StoryGraffitiTutorialPhase", (int)Phase.End);
            nextPhase = Phase.End;
        }
    }
}
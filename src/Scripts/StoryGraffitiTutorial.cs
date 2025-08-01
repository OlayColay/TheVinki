﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;

public class StoryGraffitiTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Spray,
        Spray2,
        Explore,
        Explore2,
        End
    }
    public Phase nextPhase = Phase.Spray;
    public Vector2 playerPos = Vector2.zero;
    public Vector2 TriggerPos { get; } = Vector2.zero;
    public float triggerReach = 1000f;
    public SlugBase.SaveData.SlugBaseSaveData miscSave;

    public StoryGraffitiTutorial(Room room)
    {
        this.room = room;
        miscSave = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(room.game.GetStorySession.saveState.miscWorldSaveData);
        miscSave.TryGet("StoryGraffitiTutorialPhase", out int nextPhaseInt);  // If there's nothing in the save, it should give 0
        Plugin.VLogger.LogInfo("StoryGraffitiTutorialPhase: " + (Phase)nextPhaseInt);
        if (nextPhaseInt == 0)
        {
            miscSave.TryGet("StoryGraffitiSprayed", out List<int> sprd);
            nextPhase = sprd != null && sprd.Contains(2) ? Phase.Explore : Phase.Spray;
        }
        else
        {
            nextPhase = (Phase)nextPhaseInt;
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded || !room.BeingViewed || room.PlayersInRoom.Count < 1)
        {
            return;
        }

        playerPos = room.PlayersInRoom.First().mainBodyChunk.pos;
        //Plugin.VLogger.LogInfo("Current player position: " + playerPos);
        var game = room.game;

        if (Vector2.Distance(playerPos, TriggerPos) <= triggerReach)
        {
            if (nextPhase == Phase.Spray)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("The hologram-like visions show where you need to spray in order to pursue your goal."),
                    0, 600, false, true
                );
                nextPhase = Phase.Spray2;
            }
            else if (nextPhase == Phase.Spray2)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Some large pieces may require you to spray from multiple positions."),
                    100, 600, false, true
                );
                nextPhase = Phase.End;
            }
            else if (nextPhase == Phase.Explore)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("It appears that Five Pebbles might not tolerate your graffiti being anywhere near him. Search for better spots elsewhere."),
                    0, 600, false, true
                );
                nextPhase = Phase.Explore2;
            }
            else if (nextPhase == Phase.Explore2)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("... in a future update."),
                    10, 600, false, true
                );
                //game.cameras.First().hud.textPrompt.AddMessage(
                //    game.manager.rainWorld.inGameTranslator.Translate("Access the Quest Map screen while hibernating to view hints of where to paint next."),
                //    100, 600, false, true
                //);
                nextPhase = Phase.End;
                miscSave.Set("StoryGraffitiTutorialPhase", (int)nextPhase);
            }
        }
        else if (nextPhase == Phase.End)
        {
            this.Destroy();
        }
    }
}
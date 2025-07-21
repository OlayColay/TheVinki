using SlugBase.SaveData;
using System.Linq;

namespace Vinki;

public class TagTutorial : UpdatableAndDeletable
{
    public enum Phase
    {
        Tag,
        Tag2,
        End
    }
    public Phase NextPhase { get; set; } = Phase.Tag;
    public const float landTriggerPos = 6030f;
    public const float coyoteTriggerPos = 1933f;

    public TagTutorial(Room room)
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

        var game = room.game;

        if (NextPhase == Phase.Tag)
        {
            string graffitiMode = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:graffiti", Plugin.Graffiti) + " + " : "";
            string tag = Plugin.improvedInput ? KeyCodeTranslator.GetImprovedInputKeyName(0, "thevinki:tag", Plugin.Tag) : "Special";
            if (Plugin.improvedInput)
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Press (" + graffitiMode + tag + ") while carrying a Spray Can near a creature to spend a charge to tag them."),
                    0, 600, false, true
                );
            }
            else
            {
                game.cameras.First().hud.textPrompt.AddMessage(
                    game.manager.rainWorld.inGameTranslator.Translate("Press (" + tag + ") while carrying a Spray Can near a creature to spend a charge to tag them."),
                    0, 600, false, true
                );
            }
            NextPhase = Phase.Tag2;
        }
        else if (NextPhase == Phase.Tag2)
        {
            game.cameras.First().hud.textPrompt.AddMessage(
                game.manager.rainWorld.inGameTranslator.Translate("This will stun the creature and deal damage to them over time."),
                0, 600, false, true
            );
            NextPhase = Phase.End;
        }
        else if (NextPhase == Phase.End)
        {
            SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(room.game.rainWorld.progression.currentSaveState.miscWorldSaveData);
            miscSave.Set("TagTutorialCompleted", true);
            this.Destroy();
        }
    }
}
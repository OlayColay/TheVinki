namespace Vinki;

public static partial class Hooks
{
    // Add hooks
    private static void ApplyRoomHooks()
    {
        On.Room.Loaded += Room_Loaded;

        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
    }

    private static CutsceneVinkiIntro intro = null;
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.game?.GetStorySession?.saveState?.saveStateNumber != Enums.TheVinki)
        {
            return;
        }

        if (self.abstractRoom?.name == "SS_AI" && self.game?.GetStorySession?.saveState?.miscWorldSaveData?.SSaiConversationsHad == 0)
        {
            intro = new CutsceneVinkiIntro(self);
            self.AddObject(intro);
        }
        orig(self);
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room self)
    {
        orig(self);

        if (self.game.GetStorySession.saveState.saveStateNumber != Enums.TheVinki)
        {
            return;
        }

        if (!self.abstractRoom.firstTimeRealized)
        {
            return;
        }

        // Graffiti Tutorial
        else if (self.abstractRoom?.name == "SS_E08")
        {
            self.AddObject(new GraffitiTutorial(self));
        }
    }
}

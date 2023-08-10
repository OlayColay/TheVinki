namespace Vinki;

public static partial class Hooks
{
    // Add hooks
    private static void ApplyRoomHooks()
    {
        On.Room.Loaded += Room_Loaded;
    }

    private static CutsceneVinkiIntro intro = null;
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.abstractRoom?.name == "SS_AI" && self.game?.GetStorySession?.saveState?.miscWorldSaveData?.SSaiConversationsHad == 0)
        {
            intro = new CutsceneVinkiIntro(self);
            self.AddObject(intro);
        }
        orig(self);
    }
}

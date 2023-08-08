namespace Vinki;

public static partial class Hooks
{
    // Add hooks
    private static void ApplyRoomHooks()
    {
        On.Room.Loaded += Room_Loaded;
    }

    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (self.abstractRoom.name == "SS_AI")
        {
            self.AddObject(new CutsceneVinkiIntro(self));
        }
        orig(self);
    }
}

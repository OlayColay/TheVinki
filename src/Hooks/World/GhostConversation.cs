
namespace Vinki;
public static partial class Hooks
{
	public static void ApplyGhostConversationHooks()
	{
		On.GhostConversation.AddEvents += GhostConversation_AddEvents;
	}

    private static void GhostConversation_AddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
    {
        if (self.currentSaveFile != Enums.vinki)
        {
            orig(self);
            return;
        }

        
    }
}

using RWCustom;
using System;
using System.IO;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplySaveStateHooks()
    {
        On.SaveState.GetStoryDenPosition += SaveState_GetStoryDenPosition;
    }

    private static string SaveState_GetStoryDenPosition(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
    {
        if (slugcat != Enums.vinki)
        {
            return orig(slugcat, out isVanilla);
        }

        string den = Plugin.introPlayed ? "SS_D07" : "SS_AI";

        if (WorldLoader.FindRoomFile(den, false, ".txt") != null)
        {
            // Adapted from SaveState.TrySetVanillaDen
            string root = Custom.RootFolderDirectory();
            string regionName = "";
            if (den.Contains("_"))
            {
                regionName = den.Split('_')[0];
            }

            isVanilla = File.Exists(Path.Combine(root, "World", regionName + "-Rooms", den + ".txt"))
                     || File.Exists(Path.Combine(root, "World", "Gate Shelters", den + ".txt"));

            return den;
        }
        return orig(slugcat, out isVanilla);
    }
}

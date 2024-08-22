using RWCustom;
using System;
using System.IO;
using System.Linq;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplySaveStateHooks()
    {
        On.SaveState.GetStoryDenPosition += SaveState_GetStoryDenPosition;

        On.PlayerProgression.WipeSaveState += (On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name name) => {
            if (name == Enums.vinki)
            {
                Plugin.introPlayed = false;
                // Reset graffitis on map
                Plugin.storyGraffitisOnMap = [];
            }
            orig(self, name);
        };
        On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool += PlayerProgression_MiscProgressionData_GetTokenCollected;
    }

    private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
    {
        throw new NotImplementedException();
    }

    private static void RemoveSaveStateHooks()
    {
        On.SaveState.GetStoryDenPosition -= SaveState_GetStoryDenPosition;

        On.PlayerProgression.WipeSaveState -= (On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name name) => {
            if (name == Enums.vinki)
            {
                Plugin.introPlayed = false;
                // Reset graffitis on map
                Plugin.storyGraffitisOnMap = [];
            }
            orig(self, name);
        };
        On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool -= PlayerProgression_MiscProgressionData_GetTokenCollected;
    }

    private static string SaveState_GetStoryDenPosition(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
    {
        if (slugcat != Enums.vinki)
        {
            return orig(slugcat, out isVanilla);
        }

        string den = VinkiConfig.SkipIntro.Value ? "UW_F01" : (Plugin.introPlayed ? "SS_D07" : "SS_AI");

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

    private static bool PlayerProgression_MiscProgressionData_GetTokenCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_string_bool orig, PlayerProgression.MiscProgressionData self, string tokenString, bool sandbox)
    {
        // If the current slugcat isn't Vinki and the option is disabled, disable the token
        if (!VinkiConfig.TokensInEveryCampaign.Value && self.currentlySelectedSinglePlayerSlugcat != Enums.vinki)
        {
            return true;
        }

        // Is this string in the Unlockables graffiti folder?
        string unlockablesPath = AssetManager.ResolveDirectory("decals/Unlockables/");
        string fileName = Array.Find(Directory.GetFiles(unlockablesPath).Select(Path.GetFileNameWithoutExtension).ToArray(), (file) => file.EndsWith(tokenString));
        if (fileName == null) 
        {
            // Try again in regular decals folder
            fileName = AssetManager.ResolveFilePath("decals/" + tokenString + ".png");
            if (!File.Exists(fileName))
            {
                return orig(self, tokenString, sandbox);
            }
        }

        // Attempt to find the graffiti in the vinki graffiti folder. If it's not there, the token be available
        string unlockedPath = AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki/");
        fileName = Array.Find(Directory.GetFiles(unlockedPath).Select(Path.GetFileNameWithoutExtension).ToArray(), (file) => file.EndsWith(tokenString));
        return fileName != null;
    }
}

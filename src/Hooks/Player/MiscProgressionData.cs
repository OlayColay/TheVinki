using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Vinki;

public class MiscProgressionDataData()
{
    public List<Enums.GraffitiUnlockID> graffitiTokens = [];
}
public static class MiscProgressionDataExtension
{
    private static readonly ConditionalWeakTable<PlayerProgression.MiscProgressionData, MiscProgressionDataData> cwt = new();

    public static MiscProgressionDataData Vinki(this PlayerProgression.MiscProgressionData gm) => cwt.GetValue(gm, _ => new MiscProgressionDataData());

    public static bool GetTokenCollected(this PlayerProgression.MiscProgressionData playerProgression, Enums.GraffitiUnlockID graffitiToken)
    {
        return graffitiToken != null && playerProgression.Vinki().graffitiTokens.Contains(graffitiToken);
    }

    public static bool SetTokenCollected(this PlayerProgression.MiscProgressionData playerProgression, Enums.GraffitiUnlockID graffititoken)
    {
        if (graffititoken == null || playerProgression.GetTokenCollected(graffititoken))
        {
            return false;
        }
        playerProgression.Vinki().graffitiTokens.Add(graffititoken);
        return true;
    }
}

public static partial class Hooks
{
    private static void ApplyMiscProgressionDataHooks()
    {
        On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
        //On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
    }

    private static void RemoveMiscProgressionDataHooks()
    {
        On.PlayerProgression.MiscProgressionData.FromString -= MiscProgressionData_FromString;
        //On.PlayerProgression.MiscProgressionData.ToString -= MiscProgressionData_ToString;
    }

    private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
    {
        orig(self, s);

        // Get array of graffitis that are in the Unlockables folder
        string[] names = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals" + Path.DirectorySeparatorChar + "Unlockables"), "*.png", SearchOption.AllDirectories)
            .ToArray().Select(Path.GetFileNameWithoutExtension).ToArray();
        // Get array of graffitis that are already in the VinkiGraffiti folder
        string[] vinkiNames = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals" + Path.DirectorySeparatorChar + "VinkiGraffiti" + Path.DirectorySeparatorChar + "vinki"), "*.png", SearchOption.AllDirectories)
            .ToArray().Select(Path.GetFileNameWithoutExtension).ToArray();
        // Get the intersection and chop off the author name
        var unlockedNames = names.Intersect(vinkiNames).Select(str => str.Substring(str.LastIndexOf(" - ") + 3));
        //Plugin.VLogger.LogInfo("Unlocked graffiti: " + string.Join(", ", unlockedNames));

        MiscProgressionDataData ext = self.Vinki();
        ext.graffitiTokens.AddRange(unlockedNames.Select(name => new Enums.GraffitiUnlockID(name, false)));

        //foreach (string saveStr in self.unrecognizedSaveStrings)
        //{
        //    string[] array2 = Regex.Split(saveStr, "<mpdB>");
        //    string text = array2[0];
        //    if (text == "GRAFFITITOKENS")
        //    {
        //        MiscProgressionDataData ext = self.Vinki();
        //        ext.graffitiTokens.Clear();
        //        foreach (string text11 in array2[1].Split([',']))
        //        {
        //            if (text11 != string.Empty)
        //            {
        //                ext.graffitiTokens.Add(new Enums.GraffitiUnlockID(text11, false));
        //            }
        //        }
        //        Plugin.VLogger.LogInfo("Found graffiti tokens in save!");
        //        break;
        //    }
        //}
    }

    private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
    {
        string text = orig(self);

        MiscProgressionDataData ext = self.Vinki();
        if (ext.graffitiTokens.Count > 0)
        {
            text += "GRAFFITITOKENS<mpdB>";
            for (int l = 0; l < ext.graffitiTokens.Count; l++)
            {
                string text1 = text;
                Enums.GraffitiUnlockID graffitiUnlockID = ext.graffitiTokens[l];
                text = text1 + (graffitiUnlockID?.ToString());
                if (l < ext.graffitiTokens.Count - 1)
                {
                    text += ",";
                }
            }
            text += "<mpdA>";
        }

        return text;
    }
}
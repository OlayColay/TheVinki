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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "miscProgressionData.GetTokenCollected used by Rain World")]
    public static bool GetTokenCollected(this PlayerProgression.MiscProgressionData playerProgression, Enums.GraffitiUnlockID graffitiToken)
    {
        if (graffitiToken == null)
        {
            return false;
        }

        // Get array of graffitis that are in the Unlockables folder
        var names = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals" + Path.DirectorySeparatorChar + "VinkiGraffiti" + Path.DirectorySeparatorChar + "vinki"), "*.png", SearchOption.AllDirectories)
            .ToArray().Select(Path.GetFileNameWithoutExtension).ToArray();

        Plugin.VLogger.LogInfo("Graffiti token: " + graffitiToken.value);
        return names.Any(name => name.EndsWith(graffitiToken.value));
    }
}
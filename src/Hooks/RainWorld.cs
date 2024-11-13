using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Vinki;

public class RainWorldData()
{
    public Dictionary<string, List<Enums.GraffitiUnlockID>> regionGraffitiTokens = [];
}
public static class RainWorldExtension
{
    private static readonly ConditionalWeakTable<RainWorld, RainWorldData> cwt = new();

    public static RainWorldData Vinki(this RainWorld gm) => cwt.GetValue(gm, _ => new RainWorldData());
}

public static partial class Hooks
{
    private static void ApplyRainWorldHooks()
    {
        //try
        //{
        //    IL.RainWorld.BuildTokenCache += IL_RainWorld_BuildTokenCache;
        //}
        //catch (Exception ex)
        //{
        //    Plugin.VLogger.LogError("Could not apply BuildTokenCache IL hook\n" + ex.Message);
        //}
        try
        {
            IL.RainWorld.ReadTokenCache += RainWorld_ReadTokenCache;
        }
        catch (Exception ex)
        {
            Plugin.VLogger.LogError("Could not apply ReadTokenCache IL hook\n" + ex.Message);
        }

        On.RainWorld.BuildTokenCache += On_RainWorld_BuildTokenCache;
        On.RainWorld.ClearTokenCacheInMemory += RainWorld_ClearTokenCacheInMemory;
    }

    private static void IL_RainWorld_BuildTokenCache(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel continueLabel = null;
        int placedObjectIndex = 29;

        try
        {
            c.GotoNext(
                x => x.MatchNewobj<PlacedObject>(),
                x => x.MatchStloc(out placedObjectIndex)
            );
            c.GotoNext(
                x => x.MatchLdsfld(typeof(ModManager).GetField(nameof(ModManager.MSC))),
                x => x.MatchBrfalse(out continueLabel)
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_2);
            c.Emit(OpCodes.Ldloc, placedObjectIndex);
            c.EmitDelegate((RainWorld self, string region, PlacedObject placedObject) =>
            {
                if (placedObject.type == PlacedObject.Type.GoldToken && ExtEnum<Enums.GraffitiUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
                {
                    RainWorldData ext = self.Vinki();
                    string fileName = region.ToLowerInvariant();
                    Enums.GraffitiUnlockID levelUnlockID = new((placedObject.data as CollectToken.CollectTokenData).tokenString, false);
                    if (!ext.regionGraffitiTokens[fileName].Contains(levelUnlockID))
                    {
                        ext.regionGraffitiTokens[fileName].Add(levelUnlockID);
                    }
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue_S, continueLabel);
        }
        catch (Exception e)
        {
            Plugin.VLogger.LogError("Could not complete BuildTokenCache IL Hook\n" + e.Message + '\n' + e.StackTrace);
        }
    }

    private static void RainWorld_ReadTokenCache(ILContext il)
    {
        var c = new ILCursor(il);
        int fileNameIndex = 3;
        int cacheTextIndex = 5;

        try
        {
            // Initializing region's graffiti token validSlugcats
            c.GotoNext(
                x => x.MatchCallvirt(typeof(string).GetMethod(nameof(string.ToLowerInvariant))),
                x => x.MatchStloc(out fileNameIndex)
            );
            c.GotoNext(
                x => x.MatchLdcI4(7),
                x => x.MatchNewarr<string>()
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, fileNameIndex);
            c.EmitDelegate((RainWorld self, string fileName) => self.Vinki().regionGraffitiTokens[fileName] = []);

            // Adding graffiti tokens to region
            c.GotoNext(
                MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchCall(typeof(File).GetMethod(nameof(File.ReadAllText), [typeof(string)])),
                x => x.MatchLdcI4(1),
                x => x.MatchNewarr<char>(),
                x => x.MatchDup(),
                x => x.MatchLdcI4(0),
                x => x.MatchLdcI4(out _),
                x => x.MatchStelemI2(),
                x => x.MatchCallvirt(out _),
                x => x.MatchStloc(out cacheTextIndex)
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, cacheTextIndex);
            c.Emit(OpCodes.Ldloc, fileNameIndex);
            c.EmitDelegate((RainWorld self, string[] cacheText, string fileName) =>
            {
                RainWorldData ext = self.Vinki();
                for (int j = 6; j < cacheText.Length; j++)
                {
                    if (cacheText[j] != "")
                    {
                        string[] token = Regex.Split(cacheText[j], ",");
                        for (int k = 0; k < token.Length; k++)
                        {
                            string[] tokenSections = Regex.Split(token[k], "~");
                            if (Enums.GraffitiUnlockID.values.entries.Contains(tokenSections[0]))
                            {
                                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID(tokenSections[0], false));
                            }
                        }
                    }
                }
            });
        }
        catch (Exception e)
        {
            Plugin.VLogger.LogError("Could not complete ReadTokenCache IL Hook\n" + e.Message + '\n' + e.StackTrace);
        }
    }

    private static void On_RainWorld_BuildTokenCache(On.RainWorld.orig_BuildTokenCache orig, RainWorld self, bool modded, string region)
    {
        string fileName = region.ToLowerInvariant();
        RainWorldData ext = self.Vinki();
        Dictionary<string, List<Enums.GraffitiUnlockID>> dictionary = ext.regionGraffitiTokens;
        lock (dictionary)
        {
            ext.regionGraffitiTokens[fileName] = [];
        }
        //Plugin.VLogger.LogInfo("Building token cache for " + fileName);

        // I can't figure out how to IL this, so I'm hard-coding the region graffiti tokens for now
        BuildGraffitiTokenCacheManually(ext, fileName);
        
        orig(self, modded, region);

        string text = string.Concat(
        [
            "World",
            Path.DirectorySeparatorChar.ToString(),
            "IndexMaps",
            Path.DirectorySeparatorChar.ToString()
        ]).ToLowerInvariant();
        string cachePath = AssetManager.ResolveFilePath(text + "tokencache" + fileName + ".txt");
        string cacheText = File.ReadAllText(cachePath);
        cacheText += "&";

        for (int i = 0; i < ext.regionGraffitiTokens[fileName].Count; i++)
        {
            string text9 = string.Join("|", [Enums.vinkiStr]);
            string text10 = cacheText;
            Enums.GraffitiUnlockID levelUnlockID2 = ext.regionGraffitiTokens[fileName][i];
            cacheText = text10 + (levelUnlockID2?.ToString()) + "~" + text9;
            if (i != ext.regionGraffitiTokens[fileName].Count - 1)
            {
                cacheText += ",";
            }
        }

        File.WriteAllText(cachePath, cacheText);
    }

    private static void RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
    {
        orig(self);

        self.Vinki().regionGraffitiTokens.Clear();
    }

    private static void BuildGraffitiTokenCacheManually(RainWorldData ext, string fileName)
    {
        switch (fileName)
        {
            case "cc":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Vulture Remote"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Stolen Face"));
                break;
            case "dm":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Lil Moon"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Slugcat Moon"));
                break;
            case "ds":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Lilypuck Karma"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Pop Pop Snails"));
                break;
            case "gw":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Garbage Alarm"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Acid Bath"));
                break;
            case "hi":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Dropwig Thief"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Living Paintbrush"));
                break;
            case "lf":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Spore Bombs"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Angry Pinecone"));
                break;
            case "lm":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Jellyfish"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Jetfish Friend"));
                break;
            case "sb":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Holy Gooieduck"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Leviathan Jaws"));
                break;
            case "sh":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Crybaby"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Miros Jaws"));
                break;
            case "si":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Squidburger"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Dandelion Peaches"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Gummy Lizard"));
                break;
            case "ss":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Loud Pebbles"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Neuron Dance"));
                break;
            case "su":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Noodle Spears"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Weapon Pile"));
                break;
            case "uw":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("City View"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Overseer Kiss"));
                break;
            case "vs":
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Questionable Centipede"));
                ext.regionGraffitiTokens[fileName].Add(new Enums.GraffitiUnlockID("Batnip"));
                break;
            default:
                break;
        }
    }
}
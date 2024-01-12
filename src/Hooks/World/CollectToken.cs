using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.DevInterface;
using SlugBase.SaveData;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vinki;

public class CollectTokenDataData
{
    public bool vinkiToken = false;
    public string tokenString = string.Empty;

    public static HSLColor GraffitiColor { get; } = new HSLColor(0.85f, 1f, 0.8f);
}
public static class CollectTokenExtension
{
    private static readonly ConditionalWeakTable<CollectToken.CollectTokenData, CollectTokenDataData> cwtData = new();
    public static CollectTokenDataData Vinki(this CollectToken.CollectTokenData ct) => cwtData.GetValue(ct, _ => new CollectTokenDataData());
}

public static partial class Hooks
{
    private static void ApplyCollectTokenHooks()
    {
        On.CollectToken.GoldCol += CollectToken_GoldCol;
        On.CollectToken.Pop += CollectToken_Pop;
        On.CollectToken.Update += CollectToken_Update;

        On.CollectToken.CollectTokenData.FromString += CollectTokenData_FromString;

        On.DevInterface.TokenRepresentation.TokenName += TokenRepresentation_TokenName;
    }

    private static Color CollectToken_GoldCol(On.CollectToken.orig_GoldCol orig, CollectToken self, float g)
    {
        if (self.placedObj.data is not CollectToken.CollectTokenData)
        {
            return orig(self, g);
        }

        CollectToken.CollectTokenData data = (self.placedObj.data as CollectToken.CollectTokenData);
        if (data.Vinki().vinkiToken)
        {
            return CollectTokenDataData.GraffitiColor.rgb;
        }
        return orig(self, g);
    }

    private static void CollectToken_Pop(On.CollectToken.orig_Pop orig, CollectToken self, Player player)
    {
        orig(self, player);

        CollectTokenDataData ext = (self.placedObj.data as CollectToken.CollectTokenData).Vinki();

        if (ext.vinkiToken)
        {
            self.anythingUnlocked = true;
            UnlockGraffitiMidgame(ext.tokenString);
        }
    }

    private static void CollectToken_Update(On.CollectToken.orig_Update orig, CollectToken self, bool eu)
    {
        orig(self, eu);

        CollectTokenDataData ext = (self.placedObj.data as CollectToken.CollectTokenData).Vinki();

        if (ext.vinkiToken && self.expand < 0f)
        {
            if (self.anythingUnlocked && self.room.game.cameras[0].hud != null && self.room.game.cameras[0].hud.textPrompt != null)
            {
                self.room.game.cameras[0].hud.textPrompt.messages.Clear();
                self.room.game.cameras[0].hud.textPrompt.messageString = "";
                self.room.game.cameras[0].hud.textPrompt.AddMessage(self.room.game.manager.rainWorld.inGameTranslator.Translate("New graffiti unlocked: ") + ext.tokenString, 20, 160, true, true);
            }
        }
    }

    private static void CollectTokenData_FromString(On.CollectToken.CollectTokenData.orig_FromString orig, CollectToken.CollectTokenData self, string s)
    {
        orig(self, s);

        string[] array = Regex.Split(s, "~");
        self.Vinki().vinkiToken = (ModManager.MSC && array.Contains(Enums.vinkiStr));
        self.Vinki().tokenString = array[5];
    }

    private static string TokenRepresentation_TokenName(TokenRepresentation.orig_TokenName orig, CollectToken.CollectTokenData data)
    {
        if (data.Vinki().vinkiToken)
        {
            return "Graffiti Token";
        }
        return orig(data);
    }
    private static void UnlockGraffitiMidgame(string fileName)
    {
        // Copy file to VinkiGraffiti
        string unlockPath = AssetManager.ResolveDirectory("decals/Unlockables/");
        string filePath = Array.Find(Directory.GetFiles(unlockPath).Select(Path.GetFileNameWithoutExtension).ToArray(), (file) => file.EndsWith(fileName));
        if (!Directory.Exists(unlockPath) || filePath == null || filePath == string.Empty)
        {
            filePath = AssetManager.ResolveFilePath("decals/" + fileName + ".png");
            if (!Directory.Exists(unlockPath) || filePath == null || filePath == string.Empty)
            {
                throw new Exception("Could not find unlockable graffiti " + filePath + " in Unlockables folder or decals folder!");
            }
            if (File.Exists(AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki/") + fileName + ".png"))
            {
                throw new Exception("Graffiti already unlocked! " + filePath);
            }
            File.Copy(filePath, AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki/") + fileName + ".png");
        }
        else if (File.Exists(AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki/") + fileName + ".png"))
        {
            throw new Exception("Graffiti already unlocked! " + filePath);
        }
        else
        {
            File.Copy(unlockPath + filePath + ".png", AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki/") + filePath + ".png");
        }

        PlacedObject.CustomDecalData decal = new PlacedObject.CustomDecalData(null);
        decal.fromDepth = 0.2f;

        string parentDir = AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki");

        // Get the image as a 2d texture so we can resize it to something manageable
        Texture2D img = new Texture2D(2, 2);
        if (File.Exists(parentDir + Path.DirectorySeparatorChar + filePath + ".png"))
        {
            decal.imageName = "VinkiGraffiti/vinki/" + filePath;
            byte[] tmpBytes = File.ReadAllBytes(parentDir + Path.DirectorySeparatorChar + filePath + ".png");
            ImageConversion.LoadImage(img, tmpBytes);
        }
        else
        {
            decal.imageName = "VinkiGraffiti/vinki/" + fileName;
            byte[] tmpBytes = File.ReadAllBytes(parentDir + Path.DirectorySeparatorChar + fileName + ".png");
            ImageConversion.LoadImage(img, tmpBytes);
        }

        // Get average color of image (to use for graffiti spray/smoke color)
        Plugin.graffitiAvgColors["vinki"].Add(AverageColorFromTexture(img));

        // Resize image to look good in game
        int[] newSize = ResizeAndKeepAspectRatio(img.width, img.height, 150f * 150f);
        img.Resize(newSize[0], newSize[1]);

        decal.handles[0] = new Vector2(0f, img.height);
        decal.handles[1] = new Vector2(img.width, img.height);
        decal.handles[2] = new Vector2(img.width, 0f);

        float halfWidth = img.width / 2f;
        float halfHeight = img.height / 2f;
        Plugin.graffitiOffsets["vinki"].Add(new Vector2(-halfWidth, -halfHeight));
        Plugin.graffitis["vinki"].Add(decal);

        // Flag that we have a new graffiti so that it's the next one we spray
        Plugin.newGraffiti = true;
    }
}

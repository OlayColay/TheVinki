using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.DevInterface;
using SlugBase.SaveData;
using System;
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

    public Enums.GraffitiUnlockID GraffitiUnlock
    {
        get
        {
            if (!this.vinkiToken || this.tokenString == null || this.tokenString.Length < 1)
            {
                return null;
            }
            return new Enums.GraffitiUnlockID(this.tokenString, false);
        }
        set
        {
            if (!this.vinkiToken)
            {
                return;
            }
            if (value == null)
            {
                this.tokenString = string.Empty;
                return;
            }
            this.tokenString = value.value;
        }
    }
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

        if (ext.vinkiToken && ext.GraffitiUnlock != null)
        {
            SlugBase.SaveData.SlugBaseSaveData progData = SaveDataExtension.GetSlugBaseData(self.room.game.rainWorld.progression.miscProgressionData);
            if (progData.TryGet(ext.GraffitiUnlock.ToString(), out bool unlocked) || unlocked)
            {
                self.anythingUnlocked = false;
                return;
            }

            progData.Set(ext.GraffitiUnlock.ToString(), true);
            self.anythingUnlocked = true;
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
}

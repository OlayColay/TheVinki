using Menu;
using SlugBase.DataTypes;

namespace Vinki;

public static class Enums
{
    public static readonly SlugcatStats.Name vinki = new(nameof(vinki), false);
    public static readonly ProcessManager.ProcessID FullscreenVideo = new(nameof(FullscreenVideo), true);
    public static readonly string vinkiStr = nameof(vinki);
    public static readonly MenuScene.SceneID GraffitiMap = new(nameof(GraffitiMap), true);

    public static readonly UnityEngine.Color SparkColor = new UnityEngine.Color32(0xd1, 0xfa, 0xff, 0xff);

    public static class Movement
    {
        public static readonly float CoyoteBoost = 5f;
        public static readonly float GrindVineSpeed = 6.25f;
        public static readonly float GrindXSpeed = 7.5f;
        public static readonly float GrindYSpeed = 7.5f;
        public static readonly float NormalXSpeed = 1.3f;
        public static readonly float NormalYSpeed = 1.3f;
        public static readonly float SuperJump = 1.25f;
    }

    public static class SSOracle
    {
        public static Conversation.ID Vinki_SSConvoFirstMeet = new(nameof(Vinki_SSConvoFirstMeet), true);
        public static Conversation.ID Vinki_SSConvoDrone = new(nameof(Vinki_SSConvoDrone), true);

        public static SSOracleBehavior.Action Vinki_SSActionGeneral = new(nameof(Vinki_SSActionGeneral), true);
        public static SSOracleBehavior.Action Vinki_SSActionTriggered = new(nameof(Vinki_SSActionTriggered), true);
        public static SSOracleBehavior.Action Vinki_SSActionGetOut = new(nameof(Vinki_SSActionGetOut), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Vinki_SSSubBehavGeneral = new(nameof(Vinki_SSSubBehavGeneral), true);
    }

    public static class DMOracle
    {
        public static Conversation.ID Vinki_DMConvoFirstMeet = new(nameof(Vinki_DMConvoFirstMeet), true);
        public static Conversation.ID Vinki_DMConvoSecondMeet = new(nameof(Vinki_DMConvoSecondMeet), true);

        public static SSOracleBehavior.Action Vinki_DMActionGeneral = new(nameof(Vinki_DMActionGeneral), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Vinki_DMSlumberParty = new(nameof(Vinki_DMSlumberParty), true);
    }

    public static class Color
    {
        public static PlayerColor Body;
        public static PlayerColor Eyes;
        public static PlayerColor TailStripes;
        public static PlayerColor Shoes;
        public static PlayerColor RainPods;
        public static PlayerColor Glasses;
    }

    public static class Sound
    {
        public static SoundID Grind1A = new("Vinki_Grind_LOOP", true);
    }

    public static void RegisterValues()
    {
        Color.Body = new PlayerColor("Body");
        Color.Eyes = new PlayerColor("Eyes");
        Color.TailStripes = new PlayerColor("Tail Stripes");
        Color.Shoes = new PlayerColor("Shoes");
        Color.RainPods = new PlayerColor("RainPods");
        Color.Glasses = new PlayerColor("Glasses");

        Sound.Grind1A = new SoundID("Vinki_Grind_LOOP", true);
    }
}
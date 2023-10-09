using SlugBase.DataTypes;

namespace Vinki;

public static class Enums
{
    public static SlugcatStats.Name vinki = new(nameof(vinki), false);
    public static ProcessManager.ProcessID GraffitiQuest = new(nameof(GraffitiQuest), false);
    public static string vinkiStr = nameof(vinki);

    public static class SSOracle
    {
        public static Conversation.ID Vinki_SSConvoFirstMeet = new(nameof(Vinki_SSConvoFirstMeet), true);

        public static SSOracleBehavior.Action Vinki_SSActionGeneral = new(nameof(Vinki_SSActionGeneral), true);
        public static SSOracleBehavior.Action Vinki_SSActionTriggered = new(nameof(Vinki_SSActionTriggered), true);
        public static SSOracleBehavior.Action Vinki_SSActionGetOut = new(nameof(Vinki_SSActionGetOut), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Vinki_SSSubBehavGeneral = new(nameof(Vinki_SSSubBehavGeneral), true);
    }

    public static class DMOracle
    {
        public static Conversation.ID Vinki_DMConvoFirstMeet = new(nameof(Vinki_DMConvoFirstMeet), true);

        public static SSOracleBehavior.Action Vinki_DMActionGeneral = new(nameof(Vinki_DMActionGeneral), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Vinki_DMSubBehavGeneral = new(nameof(Vinki_DMSubBehavGeneral), true);
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
        public static SoundID Grind1A = new SoundID("Vinki_Grind_LOOP", true);
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
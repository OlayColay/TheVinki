using Menu;
using SlugBase.DataTypes;

namespace Vinki;

public static class Enums
{
    public static SlugcatStats.Name vinki = new(nameof(vinki), false);
    public static ProcessManager.ProcessID GraffitiQuest = new(nameof(GraffitiQuest), false);
    public static string vinkiStr = nameof(vinki);
    public static MenuScene.SceneID GraffitiMap = new MenuScene.SceneID(nameof(GraffitiMap), true);

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
        public static SoundID Grind1A = new SoundID("Vinki_Grind_LOOP", true);
    }

    public class GraffitiUnlockID : ExtEnum<GraffitiUnlockID>
    {
        public GraffitiUnlockID(string value, bool register = false) : base(value, register) { }

        public static void Init()
        {
            values = new ExtEnumType();
            values.AddEntry(NoodleSpears.value);
            values.AddEntry(TollSkull.value);
            values.AddEntry(DropwigTheif.value);
            values.AddEntry(PopPopSnails.value);
            values.AddEntry(LilypuckKarma.value);
            values.AddEntry(ScratchedVulture.value);
            values.AddEntry(VultureRemote.value);
            values.AddEntry(GarbageAlarm.value);
            values.AddEntry(CrybabyMouse.value);
            values.AddEntry(MirosJaws.value);
            values.AddEntry(Batnip.value);
            values.AddEntry(JetfishFriend.value);
            values.AddEntry(Jellyfish.value);
            values.AddEntry(Dandelions.value);
            values.AddEntry(Squidburger.value);
            values.AddEntry(SporeBombs.value);
            values.AddEntry(Beehive.value);
            values.AddEntry(OverseerKiss.value);
            values.AddEntry(CityView.value);
            values.AddEntry(NeuronDance.value);
            values.AddEntry(Gooieducky.value);
            values.AddEntry(LeviathanJaws.value);
            values.AddEntry(LilMoon.value);
            values.AddEntry(StolenFace.value);
        }

        public static readonly GraffitiUnlockID NoodleSpears = new("Noodle Spears", true);
        public static readonly GraffitiUnlockID TollSkull = new("Toll Skull", true);
        public static readonly GraffitiUnlockID DropwigTheif = new("Dropwig Thief", true);
        public static readonly GraffitiUnlockID PopPopSnails = new("Pop Pop Snails", true);
        public static readonly GraffitiUnlockID LilypuckKarma = new("Lilypuck Karma", true);
        public static readonly GraffitiUnlockID ScratchedVulture = new("Scratched Vulture", true);
        public static readonly GraffitiUnlockID VultureRemote = new("Vulture Remote", true);
        public static readonly GraffitiUnlockID GarbageAlarm = new("Garbage Alarm", true);
        public static readonly GraffitiUnlockID CrybabyMouse = new("Crybaby Mouse", true);
        public static readonly GraffitiUnlockID MirosJaws = new("Miros Jaws", true);
        public static readonly GraffitiUnlockID Batnip = new("Batnip", true);
        public static readonly GraffitiUnlockID JetfishFriend = new("Jetfish Friend", true);
        public static readonly GraffitiUnlockID Jellyfish = new("Jellyfish", true);
        public static readonly GraffitiUnlockID Dandelions = new("Dandelions", true);
        public static readonly GraffitiUnlockID Squidburger = new("Squidburger", true);
        public static readonly GraffitiUnlockID SporeBombs = new("Spore Bombs", true);
        public static readonly GraffitiUnlockID Beehive = new("Beehive", true);
        public static readonly GraffitiUnlockID OverseerKiss = new("Overseer Kiss", true);
        public static readonly GraffitiUnlockID CityView = new("City View", true);
        public static readonly GraffitiUnlockID NeuronDance = new("Neuron Dance", true);
        public static readonly GraffitiUnlockID Gooieducky = new("Gooieducky", true);
        public static readonly GraffitiUnlockID LeviathanJaws = new("Leviathan Jaws", true);
        public static readonly GraffitiUnlockID LilMoon = new("Lil Moon", true);
        public static readonly GraffitiUnlockID StolenFace = new("Stolen Face", true);
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
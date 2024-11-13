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

        GraffitiUnlockID.Init();
    }

    public class GraffitiUnlockID(string value, bool register = false) : ExtEnum<GraffitiUnlockID>(value, register)
    {
        public static readonly GraffitiUnlockID AcidBath = new("Acid Bath", true);
        public static readonly GraffitiUnlockID AngryPinecone = new("Angry Pinecone", true);
        public static readonly GraffitiUnlockID Batnip = new("Batnip", true);
        public static readonly GraffitiUnlockID CityView = new("City View", true);
        public static readonly GraffitiUnlockID Crybaby = new("Crybaby", true);
        public static readonly GraffitiUnlockID DandelionPeaches = new("Dandelion Peaches", true);
        public static readonly GraffitiUnlockID DropwigThief = new("Dropwig Thief", true);
        public static readonly GraffitiUnlockID Echo = new("Echo", true);
        public static readonly GraffitiUnlockID GarbageAlarm = new("Garbage Alarm", true);
        public static readonly GraffitiUnlockID GummyLizard = new("Gummy Lizard", true);
        public static readonly GraffitiUnlockID HolyGooieduck = new("Holy Gooieduck", true);
        public static readonly GraffitiUnlockID Jellyfish = new("Jellyfish", true);
        public static readonly GraffitiUnlockID JetfishFriend = new("Jetfish Friend", true);
        public static readonly GraffitiUnlockID LeviathanJaws = new("Leviathan Jaws", true);
        public static readonly GraffitiUnlockID LilMoon = new("Lil Moon", true);
        public static readonly GraffitiUnlockID LilypuckKarma = new("Lilypuck Karma", true);
        public static readonly GraffitiUnlockID LivingPaintbrush = new("Living Paintbrush", true);
        public static readonly GraffitiUnlockID LoudPebbles = new("Loud Pebbles", true);
        public static readonly GraffitiUnlockID MirosJaws = new("Miros Jaws", true);
        public static readonly GraffitiUnlockID NeuronDance = new("Neuron Dance", true);
        public static readonly GraffitiUnlockID NoodleSpears = new("Noodle Spears", true);
        public static readonly GraffitiUnlockID OverseerKiss = new("Overseer Kiss", true);
        public static readonly GraffitiUnlockID PopPopSnails = new("Pop Pop Snails", true);
        public static readonly GraffitiUnlockID QuestionableCentipede = new("Questionable Centipede", true);
        public static readonly GraffitiUnlockID SlugcatMoon = new("Slugcat Moon", true);
        public static readonly GraffitiUnlockID SporeBombs = new("Spore Bombs", true);
        public static readonly GraffitiUnlockID Squidburger = new("Squidburger", true);
        public static readonly GraffitiUnlockID StolenFace = new("Stolen Face", true);
        public static readonly GraffitiUnlockID VultureRemote = new("Vulture Remote", true);
        public static readonly GraffitiUnlockID WeaponPile = new("Weapon Pile", true);

        public static void Init()
        {
            values = new ExtEnumType();
            values.AddEntry(AcidBath.value);
            values.AddEntry(AngryPinecone.value);
            values.AddEntry(Batnip.value);
            values.AddEntry(CityView.value);
            values.AddEntry(Crybaby.value);
            values.AddEntry(DandelionPeaches.value);
            values.AddEntry(DropwigThief.value);
            values.AddEntry(Echo.value);
            values.AddEntry(GarbageAlarm.value);
            values.AddEntry(GummyLizard.value);
            values.AddEntry(HolyGooieduck.value);
            values.AddEntry(Jellyfish.value);
            values.AddEntry(JetfishFriend.value);
            values.AddEntry(LeviathanJaws.value);
            values.AddEntry(LilMoon.value);
            values.AddEntry(LivingPaintbrush.value);
            values.AddEntry(LoudPebbles.value);
            values.AddEntry(MirosJaws.value);
            values.AddEntry(NeuronDance.value);
            values.AddEntry(NoodleSpears.value);
            values.AddEntry(OverseerKiss.value);
            values.AddEntry(PopPopSnails.value);
            values.AddEntry(QuestionableCentipede.value);
            values.AddEntry(SlugcatMoon.value);
            values.AddEntry(SporeBombs.value);
            values.AddEntry(Squidburger.value);
            values.AddEntry(StolenFace.value);
            values.AddEntry(VultureRemote.value);
            values.AddEntry(WeaponPile.value);
        }
    }
}
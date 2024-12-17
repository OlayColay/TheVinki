using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MoreSlugcats;
using SlugBase.SaveData;

namespace Vinki;

public static partial class Hooks
{
    private static void ApplyRegionGateHooks()
    {
        new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetGetMethod(), RegionGate_get_MeetRequirement);
    
        On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;
    }
    private static void RemoveRegionGateHooks()
    {
        HookEndpointManager.Remove(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetGetMethod(), RegionGate_get_MeetRequirement);
        
        On.RegionGate.customOEGateRequirements -= RegionGate_customOEGateRequirements;
    }

    public delegate bool orig_MeetRequirement(RegionGate self);
    private static bool RegionGate_get_MeetRequirement(orig_MeetRequirement orig, RegionGate self)
    {
        if (self.room.game.IsStorySession && self.room.game.StoryCharacter == Enums.vinki &&
            self.karmaRequirements[(!self.letThroughDir) ? 1 : 0] == MoreSlugcatsEnums.GateRequirement.RoboLock &&
            self.room.game.GetStorySession.saveState.hasRobo)
        {
            SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
            if (self.room.world.region.name == "UW" || self.room.world.region.name == "LC")
            {
                return (miscWorldSave.TryGet("LC Unlocked", out bool unlocked) && unlocked) || self.unlocked;
            }
            else if (self.room.world.region.name == "MS" || self.room.world.region.name == "DM")
            {
                // Perhaps in the future...
                return self.unlocked;
            }
        }
        
        return orig(self);
    }

    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        return (self.room.game.GetStorySession.saveState.saveStateNumber == Enums.vinki) || orig(self);
    }
}

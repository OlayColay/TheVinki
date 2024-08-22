using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Vinki;

public static partial class Hooks
{
    private static void ApplySlugcatSelectMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.CommunicateWithUpcomingProcess += SlugcatSelectMenu_CommunicateWithUpcomingProcess;

        try
        {
            IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }
        catch (Exception ex)
        {
            Plugin.VLogger.LogError("Could not apply StartGame IL hook\n" + ex.Message);
        }
    }

    private static void SlugcatSelectMenu_CommunicateWithUpcomingProcess(On.Menu.SlugcatSelectMenu.orig_CommunicateWithUpcomingProcess orig, SlugcatSelectMenu self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);

        if (nextProcess.ID == Enums.FullscreenVideo)
        {
            (nextProcess as FullscreenVideo).StartVideo("videos/VinkiIntro.mp4", ProcessManager.ProcessID.Game);
        }
    }

    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel soundLabel = null;

        try
        {
            c.GotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.White)),
                x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Inequality"))
            );

            new ILCursor(c).GotoNext(x => x.MatchBr(out soundLabel));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter) =>
            {
                if (storyGameCharacter == Enums.vinki && !VinkiConfig.SkipIntro.Value)
                {
                    self.manager.RequestMainProcessSwitch(Enums.FullscreenVideo);
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue_S, soundLabel);
        }
        catch (Exception e) 
        { 
            Plugin.VLogger.LogError("Could not complete StartGame IL Hook\n" + e.Message); 
        }
    }
}
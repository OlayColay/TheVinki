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
        IL.Menu.SlugcatSelectMenu.StartGame -= SlugcatSelectMenu_StartGame;
        IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
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
        Plugin.VLogger.LogInfo("Applying slugcat select hook");
        var c = new ILCursor(il);
        ILLabel soundLabel = null;

        c.GotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<MainLoopProcess>("manager"),
            x => x.MatchLdsfld<ProcessManager.ProcessID>("Game"),
            x => x.MatchCallvirt<ProcessManager>("RequestMainProcessSwitch")
        );
        Plugin.VLogger.LogInfo("Moved cursor to before Game process switch");
        new ILCursor(c).GotoNext(x => x.MatchBr(out soundLabel));
        Plugin.VLogger.LogInfo("Moved new cursor to " + soundLabel.Target.OpCode.Name.ToString());
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate((SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter) =>
        {
            Plugin.VLogger.LogInfo("In delegate; " + storyGameCharacter.value);
            if (storyGameCharacter == Enums.vinki)
            {
                Plugin.VLogger.LogInfo("We are Vinki");
                self.manager.RequestMainProcessSwitch(Enums.FullscreenVideo);
                return true;
            }
            return false;
        });
        c.Emit(OpCodes.Brtrue_S, soundLabel);

        Plugin.VLogger.LogDebug(il.ToString());
    }
}
using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;

namespace Vinki;

public static partial class Hooks
{
    private static void ApplyHudHooks()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }

    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        AbstractCreature vinki = cam.room.game.Players.FirstOrDefault((AbstractCreature c) => (c.realizedCreature as Player).IsVinki());
        if (vinki != null )
        {
            self.AddPart(new ComboDisplay(self, (vinki.realizedCreature as Player).Vinki()));
        }
    }
}
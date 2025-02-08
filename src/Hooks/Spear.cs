using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplySpearHooks()
    {
        On.Spear.HitSomething += Spear_HitSomething;
    }

    private static void RemoveSpearHooks()
    {
        On.Spear.HitSomething -= Spear_HitSomething;
    }

    private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        if (self.thrownBy is Player p && p.IsVinki(out VinkiPlayerData vinki) && result.obj is Creature c && !c.dead)
        {
            vinki.AddTrickToCombo("New Mobile Rail", 500);
        }

        return orig(self, result, eu);
    }
}

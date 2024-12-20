using RWCustom;
using System;
using UnityEngine;

namespace Vinki;

public static partial class Hooks
{
	private static void ApplyClimbableVineSystemHooks()
	{
        On.ClimbableVinesSystem.ConnectChunkToVine += ClimbableVinesSystem_ConnectChunkToVine;
	}
	private static void RemoveClimbableVineSystemHooks()
	{
        On.ClimbableVinesSystem.ConnectChunkToVine -= ClimbableVinesSystem_ConnectChunkToVine;
	}

    private static void ClimbableVinesSystem_ConnectChunkToVine(On.ClimbableVinesSystem.orig_ConnectChunkToVine orig, ClimbableVinesSystem self, BodyChunk chunk, ClimbableVinesSystem.VinePosition vPos, float conRad)
    {
        if (chunk.owner is not Player || (chunk.owner as Player).SlugCatClass != Enums.vinki || chunk.index != 1)
        {
            orig(self, chunk, vPos, conRad);
            return;
        }

        float feetOffset = -10f;
        Vector2 feetPos = chunk.pos + new Vector2(0f, feetOffset);

        int num = self.PrevSegAtFloat(vPos.vine, vPos.floatPos);
        int num2 = Custom.IntClamp(num + 1, 0, self.vines[vPos.vine].TotalPositions() - 1);
        float t = Mathf.InverseLerp(self.FloatAtSegment(vPos.vine, num), self.FloatAtSegment(vPos.vine, num2), vPos.floatPos);
        Vector2 vector = Vector2.Lerp(self.vines[vPos.vine].Pos(num), self.vines[vPos.vine].Pos(num2), t);
        float num3 = chunk.mass / (chunk.mass + Mathf.Lerp(self.vines[vPos.vine].Mass(num), self.vines[vPos.vine].Mass(num2), t));
        float num4 = Vector2.Distance(feetPos, vector);
        Vector2 a = Custom.DirVec(feetPos, vector);
        if (num4 > conRad)
        {
            chunk.pos += a * (num4 - conRad) * (1f - num3);
            chunk.vel += a * (num4 - conRad) * (1f - num3);
            self.vines[vPos.vine].Push(num, -a * (num4 - conRad) * num3);
            self.vines[vPos.vine].Push(num2, -a * (num4 - conRad) * num3);
        }
    }
}

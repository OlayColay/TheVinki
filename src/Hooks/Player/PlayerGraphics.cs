using UnityEngine;
using static Vinki.Plugin;

namespace Vinki
{
    public static partial class Hooks
    {

        // Add hooks
        private static void ApplyPlayerGraphicsHooks()
        {
            On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (self.player.SlugCatClass != Enums.TheVinki)
            {
                return;
            }

            if (craftCounter > 0)
            {
                foreach (SlugcatHand hand in self.hands)
                {
                    hand.pos = Vector2.Lerp(hand.pos, self.drawPositions[0, 0], (float)craftCounter / 25f);
                }

                float num10 = Mathf.InverseLerp(0f, 110f, (float)craftCounter);
                float num11 = (float)craftCounter / Mathf.Lerp(30f, 15f, num10);
                if (self.player.standing)
                {
                    self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 2f;
                    self.drawPositions[1, 0].y += -Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 3f;
                }
                else
                {
                    self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 3f;
                    self.drawPositions[0, 0].x += Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 1f;
                    self.drawPositions[1, 0].y += Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 2f;
                    self.drawPositions[1, 0].x += -Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 3f;
                }
            }
        }
    }
}
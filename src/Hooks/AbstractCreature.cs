using System;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyAbstractCreatureHooks()
        {
            On.AbstractCreature.Realize += AbstractCreature_Realize;
        }

        private static void RemoveAbstractCreatureHooks()
        {
            On.AbstractCreature.Realize -= AbstractCreature_Realize;
        }

        private static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            orig(self);

            if (self.world.game == null || self.world.game.StoryCharacter != Enums.vinki)
            {
                return;
            }

            if (self.realizedCreature is DaddyLongLegs)
            {
                self.realizedCreature.Destroy();
            }
        }
    }
}
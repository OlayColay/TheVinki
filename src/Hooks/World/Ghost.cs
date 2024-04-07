using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Vinki.Plugin;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyGhostHooks()
        {
            On.GhostConversation.AddEvents += GhostConversation_AddEvents;
        }

        private static void RemoveGhostHooks()
        {
            On.GhostConversation.AddEvents -= GhostConversation_AddEvents;
        }

        private static void GhostConversation_AddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
        {
            if (self.ghost.room.game.StoryCharacter == Enums.vinki)
            {
                UnlockGraffitiMidgame("ArtisticDragon1292 - Echo");
            }
            orig(self);
        }
    }
}
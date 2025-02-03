using On.Expedition;
﻿using Expedition;

namespace Vinki;
public static partial class Hooks
{
	public static void ApplyExpeditionHooks()
	{
        NeuronDeliveryChallenge.ValidForThisSlugcat += NeuronDeliveryChallenge_ValidForThisSlugcat;
        PearlDeliveryChallenge.Update += PearlDeliveryChallenge_Update;
	CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat;
    }
    public static void RemoveExpeditionHooks()
    {
        NeuronDeliveryChallenge.ValidForThisSlugcat -= NeuronDeliveryChallenge_ValidForThisSlugcat;
        PearlDeliveryChallenge.Update -= PearlDeliveryChallenge_Update;
	CharacterSelectPage.UpdateSelectedSlugcat -= CharacterSelectPage_UpdateSelectedSlugcat;
    }

    private static bool NeuronDeliveryChallenge_ValidForThisSlugcat(NeuronDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.NeuronDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        if (slugcat == Enums.vinki)
        {
            return false;
        }
        return orig(self, slugcat);
    }

    private static void PearlDeliveryChallenge_Update(PearlDeliveryChallenge.orig_Update orig, Expedition.PearlDeliveryChallenge self)
    {
        orig(self);

        for (int i = 0; i < self.game.Players.Count; i++)
        {
            if (self.game.Players[i] != null && self.game.Players[i].realizedCreature != null && self.game.Players[i].realizedCreature.room != null && (self.game.Players[i].realizedCreature.room.abstractRoom.name == ((self.iterator == 0) ? "SL_AI" : "SS_AI") || (ModManager.MSC && Expedition.ExpeditionData.slugcatPlayer == Enums.vinki && self.game.Players[i].realizedCreature.room.abstractRoom.name == "DM_AI")))
            {
                for (int j = 0; j < self.game.Players[i].realizedCreature.room.updateList.Count; j++)
                {
                    if (self.game.Players[i].realizedCreature.room.updateList[j] is DataPearl && Expedition.ChallengeTools.ValidRegionPearl(self.region, (self.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType) && ((self.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).firstChunk.pos.x > ((self.iterator == 0) ? 1400f : 0f) || (ModManager.MSC && Expedition.ExpeditionData.slugcatPlayer == Enums.vinki)))
                    {
                        self.CompleteChallenge();
                    }
                }
            }
        }
    }

    private static void CharacterSelectPage_UpdateSelectedSlugcat(On.Menu.CharacterSelectPage.orig_UpdateSelectedSlugcat orig, CharacterSelectPage self, int num)
    {
        if (num < 0 || num >= ExpeditionGame.playableCharacters.Count) num = 1;
        orig(self, num);

    	if (ExpeditionGame.playableCharacters[num] == Enums.vinki)
        {
            self.slugcatScene = new(nameof(MoreSlugcats.MoreSlugcatsEnums.MenuSceneID.Landscape_VS));
        }
    }
}

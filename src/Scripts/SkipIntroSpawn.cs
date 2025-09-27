﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vinki;
public class SkipIntroSpawn : UpdatableAndDeletable
{
    public SkipIntroSpawn(Room room)
    {
        this.firstStart = true;
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (this.room.game.StoryCharacter != Enums.vinki)
        {
            return;
        }
        AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
        if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
        {
            if (this.firstStart)
            {
                Vector2 vector = new(980f, 4050f);
                firstAlivePlayer.realizedCreature.bodyChunks[0].HardSetPosition(vector + new Vector2(0f, 0f));
                firstAlivePlayer.realizedCreature.bodyChunks[1].HardSetPosition(vector + new Vector2(0f, 10f));
                (firstAlivePlayer.realizedCreature as Player).standing = true;
                (firstAlivePlayer.realizedCreature as Player).playerState.foodInStomach = (firstAlivePlayer.realizedCreature as Player).MaxFoodInStomach;

                List<Player> players = this.room.PlayersInRoom;
                for (int i = 1; i < players.Count; i++)
                {
                    players[i].bodyChunks[0].HardSetPosition(vector + new Vector2(75f, 0f));
                    players[i].bodyChunks[1].HardSetPosition(vector + new Vector2(75f, 10f));
                    players[i].standing = true;
                }

                room.game.GetStorySession.saveState.cycleNumber = 1;
                room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = 1;
                this.firstStart = false;
            }
        }
    }

    private bool firstStart;
}

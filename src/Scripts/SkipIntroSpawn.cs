using System;
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
        if (this.room.game.GetStorySession.saveStateNumber != Enums.vinki)
        {
            return;
        }
        AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
        if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
        {
            if (this.firstStart)
            {
                Vector2 vector = new(980f, 4100f);
                firstAlivePlayer.realizedCreature.bodyChunks[0].HardSetPosition(vector + new Vector2(9f, 0f));
                firstAlivePlayer.realizedCreature.bodyChunks[1].HardSetPosition(vector + new Vector2(-5f, 0f));
                this.firstStart = false;
            }
        }
    }

    private bool firstStart;
}

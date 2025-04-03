using System;
using MoreSlugcats;
using UnityEngine;

namespace Vinki
{
    public class CutsceneVinkiRobo : UpdatableAndDeletable
    {
        public CutsceneVinkiRobo(Room room)
        {
            Plugin.VLogger.LogInfo("VINKI ROBO CUTSCENE START!");
            this.room = room;
            this.phase = Phase.Init;
            this.bot = new AncientBot(roboPos, new Color(0.945f, 0.3765f, 0f), null, false);
            room.AddObject(this.bot);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.UpdateBot();
            if (this.phase == Phase.End)
            {
                Plugin.VLogger.LogInfo("VINKI ROBO CUTSCENE END!");
                this.Destroy();
            }
        }

        public void UpdateBot()
        {
            if (this.Player == null)
            {
                return;
            }
            if (this.phase == Phase.Init)
            {
                if (Vector2.Distance(this.Player.mainBodyChunk.pos, roboPos) <= activateRadius)
                {
                    this.bot.tiedToObject = this.Player;
                    this.phase = Phase.ActivateRobo;
                }
            }
            else if (this.phase == Phase.ActivateRobo)
            {
                if (this.bot.myAnimation == AncientBot.Animation.IdleOffline)
                {
                    this.bot.myAnimation = AncientBot.Animation.TurnOn;
                }
                if (this.bot.myMovement != AncientBot.FollowMode.Offline)
                {
                    this.phase = Phase.End;
                    if (this.room.game.IsStorySession)
                    {
                        (this.room.game.session as StoryGameSession).saveState.hasRobo = true;
                    }
                    this.Player.myRobot = this.bot;
                }
            }
        }

        public Player Player
        {
            get
            {
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                {
                    return firstAlivePlayer.realizedCreature as Player;
                }
                return null;
            }
        }

        public AncientBot bot;
        public bool initController;
        public Phase phase;
        public Vector2 roboPos = new(520f, 640f);
        public float activateRadius = 100f;
        public class Phase(string value, bool register = false) : ExtEnum<Phase>(value, register)
        {
            public static readonly Phase Init = new("Init", true);
            public static readonly Phase ActivateRobo = new("ActivateRobo", true);
            public static readonly Phase End = new("End", true);
        }
    }
}
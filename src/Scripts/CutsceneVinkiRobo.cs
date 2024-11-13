using MoreSlugcats;
using UnityEngine;

namespace Vinki
{
    public class CutsceneVinkiRobo : UpdatableAndDeletable
    {
        public CutsceneVinkiRobo(Room room)
        {
            //Plugin.VLogger.LogInfo("VINKI ROBO CUTSCENE START!");
            this.room = room;
            phase = Phase.Init;
            bot = new AncientBot(roboPos, new Color(0.945f, 0.3765f, 0f), null, false);
            room.AddObject(bot);

            // Dead scav (lol)
            AbstractCreature abstractCreature = new(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite), null, new WorldCoordinate(room.abstractRoom.index, scavX, scavY, -1), room.game.GetNewID());
            abstractCreature.state.Die();
            room.abstractRoom.AddEntity(abstractCreature);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            UpdateBot();
            if (phase == Phase.End)
            {
                //Plugin.VLogger.LogInfo("VINKI ROBO CUTSCENE END!");
                Destroy();
            }
        }

        public void UpdateBot()
        {
            if (Player == null)
            {
                return;
            }
            if (phase == Phase.Init)
            {
                if (Vector2.Distance(Player.mainBodyChunk.pos, roboPos) <= activateRadius)
                {
                    bot.tiedToObject = Player;
                    phase = Phase.ActivateRobo;
                }
            }
            else if (phase == Phase.ActivateRobo)
            {
                if (bot.myAnimation == AncientBot.Animation.IdleOffline)
                {
                    bot.myAnimation = AncientBot.Animation.TurnOn;
                }
                if (bot.myMovement != AncientBot.FollowMode.Offline)
                {
                    phase = Phase.End;
                    if (room.game.IsStorySession)
                    {
                        (room.game.session as StoryGameSession).saveState.hasRobo = true;
                    }
                    Player.myRobot = bot;
                }
            }
        }

        public Player Player
        {
            get
            {
                AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
                if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                {
                    return firstAlivePlayer.realizedCreature as Player;
                }
                return null;
            }
        }

        public AncientBot bot;
        public bool initController;
        public Phase phase;
        public Vector2 roboPos = new(570f, 960f);
        public int scavX = 30, scavY = 47;
        public float activateRadius = 100f;
        public class Phase(string value, bool register = false) : ExtEnum<Phase>(value, register)
        {
            public static readonly Phase Init = new("Init", true);
            public static readonly Phase ActivateRobo = new("ActivateRobo", true);
            public static readonly Phase End = new("End", true);
        }
    }
}
using System.Linq;
using SprayCans;

namespace Vinki
{
    // Token: 0x020002E9 RID: 745
    public class CutsceneVinkiIntro : UpdatableAndDeletable
	{
		// Token: 0x06002094 RID: 8340 RVA: 0x0028D648 File Offset: 0x0028B848
		public CutsceneVinkiIntro(Room room)
		{
			//Plugin.VLogger.LogInfo("VINKI CUTSCENE START!");
			this.room = room;
			phase = Phase.Init;
		}

        public override void Destroy()
        {
			Plr.controller = startController = null;
            base.Destroy();
        }

        // Token: 0x06002095 RID: 8341 RVA: 0x0028D744 File Offset: 0x0028B944
        public override void Update(bool eu)
		{
			base.Update(eu);

			if (phase == Phase.Init)
			{
				if (!room.BeingViewed)
				{
					return;
				}

				if (Plr != null && !foodMeterInit)
				{
                    Plr.playerState.foodInStomach = 4;
                    if (room.game.cameras[0].hud == null)
					{
						room.game.cameras[0].FireUpSinglePlayerHUD(Plr);
					}
					foodMeterInit = true;
					room.game.cameras[0].hud.foodMeter.NewShowCount(Plr.FoodInStomach);
					room.game.cameras[0].hud.foodMeter.visibleCounter = 4;
					room.game.cameras[0].hud.foodMeter.fade = 0f;
					room.game.cameras[0].hud.foodMeter.lastFade = 0f;
					room.game.cameras[0].followAbstractCreature = Plr.abstractCreature;
				}
				if (Plr != null && Plr.room != null && !playerPosCorrect)
				{
					for (int j = 0; j < 2; j++)
					{
						Plr.bodyChunks[j].HardSetPosition(room.MiddleOfTile(28, 34));
					}
					playerPosCorrect = true;
					Plr.graphicsModule?.Reset();
					startController = new StartController(this);
					Plr.controller = startController;
					Plr.standing = true;

                    // Spawn SprayCan in hand
                    var tilePosition = Plr.room.GetTilePosition(Plr.mainBodyChunk.pos);
                    var pos = new WorldCoordinate(Plr.room.abstractRoom.index, tilePosition.x, tilePosition.y - 1, 0);
                    var abstr = new SprayCanAbstract(Plr.room.world, pos, Plr.room.game.GetNewID(), 2);
                    abstr.Realize();
                    Plr.room.abstractRoom.AddEntity(abstr);
                    Plr.room.AddObject(abstr.realizedObject);
                    Plr.SlugcatGrab(abstr.realizedObject, 0);
                }
				if (playerPosCorrect && foodMeterInit)
				{
					phase = Phase.PlayerRun;
					return;
				}
			}
			else
			{
				if (phase == Phase.PlayerRun)
				{
					cutsceneTimer++;
					return;
				}
				if (phase == Phase.Wait && Plr != null)
				{
					if (Plr.room != null)
					{
						if (Plr.room == room)
						{
							cutsceneTimer++;
						}
						else
						{
							phase = Phase.End;
						}
					}
				}
				if (phase == Phase.End)
				{
					//Plugin.VLogger.LogInfo("VINKI CUTSCENE END!");
					if (Plr != null)
					{
						Plr.controller = null;
                    }
					Plugin.introPlayed = true;
					Destroy();
				}
			}
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x0028DB88 File Offset: 0x0028BD88
		public Player.InputPackage GetInput()
		{
			if (Plr == null)
			{
				return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);
			}
			int x = 0;
			int y = 0;
			bool jmp = false;
			bool pckp = false;
			bool thrw = false;
			if (phase == Phase.PlayerRun)
			{
				int[] array2 =
                [
                    250,
					95,
					4,
					70,
					4,
					15,
					120
				];
				if (cutsceneTimer < 30)
				{
					pckp = true;
				}
				int num2 = array2[0];
				if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[1])
				{
					y = -1;
					if (cutsceneTimer >= num2 + 30)
					{
						x = -1;
                    }
                    if (cutsceneTimer >= num2 + 60)
                    {
                        y = 1;
                    }
                }
				num2 += array2[1];
				if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[2])
				{
					x = 1;
				}
				num2 += array2[2];
				if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[3])
				{
					x = 1;
					jmp = true;
				}
				num2 += array2[3];
				if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[4])
				{
					x = -1;
				}
				num2 += array2[4];
				if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[5])
				{
					x = -1;
					jmp = true;
				}
				num2 += array2[5];
				if (cutsceneTimer == num2)
				{
					x = -1;
                    if (Plr.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan).grabbed is SprayCan grasp)
                    {
                        grasp.TryUse();
                    }
                    // Trigger the cutscene
                    Hooks.TriggerSSOracleScene();
                    _ = Hooks.SprayGraffiti(Plr, 20, 0, 0.5f);
                }
				num2 += array2[6];
				if (cutsceneTimer >= num2)
				{
					//thrw = true;
					cutsceneTimer = 0;
					phase = Phase.Wait;
				}
			}
			return new Player.InputPackage(false, Options.ControlSetup.Preset.None, x, y, jmp, thrw, pckp, false, false);
		}

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x06002097 RID: 8343 RVA: 0x0028DF5C File Offset: 0x0028C15C
		public Player Plr
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

		// Token: 0x04001E82 RID: 7810
		public bool foodMeterInit;

		// Token: 0x04001E83 RID: 7811
		public bool playerPosCorrect;

		// Token: 0x04001E84 RID: 7812
		public Phase phase;

		// Token: 0x04001E85 RID: 7813
		public StartController startController;

		// Token: 0x04001E86 RID: 7814
		public int cutsceneTimer;

		// Token: 0x0200078B RID: 1931
		public class Phase(string value, bool register = false) : ExtEnum<Phase>(value, register)
		{
            // Token: 0x04003ED4 RID: 16084
            public static readonly Phase Init = new("Init", true);

			// Token: 0x04003ED5 RID: 16085
			public static readonly Phase PlayerRun = new("PlayerRun", true);

			// Token: 0x04003ED7 RID: 16087
			public static readonly Phase Wait = new("Wait", true);

			// Token: 0x04003ED8 RID: 16088
			public static readonly Phase End = new("End", true);
		}

		// Token: 0x0200078C RID: 1932
		public class StartController(CutsceneVinkiIntro owner) : Player.PlayerController
		{

            // Token: 0x06003D8E RID: 15758 RVA: 0x0045D0F0 File Offset: 0x0045B2F0
            public override Player.InputPackage GetInput()
			{
				return owner.GetInput();
			}

			// Token: 0x04003ED9 RID: 16089
			private readonly CutsceneVinkiIntro owner = owner;
		}
	}
}

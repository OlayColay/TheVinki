using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Vinki
{
	// Token: 0x020002E9 RID: 745
	public class CutsceneVinkiIntro : UpdatableAndDeletable
	{
		// Token: 0x06002094 RID: 8340 RVA: 0x0028D648 File Offset: 0x0028B848
		public CutsceneVinkiIntro(Room room)
		{
			Debug.Log("Vinki CUTSCENE START!");
			this.room = room;
			this.phase = CutsceneVinkiIntro.Phase.Init;
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x0028D744 File Offset: 0x0028B944
		public override void Update(bool eu)
		{
			base.Update(eu);
			if (this.player != null && this.player.myRobot != null)
			{
				if (this.room.world.rainCycle.timer < 400)
				{
					for (int i = 0; i < 2; i++)
					{
						this.player.bodyChunks[i].HardSetPosition(this.room.MiddleOfTile(19, 6));
					}
					this.room.game.cameras[0].followAbstractCreature = this.player.abstractCreature;
					this.player.standing = true;
					this.room.game.GetStorySession.saveState.deathPersistentSaveData.deathTime = 30;
					this.player.AddFood(4);
					this.room.world.rainCycle.timer = 400;
				}
				this.Destroy();
				return;
			}
			if (this.phase == CutsceneVinkiIntro.Phase.Init)
			{
				if (this.player != null && !this.foodMeterInit)
				{
					if (this.room.game.cameras[0].hud == null)
					{
						this.room.game.cameras[0].FireUpSinglePlayerHUD(this.player);
					}
					this.foodMeterInit = true;
					this.room.game.cameras[0].hud.foodMeter.NewShowCount(this.player.FoodInStomach);
					this.room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
					this.room.game.cameras[0].hud.foodMeter.fade = 0f;
					this.room.game.cameras[0].hud.foodMeter.lastFade = 0f;
					this.room.game.cameras[0].followAbstractCreature = this.player.abstractCreature;
				}
				if (this.player != null && !this.playerPosCorrect)
				{
					for (int j = 0; j < 2; j++)
					{
						this.player.bodyChunks[j].HardSetPosition(this.room.MiddleOfTile(47, 29));
					}
					this.playerPosCorrect = true;
					if (this.player.graphicsModule != null)
					{
						this.player.graphicsModule.Reset();
					}
					this.startController = new CutsceneVinkiIntro.StartController(this);
					this.player.controller = this.startController;
					this.player.standing = true;
				}
				if (this.playerPosCorrect && this.foodMeterInit)
				{
					this.phase = CutsceneVinkiIntro.Phase.EatScavenger;
					return;
				}
			}
			else
			{
				if (this.phase == CutsceneVinkiIntro.Phase.EatScavenger || this.phase == CutsceneVinkiIntro.Phase.PlayerRun)
				{
					this.cutsceneTimer++;
					return;
				}
				if (this.phase == CutsceneVinkiIntro.Phase.EnsureNextRoom && this.player != null)
				{
					if (this.player.room != null && this.player.room == this.room)
					{
						this.cutsceneTimer++;
						if (this.cutsceneTimer == 1 || this.cutsceneTimer % 120 == 0)
						{
							for (int k = 0; k < 2; k++)
							{
								this.player.bodyChunks[k].vel = Custom.DegToVec(Random.value * 360f) * 12f;
								this.player.bodyChunks[k].pos = new Vector2(988f, 452f);
								this.player.bodyChunks[k].lastPos = new Vector2(988f, 452f);
							}
						}
					}
					else if (this.player.room != null && this.player.room != this.room)
					{
						this.phase = CutsceneVinkiIntro.Phase.End;
					}
				}
				if (this.phase == CutsceneVinkiIntro.Phase.End)
				{
					Debug.Log("ARTIFICER CUTSCENE END!");
					if (this.player != null)
					{
						this.player.controller = null;
					}
					this.Destroy();
				}
			}
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x0028DB88 File Offset: 0x0028BD88
		public Player.InputPackage GetInput()
		{
			if (this.player == null)
			{
				return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);
			}
			int x = 0;
			int y = 0;
			bool jmp = false;
			bool pckp = false;
			bool thrw = false;
			if (this.phase == CutsceneVinkiIntro.Phase.EatScavenger)
			{
				int[] array = new int[]
				{
					160,
					40,
					30,
					200,
					10,
					5
				};
				if (this.cutsceneTimer == 1)
				{
					x = -1;
				}
				int num = array[0];
				if (this.cutsceneTimer >= num && this.cutsceneTimer < num + array[1])
				{
					y = -1;
				}
				num += array[1];
				if (this.cutsceneTimer >= num && this.cutsceneTimer < num + array[2])
				{
					x = -1;
				}
				num += array[2];
				if (this.cutsceneTimer >= num && this.cutsceneTimer < num + array[3])
				{
					pckp = true;
				}
				num += array[3];
				if (this.cutsceneTimer >= num && this.cutsceneTimer < num + array[4])
				{
					thrw = true;
					if (this.player.mainBodyChunk.pos.x >= 910f)
					{
						y = 1;
					}
				}
				num += array[4];
				if (this.cutsceneTimer >= num && (this.cutsceneTimer < num + array[5] || this.player.mainBodyChunk.pos.x < 910f))
				{
					x = 1;
				}
				num += array[5];
				if (this.cutsceneTimer >= num + 30)
				{
					y = 1;
					this.cutsceneTimer = 0;
					this.phase = CutsceneVinkiIntro.Phase.PlayerRun;
				}
			}
			else if (this.phase == CutsceneVinkiIntro.Phase.PlayerRun)
			{
				int[] array2 = new int[]
				{
					7,
					55,
					40,
					7,
					70,
					40,
					40,
					10,
					60
				};
				int num2 = 0;
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[0])
				{
					jmp = true;
				}
				num2 += array2[0];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[1])
				{
					if (this.cutsceneTimer < num2 + 2)
					{
						pckp = true;
					}
					else if (this.cutsceneTimer < num2 + 20)
					{
						x = -1;
						pckp = true;
						jmp = true;
					}
					else if (this.cutsceneTimer < num2 + array2[1] - 10)
					{
						x = -1;
						y = 1;
					}
					else
					{
						y = 1;
					}
				}
				num2 += array2[1];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[2] && this.cutsceneTimer >= num2 + 5)
				{
					y = 1;
				}
				num2 += array2[2];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[3])
				{
					jmp = true;
				}
				num2 += array2[3];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[4])
				{
					if (this.cutsceneTimer < num2 + 2)
					{
						pckp = true;
					}
					else if (this.cutsceneTimer < num2 + 5)
					{
						x = 1;
						pckp = true;
						jmp = true;
					}
					else
					{
						x = 1;
					}
					if (this.player.mainBodyChunk.pos.x > 715f)
					{
						y = 1;
					}
				}
				num2 += array2[4];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[5] && this.cutsceneTimer >= num2 + 5)
				{
					y = 1;
				}
				num2 += array2[5];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[6])
				{
					jmp = true;
					x = 1;
				}
				num2 += array2[6];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[7])
				{
					x = 1;
				}
				num2 += array2[7];
				if (this.cutsceneTimer >= num2 && this.cutsceneTimer < num2 + array2[8])
				{
					jmp = true;
					x = 1;
				}
				num2 += array2[8];
				if (this.cutsceneTimer >= num2)
				{
					this.cutsceneTimer = 0;
					this.phase = CutsceneVinkiIntro.Phase.EnsureNextRoom;
				}
			}
			else if (this.phase == CutsceneVinkiIntro.Phase.EnsureNextRoom)
			{
				x = 1;
			}
			return new Player.InputPackage(false, Options.ControlSetup.Preset.None, x, y, jmp, thrw, pckp, false, false);
		}

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x06002097 RID: 8343 RVA: 0x0028DF5C File Offset: 0x0028C15C
		public Player player
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

		// Token: 0x04001E82 RID: 7810
		public bool foodMeterInit;

		// Token: 0x04001E83 RID: 7811
		public bool playerPosCorrect;

		// Token: 0x04001E84 RID: 7812
		public CutsceneVinkiIntro.Phase phase;

		// Token: 0x04001E85 RID: 7813
		public CutsceneVinkiIntro.StartController startController;

		// Token: 0x04001E86 RID: 7814
		public int cutsceneTimer;

		// Token: 0x0200078B RID: 1931
		public class Phase : ExtEnum<CutsceneVinkiIntro.Phase>
		{
			// Token: 0x06003D8B RID: 15755 RVA: 0x0045D077 File Offset: 0x0045B277
			public Phase(string value, bool register = false) : base(value, register)
			{
			}

			// Token: 0x04003ED4 RID: 16084
			public static readonly CutsceneVinkiIntro.Phase Init = new CutsceneVinkiIntro.Phase("Init", true);

			// Token: 0x04003ED5 RID: 16085
			public static readonly CutsceneVinkiIntro.Phase PlayerRun = new CutsceneVinkiIntro.Phase("PlayerRun", true);

			// Token: 0x04003ED6 RID: 16086
			public static readonly CutsceneVinkiIntro.Phase EatScavenger = new CutsceneVinkiIntro.Phase("EatScavenger", true);

			// Token: 0x04003ED7 RID: 16087
			public static readonly CutsceneVinkiIntro.Phase EnsureNextRoom = new CutsceneVinkiIntro.Phase("EnsureNextRoom", true);

			// Token: 0x04003ED8 RID: 16088
			public static readonly CutsceneVinkiIntro.Phase End = new CutsceneVinkiIntro.Phase("End", true);
		}

		// Token: 0x0200078C RID: 1932
		public class StartController : Player.PlayerController
		{
			// Token: 0x06003D8D RID: 15757 RVA: 0x0045D0E1 File Offset: 0x0045B2E1
			public StartController(CutsceneVinkiIntro owner)
			{
				this.owner = owner;
			}

			// Token: 0x06003D8E RID: 15758 RVA: 0x0045D0F0 File Offset: 0x0045B2F0
			public override Player.InputPackage GetInput()
			{
				return this.owner.GetInput();
			}

			// Token: 0x04003ED9 RID: 16089
			private CutsceneVinkiIntro owner;
		}
	}
}

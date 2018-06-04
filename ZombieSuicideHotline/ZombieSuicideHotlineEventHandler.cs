using Smod2;
using Smod2.API;
using Smod2.Events;

namespace ZombieSuicideHotline.EventHandlers
{
	class RoundStartHandler : IEventRoundStart
	{
		private ZombieSuicideHotlinePlugin plugin;

		public RoundStartHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnRoundStart(Server server)
		{
			this.plugin.duringRound = true;
			this.plugin.scp049Kills = new System.Collections.Generic.HashSet<string>();
			this.plugin.zombieDisconnects = new System.Collections.Generic.HashSet<string>();
			foreach (TeamClass teamClass in server.GetClasses())
			{
				this.plugin.ClassList.Add(teamClass.ClassType, teamClass);
			}
			plugin.Info("ClassList SIZE " + this.plugin.ClassList.Count);
		}
	}
	class RoundEndHandler : IEventRoundEnd
	{
		private ZombieSuicideHotlinePlugin plugin;

		public RoundEndHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnRoundEnd(Server server, Round round)
		{
			this.plugin.duringRound = false;
			foreach (TeamClass teamClass in server.GetClasses())
			{
				this.plugin.ClassList.Add(teamClass.ClassType, teamClass);
			}
			plugin.Info("ClassList SIZE " + this.plugin.ClassList.Count);
		}
	}

	class PlayerJoinHandler : IEventPlayerJoin
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerJoinHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerJoin(Player player)
		{
			if (this.plugin.duringRound && this.plugin.zombieDisconnects.Contains(player.SteamId))
			{
				player.ChangeClass(Classes.SCP_049_2, true, true);
				this.plugin.zombieDisconnects.Add(player.SteamId);
			}
		}
	}

	class PlayerLeaveHandler : IEventPlayerLeave
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerLeaveHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerLeave(Player player)
		{
			if (this.plugin.duringRound && this.plugin.scp049Kills.Contains(player.SteamId))
			{
				this.plugin.scp049Kills.Remove(player.SteamId);
				this.plugin.zombieDisconnects.Add(player.SteamId);
			}
		}
	}

	class SetClassHandler : IEventSetClass
	{
		private ZombieSuicideHotlinePlugin plugin;

		public SetClassHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnSetClass(Player player, TeamClass teamclass, out TeamClass teamclassOutput)
		{
			if (this.plugin.duringRound && this.plugin.scp049Kills.Contains(player.SteamId))
			{
				plugin.Info("Remove player from scp049Kills");
				this.plugin.scp049Kills.Remove(player.SteamId);
				teamclassOutput = teamclass;
			}
			else
			{
				plugin.Info("NOT RESPAWNING ZOMBIE");
				teamclassOutput = teamclass;
			}
		}
	}

	class PlayerDieHandler : IEventPlayerDie
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerDie(Player player, Player killer, out bool spawnRagdoll)
		{
			if (this.plugin.duringRound && killer.Class.ClassType == Classes.SCP_049)
			{
				this.plugin.scp049Kills.Add(player.SteamId);
				spawnRagdoll = true;
			}
			else if (player.Class.ClassType == Classes.SCP_106)
			{
				spawnRagdoll = false;
			}
			else
			{
				spawnRagdoll = true;
			}
		}
	}

	class PlayerHurtHandler : IEventPlayerHurt
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerHurtHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerHurt(Player player, Player attacker, float damage, out float damageOutput, DamageType type, out DamageType typeOutput)
		{
			switch (player.Class.ClassType)
			{
				case Classes.SCP_049_2:
					if (type == DamageType.TESLA)
					{
						typeOutput = DamageType.NONE;
						damageOutput = 0f;
					}
					else
					{
						goto default;
					}
					break;
				default:
					typeOutput = type;
					damageOutput = damage;
					break;
			}
		}
	}
}

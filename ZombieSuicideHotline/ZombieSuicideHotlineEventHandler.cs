using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;

namespace ZombieSuicideHotline.EventHandlers
{
	class RoundStartHandler : IEventHandlerRoundStart
	{
		private ZombieSuicideHotlinePlugin plugin;

		public RoundStartHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			this.plugin.duringRound = true;
			this.plugin.scp049Kills = new System.Collections.Generic.HashSet<string>();
			this.plugin.zombieDisconnects = new System.Collections.Generic.HashSet<string>();
			foreach (TeamRole teamClass in ev.Server.GetRoles())
			{
				if (!this.plugin.ClassList.ContainsKey(teamClass.Role))
				{
					this.plugin.ClassList.Add(teamClass.Role, teamClass);
				}
			}
		}
	}

	class RoundEndHandler : IEventHandlerRoundEnd
	{
		private ZombieSuicideHotlinePlugin plugin;

		public RoundEndHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{

			if (ev.Round.Duration >= 3)
			{
				this.plugin.duringRound = false;
			}
		}
	}

	class PlayerJoinHandler : IEventHandlerPlayerJoin
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerJoinHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				this.plugin.Info("[OnPlayerJoin] Waiting 30 seconds to respawn [" + ev.Player.IpAddress + "] as a zombie.");
				if (this.plugin.duringRound && this.plugin.zombieDisconnects.Contains(ev.Player.IpAddress))
				{
					System.Timers.Timer t = new System.Timers.Timer
					{
						Interval = 30000,
						AutoReset = false,
						Enabled = true
					};
					t.Elapsed += delegate
					{
						this.plugin.Info("[OnPlayerJoin] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
						this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
						ev.Player.ChangeRole(Role.SCP_049_2, true, true);
					};
				}
			}
		}
	}

	class DisconnectHandler : IEventHandlerDisconnect
	{
		private ZombieSuicideHotlinePlugin plugin;

		public DisconnectHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnDisconnect(DisconnectEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				if (this.plugin.duringRound && this.plugin.scp049Kills.Contains(ev.Connection.IpAddress))
				{
					plugin.Info("[OnPlayerLeave] Removing player [" + ev.Connection.IpAddress + "] from scp049Kills after disconnecting.");
					this.plugin.scp049Kills.Remove(ev.Connection.IpAddress);
					plugin.Info("[OnPlayerLeave] Adding player [" + ev.Connection.IpAddress + "] to zombieDisconnects for leaving.");
					this.plugin.zombieDisconnects.Add(ev.Connection.IpAddress);
				}
			}
		}
	}

	class SpawnHandler : IEventHandlerSpawn
	{
		private ZombieSuicideHotlinePlugin plugin;

		public SpawnHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnSpawn(PlayerSpawnEvent ev)
		{
			if (this.plugin.duringRound && this.plugin.scp049Kills.Contains(ev.Player.IpAddress))
			{
				plugin.Info("[OnPlayerLeave] Removing player [" + ev.Player.IpAddress + "] from scp049Kills for spawning in.");
				this.plugin.scp049Kills.Remove(ev.Player.IpAddress);
			}
		}
	}

	class SetRoleHandler : IEventHandlerSetRole
	{
		private ZombieSuicideHotlinePlugin plugin;

		public SetRoleHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				if (this.plugin.duringRound && this.plugin.zombieDisconnects.Contains(ev.Player.IpAddress))
				{
					plugin.Debug("[OnSetClass] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
					this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
					ev.Role = Role.SCP_049_2;
				}
			}
		}
	}

	class PlayerDieHandler : IEventHandlerPlayerDie
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled")) {
				if (this.plugin.duringRound && ev.Killer.TeamRole.Role == Role.SCP_049)
				{
					plugin.Info("[OnPlayerDie] Adding player [" + ev.Player.IpAddress + "] from scp049Kills.");
					this.plugin.scp049Kills.Add(ev.Player.IpAddress);
				}
				/*else if (ev.Player.TeamRole.Role == Role.SCP_049_2)
				{
					plugin.Info("SCP-049-2 died to " + ev.DamageTypeVar);
					if (DamageType.WALL.Equals(ev.DamageTypeVar))
					{
						
					}
				}*/
			}
		}		
	}

	class PlayerHurtHandler : IEventHandlerPlayerHurt
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerHurtHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin) plugin;
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				switch (ev.Player.TeamRole.Role)
				{
					case Role.SCP_049_2:
						switch (ev.DamageType)
						{
							case DamageType.TESLA:
								//ev.DamageType = DamageType.NONE;
								//ev.Damage = 0f;
							case DamageType.WALL:
								Player targetPlayer = null;
								//TeamRole lastTeamRole = null;
								foreach (Player player in this.plugin.Server.GetPlayers())
								{
									if (ev.Player.SteamId.Equals(player.SteamId))
									{
										continue;
									}

									if (player.TeamRole.Role == Role.SCP_079)
									{
										continue;
									}

									if (targetPlayer == null)
									{
										targetPlayer = player;
									}

									if (player.TeamRole.Team == Team.SCP)
									{
										targetPlayer = player;
									}

									if (player.TeamRole.Role == Role.SCP_049)
									{
										targetPlayer = player;
										break;
									}
								}

								if (targetPlayer != null)
								{
									ev.Damage = 0;
									ev.Player.Teleport(targetPlayer.GetPosition());
								}
								break;
						}
						break;
				}
			}
		}
	}
}

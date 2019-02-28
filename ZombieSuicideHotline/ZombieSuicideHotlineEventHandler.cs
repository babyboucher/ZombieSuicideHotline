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
			//this.plugin.zombieDisconnects = new System.Collections.Generic.HashSet<string>();
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

	/*
	 * ALGORITHM:
	 * 1.  If player dies, add to list of scp049Kills.
	 * 2a. If a player spawns as SCP-049-2, remove from scp049Kills
	 * 2b. If player killed by SCP-049 disconnects, remove from scp049Kills
	 *      and add to zombieDisconnects.
	 * 3.  If the player reconnects to the server, attempt to respawn the
	 *     player as SCP-049-2.
	 */
	
	/**
	 * 1
	 */
	class PlayerDieHandler : IEventHandlerPlayerDie
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				if (this.plugin.duringRound && ev.Killer.TeamRole.Role == Role.SCP_049)
				{
					this.plugin.Info("[OnPlayerDie] Adding player [" + ev.Player.IpAddress + "] from scp049Kills.");
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

	/**
	 * 2a
	 */
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

	/**
	 * 2a
	 */
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
					plugin.Info("[OnSetClass] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
					this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
					ev.Role = Role.SCP_049_2;
				}
			}
		}
	}

	/**
	 * 2b
	 */
	class DisconnectHandler : IEventHandlerDisconnect
	{
		private ZombieSuicideHotlinePlugin plugin;

		public DisconnectHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
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

	/**
	 * 3
	 */
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
				if (this.plugin.duringRound && this.plugin.zombieDisconnects.Contains(ev.Player.IpAddress))
				{
					this.plugin.Info("[OnPlayerJoin] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
					this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
					int counter = 0;
					do
					{
						this.plugin.Info("[OnPlayerJoin] Making " + counter + " attempt(s) to respawn [" + ev.Player.IpAddress + "] as a zombie.");
						ev.Player.ChangeRole(Role.SCP_049_2, true, true);
						counter++;
					} while (ev.Player.TeamRole.Role != Role.SCP_049_2 && counter < 5);
				}
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
				if (ev.Player.TeamRole.Role == Role.SCP_049_2 && (ev.DamageType == DamageType.TESLA || ev.DamageType == DamageType.WALL))
				{
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
				}
			}
		}
	}
}

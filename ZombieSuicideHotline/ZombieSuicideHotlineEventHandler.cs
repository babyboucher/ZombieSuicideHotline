using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Text.RegularExpressions;
using System.Linq;

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
			this.plugin.lastRecall = 0;
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
				if (this.plugin.duringRound)
				{
					if (ev.Killer.TeamRole.Role == Role.SCP_049)
					{
						this.plugin.Info("[OnPlayerDie] Adding player [" + ev.Player.IpAddress + "] from scp049Kills.");
						this.plugin.scp049Kills.Add(ev.Player.IpAddress);
					}
					else if (ev.Player.TeamRole.Role == Role.SCP_049_2 && this.plugin.zombieDisconnects.Contains(ev.Player.IpAddress))
					{
						plugin.Info("[OnSetClass] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects for dying as a zombie.");
						this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
					}
				}
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
			if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
			{
				/*if (ev.Player.TeamRole.Role == Role.SCP_049_2)
				{
					foreach (Player scp049 in this.plugin.Server.GetPlayers())
					{
						if (scp049.TeamRole.Role == Role.SCP_049)
						{
							ev.Player.Teleport(scp049.GetPosition(), true);
						}
					}
				}*/

				if (this.plugin.duringRound && this.plugin.scp049Kills.Contains(ev.Player.IpAddress))
				{
					plugin.Info("[OnPlayerLeave] Removing player [" + ev.Player.IpAddress + "] from scp049Kills for spawning in.");
					this.plugin.scp049Kills.Remove(ev.Player.IpAddress);
				}
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
				/*if (ev.Player.TeamRole.Role == Role.SCP_049_2)
				{
					foreach (Player scp049 in this.plugin.Server.GetPlayers())
					{
						if (scp049.TeamRole.Role == Role.SCP_049)
						{
							ev.Player.Teleport(scp049.GetPosition(), true);
						}
					}
				}*/

				if (this.plugin.duringRound && this.plugin.zombieDisconnects.Contains(ev.Player.IpAddress))
				{
					//plugin.Info("[OnSetClass] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
					//this.plugin.zombieDisconnects.Remove(ev.Player.IpAddress);
					this.plugin.Info("[OnSetRole] Force spawning disconnecting player " + ev.Player.Name + " as zombie...");
					ev.Player.ChangeRole(Role.SCP_049_2);
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

	class CallCommandHandler : IEventHandlerCallCommand
	{
		private ZombieSuicideHotlinePlugin plugin;

		public CallCommandHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] quotedArgs = Regex.Matches(ev.Command, "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
				.Cast<Match>()
				.Select(m => m.Value)
				.ToArray()
				.Skip(1)
				.ToArray();

			switch (command)
			{
				case "recall":
					if (this.plugin.GetConfigBool("zombie_suicide_hotline_enabled"))
					{
						if (ev.Player.TeamRole.Role == Role.SCP_049)
						{
							if (this.plugin.lastRecall == 0 || this.plugin.lastRecall + 15 <= this.plugin.Server.Round.Duration)
							{
								bool zombiesFound = false;
								foreach (Player checkZombie in this.plugin.Server.GetPlayers())
								{
									if (checkZombie.TeamRole.Role == Role.SCP_049_2)
									{
										checkZombie.Teleport(ev.Player.GetPosition(), true);
										zombiesFound = true;
									}
								}

								if (zombiesFound)
								{
									ev.ReturnMessage = "All of your SCP-049-2 have been teleported to you.";
									this.plugin.lastRecall = this.plugin.Server.Round.Duration;
								}
								else
								{
									ev.ReturnMessage = "You have no SCP-049-2 alive right now.";
								}
							}
							else
							{
								ev.ReturnMessage = "You must wait " + ((this.plugin.lastRecall + 15) - this.plugin.Server.Round.Duration) + " seconds before using .recall again.";
							}
						}
						else
						{
							ev.ReturnMessage = "You cannot recall zombies unless you are SCP-049.";
						}
					}
					else
					{
						ev.ReturnMessage = "Zombie Suicide Hotline is currently disabled.";
					}
					break;
			}
		}
	}
}

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
						plugin.Info("[OnPlayerJoin] Removing player [" + ev.Player.IpAddress + "] from zombieDisconnects.");
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
					plugin.Info("[OnPlayerLeave] Removing player [" + ev.Connection.IpAddress + "] from scp049Kills.");
					this.plugin.scp049Kills.Remove(ev.Connection.IpAddress);
					plugin.Info("[OnPlayerLeave] Adding player [" + ev.Connection.IpAddress + "] to zombieDisconnects.");
					this.plugin.zombieDisconnects.Add(ev.Connection.IpAddress);
				}
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
					ev.Role = Role.SCP_049;
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
				else if (ev.Player.TeamRole.Role == Role.SCP_049_2)
				{
					plugin.Info("SCP-049-2 died to " + ev.DamageTypeVar);
					if (DamageType.WALL.Equals(ev.DamageTypeVar))
					{
						{
							System.Timers.Timer t = new System.Timers.Timer
							{
								Interval = 1000,
								AutoReset = false,
								Enabled = true
							};
							t.Elapsed += delegate
							{
								plugin.Info("[OnPlayerDie] SCP-049-2 [" + ev.Player.IpAddress + "] fell to his/her death, respawn as Chaos Insurgency.");
								ev.Player.ChangeRole(Role.CHAOS_INSURGENCY);
								t.Enabled = false;
								t.Dispose();
							};
						}
						{
							System.Timers.Timer t = new System.Timers.Timer
							{
								Interval = 2000,
								AutoReset = false,
								Enabled = true
							};
							t.Elapsed += delegate
							{
								plugin.Info("[OnPlayerDie] Killing player [" + ev.Player.IpAddress + "] as Chaos Insurgency.");
								ev.Player.Kill();
								t.Enabled = false;
								t.Dispose();
							};
						}
						{
							System.Timers.Timer t = new System.Timers.Timer
							{
								Interval = 3000,
								AutoReset = false,
								Enabled = true
							};
							t.Elapsed += delegate
							{
								plugin.Info("[OnPlayerDie] Respawning player [" + ev.Player.IpAddress + "] as SCP-049-2.");
								ev.Player.ChangeRole(Role.SCP_049_2, true, true);
								t.Enabled = false;
								t.Dispose();
							};
						}
					}
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
				switch (ev.Player.TeamRole.Role)
				{
					case Role.SCP_049_2:
						if (ev.DamageType == DamageType.TESLA)
						{
							ev.DamageType = DamageType.NONE;
							ev.Damage = 0f;
						}
						break;
				}
			}
		}
	}
}

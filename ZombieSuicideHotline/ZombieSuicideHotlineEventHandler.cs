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
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnRoundStart(Server server)
		{
			foreach (TeamClass teamClass in server.GetClasses())
			{
				this.plugin.ClassList.Add(teamClass.ClassType, teamClass);
			}
			plugin.Info("ClassList SIZE " + this.plugin.ClassList.Count);
		}
	}

	class PlayerDieHandler : IEventPlayerDie
	{
		private ZombieSuicideHotlinePlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnPlayerDie(Player player, Player killer, out bool spawnRagdoll)
		{
			//plugin.Info("ZombieSuicideHotline OnPlayerDie");
			//plugin.Info(player.ToString());
			//plugin.Info(player.Class.Name);
			//plugin.Info(killer.ToString());
			//plugin.Info(killer.Class.Name);
			if (player.Class.ClassType == Classes.SCP_049_2)
			{
				//plugin.Info("ZombieSuicideHotline OnPlayerDie 049-2");
				//if (killer == null)
				//if (player == killer)
				if (player.SteamId == killer.SteamId)
				{
					spawnRagdoll = false;

					this.plugin.zombieSuicides.Add(player.SteamId);
					/*Vector position = null;
					if (this.plugin.SCPSpawnPoints.TryGetValue(Classes.SCP_049, out position))
					{
						plugin.Info("ZombieSuicideHotline OnPlayerDie RESPAWN");
						//player.ChangeClass(Classes.SCP_049_2, true, true);
						player.Teleport(position);
					}
					else
					{
						plugin.Info("SCP-049-2 SPAWNED WITHOUT SCP-049.");
						//player.ChangeClass(Classes.SCP_049_2, true, true);
					}*/
				}
				else
				{
					spawnRagdoll = true;
				}
			}
			else if (player.Class.ClassType == Classes.SCP_106)
			{
				plugin.Info("ZombieSuicideHotline Larry");
				spawnRagdoll = false;
			}
			else
			{
				plugin.Info("ZombieSuicideHotline Other");
				spawnRagdoll = true;
			}
		}
	}

	/*class AssignTeamHandler : IEventAssignTeam
	{
		private ZombieSuicideHotlinePlugin plugin;

		public AssignTeamHandler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnAssignTeam(Player player, Teams team, out Teams teamOutput)
		{
			if (this.plugin.zombieSuicides.Contains(player.SteamId))
			{
				plugin.Info("SUICIDE ZOMBIE FOUND " + player.ToString() + " " + player.Class.ToString());
				this.plugin.zombieSuicides.Remove(player.SteamId);
				TeamClass teamClass = null;
				if (this.plugin.ClassList.TryGetValue(Classes.SCP_049_2, out teamClass))
				{
					plugin.Info("ZOMBIE CLASS FOUND, SET FROM SPECTATOR TO ZOMBIE " + teamClass.ToString());
					teamOutput = teamClass.Team;
				} else
				{
					plugin.Info("ZOMBIE CLASS NOT FOUND");
					teamOutput = team;
				}
			}
			else
			{
				plugin.Info("NON-ZOMBIE ASSIGNED TEAM " + player.ToString() + " " + player.Class.ToString());
				teamOutput = team;
			}
		}
	}*/

	class SetClass1Handler : IEventSetClass
	{
		private ZombieSuicideHotlinePlugin plugin;

		public SetClass1Handler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnSetClass(Player player, TeamClass teamclass, out TeamClass teamclassOutput)
		{
			// Player's vector is spectator vector in this event
			/*switch (teamclass.ClassType)
			{
				case Classes.SCP_049:
				case Classes.SCP_096:
				case Classes.SCP_106:
				case Classes.SCP_173:
					this.plugin.SCPSpawnPoints.Add(player.Class.ClassType, player.GetPosition());
					break;
			}*/

			/*plugin.Info("PLAYER " + player.Name + " is changing class to " + teamclass.Name);
			if (teamclass.ClassType == Classes.SCP_049)
			{
				this.plugin.plagueDoctorClass = teamclass;
				plugin.Info("PLAGUE DOCTOR ASSIGNED." + this.plugin.plagueDoctorClass.Name);
			}
			else if (teamclass.ClassType == Classes.SCP_049_2)
			{
				this.plugin.zombieClass = teamclass;
				plugin.Info("ZOMBIE CLASS ASSIGNED." + this.plugin.zombieClass.Name);
			}

			plugin.Info("ZOMBIE SUICIDES");
			foreach (string s in this.plugin.zombieSuicides)
			{
				plugin.Info(s);
			}

			plugin.Info("DEAD DOCTORS");
			foreach (string s in this.plugin.zombieSuicides)
			{
				plugin.Info(s);
			}*/

			if (this.plugin.zombieSuicides.Contains(player.SteamId))
			{
				plugin.Info("RESPAWNING AS PLAGUE DOCTOR " //+ this.plugin.plagueDoctorClass.Name
				);
				//teamclassOutput.Team = Teams.SCP;
				//teamclassOutput.ClassType = Classes.SCP_049_2;
				this.plugin.zombieSuicides.Remove(player.SteamId);
				this.plugin.deadDoctors.Add(player.SteamId);
				teamclassOutput = this.plugin.ClassList[Classes.SCP_049]; //this.plugin.plagueDoctorClass;
			}
			/*else if (this.plugin.deadDoctors.Contains(player.SteamId))
			{
				plugin.Info("KILLING DOCTOR " + this.plugin.zombieClass.Name);
				//player.Damage(9999, DamageType.TESLA);
				this.plugin.deadDoctors.Remove(player.SteamId);
				teamclassOutput = this.plugin.ClassList[Classes.SCP_049_2]; //this.plugin.zombieClass;
			}*/
			else
			{
				plugin.Info("NOT RESPAWNING ZOMBIE");
				teamclassOutput = teamclass;
			}
		}
	}

	class SetClass2Handler : IEventSetClass
	{
		private ZombieSuicideHotlinePlugin plugin;

		public SetClass2Handler(Plugin plugin)
		{
			this.plugin = (ZombieSuicideHotlinePlugin)plugin;
		}

		public void OnSetClass(Player player, TeamClass teamclass, out TeamClass teamclassOutput)
		{
			if (this.plugin.deadDoctors.Contains(player.SteamId))
			{
				plugin.Info("KILLING DOCTOR " + this.plugin.zombieClass.Name);
				//player.Damage(9999, DamageType.TESLA);
				this.plugin.deadDoctors.Remove(player.SteamId);
				teamclassOutput = this.plugin.ClassList[Classes.SCP_049_2]; //this.plugin.zombieClass;
			} else
			{
				plugin.Info("NOT RESPAWNING DOCTOR");
				teamclassOutput = teamclass;
			}
		}
	}
}

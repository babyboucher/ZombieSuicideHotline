using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Events;
using System.Collections.Generic;
using ZombieSuicideHotline.EventHandlers;

namespace ZombieSuicideHotline
{
	[PluginDetails(
		author = "PatPeter",
		name = "ZombieSuicideHotline",
		description = "Respawns zombies that intentionally kill themselves.",
		id = "patpeter.zombiesuicidehotline",
		version = "1.0-build17",
		SmodMajor = 2,
		SmodMinor = 0,
		SmodRevision = 0
		)]
	class ZombieSuicideHotlinePlugin : Plugin
    {
        public HashSet<string> zombieSuicides = new HashSet<string>();
		public HashSet<string> deadDoctors = new HashSet<string>();
		public TeamClass plagueDoctorClass = null;
		public TeamClass zombieClass = null;
		public Dictionary<Classes, TeamClass> ClassList = new Dictionary<Classes, TeamClass>();

		public override void OnEnable()
        {
            this.Info("Zombie Suicide Hotline has loaded :)");
            this.Info("Config value: " + this.GetConfigString("zombie_suicide_hotline_enabled"));
        }

        public override void OnDisable()
		{
		}

		public override void Register()
		{
            // Register Events
            this.AddEventHandler(typeof(IEventRoundStart), new RoundStartHandler(this), Priority.Highest);
            this.AddEventHandler(typeof(IEventPlayerDie), new PlayerDieHandler(this), Priority.Highest);
			//this.AddEventHandler(typeof(IEventAssignTeam), new AssignTeamHandler(this), Priority.High);
			this.AddEventHandler(typeof(IEventSetClass), new SetClass1Handler(this), Priority.High);
			this.AddEventHandler(typeof(IEventSetClass), new SetClass2Handler(this), Priority.Normal);
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("zombie_suicide_hotline_enabled", true, Smod2.Config.SettingType.BOOL, true, "Enables or disables the zombie suicide hotline."));
		}
	}
}

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
		version = "1.0-build18",
		SmodMajor = 2,
		SmodMinor = 2,
		SmodRevision = 1
		)]
	class ZombieSuicideHotlinePlugin : Plugin
    {
		internal bool duringRound = false;
		internal HashSet<string> scp049Kills = new HashSet<string>();
		internal HashSet<string> zombieDisconnects = new HashSet<string>();
		internal Dictionary<Classes, TeamClass> ClassList = new Dictionary<Classes, TeamClass>();

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
			this.AddEventHandler(typeof(IEventRoundEnd), new RoundEndHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventPlayerJoin), new PlayerJoinHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventPlayerLeave), new PlayerLeaveHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventSetClass), new SetClassHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventPlayerDie), new PlayerDieHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventPlayerHurt), new PlayerHurtHandler(this), Priority.Highest);
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("zombie_suicide_hotline_enabled", true, Smod2.Config.SettingType.BOOL, true, "Enables or disables the zombie suicide hotline."));
		}
	}
}

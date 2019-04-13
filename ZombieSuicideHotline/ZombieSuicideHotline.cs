using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using ZombieSuicideHotline.EventHandlers;

namespace ZombieSuicideHotline
{
	[PluginDetails(
		author = "PatPeter",
		name = "ZombieSuicideHotline",
		description = "Respawns zombies that intentionally kill themselves.",
		id = "patpeter.zombie.suicide.hotline",
		version = "1.3.7.31",
		SmodMajor = 3,
		SmodMinor = 2,
		SmodRevision = 0
		)]
	class ZombieSuicideHotlinePlugin : Plugin
    {
		internal bool duringRound = false;
		internal HashSet<string> scp049Kills = new HashSet<string>();
		internal HashSet<string> zombieDisconnects = new HashSet<string>();

		public override void OnEnable()
        {
            this.Info("Zombie Suicide Hotline has loaded :)");
            this.Info("Config value: " + this.GetConfigBool("zombie_suicide_hotline_enabled"));
        }

        public override void OnDisable()
		{

		}

		public override void Register()
		{
            // Register Events
            this.AddEventHandler(typeof(IEventHandlerRoundStart), new RoundStartHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerRoundEnd), new RoundEndHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerPlayerJoin), new PlayerJoinHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerDisconnect), new DisconnectHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerSpawn), new SpawnHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerSetRole), new SetRoleHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerPlayerDie), new PlayerDieHandler(this), Priority.Lowest);
			this.AddEventHandler(typeof(IEventHandlerPlayerHurt), new PlayerHurtHandler(this), Priority.Lowest);
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("zombie_suicide_hotline_enabled", true, Smod2.Config.SettingType.BOOL, true, "Enables or disables the zombie suicide hotline."));
		}
	}
}

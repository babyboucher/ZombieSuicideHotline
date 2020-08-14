using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace ZombieSuicideHotline
{

    public class Plugin : Exiled.API.Features.Plugin<SuicideConfig>
    {
        public override string Name { get; } = "Zombie Suicide Hotline";
        public override string Author { get; } = "Babyboucher20";
        public override Version Version { get; } = new Version(1, 0, 0);
        //public override Version RequiredExiledVersion { get; } = new Version(2, 0, 12);
        public override string Prefix { get; } = "ZombieSuicideHotline";

        public PlayerHandlers PlayerHandlers;
        public Dictionary<string, Zombie> zombies = new Dictionary<string, Zombie>();
        public static Plugin Singleton;

        public override void OnEnabled()
        {
            Singleton = this;

            Log.Info($"Instantiating Events..");
            PlayerHandlers = new PlayerHandlers(this);

            Log.Info($"Registering EventHandlers..");
            Player.Joined += PlayerHandlers.OnPlayerJoined;
            Player.Died += PlayerHandlers.OnPlayerDied;
            Player.Left += PlayerHandlers.OnPlayerLeft;
            Player.Hurting += PlayerHandlers.OnPlayerHurt;
            Player.ChangingRole += PlayerHandlers.OnPlayerRoleChange;
            Player.Spawning += PlayerHandlers.OnPlayerSpawn;
            Server.RoundStarted += PlayerHandlers.OnRoundEnd;
        }
        public override void OnDisabled()
        {
            Player.Joined -= PlayerHandlers.OnPlayerJoined;
            Player.Died -= PlayerHandlers.OnPlayerDied;
            Player.Left -= PlayerHandlers.OnPlayerLeft;
            Player.Hurting -= PlayerHandlers.OnPlayerHurt;
            Player.ChangingRole -= PlayerHandlers.OnPlayerRoleChange;
            Player.Spawning -= PlayerHandlers.OnPlayerSpawn;
            Server.RoundStarted -= PlayerHandlers.OnRoundEnd;

        }
    }

    public class Zombie
    {
        public int PlayerId;
        public string Name;
        public string SteamId;
        public string IpAddress;
        public int LastRecall = 0;
        public bool Undead = false;
        public bool Disconnected = false;
        //public Dictionary<string, Zombie> Zombies = new Dictionary<string, Zombie>();

        public Zombie(int playerId, string name, string steamId, string ipAddress)
        {
            this.PlayerId = playerId;
            this.Name = name;
            this.SteamId = steamId;
            this.IpAddress = ipAddress;
        }


        public override string ToString()
        {
            return "[ PlayerId: " + PlayerId + ", Name: " + Name + ", SteamID: " + SteamId + ", IpAddress: " + IpAddress + ", LastRecall: " + LastRecall + ", Undead: " + Undead + ", Disconnected: " + Disconnected + " ]";
        }
    }
}

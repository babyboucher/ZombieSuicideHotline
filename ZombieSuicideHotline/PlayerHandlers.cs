﻿namespace ZombieSuicideHotline
{
    using Exiled.Events.EventArgs;
    using Player = Exiled.API.Features.Player;
    using Exiled.API.Features;

    public class PlayerHandlers
    {
        private readonly Plugin plugin;
        public PlayerHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            Player player = ev.Player;
            if (!this.plugin.zombies.ContainsKey(ev.Player.UserId))
            {
                this.plugin.zombies[ev.Player.UserId] = new Zombie(player.Id, player.Nickname, player.UserId, player.IPAddress);
            }
        }
        public void OnRoundEnd()
        {
            foreach (Player player in Exiled.API.Features.Player.List)
            {
                if (this.plugin.zombies.ContainsKey(player.UserId))
                {
                    this.plugin.zombies[player.UserId].Disconnected = false;
                }
            }
        }
        public void OnPlayerRoleChange(ChangingRoleEventArgs ev)
        {
            Player player = ev.Player;
            if (plugin.zombies[player.UserId].Disconnected)
            {
                plugin.zombies[player.UserId].Disconnected = false;
                ev.NewRole = RoleType.Scp0492;
            }
        }

        public void OnPlayerSpawn(SpawningEventArgs ev)
        {
            Player player = ev.Player;
            if (ev.RoleType == RoleType.Scp0492)
            {
                Player targetPlayer = GetTeleportTarget(player);
                if (targetPlayer != null)
                {
                    ev.Position = targetPlayer.Position;
                }
            }
            if (ev.RoleType == RoleType.Scp049)
            {
                player.Broadcast(10, "Use .recall to bring all your zombies to you");
            }
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            Player player = ev.Target;
            if (ev.Target.Role == RoleType.Scp0492)
            {
                    plugin.zombies[player.UserId].Disconnected = false;
            }
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp0492 && (ev.DamageType == DamageTypes.Tesla || ev.DamageType == DamageTypes.Wall || ev.DamageType == DamageTypes.Decont))
                {
                Player targetPlayer = GetTeleportTarget(ev.Target);
                if (targetPlayer != null)
                    {
                    ev.Amount = plugin.Config.ZombieDmg;
                    ev.Target.Position = targetPlayer.Position;
                    }
                }
            
        }

        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player.Role == RoleType.Scp0492)
            {
                plugin.zombies[ev.Player.UserId].Disconnected = true;
            }
        }

        public Player GetTeleportTarget(Player sourcePlayer)
        {
            Player targetPlayer = null;
            foreach (Player player in Exiled.API.Features.Player.List)
            {
                if (sourcePlayer.UserId.Equals(player.UserId))
                {
                    continue;
                }

                if (player.Role == RoleType.Scp079)
                {
                    continue;
                }

                if (targetPlayer == null)
                {
                    targetPlayer = player;
                }

                if (player.Team == Team.SCP)
                {
                    targetPlayer = player;
                }

                if (player.Role == RoleType.Scp049)
                {
                    targetPlayer = player;
                    break;
                }
            }
            return targetPlayer;
        }
    }
}
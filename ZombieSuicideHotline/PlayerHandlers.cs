namespace ZombieSuicideHotline
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
            Log.Info($"PlayerJoin ran");
            Player player = ev.Player;
            if (!this.plugin.zombies.ContainsKey(ev.Player.UserId))
            {
                this.plugin.zombies[ev.Player.UserId] = new Zombie(player.Id, player.Nickname, player.UserId, player.IPAddress);
            }

            if (plugin.zombies.ContainsKey(player.UserId) && plugin.zombies[player.UserId].Undead && player.Role != RoleType.Spectator)
            {
                plugin.zombies[player.UserId].Undead = false;
                plugin.zombies[ev.Player.UserId].Disconnected = false;
            }
            else if (plugin.zombies.ContainsKey(player.UserId) && plugin.zombies[player.UserId].Disconnected && player.Role != RoleType.Scp0492)
            {
                player.SetRole(RoleType.Scp0492, true);
                Player targetPlayer = GetTeleportTarget(player);

                if (targetPlayer != null)
                {
                    player.GameObject.transform.position = targetPlayer.GameObject.transform.position;
                }
            }
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            Player player = ev.Target;
            if (ev.Killer.Role == RoleType.Scp049)
            {
                //this.plugin.scp049Kills.Add(ev.Player.IpAddress);
                if (plugin.zombies.ContainsKey(player.UserId))
                {
                    plugin.zombies[player.UserId].Undead = true;
                }
                else
                {
                    plugin.zombies[player.UserId] = new Zombie(player.Id, player.Nickname, player.UserId, player.IPAddress);
                    plugin.zombies[player.UserId].Undead = true;
                }
            }
            else if (ev.Target.Role == RoleType.Scp0492)
            {
                if (plugin.zombies.ContainsKey(player.UserId))
                {
                    plugin.zombies[player.UserId].Undead = false;
                    plugin.zombies[player.UserId].Disconnected = false;
                }
                else
                {
                    plugin.zombies[player.UserId] = new Zombie(player.Id, player.Nickname, player.UserId, player.IPAddress);
                    plugin.zombies[player.UserId].Undead = false;
                    plugin.zombies[player.UserId].Disconnected = false;
                }
            }
        }
        /// <summary>
        /// ///////////////////
        /// </summary>
        /// <param name="ev"></param>
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            Log.Info($"PlayerHurt ran");
            if (ev.Target.Role == RoleType.Scp0492 && (ev.DamageType == DamageTypes.Tesla || ev.DamageType == DamageTypes.Wall))
                {
                    Player targetPlayer = GetTeleportTarget(ev.Target);
                ev.Amount = 0;
                if (targetPlayer != null)
                    {
                    ev.Target.Position = targetPlayer.Position;
                    }
                }
            
        }

        public void OnPlayerLeft(LeftEventArgs ev)
        {
            Log.Info($"Player Leave ran");
            if (ev.Player.Role == RoleType.Scp0492)
            {
                plugin.zombies[ev.Player.UserId].Undead = false;
                plugin.zombies[ev.Player.UserId].Disconnected = true;
            }
        }
        public Player GetTeleportTarget(Player sourcePlayer)
        {
            Player targetPlayer = null;
            //TeamRole lastTeamRole = null;
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
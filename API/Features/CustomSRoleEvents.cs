using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace CustomSRole.API.Features
{
    internal static class CustomSRoleEvents
    {
        internal static void OnRoundEnded(RoundEndedEventArgs _)
        {
            foreach (var role in CustomSRoleManager.RegisteredRoles.Values)
            {
                role.TrackedPlayers.Clear();
            }
            CustomSRoleManager.PlayerRolesDict.Clear();
            CustomSRoleManager.ResetSpawnedCount();
        }

        internal static void OnSpawnedGlobal(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;

            CustomSRoleManager.TrySpawnRole(ev.Player);
        }

        internal static void OnPlayerDiedGlobal(DiedEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles.ToList())
                {
                    role.OnPlayerDied(ev);

                    if (role.CanRespawn && role.CanPlayerRespawn(ev.Player))
                    {
                        role.ScheduleRespawn(ev.Player);
                    }
                    else if (!role.KeepRoleOnDeath)
                    {
                        role.RemoveRole(ev.Player);
                    }
                }
            }
        }

        internal static void OnPlayerLeftGlobal(LeftEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles.ToList())
                {
                    role.OnPlayerLeft(ev);
                    role.RemoveRole(ev.Player);
                }
            }
        }

        private static void FFHurt(Player attacker, Player hurter, float damage)
        {
            hurter.Hurt(damage);
            attacker.ShowHitMarker(1);
        }

        internal static void OnHurtingGlobal(HurtingEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null) return;

            if (ev.Player.Id == ev.Attacker.Id) return;

            if (CustomSRoleManager.IsCustomEnemyButSameTeam(ev.Attacker, ev.Player))
            {
                if (ev.IsInstantKill)
                {
                    ev.Player.Kill("被敌对阵营击杀");
                }
                ev.IsAllowed = true;
                FFHurt(ev.Attacker, ev.Player, ev.DamageHandler.DealtHealthDamage);
            }

            var attackerRoles = CustomSRoleManager.GetPlayerRoles(ev.Attacker).ToList();
            var targetRoles = CustomSRoleManager.GetPlayerRoles(ev.Player).ToList();

            if (attackerRoles.Count == 0 && targetRoles.Count == 0)
            {
                if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
                {
                    foreach (var role in roles)
                        role.OnHurting(ev);
                }
                return;
            }

            bool isEnemy = false;
            bool isAlly = false;

            foreach (var attackerRole in attackerRoles)
            {
                if (attackerRole.IsEnemy(ev.Player))
                {
                    isEnemy = true;
                    break;
                }
                if (attackerRole.IsAlly(ev.Player))
                {
                    isAlly = true;
                    break;
                }
            }

            if (isAlly && !isEnemy)
            {
                ev.IsAllowed = false;
                return;
            }

            if (isEnemy)
            {
                ev.IsAllowed = true;
            }

            foreach (var attackerRole in attackerRoles)
            {
                ev.DamageHandler.DealtHealthDamage *= attackerRole.DamageMultiplier;
            }

            foreach (var targetRole in targetRoles)
            {
                ev.DamageHandler.DealtHealthDamage *= targetRole.IncomingDamageMultiplier;
            }

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var targetRoleSet))
            {
                foreach (var role in targetRoleSet)
                    role.OnHurting(ev);
            }
        }

        internal static void OnShootingGlobal(ShootingEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles)
                    role.OnShooting(ev);
            }
        }

        internal static void OnInteractingDoorGlobal(InteractingDoorEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles)
                    role.OnInteractingDoor(ev);
            }
        }

        internal static void OnInteractingLockerGlobal(InteractingLockerEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles)
                    role.OnInteractingLocker(ev);
            }
        }

        internal static void OnUsingItemGlobal(UsingItemEventArgs ev)
        {
            if (ev.Player == null) return;

            if (CustomSRoleManager.PlayerRolesDict.TryGetValue(ev.Player.Id, out var roles))
            {
                foreach (var role in roles)
                    role.OnUsingItem(ev);
            }
        }
    }
}

using Exiled.API.Features;
using HintServiceMeow.Core.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using HSMHint = HintServiceMeow.Core.Models.Hints.Hint;
using HSMHintAlignment = HintServiceMeow.Core.Enum.HintAlignment;

namespace CustomSRole.API.Features
{
    public static class RoleInfoHint
    {
        private const string RoleIntroIdPrefix = "RoleInfo_";

        private static readonly Dictionary<int, string> PlayerRoleInfo = [];
        private static readonly Dictionary<int, string> PlayerRoleName = [];

        private static void SetPlayerRoleInfo(Player player, string roleName, string fullInfo)
        {
            if (player == null) return;
            PlayerRoleName[player.Id] = roleName;
            PlayerRoleInfo[player.Id] = fullInfo;
        }

        private static void ClearPlayerRoleInfo(Player player)
        {
            if (player == null) return;
            PlayerRoleInfo.Remove(player.Id);
            PlayerRoleName.Remove(player.Id);
        }

        private static string ExtractRoleName(string info)
        {
            if (string.IsNullOrEmpty(info)) return null;

            int startIndex = info.IndexOf('[');
            int endIndex = info.IndexOf(']');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return info.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            return null;
        }

        public static void AddRoleInfo(Player player, string info)
        {
            if (player == null || string.IsNullOrEmpty(info)) return;

            RemoveRoleInfo(player);

            PlayerDisplay display = PlayerDisplay.Get(player);
            string hintId = $"{RoleIntroIdPrefix}{player.Id}";

            var roleHint = new HSMHint
            {
                Id = hintId,
                Text = $"<align=center><size=20>{info}</size></align>",
                Alignment = HSMHintAlignment.Center,
                XCoordinate = 0,
                YCoordinate = 100,
            };

            display.AddHint(roleHint);

            string roleName = ExtractRoleName(info);
            SetPlayerRoleInfo(player, roleName, info);
        }

        public static void RemoveRoleInfo(Player player)
        {
            if (player == null) return;

            PlayerDisplay display = PlayerDisplay.Get(player);
            string targetId = $"{RoleIntroIdPrefix}{player.Id}";

            display.RemoveHint(targetId);
            ClearPlayerRoleInfo(player);
        }

        public static IEnumerator<float> ShowRoleSpawnMessage(Player player, string first, string second, string last, float waitTime = 11f, float duration = 8f)
        {
            yield return Timing.WaitForSeconds(waitTime);

            PlayerDisplay display = PlayerDisplay.Get(player);
            string hintId = $"SCPStyleHint_{player.Id}_{Guid.NewGuid()}";

            var hint = new HSMHint
            {
                Id = hintId,
                Text = $"<size=40>{first}</size>\n<size=110>{second}</size>\n\n<size=30>{last}</size>",
                FontSize = 40,
                Alignment = HSMHintAlignment.Center,
                XCoordinate = 0,
                YCoordinate = 460,
            };

            display.AddHint(hint);

            Timing.CallDelayed(duration, () =>
            {
                display.RemoveHint(hintId);
            });
        }
    }
}

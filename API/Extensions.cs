using Exiled.API.Features;
using System.Collections.Generic;

namespace CustomSRole.API
{
    public static class Extensions
    {
        public static IEnumerable<Features.CustomSRole> GetCustomSRoles(this Player player)
        {
            return Features.CustomSRoleManager.GetPlayerRoles(player);
        }

        public static bool HasAnyCustomSRole(this Player player)
        {
            return Features.CustomSRoleManager.HasAnyRole(player);
        }

        public static bool HasCustomSRole<T>(this Player player) where T : Features.CustomSRole
        {
            return Features.CustomSRoleManager.HasRole<T>(player);
        }

        public static bool HasCustomSRole(this Player player, uint roleId)
        {
            return Features.CustomSRoleManager.HasRole(player, roleId);
        }

        public static bool HasCustomSRole(this Player player, int customRoleType)
        {
            return Features.CustomSRoleManager.HasRole(player, customRoleType);
        }

        public static void AddCustomSRole(this Player player, Features.CustomSRole role)
        {
            role.AddRole(player);
        }

        public static void RemoveCustomSRole(this Player player, Features.CustomSRole role)
        {
            role.RemoveRole(player);
        }
    }
}

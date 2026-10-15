using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LolAccountSwitcher
{
    public class PowerPlanInfo
    {
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public override string ToString() => Name;
    }

    public static class PowerManager
    {
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_NO_MORE_ITEMS = 259;
        private const uint ACCESS_SCHEME = 16;

        [DllImport("PowrProf.dll", SetLastError = true)]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

        [DllImport("PowrProf.dll", SetLastError = true)]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr pActivePolicyGuid);

        [DllImport("PowrProf.dll", SetLastError = true)]
        private static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingsGuid,
            uint AccessFlags,
            uint Index,
            byte[] Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingsGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        public static Guid? GetActiveSchemeGuid()
        {
            if (PowerGetActiveScheme(IntPtr.Zero, out IntPtr pGuid) == ERROR_SUCCESS && pGuid != IntPtr.Zero)
            {
                try
                {
                    return Marshal.PtrToStructure<Guid>(pGuid);
                }
                finally
                {
                    LocalFree(pGuid);
                }
            }
            return null;
        }

        public static List<PowerPlanInfo> GetPowerPlans()
        {
            var list = new List<PowerPlanInfo>();
            Guid? activeGuid = GetActiveSchemeGuid();
            uint index = 0;
            uint bufferSize = 16; // Guid size
            byte[] buffer = new byte[16];

            while (true)
            {
                bufferSize = 16;
                uint result = PowerEnumerate(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ACCESS_SCHEME,
                    index,
                    buffer,
                    ref bufferSize);

                if (result == ERROR_NO_MORE_ITEMS || result != ERROR_SUCCESS)
                    break;

                Guid schemeGuid = new Guid(buffer);
                string name = ReadSchemeFriendlyName(schemeGuid);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = schemeGuid.ToString();
                }

                bool isActive = activeGuid.HasValue && activeGuid.Value == schemeGuid;

                list.Add(new PowerPlanInfo
                {
                    Guid = schemeGuid,
                    Name = name,
                    IsActive = isActive
                });

                index++;
            }

            return list;
        }

        public static bool SetActiveScheme(Guid schemeGuid)
        {
            try
            {
                uint result = PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
                return result == ERROR_SUCCESS;
            }
            catch
            {
                return false;
            }
        }

        private static string ReadSchemeFriendlyName(Guid schemeGuid)
        {
            uint bufferSize = 0;
            // Get required size
            PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bufferSize);
            if (bufferSize == 0) return string.Empty;

            IntPtr pBuffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                if (PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, pBuffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    return Marshal.PtrToStringUni(pBuffer) ?? string.Empty;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }

            return string.Empty;
        }
    }
}

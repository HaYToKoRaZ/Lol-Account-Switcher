using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LolAccountSwitcher
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public static class DisplayManager
    {
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DM_DISPLAYFREQUENCY = 0x400000;

        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        public static int GetCurrentRefreshRate()
        {
            try
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
                {
                    return dm.dmDisplayFrequency;
                }
            }
            catch { }
            return 60;
        }

        public static List<int> GetSupportedRefreshRates()
        {
            var rates = new SortedSet<int>();
            try
            {
                var currentDm = new DEVMODE();
                currentDm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref currentDm) != 0)
                {
                    int currentW = currentDm.dmPelsWidth;
                    int currentH = currentDm.dmPelsHeight;

                    var dm = new DEVMODE();
                    dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                    int i = 0;
                    while (EnumDisplaySettings(null, i, ref dm) != 0)
                    {
                        if (dm.dmPelsWidth == currentW && dm.dmPelsHeight == currentH && dm.dmDisplayFrequency > 0)
                        {
                            rates.Add(dm.dmDisplayFrequency);
                        }
                        i++;
                    }
                }
            }
            catch { }

            // Yaygın hızlar garantilenir
            if (rates.Count == 0)
            {
                rates.Add(60);
                rates.Add(144);
            }

            return new List<int>(rates);
        }

        public static bool SetRefreshRate(int targetHz)
        {
            if (targetHz <= 0) return false;

            try
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
                {
                    if (dm.dmDisplayFrequency == targetHz) return true; // Zaten o hızda

                    dm.dmDisplayFrequency = targetHz;
                    dm.dmFields |= DM_DISPLAYFREQUENCY;

                    int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
                    return result == DISP_CHANGE_SUCCESSFUL;
                }
            }
            catch { }
            return false;
        }
    }
}

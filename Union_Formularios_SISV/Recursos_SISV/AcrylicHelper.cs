using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Union_Formularios_SISV.Recursos_SISV
{
    public static class AcrylicHelper
    {
        public static bool EnableAcrylic(IntPtr hwnd, Color tint)
        {
            if (!IsWin10OrGreater()) return false;

            int gradientColor = ToABGR(tint);

            var accent = new ACCENT_POLICY
            {
                AccentState = ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                AccentFlags = 2,
                GradientColor = gradientColor,
                AnimationId = 0
            };

            return SetAccentPolicy(hwnd, accent);
        }

        public static bool EnableBlur(IntPtr hwnd)
        {
            if (!IsWin10OrGreater()) return false;

            var accent = new ACCENT_POLICY
            {
                AccentState = ACCENT_STATE.ACCENT_ENABLE_BLURBEHIND,
                AccentFlags = 0,
                GradientColor = 0,
                AnimationId = 0
            };

            return SetAccentPolicy(hwnd, accent);
        }

        private static bool SetAccentPolicy(IntPtr hwnd, ACCENT_POLICY accent)
        {
            try
            {
                int size = Marshal.SizeOf(accent);
                IntPtr accentPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WINDOWCOMPOSITIONATTRIBDATA
                {
                    Attribute = WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY,
                    Data = accentPtr,
                    SizeOfData = size
                };

                int result = SetWindowCompositionAttribute(hwnd, ref data);

                Marshal.FreeHGlobal(accentPtr);
                return result != 0;
            }
            catch
            {
                return false;
            }
        }

        private static int ToABGR(Color c)
        {
            // Windows espera ABGR en GradientColor
            return (c.A << 24) | (c.B << 16) | (c.G << 8) | c.R;
        }

        private static bool IsWin10OrGreater()
        {
            Version v = Environment.OSVersion.Version;
            return v.Major >= 10;
        }

        private enum WINDOWCOMPOSITIONATTRIB
        {
            WCA_ACCENT_POLICY = 19
        }

        private enum ACCENT_STATE
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ACCENT_POLICY
        {
            public ACCENT_STATE AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWCOMPOSITIONATTRIBDATA
        {
            public WINDOWCOMPOSITIONATTRIB Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);
    }
}

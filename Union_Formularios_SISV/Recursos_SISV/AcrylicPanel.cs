using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Recursos_SISV
{
    public class AcrylicPanel : Panel
    {
        [Category("Acrylic")]
        [Description("Activa el efecto Acrylic (si está disponible).")]
        public bool UseAcrylic { get; set; } = true;

        [Category("Acrylic")]
        [Description("Si Acrylic no aplica, intenta Blur como fallback.")]
        public bool UseBlurFallback { get; set; } = true;

        [Category("Acrylic")]
        [Description("Color/tinte del panel. El canal Alpha controla la intensidad del tinte.")]
        public Color TintColor { get; set; } = Color.FromArgb(90, 20, 20, 20);

        [Category("Acrylic")]
        [Description("Color usado por Windows para el Acrylic. Alpha alto = más 'opaco'.")]
        public Color AcrylicTint { get; set; } = Color.FromArgb(160, 20, 20, 20);

        [Category("Acrylic")]
        [Description("Si falla el efecto del sistema, pinta un tinte semi-transparente como fallback visual.")]
        public bool PaintTintFallback { get; set; } = true;

        public AcrylicPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode)
            {
                ApplyEffect();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // A veces el handle está pero aún no se ve; reintentar al volverse visible ayuda.
            if (!DesignMode && Visible && IsHandleCreated)
            {
                ApplyEffect();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Limpia el efecto al destruir el handle
            if (!DesignMode && IsHandleCreated)
            {
                TryDisableEffect(this.Handle);
            }

            base.OnHandleDestroyed(e);
        }

        private void ApplyEffect()
        {
            // Garantiza que exista handle
            IntPtr hwnd = this.Handle;
            if (hwnd == IntPtr.Zero) return;

            bool ok = false;

            if (UseAcrylic)
                ok = TryApplyAcrylic(hwnd, AcrylicTint);

            if (!ok && UseBlurFallback)
                ok = TryApplyBlur(hwnd);

            // Si ok es false, igual se verá el tinte por OnPaintBackground (fallback).
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            // Fallback visual: un tinte glass (no blur real)
            if (PaintTintFallback)
            {
                using (var b = new SolidBrush(TintColor))
                {
                    e.Graphics.FillRectangle(b, this.ClientRectangle);
                }
            }
        }

        // ------------------ Win32 Acrylic / Blur ------------------

        private static bool TryApplyAcrylic(IntPtr hwnd, Color tint)
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

        private static bool TryApplyBlur(IntPtr hwnd)
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

        private static bool TryDisableEffect(IntPtr hwnd)
        {
            var accent = new ACCENT_POLICY
            {
                AccentState = ACCENT_STATE.ACCENT_DISABLED,
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

        // ------------------ P/Invoke ------------------

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

using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Usuarios
{
    public partial class UsuariosTaskCard : UserControl
    {
        public class UsuarioSeleccionadoEventArgs : EventArgs
        {
            public int UsuarioID { get; private set; }
            public UsuarioSeleccionadoEventArgs(int id) { UsuarioID = id; }
        }

        public event EventHandler<UsuarioSeleccionadoEventArgs> UsuarioSeleccionado;

        public int UsuarioID { get; private set; }

        // ====== Tus posiciones FIJAS (NO se recalculan)
        private const int BASE_H = 66;

        private const int LABEL_H = 18;
        private const int Y_TEXT = 24; // tu referencia

        private const int X_LOGIN = 14;
        private const int W_LOGIN = 140;

        private const int X_NOMBRE = 160;
        private const int W_NOMBRE = 190;

        private const int X_CORREO = 360;
        private const int W_CORREO = 180;

        private const int X_ROL = 560;
        private const int W_ROL = 115;

        // Estado fijo EXACTO (no depende del Width)
        private const int X_ESTADO = 689;
        private const int Y_ESTADO = 20;
        private const int W_ESTADO = 95;
        private const int H_ESTADO = 26;

        // Dot foto fijo
        private const int X_DOTFOTO = 6;
        private const int Y_DOTFOTO = 29;

        // ====== UI
        private Guna2Panel _panel;

        private Label _lblLogin;
        private Label _lblNombre;
        private Label _lblCorreo;
        private Label _lblRol;

        private Guna2CirclePictureBox _dotFoto;

        private Guna2Panel _pnlEstado;

        private TableLayoutPanel _estadoCenter3x3;
        private TableLayoutPanel _estadoRow;
        private Guna2CirclePictureBox _dotEstado;
        private Label _lblEstado;

        public UsuariosTaskCard()
        {
            Height = BASE_H;
            BackColor = Color.Transparent;

            _panel = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 10,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(230, 233, 240),
                BorderThickness = 1,
                Padding = new Padding(0)
            };
            Controls.Add(_panel);

            _lblLogin = NewCellLabel(FontStyle.Bold);
            _lblNombre = NewCellLabel(FontStyle.Regular);
            _lblCorreo = NewCellLabel(FontStyle.Regular);
            _lblRol = NewCellLabel(FontStyle.Bold);

            _dotFoto = new Guna2CirclePictureBox
            {
                Size = new Size(8, 8),
                FillColor = Color.FromArgb(14, 165, 233),
                Visible = false,
                BackColor = Color.Transparent
            };

            // Estado (pill)
            _pnlEstado = new Guna2Panel
            {
                BorderRadius = 13,
                BorderThickness = 1,
                BackColor = Color.Transparent
            };

            // Centro del estado
            _estadoCenter3x3 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 3,
                ColumnCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _estadoCenter3x3.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            _estadoCenter3x3.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _estadoCenter3x3.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            _estadoCenter3x3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            _estadoCenter3x3.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _estadoCenter3x3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            _estadoRow = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _estadoRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _estadoRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _estadoRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _dotEstado = new Guna2CirclePictureBox
            {
                Size = new Size(7, 7),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 8, 0),
                Anchor = AnchorStyles.None
            };

            _lblEstado = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Text = "",
                Anchor = AnchorStyles.Left
            };

            _estadoRow.Controls.Add(_dotEstado, 0, 0);
            _estadoRow.Controls.Add(_lblEstado, 1, 0);

            _estadoCenter3x3.Controls.Add(_estadoRow, 1, 1);
            _pnlEstado.Controls.Add(_estadoCenter3x3);

            // Agregar al panel
            _panel.Controls.Add(_lblLogin);
            _panel.Controls.Add(_lblNombre);
            _panel.Controls.Add(_lblCorreo);
            _panel.Controls.Add(_lblRol);
            _panel.Controls.Add(_dotFoto);
            _panel.Controls.Add(_pnlEstado);

            // ✅ POSICIONES FIJAS (SOLO AQUÍ) — ya NO se vuelven a tocar
            _lblLogin.SetBounds(X_LOGIN, Y_TEXT, W_LOGIN, LABEL_H);
            _lblNombre.SetBounds(X_NOMBRE, Y_TEXT, W_NOMBRE, LABEL_H);
            _lblCorreo.SetBounds(X_CORREO, Y_TEXT, W_CORREO, LABEL_H);
            _lblRol.SetBounds(X_ROL, Y_TEXT, W_ROL, LABEL_H);

            _dotFoto.Location = new Point(X_DOTFOTO, Y_DOTFOTO);

            _pnlEstado.SetBounds(X_ESTADO, Y_ESTADO, W_ESTADO, H_ESTADO);

            // Click
            HookClick(_panel);
            HookClick(_lblLogin);
            HookClick(_lblNombre);
            HookClick(_lblCorreo);
            HookClick(_lblRol);
            HookClick(_pnlEstado);
        }

        private Label NewCellLabel(FontStyle style)
        {
            return new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9.5F, style),
                ForeColor = Color.FromArgb(30, 30, 30),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = LABEL_H,
                AutoEllipsis = true,                 // ✅ "..."
                UseCompatibleTextRendering = false
            };
        }

        private void HookClick(Control c)
        {
            if (c == null) return;
            c.Cursor = Cursors.Hand;
            c.Click += (s, e) => RaiseSelected();
        }

        private void RaiseSelected()
        {
            UsuarioSeleccionado?.Invoke(this, new UsuarioSeleccionadoEventArgs(UsuarioID));
        }

        public void Bind(int usuarioId, string login, string nombres, string apellidos, string correo, string rol, bool activo, bool hasFoto)
        {
            UsuarioID = usuarioId;

            _lblLogin.Text = login ?? "";
            _lblNombre.Text = $"{(nombres ?? "").Trim()} {(apellidos ?? "").Trim()}".Trim();
            _lblCorreo.Text = correo ?? "";
            _lblRol.Text = rol ?? "";

            _dotFoto.Visible = hasFoto;

            if (activo)
            {
                _lblEstado.Text = "Activo";
                _lblEstado.ForeColor = Color.FromArgb(16, 122, 70);
                _dotEstado.FillColor = Color.FromArgb(16, 122, 70);

                _pnlEstado.FillColor = Color.FromArgb(232, 250, 240);
                _pnlEstado.BorderColor = Color.FromArgb(90, 200, 140);
            }
            else
            {
                _lblEstado.Text = "Inactivo";
                _lblEstado.ForeColor = Color.FromArgb(190, 30, 30);
                _dotEstado.FillColor = Color.FromArgb(190, 30, 30);

                _pnlEstado.FillColor = Color.FromArgb(255, 235, 235);
                _pnlEstado.BorderColor = Color.FromArgb(235, 110, 110);
            }
        }

        public void SetSelected(bool selected)
        {
            _panel.FillColor = selected ? Color.FromArgb(240, 247, 255) : Color.White;
            _panel.BorderColor = selected ? Color.FromArgb(29, 150, 226) : Color.FromArgb(230, 233, 240);
            _panel.BorderThickness = selected ? 2 : 1;
        }
    }
}
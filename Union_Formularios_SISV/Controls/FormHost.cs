using System;
using System.Text;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls
{
    public sealed class FormHost
    {
        private readonly Panel _panel;
        private readonly Label _lblTitulo;
        private readonly Label _lblDescripcion;
        private Form _activo;

        public FormHost(Panel panelEscritorio, Label lblTitulo = null, Label lblDescripcion = null)
        {
            _panel = panelEscritorio ?? throw new ArgumentNullException(nameof(panelEscritorio));
            _lblTitulo = lblTitulo;
            _lblDescripcion = lblDescripcion;

            // Importantísimo para que no quede “en blanco” en algunos casos
            _panel.ControlAdded += (s, e) =>
            {
                try { e.Control?.BringToFront(); } catch { }
            };
        }

        public void Open(Form form, string titulo = null, string descripcion = null)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            try
            {
                _panel.SuspendLayout();

                // Cerrar activo anterior
                if (_activo != null)
                {
                    try
                    {
                        if (!_activo.IsDisposed)
                        {
                            _activo.Close();
                            _activo.Dispose();
                        }
                    }
                    catch { /* ignora */ }
                    finally
                    {
                        _activo = null;
                    }
                }

                _panel.Controls.Clear();

                _activo = form;

                // Configuración de “form embebido”
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                // Evita efectos raros visuales
                form.Visible = true;
                form.ShowInTaskbar = false;

                _panel.Controls.Add(form);
                _panel.Tag = form;

                // FORZAR render
                form.Show();
                form.BringToFront();
                form.Refresh();
                _panel.Refresh();

                if (_lblTitulo != null) _lblTitulo.Text = titulo ?? _lblTitulo.Text;
                if (_lblDescripcion != null) _lblDescripcion.Text = descripcion ?? _lblDescripcion.Text;
            }
            catch (Exception ex)
            {
                ShowOpenError(ex);
            }
            finally
            {
                try
                {
                    _panel.ResumeLayout(true);
                    _panel.PerformLayout();
                }
                catch { }
            }
        }

        private static void ShowOpenError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("No se pudo abrir el formulario.");
            sb.AppendLine();
            sb.AppendLine("Mensaje:");
            sb.AppendLine(ex.Message);

            if (ex.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("InnerException:");
                sb.AppendLine(ex.InnerException.Message);
            }

            MessageBox.Show(sb.ToString(), "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

using System;
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
        }

        public void Open(Form form, string titulo = null, string descripcion = null)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            if (_activo != null)
            {
                _activo.Close();
                _activo.Dispose();
                _activo = null;
            }

            _activo = form;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            _panel.Controls.Clear();
            _panel.Controls.Add(form);
            _panel.Tag = form;
            form.BringToFront();
            form.Show();

            if (_lblTitulo != null)
                _lblTitulo.Text = string.IsNullOrWhiteSpace(titulo) ? form.Text : titulo;

            if (_lblDescripcion != null && descripcion != null)
                _lblDescripcion.Text = descripcion;
        }
    }
}
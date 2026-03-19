using System;
using System.Collections.Generic;
using System.Linq;
using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Facturacion;
using Dominio_SISV.Services.Facturacion;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Facturacion.Consulta
{
    public sealed class FacturaConsultaPresenter
    {
        private readonly IFacturaConsultaView _view;
        private readonly IFacturaConsultaService _service;

        private int? _selectedId;
        private string _selectedCodigo;

        private List<FacturaConsultaCardVM> _last = new List<FacturaConsultaCardVM>();

        public FacturaConsultaPresenter(IFacturaConsultaView view, IFacturaConsultaService service)
        {
            _view = view;
            _service = service;
        }

        private PermissionContext Perms
        {
            get
            {
                return new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
        }
        private bool CanConsultar()
        {
            // Para listar/ver detalle: con Consultar (o Anular, que en la práctica implica consultar)
            return Perms.HasAny("BILL_FACTURA_CONSULTAR", "BILL_FACTURA_ANULAR");
        }

        private bool CanEmitir() { return Perms.Has("BILL_FACTURA_EMITIR"); }
        private bool CanAnular() { return Perms.Has("BILL_FACTURA_ANULAR"); }

        public void LoadList()
        {
            _last = _service.Buscar(_view.TextoBusqueda, _view.EstadoFiltro, 200) ?? new List<FacturaConsultaCardVM>();

            if (_selectedId.HasValue && !_last.Any(x => x.FacturaID == _selectedId.Value))
            {
                _selectedId = null;
                _selectedCodigo = null;
                _view.SetSeleccion("--");
                _view.ShowDetallePanel(false);
            }

            _view.SetResultados(_last.Count);
            _view.RenderCards(_last, _selectedId);

            ApplyButtonsState();
        }

        private void ApplyButtonsState()
        {
            if (!_selectedId.HasValue || _selectedId.Value <= 0)
            {
                _view.SetAccionesEnabled(false, false);
                _view.SetTextoBtnAnular("Anular factura");
                return;
            }

            var selected = _last.FirstOrDefault(x => x.FacturaID == _selectedId.Value);

            bool canVerDetalle = CanConsultar();
            bool isAnulada = selected != null && selected.IsAnulada;

            bool canAccion;
            string texto;

            if (isAnulada)
            {
                // Cuando está anulada, el botón se usa para ir a Emitir
                canAccion = CanEmitir();
                texto = canAccion ? "Emitir factura" : "Factura anulada";
            }
            else
            {
                canAccion = CanAnular();
                texto = "Anular factura";
            }

            _view.SetAccionesEnabled(canVerDetalle, canAccion);
            _view.SetTextoBtnAnular(texto);
        }

        public void SelectFactura(int id, string codigo)
        {
            _selectedId = id;
            _selectedCodigo = codigo;

            _view.SetSeleccion(string.IsNullOrWhiteSpace(codigo) ? "--" : codigo);
            _view.ClearMotivo();
            _view.ShowDetallePanel(false);

            LoadList();
        }

        public void VerDetalle()
        {
            if (!CanConsultar())
            {
                _view.ShowWarn("No tiene permisos para consultar facturas.");
                _view.ShowDetallePanel(false);
                ApplyButtonsState();
                return;
            }

            if (!_selectedId.HasValue || _selectedId.Value <= 0)
            {
                _view.ShowWarn("Seleccione una factura para ver el detalle.");
                _view.ShowDetallePanel(false);
                ApplyButtonsState();
                return;
            }

            _view.ShowDetallePanel(true);

            var ds = _service.GetReporteDataSet(_selectedId.Value);
            _view.RenderReporte(ds);

            ApplyButtonsState();
        }

        public void VolverDetalle() => _view.ShowDetallePanel(false);

        public void AccionAnularOBotonEmitir()
        {
            if (!_selectedId.HasValue || _selectedId.Value <= 0)
            {
                _view.ShowWarn("Seleccione una factura.");
                return;
            }

            var selected = _last.FirstOrDefault(x => x.FacturaID == _selectedId.Value);

            // si está anulada -> emitir factura
            if (selected != null && selected.IsAnulada)
            {
                if (!CanEmitir())
                {
                    _view.ShowWarn("No tiene permisos para emitir facturas.");
                    ApplyButtonsState();
                    return;
                }
                _view.IrAEmitirFactura();
                return;
            }

            if (!CanAnular())
            {
                _view.ShowWarn("No tiene permisos para anular facturas.");
                ApplyButtonsState();
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.MotivoAnulacion))
            {
                _view.ShowWarn("Ingrese el motivo de la anulación.");
                return;
            }

            string cod = string.IsNullOrWhiteSpace(_selectedCodigo) ? "" : $" ({_selectedCodigo})";
            if (!_view.Confirm("¿Desea anular la factura" + cod + "?", "Confirmar anulación"))
                return;

            _service.Anular(_view.UsuarioId, _selectedId.Value, _view.MotivoAnulacion);
            _view.ShowInfo("Factura anulada.");

            LoadList();

            _view.ShowDetallePanel(true);
            var ds = _service.GetReporteDataSet(_selectedId.Value);
            _view.RenderReporte(ds);
        }
    }
}
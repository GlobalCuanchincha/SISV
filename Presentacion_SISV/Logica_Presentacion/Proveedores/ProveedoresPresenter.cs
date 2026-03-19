using System;
using System.Collections.Generic;
using Dominio_SISV.DTOs;
using Dominio_SISV.Services.Proveedores;

namespace Union_Formularios_SISV.Logica_Presentacion.Proveedores
{
    public sealed class ProveedoresPresenter
    {
        private readonly IProveedoresView _view;
        private readonly IProveedorService _service;

        private int? _selectedId = null;

        public ProveedoresPresenter(IProveedoresView view, IProveedorService service)
        {
            _view = view;
            _service = service;
        }

        public void CargarLista()
        {
            try
            {
                if (!_view.PuedeAcceder)
                {
                    _view.SetResultados(0);
                    _view.RenderCards(new List<ProveedorDetalleVM>(), null);
                    _view.SetGuardarHabilitado(false);
                    return;
                }

                int usuarioId = _view.UsuarioId;
                if (usuarioId <= 0)
                {
                    _view.SetResultados(0);
                    _view.RenderCards(new List<ProveedorDetalleVM>(), null);
                    _view.SetGuardarHabilitado(false);
                    return;
                }

                string texto = _view.TextoBusqueda;
                string filtro = _view.FiltroTexto;
                string estado = _view.EstadoFiltroTexto;
                if (string.Equals(estado, "Todos", StringComparison.OrdinalIgnoreCase)) estado = null;

                var list = _service.Buscar(usuarioId, texto, filtro, estado, 200);

                _view.SetResultados(list.Count);
                _view.RenderCards(list, _selectedId);

                bool puedeGuardar = _selectedId.HasValue ? _view.PuedeActualizar : _view.PuedeRegistrar;
                _view.SetGuardarHabilitado(puedeGuardar);
            }
            catch (Exception ex)
            {
                _view.ShowError("Error cargando proveedores.", ex);
            }
        }

        public void Seleccionar(int proveedorId)
        {
            try
            {
                if (!_view.PuedeAcceder)
                {
                    _view.ShowWarning("No tiene permisos para consultar proveedores.");
                    return;
                }

                int usuarioId = _view.UsuarioId;
                var det = _service.GetById(usuarioId, proveedorId);
                if (det == null) return;

                _selectedId = proveedorId;

                _view.SetSelectedLabel(det.Nombre ?? "--");
                _view.ShowDetalle(det);
                _view.SetModoActualizar(true);
                _view.SetGuardarHabilitado(_view.PuedeActualizar);

                CargarLista();
            }
            catch (Exception ex)
            {
                _view.ShowError("Error cargando detalle del proveedor.", ex);
            }
        }

        public void Guardar()
        {
            try
            {
                bool esNuevo = !_selectedId.HasValue;

                if (esNuevo && !_view.PuedeRegistrar)
                {
                    _view.ShowWarning("No tiene permisos para registrar proveedores.");
                    return;
                }

                if (!esNuevo && !_view.PuedeActualizar)
                {
                    _view.ShowWarning("No tiene permisos para actualizar proveedores.");
                    return;
                }

                int usuarioId = _view.UsuarioId;
                var input = _view.ReadForm();
                var saved = _service.Guardar(usuarioId, _selectedId, input);

                if (saved == null)
                {
                    _view.ShowWarning("No se pudo guardar.");
                    return;
                }

                _selectedId = saved.ProveedorId;

                _view.ShowInfo("Guardado correctamente.");
                _view.SetSelectedLabel(saved.Nombre ?? "--");
                _view.ShowDetalle(saved);
                _view.SetModoActualizar(true);
                _view.SetGuardarHabilitado(_view.PuedeActualizar);

                CargarLista();
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }

        public void Limpiar()
        {
            _selectedId = null;
            _view.SetSelectedLabel("--");
            _view.SetModoActualizar(false);
            _view.ClearForm();
            _view.SetGuardarHabilitado(_view.PuedeRegistrar);
            CargarLista();
        }
    }
}
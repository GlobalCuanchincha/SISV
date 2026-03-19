using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Services.Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Servicio
{
    public sealed class ServicioPresenter
    {
        private const string P_ACCESO = "OPS_SERVICIOS_ACCESO";
        private const string P_REG = "OPS_SERVICIOS_REGISTRAR";
        private const string P_UPD = "OPS_SERVICIOS_ACTUALIZAR";
        private const string P_DES = "OPS_SERVICIOS_DESACTIVAR";

        private readonly IServicioView _view;
        private readonly IServicioService _service;
        private readonly PermissionContext _perm;

        private DataTable _dtServicios;
        private int? _servicioIdSeleccionado;
        private string _codigoSeleccionado;

        public ServicioPresenter(IServicioView view, IServicioService service = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _service = service ?? new ServicioService();
            _perm = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public void Initialize()
        {
            if (_view.UsuarioId <= 0)
            {
                _view.ShowWarning("No se pudo obtener UsuarioID de sesión.");
                _view.CloseView();
                return;
            }

            if (!_perm.HasAny(P_ACCESO, P_REG, P_UPD, P_DES))
            {
                _view.ShowWarning("Acceso denegado. No tiene permisos para Servicios.");
                _view.CloseView();
                return;
            }

            CargarCombos();
            Limpiar();
            Buscar();
        }

        public void Buscar()
        {
            try
            {
                int usuarioId = _view.UsuarioId;
                string texto = (_view.TextoBusqueda ?? "").Trim();
                int? categoriaId = _view.CategoriaFiltroId;

                string estadoUi = (_view.EstadoFiltroTexto ?? "Todos").Trim();
                string estado =
                    estadoUi.Equals("Activos", StringComparison.OrdinalIgnoreCase) ? "activos" :
                    estadoUi.Equals("Inactivos", StringComparison.OrdinalIgnoreCase) ? "inactivos" : "todos";

                _dtServicios = _service.Buscar(usuarioId, texto, categoriaId, estado) ?? new DataTable();

                _view.RenderServicios(_dtServicios, _servicioIdSeleccionado);
                _view.SetResultados(_dtServicios.Rows.Count);

                if (_servicioIdSeleccionado.HasValue)
                {
                    bool existe = _dtServicios.AsEnumerable()
                        .Any(r => SafeInt(r, "ServicioID") == _servicioIdSeleccionado.Value);

                    if (!existe)
                        Deseleccionar(false);
                }

                ActualizarAcciones();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de servicios.", ex);
            }
        }

        public void Seleccionar(int servicioId)
        {
            if (_dtServicios == null) return;

            var row = _dtServicios.AsEnumerable()
                .FirstOrDefault(r => SafeInt(r, "ServicioID") == servicioId);

            if (row == null) return;

            _servicioIdSeleccionado = servicioId;
            _codigoSeleccionado = SafeString(row, "Codigo");

            _view.SetCodigoLabel(string.IsNullOrWhiteSpace(_codigoSeleccionado) ? "--" : _codigoSeleccionado);
            _view.CodigoServicio = _codigoSeleccionado ?? "";
            _view.NombreServicio = SafeString(row, "Nombre");
            _view.PrecioServicio = SafeDecimal(row, "Precio");
            _view.ActivoServicio = SafeBool(row, "Activo");

            int catId = SafeInt(row, "CategoriaServicioID");
            if (catId > 0)
            {
                // La view toma el valor desde el combo con DataSource ya cargado
                // y lo setea al renderizar la selección.
            }

            _view.SetModoActualizar(true);
            _view.RenderServicios(_dtServicios, _servicioIdSeleccionado);
            ActualizarAcciones();
        }

        public void AplicarSeleccionAlComboCategoria(int categoriaId)
        {
            // helper opcional si quieres dispararlo después del bind
        }

        public void Guardar()
        {
            try
            {
                bool esNuevo = !_servicioIdSeleccionado.HasValue;

                if (esNuevo && !_perm.Has(P_REG))
                {
                    _view.ShowWarning("No tiene permisos para registrar servicios.");
                    return;
                }

                if (!esNuevo && !_perm.Has(P_UPD))
                {
                    _view.ShowWarning("No tiene permisos para actualizar servicios.");
                    return;
                }

                int usuarioId = _view.UsuarioId;
                int categoriaId = _view.CategoriaServicioId;
                string codigo = (_view.CodigoServicio ?? "").Trim();
                string nombre = (_view.NombreServicio ?? "").Trim();
                decimal precio = _view.PrecioServicio;
                bool activo = _view.ActivoServicio;

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    _view.ShowWarning("El código del servicio es obligatorio.");
                    return;
                }

                if (categoriaId <= 0)
                {
                    _view.ShowWarning("Selecciona una categoría.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _view.ShowWarning("El nombre del servicio es obligatorio.");
                    _view.FocusNombre();
                    return;
                }

                var res = _service.Guardar(
                    usuarioId,
                    _servicioIdSeleccionado,
                    codigo,
                    categoriaId,
                    nombre,
                    precio,
                    activo
                );

                _servicioIdSeleccionado = res.ServicioID > 0 ? (int?)res.ServicioID : _servicioIdSeleccionado;
                _codigoSeleccionado = string.IsNullOrWhiteSpace(res.Codigo) ? codigo : res.Codigo;

                Buscar();
                if (string.IsNullOrWhiteSpace(codigo))
                    throw new InvalidOperationException("El código del servicio es obligatorio.");
                if (_servicioIdSeleccionado.HasValue)
                {
                    var row = _dtServicios?.AsEnumerable()
                        .FirstOrDefault(r => SafeInt(r, "ServicioID") == _servicioIdSeleccionado.Value);

                    if (row != null)
                    {
                        _view.SetCodigoLabel(string.IsNullOrWhiteSpace(_codigoSeleccionado) ? "--" : _codigoSeleccionado);
                        _view.CodigoServicio = _codigoSeleccionado ?? "";
                        _view.NombreServicio = SafeString(row, "Nombre");
                        _view.PrecioServicio = SafeDecimal(row, "Precio");
                        _view.ActivoServicio = SafeBool(row, "Activo");
                        _view.SetModoActualizar(true);
                        _view.RenderServicios(_dtServicios, _servicioIdSeleccionado);
                    }
                }

                _view.ShowInfo("Servicio guardado correctamente.");
                ActualizarAcciones();
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }
        public void Desactivar()
        {
            try
            {
                if (!_perm.Has(P_DES))
                {
                    _view.ShowWarning("No tiene permisos para desactivar servicios.");
                    return;
                }

                if (!_servicioIdSeleccionado.HasValue)
                {
                    _view.ShowWarning("Selecciona un servicio para desactivarlo.");
                    return;
                }

                _service.Desactivar(_view.UsuarioId, _servicioIdSeleccionado.Value);

                _view.ShowInfo("Servicio desactivado.");
                Buscar();
                Limpiar();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo desactivar el servicio.", ex);
            }
        }

        public void Limpiar()
        {
            _view.ClearFormInputs();
            Deseleccionar(true);
            CargarCodigoSiguiente();
            _view.SetModoActualizar(false);
            ActualizarAcciones();
        }

        private void CargarCombos()
        {
            int usuarioId = _view.UsuarioId;
            var dtCat = _service.ListarCategorias(usuarioId);

            var dtBind = new DataTable();
            dtBind.Columns.Add("CategoriaServicioID", typeof(int));
            dtBind.Columns.Add("Categoria", typeof(string));
            dtBind.Rows.Add(0, "Todos");

            if (dtCat != null)
            {
                foreach (DataRow r in dtCat.Rows)
                {
                    int id = SafeInt(r, "CategoriaServicioID");
                    string nom = SafeString(r, "Categoria");

                    if (id > 0 && !string.IsNullOrWhiteSpace(nom))
                    {
                        bool ya = dtBind.AsEnumerable()
                            .Any(x => SafeInt(x, "CategoriaServicioID") == id);

                        if (!ya) dtBind.Rows.Add(id, nom);
                    }
                }
            }

            _view.BindCategorias(dtBind);
        }

        private void CargarCodigoSiguiente()
        {
            try
            {
                if (_servicioIdSeleccionado.HasValue) return;
                _view.CodigoServicio = _service.GetNextCodigo(_view.UsuarioId);
            }
            catch
            {
                _view.CodigoServicio = "";
            }
        }

        private void Deseleccionar(bool soloLabel)
        {
            _servicioIdSeleccionado = null;
            _codigoSeleccionado = null;
            _view.SetCodigoLabel("--");

            if (!soloLabel)
                _view.RenderServicios(_dtServicios, null);
        }

        private void ActualizarAcciones()
        {
            bool editando = _servicioIdSeleccionado.HasValue;

            _view.SetGuardarEnabled(editando ? _perm.Has(P_UPD) : _perm.Has(P_REG));
            _view.SetDesactivarEnabled(editando && _perm.Has(P_DES));
        }

        private static int SafeInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private static decimal SafeDecimal(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0m;
            decimal d; return decimal.TryParse(Convert.ToString(r[col]), out d) ? d : 0m;
        }

        private static bool SafeBool(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return false;
            bool b;
            if (bool.TryParse(Convert.ToString(r[col]), out b)) return b;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) && x != 0;
        }

        private static string SafeString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return "";
            return Convert.ToString(r[col]) ?? "";
        }
    }
}
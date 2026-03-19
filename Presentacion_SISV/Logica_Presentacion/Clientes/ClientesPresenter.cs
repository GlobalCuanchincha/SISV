using Dominio_SISV.DTOs.Clientes;
using Dominio_SISV.Services.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Clientes
{
    public sealed class ClientesPresenter
    {
        private const string P_ACCESO = "CRM_CLIENTES_ACCESO";
        private const string P_REG = "CRM_CLIENTES_REGISTRAR";
        private const string P_UPD = "CRM_CLIENTES_ACTUALIZAR";

        private readonly IClientesView _view;
        private readonly IClienteService _service;
        private readonly PermissionContext _perm;

        private string _selectedCedula;
        private List<ClienteCardVM> _lastList = new List<ClienteCardVM>();

        public ClientesPresenter(IClientesView view, IClienteService service, PermissionContext perm)
        {
            _view = view;
            _service = service;
            _perm = perm;
        }

        public void Initialize()
        {
            if (_perm != null && !_perm.TryEnsure(P_ACCESO, "No tiene permiso para acceder a Clientes."))
                return;

            var filtros = new List<KeyValuePair<string, string>>();
            filtros.Add(new KeyValuePair<string, string>("nombre", "Nombre (nombres+apellidos)"));
            filtros.Add(new KeyValuePair<string, string>("cedula", "Cédula"));
            filtros.Add(new KeyValuePair<string, string>("email", "Email"));
            filtros.Add(new KeyValuePair<string, string>("telefono", "Teléfono"));
            filtros.Add(new KeyValuePair<string, string>("direccion", "Dirección"));
            filtros.Add(new KeyValuePair<string, string>("apellidos", "Apellidos"));

            _view.BindFiltroPor(filtros, "nombre");

            var estados = _service.ListarEstados();

            int? defaultKey = null;
            var activo = estados.FirstOrDefault(x =>
                (x.EstadoNombre ?? "").ToLower().Contains("activo") &&
                !(x.EstadoNombre ?? "").ToLower().Contains("inactivo"));

            if (activo != null) defaultKey = activo.EstadoKey;

            _view.BindEstados(estados, defaultKey);

            var filtroEstados = new List<ClienteEstadoVM>();
            filtroEstados.Add(new ClienteEstadoVM { EstadoKey = null, EstadoNombre = "Todos", EsActivo = null });
            foreach (var e in estados) filtroEstados.Add(e);

            _view.BindEstadosFiltro(filtroEstados);

            _view.ClearSelectionAndForm();
            Search();
        }

        public void Search()
        {
            var filtroPor = _view.FiltroPorKey ?? "nombre";
            var buscar = (_view.BuscarTexto ?? "").Trim();
            var estadoKey = _view.EstadoFiltroKey;

            var list = _service.Buscar(filtroPor, buscar, estadoKey, 200);
            _lastList = list ?? new List<ClienteCardVM>();

            _view.SetResultados(_lastList.Count);
            _view.ShowClientes(_lastList, _selectedCedula);

            if (!string.IsNullOrWhiteSpace(_selectedCedula))
            {
                bool exists = _lastList.Any(x => string.Equals(x.Cedula, _selectedCedula, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                    ClearSelectionOnly();
            }
        }

        public void EnterPressed()
        {
            Search();

            if (string.Equals(_view.FiltroPorKey, "cedula", StringComparison.OrdinalIgnoreCase) && _lastList.Count == 0)
                _view.ShowWarning("El usuario no existe.");
        }

        public void SelectFromCard(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return;
            _selectedCedula = cedula;
            LoadDetalle(cedula);
        }

        private void LoadDetalle(string cedula)
        {
            var det = _service.GetByCedula(cedula);
            if (det == null)
            {
                _view.ShowWarning("El usuario no existe.");
                _view.ClearSelectionAndForm();
                _selectedCedula = null;
                return;
            }

            _view.SetSelectedLabel("Seleccionado: " + (det.Cedula ?? ""));
            _view.ShowDetalle(det);
            _view.SetCedulaReadOnly(true);

            _view.SetActualizarEnabled(true);
        }

        public void Registrar()
        {
            if (_perm != null && !_perm.TryEnsure(P_REG, "No tiene permiso para registrar clientes."))
                return;

            var input = _view.ReadForm();
            if (input == null) return;

            if (string.IsNullOrWhiteSpace(input.Cedula))
            {
                _view.ShowWarning("Error: Ingrese la cédula.");
                return;
            }
            if (string.IsNullOrWhiteSpace(input.Nombres) || string.IsNullOrWhiteSpace(input.Apellidos))
            {
                _view.ShowWarning("Error: Campos incompletos (nombres/apellidos).");
                return;
            }

            try
            {
                var creado = _service.Crear(input);
                _view.ShowInfo("Guardado correctamente.");

                _selectedCedula = (creado != null && !string.IsNullOrWhiteSpace(creado.Cedula)) ? creado.Cedula : input.Cedula;

                _view.SetBusqueda("cedula", _selectedCedula);
                Search();
                LoadDetalle(_selectedCedula);
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }

        public void Actualizar()
        {
            if (_perm != null && !_perm.TryEnsure(P_UPD, "No tiene permiso para actualizar clientes."))
                return;

            if (string.IsNullOrWhiteSpace(_selectedCedula))
            {
                _view.ShowWarning("Seleccione un cliente para actualizar.");
                return;
            }

            var input = _view.ReadForm();
            if (input == null) return;

            if (string.IsNullOrWhiteSpace(input.Nombres) || string.IsNullOrWhiteSpace(input.Apellidos))
            {
                _view.ShowWarning("Campos incompletos.");
                return;
            }

            try
            {
                _service.Actualizar(_selectedCedula, input);
                _view.ShowInfo("Actualizado correctamente.");

                Search();
                LoadDetalle(_selectedCedula);
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }

        public void Limpiar()
        {
            ClearSelectionOnly();
            _view.ClearSelectionAndForm();
            Search();
        }

        private void ClearSelectionOnly()
        {
            _selectedCedula = null;
            _view.SetSelectedLabel("Sin seleccionar");
            _view.SetCedulaReadOnly(false);
            _view.SetActualizarEnabled(false);
        }
    }
}
using System.Collections.Generic;
using System.Data;
using Dominio_SISV.DTOs.Facturacion;

namespace Union_Formularios_SISV.Logica_Presentacion.Facturacion.Consulta
{
    public interface IFacturaConsultaView
    {
        int UsuarioId { get; }
        string TextoBusqueda { get; }
        string EstadoFiltro { get; }
        string MotivoAnulacion { get; }

        void RenderCards(List<FacturaConsultaCardVM> items, int? selectedId);
        void SetResultados(int total);
        void SetSeleccion(string codigoFacturaOrDash);

        void ShowDetallePanel(bool visible);
        void RenderReporte(DataSet ds);

        void SetAccionesEnabled(bool verDetalleEnabled, bool anularEnabled);
        void SetTextoBtnAnular(string texto);     
        void IrAEmitirFactura();                 

        void ClearMotivo();
        bool Confirm(string message, string title);

        void ShowInfo(string msg);
        void ShowWarn(string msg);
        void ShowError(string msg);
    }
}
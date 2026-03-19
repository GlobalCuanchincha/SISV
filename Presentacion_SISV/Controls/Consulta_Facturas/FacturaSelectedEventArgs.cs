using System;

namespace Union_Formularios_SISV.Controls.Consulta_Facturas
{
    public sealed class FacturaSelectedEventArgs : EventArgs
    {
        public int FacturaID { get; }
        public string CodigoFactura { get; }

        public FacturaSelectedEventArgs(int facturaId, string codigoFactura)
        {
            FacturaID = facturaId;
            CodigoFactura = codigoFactura;
        }
    }
}
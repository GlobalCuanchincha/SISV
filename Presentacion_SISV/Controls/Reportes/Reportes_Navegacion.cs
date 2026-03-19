using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Union_Formularios_SISV.Controls
{
    public enum ReporteVista
    {
        Cliente,
        Inventario,
        Servicio
    }

    public interface IReporteNavegable
    {
        event Action<ReporteVista> NavegacionSolicitada;
    }
}

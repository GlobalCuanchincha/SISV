using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Usuarios.Permisos
{
    public class PermissionContext
    {
        private readonly HashSet<string> _codes;

        public PermissionContext(HashSet<string> codes)
        {
            _codes = codes ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool Has(string code)
            => !string.IsNullOrWhiteSpace(code) && _codes.Contains(code);

        public bool HasAny(params string[] codes)
        {
            if (codes == null) return false;
            foreach (var c in codes)
                if (Has(c)) return true;
            return false;
        }

        public void Ensure(string code, string msg = "No tiene permisos para esta acción.")
        {
            if (!Has(code))
                throw new InvalidOperationException(msg);
        }

        public bool TryEnsure(string code, string msg)
        {
            if (Has(code)) return true;
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}

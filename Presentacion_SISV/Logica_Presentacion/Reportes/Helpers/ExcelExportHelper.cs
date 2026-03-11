using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes.Helpers
{
    public static class ExcelExportHelper
    {
        public static void Exportar(DataTable dt, string nombreBase)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = (nombreBase ?? "Reporte_Clientes") + "_" + DateTime.Today.ToString("yyyyMMdd") + ".xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Clientes");

                    // Headers
                    for (int c = 0; c < dt.Columns.Count; c++)
                        ws.Cell(1, c + 1).Value = dt.Columns[c].ColumnName;

                    // Rows
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var cell = ws.Cell(r + 2, c + 1);
                            object val = dt.Rows[r][c];

                            if (val == null || val == DBNull.Value)
                            {
                                cell.Value = "";
                            }
                            else if (val is DateTime)
                            {
                                cell.Value = (DateTime)val;
                            }
                            else if (val is bool)
                            {
                                cell.Value = (bool)val;
                            }
                            else if (val is byte || val is sbyte || val is short || val is ushort ||
                                     val is int || val is uint || val is long || val is ulong)
                            {
                                cell.Value = Convert.ToInt64(val);
                            }
                            else if (val is float || val is double || val is decimal)
                            {
                                cell.Value = Convert.ToDouble(val);
                            }
                            else
                            {
                                cell.Value = val.ToString();
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(sfd.FileName);
                }

                MessageBox.Show("Excel exportado.", "Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
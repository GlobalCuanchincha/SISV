using System;
using System.Data;
using System.Windows.Forms;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes.Helpers
{
    public static class PdfExportHelper
    {
        public static void Exportar(DataTable dt, string titulo)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = (titulo ?? "Reporte_Clientes") + "_" + DateTime.Today.ToString("yyyyMMdd") + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var doc = new Document();
                doc.Info.Title = titulo ?? "Reporte Clientes";

                var sec = doc.AddSection();
                sec.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape; // ✅ sin ambigüedad

                var p = sec.AddParagraph(titulo ?? "Reporte Clientes");
                p.Format.Font.Size = 14;
                p.Format.Font.Bold = true;
                p.Format.SpaceAfter = "0.3cm";

                var table = sec.AddTable();
                table.Borders.Width = 0.5;

                for (int i = 0; i < dt.Columns.Count; i++)
                    table.AddColumn(Unit.FromCentimeter(3.2));

                var header = table.AddRow();
                header.Shading.Color = Colors.LightGray;
                header.Format.Font.Bold = true;

                for (int i = 0; i < dt.Columns.Count; i++)
                    header.Cells[i].AddParagraph(dt.Columns[i].ColumnName);

                foreach (DataRow row in dt.Rows)
                {
                    var tr = table.AddRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                        tr.Cells[i].AddParagraph(Convert.ToString(row[i]));
                }

                // ✅ sin constructor obsoleto
                var renderer = new PdfDocumentRenderer();
                renderer.Document = doc;
                renderer.RenderDocument();
                renderer.PdfDocument.Save(sfd.FileName);

                MessageBox.Show("PDF exportado.", "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
using Datos_Acceso.Repositories;
using Dominio_SISV.DTOs;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Clientes
{
    public partial class Control_Proveedores_UC : Form
    {
        private readonly SISVInventarioRepository _repo;
        private readonly Timer _t = new Timer { Interval = 250 };

        public ProveedorPickVM SelectedProveedor { get; private set; }

        public Control_Proveedores_UC(SISVInventarioRepository repo)
        {
            InitializeComponent();

           
        }
    }
}

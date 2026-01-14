using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV
{
    public partial class Form_Panel_Principal : Form
    {
        private readonly Union_Formularios_SISV.LoginSession _session;

        public Form_Panel_Principal(Union_Formularios_SISV.LoginSession session)
        {
            InitializeComponent();
            _session = session;
        }

        private void Form_Panel_Principal_Load(object sender, EventArgs e)
        {
           
        }
    }

}

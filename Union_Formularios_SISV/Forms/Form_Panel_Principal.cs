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
        private readonly LoginSession _session;

        public Form_Panel_Principal(LoginSession session)
        {
            InitializeComponent();
            _session = session;
            lblUser.Text = _session.Username;
        }
    }

}

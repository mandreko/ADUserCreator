using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADUserCreator
{
    public partial class Report : Form
    {
        public object DataSource
        {
            get
            {
                return dgvData.DataSource;
            }
            set
            {
                dgvData.DataSource = value;
            }
        }

        public Report()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

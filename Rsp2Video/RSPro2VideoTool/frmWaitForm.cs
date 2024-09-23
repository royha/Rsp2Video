using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSPro2VideoTool
{
    public partial class frmWaitForm : Form
    {
        public frmWaitForm()
        {
            InitializeComponent();

            // this.StartPosition = FormStartPosition.CenterParent;
        }

        public frmWaitForm(Form parent)
        {
            this.Owner = parent;

            InitializeComponent();
        }

        private void frmWaitForm_Activated(object sender, EventArgs e)
        {
            this.CenterToParent(); ;
        }
    }
}

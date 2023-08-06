using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rsp2Video
{
    public partial class ParseError : Form
    {
        public ParseError()
        {
            InitializeComponent();
        }

        public ParseError(String ParseErrorString)
        {
            InitializeComponent();
            textBox1.Text = ParseErrorString;
        }
    }
}

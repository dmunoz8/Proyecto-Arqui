using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Proyecto_Arqui
{
    public partial class Datos : Form
    {
        private string path;

        public Datos()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void Direccion_Click(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = buscar.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                string file = buscar.FileName;
                path = new FileInfo(file).Directory.FullName;               
            }
        }

        private void SiDatos_Click(object sender, EventArgs e)
        {
            Procesador MIPS = new Procesador();
            this.Visible = false;
            MIPS.Visible = true;
            MIPS.comenzar(path);
           
        }

        private void NoDatos_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

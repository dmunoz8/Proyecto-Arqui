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
    public partial class Procesador : Form
    {
        public Procesador()
        {
            InitializeComponent();
        }

        private void P3_Enter(object sender, EventArgs e)
        {

        }

        public void comenzar(string path)
        {
            BindingList<int> data = new BindingList<int>();   
           
            try
            {
                foreach (string files in Directory.EnumerateFiles(path, "*.txt"))
                {
                    string contents = File.ReadAllText(files);
                    int valor = Int32.Parse(contents);
                    data.Add(valor);
                    CD1.DataSource = data;
                }
            }

            catch (IOException)
            {
            }
        }

        private void Procesador_FormClosing(object sender, EventArgs e)
        {
            
        }
    }
}
    
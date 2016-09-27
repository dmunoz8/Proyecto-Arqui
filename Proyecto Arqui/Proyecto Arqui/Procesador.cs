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
        private int[,] memoria; //64*4
        private int[] registros; //32 (R0 - R31)
        private int[] contexto; //34 (R0 - R31 + RL + PC)
        private int[] cacheDatos; //
        private int[] cacheInstrucciones;
        private Queue<int> hilillos;
        private int quantum;
        private int reloj;
        private int pc;
        public Procesador()
        {
            InitializeComponent();            
            hilillos = new Queue<int>();
            quantum = 0;
            reloj = 0;
            pc = 0;
        }

        private void P3_Enter(object sender, EventArgs e)
        {

        }

        public void inicializar()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    memoria[1, j] = 1;
                }
            }

            for (int i = 0; i < 32; i++)
            {
                registros[i] = 0;
            }

            for (int i = 0; i < 20; i++)
            {
                cacheInstrucciones[i] = 0;
            }

            cacheInstrucciones[20] = -1;
            cacheInstrucciones[21] = -1;
            cacheInstrucciones[22] = -1;
            cacheInstrucciones[23] = -1;

            for (int i = 0; i < 24; i++)
            {
                cacheDatos[i] = 0;
            }
        }

        public void cargarInstrucciones(string path)
        {
            BindingList<int> data = new BindingList<int>();   
           
            try
            {
                int posicion = 0;
                foreach (string files in Directory.EnumerateFiles(path, "*.txt"))
                {
                    string contents = File.ReadAllText(files);
                    string[] instrucciones = contents.Split('\n');
                    foreach (string instruccion in instrucciones)
                    {
                        string[] codigos = instruccion.Split(' ');
                        for (int i = 0; i < 4; i++)
                        {
                            memoria[posicion, i] = Int32.Parse(codigos[i]);
                        }
                    }
                    posicion++;
                }
                //int valor = Int32.Parse(contents);
                //    data.Add(valor);
                //    CD1.DataSource = data;
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
    
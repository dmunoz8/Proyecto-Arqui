﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Arqui
{
    public partial class Resultados : Form
    {
        Organizador org;
        public Resultados(Organizador organizado)
        {
            InitializeComponent();
            org = organizado;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Resultados_Load(object sender, EventArgs e)
        {

        }

        /*Imprime los resultados finales de cada hilo que se ejecuto, registros, caches, ciclos de duración y tambien el estado de la memoria
         * REQ: N/A
         * RES: N/A
         */ 
        public void imprimir()
        {
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    mem.Text += org.memoriaDatos[i, j].ToString() + " ";
                }
                mem.Text += '\n';
            }

            /*for (int i = 24; i < 64; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    mem.Text += org.memoria[i, j].ToString() + " ";
                }
                mem.Text += '\n';
            }*/
            ciclos.Text = "Reloj final: " + org.reloj+"\n";
            while (org.terminados.Count > 0)
            {
                int[] ter = org.terminados.Dequeue();
                for (int g = 0; g < 32; g++)
                {
                    regs.Text += ter[g] + " ";
                }
                regs.Text += '\n';
                regs.Text += '\n';

                for (int g = 32; g < 56; g++)
                {
                    datos.Text += ter[g] + " ";
                }

                datos.Text += '\n';
                datos.Text += '\n';

                ciclos.Text += ter[56];
                ciclos.Text += '\n';
            }
            Visible = true;
        }
    }
}

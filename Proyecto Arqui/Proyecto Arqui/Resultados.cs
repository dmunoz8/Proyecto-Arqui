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
        public void imprimir()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    mem.Text += org.memoria[i, j].ToString() + " ";
                }
                mem.Text += '\n';
            }

            Visible = true;
        }

    }
}
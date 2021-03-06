﻿using System;
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

        public int _quantum;
        public int _hilos;


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

        /*Se encarga de obtener el path cuando el usuario busca los txt
         * REQ: object, EventArgs
         * RES: N/A
         */ 
        private void Direccion_Click(object sender, EventArgs e)
        {
            DialogResult result = buscar.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                string file = buscar.FileName;
                path = new FileInfo(file).Directory.FullName;               
            }
        }
    
        private void SiDatos_Click(object sender, EventArgs e)
        {

            _hilos = Int32.Parse(hilos.Text);
            _quantum = Int32.Parse(quantum.Text);

            //this.Visible = false;
            //this.Close();
            bool lento = false;
            lento = ModoLentoCheck.Checked;
            Organizador organizador = new Organizador(_hilos,_quantum, path,lento);
            organizador.inicializaProcesadores();
            if (lento)
            {
                organizador.imprimirDatos();
            }
            else
            {
                organizador.sincronizarReloj();
                Resultados resultado = new Resultados(organizador);
                resultado.imprimir();
            }
            //organizador.eliminarHilos();


            /*Procesador MIPS = new Procesador();
             this.Visible = false;
             MIPS.Visible = true;
             MIPS.inicializar();
             MIPS.cargarInstrucciones(path); 
            */

        }

        private void NoDatos_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hilos_TextChanged(object sender, EventArgs e)
        {

        }

        private void Datos_Load(object sender, EventArgs e)
        {

        }
    }
}

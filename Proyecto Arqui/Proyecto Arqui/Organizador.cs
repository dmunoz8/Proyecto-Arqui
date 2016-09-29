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
using System.Threading;

namespace Proyecto_Arqui
{
    public class Organizador
    {
        int _cantHilillos;
        int _quantum;
        int reloj;
        Procesador procesador1;
        Procesador procesador2;
        Procesador procesador3;
        public Barrier sincronizacion;

        public Organizador(int cantHilillos, int quantum, string path)
        {
            _cantHilillos = cantHilillos;
            _quantum = quantum;
            sincronizacion = new Barrier(4);

            procesador1 = new Procesador(1, sincronizacion);
            procesador2 = new Procesador(2, sincronizacion);
            procesador3 = new Procesador(3, sincronizacion);

            cargarMemoria(path);
            inicializaProcesadores();

        }

        //Para cada procesador carga en memoria las instrucciones contenidas en los txt
        public void cargarMemoria(string path) {
            procesador1.inicializar();
            procesador2.inicializar();
            procesador3.inicializar();

            procesador1.Visible=true;
            procesador2.Visible =true;
            procesador3.Visible =true;

            procesador1.cargarInstrucciones(path);
            procesador2.cargarInstrucciones(path);
            procesador3.cargarInstrucciones(path);
        }
        //Crea los hilos que corresponden a cada uno de los nucleos o procesadores, en total se simulan 3 procesadores
        private void inicializaProcesadores()
        {
            Thread hilo_proc1= new Thread(delegate(){});

            Thread hilo_proc2 = new Thread(delegate(){});

            Thread hilo_proc3 = new Thread(delegate(){});

            hilo_proc1.Start();
            hilo_proc2.Start();
            hilo_proc3.Start();

           
        }
    }
}
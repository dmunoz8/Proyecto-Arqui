using System;
using System.Threading;
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
    public class Organizador
    {
        public int[,] memoria; //64*16 = 1024 bytes
        public Queue<int> direccionHilillo;  //direccion de donde empiezan las intrucciones de cada hilillo
        public Queue<int []> colaContexto;
        
        int _CANTHILILLOS;
        int _QUANTUM;
        int reloj;
        Procesador procesador1;
        Procesador procesador2;
        Procesador procesador3;
        public Barrier sincronizacion;

        public Organizador(int cantHilillos, int quantum, string path)
        {
            _CANTHILILLOS = cantHilillos;
            _QUANTUM = quantum;
            reloj = 0;
            sincronizacion = new Barrier(4); //barrera para los tres procesadores y procesador principal

            procesador1 = new Procesador(1, sincronizacion);
            procesador2 = new Procesador(2, sincronizacion);
            procesador3 = new Procesador(3, sincronizacion);

            procesador1.inicializarProcesador();
            procesador2.inicializarProcesador();
            procesador3.inicializarProcesador();

            cargarMemoria(path);
            inicializaProcesadores();
        }

        //el procesador principal carga en memoria las instrucciones contenidas en los txt
        public void cargarMemoria(string path)
        {
            inicializarProcesadorPrincipal();
            cargarInstrucciones(path);
        }

        //Crea los hilos que corresponden a cada uno de los nucleos o procesadores, en total se simulan 3 procesadores
        private void inicializaProcesadores()
        {
            Thread hilo_proc1 = new Thread(delegate () { procesador1.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, this); });
            Thread hilo_proc2 = new Thread(delegate() { procesador2.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, this); });
            Thread hilo_proc3 = new Thread(delegate() { procesador3.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, this); });

            hilo_proc1.Start();
            hilo_proc2.Start();
            hilo_proc3.Start();

              //despliega informacion 
            Procesador principal = new Procesador();
           
            BindingList<string> data = new BindingList<string>();
            BindingList<string> data2 = new BindingList<string>();
            BindingList<string> data3 = new BindingList<string>();

            for(int i = 0; i < 32; i ++)
            {
                data.Add("R" + i + ":" + procesador1.registros[i]);
                data2.Add("R" + i + ":" + procesador2.registros[i]);
                data3.Add("R" + i + ":" + procesador3.registros[i]);
            }
            principal.CD1.DataSource = data;
            principal.CD2.DataSource = data2;
            principal.CD3.DataSource = data3;
            principal.Visible = true;

            int a = 0;
              
            sincronizarReloj(hilo_proc1,hilo_proc2,hilo_proc3);
        }

        /// <summary>
        /// Metodo para la sincronizacion de los ciclos de reloj entre procesadores.
        /// Se revisa si hay al menos un procesador corriendo. Si hay un procesador
        /// que ya no esta corriendo, se remueve de la barrera, para mantener sincronizacion.
        /// </summary>
        /// <param name="proc1">Procesador a sincronizar</param>
        /// <param name="proc2">Procesador a sincronizar</param>
        /// <param name="proc3">Procesador a sincronizar</param>
        public void sincronizarReloj(Thread proc1, Thread proc2, Thread proc3)
        {
            Console.WriteLine("\nAvance del reloj:");

            while (sincronizacion.ParticipantCount > 1)
            {
                //Variables para saber cual hilo se ha removido de la barrera
                bool proc1Corriendo = true;
                bool proc2Corriendo = true;
                bool proc3Corriendo = true;

                //Se revisa si los procesadores siguen trabajando
                if (!proc1.IsAlive && proc1Corriendo)
                {
                    proc1Corriendo = false;
                    sincronizacion.RemoveParticipant();
                }
                if (!proc2.IsAlive && proc2Corriendo)
                {
                    proc2Corriendo = false;
                    sincronizacion.RemoveParticipant();
                }
                if (!proc3.IsAlive && proc3Corriendo)
                {
                    proc3Corriendo = false;
                    sincronizacion.RemoveParticipant();
                }

                //una vez que los hilos envían su señal (avanzaron un ciclo) se suma al reloj
                reloj++;
                procesador1.reloj = reloj;
                procesador2.reloj = reloj;
                procesador3.reloj = reloj;

                //Permite a los procesadores seguir trabajando, espera a que cada uno avance un ciclo
                sincronizacion.SignalAndWait(); 

                //Console.WriteLine(reloj);
            }
        }

        //solo para el procesador principal que va a tener la memoria 
        public void inicializarProcesadorPrincipal()
        {
            memoria = new int[64, 16];
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    memoria[i, j] = 1;
                }
            }
            direccionHilillo = new Queue<int>();
            colaContexto = new Queue<int[]>();
        }

        //el procesador principal carga instrucciones de los txt a memoria principal
        public void cargarInstrucciones(string path)
        {
            //BindingList<int> data = new BindingList<int>();
            try
            {
                int fila = 24;
                int col = 0;
                foreach (string files in Directory.EnumerateFiles(path, "*.txt"))
                {
                    string contents = File.ReadAllText(files);
                    string[] instrucciones = contents.Split('\n');
                    foreach (string instruccion in instrucciones)
                    {
                        if (instruccion == instrucciones.First())
                        {
                            direccionHilillo.Enqueue(fila * 16 + col);

                            llenarCola(fila * 16 + col);
                        }
                        if(col == 16)
                        {
                            col = 0;
                            fila++;
                        }

                        string[] codigos = instruccion.Split(' ');
                        for (int i = 0; i < codigos.Length; i++)
                        {
                            if (col < 16)
                            {
                                memoria[fila, col] = Int32.Parse(codigos[i]);
                                col++;
                            }
                            else
                            {
                                fila++;
                                col = 0;
                            }
                        }
                    }
                }
                //int valor = Int32.Parse(contents);
                //    data.Add(valor);
                //    CD1.DataSource = data;
            }
            catch (IOException)
            {
            }
        }

        public void llenarCola(int pc)
        {
            int[] a = new int[36];
            for(int i = 0; i < 35; i++)
            {
                a[i] = 0;
            }
            a[32] = pc; //32 es el PC
            colaContexto.Enqueue(a);
        }

        public void imprimirDatos()
        {

        }
    }
}
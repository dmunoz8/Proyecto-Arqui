﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Proyecto_Arqui
{
    public class Organizador
    {
        public int[,] memoriaDatos; //Estructura para la memoria de Datos
        public int[,] memoria; //Estructura para la memoria de Instrucciones
        public Queue<int> direccionHilillo;  //direccion de donde empiezan las intrucciones de cada hilillo
        public Queue<int []> colaContexto; //Cola que guarda los contextos de los hilillos cuando no se encuentren en ejecucion en algun procesador
        public Queue<int[]> terminados; //Cola de resultados para los hilillos que van terminando
        
        public int _CANTHILILLOS; //Cantidad de hilos a ejecutar
        public int _QUANTUM; //Quantum dado por el usuario
        public bool _MODOLENTO;
        public int reloj; //reloj en comun de los procesadores
        public int impresiones = 0;

        public Procesador principal;
        public Procesador procesador1;
        public Procesador procesador2;
        public Procesador procesador3;
        public Barrier sincronizacion;

        /*Constructor
         * Inicializa los procesadores, la memoria y carga las instrucciones de cada hilo segun el path que dio el usuario
         * REQ: La cantidad de hilos(int), el quamtum para cada procesador(int), el path para saber donde estan las instrucciones(string)
         * RES: N/A
        */
        public Organizador(int cantHilillos, int quantum, string path, bool modoLento)
        {
            _CANTHILILLOS = cantHilillos;
            _QUANTUM = quantum;
            _MODOLENTO = modoLento;
            reloj = 0;

            principal = new Procesador(modoLento,this);

            sincronizacion = new Barrier(4); //barrera para los tres procesadores y procesador principal

            procesador1 = new Procesador(modoLento, null, 1, sincronizacion);
            procesador2 = new Procesador(modoLento, null, 2, sincronizacion);
            procesador3 = new Procesador(modoLento, null, 3, sincronizacion);

            procesador1.inicializarProcesador();
            procesador2.inicializarProcesador();
            procesador3.inicializarProcesador();

            cargarMemoria(path);
        }

        /*El programa principal carga en memoria las instrucciones contenidas en los txt
         * REQ: La ubicacion de los txt(string)
         * RES: N/A
         */
        public void cargarMemoria(string path)
        {
            inicializarProcesadorPrincipal();
            cargarInstrucciones(path);
        }

        /*Crea los hilos que corresponden a cada uno de los nucleos o procesadores, en total se simulan 3 procesadores, los inicia y sincroniza.
         * REQ: N/A
         * RES: N/A
         */
        public void inicializaProcesadores()
        {
            Thread hilo_proc1 = new Thread(delegate () { procesador1.ejecutarInstrs(_QUANTUM, ref procesador2, ref procesador3, ref procesador1, this); });
            Thread hilo_proc2 = new Thread(delegate() { procesador2.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador3, ref procesador2, this); });
            Thread hilo_proc3 = new Thread(delegate() { procesador3.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, this); });
            hilo_proc1.Start();
            hilo_proc2.Start();
            hilo_proc3.Start();
        }

        //public void eliminarHilos()
        //{
        //    hilo_proc1.Join();
        //    hilo_proc2.Join();
        //    hilo_proc3.Join();
        //}

        /*Metodo para la sincronizacion de los ciclos de reloj entre procesadores.
         * Se revisa si hay al menos un procesador corriendo. Si hay un procesador que ya no esta corriendo, se remueve de la barrera, para mantener sincronizacion.
         * REQ: Los 3 hilos simulando los 3 procesadores (Thread)
         * RES: N/A
         */
        public void sincronizarReloj()
        {
            while (sincronizacion.ParticipantCount > 1)
            {   
                if(reloj%300 == 0)
                {
                    //despliega informacion
                    imprimirDatos();
                }

                //una vez que los hilos envían su señal (avanzaron un ciclo) se suma al reloj
                reloj++;
                procesador1.reloj = reloj;
                procesador2.reloj = reloj;
                procesador3.reloj = reloj;

                //Permite a los procesadores seguir trabajando, espera a que cada uno avance un ciclo
                sincronizacion.SignalAndWait();
            }
        }


        /* Metodo para sincronizar reloj de procesadores manualmente. Devuelve un booleano para saber si se corrieron todos los hilillos
         * REQ: N/A
         * RES: bool
         */
        public bool sincronizarModoLento()
        {
            //una vez que los hilos envían su señal (avanzaron un ciclo) se suma al reloj
            imprimirDatos();
            reloj++;
            procesador1.reloj = reloj;
            procesador2.reloj = reloj;
            procesador3.reloj = reloj;
            //Permite a los procesadores seguir trabajando, espera a que cada uno avance un ciclo
            sincronizacion.SignalAndWait();
            if(sincronizacion.ParticipantCount > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /*Inicializa toda la memoria (Instrucciones y Datos) en 1 y las colas que contienen los contextos
         * REQ: N/A
         * RES: N/A
         */
        public void inicializarProcesadorPrincipal()
        {
            memoria = new int[64, 16];
            memoriaDatos = new int[24,4];

            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    memoria[i, j] = 1;
                }
            }

            direccionHilillo = new Queue<int>();
            colaContexto = new Queue<int[]>();
            terminados = new Queue<int[]>();

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    memoriaDatos[i, j] = 1;
                }
            }
        }

        /*El programa principal carga instrucciones de los txt a memoria principal
         * Lee todos los txt uno por uno, lee por fila, divide por espacios y va poniendo los valores en memoria
         * REQ: La direccion de los txt(string)
         * RES: N/A 
        */
        public void cargarInstrucciones(string path)
        {
            try
            {
                int fila = 24;
                int col = 0;
                foreach (string files in Directory.EnumerateFiles(path, "*.txt"))
                {
                    string contents = File.ReadAllText(files);
                    string[] instrucciones = contents.Split('\n');
                    for (int i = 0; i < instrucciones.Length; i++)
                    {
                        {
                            if (i == 0)
                            {
                                direccionHilillo.Enqueue(fila * 16 + col);

                                llenarCola(fila * 16 + col, int.Parse(files.Substring(files.Length-5,1)) );
                            }
                            if (col == 16)
                            {
                                col = 0;
                                fila++;
                            }

                            string[] codigos = instrucciones[i].Split(' ');
                            for (int k = 0; k < codigos.Length; k++)
                            {
                                if (col < 16)
                                {
                                    memoria[fila, col] = Int32.Parse(codigos[k]);
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
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /* Llena la cola de contexto con cada txt que va leyendo, es decir cada hilillo
         * REQ: La PC o direccion de memoria a leer cuando el hilillo se vaya a ejecutar(int)
         * RES: N/A
         */
        public void llenarCola(int pc,int name)
        {
            int[] a = new int[36];
            for(int i = 0; i < 36; i++)
            {
                a[i] = 0;
            }
            a[32] = pc; //32 es el PC
            a[35] = name;
            colaContexto.Enqueue(a);
        }

        /*Imprime los datos de los procesadores mientras estan ejecutando, los registros, y el ciclo
         * REQ: N/A
         * RES: N/A
         */
        public void imprimirDatos()
        {
            BindingList<string> data = new BindingList<string>();
            BindingList<string> data2 = new BindingList<string>();
            BindingList<string> data3 = new BindingList<string>();

            for (int i = 0; i < 32; i++)
            {
                data.Add("R" + i + ":" + procesador1.registros[i]);
                data2.Add("R" + i + ":" + procesador2.registros[i]);
                data3.Add("R" + i + ":" + procesador3.registros[i]);
            }
            principal.CD1.DataSource = data;
            principal.CD2.DataSource = data2;
            principal.CD3.DataSource = data3;

            principal.p1HililloTxt.Text = procesador1.nombre+".txt";
            principal.p2HililloTxt.Text = procesador2.nombre+".txt";
            principal.p3HililloTxt.Text = procesador3.nombre+".txt";

            principal.p1RelojTxt.Text = reloj.ToString();
            principal.Visible = true;
        }
    }
}
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
using System.Threading;

namespace Proyecto_Arqui
{
    public partial class Procesador : Form
    {
        public int[,] memoria; //64*16 = 1024 bytes
        public int[] registros; //32 (R0 - R31)
        public int[] contexto; //33 (R0 - R31 + PC) . 34 si se agrega RL
        public int[] cacheDatos; // 4 bloques de una palabra
        public int[] cacheInstrucciones; // 4 bloques de 4 palabras
        public Queue<int> hilillos;
        public Queue<int> direccionHilillo;  //direccion de donde empiezan las intrucciones de cada hilillo
        public Barrier sincronizacion;
        public int quantumLocal;
        public int reloj;
        public int RL;
        public int PC;

        public Procesador(int numProcesador = 0, Barrier sincronizacion = null)
        {
            InitializeComponent();
            quantumLocal = 0;
            reloj = 0;
            PC = 0;
            this.sincronizacion = sincronizacion;
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
        }

        //inicializador de registros y caches de datos de los tres procesadores 
        public void inicializarProcesador()
        {
            registros = new int[32];
            contexto = new int[32];
            cacheInstrucciones = new int[72];
            cacheDatos = new int[12]; //4*(1palabra+2campos de control) --> 4 * valor, etiqueta y estado.

            for (int i = 0; i < 32; i++)
            {
                registros[i] = 0;
                contexto[i] = 0;
            }

            for (int i = 0; i < 72; i++)
            {
                cacheInstrucciones[i] = 0;
            }

            cacheInstrucciones[17] = -1;  // -1 = invalido, 1 = valido
            cacheInstrucciones[35] = -1;
            cacheInstrucciones[53] = -1;
            cacheInstrucciones[71] = -1;

            for (int i = 0; i < 12; i++)
            {
                cacheDatos[i] = 0;
            }
            cacheDatos[2] = -1;  // -1 = invalido, 1 = valido
            cacheDatos[5] = -1;
            cacheDatos[8] = -1;
            cacheDatos[11] = -1;
        }

        //el procesador principal carga instrucciones de los txt a memoria principal
        public void cargarInstrucciones(string path)
        {
            BindingList<int> data = new BindingList<int>();
            try
            {
                int posicion = 24;
                foreach (string files in Directory.EnumerateFiles(path, "*.txt"))
                {
                    string contents = File.ReadAllText(files);
                    string[] instrucciones = contents.Split('\n');
                    foreach (string instruccion in instrucciones)
                    {
                        if (instruccion == instrucciones.First()) direccionHilillo.Enqueue(posicion);

                        string[] codigos = instruccion.Split(' ');
                        for (int i = 0; i < 16; i++)
                        {
                            if (i < 16 && i < codigos.Length)
                            {
                                memoria[posicion, i] = Int32.Parse(codigos[i]);
                            }
                        }
                        posicion++;
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

        public void pasarInstrMemoriaCache()
        {
            int bloque = PC / 16;
            int posicionCache = bloque % 4;
            int lengthMemoria = 0;
            int i = -1;
            if (Monitor.TryEnter(cacheInstrucciones))
            {
                try
                {
                    switch (posicionCache)
                    {
                        case 0:
                            i = 0;
                            break;

                        case 1:
                            i = 18;
                            break;

                        case 2:
                            i = 35;
                            break;

                        case 3:
                            i = 54;
                            break;
                    }
                    if (Monitor.TryEnter(memoria))
                    {
                        try
                        {
                            for (int w = 0; w < 28; w++)
                            {
                                //de caché a memoria = 28 ciclos
                                sincronizacion.SignalAndWait();
                            }
                            while (lengthMemoria < 16)
                            {
                                cacheInstrucciones[i] = memoria[bloque, lengthMemoria];
                                lengthMemoria++;
                                i++;
                            }
                            PC += 4;
                        }
                        finally
                        {
                            Monitor.Exit(memoria);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(cacheInstrucciones);
                }
            }
        }

        public void ejecutarInstrs(int quantum, ref Procesador a, ref Procesador b, ref Procesador c)
        {
            //hay hilillos que correr?
            while (direccionHilillo.Count > 0)
            {
                quantumLocal = quantum;
                int dirHilillo = direccionHilillo.Dequeue();
                contexto[32] = dirHilillo;
                cargarContexto();
                while (quantumLocal > 0)
                {
                    int[] instruccion = buscarInstruccion();
                    PC += 4;
                    switch (instruccion[0])
                    {
                        case 2:     // JR
                            PC = registros[instruccion[1]];
                            break;

                        case 3:     // JAL
                            registros[31] = PC;
                            PC += instruccion[3];
                            break;

                        case 4:     // BEQZ
                            if (registros[instruccion[1]] == 0)
                            {
                                PC += instruccion[3] * 4;
                            }
                            break;

                        case 5:     // BENZ
                            if (registros[instruccion[1]] != 0)
                            {
                                PC += instruccion[3] * 4;
                            }
                            break;

                        case 8:     // DADDI
                            registros[instruccion[2]] = registros[instruccion[1]] + instruccion[3];
                            break;

                        case 12:    // DMUL
                            registros[instruccion[3]] = registros[instruccion[1]] * registros[instruccion[2]];
                            break;

                        case 14:    // DDIV
                            registros[instruccion[3]] = registros[instruccion[1]] / registros[instruccion[2]];
                            break;

                        case 32:    // DADD
                            registros[instruccion[3]] = registros[instruccion[1]] + registros[instruccion[2]];
                            break;

                        case 34:    // DSUB
                            registros[instruccion[3]] = registros[instruccion[1]] - registros[instruccion[2]];
                            break;

                        case 35:    // LW
                            ejecutarLW(ref registros[instruccion[1]] + instruccion[3], instruccion[2]);
                            break;

                        case 43:    // SW
                            ejecutarSW(ref a, ref b, ref c, registros[instruccion[1]] + instruccion[3], instruccion[2]);
                            break;

                        case 63:    // Codigo para terminar el programa
                                    // hilillosTerminados++;
                            direccionHilillo.Enqueue(PC);
                            guardarContexto();
                            break;
                    }
                    quantumLocal--;
                    sincronizacion.SignalAndWait();
                }
            }
        }

        private void ejecutarSW(ref Procesador a, ref Procesador b, ref Procesador c, int dirMem, int numRegistro)
        {
            int bloque = calcularBloque(dirMem);
            int posicionC = posicionCache(bloque);
            int cantInvalidadas = 0;

            int posEtiqueta = 0;
            int posEstado = 0;

            bool datoEnMiCache = false;

            int datoEscribir = this.registros[numRegistro];

            if (Monitor.TryEnter(a.cacheDatos))
            {
                try
                {
                    //verificar si esta en la cache del procesador a
                    if (a.cacheDatos[posEtiqueta] == bloque)
                    {    //si tiene el dato
                        if (a.cacheDatos[posEstado] == 1)          //si esta valido, lo invalido
                        {
                            a.cacheDatos[posEstado] = -1;
                        }
                        cantInvalidadas++;
                    }
                }
                finally
                {
                    Monitor.Exit(a.cacheDatos);
                }
            }
            if (Monitor.TryEnter(b.cacheDatos))
            {
                try
                {
                    //verificar si esta en la cache del procesador b
                    if (b.cacheDatos[posEtiqueta] == bloque)
                    {    //si tiene el dato
                        if (b.cacheDatos[posEstado] == 1)          //si esta valido, lo invalido
                        {
                            b.cacheDatos[posEstado] = -1;
                        }
                        cantInvalidadas++;
                    }
                }
                finally
                {
                    Monitor.Exit(b.cacheDatos);
                }
            }


            //verificar si lo tengo en mi cahe e invalidarlo
            if (Monitor.TryEnter(this.cacheDatos))
            {
                try
                {
                    //verificar si esta en la cache del procesador b
                    if (this.cacheDatos[posEtiqueta] == bloque)
                    {    //si tiene el dato
                        if (this.cacheDatos[posEstado] == 1)          //si esta valido, lo invalido
                        {
                            this.cacheDatos[posEstado] = -1;
                        }
                        datoEnMiCache = true;
                    }
                }
                finally
                {
                    Monitor.Exit(this.cacheDatos);
                }
            }


            //verificar si obtuve los recursos de las otras dos para poder escribir  en memoria
            if (cantInvalidadas == 2)
            {
                if (datoEnMiCache)
                {
                    //escribo en mi cache
                    this.cacheDatos[posicionC] = datoEscribir;
                    this.cacheDatos[posEstado] = 1;
                }
                //escribo en memoria
                memoria[1, 2] = datoEscribir;
            }

        }

        public void ejecutarLW(int direccionMemoria, int numeroRegistro)
        {
            int bloque = calcularBloque(direccionMemoria);          // Bloque en memoria donde esta el dato            
            int posicionC = posicionCache(bloque);                  // Donde deberia estar en cache                        
                                                                    //int desplazamiento = (direccionMemoria % 16) / 4;     // Numero de palabras a partir del bloque

            if (Monitor.TryEnter(cacheDatos))
            {
                try
                {
                    //Si el dato está en caché y está valido, lo copio al registro 
                    if (cacheDatos[posicionC * 3 + 1] == bloque && cacheDatos[posicionC * 3 + 2] != -1)
                    {
                        registros[numeroRegistro] = cacheDatos[posicionC * 3];//carga al registro
                    }
                    else
                    {
                        //no esta en cache o esta invalido, lo traigo de memoria
                        if (Monitor.TryEnter(memoria))
                        {
                            try
                            {
                                for (int w = 0; w < 28; w++)
                                {
                                    //  Tarda 28 ciclos, se envían 28 señales
                                    sincronizacion.SignalAndWait();
                                }
                                //Se sube a caché y se carga en el registro
                                cacheDatos[posicionC * 3] = memoria[bloque, 0]; //REVISAR ESTA POSICION
                                cacheDatos[posicionC * 3 + 1] = bloque; //Etiqueta
                                cacheDatos[posicionC * 3 + 2] = 1;  //Bloque valido
                                registros[numeroRegistro] = cacheDatos[posicionC * 3];
                            }
                            finally
                            {
                                Monitor.Exit(memoria);
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(cacheDatos);
                }
            }
        }

        public int calcularBloque(int direccionMemoria)
        {
            int bloque = direccionMemoria / 16;
            return bloque;
        }

        public int posicionCache(int bloque)
        {
            int posCache = bloque % 4;
            return posCache;
        }

        private void cargarContexto()
        {
            for (int i = 0; i < 32; i++)
            {
                registros[i] = contexto[i];
            }
            PC = contexto[32];
        }

        private void guardarContexto()
        {
            for (int i = 0; i < 32; i++)
            {
                contexto[i] = registros[i];
            }
            contexto[32] = PC;
        }

        // Busca las instruccion a ejecutar en la cache de instrucciones
        public int[] buscarInstruccion()
        {
            int bloque = PC / 16;      // Para buscar en la etiqueta de la memoria caché
            int desplazamiento = PC - (16 * bloque);    // De aqui se saca el numero de columna. A partir de donde comienza el bloque, cuantas palabras me desplazo            
            int[] instruccion = new int[4];
            if (Monitor.TryEnter(cacheInstrucciones))
            {
                try
                {
                    if (cacheInstrucciones[bloque * 18 + 16] != bloque)    // Hay fallo de cache?
                    {
                        pasarInstrMemoriaCache();
                    }

                    for (int i = 0; i < 4; i++, desplazamiento++)
                    {
                        instruccion[i] = cacheInstrucciones[bloque * 18 + desplazamiento];
                    }
                }
                finally
                {
                    Monitor.Exit(cacheInstrucciones);
                }
            }
            else
            {
                //PC -= 4;
                //libero
            }
            return instruccion;
        }
    }
}

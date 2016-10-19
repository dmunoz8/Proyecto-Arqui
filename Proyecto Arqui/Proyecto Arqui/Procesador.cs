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
    public partial class Procesador : Form
    {
        public int[] registros; //32 (R0 - R31)
        public int[] contexto; //36 R0-R31, PC, PR relojInicio, relojFin 
        public int[] cacheDatos; // 4 bloques de una palabra
        public int[] cacheInstrucciones; // 4 bloques de 4 palabras
        public Queue<int> hilillos;
        public Barrier sincronizacion;
        public int quantumLocal;
        public int reloj;
        public int RL;
        public int PC;
        public int relojInicio;
        public int relojFin;

        public Procesador(int numProcesador = 0, Barrier sincronizacion = null)
        {
            InitializeComponent();
            quantumLocal = 0;
            reloj = 0;
            PC = 0;
            this.sincronizacion = sincronizacion;
        }

        //inicializador de registros y caches de datos de los tres procesadores 
        public void inicializarProcesador()
        {
            registros = new int[32];
            contexto = new int[36];
            cacheInstrucciones = new int[72];
            cacheDatos = new int[12]; //4*(1palabra+2campos de control) --> 4 * valor, etiqueta y estado.

            for (int i = 0; i < 32; i++)
            {
                registros[i] = 0;
                contexto[i] = 0;
            }

            contexto[32] = 0;

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

        public void pasarInstrMemoriaCache(ref Organizador p)
        {
            int bloque = calcularBloque(PC);
            int posicionC = posicionCache(bloque);
            int lengthMemoria = 0;
            int i = -1;
            if (Monitor.TryEnter(cacheInstrucciones))
            {
                try
                {
                    switch (posicionC)
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
                    if (Monitor.TryEnter(p.memoria))
                    {
                        try
                        {
                            /* for (int w = 0; w < 28; w++)
                             {
                                 //de caché a memoria = 28 ciclos
                                 //sincronizacion.SignalAndWait();
                             }*/
                            while (lengthMemoria < 16)
                            {
                                cacheInstrucciones[i] = p.memoria[bloque, lengthMemoria];
                                lengthMemoria++;
                                i++;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(p.memoria);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(cacheInstrucciones);
                }
            }
        }

        public void ejecutarInstrs(int quantum, ref Procesador a, ref Procesador b, ref Procesador c, Organizador p)
        {
            int[] instruccion = new int[4];
            int fin = -1;
            //hay hilillos que correr?
            while (p.colaContexto.Count > 0)
            {
                quantumLocal = quantum;
                //int dirHilillo = p.direccionHilillo.Dequeue();
                //contexto[32] = dirHilillo;
                cargarContexto(p.colaContexto.Dequeue());//saca un contexto de la cola
                while (quantumLocal > 0 && fin != 63)
                {
                    instruccion = buscarInstruccion(ref p);
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
                            ejecutarLW(registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);
                            break;

                        case 43:    // SW
                            ejecutarSW(ref a, ref b, ref c, registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);
                            break;

                        case 63:    // Codigo para terminar el programa
                            //p.direccionHilillo.Enqueue(PC);
                            //int [] contextoGuardar=guardarContexto();
                            //p.colaContexto.Enqueue(contextoGuardar);//mete el contexto a la cola
                            fin = 63;
                            break;
                    }
                    quantumLocal--;
                    //reloj++;
                    //!!!!!!!!!!!!!!!!!!!------- OJO --------------!!!!!!!!!!!!!!!
                    //TODO: preguntar a la profe si cuando no logra bloquear bus es un ciclo extra o este ya lo considera
                    sincronizacion.SignalAndWait();
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //reloj++;
                    //imprimir(ref p);
                }

                if (fin != 63)
                {
                    int[] contextoGuardar1 = guardarContexto();
                    p.colaContexto.Enqueue(contextoGuardar1);
                }
            }
        }

        private void ejecutarSW(ref Procesador a, ref Procesador b, ref Procesador c, int dirMem, int numRegistro, ref Organizador p)
        {
            int bloque = calcularBloque(dirMem);
            int palabra = calcularPalabra(dirMem);
            int posicionC = posicionCache(bloque);
            int cantInvalidadas = 0;

            //posiciones de la cache
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
                            //verificar si lo tengo en mi cahe e invalidarlo
                            if (Monitor.TryEnter(cacheDatos))
                            {
                                try
                                {
                                    //verificar si esta en mi cache
                                    if (cacheDatos[posEtiqueta] == bloque)
                                    {    //si tiene el dato
                                        if (cacheDatos[posEstado] == 1)   //si esta valido, lo invalido
                                        {
                                            cacheDatos[posEstado] = -1;
                                        }
                                        datoEnMiCache = true;
                                    }
                                    //verificar si obtuve los recursos de las otras dos para poder escribir  en memoria
                                    //si el dato esta en mi cache lo escribo ahi y en memoria
                                    //si no esta en mi cache solo escribo en memoria
                                    if (cantInvalidadas == 2)
                                    {
                                        if (datoEnMiCache)
                                        {
                                            //escribo en mi cache
                                            cacheDatos[posicionC] = datoEscribir;
                                            cacheDatos[posEstado] = 1;
                                        }
                                        //escribo en memoria
                                        if (Monitor.TryEnter(p.memoria))
                                        {
                                            try
                                            {
                                                for (int w = 0; w < 7; w++)
                                                {
                                                    //  Tarda 7 ciclos, se envían 7 señales
                                                    sincronizacion.SignalAndWait();
                                                }
                                                p.memoria[bloque, palabra] = datoEscribir;
                                            }
                                            finally
                                            {
                                                Monitor.Exit(p.memoria);
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
                        finally
                        {
                            Monitor.Exit(b.cacheDatos);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(a.cacheDatos);
                }
            }
        }

        public void ejecutarLW(int direccionMemoria, int numeroRegistro, ref Organizador p)
        {
            int palabra = calcularPalabra(direccionMemoria);
            int bloque = calcularBloque(direccionMemoria);          // Bloque en caché donde esta el dato            
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
                        if (Monitor.TryEnter(p.memoria))
                        {
                            try
                            {
                                for (int w = 0; w < 28; w++)
                                {
                                    //  Tarda 28 ciclos, se envían 28 señales
                                    sincronizacion.SignalAndWait();
                                }
                                //Se sube a caché y se carga en el registro
                                cacheDatos[posicionC * 3] = p.memoria[bloque, palabra];
                                cacheDatos[posicionC * 3 + 1] = bloque; //Etiqueta
                                cacheDatos[posicionC * 3 + 2] = 1;  //Bloque valido
                                registros[numeroRegistro] = cacheDatos[posicionC * 3];
                            }
                            finally
                            {
                                Monitor.Exit(p.memoria);
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

        /// <summary>
        /// A partir de la direccion de memoria calcular la palabra en el bloque de memoria.
        /// </summary>
        /// <param name="direccionMemoria">Direccion en la memoria principal</param>
        /// <returns></returns>
        public int calcularPalabra(int direccionMemoria)
        {
            return direccionMemoria % 16;
        }

        private void cargarContexto(int [] contextoCargar)
        {
           
            for (int i = 0; i < 32; i++)
            {
                registros[i] = contextoCargar[i];
            }
            PC = contextoCargar[32];
            RL = contextoCargar[33];
            relojInicio= contextoCargar[34];
            relojFin=contextoCargar[35];

        }

        private int []  guardarContexto()
        {
            int[] contextoGuardar =new int[36];
            for (int i = 0; i < 32; i++)
            {
                contextoGuardar[i] = registros[i];
            }
            contextoGuardar[32] = PC;
            contextoGuardar[33] = RL;
            contextoGuardar[34] = 0;
            contextoGuardar[35] = 0;

            return contextoGuardar;

        }

        // Busca las instruccion a ejecutar en la cache de instrucciones
        public int[] buscarInstruccion(ref Organizador p)
        {
            int bloque = calcularBloque(PC);
            int pos = posicionCache(bloque); // Para buscar en la etiqueta de la memoria caché
            int desplazamiento = PC - (16 * bloque);    // De aqui se saca el numero de columna. A partir de donde comienza el bloque, cuantas palabras me desplazo            
            int[] instruccion = new int[4];
            if (Monitor.TryEnter(cacheInstrucciones))
            {
                try
                {
                    if (cacheInstrucciones[pos * 18 + 16] != bloque)    // Hay fallo de cache?
                    {
                        pasarInstrMemoriaCache(ref p);
                    }

                    for (int i = 0; i < 4; i++, desplazamiento++)
                    {
                        instruccion[i] = cacheInstrucciones[pos * 18 + desplazamiento];
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

        public void imprimir(ref Procesador p)
        {
            BindingList<int> data = new BindingList<int>();
            for (int i = 0; i < 32; i++)
            {
                data.Add(registros[i]);
            }
            CD1.DataSource = data;
        }

        private void P1_Enter(object sender, EventArgs e)
        {

        }
    }
}

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
        private int numeroProcesador;
        public int[] registros; //estructura de los registros, 32 (R0 - R31)
        public int[] contexto; //estructura de los contextos, 35 R0-R31, PC, RL, duracion 
        public int[] cacheDatos; //estructura de las caches de datos, 4 bloques de una palabra
        public int[] cacheInstrucciones; //estructura de las caches de instrucciones, 4 bloques de 4 palabras
        public Queue<int> hilillos; //Cola de hilillo
        public Barrier sincronizacion;
        public static Mutex varsMutex;
        public int quantumLocal; //Quantum que tiene cada procesador
        public int reloj; //reloj en comun para contabilizar los ciclos
        public int RL;
        public int PC;
        public int duracion; //duracion de los hilillos
        public bool lento;
        public string nombre;
        public Organizador org;

        /*Constructor
         * Inicializa varios componentes de los procesadores
         * REQ: int, bool, Barrier
         * RES: N/A
         */ 
        public Procesador(bool modoLento, Organizador o = null, int numProcesador = 0, Barrier sincronizacion = null)
        {
            numeroProcesador = numProcesador;
            InitializeComponent();
            quantumLocal = 0;
            reloj = 0;
            duracion = 0;
            PC = 0;
            nombre = "";
            this.sincronizacion = sincronizacion;
            siguienteBtn.Visible = modoLento;  // se muestra el boton solo si esta activado el modo lento
            varsMutex = new Mutex();
            org = o;
        }

        /*Inicializador de registros y caches de datos de los tres procesadores 
         * REQ: N/A
         * RES: N/A
         */ 
        public void inicializarProcesador()
        {
            registros = new int[32];
            contexto = new int[36];
            cacheInstrucciones = new int[72]; //40 bloques
            cacheDatos = new int[24]; //4*(1palabra(4 enteros)+2campos de control) --> 4 * valor, etiqueta y estado. 24 bloques

            for (int i = 0; i < 32; i++)
            {
                registros[i] = 0;
                contexto[i] = 0;
            }

            contexto[32] = 0;  //PC
            contexto[33] = 0; //RL
            contexto[34] = 0;  //Duracion
            contexto[35] = 0;


            for (int i = 0; i < 72; i++)
            {
                cacheInstrucciones[i] = 0;
            }

            cacheInstrucciones[17] = -1;  // -1 = invalido, 1 = valido
            cacheInstrucciones[35] = -1;
            cacheInstrucciones[53] = -1;
            cacheInstrucciones[71] = -1;

            for (int i = 0; i < 24; i++)
            {
                cacheDatos[i] = 0;
            }
            cacheDatos[5] = -1;  // -1 = invalido, 1 = valido
            cacheDatos[11] = -1;
            cacheDatos[17] = -1;
            cacheDatos[23] = -1;
        }

        /*Se encarga de pasar las instrucciones de memoria a cache cuando hay un fallo, contabiliza los ciclos correspondientes
         * REQ: referencia de quien posee la memoria(ref Organizador)
         * RES: N/A
         */ 
        public void pasarInstrMemoriaCache(ref Organizador p)
        {
            int bloque = calcularBloque(PC);
            int posicionC = posicionCache(bloque);
            int lengthMemoria = 0;
            int i = -1;
            bool leyo = false; //variable para saber si pudo leer de memoria, sino para seguir intentando
            while (!leyo)
            {
                if (Monitor.TryEnter(cacheInstrucciones))
                {
                    try
                    {
                        switch (posicionC)
                        {
                            case 0:
                                i = 0; //inicio de bloque 0
                                break;

                            case 1:
                                i = 18; //inicio de bloque 1
                                break;

                            case 2:
                                i = 36; //inicio de bloque 2
                                break;

                            case 3:
                                i = 54; //inicio de bloque 3
                                break;
                        }
                        if (Monitor.TryEnter(p.memoria))
                        {
                            try
                            {
                                for (int w = 0; w < 28; w++)
                                {
                                    //de memoria a caché = 28 ciclos
                                    sincronizacion.SignalAndWait();
                                }
                                while (lengthMemoria < 16)
                                {
                                    try { cacheInstrucciones[i] = p.memoria[bloque, lengthMemoria]; } //escribe de memoria a cache de instrucciones
                                    catch { Console.WriteLine("Bloque: {0}, length: {1}", bloque, lengthMemoria); }
                                    lengthMemoria++;
                                    i++;
                                }
                            }
                            finally
                            {
                                leyo = true;
                                Monitor.Exit(p.memoria);
                            }
                        }
                        else
                        {
                            sincronizacion.SignalAndWait();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(cacheInstrucciones);
                    }
                }
            }
        }

        /*Metodo central donde cada hilillo revisa que debe ejecutar y hasta cuando continuar ejecutando sus instrucciones
         * REQ: el quamtum para cada procesador(int), referencia de cada procesador para el bloque de caches si es necesario(ref Procesador), el que posee los elementos compartidos(Organizador)
         * RES: N/A
         */ 
        public void ejecutarInstrs(int quantum, ref Procesador a, ref Procesador b, ref Procesador c, Organizador p)
        {
            int[] instruccion = new int[4]; //almacena la instruccion a ejecutar cada ciclo
            int hilillos = 0; //hilillos que restan en cola
            int relojInicio = 0;
            int relojFinal = 0;           
            varsMutex.WaitOne(); //hay hilillos que correr?
            while (p.colaContexto.Count > 0)
            {
                varsMutex.ReleaseMutex();
                quantumLocal = quantum;
                int fin = -1; //valor para saber que el hilillo no ha terminado
                varsMutex.WaitOne();
                hilillos = p.colaContexto.Count; //cuenta cuantos hilos hay en cola
                if (hilillos > 0)
                {
                    int[] contextoACargar = p.colaContexto.Dequeue(); //saca un hilillo de la cola
                    if (contextoACargar == null)
                    {
                        Console.WriteLine("Hilillos: {0}", hilillos);
                    }
                    varsMutex.ReleaseMutex();
                    cargarContexto(contextoACargar);//Carga los valores correspondientes dentro del procesador
                    relojInicio = reloj;
                    while (quantumLocal > 0 && fin != 63) //corre mientras haya quamtum y la instruccion 63 no se haya ejecutado
                    {
                        instruccion = buscarInstruccion(ref p);

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
                                    PC += (instruccion[3] * 4);
                                }
                                break;

                            case 5:     // BNEZ
                                if (registros[instruccion[1]] != 0)
                                {
                                    PC += (instruccion[3] * 4);
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
                                int leyo = ejecutarLW(registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);
                                if (leyo == 0) //Si no leyo devuelvase
                                {
                                    PC -= 4;
                                }
                                break;

                            case 43:    // SW
                                int escribi = ejecutarSW(ref a, ref b, registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);
                                
                                if (escribi == 0) //Si no ha escrito devuelvase
                                {
                                    PC -= 4;
                                }
                                break;

                            case 50: //LL
                                
                                int leyoLL= ejecutarLW(registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);
                                break;

                            case 51: //SC;
                               int coincideRL = ejecutarSC(ref a, ref b, registros[instruccion[1]] + instruccion[3], instruccion[2], ref p);

                               break;

                            case 63: // FIN, Codigo para terminar el programa
                                fin = 63;
                                relojFinal = reloj;
                                duracion += (relojFinal - relojInicio);
                                terminarHilillo(duracion, ref p);
                                break;
                        }
                        quantumLocal--;                        
                        sincronizacion.SignalAndWait();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!                        
                    }

                    if (fin != 63)
                    {
                        relojFinal = reloj;
                        duracion += (relojFinal - relojInicio); // Cuanto duro ejecutandose ese hilillo
                        int[] contextoGuardar1 = guardarContexto();
                        varsMutex.WaitOne();
                        p.colaContexto.Enqueue(contextoGuardar1);
                        varsMutex.ReleaseMutex();
                    }
                }
                else
                {
                    varsMutex.ReleaseMutex();
                }
                varsMutex.WaitOne();
            }
            varsMutex.ReleaseMutex();
            sincronizacion.SignalAndWait();
            sincronizacion.RemoveParticipant();
        }

        /*Ejecutar un SW.
         * Calcula la posicion en memoria a la que debe escribir.
         * Pide el bus para ir a invalidar las otras caches, y luego libera
         * Invalida la propia
         * Si es fallo, escribe solo en memoria
         * Si es hit, escribe en memoria y cache
         * REQ: referencias para las otras caches(ref Procesador), la direccion en cual escribir(int), de cual registro escribir el valor(int), referencia a la memoria(ref Organizador)
         * RES: Si escribio o no (int)
         */ 
        private int ejecutarSW(ref Procesador a, ref Procesador b, int dirMem, int numRegistro, ref Organizador p)
        {
            int palabra = calcularPalabra(dirMem);
            int bloque = calcularBloque(dirMem); // Bloque en caché donde esta el dato            
            int posicionC = posicionCache(bloque);
            int escribio = 0; //variable para ver si hay escritura o no

            int cantInvalidadas = 0;
            int cantCompartidos = 0;
            int cantCaches = 0;

            //posiciones de la cache
            int posEtiqueta = 4;
            int posEstado = 5;

            bool datoEnMiCache = false;

            int _datoEscribir = registros[numRegistro]; //valor a escribir

            if (Monitor.TryEnter(p.memoriaDatos))
            {
                try
                {
                    if (Monitor.TryEnter(a.cacheDatos))
                    {
                        try
                        {
                            cantCaches++;
                            //verificar si esta en la cache del procesador a
                            if (a.cacheDatos[posicionC * 6 + posEtiqueta] == bloque)
                            {
                                //sí, tiene el dato
                                if (a.cacheDatos[posicionC * 6 + posEstado] == 1) //si esta valido, lo invalido
                                {
                                    a.cacheDatos[posicionC * 6 + posEstado] = -1;
                                    if (a.RL == dirMem) {
                                        a.RL = -1;
                                    }
                                }
                                cantCompartidos++;
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
                            cantCaches++;
                            //verificar si esta en la cache del procesador b
                            if (b.cacheDatos[posicionC * 6 + posEtiqueta] == bloque)
                            {   //si tiene el dato
                                if (b.cacheDatos[posicionC * 6 + posEstado] == 1) //si esta valido, lo invalido
                                {
                                    b.cacheDatos[posicionC * 6 + posEstado] = -1;
                                    if (b.RL == dirMem)
                                    {
                                        b.RL = -1;
                                    }
                                }
                                cantCompartidos++;
                                cantInvalidadas++;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(b.cacheDatos);
                        }
                    }
                    if (Monitor.TryEnter(cacheDatos))
                    {
                        try
                        {
                            //verificar si esta en mi cache
                            if (cacheDatos[posicionC * 6 + posEtiqueta] == bloque)
                                    {    //si tiene el dato
                                        if (cacheDatos[posicionC * 6 + posEstado] == 1) //si esta valido, lo invalido
                                        {
                                            cacheDatos[posicionC * 6 + posEstado] = -1;
                                        }
                                        datoEnMiCache = true;
                                    }
                                    //verificar si obtuve los recursos de las otras dos para poder escribir  en memoria
                                    //si el dato esta en mi cache lo escribo ahi y en memoria
                                    //si no esta en mi cache solo escribo en memoria
                                    if (cantCaches == 2)
                                    {
                                        if (datoEnMiCache)
                                        {
                                            //escribo en mi cache
                                            cacheDatos[posicionC + palabra] = _datoEscribir;
                                            cacheDatos[posicionC * 6 + posEstado] = 1;
                                        }
                                        for (int w = 0; w < 7; w++)
                                        {
                                            //  Tarda 7 ciclos, se envían 7 señales
                                            sincronizacion.SignalAndWait();
                                        }
                                        p.memoriaDatos[bloque, palabra] = _datoEscribir;
                                        escribio = 1;
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
                    Monitor.Exit(p.memoriaDatos);
                }
            }
            return escribio;
        }

        /*Ejecutar un LW
         * Calcula la direccion de cual leer segun la estructura utilizada
         * Calcula en que bloque de cache subir lo que se va a leer
         * REQ: la direccion de donde leer(int), a cual registro guardarlo(int), la referencia de la memoria(ref Organizador)
         * RES: Si leyo o no (int)
         */ 
        public int ejecutarLW(int direccionMemoria, int numeroRegistro, ref Organizador p)
        {
            int palabra = calcularPalabra(direccionMemoria);
            int bloque = calcularBloque(direccionMemoria); // Bloque en caché donde esta el dato            
            int posicionC = posicionCache(bloque); // Donde deberia estar en cache                        
            int leyo = 0; //variable para saber si leyo o no

            if (Monitor.TryEnter(cacheDatos))
            {
                try
                {
                    //Si el dato está en caché y está valido, lo copio al registro 
                    if (cacheDatos[posicionC * 6 + 4] == bloque && cacheDatos[posicionC * 6 + 5] != -1)
                    {
                        registros[numeroRegistro] = cacheDatos[posicionC * 6 + palabra];//carga al registro
                        leyo = 1;
                        RL = direccionMemoria;
                       // Console.WriteLine("hilo  " + registros[31] +"----> RL= "+ RL);

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
                                    //Console.WriteLine("LW: {0}. Reloj: {1}", w, reloj);
                                    sincronizacion.SignalAndWait();
                                }
                                //Se sube a caché y se carga en el registro
                                for (int i = 0; i < 4; i++)
                                {
                                    cacheDatos[posicionC * 6 + i] = p.memoriaDatos[bloque, i]; //cambiar a estructura de memoria de datos!!!!!!!!!!
                                }
                                cacheDatos[posicionC * 6 + 4] = bloque; //Etiqueta
                                cacheDatos[posicionC * 6 + 5] = 1;  //Bloque valido
                                registros[numeroRegistro] = cacheDatos[posicionC * 6 + palabra];
                                leyo = 1;
                                RL = direccionMemoria;
                                //Console.WriteLine("hilo  " + registros[31] + "----> RL= " + RL);
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

            return leyo;
        }

        /*Ejecutar un SC
         * Revisa si el RL es igual a la direccion donde debe escribir
         * Si es asi, trata de escribir, esto coloca un candado
         * Si no pudo se coloca un 0 en el registro designado
         * REQ: referencias de los procesadores para las caches(ref Procesador), direccion en la cual escribir(int), registro de donde guardar el valor(int), referencia para la memoria(ref Organizador)
         */ 
        public int ejecutarSC(ref Procesador a, ref Procesador b, int dirMem, int numRegistro, ref Organizador p)
        {
            int _dirMem = dirMem;
            int escribio = 0;
            if (Monitor.TryEnter(cacheDatos))
            {
                try
                {
                    if (RL==_dirMem)
                    {
                        escribio = ejecutarSW(ref a, ref b, dirMem, numRegistro, ref p);
                        if (escribio == 1)
                        {
                            registros[numRegistro] = 1;
                        }
                        else {
                            registros[numRegistro] = 0;
                        }
                    }
                    else 
                    {
                        registros[numRegistro] = 0;
                    }
                    //Console.WriteLine("hilo  " + registros[31] + " intento SC en  pos de memoria: " + dirMem + "   Registro: " + numRegistro + " = " + registros[numRegistro]);
                    RL = -1;
                }
                finally
                {
                    Monitor.Exit(cacheDatos);
                }
            }
            return escribio;
        }

        /*Calcula el bloque para las instrucciones de lectura y escritura, es decir conviete la direccion en bloque para la estructura utilizada
         * REQ: la direccion de memoria(int)
         * RES: el numero de bloque(int)
         */ 
        public int calcularBloque(int direccionMemoria)
        {
            int bloque = direccionMemoria / 16;
            return bloque;
        }

        /*Posicion de la cache,es decir el bloque que se va a usar cuando se sube un bloque de memoria
         * REQ: el bloque de memoria(int)
         * RES: La posicion o bloque dentro de la cache(int)
         */ 
        public int posicionCache(int bloque)
        {
            int posCache = bloque % 4;
            return posCache;
        }

        /*A partir de la direccion de memoria calcular la palabra en el bloque de memoria.
         * REQ: la direccion de memoria(int)
         * RES: la palabra que se va a usar(int)
         */ 
        public int calcularPalabra(int direccionMemoria)
        {
            return (direccionMemoria % 16) / 4;
        }

        /*Se encarga de poner los valores correspondientes para la ejecucion de un hilillo segun su contexto
         * REQ: el contexto del hilillo(int[])
         * RES: N/A
         */ 
        private void cargarContexto(int[] contextoCargar)
        {
            for (int i = 0; i < 32; i++)
            {
                registros[i] = contextoCargar[i];
            }
            PC = contextoCargar[32];
            RL = -1;
            duracion = contextoCargar[34];
            nombre = contextoCargar[35].ToString();

        }

        /*Se encarga de guardar los valores de cada hilillo en un array para ser guardado en una cola
         * REQ: N/A
         * RES: el contexto en la estructura de un array(int[])
         */ 
        private int[] guardarContexto()
        {
            int[] contextoGuardar = new int[36];
            for (int i = 0; i < 32; i++)
            {
                contextoGuardar[i] = registros[i];
            }
            contextoGuardar[32] = PC;
            contextoGuardar[33] = RL;
            contextoGuardar[34] = duracion;
            contextoGuardar[35] = int.Parse(nombre);

            return contextoGuardar;

        }

        /*Busca las instruccion a ejecutar en la cache de instrucciones
         *REQ: Pasa la referencia de la memoria si no esta en cache(ref Organizador)
         * RES: la instruccion a ejecutar(int[]) 
        */
        public int[] buscarInstruccion(ref Organizador p)
        {
            int bloque = calcularBloque(PC);
            int pos = posicionCache(bloque); // Para buscar en la etiqueta de la memoria caché
            int desplazamiento = PC - (16 * bloque); // De aqui se saca el numero de columna. A partir de donde comienza el bloque, cuantas palabras me desplazo            
            int[] instruccion = new int[4];
            if (Monitor.TryEnter(cacheInstrucciones))
            {
                try
                {
                    if (cacheInstrucciones[pos * 18 + 16] != bloque)    // Hay fallo de cache?
                    {
                        pasarInstrMemoriaCache(ref p);
                        //if(pasarInstrMemoriaCache(ref p)) leyo = true;
                    }

                    for (int i = 0; i < 4; i++, desplazamiento++)
                    {
                        instruccion[i] = cacheInstrucciones[pos * 18 + desplazamiento];
                    }
                }
                finally
                {
                    Monitor.Exit(cacheInstrucciones);
                    PC += 4;
                }
            }
            return instruccion;
        }

        /*Metodo que guarda los valores de cada hilillo en una cola para imprimirlos al final
         * REQ: duracion de cada hilillo(int), referencia de donde guardar el array(ref Organizador)
         * RES: N/A
         */ 
        public void terminarHilillo(int duracion, ref Organizador org)
        {
            int[] final = new int[57];
            int espacios = 0;

            for (int i = 0; i < 32; i++)
            {
                final[i] = registros[i];
            }

            for (int j = 0; j < 24; j++)
            {
                if (espacios < 4)
                {
                    final[j + 32] = cacheDatos[j];
                }
                if(espacios == 5)
                {
                    espacios = 0;
                }
                espacios++;
            }

            final[56] = duracion;

            org.terminados.Enqueue(final);
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

        private void Procesador_Load(object sender, EventArgs e)
        {

        }

        private void siguienteBtn_Click(object sender, EventArgs e)
        {
            bool fin = false;
            fin = org.sincronizarModoLento();
            if (fin)
            {
                Resultados resultado = new Resultados(org);
                resultado.imprimir();
            }
        }
    }
}

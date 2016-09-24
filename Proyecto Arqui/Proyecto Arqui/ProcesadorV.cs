using System;
using System.Threading;
using System.Collections;


namespace ProyectoArquitectura
{
    public class Procesador
    {
        public int[,] cacheInstrucciones;   // 4*17 -> 4 bloques (filas) y cada una con 4 palabras de 4 bytes + 1 columna para la etiqueta (sirve para 
        public int[] registros;             // 0-31
        public int[] memPrincipal;          // 16*4*4 -> 16 bloques de 4 palabras, de 4 bytes cada palabra. 1 instruccion por palabra
        public int[,] matContext;           // 4*33 -> 4 filas por cada uno de los 4 posibles hilillos, 32 columnas par los registros y 1 columna para el PC
        public int[] tamTxt;                // Array donde se guarda donde inicia cada hilillo
        public int numeroProcesador;        // Para identificar el procesador
        public Barrier sync;                // Barrier para sincronizar
        public int hilillosTerminados;      // Contador para saber si ya se ejecutaron todos los hilillos
        public int quantumLocal;            // Quantum que tiene para ejecutar las instrucciones
        public int indiceTXT;               // Para movernos por tamTXT y tambien sirve como la cantidad de hilillos que se deberian ejecutar
        public int PC;                      // Program Count del procesador
        public int reloj;                   // Reloj del procesador
        public int indContexto;             // Indice para cambiar de contexto (fila de la matriz)

        public int[,] memCompartida;        // Memoria a la que todos los hilillos pueden acceder
        public int[,] cacheDatos;           // Donde se almacenan los datos que se cargan de memoria principal
        public int[,] directorio;           // El estado puede ser 0, 1 o 2. 0 = Invalido  1 = Modificado  2 = Compartido
        public bool guardoExito;

        public bool cargoExito;
        public int llactivo;
        public int bloquell;

        public Procesador(int numeroProcesador = 0, Barrier sync = null)
        {
            // Los arrays se inicializan en 0 por defecto
            cargoExito = false;
            guardoExito = false; 
            llactivo = 0;
            bloquell = 0;
            cacheInstrucciones = new int[4, 17];
            cacheInstrucciones[0, 16] = cacheInstrucciones[1, 16] = cacheInstrucciones[2, 16] = cacheInstrucciones[3, 16] = -1;
            registros = new int[35];//0-31 registros, 32 rl, 33 boolean ll,34 bloque al que pertenece el ll
            memPrincipal = new int[256];
            matContext = new int[4, 37];
            tamTxt = new int[4];
            tamTxt[0] = tamTxt[1] = tamTxt[2] = tamTxt[3] = -1;
            memCompartida = new int[8, 4];

            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    memCompartida[i, j] = 1;
                }
            }

            cacheDatos = new int[4, 6];
            for (int i = 0; i < 4; ++i)
            {
                cacheDatos[i, 4] = -1;
            }

            directorio = new int[8, 5];
            if (numeroProcesador == 1)
            {
                for (int i = 0; i < 8; ++i)
                {
                    directorio[i, 0] = i;
                }
            }
            else if (numeroProcesador == 2)
            {
                for (int i = 0; i < 8; ++i)
                {
                    directorio[i, 0] = i + 8;
                }
            }
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    directorio[i, 0] = i + 16;
                }
            }
            //trabajo del hilillo 0
           /*if (numeroProcesador == 2)
            {
                memCompartida[0, 0] = 0;
                memCompartida[0, 1] = 0;
                memCompartida[0, 2] = 0;
                memCompartida[0, 3] = 0;
                memCompartida[1, 0] = 0;
                memCompartida[1, 1] = 0;
            }
            if (numeroProcesador == 3)
            {
                memCompartida[0, 1] = 0;
            }*/
            
            this.numeroProcesador = numeroProcesador;
            this.sync = sync;
            hilillosTerminados = 0;
            quantumLocal = 0;
            indiceTXT = 0;
            reloj = 0;
            matContext[0, 33] = reloj;  // -> Para el reloj
            indContexto = 0;
        }

        // Se encarga de llamar a los metodos que buscan la instruccione en la cache de instrucciones y ejecuta la misma
        public void ejecutarInstruccion(int quantum, int r, ref Procesador a, ref Procesador b, ref Procesador c)
        {
            if (indiceTXT > 0)  // ¿Hay hilillos para ser ejecutados?
            {
                for (int i = 0; i < indiceTXT; ++i)
                {
                    matContext[i, 32] = tamTxt[i];
                    if (i > 0)
                    {
                        matContext[i, 33] = i * quantum;  // Ciclo de reloj estimado en el que se ejecutará el hilillo por primera vez
                    }
                }

                while (hilillosTerminados < indiceTXT)
                {
                    quantumLocal = quantum;
                    cargarContexto();
                    while (quantumLocal > 0 && hilillosTerminados < indiceTXT)
                    {
                        int[] instruccion = recuperarInstruccion();

                        PC += 4;

                        int codigoOperacion = instruccion[0];
                        int R1 = instruccion[1];
                        int R2 = instruccion[2];
                        int R3 = instruccion[3];     // n en el caso del DADDI

                        int direcccionMem;

                        switch (codigoOperacion)
                        {
                            case 2:     // JR
                                PC = registros[R1];
                                break;
                            case 3:     // JAL
                                registros[31] = PC;
                                PC += R3;
                                break;
                            case 4:     // BEQZ
                                if (registros[R1] == 0)
                                {
                                    PC += R3 * 4;
                                }
                                break;
                            case 5:     // BENZ
                                if (registros[R1] != 0)
                                {
                                    PC += R3 * 4;
                                }
                                break;
                            case 8:     // DADDI
                                registros[R2] = registros[R1] + R3;
                                break;
                            case 12:    // DMUL
                                registros[R3] = registros[R1] * registros[R2];
                                break;
                            case 14:    // DDIV
                                registros[R3] = registros[R1] / registros[R2];
                                break;
                            case 32:    // DADD
                                registros[R3] = registros[R1] + registros[R2];
                                break;
                            case 34:    // DSUB
                                registros[R3] = registros[R1] - registros[R2];
                                break;
                            case 35:    // LW
                                ejecutarLW(ref a, ref b, ref c, registros[R1] + R3, R2);
                                break;
                            case 43:    // SW
                                ejecutarSW(ref a, ref b, ref c, registros[R1] + R3, R2);
                                break;
                            case 50:    // LL
                                cargoExito = false;
                                ejecutarLW(ref a, ref b, ref c, registros[R1] + R3, R2);
                                if (cargoExito)
                                {
                                    int x = registros[R1] + R3;
                                    //Console.WriteLine("LOADLINK rl:" + x + "---" + numeroProcesador);
                                    cargoExito = false;
                                    registros[32] = registros[R1] + R3;//guarda el rl
                                    llactivo = 1;
                                    matContext[indContexto, 35] = registros[R1] + R3;//guarda el rl
                                }
                                break;
                            case 51:    // SC
                                guardoExito = false;
                                direcccionMem = registros[R1] + R3;
                                //Console.WriteLine("SC rl:" + " " + registros[32] + "llactivo ?" + llactivo +"---" + numeroProcesador);
                                if (registros[32] == direcccionMem)
                                {
                                    //Console.WriteLine("conditional" + " " + numeroProcesador);
                                    ejecutarSC(ref a, ref b, ref c, registros[R1] + R3, R2);
                                    if (guardoExito)
                                    {
                                        //Console.WriteLine("logre ejecutar sc----"+numeroProcesador);
                                        llactivo = 0;
                                        bloquell = 0;
                                        guardoExito = false;
                                    }   
                                }
                                else
                                {
                                    registros[32] = -1;//pongo el rl en -1
                                    registros[R1] = 0;
                                    llactivo = 0;
                                    bloquell = 0;
                                    //registros[33] = llactivo;
                                }
                                break;
                            case 63:    // Termino el hilillo
                                ++hilillosTerminados;
                                //Console.WriteLine("termine" + " " + numeroProcesador);
                                //Console.ReadKey();
                                PC = -1;    // "Stamp" para indicar que el hilillo ya se ejecutó en su totalidad
                                guardarContexto();
                                cargarContexto();
                                break;
                        }
                        --quantumLocal;
                        sync.SignalAndWait();
                    }
                    guardarContexto();
                }
            }
            else
            {
                sync.SignalAndWait();
            }
        }

        public void ejecutarLW(ref Procesador a, ref Procesador b, ref Procesador c, int dirMem, int numRegistro)
        {
            int bloque = dirMem / 16;                   // Bloque en memoria compartida donde esta el dato
            bloquell = bloque;
            int desplazamiento = (dirMem % 16) / 4;     // Numero de palabras a partir del bloque
            int indiceC = bloque % 4;                   // Donde deberia estar en cache
            int idMemCompartida = (dirMem / 128) + 1;   // En qué procesador esta el bloque en memoria compartida
            int numBloqueVictima = this.cacheDatos[indiceC, 4];
            int idMemVictima = 0;
            if (numBloqueVictima < 8)
            {
                idMemVictima = 1;
            }
            else if (numBloqueVictima < 16)
            {
                idMemVictima = 2;
            }
            else
            {
                idMemVictima = 3;
            }

            bool libere = false;

            if (Monitor.TryEnter(this.cacheDatos))
            {
                try
                {
                    if (this.cacheDatos[indiceC, 4] == bloque && this.cacheDatos[indiceC, 5] != 0) //4 esta el bloque,5 estado
                    {
                        registros[numRegistro] = this.cacheDatos[indiceC, desplazamiento];//cargamos al registro
                        libere = true;
                    }
                    else
                    {
                        //victima 
                        if (numBloqueVictima != -1)
                        {
                            if (this.cacheDatos[indiceC, 5] == 1)//si esta modificado
                            {
                                if (idMemVictima == 1)
                                {
                                    if (Monitor.TryEnter(a.directorio))
                                    {
                                        try
                                        {
                                            a.directorio[numBloqueVictima, 1] = 0;//unallocated
                                            a.directorio[numBloqueVictima, idMemVictima + 1] = 0;
                                            this.cacheDatos[indiceC, 5] = 0;//invalido
                                        }
                                        finally
                                        {
                                            Monitor.Exit(a.directorio);
                                        }
                                        guardarVictima(idMemVictima, numBloqueVictima, indiceC, ref a, ref b, ref c);
                                    }
                                    else
                                    {
                                        sync.SignalAndWait();
                                        PC -= 4;
                                        libere = true;
                                    }
                                }
                                else if (idMemVictima == 2)
                                {
                                    if (Monitor.TryEnter(b.directorio))
                                    {
                                        try
                                        {
                                            b.directorio[numBloqueVictima - 8, 1] = 0;//unallocated
                                            b.directorio[numBloqueVictima - 8, idMemVictima + 1] = 0;
                                            this.cacheDatos[indiceC, 5] = 0;//invalido
                                        }
                                        finally
                                        {
                                            Monitor.Exit(b.directorio);
                                        }
                                        guardarVictima(idMemVictima, numBloqueVictima, indiceC, ref a, ref b, ref c);
                                    }
                                    else
                                    {
                                        sync.SignalAndWait();
                                        PC -= 4;
                                        libere = true;
                                    }
                                }
                                else if (idMemVictima == 3)
                                {
                                    if (Monitor.TryEnter(c.directorio))
                                    {
                                        try
                                        {
                                            c.directorio[numBloqueVictima - 16, 1] = 0;//unallocated
                                            c.directorio[numBloqueVictima - 16, idMemVictima + 1] = 0;
                                            this.cacheDatos[indiceC, 5] = 0;//invalido
                                        }
                                        finally
                                        {
                                            Monitor.Exit(c.directorio);
                                        }
                                        guardarVictima(idMemVictima, numBloqueVictima, indiceC, ref a, ref b, ref c);
                                    }
                                    else
                                    {
                                        sync.SignalAndWait();
                                        PC -= 4;
                                        libere = true;
                                    }
                                }
                            }
                            else if (this.cacheDatos[indiceC, 5] == 2)//si esta compartido
                            {
                                if (idMemVictima == 1)
                                {
                                    libere = unallocate(ref a, numBloqueVictima, idMemVictima, 0);
                                }
                                else if (idMemVictima == 2)
                                {
                                    libere = unallocate(ref b, numBloqueVictima, idMemVictima, 8);
                                }
                                else if (idMemVictima == 3)
                                {
                                    libere = unallocate(ref c, numBloqueVictima, idMemVictima, 16);
                                }
                            }
                        }
                        //fin del victima
                    }

                    if (!libere)
                    {
                        Procesador x = retornaProcesador(idMemCompartida, ref a, ref b, ref c); // Procesador al que pertenece el bloque

                        if (Monitor.TryEnter(x.directorio))
                        {
                            try
                            {
                                int suma = 0;
                                if (idMemCompartida == 2)
                                {
                                    bloque = bloque - 8;
                                    suma = 8;
                                }
                                else if (idMemCompartida == 3)
                                {
                                    bloque = bloque - 16;
                                    suma = 16;
                                }

                                if (x.directorio[bloque, 1] == 1)//si esta modificado
                                {
                                    if (x.directorio[bloque, 2] == 1)
                                    {
                                        if (Monitor.TryEnter(a.cacheDatos))
                                        {
                                            try
                                            {
                                                escribirBloque(ref x, ref a, bloque, 0, numRegistro, indiceC, desplazamiento);
                                                traerBloque(ref x, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                            }
                                            finally
                                            {
                                                Monitor.Exit(a.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else if (x.directorio[bloque, 3] == 1)
                                    {
                                        if (Monitor.TryEnter(b.cacheDatos))
                                        {
                                            try
                                            {
                                                escribirBloque(ref x, ref b, bloque, 0, numRegistro, indiceC, desplazamiento);
                                                traerBloque(ref x, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                            }
                                            finally
                                            {
                                                Monitor.Exit(b.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else
                                    {
                                        if (Monitor.TryEnter(c.cacheDatos))
                                        {
                                            try
                                            {
                                                escribirBloque(ref x, ref c, bloque, 0, numRegistro, indiceC, desplazamiento);
                                                traerBloque(ref x, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                            }
                                            finally
                                            {
                                                Monitor.Exit(c.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    sync.SignalAndWait();

                                }
                                else  //si no esta modificado, es porque esta compartido o unallocated 
                                {
                                    //escribo en memoria
                                    if (idMemCompartida == 1)
                                    {
                                        traerBloque(ref x, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else if (idMemCompartida == 2)
                                    {
                                        traerBloque(ref x, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else
                                    {
                                        traerBloque(ref x, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                }
                            }
                            finally
                            {
                                Monitor.Exit(x.directorio);
                            }
                        }
                        else
                        {
                            sync.SignalAndWait();
                            PC -= 4;
                            libere = true;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this.cacheDatos);
                }
            }
            else
            {
                sync.SignalAndWait();
                PC -= 4;
            }
            if (!libere)
            {
                cargoExito = true;
            }
        }

        public void ejecutarSW(ref Procesador a, ref Procesador b, ref Procesador c, int dirMem, int numRegistro)
        {
            int bloque = dirMem / 16;                   // Bloque en memoria compartida donde esta el dato
            int desplazamiento = (dirMem % 16) / 4;     // Numero de palabras a partir del bloque
            int indiceC = bloque % 4;                   // Donde deberia estar en cache
            int idMemCompartida = (dirMem / 128) + 1;   // En qué procesador esta el bloque en memoria compartida
            int resta2;
            int idMemVictima;
            int numBloqueVictima;
            bool libere = false;

            int resta1 = 0;
            if (bloque < 8)
            {
                resta1 = 0;
            }
            else if (bloque < 16)
            {
                resta1 = 8;
            }
            else
            {
                resta1 = 16;
            }
            if (Monitor.TryEnter(this.cacheDatos))
            {
                try
                {
                    numBloqueVictima = this.cacheDatos[indiceC, 4];
                    idMemVictima = 0;
                    resta2 = 0;
                    if (numBloqueVictima < 8)
                    {
                        idMemVictima = 1;
                        resta2 = 0;
                    }
                    else if (numBloqueVictima < 16)
                    {
                        idMemVictima = 2;
                        resta2 = 8;
                    }
                    else
                    {
                        idMemVictima = 3;
                        resta2 = 16;
                    }
                    if (this.cacheDatos[indiceC, 4] == bloque && this.cacheDatos[indiceC, 5] != 0)//si el bloque del dato esta en mi cache
                    {
                        if (this.cacheDatos[indiceC, 5] == 1)//si el bloque esta modificado en mi cache
                        {
                            this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                            libere = true;
                            sync.SignalAndWait();
                        }
                        else if (this.cacheDatos[indiceC, 5] == 2)//es porque esta compartido. Si esta unallocated es lo mismo que un fallo
                        {
                            Procesador x = retornaProcesador(idMemCompartida, ref a, ref b, ref c);
                            if (Monitor.TryEnter(x.directorio))
                            {
                                try
                                {
                                    int contadorCompartidas = 0;
                                    bool estaComp1 = false;
                                    bool estaComp2 = false;
                                    bool estaComp3 = false;
                                    if (numeroProcesador == 1)
                                    {
                                        if (x.directorio[bloque - resta1, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else if (numeroProcesador == 2)
                                    {

                                        if (x.directorio[bloque - resta1, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else
                                    {
                                        if (x.directorio[bloque - resta1, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    if (contadorCompartidas == 1)//si esta compartida en una cache aparte de la mia
                                    {
                                        if (estaComp1)
                                        {
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else if (estaComp2)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else if(estaComp3)
                                        {
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                    }
                                    else if (contadorCompartidas == 2) //esta compartida en dos caches aparte de la mia
                                    {
                                        int bloqueo = 0;
                                        if (numeroProcesador == 1)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 2)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 3)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
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
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                    }
                                    else//esta compartida solo en mi cache
                                    {
                                        x.directorio[bloque - resta1, 1] = 1;//pongo el bloque modificado en el directorio
                                        this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                        this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado en mi cache
                                        sync.SignalAndWait();
                                    }
                                }
                                finally
                                {
                                    Monitor.Exit(x.directorio);
                                }
                            }
                            else
                            {
                                sync.SignalAndWait();
                                PC -= 4;
                                libere = true;
                            }
                        }
                    }
                    else //el bloque no esta en mi cache por lo tanto hay un bloque victima
                    {
                        Procesador y = retornaProcesador(idMemVictima, ref a, ref b, ref c);
                        if (numBloqueVictima != -1)
                        {
                            if (this.cacheDatos[indiceC, 5] == 1)//si el bloque victima esta modificado
                            {
                                if (Monitor.TryEnter(y.directorio))
                                {
                                    try
                                    {
                                        y.directorio[numBloqueVictima - resta2, 1] = 0;//lo ponemos como unallocated
                                        this.cacheDatos[indiceC, 5] = 0;//invalido el bloque n mi cache
                                        guardarVictima(idMemVictima, numBloqueVictima, indiceC, ref a, ref b, ref c);
                                        sync.SignalAndWait();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(y.directorio);
                                    }
                                }
                                else
                                {
                                    sync.SignalAndWait();
                                    PC -= 4;
                                    libere = true;
                                }
                            }
                            else if (this.cacheDatos[indiceC, 5] == 1)//si el bloque victima esta compartido, unallocated no importa nada mas se le cae encima
                            {
                                if (Monitor.TryEnter(y.directorio))
                                {
                                    try
                                    {
                                        y.directorio[numBloqueVictima - resta2, numeroProcesador + 1] = 0;//decimos que ya no esta compartido en mi
                                        if (y.directorio[numBloqueVictima - resta2, 2] == 0 && y.directorio[numBloqueVictima - resta2, 3] == 0 && y.directorio[numBloqueVictima - resta2, 4] == 0)//si ya nadie lo compartido
                                        {
                                            y.directorio[numBloqueVictima - resta2, 1] = 0;//ponemos el bloque como unallocated
                                        }
                                        this.cacheDatos[indiceC, 5] = 0;//invalido el bloque en mi cache
                                        sync.SignalAndWait();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(y.directorio);
                                    }
                                }
                                else
                                {
                                    sync.SignalAndWait();
                                    PC -= 4;
                                    libere = true;
                                }
                            }
                        }
                    }//fin del bloque victima
                    if (!libere)
                    {
                        Procesador z = retornaProcesador(idMemCompartida, ref a, ref b, ref c);

                        int suma = 0;
                        if (idMemCompartida == 2)
                        {
                            bloque = bloque - 8;
                            suma = 8;
                        }
                        else if (idMemCompartida == 3)
                        {
                            bloque = bloque - 16;
                            suma = 16;
                        }
                        if (Monitor.TryEnter(z.directorio))
                        {
                            try
                            {
                                if (z.directorio[bloque, 1] == 0)// si el bloque en el que quiero escribir esta unallocated
                                {
                                    z.directorio[bloque, 1] = 1;//pongo el estado en modificado
                                    z.directorio[bloque, numeroProcesador + 1] = 1;//pongo que esta modificado en mi procesador
                                    //escribo en memoria
                                    if (idMemCompartida == 1)
                                    {
                                        traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else if (idMemCompartida == 2)
                                    {
                                        traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else
                                    {
                                        traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//escribo el dato en mi cache
                                    sync.SignalAndWait();
                                }
                                else if (z.directorio[bloque, 1] == 1)//si el estado del bloque esta modificado
                                {
                                    if (z.directorio[bloque, 2] == 1)//si esta modificado en el procesador 1
                                    {
                                        if (Monitor.TryEnter(a.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                z.directorio[bloque, a.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                a.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                escribirBloque2(ref z, ref a, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(a.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else if (z.directorio[bloque, 3] == 1)//si esta modificado en el porcesador 2
                                    {
                                        if (Monitor.TryEnter(b.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                b.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                z.directorio[bloque, b.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                escribirBloque2(ref z, ref b, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(b.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else//modificado en el procesador 3
                                    {
                                        if (Monitor.TryEnter(c.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                c.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                z.directorio[bloque, c.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                escribirBloque2(ref z, ref c, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(c.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                }
                                else//si el estado del bloque es compartido
                                {
                                    int contadorCompartidas = 0;
                                    bool estaComp1 = false;
                                    bool estaComp2 = false;
                                    bool estaComp3 = false;
                                    //Procesador x = retornaProcesador(idMemCompartida, ref a, ref b, ref c);
                                    if (numeroProcesador == 1)
                                    {
                                        if (z.directorio[bloque, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else if (numeroProcesador == 2)
                                    {
                                        if (z.directorio[bloque, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else
                                    {
                                        if (z.directorio[bloque, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    if (contadorCompartidas == 1)//si esta compartida en una cache
                                    {
                                        if (estaComp1)
                                        {
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else if (estaComp2)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else
                                        {
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                    }
                                    else if (contadorCompartidas == 2) //esta compartida en dos caches aparte de la mia
                                    {
                                        int bloqueo = 0;
                                        if (numeroProcesador == 1)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 2)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }

                                        }
                                        if (numeroProcesador == 3)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
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
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                        }
                                    }
                                }//fin del bloque compartido y de revisar
                            }
                            finally
                            {
                                Monitor.Exit(z.directorio);
                            }
                        }
                        else
                        {
                            sync.SignalAndWait();
                            PC -= 4;
                            libere = true;
                        }
                    }

                }
                finally
                {
                    Monitor.Exit(this.cacheDatos);
                }
            }
            else
            {
                sync.SignalAndWait();
                PC -= 4;
                libere = true;
            }
        }

        public void ejecutarSC(ref Procesador a, ref Procesador b, ref Procesador c, int dirMem, int numRegistro)
        {
            int bloque = dirMem / 16;                   // Bloque en memoria compartida donde esta el dato
            int desplazamiento = (dirMem % 16) / 4;     // Numero de palabras a partir del bloque
            int indiceC = bloque % 4;                   // Donde deberia estar en cache
            int idMemCompartida = (dirMem / 128) + 1;   // En qué procesador esta el bloque en memoria compartida
            int resta2;
            int idMemVictima;
            int numBloqueVictima;
            bool libere = false;

            int resta1 = 0;
            if (bloque < 8)
            {
                resta1 = 0;
            }
            else if (bloque < 16)
            {
                resta1 = 8;
            }
            else
            {
                resta1 = 16;
            }
            if (Monitor.TryEnter(this.cacheDatos))
            {
                try
                {
                    numBloqueVictima = this.cacheDatos[indiceC, 4];
                    idMemVictima = 0;
                    resta2 = 0;
                    if (numBloqueVictima < 8)
                    {
                        idMemVictima = 1;
                        resta2 = 0;
                    }
                    else if (numBloqueVictima < 16)
                    {
                        idMemVictima = 2;
                        resta2 = 8;
                    }
                    else
                    {
                        idMemVictima = 3;
                        resta2 = 16;
                    }
                    if (this.cacheDatos[indiceC, 4] == bloque && this.cacheDatos[indiceC, 5] != 0)//si el bloque del dato esta en mi cache
                    {
                        if (this.cacheDatos[indiceC, 5] == 1)//si el bloque esta modificado en mi cache
                        {
                            this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                            libere = true;
                            sync.SignalAndWait();
                        }
                        else if (this.cacheDatos[indiceC, 5] == 2)//es porque esta compartido. Si esta unallocated es lo mismo que un fallo
                        {
                            Procesador x = retornaProcesador(idMemCompartida, ref a, ref b, ref c);
                            if (Monitor.TryEnter(x.directorio))
                            {
                                try
                                {
                                    int contadorCompartidas = 0;
                                    bool estaComp1 = false;
                                    bool estaComp2 = false;
                                    bool estaComp3 = false;
                                    if (numeroProcesador == 1)
                                    {
                                        if (x.directorio[bloque - resta1, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else if (numeroProcesador == 2)
                                    {

                                        if (x.directorio[bloque - resta1, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else
                                    {
                                        if (x.directorio[bloque - resta1, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (x.directorio[bloque - resta1, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    if (contadorCompartidas == 1)//si esta compartida en una cache aparte de la mia
                                    {
                                        if (estaComp1)
                                        {
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    if (a.llactivo == 1 && a.bloquell == bloque)
                                                    {
                                                        a.registros[32] = -1;  //cambio el rl 
                                                        a.llactivo = 0;
                                                        a.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else if (estaComp2)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    if (b.llactivo == 1 && b.bloquell == bloque)
                                                    {
                                                        b.registros[32] = -1;  //cambio el rl 
                                                        b.llactivo = 0;
                                                        b.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else
                                        {
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    if (c.llactivo == 1 && c.bloquell == bloque)
                                                    {
                                                        c.registros[32] = -1;  //cambio el rl 
                                                        c.llactivo = 0;
                                                        c.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                    }
                                    else if (contadorCompartidas == 2) //esta compartida en dos caches aparte de la mia
                                    {
                                        int bloqueo = 0;
                                        if (numeroProcesador == 1)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (b.llactivo == 1 && b.bloquell == bloque)
                                                    {
                                                        b.registros[32] = -1;  //cambio el rl 
                                                        b.llactivo = 0;
                                                        b.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (c.llactivo == 1 && c.bloquell == bloque)
                                                    {
                                                        c.registros[32] = -1;  //cambio el rl 
                                                        c.llactivo = 0;
                                                        c.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 2)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (a.llactivo == 1 && a.bloquell == bloque)
                                                    {
                                                        a.registros[32] = -1;  //cambio el rl 
                                                        a.llactivo = 0;
                                                        a.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (c.llactivo == 1 && c.bloquell == bloque)
                                                    {
                                                        c.registros[32] = -1;  //cambio el rl 
                                                        c.llactivo = 0;
                                                        c.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 3)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (a.llactivo == 1 && a.bloquell == bloque)
                                                    {
                                                        a.registros[32] = -1;  //cambio el rl 
                                                        a.llactivo = 0;
                                                        a.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
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
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    x.directorio[bloque - resta1, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    bloqueo++;
                                                    if (b.llactivo == 1 && b.bloquell == bloque)
                                                    {
                                                        b.registros[32] = -1;  //cambio el rl 
                                                        b.llactivo = 0;
                                                        b.bloquell = 0;
                                                    }
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                            else
                                            {
                                                x.directorio[bloque - resta1, 1] = 1;  // pongo el estado modificado 
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                sync.SignalAndWait();
                                            }
                                        }
                                    }
                                    else//esta compartida solo en mi cache
                                    {
                                        x.directorio[bloque - resta1, 1] = 1;//pongo el bloque modificado en el directorio
                                        this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                        this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado en mi cache
                                        sync.SignalAndWait();
                                    }
                                }
                                finally
                                {
                                    Monitor.Exit(x.directorio);
                                }
                            }
                            else
                            {
                                sync.SignalAndWait();
                                PC -= 4;
                                libere = true;
                            }
                        }
                    }
                    else //el bloque no esta en mi cache por lo tanto hay un bloque victima
                    {
                        Procesador y = retornaProcesador(idMemVictima, ref a, ref b, ref c);
                        if (numBloqueVictima != -1)
                        {
                            if (this.cacheDatos[indiceC, 5] == 1)//si el bloque victima esta modificado
                            {
                                if (Monitor.TryEnter(y.directorio))
                                {
                                    try
                                    {
                                        y.directorio[numBloqueVictima - resta2, 1] = 0;//lo ponemos como unallocated
                                        this.cacheDatos[indiceC, 5] = 0;//invalido el bloque n mi cache
                                        guardarVictima(idMemVictima, numBloqueVictima, indiceC, ref a, ref b, ref c);
                                        sync.SignalAndWait();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(y.directorio);
                                    }
                                }
                                else
                                {
                                    sync.SignalAndWait();
                                    PC -= 4;
                                    libere = true;
                                }
                            }
                            else if (this.cacheDatos[indiceC, 5] == 1)//si el bloque victima esta compartido, unallocated no importa nada mas se le cae encima
                            {
                                if (Monitor.TryEnter(y.directorio))
                                {
                                    try
                                    {
                                        y.directorio[numBloqueVictima - resta2, numeroProcesador + 1] = 0;//decimos que ya no esta compartido en mi
                                        if (y.directorio[numBloqueVictima - resta2, 2] == 0 && y.directorio[numBloqueVictima - resta2, 3] == 0 && y.directorio[numBloqueVictima - resta2, 4] == 0)//si ya nadie lo compartido
                                        {
                                            y.directorio[numBloqueVictima - resta2, 1] = 0;//ponemos el bloque como unallocated
                                        }
                                        this.cacheDatos[indiceC, 5] = 0;//invalido el bloque en mi cache
                                        sync.SignalAndWait();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(y.directorio);
                                    }
                                }
                                else
                                {
                                    sync.SignalAndWait();
                                    PC -= 4;
                                    libere = true;
                                }
                            }
                        }
                    }//fin del bloque victima
                    if (!libere)
                    {
                        Procesador z = retornaProcesador(idMemCompartida, ref a, ref b, ref c);

                        int suma = 0;
                        if (idMemCompartida == 2)
                        {
                            bloque = bloque - 8;
                            suma = 8;
                        }
                        else if (idMemCompartida == 3)
                        {
                            bloque = bloque - 16;
                            suma = 16;
                        }
                        if (Monitor.TryEnter(z.directorio))
                        {
                            try
                            {
                                if (z.directorio[bloque, 1] == 0)// si el bloque en el que quiero escribir esta unallocated
                                {
                                    z.directorio[bloque, 1] = 1;//pongo el estado en modificado
                                    z.directorio[bloque, numeroProcesador + 1] = 1;//pongo que esta modificado en mi procesador
                                    //escribo en memoria
                                    if (idMemCompartida == 1)
                                    {
                                        traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else if (idMemCompartida == 2)
                                    {
                                        traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    else
                                    {
                                        traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                    }
                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//escribo el dato en mi cache
                                    sync.SignalAndWait();
                                }
                                else if (z.directorio[bloque, 1] == 1)//si el estado del bloque esta modificado
                                {
                                    if (z.directorio[bloque, 2] == 1)//si esta modificado en el procesador 1
                                    {
                                        if (Monitor.TryEnter(a.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                z.directorio[bloque, a.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                a.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                escribirBloque2(ref z, ref a, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(a.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else if (z.directorio[bloque, 3] == 1)//si esta modificado en el porcesador 2
                                    {
                                        if (Monitor.TryEnter(b.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                b.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                z.directorio[bloque, b.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                escribirBloque2(ref z, ref b, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(b.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                    else//modificado en el procesador 3
                                    {
                                        if (Monitor.TryEnter(c.cacheDatos))
                                        {
                                            try
                                            {
                                                z.directorio[bloque, 1] = 0;//pongo el estado en invalido
                                                c.cacheDatos[indiceC, 5] = 0;//invalido el bloque en la cache
                                                z.directorio[bloque, c.numeroProcesador + 1] = 0;//pongo que ya no esta modificado en ese procesador
                                                escribirBloque2(ref z, ref c, bloque, 0, 0, indiceC, desplazamiento);
                                                this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];//modifico el dato en mi cache
                                                this.cacheDatos[indiceC, 5] = 1;//pongo que ahora esta modificado por mi
                                                z.directorio[bloque, 1] = 1;//pongo que esta modificado por mi
                                                z.directorio[bloque, this.numeroProcesador + 1] = 1;
                                                sync.SignalAndWait();
                                            }
                                            finally
                                            {
                                                Monitor.Exit(c.cacheDatos);
                                            }
                                        }
                                        else
                                        {
                                            sync.SignalAndWait();
                                            PC -= 4;
                                            libere = true;
                                        }
                                    }
                                }
                                else//si el estado del bloque es compartido
                                {
                                    int contadorCompartidas = 0;
                                    bool estaComp1 = false;
                                    bool estaComp2 = false;
                                    bool estaComp3 = false;
                                   // Procesador x = retornaProcesador(idMemCompartida, ref a, ref b, ref c);
                                    if (numeroProcesador == 1)
                                    {
                                        if (z.directorio[bloque, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else if (numeroProcesador == 2)
                                    {
                                        if (z.directorio[bloque, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 4] == 1)//compartido en el procesador 3
                                        {
                                            estaComp3 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    else
                                    {
                                        if (z.directorio[bloque, 2] == 1)//compartido en el procesador 1
                                        {
                                            estaComp1 = true;
                                            contadorCompartidas++;
                                        }
                                        if (z.directorio[bloque, 3] == 1)//compartido en el procesador 2
                                        {
                                            estaComp2 = true;
                                            contadorCompartidas++;
                                        }
                                    }
                                    if (contadorCompartidas == 1)//si esta compartida en una cache
                                    {
                                        if (estaComp1)
                                        {
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else if (estaComp2)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                        else
                                        {
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            else
                                            {
                                                sync.SignalAndWait();
                                                PC -= 4;
                                                libere = true;
                                            }
                                        }
                                    }
                                    else if (contadorCompartidas == 2) //esta compartida en dos caches aparte de la mia
                                    {
                                        int bloqueo = 0;
                                        if (numeroProcesador == 1)
                                        {
                                            if (Monitor.TryEnter(b.cacheDatos))
                                            {
                                                try
                                                {
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                        }
                                        if (numeroProcesador == 2)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(a.cacheDatos);
                                                }
                                            }
                                            if (Monitor.TryEnter(c.cacheDatos))
                                            {
                                                try
                                                {
                                                    c.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 4] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref c, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(c.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }

                                        }
                                        if (numeroProcesador == 3)
                                        {
                                            bloqueo = 0;
                                            if (Monitor.TryEnter(a.cacheDatos))
                                            {
                                                try
                                                {
                                                    a.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 2] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref a, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
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
                                                    b.cacheDatos[indiceC, 5] = 0; // pongo el bloque en esa cache como invalido
                                                    z.directorio[bloque, 1] = 1;  // pongo el estado modificado 
                                                    z.directorio[bloque, 3] = 0;  // pongo en el campo del procesador 1 un 0
                                                    traerBloque2(ref z, ref b, bloque, suma, numRegistro, indiceC, desplazamiento);
                                                    this.cacheDatos[indiceC, desplazamiento] = registros[numRegistro];
                                                    this.cacheDatos[indiceC, 5] = 1;//pongo el bloque como modificado
                                                    bloqueo++;
                                                    sync.SignalAndWait();
                                                }
                                                finally
                                                {
                                                    Monitor.Exit(b.cacheDatos);
                                                }
                                            }
                                            if (bloqueo == 1 || bloqueo == 0)
                                            {
                                                PC -= 4;
                                                libere = true;
                                                sync.SignalAndWait();
                                            }
                                        }
                                    }
                                }//fin del bloque compartido y de revisar
                            }
                            finally
                            {
                                Monitor.Exit(z.directorio);
                            }
                        }
                        else
                        {
                            sync.SignalAndWait();
                            PC -= 4;
                            libere = true;
                        }
                    }

                }
                finally
                {
                    Monitor.Exit(this.cacheDatos);
                }
            }
            else
            {
                sync.SignalAndWait();
                PC -= 4;
                libere = true;
            }
            if (!libere)
            {
                guardoExito = true;
            }
        }

        // Busca las instruccion a ejecutar en la cache de instrucciones
        public int[] recuperarInstruccion()
        {
            int bloque = PC / 16;      // Para buscar en la etiqueta de la memoria caché
            int desplazamiento = PC - (16 * bloque);    // De aqui se saca el numero de columna. A partir de donde comienza el bloque, cuantas palabras (instricciones) me desplazo (brinco)
            int indice = bloque % 4;    // Sirve para saber la posicion (fila) de la memoria caché en que está

            if (cacheInstrucciones[indice, 16] != bloque)    // ¿Hay fallo de caché?
            {
                traerInstruccion(bloque, indice);
            }

            int[] instruccion = new int[4];

            for (int i = 0; i < 4; ++i, ++desplazamiento)
            {
                instruccion[i] = cacheInstrucciones[indice, desplazamiento];
            }

            return instruccion;
        }

        // Cargar la instruccion de la memoria principal a la cacheInstrucciones de intstrucciones
        public void traerInstruccion(int bloque, int indice)
        {
            for (int i = 0; i < 16; ++i)
            {
                // Enviar 16 señales para que los otros procesadores puedan seguir trabajando
                sync.SignalAndWait();
            }

            int fin = bloque * 16 + 16;
            for (int i = bloque * 16, j = 0; i < fin; ++i, ++j)
            {
                cacheInstrucciones[indice, j] = memPrincipal[i];
            }
            cacheInstrucciones[indice, 16] = bloque;
        }

        // Guarda los registros de la instruccion actual y el PC en la matriz de cambio de contexto
        public void guardarContexto()
        {
            for (int i = 0; i < 32; ++i)
            {
                matContext[indContexto, i] = registros[i];
            }
            matContext[indContexto, 32] = PC;
            matContext[indContexto, 35] = -1;//rl
            matContext[indContexto, 34] = reloj;    // Sobreescribe la hora de fin hasta que se deje de ejecutar
            registros = new int[35];

            if (hilillosTerminados < indiceTXT)  // ¿Voy a seguir ejecutando hilillos?
            {
                ++indContexto;
                if (indContexto == indiceTXT)
                {
                    indContexto = 0;
                }
                while (matContext[indContexto, 32] == -1)   // Verifica si el hilillo que se cargaría no ha sido ya ejecutado por completo
                {
                    ++indContexto;
                    if (indContexto == indiceTXT)
                    {
                        indContexto = 0;
                    }
                }
            }
        }

        // Carga los registros de la instruccion actual y el PC en la matriz de cambio de contexto
        public void cargarContexto()
        {
            for (int i = 0; i < 32; ++i)
            {
                registros[i] = matContext[indContexto, i];
            }
            PC = matContext[indContexto, 32];

            if (reloj < matContext[indContexto, 33]) // En caso que se cargue el hilillo antes del ciclo de reloj estimado
            {
                matContext[indContexto, 33] = reloj;
            }
            registros[32] = matContext[indContexto, 35];//cargar rl
        }

        // Guarda en que posicion de memoria comienza cada hilillo (archivo)
        public void iniciaArchivo(int direccion)
        {
            tamTxt[indiceTXT] = direccion;
            ++indiceTXT;
        }

        public void guardarVictima(int idMemCompartida, int bloque2, int indiceC, ref Procesador a, ref Procesador b, ref Procesador c)
        {
            for (int i = 0; i < 32; ++i)//tiempo que duro en escribir un bloque a memoria
            {
                // Enviar 32 señales para que los otros procesadores puedan seguir trabajando
                sync.SignalAndWait();
            }
            if (idMemCompartida == 1)
            {

                for (int i = 0, j = 0; i < 4; ++i, j += 4)
                {
                    a.memCompartida[bloque2, i] = this.cacheDatos[indiceC, i];
                }
            }
            else if (idMemCompartida == 2)
            {
                for (int i = 0, j = 0; i < 4; ++i, j += 4)
                {
                    b.memCompartida[bloque2 - 8, i] = this.cacheDatos[indiceC, i];
                }
            }
            else if (idMemCompartida == 3)
            {
                for (int i = 0, j = 0; i < 4; ++i, j += 4)
                {
                    c.memCompartida[bloque2 - 16, i] = this.cacheDatos[indiceC, i];
                }
            }
        }

        public bool unallocate(ref Procesador x, int numBloqueVictima, int idMemVictima, int resta)
        {
            bool libere = false;
            if (Monitor.TryEnter(x.directorio))
            {
                try
                {
                    x.directorio[numBloqueVictima - resta, idMemVictima + 1] = 0;//para saber que ya no lo tengo compartido
                    //para saber si solo estaba compartido en mi cache
                    if (x.directorio[numBloqueVictima - resta, 3] == 0 && x.directorio[numBloqueVictima - resta, 4] == 0)
                    {
                        x.directorio[numBloqueVictima - resta, 1] = 0;//unallocated
                    }
                }
                finally
                {
                    Monitor.Exit(x.directorio);
                }
            }
            else
            {
                sync.SignalAndWait();
                PC -= 4;
                libere = true;
            }

            return libere;
        }

        public Procesador retornaProcesador(int idMemCompartida, ref Procesador a, ref Procesador b, ref Procesador c)
        {
            if (idMemCompartida == 1)
            {
                return a;
            }
            else if (idMemCompartida == 2)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        public void traerBloque(ref Procesador x, ref Procesador y, int bloque, int suma, int numRegistro, int indiceC, int desplazamiento)
        {
            //modifico el directorio a compartido;
            x.directorio[bloque, 1] = 2;
            //sync.SignalAndWait();
            x.directorio[bloque, this.numeroProcesador + 1] = 1;
            //sync.SignalAndWait();

            //Guarda el numero de blque;
            this.cacheDatos[indiceC, 4] = bloque + suma;
            //sync.SignalAndWait();

            //modifico la cache a compartido;
            this.cacheDatos[indiceC, 5] = 2;
            //sync.SignalAndWait();

            // copio el bloque de memoria compartida a mi cache;
            for (int i = 0; i < 2; ++i)
            {
                sync.SignalAndWait();
            }
            for (int i = 0; i < 4; ++i)
            {
                this.cacheDatos[indiceC, i] = x.memCompartida[bloque, i];
            }

            //guardo el dato en el registro;
            this.registros[numRegistro] = this.cacheDatos[indiceC, desplazamiento];
        }

        public void traerBloque2(ref Procesador x, ref Procesador y, int bloque, int suma, int numRegistro, int indiceC, int desplazamiento)
        {

            //Guarda el numero de blque;
            this.cacheDatos[indiceC, 4] = bloque + suma;
            //sync.SignalAndWait();

            //modifico la cache a modificado;
            this.cacheDatos[indiceC, 5] = 1;
            //sync.SignalAndWait();

            // copio el bloque de memoria compartida a mi cache;
            for (int i = 0; i < 2; ++i)
            {
                sync.SignalAndWait();
            }
            for (int i = 0; i < 4; ++i)
            {
                this.cacheDatos[indiceC, i] = x.memCompartida[bloque, i];
            }
        }

        public void escribirBloque(ref Procesador x, ref Procesador y, int bloque, int resta, int numRegistro, int indiceC, int desplazamiento)
        {
            //escribo el bloque en la memoria compartida casa;
            for (int i = 0; i < 2; ++i)//normalmente es 32
            {
                sync.SignalAndWait();
            }
            for (int i = 0; i < 4; ++i)
            {
                x.memCompartida[bloque, i] = y.cacheDatos[indiceC, i];
            }
        }

        public void escribirBloque2(ref Procesador x, ref Procesador y, int bloque, int resta, int numRegistro, int indiceC, int desplazamiento)
        {
            //escribo el bloque en la memoria compartida casa y luego a mi cache;
            for (int i = 0; i < 2; ++i)//normalmente es 32
            {
                sync.SignalAndWait();
            }
            for (int i = 0; i < 4; ++i)
            {
                x.memCompartida[bloque, i] = y.cacheDatos[indiceC, i];
            }
            for (int i = 0; i < 5; ++i)
            {
                this.cacheDatos[indiceC, i] = y.cacheDatos[indiceC, i];
            }
        }
    }
}
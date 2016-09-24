using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProyectoArquitectura
{
    class Controlador
    {
        private static int numeroHilos; // Cuantos hilos (hilillos) se van a ejecutar en los procesadores (hilos)
        private static int quantum;     // Valor del quantum
        private static string path;     // Ubicacion de los hilillos
        public Barrier sync;            // Sincronización
        public Procesador p1;           // Instancia para el procesador 1
        public Procesador p2;           // Instancia para el procesador 2
        public Procesador p3;           // Instancia para el procesador 3
        public int reloj;               // Reloj de la simulación
        public string[] nombreArchivos; // Array con los nombres de los hilillos

        public Controlador(int n, int q, string p)
        {
            numeroHilos = n;
            quantum = q;
            path = p;
            reloj = 0;

            sync = new Barrier(4);  // Crea un nuevo barrier con 4 participantes (los 3 procesadores y el hilo controlador)

            // Se inicializan los procesadores y la variable para sincronización se pasa como parametro
            p1 = new Procesador(1, sync);
            p2 = new Procesador(2, sync);
            p3 = new Procesador(3, sync);

        }

        public void iniciarPrograma()
        {
            cargarTxt();
            crearHilillos();
            imprimirInformacion();
        }

        // Busca los txt (hilillos) y los carga en la memoria principal de cada procesador
        public void cargarTxt()
        {

            Procesador p = new Procesador();

            int indiceMem = 0;  // Para movernos por el array de la memoria principal
            int indiceMem1 = 0;
            int indiceMem2 = 0;
            int indiceMem3 = 0;

            nombreArchivos = System.IO.Directory.GetFiles(path, "*.txt");
            // Carga los TXT a la memoria de cada procesador
            for (int i = 1; i <= numeroHilos; ++i)
            {
                if (i % 3 == 1)
                {
                    p = p1;
                    indiceMem = indiceMem1;

                }
                else if (i % 3 == 2)
                {
                    p = p2;
                    indiceMem = indiceMem2;
                }
                else
                {
                    p = p3;
                    indiceMem = indiceMem3;
                }

                if (indiceMem != 0)
                {
                    indiceMem += 4;
                }
                p.iniciaArchivo(indiceMem);

                string[] instrucciones = System.IO.File.ReadAllLines(nombreArchivos[i - 1]);
                // Cada string de instrucciones se descompone y pasa a int
                for (int j = 0; j < instrucciones.Length; ++j, ++indiceMem)  // Se itera por el array de instrucciones
                {
                    string numero = "";     // El numero que se va a parsear
                    for (int k = 0; k < instrucciones[j].Length; ++k) // Se van a separar los numeros
                    {
                        if (instrucciones[j][k] != ' ')
                        {
                            numero += instrucciones[j][k];
                        }
                        else
                        {
                            p.memPrincipal[indiceMem] = Int32.Parse(numero);
                            numero = "";
                            ++indiceMem;
                        }
                    }
                    p.memPrincipal[indiceMem] = Int32.Parse(numero);
                    numero = "";
                }

                if (i % 3 == 1)
                {
                    indiceMem1 = indiceMem;
                }
                else if (i % 3 == 2)
                {
                    indiceMem2 = indiceMem;
                }
                else
                {
                    indiceMem3 = indiceMem;
                }
            }
        }

        // Crea e inicia los hilos (procesadores) donde se corren los hilillos
        public void crearHilillos()
        {
            Thread Procesador1 = new Thread(delegate()
            {
                p1.ejecutarInstruccion(quantum, reloj, ref p1, ref p2, ref p3);
            });

            Thread Procesador2 = new Thread(delegate()
            {
                p2.ejecutarInstruccion(quantum, reloj, ref p1, ref p2, ref p3);
            });

            Thread Procesador3 = new Thread(delegate()
            {
                p3.ejecutarInstruccion(quantum, reloj, ref p1, ref p2, ref p3);
            });

            // Inicia los hilos correspondientes a los procesadores
            Procesador1.Start();
            Procesador2.Start();
            Procesador3.Start();

            controlarReloj(Procesador1, Procesador2, Procesador3);
        }

        // Incrementa el reloj del sistema mientras los hilos (procesadores) esten corriendo
        public void controlarReloj(Thread p1, Thread p2, Thread p3)
        {
            bool p1Vivo = true;
            bool p2Vivo = true;
            bool p3Vivo = true;

            Console.WriteLine("\nAvance del reloj:");

            while (sync.ParticipantCount > 1)
            {
                if (!p1.IsAlive && p1Vivo)
                {
                    p1Vivo = false;
                    sync.RemoveParticipant();
                }
                if (!p2.IsAlive && p2Vivo)
                {
                    p2Vivo = false;
                    sync.RemoveParticipant();
                }
                if (!p3.IsAlive && p3Vivo)
                {
                    p3Vivo = false;
                    sync.RemoveParticipant();
                }

                // Incrementa el reloj y lo asigna a los 3 procesadores
                ++reloj;
                this.p1.reloj = reloj;
                this.p2.reloj = reloj;
                this.p3.reloj = reloj;

                sync.SignalAndWait();   // Envia una señal y espera a que los 3 procesadores ejecuten envien señal

                Console.WriteLine(reloj);
            }
        }

        public void imprimirInformacion()
        {

            for (int i = 0; i < p1.indiceTXT; ++i)
            {
                Console.WriteLine("\nRegistros del hilillo " + (i + 1) + " del procesador 1: ");

                for (int j = 0; j < 32; ++j)
                {
                    Console.WriteLine("Registro" + j + ": " + p1.matContext[i, j]);
                }
                Console.WriteLine("Reloj inicio: " + p1.matContext[i, 33]);
                Console.WriteLine("Reloj fin: " + p1.matContext[i, 34]);
                Console.WriteLine("Tardó " + (p1.matContext[i, 34] - p1.matContext[i, 33]) + " ciclos en ejecutarse");
            }

            for (int i = 0; i < p2.indiceTXT; ++i)
            {
                Console.WriteLine("\nRegistros del hilillo " + (i + 1) + " del procesador 2: ");

                for (int j = 0; j < 32; ++j)
                {
                    Console.WriteLine("Registro" + j + "= " + p2.matContext[i, j]);
                }
                Console.WriteLine("Reloj inicio: " + p2.matContext[i, 33]);
                Console.WriteLine("Reloj fin: " + p2.matContext[i, 34]);
                Console.WriteLine("Tardó " + (p2.matContext[i, 34] - p2.matContext[i, 33]) + " ciclos en ejecutarse");
            }

            for (int i = 0; i < p3.indiceTXT; ++i)
            {
                Console.WriteLine("\nRegistros del hilillo " + (i + 1) + " del procesador 3: ");

                for (int j = 0; j < 32; ++j)
                {
                    Console.WriteLine("Registro" + j + "= " + p3.matContext[i, j]);
                }
                Console.WriteLine("Reloj inicio: " + p3.matContext[i, 33]);
                Console.WriteLine("Reloj fin: " + p3.matContext[i, 34]);
                Console.WriteLine("Tardó " + (p3.matContext[i, 34] - p3.matContext[i, 33]) + " ciclos en ejecutarse");
            }

            Console.WriteLine();

            for (int i = 1; i <= numeroHilos; ++i)
            {
                if (i % 3 == 0)
                {
                    Console.WriteLine(nombreArchivos[i - 1] + " corrió en el procesador 3");
                }
                else
                {
                    Console.WriteLine(nombreArchivos[i - 1] + " corrió en el procesador " + (i % 3));
                }
            }
            Console.WriteLine("Memoria Compartida del Procesador 1");
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(p1.memCompartida[i, j] + " ,");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Memoria Compartida del Procesador 2");
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(p2.memCompartida[i, j] + " ,");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Memoria Compartida del Procesador 3");
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(p3.memCompartida[i, j] + " ,");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Cache de Datos del Procesador 1");
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 6; ++j)
                {
                    Console.Write(p1.cacheDatos[i, j] + " ,");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Cache de Datos del Procesador 2");
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 6; ++j)
                {
                    Console.Write(p2.cacheDatos[i, j] + " ,");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Cache de Datos del Procesador 3");
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 6; ++j)
                {
                    Console.Write(p3.cacheDatos[i, j] + " ,");
                }
                Console.WriteLine("");
            }
        }
    }
}
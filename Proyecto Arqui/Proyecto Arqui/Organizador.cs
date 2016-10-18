using System;
using System.Threading;

namespace Proyecto_Arqui
{
    public class Organizador
    {
        int _CANTHILILLOS;
        int _QUANTUM;
        int reloj;
        Procesador principal;
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
            principal = new Procesador();
            principal.inicializarProcesadorPrincipal();
            principal.Visible = true;
            principal.cargarInstrucciones(path);
        }

        //Crea los hilos que corresponden a cada uno de los nucleos o procesadores, en total se simulan 3 procesadores
        private void inicializaProcesadores()
        {
            Thread hilo_proc1 = new Thread(delegate () { procesador1.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, ref principal); });
            Thread hilo_proc2 = new Thread(delegate() { procesador2.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, ref principal); });
            Thread hilo_proc3 = new Thread(delegate() { procesador3.ejecutarInstrs(_QUANTUM, ref procesador1, ref procesador2, ref procesador3, ref principal); });

            hilo_proc1.Start();
            hilo_proc2.Start();
            hilo_proc3.Start();

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
        public void imprimirDatos()
        {

        }
    }
}
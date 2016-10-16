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

        private void repartirHilillos(Thread hilo_proc1, Thread hilo_proc2, Thread hilo_proc3)
        {


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
            Thread hilo_proc1 = new Thread(delegate () { });
            Thread hilo_proc2 = new Thread(delegate () { });
            Thread hilo_proc3 = new Thread(delegate () { });

            hilo_proc1.Start();
            hilo_proc2.Start();
            hilo_proc3.Start();

            repartirHilillos(hilo_proc1, hilo_proc2, hilo_proc3);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProyectoArquitectura
{
    class Program
    {
        private static int numeroHilos;
        private static int quantum;
        private static string path;

        static void Main(string[] args)
        {
            Console.WriteLine("Ingrese el numero de hilos: ");  // Numero de hillos por procesador (Hilo)
            numeroHilos = int.Parse(Console.ReadLine());

            Console.WriteLine("Ingrese valor del quantum: ");   // Valor del quantum
            quantum = int.Parse(Console.ReadLine());

            Console.WriteLine("Ingrese la direccion donde se encuentran los hilos: ");  // Ubicacion de los hilillos
            path = @Console.ReadLine() + @"\";


            Controlador cont = new Controlador(numeroHilos, quantum, path);
            Thread hiloPrincipal = new Thread(new ThreadStart(cont.iniciarPrograma));    // Se crea un nuevo hilo controlador
            hiloPrincipal.Start();  // Se inicia el hilo controlador

            while (hiloPrincipal.IsAlive)   // El programa principal espera a que el hilo principal haya terminado
            {

            }

            Console.WriteLine("\nPresione cualquier tecla para salir del programa");
            Console.ReadKey();
        }
    }
}
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
        private int[,] memoria; //64*16 = 1024 bytes
        private int[] registros; //32 (R0 - R31)
        private int[] contexto; //34 (R0 - R31 + RL + PC)
        private int[] cacheDatos; // 4 bloques de una palabra
        private int[] cacheInstrucciones; // 4 bloques de 4 palabras
        private Queue<int> hilillos;
        private int quantumLocal;
        private int reloj;
        private int PC;
        public Queue <int> direccionHilillo ;


        

        public Procesador(int numProcesador = 0, Barrier sync = null)
        {
            InitializeComponent();
            quantumLocal = 0;
            reloj = 0;
            PC = 0;
        }


    
        //solo para el procesador principal que va a tener la memoria 
        public void inicializarProcesadorPrincipal() {
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
        //inicializador de registros y caches  de datos de los tres procesadores 
        public void inicializarProcesador()
        {
          
            registros = new int[32];
            cacheInstrucciones = new int[72];
            cacheDatos = new int[4];


            for (int i = 0; i < 32; i++)
            {
                registros[i] = 0;
            }

            for (int i = 0; i < 72; i++)
            {
                cacheInstrucciones[i] = 0;
            }

            cacheInstrucciones[17] = -1;
            cacheInstrucciones[35] = -1;
            cacheInstrucciones[53] = -1;
            cacheInstrucciones[71] = -1;

            for (int i = 0; i < 4; i++)
            {
                cacheDatos[i] = 0;
            }
        }


        //el procesador principal carga instrucciones de os txt a memoria principal
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
                            memoria[posicion, i] = Int32.Parse(codigos[i]);
                        }                       
                        posicion++;
                    }

                }
                //int valor = Int32.Parse(contents);
                //    data.Add(valor);
                //    CD1.DataSource = data;
                int a = 0; //Break Point por si quieren revisar la memoria
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
            //bloquear BUS

            switch(posicionCache)
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

            while(lengthMemoria < 16)
            {
                cacheInstrucciones[i] = memoria[bloque, lengthMemoria];
                lengthMemoria++;
                i++;
            }
            PC += 4;
            //Liberar BUS
        }

        public void EjecutarInstrs()
        {
            int codigoOperacion = 0;
            int P1 = 0;
            int P2 = 0;
            int P3 = 0;
            int P4 = 0;  // Son las instrs en Cache

            switch (codigoOperacion)
            {
                case 2:     // JR
                    PC = registros[P1];
                    break;

                case 3:     // JAL
                    registros[31] = PC;
                    PC += P3;
                    break;

                case 4:     // BEQZ
                    if (registros[P1] == 0)
                    {
                        PC += P3 * 4;
                    }
                    break;

                case 5:     // BENZ
                    if (registros[P1] != 0)
                    {
                        PC += P3 * 4;
                    }
                    break;

                case 8:     // DADDI
                    registros[P2] = registros[P1] + P3;
                    break;

                case 12:    // DMUL
                    registros[P3] = registros[P1] * registros[P2];
                    break;

                case 14:    // DDIV
                    registros[P3] = registros[P1] / registros[P2];
                    break;

                case 32:    // DADD
                    registros[P3] = registros[P1] + registros[P2];
                    break;

                case 34:    // DSUB
                    registros[P3] = registros[P1] - registros[P2];
                    break;

                case 35:    // LW
                   // ejecutarLW(ref a, ref b, ref c, registros[R1] + R3, R2);
                    break;

                case 43:    // SW
                   // ejecutarSW(ref a, ref b, ref c, registros[R1] + R3, R2);
                    break;

                case 63:    // Codigo para terminar el programa
                   // ++hilillosTerminados;
                    break;
            }
        }


        // lectura seria el bloque que desea leer
        public void ejecutarLW(int direccionMemoria) 
        {
            int bloque = calcularBloque(direccionMemoria);
            int subirBloque = posicionCache(bloque);

            if(bloque == cacheInstrucciones[subirBloque*18 + 16] && cacheInstrucciones[subirBloque*18 + 17] == 1)
            {
                //lectura en cache
            }
            else //traer bloque a memoria por fallo de cache
            {
                //bloquear Cache
                //bloquear Bus
                for (int k = 0; k < 16; k++)
                {
                    cacheInstrucciones[subirBloque * 18 + k] = memoria[bloque, k];
                }
                
            }
            
        }

        public int calcularBloque(int dirMem)
        {
            int bloque = dirMem / 16;
            return bloque;
        }

        public int posicionCache(int bloque)
        {
            int posCache = bloque % 4;
            return posCache;
        }

        private void ejecutarInstruccion()
        {
            
        }

        private void Procesador_FormClosing(object sender, EventArgs e)
        {
            
        }
        private void P3_Enter(object sender, EventArgs e)
        {

        }
    }
}
    
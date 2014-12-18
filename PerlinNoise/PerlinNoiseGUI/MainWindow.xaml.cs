using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PerlinNoise;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace PerlinNoiseGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public List<Result> Results { get; set; }

        public static void CreateThreadsMasm(int threads, double[][] BitMap, int[] permutationTable, double[] gradients)
        {
            Thread[] Threads = new Thread[threads];
            int partition = BitMap.GetLength(0) / threads;
            int i = 0;
            for (int j = 0; j < Threads.Length; j++)
            {
                if (j == Threads.Length - 1)
                {
                    int LocalCopy = i;
                    Threads[j] = new Thread(() => tableWorkerMasm(LocalCopy, BitMap.GetLength(0), BitMap, permutationTable, gradients));
                }
                else
                {
                    int LocalCopy = i;
                    Threads[j] = new Thread(() => tableWorkerMasm(LocalCopy, LocalCopy + partition, BitMap, permutationTable, gradients));
                    i += partition;
                }
            }

            for (int j = 0; j < Threads.Length; j++)
            {
                Threads[j].Start();
            }
            for (int j = 0; j < Threads.Length; j++)
            {
                Threads[j].Join();
            }

        }

        public static void tableWorkerMasm(int StartingIndex, int EndingIndex, double[][] BitMap, int[] permutationTable, double[] gradients)
        {
            for (int i = StartingIndex; i < EndingIndex; i++)
            {
                BitMap[i] = new double[BitMap.GetLength(0)];
                for (int k = 0; k < BitMap.GetLength(0); k++)
                {
                    double noise = Math.Max(Math.Min(MainWindow.Noise((double)i / 512, (double)k / 512, permutationTable, gradients), (double)1), (double)-1);
                    BitMap[i][k] = (noise + 1) * 127.5;
                }
            }
        }


        public static void CreateThreads(int threads, double[][] BitMap, int[] permutationTable, Vector2[] gradients)
        {
            Thread[] Threads = new Thread[threads];
            int partition = BitMap.GetLength(0) / threads;
            int i = 0;
            for (int j = 0; j < Threads.Length; j++)
            {
                if (j == Threads.Length - 1)
                {
                    int LocalCopy = i;
                    Threads[j] = new Thread(() => tableWorker(LocalCopy, BitMap.GetLength(0), BitMap, permutationTable, gradients));
                }
                else
                {
                    int LocalCopy = i;
                    Threads[j] = new Thread(() => tableWorker(LocalCopy, LocalCopy + partition, BitMap, permutationTable, gradients));
                    i += partition;
                }
            }

            for (int j = 0; j < Threads.Length; j++)
            {
                Threads[j].Start();
            }
            for (int j = 0; j < Threads.Length; j++)
            {
                Threads[j].Join();
            }

        }

        public static void tableWorker(int StartingIndex, int EndingIndex, double[][] BitMap, int[] permutationTable, Vector2[] gradients)
        {
            for (int i = StartingIndex; i < EndingIndex; i++)
            {
                BitMap[i] = new double[BitMap.GetLength(0)];
                for (int k = 0; k < BitMap.GetLength(0); k++)
                {
                    double noise = Math.Max(Math.Min(Noise2d.Noise((double)i / 512, (double)k / 512, permutationTable, gradients), (double)1), (double)-1);
                    BitMap[i][k] = (noise + 1) * 127.5;
                }
            }
        }

        private static void CalculatePermutation(out int[] p, Random siema)
        {
            p = new int[256];
            p = Enumerable.Range(0, 256).ToArray();

            /// shuffle the array
            for (var i = 0; i < p.Length; i++)
            {
                var source = siema.Next(p.Length);

                var t = p[i];
                p[i] = p[source];
                p[source] = t;
            }
        }

        private static void CalculateGradients(out Vector2[] grad, Random seed)
        {
            grad = new Vector2[256];

            for (var i = 0; i < grad.Length; i++)
            {
                Vector2 gradient;

                do
                {
                    gradient = new Vector2((double)(seed.NextDouble() * 2 - 1), (double)(seed.NextDouble() * 2 - 1));
                }
                while (gradient.LengthSquared() >= 1);

                gradient.Normalize();

                grad[i] = gradient;
            }

        }
        private static void PrepareGradients(Vector2[] grads, out double[] gradsTable, out GCHandle gradientsTableHandle)
        {
            int i = 0;
            gradsTable = new double[512];
            gradientsTableHandle = GCHandle.Alloc(gradsTable, GCHandleType.Pinned);
            foreach (Vector2 Vec in grads)
            {
                gradsTable[i] = Vec.x;
                gradsTable[i + 1] = Vec.y;
                i += 2;
            }
           
        }

        [DllImport("PerlinNoiseASM.dll")]
        public static extern double Noise(double x, double y, int[] permutationTable, double[] gradients);
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            
        }

        



        public MainWindow()
        {
            InitializeComponent();
            this.MasmRadio.IsChecked = true;
            this.Results = new List<Result>();
        }

        private void Genarate(object sender, RoutedEventArgs e)
        {
           
            int[] permutationTable;
            Vector2[] grads;
            double[] gradsTable;

            int resolution = 0;
            int threadsNum = 0;
            int seedNum = 0;
            try
            {
                resolution = Convert.ToInt32(this.Size.Text);
                
            }
            catch(FormatException ex)
            {
                MessageBox.Show("Nieprawidłowy format rozmiaru");
                return;
                // komunikat
            }
            try
            {
                threadsNum = Convert.ToInt32(this.Threads.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Nieprawidłowy format liczby wątków");
                return;
            }
            try
            {
                seedNum = Convert.ToInt32(this.Seed.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Nieprawidłowy format ziarna");
                return;
            }

            if(resolution == 0 ||threadsNum == 0)
            {
                MessageBox.Show("Wartość rozmiaru i liczby wątków musi być różna od 0");
                return;
            }
            Random seed;
            if(seedNum == 0)
            {
                seed = new Random();
            }
            else
            {
                seed = new Random(seedNum);
            }
           
            MainWindow.CalculatePermutation(out permutationTable, seed);
            MainWindow.CalculateGradients(out grads, seed);
            
            GCHandle permutationHandle = GCHandle.Alloc(permutationTable, GCHandleType.Pinned);
            GCHandle gradsHandle;
            MainWindow.PrepareGradients(grads, out gradsTable, out gradsHandle);

            

            double[][] imageBitMapTable = new double[resolution][];
            byte[] imageBitmap = new byte[resolution * resolution * 4];




            
            var watch = Stopwatch.StartNew();
            if (this.MasmRadio.IsChecked == true)
            {
                CreateThreadsMasm(threadsNum, imageBitMapTable, permutationTable, gradsTable);
            }
            else
            {
                CreateThreads(threadsNum, imageBitMapTable, permutationTable, grads);
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            this.Time.Text = Convert.ToString(elapsedMs) + " ms";



            // utworzenie bitmapy
            for (int y = 0; y < resolution; y++)
            {
                int yIndex = y * resolution;
                for (int x = 0; x < resolution; x++)
                {

                    var index = (y * resolution*4) + (x * 4);

                    imageBitmap[index] = (byte)((int)imageBitMapTable[y][x]);
                    imageBitmap[index + 1] = (byte)((int)imageBitMapTable[y][x]);
                    imageBitmap[index + 2] = (byte)((int)imageBitMapTable[y][x]);
                    imageBitmap[index + 3] = 255;
                }
            }

            this.ImageBitmap.Source = null;
            var source = BitmapSource.Create(resolution, resolution, 96, 96, PixelFormats.Bgra32, null, imageBitmap, resolution*4);
            this.ImageBitmap.Source = source;

            string LibraryName;

            if (this.MasmRadio.IsChecked == true)
            {
                LibraryName = "MASM64";
            }
            else
            {
               LibraryName = "C#";
            }

            Results.Add(new Result() { Id = Results.Count + 1, Rozmiar = Convert.ToString(resolution) + "x" + Convert.ToString(resolution), Czas = Convert.ToString(elapsedMs) + " ms", Ziarno = seedNum, Biblioteka = LibraryName, Wątki = threadsNum });
            DataGrid.ItemsSource = Results;
            this.DataGrid.Items.Refresh();

            string FileName = Convert.ToString(Results.Count)+  ".png";
            FileStream stream = new FileStream(FileName, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            

            permutationHandle.Free();
            gradsHandle.Free();
         
           
        }



    }
}

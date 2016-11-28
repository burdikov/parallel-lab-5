using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace parallel_lab_5
{
    [SuppressMessage("ReSharper", "TailRecursiveCall")]
    internal class Program
    {
        /// <summary>
        /// List of commands to be applied before the others.
        /// </summary>
        private static readonly Dictionary<string, Action<string>> UrgentCommands = new Dictionary
            <string, Action<string>>
            {
                {"--n-rnd", _n_rnd},
                {"--n-asc", _n_asc},
                {"--n-desc", _n_desc},
                {"--asc", _asc},
                {"--desc", _desc}
            };

        /// <summary>
        /// List of main commands.
        /// </summary>
        private static readonly Dictionary<string, Action> Commands = new Dictionary<string, Action>
        {
            {"-s", Sequential},
            {"-p", Parallel},
            {"-n", Net},
            {"-h", Help}
        };


        /// <summary>
        /// Flag used to prevent multiple new array creations.
        /// </summary>
        private static bool _bNewArrayCreated;

        /// <summary>
        /// Flag used to prevent multiple sort method changes.
        /// </summary>
        private static bool _bSortMethodSet;

        /// <summary>
        /// Whether to sort in ascendig order.
        /// </summary>
        private static bool _bSortAsc = true;

        /// <summary>
        /// Threads count. Usually Environment.ProcessorCount.
        /// </summary>
        private static readonly int Procs = Environment.ProcessorCount;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (!UrgentCommands.ContainsKey(args[i])) continue;
                    if (i + 1 < args.Length && args[i + 1][0] != '-') UrgentCommands[args[i]](args[i++ + 1]);
                    else UrgentCommands[args[i]]("");
                }
                foreach (var t in args)
                {
                    if (Commands.ContainsKey(t)) Commands[t]();
                }
            }
            else Commands["-h"]();
        }

    #region	urgent commands

        private static void NewArray(int sortType, int count)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var arr = new int[count];

            for (var i = 0; i < count; i++)
            {
                arr[i] = rnd.Next(0, count * 2);
            }
            if (sortType == 1)
                Array.Sort(arr, (x, y) => x.CompareTo(y));
            else if (sortType == 2)
                Array.Sort(arr, (x, y) => y.CompareTo(x));

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\in.txt", FileMode.Create));
                foreach (var item in arr)
                {
                    writer.Write(item + " ");
                }

                Console.WriteLine("New in.txt was created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                writer?.Close();
            }
        }

        private static void _n_rnd(string param)
        {
            if (_bNewArrayCreated) return;
            int count;
            if (int.TryParse(param, out count))
            {
                NewArray(0, count);
                _bNewArrayCreated = true;
            }
            else
                Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
        }

        private static void _n_asc(string param)
        {
            if (_bNewArrayCreated) return;
            int count;
            if (int.TryParse(param, out count))
            {
                NewArray(1, count);
                _bNewArrayCreated = true;
            }
            else
                Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
        }

        private static void _n_desc(string param)
        {
            if (_bNewArrayCreated) return;
            int count;
            if (int.TryParse(param, out count))
            {
                NewArray(2, count);
                _bNewArrayCreated = true;
            }
            else
                Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
        }

        private static void _asc(string param)
        {
            if (!_bSortMethodSet)
                _bSortMethodSet = true;
        }

        private static void _desc(string param)
        {
            if (_bSortMethodSet) return;
            _bSortMethodSet = true;
            _bSortAsc = false;
        }

    #endregion urgent commands

    #region main commands

        private static int[] GetInitialArray()
        {
            int[] res;
            StreamReader f = null;
            try
            {
                f = File.OpenText(Environment.CurrentDirectory + "\\in.txt");

                var s = f.ReadToEnd().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                res = s.Select(item => Convert.ToInt32(item)).ToArray();
            }
            finally
            {
                f?.Close();
            }

            return res;
        }

        private static void Net()
        {
            var arr = GetInitialArray();

            var timer = new Stopwatch();
            timer.Start();
            Array.Sort(arr);
            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds);
        }

        private static void Sequential()
        {
            StreamWriter f = null;
            try
            {
                var arr = GetInitialArray();

                Stopwatch timer = new Stopwatch();

                timer.Start();
                if (!_bSortAsc) _seqQuickSortDesc(arr, 0, arr.Length - 1);
                else _seqQuickSortAsc(arr, 0, arr.Length - 1);
                timer.Stop();

                f = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\out.txt", FileMode.Create));
                foreach (var i in arr)
                {
                    f.Write(i + " ");
                }
                f.Close();
                Console.WriteLine("New out.txt created.");

                f = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\summary.txt", FileMode.Create));
                f.WriteLine("Режим: однопоточное выполнение.");
                f.WriteLine("Затрачено времени: " + timer.ElapsedMilliseconds + " мс.");
                f.Close();
                Console.WriteLine("New summary.txt created.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                f?.Close();
            }
        }

        private static void _seqQuickSortAsc(IList<int> ints, int l, int r)
        {
            if (l == r) return;
            int min = int.MaxValue, max = int.MinValue;

            for (var i = l; i < r + 1; i++)
            {
                if (ints[i] < min) min = ints[i];
                if (ints[i] > max) max = ints[i];
            }

            var pivot = (min + max) / 2;

            int lNew = l, rNew = r;

            while (lNew <= rNew)
            {
                while (ints[lNew] < pivot) lNew++;
                while (ints[rNew] > pivot) rNew--;
                // ReSharper disable once InvertIf
                if (lNew <= rNew)
                {
                    var temp = ints[lNew];
                    ints[lNew++] = ints[rNew];
                    ints[rNew--] = temp;
                }
            }

            if (l < rNew) _seqQuickSortAsc(ints, l, rNew);
            if (lNew < r) _seqQuickSortAsc(ints, lNew, r);
        }

        private static void _seqQuickSortDesc(IList<int> ints, int l, int r)
        {
            if (l == r) return;
            int min = int.MaxValue, max = int.MinValue;

            for (var i = l; i < r + 1; i++)
            {
                if (ints[i] < min) min = ints[i];
                if (ints[i] > max) max = ints[i];
            }

            var pivot = (min + max) / 2;

            int lNew = l, rNew = r;

            while (lNew <= rNew)
            {
                while (ints[lNew] > pivot) lNew++;
                while (ints[rNew] < pivot) rNew--;
                // ReSharper disable once InvertIf
                if (lNew <= rNew)
                {
                    var temp = ints[lNew];
                    ints[lNew++] = ints[rNew];
                    ints[rNew--] = temp;
                }
            }

            if (l < rNew) _seqQuickSortDesc(ints, l, rNew);
            if (lNew < r) _seqQuickSortDesc(ints, lNew, r);
        }

        private static void Parallel()
        {
            StreamWriter f = null;
            try
            {
                var arr = GetInitialArray();

                var timer = new Stopwatch();

                timer.Start();

                for (var i = 0; i < Procs; i++)
                {
                    BagsForExchange[i] = new List<int>();
                    SyncStage[i] = new ManualResetEvent(false);
                    SyncStage2[i] = new ManualResetEvent[Procs];
                    for (var j = 0; j < Procs; j++)
                    {
                        SyncStage2[i][j] = new ManualResetEvent(false);
                    }
                }

                var threads = new Thread[Procs];
                var step = arr.Length / Procs;

                for (var i = 0; i < Procs; i++)
                {
                    var l = i * step;
                    var r = i == Procs - 1 ? arr.Length - 1 : (i + 1) * step - 1;

                    var ints = new int[r - l + 1];
                    Array.Copy(arr, l, ints, 0, r - l + 1);

                    threads[i] = new Thread(_parQuickSort);
                    threads[i].Start(new Params(ints, i));
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
                timer.Stop();

                f = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\out.txt", FileMode.Create));
                foreach (var list in BagsForExchange)
                {
                    foreach (var i in list)
                    {
                        f.Write(i + " ");
                    }
                }
                f.Close();
                Console.WriteLine("New out.txt created.");

                f = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\summary.txt", FileMode.Create));
                f.WriteLine("Режим: многопоточное выполнение.");
                f.WriteLine("Затрачено времени: " + timer.ElapsedMilliseconds + " мс.");
                f.Close();
                Console.WriteLine("New summary.txt created.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                f?.Close();
            }
	    }

        private struct Params
        {
            public readonly int[] Ints;
            public readonly int Number;

            public Params(int[] ints, int i)
            {
                Ints = ints;
                Number = i;
            }
        }

        private static void _isSorted()
        {
            IEnumerable<int> a = BagsForExchange[0];
            a = a.Concat(BagsForExchange[1]);
            a = a.Concat(BagsForExchange[2]);
            a = a.Concat(BagsForExchange[3]);

            var c = a.ToArray();
            var b = true;
            var prev = c[0];

            for (int i = 1; i < c.Length; i++)
            {
                if (prev > c[i])
                {
                    b = false;
                    break;
                }
                prev = c[i];
            }

            Console.WriteLine(!b ? "SHIT THIS IS NOT SORTED" : "OL RIHGT!");
        }

        private static readonly List<int>[] BagsForExchange = new List<int>[Procs];
        private static readonly List<int> Samples = new List<int>();

        private static readonly ManualResetEvent SamplesSorted = new ManualResetEvent(false);
        private static readonly ManualResetEvent[] SyncStage = new ManualResetEvent[Procs];
        private static readonly ManualResetEvent[][] SyncStage2 = new ManualResetEvent[Procs][];

        private static void _parQuickSort(object o)
        {
            var number = ((Params) o).Number;
            var ints = ((Params) o).Ints;

            if (_bSortAsc) _seqQuickSortAsc(ints, 0, ints.Length - 1);
            else _seqQuickSortDesc(ints, 0, ints.Length - 1);

            var samples = new List<int>();
            for (var i = 0; i < Procs; i++)
            {
                samples.Add(ints[ints.Length * i / (Procs * Procs)]);
            }

            lock ("samples")
            {
                foreach (var sample in samples)
                {
                    Samples.Add(sample);
                }
            }

            if (number == 0)
            {
                SyncStage[number].Set();
                WaitHandle.WaitAll(SyncStage);

                Samples.Sort();
                if (!_bSortAsc) Samples.Reverse();

                samples.Clear();
                for (var i = 1; i < Procs; i++)
                {
                    samples.Add(Samples[i*(Samples.Count/Procs)]);
                }
                Samples.Clear();
                foreach (var sample in samples)
                {
                    Samples.Add(sample);
                }

                SamplesSorted.Set();
            }
            else
            {
                SyncStage[number].Set();
                SamplesSorted.WaitOne();
            }

            var syncLists = new List<int>[Procs];
            var j = 0;
            for (var i = 0; i < Procs - 1; i++)
            {
                syncLists[i] = new List<int>();
                if (_bSortAsc)
                    while (j < ints.Length && ints[j] <= Samples[i])
                        syncLists[i].Add(ints[j++]);
                else
                    while (j < ints.Length && ints[j] >= Samples[i])
                        syncLists[i].Add(ints[j++]);
            }
            syncLists[Procs - 1] = new List<int>();
            if (_bSortAsc)
                while (j < ints.Length && ints[j] > Samples[Procs - 2])
                    syncLists[Procs - 1].Add(ints[j++]);
            else
                while (j < ints.Length && ints[j] < Samples[Procs - 2])
                    syncLists[Procs - 1].Add(ints[j++]);

            for (var i = 0; i < Procs; i++)
            {
                lock (BagsForExchange[i])
                {
                    foreach (var item in syncLists[i])
                    {
                        BagsForExchange[i].Add(item);
                    }
                }
                SyncStage2[i][number].Set();
                WaitHandle.WaitAll(SyncStage2[i]);
            }

            if (_bSortAsc) BagsForExchange[number].Sort();
            else BagsForExchange[number].Sort((a, b) => b.CompareTo(a));
        }


	    private static void Help()
		{
			Console.WriteLine("This program enables you to sort arrays via parallel and non-parallel quick-sort " +
							  "algorithms.\n\t-s Non-parallel sorting\n\t-p Parallel sorting\n\t-h This message" +
			                  "\n\t--asc Sort in ascending order\n\t--desc Sort in descending order" +
			                  "\n\t--n-rnd [n] Create new array of n elements. Not sorted. If used with -s or -p, will" +
			                  " create new array before starting sorting." +
			                  "\n\t--n-asc [n], --n-desc [n] Same as the --n-rnd, but arrays are sorted." +
							  "\nData is taken from in.txt so make sure it exists. When complete, out.txt and summary" +
							  ".txt are generated. Previous data is overwritten.");
		}

	#endregion main commands
	}
}

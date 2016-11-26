using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace parallel_lab_5
{
    [SuppressMessage("ReSharper", "TailRecursiveCall")]
    internal class Program
    {
        // --n-rnd, --n-asc, --n-desc, -s, -p, -h, -asc, -desc

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
            List<int> res;
            StreamReader f = null;
            try
            {
                f = File.OpenText(Environment.CurrentDirectory + "\\in.txt");

                var s = f.ReadToEnd().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                res = s.Select(item => Convert.ToInt32(item)).ToList();
            }
            finally
            {
                f?.Close();
            }

            return res.ToArray();
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

        private static void _seqQuickSortAsc(int[] ints, int l, int r)
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

        private static void _seqQuickSortDesc(int[] ints, int l, int r)
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

                Stopwatch timer = new Stopwatch();

                timer.Start();

                timer.Stop();

                f = new StreamWriter(File.Open(Environment.CurrentDirectory + "\\out.txt", FileMode.Create));
                foreach (var i in arr)
                {
                    f.Write(i + " ");
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

        private static void _parQuickSort(int[] ints, int l, int r)
        {

        }

	    private static void Help()
		{
			Console.WriteLine("This program enables you to sort arrays via parallel and non-parallel quick-sort " +
							  "algorithms.\n\t-s Non-parallel sorting\n\t-p Parallel sorting\n\t-h This message\n" +
							  "Data is taken from in.txt so make sure it exists. When complete, out.txt and summary" +
							  ".txt are generated.");
		}

		#endregion main commands
	}
}

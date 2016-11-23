using System;
using System.Collections.Generic;
using System.IO;

namespace parallel_lab_5
{
	class Program
	{
		// --n-rnd, --n-asc, --n-desc, -s, -p, -h, -asc, -desc

		/// <summary>
		/// List of commands to be applied before the others.
		/// </summary>
		static Dictionary<string, Action<string>> urgentCommands = new Dictionary<string, Action<string>>()
		{
			{"--n-rnd", _n_rnd },
			{"--n-asc", _n_asc },
			{"--n-desc", _n_desc },
			{"--asc", _asc },
			{"--desc", _desc }
		};

		/// <summary>
		/// List of main commands.
		/// </summary>
		static Dictionary<string, Action> commands = new Dictionary<string, Action>
		{
			{"-s", Sequential},
			{"-p", Parallel},
			{"-h", Help}
		};

		/// <summary>
		/// Flag used to prevent multiple new array creations.
		/// </summary>
		static bool bNewArrayCreated = false;

		/// <summary>
		/// Flag used to prevent multiple sort method changes.
		/// </summary>
		static bool bSortMethodSet = false;

		/// <summary>
		/// Whether to sort in ascendig order.
		/// </summary>
		static bool bSortAsc = true;

		public static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (urgentCommands.ContainsKey(args[i]))
					{
						if (i + 1 < args.Length && args[i + 1][0] != '-') urgentCommands[args[i]](args[i++ + 1]);
						else urgentCommands[args[i]]("");
					}
				}
				for (int i = 0; i < args.Length; i++)
				{
					if (commands.ContainsKey(args[i])) commands[args[i]]();
				}
			}
			else commands["-h"]();
		}
		#region	urgent commands

		static void NewArray(int sortType, int count)
		{
			var rnd = new Random(DateTime.Now.Millisecond);
			int[] arr = new int[count];

			for (int i = 0; i < count; i++)
			{
				arr[i] = rnd.Next(0, count * 2);
			}
			switch (sortType)
			{
				case 1: Array.Sort(arr, (x, y) => x.CompareTo(y)); break;
				case 2: Array.Sort(arr, (x, y) => y.CompareTo(x)); break;
				default:
					break;
			}

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
				if (writer != null) writer.Close();
			}
		}

		static void _n_rnd(string param)
		{
			int count;
			if (!bNewArrayCreated)
			{
				if (int.TryParse(param, out count))
				{
					NewArray(0, count);
					bNewArrayCreated = true;
				}
				else
					Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
			}
		}

		static void _n_asc(string param)
		{
			int count;
			if (!bNewArrayCreated)
			{
				if (int.TryParse(param, out count))
				{
					NewArray(1, count);
					bNewArrayCreated = true;
				}
				else
					Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
			}
		}

		static void _n_desc(string param)
		{
			int count;
			if (!bNewArrayCreated)
			{
				if (int.TryParse(param, out count))
				{
					NewArray(2, count);
					bNewArrayCreated = true;
				}
				else
					Console.WriteLine("Wrong argument to --n-rnd. Statement ignored.");
			}
		}

		static void _asc(string param)
		{
			if (!bSortMethodSet)
				bSortMethodSet = true;
		}

		static void _desc(string param)
		{
			if (!bSortMethodSet)
			{
				bSortMethodSet = true;
				bSortAsc = false;
			}
		}
		#endregion urgent commands

		#region main commands

		static int[] GetInitialArray()
		{
			List<int> res;
			StreamReader f = null;
			try
			{
				f = File.OpenText(Environment.CurrentDirectory + "\\in.txt");

				var s = f.ReadToEnd().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				res = new List<int>();
				foreach (var item in s)
				{
					res.Add(Convert.ToInt32(item));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw;
			}
			finally
			{
				if (f != null) f.Close();
			}

			return res.ToArray();
		}

		static void Sequential()
		{
			int[] arr;
			try
			{
				arr = GetInitialArray();
			}
			catch (Exception)
			{
				return;
			}

		}

		static void Parallel() { }

		static void Help()
		{
			Console.WriteLine("This program enables you to sort arrays via parallel and non-parallel quick-sort " +
							  "algorithms.\n\t-s Non-parallel sorting\n\t-p Parallel sorting\n\t-h This message\n" +
							  "Data is taken from in.txt so make sure it exists. When complete, out.txt and summary" +
							  ".txt are generated.");
		}

		#endregion main commands
	}
}

using System.Diagnostics;
using System.Text.Json;

namespace Dijkstra; 
internal class Program {
	public struct Sommet {
		public double x;
		public double y;
		public double z;

		public string name;
		public List<string>? neighbours;

		public Sommet(string Name, double X, double Y, double Z = 0.0, List<string>? Neighbours=null) {
			name = Name;
			x = X;
			y = Y;
			z = Z;
			neighbours = Neighbours;
		}

		public readonly override string ToString() {
			return $"Sommet(name={name}, x={x}, y={y}, z={z})";
		}

		public override readonly bool Equals(object? obj) => obj is Sommet other && this.Equals(other);

		public readonly bool Equals(Sommet s) => ((s.x == x) && (s.y == y) && (s.z == z) && (s.name == name));

		public static bool operator ==(Sommet s1, Sommet s2) => s1.Equals(s2);

		public static bool operator !=(Sommet s1, Sommet s2) => !(s1.Equals(s2));

	}

	// ------------------------------------------------------------------------


	// ------------------------------------------------------------------------

	public static List<Sommet> SOMMETS_OG = new List<Sommet>();
	public static List<Tuple<string, string>> LIENS_OG = new List<Tuple<string, string>>();

	public static Dictionary<string, Sommet> CORRESPONDANCE = new Dictionary<string, Sommet>();

	static protected readonly Sommet SOMMET_NULL = new Sommet("null", double.PositiveInfinity, double.PositiveInfinity);

	// ------------------------------------------------------------------------

	public static double Poids(Sommet s1, Sommet s2) {
		double poids = Math.Sqrt(Math.Pow(s2.x-s1.x, 2) + Math.Pow(s2.y-s1.y, 2) + Math.Pow(s2.z-s1.z, 2));
		return poids;
	}

	public static List<Sommet> Voisins(Sommet s, List<Sommet> SOMMETS, List<Tuple<string, string>> LIENS) {
		string s_name = s.name;
		List<Sommet> voisins = new List<Sommet>();

		foreach (Tuple<string, string> L in LIENS) {
			if (s_name == L.Item1) {
				Sommet s2 = CORRESPONDANCE[L.Item2];
				if (SOMMETS.Contains(s2)) {
					voisins.Add(s2);
				}
			}
		}

		return voisins;
	}

	public static Dictionary<Sommet, double> Initialisation(List<Sommet> SOMMETS, Sommet s_dep) {
		Dictionary<Sommet, double> d = new Dictionary<Sommet, double>();
		foreach (Sommet s in SOMMETS) {
			if (s != s_dep) {
				d[s] = double.PositiveInfinity;
			} else {
				d[s] = 0d;
			}
		}
		return d;
	}

	public static Sommet Trouve_min(List<Sommet> SOMMETS, Dictionary<Sommet, double> d) {
		double mini = double.PositiveInfinity;
		Sommet sommet = SOMMET_NULL;

		if (SOMMETS.Count > 0) {
			foreach (Sommet s in SOMMETS) {
				if (d[s] < mini) {
					mini = d[s];
					sommet = s;
				}
			}
		}
		return sommet;
	}

	public static Tuple<Dictionary<Sommet, double>, Dictionary<Sommet, Sommet>> maj_distances(Sommet s1, Sommet s2, Dictionary<Sommet, double> d, Dictionary<Sommet, Sommet> predecesseur) { 
		if (d[s2] > (d[s1] + Poids(s1, s2))) {
			d[s2] = d[s1] + Poids(s1, s2);
			predecesseur[s2] = s1;
		}
		return Tuple.Create(d, predecesseur);
	}

	public static Tuple<Dictionary<Sommet, double>, Dictionary<Sommet, Sommet>> Algo(List<Tuple<string, string>> LIENS, List<Sommet> SOMMETS, Sommet s_dep) {
		Dictionary<Sommet, double> d = Initialisation(SOMMETS, s_dep);
		Dictionary<Sommet, Sommet> predecesseur = new Dictionary<Sommet, Sommet>();

		List<Sommet> SOMMETS_WORK = new List<Sommet>();
		foreach (Sommet s in SOMMETS) {
			SOMMETS_WORK.Add(s);
		}

		Tuple<Dictionary<Sommet, double>, Dictionary<Sommet, Sommet>> Tuple_dP;

		while (SOMMETS_WORK.Count != 0) {
			Sommet s1 = Trouve_min(SOMMETS_WORK, d);

			if (s1 == SOMMET_NULL) {
				break;
			}
			if (SOMMETS_WORK.Contains(s1)) {
				SOMMETS_WORK.Remove(s1);
			}

			foreach (Sommet s2 in Voisins(s1, SOMMETS_WORK, LIENS)) {
				Tuple_dP = maj_distances(s1, s2, d, predecesseur);
				d = Tuple_dP.Item1;
				predecesseur = Tuple_dP.Item2;
			}
		}
		return Tuple.Create(d, predecesseur);
	}

	public static Tuple<List<string>, double> trouver_chemin(Sommet s_dep, Sommet s_fin, Dictionary<string, string> predecesseurs_names, Dictionary<Sommet, double> d) {
		string s_name = s_fin.name;
		string dep_name = s_dep.name;

		List<string> A = new List<string>();

		while (s_name != dep_name) {
			A.Add(s_name);
			s_name = predecesseurs_names[s_name];
		}

		A.Add(dep_name);
		A.Reverse();

		return Tuple.Create(A, d[s_fin]);
	}

	public static Tuple<List<string>, double> faux_main(String s_dep, String s_fin, List<Tuple<string, string>> LIENS, List<Sommet> SOMMETS) {
		Sommet S_dep = CORRESPONDANCE[s_dep];
		Sommet S_fin = CORRESPONDANCE[s_fin];

		Tuple<Dictionary<Sommet, double>, Dictionary<Sommet, Sommet>> dP = Algo(LIENS, SOMMETS, S_dep);
		Dictionary<Sommet, double> d = dP.Item1;
		Dictionary<string, string> p = new Dictionary<string, string>();

		foreach (KeyValuePair<Sommet, Sommet> kvp in dP.Item2) {
			p[kvp.Key.name] = kvp.Value.name;
		}

		return trouver_chemin(S_dep, S_fin, p, d);
	}

	public static Dictionary<Tuple<string, string>, Tuple<List<string>, double>> AllPaths(List<Sommet> SOMMETS, List<Tuple<string, string>> LIENS) {
		Dictionary<Tuple<string, string>, Tuple<List<string>, double>> PATHS = new Dictionary<Tuple<string, string>, Tuple<List<string>, double>>();
		Console.WriteLine($"{SOMMETS.Count() * (SOMMETS.Count()-1)/2} chemins à déterminer");

		Stopwatch StopWatch = Stopwatch.StartNew();

		double i = 0;

		foreach (Sommet s_dep in SOMMETS) {
			foreach (Sommet s_fin in SOMMETS) {
				if ((s_dep.name != s_fin.name) && !(PATHS.Keys.Contains(Tuple.Create(s_fin.name, s_dep.name)))) {
					PATHS[Tuple.Create(s_dep.name, s_fin.name)] = faux_main(s_dep.name, s_fin.name, LIENS, SOMMETS);
					i++;
				}
			}
		}
		StopWatch.Stop();
		double ElapsedTime = (double)StopWatch.ElapsedMilliseconds/1000d;
		Console.WriteLine($"{i} chemins générés en {ElapsedTime} secondes, soit {i/ElapsedTime} chemins/seconde");
		return PATHS;
	}

	static void Main() {
		Console.Clear();
		Console.Title = "Algorithme de Dijkstra";

		Console.WriteLine("Saisissez le chemin d'accès au .json : ");
		string? FilePath = Console.ReadLine();

		if (FilePath != null && FilePath != "") {
			Console.Clear();
			uint version = Fichiers.IdentifyFileVersion(@FilePath);
			switch (version) {
				case 2:
					Fichiers.JsonFile fichier = Fichiers.OpenFile(FilePath, version);

					Dictionary<string, Dictionary<string, object>> Vertices = Fichiers.ParseDots(fichier, version);
					List<Tuple<string, string>>? Links = Fichiers.ParseLinks(fichier, version);
					break;
				default:
					throw new Exception("Version du JSON non prise en charge");
			}

			string? dep_name, fin_name;

			Console.WriteLine("Saisissez le nom du point de départ : ");
			dep_name = Console.ReadLine();
			Console.WriteLine("Saisissez le nom du point d'arrivée : ");
			fin_name = Console.ReadLine();

			if (dep_name != null && fin_name != null && dep_name != "" && fin_name != "") {
				foreach (KeyValuePair<string, Tuple<double, double, double>> dot in Dots) {
					var Coords = dot.Value;
					SOMMETS_OG.Add(new Sommet(dot.Key, Coords.Item1, Coords.Item2, Coords.Item3));
				}

				foreach (Sommet s in SOMMETS_OG) {
					CORRESPONDANCE[s.name] = s;
				}

				foreach (Tuple<string, string> l in Links) {
					LIENS_OG.Add(l);
				}

				Sommet s_dep = CORRESPONDANCE[dep_name];
				Sommet s_fin = CORRESPONDANCE[fin_name];

				Stopwatch StopWatch = Stopwatch.StartNew();

				Tuple<Dictionary<Sommet, double>, Dictionary<Sommet, Sommet>> AlgoOutput = Algo(LIENS_OG, SOMMETS_OG, s_dep);
				Dictionary<Sommet, double> d = AlgoOutput.Item1;
				Dictionary<Sommet, Sommet> predecesseurs = AlgoOutput.Item2;

				Dictionary<string, string> predecesseurs_names = new Dictionary<string, string>();

				foreach (KeyValuePair<Sommet, Sommet> kvp in predecesseurs) {
					predecesseurs_names[kvp.Key.name] = kvp.Value.name;
				}

				Tuple<List<string>, double> TupleChemin = trouver_chemin(s_dep, s_fin, predecesseurs_names, d);
				double DistanceChemin = TupleChemin.Item2;
				List<string> Chemin = TupleChemin.Item1;

				StopWatch.Stop();

				long ElapsedTime = StopWatch.ElapsedMilliseconds;

				string output;

				output = string.Join(" -> ", Chemin.ToArray());

				Console.WriteLine($"Chemin pour aller de {dep_name} à {fin_name} ({DistanceChemin}) :\n{output}\nChemin trouvé en {ElapsedTime} ms\nAppuyez sur une 'Entrée' pour quitter");

				Console.ReadLine();
			} else {
				Console.WriteLine("Sortie du programme : Un des deux points n'a pas été renseigné\nAppuyez sur 'Entrée' pour fermer le programme :");
				Console.ReadLine();
			}
		} else {
			Console.WriteLine("Sortie du programme : Aucune valeur n'a été saisie dans le champ du chemin d'accès au .json\nAppuyez sur 'Entrée' pour fermer le programme :");
			Console.ReadLine();
		}
	}
}

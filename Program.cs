using System.Diagnostics;
using System.Text.Json;

namespace Dijkstra; 
internal class Program {

	// ------------------------------------------------------------------------

	public static Fichiers.JsonFile fichier;

	public static List<Utils.Sommet> SOMMETS_OG = new List<Utils.Sommet>();
	public static List<Tuple<string, string>> LIENS_OG = new List<Tuple<string, string>>();

	public static Dictionary<string, Utils.Sommet> CORRESPONDANCE = new Dictionary<string, Utils.Sommet>();

	static protected readonly Utils.Sommet SOMMET_NULL = new Utils.Sommet("null", double.PositiveInfinity, double.PositiveInfinity);

	// ------------------------------------------------------------------------

	public static void PreInit() {
		CORRESPONDANCE = Fichiers.ParseVertices(fichier);
		LIENS_OG = Fichiers.ParseLinks(fichier);

		foreach (Tuple<string, string> lien in LIENS_OG) {
			string A = lien.Item1;
			string B = lien.Item2;

			if (!CORRESPONDANCE[A].neighbours.Contains(B)) {
				CORRESPONDANCE[A].neighbours.Add(B);
			}
		}

		foreach (Utils.Sommet sommet in CORRESPONDANCE.Values) {
			SOMMETS_OG.Add(sommet);
		}
	}

	public static double Poids(Utils.Sommet s1, Utils.Sommet s2) {
		double poids = Math.Sqrt(Math.Pow(s2.x-s1.x, 2) + Math.Pow(s2.y-s1.y, 2) + Math.Pow(s2.z-s1.z, 2));
		return poids;
	}

	public static List<Utils.Sommet> Voisins(Utils.Sommet s, List<Utils.Sommet> SOMMETS, List<Tuple<string, string>> LIENS) {
		string s_name = s.name;

		List<Utils.Sommet> voisins = new List<Utils.Sommet>();

		foreach (Tuple<string, string> L in LIENS) {
			if (s_name == L.Item1) {
				Utils.Sommet s2 = CORRESPONDANCE[L.Item2];
				if (SOMMETS.Contains(s2)) {
					voisins.Add(s2);
				}
			}
		}

		return voisins;
	}

	public static Dictionary<Utils.Sommet, double> Initialisation(List<Utils.Sommet> SOMMETS, Utils.Sommet s_dep) {
		Dictionary<Utils.Sommet, double> d = new Dictionary<Utils.Sommet, double>();
		foreach (Utils.Sommet s in SOMMETS) {
			if (s != s_dep) {
				d[s] = double.PositiveInfinity;
			} else {
				d[s] = 0d;
			}
		}
		return d;
	}

	public static Utils.Sommet Trouve_min(List<Utils.Sommet> SOMMETS, Dictionary<Utils.Sommet, double> d) {
		double mini = double.PositiveInfinity;
		Utils.Sommet sommet = SOMMET_NULL;

		if (SOMMETS.Count > 0) {
			foreach (Utils.Sommet s in SOMMETS) {
				if (d[s] < mini) {
					mini = d[s];
					sommet = s;
				}
			}
		}
		return sommet;
	}

	public static Tuple<Dictionary<Utils.Sommet, double>, Dictionary<Utils.Sommet, Utils.Sommet>> maj_distances(Utils.Sommet s1, Utils.Sommet s2, Dictionary<Utils.Sommet, double> d, Dictionary<Utils.Sommet, Utils.Sommet> predecesseur) { 
		if (d[s2] > (d[s1] + Poids(s1, s2))) {
			d[s2] = d[s1] + Poids(s1, s2);
			predecesseur[s2] = s1;
		}
		return Tuple.Create(d, predecesseur);
	}

	public static Tuple<Dictionary<Utils.Sommet, double>, Dictionary<Utils.Sommet, Utils.Sommet>> Algo(List<Tuple<string, string>> LIENS, List<Utils.Sommet> SOMMETS, Utils.Sommet s_dep) {
		Dictionary<Utils.Sommet, double> d = Initialisation(SOMMETS, s_dep);
		Dictionary<Utils.Sommet, Utils.Sommet> predecesseur = new Dictionary<Utils.Sommet, Utils.Sommet>();

		List<Utils.Sommet> SOMMETS_WORK = new List<Utils.Sommet>();
		foreach (Utils.Sommet s in SOMMETS) {
			SOMMETS_WORK.Add(s);
		}

		Tuple<Dictionary<Utils.Sommet, double>, Dictionary<Utils.Sommet, Utils.Sommet>> Tuple_dP;

		while (SOMMETS_WORK.Count != 0) {
			Utils.Sommet s1 = Trouve_min(SOMMETS_WORK, d);

			if (s1 == SOMMET_NULL) {
				break;
			}
			if (SOMMETS_WORK.Contains(s1)) {
				SOMMETS_WORK.Remove(s1);
			}

			foreach (Utils.Sommet s2 in Voisins(s1, SOMMETS_WORK, LIENS)) {
				Tuple_dP = maj_distances(s1, s2, d, predecesseur);
				d = Tuple_dP.Item1;
				predecesseur = Tuple_dP.Item2;
			}
		}
		return Tuple.Create(d, predecesseur);
	}

	public static Tuple<List<string>, double> trouver_chemin(Utils.Sommet s_dep, Utils.Sommet s_fin, Dictionary<string, string> predecesseurs_names, Dictionary<Utils.Sommet, double> d) {
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

	public static Tuple<List<string>, double> faux_main(String s_dep, String s_fin, List<Tuple<string, string>> LIENS, List<Utils.Sommet> SOMMETS) {
		/* Cette fonction ne sert plus mais je la laisse là ne sachant pas quoi en faire */
		Utils.Sommet S_dep = CORRESPONDANCE[s_dep];
		Utils.Sommet S_fin = CORRESPONDANCE[s_fin];

		Tuple<Dictionary<Utils.Sommet, double>, Dictionary<Utils.Sommet, Utils.Sommet>> dP = Algo(LIENS, SOMMETS, S_dep);
		Dictionary<Utils.Sommet, double> d = dP.Item1;
		Dictionary<string, string> p = new Dictionary<string, string>();

		foreach (KeyValuePair<Utils.Sommet, Utils.Sommet> kvp in dP.Item2) {
			p[kvp.Key.name] = kvp.Value.name;
		}

		return trouver_chemin(S_dep, S_fin, p, d);
	}

	public static Dictionary<Tuple<string, string>, Tuple<List<string>, double>> AllPaths(List<Utils.Sommet> SOMMETS, List<Tuple<string, string>> LIENS) {
		/* Détermine tous les chemins possible avec une méthode bruteforce */
		Dictionary<Tuple<string, string>, Tuple<List<string>, double>> PATHS = new Dictionary<Tuple<string, string>, Tuple<List<string>, double>>();
		Console.WriteLine($"{SOMMETS.Count() * (SOMMETS.Count()-1)/2} chemins à déterminer");

		Stopwatch StopWatch = Stopwatch.StartNew();

		double i = 0;

		foreach (Utils.Sommet s_dep in SOMMETS) {
			foreach (Utils.Sommet s_fin in SOMMETS) {
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

		//List<Tuple<string, string>>? Links;

		if (!String.IsNullOrEmpty(FilePath)) {
			Console.Clear();

			fichier = Fichiers.OpenFile(FilePath);
			PreInit();

			// Console.WriteLine($"{LIENS_OG.Count()} liens et {SOMMETS_OG.Count()} points en mémoire, {CORRESPONDANCE.Count()}");

			string? dep_name, fin_name;

			Console.WriteLine("Saisissez le nom du point de départ : ");
			dep_name = Console.ReadLine();
			Console.WriteLine("Saisissez le nom du point d'arrivée : ");
			fin_name = Console.ReadLine();

			if (!(String.IsNullOrEmpty(dep_name) || String.IsNullOrEmpty(fin_name))) {

				Utils.Sommet s_dep = CORRESPONDANCE[dep_name];
				Utils.Sommet s_fin = CORRESPONDANCE[fin_name];

				Stopwatch StopWatch = Stopwatch.StartNew(); // On démarre le chrono pour mesurer le temps d'exécution de l'algo

				Tuple<Dictionary<Utils.Sommet, double>, Dictionary<Utils.Sommet, Utils.Sommet>> AlgoOutput = Algo(LIENS_OG, SOMMETS_OG, s_dep);
				Dictionary<Utils.Sommet, double> d = AlgoOutput.Item1;
				Dictionary<Utils.Sommet, Utils.Sommet> predecesseurs = AlgoOutput.Item2;

				Dictionary<string, string> predecesseurs_names = new Dictionary<string, string>();

				foreach (KeyValuePair<Utils.Sommet, Utils.Sommet> kvp in predecesseurs) {
					predecesseurs_names[kvp.Key.name] = kvp.Value.name;
				}

				Tuple<List<string>, double> TupleChemin = trouver_chemin(s_dep, s_fin, predecesseurs_names, d);
				double DistanceChemin = TupleChemin.Item2;
				List<string> Chemin = TupleChemin.Item1;

				StopWatch.Stop(); // Arrêt du chrono

				long ElapsedTime = StopWatch.ElapsedMilliseconds;

				string output;

				output = string.Join(" -> ", Chemin.ToArray());

				Console.WriteLine($"Chemin pour aller de {dep_name} à {fin_name} ({DistanceChemin}) :\n{output}\nChemin trouvé en {ElapsedTime} ms\nAppuyez sur 'Entrée' pour quitter");

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

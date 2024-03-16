using System.Text.Json;

namespace Dijkstra;

internal class Fichiers {

	// -----------------------------------------------------------------------------------------

	public struct Vertex {
		public double x { get; set; }
		public double y { get; set; }
		public double z { get; set; }
	}

	public struct JsonFile {
		public Dictionary<string, Vertex> Vertices { get; set; }
		public List<string> Segments { get; set; }
	}

	// -----------------------------------------------------------------------------------------

	public static JsonFile OpenFile(string filename) {
		string text = File.ReadAllText(filename);
		JsonFile fichier = JsonSerializer.Deserialize<JsonFile>(text);

		return fichier;
	}

	// -----------------------------------------------------------------------------------------

	public static Dictionary<string, Utils.Sommet> ParseVertices(JsonFile fichier) {
		Dictionary<string, Utils.Sommet> Vertices = new Dictionary<string, Utils.Sommet>();

		foreach (var pair in fichier.Vertices) {

			string name = pair.Key;

			Vertex CurrentVertex = pair.Value;

			Utils.Sommet CurrentSommet = new Utils.Sommet(name, CurrentVertex.x, CurrentVertex.y, CurrentVertex.z);
			Vertices.Add(name, CurrentSommet);
		}
		return Vertices;
	}

	public static List<Tuple<string, string>> ParseLinks(JsonFile fichier) {
		List<Tuple<string, string>> OutputList = new List<Tuple<string, string>>();

		string[] Separators = { " <-> ", " ->- "};

		foreach (string seg_name in fichier.Segments) {
			foreach (string sep in Separators) {
				if (seg_name.Contains(sep)) {
					string[] SplitName = seg_name.Split(sep);

					if (!OutputList.Contains(Tuple.Create(SplitName[0], SplitName[1]))) {
						OutputList.Add(Tuple.Create(SplitName[0], SplitName[1]));
					}

					if ((sep == " <-> " ) && !OutputList.Contains(Tuple.Create(SplitName[1], SplitName[0]))) {
						OutputList.Add(Tuple.Create(SplitName[1], SplitName[0]));
					}
					break;
				}
			}
		}

		return OutputList;
	}
}
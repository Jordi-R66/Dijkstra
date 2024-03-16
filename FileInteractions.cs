using System.Text.Json;

namespace Dijkstra;

public static class Fichiers {

	public static uint IdentifyFileVersion(string filepath) {
		string StringVersion = "";
		uint version = 0;

		using StreamReader stream = new StreamReader(filepath);
		List<string> Lines = stream.ReadToEnd().Split("\n").ToList();

		if (Lines.Count > 0 && Lines[1].Contains("\"Version\":")) {
			StringVersion = Lines[1].Split(":")[1];
		}

		version = Convert.ToUInt32(StringVersion);

		return version;
	}

	// -----------------------------------------------------------------------------------------

	public struct JsonFile {
		public uint Version { get; set; }
		public Dictionary<string, Dictionary<string, List<object>>> Vertices { get; set; }
	}

	// -----------------------------------------------------------------------------------------

	public static JsonFile OpenFile(string filename, uint Version) {
		string text = File.ReadAllText(filename);
		JsonFile fichier = JsonSerializer.Deserialize<JsonFile>(text);

		return fichier;
	}

	public static Dictionary<string, Dictionary<string, object>> ParseDots(JsonFile fichier, uint Version) {
		Dictionary<string, Dictionary<string, object>> Vertices = new Dictionary<string, Dictionary<string, object>>();

		foreach (var pair in fichier.Vertices) {

			Tuple<double, double, double> Coords;
			string name = pair.Key;

			foreach (var VertexDetails in pair.Value) {
				Console.WriteLine($"{pair.Key}: {VertexDetails.Key}: {VertexDetails.Value}");
				if (VertexDetails.Key == "Coordinates") {
					List<object> CoordsList_objects = VertexDetails.Value;
					List<double> CoordsList = new List<double>();

					for (byte i = 0; i < 3; i++) {
						CoordsList.Add(Convert.ToDouble(CoordsList_objects[i]));
					}
					Coords = Tuple.Create(CoordsList[0], CoordsList[1], CoordsList[2]);
					Vertices[name]["Coordinates"] = Coords;
				} else if (VertexDetails.Key == "Neighbours") {
					List<object> Neighbours_objects = VertexDetails.Value;
					List<string> Neighbours = new List<string>();

					for (uint i = 0; i < (uint)Neighbours_objects.Count(); i++) {
						Neighbours.Append(Convert.ChangeType(Neighbours_objects, typeof(string)));
					}
					Vertices[name]["Neighbours"] = Neighbours;
				}
			}
		}
		return Vertices;
	}

	public static List<Tuple<string, string>>? ParseLinks(JsonFile fichier, uint Version) {
		return null;
	}
}
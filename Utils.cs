using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dijkstra {
	internal class Utils {
		public struct Sommet {
			public double x;
			public double y;
			public double z;

			public string name;
			public List<string> neighbours;

			public Sommet(string Name, double X, double Y, double Z = 0.0, List<string>? Neighbours = null) {
				name = Name;
				x = X;
				y = Y;
				z = Z;
				neighbours = ( Neighbours != null ) ? Neighbours : new List<string>();
			}

			public readonly override string ToString() {
				return $"Sommet(name={name}, x={x}, y={y}, z={z})";
			}

			public override readonly bool Equals(object? obj) => obj is Sommet other && this.Equals(other);

			public readonly bool Equals(Sommet s) => ( ( s.x == x ) && ( s.y == y ) && ( s.z == z ) && ( s.name == name ) );

			public static bool operator ==(Sommet s1, Sommet s2) => s1.Equals(s2);

			public static bool operator !=(Sommet s1, Sommet s2) => !( s1.Equals(s2) );

		}
	}
}

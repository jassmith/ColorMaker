using System;
using System.Linq;
using System.Collections.Generic;

namespace ColorMixer
{
	public class Pallet
	{
		List<Color> colors;
		List<Scheme> schemes;

		public IEnumerable<Color> Colors {
			get {
				return colors.AsEnumerable ();
			}
		}

		public IEnumerable<Scheme> Scheme {
			get {
				return schemes.AsEnumerable ();
			}
		}

		public Pallet ()
		{
			colors = new List<Color> ();
			schemes = new List<Scheme> ();
		}

		public void AddScheme (Scheme scheme)
		{
			schemes.Add (scheme);
		}

		public void RemoveScheme (Scheme scheme)
		{
			schemes.Remove (scheme);
		}

		public void RemoveScheme (string name)
		{
			schemes.RemoveAll ((s) => s.Name == name);
		}

		public Scheme GetScheme (string name)
		{
			return schemes.FirstOrDefault ((s) => s.Name == name);
		}

		public void AddColor (Color color)
		{
			colors.Add (color);
		}

		public void RemoveColor (Color color)
		{
			colors.Remove (color);
		}
	}
}


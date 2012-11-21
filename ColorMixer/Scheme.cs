using System;
using System.Linq;
using System.Collections.Generic;

namespace ColorMixer
{
	public delegate IEnumerable<Color> GeneratorDelegate (Color color, int size);

	public static class SchemeGenerator
	{
		static IEnumerable<Color> randomOffset (Color color, int size)
		{
			Random rand = new Random ();
			for (int i = 0; i < size; i++) {
				double offset = (rand.NextDouble () - 0.5) * 0.2;
				yield return new Color (color.R + color.R * offset, 
				                        color.G + color.G * offset, 
				                        color.B + color.B * offset, 
				                        color.A);
			}
		}
		public static GeneratorDelegate RandomOffset = randomOffset;

		static IEnumerable<Color> desaturate (Color color, int size)
		{
			double startingSat = color.Saturation;
			for (int i = 0; i < size; i++) {
				Color newColor = color;
				newColor.Saturation = (startingSat / size) * (size - i);
				yield return newColor;
			}
		}
		public static GeneratorDelegate Desaturate = desaturate;
	}

	public class Scheme
	{

		List<Color> colors;

		public IEnumerable<Color> Colors { get {
				return colors.AsEnumerable ();
			}
		}

		public string Name { get; set; }

		public Scheme ()
		{
			colors = new List<Color> ();
		}

		public void AddColor (Color color)
		{
			colors.Add (color);
		}

		public void RemoveColor (Color color)
		{
			colors.Remove (color);
		}

		public static Scheme FromColor (Color source, GeneratorDelegate generator, int size)
		{
			var result = new Scheme ();

			foreach (var color in generator (source, size))
				result.AddColor (color);

			return result;
		}
	}
}


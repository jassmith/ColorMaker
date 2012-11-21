using System;

namespace ColorMixer
{
	public static class NumericExtensions
	{
		public static double Clamp (this double self, double min, double max)
		{
			return Math.Min (max, Math.Max (self, min));
		}
	}
}


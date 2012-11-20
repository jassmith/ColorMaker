using System;

namespace ColorMaker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				Console.WriteLine ("Hello World");
				ColorMixer.Color color = new ColorMixer.Color (0, 0, 1);
				Console.WriteLine ("H: {0} S: {1} L: {2}", color.Hue, color.Saturation, color.Luminosity);
				Console.WriteLine ("Does Not Print");
			} catch (Exception ex) {
				Console.WriteLine (ex.StackTrace);
				Console.WriteLine ("Alos Does Not Print");
			}
		}
	}
}

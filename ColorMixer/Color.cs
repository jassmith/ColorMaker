using System;

namespace ColorMixer
{
	public struct Color
	{
		bool hslDirty;
		bool rgbDirty;

		double a;
		public double A {
			get { return a; }
			set { a = value; }
		}

		double r;
		public double R {
			get {
				CheckDirty ();
				return r;
			}
			set {
				if (r == value)
					return;
				r = value;
				hslDirty = true;
			}
		}
		double g;
		public double G {
			get {
				CheckDirty ();
				return g;
			}
			set {
				if (g == value)
					return;
				g = value;
				hslDirty = true;
			}
		}
		double b;
		public double B {
			get {
				CheckDirty ();
				return b;
			}
			set {
				if (b == value)
					return;
				b = value;
				hslDirty = true;
			}
		}

		double hue;
		public double Hue {
			get {
				CheckDirty ();
				return hue;
			}
			set {
				if (hue == value)
					return;
				hue = value;
				rgbDirty = true;
			}
		}
		double saturation;
		public double Saturation {
			get {
				CheckDirty ();
				return saturation;
			}
			set {
				if (saturation == value)
					return;
				saturation = value;
				rgbDirty = true;
			}
		}
		double luminosity;
		public double Luminosity {
			get {
				CheckDirty ();
				return luminosity;
			}
			set {
				if (luminosity == value)
					return;
				luminosity = value;
				rgbDirty = true;
			}
		}

		public Color (double r, double g, double b, double a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;

			hue = luminosity = saturation = 0;

			hslDirty = true;
			rgbDirty = false;
		}

		public Color (double r, double g, double b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 1;

			hue = luminosity = saturation = 0;

			hslDirty = true;
			rgbDirty = false;
		}

		void CheckDirty ()
		{
			if (rgbDirty)
				UpdateRGBFromHSL ();
			else if (hslDirty)
				UpdateHSLFromRGB ();

			rgbDirty = false;
			hslDirty = false;
		}

		void UpdateRGBFromHSL ()
		{
			if (luminosity > 1) luminosity = 1;
			if (luminosity < 0) luminosity = 0;
			if (hue > 1) hue = 1;
			if (hue < 0) hue = 0;
			if (saturation > 1) saturation = 1;
			if (saturation < 0) saturation = 0;

			if (luminosity == 0) {
				r = g = b = 0;
				return;
			}
			
			if (saturation == 0) {
				r = g = b = luminosity;
			} else {
				double temp2 = luminosity <= 0.5 ? luminosity * (1.0 + saturation) : luminosity + saturation -(luminosity * saturation);
				double temp1 = 2.0 * luminosity - temp2;
				
				double[] t3 = new double[] { hue + 1.0 / 3.0, hue, hue - 1.0 / 3.0};
				double[] clr= new double[] { 0, 0, 0};
				for (int i = 0; i < 3; i++) {
					if (t3[i] < 0)
						t3[i] += 1.0;
					if (t3[i] > 1)
						t3[i]-=1.0;
					if (6.0 * t3[i] < 1.0)
						clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
					else if (2.0 * t3[i] < 1.0)
						clr[i] = temp2;
					else if (3.0 * t3[i] < 2.0)
						clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
					else
						clr[i] = temp1;
				}
				
				r = clr[0];
				g = clr[1];
				b = clr[2];
			}
		}

		void UpdateHSLFromRGB ()
		{
			double v = System.Math.Max (r, g);
			v = System.Math.Max (v, b);

			double m = System.Math.Min (r, g);
			m = System.Math.Min (m, b);
			
			double l = (m + v) / 2.0;
			if (l <= 0.0) {
				hue = saturation = luminosity = 0;
				return;
			}
			double vm = v - m;
			double s = vm;
			
			if (s > 0.0) {
				s /= (l <= 0.5) ? (v + m) : (2.0 - v - m);
			} else {
				hue = 0; saturation = 0; luminosity = l;
				return;
			}
			
			double r2 = (v - r) / vm;
			double g2 = (v - g) / vm;
			double b2 = (v - b) / vm;

			double h;
			if (r == v) {
				h = (g == m ? 5.0 + b2 : 1.0 - g2);
			} else if (G == v) {
				h = (b == m ? 1.0 + r2 : 3.0 - b2);
			} else {
				h = (r == m ? 3.0 + g2 : 5.0 - r2);
			}
			h /= 6.0;

			hue = h;
			saturation = s;
			luminosity = l;
		}

		// Cairo conversion
		public static implicit operator Cairo.Color (Color color)
		{
			return new Cairo.Color (color.R, color.G, color.B, color.A);
		}

		public static implicit operator Color (Cairo.Color cairoColor)
		{
			return new Color (cairoColor.R, cairoColor.G, cairoColor.B, cairoColor.A);
		}

		// Gdk conversion
		public static implicit operator Gdk.Color (Color color)
		{
			if (color.A != 1)
				Console.WriteLine ("Warning: Converting to non-alpha format from color with alpha");

			return new Gdk.Color ((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}

		public static implicit operator Color (Gdk.Color gdkColor)
		{
			return new Color (gdkColor.Red / (double)ushort.MaxValue, gdkColor.Green / (double)ushort.MaxValue, gdkColor.Blue / (double)ushort.MaxValue, 1);
		}
	}
}


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gtk;

using ColorMixer;

namespace ColorMaker
{
	public class PickerWidget : Gtk.EventBox
	{
		Cairo.ImageSurface pickerCircle;

		int radius;
		public int Radius {
			get { return radius; }
			set {
				radius = value;
				UpdateSize ();
			}
		}

		double saturation;
		public double Saturation {
			get {
				return saturation;
			}
			set {
				saturation = value;
				if (pickerCircle != null) {
					pickerCircle.Dispose ();
					pickerCircle = null;
				}
				QueueDraw ();
			}
		}

		Color? currentColor;
		Color? HighlightColor {
			get {
				return currentColor;
			}
			set {
				currentColor = value;
				if (value.HasValue) {
					var pmap = new Gdk.Pixmap (null, 1, 1, 1);
					var color = new Gdk.Color ();
					Gdk.Cursor cursor = new Gdk.Cursor (pmap, pmap, color, color, 0, 0);
					GdkWindow.Cursor = cursor;
				} else {
					GdkWindow.Cursor = null;
				}
				QueueDraw ();
			}
		}

		Color selectedColor;
		Color SelectedColor {
			get {
				return selectedColor;
			}
			set {
				selectedColor = value;
				QueueDraw ();
			}
		}

		public PickerWidget ()
		{
			VisibleWindow = false;
			AppPaintable = true;
			Radius = 150;
			Saturation = 1;

			SelectedColor = new Color (1, 1, 1, 1);

			Events |= Events | Gdk.EventMask.ButtonMotionMask | Gdk.EventMask.PointerMotionMask;
		}

		void UpdateSize ()
		{
			SetSizeRequest (Radius * 2 + 100, Radius * 2 + 200);
		}

		static double ConvertAngleToHue (double angle)
		{
			return (angle / (Math.PI * 2)) + 0.5;
		}

		static double ConvertHueToAngle (double hue)
		{
			return (hue - 0.5) * Math.PI * 2;
		}

		unsafe void DrawPickerCircle (Cairo.ImageSurface surface, int radius)
		{
			int width = surface.Width;
			int height = surface.Height;
			byte* data = (byte*) surface.DataPtr;
			Gdk.Point center = new Gdk.Point (width / 2, height / 2);
			Color pixelColor = new Color ();
			pixelColor.Saturation = Saturation;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					double deltaX = x - center.X;
					double deltaY = y - center.Y;
					double lumin = 1.0 - Math.Sqrt (deltaX * deltaX + deltaY * deltaY) / (radius);

					if (lumin < -0.1) {
						continue;
					}

					pixelColor.Hue = ConvertAngleToHue (Math.Atan2 (deltaY, deltaX));
					pixelColor.Luminosity = lumin;

					byte* pixelData = data + y * surface.Stride + x * 4;

					pixelData [0] = (byte)(byte.MaxValue * pixelColor.B);
					pixelData [1] = (byte)(byte.MaxValue * pixelColor.G);
					pixelData [2] = (byte)(byte.MaxValue * pixelColor.R);
				}
			}
		}

		Gdk.Point PickerCenter ()
		{
			return new Gdk.Point (Allocation.X + Allocation.Width / 2, Allocation.Y + Radius + 50);
		}

		Color? GetColorAtPosition (int x, int y)
		{
			Gdk.Point center = PickerCenter ();
			double deltaX = x - center.X;
			double deltaY = y - center.Y;

			double lumin = 1.0 - Math.Sqrt (deltaX * deltaX + deltaY * deltaY) / (radius);

			if (lumin < 0) {
				return null;
			}

			Color result;
			result.Hue = ConvertAngleToHue (Math.Atan2 (deltaY, deltaX));
			result.Luminosity = lumin;
			result.Saturation = Saturation;
			result.A = 1;

			return result;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evnt)
		{
			if (evnt.State.HasFlag (Gdk.ModifierType.Button1Mask)) {
				HighlightColor = GetColorAtPosition ((int)evnt.X, (int)evnt.Y);
				if (HighlightColor.HasValue)
					SelectedColor = HighlightColor.Value;
			}
			return base.OnMotionNotifyEvent (evnt);
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if (evnt.Button == 1) {
				HighlightColor = GetColorAtPosition ((int)evnt.X, (int)evnt.Y);
			}
			return base.OnButtonPressEvent (evnt);
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			if (evnt.Button == 1) {
				Color? color = GetColorAtPosition ((int)evnt.X, (int)evnt.Y);
				if (color.HasValue)
					SelectedColor = color.Value;
				HighlightColor = null;
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		void DrawColorOverlay (Cairo.Context context, Gdk.Point center, Color color)
		{
			double radius = 14.5;
			double angle = ConvertHueToAngle (color.Hue);
			double y = Math.Sin (angle) * Radius * (1.0 - color.Luminosity);
			double x = Math.Cos (angle) * Radius * (1.0 - color.Luminosity);
			context.Translate (x + center.X, y + center.Y + 1);
			context.Arc (0, 0, radius, 0, Math.PI * 2);
			context.Color = new Color (0, 0, 0, 0.3);
			context.LineWidth = 1;
			context.Stroke ();
			context.Translate (-(x + center.X), -(y + center.Y + 1));

			context.Translate (x + center.X, y + center.Y);
			context.Arc (0, 0, radius, 0, Math.PI * 2);
			context.Color = color;
			context.Fill ();
			context.Translate (-(x + center.X), -(y + center.Y));

			context.Translate (x + center.X, y + center.Y);
			context.Arc (0, 0, radius, 0, Math.PI * 2);
			context.Color = new Color (1, 1, 1);
			context.LineWidth = 1;
			context.Stroke ();
			context.Translate (-(x + center.X), -(y + center.Y));

		}

		void DrawSelectedColorBox (Cairo.Context context, Gdk.Rectangle region)
		{
			context.Rectangle (region.X + 0.5, region.Y + 0.5, region.Width - 1, region.Height - 1);
			context.Color = SelectedColor;
			context.FillPreserve ();

			context.LineWidth = 1;
			context.Color = new Color (0, 0, 0);
			context.Stroke ();
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (var context = Gdk.CairoHelper.Create (evnt.Window)) {

				if (pickerCircle == null || pickerCircle.Width != Radius * 2 + 100 || pickerCircle.Height != Radius * 2 + 100) {
					pickerCircle = new Cairo.ImageSurface (Cairo.Format.ARGB32, Radius * 2 + 100, Radius * 2 + 100);
					using (var g = new Cairo.Context (pickerCircle)) {
						// apparently quartz needs the surface initialized
						g.Color = new Color (0, 0, 0);
						g.Paint ();
					}
					DrawPickerCircle (pickerCircle, Radius);
				}

				Gdk.Point center = PickerCenter ();

				context.Arc (center.X, center.Y, Radius, 0, Math.PI * 2);
				context.Clip ();
				pickerCircle.Show (context, center.X - pickerCircle.Width / 2, center.Y - pickerCircle.Height / 2);

				context.ResetClip ();
				context.Arc (center.X, center.Y, Radius + 5, 0, Math.PI * 2);
				context.LineWidth = 10;

				using (var lg = new Cairo.LinearGradient (0, center.Y - (Radius + 30), 0, center.Y + (Radius + 30))) {
					lg.AddColorStop (0, new Color (0, 0, 0, 0.3));
					lg.AddColorStop (1, new Color (0, 0, 0, 0.1));
					context.Pattern = lg;
					context.Stroke ();
				}

				context.Arc (center.X, center.Y, Radius + 15, 0, Math.PI * 2);
				context.LineWidth = 10;

				using (var lg = new Cairo.LinearGradient (0, center.Y - (Radius + 30), 0, center.Y + (Radius + 30))) {
					lg.AddColorStop (0, new Color (0, 0, 0, 0.1));
					lg.AddColorStop (1, new Color (0, 0, 0, 0.3));
					context.Pattern = lg;
					context.Stroke ();
				}

				if (HighlightColor.HasValue) {
					DrawColorOverlay (context, center, HighlightColor.Value);
				}

				Gdk.Rectangle selectedRegion = new Gdk.Rectangle (Allocation.X + 40, Allocation.Bottom - 100, Allocation.Width - 80, 50);
				DrawSelectedColorBox (context, selectedRegion);
			}
			return base.OnExposeEvent (evnt);
		}
	}
}


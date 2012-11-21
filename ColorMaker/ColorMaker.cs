using System;

namespace ColorMaker
{
	public class ColorMaker : Gtk.EventBox
	{
		Gtk.HScale saturationScale;
		PickerWidget picker;

		public ColorMaker ()
		{
			Build ();
		}

		void Build ()
		{
			Gtk.VBox mainBox = new Gtk.VBox ();
			mainBox.BorderWidth = 10;

			picker = new PickerWidget ();
			mainBox.PackStart (picker);


			Gtk.HBox hbox = new Gtk.HBox ();
			hbox.PackStart (new Gtk.Label ("S:"), false, false, 0);
			saturationScale = new Gtk.HScale (0, 1, 0.01);
			saturationScale.Value = 1;
			saturationScale.ShowFillLevel = false;
			saturationScale.CanFocus = false;
			saturationScale.DrawValue = false;
			saturationScale.ValueChanged += (sender, e) => picker.Saturation = saturationScale.Value;

			hbox.PackStart (saturationScale, true, true, 0);
			mainBox.PackStart (hbox);

			Add (mainBox);
			ShowAll ();
		}
	}
}


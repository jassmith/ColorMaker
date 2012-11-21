using System;

using ColorMixer;

namespace ColorMaker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			GLib.Thread.Init ();
			Gtk.Application.Init ();

			ColorMaker maker = new ColorMaker ();

			Gtk.Window window = new Gtk.Window ("Test");
			window.Add (maker);
			window.ShowAll ();

			Gtk.Application.Run ();
		}
	}
}

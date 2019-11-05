// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED!
//
// IF YOU FIND ERRORS OR POSSIBLE IMPROVEMENTS, PLEASE LET ME KNOW.
// MAYBE TOGETHER WE CAN SOLVE THIS.
//
// YOU MAY USE THIS CODE: HOWEVER THIS GRANTS NO FUTURE RIGHTS.

using System;
using System.Text;

using Core;
using Telnet;

namespace Telnet.Demo
{
    //removed 25.06.2015  => Buildaction = NONE
	/// <summary>
	/// Some Test methods for the screen class
	/// </summary>
	public class VirtualScreenTest
	{
		/**
		/// <summary>
		/// The main entry point for the application.
		/// Can be used to test the programm and run it from command line.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			const string SEH = "--- screen ends here ---";
			// coordinates are 0,0 based since we do not specify anything
			VirtualScreen vs = new VirtualScreen(80, 40);
			// simple fill
			VirtualScreenTest.FillScreen(vs);
			Console.WriteLine(vs.Hardcopy());
			Console.WriteLine(SEH);
			Console.ReadLine();
			// deleting of an area
			vs.CleanScreen(4,4,75,35);
			Console.WriteLine(vs.Hardcopy());
			Console.WriteLine(SEH);
			Console.ReadLine();
			// scrolling
			vs.ScrollUp(2);
			Console.WriteLine(vs.Hardcopy());
			Console.WriteLine(SEH);
			Console.ReadLine();
			// block operations (write to screen)
			VirtualScreenTest.WriteBlockOperation(vs);
			Console.WriteLine(vs.Hardcopy());
			Console.WriteLine(SEH);
			Console.ReadLine();
			// cursor movements
			VirtualScreenTest.WriteBigX(vs);
			Console.WriteLine(vs.Hardcopy());
			Console.WriteLine(SEH);
			Console.ReadLine();

		} // main
		
		**/

		/// <summary>
		/// Fill the screen with a pattern
		/// </summary>
		/// <param name="vs">Virtual screen</param>
		public static void FillScreen(VirtualScreen vs) 
		{
			byte w = 48; // 48=0 57=9
			for (int y = 0; y < vs.Height; y++) 
			{
				w = 48; 
				for (int x = 0; x < vs.Width; x++) 
				{
					vs.WriteByte(w);
					if (w<57)
						w++;
					else
						w = 48;
				}
			}
		}

		/// <summary>
		/// Write a byte block and strings
		/// </summary>
		/// <param name="vs">Virtual screen</param>
		public static void WriteBlockOperation(VirtualScreen vs) 
		{
			vs.CleanScreen();
			byte[] tb = Encoding.ASCII.GetBytes("This is a test output");
			vs.WriteLine("New screen with test output:");
			for (int i=0; i<10; i++) 
			{
				vs.WriteByte(tb);
			}
			vs.WriteLine("\n");
			for (int i=0; i<10; i++) 
			{
				vs.WriteLine("Line output");
			}
		}

		/// <summary>
		/// Test cursor movements
		/// </summary>
		/// <param name="vs">Virtual screen</param>
		public static void WriteBigX(VirtualScreen vs) 
		{
			vs.CleanScreen();
			for (int i=0; i<vs.Height; i++) 
			{
				vs.Write('X'); // already moves cursor about one
				vs.MoveCursor(vs.Width); // test overflow
			}
			vs.Write('\r'); // beginning of last line
			for (int i=0; i<vs.Height; i++) 
			{
				vs.Write('X'); // already moves cursor about one
				vs.MoveCursor(-vs.Width); // test overflow
			}
		}

	} // class
} // namespace
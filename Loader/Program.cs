using System;
namespace RPGEngine
{
	class Program
	{
		public static Engine Engine;
		[STAThread]
		static void Main(string[] args)
		{
			Engine = new Engine(args);
			Engine.Init();
		}
	}
}

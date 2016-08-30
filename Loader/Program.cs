using System;
namespace RPGEngine
{
	class Program
	{
		public static RPGEngine Engine;
		[STAThread]
		static void Main(string[] args)
		{
			Engine = new RPGEngine(args);
			Engine.Init();
			Engine.Close();

		}
	}
}

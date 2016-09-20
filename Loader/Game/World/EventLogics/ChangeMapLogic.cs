using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGEngine
{
	public class ChangeMapLogic
	{
		public ChangeMapLogic(ref Map old_map, ref Map new_map, ref Player player, ref IntPtr renderer)
		{
			if (old_map != null)
				old_map.Close();

			SDL2.SDL.SDL_Delay(1);

			old_map = new_map;
			old_map.Load(ref renderer, ref player);
		}
	}
}

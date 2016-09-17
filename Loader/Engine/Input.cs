using System;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Input
	{
		private IntPtr joystick = IntPtr.Zero;
		private string name;

		public delegate void InputInitDoneEventHandler(object source, FinishEventArgs args);

		public delegate void InputInitErrorEventHandler(object source, ErrorEventArgs args);

		public delegate void InputInitStateEventHandler(object source, StateEventArgs args);

		public event InputInitDoneEventHandler InputInitDone;

		public event InputInitStateEventHandler InputInitState;

		public event InputInitErrorEventHandler InputInitError;

		public void Init(object obj)
		{
			this.joystick = SDL_JoystickOpen(SDL_NumJoysticks() - 1);
			this.name = SDL_JoystickName(this.joystick);

			if (this.joystick != IntPtr.Zero)
				this.OnInputInitState(GetType().ToString(), "using Joystick '{0}'".F(this.name));

			this.OnInputInitDone(GetType().ToString(), "Input initialized!");
		}

		public void Close()
		{
			if (this.joystick != IntPtr.Zero)
				SDL_JoystickClose(this.joystick);
		}

		#region Events
		protected virtual void OnInputInitDone(string source, string message)
		{
			var fe = new FinishEventArgs();
			fe.Source = source;
			fe.Message = message;

			this.InputInitDone?.Invoke(this, fe);
		}

		protected virtual void OnInputInitError(string source, string message)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = message;
			errvtargs.Source = source;

			this.InputInitError?.Invoke(this, errvtargs);
		}

		protected virtual void OnInputInitState(string source, string message)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = message;
			statevtargs.Source = source;

			this.InputInitState?.Invoke(this, statevtargs);
		}
		#endregion
	}
}

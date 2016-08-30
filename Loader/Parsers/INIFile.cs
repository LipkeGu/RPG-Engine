using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RPGEngine
{
	public class IniFile
	{
		string pfad;
		string defvalue;

		public IniFile(string path, string defvalue = null)
		{
			if (File.Exists(path))
			{
				this.pfad = path;
				this.defvalue = defvalue;
			}
		}

		[DllImport("kernel32", EntryPoint = "GetPrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		// DLL-Funktionen zum LESEN der INI deklarieren
		private static extern int GetPrivateProfileString(string lpApplicationName, string lpSchlüsselName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
		[DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

		//DLL-Funktion zum SCHREIBEN in die INI deklarieren
		private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
		[DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

		//DLL-Funktion zum Löschen einer ganzen Sektion deklarieren
		private static extern int DeletePrivateProfileSection(string Section, int NoKey, int NoSetting, string FileName);

		// Öffentliche Klassenvariablen

		public string WertLesen(string Sektion, string Schlüssel, int BufferSize = 1024)
		{
			// Auslesen des Wertes
			var value = new StringBuilder(BufferSize);
			GetPrivateProfileString(Sektion, Schlüssel, this.defvalue, value, BufferSize, this.pfad);
			return value.ToString();
		}

		public void WertSchreiben(string Sektion, string Schlüssel, string Wert)
		{
			WritePrivateProfileString(Sektion, Schlüssel, Wert, this.pfad);
		}

		public void SchlüsselLöschen(string Sektion, string Schlüssel)
		{
			WritePrivateProfileString(Sektion, Schlüssel, null, this.pfad);
		}

		public void SektionLöschen(string Sektion)
		{
			DeletePrivateProfileSection(Sektion, 0, 0, this.pfad);
		}
	}
}
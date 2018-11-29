using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Factunet
{

	public class IniFileHandler
	{
		private Dictionary<string, Dictionary<string, string>> _values = new Dictionary<string,Dictionary<string,string>>();

		private readonly char[] _crlf = new char[] { '\n', '\r' };
		private readonly char[] _valuetrimchars = new char[] {'"', ' '};
		private readonly string _filename;


		public IniFileHandler(string filename)
		{
			_filename = filename;
            ReadFile();
		}

		public void ReadFile()
		{
			string[] lines;
			using (StreamReader srFile = new System.IO.StreamReader(_filename, System.Text.Encoding.Default, true)) // File.OpenText(_filename))
			{
				lines = srFile.ReadToEnd().Split(_crlf);
			}

			Dictionary<string, string> currentValues = null;
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim();
				if ((line.Length > 0) && (!line.StartsWith(";")))
				{
					if (line.StartsWith("["))
					{
						// Add new section
						string section = line.Substring(1, line.Length - 2);
						if (_values.ContainsKey(section))
							throw new InvalidDataException("Duplicate section found in INI file: " +
							                               section);
						currentValues = new Dictionary<string, string>();
						_values.Add(section, currentValues);
					}
					else if (line.Contains("="))
					{
						string[] keyvalue = line.Split('=');
						string key = keyvalue[0];
						string value = keyvalue[1].Trim(_valuetrimchars);

						if (currentValues == null)
							throw new InvalidDataException("INI file format is invalid: " +
							                               "key before section found");
						if (currentValues.ContainsKey(key))
							throw new InvalidDataException("Duplicate key found in INI section: " +
							                               key);
						currentValues.Add(key, value);
					}
				}
			}
		}

		public string Requerido(string seccion, string nombre)
		{
			if (!_values.ContainsKey(seccion))
				throw new InvalidDataException("No se pudo agregar un nodo requerido. " +
					"La sección " + seccion + " del archivo " + _filename + " no existe " +
					"(valor buscado: " + nombre + ")");

			if (!_values[seccion].ContainsKey(nombre))
				throw new InvalidDataException("No se pudo agregar un nodo requerido. " +
					"El valor " + nombre + " en la sección " + seccion +
					" del archivo " + _filename + " no existe");

			return _values[seccion][nombre];
		}
		public decimal RequeridoDecimal(string seccion, string nombre)
		{
			return Convert.ToDecimal(Requerido(seccion, nombre));
		}
		public DateTime RequeridoFecha(string seccion, string nombre)
		{
			return Convert.ToDateTime(Requerido(seccion, nombre));
		}
		public T RequeridoEnum<T>(string seccion, string nombre) where T : struct
		{
			if(!typeof(T).IsEnum)
				throw new ArgumentException("Type T Must be an Enum");

			try
			{
				return (T)Enum.Parse(typeof(T), Requerido(seccion, nombre));
			}
			catch
			{
				throw new ArgumentException("El parámetro especificado en [" +
				                            seccion + "][" + nombre + "] no es válido: " +
				                            Requerido(seccion, nombre));
			}
		}

		public bool Existe(string seccion, string nombre)
		{
			return ((_values.ContainsKey(seccion)) &&
					(_values[seccion].ContainsKey(nombre)) &&
					!String.IsNullOrEmpty(_values[seccion][nombre]));
		}
		public string Opcional(string seccion, string nombre)
		{
			if (!_values.ContainsKey(seccion))
				return null;

			if (!_values[seccion].ContainsKey(nombre))
				return null;

			if (String.IsNullOrEmpty(_values[seccion][nombre]))
				return null;

			return _values[seccion][nombre];
		}
		public DateTime OpcionalFecha(string seccion, string nombre)
		{
			if (Existe(seccion, nombre))
				return Convert.ToDateTime(Opcional(seccion, nombre));
			else
				return DateTime.MinValue;
		}
		public T OpcionalEnum<T>(string seccion, string nombre) where T : struct
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException("Type T Must be an Enum");

			if (Opcional(seccion, nombre) == null)
				return default(T);

			try
			{
				return (T)Enum.Parse(typeof(T), Opcional(seccion, nombre));
			}
			catch
			{
				throw new ArgumentException("El parámetro especificado en [" +
											seccion + "][" + nombre + "] no es válido: " +
											Opcional(seccion, nombre));
			}
		}
		public decimal OpcionalDecimal(string seccion, string nombre)
		{
			if (Opcional(seccion, nombre) == null)
				return decimal.Zero;
			else
				return Convert.ToDecimal(Opcional(seccion, nombre));			
		}
		public int OpcionalEntero(string seccion, string nombre)
		{
			if (Opcional(seccion, nombre) == null)
				return 0;
			else
				return Convert.ToInt32(Opcional(seccion, nombre));
		}

        public string AddendaEdifact(string cadenaOriginal, string sello)
        {
            string[] lines;
            string line = string.Empty;
            using (StreamReader srFile = new System.IO.StreamReader(_filename, System.Text.Encoding.Default, true))
            {
                lines = srFile.ReadToEnd().Split(_crlf);
            }
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    line += lines[i] + "\r";
            }
            // Agrega la cadena original
            char[] _trimPipe = new char[] { '|' };
            line += @"[CO]|" + cadenaOriginal.Trim(_trimPipe) + "|\r";
            // Agrega el sello
            line += @"[SE]|" + sello.Trim(_trimPipe) + "|\r";

            return line;
        }
	}
}
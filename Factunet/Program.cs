using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Security.Cryptography;

namespace Factunet
{
	class Program
	{
		/// <summary>
		/// Genera una factura electrónica a partir de 4 archivos de entrada
		/// </summary>
		/// <param name="args">
		/// 1 - Datos Factura
		/// 2 - Datos Addenda
		/// 3 - Archivo FEL
		/// 4 - Archivo FEC
        /// 5 - Archivo de salida de la factura
        /// 6 - 2/3
        /// 7 - N/D/A/DA/E/S/C/Z
		/// 8 - Archivo de salida de la cadena
        /// 9 - Archivo de salida del sello
		/// </param>
		static void Main(string[] args)
		{
			

			try
			{
				if (args.Length < 6)
					throw new ArgumentException(
						"Se requieren al menos 5 parámetros: " + Environment.NewLine +
						"1 - Datos de la factura en formato de texto" + Environment.NewLine +
						"2 - Datos de la addenda en formato de texto" + Environment.NewLine +
						"3 - Archivo FEL" + Environment.NewLine +
						"4 - Archivo FEC" + Environment.NewLine +
						"5 - Nombre del archivo de salida" + Environment.NewLine +
                        "6 - Versión del CFD (2/3)" + Environment.NewLine +
						"7 - Tipo de factura a generar (N = Normal, D = Detallista, A = Addenda, S = AlSuper, E = EDIFACT, C = ComercExt, Z = Amazon)" + Environment.NewLine +
						"8 - Nombre del archivo de cadena (opcional)" + Environment.NewLine +
						"9 - Nombre del archivo de sello (opcional)");

				// Inicialización
				string iniFactura = args[0];
				string iniAdd = args[1];
				string archivoFel = args[2];
				string archivoFec = args[3];
				string archivoSalida = args[4];
                int versionFactura = Convert.ToInt32(args[5]);
				string tipoFactura = args[6];
				string archivoCadena = String.Empty;
				if (args.Length > 7)
					archivoCadena = args[7];
				string archivoSello = String.Empty;
				if (args.Length > 8)
					archivoSello = args[8];

				string llavePrivada;
				string llavePublica;
				string certificado;

                if ((versionFactura < 2) && (versionFactura > 3))
                    throw new Exception("La versión del CFD debe ser 2 o 3");

				try
				{
					llavePrivada = File.ReadAllText(archivoFel);
				}
				catch
				{
					throw new Exception("No se pudo leer el archivo fel especificado: " + archivoFel);
				}
				try
				{
					llavePublica = File.ReadAllText(archivoFec);
				}
				catch
				{
					throw new Exception("No se pudo leer el archivo fec especificado: " + archivoFec);
				}

				// Verificamos el certificado
				try
				{
					certificado = llavePublica.Substring(
						llavePublica.IndexOf("-----BEGIN CERTIFICATE-----") + "-----BEGIN CERTIFICATE-----".Length,
						llavePublica.IndexOf("-----END CERTIFICATE-----") - llavePublica.IndexOf("-----BEGIN CERTIFICATE-----") - "-----BEGIN CERTIFICATE-----".Length);
					certificado = certificado.Replace("\r", "");
					certificado = certificado.Replace("\n", "");
					certificado = certificado.Replace(" ", "");
					if (String.IsNullOrEmpty(certificado))
						throw new ArgumentException("Certificate");
				}
				catch
				{
					throw new Exception("No se pudo obtener el certificado del archivo fec especificado: " + archivoFec);
				}


				// Verificamos que la llave privada 

                string cadenaOriginal = string.Empty, sello = string.Empty;
                bool incluirDetallista = tipoFactura.ToLower().Contains("d");
                bool incluirAddenda = tipoFactura.ToLower().Contains("a");
                bool incluirAlsuper = tipoFactura.ToLower().Contains("s");
                bool incluirEdifact = tipoFactura.ToLower().Contains("e");
                bool incluirComercExt = tipoFactura.ToLower().Contains("c");
                bool incluirPago = tipoFactura.ToLower().Contains("p");
                bool incluirAmazon = tipoFactura.ToLower().Contains("z");

                // Si la versión del CFD es 2
                if (versionFactura == 2)
                {
                    // Creamos la representación en XML de la factura
                    // A partir de Julio 2012 se utiliza la version 22
                    if (DateTime.Now < Convert.ToDateTime("2012-07-01"))
                    {
                        FacturaElectronica fe = new FacturaElectronica(iniFactura, iniAdd,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact);
                        XmlDocument facturaXmlDoc = fe.GeneraFacturaXml(certificado, llavePrivada,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, out cadenaOriginal, out sello);
                        // Guardamos la factura generada
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(archivoSalida, new UTF8Encoding(true)))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            facturaXmlDoc.Save(xmlWriter);
                            xmlWriter.Close();
                        }
                    }
                    else
                    {
                        FacturaElectronicaV22 fe = new FacturaElectronicaV22(iniFactura, iniAdd,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact);
                        XmlDocument facturaXmlDoc = fe.GeneraFacturaXml(certificado, llavePrivada,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, out cadenaOriginal, out sello);
                        // Guardamos la factura generada
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(archivoSalida, new UTF8Encoding(true)))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            facturaXmlDoc.Save(xmlWriter);
                            xmlWriter.Close();
                        }
                    }
                }
                else if (versionFactura == 3)
                {
                    // Creamos la representación en XML de la factura
                    // A partir de Julio 2012 se utiliza la version 32
                    if (DateTime.Now < Convert.ToDateTime("2012-07-01"))
                    {
                        FacturaElectronicaV3 fe = new FacturaElectronicaV3(iniFactura, iniAdd,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact);
                        XmlDocument facturaXmlDoc = fe.GeneraFacturaXml(certificado, llavePrivada,
                            incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact,  out cadenaOriginal, out sello);

                        // Guardamos la factura generada
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(archivoSalida, new UTF8Encoding(true)))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            facturaXmlDoc.Save(xmlWriter);
                            xmlWriter.Close();
                        }
                    }
                    else if (DateTime.Now >= Convert.ToDateTime("2017-12-01"))
                    {
                        // A partir de Diciembre 2017 se utiliza la version 33
                        FacturaElectronicaV33modif fe = new FacturaElectronicaV33modif(iniFactura, iniAdd,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, incluirComercExt, incluirPago, incluirAmazon);
                        XmlDocument facturaXmlDoc = fe.GeneraFacturaXml(certificado, llavePrivada,
                            incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, incluirComercExt, incluirPago, incluirAmazon, out cadenaOriginal, out sello);

                        // Guardamos la factura generada
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(archivoSalida, new UTF8Encoding(true)))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            facturaXmlDoc.Save(xmlWriter);
                            xmlWriter.Close();
                        }
                    }
                    else
                    {
                        FacturaElectronicaV32 fe = new FacturaElectronicaV32(iniFactura, iniAdd,
                        incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, incluirComercExt);
                        XmlDocument facturaXmlDoc = fe.GeneraFacturaXml(certificado, llavePrivada,
                            incluirDetallista, incluirAddenda, incluirAlsuper, incluirEdifact, incluirComercExt, out cadenaOriginal, out sello);

                        // Guardamos la factura generada
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(archivoSalida, new UTF8Encoding(true)))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            facturaXmlDoc.Save(xmlWriter);
                            xmlWriter.Close();
                        }
                    }
                }

                // Guardamos la cadena en el archivo especificado
                if (!String.IsNullOrEmpty(archivoCadena))
                {
                    File.WriteAllText(archivoCadena, cadenaOriginal);
                }

                // Guardamos el sello en el archivo especificado
                if (!String.IsNullOrEmpty(archivoSello))
                {
                    File.WriteAllText(archivoSello, sello);
                }

				// Finalizamos el programa
				Console.WriteLine("Factura generada: " + archivoSalida);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.InnerException + ex.StackTrace);
				Environment.Exit(1);
			}
		}
	}
}

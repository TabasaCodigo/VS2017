﻿using System;
using System.Xml;
using Schemas;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Security.Cryptography;

namespace Factunet
{
    class FacturaElectronicaV22
    {
        private readonly IniFileHandler _iniFac;
		private readonly IniFileHandler _iniAdd;
        
		private Schemasv22.Comprobante _comprobante;
		
		private void GeneraComprobante()
		{
			#region Información del comprobante

			_comprobante = new Schemasv22.Comprobante
			{
				version = "2.2",
				serie = _iniFac.Opcional("Generales", "serie"),
				folio = _iniFac.Requerido("Generales", "folio"),
				fecha = _iniFac.RequeridoFecha("Generales", "fecha"),
				noAprobacion = _iniFac.Requerido("Generales", "noAprobacion"),
				anoAprobacion = _iniFac.Requerido("Generales", "anoAprobacion"),
				formaDePago = _iniFac.Requerido("Generales", "formaDePago"),
				noCertificado = _iniFac.Requerido("Generales", "noCertificado"),
                condicionesDePago = _iniFac.Opcional("Generales", "condicionesDePago"),
				subTotal = _iniFac.RequeridoDecimal("Generales", "subTotal"),
				descuento = _iniFac.OpcionalDecimal("Generales", "descuento"),
				descuentoSpecified = _iniFac.Existe("Generales", "descuento"),
				motivoDescuento = null,
                // Opcional
                TipoCambio = _iniFac.Opcional("Generales", "TipoCambio"),
                Moneda = _iniFac.Opcional("Generales", "Moneda"),
				total = _iniFac.RequeridoDecimal("Generales", "total"),
				tipoDeComprobante = _iniFac.RequeridoEnum<Schemasv22.ComprobanteTipoDeComprobante>(
					"Generales", "tipoDeComprobante"),
                metodoDePago = _iniFac.Requerido("Generales", "metodoDePago"),
                LugarExpedicion = _iniFac.Requerido("Generales", "LugarExpedicion"),
                // Datos opcionales para Ver22
                NumCtaPago = _iniFac.Opcional("Generales", "NumCtaPago"),
                FolioFiscalOrig = null,
                SerieFolioFiscalOrig = null,
                //FechaFolioFiscalOrig = null,
                FechaFolioFiscalOrigSpecified = false,
                MontoFolioFiscalOrig = 0,
                MontoFolioFiscalOrigSpecified = false,
			};

			#endregion

			#region Emisor

			_comprobante.Emisor = new Schemasv22.ComprobanteEmisor
			{
				rfc = _iniFac.Requerido("Emisor", "rfc"),
				nombre = _iniFac.Opcional("Emisor", "nombre"),  // de Requerido a Opcional

				DomicilioFiscal = new Schemasv22.t_UbicacionFiscal // Opcional
				{
					calle = _iniFac.Requerido("EmisorDomicilioFiscal", "calle"),
					noExterior =
						_iniFac.Opcional("EmisorDomicilioFiscal", "noExterior"),
					noInterior =
						_iniFac.Opcional("EmisorDomicilioFiscal", "noInterior"),
					colonia = _iniFac.Opcional("EmisorDomicilioFiscal", "colonia"),
                    localidad = _iniFac.Opcional("EmisorDomicilioFiscal", "localidad"),
                    referencia = _iniFac.Opcional("EmisorDomicilioFiscal", "referencia"),
					municipio =
						_iniFac.Requerido("EmisorDomicilioFiscal", "municipio"),
					estado = _iniFac.Requerido("EmisorDomicilioFiscal", "estado"),
					pais = _iniFac.Requerido("EmisorDomicilioFiscal", "pais"),
					codigoPostal =
						_iniFac.Requerido("EmisorDomicilioFiscal", "codigoPostal")
				},
				ExpedidoEn = new Schemasv22.t_Ubicacion // Opcional
				{
					calle = _iniFac.Opcional("EmisorExpedidoEn", "calle"),
					noExterior = _iniFac.Opcional("EmisorExpedidoEn", "noExterior"),
					noInterior = _iniFac.Opcional("EmisorExpedidoEn", "noInterior"),
					colonia = _iniFac.Opcional("EmisorExpedidoEn", "colonia"),
					municipio = _iniFac.Opcional("EmisorExpedidoEn", "municipio"),
					estado = _iniFac.Opcional("EmisorExpedidoEn", "estado"),
					pais = _iniFac.Requerido("EmisorExpedidoEn", "pais"),
					codigoPostal = _iniFac.Opcional("EmisorExpedidoEn", "codigoPostal")
				},
            };
            int cantidadRegimenFiscal =     // Nuevo en Ver22
                Convert.ToInt32(_iniFac.Requerido("EmisorRegimenFiscal", "cantidadRegimenFiscal"));
            _comprobante.Emisor.RegimenFiscal = new Schemasv22.ComprobanteEmisorRegimenFiscal[cantidadRegimenFiscal];
            for (int i = 0; i < cantidadRegimenFiscal; i++)
            {
                string regimenFiscalSection = String.Format("regimenFiscal{0}", (i + 1));
                _comprobante.Emisor.RegimenFiscal[i] = new Schemasv22.ComprobanteEmisorRegimenFiscal
                {
                    Regimen = _iniFac.Requerido("EmisorRegimenFiscal", regimenFiscalSection)
                };
            }
            
			#endregion

			#region Receptor

			_comprobante.Receptor = new Schemasv22.ComprobanteReceptor
			{
				rfc = _iniFac.Requerido("Receptor", "rfc"),
				nombre = _iniFac.Opcional("Receptor", "nombre"),
				Domicilio = new Schemasv22.t_Ubicacion
				{
					calle = _iniFac.Opcional("ReceptorDomicilio", "calle"),
					noExterior =
						_iniFac.Opcional("ReceptorDomicilio", "noExterior"),
					noInterior =
						_iniFac.Opcional("ReceptorDomicilio", "noInterior"),
					colonia = _iniFac.Opcional("ReceptorDomicilio", "colonia"),
                    localidad = _iniFac.Opcional("ReceptorDomicilio", "localidad"),
                    referencia = _iniFac.Opcional("ReceptorDomicilio", "referencia"),
					municipio = _iniFac.Opcional("ReceptorDomicilio", "municipio"),
					estado = _iniFac.Opcional("ReceptorDomicilio", "estado"),
					pais = _iniFac.Requerido("ReceptorDomicilio", "pais"),
					codigoPostal =
						_iniFac.Opcional("ReceptorDomicilio", "codigoPostal")
				}
			};

			#endregion

			#region Conceptos

			int cantidadConceptos =
				Convert.ToInt32(_iniFac.Requerido("Conceptos", "cantidadConceptos"));
			_comprobante.Conceptos = new Schemasv22.ComprobanteConcepto[cantidadConceptos];
			for (int i = 0; i < cantidadConceptos; i++)
			{
				string conceptoSection = String.Format("Concepto{0}", (i + 1));
				_comprobante.Conceptos[i] = new Schemasv22.ComprobanteConcepto
				{
					cantidad = _iniFac.RequeridoDecimal(conceptoSection, "cantidad"),
					unidad = _iniFac.Requerido(conceptoSection, "unidad"),      // de Opcional a Requerido
					noIdentificacion = null,
					// Opcional
					descripcion = _iniFac.Requerido(conceptoSection, "descripcion"),
					valorUnitario =
						_iniFac.RequeridoDecimal(conceptoSection, "valorUnitario"),
					importe = _iniFac.RequeridoDecimal(conceptoSection, "importe"),
					Items = GetInformacionAduanera(_iniFac, conceptoSection)
				};
			}

			#endregion

			#region Impuestos

			_comprobante.Impuestos = new Schemasv22.ComprobanteImpuestos
			{
				totalImpuestosRetenidos =
					_iniFac.OpcionalDecimal("Impuestos", "totalImpuestosRetenidos"),
				totalImpuestosRetenidosSpecified =
					_iniFac.Existe("Impuestos", "totalImpuestosRetenidos"),
				totalImpuestosTrasladados =
					_iniFac.OpcionalDecimal("Impuestos", "totalImpuestosTrasladados"),
				totalImpuestosTrasladadosSpecified =
					_iniFac.Existe("Impuestos", "totalImpuestosTrasladados"),
			};

            int cantidadRetenciones = _iniFac.OpcionalEntero("Traslados", "cantidadRetenciones");
            if (cantidadRetenciones > 0)
            {
                _comprobante.Impuestos.Retenciones =
                        new Schemasv22.ComprobanteImpuestosRetencion[cantidadRetenciones];

                for (int i = 0; i < cantidadRetenciones; i++)
                {
                    string retencionSection = String.Format("Retencion{0}", i + 1);
                    _comprobante.Impuestos.Retenciones[i] = new Schemasv22.ComprobanteImpuestosRetencion
                    {
                        impuesto = _iniFac.RequeridoEnum<Schemasv22.ComprobanteImpuestosRetencionImpuesto>
                                (retencionSection, "impuesto"),
                        importe = _iniFac.RequeridoDecimal(retencionSection, "importe")
                    };
                }
            }

			int cantidadTraslados = _iniFac.OpcionalEntero("Traslados",
															  "cantidadTraslados");
			_comprobante.Impuestos.Traslados =
				new Schemasv22.ComprobanteImpuestosTraslado[cantidadTraslados];

			for (int i = 0; i < cantidadTraslados; i++)
			{
				string trasladoSection = String.Format("Traslado{0}", i + 1);
				_comprobante.Impuestos.Traslados[i] = new Schemasv22.ComprobanteImpuestosTraslado
				{
					impuesto = _iniFac.RequeridoEnum<Schemasv22.ComprobanteImpuestosTrasladoImpuesto>
						(trasladoSection, "impuesto"),
					tasa = _iniFac.RequeridoDecimal(trasladoSection, "tasa"),
					importe = _iniFac.RequeridoDecimal(trasladoSection, "importe")
				};
			}

			#endregion
		}
		
		// Carga valores de la factura desde archivo de texto
		public FacturaElectronicaV22(string archivoFac, string archivoAdd,
            bool incluirDetallista, bool incluirAddenda, bool incluirAlsuper, bool incluirEdifact)
		{
			// Carga los valores del archivo de factura
			_iniFac = new IniFileHandler(archivoFac);
            if (incluirDetallista || incluirAddenda || incluirAlsuper || incluirEdifact)
            {
                _iniAdd = new IniFileHandler(archivoAdd);
            }
		}

		#region Funciones auxiliares para generar la estructura de la factura
        private static Schemasv22.t_InformacionAduanera[] GetInformacionAduanera(IniFileHandler iniHandler, string seccion)
		{
            int cantidadPedimentos = Convert.ToInt32(iniHandler.Requerido(seccion, "cantidadPedimentos"));
            if (cantidadPedimentos == 0)
                return null;

            Schemasv22.t_InformacionAduanera[] result = new Schemasv22.t_InformacionAduanera[cantidadPedimentos];
            for (int i = 0; i < cantidadPedimentos; i++)
            {
                string pedimentoSection = String.Format("InformacionAduaneranumero{0}", (i + 1));
                string fechaSection = String.Format("InformacionAduanerafecha{0}", (i + 1));
                string aduanaSection = String.Format("InformacionAduaneraaduana{0}", (i + 1));

                result[i] = new Schemasv22.t_InformacionAduanera
                {
                    numero = iniHandler.Requerido(seccion, pedimentoSection),
                    fecha = iniHandler.RequeridoFecha(seccion, fechaSection),
                    aduana = iniHandler.Opcional(seccion, aduanaSection),   // de Requerido a Opcional
                };
            }
			return result;
		}
		#endregion

		public XmlDocument GeneraFacturaXml(string certificado, string llave,
			bool incluirDetallista, bool incluirAddenda, bool incluirAlsuper, 
			bool incluirEdifact, out string cadenaOriginal, out string sello)
		{
            // tipoComplemento: detallista, addenda, alsuper
            string tipoComplemento = string.Empty;

			// Generamos el comprobante y agregamos el certificado
			GeneraComprobante();
			_comprobante.certificado = certificado;

			#region Agregamos el nodo detallista

			if (incluirDetallista)
			{
                tipoComplemento = "detallista";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

				_comprobante.Complemento = new Schemasv22.ComprobanteComplemento
					{
						Any = new[] {tempDocument["detallista:detallista"]}
					};
			}

			#endregion

			#region Generamos un documento XML usando la información actual de comprobante y detallista

			XmlDocument doc = new XmlDocument();
			using (MemoryStream tempStream = new MemoryStream())
			{
				XmlSerializer serializer = new XmlSerializer(typeof (Schemasv22.Comprobante));
				serializer.Serialize(tempStream, _comprobante);
				tempStream.Seek(0, SeekOrigin.Begin);
				doc.Load(tempStream);
			}

			#endregion

			#region Generamos la cadena original

			using (MemoryStream tempStream = new MemoryStream())
			{
				using (
					XmlTextWriter xmlWriter = new XmlTextWriter(tempStream, Encoding.UTF8))
				{

					doc.WriteContentTo(xmlWriter);
					xmlWriter.Flush();
					tempStream.Seek(0, SeekOrigin.Begin);
					XPathDocument xpathFactura = new XPathDocument(tempStream);
					xmlWriter.Close();

					// Generamos la cadena original usando el archivo XSLT del SAT Ver22
					XslCompiledTransform xslCadena = new XslCompiledTransform();
                    xslCadena.Load("cadenaoriginal22.xslt");

					using (MemoryStream cadenaStream = new MemoryStream())
					{
						xslCadena.Transform(xpathFactura, null, cadenaStream);
						cadenaOriginal = cadenaStream.GetString();
					}
				}
			}

            // Elimina saltos de linea y espacios en blanco entre los campos de la cadena original
            char[] crlf = new char[] { '\n', '\r' };
            string[] cadenaLineas = cadenaOriginal.Split(crlf);
            cadenaOriginal = null;
            for (int i = 0; i < cadenaLineas.Length; i++)
            {
                if ((cadenaLineas[i].Length >= 2) || (cadenaLineas[i].StartsWith("-")))
                    cadenaOriginal += cadenaLineas[i].Substring(2).Trim();
            }

			#endregion

			#region Generamos el sello de la factura

			RSACryptoServiceProvider provider = OpenSSL.GetRsaProviderFromPem(llave);
			if (provider == null)
				throw new Exception(
					"No se pudo crear el proveedor de seguridad a partir del archivo fel");

            // Si la fecha es menor al 1 de Enero 2011, codifica con MD5 sino codifica con SHA1
            byte[] selloBytes;
            if (DateTime.Now < Convert.ToDateTime("2011-01-01"))
            {
                selloBytes = provider.SignData(
                Encoding.UTF8.GetBytes(cadenaOriginal), "MD5");
            }
            else
            {
                selloBytes = provider.SignData(
                Encoding.UTF8.GetBytes(cadenaOriginal), "SHA1");
            }
			
			sello = Convert.ToBase64String(selloBytes);

			// Actualizamos el documento original con el sello
			_comprobante.sello = sello;

			#endregion

			#region Agregamos la addenda

			if (incluirAddenda)
			{
				tipoComplemento = "addenda";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento, cadenaOriginal);
                
                _comprobante.Addenda = new Schemasv22.ComprobanteAddenda
					{
						Any = new[]
							{
								tempDocument["Addenda"]["requestForPayment"]
							}
					};
			}

			#endregion

            #region Agrega addenda Alsuper
            if (incluirAlsuper)
            {
                tipoComplemento = "alsuper";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

                _comprobante.Addenda = new Schemasv22.ComprobanteAddenda
                {
                    Any = new[]
							{
								tempDocument["alsuper:Alsuper"]
							}
                };
            }
            #endregion

            #region Agrega addenda EDIFACT
            if (incluirEdifact)
            {
                tipoComplemento = "edifact";
                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);
                
                _comprobante.Addenda = new Schemasv22.ComprobanteAddenda
                {
                    Any = new[]
                    {
                        tempDocument["Addenda"]["Documento"]
                    }
                };
            }
            #endregion

            #region Generamos el documento final
            using (MemoryStream tempStream = new MemoryStream())
			{
				doc = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(typeof(Schemasv22.Comprobante));			
				serializer.Serialize(tempStream, _comprobante);
				tempStream.Seek(0, SeekOrigin.Begin);
				doc.Load(tempStream);
			}
			#endregion

			#region Agregamos otros atributos del documento
			XmlAttribute xsiAttrib = doc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            string textoAtributo = "http://www.sat.gob.mx/cfd/2 " +
                "http://www.sat.gob.mx/sitio_internet/cfd/2/cfdv22.xsd " +
                "http://www.sat.gob.mx/detallista " +
                "http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd";
            if (incluirAlsuper)
            {
                textoAtributo = textoAtributo + " http://proveedores.alsuper.com/CFD " +
                    "http://proveedores.alsuper.com/addenda/1.xsd";
            }
            xsiAttrib.InnerText = textoAtributo; 
            doc["Comprobante"].Attributes.Append(xsiAttrib);
			#endregion

            return doc;
		}
    }
}

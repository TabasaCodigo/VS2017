using System;
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
    class FacturaElectronicaV3
    {
        private readonly IniFileHandler _iniFac;
		private readonly IniFileHandler _iniAdd;

		private Schemasv3.Comprobante _comprobante;
		
		private void GeneraComprobante()
		{
			#region Información del comprobante

            _comprobante = new Schemasv3.Comprobante
			{
				version = "3.0",
				serie = _iniFac.Opcional("Generales", "serie"),
				folio = _iniFac.Opcional("Generales", "folio"),
				fecha = _iniFac.RequeridoFecha("Generales", "fecha"),
				formaDePago = _iniFac.Requerido("Generales", "formaDePago"),
				noCertificado = _iniFac.Requerido("Generales", "noCertificado"),
                condicionesDePago = null,
				// Opcional
                subTotal = _iniFac.RequeridoDecimal("Generales", "subTotal"),
				descuento = _iniFac.OpcionalDecimal("Generales", "descuento"),
				descuentoSpecified = _iniFac.Existe("Generales", "descuento"),
				motivoDescuento = null,
				// Opcional
                TipoCambio = _iniFac.Opcional("Generales", "TipoCambio"),
                Moneda = _iniFac.Opcional("Generales", "Moneda"),
				total = _iniFac.RequeridoDecimal("Generales", "total"),
                tipoDeComprobante = _iniFac.RequeridoEnum<Schemasv3.ComprobanteTipoDeComprobante>(
					"Generales", "tipoDeComprobante")
			};

			#endregion

			#region Emisor

            _comprobante.Emisor = new Schemasv3.ComprobanteEmisor
			{
				rfc = _iniFac.Requerido("Emisor", "rfc"),
				nombre = _iniFac.Requerido("Emisor", "nombre"),

                DomicilioFiscal = new Schemasv3.t_UbicacionFiscal // Requerido
				{
					calle = _iniFac.Requerido("EmisorDomicilioFiscal", "calle"),
					noExterior =
						_iniFac.Opcional("EmisorDomicilioFiscal", "noExterior"),
					noInterior =
						_iniFac.Opcional("EmisorDomicilioFiscal", "noInterior"),
					colonia = _iniFac.Opcional("EmisorDomicilioFiscal", "colonia"),
					localidad =
						_iniFac.Opcional("EmisorDomicilioFiscal", "localidad"),
                    referencia = 
                        _iniFac.Opcional("EmisorDomicilioFiscal", "referencia"),
                    municipio = 
                        _iniFac.Requerido("EmisorDomicilioFiscal", "municipio"),
					estado = _iniFac.Requerido("EmisorDomicilioFiscal", "estado"),
					pais = _iniFac.Requerido("EmisorDomicilioFiscal", "pais"),
					codigoPostal =
						_iniFac.Requerido("EmisorDomicilioFiscal", "codigoPostal")
				},
                ExpedidoEn = new Schemasv3.t_Ubicacion // Opcional
				{
					calle = _iniFac.Opcional("EmisorExpedidoEn", "calle"),
					noExterior = _iniFac.Opcional("EmisorExpedidoEn", "noExterior"),
					noInterior = _iniFac.Opcional("EmisorExpedidoEn", "noInterior"),
					colonia = _iniFac.Opcional("EmisorExpedidoEn", "colonia"),
                    localidad = _iniFac.Opcional("EmisorExpedidoEn", "localidad"),
                    referencia = _iniFac.Opcional("EmisorExpedidoEn", "referencia"),
					municipio = _iniFac.Opcional("EmisorExpedidoEn", "municipio"),
					estado = _iniFac.Opcional("EmisorExpedidoEn", "estado"),
					pais = _iniFac.Requerido("EmisorExpedidoEn", "pais"),
					codigoPostal = _iniFac.Opcional("EmisorExpedidoEn", "codigoPostal")
				}
            };

			#endregion

			#region Receptor

            _comprobante.Receptor = new Schemasv3.ComprobanteReceptor
			{
				rfc = _iniFac.Requerido("Receptor", "rfc"),
				nombre = _iniFac.Opcional("Receptor", "nombre"),
                Domicilio = new Schemasv3.t_Ubicacion
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
            _comprobante.Conceptos = new Schemasv3.ComprobanteConcepto[cantidadConceptos];
			for (int i = 0; i < cantidadConceptos; i++)
			{
				string conceptoSection = String.Format("Concepto{0}", (i + 1));
                _comprobante.Conceptos[i] = new Schemasv3.ComprobanteConcepto
				{
					cantidad = _iniFac.RequeridoDecimal(conceptoSection, "cantidad"),
					unidad = _iniFac.Opcional(conceptoSection, "unidad"),
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

            _comprobante.Impuestos = new Schemasv3.ComprobanteImpuestos
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

			int cantidadTraslados = _iniFac.OpcionalEntero("Traslados",
															  "cantidadTraslados");

			_comprobante.Impuestos.Traslados =
                new Schemasv3.ComprobanteImpuestosTraslado[cantidadTraslados];

			for (int i = 0; i < cantidadTraslados; i++)
			{
				string trasladoSection = String.Format("Traslado{0}", i + 1);
                _comprobante.Impuestos.Traslados[i] = new Schemasv3.ComprobanteImpuestosTraslado
				{
                    impuesto = _iniFac.RequeridoEnum<Schemasv3.ComprobanteImpuestosTrasladoImpuesto>
						(trasladoSection, "impuesto"),
					tasa = _iniFac.RequeridoDecimal(trasladoSection, "tasa"),
					importe = _iniFac.RequeridoDecimal(trasladoSection, "importe")
				};
			}

			// TODO: Faltan los impuestos retenidos (opcional)

			#endregion
		}
		
		// Carga valores de la factura desde archivo de texto
		public FacturaElectronicaV3(string archivoFac, string archivoAdd,
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
		private static Schemasv3.t_InformacionAduanera[] GetInformacionAduanera(IniFileHandler iniHandler, string seccion)
		{
            int cantidadPedimentos = Convert.ToInt32(iniHandler.Requerido(seccion, "cantidadPedimentos"));
            if (cantidadPedimentos == 0)
                return null;

            Schemasv3.t_InformacionAduanera[] result = new Schemasv3.t_InformacionAduanera[cantidadPedimentos];
            for (int i = 0; i < cantidadPedimentos; i++)
            {
                string pedimentoSection = String.Format("InformacionAduaneranumero{0}", (i + 1));
                string fechaSection = String.Format("InformacionAduanerafecha{0}", (i + 1));
                string aduanaSection = String.Format("InformacionAduaneraaduana{0}", (i + 1));

                result[i] = new Schemasv3.t_InformacionAduanera
                {
                    numero = iniHandler.Requerido(seccion, pedimentoSection),
                    fecha = iniHandler.RequeridoFecha(seccion, fechaSection),
                    aduana = iniHandler.Requerido(seccion, aduanaSection),
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

                _comprobante.Complemento = new Schemasv3.ComprobanteComplemento
					{
						Any = new[] {tempDocument["detallista:detallista"]}
					};
			}

			#endregion

			#region Generamos un documento XML usando la información actual de comprobante y detallista

			XmlDocument doc = new XmlDocument();
			using (MemoryStream tempStream = new MemoryStream())
			{
				XmlSerializer serializer = new XmlSerializer(typeof (Schemasv3.Comprobante));

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

					// Generamos la cadena original usando el archivo XSLT del SAT
					XslCompiledTransform xslCadena = new XslCompiledTransform();
					xslCadena.Load( "cadenaoriginal30.xslt");

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

            // La encriptación de la versión 3 debe ser en SHA-1
			RSACryptoServiceProvider provider = OpenSSL.GetRsaProviderFromPem(llave);
			if (provider == null)
				throw new Exception(
					"No se pudo crear el proveedor de seguridad a partir del archivo fel");
			byte[] selloBytes = provider.SignData(
				Encoding.UTF8.GetBytes(cadenaOriginal), "SHA1" );
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

				_comprobante.Addenda = new Schemasv3.ComprobanteAddenda
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

                _comprobante.Addenda = new Schemasv3.ComprobanteAddenda
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

                _comprobante.Addenda = new Schemasv3.ComprobanteAddenda
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
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
                XmlSerializer serializer = new XmlSerializer(typeof(Schemasv3.Comprobante));
                
				serializer.Serialize(tempStream, _comprobante, namespaces);
				tempStream.Seek(0, SeekOrigin.Begin);
				doc.Load(tempStream);
			}
			#endregion

			#region Agregamos otros atributos del documento
			XmlAttribute xsiAttrib = doc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            string textoAtributo = "http://www.sat.gob.mx/cfd/3 cfdv3.xsd " +
                "http://www.sat.gob.mx/detallista " +
                "http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd";
            if (incluirAlsuper)
            {
                textoAtributo = textoAtributo + " http://proveedores.alsuper.com/CFD " +
                    "http://proveedores.alsuper.com/addenda/1.xsd";
            }
            xsiAttrib.InnerText = textoAtributo;
			doc["cfdi:Comprobante"].Attributes.Append(xsiAttrib);
			#endregion
            return doc;
		}
    }
}

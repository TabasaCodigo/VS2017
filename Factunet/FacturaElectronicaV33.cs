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
    class FacturaElectronicaV33
    {
        private readonly IniFileHandler _iniFac;
        private readonly IniFileHandler _iniAdd;
        
        private Schemasv33.Comprobante _comprobante;

        private void GeneraComprobante()
        {
            #region Información del comprobante
            _comprobante = new Schemasv33.Comprobante
            {
                Version = "3.3",
                Serie = _iniFac.Opcional("Generales", "Serie"),
                Folio = _iniFac.Opcional("Generales", "Folio"),
                Fecha = _iniFac.RequeridoFecha("Generales", "Fecha"),
                FormaPago = _iniFac.OpcionalEnum<Schemasv33.c_FormaPago>("Generales", "FormaPago"),
                FormaPagoSpecified = _iniFac.Existe("Generales", "FormaPago"),
                NoCertificado = _iniFac.Requerido("Generales", "NoCertificado"),
                CondicionesDePago = _iniFac.Opcional("Generales", "CondicionesDePago"),
                SubTotal = _iniFac.RequeridoDecimal("Generales", "SubTotal"),
                Descuento = _iniFac.OpcionalDecimal("Generales", "Descuento"),
                DescuentoSpecified = _iniFac.Existe("Generales", "Descuento"),
                Moneda = _iniFac.RequeridoEnum<Schemasv33.c_Moneda>("Generales", "Moneda"),
                TipoCambio = _iniFac.OpcionalDecimal("Generales", "TipoCambio"),
                TipoCambioSpecified = _iniFac.Existe("Generales", "TipoCambio"),
                Total = _iniFac.RequeridoDecimal("Generales", "Total"),
                TipoDeComprobante = _iniFac.RequeridoEnum<Schemasv33.c_TipoDeComprobante>(
                    "Generales", "TipoDeComprobante"),
                MetodoPago = _iniFac.OpcionalEnum<Schemasv33.c_MetodoPago>("Generales", "MetodoPago"),
                MetodoPagoSpecified = _iniFac.Existe("Generales", "MetodoPago"),
                LugarExpedicion = _iniFac.RequeridoEnum<Schemasv33.c_CodigoPostal>("Generales", "LugarExpedicion"),
                Confirmacion = _iniFac.Opcional("Generales", "Confirmacion")
            };
            #endregion

            #region Cfdi Relacionados
            int CantidadCfdiRelacionados = Convert.ToInt32(_iniFac.Requerido("CfdiRelacionados", "CantidadCfdiRelacionados"));
            if (CantidadCfdiRelacionados > 0)
            {
                _comprobante.CfdiRelacionados = new Schemasv33.ComprobanteCfdiRelacionados
                {
                    CfdiRelacionado = GetCfdiRelacionados(_iniFac, CantidadCfdiRelacionados),
                    TipoRelacion = _iniFac.RequeridoEnum<Schemasv33.c_TipoRelacion>("CfdiRelacionados", "TipoRelacion")
                };
            }
            #endregion

            #region Emisor
            _comprobante.Emisor = new Schemasv33.ComprobanteEmisor
            {
                Rfc = _iniFac.Requerido("Emisor", "Rfc"),
                Nombre = _iniFac.Opcional("Emisor", "Nombre"),
                RegimenFiscal = _iniFac.RequeridoEnum<Schemasv33.c_RegimenFiscal>("Emisor", "RegimenFiscal")
            };
            #endregion

            #region Receptor
            _comprobante.Receptor = new Schemasv33.ComprobanteReceptor
            {
                Rfc = _iniFac.Requerido("Receptor", "Rfc"),
                Nombre = _iniFac.Opcional("Receptor", "Nombre"),
                ResidenciaFiscal = _iniFac.OpcionalEnum<Schemasv33.c_Pais>("Receptor", "ResidenciaFiscal"), // Requerido si se incluye Compl ComercExt
                ResidenciaFiscalSpecified = _iniFac.Existe("Receptor", "ResidenciaFiscal"),
                NumRegIdTrib = _iniFac.Opcional("Receptor", "NumRegIdTrib"),    // Requerido si se incluye Compl ComercExt
                UsoCFDI = _iniFac.RequeridoEnum<Schemasv33.c_UsoCFDI>("Receptor", "UsoCFDI")
            };
            #endregion

            #region Conceptos
            int CantidadConceptos =
                Convert.ToInt32(_iniFac.Requerido("Conceptos", "CantidadConceptos"));
            _comprobante.Conceptos = new Schemasv33.ComprobanteConcepto[CantidadConceptos];
            for (int i = 0; i < CantidadConceptos; i++)
            {
                string conceptoSection = String.Format("Concepto{0}", (i + 1));
                _comprobante.Conceptos[i] = new Schemasv33.ComprobanteConcepto
                {
                    Impuestos = GetConceptoImpuestos(_iniFac, conceptoSection),
                    InformacionAduanera = GetInformacionAduanera(_iniFac, conceptoSection),
                    ClaveProdServ = _iniFac.RequeridoEnum<Schemasv33.c_ClaveProdServ>(conceptoSection, "ClaveProdServ"),
                    NoIdentificacion = _iniFac.Opcional(conceptoSection, "NoIdentificacion"),    // CodBarras o SKU o similar
                    Cantidad = _iniFac.RequeridoDecimal(conceptoSection, "Cantidad"),
                    ClaveUnidad = _iniFac.RequeridoEnum<Schemasv33.c_ClaveUnidad>(conceptoSection, "ClaveUnidad"),
                    Unidad = _iniFac.Opcional(conceptoSection, "Unidad"),
                    Descripcion = _iniFac.Requerido(conceptoSection, "Descripcion"),
                    ValorUnitario = _iniFac.RequeridoDecimal(conceptoSection, "ValorUnitario"),
                    Importe = _iniFac.RequeridoDecimal(conceptoSection, "Importe"),
                    Descuento = _iniFac.OpcionalDecimal(conceptoSection, "Descuento"),
                    DescuentoSpecified = _iniFac.Existe(conceptoSection, "Descuento")
                };
            }
            #endregion

            #region Impuestos
            _comprobante.Impuestos = new Schemasv33.ComprobanteImpuestos
            {
                Retenciones = GetImpuestosRetencion(_iniFac, "Impuestos"),
                Traslados = GetImpuestosTraslado(_iniFac, "Impuestos"),
                TotalImpuestosRetenidos = _iniFac.OpcionalDecimal("Impuestos", "TotalImpuestosRetenidos"),
                TotalImpuestosRetenidosSpecified = _iniFac.Existe("Impuestos", "TotalImpuestosRetenidos"),
                TotalImpuestosTrasladados = _iniFac.OpcionalDecimal("Impuestos", "TotalImpuestosTrasladados"),
                TotalImpuestosTrasladadosSpecified = _iniFac.Existe("Impuestos", "TotalImpuestosTrasladados")
            };
            #endregion
        }

        // Carga valores de la factura desde archivo de texto
        public FacturaElectronicaV33(string archivoFac, string archivoAdd,
            bool incluirDetallista, bool incluirAddenda, bool incluirAlsuper, bool incluirEdifact, bool incluirComercExt, bool incluirPago)
        {
            // Carga los valores del archivo de factura
            _iniFac = new IniFileHandler(archivoFac);
            if (incluirDetallista || incluirAddenda || incluirAlsuper || incluirEdifact || incluirComercExt || incluirPago)
            {
                _iniAdd = new IniFileHandler(archivoAdd);
            }
        }

        #region Funciones auxiliares para generar la estructura de la factura
        Schemasv33.ComprobanteCfdiRelacionadosCfdiRelacionado[] GetCfdiRelacionados(IniFileHandler iniHandler, int cantidadCfdiRelacionados)
        {
            if (cantidadCfdiRelacionados == 0)
                return null;

            Schemasv33.ComprobanteCfdiRelacionadosCfdiRelacionado[] result = 
                new Schemasv33.ComprobanteCfdiRelacionadosCfdiRelacionado[cantidadCfdiRelacionados];
            for (int i = 0; i < cantidadCfdiRelacionados; i++)
            {
                string seccion = String.Format("CfdiRelacionado{0}", (i + 1));
                result[i] = new Schemasv33.ComprobanteCfdiRelacionadosCfdiRelacionado
                {
                    UUID = iniHandler.Requerido(seccion, "UUID")
                };
            }
            return result;
        }

        Schemasv33.ComprobanteConceptoInformacionAduanera[] GetInformacionAduanera(IniFileHandler iniHandler, string seccion)
        {
            int cantidadPedimentos = Convert.ToInt32(iniHandler.Requerido(seccion, "CantidadPedimentos"));
            if (cantidadPedimentos == 0)
                return null;

            Schemasv33.ComprobanteConceptoInformacionAduanera[] result = new Schemasv33.ComprobanteConceptoInformacionAduanera[cantidadPedimentos];
            for (int i = 0; i < cantidadPedimentos; i++)
            {
                string pedimentoSection = String.Format("InformacionAduaneraNumeroPedimento{0}", (i + 1));
                result[i] = new Schemasv33.ComprobanteConceptoInformacionAduanera
                {
                    NumeroPedimento = iniHandler.Requerido(seccion, pedimentoSection)
                };
            }
            return result;
        }

        Schemasv33.ComprobanteConceptoImpuestos GetConceptoImpuestos(IniFileHandler iniHandler, string seccion)
        {
            int cantidadTraslados = Convert.ToInt32(iniHandler.Requerido(seccion, "CantidadTraslados"));
            int cantidadRetenciones = Convert.ToInt32(iniHandler.Requerido(seccion, "CantidadRetenciones"));

            if (cantidadRetenciones == 0 && cantidadTraslados == 0)
                return null;

            Schemasv33.ComprobanteConceptoImpuestosTraslado[] traslados = new Schemasv33.ComprobanteConceptoImpuestosTraslado[cantidadTraslados];
            Schemasv33.ComprobanteConceptoImpuestosRetencion[] retenciones = new Schemasv33.ComprobanteConceptoImpuestosRetencion[cantidadRetenciones];

            if (cantidadTraslados > 0)
            {
                for (int i = 0; i < cantidadTraslados; i++)
                {
                    string baseSection = String.Format("ImpuestosTraslado{0}Base", (i + 1));
                    string impuestoSection = String.Format("ImpuestosTraslado{0}Impuesto", (i + 1));
                    string tipoFactorSection = String.Format("ImpuestosTraslado{0}TipoFactor", (i + 1));
                    string tasaOCuotaSection = String.Format("ImpuestosTraslado{0}TasaOCuota", (i + 1));
                    string importeSection = String.Format("ImpuestosTraslado{0}Importe", (i + 1));
                    traslados[i] = new Schemasv33.ComprobanteConceptoImpuestosTraslado
                    {
                        Base = iniHandler.RequeridoDecimal(seccion, baseSection),
                        Impuesto = iniHandler.RequeridoEnum<Schemasv33.c_Impuesto>(seccion, impuestoSection),
                        TipoFactor = iniHandler.RequeridoEnum<Schemasv33.c_TipoFactor>(seccion, tipoFactorSection),
                        TasaOCuota = iniHandler.OpcionalEnum<Schemasv33.c_TasaOCuota>(seccion, tasaOCuotaSection),
                        TasaOCuotaSpecified = iniHandler.Existe(seccion, tasaOCuotaSection),
                        Importe = iniHandler.OpcionalDecimal(seccion, importeSection),
                        ImporteSpecified = iniHandler.Existe(seccion, importeSection)
                    };
                }
            }
            if (cantidadRetenciones > 0)
            {
                for (int i = 0; i < cantidadRetenciones; i++)
                {
                    string baseSection = String.Format("ImpuestosRetencion{0}Base", (i + 1));
                    string impuestoSection = String.Format("ImpuestosRetencion{0}Impuesto", (i + 1));
                    string tipoFactorSection = String.Format("ImpuestosRetencion{0}TipoFactor", (i + 1));
                    string tasaOCuotaSection = String.Format("ImpuestosRetencion{0}TasaOCuota", (i + 1));
                    string importeSection = String.Format("ImpuestosRetencion{0}Importe", (i + 1));
                    retenciones[i] = new Schemasv33.ComprobanteConceptoImpuestosRetencion
                    {
                        Base = iniHandler.RequeridoDecimal(seccion, baseSection),
                        Impuesto = iniHandler.RequeridoEnum<Schemasv33.c_Impuesto>(seccion, impuestoSection),
                        TipoFactor = iniHandler.RequeridoEnum<Schemasv33.c_TipoFactor>(seccion, tipoFactorSection),
                        TasaOCuota = iniHandler.RequeridoDecimal(seccion, tasaOCuotaSection),
                        Importe = iniHandler.RequeridoDecimal(seccion, importeSection)
                    };
                }
            }

            Schemasv33.ComprobanteConceptoImpuestos result = new Schemasv33.ComprobanteConceptoImpuestos
            {
                Traslados = traslados,
                Retenciones = retenciones
            };
            return result;
        }

        Schemasv33.ComprobanteImpuestosRetencion[] GetImpuestosRetencion(IniFileHandler iniHandler, string seccion)
        {
            int cantidadImpuestosRetencion = Convert.ToInt32(_iniFac.Requerido("Impuestos", "CantidadRetenciones"));
            if (cantidadImpuestosRetencion == 0)
                return null;

            Schemasv33.ComprobanteImpuestosRetencion[] result = new Schemasv33.ComprobanteImpuestosRetencion[cantidadImpuestosRetencion];
            for (int i = 0; i < cantidadImpuestosRetencion; i++)
            {
                string impuestoSection = String.Format("ImpuestosRetencion{0}Impuesto", (i + 1));
                string importeSection = String.Format("ImpuestosRetencion{0}Importe", (i + 1));
                result[i] = new Schemasv33.ComprobanteImpuestosRetencion
                {
                    Impuesto = iniHandler.RequeridoEnum<Schemasv33.c_Impuesto>(seccion, impuestoSection),
                    Importe = iniHandler.RequeridoDecimal(seccion, importeSection),
                };
            }
            return result;
        }

        Schemasv33.ComprobanteImpuestosTraslado[] GetImpuestosTraslado(IniFileHandler iniHandler, string seccion)
        {
            int cantidadImpuestosTraslado = Convert.ToInt32(_iniFac.Requerido("Impuestos", "CantidadTraslados"));
            if (cantidadImpuestosTraslado == 0)
                return null;
            Schemasv33.ComprobanteImpuestosTraslado[] result = new Schemasv33.ComprobanteImpuestosTraslado[cantidadImpuestosTraslado];
            for (int i = 0; i < cantidadImpuestosTraslado; i++)
            {
                string impuestoSection = String.Format("ImpuestosTraslado{0}Impuesto", (i + 1));
                string tipoFactorSection = String.Format("ImpuestosTraslado{0}TipoFactor", (i + 1));
                string tasaOCuotaSection = String.Format("ImpuestosTraslado{0}TasaOCuota", (i + 1));
                string importeSection = String.Format("ImpuestosTraslado{0}Importe", (i + 1));
                
                result[i] = new Schemasv33.ComprobanteImpuestosTraslado
                {
                    Impuesto = iniHandler.RequeridoEnum<Schemasv33.c_Impuesto>(seccion, impuestoSection),
                    TipoFactor = iniHandler.RequeridoEnum<Schemasv33.c_TipoFactor>(seccion, tipoFactorSection),
                    TasaOCuota = iniHandler.RequeridoEnum<Schemasv33.c_TasaOCuota>(seccion, tasaOCuotaSection),
                    Importe = iniHandler.RequeridoDecimal(seccion, importeSection),
                };
            }
            return result;
        }

        #endregion Funciones auxiliares para generar la estructura de la factura

        public XmlDocument GeneraFacturaXml(string certificado, string llave,
            bool incluirDetallista, bool incluirAddenda, bool incluirAlsuper,
            bool incluirEdifact, bool incluirComercExt, bool incluirPago, out string cadenaOriginal, out string sello)
        {
            int cantComplementos = 0;

            // tipoComplemento: detallista, addenda, alsuper
            string tipoComplemento = string.Empty;

            // Generamos el comprobante y agregamos el certificado
            GeneraComprobante();
            _comprobante.Certificado = certificado;

            #region Agregamos el nodo detallista
            if (incluirDetallista)
            {
                tipoComplemento = "detallista";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

                _comprobante.Complemento = new Schemasv33.ComprobanteComplemento[1];
                _comprobante.Complemento[0] = new Schemasv33.ComprobanteComplemento
                {
                    Any = new[] { tempDocument["detallista:detallista"] }
                };
                cantComplementos += 1;
            }
            #endregion

            #region Agrega Complemento Comercio Exterior
            if (incluirComercExt)
            {
                tipoComplemento = "comercExterior";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

                _comprobante.Complemento = new Schemasv33.ComprobanteComplemento[1];
                _comprobante.Complemento[0] = new Schemasv33.ComprobanteComplemento
                {
                    Any = new[] { tempDocument["cce11:ComercioExterior"] }
                };
                cantComplementos += 1;
            }

            #endregion

            #region Agrega Complemento de Pago
            if (incluirPago)
            {
                tipoComplemento = "pago";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

                _comprobante.Complemento = new Schemasv33.ComprobanteComplemento[1];
                _comprobante.Complemento[0] = new Schemasv33.ComprobanteComplemento
                {
                    Any = new[] { tempDocument["pago10:Pagos"] }
                };
                cantComplementos += 1;
            }
            #endregion

            #region Generamos un documento XML usando la información actual de comprobante, detallista y ComercExt
            XmlDocument doc = new XmlDocument();
            using (MemoryStream tempStream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Schemasv33.Comprobante));

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

                    // Generamos la cadena original usando el archivo XSLT del SAT Ver33
                    XslCompiledTransform xslCadena = new XslCompiledTransform();
                    xslCadena.Load("cadenaoriginal_3_3.xslt");

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
            // La encriptación de la versión 33 debe ser en SHA-256
            RSACryptoServiceProvider provider = OpenSSL.GetRsaProviderFromPem(llave);
            if (provider == null)
                throw new Exception(
                    "No se pudo crear el proveedor de seguridad a partir del archivo fel");
            byte[] selloBytes = provider.SignData(
                Encoding.UTF8.GetBytes(cadenaOriginal), "SHA256");
            sello = Convert.ToBase64String(selloBytes);

            // Actualizamos el documento original con el sello
            _comprobante.Sello = sello;

            #endregion

            #region Agregamos la addenda
            if (incluirAddenda)
            {
                tipoComplemento = "addenda";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento, cadenaOriginal);

                _comprobante.Addenda = new Schemasv33.ComprobanteAddenda
                {
                    Any = new[] { tempDocument["Addenda"]["requestForPayment"] }
                };
            }
            #endregion

            #region Agrega addenda Alsuper
            if (incluirAlsuper)
            {
                tipoComplemento = "alsuper";

                ComplementoFE complemento = new ComplementoFE(_iniAdd);
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento);

                _comprobante.Addenda = new Schemasv33.ComprobanteAddenda
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
                XmlDocument tempDocument = complemento.GeneraComplementoXml(tipoComplemento, cadenaOriginal, sello);

                _comprobante.Addenda = new Schemasv33.ComprobanteAddenda
                {
                    Any = new[]
                    {
                        tempDocument["lev1add:EDCINVOICE"]
                    }
                };
            }
            #endregion

            #region  Genera documento final
            using (MemoryStream tempStream = new MemoryStream())
            {
                doc = new XmlDocument();
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
                XmlSerializer serializer = new XmlSerializer(typeof(Schemasv33.Comprobante));

                serializer.Serialize(tempStream, _comprobante, namespaces);
                tempStream.Seek(0, SeekOrigin.Begin);
                doc.Load(tempStream);
            }

            #endregion

            #region Agregar atributos finales al documento
            XmlAttribute xsiAttrib = doc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            string textoAtributo = "http://www.sat.gob.mx/cfd/3 " +
                "http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd " +
                "http://www.sat.gob.mx/detallista " +
                "http://www.sat.gob.mx/sitio_internet/cfd/detallista/detallista.xsd";
            if (incluirAlsuper)
            {
                textoAtributo = textoAtributo + " http://proveedores.alsuper.com/CFD " +
                    "http://proveedores.alsuper.com/addenda/1.xsd";
            }
            if (incluirComercExt)
            {
                textoAtributo = textoAtributo + " http://www.sat.gob.mx/ComercioExterior11 " +
                    "http://www.sat.gob.mx/sitio_internet/cfd/ComercioExterior11/ComercioExterior11.xsd";
            }
            if (incluirPago)
            {
                textoAtributo = textoAtributo + " http://www.sat.gob.mx/Pagos " +
                    "http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos10.xsd";
            }

            xsiAttrib.InnerText = textoAtributo;
            doc["cfdi:Comprobante"].Attributes.Append(xsiAttrib);
            #endregion

            return doc;
        }
    }
}

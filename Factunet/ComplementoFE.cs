using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Schemas;
using System.Xml.Serialization;
using System.IO;

namespace Factunet
{
    public class ComplementoFE
    {
        private readonly IniFileHandler _iniAdd;

        private detallista _detallista;
        private Addenda _addenda;
        private Alsuper _alSuper;
        private ElementosAmazon _amazon;
        private Schemas.ComercExt11.ComercioExterior _comercExt;
        private Schemas.ComercExt11modif.ComercioExterior _comercExtModif;
        private Schemas.ComercExt10.ComercioExterior _comercExt10;
        private Schemas.Pagos _pagos10;

        private void GeneraDetallista()
        {
            #region General
            _detallista = new detallista
            {
                // Atributos
                type = _iniAdd.Opcional("requestForPayment", "type"),
                contentVersion = _iniAdd.Requerido("requestForPayment", "contentVersion"),
                documentStructureVersion = "AMC8.1",
                documentStatus = _iniAdd.RequeridoEnum<detallistaDocumentStatus>(
                    "requestForPayment", "documentStatus")
            };

            #endregion

            #region detallista:requestForPaymentIdentification

            _detallista.requestForPaymentIdentification = new detallistaRequestForPaymentIdentification
            {
                entityType =
                    _iniAdd.RequeridoEnum<detallistaRequestForPaymentIdentificationEntityType>(
                        "requestForPaymentIdentification", "entityType")
            };

            #endregion

            #region detallista:specialInstruction

            // Opcional
            if (_iniAdd.Existe("specialInstruction", "code"))
            {
                _detallista.specialInstruction = new detallistaSpecialInstruction[]
					{
						new detallistaSpecialInstruction
							{
								code = _iniAdd.RequeridoEnum<detallistaSpecialInstructionCode>(
									"specialInstruction", "code"),
								text = new[]
									{
										_iniAdd.Requerido("specialInstruction", "text")
									}
							}
					};
            }

            #endregion

            #region detallista:orderIdentification

            _detallista.orderIdentification = new detallistaOrderIdentification
            {
                referenceIdentification =
                    new[]
							{
								new detallistaOrderIdentificationReferenceIdentification
									{
										type = detallistaOrderIdentificationReferenceIdentificationType.ON,
										// Fixed
										// "orderIdentification", "referenceIdentificationtype"
										Value =
											_iniAdd.Requerido("orderIdentification", "referenceIdentification")
									}
							},
                ReferenceDate = _iniAdd.OpcionalFecha("orderIdentification", "referenceDate"),
                ReferenceDateSpecified = _iniAdd.Existe("orderIdentification", "referenceDate")
            };

            #endregion

            #region detallista:AdditionalInformation

            _detallista.AdditionalInformation = new[]
				{
					new detallistaReferenceIdentification
						{
							Value =
								_iniAdd.Requerido("AdditionalInformation", "referenceIdentification"),
							type = _iniAdd.RequeridoEnum<detallistaReferenceIdentificationType>(
								"AdditionalInformation", "referenceIdentificationtype")
						}
				};

            #endregion

            #region detallista:DeliveryNote
            // Opcional
            if (_iniAdd.Existe("DeliveryNote", "referenceIdentification"))
            {
                _detallista.DeliveryNote = new detallistaDeliveryNote
                {
                    referenceIdentification = new[]
							{
								_iniAdd.Requerido("DeliveryNote", "referenceIdentification")
							},
                    ReferenceDate =
                        _iniAdd.OpcionalFecha("DeliveryNote", "ReferenceDate"),
                    ReferenceDateSpecified = _iniAdd.Existe("DeliveryNote", "ReferenceDate")
                };
            }

            #endregion

            #region detallista:buyer

            _detallista.buyer = new detallistaBuyer
            {
                gln = _iniAdd.Requerido("buyer", "gln"),
            };

            // Esto es requerido por algunas departamentales
            if (_iniAdd.Existe("buyer", "personOrDepartmentName"))
            {
                _detallista.buyer.contactInformation = new detallistaBuyerContactInformation
                {
                    personOrDepartmentName =
                        new detallistaBuyerContactInformationPersonOrDepartmentName
                        {
                            text = _iniAdd.Requerido("buyer", "personOrDepartmentName")
                        }
                };
            }

            #endregion

            #region detallista:seller

            _detallista.seller = new detallistaSeller
            {
                gln = _iniAdd.Requerido("seller", "gln"),
                alternatePartyIdentification =
                    new detallistaSellerAlternatePartyIdentification
                    {
                        type =
                            _iniAdd.RequeridoEnum<detallistaSellerAlternatePartyIdentificationType>(
                                "seller", "alternatePartyIdentificationtype"),
                        Value = _iniAdd.Requerido("seller", "alternatePartyIdentification")
                    }
            };

            #endregion

            #region Nodos opcionales

            // Estos nodos son opcionales

            //_detallista.shipTo
            //_detallista.InvoiceCreator
            //_detallista.Customs
            //_detallista.currency
            //_detallista.paymentTerms
            //_detallista.shipmentDetail

            #endregion

            #region detallista:allowanceCharge
            if (_iniAdd.Existe("allowanceCharge", "type"))
            {

                _detallista.allowanceCharge = new[]
					{
						new detallistaAllowanceCharge
							{
								allowanceChargeType =
									_iniAdd.RequeridoEnum<detallistaAllowanceChargeAllowanceChargeType>(
										"allowanceCharge", "type"),
								settlementType =
									_iniAdd.RequeridoEnum<detallistaAllowanceChargeSettlementType>(
										"allowanceCharge", "settlementType"),
								sequenceNumber = null,
								// Opcional

								specialServicesType =
									_iniAdd.OpcionalEnum<detallistaAllowanceChargeSpecialServicesType>(
										"allowanceCharge", "specialServicesType"),
								specialServicesTypeSpecified =
									_iniAdd.Existe("allowanceCharge", "specialServicesType"),
							}
					};
                // Este nodo es opcional
                if (_iniAdd.Existe("allowanceChargemonetary", "percentage"))
                {
                    _detallista.allowanceCharge[0].monetaryAmountOrPercentage =
                        new detallistaAllowanceChargeMonetaryAmountOrPercentage
                        {
                            rate = new detallistaAllowanceChargeMonetaryAmountOrPercentageRate
                            {
                                percentage =
                                    _iniAdd.RequeridoDecimal("allowanceChargemonetary", "percentage"),
                                @base =
                                    _iniAdd.RequeridoEnum
                                        <detallistaAllowanceChargeMonetaryAmountOrPercentageRateBase>(
                                        "allowanceChargemonetary", "ratebase")
                            }
                        };
                }
            }

            #endregion

            #region detallista:lineItem
            int cantidadLineItems = _iniAdd.OpcionalEntero("items", "cantidadItems");
            _detallista.lineItem = new detallistaLineItem[cantidadLineItems];
            for (int i = 0; i < cantidadLineItems; i++)
            {
                string itemName = String.Format("item{0}", i + 1);
                _detallista.lineItem[i] = new detallistaLineItem
                {
                    // Atributos
                    type = "SimpleInvoiceLineItemType",
                    number = (i + 1).ToString(),
                    // Nodos
                    tradeItemIdentification = new detallistaLineItemTradeItemIdentification
                    {
                        gtin = _iniAdd.Requerido(itemName, "tradeItemgtin")
                    },
                    alternateTradeItemIdentification = new[]
							{
								// TODO: Esto es opcional y pueden ser varias descripciones
								new detallistaLineItemAlternateTradeItemIdentification
									{
										type =
											_iniAdd.OpcionalEnum
												<detallistaLineItemAlternateTradeItemIdentificationType>(
												itemName, "alternateTradeItemIdentificationType"),
										Text = new[]
											{
												_iniAdd.Opcional(itemName, "alternateTradeItemIdentification")
											}
									}
							},
                    tradeItemDescriptionInformation =
                        new detallistaLineItemTradeItemDescriptionInformation
                        {
                            longText =
                                _iniAdd.Requerido(itemName, "tradeItemDescriptionInformation"),
                            // Idoma fijo en español
                            language = detallistaLineItemTradeItemDescriptionInformationLanguage.ES,
                            languageSpecified = true
                        },
                    invoicedQuantity = new detallistaLineItemInvoicedQuantity
                    {
                        Text = new[]
									{
										_iniAdd.Requerido(itemName, "quantity")
									},
                        // TODO: Obtener la unidad correcta de la base de datos
                        unitOfMeasure = "PZA"
                    },
                    aditionalQuantity = null,
                    // Opcional
                    grossPrice = new detallistaLineItemGrossPrice
                    {
                        Amount = _iniAdd.RequeridoDecimal(itemName, "grossPrice")
                    },
                    netPrice = new detallistaLineItemNetPrice
                    {
                        Amount = _iniAdd.RequeridoDecimal(itemName, "netPrice")
                    },
                    // Additionalinformation
                    // Customs
                    // LogisticsUnits
                    // palletInformation
                    // extendedAttributes
                    // tradeItemTaxInformation
                    totalLineAmount = new detallistaLineItemTotalLineAmount
                    {
                        grossAmount = new detallistaLineItemTotalLineAmountGrossAmount
                        {
                            Amount = _iniAdd.RequeridoDecimal(itemName, "grossAmount")
                        },
                        netAmount = new detallistaLineItemTotalLineAmountNetAmount
                        {
                            Amount = _iniAdd.RequeridoDecimal(itemName, "netAmount")
                        }
                    }
                };
            }

            #endregion

            #region detallista:totalAmount
            _detallista.totalAmount = new detallistaTotalAmount
            {
                Amount = _iniAdd.RequeridoDecimal("totalAmount", "Amount")
            };
            #endregion

            #region detallista:totalAllowanceCharge
            if (_iniAdd.Existe("totalAllowanceCharge", "type"))
            {
                _detallista.TotalAllowanceCharge = new[]
					{
						// TODO: Es posible tener multiples cargos/créditos
						new detallistaTotalAllowanceCharge
							{
								allowanceOrChargeType =
									_iniAdd.RequeridoEnum<detallistaTotalAllowanceChargeAllowanceOrChargeType>(
									"totalAllowanceCharge", "type"),
								Amount = _iniAdd.RequeridoDecimal("totalAllowanceCharge", "Amount"),
                                AmountSpecified = _iniAdd.Existe("totalAllowanceCharge", "Amount"),
								specialServicesType =
									_iniAdd.OpcionalEnum<detallistaTotalAllowanceChargeSpecialServicesType>(
										"totalAllowanceCharge", "specialServicesType"),
								specialServicesTypeSpecified = _iniAdd.Existe("totalAllowanceCharge", "specialServicesType")
							}
					};
            }

            #endregion
        }

        private void GeneraAddenda()
        {
            #region Atributos
            _addenda = new Addenda
            {
                requestForPayment = new AddendaRequestForPayment
                {
                    // Attributos
                    type = _iniAdd.Opcional("requestForPayment", "type"),
                    contentVersion = _iniAdd.Opcional("requestForPayment", "contentVersion"),
                    documentStructureVersion =
                        _iniAdd.Requerido("requestForPayment", "documentStructureVersion"),
                    documentStatus =
                        _iniAdd.RequeridoEnum<AddendaRequestForPaymentDocumentStatus>(
                            "requestForPayment", "documentStatus"),
                    DeliveryDate = _iniAdd.OpcionalFecha("requestForPayment", "DeliveryDate"),
                    DeliveryDateSpecified = _iniAdd.Existe("requestForPayment", "DeliveryDate")
                }
            };
            #endregion

            #region RequestForPaymentIdentification
            _addenda.requestForPayment.requestForPaymentIdentification = new AddendaRequestForPaymentRequestForPaymentIdentification
            {
                entityType = _iniAdd.RequeridoEnum<AddendaRequestForPaymentRequestForPaymentIdentificationEntityType>(
                    "requestForPaymentIdentification", "entityType"),
                uniqueCreatorIdentification = _iniAdd.Requerido("requestForPaymentIdentification",
                    "uniqueCreatorIdentification")
            };
            #endregion

            #region specialInstruction
            if (_iniAdd.Existe("specialInstruction", "code"))
            {
                _addenda.requestForPayment.specialInstruction = new AddendaRequestForPaymentSpecialInstruction[]
					{
						new AddendaRequestForPaymentSpecialInstruction
							{
								code = _iniAdd.RequeridoEnum<AddendaRequestForPaymentSpecialInstructionCode>(
									"specialInstruction", "code"),
								text = new[]
									{
										_iniAdd.Requerido("specialInstruction", "text")
									}
							}
					};
            }
            #endregion

            #region orderIdentification

            _addenda.requestForPayment.orderIdentification =
                new AddendaRequestForPaymentOrderIdentification
                {
                    referenceIdentification = new[]
							{
								new AddendaRequestForPaymentOrderIdentificationReferenceIdentification
									{
										type =
											_iniAdd.RequeridoEnum
												<
													AddendaRequestForPaymentOrderIdentificationReferenceIdentificationType
													>(
												"orderIdentification", "referenceIdentificationtype"),
										Value =
											_iniAdd.Requerido("orderIdentification", "referenceIdentification")
									}
							},
                };
            if (_iniAdd.Existe("orderIdentification", "referenceDate"))
            {
                _addenda.requestForPayment.orderIdentification.ReferenceDate = _iniAdd.OpcionalFecha("orderIdentification", "referenceDate");
                _addenda.requestForPayment.orderIdentification.ReferenceDateSpecified = _iniAdd.Existe("orderIdentification", "referenceDate");
            }
            #endregion

            #region AdditionalInformation
            _addenda.requestForPayment.AdditionalInformation = new[]
                {
                    new AddendaRequestForPaymentReferenceIdentification
                    {
                        Value =
								_iniAdd.Requerido("AdditionalInformation", "referenceIdentification"),
							type = _iniAdd.RequeridoEnum<AddendaRequestForPaymentReferenceIdentificationType>(
								"AdditionalInformation", "referenceIdentificationtype")
                    }
                };
            #endregion

            #region DeliveryNote
            if (_iniAdd.Existe("DeliveryNote", "referenceIdentification"))
            {
                _addenda.requestForPayment.DeliveryNote = new AddendaRequestForPaymentDeliveryNote
                {
                    referenceIdentification = new[]
							{
								_iniAdd.Requerido("DeliveryNote", "referenceIdentification")
							},
                    ReferenceDate =
                        _iniAdd.OpcionalFecha("DeliveryNote", "ReferenceDate"),
                    ReferenceDateSpecified = _iniAdd.Existe("DeliveryNote", "ReferenceDate")
                };
            }
            #endregion

            #region buyer
            if (_iniAdd.Existe("buyer", "gln"))
            {
                _addenda.requestForPayment.buyer = new AddendaRequestForPaymentBuyer
                {
                    gln = _iniAdd.Opcional("buyer", "gln")
                };
                if (_iniAdd.Existe("buyer", "personOrDepartmentName"))
                {
                    _addenda.requestForPayment.buyer.contactInformation = new AddendaRequestForPaymentBuyerContactInformation
                    {
                        personOrDepartmentName = new AddendaRequestForPaymentBuyerContactInformationPersonOrDepartmentName
                        {
                            text = _iniAdd.Requerido("buyer", "personOrDepartmentName")
                        }
                    };
                }
            }
            #endregion

            #region seller
            _addenda.requestForPayment.seller = new AddendaRequestForPaymentSeller
            {
                gln = _iniAdd.Requerido("seller", "gln"),
                alternatePartyIdentification = new AddendaRequestForPaymentSellerAlternatePartyIdentification
                {
                    type = _iniAdd.RequeridoEnum<AddendaRequestForPaymentSellerAlternatePartyIdentificationType>(
                        "seller", "alternatePartyIdentificationtype"),
                    Value = _iniAdd.Requerido("seller", "alternatePartyIdentification")

                },
                IndentificaTipoProv = _iniAdd.Opcional("seller", "IdentificaTipoProv")  // Modificado de Requerido a Opcional
            };
            #endregion

            #region shipTo
            _addenda.requestForPayment.shipTo = new AddendaRequestForPaymentShipTo
            {
                gln = _iniAdd.Opcional("shipTo", "gln"),
                nameAndAddress = new AddendaRequestForPaymentShipToNameAndAddress
                {
                    name = new[]
                    {
                        _iniAdd.Opcional("shipTo", "name")
                    },
                    streetAddressOne = new[]
                    {
                        _iniAdd.Opcional("shipTo", "streetAddressOne")
                    },
                    city = new[]
                    {
                        _iniAdd.Opcional("shipTo", "city")
                    },
                    postalCode = new[]
                    {
                        _iniAdd.Opcional("shipTo", "postalCode")
                    }
                }
            };
            if (_iniAdd.Existe("shipTo", "bodegaEnt"))
            {
                _addenda.requestForPayment.shipTo.nameAndAddress.bodegaEnt = new[]
                {
                    _iniAdd.Opcional("shipTo", "bodegaEnt")
                };
            }
            #endregion

            #region Currency
            // Estos valores son fijos
            _addenda.requestForPayment.currency = new[]
				{
					new AddendaRequestForPaymentCurrency
						{
							currencyISOCode = AddendaRequestForPaymentCurrencyCurrencyISOCode.MXN,
							currencyFunction = new[]
								{
									AddendaRequestForPaymentCurrencyCurrencyFunction.BILLING_CURRENCY
								},
							rateOfChange = 1.00M,
							rateOfChangeSpecified = true
						}
				};
            #endregion

            #region TotalLotes
            if (_iniAdd.Existe("TotalLotes", "cantidad"))
            {
                _addenda.requestForPayment.TotalLotes = new AddendaRequestForPaymentTotalLotes
                {
                    cantidad = _iniAdd.OpcionalDecimal("TotalLotes", "cantidad"),
                };
            }
            #endregion

            #region paymentTerms
            if (_iniAdd.Existe("paymentTerms", "value"))
            {
                _addenda.requestForPayment.paymentTerms = new AddendaRequestForPaymentPaymentTerms
                {
                    paymentTermsEvent = _iniAdd.RequeridoEnum<AddendaRequestForPaymentPaymentTermsPaymentTermsEvent>(
                            "paymentTerms", "paymentTermsEvent"),
                    paymentTermsEventSpecified = _iniAdd.Existe("paymentTerms", "paymentTermsEvent"),
                    PaymentTermsRelationTime = _iniAdd.RequeridoEnum<AddendaRequestForPaymentPaymentTermsPaymentTermsRelationTime>(
                            "paymentTerms", "PaymentTermsRelationTime"),
                    PaymentTermsRelationTimeSpecified = _iniAdd.Existe("paymentTerms", "PaymentTermsRelationTime"),
                    netPayment = new AddendaRequestForPaymentPaymentTermsNetPayment
                    {
                        netPaymentTermsType = _iniAdd.RequeridoEnum<AddendaRequestForPaymentPaymentTermsNetPaymentNetPaymentTermsType>(
                            "paymentTerms", "netPaymentTermsType"),
                        paymentTimePeriod = new AddendaRequestForPaymentPaymentTermsNetPaymentPaymentTimePeriod
                        {
                            timePeriodDue = new AddendaRequestForPaymentPaymentTermsNetPaymentPaymentTimePeriodTimePeriodDue
                            {
                                timePeriod = _iniAdd.RequeridoEnum<AddendaRequestForPaymentPaymentTermsNetPaymentPaymentTimePeriodTimePeriodDueTimePeriod>(
                                        "paymentTerms", "timePeriod"),
                                value = _iniAdd.Requerido("paymentTerms", "value")
                            }
                        }
                    }
                };
                if (_iniAdd.Existe("paymentTerms", "percentage"))
                {
                    _addenda.requestForPayment.paymentTerms.discountPayment = new AddendaRequestForPaymentPaymentTermsDiscountPayment
                    {
                        discountType = _iniAdd.RequeridoEnum<AddendaRequestForPaymentPaymentTermsDiscountPaymentDiscountType>(
                            "paymentTerms", "discountType"),
                        percentage = _iniAdd.Requerido("paymentTerms", "percentage")
                    };
                }
            }
            #endregion

            #region allowanceCharge
            if (_iniAdd.Existe("allowanceCharge", "type"))
            {
                _addenda.requestForPayment.allowanceCharge = new[]
                    {
                        new AddendaRequestForPaymentAllowanceCharge
                        {
                            allowanceChargeType =
									_iniAdd.RequeridoEnum<AddendaRequestForPaymentAllowanceChargeAllowanceChargeType>(
										"allowanceCharge", "type"),
                            settlementType =
									_iniAdd.RequeridoEnum<AddendaRequestForPaymentAllowanceChargeSettlementType>(
										"allowanceCharge", "settlementType"),
							sequenceNumber = null,
                            specialServicesType = _iniAdd.OpcionalEnum<AddendaRequestForPaymentAllowanceChargeSpecialServicesType>(
                                    "allowanceCharge", "specialServicesType"),
                            specialServicesTypeSpecified =
									_iniAdd.Existe("allowanceCharge", "specialServicesType"),
                        }
                    };
                // Este nodo es opcional
                if (_iniAdd.Existe("allowanceChargemonetary", "percentage"))
                {
                    _addenda.requestForPayment.allowanceCharge[0].monetaryAmountOrPercentage =
                        new AddendaRequestForPaymentAllowanceChargeMonetaryAmountOrPercentage
                        {
                            rate = new AddendaRequestForPaymentAllowanceChargeMonetaryAmountOrPercentageRate
                            {
                                percentage =
                                    _iniAdd.RequeridoDecimal("allowanceChargemonetary", "percentage"),
                                @base =
                                    _iniAdd.RequeridoEnum
                                        <AddendaRequestForPaymentAllowanceChargeMonetaryAmountOrPercentageRateBase>(
                                        "allowanceChargemonetary", "ratebase")
                            }
                        };
                }
            }
            #endregion

            #region lineItem
            int cantidadLineItems = _iniAdd.OpcionalEntero("items", "cantidadItems");
            _addenda.requestForPayment.lineItem = new AddendaRequestForPaymentLineItem[cantidadLineItems];
            for (int i = 0; i < cantidadLineItems; i++)
            {
                string itemName = String.Format("item{0}", i + 1);
                _addenda.requestForPayment.lineItem[i] = new AddendaRequestForPaymentLineItem
                {
                    // Atributos
                    type = "SimpleInvoiceLineItemType",
                    number = (i + 1).ToString(),
                    // Nodos
                    tradeItemIdentification =
                        new AddendaRequestForPaymentLineItemTradeItemIdentification
                        {
                            gtin = _iniAdd.Requerido(itemName, "tradeItemgtin")
                        },
                    alternateTradeItemIdentification = new[]
							{
								// TODO: Esto es opcional y pueden ser varias descripciones 
								new AddendaRequestForPaymentLineItemAlternateTradeItemIdentification
									{
										type =
											_iniAdd.RequeridoEnum
												<
													AddendaRequestForPaymentLineItemAlternateTradeItemIdentificationType
													>(
												itemName, "alternateTradeItemIdentificationType"),
										Text = new[]
											{
												_iniAdd.Requerido(itemName, "alternateTradeItemIdentification")
											}
									}
							},
                    tradeItemDescriptionInformation =
                        new AddendaRequestForPaymentLineItemTradeItemDescriptionInformation
                        {
                            longText =
                                _iniAdd.Requerido(itemName, "tradeItemDescriptionInformation"),
                            // Idoma fijo en español
                            language =
                                AddendaRequestForPaymentLineItemTradeItemDescriptionInformationLanguage
                                    .ES,
                            languageSpecified = true
                        },
                    invoicedQuantity = new AddendaRequestForPaymentLineItemInvoicedQuantity
                    {
                        Text = new[]
									{
										_iniAdd.Requerido(itemName, "quantity")
									},
                        // TODO: Obtener la unidad correcta de la base de datos
                        unitOfMeasure = "PZA"
                    },
                    grossPrice = new AddendaRequestForPaymentLineItemGrossPrice
                    {
                        Amount = _iniAdd.RequeridoDecimal(itemName, "grossPrice")
                    },
                    netPrice = new AddendaRequestForPaymentLineItemNetPrice
                    {
                        Amount = _iniAdd.RequeridoDecimal(itemName, "netPrice")
                    },
                    
                    totalLineAmount = new AddendaRequestForPaymentLineItemTotalLineAmount
                    {
                        grossAmount =
                            new AddendaRequestForPaymentLineItemTotalLineAmountGrossAmount
                            {
                                Amount = _iniAdd.RequeridoDecimal(itemName, "grossAmount")
                            },
                        netAmount = new AddendaRequestForPaymentLineItemTotalLineAmountNetAmount
                        {
                            Amount = _iniAdd.RequeridoDecimal(itemName, "netAmount")
                        }
                    }
                };
                if (_addenda.requestForPayment.buyer.gln == "BOD9809059Z6") // Si es BODESA, requiere informacion de empaquetado
                {
                    _addenda.requestForPayment.lineItem[i].palletInformation = new AddendaRequestForPaymentLineItemPalletInformation
                    {
                        description = new AddendaRequestForPaymentLineItemPalletInformationDescription
                        {
                            type = AddendaRequestForPaymentLineItemPalletInformationDescriptionType.BOX,
                            Text = new[]
											{
												"EMPAQUETADO"
											}
                        },
                        palletQuantity = _iniAdd.Opcional(itemName, "palletQuantity"),  // Modificado de Requerido a Opcional
                        transport =
                            new AddendaRequestForPaymentLineItemPalletInformationTransport
                            {
                                methodOfPayment =
                                    AddendaRequestForPaymentLineItemPalletInformationTransportMethodOfPayment
                                        .PAID_BY_BUYER, // TODO: Obtener del sistema
                                prepactCant = _iniAdd.Opcional(itemName, "prepactCant"),    // Modificado de Requerido a Opcional
                            }
                    };
                }
                if (_iniAdd.Existe(itemName, "specialServicesType"))
                {
                    _addenda.requestForPayment.lineItem[i].allowanceCharge = new[]
                                    {
                                        new AddendaRequestForPaymentLineItemAllowanceCharge
                                        {
                                            specialServicesType = _iniAdd.RequeridoEnum<AddendaRequestForPaymentLineItemAllowanceChargeSpecialServicesType>(
												itemName, "specialServicesType"),
                                                specialServicesTypeSpecified = _iniAdd.Existe(itemName, "specialServicesType"),
                                            monetaryAmountOrPercentage = new AddendaRequestForPaymentLineItemAllowanceChargeMonetaryAmountOrPercentage
											{
												percentagePerUnit = _iniAdd.Requerido(itemName, "percentagePerUnit"),
                                                ratePerUnit = new AddendaRequestForPaymentLineItemAllowanceChargeMonetaryAmountOrPercentageRatePerUnit 
                                                {
                                                    amountPerUnit = _iniAdd.Requerido(itemName, "amountPerUnit")
                                                }
											},
											allowanceChargeType = _iniAdd.RequeridoEnum<AddendaRequestForPaymentLineItemAllowanceChargeAllowanceChargeType>
												(itemName, "allowanceChargeType") 
                                        }
                                    };
                };
                if (_iniAdd.Existe(itemName, "taxTypeDescription"))
                {
                    _addenda.requestForPayment.lineItem[i].tradeItemTaxInformation = new[]
                            {
                                new AddendaRequestForPaymentLineItemTradeItemTaxInformation
                                {
                                    taxTypeDescription = _iniAdd.RequeridoEnum<AddendaRequestForPaymentLineItemTradeItemTaxInformationTaxTypeDescription>
                                        (itemName, "taxTypeDescription"),
                                    tradeItemTaxAmount = new AddendaRequestForPaymentLineItemTradeItemTaxInformationTradeItemTaxAmount
                                    {
                                        taxAmount = _iniAdd.RequeridoDecimal(itemName, "taxAmount"),
                                        taxPercentage = _iniAdd.RequeridoDecimal(itemName, "taxPercentage")
                                    },
                                    taxCategory = _iniAdd.RequeridoEnum<AddendaRequestForPaymentLineItemTradeItemTaxInformationTaxCategory>
                                            (itemName, "taxCategory"),
                                    taxCategorySpecified = _iniAdd.Existe(itemName, "taxCategory")
                                }
                            };
                };
                if (_iniAdd.Existe(itemName, "codigo"))
                {
                    _addenda.requestForPayment.lineItem[i].codigoTallaInternoCop = new AddendaRequestForPaymentLineItemCodigoTallaInternoCop
                    {
                        codigo = _iniAdd.Opcional(itemName, "codigo"),  // Modificado de Requerido a Opcional
                        talla = _iniAdd.Opcional(itemName, "talla"),    // Modificado de Requerido a Opcional
                    };
                }
                if (_iniAdd.Existe(itemName, "aditionalQuantity"))
                {
                    _addenda.requestForPayment.lineItem[i].aditionalQuantity = new[]
                    {
                        new AddendaRequestForPaymentLineItemAditionalQuantity
                        {
                            Text = new[]
                                {
                                    _iniAdd.Opcional(itemName, "aditionalQuantity")
                                },
                                QuantityType = AddendaRequestForPaymentLineItemAditionalQuantityQuantityType.NUM_CONSUMER_UNITS
                        }
                    };
                };
                if (_iniAdd.Existe(itemName, "referenceIdentification"))
                {
                    _addenda.requestForPayment.lineItem[i].AdditionalInformation = new AddendaRequestForPaymentLineItemAdditionalInformation
                    {
                        referenceIdentification = new AddendaRequestForPaymentLineItemAdditionalInformationReferenceIdentification
                        {
                            type = AddendaRequestForPaymentLineItemAdditionalInformationReferenceIdentificationType.ON,
                            Value = _iniAdd.Opcional(itemName, "referenceIdentification")
                        }
                    };
                };
            }

            #endregion

            #region totalAmount
            _addenda.requestForPayment.totalAmount = new AddendaRequestForPaymentTotalAmount
            {
                Amount = _iniAdd.OpcionalDecimal("totalAmount", "Amount"),
            };
            #endregion

            #region TotalAllowanceCharge
            _addenda.requestForPayment.TotalAllowanceCharge = new[]
				{
					// TODO: Es posible tener multiples cargos/créditos
					new AddendaRequestForPaymentTotalAllowanceCharge
						{
							allowanceOrChargeType =
								_iniAdd.RequeridoEnum
									<AddendaRequestForPaymentTotalAllowanceChargeAllowanceOrChargeType>(
									"totalAllowanceCharge", "type"),
							Amount = _iniAdd.RequeridoDecimal("totalAllowanceCharge", "Amount"),
                            AmountSpecified = _iniAdd.Existe("totalAllowanceCharge", "Amount"),
							specialServicesType =
								_iniAdd.OpcionalEnum
									<AddendaRequestForPaymentTotalAllowanceChargeSpecialServicesType>(
									"totalAllowanceCharge", "specialServicesType"),
							specialServicesTypeSpecified =
								_iniAdd.Existe("totalAllowanceCharge", "specialServicesType")
						}
				};
            #endregion

            #region baseAmount
            _addenda.requestForPayment.baseAmount = new AddendaRequestForPaymentBaseAmount
            {
                Amount = _iniAdd.RequeridoDecimal("baseAmount", "Amount")
            };
            #endregion

            #region tax
            _addenda.requestForPayment.tax = new[]
				{
					new AddendaRequestForPaymentTax
						{
							taxAmount = _iniAdd.RequeridoDecimal("tax", "taxAmount"),
                            taxAmountSpecified = _iniAdd.Existe("tax", "taxAmount"),
							taxPercentage = _iniAdd.RequeridoDecimal("tax", "taxPercentage"),
                            taxPercentageSpecified = _iniAdd.Existe("tax", "taxPercentage"),
							type = _iniAdd.RequeridoEnum<AddendaRequestForPaymentTaxType>(
								"tax", "type"),
                            typeSpecified = _iniAdd.Existe("tax", "type"),
                            taxCategory = _iniAdd.RequeridoEnum<AddendaRequestForPaymentTaxTaxCategory>
                                            ("tax", "taxCategory"),
                            taxCategorySpecified = _iniAdd.Existe("tax", "taxCategory"),
						}
				};
            #endregion

            #region payableAmount
            _addenda.requestForPayment.payableAmount = new AddendaRequestForPaymentPayableAmount
            {
                Amount = _iniAdd.RequeridoDecimal("payableAmount", "Amount")
            };
            #endregion

        }

        private void GeneraAlSuper()
        {
            #region Atributos
            _alSuper = new Alsuper
            {
                // Atributos
                version = "1.0",    // Valor fijo
                remision = _iniAdd.Opcional("Alsuper", "remision"),
                ordenDeCompra = _iniAdd.Opcional("Alsuper", "ordenDeCompra"),
                sucursal = _iniAdd.Requerido("Alsuper", "sucursal"),
                cita = _iniAdd.Opcional("Alsuper", "cita"),
                fechaCita = _iniAdd.OpcionalFecha("Alsuper", "fechaCita"),
                fechaCitaSpecified = _iniAdd.Existe("Alsuper", "fechaCita"),
                tipoMoneda = _iniAdd.OpcionalEnum<AlsuperTipoMoneda>("Alsuper", "tipoMoneda"),
                tipoMonedaSpecified = _iniAdd.Existe("Alsuper", "tipoMoneda"),
                tipoDeCambio = _iniAdd.OpcionalEntero("Alsuper", "tipoDeCambio"),
                tipoDeCambioSpecified = _iniAdd.Existe("Alsuper", "tipoDeCambio"),
                tipoBulto = _iniAdd.OpcionalEnum<AlsuperTipoBulto>("Alsuper", "tipoBulto"),
                tipoBultoSpecified = _iniAdd.Existe("Alsuper", "tipoBulto"),
                valorFlete = _iniAdd.OpcionalDecimal("Alsuper", "valorFlete"),
                valorFleteSpecified = _iniAdd.Existe("Alsuper", "valorFlete"),
                email = _iniAdd.Requerido("Alsuper", "email")
            };
            #endregion

            #region Conceptos
            int cantidadConceptos = _iniAdd.OpcionalEntero("Conceptos", "cantidadConceptos");
            _alSuper.Conceptos = new AlsuperConcepto[cantidadConceptos];
            for (int i = 0; i < cantidadConceptos; i++)
            {
                string conceptoName = String.Format("Concepto{0}", i + 1);
                _alSuper.Conceptos[i] = new AlsuperConcepto
                {
                    noPartida = _iniAdd.Requerido(conceptoName, "noPartida"),
                    codigoDeBarras = _iniAdd.Requerido(conceptoName, "codigoDeBarras"),
                    factorEmpaque = _iniAdd.OpcionalDecimal(conceptoName, "factorEmpaque"),
                    factorEmpaqueSpecified = _iniAdd.Existe(conceptoName, "factorEmpaque"),
                    empaqueIngreso = _iniAdd.OpcionalDecimal(conceptoName, "empaqueIngreso"),
                    empaqueIngresoSpecified = _iniAdd.Existe(conceptoName, "empaqueIngreso"),
                    costoPagar = _iniAdd.OpcionalDecimal(conceptoName, "costoPagar"),
                    costoPagarSpecified = _iniAdd.Existe(conceptoName, "costoPagar"),
                    valorIva = _iniAdd.OpcionalDecimal(conceptoName, "valorIva"),
                    valorIvaSpecified = _iniAdd.Existe(conceptoName, "valorIva"),
                    valorIeps = _iniAdd.OpcionalDecimal(conceptoName, "valorIeps"),
                    valorIepsSpecified = _iniAdd.Existe(conceptoName, "valorIeps")
                };
            }
            #endregion
        }

        private string GeneraEdifact(string cadenaOriginal, string sello)
        {
            return _iniAdd.AddendaEdifact(cadenaOriginal, sello);
        }

        private void GeneraComercioExterior()
        {
            #region Atributos

            _comercExt = new Schemas.ComercExt11.ComercioExterior
            {
                Version = "1.1",    // Valor fijo
                MotivoTraslado = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_MotivoTraslado>("ComercioExterior", "MotivoTraslado"),
                TipoOperacion = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_TipoOperacion>("ComercioExterior", "TipoOperacion"),
                ClaveDePedimento = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_ClavePedimento>("ComercioExterior", "ClaveDePedimento"),
                ClaveDePedimentoSpecified = _iniAdd.Existe("ComercioExterior", "ClaveDePedimento"),
                CertificadoOrigen = _iniAdd.OpcionalEntero("ComercioExterior", "CertificadoOrigen"),
                CertificadoOrigenSpecified = _iniAdd.Existe("ComercioExterior", "CertificadoOrigen"),
                NumCertificadoOrigen = _iniAdd.Opcional("ComercioExterior", "NumCertificadoOrigen"),
                NumeroExportadorConfiable = _iniAdd.Opcional("ComercioExterior", "NumeroExportadorConfiable"),
                Incoterm = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_INCOTERM>("ComercioExterior", "Incoterm"),
                IncotermSpecified = _iniAdd.Existe("ComercioExterior", "Incoterm"),
                Subdivision = _iniAdd.OpcionalEntero("ComercioExterior", "Subdivision"),
                SubdivisionSpecified = _iniAdd.Existe("ComercioExterior", "Subdivision"),
                Observaciones = _iniAdd.Opcional("ComercioExterior", "Observaciones"),
                TipoCambioUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TipoCambioUSD"),
                TipoCambioUSDSpecified = _iniAdd.Existe("ComercioExterior", "TipoCambioUSD"),
                TotalUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TotalUSD"),
                TotalUSDSpecified = _iniAdd.Existe("ComercioExterior", "TotalUSD")
            };
            #endregion

            #region Emisor
            _comercExt.Emisor = new Schemas.ComercExt11.ComercioExteriorEmisor
            {
                Curp = _iniAdd.Opcional("Emisor", "Curp"),
                Domicilio = new Schemas.ComercExt11.ComercioExteriorEmisorDomicilio
                {
                    Calle = _iniAdd.Requerido("Emisor", "Calle"),
                    NumeroExterior = _iniAdd.Opcional("Emisor", "NumeroExterior"),
                    NumeroInterior = _iniAdd.Opcional("Emisor", "NumeroInterior"),
                    Colonia = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_Colonia>("Emisor", "Colonia"),
                    ColoniaSpecified = _iniAdd.Existe("Emisor", "Colonia"),
                    Localidad = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_Localidad>("Emisor", "Localidad"),
                    LocalidadSpecified = _iniAdd.Existe("Emisor", "Localidad"),
                    Referencia = _iniAdd.Opcional("Emisor", "Referencia"),
                    Municipio = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_Municipio>("Emisor", "Municipio"),
                    MunicipioSpecified = _iniAdd.Existe("Emisor", "Municipio"),
                    Estado = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_Estado>("Emisor", "Estado"),
                    Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_Pais>("Emisor", "Pais"),
                    CodigoPostal = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_CodigoPostal>("Emisor", "CodigoPostal")
                }
            };
            
            #endregion

            #region Propietario
            if (_iniAdd.Existe("Propietario", "NumRegIdTrib"))
            {
                _comercExt.Propietario = new Schemas.ComercExt11.ComercioExteriorPropietario[1];
                _comercExt.Propietario[0] = new Schemas.ComercExt11.ComercioExteriorPropietario
                {
                    NumRegIdTrib = _iniAdd.Requerido("Propietario", "NumRegIdTrib"),
                    ResidenciaFiscal = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_Pais>("Propietario", "Pais")
                };
            }
            #endregion

            #region Receptor
            _comercExt.Receptor = new Schemas.ComercExt11.ComercioExteriorReceptor
            {
                NumRegIdTrib = _iniAdd.Opcional("Receptor", "NumRegIdTrib")/*,
                Domicilio = new Schemas.ComercExt11.ComercioExteriorReceptorDomicilio
                {
                    Calle = _iniAdd.Requerido("Receptor", "Calle"),
                    NumeroExterior = _iniAdd.Opcional("Receptor", "NumeroExterior"),
                    NumeroInterior = _iniAdd.Opcional("Receptor", "NumeroInterior"),
                    Colonia = _iniAdd.Opcional("Receptor", "Colonia"),
                    Localidad = _iniAdd.Opcional("Receptor", "Localidad"),
                    Referencia = _iniAdd.Opcional("Receptor", "Referencia"),
                    Municipio = _iniAdd.Opcional("Receptor", "Municipio"),
                    Estado = _iniAdd.Requerido("Receptor", "Estado"),
                    Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_Pais>("Receptor", "Pais"),
                    CodigoPostal = _iniAdd.Requerido("Receptor", "CodigoPostal")
                }*/
            };
            #endregion

            #region Destinatario
            if (_iniAdd.Existe("Destinatario", "Nombre"))
            {
                _comercExt.Destinatario = new Schemas.ComercExt11.ComercioExteriorDestinatario[1];
                _comercExt.Destinatario[0] = new Schemas.ComercExt11.ComercioExteriorDestinatario
                {
                    NumRegIdTrib = _iniAdd.Opcional("Destinatario", "NumRegIdTrib"),
                    Nombre = _iniAdd.Opcional("Destinatario", "Nombre"),
                    Domicilio = GetDestinatarioDomicilio("Destinatario")
                };
            }
            #endregion

            #region Mercancias
            int cantidadMercancias = Convert.ToInt32(_iniAdd.Requerido("Mercancias", "CantidadMercancias"));
            _comercExt.Mercancias = new Schemas.ComercExt11.ComercioExteriorMercancia[cantidadMercancias];
            for (int i = 0; i < cantidadMercancias; i++)
            {
                string mercanciaSection = String.Format("Mercancia{0}", i + 1);
                _comercExt.Mercancias[i] = new Schemas.ComercExt11.ComercioExteriorMercancia
                {
                    NoIdentificacion = _iniAdd.Requerido(mercanciaSection, "NoIdentificacion"),
                    FraccionArancelaria = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_FraccionArancelaria>(mercanciaSection, "FraccionArancelaria"),
                    FraccionArancelariaSpecified = _iniAdd.Existe(mercanciaSection, "FraccionArancelaria"),
                    CantidadAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "CantidadAduana"),
                    CantidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "CantidadAduana"),
                    UnidadAduana = _iniAdd.OpcionalEnum<Schemas.ComercExt11.c_UnidadAduana>(mercanciaSection, "UnidadAduana"),
                    UnidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "UnidadAduana"),
                    ValorUnitarioAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "ValorUnitarioAduana"),
                    ValorUnitarioAduanaSpecified = _iniAdd.Existe(mercanciaSection, "ValorUnitarioAduana"),
                    ValorDolares = _iniAdd.RequeridoDecimal(mercanciaSection, "ValorDolares"),
                    DescripcionesEspecificas = GetDescripcionesMercancia(mercanciaSection)
                };
            }
            #endregion
        }

        private void GeneraComercioExteriorModif()
        {
            #region Atributos

            _comercExtModif = new Schemas.ComercExt11modif.ComercioExterior
            {
                Version = "1.1",    // Valor fijo
                MotivoTraslado = _iniAdd.OpcionalEnum<Schemas.ComercExt11modif.c_MotivoTraslado>("ComercioExterior", "MotivoTraslado"),
                TipoOperacion = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_TipoOperacion>("ComercioExterior", "TipoOperacion"),
                ClaveDePedimento = _iniAdd.OpcionalEnum<Schemas.ComercExt11modif.c_ClavePedimento>("ComercioExterior", "ClaveDePedimento"),
                ClaveDePedimentoSpecified = _iniAdd.Existe("ComercioExterior", "ClaveDePedimento"),
                CertificadoOrigen = _iniAdd.OpcionalEntero("ComercioExterior", "CertificadoOrigen"),
                CertificadoOrigenSpecified = _iniAdd.Existe("ComercioExterior", "CertificadoOrigen"),
                NumCertificadoOrigen = _iniAdd.Opcional("ComercioExterior", "NumCertificadoOrigen"),
                NumeroExportadorConfiable = _iniAdd.Opcional("ComercioExterior", "NumeroExportadorConfiable"),
                Incoterm = _iniAdd.OpcionalEnum<Schemas.ComercExt11modif.c_INCOTERM>("ComercioExterior", "Incoterm"),
                IncotermSpecified = _iniAdd.Existe("ComercioExterior", "Incoterm"),
                Subdivision = _iniAdd.OpcionalEntero("ComercioExterior", "Subdivision"),
                SubdivisionSpecified = _iniAdd.Existe("ComercioExterior", "Subdivision"),
                Observaciones = _iniAdd.Opcional("ComercioExterior", "Observaciones"),
                TipoCambioUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TipoCambioUSD"),
                TipoCambioUSDSpecified = _iniAdd.Existe("ComercioExterior", "TipoCambioUSD"),
                TotalUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TotalUSD"),
                TotalUSDSpecified = _iniAdd.Existe("ComercioExterior", "TotalUSD")
            };
            #endregion

            #region Emisor
            _comercExtModif.Emisor = new Schemas.ComercExt11modif.ComercioExteriorEmisor
            {
                Curp = _iniAdd.Opcional("Emisor", "Curp"),
                Domicilio = new Schemas.ComercExt11modif.ComercioExteriorEmisorDomicilio
                {
                    Calle = _iniAdd.Requerido("Emisor", "Calle"),
                    NumeroExterior = _iniAdd.Opcional("Emisor", "NumeroExterior"),
                    NumeroInterior = _iniAdd.Opcional("Emisor", "NumeroInterior"),
                    Colonia = _iniAdd.Opcional("Emisor", "Colonia"),
                    Localidad = _iniAdd.Opcional("Emisor", "Localidad"),
                    Referencia = _iniAdd.Opcional("Emisor", "Referencia"),
                    Municipio = _iniAdd.Opcional("Emisor", "Municipio"),
                    Estado = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_Estado>("Emisor", "Estado"),
                    Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_Pais>("Emisor", "Pais"),
                    CodigoPostal = _iniAdd.Requerido("Emisor", "CodigoPostal")
                }
            };

            #endregion

            #region Propietario
            if (_iniAdd.Existe("Propietario", "NumRegIdTrib"))
            {
                _comercExtModif.Propietario = new Schemas.ComercExt11modif.ComercioExteriorPropietario[1];
                _comercExtModif.Propietario[0] = new Schemas.ComercExt11modif.ComercioExteriorPropietario
                {
                    NumRegIdTrib = _iniAdd.Requerido("Propietario", "NumRegIdTrib"),
                    ResidenciaFiscal = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_Pais>("Propietario", "Pais")
                };
            }
            #endregion

            #region Receptor
            _comercExtModif.Receptor = new Schemas.ComercExt11modif.ComercioExteriorReceptor
            {
                NumRegIdTrib = _iniAdd.Opcional("Receptor", "NumRegIdTrib"),
                Domicilio = new Schemas.ComercExt11modif.ComercioExteriorReceptorDomicilio
                {
                    Calle = _iniAdd.Requerido("Receptor", "Calle"),
                    NumeroExterior = _iniAdd.Opcional("Receptor", "NumeroExterior"),
                    NumeroInterior = _iniAdd.Opcional("Receptor", "NumeroInterior"),
                    Colonia = _iniAdd.Opcional("Receptor", "Colonia"),
                    Localidad = _iniAdd.Opcional("Receptor", "Localidad"),
                    Referencia = _iniAdd.Opcional("Receptor", "Referencia"),
                    Municipio = _iniAdd.Opcional("Receptor", "Municipio"),
                    Estado = _iniAdd.Requerido("Receptor", "Estado"),
                    Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_Pais>("Receptor", "Pais"),
                    CodigoPostal = _iniAdd.Requerido("Receptor", "CodigoPostal")
                }
            };
            #endregion

            #region Destinatario
            if (_iniAdd.Existe("Destinatario", "Nombre"))
            {
                _comercExtModif.Destinatario = new Schemas.ComercExt11modif.ComercioExteriorDestinatario[1];
                _comercExtModif.Destinatario[0] = new Schemas.ComercExt11modif.ComercioExteriorDestinatario
                {
                    NumRegIdTrib = _iniAdd.Opcional("Destinatario", "NumRegIdTrib"),
                    Nombre = _iniAdd.Opcional("Destinatario", "Nombre"),
                    Domicilio = GetDestinatarioModifDomicilio("Destinatario")
                };
            }
            #endregion

            #region Mercancias
            int cantidadMercancias = Convert.ToInt32(_iniAdd.Requerido("Mercancias", "CantidadMercancias"));
            _comercExtModif.Mercancias = new Schemas.ComercExt11modif.ComercioExteriorMercancia[cantidadMercancias];
            for (int i = 0; i < cantidadMercancias; i++)
            {
                string mercanciaSection = String.Format("Mercancia{0}", i + 1);
                _comercExtModif.Mercancias[i] = new Schemas.ComercExt11modif.ComercioExteriorMercancia
                {
                    NoIdentificacion = _iniAdd.Requerido(mercanciaSection, "NoIdentificacion"),
                    FraccionArancelaria = _iniAdd.Opcional(mercanciaSection, "FraccionArancelaria"),
                    CantidadAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "CantidadAduana"),
                    CantidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "CantidadAduana"),
                    UnidadAduana = _iniAdd.OpcionalEnum<Schemas.ComercExt11modif.c_UnidadAduana>(mercanciaSection, "UnidadAduana"),
                    UnidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "UnidadAduana"),
                    ValorUnitarioAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "ValorUnitarioAduana"),
                    ValorUnitarioAduanaSpecified = _iniAdd.Existe(mercanciaSection, "ValorUnitarioAduana"),
                    ValorDolares = _iniAdd.RequeridoDecimal(mercanciaSection, "ValorDolares"),
                    DescripcionesEspecificas = GetDescripcionesMercanciaModif(mercanciaSection)
                };
            }
            #endregion
        }

        private void GeneraComercioExterior10()
        {
            #region Atributos
            _comercExt10 = new Schemas.ComercExt10.ComercioExterior
            {
                Version = "1.0",
                TipoOperacion = _iniAdd.RequeridoEnum<Schemas.ComercExt10.c_TipoOperacion>("ComercioExterior", "TipoOperacion"),
                ClaveDePedimento = _iniAdd.OpcionalEnum<Schemas.ComercExt10.c_ClavePedimento>("ComercioExterior", "ClaveDePedimento"),
                ClaveDePedimentoSpecified = _iniAdd.Existe("ComercioExterior", "ClaveDePedimento"),
                CertificadoOrigen = _iniAdd.OpcionalEntero("ComercioExterior", "CertificadoOrigen"),
                CertificadoOrigenSpecified = _iniAdd.Existe("ComercioExterior", "CertificadoOrigen"),
                NumCertificadoOrigen = _iniAdd.Opcional("ComercioExterior", "NumCertificadoOrigen"),
                NumeroExportadorConfiable = _iniAdd.Opcional("ComercioExterior", "NumeroExportadorConfiable"),
                Incoterm = _iniAdd.OpcionalEnum<Schemas.ComercExt10.c_INCOTERM>("ComercioExterior", "Incoterm"),
                IncotermSpecified = _iniAdd.Existe("ComercioExterior", "Incoterm"),
                Subdivision = _iniAdd.OpcionalEntero("ComercioExterior", "Subdivision"),
                SubdivisionSpecified = _iniAdd.Existe("ComercioExterior", "Subdivision"),
                Observaciones = _iniAdd.Opcional("ComercioExterior", "Observaciones"),
                TipoCambioUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TipoCambioUSD"),
                TipoCambioUSDSpecified = _iniAdd.Existe("ComercioExterior", "TipoCambioUSD"),
                TotalUSD = _iniAdd.OpcionalDecimal("ComercioExterior", "TotalUSD"),
                TotalUSDSpecified = _iniAdd.Existe("ComercioExterior", "TotalUSD")
            };
            #endregion

            #region Emisor
            if (_iniAdd.Existe("Emisor", "Curp"))
            {
                _comercExt10.Emisor = new Schemas.ComercExt10.ComercioExteriorEmisor
                {
                    Curp = _iniAdd.Opcional("Emisor", "Curp")
                };
            }
            #endregion

            #region Receptor
            _comercExt10.Receptor = new Schemas.ComercExt10.ComercioExteriorReceptor
            {
                Curp = _iniAdd.Opcional("Receptor", "Curp"),
                NumRegIdTrib = _iniAdd.Requerido("Receptor", "NumRegIdTrib")
            };
            #endregion

            #region Destinatario
            if (_iniAdd.Existe("Destinatario", "Nombre"))
            {
                _comercExt10.Destinatario = new Schemas.ComercExt10.ComercioExteriorDestinatario
                {
                    NumRegIdTrib = _iniAdd.Opcional("Destinatario", "NumRegIdTrib"),
                    Rfc = _iniAdd.Opcional("Destinatario", "Rfc"),
                    Curp = _iniAdd.Opcional("Destinatario", "Curp"),
                    Nombre = _iniAdd.Opcional("Destinatario", "Nombre"),
                    Domicilio = new Schemas.ComercExt10.ComercioExteriorDestinatarioDomicilio
                    {
                        Calle = _iniAdd.Requerido("Destinatario", "Calle"),
                        NumeroExterior = _iniAdd.Opcional("Destinatario", "NumeroExterior"),
                        NumeroInterior = _iniAdd.Opcional("Destinatario", "NumeroInterior"),
                        Colonia = _iniAdd.Opcional("Destinatario", "Colonia"),
                        Localidad = _iniAdd.Opcional("Destinatario", "Localidad"),
                        Referencia = _iniAdd.Opcional("Destinatario", "Referencia"),
                        Municipio = _iniAdd.Opcional("Destinatario", "Municipio"),
                        Estado = _iniAdd.Requerido("Destinatario", "Estado"),
                        Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt10.c_Pais>("Destinatario", "Pais"),
                        CodigoPostal = _iniAdd.Requerido("Destinatario", "CodigoPostal")
                    }
                };
            }
            #endregion

            #region Mercancias
            int cantidadMercancias = Convert.ToInt32(_iniAdd.Requerido("Mercancias", "CantidadMercancias"));
            _comercExt10.Mercancias = new Schemas.ComercExt10.ComercioExteriorMercancia[cantidadMercancias];
            for (int i = 0; i < cantidadMercancias; i++)
            {
                string mercanciaSection = String.Format("Mercancia{0}", i + 1);
                _comercExt10.Mercancias[i] = new Schemas.ComercExt10.ComercioExteriorMercancia
                {
                    NoIdentificacion = _iniAdd.Requerido(mercanciaSection, "NoIdentificacion"),
                    FraccionArancelaria = _iniAdd.OpcionalEnum<Schemas.ComercExt10.c_FraccionArancelaria>(mercanciaSection, "FraccionArancelaria"),
                    FraccionArancelariaSpecified = _iniAdd.Existe(mercanciaSection, "FraccionArancelaria"),
                    CantidadAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "CantidadAduana"),
                    CantidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "CantidadAduana"),
                    UnidadAduana = _iniAdd.OpcionalEnum<Schemas.ComercExt10.c_UnidadMedidaAduana>(mercanciaSection, "UnidadAduana"),
                    UnidadAduanaSpecified = _iniAdd.Existe(mercanciaSection, "UnidadAduana"),
                    ValorUnitarioAduana = _iniAdd.OpcionalDecimal(mercanciaSection, "ValorUnitarioAduana"),
                    ValorUnitarioAduanaSpecified = _iniAdd.Existe(mercanciaSection, "ValorUnitarioAduana"),
                    ValorDolares = _iniAdd.RequeridoDecimal(mercanciaSection, "ValorDolares"),
                    DescripcionesEspecificas = null
                };
            }
            #endregion
        }

        private void GeneraPagos()
        {
            #region Atributos
            _pagos10 = new Pagos
            {
                Version = "1.0",
            };
            #endregion

            #region Pagos
            int cantidadPagos = _iniAdd.OpcionalEntero("Pagos", "cantidadPagos");
            int cantidadDoctos;
            string pagoNombre;
            _pagos10.Pago = new PagosPago[cantidadPagos];
            for (int i = 0; i < cantidadPagos; i++)
            {
                pagoNombre = string.Format("Pago{0}", i + 1);
                cantidadDoctos = _iniAdd.OpcionalEntero(pagoNombre, "CantidadDoctoRelacionado");
                _pagos10.Pago[i] = new PagosPago
                {
                    FechaPago = _iniAdd.RequeridoFecha(pagoNombre, "FechaPago"),
                    FormaDePagoP = _iniAdd.RequeridoEnum<c_FormaPago>(pagoNombre, "FormaDePagoP"),
                    MonedaP = _iniAdd.RequeridoEnum<c_Moneda>(pagoNombre, "MonedaP"),
                    TipoCambioP = _iniAdd.OpcionalDecimal(pagoNombre, "TipoCambioP"),
                    TipoCambioPSpecified = _iniAdd.Existe(pagoNombre, "TipoCambioP"),
                    Monto = _iniAdd.RequeridoDecimal(pagoNombre, "Monto"),
                    RfcEmisorCtaOrd = _iniAdd.Opcional(pagoNombre, "RfcEmisorCtaOrd"),
                    NomBancoOrdExt = _iniAdd.Opcional(pagoNombre, "NomBancoOrdExt"),
                    CtaOrdenante = _iniAdd.Opcional(pagoNombre, "CtaOrdenante"),
                    DoctoRelacionado = GetPagoDocumentosRelacionados(pagoNombre, cantidadDoctos),
                    NumOperacion = _iniAdd.Opcional(pagoNombre, "NumOperacion"),
                    RfcEmisorCtaBen = _iniAdd.Opcional(pagoNombre, "RfcEmisorCtaBen"),
                    CtaBeneficiario = _iniAdd.Opcional(pagoNombre, "CtaBeneficiario")
                };
            };
            #endregion
        }

        private void GeneraAmazonAddenda()
        {
            ElementosAmazonLosAtributos[] atributos = new ElementosAmazonLosAtributos[1];
            atributos[0] = new ElementosAmazonLosAtributos()
            {
                identificacionUnica = _iniAdd.Requerido("Atributos", "identificacionUnica"),
                nombreDelAtributo = _iniAdd.Requerido("Atributos", "nombreDelAtributo"),
                valorDelAtributo = _iniAdd.Requerido("Atributos", "valorDelAtributo")
            };

            _amazon = new ElementosAmazon()
            {
                TextoLibre = _iniAdd.Requerido("General", "TextoLibre"),
                LosAtributos = atributos
            };
        }

        #region Auxiliares para ComercioExterior
        private Schemas.ComercExt11.ComercioExteriorMercanciaDescripcionesEspecificas[] GetDescripcionesMercancia(string section)
        {
            int cantidadDescripciones = Convert.ToInt32(_iniAdd.Requerido(section, "CantidadDescripciones"));
            if (cantidadDescripciones == 0)
                return null;

            Schemas.ComercExt11.ComercioExteriorMercanciaDescripcionesEspecificas[] result = 
                new Schemas.ComercExt11.ComercioExteriorMercanciaDescripcionesEspecificas[cantidadDescripciones];

            for (int i = 0; i < cantidadDescripciones; i++)
            {
                string descripcionName = String.Format("Descripcion{0}", i + 1);
                result[i] = new Schemas.ComercExt11.ComercioExteriorMercanciaDescripcionesEspecificas
                {
                    Marca = _iniAdd.Requerido(section, string.Format("{0}{1}", descripcionName,"Marca")),
                    Modelo = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName,"Modelo")),
                    SubModelo = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName,"SubModelo")),
                    NumeroSerie = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName,"NumeroSerie"))
                };
            };

            return result;
        }

        private Schemas.ComercExt11modif.ComercioExteriorMercanciaDescripcionesEspecificas[] GetDescripcionesMercanciaModif(string section)
        {
            int cantidadDescripciones = Convert.ToInt32(_iniAdd.Requerido(section, "CantidadDescripciones"));
            if (cantidadDescripciones == 0)
                return null;

            Schemas.ComercExt11modif.ComercioExteriorMercanciaDescripcionesEspecificas[] result =
                new Schemas.ComercExt11modif.ComercioExteriorMercanciaDescripcionesEspecificas[cantidadDescripciones];

            for (int i = 0; i < cantidadDescripciones; i++)
            {
                string descripcionName = String.Format("Descripcion{0}", i + 1);
                result[i] = new Schemas.ComercExt11modif.ComercioExteriorMercanciaDescripcionesEspecificas
                {
                    Marca = _iniAdd.Requerido(section, string.Format("{0}{1}", descripcionName, "Marca")),
                    Modelo = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName, "Modelo")),
                    SubModelo = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName, "SubModelo")),
                    NumeroSerie = _iniAdd.Opcional(section, string.Format("{0}{1}", descripcionName, "NumeroSerie"))
                };
            };

            return result;
        }

        private Schemas.ComercExt11.ComercioExteriorDestinatarioDomicilio[] GetDestinatarioDomicilio(string section)
        {
            Schemas.ComercExt11.ComercioExteriorDestinatarioDomicilio[] result =
                new Schemas.ComercExt11.ComercioExteriorDestinatarioDomicilio[1];
            result[0] = new Schemas.ComercExt11.ComercioExteriorDestinatarioDomicilio
            {
                Calle = _iniAdd.Requerido(section, "Calle"),
                NumeroExterior = _iniAdd.Opcional(section, "NumeroExterior"),
                NumeroInterior = _iniAdd.Opcional(section, "NumeroInterior"),
                Colonia = _iniAdd.Opcional(section, "Colonia"),
                Localidad = _iniAdd.Opcional(section, "Localidad"),
                Referencia = _iniAdd.Opcional(section, "Referencia"),
                Municipio = _iniAdd.Opcional(section, "Municipio"),
                Estado = _iniAdd.Requerido(section, "Estado"),
                Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11.c_Pais>(section, "Pais"),
                CodigoPostal = _iniAdd.Requerido(section, "CodigoPostal")
            };
            return result;
        }

        private Schemas.ComercExt11modif.ComercioExteriorDestinatarioDomicilio[] GetDestinatarioModifDomicilio(string section)
        {
            Schemas.ComercExt11modif.ComercioExteriorDestinatarioDomicilio[] result =
                new Schemas.ComercExt11modif.ComercioExteriorDestinatarioDomicilio[1];
            result[0] = new Schemas.ComercExt11modif.ComercioExteriorDestinatarioDomicilio
            {
                Calle = _iniAdd.Requerido(section, "Calle"),
                NumeroExterior = _iniAdd.Opcional(section, "NumeroExterior"),
                NumeroInterior = _iniAdd.Opcional(section, "NumeroInterior"),
                Colonia = _iniAdd.Opcional(section, "Colonia"),
                Localidad = _iniAdd.Opcional(section, "Localidad"),
                Referencia = _iniAdd.Opcional(section, "Referencia"),
                Municipio = _iniAdd.Opcional(section, "Municipio"),
                Estado = _iniAdd.Requerido(section, "Estado"),
                Pais = _iniAdd.RequeridoEnum<Schemas.ComercExt11modif.c_Pais>(section, "Pais"),
                CodigoPostal = _iniAdd.Requerido(section, "CodigoPostal")
            };
            return result;
        }

        #endregion

        #region Auxiliar para Pagos
        private Schemas.PagosPagoDoctoRelacionado[] GetPagoDocumentosRelacionados(string section, int cantidadDoctos)
        {
            Schemas.PagosPagoDoctoRelacionado[] result = new PagosPagoDoctoRelacionado[cantidadDoctos];
            string idDocumentoNombre, serieNombre, folioNombre, monedaDRNombre, tipoCambioDRNombre, metodoPagoDRNombre, numParcialidadNombre;
            string impSaldoAnteriorNombre, impPagadoNombre, impSaldoInsolutoNombre;

            for (int i = 0; i < cantidadDoctos; i++)
            {
                idDocumentoNombre = string.Format("IdDocumento{0}", i + 1);
                serieNombre = string.Format("Serie{0}", i + 1);
                folioNombre = string.Format("Folio{0}", i + 1);
                monedaDRNombre = string.Format("MonedaDR{0}", i + 1);
                tipoCambioDRNombre = string.Format("TipoCambioDR{0}", i + 1);
                metodoPagoDRNombre = string.Format("MetodoDePagoDR{0}", i + 1);
                numParcialidadNombre = string.Format("NumParcialidad{0}", i + 1);
                impSaldoAnteriorNombre = string.Format("ImpSaldoAnt{0}", i + 1);
                impPagadoNombre = string.Format("ImpPagado{0}", i + 1);
                impSaldoInsolutoNombre = string.Format("ImpSaldoInsoluto{0}", i + 1);

                result[i] = new PagosPagoDoctoRelacionado
                {
                    IdDocumento = _iniAdd.Requerido(section, idDocumentoNombre),
                    Serie = _iniAdd.Opcional(section, serieNombre),
                    Folio = _iniAdd.Opcional(section, folioNombre),
                    MonedaDR = _iniAdd.RequeridoEnum<c_Moneda>(section, monedaDRNombre),
                    TipoCambioDR = _iniAdd.OpcionalDecimal(section, tipoCambioDRNombre),
                    TipoCambioDRSpecified = _iniAdd.Existe(section, tipoCambioDRNombre),
                    MetodoDePagoDR = _iniAdd.RequeridoEnum<c_MetodoPago>(section, metodoPagoDRNombre),
                    NumParcialidad = _iniAdd.Opcional(section, numParcialidadNombre),
                    ImpSaldoAnt = _iniAdd.OpcionalDecimal(section, impSaldoAnteriorNombre),
                    ImpSaldoAntSpecified = _iniAdd.Existe(section, impSaldoAnteriorNombre),
                    ImpPagado = _iniAdd.OpcionalDecimal(section, impPagadoNombre),
                    ImpPagadoSpecified = _iniAdd.Existe(section, impPagadoNombre),
                    ImpSaldoInsoluto = _iniAdd.OpcionalDecimal(section, impSaldoInsolutoNombre),
                    ImpSaldoInsolutoSpecified = _iniAdd.Existe(section, impSaldoInsolutoNombre)
                };
            };
            return result;
        }
        #endregion

        public ComplementoFE(IniFileHandler iniComplemento)
        {
            _iniAdd = iniComplemento;
        }

        public XmlDocument GeneraComplementoXml(string tipoComplemento, string cadenaOriginal = "", string sello = null)
        {
            XmlDocument tempDocument = new XmlDocument();

            if (tipoComplemento.ToLower() == "detallista")
            {
                GeneraDetallista();

                // Obtenemos el documento XML detallista
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("detallista", "http://www.sat.gob.mx/detallista");
                XmlSerializer tempSerializer = new XmlSerializer(typeof(detallista));
                
                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _detallista, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
            }
            else if (tipoComplemento.ToLower() == "edifact")
            {
                string strEdifact = GeneraEdifact(cadenaOriginal, sello);
                string strXmlDocument = @"<lev1add:EDCINVOICE xmlns:lev1add=""http://www.edcinvoice.com/lev1add""></lev1add:EDCINVOICE>";
                tempDocument.LoadXml(strXmlDocument);
                tempDocument["lev1add:EDCINVOICE"].InnerText = strEdifact;
            }
            else if (tipoComplemento.ToLower() == "addenda")
            {
                GeneraAddenda();

                // Agregamos la cadena original generada
                // Si es CityFresko (GLN 7505000355431) no agrega cadenaOriginal
                if (_iniAdd.Opcional("buyer", "gln") != "7505000355431")
                {
                    _addenda.requestForPayment.cadenaOriginal = new AddendaRequestForPaymentCadenaOriginal
                    {
                        Cadena = cadenaOriginal
                    };
                }
                
                // Obtenemos el documento XML de la addenda
                XmlSerializer tempSerializer = new XmlSerializer(typeof(Addenda));
                
                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _addenda);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
            }
            else if (tipoComplemento.ToLower() == "amazon")
            {
                GeneraAmazonAddenda();

                // Obtenemos el documento XML addenda AlSuper
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("amazon", "http://www.amazon.com.mx/AmazonAddenda");
                XmlSerializer tempSerializer = new XmlSerializer(typeof(ElementosAmazon));

                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _amazon, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
                XmlAttribute xsiAttrib = tempDocument.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                string textoAtributo = "http://www.amazon.com.mx/AmazonAddenda " +
                    "http://repository.edicomnet.com/schemas/mx/cfd/addenda/AmazonAddenda.xsd";
                xsiAttrib.InnerText = textoAtributo;
                tempDocument["amazon:ElementosAmazon"].Attributes.Append(xsiAttrib);
            }
            else if (tipoComplemento.ToLower() == "alsuper")
            {
                GeneraAlSuper();

                // Obtenemos el documento XML addenda AlSuper
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("alsuper", "http://proveedores.alsuper.com/CFD");
                XmlSerializer tempSerializer = new XmlSerializer(typeof(Alsuper));

                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _alSuper, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
            }
            else if (tipoComplemento.ToLower() == "comercexterior")
            {
                //GeneraComercioExterior();
                GeneraComercioExteriorModif();

                // Obtenemos el documento XML del complemento ComercioExterior
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("cce11", "http://www.sat.gob.mx/ComercioExterior11");
                //XmlSerializer tempSerializer = new XmlSerializer(typeof(Schemas.ComercExt11.ComercioExterior));
                XmlSerializer tempSerializer = new XmlSerializer(typeof(Schemas.ComercExt11modif.ComercioExterior));

                using (MemoryStream tempStream = new MemoryStream())
                {
                    //tempSerializer.Serialize(tempStream, _comercExt, ns);
                    tempSerializer.Serialize(tempStream, _comercExtModif, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
                /*GeneraComercioExterior10();

                // Obtenemos el documento XML del complemento ComercioExterior
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("cce", "http://www.sat.gob.mx/ComercioExterior");
                XmlSerializer tempSerializer = new XmlSerializer(typeof(Schemas.ComercExt10.ComercioExterior));

                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _comercExt10, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }*/
            }
            else if (tipoComplemento.ToLower() == "pago")
            {
                GeneraPagos();

                // Obtenemos el documento XML Complemento Pago
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("pago10", "http://www.sat.gob.mx/Pagos");
                XmlSerializer tempSerializer = new XmlSerializer(typeof(Pagos));

                using (MemoryStream tempStream = new MemoryStream())
                {
                    tempSerializer.Serialize(tempStream, _pagos10, ns);
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempDocument.Load(tempStream);
                }
            }

            return tempDocument;
        }
    }
}

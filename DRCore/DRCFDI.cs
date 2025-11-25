using DRBD;
using DRBD.Interfaces;
using DRCore.Enums;
using DRCore.Interfaces;
using DRCore.Models;
using DRCore.Models.SAT;

namespace DRCore
{
    public class DRCFDI(IDBHelper dbHelper) : ICFDI
    {
        private readonly IDBHelper _dbHelper = dbHelper;
        private ObjTypeEnum _objType;
        private int _docEntry;

        private List<DBParameter> Parameters =>
        [
            new("@ObjType", (int)_objType),
            new("@DocEntry", _docEntry)
        ];

        public async Task<DRResult<string>> Timbrar(ObjTypeEnum objType, int docEntry)
        {
            _objType = objType;
            _docEntry = docEntry;
            var comprobante = await ObtenerEncabezado();
            if (comprobante is null)
                throw new Exception("Documento no encontrado");

            comprobante.Emisor = await ObtenerEmisor();
            comprobante.Receptor = await ObtenerReceptor();
            comprobante.Conceptos = await ObtenerConceptos();

            var impuestosTraslados = CalcularImpuestos(comprobante.Conceptos);
            if (impuestosTraslados.Length > 0)
            {
                comprobante.Impuestos = new ComprobanteImpuestos
                {
                    TotalImpuestosTrasladados = impuestosTraslados.Sum(t => t.Importe),
                    Traslados = impuestosTraslados
                };
            }

            comprobante.Descuento = comprobante.Conceptos.Sum(c => c.Descuento);
            comprobante.SubTotal = comprobante.Conceptos.Sum(c => c.Importe);
            comprobante.Total = comprobante.SubTotal - comprobante.Descuento +
                               (comprobante.Impuestos?.TotalImpuestosTrasladados ?? 0);



            // Aquí iría la lógica para generar el XML del comprobante y enviarlo al PAC para su timbrado.


            // Simulación de timbrado exitoso
            return new DRResult<string>
            {
                IsSuccess = true,
                Message = "Comprobante timbrado exitosamente.",
                Data = "UUID-1234-5678-9012"
            };
        }


        private async Task<Comprobante?> ObtenerEncabezado()
        {
            var query = "EXEC dbo.DRCFDI_Encabezado @ObjType, @DocEntry";

            using (var reader = await _dbHelper.DoQueryAsync(query, Parameters))
            {
                if (await reader.ReadAsync())
                {
                    var comprobante = new Comprobante()
                    {
                        Version = reader["Version"]?.ToString() ?? string.Empty,
                        Serie = reader["Serie"]?.ToString() ?? string.Empty,
                        Folio = reader["Folio"]?.ToString() ?? string.Empty,
                        LugarExpedicion = reader["LugarExpedicion"]?.ToString() ?? string.Empty,
                        TipoDeComprobante = reader["TipoDeComprobante"]?.ToString() ?? string.Empty,
                        MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty,
                        FormaPago = reader["FormaPago"]?.ToString() ?? string.Empty,
                        Moneda = reader["Moneda"]?.ToString() ?? string.Empty,
                        Exportacion = reader["Exportacion"]?.ToString() ?? string.Empty,
                    };
                }
            }
            return null;
        }

        private async Task<ComprobanteEmisor> ObtenerEmisor()
        {
            var query = "EXEC dbo.DRCFDI_Emisor @ObjType, @DocEntry";

            using (var reader = await _dbHelper.DoQueryAsync(query, Parameters))
            {
                if (await reader.ReadAsync())
                {
                    return new ComprobanteEmisor
                    {
                        Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                        Rfc = reader["Rfc"]?.ToString() ?? string.Empty,
                        RegimenFiscal = reader["RegimenFiscal"]?.ToString() ?? string.Empty
                    };
                }
            }
            throw new Exception("Emisor no encontrado.");
        }
        private async Task<ComprobanteReceptor> ObtenerReceptor()
        {
            var query = "EXEC dbo.DRCFDI_Receptor @ObjType, @DocEntry";

            using (var reader = await _dbHelper.DoQueryAsync(query, Parameters))
            {
                if (await reader.ReadAsync())
                {
                    return new ComprobanteReceptor
                    {
                        Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                        Rfc = reader["Rfc"]?.ToString() ?? string.Empty,
                        UsoCFDI = reader["UsoCFDI"]?.ToString() ?? string.Empty,
                        DomicilioFiscalReceptor = reader["DomicilioFiscalReceptor"]?.ToString() ?? string.Empty,
                        RegimenFiscalReceptor = reader["RegimenFiscalReceptor"]?.ToString() ?? string.Empty
                    };
                }
            }
            throw new Exception("Receptor no encontrado.");
        }

        private async Task<ComprobanteConcepto[]> ObtenerConceptos()
        {
            var query = "EXEC dbo.DRCFDI_Conceptos @ObjType, @DocEntry";
            var conceptos = new List<ComprobanteConcepto>();
            using (var reader = await _dbHelper.DoQueryAsync(query, Parameters))
            {
                while (await reader.ReadAsync())
                {
                    var concepto = new ComprobanteConcepto
                    {
                        ClaveProdServ = reader["ClaveProdServ"]?.ToString() ?? string.Empty,
                        NoIdentificacion = reader["NoIdentificacion"]?.ToString() ?? string.Empty,
                        Cantidad = decimal.Parse(reader["Cantidad"]?.ToString() ?? "0"),
                        ClaveUnidad = reader["ClaveUnidad"]?.ToString() ?? string.Empty,
                        Unidad = reader["Unidad"]?.ToString() ?? string.Empty,
                        Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                        ValorUnitario = decimal.Parse(reader["ValorUnitario"]?.ToString() ?? "0"),
                        Importe = decimal.Parse(reader["Importe"]?.ToString() ?? "0"),
                        Descuento = decimal.Parse(reader["Descuento"]?.ToString() ?? "0"),
                        ObjetoImp = reader["ObjetoImp"]?.ToString() ?? string.Empty
                    };
                    if (concepto.ObjetoImp == "02")
                    {
                        var traslado = new ComprobanteConceptoImpuestosTraslado()
                        {
                            Base = concepto.Importe,
                            Impuesto = "002",
                            TipoFactor = c_TipoFactor.Tasa,
                            TasaOCuota = 0.160000M,
                            Importe = Math.Round(concepto.Importe * 0.16M, 2)
                        };
                        concepto.Impuestos = new ComprobanteConceptoImpuestos
                        {
                            Traslados = [traslado]
                        };
                    }
                    conceptos.Add(concepto);
                }
            }
            return conceptos.ToArray();
        }

        private ComprobanteImpuestosTraslado[] CalcularImpuestos(ComprobanteConcepto[] conceptos)
        {
            var impuestosDict = new Dictionary<string, ComprobanteImpuestosTraslado>();
            foreach (var concepto in conceptos)
            {
                if (concepto.Impuestos?.Traslados != null)
                {
                    foreach (var traslado in concepto.Impuestos.Traslados)
                    {
                        if (impuestosDict.ContainsKey(traslado.Impuesto))
                        {
                            impuestosDict[traslado.Impuesto].Base += traslado.Base;
                            impuestosDict[traslado.Impuesto].Importe += traslado.Importe;
                        }
                        else
                        {
                            impuestosDict[traslado.Impuesto] = new ComprobanteImpuestosTraslado
                            {
                                Impuesto = traslado.Impuesto,
                                TipoFactor = traslado.TipoFactor,
                                TasaOCuota = traslado.TasaOCuota,
                                Base = traslado.Base,
                                Importe = traslado.Importe
                            };
                        }
                    }
                }
            }
            return impuestosDict.Values.ToArray();
        }
    }
}

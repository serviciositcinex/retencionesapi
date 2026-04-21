using restAPI_RetencionesV1.Class;
using restAPI_RetencionesV1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace restAPI_RetencionesV1.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/Retenciones")]
    public class RetencionesController : ApiController
    {
        CultureInfo esVE = CultureInfo.CreateSpecificCulture("es-VE");
        static clsExcel excel = new clsExcel();

        public RetencionesController()
        {
        }

        private DataTable ExecuteStoredProcedure(string spName, Dictionary<string, object> parameters = null)
        {
            var connectString = ConfigurationManager.AppSettings["SAP"];
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable resultTable = new DataTable();
                        da.Fill(resultTable);
                        return resultTable;
                    }
                }
            }
        }

        [HttpGet]
        [Route("GetRetenciones/{valor1?}/{valor2?}/{valor3?}/{valor4?}/{valor5?}")]
        [Description("Obtiene las retenciones asociadas a un vendedor")]
        public IEnumerable<Retenciones> GetRetenciones(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null, string valor5 = "N")
        {
            List<Retenciones> lst = new List<Retenciones>();
            if (!string.IsNullOrEmpty(valor4) && valor4.ToUpper() == "NULL") valor4 = null;

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", valor2 },
                    { "F1", valor3 },
                    { "F2", valor4 }
                };
                
                DataTable dt = ExecuteStoredProcedure("SP_CRI", parameters);

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Retenciones
                    {
                        RifCompania = dr[0].ToString(),
                        Compania = dr[1].ToString(),
                        DireccionCompania = dr[2].ToString(),
                        RifProv = dr[3].ToString(),
                        Nombre = dr[4].ToString(),
                        E_mail = dr[5].ToString(),
                        NroComprobante = dr[6].ToString(),
                        DocEntry = dr[7].ToString(),
                        TipoDoc = dr[8].ToString(),
                        FAC = dr[9].ToString(),
                        FechaRet = (DateTime)dr[10],
                        FecFactura = (DateTime)dr[11],
                        MontoRetenido = Convert.ToDecimal(dr[12].ToString()),
                        MontoRetencion = Convert.ToDecimal(dr[13].ToString()),
                        BaseImponible = Convert.ToDecimal(dr[14].ToString()),
                        BaseIVA = Convert.ToDecimal(dr[15].ToString()),
                        Id = Convert.ToInt32(dr[16].ToString())
                    });
                }

                if (!string.IsNullOrEmpty(valor5) && valor5.ToUpper() == "Y")
                {
                    Export_to_Excel(dt);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                else throw;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetRetenciones_xls/{valor1?}/{valor2?}/{valor3?}/{valor4?}")]
        [Description("Obtiene las retenciones asociadas a un vendedor y exporta a excel")]
        public byte[] GetRetenciones_xls(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null)
        {
            DataTable dt = new DataTable();
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", valor2 },
                    { "F1", valor3 },
                    { "F2", valor4 }
                };
                dt = ExecuteStoredProcedure("SP_CRI", parameters);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return Export_to_Excel_Byte(dt);
        }

        [HttpGet]
        [Route("GetProveedores_Retenciones/{valor1?}/{valor2?}")]
        [Description("Obtiene las retenciones asociadas a un vendedor")]
        public IEnumerable<Proveedores_Retenciones> GetProveedores_Retenciones(string valor1 = null, string valor2 = null)
        {
            List<Proveedores_Retenciones> lst = new List<Proveedores_Retenciones>();
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", valor2 },
                    { "F1", null },
                    { "F2", null }
                };
                DataTable dt = ExecuteStoredProcedure("SP_CRI", parameters);

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Proveedores_Retenciones
                    {
                        SIN_nro_comprobante = dr[0].ToString(),
                        APG_nro_consecutivo = dr[1].ToString(),
                        APG_Year = dr[2].ToString(),
                        APG_Month = dr[3].ToString(),
                        APG_Status = dr[4].ToString(),
                        APG_tipo_documento = dr[5].ToString(),
                        VENDORID = dr[6].ToString(),
                        SIN_FechaCompIva = dr[7].ToString(),
                        NumFac = dr[8].ToString(),
                        Tipo = dr[9].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetProveedores_RetencionesDetalles/{valor1?}/{valor2?}/{valor3?}")]
        [Description("Obtiene el detalle de las retenciones asociadas a un vendedor")]
        public IEnumerable<Proveedores_RetencionesDetalles> GetProveedores_RetencionesDetalles(string valor1 = null, string valor2 = null, string valor3 = null)
        {
            List<Proveedores_RetencionesDetalles> lst = new List<Proveedores_RetencionesDetalles>();
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", valor2 },
                    { "F1", valor3 },
                    { "F2", null }
                };
                DataTable dt = ExecuteStoredProcedure("SP_CRI", parameters);

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Proveedores_RetencionesDetalles
                    {
                        SIN_nro_comprobante = dr[0].ToString(),
                        APG_nro_consecutivo = dr[1].ToString(),
                        APG_Year = dr[2].ToString(),
                        APG_Month = dr[3].ToString(),
                        DOCNUMBR = dr[4].ToString(),
                        APG_tipo_documento = dr[5].ToString(),
                        PSTGDATE = Convert.ToDateTime(dr[6].ToString()),
                        VENDORID = dr[7].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetProveedores_Datos/{valor1?}")]
        [Description("Obtiene los datos de un proveedor")]
        public IEnumerable<Proveedores_Datos> GetProveedores_Datos(string valor1 = null)
        {
            List<Proveedores_Datos> lst = new List<Proveedores_Datos>();
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", null },
                    { "F1", null },
                    { "F2", null }
                };
                DataTable dt = ExecuteStoredProcedure("SP_CRI", parameters);

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Proveedores_Datos
                    {
                        RIF = dr[0].ToString(),
                        Nombre = dr[1].ToString(),
                        Telefono = dr[2].ToString(),
                        Correo = dr[3].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetProveedores_Empresas/{valor1?}")]
        [Description("Obtiene las empresas en las cuales el proveedor este registrado")]
        public IEnumerable<Proveedores_Empresas> GetProveedores_Empresas(string valor1 = null)
        {
            List<Proveedores_Empresas> lst = new List<Proveedores_Empresas>();
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", null },
                    { "F1", null },
                    { "F2", null }
                };
                DataTable dt = ExecuteStoredProcedure("SP_CRI", parameters);

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Proveedores_Empresas
                    {
                        Empresa = dr[0].ToString(),
                        Inter = dr[1].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetProveedores_Empresas_Todos")]
        [Description("Obtiene las empresas en las cuales el proveedor este registrado")]
        public IEnumerable<Proveedores_Empresas> GetProveedores_Empresas_Todos()
        {
            List<Proveedores_Empresas> lst = new List<Proveedores_Empresas>();
            try
            {
                DataTable dt = ExecuteStoredProcedure("USP_CRI_Reciente_Automatico");

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Proveedores_Empresas
                    {
                        Empresa = dr[0].ToString(),
                        Inter = dr[1].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                // Aquí la excepción nativa de base de datos explotará y nos dirá si es error de logon, procedimiento inexistente, etc.
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                else throw;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Route("GetRetenciones_ISLR_xls/{valor1?}/{valor2?}/{valor3?}/{valor4?}")]
        [Description("Obtiene las retenciones de ISLR asociadas a un vendedor y exporta a excel")]
        public IHttpActionResult GetRetenciones_ISLR_xls(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null)
        {
            DataTable dt = null;
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 },
                    { "SN2", valor2 },
                    { "F1", valor3 },
                    { "F2", valor4 }
                };
                dt = ExecuteStoredProcedure("SP_CR_ISLR_PORTAL", parameters);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return InternalServerError(ex);
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

            if (dt == null || dt.Rows.Count == 0)
            {
                return Content(HttpStatusCode.NoContent, new { Message = "No se encontraron registros para los filtros indicados." });
            }

            byte[] fileBytes;
            try
            {
                fileBytes = ExportISLR_to_Excel_Byte(dt);
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return Content(HttpStatusCode.InternalServerError, new { Message = "La generación del Excel devolvió contenido vacío." });
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return InternalServerError(ex);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileBytes)
            };

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var fileName = string.Format("RETENCIONES_ISLR_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return ResponseMessage(response);
        }

        private void Export_to_Excel(DataTable dt_Consulta)
        {
            var file = "Formato_RETIVA.xlsx";
            var strFilePathTemplate = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;
            byte[] vByte = excel.Generar_Excel_Retenciones(dt_Consulta, strFilePathTemplate);
            var vDirectorio = @"C:\temp2\";

            if (!Directory.Exists(vDirectorio))
            {
                Directory.CreateDirectory(vDirectorio);
            }

            bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - RETENCIONES -" + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][1].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][4].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][6].ToString() : "") + ".xlsx", vByte);
        }

        private byte[] Export_to_Excel_Byte(DataTable dt_Consulta)
        {
            var file = "Formato_RETIVA.xlsx";
            var strFilePathTemplate = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;
            byte[] vByte = excel.Generar_Excel_Retenciones(dt_Consulta, strFilePathTemplate);
            var vDirectorio = @"C:\temp2\";

            if (!Directory.Exists(vDirectorio))
            {
                Directory.CreateDirectory(vDirectorio);
            }

            bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - RETENCIONES IVA-" + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][1].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][4].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][6].ToString() : "") + ".xlsx", vByte);

            return vByte;
        }

        private byte[] ExportISLR_to_Excel_Byte(DataTable dt_Consulta)
        {
            var file = "Formato_RETISLR.xlsx";
            var strFilePathTemplate = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;
            byte[] vByte = excel.Generar_Excel_RetencionesISLR(dt_Consulta, strFilePathTemplate);
            var vDirectorio = @"C:\temp2\";

            if (!Directory.Exists(vDirectorio))
            {
                Directory.CreateDirectory(vDirectorio);
            }

            bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - RETENCIONES ISLR-" + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][1].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][4].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 0 ? dt_Consulta.Rows[0][6].ToString() : "") + ".xlsx", vByte);

            return vByte;
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        public void Log(string mensaje)
        {
        }

        #region ARCV

        [HttpGet]
        [Route("GetRetenciones_ARCV_xls/{valor1?}/{valor2?}/{valor3?}")]
        [Description("Obtiene las retenciones para el ARCV asociadas a un vendedor y exporta a excel")]
        public byte[] GetRetenciones_ARCV_xls(string valor1 = null, string valor2 = null, string valor3 = null)
        {
            DataTable dt = new DataTable();

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "PE_EMPRESA", valor1 },
                    { "PE_VENDORID", valor2 },
                    { "PE_ANIO", valor3 }
                };
                dt = ExecuteStoredProcedure("Lee_RetencionesARCV_ProveedorDetallesV1", parameters);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                if (Request != null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return ExportARCV_to_Excel_Byte(dt);
        }

        private byte[] ExportARCV_to_Excel_Byte(DataTable dt_Consulta)
        {
            byte[] vByte = null;
            try
            {
                var file = "Formato_ARCV.xlsx";
                var strFilePathTemplate = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;
                vByte = excel.Generar_Excel_RetencionesARCV(dt_Consulta, strFilePathTemplate);
                var vDirectorio = @"C:\temp2\";

                if (!Directory.Exists(vDirectorio))
                {
                    Directory.CreateDirectory(vDirectorio);
                }

                if (dt_Consulta != null && dt_Consulta.Rows.Count > 0)
                {
                    bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - ARCV - " + dt_Consulta.Rows[0][4].ToString() + ".xlsx", vByte);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            return vByte;
        }

        #endregion

        [HttpGet]
        [Route("GetRetenciones_ISLR_Preview/{valor1?}")]
        public IHttpActionResult GetRetenciones_ISLR_Preview(string valor1 = null)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "SN1", valor1 }
                };
                DataTable dt = ExecuteStoredProcedure("SP_CR_ISLR_PORTAL", parameters);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Content(HttpStatusCode.InternalServerError, new { Message = "El procedimiento almacenado no devolvió datos, o falló la conexión." });
                }

                var list = dt.AsEnumerable()
                    .Select(r => dt.Columns.Cast<DataColumn>()
                        .ToDictionary(c => c.ColumnName, c => r.IsNull(c) ? null : r[c]))
                    .ToList();

                return Ok(new { RowCount = dt.Rows.Count, Data = list });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}

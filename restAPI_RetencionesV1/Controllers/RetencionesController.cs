using dllConnect.ADO_Net;
using restAPI_RetencionesV1.Class;
using restAPI_RetencionesV1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace restAPI_RetencionesV1.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class RetencionesController : ApiController
    {
        CultureInfo esVE = CultureInfo.CreateSpecificCulture("es-VE");
        static clsExcel excel = new clsExcel();
        DataTable dt = null;
        static string strFilePath;
        static string strFilePath_destino = string.Empty;
        public RetencionesController()
        {
            dllConnect.ADO_Net.Conexion.connectString = ConfigurationManager.AppSettings["SAP"];

            //Log("Conexion: " + ConfigurationManager.AppSettings[complejo]);

        }

        [HttpGet]
        [Description("Obtiene las retenciones asociadas a un vendedor")]
        public IEnumerable<Retenciones> GetRetenciones(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null, string valor5 = "N")
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Retenciones> lst = new List<Retenciones>();

            if (!string.IsNullOrEmpty(valor4) && valor4.ToUpper() == "NULL")
            {
                valor4 = null;
            }

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                // SP_CRI espera: @SN1, @SN2, @F1, @F2
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, valor2);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, valor3);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, valor4);
                adonet.leer();

                dt = adonet.DS.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new Retenciones
                    {
                        // Mapeo actualizado según lista proporcionada:
                        // RifAgente -> RifCompania
                        RifCompania = dr[0].ToString(),
                        // NombreAgente -> Compania
                        Compania = dr[1].ToString(),
                        // DirAgente -> DireccionCompania
                        DireccionCompania = dr[2].ToString(),
                        // Rif -> RifProv
                        RifProv = dr[3].ToString(),
                        // Nombre -> Nombre (se mantiene nombre, tamaño en BD es nvarchar(50))
                        Nombre = dr[4].ToString(),
                        // Email -> E_mail
                        E_mail = dr[5].ToString(),
                        // NroComprobante -> NroComprobante (nota: ISLR no usa comprobante)
                        NroComprobante = dr[6].ToString(),
                        // VCHRNMBR -> DocEntry
                        DocEntry = dr[7].ToString(),
                        // TipoDocumento -> TipoDoc
                        TipoDoc = dr[8].ToString(),
                        // NroFactura -> FAC
                        FAC = dr[9].ToString(),
                        // FechaDocumento -> FechaRet
                        FechaRet = (DateTime)dr[10],
                        // FechaEmision -> FecFactura
                        FecFactura = (DateTime)dr[11],
                        // MontoImpuesto -> MontoRetenido
                        MontoRetenido = Convert.ToDecimal(dr[12].ToString()),
                        // MontoRetencion (se mantiene si el SP devuelve este valor en la columna 13)
                        MontoRetencion = Convert.ToDecimal(dr[13].ToString()),
                        // BaseImponible -> BaseImponible (misma propiedad)
                        BaseImponible = Convert.ToDecimal(dr[14].ToString()),
                        // MontoTotal -> BaseIVA
                        BaseIVA = Convert.ToDecimal(dr[15].ToString()),
                        Id = Convert.ToInt32(dr[16].ToString())
                    });
                    //lst.Add(new ComplejoActivo { area = dr[0].ToString(), rif = dr[1].ToString(), descripcion = dr[2].ToString(), complejo = dr[3].ToString(), codigo = dr[4].ToString(), grupo = dr[5].ToString() });
                }

                if (!string.IsNullOrEmpty(valor5) && valor5.ToUpper() == "Y")
                {
                    Export_to_Excel(dt);
                }

            }
            catch (Exception ex)
            {
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Description("Obtiene las retenciones asociadas a un vendedor y exporta a excel")]
        public byte[] GetRetenciones_xls(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null)
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Retenciones> lst = new List<Retenciones>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, valor2);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, valor3);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, valor4);
                adonet.leer();

                dt = adonet.DS.Tables[0];

            }
            catch (Exception ex)
            {
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return Export_to_Excel_Byte(dt);
        }

        [HttpGet]
        [Description("Obtiene las retenciones asociadas a un vendedor")]
        public IEnumerable<Proveedores_Retenciones> GetProveedores_Retenciones(string valor1 = null, string valor2 = null)
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Proveedores_Retenciones> lst = new List<Proveedores_Retenciones>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                // Mapear vendor/empresa a SN1/SN2, filtros adicionales vacíos
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, valor2);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.leer();

                dt = adonet.DS.Tables[0];

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
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Description("Obtiene el detalle de las retenciones asociadas a un vendedor")]
        public IEnumerable<Proveedores_RetencionesDetalles> GetProveedores_RetencionesDetalles(string valor1 = null, string valor2 = null, string valor3 = null)
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Proveedores_RetencionesDetalles> lst = new List<Proveedores_RetencionesDetalles>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                // valor3 mapeado a F1 (filtro)
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, valor2);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, valor3);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.leer();

                dt = adonet.DS.Tables[0];

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
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Description("Obtiene los datos de un proveedor")]
        public IEnumerable<Proveedores_Datos> GetProveedores_Datos(string valor1 = null)
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Proveedores_Datos> lst = new List<Proveedores_Datos>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                // vendor -> SN1
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.leer();

                dt = adonet.DS.Tables[0];

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
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }

        [HttpGet]
        [Description("Obtiene las empresas en las cuales el proveedor este registrado")]
        public IEnumerable<Proveedores_Empresas> GetProveedores_Empresas(string valor1 = null)
        {

            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<Proveedores_Empresas> lst = new List<Proveedores_Empresas>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "SP_CRI";
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, null);
                adonet.leer();

                dt = adonet.DS.Tables[0];

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
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return lst;
        }


        [HttpGet]
        [Description("Obtiene las retenciones de ISLR asociadas a un vendedor y exporta a excel")]
        public IHttpActionResult GetRetenciones_ISLR_xls(string valor1 = null, string valor2 = null, string valor3 = null, string valor4 = null)
        {
            ADO_V2 adonet = new ADO_V2();
            DataTable dt = null;

            try
            {
                adonet.SP = "SP_CR_ISLR_PORTAL";
                adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
                adonet.AgregarParametro("SN2", ADO_V2.enumTipo_de_Datos._nvarchar, valor2);
                adonet.AgregarParametro("F1", ADO_V2.enumTipo_de_Datos._nvarchar, valor3);
                adonet.AgregarParametro("F2", ADO_V2.enumTipo_de_Datos._nvarchar, valor4);
                adonet.leer();

                dt = (adonet.DS != null && adonet.DS.Tables.Count > 0) ? adonet.DS.Tables[0] : null;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return InternalServerError(ex);
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

            if (dt == null || dt.Rows.Count == 0)
            {
                // No hay datos: informar al cliente
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
            var fileName = $"RETENCIONES_ISLR_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return ResponseMessage(response);
        }

        /// <summary>
        /// Exporta y escribe en disco el archivo
        /// </summary>
        /// <param name="dt_Consulta">data con las retenciones</param>
        private void Export_to_Excel(DataTable dt_Consulta)
        {
            var file = "Formato_RETIVA.xlsx";
            var strFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;

            byte[] vByte = excel.Generar_Excel_Retenciones(dt_Consulta, strFilePath);

            var vDirectorio = @"C:\temp2\";

            if (!Directory.Exists(vDirectorio))
            {
                Directory.CreateDirectory(vDirectorio);
            }

            bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - RETENCIONES -" + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][1].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][4].ToString() : "") + " - " + (dt_Consulta.Rows.Count > 1 ? dt_Consulta.Rows[1][6].ToString() : "") + ".xlsx", vByte);

        }

        /// <summary>
        /// genera un array de byte para guardar del lado del cliente
        /// </summary>
        /// <param name="dt_Consulta">data con las retenciones</param>
        /// <returns></returns>
        private byte[] Export_to_Excel_Byte(DataTable dt_Consulta)
        {
            var file = "Formato_RETIVA.xlsx";
            var strFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;

            byte[] vByte = excel.Generar_Excel_Retenciones(dt_Consulta, strFilePath);

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
            var strFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;

            byte[] vByte = excel.Generar_Excel_RetencionesISLR(dt_Consulta, strFilePath);

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

        /// <summary>
        /// Genera un log de los procesos del API
        /// </summary>
        /// <param name="mensaje">mensaje a guardar</param>
        public void Log(string mensaje)
        {

        }

        #region ARCV

        //http://localhost:1503/api/Retenciones/GetRetenciones_ARCV_xls/RPSAS/J-409131351/2023

        /// <summary>
        /// Obtiene las retenciones para el ARCV asociadas a un vendedor y exporta a excel
        /// </summary>
        /// <param name="valor1">Empresa</param>
        /// <param name="valor2">RIF</param>
        /// <param name="valor3">Año a consultar</param>
        /// <returns></returns>
        [HttpGet]
        [Description("Obtiene las retenciones para el ARCV asociadas a un vendedor y exporta a excel")]
        public byte[] GetRetenciones_ARCV_xls(string valor1 = null, string valor2 = null, string valor3 = null)
        {
            ADO_V2 adonet;
            DataTable dt = new DataTable();
            List<RetencionesARCV> lst = new List<RetencionesARCV>();

            try
            {
                adonet = new ADO_V2();
                adonet.SP = "Lee_RetencionesARCV_ProveedorDetallesV1";
                adonet.AgregarParametro("PE_EMPRESA", ADO_V2.enumTipo_de_Datos._varchar, valor1);
                adonet.AgregarParametro("PE_VENDORID", ADO_V2.enumTipo_de_Datos._varchar, valor2);
                adonet.AgregarParametro("PE_ANIO", ADO_V2.enumTipo_de_Datos._nvarchar, valor3);
                adonet.leer();

                dt = adonet.DS.Tables[0];
            }
            catch (Exception ex)
            {
                string Ex = ex.Message;
                Log(ex.Message);
                dt = null;
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            byte[] tmp = ExportARCV_to_Excel_Byte(dt);

            return tmp;
        }

        private byte[] ExportARCV_to_Excel_Byte(DataTable dt_Consulta)
        {
            byte[] vByte = null;
            try
            {
                var file = "Formato_ARCV.xlsx";
                var strFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Template\" + file;

                vByte = excel.Generar_Excel_RetencionesARCV(dt_Consulta, strFilePath);

                var vDirectorio = @"C:\temp2\";

                if (!Directory.Exists(vDirectorio))
                {
                    Directory.CreateDirectory(vDirectorio);
                }

                bool resp = ByteArrayToFile(vDirectorio + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + " - ARCV - " + dt_Consulta.Rows[0][4].ToString() + ".xlsx", vByte);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return vByte;
        }

        #endregion

        [HttpGet]
        public IHttpActionResult GetRetenciones_ISLR_Preview(string valor1 = null)
        {
            ADO_V2 adonet = new ADO_V2();
            adonet.SP = "SP_CR_ISLR_PORTAL";
            adonet.AgregarParametro("SN1", ADO_V2.enumTipo_de_Datos._nvarchar, valor1);
            adonet.leer();
            var dt = adonet.DS.Tables[0];

            var list = dt.AsEnumerable()
                .Select(r => dt.Columns.Cast<DataColumn>()
                    .ToDictionary(c => c.ColumnName, c => r.IsNull(c) ? null : r[c]))
                .ToList();

            return Ok(new { RowCount = dt.Rows.Count, Data = list });
        }
    }
}

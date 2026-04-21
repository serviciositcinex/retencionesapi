using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace restAPI_RetencionesV1.Models
{
    public class Retenciones
    {
        public string RifCompania { get; set; }
        public string Compania { get; set; }
        public string DireccionCompania { get; set; }
        public string RifProv { get; set; }
        public string Nombre { get; set; }
        public string E_mail { get; set; }
        public string NroComprobante { get; set; }
        public string DocEntry { get; set; }
        public string TipoDoc { get; set; }
        public string FAC { get; set; }
        public DateTime FechaRet { get; set; }
        public DateTime FecFactura { get; set; }
        public decimal MontoRetenido { get; set; }
        public decimal MontoRetencion { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal BaseIVA { get; set; }
        public int Id { get; set; }
    }

	public class RetencionesISLR
	{
		public string RifAgente { get; set; }
		public string NombreAgente { get; set; }
		public string DirAgente { get; set; }
		public string Rif { get; set; }
		public string Nombre { get; set; }
		public string Email { get; set; }
		public string NroComprobante { get; set; }
		public string CompInterno { get; set; }
		public string TipoDocumento { get; set; }
		public string NroFactura { get; set; }
		public DateTime FechaDocumento { get; set; }
		public DateTime FechaEmision { get; set; }
		public string TipoImpuesto { get; set; }
		public decimal Porcentaje { get; set; }
		public decimal MontoImpuesto { get; set; }
		public decimal BaseImponible { get; set; }
		public string DirDestinatario { get; set; }
		//public int Id { get; set; }

	}
	public class Proveedores_Retenciones
    {
		public string SIN_nro_comprobante { get; set; }
		public string APG_nro_consecutivo { get; set; }
		public string APG_Year { get; set; }
		public string APG_Month { get; set; }
		public string APG_Status { get; set; }
		public string APG_tipo_documento { get; set; }
		public string VENDORID { get; set; }
		public string SIN_FechaCompIva { get; set; }
		public string NumFac{ get; set; }
		public string Tipo { get; set; }
	}

	public class Proveedores_RetencionesDetalles
	{
		public string SIN_nro_comprobante { get; set; }
		public string APG_nro_consecutivo { get; set; }
		public string APG_Year { get; set; }
		public string APG_Month { get; set; }
		public string DOCNUMBR { get; set; }
		public string APG_tipo_documento { get; set; }
		public DateTime PSTGDATE { get; set; }
		public string VENDORID { get; set; }

	}

	public class Proveedores_Datos
	{
		public string RIF { get; set; }
		public string Nombre { get; set; }
		public string Telefono { get; set; }
		public string Correo { get; set; }

	}

	public class Proveedores_Empresas
	{
		public string Empresa { get; set; }
		public string Inter { get; set; }

	}


    public class RetencionesARCV
    {
        public string RifAgente { get; set; }
        public string NombreAgente { get; set; }
        public string DirAgente { get; set; }
        public string Rif { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal BaseImponible { get; set; }
        public string DirDestinatario { get; set; }

    }



}
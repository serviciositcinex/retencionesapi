using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace restAPI_RetencionesV1.Class
{
    public class clsExcel
    {

        #region Generar Distribuidores
        public byte[] Generar_Excel_Retenciones(DataTable dt, string plantilla)
        {
            int vIndice = 1;
            try
            {

                using (ExcelPackage objExcelPackage = new ExcelPackage(new FileInfo(plantilla)))
                {
                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets["Hoja1"];

                    if (dt.Rows.Count == 1)
                    {
                        vIndice = 0;
                    }


                    objWorksheet.Cells[3, 10].Value = dt.Rows[vIndice][6].ToString();
                    objWorksheet.Cells[3, 12].Value = "Año: " + Convert.ToDateTime(dt.Rows[vIndice][10]).Year.ToString() + "     Mes: " + Convert.ToDateTime(dt.Rows[vIndice][10]).Month.ToString().PadLeft(2, '0');
                    //objWorksheet.Cells[3, 12].Value = "Año: " + Convert.ToDateTime(dt.Rows[vIndice][11]).Year.ToString() + "     Mes: " + Convert.ToDateTime(dt.Rows[vIndice][11]).Month.ToString().PadLeft(2, '0');

                    objWorksheet.Cells[6, 1].Value = dt.Rows[vIndice][1].ToString();
                    objWorksheet.Cells[6, 7].Value = dt.Rows[vIndice][0].ToString();

                    var dtData = dt.AsEnumerable().OrderByDescending(r => (Convert.ToDateTime(r.Field<DateTime>("FechaDocumento"))))
                        .Take(1)
                        .Select(s => s.Field<DateTime>("FechaDocumento"));

                    DateTime colFecha = dtData.FirstOrDefault();
                    //objWorksheet.Cells[6, 12].Value = DateTime.ParseExact(colFecha.ToString(), "MM/dd/yyyy", CultureInfo.CurrentCulture);
                    objWorksheet.Cells[6, 12].Value = Convert.ToDateTime(colFecha.ToString("MM/dd/yyyy"));
                    //objWorksheet.Cells[6, 12].Value = Convert.ToDateTime(colFecha.ToString("dd/MM/yyyy"));

                    //objWorksheet.Cells[6, 12].Value = Convert.ToDateTime(dt.Rows[vIndice][10].ToString()).ToString("dd/MM/yyyy");

                    objWorksheet.Cells[9, 1].Value = dt.Rows[vIndice][2].ToString();

                    objWorksheet.Cells[12, 1].Value = dt.Rows[vIndice][4].ToString();
                    objWorksheet.Cells[12, 5].Value = dt.Rows[vIndice][3].ToString();

                    int x = 15;
                    int i = 15;

                    foreach (DataRow item in dt.Rows)
                    {
                        objWorksheet.Cells[x, 1].Value = item[16].ToString();
                        objWorksheet.Cells[x, 2].Value = Convert.ToDateTime(item[10].ToString()).ToString("dd/MM/yyyy");
                        objWorksheet.Cells[x, 3].Value = item[9].ToString();
                        objWorksheet.Cells[x, 4].Value = item[17].ToString();
                        objWorksheet.Cells[x, 8].Value = Convert.ToDecimal(item[15]);
                        objWorksheet.Cells[x, 10].Value = Convert.ToDecimal(item[14]);
                        objWorksheet.Cells[x, 12].Value = Convert.ToDecimal(item[12]);
                        objWorksheet.Cells[x, 11].Value = Convert.ToDecimal(item[18]); /* IVA */
                        objWorksheet.Cells[x, 13].Value = Convert.ToDecimal(item[13]) * -1;

                        objWorksheet.Cells[x, 8, x, 13].Style.Numberformat.Format = "#,##0.00";

                        i++;
                        x++;
                    }

                    objWorksheet.Cells[39, 1].Value = Convert.ToDateTime(dt.Rows[vIndice][11].ToString()).ToShortDateString();

                    //objWorksheet.Cells["H26"].Formula = "=SUM(H15:H" + x.ToString() + ")"; //Autosuma
                    //objWorksheet.Cells["I26"].Formula = "=SUM(I15:I" + x.ToString() + ")"; //Autosuma 
                    //objWorksheet.Cells["J26"].Formula = "=SUM(J15:J" + x.ToString() + ")"; //Autosuma 
                    //objWorksheet.Cells["K26"].Formula = "=SUM(K15:K" + x.ToString() + ")"; //Autosuma 
                    //objWorksheet.Cells["L26"].Formula = "=SUM(L15:L" + x.ToString() + ")"; //Autosuma 
                    //objWorksheet.Cells["M26"].Formula = "=SUM(M15:M" + x.ToString() + ")"; //Autosuma 

                    objWorksheet.Cells["H36"].Formula = "=SUM(H15:H" + x.ToString() + ")"; //Autosuma
                    objWorksheet.Cells["I36"].Formula = "=SUM(I15:I" + x.ToString() + ")"; //Autosuma 
                    objWorksheet.Cells["J36"].Formula = "=SUM(J15:J" + x.ToString() + ")"; //Autosuma 
                    objWorksheet.Cells["K36"].Formula = "=K15:K15"; //Autosuma 
                    //objWorksheet.Cells["K36"].Formula = "=SUM(K15:K" + x.ToString() + ")"; //Autosuma 
                    objWorksheet.Cells["L36"].Formula = "=SUM(L15:L" + x.ToString() + ")"; //Autosuma 
                    objWorksheet.Cells["M36"].Formula = "=SUM(M15:M" + x.ToString() + ")"; //Autosuma 

                    objWorksheet.Cells[36, 8, 36, 13].Style.Numberformat.Format = "#,##0.00";

                    //objWorksheet.Cells[vRows, 1].Value = "El uso de este formato es estrictamente DIGITAl, y su envío debe ser a través del correo suministrado en el Manual de Especificaciones Técnicas.";

                    objWorksheet.Protection.IsProtected = true;
                    objWorksheet.Protection.AllowSelectLockedCells = false;


                    return objExcelPackage.GetAsByteArray();
                }

            }
            catch (Exception ex)
            {
                string tmp = ex.StackTrace;

                System.IO.File.AppendAllText(@"c:\temp2\txt\Log_Retenciones.txt", "API Retenciones --> " + DateTime.Now + " --> " + tmp);

                byte[] data = new byte[0];
                return data;
            }


        }

        public byte[] Generar_Excel_RetencionesISLR(DataTable dt, string plantilla)
        {
            int vIndice = 1;
            try
            {
                using (ExcelPackage objExcelPackage = new ExcelPackage(new FileInfo(plantilla)))
                {
                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets["Hoja1"];

                    if (dt.Rows.Count == 1)
                    {
                        vIndice = 0;
                    }

                    string aa = DateTime.Now.ToString("dd/MM/yyyy");
                    objWorksheet.Cells[2, 10].Value = DateTime.Now.ToString("dd/MM/yyyy");
                    objWorksheet.Cells[2, 11].Value = DateTime.Now.ToString("hh:mm tt");
                    objWorksheet.Cells[2, 12].Value = 1;


                    objWorksheet.Cells[1, 4].Value = dt.Rows[vIndice][6].ToString().Replace("-", "");
                    objWorksheet.Cells[2, 2].Value = Convert.ToDateTime(dt.Rows[vIndice][11]).ToString("dd/MM/yyyy");
                    objWorksheet.Cells[6, 3].Value = dt.Rows[vIndice][1].ToString();
                    objWorksheet.Cells[7, 3].Value = dt.Rows[vIndice][0].ToString();
                    objWorksheet.Cells[8, 3].Value = dt.Rows[vIndice][2].ToString();

                    objWorksheet.Cells[6, 9].Value = dt.Rows[vIndice][4].ToString();
                    objWorksheet.Cells[7, 9].Value = dt.Rows[vIndice][3].ToString();
                    objWorksheet.Cells[8, 9].Value = dt.Rows[vIndice][16].ToString();
                    objWorksheet.Cells[11, 3].Value = "JURIDICA";
                    objWorksheet.Cells[11, 9].Value = "JURIDICA";

                    int x = 15;
                    int i = 15;

                    foreach (DataRow item in dt.Rows)
                    {
                        objWorksheet.Cells[x, 1].Value = Convert.ToDateTime(item[10]).ToString("dd/MM/yyyy");
                        objWorksheet.Cells[x, 2].Value = item[9].ToString();
                        objWorksheet.Cells[x, 3].Value = item[7].ToString();
                        objWorksheet.Cells[x, 4].Value = ""; //item[9].ToString();
                        decimal a = Convert.ToDecimal(item[15]);
                        decimal total = a + (a * 16 / 100);
                        
                        //objWorksheet.Cells[x, 5].Value = Convert.ToDecimal(total);  //item[9].ToString();
                        objWorksheet.Cells[x, 5].Value = Convert.ToDecimal(item[17]);  //item[9].ToString();
                        objWorksheet.Cells[x, 6].Value = Convert.ToDecimal(item[15]);
                        objWorksheet.Cells[x, 7].Value = 0; //Convert.ToDecimal(item[15]);

                        objWorksheet.Cells[x, 8].Value = 0;
                        objWorksheet.Cells[x, 9].Value = Convert.ToDecimal(item[15]);
                        objWorksheet.Cells[x, 10].Value = item[12].ToString();
                        objWorksheet.Cells[x, 11].Value = Convert.ToDecimal(item[13]);
                        objWorksheet.Cells[x, 12].Value = Convert.ToDecimal(item[14]);

                        objWorksheet.Cells[x, 5, x, 12].Style.Numberformat.Format = "#,##0.00";

                        i++;
                        x++;
                    }

                    int y = x;
                    x = x + 2;
                    objWorksheet.Cells[x, 4, x, 4].Merge = true;
                    objWorksheet.Cells[x, 4].Value = "TOTALES";
                    objWorksheet.Cells[x, 4].Style.Font.Bold = true;

                    objWorksheet.Cells[x, 5].Formula = "=SUM(E15:E" + y.ToString() + ")"; //Autosuma
                    objWorksheet.Cells[x, 6].Formula = "=SUM(F15:F" + y.ToString() + ")"; //Autosuma
                    objWorksheet.Cells[x, 9].Formula = "=SUM(I15:I" + y.ToString() + ")"; //Autosuma 
                    objWorksheet.Cells[x, 12].Formula = "=SUM(L15:L" + y.ToString() + ")"; //Autosuma 

                    objWorksheet.Cells[x, 5, x, 12].Style.Numberformat.Format = "#,##0.00";

                    objWorksheet.Cells[x, 5, x, 12].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    //objWorksheet.Cells[vRows, 1].Value = "El uso de este formato es estrictamente DIGITAl, y su envío debe ser a través del correo suministrado en el Manual de Especificaciones Técnicas.";

                    objWorksheet.Protection.IsProtected = true;
                    objWorksheet.Protection.AllowSelectLockedCells = false;

                    return objExcelPackage.GetAsByteArray();
                }

            }
            catch (Exception ex)
            {
                string msg = ex.Message;


                byte[] data = new byte[0];
                return data;
            }


        }

        public byte[] Generar_Excel_RetencionesARCV(DataTable dt, string plantilla)
        {
            int vIndice = 1;
            try
            {

                using (ExcelPackage objExcelPackage = new ExcelPackage(new FileInfo(plantilla)))
                {
                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets["Hoja1"];

                    if (dt.Rows.Count == 1)
                    {
                        vIndice = 0;
                    }

                    objWorksheet.Cells[7, 1].Value = dt.Rows[vIndice][1].ToString();
                    objWorksheet.Cells[9, 1].Value = dt.Rows[vIndice][2].ToString();
                    objWorksheet.Cells[7, 5].Value = dt.Rows[vIndice][4].ToString();
                    objWorksheet.Cells[11, 5].Value = dt.Rows[vIndice][11].ToString();
                    objWorksheet.Cells[11, 7].Value = dt.Rows[vIndice][3].ToString();
                    objWorksheet.Cells[14, 8].Value = "01/01/" + dt.Rows[vIndice][7].ToString();
                    objWorksheet.Cells[15, 8].Value = "31/12/" + dt.Rows[vIndice][7].ToString();

                    int x = 19;
                    int i = 1;

                    foreach (DataRow item in dt.Rows)
                    {
                        string tmp1 = "01/0" + item[6].ToString() + "/2000";

                        string mes1 = Convert.ToDateTime("01/" + item[6].ToString() + "/2000").ToString("MMMM");

                        //objWorksheet.Cells[x, 1].Value = item[6].ToString();
                        objWorksheet.Cells[x, 1].Value = Convert.ToDateTime("01/" + item[6].ToString() + "/2000").ToString("MMMM");
                        objWorksheet.Cells[x, 2].Value = item[7].ToString();
                        objWorksheet.Cells[x, 3].Value = Convert.ToDecimal(item[10]);
                        objWorksheet.Cells[x, 4].Value = Convert.ToDecimal(item[8]);
                        objWorksheet.Cells[x, 5].Value = Convert.ToDecimal(item[9]);
                        objWorksheet.Cells[x, 6].Value = Convert.ToDecimal(item[10]);
                        objWorksheet.Cells[x, 7].Value = Convert.ToDecimal(item[9]);

                        objWorksheet.Cells[x, 3, x, 7].Style.Numberformat.Format = "#,##0.00";

                        i++;
                        x++;
                    }

                    objWorksheet.Cells[34, 3].Formula = "=SUM(C19:C33)"; //Autosuma
                    objWorksheet.Cells[34, 5].Formula = "=SUM(E19:E33)"; //Autosuma 
                    objWorksheet.Cells[34, 6].Formula = "=SUM(F19:F33)"; //Autosuma 
                    objWorksheet.Cells[34, 7].Formula = "=SUM(G19:H33)"; //Autosuma 

                    objWorksheet.Cells[34, 3, 34, 3].Style.Numberformat.Format = "#,##0.00";
                    objWorksheet.Cells[34, 5, 34, 7].Style.Numberformat.Format = "#,##0.00";


                    objWorksheet.Protection.IsProtected = true;
                    objWorksheet.Protection.AllowSelectLockedCells = false;


                    var tmp2 = objExcelPackage.GetAsByteArray();
                    return tmp2; //objExcelPackage.GetAsByteArray();
                }

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                byte[] data = new byte[0];
                return data;
            }


        }

        #endregion


    }
}





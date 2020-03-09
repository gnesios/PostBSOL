using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Configuration;

using System.Net.Mail;
using System.Net.Mime;

namespace ReportesCruceATC
{
    public partial class ReporteCruce : Form
    {
        String vSeparador = string.Empty;
        StringBuilder vProcesoDeLaFecha = new StringBuilder();
        String vRutaNomArchivoCrucebsoH2H = ConfigurationManager.AppSettings["RutaArchivoCrucebsoH2H"].ToString();
        String vRutaNomArchivoCrucebso = ConfigurationManager.AppSettings["RutaArchivoCrucebso"].ToString();
        String vRutaNomArchivoInctf02 = ConfigurationManager.AppSettings["RutaArchivoInctf02"].ToString();
        String vRutaNomArchivoRepConATCDet = ConfigurationManager.AppSettings["NombreArchivoRepCruceATCDet"].ToString();
        String vRutaNomArchivoRepConATCRes = ConfigurationManager.AppSettings["NombreArchivoRepCruceATCRes"].ToString();
        String vTipoCambioTexto = ConfigurationManager.AppSettings["TipoCambioReporte"].ToString();
        long vContadorCrucebsoH2H = 0;
        long vContadorCrucebso = 0;
        long vContadorInctf02 = 0;
        decimal vSumadorCrucebsoH2H = 0;
        decimal vSumadorComisionCrucebsoH2H = 0;
        decimal vSumadorCrucebso = 0;
        decimal vSumadorComisionCrucebso = 0;
        decimal vSumadorInctf02 = 0;
        decimal vSumadorComisionInctf02 = 0;
        long vContadorTrjVISA = 0;
        long vContadorTrjMASTER = 0;
        long vContadorTrjOtros = 0;
        decimal vSumadorTrjVISA = 0;
        decimal vSumadorTrjMASTER = 0;
        decimal vSumadorTrjOtros = 0;
        decimal vTrjVISAComisionCobrar = 0;
        decimal vTrjMASTERComisionCobrar = 0;
        decimal vTrjOtrosComisionCobrar = 0;
        decimal vTrjVISAComisionPagar = 0;
        decimal vTrjMASTERComisionPagar = 0;
        decimal vTrjOtrosComisionPagar = 0;
        decimal vSumadorTrjVISAComision = 0;
        decimal vSumadorTrjMASTERComision = 0;
        decimal vSumadorTrjOtrosComision = 0;

        public ReporteCruce()
        {
            InitializeComponent();
        }

        private void ReporteConciliacion_Load(object sender, EventArgs e)
        {
            try
            {
                txtboxRutaNomArchivoCrucebsoH2H.Text = vRutaNomArchivoCrucebsoH2H;
                txtboxRutaNomArchivoCrucebso.Text = vRutaNomArchivoCrucebso;
                txtboxRutaNomArchivoInctf02.Text = vRutaNomArchivoInctf02;
                txtBoxRutaArchivoDetalle.Text = vRutaNomArchivoRepConATCDet;
                txtBoxRutaArchivoResumen.Text = vRutaNomArchivoRepConATCRes;
                txtBoxTipoDeCambio.Text = vTipoCambioTexto;
                vTrjVISAComisionCobrar = decimal.Parse(ConfigurationManager.AppSettings["VISAComisionCobrar"].ToString());
                vTrjMASTERComisionCobrar = decimal.Parse(ConfigurationManager.AppSettings["MASTERComisionCobrar"].ToString());
                vTrjOtrosComisionCobrar = decimal.Parse(ConfigurationManager.AppSettings["OtrosComisionCobrar"].ToString());
                vTrjVISAComisionPagar = decimal.Parse(ConfigurationManager.AppSettings["VISAComisionPagar"].ToString());
                vTrjMASTERComisionPagar = decimal.Parse(ConfigurationManager.AppSettings["MASTERComisionPagar"].ToString());
                vTrjOtrosComisionPagar = decimal.Parse(ConfigurationManager.AppSettings["OtrosComisionPagar"].ToString());

                if (1 == Program.gAutomatico)
                {
                    txtboxResultado.Text = Program.gArgumentos;

                    // Llamar a las rutina de Proceso de los Archivos de ATC
                    ProcesarArchivosATC();

                    this.Close();
                }
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Error encontrado en el Proceso");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
            }
        }

        private void ProcesarArchivosATC()
        {
            //procesar el primer archivo
            ProcesarArchivoCrucebsoH2H();

            //procesar el segundo archivo
            ProcesarArchivoCrucebso();

            //procesar el tercer archivo
            ProcesarArchivoInctf02();

            /*Enviar correo de Confirmación de proceso*/

            EnviarCorreo("Correo de Validación Cruce ATC Archivos[CrucebsoH2H - Crucebso - Inctf02] " + DateTime.Today.ToString("yyyyMMdd"),
                         vProcesoDeLaFecha.ToString());

            /*Fin Enviar correo de Confirmación de proceso*/
        }

        private void ProcesarArchivoCrucebsoH2H()
        {
            Int32 vContador = 0;

            String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();

            String vNomArchivoRepATCDet = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCDet"].ToString() + ".txt";
            String vNomArchivoRepATCRes = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCRes"].ToString() + ".txt";
            String vMarcaTrj = string.Empty;
            decimal vTipoCambio = 0;
            vTipoCambio = decimal.Parse(txtBoxTipoDeCambio.Text);

            try
            {
                txtboxResultado.Text = String.Empty;
                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                GrabarLog("Inicio Proceso archivo Crucebsoh2h");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoCrucebsoH2H.Text))
                {

                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoCrucebsoH2H.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        int vLongitudLinea = 0;

                        StreamWriter vSwArchivoRepATCDet = new StreamWriter(vNomArchivoRepATCDet);
                        StreamWriter vSwArchivoRepATCRes = new StreamWriter(vNomArchivoRepATCRes);


                        while ((vLinea = vStreamReader.ReadLine()) != null)
                        {
                            vLongitudLinea = vLinea.Length;
                            if (vLongitudLinea == 25)
                            {
                                //el registro es una cabecera
                                StringBuilder vConsLinea = new StringBuilder();
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(0, 2));
                                vTipoMensaje = vLinea.Substring(0, 2);
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(2, 6));
                                int vAuxNumero = 0;
                                vAuxNumero = int.Parse(vLinea.Substring(2, 6));
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(8, 17));
                                decimal vAuxMonto = 0;
                                vAuxMonto = (decimal.Parse(vLinea.Substring(8, 17))) / 100;
                                txtboxResultado.Text = txtboxResultado.Text + "Cabecera: " + vConsLinea.ToString() + Environment.NewLine;
                                txtboxResultado.Text = txtboxResultado.Text + "Nro: " + vAuxNumero.ToString() + " Suma: " + vAuxMonto.ToString() + Environment.NewLine;
                            }
                            else
                            {
                                //el registro es de detalle
                                vContador++;

                                StringBuilder vDetalleLinea = new StringBuilder();

                                switch (vTipoMensaje)
                                {
                                    case "01":
                                    case "03":
                                        vDetalleLinea.Append("ATM");
                                        vDetalleLinea.Append(vSeparador);
                                        string vNumTarjeta = vLinea.Substring(9, 6) + "******" + vLinea.Substring(21, 4);
                                        vDetalleLinea.Append(vNumTarjeta);
                                        vDetalleLinea.Append(vSeparador);
                                        string vMoneda = vLinea.Substring(63, 3);
                                        vDetalleLinea.Append(vMoneda);
                                        vDetalleLinea.Append(vSeparador);
                                        decimal vMonto = 0;
                                        decimal vMontoBs = 0;
                                        vMonto = decimal.Parse(vLinea.Substring(48, 15));
                                        vMonto = vMonto / 100;
                                        vDetalleLinea.Append(vMonto.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        if ("068" == vMoneda)
                                        {
                                            vMontoBs = vMonto;
                                        }
                                        else
                                        {
                                            vMontoBs = vMonto * vTipoCambio;
                                        }
                                        vDetalleLinea.Append(vMontoBs.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        decimal vMontoComision = 0;

                                        //clasificar tarjeta
                                        vContadorCrucebsoH2H++;
                                        vSumadorCrucebsoH2H += vMontoBs;
                                        vMarcaTrj = vLinea.Substring(9, 1);
                                        switch (vMarcaTrj)
                                        {
                                            //VISA sus tarjetas inician con 4
                                            case "4":
                                                vContadorTrjVISA++;
                                                vSumadorTrjVISA += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjVISAComisionCobrar;
                                                vSumadorTrjVISAComision += vMontoComision;
                                                vDetalleLinea.Append("VISA  ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //MASTERCARD sus tarjetas inician con 5
                                            case "5":
                                                vContadorTrjMASTER++;
                                                vSumadorTrjMASTER += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjMASTERComisionCobrar;
                                                vSumadorTrjMASTERComision += vMontoComision;
                                                vDetalleLinea.Append("MASTER");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //otras tarjetas
                                            default:
                                                vContadorTrjOtros++;
                                                vSumadorTrjOtros += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjOtrosComisionCobrar;
                                                vSumadorTrjOtrosComision += vMontoComision;
                                                vDetalleLinea.Append("OTROS ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                        }
                                        vSumadorComisionCrucebsoH2H += vMontoComision;
                                        vDetalleLinea.Append(vMontoComision.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        string vFechaDet = vLinea.Substring(26, 8);
                                        vDetalleLinea.Append(vFechaDet);
                                        vDetalleLinea.Append(vSeparador);
                                        txtboxTramaRecon.Text = String.Empty;
                                        txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                        vSwArchivoRepATCDet.WriteLine(vDetalleLinea.ToString());

                                        break;
                                    default:
                                        txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                        break;
                                }

                                lblNumeroRegistros.Text = vContador.ToString("00000");
                                this.Refresh();
                            }
                        }

                        StringBuilder vResumenLinea = new StringBuilder();
                        vSwArchivoRepATCRes.WriteLine("Resumen Proceso Archivo CruceBsoH2H - Por Cobrar");
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas VISA procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjVISA.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISA.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISAComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas MASTERCARD procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjMASTER.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTER.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTERComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Otras Tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjOtros.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtros.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtrosComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Total CRUCEBSOH2H tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorCrucebsoH2H.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorCrucebsoH2H.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorComisionCrucebsoH2H.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine("".PadRight(90, '-'));
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();

                        vSwArchivoRepATCDet.Close();
                        vSwArchivoRepATCRes.Close();

                        vProcesoDeLaFecha.Append("Archivo Crucebsoh2h procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();
                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
                        txtboxTramaRecon.Text = "Archivo Crucebsoh2h procesado";
                        this.Refresh();
                    }
                }
                else
                {
                    //no existe el archivo para el proceso.
                    GrabarLog("No se encontro el archivo crucebsoh2h para el proceso de la fecha.");
                    vProcesoDeLaFecha.Append("Archivo Crucebsoh2h No procesado");
                    vProcesoDeLaFecha.AppendLine();
                }
                GrabarLog("Fin Proceso archivo Crucebsoh2h");
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Detalle del error presentado en el proceso del archivo crucebsoh2h");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
            }
        }

        private void ProcesarArchivoCrucebso()
        {
            Int32 vContador = 0;

            String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();

            String vNomArchivoRepATCDet = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCDet"].ToString() + ".txt";
            String vNomArchivoRepATCRes = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCRes"].ToString() + ".txt";
            String vMarcaTrj = string.Empty;
            decimal vTipoCambio = 0;
            vTipoCambio = decimal.Parse(txtBoxTipoDeCambio.Text);

            vContadorTrjVISA = 0;
            vSumadorTrjVISA = 0;
            vSumadorTrjVISAComision = 0;

            vContadorTrjMASTER = 0;
            vSumadorTrjMASTER = 0;
            vSumadorTrjMASTERComision = 0;

            vContadorTrjOtros = 0;
            vSumadorTrjOtros = 0;
            vSumadorTrjOtrosComision = 0;

            try
            {
                txtboxResultado.Text = String.Empty;
                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                GrabarLog("Inicio Proceso archivo Crucebso");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoCrucebso.Text))
                {

                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoCrucebso.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        int vLongitudLinea = 0;

                        StreamWriter vSwArchivoRepATCDet = new StreamWriter(vNomArchivoRepATCDet, true);
                        StreamWriter vSwArchivoRepATCRes = new StreamWriter(vNomArchivoRepATCRes, true);

                        while ((vLinea = vStreamReader.ReadLine()) != null)
                        {
                            vLongitudLinea = vLinea.Length;
                            if (vLongitudLinea == 25)
                            {
                                //el registro es una cabecera
                                StringBuilder vConsLinea = new StringBuilder();
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(0, 2));
                                vTipoMensaje = vLinea.Substring(0, 2);
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(2, 6));
                                int vAuxNumero = 0;
                                vAuxNumero = int.Parse(vLinea.Substring(2, 6));
                                vConsLinea.Append(vSeparador);
                                vConsLinea.Append(vLinea.Substring(8, 17));
                                decimal vAuxMonto = 0;
                                vAuxMonto = (decimal.Parse(vLinea.Substring(8, 17))) / 100;
                                txtboxResultado.Text = txtboxResultado.Text + "Cabecera: " + vConsLinea.ToString() + Environment.NewLine;
                                txtboxResultado.Text = txtboxResultado.Text + "Nro: " + vAuxNumero.ToString() + " Suma: " + vAuxMonto.ToString() + Environment.NewLine;
                            }
                            else
                            {
                                //el registro es de detalle
                                vContador++;

                                StringBuilder vDetalleLinea = new StringBuilder();

                                switch (vTipoMensaje)
                                {
                                    case "01":
                                    case "03":
                                        vDetalleLinea.Append("ATM");
                                        vDetalleLinea.Append(vSeparador);
                                        string vNumTarjeta = vLinea.Substring(9, 6) + "******" + vLinea.Substring(21, 4);
                                        vDetalleLinea.Append(vNumTarjeta);
                                        vDetalleLinea.Append(vSeparador);
                                        string vMoneda = vLinea.Substring(63, 3);
                                        vDetalleLinea.Append(vMoneda);
                                        vDetalleLinea.Append(vSeparador);
                                        decimal vMonto = 0;
                                        decimal vMontoBs = 0;
                                        vMonto = decimal.Parse(vLinea.Substring(48, 15));
                                        vMonto = vMonto / 100;
                                        vDetalleLinea.Append(vMonto.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        if ("068" == vMoneda)
                                        {
                                            vMontoBs = vMonto ;
                                        }
                                        else
                                        {
                                            vMontoBs = vMonto * vTipoCambio;
                                        }
                                        vDetalleLinea.Append(vMontoBs.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        decimal vMontoComision = 0;

                                        //clasificar tarjeta
                                        vContadorCrucebso++;
                                        vSumadorCrucebso += vMontoBs;
                                        vMarcaTrj = vLinea.Substring(9, 1);
                                        switch (vMarcaTrj)
                                        {
                                            //VISA sus tarjetas inician con 4
                                            case "4":
                                                vContadorTrjVISA++;
                                                vSumadorTrjVISA += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjVISAComisionPagar;
                                                vSumadorTrjVISAComision += vMontoComision;
                                                vDetalleLinea.Append("VISA  ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //MASTERCARD sus tarjetas inician con 5
                                            case "5":
                                                vContadorTrjMASTER++;
                                                vSumadorTrjMASTER += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjMASTERComisionPagar;
                                                vSumadorTrjMASTERComision += vMontoComision;
                                                vDetalleLinea.Append("MASTER");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //otras tarjetas
                                            default:
                                                vContadorTrjOtros++;
                                                vSumadorTrjOtros += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjOtrosComisionPagar;
                                                vSumadorTrjOtrosComision += vMontoComision;
                                                vDetalleLinea.Append("OTROS ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                        }
                                        vSumadorComisionCrucebso += vMontoComision;
                                        vDetalleLinea.Append(vMontoComision.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        string vFechaDet = vLinea.Substring(26, 8);
                                        vDetalleLinea.Append(vFechaDet);
                                        vDetalleLinea.Append(vSeparador);
                                        txtboxTramaRecon.Text = String.Empty;
                                        txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                        vSwArchivoRepATCDet.WriteLine(vDetalleLinea.ToString());

                                        break;
                                    case "02":
                                        vDetalleLinea.Append("POS");
                                        vDetalleLinea.Append(vSeparador);
                                        vNumTarjeta = vLinea.Substring(21, 6) + "******" + vLinea.Substring(33, 4);
                                        vDetalleLinea.Append(vNumTarjeta);
                                        vDetalleLinea.Append(vSeparador);
                                        vMoneda = vLinea.Substring(86, 3);
                                        vDetalleLinea.Append(vMoneda);
                                        vDetalleLinea.Append(vSeparador);
                                        vMonto = 0;
                                        vMontoBs = 0;
                                        vMonto = decimal.Parse(vLinea.Substring(51, 15));
                                        vMonto = vMonto / 100;
                                        vDetalleLinea.Append(vMonto.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        if ("068" == vMoneda)
                                        {
                                            vMontoBs = vMonto;
                                        }
                                        else
                                        {
                                            vMontoBs = vMonto * vTipoCambio;
                                        }
                                        vDetalleLinea.Append(vMontoBs.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        vMontoComision = 0;

                                        //clasificar tarjeta
                                        vContadorCrucebso++;
                                        vSumadorCrucebso += vMontoBs;
                                        vMarcaTrj = vLinea.Substring(21, 1);
                                        switch (vMarcaTrj)
                                        {
                                            //VISA sus tarjetas inician con 4
                                            case "4":
                                                vContadorTrjVISA++;
                                                vSumadorTrjVISA += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjVISAComisionPagar;
                                                vSumadorTrjVISAComision += vMontoComision;
                                                vDetalleLinea.Append("VISA  ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //MASTERCARD sus tarjetas inician con 5
                                            case "5":
                                                vContadorTrjMASTER++;
                                                vSumadorTrjMASTER += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjMASTERComisionPagar;
                                                vSumadorTrjMASTERComision += vMontoComision;
                                                vDetalleLinea.Append("MASTER");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                            //otras tarjetas
                                            default:
                                                vContadorTrjOtros++;
                                                vSumadorTrjOtros += vMontoBs;
                                                vMontoComision = vMontoBs * vTrjOtrosComisionPagar;
                                                vSumadorTrjOtrosComision += vMontoComision;
                                                vDetalleLinea.Append("OTROS ");
                                                vDetalleLinea.Append(vSeparador);
                                                break;
                                        }
                                        vSumadorComisionCrucebso += vMontoComision;
                                        vDetalleLinea.Append(vMontoComision.ToString("0000000000000.00"));
                                        vDetalleLinea.Append(vSeparador);
                                        vFechaDet = vLinea.Substring(37, 8);
                                        vDetalleLinea.Append(vFechaDet);
                                        vDetalleLinea.Append(vSeparador);
                                        txtboxTramaRecon.Text = String.Empty;

                                        vSwArchivoRepATCDet.WriteLine(vDetalleLinea.ToString());
                                        break;
                                    default:
                                        txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                        break;
                                }

                                lblNumeroRegistros.Text = vContador.ToString("00000");
                                this.Refresh();
                            }

                        }

                        StringBuilder vResumenLinea = new StringBuilder();
                        vSwArchivoRepATCRes.WriteLine("Resumen Proceso Archivo CruceBso - Por Pagar");
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas VISA procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjVISA.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISA.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISAComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas MASTERCARD procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjMASTER.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTER.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTERComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Otras Tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjOtros.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtros.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtrosComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Total CRUCEBSO tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorCrucebso.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorCrucebso.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorComisionCrucebso.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine("".PadRight(90, '-'));
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();

                        vSwArchivoRepATCDet.Close();
                        vSwArchivoRepATCRes.Close();

                        vProcesoDeLaFecha.Append("Archivo Crucebso procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();
                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
                        txtboxTramaRecon.Text = "Archivo Crucebso procesado";
                        this.Refresh();
                    }
                }
                else
                {
                    //no existe el archivo para el proceso.
                    GrabarLog("No se encontro el archivo crucebso para el proceso de la fecha.");
                    vProcesoDeLaFecha.Append("Archivo Crucebso No procesado");
                    vProcesoDeLaFecha.AppendLine();
                }
                GrabarLog("Fin Proceso archivo Crucebso");
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Detalle del error presentado en el proceso del archivo crucebso");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
            }
        }

        private void ProcesarArchivoInctf02()
        {
            Int32 vContador = 0;
            txtboxResultado.Text = String.Empty;

            try
            {
                String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();

                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                String vNomArchivoRepATCDet = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCDet"].ToString() + ".txt";
                String vNomArchivoRepATCRes = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoRepCruceATCRes"].ToString() + ".txt";
                String vMarcaTrj = string.Empty;
                decimal vTipoCambio = 0;
                vTipoCambio = decimal.Parse(txtBoxTipoDeCambio.Text);

                vContadorTrjVISA = 0;
                vSumadorTrjVISA = 0;
                vSumadorTrjVISAComision = 0;

                vContadorTrjMASTER = 0;
                vSumadorTrjMASTER = 0;
                vSumadorTrjMASTERComision = 0;

                vContadorTrjOtros = 0;
                vSumadorTrjOtros = 0;
                vSumadorTrjOtrosComision = 0;

                GrabarLog("Inicio Proceso archivo Inctf02");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoInctf02.Text))
                {
                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoInctf02.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;

                        StreamWriter vSwArchivoRepATCDet = new StreamWriter(vNomArchivoRepATCDet, true);
                        StreamWriter vSwArchivoRepATCRes = new StreamWriter(vNomArchivoRepATCRes, true);

                        while ((vLinea = vStreamReader.ReadLine()) != null)
                        {
                            StringBuilder vDetalleLinea = new StringBuilder();
                            vContador++;
                            vTipoMensaje = vLinea.Substring(0, 4);
                            switch (vTipoMensaje)
                            {
                                case "0700":
                                case "0500":
                                    txtboxTramaRecon.Text = String.Empty;
                                    txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                    if (vLinea.Substring(0, 2) == "07")
                                    {
                                        vDetalleLinea.Append("ATM");
                                    }
                                    else
                                    {
                                        vDetalleLinea.Append("POS");
                                    }
                                    
                                    vDetalleLinea.Append(vSeparador);
                                    string vNumTarjeta = vLinea.Substring(4, 6) + "******" + vLinea.Substring(16, 4);
                                    vDetalleLinea.Append(vNumTarjeta);
                                    vDetalleLinea.Append(vSeparador);
                                    decimal vMonto = 0;
                                    decimal vMontoBs = 0;
                                    string vMoneda = vLinea.Substring(88, 3);
                                    if ("068" == vMoneda)
                                    {
                                        vMonto = decimal.Parse(vLinea.Substring(76, 12));
                                        vMonto = vMonto / 100;
                                        vMontoBs = vMonto;
                                    }
                                    else
                                    {
                                        vMonto = decimal.Parse(vLinea.Substring(61, 12));
                                        vMonto = vMonto / 100;
                                        vMontoBs = vMonto * vTipoCambio;
                                        vMoneda = vLinea.Substring(73, 3);
                                    }

                                    vDetalleLinea.Append(vMoneda);
                                    vDetalleLinea.Append(vSeparador);
                                    
                                    
                                    vDetalleLinea.Append(vMonto.ToString("0000000000000.00"));
                                    vDetalleLinea.Append(vSeparador);
                                    
                                    vDetalleLinea.Append(vMontoBs.ToString("0000000000000.00"));
                                    vDetalleLinea.Append(vSeparador);
                                    decimal vMontoComision = 0;

                                    //clasificar tarjeta
                                    vContadorInctf02++;
                                    vSumadorInctf02 += vMontoBs;
                                    vMarcaTrj = vLinea.Substring(4, 1);
                                    switch (vMarcaTrj)
                                    {
                                        //VISA sus tarjetas inician con 4
                                        case "4":
                                            vContadorTrjVISA++;
                                            vSumadorTrjVISA += vMontoBs;
                                            vMontoComision = vMontoBs * vTrjVISAComisionPagar;
                                            vSumadorTrjVISAComision += vMontoComision;
                                            vDetalleLinea.Append("VISA  ");
                                            vDetalleLinea.Append(vSeparador);
                                            break;
                                        //MASTERCARD sus tarjetas inician con 5
                                        case "5":
                                            vContadorTrjMASTER++;
                                            vSumadorTrjMASTER += vMontoBs;
                                            vMontoComision = vMontoBs * vTrjMASTERComisionPagar;
                                            vSumadorTrjMASTERComision += vMontoComision;
                                            vDetalleLinea.Append("MASTER");
                                            vDetalleLinea.Append(vSeparador);
                                            break;
                                        //otras tarjetas
                                        default:
                                            vContadorTrjOtros++;
                                            vSumadorTrjOtros += vMontoBs;
                                            vMontoComision = vMontoBs * vTrjOtrosComisionPagar;
                                            vSumadorTrjOtrosComision += vMontoComision;
                                            vDetalleLinea.Append("OTROS ");
                                            vDetalleLinea.Append(vSeparador);
                                            break;
                                    }
                                    vSumadorComisionInctf02 += vMontoComision;
                                    vDetalleLinea.Append(vMontoComision.ToString("0000000000000.00"));
                                    vDetalleLinea.Append(vSeparador);
                                    int vAnioActual = DateTime.Today.Year;
                                    int vMesActual = DateTime.Today.Month;
                                    int vMesProceso = 0;
                                    string vFechaDet = string.Empty;
                                    vMesProceso = int.Parse(vLinea.Substring(57, 2));
                                    if (vMesProceso >= 11 && vMesActual <= 2)
                                    {
                                        vFechaDet = (vAnioActual-1).ToString() + vLinea.Substring(57, 4);
                                    }
                                    else
                                    {
                                        vFechaDet = vAnioActual.ToString() + vLinea.Substring(57, 4);
                                    }
                                    
                                    vDetalleLinea.Append(vFechaDet);
                                    vDetalleLinea.Append(vSeparador);

                                    vSwArchivoRepATCDet.WriteLine(vDetalleLinea.ToString());

                                    break;
                                default:
                                    txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                    break;
                            }

                            lblNumeroRegistros.Text = vContador.ToString("00000");
                            this.Refresh();
                        }

                        StringBuilder vResumenLinea = new StringBuilder();
                        vSwArchivoRepATCRes.WriteLine("Resumen Proceso Archivo Intcf02 - Por Pagar");
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas VISA procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjVISA.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISA.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjVISAComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Tarjetas MASTERCARD procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjMASTER.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTER.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjMASTERComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Otras Tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorTrjOtros.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtros.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorTrjOtrosComision.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine("Total INTCF02 tarjetas procesadas");
                        vSwArchivoRepATCRes.WriteLine();
                        vResumenLinea.Length = 0;
                        vResumenLinea.Append("Cantidad:");
                        vResumenLinea.Append(vContadorInctf02.ToString("00000"));
                        vResumenLinea.Append("  Total Monto en Bs.");
                        vResumenLinea.Append(vSumadorInctf02.ToString("0000000000000.00"));
                        vResumenLinea.Append("  Total Comisión en Bs.");
                        vResumenLinea.Append(vSumadorComisionInctf02.ToString("0000000000000.00"));
                        vSwArchivoRepATCRes.WriteLine(vResumenLinea.ToString());
                        vSwArchivoRepATCRes.WriteLine("".PadRight(90, '-'));
                        vSwArchivoRepATCRes.WriteLine();
                        vSwArchivoRepATCRes.WriteLine();

                        vSwArchivoRepATCDet.Close();
                        vSwArchivoRepATCRes.Close();

                        vProcesoDeLaFecha.Append("Archivo Inctf02 procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();

                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
                        txtboxTramaRecon.Text = "Archivo Inctf02 procesado";
                        this.Refresh();
                    }
                }
                else
                {
                    //no existe el archivo para el proceso.
                    GrabarLog("No se encontro el archivo inctf02 para el proceso de la fecha.");
                    vProcesoDeLaFecha.Append("Archivo Inctf02 No procesado");
                    vProcesoDeLaFecha.AppendLine();
                }
                GrabarLog("Fin Proceso archivo Inctf02");
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Detalle del error presentado en el proceso del archivo inctf02");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
            }
        }

        private void EnviarCorreo(string asuntoCorreo, string detalleCorreo)
        {
            //creamos el objeto de correo
            MailMessage msgCorreo = new MailMessage();

            //asignamos la dirección de correo de origen
            msgCorreo.From = new MailAddress(ConfigurationManager.AppSettings["UsuarioOrigenCorreo"].ToString());

            //asignamos la dirección de correo de destino
            msgCorreo.To.Add(ConfigurationManager.AppSettings["ListaUsuarioDestino"].ToString());

            //asignamos la dirección de correo Con Copia
            string validaCC = string.Empty;
            validaCC = ConfigurationManager.AppSettings["ListaConCopiaDestino"].ToString();
            if (!string.IsNullOrEmpty(validaCC))
            {
                msgCorreo.CC.Add(ConfigurationManager.AppSettings["ListaConCopiaDestino"].ToString());
            }

            //añadimos el asunto del correo
            msgCorreo.Subject = asuntoCorreo;

            //añadimos el detalle del correo
            msgCorreo.Body = detalleCorreo;

            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoLog = vRutaBaseLog + "ReportesCruceATC" + DateTime.Today.ToString("yyyyMMdd") + ".log";
            //validar la existencia del archivo a Adjuntar
            if (File.Exists(vNombreArchivoLog))
            {
                Attachment vArchivoLog = new Attachment(vNombreArchivoLog, MediaTypeNames.Text.Plain);
                msgCorreo.Attachments.Add(vArchivoLog);
            }

            //enviar el mensaje de correo
            SmtpClient smtpCliente = new SmtpClient(ConfigurationManager.AppSettings["ServidorCorreo"].ToString());
            smtpCliente.Send(msgCorreo);
        }

        private void GrabarLog(string vTextoAGrabar)
        {
            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoLog = vRutaBaseLog + "ReportesCruceATC" + DateTime.Today.ToString("yyyyMMdd") + ".log";
            StringBuilder vCadena = new StringBuilder();

            vCadena.Append("[");
            vCadena.Append(DateTime.Today.ToString("dd/MM/yyyy"));
            vCadena.Append(" ");
            vCadena.Append(DateTime.Now.ToString("HH:mm:ss"));
            vCadena.Append("] ");
            vCadena.Append(vTextoAGrabar);

            StreamWriter vStreamLog = new StreamWriter(vNombreArchivoLog, true);
            vStreamLog.WriteLine(vCadena.ToString());
            vStreamLog.Close();

        }

        private void btnProcesarArchivo_Click(object sender, EventArgs e)
        {
            ProcesarArchivosATC();
        }

    }
}

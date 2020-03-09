using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Configuration;

using System.Net.Mail;
using System.Net.Mime;

namespace GeneraBsolRecon
{
    public partial class Principal : Form
    {
        String vSeparador = string.Empty;
        StringBuilder vProcesoDeLaFecha = new StringBuilder();
        String vRutaNomArchivoCrucebsoH2H = ConfigurationManager.AppSettings["RutaArchivoCrucebsoH2H"].ToString();
        String vRutaNomArchivoCrucebso = ConfigurationManager.AppSettings["RutaArchivoCrucebso"].ToString();
        String vRutaNomArchivoInctf02 = ConfigurationManager.AppSettings["RutaArchivoInctf02"].ToString();

        public Principal()
        {
            InitializeComponent();
        }

        private void Principal_Load(object sender, EventArgs e)
        {
            try
            {
                txtboxRutaNomArchivoCrucebsoH2H.Text = vRutaNomArchivoCrucebsoH2H;
                txtboxRutaNomArchivoCrucebso.Text = vRutaNomArchivoCrucebso;
                txtboxRutaNomArchivoInctf02.Text = vRutaNomArchivoInctf02;

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

            EnviarCorreo("Correo de Validación Archivos[CrucebsoH2H - Crucebso - Inctf02] " + DateTime.Today.ToString("yyyyMMdd"),
                         vProcesoDeLaFecha.ToString());

            /*Fin Enviar correo de Confirmación de proceso*/
        }

        private void btnProcesarArchivo_Click(object sender, EventArgs e)
        {
            ProcesarArchivosATC();

        }

        private void ProcesarArchivoCrucebsoH2H()
        {
            Int32 vContador = 0;
            int vCantidadCamposRecon = 0;

            String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();
            String vNomArchivoSink = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoATCReconSink"].ToString() + ".txt";
            String vListaTarjetasBsol = string.Empty;
            Int32 vTarjetaBSol = 1;

            try
            {
                txtboxResultado.Text = String.Empty;
                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                vListaTarjetasBsol = ConfigurationManager.AppSettings["TarjetasBSOL"].ToString();

                vCantidadCamposRecon = int.Parse(ConfigurationManager.AppSettings["CantidadCamposRecon"]);

                GrabarLog("Inicio Proceso archivo Crucebsoh2h");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoCrucebsoH2H.Text))
                {

                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoCrucebsoH2H.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        int vLongitudLinea = 0;

                        StreamWriter vSwArchivoReconSink = new StreamWriter(vNomArchivoSink);

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
                                ///CantidadCamposRecon;
                                string[] aCampos = new string[vCantidadCamposRecon + 1];
                                StringBuilder vReconLinea = new StringBuilder();
                                int vPosicion = 0;
                                int vLongitud = 0;

                                switch (vTipoMensaje)
                                {
                                    case "01":
                                    case "03":
                                        txtboxTramaRecon.Text = String.Empty;
                                        //txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                        aCampos[1] = "0200";
                                        aCampos[15] = "00";
                                        aCampos[30] = "BSO";
                                        aCampos[2] = vLinea.Substring(0, 6);
                                        aCampos[3] = vLinea.Substring(9, 16);
                                        //aCampos[3] = vLinea.Substring(9, 19);
                                        if (vListaTarjetasBsol.Contains(aCampos[3].Substring(0, 6)))
                                            vTarjetaBSol = 1;
                                        else
                                            vTarjetaBSol = 0;

                                        //CON PAN de 16
                                        aCampos[7] = vLinea.Substring(26, 8);
                                        aCampos[8] = vLinea.Substring(34, 6);
                                        aCampos[10] = vLinea.Substring(40, 8);
                                        aCampos[11] = vLinea.Substring(48, 15);
                                        aCampos[13] = vLinea.Substring(63, 3);
                                        aCampos[4] = vLinea.Substring(66, 2);
                                        aCampos[17] = vLinea.Substring(68, 3);

                                        //CON PAN de 19
                                        //aCampos[7] = vLinea.Substring(29, 8);
                                        //aCampos[8] = vLinea.Substring(37, 6);
                                        //aCampos[10] = vLinea.Substring(43, 8);
                                        //aCampos[11] = vLinea.Substring(51, 15);
                                        //aCampos[13] = vLinea.Substring(66, 3);
                                        //aCampos[4] = vLinea.Substring(69, 2);
                                        //aCampos[17] = vLinea.Substring(71, 3);

                                        /// datos archivo Recon
                                        vReconLinea = new StringBuilder();

                                        vPosicion = 0;
                                        vLongitud = 0;
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                            if (vLongitud > 0)
                                            {
                                                vReconLinea.Append(aCampos[i]);
                                                vPosicion = vLongitud;
                                            }
                                            vReconLinea.Append(vSeparador);

                                        }

                                        if (0 == vTarjetaBSol)
                                            vSwArchivoReconSink.WriteLine(vReconLinea.ToString());
                                        /// fin datos archivo Recon

                                        break;
                                    default:
                                        txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                        break;
                                }

                                lblNumeroRegistros.Text = vContador.ToString("00000");
                                this.Refresh();
                            }

                        }

                        vSwArchivoReconSink.Close();

                        vProcesoDeLaFecha.Append("Archivo Crucebsoh2h procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();
                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
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
            int vCantidadCamposRecon = 0;

            String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();
            String vNomArchivoSource = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoATCReconSource"].ToString() + ".txt";
            String vListaTarjetasBsol = string.Empty;
            Int32 vTarjetaBSol = 1;

            try
            {
                txtboxResultado.Text = String.Empty;
                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                vListaTarjetasBsol = ConfigurationManager.AppSettings["TarjetasBSOL"].ToString();

                vCantidadCamposRecon = int.Parse(ConfigurationManager.AppSettings["CantidadCamposRecon"]);

                GrabarLog("Inicio Proceso archivo Crucebso");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoCrucebso.Text))
                {

                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoCrucebso.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        int vLongitudLinea = 0;

                        StreamWriter vSwArchivoReconSource = new StreamWriter(vNomArchivoSource);

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
                                ///CantidadCamposRecon;
                                string[] aCampos = new string[vCantidadCamposRecon + 1];
                                StringBuilder vReconLinea = new StringBuilder();
                                int vPosicion = 0;
                                int vLongitud = 0;

                                switch (vTipoMensaje)
                                {
                                    case "01":
                                    case "03":
                                        txtboxTramaRecon.Text = String.Empty;
                                        //txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                        aCampos[1] = "0200";
                                        aCampos[15] = "00";
                                        aCampos[30] = "BSO";
                                        aCampos[2] = vLinea.Substring(0, 6);
                                        aCampos[3] = vLinea.Substring(9, 16);
                                        if (vListaTarjetasBsol.Contains(aCampos[3].Substring(0, 6)))
                                            vTarjetaBSol = 1;
                                        else
                                            vTarjetaBSol = 0;

                                        aCampos[7] = vLinea.Substring(26, 8);
                                        aCampos[8] = vLinea.Substring(34, 6);
                                        aCampos[10] = vLinea.Substring(40, 8);
                                        aCampos[11] = vLinea.Substring(48, 15);
                                        aCampos[13] = vLinea.Substring(63, 3);
                                        aCampos[4] = vLinea.Substring(66, 2);
                                        aCampos[17] = vLinea.Substring(68, 3);

                                        /// datos archivo Recon
                                        vReconLinea = new StringBuilder();

                                        vPosicion = 0;
                                        vLongitud = 0;
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                            if (vLongitud > 0)
                                            {
                                                vReconLinea.Append(aCampos[i]);
                                                vPosicion = vLongitud;
                                            }
                                            vReconLinea.Append(vSeparador);

                                        }

                                        if (1 == vTarjetaBSol)
                                            vSwArchivoReconSource.WriteLine(vReconLinea.ToString());
                                        /// fin datos archivo Recon

                                        break;
                                    case "02":
                                        txtboxTramaRecon.Text = String.Empty;

                                        aCampos[1] = "0200";
                                        aCampos[4] = "25";
                                        aCampos[15] = "00";
                                        //solo los 8 caracteres finales 
                                        aCampos[10] = vLinea.Substring(2, 8);
                                        aCampos[3] = vLinea.Substring(21, 16);
                                        if (vListaTarjetasBsol.Contains(aCampos[3].Substring(0, 6)))
                                            vTarjetaBSol = 1;
                                        else
                                            vTarjetaBSol = 0;
                                        aCampos[7] = vLinea.Substring(37, 8);
                                        aCampos[8] = vLinea.Substring(45, 6);
                                        aCampos[11] = vLinea.Substring(51, 15);
                                        aCampos[25] = vLinea.Substring(66, 5);
                                        aCampos[13] = vLinea.Substring(86, 3);
                                        //si de ATC llega en blanco este campo
                                        //completar el valor a Dolares y el tipo de registro.
                                        if ("   " == aCampos[13])
                                        {
                                            aCampos[13] = "840";
                                        }
                                        if ("068" == aCampos[13])
                                            aCampos[30] = "MOV";
                                        else
                                            aCampos[30] = "MI5";

                                        aCampos[24] = vLinea.Substring(89, 4);

                                        if (vLongitudLinea > 94)
                                        {
                                            //aCampos[2] = vLinea.Substring(93, 6);
                                            aCampos[28] = vLinea.Substring(93, 6);
                                        }


                                        /// datos archivo Recon
                                        vReconLinea = new StringBuilder();

                                        vPosicion = 0;
                                        vLongitud = 0;
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                            if (vLongitud > 0)
                                            {
                                                vReconLinea.Append(aCampos[i]);
                                                vPosicion = vLongitud;
                                            }
                                            vReconLinea.Append(vSeparador);

                                        }

                                        if (1 == vTarjetaBSol)
                                            vSwArchivoReconSource.WriteLine(vReconLinea.ToString());
                                        /// fin datos archivo Recon
                                        break;
                                    default:
                                        txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                        break;
                                }

                                lblNumeroRegistros.Text = vContador.ToString("00000");
                                this.Refresh();
                            }

                        }

                        vSwArchivoReconSource.Close();

                        vProcesoDeLaFecha.Append("Archivo Crucebso procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();
                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
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
            int vCantidadCamposRecon = 0;
            txtboxResultado.Text = String.Empty;

            try
            {
                String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();
                String vNomArchivoSourceBaseII = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoATCReconSourceBaseII"].ToString() + ".txt";

                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();
                vCantidadCamposRecon = int.Parse(ConfigurationManager.AppSettings["CantidadCamposRecon"]);

                GrabarLog("Inicio Proceso archivo Inctf02");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoInctf02.Text))
                {
                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoInctf02.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        StreamWriter vSwArchivoReconSourceBaseII = new StreamWriter(vNomArchivoSourceBaseII);
                        while ((vLinea = vStreamReader.ReadLine()) != null)
                        {
                            vContador++;
                            vTipoMensaje = vLinea.Substring(0, 4);
                            switch (vTipoMensaje)
                            {
                                case "0700":
                                case "0500":
                                    txtboxTramaRecon.Text = String.Empty;
                                    txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;
                                    string[] aCampos = new string[vCantidadCamposRecon + 1];

                                    aCampos[1] = "0100";
                                    aCampos[15] = "00";
                                    //aCampos[10] = "TERMIN00";
                                    if (vLinea.Substring(0, 2) == "07")
                                    {
                                        aCampos[30] = "BSO";
                                        aCampos[4] = "04";
                                    }
                                    else
                                    {
                                        aCampos[30] = "MI5";
                                        aCampos[4] = "21";
                                    }
                                    aCampos[3] = vLinea.Substring(4, 16);
                                    aCampos[16] = vLinea.Substring(49, 8);
                                    aCampos[7] = DateTime.Today.Year.ToString() + vLinea.Substring(57, 4);
                                    //aCampos[8] = "000000";
                                    aCampos[9] = DateTime.Today.Year.ToString() + vLinea.Substring(57, 4);
                                    if ("068" == vLinea.Substring(88, 3))
                                    {
                                        aCampos[11] = "000" + vLinea.Substring(76, 12);
                                        aCampos[13] = vLinea.Substring(88, 3);
                                    }
                                    else
                                    {
                                        aCampos[11] = "000" + vLinea.Substring(61, 12);
                                        aCampos[13] = vLinea.Substring(73, 3);
                                    }
                                    aCampos[22] = vLinea.Substring(91, 25);
                                    //aCampos[2] = vLinea.Substring(151, 6);
                                    aCampos[27] = vLinea.Substring(161, 2);
                                    aCampos[28] = vLinea.Substring(151, 6);
                                    /// datos archivo Recon
                                    StringBuilder vReconLinea = new StringBuilder();

                                    int vPosicion = 0;
                                    int vLongitud = 0;
                                    for (int i = 1; i <= 30; i++)
                                    {
                                        vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                        if (vLongitud > 0)
                                        {
                                            vReconLinea.Append(aCampos[i]);
                                            vPosicion = vLongitud;
                                        }
                                        vReconLinea.Append(vSeparador);
                                    }

                                    vSwArchivoReconSourceBaseII.WriteLine(vReconLinea.ToString());
                                    /// fin archivo datos Recon

                                    break;
                                default:
                                    txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                    break;
                            }

                            lblNumeroRegistros.Text = vContador.ToString("00000");
                            this.Refresh();
                        }
                        vSwArchivoReconSourceBaseII.Close();
                        vProcesoDeLaFecha.Append("Archivo Inctf02 procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();

                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
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
            String vNombreArchivoLog = vRutaBaseLog + "GeneraATCRecon" + DateTime.Today.ToString("yyyyMMdd") + ".log";
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
            String vNombreArchivoLog = vRutaBaseLog + "GeneraATCRecon" + DateTime.Today.ToString("yyyyMMdd") + ".log";
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
    }
}


// Versión de un solo Archivo Crucebso
/*
 private void ProcesarArchivoCrucebsoH2H()
        {
            Int32 vContador = 0;
            int vCantidadCamposRecon = 0;

            String vRutaBase = ConfigurationManager.AppSettings["RutaBase"].ToString();
            String vNomArchivoSink = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoATCReconSink"].ToString() + ".txt";
            String vNomArchivoSource = vRutaBase + ConfigurationManager.AppSettings["NombreArchivoATCReconSource"].ToString() + ".txt";
            String vListaTarjetasBsol = string.Empty;
            Int32 vTarjetaBSol = 1;

            try
            {
                txtboxResultado.Text = String.Empty;
                vSeparador = ConfigurationManager.AppSettings["Separador"].ToString();

                vListaTarjetasBsol = ConfigurationManager.AppSettings["TarjetasBSOL"].ToString();

                vCantidadCamposRecon = int.Parse(ConfigurationManager.AppSettings["CantidadCamposRecon"]);

                GrabarLog("Inicio Proceso archivo Crucebsoh2h");

                //validar la existencia del archivo a procesar
                if (File.Exists(txtboxRutaNomArchivoCrucebsoH2H.Text))
                {

                    using (StreamReader vStreamReader = new StreamReader(txtboxRutaNomArchivoCrucebsoH2H.Text))
                    {
                        String vLinea;
                        String vTipoMensaje = String.Empty;
                        int vLongitudLinea = 0;

                        StreamWriter vSwArchivoReconSink = new StreamWriter(vNomArchivoSink);
                        StreamWriter vSwArchivoReconSource = new StreamWriter(vNomArchivoSource);

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
                                ///CantidadCamposRecon;
                                string[] aCampos = new string[vCantidadCamposRecon + 1];
                                StringBuilder vReconLinea = new StringBuilder();
                                int vPosicion = 0;
                                int vLongitud = 0;

                                switch (vTipoMensaje)
                                {
                                    case "01":
                                    case "03":
                                        txtboxTramaRecon.Text = String.Empty;
                                        //txtboxResultado.Text = txtboxResultado.Text + vLinea + Environment.NewLine;

                                        aCampos[1] = "0200";
                                        aCampos[15] = "00";
                                        aCampos[30] = "BSO";
                                        aCampos[2] = vLinea.Substring(0, 6);
                                        aCampos[3] = vLinea.Substring(9, 16);
                                        if (vListaTarjetasBsol.Contains(aCampos[3].Substring(0, 6)))
                                            vTarjetaBSol = 1;
                                        else
                                            vTarjetaBSol = 0;

                                        aCampos[7] = vLinea.Substring(26, 8);
                                        aCampos[8] = vLinea.Substring(34, 6);
                                        aCampos[10] = vLinea.Substring(40, 8);
                                        aCampos[11] = vLinea.Substring(48, 15);
                                        aCampos[13] = vLinea.Substring(63, 3);
                                        aCampos[4] = vLinea.Substring(66, 2);
                                        aCampos[17] = vLinea.Substring(68, 3);

                                        /// datos archivo Recon
                                        vReconLinea = new StringBuilder();

                                        vPosicion = 0;
                                        vLongitud = 0;
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                            if (vLongitud > 0)
                                            {
                                                vReconLinea.Append(aCampos[i]);
                                                vPosicion = vLongitud;
                                            }
                                            vReconLinea.Append(vSeparador);

                                        }

                                        if (1 == vTarjetaBSol)
                                            vSwArchivoReconSource.WriteLine(vReconLinea.ToString());
                                        else
                                            vSwArchivoReconSink.WriteLine(vReconLinea.ToString());
                                        /// fin datos archivo Recon

                                        break;
                                    case "02":
                                        txtboxTramaRecon.Text = String.Empty;

                                        aCampos[1] = "0200";
                                        aCampos[4] = "25";
                                        aCampos[15] = "00";
                                        aCampos[30] = "MOV";
                                        //solo los 8 caracteres finales 
                                        aCampos[10] = vLinea.Substring(2, 8);
                                        aCampos[3] = vLinea.Substring(21, 16);
                                        if (vListaTarjetasBsol.Contains(aCampos[3].Substring(0, 6)))
                                            vTarjetaBSol = 1;
                                        else
                                            vTarjetaBSol = 0;
                                        aCampos[7] = vLinea.Substring(37, 8);
                                        aCampos[8] = vLinea.Substring(45, 6);
                                        aCampos[11] = vLinea.Substring(51, 15);
                                        aCampos[25] = vLinea.Substring(66, 5);
                                        aCampos[13] = vLinea.Substring(86, 3);
                                        aCampos[24] = vLinea.Substring(89, 4);
                                        
                                        if (vLongitudLinea > 94)
                                        {
                                            //aCampos[2] = vLinea.Substring(93, 6);
                                            aCampos[28] = vLinea.Substring(93, 6);
                                        }
                                        

                                        /// datos archivo Recon
                                        vReconLinea = new StringBuilder();

                                        vPosicion = 0;
                                        vLongitud = 0;
                                        for (int i = 1; i <= 30; i++)
                                        {
                                            vLongitud = int.Parse(ConfigurationManager.AppSettings["CSalida" + i.ToString("00")]);
                                            if (vLongitud > 0)
                                            {
                                                vReconLinea.Append(aCampos[i]);
                                                vPosicion = vLongitud;
                                            }
                                            vReconLinea.Append(vSeparador);

                                        }

                                        if (1 == vTarjetaBSol)
                                            vSwArchivoReconSource.WriteLine(vReconLinea.ToString());
                                        else
                                            vSwArchivoReconSink.WriteLine(vReconLinea.ToString());
                                        /// fin datos archivo Recon
                                        break;
                                    default:
                                        txtboxResultado.Text = txtboxResultado.Text + "Mensaje no reconocido" + Environment.NewLine;
                                        break;
                                }

                                lblNumeroRegistros.Text = vContador.ToString("00000");
                                this.Refresh();
                            }

                        }

                        vSwArchivoReconSink.Close();
                        vSwArchivoReconSource.Close();

                        vProcesoDeLaFecha.Append("Archivo Crucebsoh2h procesado");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.Append("Se procesaron " + vContador.ToString("00000") + " registros");
                        vProcesoDeLaFecha.AppendLine();
                        vProcesoDeLaFecha.AppendLine();
                        GrabarLog("Se procesaron " + vContador.ToString("00000") + " registros");
                        GrabarLog("");
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
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Configuration;
using System.IO;
using System.Xml;

using System.Net.Mail;

using ProxyBsolContabiliza;
using System.Xml.XPath;
using System.Net.Mime;

namespace BsolConciliacion
{
    public partial class Conciliacion : Form
    {
        StringBuilder vProcesoDeLaFecha = new StringBuilder();

        public Conciliacion()
        {
            InitializeComponent();
        }

        private void Conciliacion_Load(object sender, EventArgs e)
        {
            try
            {
                DateTime vFechaAux = DateTime.Today;
                txtboxFechaPC.Text = vFechaAux.ToString("dd/MM/yyyy");
                vFechaAux = vFechaAux.AddDays(-1);
                dtpickFechaProceso.Value = vFechaAux;

                if (1 == Program.gAutomatico)
                {
                    txtboxResultado.Text = Program.gArgumentos;

                    // Llamar a la rutina de Proceso del Archivo Extract
                    EjecutaProceso();

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

        private void btnProcesaConciliacion_Click(object sender, EventArgs e)
        {
            EjecutaProceso();
        }

        private void EjecutaProceso()
        {
            int vResultado = 0;
            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoControl = vRutaBaseLog + "ProcesoExtract" + dtpickFechaProceso.Value.ToString("yyyyMMdd") + ".xml";

            txtboxResultado.Text = txtboxResultado.Text + "Inicio Proceso" + Environment.NewLine;

            //validar la existencia del archivo a procesar
            if (!File.Exists(vNombreArchivoControl))
            {
                GrabarLog("Inicio Proceso");
                vResultado = ProcesaArchivoExtract();
            }
            else
            {
                //Si el archivo existe quiere decir que ya corrio el proceso por lo menos una vez
                //ejecutar el programa como reproceso; registrar eso en el log
                GrabarLog("El archivo de Control " + vNombreArchivoControl + " para la fecha ya existe.");
                GrabarLog("Inicio Reproceso");
                vResultado = 1;
            }

            //solo se contabiliza si el archivo XML fue correctamente generado
            if (1 == vResultado)
            {
                ContabilizaArchivoXML();
            }
            txtboxResultado.Text = txtboxResultado.Text + "Fin Proceso";
            GrabarLog("Fin Proceso");

            /*Enviar correo de Confirmación de proceso*/
            string vCorreoConfirmacion = string.Empty;
            vCorreoConfirmacion = ConfigurationManager.AppSettings["CorreoConfirmacion"].ToString();
            vCorreoConfirmacion = vCorreoConfirmacion.ToUpper();
            if (vResultado != 1)
            {
                vProcesoDeLaFecha = new StringBuilder();
                vProcesoDeLaFecha.Append("Correo de confirmación BSolContabiliza, ejecutado con Errores.");
                vProcesoDeLaFecha.AppendLine();
                vProcesoDeLaFecha.Append("Favor revisar el archivo de Log adjunto");
                vProcesoDeLaFecha.AppendLine();
            }
            //cambios 20160411
            if ("SI" == vCorreoConfirmacion)
            {
                EnviarCorreo("Proceso BSolContabiliza " + dtpickFechaProceso.Value.ToString("yyyyMMdd"),
                         vProcesoDeLaFecha.ToString());
            }

            
            /*Fin Enviar correo de Confirmación de proceso*/
        }

        private int ProcesaArchivoExtract()
        {
            int vResultado = 0;
            Dictionary<string, MovCajero> vCajerosDic = new Dictionary<string, MovCajero>();

            try
            {
                using (StreamReader vStreamReader = new StreamReader(txtboxRutaArchivo.Text))
                {
                    String vLinea;
                    String vTipoMensaje = String.Empty;
                    int vContador = 0;
                    int vLongitudLinea = 0;

                    string vTerminalID = string.Empty;
                    string vMensaje = string.Empty;
                    int vNumeroOper = 0;
                    decimal vMontoOper = 0;
                    int vMonedaOper = 0;

                    //La primera linea debe ser la cabecera 
                    if ((vLinea = vStreamReader.ReadLine()) != null)
                    {
                        vLongitudLinea = vLinea.Length; ;
                        if ((vLongitudLinea == 60) && vLinea.StartsWith("PostilionWarehouseFile"))
                        {
                            //leer la cabecera (header)
                            GrabarLog("Se leyo el registro de cabecera(header) correctamente");
                        }
                        else
                        {
                            //error al leer la primera linea, no es cabecera o el archivo es incorrecto
                            GrabarLog("El registro de cabecera(header) es incorrecto");
                            return vResultado;
                        }
                    }
                    else
                    {
                        //indicar en el log que el archivo Extract esta vacio.
                        GrabarLog("El registro de Extract de Origen esta vacio");
                        return vResultado;
                    }

                    vLongitudLinea = 0;

                    while ((vLinea = vStreamReader.ReadLine()) != null)
                    {
                        vLongitudLinea = vLinea.Length; ;
                        if (305 == vLongitudLinea)
                        {
                            //el registro es de detalle
                            vContador++;

                            vMensaje = vLinea.Substring(0, 4);

                            //terminal ID
                            vTerminalID = vLinea.Substring(55, 8);

                            MovCajero vMovCajero = new MovCajero();
                            vMovCajero.IdCajero = vTerminalID;

                            //obtener el monto
                            vMontoOper = decimal.Parse(vLinea.Substring(76, 12));
                            vNumeroOper = 1;
                            if (vMensaje == "0420")
                            {
                                //si es una reversa se cambia el signo, para que sea resta
                                vMontoOper = vMontoOper * (-1);
                            }

                            if ("068" == vLinea.Substring(113, 3))
                            {
                                vMonedaOper = 0;
                                vMovCajero.NumOperBolivianos = vNumeroOper;
                                vMovCajero.MontoBolivianos = vMontoOper;
                            }
                            else
                            {
                                vMonedaOper = 101;
                                vMovCajero.NumOperDolares = vNumeroOper;
                                vMovCajero.MontoDolares = vMontoOper;
                            }

                            try
                            {
                                vCajerosDic.Add(vTerminalID, vMovCajero);
                            }
                            catch (ArgumentException)
                            {
                                //la llave ya existe
                                //actualizar los valores de la llave
                                vMovCajero = vCajerosDic[vTerminalID];
                                vMovCajero.ActualizaCajero(vMonedaOper, vNumeroOper, vMontoOper);
                                vCajerosDic[vTerminalID] = vMovCajero;
                            }

                            lblNumeroRegistros.Text = vContador.ToString("0000");
                            this.Refresh();
                        }
                        else
                        {
                            if ((19 == vLinea.Length) && vLinea.StartsWith("NrRecords"))
                            {
                                //el registro es un registro de finalización - trailer
                                GrabarLog("Se leyo el registro de finalización (trailer) correctamente");
                            }
                            else
                            {
                                //error al leer registro del detalle
                                GrabarLog("La Longitud del registro es incorrecta");
                                return vResultado;
                            }
                        }
                    }
                }

                //Llamar al proceso que crea el archivo XML
                vResultado = GeneraArchivoXML(vCajerosDic);
                return vResultado;
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Error encontrado en la Lectura del Archivo Extract de Postilion");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
                return vResultado;
            }
        }

        private void dtpickFechaProceso_ValueChanged(object sender, EventArgs e)
        {
            DateTime vFechaAux;
            txtboxRutaArchivo.Text = ConfigurationManager.AppSettings["RutaArchivoExtract"].ToString();
            vFechaAux = dtpickFechaProceso.Value;
            txtboxRutaArchivo.Text = txtboxRutaArchivo.Text + vFechaAux.ToString("yyyyMMdd") + ".txt";
        }

        private int GeneraArchivoXML(Dictionary<string, MovCajero> vCajerosDiccionario)
        {
            //crear el archivo XML para el proceso
            int vResultado = 0;
            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoXml = vRutaBaseLog + "ProcesoExtract" + dtpickFechaProceso.Value.ToString("yyyyMMdd") + ".xml";

            try
            {
                XmlTextWriter xmlwrt = new XmlTextWriter(vNombreArchivoXml, null);
                xmlwrt.Formatting = Formatting.Indented;

                xmlwrt.WriteStartDocument();

                xmlwrt.WriteStartElement("proceso");

                foreach (KeyValuePair<string, MovCajero> kvp in vCajerosDiccionario)
                {
                    if (kvp.Value.MontoBolivianos > 0)
                    {
                        xmlwrt.WriteStartElement("asiento");

                        xmlwrt.WriteStartElement("idcajero");
                        xmlwrt.WriteValue(kvp.Value.IdCajero);
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("moneda");
                        xmlwrt.WriteValue(kvp.Value.MonedaBolivianos.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("monto");
                        xmlwrt.WriteValue(kvp.Value.MontoBolivianos.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("numoper");
                        xmlwrt.WriteValue(kvp.Value.NumOperBolivianos.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("procesado");
                        xmlwrt.WriteValue("NO");
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("fechahora");
                        xmlwrt.WriteValue(DateTime.Today.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss"));
                        xmlwrt.WriteEndElement();

                        xmlwrt.WriteEndElement();
                    }

                    if (kvp.Value.MontoDolares > 0)
                    {
                        xmlwrt.WriteStartElement("asiento");

                        xmlwrt.WriteStartElement("idcajero");
                        xmlwrt.WriteValue(kvp.Value.IdCajero);
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("moneda");
                        xmlwrt.WriteValue(kvp.Value.MonedaDolares.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("monto");
                        xmlwrt.WriteValue(kvp.Value.MontoDolares.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("numoper");
                        xmlwrt.WriteValue(kvp.Value.NumOperDolares.ToString());
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("procesado");
                        xmlwrt.WriteValue("NO");
                        xmlwrt.WriteEndElement();
                        xmlwrt.WriteStartElement("fechahora");
                        xmlwrt.WriteValue(DateTime.Today.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss"));
                        xmlwrt.WriteEndElement();

                        xmlwrt.WriteEndElement();
                    }
                }

                xmlwrt.WriteEndDocument();

                xmlwrt.Close();
                vResultado = 1;
                //fin crear el archivo XML para el proceso
                return vResultado;
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Error encontrado al generar el archivo de Proceso xml");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
                return vResultado;
            }
        }

        private int ContabilizaArchivoXML()
        {
            int vResultado = 0;
            int vAsientosNoContabilizados = 0;
            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoXml = vRutaBaseLog + "ProcesoExtract" + dtpickFechaProceso.Value.ToString("yyyyMMdd") + ".xml";
            String vNombreArchivoControl = vRutaBaseLog + "ControlExtract" + dtpickFechaProceso.Value.ToString("yyyyMMdd") + ".txt";

            try
            {
                //Preparar parametros del Web Service de Contabilizacion
                contabiliza wsContabiliza = new contabiliza();
                wsContabiliza.Url = ConfigurationManager.AppSettings["UrlWSContabiliza"].ToString();
                short vCanal = 80;
                string vUsuario = ConfigurationManager.AppSettings["ValorUsuario"].ToString(); ;
                string vClave = ConfigurationManager.AppSettings["ValorClave"].ToString(); ;
                string vIdAsiento = "POST_TDAJENAS";

                //Generar archivo de Control
                StreamWriter vStreamControl = new StreamWriter(vNombreArchivoControl,true);
                vStreamControl.WriteLine();
                vStreamControl.WriteLine("Fecha de Proceso: " + DateTime.Today.ToString("dd/MM/yyyy"));
                vStreamControl.WriteLine("Hora de Proceso: " + DateTime.Now.ToString("HH:mm:ss"));

                XmlDocument vXmlDoc = new XmlDocument();

                vXmlDoc.Load(vNombreArchivoXml);
                XPathNavigator vNav = vXmlDoc.CreateNavigator();

                XPathNodeIterator vIterator = vNav.Select("/proceso/asiento");

                while (vIterator.MoveNext())
                {
                    XPathNavigator vCopiaIterator = vIterator.Current.Clone();
                    string vXMLEntrada = @"<registro asiento=""POST_TDAJENAS""> <dato importe=""";

                    //nodo idcajero
                    vCopiaIterator.MoveToFirstChild();
                    string vIdCajero = vCopiaIterator.Value;

                    //nodo moneda
                    vCopiaIterator.MoveToNext();
                    string vMoneda = vCopiaIterator.Value;

                    //nodo monto
                    vCopiaIterator.MoveToNext();
                    string vMontoCadena = vCopiaIterator.Value;
                    decimal vMonto = decimal.Parse(vMontoCadena);

                    //nodo numoper
                    vCopiaIterator.MoveToNext();

                    //nodo procesado
                    vCopiaIterator.MoveToNext();
                    string vProcesado = vCopiaIterator.Value;

                    if (("NO" == vProcesado) && (vMonto > 0))
                    {
                        //validar que el monto sea mayor a cero
                        vXMLEntrada = vXMLEntrada + vMontoCadena;
                        vXMLEntrada = vXMLEntrada + @""" moneda =""" + vMoneda + @""" atm=""";
                        vXMLEntrada = vXMLEntrada + vIdCajero + @""" idTipo=""1"" /> </registro>";

                        string vEstado = string.Empty;
                        string vMensaje = string.Empty;
                        string vXMLSalida = string.Empty;

                        vStreamControl.WriteLine(vXMLEntrada);
                        txtboxResultado.Text = txtboxResultado.Text + vXMLEntrada;

                        wsContabiliza.Execute(vCanal, vUsuario, vClave, vIdAsiento, vXMLEntrada, ref vEstado, ref vMensaje, ref vXMLSalida);
 
                        if ("OK" == vEstado )
                        {
                            //respuesta exitosa del webservice
                            txtboxResultado.Text = txtboxResultado.Text + "Se contabilizo exitosamente";
                            vStreamControl.WriteLine("Se contabilizo exitosamente");

                            vCopiaIterator.InnerXml = "SI";

                            //nodo fechahora
                            vCopiaIterator.MoveToNext();
                            vCopiaIterator.InnerXml = DateTime.Today.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss");
                            vXmlDoc.Save(vNombreArchivoXml);
                        }
                        else
                        {
                            //respuesta con error del webservice
                            txtboxResultado.Text = txtboxResultado.Text + vMensaje + " " + vXMLSalida;
                            vStreamControl.WriteLine(vMensaje + " " + vXMLSalida);
                            vAsientosNoContabilizados++;
                        }
                    }
                }
                vStreamControl.Close();
                vResultado = 1;

                if (0 == vAsientosNoContabilizados)
                {
                    vProcesoDeLaFecha.Append("Correo de confirmación BSolContabiliza, ejecutado correctamente.");
                    vProcesoDeLaFecha.AppendLine();
                }
                else
                {
                    vProcesoDeLaFecha.Append("Correo de confirmación BSolContabiliza, ejecutado con Errores.");
                    vProcesoDeLaFecha.AppendLine();
                    vProcesoDeLaFecha.Append("No se contabilizaron ");
                    vProcesoDeLaFecha.Append(vAsientosNoContabilizados);
                    vProcesoDeLaFecha.Append(" Cajeros, favor su revisión");
                    vProcesoDeLaFecha.AppendLine();
                }

                return vResultado;
            }
            catch (Exception vExcepcion)
            {
                StringBuilder sbMensajeError = new StringBuilder();

                sbMensajeError.Append("Error encontrado al Contabilizar los registros del archivo de XML");
                sbMensajeError.AppendLine();
                sbMensajeError.Append(vExcepcion.Message);
                sbMensajeError.Append(vExcepcion.Source);
                sbMensajeError.Append(vExcepcion.StackTrace);

                if (null != vExcepcion.InnerException)
                {
                    sbMensajeError.Append(vExcepcion.InnerException.Message);
                }

                GrabarLog(sbMensajeError.ToString());
                return vResultado;
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

            //añadir el archivo de Control para adjuntar al correo.
            String vRutaBaseLog = ConfigurationManager.AppSettings["RutaArchivoControl"].ToString();
            String vNombreArchivoControl = vRutaBaseLog + "ControlExtract" + dtpickFechaProceso.Value.ToString("yyyyMMdd") + ".txt";

            //validar la existencia del archivo a Adjuntar
            if (File.Exists(vNombreArchivoControl))
            {
                Attachment vArchivoControl = new Attachment(vNombreArchivoControl, MediaTypeNames.Text.Plain);
                msgCorreo.Attachments.Add(vArchivoControl);
            }

            String vNombreArchivoLog = vRutaBaseLog + "BSOLContabiliza" + DateTime.Today.ToString("yyyyMMdd") + ".log";
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
            String vNombreArchivoLog = vRutaBaseLog + "BSOLContabiliza" + DateTime.Today.ToString("yyyyMMdd") + ".log";
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

namespace BsolConciliacion
{
    partial class Conciliacion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnProcesaConciliacion = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtboxFechaPC = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtboxRutaArchivo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblNumeroRegistros = new System.Windows.Forms.Label();
            this.txtboxResultado = new System.Windows.Forms.TextBox();
            this.dtpickFechaProceso = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // btnProcesaConciliacion
            // 
            this.btnProcesaConciliacion.Location = new System.Drawing.Point(219, 345);
            this.btnProcesaConciliacion.Name = "btnProcesaConciliacion";
            this.btnProcesaConciliacion.Size = new System.Drawing.Size(122, 23);
            this.btnProcesaConciliacion.TabIndex = 0;
            this.btnProcesaConciliacion.Text = "Procesar Conciliación";
            this.btnProcesaConciliacion.UseVisualStyleBackColor = true;
            this.btnProcesaConciliacion.Click += new System.EventHandler(this.btnProcesaConciliacion_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Fecha PC";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(317, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Fecha Proceso";
            // 
            // txtboxFechaPC
            // 
            this.txtboxFechaPC.Location = new System.Drawing.Point(98, 28);
            this.txtboxFechaPC.Name = "txtboxFechaPC";
            this.txtboxFechaPC.ReadOnly = true;
            this.txtboxFechaPC.Size = new System.Drawing.Size(100, 20);
            this.txtboxFechaPC.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Archivo";
            // 
            // txtboxRutaArchivo
            // 
            this.txtboxRutaArchivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtboxRutaArchivo.Location = new System.Drawing.Point(98, 60);
            this.txtboxRutaArchivo.Name = "txtboxRutaArchivo";
            this.txtboxRutaArchivo.ReadOnly = true;
            this.txtboxRutaArchivo.Size = new System.Drawing.Size(404, 20);
            this.txtboxRutaArchivo.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Nro.Regs.";
            // 
            // lblNumeroRegistros
            // 
            this.lblNumeroRegistros.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblNumeroRegistros.Location = new System.Drawing.Point(96, 99);
            this.lblNumeroRegistros.Name = "lblNumeroRegistros";
            this.lblNumeroRegistros.Size = new System.Drawing.Size(100, 13);
            this.lblNumeroRegistros.TabIndex = 8;
            // 
            // txtboxResultado
            // 
            this.txtboxResultado.Location = new System.Drawing.Point(96, 149);
            this.txtboxResultado.Multiline = true;
            this.txtboxResultado.Name = "txtboxResultado";
            this.txtboxResultado.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtboxResultado.Size = new System.Drawing.Size(406, 169);
            this.txtboxResultado.TabIndex = 9;
            // 
            // dtpickFechaProceso
            // 
            this.dtpickFechaProceso.Cursor = System.Windows.Forms.Cursors.Default;
            this.dtpickFechaProceso.CustomFormat = "\"dd/MM/yyyy\"";
            this.dtpickFechaProceso.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpickFechaProceso.Location = new System.Drawing.Point(402, 25);
            this.dtpickFechaProceso.Name = "dtpickFechaProceso";
            this.dtpickFechaProceso.Size = new System.Drawing.Size(100, 20);
            this.dtpickFechaProceso.TabIndex = 10;
            this.dtpickFechaProceso.ValueChanged += new System.EventHandler(this.dtpickFechaProceso_ValueChanged);
            // 
            // Conciliacion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 416);
            this.Controls.Add(this.dtpickFechaProceso);
            this.Controls.Add(this.txtboxResultado);
            this.Controls.Add(this.lblNumeroRegistros);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtboxRutaArchivo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtboxFechaPC);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnProcesaConciliacion);
            this.Name = "Conciliacion";
            this.Text = "Conciliación Tarjetas Externas";
            this.Load += new System.EventHandler(this.Conciliacion_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnProcesaConciliacion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtboxFechaPC;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtboxRutaArchivo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblNumeroRegistros;
        private System.Windows.Forms.TextBox txtboxResultado;
        private System.Windows.Forms.DateTimePicker dtpickFechaProceso;
    }
}


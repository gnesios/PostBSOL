using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BsolConciliacion
{
    class MovCajero
    {

        /// <summary>
        /// variable con la identificación del cajero automático ATM
        /// </summary>
        protected string idCajero;

        protected int monedaBolivianos;
        protected int monedaDolares;
        protected int numOperBolivianos;
        protected decimal montoBolivianos;
        protected int numOperDolares;
        protected decimal montoDolares;

        public string IdCajero
        { get {return idCajero;}
          set { idCajero = value; }
        }

        public int MonedaBolivianos
        {
            get { return monedaBolivianos; }
            set { monedaBolivianos = value; }
        }
        public int MonedaDolares
        {
            get { return monedaDolares; }
            set { monedaDolares = value; }
        }
        public int NumOperBolivianos
        {
            get { return numOperBolivianos; }
            set { numOperBolivianos = value; }
        }
        public int NumOperDolares
        {
            get { return numOperDolares; }
            set { numOperDolares = value; }
        }
        public decimal MontoBolivianos
        {
            get { return montoBolivianos; }
            set { montoBolivianos = value; }
        }
        public decimal MontoDolares
        {
            get { return montoDolares; }
            set { montoDolares = value; }
        }

        public MovCajero()
        {
            IdCajero = string.Empty;
            MonedaBolivianos = 0;
            MonedaDolares = 101;
            NumOperBolivianos = 0;
            NumOperDolares = 0;
            MontoBolivianos = 0;
            MontoDolares = 0;
        }

        public int ActualizaCajero(int monedaOper ,int numeroOper, decimal montoOper)
        {
            int resultado = 0;

            if (MonedaBolivianos == monedaOper)
            {
                MontoBolivianos = MontoBolivianos + montoOper;
                NumOperBolivianos = NumOperBolivianos + numeroOper;
            }
            else
            {
                MontoDolares = MontoDolares + montoOper;
                NumOperDolares = NumOperDolares + numeroOper;
            }

            return resultado;
        }
    }
}

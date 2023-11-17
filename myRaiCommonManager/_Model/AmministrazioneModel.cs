using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class AmministrazioneModel
    {
        public AmministrazioneModel()
        {
           
        }
        public class BustaPaga
        {

            public BustaPaga()
            {
                elencoVoci = new List<VociCedolino>();
            }
            public string matricola { get; set; }
            public string dataCompetenza { get; set; }
            public string dataContabilizzazione { get; set; }
            public string inquadramento { get; set; }
            public string intestazione { get; set; }
            public string competenza { get; set; }
            public string esito { get; set; }

            public List<VociCedolino> elencoVoci { get; set; }
            public string DtContab { get; set; }
            public string DtCompetenza { get; set; }
            public string Tipo { get; set; }
        }
        public class VociCedolino
        {
            public string Descrittiva { get; set; }
            private string _valore;

            public string Valore
            {
                get { return _valore; }
                set {
                    _valore = value;
                    if (!String.IsNullOrWhiteSpace(_valore))
                    {
                        string sDec = _valore.Substring(_valore.Length - 2, 2);
                        string sInt = _valore.Substring(0, _valore.Length - 2);

                        ValoreDec = Convert.ToDecimal(sInt) + Convert.ToDecimal(sDec) / 100;
                    }
                }
            }

            public decimal ValoreDec { get; set; }
            public TipoImporto Tipo { get; set; }
            public SezioneVoci Sezione { get; set; }
        }
        public class CedoliniPossibili
        {
            public string Codice { get; set; }
            public string Descrittiva { get; set; }

        }
        public enum TipoImporto
        {
            Credito = 1,
            Debito = 2,
            Riepilogo = 3
        }
        public enum SezioneVoci
        {
            Riepilogo = 1,
            Dettaglio = 2
        }

    }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper
{
    public class ParametriSistemaValoreJson
    {
        public string Attributo { get; set; }
        //public TipologiaAttributo TipoAttributo { get; set; }
        public string Valore1 { get; set; }
        public string Valore2 { get; set; }
    }

    public class HrisParamJson<T>
    {
        public string COD_PARAM { get; set; }
        public T COD_VALUE1 { get; set; }
        public T COD_VALUE2 { get; set; }
        public T COD_VALUE3 { get; set; }
        public T COD_VALUE4 { get; set; }

        public T[] ToArray()
        {
            return new T[] { COD_VALUE1, COD_VALUE2, COD_VALUE3, COD_VALUE4 };
        }
    }

    public enum TipologiaAttributo
    {
        /// <summary>
        /// Solo valore1 di tipo datatetime
        /// </summary>
        DataAttivazione = 1,
        /// <summary>
        /// Valore1 e valore 2 di tipo datetime
        /// </summary>
        DataAttivazioneEFine = 2,
        /// <summary>
        /// Valore1 stringa contenente elenco di matricole
        /// </summary>
        MatricoleAbilitate = 3,
        /// <summary>
        /// Valore1 stringa contenente elenco di matricole
        /// </summary>
        MatricoleEscluse = 4,
        /// <summary>
        /// valore1 booleano
        /// </summary>
        WidgetAttivo = 5,
        /// <summary>
        /// valore1 booleano
        /// </summary>
        ElementoAttivo = 6,
        /// <summary>
        /// Valore1 stringa solitamente separata da virgola
        /// contenente le tipologie di utenti
        /// </summary>
        TipologiaDipendentiAbilitati = 7,
        /// <summary>
        /// Valore1 stringa solitamente separata da virgola
        /// contenente le tipologie di utenti
        /// </summary>
        TipologiaDipendentiEsclusi = 8
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiService
{
    public class Utente
    {
        public string matricola { get; set; }
        public string nominativo { get; set; }
        public string data_nascita { get; set; }
        public string data_inizio_rapporto_lavorativo { get; set; }
        public string data_fine_rapporto_lavorativo { get; set; }
        public string data_inizio_validita { get; set; }
        public string data_fine_validita { get; set; }
        public string sede_gapp { get; set; }
        public string tipo_dipendente { get; set; }
        public string categoria { get; set; }
        public string mansione { get; set; }
        public string tipo_mansione { get; set; }
        public string tipo_minimo { get; set; }
        public string forma_contratto { get; set; }
        public bool isBoss { get; set; }
        public string grado { get; set; }
        public string codice_inail { get; set; }
        public string qualifica { get; set; }
        public bool indennita_113 { get; set; }
        public bool indennita_106 { get; set; }
        public bool indennita_107 { get; set; }
        public bool indennita_SIRIO { get; set; }
        public bool indennita_ex_33_perc { get; set; }
        public bool indennita_11B { get; set; }
        /// <summary>
        /// byte 31 - tipi dipendente T e F
        /// </summary>
        public bool forfait_straordinari { get; set; }
        /// <summary>
        /// byte 32 - tipi dipendente G e H
        /// </summary>
        public bool indennita_lavoro_notturno_124 { get; set; }
        /// <summary>
        /// byte 33 - tipi dipendente I Y Q K
        /// </summary>
        public bool di_produzione { get; set; }
        /// <summary>
        /// byte 34 - tipi dipendente T V
        /// </summary>
        public bool indennita_turni_avvicendati_153 { get; set; }
        /// <summary>
        /// byte 35 - tipi dipendente T L
        /// </summary>
        public bool indennita_mancata_limitazione_oraria_11c { get; set; }
        /// <summary>
        /// byte 36 - tipi dipendente Q I Y K
        /// </summary>
        public bool ex_art12_in_equipe_ex_33_perc { get; set; }
        /// <summary>
        /// byte 37 - 5 giorni su 7 lavorativi tipi dipendente *
        /// </summary>
        public bool settimana_corta { get; set; }
        /// <summary>
        /// byte 38 - tipi dipendente G 1 I Y N K F T L V
        /// </summary>
        public bool gestito_SIRIO { get; set; }
        /// <summary>
        /// byte 39 - tipi dipendente *
        /// </summary>
        public bool part_time { get; set; }
        /// <summary>
        /// byte 40 - numero di giorni a part time tipi dipendente *
        /// </summary>
        public string num_giorni_part_time { get; set; }
        /// <summary>
        /// byte 41 - tipi dipendente * - forma contratto 2 4 5 6 7 8 H G C
        /// </summary>
        public bool indennita_35perc_o_40perc { get; set; }
        /// <summary>
        /// byte 42 - tipi dipendente *
        /// </summary>
        public bool diritto_reperibilita { get; set; }
        /// <summary>
        /// byte 43 - tipi dipendente T L - forma contratto 7 8 H G 
        /// </summary>
        public bool forfait_straordinari_TD_95_euro { get; set; }
        /// <summary>
        /// byte 44 - tipi dipendente H G  
        /// </summary>
        public bool giornalisti_delle_reti { get; set; }
        /// <summary>
        /// byte 45 - tipi dipendente H G  
        /// </summary>
        public bool part_time_stagionale { get; set; }
        /// <summary>
        /// byte 46 - tipi dipendente H G  
        /// </summary>
        public bool videoterminalista { get; set; }
        /// <summary>
        /// byte 47 - tipi dipendente *
        /// </summary>
        public bool squadra_pronto_intervento { get; set; }
        /// <summary>
        /// byte 48 - tipi dipendente *
        /// </summary>
        public bool soggetto_a_visite_periodiche { get; set; }
        /// <summary>
        /// byte 49 - tipi dipendente A
        /// </summary>
        public bool timbrature_visibili { get; set; }
        /// <summary>
        /// byte 50 - tipi dipendente A
        /// </summary>
        public bool domanda_qualifica { get; set; }
        /// <summary>
        /// byte 51 - tipi dipendente *
        /// </summary>
        public bool utilizza_tecnologie_leggere_rip_mont_tras { get; set; }

		public bool ElementoRisposta { get; set; }

		public DateTime? DataAnzianitaFerie { get; set; }

		public string CodiceReparto { get; set; }

		public string CodiceRaggruppamentoSettore { get; set; }

		public int ProgressivoBadge { get; set; }

		public DateTime? DataRilascioBadge { get; set; }

		public string Cognome { get; set; }

		public string Nome { get; set; }

        public bool SmartWorkerAllaData { get; set; }

        public bool SmartWorkerGenerico { get; set; }
    }
}
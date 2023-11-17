using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Approvatore
{
    public class WidgetElencoRichiesteItem
    {
        public string CodiceSedeGapp { get; set; }
        public string DescrizioneSedeGapp { get; set; }
        public string Reparto { get; set; }       
        public int TotRichiesteUrgenti { get; set; }
        public int TotRichiesteScadute { get; set; }
        public int TotRichiesteOrdinarie { get; set; }
        public int TotRichieste
        {
            get
            {
                return this.TotRichiesteOrdinarie + this.TotRichiesteScadute + this.TotRichiesteUrgenti;
            }
            set { }
        }
    }

    public class WidgetElencoRichieste
    {
        public List<WidgetElencoRichiesteItem> Richieste { get; set; }
    }


    public class WidgetMioTeamItem
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Posizione { get; set; }
        public string OrarioGiornaliero { get; set; }
        public int TotRichieste { get; set; }
    }

    public class WidgetMioTeam
    {
        public List<WidgetMioTeamItem> Dipendenti { get; set; }
    }

    public class L2D_SEDE_GAPP_EXT: L2D_SEDE_GAPP
    {
        protected void Init( L2D_SEDE_GAPP obj )
        {
            this.sky_sede_gapp = obj.sky_sede_gapp;
            this.cod_sede_gapp = obj.cod_sede_gapp;
            this.desc_sede_gapp = obj.desc_sede_gapp;
            this.cod_rsu = obj.cod_rsu;
            this.desc_rsu = obj.desc_rsu;
            this.data_inizio_validita = obj.data_inizio_validita;
            this.data_fine_validita = obj.data_fine_validita;
            this.flag_ivt = obj.flag_ivt;
            this.flag_presenza_sirio = obj.flag_presenza_sirio;
            this.minimo_car = obj.minimo_car;
            this.cod_serv_cont = obj.cod_serv_cont;
            this.cod_sede = obj.cod_sede;
            this.partenza_fase_2 = obj.partenza_fase_2;
            this.partenza_fase_3 = obj.partenza_fase_3;
            this.scadenza = obj.scadenza;
            this.flg_ultimo = obj.flg_ultimo;
            this.data_inizio_val = obj.data_inizio_val;
            this.Data_Ins = obj.Data_Ins;
            this.Data_Fine_Val = obj.Data_Fine_Val;
            this.Data_Agg = obj.Data_Agg;
            this.Data_Elim = obj.Data_Elim;
            this.giorno_inizio_settimana = obj.giorno_inizio_settimana;
            this.importo_rimborso = obj.importo_rimborso;
            this.intervallo_mensa = obj.intervallo_mensa;
            this.Data_Patrono = obj.Data_Patrono;
            this.CalendarioDiSede = obj.CalendarioDiSede;
            this.intervallo_mensa_serale = obj.intervallo_mensa_serale;
            this.periodo_mensa = obj.periodo_mensa;
            this.RMTR_intervallo = obj.RMTR_intervallo;
            this.mensa_disponibile = obj.mensa_disponibile;
            this.id = obj.id;
            this.ceiton_obbligatorio = obj.ceiton_obbligatorio;
            this.eccezioni_specifiche = obj.eccezioni_specifiche;
        }

        public L2D_SEDE_GAPP_EXT ( )
        {
            L2D_SEDE_GAPP obj = new L2D_SEDE_GAPP( );
            Init( obj );
        }

        public L2D_SEDE_GAPP_EXT( L2D_SEDE_GAPP obj )
        {
            Init( obj );
        }

        public L2D_SEDE_GAPP_EXT ( L2D_SEDE_GAPP_EXT obj )
        {
            Init( obj );
            this.Reparto = obj.Reparto;
            this.DescrizioneReparto = obj.DescrizioneReparto;
        }

        public string Reparto { get; set; }
        public string DescrizioneReparto { get; set; }
    }
}
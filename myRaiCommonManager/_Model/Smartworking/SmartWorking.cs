using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace myRaiCommonManager.Model.Smartworking
{
    public enum TipoRecesso
    {
        Nessuna = 0,
        [Description("Ordinario")]
        [AmbientValue("Ordinario")]
        Ordinario = 1,
        [Description("Giustificato motivo")]
        [AmbientValue("Giustificato motivo")]
        GiustificatoMotivo = 2
    }
    public class Recesso
    {
        public Recesso()
        {

        }
        public Recesso(XR_WKF_RICHIESTE rich, bool loadByte=false)
        {
            Richiesta = rich;
            
            Tipologia = (TipoRecesso)rich.GetField<int>("Tipologia");
            Nota = rich.GetField<string>("Nota");
            RichiestaMatr = rich.GetField<string>("RichiestaMatr");
            Approvato = rich.GetField<bool?>("Approvato");
            NotaApprovazione = rich.GetField<string>("NotaApprovazione");
            ApprMatricola = rich.GetField<string>("ApprMatricola");
            ApprData = rich.GetField<DateTime?>("ApprData");
            if (loadByte) 
                ApprPdf = rich.GetTemplate("SW_Recesso", "ApprPdf");
            ApprDatiProtocollo = rich.GetField<string>("ApprDatiProtocollo");
            if (loadByte)
                RicevutaPDF = rich.GetTemplate("SW_Recesso", "RicevutaPdf");
            RicevutaData = rich.GetField<DateTime?>("RicevutaData");
            Provenienza = (ApplicationType)rich.GetField<int>("Provenienza");
        }

        public TipoRecesso Tipologia { get; set; }
        
        public string Nota { get; set; }
        public string RichiestaMatr { get; set; }


        public bool? Approvato { get; set; }
        public string NotaApprovazione { get; set; }
        public string ApprMatricola { get; set; }
        public DateTime? ApprData { get; set; }
        public byte[] ApprPdf { get; set; }
        public string ApprDatiProtocollo { get; set; }

        public byte[] RicevutaPDF { get; set; }
        public DateTime? RicevutaData { get; set; }

        public ApplicationType Provenienza { get; set; }

        public XR_WKF_RICHIESTE Richiesta { get; set; }
        public XR_STATO_RAPPORTO Rapporto { get; set; }
        public XR_MOD_DIPENDENTI Modulo { get; set; }
    }

    public class GenericKeyValue
    {
        public string text { get; set; }
        public string value { get; set; }
    }
}

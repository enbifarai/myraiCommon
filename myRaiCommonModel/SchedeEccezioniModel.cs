using myRaiData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using myRaiHelper;

namespace myRaiCommonModel
{
    public class SchedeEccezioniModel
    {
       public  List<myRaiData.MyRai_Regole_SchedeEccezioni> Schede { get; set; }
    }
    public class PopupEccezioneModel
    {
        public Boolean IsNew { get; set; }
        public PopupEccezioneModel()
        {
            allegati = new List<MyRai_Regole_Allegati>();
            List<SelectListItem> list = new List<SelectListItem>();

            var db = new digiGappEntities();
            foreach (var item in db.MyRai_Regole_TipoAssenza)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.id.ToString(),
                    Text = item.tipo_assenza,
                    Selected = false
                });
            }
            this.TipoAssenza_list= new SelectList(list, "Value", "Text");

            List<SelectListItem> listecc = new List<SelectListItem>();
            foreach (var ecc in db.MyRai_Regole_SchedeEccezioni.ValidToday().OrderBy(x=>x.codice))
            {
                listecc.Add(new SelectListItem() {
                     Text=ecc.codice,
                     Value=ecc.id.ToString()
                });
            }
            EccezioniCollegate_list = new SelectList(listecc, "Value", "Text");

           
           

            List<SelectListItem> listPosizioni = new List<SelectListItem>();
            foreach (EnumPosizioniCampo val in Enum.GetValues(typeof(EnumPosizioniCampo)))
            {
                listPosizioni.Add(new SelectListItem() { Text = val.ToString().Replace ("_"," "), Value = ((int)val).ToString() });
            }
            PosizioneCampoDinamico_list = new SelectList(listPosizioni, "Value", "Text");
            


            this.ListaDestinatari = db.MyRai_Regole_Destinatari.ToList();
            this.ListaUtenti = db.MyRai_Regole_Utenti.ToList();
            this.ListaTematiche = db.MyRai_Regole_Tematiche.ToList();

            List<SelectListItem> listEcc = new List<SelectListItem>();
            var inserite = db.MyRai_Regole_SchedeEccezioni.Select(x => x.codice.Trim()).ToList();
            foreach (var item in db.L2D_ECCEZIONE.Where (z=> 
                !z.cod_eccezione.StartsWith ("$") &&
                !z.cod_eccezione.StartsWith ("+") && 
                            ! db.MyRai_Regole_SchedeEccezioni.Select(x =>
                                x.codice.Trim())
                                    .Contains (z.cod_eccezione.Trim())))
            {
                listEcc.Add(new SelectListItem()
                {
                    Value = item.cod_eccezione,
                    Text = item.cod_eccezione,
                    Selected = false
                });
            }
             this.Eccezioni_list= new SelectList(listEcc ,"Value", "Text");


             List<SelectListItem> listTipiDoc = new List<SelectListItem>();
             foreach (var item in db.MyRai_Regole_TipiDocumentazione)
             {
                 listTipiDoc.Add(new SelectListItem() { 
                  Value=item.id.ToString(),
                   Text=item.TipoDocumentazione
                 });
             }
             this.TipoDocumentazione_list = new SelectList(listTipiDoc, "Value", "Text");
        }

        public int? IdEccezione { get; set; }

        [Required]
        public string CodiceEccezione { get; set; }

        [Required]
        public string DescrittivaEccezione { get; set; }

        public string DescrittivaLibera { get; set; }

        [Required]
        [AllowHtml]
        public string Definizione { get; set; }

        [Required]
        [AllowHtml]
        public string CriteriInserimento { get; set; }

        [Required]
        [AllowHtml]
        public string TrattamentoEconomico { get; set; }

        [Required]
        [AllowHtml]
        public string PresuppostiProcedure { get; set; }

        [Required]
        [AllowHtml]
        public string Presupposti { get; set; }


        [Required]
        public int TipoAssenza { get; set; }

        [Required]
        public SelectList TipoAssenza_list { get; set; }
        
        public string EccezioneSelezionata { get; set; }
        public string EccezioniCollegate { get; set; }
        public SelectList EccezioniCollegate_list { get; set; }

        public SelectList PosizioneCampoDinamico_list { get; set; }

        [Required]
        public int TipoDocumentazione { get; set; }
        public SelectList TipoDocumentazione_list { get; set; }

         [Required]
        public int[] tematiche { get; set; }
        [Required]
        public List<myRaiData.MyRai_Regole_Tematiche> ListaTematiche { get; set; }

        public int[] destinatari { get; set; }
        [Required]
        public List<myRaiData.MyRai_Regole_Destinatari> ListaDestinatari { get; set; }


        public int[] utenti { get; set; }
        [Required]
        public List<myRaiData.MyRai_Regole_Utenti> ListaUtenti { get; set; }


        public SelectList Eccezioni_list { get; set; }

        public string[] fonti { get; set; }
        public string[] urlfonti { get; set; }


        public string[] campodinamico { get; set; }
        public string[] valoredinamico { get; set; }
        public string[] posizionedinamico { get; set; }

         [AllowHtml]
        public string Note { get; set; }

        public List<myRaiData.MyRai_Regole_Allegati> allegati { get; set; }

        public bool Pubblicata { get; set; }

    }
}
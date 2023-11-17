using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;

namespace myRaiCommonModel.Gestionale
{
    public class DettaglioModel
    {
        public string Mission { get; set; }
        public string ValidaDal { get; set; }
        public string ChiusaIl { get; set; }
        public string Sede { get; set; }
        public string Servizio { get; set; }

        public string CodiceSezione { get; set; }
        public string DescSezione { get; set; }
        public string MissionSezione { get; set; }
        public List<myRaiDataTalentia.SINTESI1> Dipendenti { get; set; }

       public int IdSezione { get; set; }
        
        public int LivelloNodo { get; set; }

        public string LineaStaff { get; set; }
    }

    public class GestioneIncaricoModel
    {
       
        public List<myRaiDataTalentia.XR_STR_DINCARICO> IncarichiAll { get; set; }
        public myRaiDataTalentia.XR_STR_TINCARICO Incarico { get; set; }
    }
    public class GestioneSezioneModel
    {
        public myRaiDataTalentia.XR_STR_TSEZIONE Sezione { get; set; }
        public int? punteggio { get; set; }
        public int? grade { get; set; }
        public string flag_prodotto { get; set; }
        public string RamoPadre { get; set; }
        public int IdSezionePadre { get; set; }
        public string PrefNuova { get; set; }
        public List<myRaiDataTalentia.SEDE> SediContabili { get; set; }
        public List<myRaiDataTalentia. XR_TB_SERVIZIO> Servizi { get; set; }
        public List<myRaiDataTalentia. XR_STR_DAREAORG> Aree { get; set; }

        public bool IsChiusuraSezione { get; set; }
        public bool IsEliminaSezione { get; set; }
        public bool IsNuovaSezione { get; set; }
        public bool HaFigli { get; set; }
        public List<myRaiDataTalentia.XR_TB_STR_TIPOLOGIA> Tipologie { get; set; }
        public string DataEffettiva { get; set; }

        public static string GetParentFlagProdottoSupporto(int idsezione, myRaiDataTalentia.TalentiaEntities db)
        {
             var sez = db.XR_STR_TSEZIONE.Where(x => x.id == idsezione && x.data_fine_validita == "99991231").FirstOrDefault();
            if (sez == null) return null;

            if (!String.IsNullOrWhiteSpace(sez.flag_prodotto))
                return sez.flag_prodotto;

            while (true)
            {
                var itemAlbero = db.XR_STR_TALBERO.Where(x => x.id == idsezione).FirstOrDefault();

                if (itemAlbero.id == itemAlbero.subordinato_a) return null;
                if (itemAlbero == null) return null;

                var sezParent = db.XR_STR_TSEZIONE.Where(x => x.id == itemAlbero.subordinato_a && x.data_fine_validita == "99991231").FirstOrDefault();
                if (sezParent == null) return null;

                if (!string.IsNullOrWhiteSpace(sezParent.flag_prodotto))
                    return sezParent.flag_prodotto;

                idsezione = sezParent.id;
            }
        }
        public string GetRandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var stringChars = new char[2];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        public void GetSezione(int idsezione, string data, string azione, myRaiDataTalentia.TalentiaEntities db, int liv)
        {
            IsChiusuraSezione = azione == "C";
            IsEliminaSezione = azione == "D";
            IsNuovaSezione = azione == "N";

           
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            int d = Convert.ToInt32(D.ToString("yyyyMMdd"));
            
            SediContabili = db.SEDE.OrderBy(x => x.COD_SEDE).ToList();
            Servizi = db.XR_TB_SERVIZIO.OrderBy(x => x.COD_SERVIZIO).ToList();
            Aree = db.XR_STR_DAREAORG.OrderBy(x => x.CodAreaOrg).ToList();
            this.Tipologie = db.XR_TB_STR_TIPOLOGIA.OrderBy(x => x.QTA_ORDINE).ThenBy(x => x.DES_TIPOLOGIA).ToList();

            if (IsNuovaSezione)
            {
                string codiceNuovo = null;
                if (liv == 1)
                {
                    var codiciUsati = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile.Length == 2)
                        .Select(x=>x.codice_visibile.Trim()).ToList();
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    bool found = false;
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (found) break;
                        for (int j = 0; j < chars.Length; j++)
                        {
                             codiceNuovo = chars.Substring(i, 1) + chars.Substring(j, 1);
                            if ( ! codiciUsati.Contains(codiceNuovo))
                            {
                                found = true;
                                break;
                                
                            }
                        }
                    }
                    

                }
                this.Sezione = new myRaiDataTalentia.XR_STR_TSEZIONE();
                this.Sezione.data_inizio_validita = d.ToString(); ;
                this.Sezione.data_fine_validita = "99991231";
                
                var SezionePadre = db.XR_STR_TSEZIONE.Where(x => x.id == idsezione)
                                       .AsEnumerable()
                                       .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
                                       .FirstOrDefault();
                if (SezionePadre != null)
                {
                    this.RamoPadre = String.IsNullOrWhiteSpace(SezionePadre.descrizione_lunga) ? "" : SezionePadre.descrizione_lunga.ToUpper();
                    this.Sezione.codice_visibile = SezionePadre.codice_visibile.Trim();
                    if (this.Sezione.codice_visibile.Length > 5) this.Sezione.codice_visibile = this.Sezione.codice_visibile.Substring(0, 3);
                    if (codiceNuovo != null)
                    {
                        this.Sezione.codice_visibile = codiceNuovo;
                    }
                    this.Sezione.sede_contabile = SezionePadre.sede_contabile;
                    this.Sezione.servizio = SezionePadre.servizio;
                    this.Sezione.area = SezionePadre.area;
                    this.IdSezionePadre = SezionePadre.id;
                    this.Sezione.flag_prodotto = GetParentFlagProdottoSupporto(idsezione, db);
                    this.Sezione.pubblicato = SezionePadre.pubblicato;
                }
                
                return;
            }

            this.Sezione =  db.XR_STR_TSEZIONE.Where(x => x.id == idsezione)
                                        .AsEnumerable()
                                        .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
                                        .FirstOrDefault();
            var peso = db.XR_STR_PESO_SEZIONE.Where(x => x.id_sezione == idsezione && x.data_fine_validita=="99991231").FirstOrDefault();
            if (peso != null)
            {
                this.punteggio = peso.punteggio;
                this.grade = peso.grade;
            }
            var ramo=db.XR_STR_TALBERO.Where(x => x.id == idsezione).FirstOrDefault();
            if (ramo != null)
            {
                var SezionePadre= db.XR_STR_TSEZIONE.Where(x => x.id == ramo.subordinato_a)
                                       .AsEnumerable()
                                       .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
                                       .FirstOrDefault();
                if (SezionePadre != null)
                    this.RamoPadre = String.IsNullOrWhiteSpace(SezionePadre.descrizione_lunga) ? "" : SezionePadre.descrizione_lunga.ToUpper() ;
            }
            this.HaFigli = db.XR_STR_TALBERO.Any(x => x.subordinato_a == idsezione);
          
        }
    }
    public class IncarichiTreeModel
    {
        public IncarichiTreeModel(string dborig)
        {
            AlberoItems = new List<TAlberoSezioneModel>();
            Dettaglio = new DettaglioModel();
            this.DBorigine = dborig;
        }
        public List<TAlberoSezioneModel> AlberoItems { get; set; }
       

        public string DataStruttura { get; set; }

        public DettaglioModel Dettaglio { get; set; }

        public List<myRaiDataTalentia.XR_STR_TINCARICO> IncarichiSezione { get; set; }
        public List<myRaiDataTalentia.XR_STR_DINCARICO> IncarichiAll { get; set; }
         
        public string DBorigine { get; set; }

      

        public void GetModel(myRaiDataTalentia.TalentiaEntities db, string dataStruttura=null)
        {
            
            
            this.Dettaglio.Mission = db.XR_STR_TSEZIONE.Where(x => x.id == 1 && x.data_fine_validita == "99991231").Select(x => x.mission).FirstOrDefault();
          

            if (dataStruttura == null)
            {
                this.DataStruttura = DateTime.Now.ToString("dd/MM/yyyy");
                this.AlberoItems = db.XR_STR_TALBERO
                              .Join(db.XR_STR_TSEZIONE.Where(x => x.data_fine_validita == "99991231"),

                              alb => alb.id,
                              sez => sez.id,
                              (alb, sez) => new TAlberoSezioneModel
                              {
                                  area = sez.area,
                                  attivita = sez.attivita,
                                  codice_visibile = sez.codice_visibile,
                                  data_convalida = sez.data_convalida,
                                  data_fine_contabile = sez.data_fine_contabile,
                                  data_fine_validita = sez.data_fine_validita,
                                  data_formalizza = sez.data_formalizza,
                                  data_inizio_contabile = sez.data_inizio_contabile,
                                  data_inizio_validita = sez.data_inizio_validita,
                                  descrizione_breve = sez.descrizione_breve,
                                  descrizione_lunga = sez.descrizione_lunga,
                                  id = sez.id,
                                  indirizzo = sez.indirizzo,
                                  mission = sez.mission,
                                  num_ordina = sez.num_ordina,
                                  sede_contabile = sez.sede_contabile,
                                  servizio = sez.servizio,
                                  subordinato_a = alb.subordinato_a,
                                  tel_internazionale = sez.tel_internazionale,
                                  tipo_schema = alb.tipo_schema,
                                  tipologia=sez.tipo,
                                  pubblicato = sez.pubblicato

                              }).ToList();

                
            }
            else
            {
                this.DataStruttura = dataStruttura;
                DateTime D;
                DateTime.TryParseExact(dataStruttura, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
                int d = Convert.ToInt32(D.ToString("yyyyMMdd"));
                this.AlberoItems = db.XR_STR_TALBERO 
                             .Join(db.XR_STR_TSEZIONE,

                             alb => alb.id,
                             sez => sez.id,
                             (alb, sez) => new TAlberoSezioneModel
                             {
                                 area = sez.area,
                                 attivita = sez.attivita,
                                 codice_visibile = sez.codice_visibile,
                                 data_convalida = sez.data_convalida,
                                 data_fine_contabile = sez.data_fine_contabile,
                                 data_fine_validita = sez.data_fine_validita,
                                 data_formalizza = sez.data_formalizza,
                                 data_inizio_contabile = sez.data_inizio_contabile,
                                 data_inizio_validita = sez.data_inizio_validita,
                                 descrizione_breve = sez.descrizione_breve,
                                 descrizione_lunga = sez.descrizione_lunga,
                                 id = sez.id,
                                 indirizzo = sez.indirizzo,
                                 mission = sez.mission,
                                 num_ordina = sez.num_ordina,
                                 sede_contabile = sez.sede_contabile,
                                 servizio = sez.servizio,
                                 subordinato_a = alb.subordinato_a,
                                 tel_internazionale = sez.tel_internazionale,
                                 tipo_schema = alb.tipo_schema,
                                 tipologia=sez.tipo
                             }).AsEnumerable ()
                               .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d).ToList();

            }
        }

        public List<myRaiDataTalentia. SINTESI1> GetDipendenti(string codiceSezione,string sede, string dataStruttura, myRaiDataTalentia.TalentiaEntities db)
        {
            if (String.IsNullOrWhiteSpace(codiceSezione) || String.IsNullOrWhiteSpace(sede)) return new List<myRaiDataTalentia.SINTESI1>();

            
            int nsede = Convert.ToInt32(sede.Split('-')[0]);

            DateTime D;
            DateTime.TryParseExact(dataStruttura, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            int d = Convert.ToInt32(D.ToString("yyyyMMdd"));


            //var list = (
            //   from dip in db.SINTESI1
            //   join sez in db.XR_STR_TSEZIONE
            //       on dip.COD_UNITAORG equals sez.codice_visibile
            //       into l
            //   from sez in l.DefaultIfEmpty()
            //   join incarico in db.XR_STR_TINCARICO
            //      on new { idsezione = sez.id, flagresp = "1" } equals
            //      new { idsezione = incarico.id_sezione, flagresp = incarico.flag_resp }
            //      into j
            //   from incarico in j.DefaultIfEmpty()
            //   join mansione in db.XR_STR_DINCARICO
            //       on incarico.cod_incarico equals mansione.COD_INCARICO into f
            //   from mansione in f.DefaultIfEmpty()
            //   where dip.COD_UNITAORG == codiceSezione
            //   select new
            //   {
            //       dip,
            //       sez,
            //       incarico,
            //       mansione,
            //       ordine = dip.COD_TPCNTR == "9" ? 1 : dip.COD_TPCNTR == "P" ? 3 : 2,
            //       qualifica = dip.COD_QUALIFICA,
            //       ruolo = dip.COD_RUOLO
            //   }
            //  )
            //  .OrderBy(o=> new { o.ordine, o.qualifica, o.ruolo})
            //  //.ThenBy(o=>o.qualifica)
            //  //.ThenBy(o=>o.ruolo)
            //  .Select(x=> x.dip).Distinct().ToList();
            var list = db.SINTESI1
                .Where(i => i.INCARLAV_HISTORY.Any(x => x.DTA_INIZIO <= D && D<=x.DTA_FINE && x.UNITAORG.COD_UNITAORG==codiceSezione))
                //.Where(x => x.COD_UNITAORG == codiceSezione)
                .OrderBy(x => (x.COD_TPCNTR == "9" ? 1 : x.COD_TPCNTR == "P" ? 3 : 2))
                .ThenBy(x => x.COD_QUALIFICA)
                .ThenBy(x => x.COD_RUOLO).ToList();

            return list;
        }

        public void GetDettaglio(string idsezione,string dataStruttura, myRaiDataTalentia.TalentiaEntities db)
        {
            int id = 0;
            int.TryParse(idsezione, out id);

            DateTime D;
            DateTime.TryParseExact(dataStruttura, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            int d = Convert.ToInt32(D.ToString("yyyyMMdd"));

            
          

            var sezione = db.XR_STR_TSEZIONE.Where(x => x.id == id)
               .AsEnumerable()
                .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >=d)
               .FirstOrDefault();
            if (sezione != null && sezione.codice_visibile.Trim () =="0")
            {
                this.Dettaglio.DescSezione = "Logiche di funzionamento";
                this.Dettaglio.Mission = sezione.mission;
                return;
            }
            this.Dettaglio.DescSezione = (sezione != null ? sezione.descrizione_lunga : null);

            var mission = db.XR_STR_TMISSION.Where(x => x.id == id)
              .AsEnumerable()
               .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) > d)
              .FirstOrDefault();

            this.Dettaglio.IdSezione = id;

            if (mission != null && !String.IsNullOrWhiteSpace(mission.mission))
            {
                this.Dettaglio.Mission = mission.mission;
                return;
            }

            this.Dettaglio.CodiceSezione = (sezione != null ? sezione.codice_visibile : null);
          

            if (sezione != null)
            {
                this.Dettaglio.ValidaDal = GetDateFrom_yyMMdd(sezione.data_inizio_validita).ToString("dd/MM/yyyy");
                if (sezione.data_fine_validita != "99991231")
                {
                    this.Dettaglio.ChiusaIl= GetDateFrom_yyMMdd(sezione.data_fine_validita).ToString("dd/MM/yyyy");
                }
                if (!String.IsNullOrWhiteSpace(sezione.sede_contabile))
                {
                    this.Dettaglio.Sede = sezione.sede_contabile;
                    string descSede = GetSedeContabileDescr(sezione.sede_contabile,db);
                    this.Dettaglio.Sede = descSede;
                    //if (!String.IsNullOrWhiteSpace(descSede)) this.Dettaglio.Sede += " - " + descSede;
                }
               
                if (!String.IsNullOrWhiteSpace(sezione.servizio))
                {
                    this.Dettaglio.Servizio = sezione.servizio;
                    string descServizio = GetServizioDescr(sezione.servizio,db);
                    this.Dettaglio.Servizio = descServizio;
                   // if (!String.IsNullOrWhiteSpace(descServizio)) this.Dettaglio.Servizio += " - " + descServizio;
                }

                this.Dettaglio.Dipendenti = GetDipendenti(sezione.codice_visibile, sezione.sede_contabile, dataStruttura,db);
                this.Dettaglio.LineaStaff = sezione.livello;
            }
            IncarichiSezione= db.XR_STR_TINCARICO
                .AsEnumerable()
                .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d && x.id_sezione == id).ToList();
             
            IncarichiAll = db.XR_STR_DINCARICO.OrderBy(x => x.COD_INCARICO).ToList();
            
        }
        public string GetSedeContabileDescr(string sede, myRaiDataTalentia.TalentiaEntities db)
        {
           
            var s = db.SEDE.Where(x => x.COD_SEDE == sede).FirstOrDefault();
            if (s == null || s.DES_SEDE==null) return null;
            else return s.DES_SEDE.Trim();
        }
        public string GetServizioDescr(string servizio, myRaiDataTalentia.TalentiaEntities db)
        {
             
            var s = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO == servizio).FirstOrDefault();

            if (s == null || s.DES_SERVIZIO  == null) return null;
            else return s.DES_SERVIZIO.Trim();
        }

        public DateTime GetDateFrom_yyMMdd(string data)
        {
            DateTime D;
            DateTime.TryParseExact(data, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D);
            return D;
        }

       
       
      
    }



    public partial class TAlberoSezioneModel
    {
        public TAlberoSezioneModel()
        {
            Incarichi = new List<IncaricoResponsabile>();
        }
        public int id { get; set; }

        public string tipo_schema { get; set; }
        public int subordinato_a { get; set; }

        public string descrizione_breve { get; set; }
        public string data_inizio_validita { get; set; }
        public string data_inizio_contabile { get; set; }
        public string data_fine_validita { get; set; }
        public string data_fine_contabile { get; set; }
        public string codice_visibile { get; set; }
        public string data_formalizza { get; set; }
        public string data_convalida { get; set; }
        public string mission { get; set; }
        public string descrizione_lunga { get; set; }
        public Nullable<decimal> num_ordina { get; set; }
        public string attivita { get; set; }
        public string indirizzo { get; set; }
        public string area { get; set; }
        public string sede_contabile { get; set; }
        public string servizio { get; set; }
        public string tel_internazionale { get; set; }
        public string tipologia { get; set; }
        public bool pubblicato { get; set; }
        public List<IncaricoResponsabile> Incarichi { get; set; }
        public bool FirstShowing { get; set; }
        public bool HasChild { get; set; }
    }
    public class IncaricoResponsabile
    {
        public string Responsabile { get; set; }
        public string Incarico { get; set; }
        public string Matr { get; set; }
        public DateTime? DataInizioIncarico { get; set; }
    }
    public class IncarichiElencoAnagrafiche
    {
        public bool TreeSearch { get; set; }
        public int IdCampagna { get; set; }
        public DateTime Decorrenza { get; set; }
        public IEnumerable<myRaiDataTalentia.SINTESI1> anagrafiche { get; set; }
    }
}

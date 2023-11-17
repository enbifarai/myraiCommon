using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;
using myRaiService.it.rai.servizi.svildigigappws;

namespace myRaiService
{
    public class AggiungiEccezioneResponse
    {
        public string NumDoc { get; set; }
        public string Error { get; set; }
        public updateResponse response { get; set; }
    }
  

    public class Eccezioni
    {
        public static AggiungiEccezioneResponse aggiungiEccezione(int IDrichiesta)
        {
           // InserimentoEccezioneModel model = new InserimentoEccezioneModel();
            var db = new digiGappEntities();
            MyRai_Richieste richiesta = db.MyRai_Richieste.Where(x => x.id_richiesta == IDrichiesta).FirstOrDefault();
            if (richiesta == null || richiesta.MyRai_Eccezioni_Richieste ==null || richiesta.MyRai_Eccezioni_Richieste.Count==0)
                return new AggiungiEccezioneResponse() {  Error="Record non trovato id:" + IDrichiesta };
            
            MyRai_Eccezioni_Richieste eccezione_richiesta = richiesta.MyRai_Eccezioni_Richieste.First();

            string matricola = richiesta.matricola_richiesta;

            DateTime dataDa = richiesta.periodo_dal;
            DateTime dataA = richiesta.periodo_al;

            string cod = eccezione_richiesta.cod_eccezione.PadRight(4, ' ');

            string dalle = eccezione_richiesta.dalle == null ? "" : calcolaMinuti(((DateTime)eccezione_richiesta.dalle).ToString("HH:mm")).ToString().PadLeft(4, '0');
            string alle = eccezione_richiesta.alle == null ? "" : calcolaMinuti(((DateTime)eccezione_richiesta.alle).ToString("HH:mm")).ToString().PadLeft(4, '0');

            string quantita = "1";
            if (!String.IsNullOrWhiteSpace(dalle) && !String.IsNullOrWhiteSpace(alle))
                quantita = CalcolaQuantitaOreMinuti(dalle, alle);

            //int wf= EccezioniManager.GetWorkflow(model.cod_eccezione);

           

            try
            {
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = new System.Net.NetworkCredential( GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                };

                //ottieni lo status attuale della giornata
                var resp = service.getEccezioni(matricola, dataDa.ToString("ddMMyyyy"), "BU", 70);

                string oldData = resp.data.Substring(142, 62) + resp.giornata.tipoDipendente;
                // se eccez di tipo C, possibile una sola al giorno
                string tipoEccezione = GetTipoEccezione(cod);
                if (tipoEccezione != null && tipoEccezione.Trim() == "C")
                {
                    if (resp.eccezioni.Any(x => x.cod != null && x.cod.Trim().ToUpper() == cod.Trim().ToUpper()))
                    {
                        return new AggiungiEccezioneResponse() { Error = "Tipo di inserimento già presente in questa data" };
                    }
                }

                int livUtente = 70;
                //if (Utente.IsBoss() && wf==(int)EnumWorkflows.SelfService) livUtente = 81;

                updateResponse Response = service.updateEccezione(
                    matricola,
                    dataDa.ToString("ddMMyyyy"),
                    cod,            //cod
                    oldData,        //
                    quantita,       //formato 01:30
                    dalle,          //formato int minuti
                    alle,           //formato int minuti
                    "",             //importo
                    ' ',            //storno
                    "",             //ndoc
                    "",             //note
                    "",             //uorg
                    "",             //df
                    "",             //ms
                    "",             //orario teorico
                    "",             //orario reale
                    livUtente);     //liv utente

                if (Response.esito == true && Response.codErrore == "0000")
                {
                    //se ok recupera ndoc e rispondi
                    AggiungiEccezioneResponse r = new AggiungiEccezioneResponse()
                    {
                        Error = null,
                        response = Response
                    };

                    int[] numdocPosition = GetParametri<int>(EnumParametriSistema.PosizioneNumDoc);

                    if (!String.IsNullOrWhiteSpace(Response.rawInput)
                            && Response.rawInput.Length > numdocPosition[0] + numdocPosition[1])
                    {
                        r.NumDoc = Response.rawInput.Substring(numdocPosition[0], numdocPosition[1]);
                    }
                    int numdoc;
                    bool b = int.TryParse(r.NumDoc, out numdoc);
            
                    if (b == false)
                    {
                        var err = new MyRai_LogErrori()
                        {
                            applicativo = "MYRAISERVICE",
                            data = DateTime.Now,
                            error_message = "Ndoc non presente in risposta " + r.response.rawInput,
                            matricola = matricola,
                            provenienza = "myRaiService .aggiungiEccezione"
                        };
                        db.MyRai_LogErrori.Add(err);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        var rich = db.MyRai_Richieste.Where(x => x.id_richiesta == IDrichiesta).FirstOrDefault();
                        if (rich != null)
                        {
                            var ec = rich.MyRai_Eccezioni_Richieste.FirstOrDefault();
                            if (ec != null)
                            {
                                ec.numero_documento = numdoc;
                                db.SaveChanges();
                            }
                        }
                    }
                    return r;
                }
                else
                    return new AggiungiEccezioneResponse() { Error = Response.codErrore + ": " + Response.descErrore };
            }
            catch (Exception ex)
            {
                return new AggiungiEccezioneResponse() { Error = ex.Message + "-" + ex.InnerException };
            }
        }
        public static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            using (digiGappEntities db = new digiGappEntities())
            {
                String NomeParametro = chiave.ToString();
                MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                if (p == null) return null;
                else
                {
                    T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                    return parametri;
                }
            }
        }
        public static string GetTipoEccezione(string cod)
        {
            var db = new digiGappEntities();
            var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == cod.Trim()).FirstOrDefault();
            if (e != null)
                return e.flag_eccez;
            else
                return null;
        }
        public static string CalcolaQuantitaOreMinuti(string dallemin, string allemin)
        {
            int dalle = Convert.ToInt32(dallemin);
            int alle = Convert.ToInt32(allemin);
            int diff = alle - dalle;
            int h = (int)diff / 60;
            int min = diff - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
        public static int calcolaMinuti(DateTime D)
        {
            int minuti = (D.Hour * 60) + D.Minute;
            return minuti;
        }

        public static int calcolaMinuti(string orarioHHMM)
        {
            int minuti = 0;
            string[] array = new string[2];

            if (orarioHHMM.IndexOf(':') > 0)
                array = orarioHHMM.Split(':');
            else
            {
                array[0] = orarioHHMM.Substring(0, 2);
                array[1] = orarioHHMM.Substring(2, 2);
            }

            minuti = Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);

            return minuti;
        }
    }
    public enum EnumParametriSistema
    {
        MaxRowsVisualizzabiliDaApprovare,
        AccountUtenteServizio,
        MatricolaSimulata,
        MailApprovazioneSubject,
        MailApprovazioneFrom,
        MailApprovazioneTemplate,
        MailRifiutaTemplate,
        UrlImmagineDipendente,
        OrariGapp,
        ValidazioneGenericaEccezioni,
        RowsPerScroll,
        LimiteMesiBackPerEvidenze,
        MessaggioAssenteIngiustificato,
        PosizioneNumDoc,
        TipiDipQuadraturaSettimanale,
        GestisciDateSuDocumenti,
        OreRichiesteUrgenti,
        SovrascritturaTipoDipendente,
        CodiceCSharp,
        IgnoraAssenzeIngiustificatePerMatricole,
        GiornoEsecuzioneBatchPDF,
        IntervalloBatchSecondi

    }

     
}
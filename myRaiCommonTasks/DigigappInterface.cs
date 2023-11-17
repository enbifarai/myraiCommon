using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
    public class DigigappInterface
    {
        public static string GetNominativoPerMatricola(string matricola)
        {
            try
            {
                MyRaiServiceInterface.it.rai.servizi.hrgb.Service s = new MyRaiServiceInterface.it.rai.servizi.hrgb.Service();
                string r = s.EsponiAnagrafica("raicv;" + matricola + ";;E;0");
                return r.Split(';')[1] + " " + r.Split(';')[2];
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static Boolean IsGestitoSirio(string matricola, DateTime data)
        {

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
                                                             CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp response = cl.recuperaUtente( matricola.PadLeft( 7 , '0' ) , data.ToString( "ddMMyyyy" ) );

            string sediGestiteSirio = CommonTasks.GetParametro<string>(CommonTasks.EnumParametriSistema.SediEffettivamenteGestiteSirio);

            List<string> sedi = new List<string>( );
            List<string> sediTemp = sediGestiteSirio.Split( ',' ).ToList( );
            if ( sediTemp != null && sediTemp.Any())
            {
                foreach(var s in sediTemp)
                {
                    string sede = s.Replace( "\r" , "" );
                    sede = sede.Replace( "\n" , "" );
                    sede = sede.Trim( );
                    sedi.Add( sede );
                }
            }

            bool sedeContain = sedi.Contains( response.data.sede_gapp.Trim( ) );

            return response.data.gestito_SIRIO && !String.IsNullOrWhiteSpace(sediGestiteSirio) && sedeContain;
        }
        public static int[] GetLivelli(string matricola, string sedegapp)
        {
            string[] responseDB = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCachedNolevel);
            it.rai.servizi.HRGA.CategorieDatoAbilitate response = Newtonsoft.Json.JsonConvert.DeserializeObject<it.rai.servizi.HRGA.CategorieDatoAbilitate>(responseDB[0]);
            Abilitazioni AB = new Abilitazioni();

            foreach (var item in response.CategorieDatoAbilitate_Array)
            {
                AbilitazioneSede absede = new AbilitazioneSede()
                {
                    Sede = item.Codice_categoria_dato,
                    DescrSede = item.Descrizione_categoria_dato
                };
                foreach (System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows)
                {
                    if (row["codice_sottofunzione"].ToString() == "01GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello1.Add(ms);
                    }
                    if (row["codice_sottofunzione"].ToString() == "02GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello2.Add(ms);
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }

            List<int> Livelli = new List<int>();
            var list_L1 = AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Any(z => z.Matricola == "P" + matricola)).Select(x => x.Sede).ToList();
            if (list_L1.Contains(sedegapp)) Livelli.Add(1);

            var list_L2 = AB.ListaAbilitazioni.Where(x => x.MatrLivello2.Any(z => z.Matricola == "P" + matricola)).Select(x => x.Sede).ToList();
            if (list_L2.Contains(sedegapp)) Livelli.Add(2);

            return Livelli.ToArray();
        }

        public static Boolean IsInDB(string codiceEccezione, DateTime data, string matricola)
        {
            var db = new digiGappEntities();
            return db.MyRai_Eccezioni_Richieste.Any(x => x.data_eccezione == data && x.cod_eccezione == codiceEccezione && x.MyRai_Richieste.matricola_richiesta == matricola);
        }
        public Boolean IsInGapp(string codiceEccezione, DateTime data, string matricola)
        {
            WSDigigapp serviceWS = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
              CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var resp = serviceWS.getEccezioni(matricola, data.ToString("ddMMyyyy"), "BU", 70);
            return resp != null && resp.eccezioni != null && resp.eccezioni.Any(x => x.cod.Trim() == codiceEccezione);
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

        public static AggiungiEccezioneResponse updateEccezione(WSDigigapp service,
                    string matricola,
                    string data,
                    string cod,
                    string quantita,
                    string dalle,
                    string alle,
                    string importo,
                    char storno,
                    string ndoc,
                    string note,
                    string uorg,
                    string df,
                    string ms,
                    string new_orario_teorico,
                    string new_orario_reale,
                    int livelloUtente)
        {

            var respEcc= service.getEccezioni(matricola, data, "BU", 70);
            if (respEcc.giornata.eccezioni.Any(x => x.cod.Trim() == cod.Trim()))
                return new AggiungiEccezioneResponse() { Error="Eccezione già presente alla data richiesta" };


            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var db = new digiGappEntities();
            var az=(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "BATCH",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Request",
                provenienza = " Batch DigigappInterface.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "UpdateEccezione matr:'" + matricola + "' data:'" + data + "' cod:'" + cod 
                + "' quantita:'" + quantita + "' dalle:'" + dalle + "'" +
                " alle:'" + alle + "' importo:'" + importo + "' storno:'" + storno + "' ndoc:'" + ndoc + "' note:'" + note +
                "' uorg:'" + uorg + "' df:'" + df + "' ms:'" + ms + "' new_orario_teorico:'" + new_orario_teorico +
                "' new_orario_reale:'" + new_orario_reale + "' livUtente:'" + livelloUtente + "'"
            });
            db.MyRai_LogAzioni.Add(az);
            db.SaveChanges();
            
            string oldData = respEcc.data.Substring(142, 62) + respEcc.giornata.tipoDipendente;

            updateResponse response = new updateResponse();
            
            try
            {
                response = service.updateEccezione(
                             matricola,
                             data,
                             cod,
                             oldData,
                             quantita,
                             dalle,
                             alle,
                             importo,
                             storno,
                             ndoc,
                             note,
                             uorg,
                             df,
                             ms,
                             new_orario_teorico,
                             new_orario_reale,
                             livelloUtente);

            }
            catch (Exception ex)
            {
                var err= new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "BATCH",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "Batch DigigappInterface.updateEccezione [" + timestamp + "]",
                    error_message = ex.ToString()
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                response.esito = false;
                response.descErrore = ex.Message;
                return new AggiungiEccezioneResponse() { response=response, Error=ex.Message };
            }


           

            var azione= new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "BATCH",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Response ",
                provenienza = "Batch DigigappInterface.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.descErrore + " Raw:" + response.rawInput
            };
            db.MyRai_LogAzioni.Add(azione);
            db.SaveChanges();
            return null;
        }

        public static string AddEccezione(int idPianoFerieBatch)
        {
            var db = new digiGappEntities();
            var PFbatch = db.MyRai_PianoFerieBatch.Where(x => x.id == idPianoFerieBatch).FirstOrDefault();
            CommonTasks.Log("ID PianoFerieBatch non trovato " + idPianoFerieBatch);
            if (PFbatch == null) return "NOT FOUND";

            if (IsInDB(PFbatch.codice_eccezione, PFbatch.data_eccezione, PFbatch.matricola))
            {
                string l = "Già nel DB: " + PFbatch.matricola + " " + PFbatch.data_eccezione.ToString("dd/MM/yyyy") + " " + PFbatch.codice_eccezione;
                CommonTasks.Log(l);
                return l;
            }
            WSDigigapp serviceWS = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
               CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1])
            };

            List<DateTime> DateContigue = new List<DateTime>() { PFbatch.data_eccezione };

            DateTime D = new DateTime(PFbatch.data_eccezione.Year, PFbatch.data_eccezione.Month, PFbatch.data_eccezione.Day);

            while (true)
            {
                D = D.AddDays(1);
                if (db.MyRai_PianoFerieBatch.Any(x => x.matricola == PFbatch.matricola && x.data_eccezione == D && x.codice_eccezione==PFbatch.codice_eccezione)
                    && !IsInDB(PFbatch.codice_eccezione, D, PFbatch.matricola))
                {
                    DateContigue.Add(D);
                }
                else
                    break;
            }
            Debug.Print(DateContigue.Min().ToShortDateString() + "/" + DateContigue.Max().ToShortDateString());


            myRaiData.MyRai_Richieste Richiesta = new myRaiData.MyRai_Richieste();
            Richiesta.codice_sede_gapp = PFbatch.sedegapp.Substring(0, 5);
            if (PFbatch.sedegapp.Length > 5)
                Richiesta.reparto = PFbatch.sedegapp.Substring(6);
            else
                Richiesta.reparto = "";
            Richiesta.data_richiesta = DateTime.Now;
            Richiesta.id_tipo_richiesta = 1;
            Richiesta.matricola_richiesta = PFbatch.matricola;
            Richiesta.periodo_dal = DateContigue.Min();
            Richiesta.periodo_al = DateContigue.Max();
            Richiesta.nominativo = GetNominativoPerMatricola(PFbatch.matricola);
            Richiesta.id_stato = 20;
            Richiesta.urgente = false;
            Richiesta.scaduta = false;
            Richiesta.richiesta_in_resoconto = false;
            Richiesta.gestito_sirio = IsGestitoSirio(PFbatch.matricola, PFbatch.data_eccezione);

            int[] Livelli = GetLivelli(PFbatch.matricola, PFbatch.sedegapp.Substring(0, 5));
            Richiesta.richiedente_L1 = Livelli.Contains(1);
            Richiesta.richiedente_L2 = Livelli.Contains(2);

            foreach (DateTime d in DateContigue)
            {
                MyRai_Eccezioni_Richieste eccRich = new myRaiData.MyRai_Eccezioni_Richieste()
                {
                    alle = null,
                    azione = "I",
                    codice_sede_gapp = PFbatch.sedegapp.Substring(0, 5),
                    cod_eccezione = PFbatch.codice_eccezione,
                    dalle = null,
                    data_creazione = DateTime.Now,
                    data_eccezione = d,
                    id_stato = 20,
                    importo = null,
                    quantita = 1,
                    motivo_richiesta = "PIANO FERIE",
                    tipo_eccezione = GetTipoEccezione(PFbatch.codice_eccezione)
                };
                var respws = serviceWS.getEccezioni(PFbatch.matricola, d.ToString("ddMMyyyy"), "BU", 80);
                if (respws != null && respws.giornata != null)
                    eccRich.turno = respws.giornata.orarioReale;



            }

            var response = updateEccezione(serviceWS, PFbatch.matricola, PFbatch.data_eccezione.ToString("ddMMyyyy"), PFbatch.codice_eccezione, PFbatch.quantita, 
                PFbatch.dalle, PFbatch.alle, PFbatch.importo, ' ', "", "","", "", "", "", "", 82);


            return null;
        }

    }
    public class AggiungiEccezioneResponse
    {
        public string NumDoc { get; set; }
        public string Error { get; set; }
        public updateResponse response { get; set; }
    }
}
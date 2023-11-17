using myRaiData;
using myRaiServiceHub.Autorizzazioni;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace myRaiHelper
{
    public class BatchManager
    {
        public enum Quadratura
        {
            Giornaliera,
            Settimanale
        }

        public static Boolean GetLevel1Boss(string matricola)
        {
            Sedi SEDI = new Sedi();
            SEDI.Credentials = CommonHelper.GetUtenteServizioCredentials();
            string pmatricola = "P" + matricola;
            string strAuth = SEDI.autorizzazioni(pmatricola + ";HRUP;;;;E;0");
            string[] autorizzazioni = strAuth.Split(';');
            bool BossL1 = (autorizzazioni != null && autorizzazioni.Length > 3 &&
                                 autorizzazioni[0] == "01" && autorizzazioni[1] == "01" &&
                                 autorizzazioni[3] != null && autorizzazioni[3].ToUpper().Contains("01GEST"));
            return BossL1;
        }

        public static Utente GetUserData(string matricola, DateTime? D=null)
        {
            Utente result = new Utente();
            if (D == null) D = DateTime.Now;

            string[] valori = GetParametri<string>(EnumParametriSistema.OrariGapp);

            MyRaiService1Client wcf1 = new MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
            try
            {
                var resp = wcf1.GetRecuperaUtenteNoSession(matricola, D.Value.ToString("ddMMyyyy"));
                if (resp.data != null)
                    resp.data.isBoss = GetLevel1Boss(matricola);

                result = resp.data;
            }
            catch (Exception ex)
            {
            }
            
            return result;
        }

        public static bool IsGestitoSirio ( string matricola )
        {
            Utente utente = new Utente( );

            string[] valori = GetParametri<string>( EnumParametriSistema.OrariGapp );

            MyRaiService1Client wcf1 = new MyRaiService1Client( );
            wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
            try
            {
                var resp = wcf1.GetRecuperaUtenteNoSession( matricola , DateTime.Today.ToString( "ddMMyyyy" ) );
                if ( resp.data != null )
                    resp.data.isBoss = GetLevel1Boss( matricola );

                utente = resp.data;
                string sedeGAPP = BatchManager.SedeGapp( utente );

                string sediGestiteSirio = CommonHelper.GetParametro<string>( EnumParametriSistema.SediEffettivamenteGestiteSirio );
                if ( !String.IsNullOrWhiteSpace( sediGestiteSirio ) && !String.IsNullOrWhiteSpace( sedeGAPP ) )
                {
                    if ( sediGestiteSirio.ToUpper( ).Split( ',' ).Contains( sedeGAPP.ToUpper( ) ) )
                    {
                        utente.gestito_SIRIO = true;
                    }
                }
            }
            catch ( Exception ex )
            {
            }

            return utente.gestito_SIRIO;
        }

        public static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            var db = new digiGappEntities();
            String NomeParametro = chiave.ToString();

            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
            if (p == null) return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                return parametri;
            }
        }

        public static T GetParametro<T>(EnumParametriSistema chiave)
        {
            using (digiGappEntities db = new digiGappEntities())
            {
                String NomeParametro = chiave.ToString();
                MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                if (p == null) return default(T);
                else return (T)Convert.ChangeType(p.Valore1, typeof(T));
            }
        }

        public static Boolean L1propriaSede(Utente utente)
        {
            string rep = "";
            if (utente.CodiceReparto != null && utente.CodiceReparto.Trim() != "" && utente.CodiceReparto != "00") rep = utente.CodiceReparto;

            if (utente.isBoss)
                return GetSediL1(utente.matricola).Contains(SedeGapp(utente) + rep);
            else
                return false;
        }

        public static Boolean L2propriaSede(Utente utente)
        {
            if (IsBossLiv2(utente.matricola))
                return GetSediL2(utente.matricola).Contains(SedeGapp(utente));
            else
                return false;
        }

        public static List<string> GetSediL1(string matricola)
        {
            string matr = "P" + matricola;
            Abilitazioni ab = getAbilitazioni( matricola );
            return ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello1.Any(z => z.Matricola == matr))
                    .Select(a => a.Sede).ToList();
        }

        public static List<string> GetSediL2(string matricola)
        {
            string matr = "P" + matricola;
            Abilitazioni ab = getAbilitazioni( matricola );
            return ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello2.Any(z => z.Matricola == matr))
                    .Select(a => a.Sede).ToList();
        }

        public static DateTime? DataInizioValidita(Utente utente)
        {
            DateTime d;
            if (DateTime.TryParseExact(utente.data_inizio_validita, "yyyyMMdd", null,
                        System.Globalization.DateTimeStyles.None, out d))
                return d;
            else
                return null;
        }

        public static string SedeGapp(Utente utente)
        {
            DateTime data = DateTime.Now;

            if (DataInizioValidita(utente) != null && data < DataInizioValidita(utente))
            {
                MyRaiService1Client wcf1 = new MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                var resp = wcf1.GetRecuperaUtente(utente.matricola, data.ToString("ddMMyyyy"));
                return resp.data.sede_gapp;
            }
            else
            {
                return utente.sede_gapp;
            }
        }

        public static Boolean IsBossLiv2(string matricola)
        {
            List<string> s = GetSediL2(matricola);
            return (s != null && s.Count > 0);
        }
        public static List<string> GetSediAbilitateMessaggiSegreteria(string matricola)
        {
            List<string> Lsedi00 = GetSediSegreteria(matricola);
            List<string> Lsedi20 = GetSediUffPers(matricola);

            Lsedi00.AddRange(Lsedi20);
            return Lsedi00.Distinct().ToList().OrderBy(x=>x).ToList();
        }
        public static List<string> GetSediSegreteria(string matricola)
        {
            Abilitazioni AB00 = GetAbilitazioniSeg();
            List<string> Lsedi00 = AB00.ListaAbilitazioni
                .Where(x => x.MatrLivello1 != null && x.MatrLivello1.Any(z => z.Matricola == "P"+matricola))
                .Select(x => x.Sede).ToList();
            return Lsedi00;
        }
        public static List<string> GetSediUffPers(string matricola)
        {
            Abilitazioni AB20 = GetAbilitazioniUffPers();
            List<string> Lsedi20 = AB20.ListaAbilitazioni
               .Where(x => x.MatrLivello1 != null && x.MatrLivello1.Any(z => z.Matricola == "P"+matricola))
               .Select(x => x.Sede).ToList();
            return Lsedi20;
        }

        public static Abilitazioni GetAbilitazioniSeg()
        {
            Sedi service = new Sedi();
            service.Credentials = new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                                                                    GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            CategorieDatoAbilitate response =
                CommonHelper.Get_CategoriaDato_Net_Cached_Liv00();

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
                    if (row["codice_sottofunzione"].ToString() == "00GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello1.Add(ms);
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }
                return AB;
        }
        public static Abilitazioni GetAbilitazioniUffPers()
        {
            Sedi service = new Sedi();
            service.Credentials = new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                                                                    GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            CategorieDatoAbilitate response =
                CommonHelper.Get_CategoriaDato_Net_Cached_Liv20();

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
                    if (row["codice_sottofunzione"].ToString() == "20GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello1.Add(ms);
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }
            return AB;
        }

        public static Abilitazioni getAbilitazioni(string matricola = null)
        {
            Sedi service = new Sedi();
            service.Credentials = CommonHelper.GetUtenteServizioCredentials();

            CategorieDatoAbilitate response =
                CommonHelper.Get_CategoriaDato_Net_Cached(0, true, matricola );

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
                    if ( row["codice_sottofunzione"].ToString( ) == "03GEST" )
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza( );
                        ms.Matricola = row["logon_id"].ToString( );
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString( );
                        ms.Delegante = row["Delegante"].ToString( );
                        ms.Delegato = row["Delegato"].ToString( );
                        absede.MatrLivello3.Add( ms );
                    }
                    if ( row["codice_sottofunzione"].ToString( ) == "04GEST" )
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza( );
                        ms.Matricola = row["logon_id"].ToString( );
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString( );
                        ms.Delegante = row["Delegante"].ToString( );
                        ms.Delegato = row["Delegato"].ToString( );
                        absede.MatrLivello4.Add( ms );
                    }
                    if ( row["codice_sottofunzione"].ToString( ) == "05GEST" )
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza( );
                        ms.Matricola = row["logon_id"].ToString( );
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString( );
                        ms.Delegante = row["Delegante"].ToString( );
                        ms.Delegato = row["Delegato"].ToString( );
                        absede.MatrLivello5.Add( ms );
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }
            AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();

            return AB;
        }

        public static string TipoDipendente(Utente utente)
        {
            var db = new digiGappEntities();
            string s = EnumParametriSistema.SovrascritturaTipoDipendente.ToString();
            string u = "";
            var tipo = db.MyRai_ParametriSistema.Where(x => x.Chiave == s && x.Valore1 == u).FirstOrDefault();
            if (tipo != null) return tipo.Valore2;

            return utente.tipo_dipendente;
        }

        public static Quadratura? GetQuadratura(Utente utente)
        {
            string tipi = GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);
            if (tipi != null && TipoDipendente(utente) != null && tipi.ToUpper().Contains(TipoDipendente(utente).ToUpper()))
                return Quadratura.Settimanale;

            string tipiGiorn = GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);
            if (tipiGiorn != null && TipoDipendente(utente) != null && tipiGiorn.ToUpper().Contains(TipoDipendente(utente).ToUpper()))
                return Quadratura.Giornaliera;

            return null;

        }

        public static bool IsAbilitatoGapp ( string matricola )
        {
            Utente utente = GetUserData( matricola,DateTime.Now );
            string sedeGapp = SedeGapp( utente );
            
            CategorieDatoAbilitate response = CommonHelper.Get_CategoriaDato_Net_Cached( 1 );

            List<string> list = new List<string>( );
            foreach ( DataRow item in response.DT_CategorieDatoAbilitate.Rows )
            {
                if ( item != null && item["cod"] != null && item["cod"].ToString( ) != "" )
                {
                    list.Add( item["cod"].ToString( ).Trim( ) );
                }
            }
            string rep = utente.CodiceReparto;
            Boolean Abilitato = false;
            if ( rep != null && rep.Trim( ) != "" && rep.Trim( ) != "0" && rep.Trim( ) != "00" )
                Abilitato = list.Contains( sedeGapp ) || list.Contains( sedeGapp + rep );
            else
                Abilitato = list.Contains( sedeGapp );

            return Abilitato;
        }


    }
}
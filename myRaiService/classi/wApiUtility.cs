using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;
using myRaiService.classi;
using System.Reflection;


namespace myRaiService
{
    public class wApiUtility
    {

        public wApiUtility()
        {
            //
            // TODO: aggiungere qui la logica del costruttore
            //
        }


        #region strutture

        public struct updateResponse
        {
            public bool esito;
            public string codErrore;
            public string descErrore;
            //freak - rawInput
            public string rawInput;
        }


        public struct presenzeGiornaliere_resp
        {
            public bool esito;
            public string errore;
            public List<presentiGiornaliere> dati;
           
        }

        public struct presentiGiornaliere
        {
            public string matricola;
            public string nominativo;
            public string tipoDip;
            public string codiceOrario;
            public string formaContratto;
            public string CodTeminalePrimaEntrata;
            public string CodTeminaleUltimaEntrata;
            public string CodTeminaleUltimaUscita;
            public string OrarioPrimaEntrata;
            public string OrarioUltimaEntrata;
            public string OrarioUltimaUscita;
            public string EccezioneUno;
            public string EccezioneDue;
            public string EccezioneTre;
            public string CodiceReparto;
            public myRaiData.L2D_ORARIO DefinizioniOrario;
        }

        public struct presenzeSettimanali_resp
        {
            public bool esito;
            public string errore;
            public presenzeSettimanali dati;
            public string rispostaCics;
        }

        public struct presenzeSettimanali
        {
            public string dataDa;
            public string dataA;
            public string SedeGAPP;
            public string deltaCarenze;
            public string deltaMaggioriPresenze;
            public string deltaTotale;
            public List<presenzeSettimanali_Item> items;
        }


        public struct presenzeSettimanali_Item
        {
            public DateTime data;
            public string giornoSettimana;
            public string orarioReale;
            public string descOrarioReale;
            public string MacroAssenza1;
            public string MacroAssenza2;
            public string MacroAssenza3;
            public string carenza;
            public string maggiorPresenza;
            public bool assenzaIngiustificata;
            public bool transitoSfasato;
        }


        public struct cicsResponse
        {
            public bool esito;
            public bool richiama;
            public int lunghezza;
            public string codErrore;
            public string descErrore;
        }

        //debug - aggiunta campo di debug risposta
        public struct dipendente_resp
        {
            public bool esito;
            public string errore;
            public Utente data;
            //public string rawOutput; //debug
        }

        public struct riepilogoSedeGappResponse
        {
            public bool esito;
            public bool esitoCics;
            public bool esitoWS;
            public string errore;
            public int nrStampa;
            public string sedeGapp;
            public string descSedeGapp;
            public string dataDa;
            public string dataA;
            public List<Eccezione> eccezioni;
            public byte[] PDF;
            public int ID;

            //debug
            public string raw;
        }



        public static string comti_decResponse(string sCics)
        {
            try
            {
                switch (sCics)
                {
                    case "91":
                        {
                            return "Dati momentaneamente non disponibili";
                        }
                    case "81":
                        {
                            return "";
                        }
                    case "93":
                        {
                            return "Archivi momentaneamente non disponibili";
                        }
                    case "94":
                        {
                            return "Richiesta non pertinente";
                        }
                    case "98":
                        {
                            return "Funzione attualmente non disponibile";
                        }
                    case "99":
                        {
                            return "Errore grave nell'esecuzione della funzione";
                        }
                    case "90":
                        {
                            return "Dati non disponibili";
                        }

                    default:
                        {
                            return "Segnalazione generica";
                        }
                }

            }
            catch (Exception d)
            {
                //pensare a salvataggio errore su db
                return "";
            }

        }






        //private string normalizeOutput(string output, int lunghezza, int elementLenght)
        //{
        //    int resto = lunghezza % elementLenght;

        //    return output.PadRight(lunghezza + resto, ' ');

        //}

       

        //Riceve in input un orario in formato HH:MM o HHMM e
        // restituisce l'intero dei minuti
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


         public static string getTipoEccezione(string cod)
    {
        myRaiData.digiGappEntities context = new myRaiData.digiGappEntities();

        string desc = "";

        if (cod != "    ")
        {
            try
            {
                desc = context.L2D_ECCEZIONE.Where(f => f.cod_eccezione == cod).FirstOrDefault().flag_eccez;
            }

            catch (Exception e)
            {
                desc = cod;
            }
            if (desc == null) desc = "";
        }

        return desc;
    }


        public static string getUnitaMisura(string codEccezione)
        {
            myRaiData.digiGappEntities context = new myRaiData.digiGappEntities();
            L2D_ECCEZIONE eccezione;

            try
            {
                eccezione = context.L2D_ECCEZIONE.Where(f => f.cod_eccezione == codEccezione).FirstOrDefault();
                return eccezione.unita_misura;
            }
            catch
            {
                return "Eccezione non trovata";
            }
        }


        public static string formatQuantita(string unitaMisura, string unformattedQta, bool forCics = false)
        {
            try
            {
                if (!forCics)
                {
                    switch (unitaMisura)
                    {
                        case "G":
                        case "N": return (float.Parse(unformattedQta) / 100).ToString();
                        case "H": return calcolaOrario(int.Parse(unformattedQta));
                        default: return unformattedQta;
                    }
                }
                else
                {
                    switch (unitaMisura)
                    {
                        case "G":
                        case "N": return (float.Parse(unformattedQta) * 100).ToString();
                        case "H": return calcolaMinuti(unformattedQta).ToString();
                        default: return unformattedQta;
                    }
                }
            }
            catch
            {
                return "Errore durante il parsing della quantità.";
            }
        }

    



        //Riceve in input un orario in formato minuti e
        // restituisce la stringa in formato HH:MM
        public static string calcolaOrario(int minuti)
        {

            try
            {
                string orarioHHMM = "";
                int oraInt = 0;
                string min = "00";
                string ora = "00";

                string segno = "";

                if (minuti < 0) segno = "-";


                minuti = Math.Abs(minuti);


                while (minuti - 60 >= 0)
                {
                    minuti -= 60;
                    oraInt++;
                }

                ora = oraInt.ToString();
                min = minuti.ToString();

                if (oraInt < 10)
                    ora = "0" + oraInt.ToString();

                if (minuti < 10)
                    min = "0" + minuti.ToString();

                orarioHHMM = ora + ":" + min;

                return segno + " " + orarioHHMM;
            }
            catch
            {
                return minuti.ToString();
            }
        }

        private static string areaSistema(string funzione, string matricola, int livelloUtente, bool richiama = false)
        {
            string flg_chiamata = richiama ? "S" : " ";


            // Le a in padleft(25,'a') andranno tolte e sostituite con gli spazi, è stato fatto per debug
            // return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft(10, '0') + DateTime.Now.ToString("ddMMyyyy") + "".PadRight(5,'0') + flg_chiamata +  ",";
            //freak
            return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft(10, '0') + DateTime.Now.ToString("ddMMyyyy") + "".PadRight(5, '0') + flg_chiamata + "".PadRight(6, ' ') + "".PadRight(25, 'a') + livelloUtente.ToString() + DateTime.Now.ToString("HH:mm:ss").PadLeft(14, ' ') + ",";
        }

       

        #endregion


        #region Utilities
        public static bool KeyStringVerified(string keystring)
        {
            if (String.IsNullOrWhiteSpace(keystring)) return false;

            var db = new myRaiData.digiGappEntities();
            var par=db.MyRai_ParametriSistema.Where(x => x.Chiave == "CeitonWcfKeyString" && x.Valore1 !=null && x.Valore1.Trim().ToUpper()==keystring.Trim().ToUpper()).FirstOrDefault();
            return (par != null);
        }

        public static myRaiData.MyRai_LogAzioni getAzione(string woid,string operazione,string provenienza)
        {
            myRaiData.MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "CEITON_WCF",
                data = DateTime.Now,
                descrizione_operazione = woid,
                operazione = operazione,
                provenienza = provenienza,
                matricola = ""
            };
            return az;
        }
        public static void Copy(object copyToObject, object copyFromObject)
        {
            foreach (PropertyInfo sourcePropertyInfo in copyFromObject.GetType().GetProperties())
            {
                PropertyInfo destPropertyInfo = copyToObject.GetType().GetProperty(sourcePropertyInfo.Name);

                if (destPropertyInfo != null)
                {
                    destPropertyInfo.SetValue(
                        copyToObject,
                        sourcePropertyInfo.GetValue(copyFromObject, null),
                        null);
                }
            }
        }


        // recupera descrittiva dell'eccezione
        public static string getDescrizioneEccezione(string cod)
        {
           myRaiData.digiGappEntities context = new myRaiData.digiGappEntities();

            string desc = "";

            if (cod != "    ")
            {
                try
                {
                    desc = context.L2D_ECCEZIONE.Where(f => f.cod_eccezione == cod).FirstOrDefault().desc_eccezione;
                }

                catch (Exception e)
                {
                    desc = cod;
                }
                if (desc == null) desc = "";
            }

            return desc;
        }


        public static string getDescrizionecodOrario(string cod)
        {
            myRaiData.digiGappEntities context = new myRaiData.digiGappEntities();

            string desc = "";

            if (cod != "  ")
            {

                try
                {

                    desc = context.L2D_ORARIO.Where(f => f.cod_orario == cod).FirstOrDefault().desc_orario;
                }
                catch (Exception e)
                {
                    desc = e.Message;
                }
                if (desc == null) desc = "";
            }

            return desc;
        }


        // analizza la parte iniziale della risposta del cics e la spacchetta dentro l'oggetto ( struttura) cicsResponse
        public static wApiUtility.cicsResponse checkResponse(string data)
        {

            wApiUtility.cicsResponse resp = new wApiUtility.cicsResponse();
            try
            {

                string esito = data.Substring(45, 2);
                resp.esito = esito == "OK" || esito == "WW" ? true : false; // WW = Operazione non andata a buon fine ma che non è bloccante...possibile cambiamento in futuro
                resp.richiama = data.Substring(12 + 32, 1) == "S" ? true : false;// S per Richiama (riciclo) N per nessun riciclo
                resp.lunghezza = int.Parse(data.Substring(12 + 27, 5)); // lunghezza dell area dati della risposta dal cics
                resp.codErrore = data.Substring(47, 4); // Sul DB c'è la tabella con i codici e la decodifica

                //se il codice è 0000 è inutile
                if (resp.codErrore != "0000")
                {
                    resp.descErrore = new Errore(resp.codErrore).descrizione; // data.Substring(51, 25);
                }
                else
                {
                    resp.descErrore = "";
                }


            }
            catch
            {

                resp.esito = false;
                resp.richiama = false;
                resp.lunghezza = 0;
                resp.codErrore = data.Substring(3);
                resp.descErrore = "Errore area sistema. ";// +data;

            }
            return resp;
        }

        public static bool utenteAutorizzato()
        {

            myRaiData.digiGappEntities db = new myRaiData.digiGappEntities();
            if ((db.MyRai_ParametriSistema.Where(a => a.Chiave == "accountutenteservizio").FirstOrDefault().Valore1) != System.Security.Principal.WindowsIdentity.GetCurrent().Name)
            {
                return false;

            }
            else
            {

                return true;

            }

        }
         
        #endregion

    }
}
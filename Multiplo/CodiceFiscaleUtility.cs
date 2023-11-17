using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Multiplo
{
    public class CodiceFiscaleUtility
    {
        #region Private Members
        private static readonly string Months = "ABCDEHLMPRST";
        private static readonly string Vocals = "AEIOU";
        private static readonly string Consonants = "BCDFGHJKLMNPQRSTVWXYZ";
        private static readonly string OmocodeChars = "LMNPQRSTUV";
        private static readonly int[] ControlCodeArray = new[] { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23 };
        private static readonly Regex CheckRegex = new Regex(@"^[A-Z]{6}[\d]{2}[A-Z][\d]{2}[A-Z][\d]{3}[A-Z]$");
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Costruisce un codice fiscale "formalmente corretto" sulla base dei parametri indicati.
        /// 
        /// - Il codice ISTAT, relativo al comune di nascita, può essere recuperato da questo elenco:
        ///   http://www.agenziaentrate.gov.it/wps/content/Nsilib/Nsi/Strumenti/Codici+attivita+e+tributo/Codici+territorio/Comuni+italia+esteri/
        ///   
        /// IMPORTANTE: Si ricorda che il Codice Fiscale generato potrebbe non corrispondere effettivamente a quello reale.
        /// </summary>
        /// <param name="nome">Nome</param>
        /// <param name="cognome">Cognome</param>
        /// <param name="dataDiNascita">Data di nascita</param>
        /// <param name="genere">Genere ('M' o 'F')</param>
        /// <param name="codiceISTAT">Codice ISTAT (1 lettera e 3 numeri. Es.: H501 per Roma)</param>
        /// <returns>Un Codice Fiscale "formalmente corretto", calcolato sulla base dei parametri indicati.</returns>
        public static string CalcolaCodiceFiscale(string nome, string cognome, DateTime dataDiNascita, char? genere, string codiceISTAT)
        {
            if (String.IsNullOrEmpty(nome)) throw new NotSupportedException("ERRORE: Il parametro 'nome' è obbligatorio.");
            if (String.IsNullOrEmpty(cognome)) throw new NotSupportedException("ERRORE: Il parametro 'cognome' è obbligatorio.");
            if (genere != 'M' && genere != 'F') throw new NotSupportedException("ERRORE: Il parametro 'genere' deve essere 'M' oppure 'F'.");
            if (String.IsNullOrEmpty(codiceISTAT)) throw new NotSupportedException("ERRORE: Il parametro 'codiceISTAT' è obbligatorio.");

            string cf = String.Format("{0}{1}{2}{3}",
                                         CalcolaCodiceCognome(cognome),
                                         CalcolaCodiceNome(nome),
                                         CalcolaCodiceDataDiNascitaGenere(dataDiNascita, genere),
                                         codiceISTAT
                                        );
            cf += CalcolaCarattereDiControllo(cf);
            return cf;
        }

        /// <summary>
        /// Effettua un "controllo formale" del Codice Fiscale indicato secondo i seguenti criteri:
        /// 
        /// - Controlla che non sia un valore nullo/vuoto.
        /// - Controlla che il codice sia coerente con le specifiche normative per i Codici Fiscali (inclusi possibili casi di omocodia).
        /// - Controlla che il carattere di controllo sia coerente rispetto al Codice Fiscale indicato.
        /// 
        /// IMPORTANTE: Si ricorda che, anche se il Codice Fiscale risulta "formalmente corretto", 
        /// non ci sono garanzie che si tratti di un Codice Fiscale relativo a una persona realmente esistente o esistita.
        /// </summary>
        /// <param name="cf">il codice fiscale da controllare</param>
        /// <returns>TRUE se il codice è formalmente corretto, FALSE in caso contrario</returns>
        public static bool ControlloFormaleOK(string cf)
        {
            if (String.IsNullOrEmpty(cf) || cf.Length < 16) return false;
            cf = cf.Trim();
            cf = Normalize(cf, false);
            if (!CheckRegex.Match(cf).Success)
            {
                // Regex failed: it can be either an omocode or an invalid Fiscal Code
                string cf_NoOmocodia = SostituisciLettereOmocodia(cf);
                if (!CheckRegex.Match(cf_NoOmocodia).Success) return false; // invalid Fiscal Code
            }
            return cf[15] == CalcolaCarattereDiControllo(cf.Substring(0, 15));
        }

        /// <summary>
        /// Effettua un "controllo formale" del Codice Fiscale indicato secondo i seguenti criteri:
        /// 
        /// - Controlla che non sia un valore nullo/vuoto.
        /// - Controlla che il codice sia coerente con le specifiche normative per i Codici Fiscali (inclusi possibili casi di omocodia).
        /// - Controlla che il carattere di controllo sia coerente rispetto al Codice Fiscale indicato.
        /// - Controlla la corrispondenza tra il codice fiscale e i dati anagrafici indicati.
        /// 
        /// </summary>
        /// <param name="cf">il codice fiscale da controllare</param>
        /// <param name="nome">Nome</param>
        /// <param name="cognome">Cognome</param>
        /// <param name="dataDiNascita">Data di nascita</param>
        /// <param name="genere">Genere ('M' o 'F')</param>
        /// <param name="codiceISTAT">Codice ISTAT (1 lettera e 3 numeri. Es.: H501 per Roma)</param>
        /// <returns>TRUE se il codice è formalmente corretto, FALSE in caso contrario</returns>
        public static bool ControlloFormaleOK(string cf, string nome, string cognome, DateTime dataDiNascita, char genere, string codiceISTAT)
        {
            if (String.IsNullOrEmpty(cf) || cf.Length < 16) return false;
            cf = Normalize(cf, false);
            string cf_NoOmocodia = string.Empty;
            if (!CheckRegex.Match(cf).Success)
            {
                // Regex failed: it can be either an omocode or an invalid Fiscal Code
                cf_NoOmocodia = SostituisciLettereOmocodia(cf);
                if (!CheckRegex.Match(cf_NoOmocodia).Success) return false; // invalid Fiscal Code
            }
            else cf_NoOmocodia = cf;

            // NOTE: 
            // - 'fc' è il codice fiscale inserito (potrebbe contenere lettere al posto di numeri per omocodia)
            // - 'cf_NoOmocodia' è il codice fiscale epurato di eventuali modifiche dovute a omocodia.

            if (String.IsNullOrEmpty(nome) || cf_NoOmocodia.Substring(3, 3) != CalcolaCodiceNome(nome)) return false;
            if (String.IsNullOrEmpty(cognome) || cf_NoOmocodia.Substring(0, 3) != CalcolaCodiceCognome(cognome)) return false;
            if (cf_NoOmocodia.Substring(6, 5) != CalcolaCodiceDataDiNascitaGenere(dataDiNascita, genere)) return false;
            if (String.IsNullOrEmpty(codiceISTAT) || cf_NoOmocodia.Substring(11, 4) != Normalize(codiceISTAT, false)) return false;

            // Il carattere di controllo, in caso di omocodia, è anch'esso calcolato sul codice fiscale modificato, quindi occorre utilizzare quest'ultimo.
            if (cf[15] != CalcolaCarattereDiControllo(cf.Substring(0, 15))) return false;

            return true;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Calcola le 3 lettere del cognome indicato, utilizzate per il calcolo del Codice Fiscale.
        /// </summary>
        /// <param name="s">Il cognome della persona</param>
        /// <returns>Le 3 lettere che saranno utilizzate per il calcolo del Codice Fiscale</returns>
        public static string CalcolaCodiceCognome(string s)
        {
            s = Normalize(s, true);
            string code = string.Empty;
            int i = 0;

            // pick Consonants
            while ((code.Length < 3) && (i < s.Length))
            {
                for (int j = 0; j < Consonants.Length; j++)
                {
                    if (s[i] == Consonants[j]) code += s[i];
                }
                i++;
            }
            i = 0;

            // pick Vocals (if needed)
            while (code.Length < 3 && i < s.Length)
            {
                for (int j = 0; j < Vocals.Length; j++)
                {
                    if (s[i] == Vocals[j]) code += s[i];
                }
                i++;
            }

            // add trailing X (if needed)
            return (code.Length < 3) ? code.PadRight(3, 'X') : code;
        }

        /// <summary>
        /// Calcola le 3 lettere del nome indicato, utilizzate per il calcolo del Codice Fiscale.
        /// </summary>
        /// <param name="s">Il nome della persona</param>
        /// <returns>Le 3 lettere che saranno utilizzate per il calcolo del Codice Fiscale</returns>
        public static string CalcolaCodiceNome(string s)
        {
            s = Normalize(s, true);
            string code = string.Empty;
            string cons = string.Empty;
            int i = 0;
            while ((cons.Length < 4) && (i < s.Length))
            {
                for (int j = 0; j < Consonants.Length; j++)
                {
                    if (s[i] == Consonants[j]) cons = cons + s[i];
                }
                i++;
            }

            code = (cons.Length > 3)
                // if we have 4 or more consonants we need to pick 1st, 3rd and 4th
                ? cons[0].ToString() + cons[2].ToString() + cons[3].ToString()
                // otherwise we pick them all
                : code = cons;

            i = 0;
            // add Vocals (if needed)
            while ((code.Length < 3) && (i < s.Length))
            {
                for (int j = 0; j < Vocals.Length; j++)
                {
                    if (s[i] == Vocals[j]) code += s[i];
                }
                i++;
            }

            // add trailing X (if needed)
            return (code.Length < 3) ? code.PadRight(3, 'X') : code;
        }


        /// <summary>
        /// Calcola le 5 lettere relative a data di nascita e genere, utilizzate per il calcolo del Codice Fiscale.
        /// </summary>
        /// <param name="d">La data di nascita</param>
        /// <param name="g">Il genere ('M' o 'F')</param>
        /// <returns>Le 5 lettere che saranno utilizzate per il calcolo del Codice Fiscale.</returns>
        private static string CalcolaCodiceDataDiNascitaGenere(DateTime d, char? g)
        {
            string code = d.Year.ToString().Substring(2);
            code += Months[d.Month - 1];
            if (g == 'M' || g == 'm') code += (d.Day <= 9) ? "0" + d.Day.ToString() : d.Day.ToString();
            else if (g == 'F' || g == 'f') code += (d.Day + 40).ToString();
            else throw new NotSupportedException("ERROR: genere must be either 'M' or 'F'.");
            return code;
        }

        /// <summary>
        /// Calcola il carattere di controllo sulla base dei precedenti 15 caratteri del Codice Fiscale.
        /// </summary>
        /// <param name="f15">I primi 15 caratteri del Codice Fiscale (ovvero tutti tranne il Carattere di Controllo)</param>
        /// <returns>Il carattere di controllo da utilizzare per il calcolo del Codice Fiscale</returns>
        public static char CalcolaCarattereDiControllo(string f15)
        {
            int tot = 0;
            byte[] arrCode = Encoding.ASCII.GetBytes(f15.ToUpper());
            for (int i = 0; i < f15.Length; i++)
            {
                if ((i + 1) % 2 == 0) tot += (char.IsLetter(f15, i))
                    ? arrCode[i] - (byte)'A'
                    : arrCode[i] - (byte)'0';
                else tot += (char.IsLetter(f15, i))
                    ? ControlCodeArray[(arrCode[i] - (byte)'A')]
                    : ControlCodeArray[(arrCode[i] - (byte)'0')];
            }
            tot %= 26;
            char l = (char)(tot + 'A');
            return l;
        }

        /// <summary>
        /// Sostituisce le lettere utilizzate per modificare il Codice Fiscale in caso di omocodia (se presenti) con i relativi numeri.
        /// </summary>
        /// <param name="cf">Fiscal Code potentially containing omocode chars</param>
        /// <returns>Il Codice Fiscale epurato dalle eventuali modifiche dovute a casi di omocodia (da utilizzare per il calcolo di nome, cognome et. al.)</returns>
        private static string SostituisciLettereOmocodia(string cf)
        {
            char[] cfChars = cf.ToCharArray();
            int[] pos = new[] { 6, 7, 9, 10, 12, 13, 14 };
            foreach (int i in pos) if (!Char.IsNumber(cfChars[i])) cfChars[i] = OmocodeChars.IndexOf(cfChars[i]).ToString()[0];
            return new string(cfChars);
        }

        /// <summary>
        /// Effettua varie operazioni di normalizzazione su una stringa, rimuovendo spazi e/o caratteri non utilizzati.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="normalizeDiacritics">TRUE per sostituire le lettere accentate con il loro equivalente non accentato</param>
        /// <returns></returns>
        private static string Normalize(string s, bool normalizeDiacritics)
        {
            if (String.IsNullOrEmpty(s)) return s;
            s = s.Trim().ToUpper();
            if (normalizeDiacritics)
            {
                string src = "ÀÈÉÌÒÙàèéìòù";
                string rep = "AEEIOUAEEIOU";
                for (int i = 0; i < src.Length; i++) s = s.Replace(src[i], rep[i]);
                return s;
            }
            return s;
        }
        #endregion Private Methods
    }
}

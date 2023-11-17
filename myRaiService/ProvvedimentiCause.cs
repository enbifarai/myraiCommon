using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace myRaiService
{
    public class ProvvedimentiCause
    {
        public ProvvedimentiCauseResponse GetProvvedimentiCause(string RispostaCics)
        {
            ProvvedimentiCauseResponse Response = new ProvvedimentiCauseResponse() { Esito = true, Cause= new List<Causa> (), Provvedimenti = new List<Provvedimento> (), Raw = RispostaCics };

            //SAMPLES
            //String RispostaCics = "ACK0100520 010119Q11/0 LV.0A.0301282010606                                                                    00100000000010                                                                                      TN.ro   Data    Servizio ---------- C a u s a l e ---------  Tipo Provv.        T   ----- Stato -----                                        -- Durata --       P  1 30.10.1985 -DP-     MANCATO/IRREG. ADEMPIM. FORMALITA'  RICHIAMO           P   DEFINITIVO           CONTROLLO PRESENZE/OSSERVAN.ORARIO";
            //String RispostaCics = "ACK0101320 010119V04/0 LV.05.0060406060406                                                                    00000000500101                                                                                      H N.ro Causa     Data        -----------T i p o    M o t i v o-----------       H                  ---E s i t o--- ---S t a t o   P r o c e s s u a l e ---     C 040103 0    30.12.1899 Individ. REINTEGRAZIONE OPERATORE DI RIPRESA 5 LIV.    C       Estinta    ESTINZIONE      Estinta                                      C 080071 0    29.07.2008 Individ. REINTEGRAZIONE OPERATORE DI RIPRESA 5 LIV.    C       Estinta    PASS.GIUDICAT   Estinta                                      C 090036 0    19.03.2009 Individ. REINTEGRAZIONE OPERATORE DI RIPRESA 5 LIV.    C       Estinta    CONFLUENZA      Estinta                                      C 090113 0    12.11.2009 Individ. REINTEGRAZIONE OPERATORE DI RIPRESA 5 LIV.    C       Estinta    PASS.GIUDICAT   Estinta                                      C 100123 0    23.09.2015 Individ. QUANTUM REINTEGRAZIONI                        C       Estinta                    Estinta                                      C 130087 0    06.06.2013 Individ. INQUADRAMENTO OPERATORE DI RIPRESA 3+ ALTRE   C       In vita                    In vita";
            //String RispostaCics = "ACK0100200 010119V04/0 LV.05.0060406060406                                                                    00000000000000";

            if (String.IsNullOrWhiteSpace(RispostaCics) || RispostaCics.StartsWith("ACK9"))
            {
                Response.Esito = false;
                Response.Error = RispostaCics;
                return Response;
            }

            int LunghezzaRisposta = Convert.ToInt32(RispostaCics.Substring(5, 5));
            int iSpazio = LunghezzaRisposta + 10 - RispostaCics.Length;
            if (iSpazio > 0)
                RispostaCics = RispostaCics.Substring(10) + "".PadRight(iSpazio, ' ');
            else
                RispostaCics = RispostaCics.Substring(10, LunghezzaRisposta);  


            try
            {
                String Contenuti = RispostaCics.Substring(100);

                Response.PDchiusi = Convert.ToInt32(Contenuti.Substring(0, 3));
                Response.PDaperti = Convert.ToInt32(Contenuti.Substring(3, 3));
                Response.Cchiuse  = Convert.ToInt32(Contenuti.Substring(6, 3));
                Response.Caperte  = Convert.ToInt32(Contenuti.Substring(9, 3));
                Response.filPD    = Convert.ToInt32(Contenuti.Substring(12, 1));
                Response.filCause = Convert.ToInt32(Contenuti.Substring(13, 1));

                if (Response.filPD == 1) Response.StatoArchivioProvvedimenti = "Archivio disponibile";
                else if (Response.filPD == 2) Response.StatoArchivioProvvedimenti = "Archivio momentaneamente non disponibile";
                else Response.StatoArchivioProvvedimenti = "Nessun provvedimento in archivio";

                if (Response.filCause == 1) Response.StatoArchivioCause = "Archivio disponibile";
                else if (Response.filCause == 2) Response.StatoArchivioCause = "Archivio momentaneamente non disponibile";
                else Response.StatoArchivioCause = "Nessuna causa in archivio";

                if (Response.PDchiusi + Response.PDaperti + Response.Cchiuse + Response.Caperte == 0)
                    return Response;

                //Contenuti = Contenuti.Substring(14);
                //Contenuti = Contenuti.Substring(86);
                //Contenuti = Contenuti.Substring(160);
                Contenuti = Contenuti.Substring(260);

                for (int i = 0; i < Response.Caperte + Response.Cchiuse; i++)
                {
                    Causa c = new Causa();
                    c.Ncausa = Contenuti.Substring(1, 13).Trim();
                    c.Stato = Contenuti.Substring(81, 18).Trim();

                    c.DataText = Contenuti.Substring(14, 10);
                    DateTime D;
                    if (DateTime.TryParseExact(c.DataText.Trim(), "dd.MM.yyyy", null, DateTimeStyles.None, out D)) c.Date = D;
                    else c.Date = null;

                    c.TipoMotivo = Contenuti.Substring(33, 50).Trim();
                    c.Tipo = Contenuti.Substring(24, 8).Trim();
                    c.Esito = Contenuti.Substring(99, 16).Trim();
                    c.StatoProcessuale = Contenuti.Substring(115, 45).Trim();
                    Response.Cause.Add(c);
                    Contenuti = Contenuti.Substring(160);
                }

                for (int i = 0; i < Response.PDchiusi + Response.PDaperti; i++)
                {
                    Provvedimento p = new Provvedimento();
                    p.Progressivo = Contenuti.Substring(1, 4).Trim();
                    p.DataText = Contenuti.Substring(5, 11);
                    DateTime D;

                    if (DateTime.TryParseExact(p.DataText.Trim(), "dd.MM.yyyy", null, DateTimeStyles.None, out D)) p.Date = D;
                    else p.Date = null;

                    p.Servizio = Contenuti.Substring(16, 9).Trim();
                    p.Stato = Contenuti.Substring(81, 24).Trim();
                    p.Provvedim = Contenuti.Substring(61, 19).Trim();

                    p.Durata = Contenuti.Substring(141, 19).Trim();

                    p.Testo1 = Contenuti.Substring(25, 36).Trim();
                    p.Testo2 = Contenuti.Substring(105, 35).Trim();

                    Response.Provvedimenti.Add(p);
                    Contenuti = Contenuti.Substring(160);
                }
            }
            catch (Exception ex)
            {
                Response.Esito = false;
                Response.Error = ex.ToString();
            }

            return Response;
        }
    }
    public class ProvvedimentiCauseResponse
    {
        public int PDchiusi { get; set; }
        public int PDaperti { get; set; }
        public int Cchiuse { get; set; }
        public int Caperte { get; set; }
        public int filPD { get; set; }
        public int filCause { get; set; }
        public List<Provvedimento> Provvedimenti { get; set; }
        public string StatoArchivioProvvedimenti { get; set; }
        public List<Causa> Cause { get; set; }
        public string StatoArchivioCause { get; set; }

        public string Raw { get; set; }
        public string Error { get; set; }
        public Boolean Esito { get; set; }
    }

    public class Provvedimento
    {
        public string Progressivo { get; set; }
        public string DataText { get; set; }
        public DateTime? Date { get; set; }
        public string Servizio { get; set; }
        public string Stato { get; set; }
        public string Provvedim { get; set; }
        public string Durata { get; set; }
        public string Testo1 { get; set; }
        public string Testo2 { get; set; }
    }

    public class Causa
    {
        public string Ncausa { get; set; }
        public string DataText { get; set; }
        public DateTime? Date { get; set; }
        public string TipoMotivo { get; set; }
        public string Stato { get; set; }
        public string Tipo { get; set; }
        public string Esito { get; set; }
        public string StatoProcessuale { get; set; }

    }
}
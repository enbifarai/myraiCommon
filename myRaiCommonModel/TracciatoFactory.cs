using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{

    public class TracciatoFactory
    {
        public TracciatoGenerico GetTracciatoClass(int idTracciato,
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse Response,
            InfoPerTracciato Info
            )
        {
            switch (idTracciato)
            {
                case 843:
                    return new ASSENZA_FACOLTATIVA_AF_CF_DF_DK_9000() { Response = Response, Info = Info };
                case 187:
                    return new STORNO_CEDOLINO() { Response = Response, Info = Info };
                case 192:
                    return new MATERNITA_MT_9000() { Response = Response, Info = Info };
                case 923:
                    return new ASSENZE_BF_NI_9000() { Response = Response, Info = Info };
                case 844:
                    return new CONGEDO_PATERNO_MG_MV_9000() { Response = Response, Info = Info };
                case 892:
                    return new MATERNITA_MU_9000() { Response = Response, Info = Info };
                case 234:
                    return new DESCRITTIVA() { Response = Response, Info = Info };
                case 922:
                    return new ASSENZA_FACOLTATIVA_HF_9000() { Response = Response, Info = Info };
                case 191: //non ancora implementata
                    return new ARRETRATI_E_RECUPERI_9000() { Response = Response, Info = Info };
            }
            return null;
        }
    }

    public abstract class TracciatoGenerico
    {
        public string TestoTracciato { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse Response { get; set; }
        public InfoPerTracciato Info { get; set; }

        public abstract void FillRecord();

        public List<CampoContent> ExplodeTrack(string tracciatoDaDB)
        {
            List<CampoContent> LC = new List<CampoContent>();
            foreach (var campo in Response.Campi)
            {
                LC.Add(new CampoContent()
                {
                    NomeCampo = campo.NomeCampo,
                    TipoCampo = campo.TipoCampoEsteso,
                    ContenutoCampo = tracciatoDaDB.Substring((int)campo.Posizione - 1, (int)campo.Lunghezza),
                    LunghezzaCampo = (int)campo.Lunghezza,
                    PosizioneTracciato = (int)campo.Posizione,
                    CodiceTipoCampo = campo.TipoCampo
                });
            }
            return LC;
        }
        public string Get_TIPO_CEDOLINO(MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciato campo, DateTime InizioPeriodoEccezione)
        {
            DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime PrimoDelMesePeriodoEccezione = new DateTime(InizioPeriodoEccezione.Year, InizioPeriodoEccezione.Month, 1);
            string valore = null;
            //  if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
            //  {
            valore = GetCampo(campo, " ");
            //  }
            // else
            // {
            //    valore = GetCampo(campo, "0");
            // }
            return valore;
        }
        public string GetCampo(MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciato campo, string valore)
        {
            int differenza = (int)campo.Lunghezza - valore.Length;

            string testo = valore;

            if (differenza > 0)
            {
                var Riempimento = ' ';
                if (campo.SimboliRiempimento == "0" || campo.SimboliRiempimento == "Z")
                    Riempimento = '0';
                if (campo.Allineamento == "D")
                    testo = valore.PadLeft((int)campo.Lunghezza, Riempimento);
                else
                    testo = valore.PadRight((int)campo.Lunghezza, Riempimento);
            }
            return testo;
        }
    }
    public class InfoPerTracciato
    {
        public InfoPerTracciato(myRaiData.Incentivi.XR_MAT_RICHIESTE richiesta,
            string giorni26mi, string importoRetribuzione, String inizioPeriodoEccezione, String finePeriodoEccezione,
            string eccezioneRisultante, DateTime dataInizioPratica, string importo13ma, string importo14ma, string importoPremio,
            string progressivoFamiliare = "  ", string descrittiva = null)
        {


            this.Richiesta = richiesta;

            this.Giorni26mi = giorni26mi;
            this.ImportoRetribuzione = importoRetribuzione;
            this.InizioPeriodoEccezione = inizioPeriodoEccezione;
            this.FinePeriodoEccezione = finePeriodoEccezione;
            this.EccezioneRisultante = eccezioneRisultante;
            this.DataInizioPratica = dataInizioPratica;
            this.Importo13ma = importo13ma;
            this.Importo14ma = importo14ma;
            this.ImportoPremio = importoPremio;
            this.ProgressivoFamiliare = progressivoFamiliare;
            if (String.IsNullOrWhiteSpace(this.ProgressivoFamiliare))
                this.ProgressivoFamiliare = "  ";

            this.Descrittiva = descrittiva;
        }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }

        public string Giorni26mi { get; set; }
        public string ImportoRetribuzione { get; set; }
        public string Importo13ma { get; set; }
        public string Importo14ma { get; set; }
        public string ImportoPremio { get; set; }
        public String InizioPeriodoEccezione { get; set; }
        public String FinePeriodoEccezione { get; set; }
        public string EccezioneRisultante { get; set; }
        public DateTime DataInizioPratica { get; set; }
        public string ProgressivoFamiliare { get; set; }
        public string Descrittiva { get; set; }
    }
    public class ASSENZA_FACOLTATIVA_HF_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            string valore = "";
            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];
                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA")  
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    if (Info.Giorni26mi == "0,6" || Info.Giorni26mi == "0,3")
                        valore = "00";

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "CAMPO FILLER")
                {
                    valore = "              ";//14
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA AGGANCIO M1") // anno precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM AGGANCIO M1")// mese precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI DECIMALI") // sempre 00
                {
                    valore = "00"; //00 25 50
                    if (Info.Giorni26mi == "0,6") valore = "50";
                    if (Info.Giorni26mi == "0,3") valore = "25";

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                /*MATRICOLA RAI
AA COMPETENZA
MM COMPETENZA
TIPO CEDOLINO
CODICE
IV DEL CODICE
MOTIVO ASSENZA
GG RETRIB DAL
MM RETRIB DAL
AA RETRIB DAL
GG RETRIB AL
MM RETRIB AL
AA RETRIB AL
TIPO VARIAZIONE
GIORNI RETRIBUTIVI
GIORNI MENSA
GIORNI 30MI
IMPORTO RETRIBUZIONE
CAMPO FILLER
AA INIZIO TRATTAMENTO
MM INIZIO TRATTAMENTO
AA AGGANCIO M1
MM AGGANCIO M1
GIORNI 30MI DECIMALI
RISERVATO
RICHIAMO ANAGRAFICA*/


            }
        }
    }
    public class DESCRITTIVA : TracciatoGenerico
    {
        public override void FillRecord()
        {
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            string valore = "";
            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "1");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "V DEL CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI ORE O PRESTAZ INTERO")
                {
                    valore = "   ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI ORE O PRESTAZ DECIMALE")
                {
                    valore = "  ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "DESCRITTIVA")
                {
                    valore = Info.Descrittiva;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }
    public class ASSENZA_FACOLTATIVA_AF_CF_DF_DK_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {

            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');

            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);

                    //DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month,1);
                    //DateTime PrimoDelMesePeriodoEccezione = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);

                    //if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
                    //{
                    //    valore = GetCampo(campo, " ");
                    //}
                    //else
                    //{
                    //    valore = GetCampo(campo, "0");
                    //}


                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    if (Info.Giorni26mi == "0,6" || Info.Giorni26mi == "0,3")
                        valore = "00";

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "CAMPO FILLER")
                {
                    valore = "              ";//14
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA AGGANCIO M1") // anno precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM AGGANCIO M1")// mese precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI DECIMALI") // sempre 00
                {
                    valore = "00"; //00 25 50
                    if (Info.Giorni26mi == "0,6") valore = "50";
                    if (Info.Giorni26mi == "0,3") valore = "25";

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "PROGRESSIVO FAMILIARE")
                {
                    valore = Info.ProgressivoFamiliare;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else
                {
                    var tt = campo.NomeCampo;
                }
            }
        }
    }


    public class ASSENZE_BF_NI_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');

            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI DECIMALI") // sempre 00
                {
                    valore = "00"; //00 25 50
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "PROGRESSIVO FAMILIARE")
                {
                    valore = "  ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else
                {
                    var tt = campo.NomeCampo;
                }
            }
        }
    }


    public class ARRETRATI_E_RECUPERI_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);

                    //DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //DateTime PrimoDelMesePeriodoEccezione = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);

                    //if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
                    //{
                    //    valore = GetCampo(campo, " ");
                    //}
                    //else
                    //{
                    //    valore = GetCampo(campo, "0");
                    //}
                    //if (DataInizioPeriodo .Month == DateTime.Now.Month && DataInizioPeriodo.Year == DateTime.Now.Year)
                    //     valore = GetCampo(campo, " ");
                    //else
                    //     valore = GetCampo(campo, "0");

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //anno e mese corrente = blank
                    //mese precedente 0
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RATEO 13MA")
                {
                    float v = Convert.ToSingle(Info.Importo13ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RATEO 14MA O REDAZ")
                {
                    float v = Convert.ToSingle(Info.Importo14ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA AGGANCIO M1") // anno precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM AGGANCIO M1")// mese precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }

    public class MATERNITA_MU_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);

                    //DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //DateTime PrimoDelMesePeriodoEccezione = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);

                    //if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
                    //{
                    //    valore = GetCampo(campo, " ");
                    //}
                    //else
                    //{
                    //    valore = GetCampo(campo, "0");
                    //}
                    //if (DataInizioPeriodo .Month == DateTime.Now.Month && DataInizioPeriodo.Year == DateTime.Now.Year)
                    //     valore = GetCampo(campo, " ");
                    //else
                    //     valore = GetCampo(campo, "0");

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //anno e mese corrente = blank
                    //mese precedente 0
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RATEO 13MA")
                {
                    float v = Convert.ToSingle(Info.Importo13ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RATEO 14MA O REDAZ")
                {
                    float v = Convert.ToSingle(Info.Importo14ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA AGGANCIO M1") // anno precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM AGGANCIO M1")// mese precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }
    public class MATERNITA_MT_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);

                    //DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //DateTime PrimoDelMesePeriodoEccezione = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);

                    //if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
                    //{
                    //    valore = GetCampo(campo, " ");
                    //}
                    //else
                    //{
                    //    valore = GetCampo(campo, "0");
                    //}
                    //if (DataInizioPeriodo .Month == DateTime.Now.Month && DataInizioPeriodo.Year == DateTime.Now.Year)
                    //     valore = GetCampo(campo, " ");
                    //else
                    //     valore = GetCampo(campo, "0");

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //anno e mese corrente = blank
                    //mese precedente 0
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RATEO 13MA")
                {
                    float v = Convert.ToSingle(Info.Importo13ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RATEO 14MA O REDAZ")
                {
                    float v = Convert.ToSingle(Info.Importo14ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "AA INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM INIZIO TRATTAMENTO") // inizio pratica
                {
                    valore = Info.DataInizioPratica.ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA AGGANCIO M1") // anno precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("yy");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM AGGANCIO M1")// mese precedente inizio pratica
                {
                    valore = Info.DataInizioPratica.AddMonths(-1).ToString("MM");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }


    public class CONGEDO_PATERNO_MG_MV_9000 : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');
            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];

                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    valore = Get_TIPO_CEDOLINO(campo, DataInizioPeriodo);

                    //DateTime PrimoDelMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //DateTime PrimoDelMesePeriodoEccezione = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);

                    //if (PrimoDelMesePeriodoEccezione >= PrimoDelMeseCorrente)
                    //{
                    //    valore = GetCampo(campo, " ");
                    //}
                    //else
                    //{
                    //    valore = GetCampo(campo, "0");
                    //}
                    //if (DataInizioPeriodo .Month == DateTime.Now.Month && DataInizioPeriodo.Year == DateTime.Now.Year)
                    //     valore = GetCampo(campo, " ");
                    //else
                    //     valore = GetCampo(campo, "0");

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //anno e mese corrente = blank
                    //mese precedente 0
                }
                else if (campo.NomeCampo.Trim() == "CODICE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "IV DEL CODICE")
                {
                    //accredito 2
                    //negativo 3
                    valore = GetCampo(campo, "3");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MOTIVO ASSENZA") //AF;CF;DF;DK
                {
                    valore = Info.EccezioneRisultante;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB DAL")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GG RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("dd"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                    //TestoTracciato += "07";
                }
                else if (campo.NomeCampo.Trim() == "AA RETRIB AL")
                {
                    valore = GetCampo(campo, DataFinePeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //TestoTracciato += "20";
                }
                else if (campo.NomeCampo.Trim() == "TIPO VARIAZIONE")
                {
                    valore = campo.ValorePrecompilato;
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI RETRIBUTIVI")// giorni 26mi
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    valore = (Math.Round(v * 100)).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "GIORNI MENSA")// giorni 26 mi senza decimali
                {
                    float v = Convert.ToSingle(Info.Giorni26mi);
                    int intero = (int)v;
                    valore = (intero * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "GIORNI 30MI")
                {
                    TimeSpan TS = DataFinePeriodo - DataInizioPeriodo;

                    valore = (TS.TotalDays + 1).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "IMPORTO RETRIBUZIONE")
                {
                    float v = String.IsNullOrWhiteSpace(Info.ImportoRetribuzione) ? 0 : Convert.ToSingle(Info.ImportoRetribuzione);
                    v = (float)Math.Round(v, 2);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RATEO 13MA")
                {
                    float v = String.IsNullOrWhiteSpace(Info.Importo13ma) ? 0 : Convert.ToSingle(Info.Importo13ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RATEO 14MA O REDAZ")
                {
                    float v = String.IsNullOrWhiteSpace(Info.Importo14ma) ? 0 : Convert.ToSingle(Info.Importo14ma);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);

                }
                else if (campo.NomeCampo.Trim() == "RATEO PREMIO PROD PARTE FISSA")
                {
                    float v = String.IsNullOrWhiteSpace(Info.ImportoPremio) ? 0 : Convert.ToSingle(Info.ImportoPremio);
                    valore = (v * 100).ToString().PadLeft((int)campo.Lunghezza, '0');
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RISERVATO")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "RICHIAMO ANAGRAFICA")
                {
                    valore = " ";
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }


    public class STORNO_CEDOLINO : TracciatoGenerico
    {
        public override void FillRecord()
        {
            TestoTracciato = "".PadLeft((int)Response.Lunghezza, ' ');

            DateTime DataInizioPeriodo;
            DateTime.TryParseExact(Info.InizioPeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataInizioPeriodo);

            DateTime DataFinePeriodo;
            DateTime.TryParseExact(Info.FinePeriodoEccezione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataFinePeriodo);

            string valore = "";

            for (int i = 0; i < Response.Campi.Length; i++)
            {
                var campo = Response.Campi[i];
                if (campo.NomeCampo.Trim() == "MATRICOLA RAI")
                {
                    valore = GetCampo(campo, Info.Richiesta.MATRICOLA);
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM COMPETENZA")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "AA CONTABILIZZAZIONE")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("yy"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "MM CONTABILIZZAZIONE")
                {
                    valore = GetCampo(campo, DataInizioPeriodo.ToString("MM"));
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
                else if (campo.NomeCampo.Trim() == "TIPO CEDOLINO")
                {
                    // if (DataInizioPeriodo.Month == DateTime.Now.Month && DataInizioPeriodo.Year == DateTime.Now.Year)
                    valore = GetCampo(campo, " ");
                    //else
                    //   valore = GetCampo(campo, "0");

                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                    //anno e mese corrente = blank
                    //mese precedente 0
                }
                else if (campo.NomeCampo.Trim() == "CREAZIONE MASTER")
                {
                    valore = GetCampo(campo, "*");
                    TestoTracciato = TestoTracciato.Remove((int)campo.Posizione - 1, valore.Length).Insert((int)campo.Posizione - 1, valore);
                }
            }
        }
    }

    public class CampoContent
    {
        public string NomeCampo { get; set; }
        public string ContenutoCampo { get; set; }
        public string TipoCampo { get; set; }
        public int LunghezzaCampo { get; set; }
        public int PosizioneTracciato { get; set; }
        public string CodiceTipoCampo { get; internal set; }
    }
    public class ContenutoCampiPerMeseTask
    {
        public ContenutoCampiPerMeseTask()
        {
            Campi = new List<CampoContent>();
        }
        public List<CampoContent> Campi { get; set; }
        public int anno { get; set; }
        public int mese { get; set; }
        public DateTime DataRiferimentoPrimoMese { get; set; }
        public int IdElencoTask { get; set; }
        public DateTime PeriodoDa { get; set; }
        public DateTime PeriodoA { get; set; }
        public string TracciatoIntero { get; set; }
    }
}

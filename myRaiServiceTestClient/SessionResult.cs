using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace myRaiServiceTestClient
{
    public class SessionResult
    {
        public int y { get; set; }
        public int m { get; set; }
        public int d { get; set; }

        public int h { get; set; }
        public int tot { get; set; }

        public void ImportCsvFile(string filename)
        {
            string sConnection = "Provider=Microsoft.Jet.OLEDB.4.0;"
                      + "Data Source=\"" + "d:\\test\\ppx3038.csv"+"\\\\;"
                      + "Extended Properties=\"text;HDR=YES;FMT=Delimited\"";

            var connString = string.Format(
                                    "Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=\"Text;HDR=YES;FMT=Delimited\"",
                                    Path.GetDirectoryName(filename)
                                );

            string c = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;DataSource=" + "ppx3038.csv" + ";Extended Properties=\"text;HDR=Yes;FMT=Delimited\"");

            using (var conn = new OleDbConnection(c))
            {
                conn.Open();
                var query = "SELECT * FROM [" + Path.GetFileName(filename) + "]";
                using (var adapter = new OleDbDataAdapter(query, conn))
                {
                    var ds = new DataSet("CSV File");
                    adapter.Fill(ds);
                }
            }

            var db = new myRaiData.digiGappEntities();
            using (StreamReader sr = new StreamReader(filename))
            {
                string l = null;
                while ((l = sr.ReadLine()) != null)
                {
                    if (l.Trim().StartsWith("SEDE")) continue;
                    RecordCICS r = new RecordCICS(l.Split(';'));
                    int ndocCics = Convert.ToInt32(r.NumDoc);
                    DateTime dataCics = r.ToDate(r.DataDoc, "ddMMyyyy");

                    var EccezioneDB = db.MyRai_Eccezioni_Richieste.Where(x => x.codice_sede_gapp == r.Sede && x.numero_documento == ndocCics).FirstOrDefault();
                    if (EccezioneDB == null)
                    {
                        var eccBymatrDateEcc = db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.matricola_richiesta == r.Matricola && x.data_eccezione == dataCics && x.cod_eccezione.Trim()==r.CodiceEccezione.Trim()).ToList();
                        if ( ! eccBymatrDateEcc.Any())
                        {
                            //non esistono eccezioni CODICE DATA MATRICOLA
                        }
                    }
                    else
                    {

                    }
                }
            }
        }
    }
    //public class Confronto
    //{
    //    public enum EsitoConfronto
    //    {
    //        Coincidente,
    //        Non_presente_lato_CICS,
    //        Non_presente_lato_DB,
    //        NDOC_diverso,
    //        Stato_Eccezione_diverso
    //    }
    //    public Boolean Coincidente { get; set; }
    //    public EsitoConfronto Esito { get; set;  }
    //    public void Confronta(RecordCICS r, myRaiData.MyRai_Eccezioni_Richieste DBecc)
    //    {
    //        if (DBecc == null)
    //        {
    //            this.Esito = EsitoConfronto.Non_presente_lato_DB;
    //            return;
    //        }
    //        if ( Convert.ToInt32( r.NumDoc)==DBecc.numero_documento )
    //    }
    //}

    public class RecordCICS
    {
        public string Sede { get; set; }
        public string DataDoc { get; set; }
        public string NumDoc { get; set; }
        public string DataRiferimentoChiaveSecond { get; set; }
        public string DataInnissmioneChiaveSecond { get; set; }
        public string Matricola { get; set; }
        public string CodiceEccezione { get; set; }
        public string TipoEccezione { get; set; }
        public string StatoEccezione { get; set; }
        public string CodiceStorno { get; set; }
        public string Quantita { get; set; }
        public string UnitaDiMisura { get; set; }
        public string CodiceOrario { get; set; }
        public string Dalle { get; set; }
        public string Alle { get; set; }
        public string Ore { get; set; }
        public string CodiceTransazione { get; set; }
        public string Importo { get; set; }

        public RecordCICS(string[] fields)
        {
            System.Reflection.PropertyInfo[] props = this.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                this.GetType().GetProperty(props[i].Name).SetValue(this, fields[i].ToString(), null);
            }
        }
        public string ToHHMM(int minuti)
        {
            int h = (int)minuti / 60;
            int min = minuti - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
        public DateTime ToDate(string date,string format)
        {
            DateTime.TryParseExact(date, format, null, System.Globalization.DateTimeStyles.None, out DateTime D);
            return D;
        }
    }

    public class mensa
    {
        public string locationid { get; set; }
        public string transactionid { get; set; }
        public string badge { get; set; }

        public string data { get; set; }
    }
}
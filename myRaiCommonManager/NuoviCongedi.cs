using myRaiCommonManager._Model;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonManager
{
    public class NuoviCongedi
    {
        public static string GetDateFromFiscalCode(string fiscalCode)
        {
            try
            {
                Dictionary<string, string> month = new Dictionary<string, string>();
                // To Upper
                fiscalCode = fiscalCode.ToUpper().Trim();
                month.Add("A", "01");
                month.Add("B", "02");
                month.Add("C", "03");
                month.Add("D", "04");
                month.Add("E", "05");
                month.Add("H", "06");
                month.Add("L", "07");
                month.Add("M", "08");
                month.Add("P", "09");
                month.Add("R", "10");
                month.Add("S", "11");
                month.Add("T", "12");
                // Get Date
                string date = fiscalCode.Substring(6, 5);
                int y = int.Parse(date.Substring(0, 2));
                string yy = "20" + y.ToString("00");
                string m = month[date.Substring(2, 1)];
                int d = int.Parse(date.Substring(3, 2));
                if (d > 31)
                    d -= 40;
                // Return Date
                return string.Format("{0}/{1}/{2}", d.ToString("00"), m, yy);
            }
            catch
            {
                return string.Empty;
            }
        }
        public static ContatoreCongedi GetCodiceUniEmens(string matricola, string CFminore, string eccezione, DateTime DataEccezione, DateTime? DataNascita)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            ContatoreCongedi C = new ContatoreCongedi();
            bool IsAF = eccezione.StartsWith("AF") || eccezione.StartsWith("BF") || eccezione.StartsWith("CF");

            List<myRaiData.Incentivi.XR_MAT_ARRETRATI_DIPENDENTE> ArretratiDipendentePerMinore = null;
            if (IsAF)
            {
                ArretratiDipendentePerMinore = db.XR_MAT_ARRETRATI_DIPENDENTE
                 .Where(x => x.MATRICOLA == matricola && x.DATA < DataEccezione &&
                 x.CODICE_FISCALE_FIGLIO == CFminore &&
                 (x.ECCEZIONE.StartsWith("AF") || x.ECCEZIONE.StartsWith("BF") || x.ECCEZIONE.StartsWith("CF"))
                 )
                 .OrderBy(x => x.DATA)
                 .ToList();
            }
            else
            {
                ArretratiDipendentePerMinore = db.XR_MAT_ARRETRATI_DIPENDENTE
                                 .Where(x => x.MATRICOLA == matricola && x.DATA < DataEccezione &&
                                 x.CODICE_FISCALE_FIGLIO == CFminore &&
                                 x.ECCEZIONE.StartsWith(eccezione))
                                 .OrderBy(x => x.DATA)
                                 .ToList();
            }


            var ArretratiConiugePerMinore = db.XR_MAT_ARRETRATI_CONIUGE.Where(x => x.MATRICOLA_DIPENDENTE == matricola &&
                                            x.CODICE_FISCALE_FIGLIO == CFminore)
                                            .OrderBy(x => x.PERIODO_DA)
                                            .ToList();

            string _dataNascitaMinore = DataNascita != null ? DataNascita.Value.ToString("dd/MM/yyyy") : GetDateFromFiscalCode(CFminore);

            if (String.IsNullOrWhiteSpace(_dataNascitaMinore))
            {
                C.Errore = "Data nascita non trovata";
                return C;
            }
            DateTime DataNascitaMinore;
            if (!DateTime.TryParseExact(_dataNascitaMinore, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DataNascitaMinore))
            {
                C.Errore = "Data nascita non valorizzata";
                return C;
            }

            if (DataNascitaMinore.AddYears(12) < DataEccezione)  // oltre 12 anni
            {
                C.Errore = "Maggiore 12 anni";
                return C;
            }

            C.Minore_6_anni = DataNascitaMinore.AddYears(6) >= DataEccezione;
            C.Minore_8_anni = DataNascitaMinore.AddYears(8) >= DataEccezione;
            C.Minore_12_anni = DataNascitaMinore.AddYears(12) >= DataEccezione;
            C.Fra_6_12_anni = DataNascitaMinore.AddYears(6) < DataEccezione &&
                                 DataNascitaMinore.AddYears(12) >= DataEccezione;
            C.Fra_8_12_anni = DataNascitaMinore.AddYears(8) < DataEccezione &&
                                 DataNascitaMinore.AddYears(12) >= DataEccezione;

            C.DataNascitaMinore = DataNascitaMinore;
            C.GiorniTotaliDipendente = ArretratiDipendentePerMinore.Where(x => x.DATA < DataEccezione).Sum(x => x.QUANTITA);
            C.GiorniTotaliConiuge = ArretratiConiugePerMinore.Where(x => x.PERIODO_A < DataEccezione).Sum(x => (decimal)x.QUANTITA);

            eccezione = eccezione.ToUpper().Trim();

            if (eccezione.StartsWith("AF"))
            {
                if (C.GiorniTotaliDipendente < 90 && C.Minore_6_anni) // NON TRASF
                    C.CodiceUNIEMENS = eccezione == "AF" ? CodiciUNIEMENS.MA2 : CodiciUNIEMENS.MA0;

                else if (C.GiorniTotaliDipendente < 90 && C.Fra_6_12_anni) // NON TRASF
                    C.CodiceUNIEMENS = eccezione == "AF" ? CodiciUNIEMENS.PD1 : CodiciUNIEMENS.PD0;

                else if (C.GiorniTotaliDipendente >= 90 && C.Minore_12_anni) // TRASF
                    C.CodiceUNIEMENS = eccezione == "AF" ? CodiciUNIEMENS.PE1 : CodiciUNIEMENS.PE0;
            }
            else if (eccezione.StartsWith("CF"))
            {
                if (C.GiorniTotaliDipendente + C.GiorniTotaliConiuge > 270 && C.Minore_8_anni)
                    C.CodiceUNIEMENS = eccezione == "CF" ? CodiciUNIEMENS.PB1 : CodiciUNIEMENS.PB0;

                else if (C.GiorniTotaliDipendente + C.GiorniTotaliConiuge > 270 && C.Fra_8_12_anni)
                    C.CodiceUNIEMENS = eccezione == "CF" ? CodiciUNIEMENS.TB1 : CodiciUNIEMENS.TB0;
            }

            else if (eccezione.StartsWith("BF"))
            {
                if (C.GiorniTotaliDipendente + C.GiorniTotaliConiuge > 270 && C.Minore_8_anni)
                    C.CodiceUNIEMENS = eccezione == "BF" ? CodiciUNIEMENS.PB1 : CodiciUNIEMENS.PB0;

                else if (C.GiorniTotaliDipendente + C.GiorniTotaliConiuge > 270 && C.Fra_8_12_anni)
                    C.CodiceUNIEMENS = eccezione == "BF" ? CodiciUNIEMENS.TB1 : CodiciUNIEMENS.TB0;
            }
            else if (eccezione == "MG")
            {
                C.CodiceUNIEMENS = CodiciUNIEMENS.PF1;
            }
            return C;

        }

        public static DateTime GetLimitFirst(DateTime d, List<CongediArretratiHRDW> res, string ecc)
        {
            int c = 0;
            while (true)
            {
                c++;
                DateTime Dfirst = d.AddDays(-c);
                if (res.Any(x => x.data == Dfirst && x.cod_eccezione == ecc))
                    continue;
                else
                    return Dfirst.AddDays(1);
            }
        }
        public static DateTime GetLimitAfter(DateTime d, List<CongediArretratiHRDW> res, string ecc)
        {
            int c = 0;
            while (true)
            {
                c++;
                DateTime Dafter = d.AddDays(c);
                if (res.Any(x => x.data == Dafter && x.cod_eccezione == ecc))
                    continue;
                else
                    return Dafter.AddDays(-1);
            }
        }
        public static int EsisteID_HRIS(int id, DateTime D)
        {
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
            string sql = @"select ID_CONGEDO from  OPENQUERY(DB2LINK, 'SELECT * FROM " + prefix + ".EMEN_TB_CONG_PAR') " +
                "where DATA_ECCEZIONE='" + D.ToString("yyyy-MM-dd") + "' and ID_HRIS=" + id;

            var db = new IncentiviEntities();
            db.SetCommandTimeout(120);
            List<int> rows = db.Database.SqlQuery<int>(sql).ToList();
            return rows.Count() > 0 ?   rows.First() : 0;
        }
        public static bool EsisteID_SELF(int id, DateTime D)
        {
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
            string sql = @"select ID_CONGEDO from  OPENQUERY(DB2LINK, 'SELECT * FROM " + prefix + ".EMEN_TB_CONG_PAR') " +
                "where DATA_ECCEZIONE='" + D.ToString("yyyy-MM-dd") + "' ID_SELF=" + id;

            var db = new IncentiviEntities();
            db.SetCommandTimeout(120);
            IEnumerable<int> rows = db.Database.SqlQuery<int>(sql);
            return rows.Count() > 0;
        }
        public static int GetMaxID_DB2Congedi()
        {
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
            string sql = @"select * from  OPENQUERY(DB2LINK, 'SELECT MAX(ID_CONGEDO) FROM " + prefix + ".EMEN_TB_CONG_PAR') ";

            var db = new IncentiviEntities();
            db.SetCommandTimeout(120);
            List<int> MaxID = db.Database.SqlQuery<int>(sql).ToList(); 
            if (MaxID != null && !MaxID.Any()) return 0;
            else return MaxID.First();
        }
        public static string GetDataNascitaPerDb2(XR_MAT_RICHIESTE rich)
        {
            string datanascita;
            if (rich.DATA_NASCITA_BAMBINO!= null)
                datanascita = "'" + rich.DATA_NASCITA_BAMBINO.Value.ToString("yyyy-MM-dd") + "'";
            else
            {
                datanascita = "'1900-01-01'";
                if (!String.IsNullOrWhiteSpace(rich.CF_BAMBINO))
                {
                    string dn = myRaiCommonManager.NuoviCongedi.GetDateFromFiscalCode(rich.CF_BAMBINO);
                    DateTime DN;
                    if (DateTime.TryParseExact(dn, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DN))
                    {
                        if (DN > DateTime.Now)
                        {
                            DN = DN.AddYears(-100);
                        }
                        datanascita = "'" + DN.ToString("yyyy-MM-dd") + "'";
                    }
                }
            }
            return datanascita;
        }
        public static EsitoCongedoDB2 InsertToDB2congediCSV(Congedo cong, string datanascita, string file)
        {
            
            
            if (cong == null)
            {
                System.IO.File.AppendAllText(file, "MATRICOLA,DATA_ECCEZIONE,ECCEZIONE,CODICE_FISCALE,DATA_NASCITA,DATA_INSERIMENTO,"+
                    "DATA_INIZIO,DATA_FINE,CTR_TRASFERIBILE,CTR_NON_TRASFERIBILE,CODICE_INPS,DATA_LOG,ID_HRIS,ID_SELF"+ "\r\n");
                return new EsitoCongedoDB2() { Success = true };
            }
            string format = "yyyy-MM-dd";
            string datainizio = cong.Data_inizio != null ?  cong.Data_inizio.Value.ToString(format)  : "NULL";
            string datafine = cong.Data_fine != null ? cong.Data_fine.Value.ToString(format) : "NULL";
            string dataeccezione = cong.Data_eccezione != null ? cong.Data_eccezione.ToString(format) : "NULL";
            string datainserimento = cong.Data_inserimento != null ? cong.Data_inserimento.ToString(format + " HH:mm:ss")  : "NULL";
            string datalog = cong.Data_log != null ?  cong.Data_log.ToString(format + " HH:mm:ss") : "NULL";
            string idhris = cong.IdHRIS == null ? " NULL " : cong.IdHRIS.ToString();
            string idself = cong.IdSELF == null ? " NULL " : cong.IdSELF.ToString();
            string csvLine = $"{cong.Matricola.PadLeft(6,'0')},{dataeccezione},{cong.Eccezione},{cong.CF?.Trim().ToUpper()},{datanascita.Replace("'","")}," +
                          $"{datainserimento},{datainizio},{datafine},{(cong.Ctr_trasferibile).ToString().Replace(",", ".")},{(cong.Ctr_non_trasferibile).ToString().Replace(",", ".")}," +
                          $"{cong.Codice_inps},{datalog},{idhris}," +
                          $" {idself}";

            System.IO.File.AppendAllText(file,csvLine+"\r\n");
            return new EsitoCongedoDB2() {  Success =true};
        }
        public static EsitoCongedoDB2 InsertToDB2congedi(Congedo cong, string datanascita, bool commit=false)
        {

            EsitoCongedoDB2 esito = new EsitoCongedoDB2()
            {
                InsDel = "I"
            };
            if (cong.IdHRIS != null)
            {
                myRaiCommonTasks.CommonTasks.Log("Check se esiste...");
                int ID_CONGEDO = EsisteID_HRIS((int)cong.IdHRIS, cong.Data_eccezione);
                if (ID_CONGEDO > 0)
                {
                    myRaiCommonTasks.CommonTasks.Log("TRUE");
                    esito.Success = true;
                    esito.IDinserito = ID_CONGEDO;
                    //esito.Error = "ID hris e data gia esistente in db2 (coincidente con XR_MAT_RICHIESTE)";
                    return esito;
                }
                else myRaiCommonTasks.CommonTasks.Log("FALSE");
            }
            if (cong.IdSELF != null)
            {
                if (EsisteID_SELF((int)cong.IdSELF, cong.Data_eccezione))
                {
                    esito.Success = true;
                    //esito.Error = "ID self e data gia esistente in db2 (coincidente con XR_MAT_Arretrati_Dipendente)";
                    return esito;
                }
            }
            if (cong.IdSELF == null && cong.IdHRIS == null)
            {
                esito.Success = false;
                esito.Error = "ID mancanti nei riferimenti alle tabelle hris";
                return esito;
            }
            string format = "yyyy-MM-dd";
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
            var db = new IncentiviEntities();
            try
            {

                
                //datanascita = cong.Data_nascita != null ? "'" + cong.Data_nascita.Value.ToString(format) + "'" : "'1900-01-01'";
                string datainizio = cong.Data_inizio != null ? "'" + cong.Data_inizio.Value.ToString(format) + "'" : "NULL";
                string datafine = cong.Data_fine != null ? "'" + cong.Data_fine.Value.ToString(format) + "'" : "NULL";
                string dataeccezione = cong.Data_eccezione != null ? "'" + cong.Data_eccezione.ToString(format) + "'" : "NULL";
                string datainserimento = cong.Data_inserimento != null ? "'" + cong.Data_inserimento.ToString(format + " HH:mm:ss") + "'" : "NULL";
                string datalog = cong.Data_log != null ? "'" + cong.Data_log.ToString(format + " HH:mm:ss") + "'" : "NULL";
                string idhris = cong.IdHRIS == null ? " NULL " : cong.IdHRIS.ToString();
                string idself = cong.IdSELF == null ? " NULL " : cong.IdSELF.ToString();

                string sql = @" INSERT OPENQUERY(DB2LINK, 
            'SELECT 
                    MATRICOLA,
                    DATA_ECCEZIONE,
                    ECCEZIONE,
                    CODICE_FISCALE,
                    DATA_NASCITA,
                    DATA_INSERIMENTO,
                    DATA_INIZIO,
                    DATA_FINE,
                    CTR_TRASFERIBILE,
                    CTR_NON_TRASFERIBILE,
                    CODICE_INPS,
                    DATA_LOG,
                    ID_HRIS,
                    ID_SELF
                    FROM " + prefix + ".EMEN_TB_CONG_PAR') " +
                            $" VALUES ('{cong.Matricola}',{dataeccezione},'{cong.Eccezione}','{cong.CF}',{datanascita}," +
                            $" {datainserimento},{datainizio},{datafine},{(cong.Ctr_trasferibile).ToString().Replace(",", ".")},{(cong.Ctr_non_trasferibile).ToString().Replace(",", ".")}," +
                            $"'{cong.Codice_inps}',{datalog},{idhris}," +
                            $" {idself});";

                esito.Query = sql;
                myRaiCommonTasks.CommonTasks.Log("Invio insert...");
                db.SetCommandTimeout(120);
                esito.Rows = db.Database.ExecuteSqlCommand(sql);
                myRaiCommonTasks.CommonTasks.Log("rows inserite: " + esito.Rows);

                esito.Success = esito.Rows == 1;
                if (esito.Success)
                {
                    myRaiCommonTasks.CommonTasks.Log("Recupero ID...");
                    esito.IDinserito = GetMaxID_DB2Congedi();
                    myRaiCommonTasks.CommonTasks.Log(esito.IDinserito.ToString());
                }
                   
                else
                    esito.Error = "0 rows";

            }
            catch (Exception ex)
            {
                esito.Success = false;
                esito.Error = ex.ToString();
            }
            return esito;
        }

    }
    public class ContatoreCongedi
    {
        public CodiciUNIEMENS? CodiceUNIEMENS { get; set; }
        public DateTime DataNascitaMinore { get; set; }
        public decimal GiorniTotaliDipendente { get; set; }
        public decimal GiorniTotaliConiuge { get; set; }
        public bool Minore_6_anni { get; set; }
        public bool Minore_8_anni { get; set; }
        public bool Minore_12_anni { get; set; }
        public bool Fra_6_12_anni { get; set; }
        public bool Fra_8_12_anni { get; set; }
        public string Errore { get; set; }
    }
    public enum CodiciUNIEMENS
    {
        MA2,  //AF
        MA0,  //AF
        PD1,  //AF
        PD0,  //AF
        PE1,  //CF-BF
        PE0,  //CF-BF
        PB1,  //CF-BF
        PB0,  //CF-BF
        TB1,  //CF-BF
        TB0,   //CF-BF
        PF1
    }
}

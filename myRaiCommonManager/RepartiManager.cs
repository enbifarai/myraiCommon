using myRai.DataAccess;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonManager
{
    public class RepartiManager
    {
        public static List<string> RepartiDiCuiSonoResponsabile(string sede)
        {
            List<string> sedi = RepartiManager.GetSediReparti_MatricolaLivello1(CommonHelper.GetCurrentUserMatricola());
            sedi = sedi.Where(x => x.Trim().ToUpper().StartsWith(sede.Trim().ToUpper())).ToList();

            return sedi.Where(x => x.Length > 5).ToList();
        }
        public static Boolean SonoResponsabileDiReparti(string sede)
        {
            var sedi = RepartiDiCuiSonoResponsabile(sede);
            return sedi.Count > 0;
        }
        public static bool MieiRepartiGiaVisionati(string sede, DateTime data_da, DateTime data_a)
        {
            var myrep = RepartiDiCuiSonoResponsabile(sede).Select(x => x.Substring(5)).ToList();

            var db = new myRaiData.digiGappEntities();
            var visionati = db.MyRai_PdfReparti.Where(x =>
                x.sedegapp == sede &&
                x.periodo_dal == data_da &&
                x.periodo_al == data_a).Select(x => x.reparto).ToList();

            return (visionati.Intersect(myrep).Count() == myrep.Count());

        }
        public static bool ServeGenerazionePdf(string sede, string da, string a)
        {
            List<string> SediReparti = RepartiDiCuiSonoResponsabile(sede);
            DateTime data_da;
            DateTime.TryParseExact(da, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out data_da);

            DateTime data_a;
            DateTime.TryParseExact(a, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out data_a);
            var db = new myRaiData.digiGappEntities();

            if (SediReparti.Count > 0)
            {

                List<string> repartiSede = RepartiManager.GetRepartiAttiviPerSedeGapp(sede, data_da, data_a);
                List<string> repartiMiei = SediReparti.Select(x => x.Substring(5)).ToList();
                Boolean Completa = RepartiManager.RepartiCompleti(repartiSede, repartiMiei, sede, data_da, data_a);

                foreach (string reparto in repartiMiei)
                {
                    MyRai_PdfReparti p = new MyRai_PdfReparti()
                    {
                        data_visione = DateTime.Now,
                        matricola_liv1 = CommonHelper.GetCurrentUserMatricola(),
                        pdf_generato = Completa,
                        periodo_dal = data_da,
                        periodo_al = data_a,
                        reparto = reparto,
                        sedegapp = sede
                    };
                    db.MyRai_PdfReparti.Add(p);
                }
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                return Completa;
            }
            else
            {
                MyRai_PdfReparti p = new MyRai_PdfReparti()
                {
                    data_visione = DateTime.Now,
                    matricola_liv1 = CommonHelper.GetCurrentUserMatricola(),
                    pdf_generato = true,
                    periodo_dal = data_da,
                    periodo_al = data_a,
                    reparto = "**",
                    sedegapp = sede
                };
                db.MyRai_PdfReparti.Add(p);
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                return true;
            }

        }
        public static List<string> GetRepartiAttiviPerSedeGapp(string sede, DateTime dateStartWeek, DateTime dateEndWeek)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials()
            };

            presenzeResponse presenze = service.getPresenzeNoPDF(CommonHelper.GetCurrentUserMatricola(), "*",
                   dateStartWeek.ToString("ddMMyyyy"),
                   dateEndWeek.ToString("ddMMyyyy"),
                   sede, 75, "**");

            List<string> list = presenze.periodi.Select(x => x.dipendente.reparto).Distinct().OrderBy(x => x).ToList();
            list.RemoveAll(x => x == "00" || String.IsNullOrWhiteSpace(x));



            if (DateTime.Now < new DateTime(2018, 9, 13)) list = new List<string> { "01", "02", "03" }; //simuladati

            return list;
        }


        public static bool RepartiCompleti(List<string> RepartiTot, List<string> RepartiEventuali, string sede,
            DateTime data_da, DateTime data_a)
        {
            var db = new myRaiData.digiGappEntities();
            var visionati = db.MyRai_PdfReparti.Where(x =>
                            x.periodo_dal == data_da &&
                            x.periodo_al == data_a &&
                            x.sedegapp == sede)
                            .Select(x => x.reparto)
                            .ToList();

            if (visionati.Contains("**")) return true;

            RepartiTot.RemoveAll(x => RepartiEventuali.Contains(x));
            RepartiTot.RemoveAll(x => visionati.Contains(x));
           
            return (RepartiTot.Count == 0);

        }

        public static List<string> GetMatricoleLivello1_SedeReparto(string sede, string reparto)
        {
            List<string> matricole =
                DaFirmareManager.GetMatricolaLivelloPerSede(sede + reparto, 1);

            return matricole;
        }

        public static List<string> GetSediReparti_MatricolaLivello1(string matricolaL1)
        {
            Abilitazioni AB = CommonHelper.getAbilitazioni();
            List<string> sedi =
                AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Select(a => a.Matricola).Contains("P" + matricolaL1)).Select(x => x.Sede).ToList();

            return sedi;
        }


        public static List<string> RepartiDiCuiSonoResponsabileForBatch ( string sede, string matricola )
        {
            List<string> sedi = RepartiManager.GetSediReparti_MatricolaLivello1ForBatch ( matricola );
            sedi = sedi.Where ( x => x.Trim ( ).ToUpper ( ).StartsWith ( sede.Trim ( ).ToUpper ( ) ) ).ToList ( );

            return sedi.Where ( x => x.Length > 5 ).ToList ( );
        }

        public static List<string> GetSediReparti_MatricolaLivello1ForBatch( string matricolaL1 )
        {
            Abilitazioni AB = CommonHelper.getAbilitazioniForBatch ( matricolaL1 );
            List<string> sedi =
                AB.ListaAbilitazioni.Where ( x => x.MatrLivello1.Select ( a => a.Matricola ).Contains ( "P" + matricolaL1 ) ).Select ( x => x.Sede ).ToList ( );

            return sedi;
        }
    }
}
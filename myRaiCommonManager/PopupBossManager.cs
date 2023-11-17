using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiCommonModel;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonManager
{
    public class PopupBossManager
    {
        public static ModelDash GetEvidenzeModelPerMatricola(string matricola)
        {
            if (matricola != null && matricola.Length == 7) matricola = matricola.Substring(1);

            WSDigigapp datiBack_ws1 = new WSDigigapp();
            ModelDash model = new ModelDash();
            string dateBack = UtenteHelper.GetDateBackPerEvidenze();
            datiBack_ws1.Credentials = CommonHelper.GetUtenteServizioCredentials();
            model.listaEvidenze= myRaiHelper.ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, matricola,
               dateBack,
               DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

            var resp = wcf1.GetRecuperaUtente(matricola, DateTime.Today.ToString("ddMMyyyy"));
            string tipoDipendente = resp.data.tipo_dipendente;

            Quadratura? q = null;

            string tipi = CommonHelper.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);
            if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                q = Quadratura.Settimanale;

            string tipiGiorn = CommonHelper.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);
            if (tipiGiorn != null && tipoDipendente != null && tipiGiorn.ToUpper().Contains(tipoDipendente.ToUpper()))
                q = Quadratura.Giornaliera;


            if (q == Quadratura.Settimanale   )
            {
                using (myRaiData.digiGappEntities db = new myRaiData.digiGappEntities())
                {
                    string sede = UtenteHelper.SedeGapp(DateTime.Now );
                    var listagiornate = model.listaEvidenze.data.giornate.ToList();
                    DateTime maxDate;

                    try
                    {
                        maxDate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Max(z => z.data_fine);
                    }
                    catch(Exception ex)
                    {
                        maxDate = DateTime.MinValue;
                    }

                    listagiornate.RemoveAll( x => x.TipoEcc == TipoEccezione.Carenza && x.data <= maxDate.Date );
                    model.listaEvidenze.data.giornate = listagiornate.ToArray();
                }
                model.listaEvidenze.data.giornate = ScrivaniaManager.ClearCarenze( model.listaEvidenze.data.giornate );

            }
            model.QuadraturaUtenteTerzo = (q == Quadratura.Settimanale ? "s" : "g");


            UtenteTerzo u = new UtenteTerzo();
            var anecc = u.GetAnalisiEcc(CommonHelper.GetCurrentUserMatricola(), matricola);
            int pohMinuti = Convert.ToInt32(anecc.AnalisiEccezione[0].totale);
            int rohMinuti = Convert.ToInt32(anecc.AnalisiEccezione[1].totale);
            int diff = pohMinuti - rohMinuti;
            model.BilancioPoh = CommonHelper.CalcolaStringaOreMinuti(diff);
            if (diff > 0) model.BilancioPoh = "- " + model.BilancioPoh;
            if (diff < 0) model.BilancioPoh = "+ " + model.BilancioPoh;


            DateTime dt = DateTime.Now;
            while (dt.DayOfWeek != DayOfWeek.Monday) dt = dt.AddDays(-1);

            //DettaglioSettimanaleModel dettModel = new DettaglioSettimanaleModel(
            //    wcf1.PresenzeSettimanali(matricola,
            //    dt.ToString("ddMMyyyy"), dt.AddDays(6).ToString("ddMMyyyy")));
            DettaglioSettimanaleModel dettModel = new DettaglioSettimanaleModel(
               wcf1.GetPresenzeSettimanaliProtected(matricola,
               dt.ToString("ddMMyyyy"), dt.AddDays(6).ToString("ddMMyyyy"),UtenteHelper.DataInizioValidita(),dt));

            model.BilancioDettSettimanale = dettModel.DeltaTotale;

			model.MatricolaVisualizzata = matricola;
            return model;

        }
    }
}
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace myRaiCommonModel
{
    public class CarenzaTimbratureModel
    {
        public CarenzaTimbratureModel(string currentMatricola, string matricola, DateTime Date, 
            MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse giornata)
        {
            Quadratura? quadratura = UtenteHelper.GetQuadratura();

            string eccez = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniOneClick);
            if (!String.IsNullOrWhiteSpace(eccez))
            {  
                List<myRaiData.MyRai_Eccezioni_Ammesse> ammesse = EccezioniHelper.GetListaEccezioniPossibili(matricola, Date,giornata);

                var db = new myRaiData.digiGappEntities();
               // var H= db.L2D_ECCEZIONE.Where(x => x.unita_misura == "H").Select (x=>x.cod_eccezione).ToList();
                List<string>  Ec = eccez.Split(',').ToList();
                Ec.RemoveAll(x => ! ammesse.Select(z => z.cod_eccezione.Trim()).ToList().Contains(x.Trim()));

                if (quadratura == Quadratura.Settimanale) Ec.RemoveAll(x => x.ToUpper().Trim() == "POH");

                EccezioniPerCopertura = new List<EccezioneCopertura>();

                //List<string> Ec = ammesse.Where(x => H.Contains(x.cod_eccezione))
                //    .Select(x => x.cod_eccezione)
                //    .OrderBy (x=>x)
                //    .ToList();
                foreach (string e in Ec)
                {
                    EccezioneCopertura ecop = new EccezioneCopertura();
                    ecop.nome = e;
                    var ecc = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == e).FirstOrDefault();
                    if (ecc != null)
                    {
                        if (ecc.CaratteriMotivoRichiesta == null) ecop.caratteriObbligatori = 0;
                        else ecop.caratteriObbligatori = (int)ecc.CaratteriMotivoRichiesta;

                        if (ecop.nome == "UMH") ecop.maxminuti = 35;
                    }
                    EccezioniPerCopertura.Add(ecop);
                }
                if (quadratura == Quadratura.Settimanale)
                {
                    EccezioniPerCopertura.Add(new EccezioneCopertura() {
                        nome = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioneFittiziaIgnora)
                    });
                    EccezioniPerCopertura.Add(new EccezioneCopertura()
                    {
                        nome = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioneFittiziaSpostamento),
                        maxminuti = CommonHelper.GetMinutiCarenzaPerSede(giornata.giornata.sedeGapp,Date)
                    });
                }
                if (quadratura == Quadratura.Giornaliera)
                {
                    EccezioniPerCopertura.Add(new EccezioneCopertura()
                    {
                        nome = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioneFittiziaSpostamento),
                        maxminuti= CommonHelper.GetMinutiCarenzaPerSede(giornata.giornata.sedeGapp,Date)});
                }
            }
        }

        public TimbratureCore.CarenzaTimbrature carenze { get; set; }
        public List<EccezioneCopertura> EccezioniPerCopertura { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse giornata { get; set; }
        public int CarenzaGiornataMinuti { get; set; }
        public Boolean ShowProposteAutomaticheButton { get; set; }
    }
    public class EccezioneCopertura
    {
        public string nome { get; set; }
        public int caratteriObbligatori { get; set; }
        public int maxminuti { get; set; }
    }
    public class InserimentoDaTimbratureModel
    {
        public string DataEccezioni { get; set; }
        public string[] Timbrature { get; set; }
        public int[] MinutiTotali { get; set; }
        public string[] EccSel { get; set; }
        public string[] Nota { get; set; }
    }
}
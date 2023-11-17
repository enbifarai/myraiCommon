using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class MensaController : BaseCommonController
    {
        public ActionResult Index()
        {
            MensaModel mensile = CercaScontrini(DateTime.Now);
            return View(mensile);
        }

        public ActionResult GetMensaAjaxView(string date)
        {
            DateTime datarif = DateTime.Parse(date);
            MensaModel mensile = CercaScontrini(datarif);

            return PartialView("~/Views/Mensa/subpartial/mensa.cshtml", mensile);
        }

        private static MensaModel CercaScontrini(DateTime datarif)
        {
            digiGappEntities db = new digiGappEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola7chars().PadLeft(8, '0');
            MensaModel mensile = new MensaModel();
            var scontrinimese = db.MyRai_MensaXML.Where(x => x.Badge == matricola && x.TransactionDateTime.Month == datarif.Month && x.TransactionDateTime.Year == datarif.Year).OrderByDescending(x => x.TransactionDateTime);
            mensile.mese = new DateTime(datarif.Year, datarif.Month, 01);
            if (scontrinimese.Count() == 0)
            {
                return mensile;
            }
            mensile.quantiPasti = scontrinimese.Count().ToString();
            List<DatiScontrino> elencoscontrini = new List<DatiScontrino>();
            foreach (var scontrino in scontrinimese)
            {
                DatiScontrino dati = new DatiScontrino()
                {
                    dataScontrino = scontrino.TransactionDateTime.ToString("dd MMMM yyyy"),
                    oraScontrino = scontrino.TransactionDateTime.ToString("HH:mm"),
                    idScontrino = scontrino.TransactionID
                };
                List<Pasto> pasti = new List<Pasto>();
                string[] linee = scontrino.XMLorig.Split(new string[] { "<Line>" }, StringSplitOptions.None).Where(item => item != "</Line>").ToArray();
                bool isStorno = scontrino.XMLorig.Contains("Transazione Battuta errata");
                try
                {
                    dati.mensa = (linee[2].Split(new string[] { "</Line>" }, StringSplitOptions.None))[0];
                    if (!String.IsNullOrWhiteSpace(linee[3]))
                    {
                        string tmp = (linee[3].Split(new string[] { "</Line>" }, StringSplitOptions.None))[0];
                        dati.mensa += " - " + tmp.Substring(tmp.IndexOf('-') + 1);
                    }
                    for (int i = 6; i < linee.Count(); i++)
                    {
                        if (linee[i].Contains("Totale"))
                        {
                            var datitotale = (linee[i].Split(new string[] { "</Line>" }, StringSplitOptions.None))[0].TrimEnd().Split(' ').Where(item => item != "").ToList();
                            dati.prezzoTotale = (isStorno?"-":"")+datitotale.Last();
                            break;
                        }
                        if ((linee[i].Split(new string[] { "</Line>" }, StringSplitOptions.None))[0].Substring(0, 1) == " ")
                        {
                            pasti.Add(new Pasto() { descrizione = linee[i].Split(new string[] { "</Line>" }, StringSplitOptions.None)[0], prezzo = null });
                        }
                        else
                        {
                            var datipasto = (linee[i].Split(new string[] { "</Line>" }, StringSplitOptions.None))[0].TrimEnd().Split(' ').Where(item => item != "").ToList();
                            string costo = datipasto.Last();
                            datipasto.Remove(datipasto.Last());
                            pasti.Add(new Pasto() { descrizione = string.Join(" ", datipasto), prezzo = costo });
                        }
                    }
                }
                catch 
                {
                    continue;
                }
                dati.pasti = pasti;
                elencoscontrini.Add(dati);
            }
            double totale = elencoscontrini.Sum(x => Convert.ToDouble(x.prezzoTotale));
            mensile.totalePrezzoPasti = Math.Round(totale,2) * 1.00;// (double)(totale * 1);
            mensile.elencoScontrini = elencoscontrini;
            return mensile;
        }
    }
}
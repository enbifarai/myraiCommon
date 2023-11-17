using myRai.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using myRaiData;
using MyRaiServiceInterface;

namespace myRaiCommonModel
{
    public class DettaglioSettimanaleModel
    {
        digiGappEntities db = new digiGappEntities();

        public string EvidenzaBloccanteTipo { get; set; }
        public DateTime? EvidenzaBloccanteData { get; set; }
        public int EvidenzaBloccanteIDSWdaStornare { get; set;
        }
        public List<GiornoSettimanaModel> Settimana { get; set; }
        public string DeltaTotale { 
            get; 
            set; }
        //public List<EccezionePerAssenzaIngiustificata> EccezioniPerAssenzaIngiustificata { get; set; }

        public DettaglioSettimanaleModel(MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitypresenzeSettimanali_resp data)
        {
            if (data.esito)
            {
                var EccezioniPerAssenzaIngiustificata =
                    db.MyRai_Eccezioni_Ammesse.Where(x => x.StatoGiornata == "ASSING");
                var EccezioniInteraGiornata = db.L2D_ECCEZIONE.Where(x => x.unita_misura == "G").ToList();

                Settimana = new List<GiornoSettimanaModel>();
                for (int i = 0; i < data.dati.items.Count(); i++)
                {
                    GiornoSettimanaModel giorno = new GiornoSettimanaModel();
                    var element = data.dati.items.ElementAt(i);

                    giorno.GiornoSettimana = element.giornoSettimana;
                    giorno.GiornoData = element.data;
                    giorno.MaggiorPresenza = element.maggiorPresenza.Trim();
                    giorno.Carenza = element.carenza.Trim();
                    giorno.CodiceOrario = element.descOrarioReale;

                    //DateTime dt = DateTime.ParseExact(giorno.MaggiorPresenza, "hh:mm", CultureInfo.InvariantCulture);
                    //DateTime dt1 = DateTime.Parse(giorno.MaggiorPresenza, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //DateTime dt2 = DateTime.Parse(giorno.MaggiorPresenza);
                    //DateTime dt3 = DateTime.Parse(giorno.Carenza);
                    //TimeSpan tdiff = dt2 - dt3;
                    //int tdiff2 = tdiff.Hours;
                    //string dtx = tdiff.ToString(@"hh\:mm");
                    //TimeSpan ts = TimeSpan.Parse(giorno.MaggiorPresenza);
                    //TimeSpan ts2 = TimeSpan.Parse(giorno.Carenza);
                    //int delta = TimeSpan.Parse(giorno.MaggiorPresenza).CompareTo(TimeSpan.Parse(giorno.Carenza));

                    giorno.Delta = (DateTime.Parse(giorno.MaggiorPresenza) - DateTime.Parse(giorno.Carenza)).ToString(@"hh\:mm");

                    if (DateTime.Parse(giorno.MaggiorPresenza) >= DateTime.Parse(giorno.Carenza))
                        giorno.Delta = "+ " + giorno.Delta;
                    else giorno.Delta = "- " + giorno.Delta;

                    //attribuisci alla giornata la descrizione di eventuale eccezione G
                    giorno.EccezioneAssenzaIngiustificata = 
                        EccezioniInteraGiornata.Where(item =>
                            new List<string>() { element.MacroAssenza1.Trim(), 
                                                 element.MacroAssenza2.Trim(),
                                                 element.MacroAssenza3.Trim() }
                                                     .Contains(item.cod_eccezione.Trim()) &&
                                                     !String.IsNullOrWhiteSpace(item.cod_eccezione))
                                                             .Select(item=>
                                                                 item.desc_eccezione.Trim())
                                                                   .FirstOrDefault();

                    Settimana.Add(giorno);
                }

                DeltaTotale = data.dati.deltaTotale;
            }
        }

        public class GiornoSettimanaModel
        {
            string _GiornoSettimana;
            DateTime _GiornoData;
            string _Carenza;
            string _MaggiorPresenza;
            string _Delta;
            string _CodiceEccezione;
            string _CodiceOrario;

            public string GiornoSettimana { get { return _GiornoSettimana; } set { if (string.IsNullOrEmpty(value)) { this._GiornoSettimana = "--"; } else { this._GiornoSettimana = value; } } }
            public DateTime GiornoData { get; set; }
            public string Carenza { get { return _Carenza; } set { if (string.IsNullOrEmpty(value)) { this._Carenza = "--"; } else { this._Carenza = value; } } }
            public string MaggiorPresenza { get { return _MaggiorPresenza; } set { if (string.IsNullOrEmpty(value)) { this._MaggiorPresenza = "--"; } else { this._MaggiorPresenza = value; } } }
            public string Delta { get { return _Delta; } set { if (string.IsNullOrEmpty(value)) { this._Delta = "+ 00:00"; } else { this._Delta = value; } } }
            public string CodiceEccezione { get; set; }
            public string CodiceOrario { get; set; }
            public string EccezioneAssenzaIngiustificata { get; set; }

            public GiornoSettimanaModel()
            {

            }
        }

        //public class EccezionePerAssenzaIngiustificata
        //{
        //    public string CodiceEccezione { get; set; }
        //    public string DeescrizioneEccezione { get; set; }
        //}
    }


}
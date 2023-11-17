using myRaiData.Incentivi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel.Gestionale
{
    public class PianoModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            int idValutazione = Convert.ToInt32(request.Form.Get("IdValutazione"));
            string nomePiano = request.Form.Get("NomePiano");

            string pianoNamespace = PianoSviluppo_Base.PIANO_NAMESPACE;
            Type type = Type.GetType(pianoNamespace + nomePiano);

            bindingContext.ModelName = nomePiano;
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);

            return new PianoContainer()
            {
                IdValutazione = idValutazione,
                NomePiano = nomePiano,
                Piano = base.BindModel(controllerContext, bindingContext)
            };
        }
    }

    public class PianoContainer
    {
        public int IdValutazione { get; set; }
        public string NomePiano { get; set; }
        public object Piano { get; set; }
    }
    public class PianoSviluppo_Base
    {
        [JsonIgnore]
        public const string PIANO_NAMESPACE = "myRaiCommonModel.Gestionale.";

        [JsonIgnore]
        public int IdValutazione { get; set; }
        [JsonIgnore]
        public string NomePiano { get; set; }
        [JsonIgnore]
        public bool CanModify { get; set; }
        [JsonIgnore]
        public bool CanApprove { get; set; }
        [JsonIgnore]
        public bool IsApproved { get; set; }
        [JsonIgnore]
        public XR_VAL_EVALUATION_NOTE Nota { get; set; }

        public string RuoloAutore { get; set; }
        public int IdPersonaAutore { get; set; }
        public string MatricolaAutore { get; set; }
    }
    public class PianoSviluppo_2021 : PianoSviluppo_Base
    {
        public string[] PuntiForza { get; set; }
        public string[] PuntiMiglioramento { get; set; }

        public string CosaInizio { get; set; }
        public string CosaSmetto { get; set; }
        public string CosaContinuo { get; set; }

        public string Aspirazione { get; set; }

        public string Bisogno { get; set; }
        public string DiChiHoBisogno { get; set; }
        public string Traguardi { get; set; }

        public List<CorsoPianoSvil> GetCorsi()
        {
            List<CorsoPianoSvil> suggerimenti = new List<CorsoPianoSvil>();

            var db = new IncentiviEntities();
            var dbTal = new myRaiDataTalentia.TalentiaEntities();

            if (PuntiMiglioramento != null && PuntiMiglioramento.Any())
            {
                foreach (var item in PuntiMiglioramento)
                {
                    int idGr = 0;
                    string desGr = "";
                    if (item.Contains('|'))
                    {
                        idGr = Convert.ToInt32(item.Split('|')[0]);
                        desGr = item.Split('|')[1];
                    }
                    else
                        desGr = item;

                    IQueryable<XR_VAL_COURSE> elencoCorsi = null;
                    if (idGr > 0)
                        elencoCorsi = db.XR_VAL_COURSE.Where(x => x.ID_ITEM == idGr && x.COD_TIPO_ITEM == "group");
                    else
                        elencoCorsi = db.XR_VAL_COURSE.Join(db.XR_VAL_QUESTION_GROUP, x => x.ID_ITEM, y => y.ID_QST_GROUP, (x, y) => new { Corso = x, DesComp = y.NAME })
                                        .Where(x => x.Corso.COD_TIPO_ITEM == "group" && x.DesComp == desGr).Select(x => x.Corso);

                    if (elencoCorsi != null && elencoCorsi.Any())
                    {
                        foreach (var pack in elencoCorsi.GroupBy(x => x.COD_NOME_PACCHETTO))
                        {
                            if (pack.Key == null)
                            {
                                foreach (var sugg in pack)
                                {
                                    var corso = dbTal.CORSO.Find(sugg.ID_CORSO);
                                    if (corso != null)
                                    {
                                        CorsoPianoSvil corsoPianoSvil = new CorsoPianoSvil()
                                        {
                                            IdCorso = corso.ID_CORSO,
                                            CodCorso = corso.COD_CORSO,
                                            IdComp = idGr,
                                            CodComp = desGr,
                                            TipoSugg = sugg.COD_TIPO_CONS
                                        };
                                        suggerimenti.Add(corsoPianoSvil);
                                    }
                                }
                            }
                            else
                            {
                                CorsoPianoSvil pacchetto = new CorsoPianoSvil()
                                {
                                    IdCorso = 0,
                                    CodCorso = pack.Key,
                                    IdComp = idGr,
                                    CodComp = desGr,
                                    TipoSugg = pack.First().COD_TIPO_CONS
                                };
                                pacchetto.CorsiPacchetto = new List<CorsoPianoSvil>();
                                foreach (var sugg in pack)
                                {
                                    var corso = dbTal.CORSO.Find(sugg.ID_CORSO);
                                    if (corso != null)
                                    {
                                        CorsoPianoSvil corsoPianoSvil = new CorsoPianoSvil()
                                        {
                                            IdCorso = corso.ID_CORSO,
                                            CodCorso = corso.COD_CORSO,
                                            IdComp = idGr,
                                            CodComp = desGr,
                                            TipoSugg = sugg.COD_TIPO_CONS
                                        };
                                        pacchetto.CorsiPacchetto.Add(corsoPianoSvil);
                                    }
                                }
                                suggerimenti.Add(pacchetto);
                            }
                        }
                    }
                }

            }

            return suggerimenti;
        }
    }
    public class CorsoPianoSvil
    {
        public int IdCorso { get; set; }
        public string CodCorso { get; set; }
        public int IdComp { get; set; }
        public string CodComp { get; set; }
        public string TipoSugg { get; set; }
        public List<CorsoPianoSvil> CorsiPacchetto { get; set; }
    }
}

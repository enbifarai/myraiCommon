using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonModel;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class OrganizzazioneManager
    {
        public List<NodeModel> getAlbero(int? idRoot, myRaiDataTalentia.TalentiaEntities instanceDBContext, bool viewEmployee)
        {
            var db = instanceDBContext;
            NodeModel root = db.XR_STR_TALBERO.
                                    Where(s => s.id == idRoot).Select(s => new NodeModel()
                                    {
                                        pid = 0,
                                        id = s.id,
                                        Nominativo = "  "
                                    }).FirstOrDefault();
            if (root == null)
            {
                return new List<NodeModel>();
            }
            List<NodeModel> tree = new List<NodeModel>();
            tree.Add(root);
            tree.AddRange(getAlberoRicorsiva(idRoot, instanceDBContext));
            List<NodeModel> dettaglio_nodi = GetDettagliStrutturaGrafo(tree, instanceDBContext, viewEmployee);
            return dettaglio_nodi;
        }
        public List<NodeModel> getAlberoRicorsiva(int? idRoot, myRaiDataTalentia.TalentiaEntities instanceDBContext)
        {
            var db = instanceDBContext;
            List<NodeModel> childs = db.XR_STR_TALBERO.Where(w => w.subordinato_a == idRoot && w.id != w.subordinato_a && w.id != idRoot && w.id != 0)
                                    .Select(s => new NodeModel()
                                    {
                                        pid = s.subordinato_a,
                                        id = s.id,

                                    }).ToList();
            if (!childs.Any())
            {
                return childs;
            }
            List<NodeModel> result = new List<NodeModel>();
            result.AddRange(childs);
            foreach (var nodofiglio in childs)
            {
                result.AddRange(getAlberoRicorsiva(nodofiglio.id, instanceDBContext).Distinct());
            }
            return result;

        }
        public List<NodeModel> GetDettagliStrutturaGrafo(List<NodeModel> tree, myRaiDataTalentia.TalentiaEntities instanceDBContext, bool viewEmployee)
        {
            int dataFineValiditaCorrente = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;
            var db = instanceDBContext;
            var listaId = tree.Select(s => s.id);
            var listadettagli = (from sez in db.XR_STR_TSEZIONE
                                 join dip in db.SINTESI1
                                    on sez.codice_visibile equals dip.COD_UNITAORG
                                    into l
                                    where listaId.Contains(sez.id)
                                    from dip in l.DefaultIfEmpty()
                                 join incarico in db.XR_STR_TINCARICO
                                    on new { idsezione = sez.id, flagresp = "1" } equals
                                    new { idsezione = incarico.id_sezione, flagresp = incarico.flag_resp }
                                    into j
                                     from incarico in j.DefaultIfEmpty()
                                 join mansione in db.XR_STR_DINCARICO
                                     on incarico.cod_incarico equals mansione.COD_INCARICO into f
                                     from mansione in f.DefaultIfEmpty()
                                 join mission in db.XR_STR_TMISSION
                                     on sez.id equals mission.id into m
                                     from mission in m.DefaultIfEmpty()
                                 select new
                                 {
                                     description = sez.descrizione_lunga,
                                     tipoSezione = sez.tipo.ToUpper(),
                                     pubblicato = sez.pubblicato,
                                     incarico = incarico,
                                     id = sez.id,
                                     persona = dip,
                                     mansione = mansione.DES_INCARICO,
                                     livelloStaff = sez.livello,
                                     datavalidita = sez.data_fine_validita,
                                     mission = mission,
                                     ordine = (dip.COD_TPCNTR == "9") ? 1 : (dip.COD_TPCNTR == "P") ? 3 : 2
                                 }).OrderBy(o => o.ordine).ThenBy(o => o.mansione).ThenBy(o => o.incarico).ToList().Where(w => Convert.ToInt32(w.datavalidita) >= dataFineValiditaCorrente && w.tipoSezione != "B" && w.tipoSezione != "P");
                                 

            //listadettagli = listadettagli.Select(data =>
            //                        new
            //                        {
            //                            data,
            //                            ordine = (data.persona.COD_TPCNTR.Equals('9')) ? 1 : (data.persona.COD_TPCNTR.Equals('P')) ? 3 : 2
            //                        }).Select(s => s.data).ToList();

            List<NodeModel> listaNodidettagli = new List<NodeModel>();
            //Per ogni id aggiungo le diverse prop estrapolandole da listadettagli
            foreach (var item in listadettagli.GroupBy(g => g.id))
            {
                var temp = tree.FirstOrDefault(w => w.id == item.Key);
                if (item.First().pubblicato == true)
                {
                    temp.Direzione = item.First().description;
                    if (item.First().incarico != null)
                    {
                        DateTime D1;
                        bool e1 = DateTime.TryParseExact(item.First().incarico.data_inizio_validita, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D1);
                        DateTime D2;
                        bool e2 = DateTime.TryParseExact(item.First().incarico.data_fine_validita, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D2);
                        if (e1 && e2 && DateTime.Now > D1 && DateTime.Now < D2)
                        {
                            temp.Nominativo = item.First().incarico.nominativo;
                        }
                        
                        temp.Mansione = item.First().mansione;
                        temp.Numero_Risorse_Strut = item.Count();
                    }
                    if (!viewEmployee)
                    {
                        temp.Data_Di_Validita = item.First().datavalidita;
                        temp.Numero_Risorse_Strut = item.Count();
                    }
                    else
                    {
                        var itemMission = item.FirstOrDefault(x => x.mission != null && Convert.ToInt32(x.mission.data_fine_validita) >= dataFineValiditaCorrente);
                        if (itemMission != null)
                            temp.Missione = itemMission.mission.mission;
                    }

                    if (item.First().livelloStaff != null)
                    {
                        temp.Tags = item.First().livelloStaff.Cast<char>().ToArray();
                    }
                    else
                    {
                        temp.Tags = new char[] { ' ' };
                    }
                    if (viewEmployee)
                    {
                        temp.Nominativo = item.First().incarico.nominativo.TitleCase();
                    }

                    listaNodidettagli.Add(temp);
                }
              
            }

            return listaNodidettagli;
        }
    }
}


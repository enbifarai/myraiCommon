using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Linq;

namespace myRaiCommonManager
{
    public class VisualizzaEccezioniManager
    {
        public static VisualizzaEccezioniModel GetVisualizzaEccezioniModel(int[] idTematiche = null, int[] idTipologieAssenza=null )
        {
            // return null;
            var db = new digiGappEntities();
            var model = new VisualizzaEccezioniModel();
            string tipo = UtenteHelper.TipoDipendente();
            int Dest = 1;

            switch (tipo)
            {
                case "D":
                    Dest = 2;
                    break;
                case "G":
                    Dest = 3;
                    break;
                case "O":
                    Dest = 4;
                    break;
            }
            DateTime D = DateTime.Now;

            model.Eccezioni
                = db.MyRai_Regole_SchedeEccezioni
            .Where(a => a.Pubblicato == true && a.data_inizio_validita <= D && (a.data_fine_validita == null || a.data_fine_validita >= D))
            .Where(x => x.MyRai_Regole_SchedeEccezioni_Destinatari.Any(k => k.id_destinatario == Dest))
            .ToList();

            model.Visibili = model.Eccezioni.Select(x => x.codice).ToList();

            if (idTematiche != null)
                model.Eccezioni = model.Eccezioni.Where(x => x.MyRai_Regole_SchedeEccezioni_Tematiche.Any(a =>a.data_inizio_validita<=D && ( a.data_fine_validita==null || a.data_fine_validita>=D) &&  idTematiche.Contains(a.id_tematica))).ToList();

            if (idTipologieAssenza != null)
                model.Eccezioni = model.Eccezioni.Where(x => idTipologieAssenza.Contains(x.id_tipo_assenza)).ToList();

            model.Eccezioni = model.Eccezioni.OrderBy(x => x.codice).ToList();

            model.Tematiche = db.MyRai_Regole_Tematiche.OrderBy(x => x.tematica).ToList();
            model.TipiAssenza = db.MyRai_Regole_TipoAssenza.OrderBy(x => x.tipo_assenza).ToList();
            return model;
        }
    }
}
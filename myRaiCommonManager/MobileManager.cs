using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace myRaiCommonManager
{
    public class Conversazione
    {
        public Conversazione()
        {
            this.dialoghi = new List<MyRai_Note_Richieste>();
            
        }
        public List<MyRai_Note_Richieste> dialoghi { get; set; }
        public int idNotaIniziale { get; set; }
    }
    public class MobileManager
    {
        public static NoteSegreteriaResponse.NotaSegreteria GetNotaSeg(MyRai_Note_Richieste lastDialogo, int idNotaIniziale)
        {
            var n= new NoteSegreteriaResponse.NotaSegreteria()
            {
                id = lastDialogo.Id,
                destinatario = lastDialogo.Destinatario,
                datacreazione = lastDialogo.DataCreazione != null ? lastDialogo.DataCreazione.ToString("ddMMyyyy HHmm") : "",
                datagiornata = lastDialogo.DataGiornata.ToString("ddMMyyyy"),
                matricola = lastDialogo.Mittente,
                nominativo = lastDialogo.DescrizioneMittente,
                messaggio = lastDialogo.Messaggio,
                sede = lastDialogo.SedeGapp,
                idnotainiziale=idNotaIniziale,
                datalettura = lastDialogo.DataLettura == null ? "" : ((DateTime)lastDialogo.DataLettura).ToString("ddMMyyyy HHmm")
            };
            
            return n;
        }
        public static NoteSegreteriaResponse GetMessaggiSegreteria(string matr, bool AggiungiRisposteOdierne)
        {
            NoteSegreteriaResponse response = new NoteSegreteriaResponse();
            List<string> Sedi = BatchManager.GetSediAbilitateMessaggiSegreteria(matr);
            digiGappEntities db = new digiGappEntities();
            int d = CommonHelper.GetParametro<int>(EnumParametriSistema.GiorniBackMessaggiSegreteria);

            DateTime D = DateTime.Now.AddDays(-d);
            DateTime Dtoday = DateTime.Now.Date;
            List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>();
            note = db.MyRai_Note_Richieste.Where(x =>
                    (x.DataCreazione > D && Sedi.Contains(x.SedeGapp))
                    ).OrderBy(x=>x.DataCreazione).ToList();

            var Iniziali = note.Where(x => x.Destinatario == "Segreteria").OrderByDescending(x=>x.DataCreazione) .ToList();
        

            List<Conversazione> LC = new List<Conversazione>();
            foreach (var notaIniziale in Iniziali)
            {
                var responder = note.Where(x =>
                        x.DataGiornata == notaIniziale.DataGiornata 
                        && x.Destinatario == notaIniziale.Mittente
                        && x.Id > notaIniziale.Id 
                ).OrderBy (x=>x.Id).FirstOrDefault();

                if (responder == null)
                    LC.Add(new Conversazione() {
                        dialoghi = new List<MyRai_Note_Richieste>() { notaIniziale },
                        idNotaIniziale=notaIniziale.Id
                    });
                else
                {
                    string e1 = notaIniziale.Mittente;
                    string e2 = responder.Mittente;

                    var conv = note.Where(x =>x.Id==notaIniziale.Id || 
                            ( x.DataGiornata == notaIniziale.DataGiornata &&
                                 ((x.Mittente == e1 && x.Destinatario == e2)|| (x.Mittente == e2 && x.Destinatario == e1))
                            ))
                           .OrderBy(x => x.DataCreazione)
                           .ToList();
                 
                    LC.Add(new Conversazione() { dialoghi = conv , idNotaIniziale =notaIniziale.Id });
                }

               
            }
            

            foreach (var conversazione in LC)
            {
                var lastDialogo = conversazione.dialoghi.LastOrDefault();
               
                if (lastDialogo == null) continue;
                var n = GetNotaSeg(lastDialogo, conversazione.idNotaIniziale);
                if (n.id != n.idnotainiziale)
                {

                }
                if (lastDialogo.Destinatario == "Segreteria" || lastDialogo.Destinatario == matr)
                {
                    if (! response.note.Any(x=>x.id==n.id))
                                response.note.Add( n);
                }
                else
                {
                    if (AggiungiRisposteOdierne && lastDialogo.Mittente == matr && lastDialogo.DataCreazione > Dtoday)
                    {
                        n.miodioggi = true;

                        if (!response.note.Any(x => x.id == n.id))
                            response.note.Add(n);
                    }
                }
            }
            
            return response;
        }

      
    }
}
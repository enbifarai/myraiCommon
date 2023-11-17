using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData;

namespace myRaiCommonModel
{
    public class Parametri
    {
        public Parametri()
        {
            
        }
        public enum Categoria
        {

            Abbonamenti,
            Academy,
            Admin,
            Cache,
            Ceiton,
            Configurazione,
            Corsi,
            CVOnline,
            Detassazione,
            Event,
            Firma,
            JobPosting,
            Log,
            Note,
            Richieste,
            Servizio,
            Situazione,
            Gestionale,
            News,
            Approvazioni

        }

        public long ID { get; set; }
        public string Chiave { get; set; }
        public string Valore1 { get; set; }
        public string Valore2 { get; set; }
        public string categoria { get; set; }
        public Categoria categorie { get; set; } 

        public List<Parametri> listaparametri { get; set; }

        public void Init()
        {
            listaparametri = new List<Parametri>();
        }
        public List<Parametri> GetListItem()
        {
            using (var db = new myRaiData.digiGappEntities())
            {
                listaparametri = db.MyRai_ParametriSistema.Select(s=> new Parametri()
                {
                    ID=s.Id,
                    Chiave =s.Chiave.Trim(),
                    Valore1 =s.Valore1.Trim(),
                    Valore2 =s.Valore2.Trim(),
                    categoria = s.Categoria_Parametri

                }).ToList();  
              
                return listaparametri;
            }
        }
        public List<Parametri> GetDettaglioParametro(long id)
        {
            using( var db = new myRaiData.digiGappEntities())
            {
                var parametro =( from par in db.MyRai_ParametriSistema.Where(w => w.Id == id)
                                select new Parametri() {Chiave = par.Chiave, ID=par.Id, Valore1=par.Valore1,Valore2=par.Valore2, categoria=par.Categoria_Parametri}).ToList();
               
                return parametro;
            }
        }
    }

   
}
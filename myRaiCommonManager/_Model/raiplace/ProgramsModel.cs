using System;
using System.Collections.Generic;
using System.Linq;
using myRaiData;
using System.Web.Mvc;

namespace myRaiCommonModel
{

    public class ProgramsModel
    {
        public Int32 Id { get; set; }
        public string Titolo { get; set; }
        public Int32 Anno { get; set; }
        public string Luogo { get; set; }
        public string Amministratore { get; set; }

        public Int32 NumeroMedio { get; set; }
        public Int32 NumeroAtteso { get; set; }
        public Int32 NumeroMassimo { get; set; }

        public string matricola { get; set; }
        public string cod_sede_gapp { get; set; }

        public SelectList listaSedi { get; set; }
        public SelectList listaMatricole { get; set; }
        public SelectList listaProgrammi { get; set; }
        public List<B2RaiPlace_Eventi_Programma> elencoProgrammi { get; set; }
        
        digiGappEntities db = new digiGappEntities();

        public ProgramsModel(string titolo = "", string luogo = "", Int32 data_da = 0, Int32 data_a = 0)
        {
            
            var queryListaProgrammi = (IQueryable<B2RaiPlace_Eventi_Programma>)db.B2RaiPlace_Eventi_Programma;
        
            if (titolo == "" && luogo == "" && data_da == 0 && data_a == 0)
            {
                queryListaProgrammi.Where(x => x.id >= 0);
            }
            else
            {
                if (titolo != "")
                { queryListaProgrammi = queryListaProgrammi.Where(x => x.titolo.Contains(titolo)); }

                if (luogo != "")
                { queryListaProgrammi = queryListaProgrammi.Where(x => x.luogo.Contains(luogo)); }
                
                if (data_da > 0 && data_a == 0)
                {
                    queryListaProgrammi = queryListaProgrammi.Where(x => x.anno >= data_da && x.anno <= DateTime.Now.Year);
                }
                else if (data_da > 0 && data_a > 0)
                {
                    queryListaProgrammi = queryListaProgrammi.Where(x => x.anno >= data_da && x.anno <= data_a);
                }
            }


            this.elencoProgrammi = queryListaProgrammi.OrderByDescending(x => x.id).ToList();
        }

       


    }
}
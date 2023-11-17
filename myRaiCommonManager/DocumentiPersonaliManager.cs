using System.Linq;
using myRaiCommonModel.ess;

namespace myRaiCommonManager
{
    public class DocumentiPersonaliManager
    {

        public static BustaPagaModel GetBustaPagaModel(bool ultimoAnno,bool solononLetti, myRaiServiceHub.it.rai.servizi.hrpaga.ListaDocumenti result)
        {
            BustaPagaModel model = new BustaPagaModel();
            model.flagUltimoAnno = ultimoAnno;
            if (result.ListaDatiDocumenti != null)
            {
                if (!solononLetti)
                {
                    if (ultimoAnno)
                    {
                        model.elencoDocumenti = result.ListaDatiDocumenti.
                                    OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                                    GroupBy(a => a.DataContabile.Substring(0, 4)).Take(1);
                        model.elencoDocumentiLungo = result.ListaDatiDocumenti.
                                    OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile);
                    }
                    else
                    {
                        model.elencoDocumenti = result.ListaDatiDocumenti.
                                    OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                                    GroupBy(a => a.DataContabile.Substring(0, 4));
                        model.elencoDocumentiLungo = result.ListaDatiDocumenti.
                                   OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile);
                    }
                }
                else
                {
                    model.elencoDocumenti = result.ListaDatiDocumenti.Where(a => a.FlagLetto == 2).
                                            OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                                              GroupBy(a => a.DataContabile.Substring(0, 4));
                }
                model.elencoDocumentiperTipo = result.ListaDatiDocumenti.
                OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                GroupBy(a => a.DescrittivaTipoDoc);
            }
            return model;
        }

        public static BustaPagaModel GetDocumentiperTipoModel(string tipoDoc, myRaiServiceHub.it.rai.servizi.hrpaga.ListaDocumenti result)
        {
            BustaPagaModel model = new BustaPagaModel();
           
            
                model.elencoDocumenti = result.ListaDatiDocumenti.
                            OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                            GroupBy(a => a.DataContabile.Substring(0, 4));
                model.elencoDocumentiLungo = result.ListaDatiDocumenti.
                           OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile);


           

            return model;
        }


        public static BustaPagaModel GetDocumentiNonLettiModel(myRaiServiceHub.it.rai.servizi.hrpaga.ListaDocumenti result)
        {
            BustaPagaModel model = new BustaPagaModel();


            model.elencoDocumenti = result.ListaDatiDocumenti.Where(a => a.FlagLetto == 2).
                                    OrderByDescending(ListaDatiDocumenti => ListaDatiDocumenti.DataContabile).
                                      GroupBy(a => a.DataContabile.Substring(0, 4));


            return model;
        }


        public static string GettipofromDesc(string Descrizione)
        {
            myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga hrPaga = new myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga();
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            myRaiServiceHub.it.rai.servizi.hrpaga.ListaNavBar lista = hrPaga.ElencoTipoDocumento();
            string Tipo = lista.Elementi.Where(a => a.Split('|')[1].Trim() == Descrizione).FirstOrDefault().Split('|')[0];
            
            return Tipo;
        }

    }
}
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace myRaiCommonManager
{
    public class Widget_Da_Fare_Manager
    {

        public enum WidgetDaFareManagerEnum
        {
            [AmbientValue("")]
            TFR = 1,
            [AmbientValue("")]
            ASSUNZIONI = 2
        }
        
        public static List<XR_HRIS_WIDGET_DA_FARE> GetAzioniDaFare(string matricolaCorrente, string matricolaDestinatario = null)
        {
            List<XR_HRIS_WIDGET_DA_FARE> result = null;
            IEnumerable<XR_HRIS_WIDGET_DA_FARE> _temp = null;

            try
            {
                DateTime oggi = DateTime.Now;

                using (IncentiviEntities db = new IncentiviEntities())
                {
                    _temp = db.XR_HRIS_WIDGET_DA_FARE
                        .Where(w => (w.MatricolaOperatore == matricolaCorrente || w.MatricolaOperatore == "000000"))
                        .Where(w => w.DataInizioValidita <= oggi && w.DataFineValidita >= oggi);

                    if (String.IsNullOrEmpty(matricolaDestinatario))
                    {
                        _temp = _temp.Where(w => w.MatricolaDestinatario == null);
                    }
                    else
                    {
                        _temp = _temp.Where(w => w.MatricolaDestinatario != null && w.MatricolaDestinatario == matricolaDestinatario);
                    }

                    result = _temp.ToList();
                }
            }
            catch(Exception ex)
            {
                HrisHelper.LogOperazione("GetAzioniDaFare", $"Matricola:{matricolaCorrente} matricolaDestinatario:{matricolaDestinatario}", false, ex.Message, null, ex);
            }

            return result;
        }
        
        public static bool SetAzioniDaFare(string matricolaOperatore,
                                            string matricolaDestinatario,
                                            WidgetDaFareManagerEnum tipologia,
                                            string descrizione,
                                            DateTime dataInizioValidita,
                                            DateTime dataFineValidita,
                                            string url = null,
                                            string controller = null,
                                            string azione = null,
                                            string parametri = null,
                                            string azioneJavascript = null,
                                            string classe = null,
                                            string style = null,
                                            string note = null)
        {
            bool esito = false;

            try
            {
                if (String.IsNullOrWhiteSpace(matricolaOperatore))
                {
                    matricolaOperatore = "000000";
                }

                XR_HRIS_WIDGET_DA_FARE nuovoItem = new XR_HRIS_WIDGET_DA_FARE()
                {
                    Azione = azione,
                    AzioneJavascript = azioneJavascript,
                    Classe = classe,
                    Controller = controller,
                    DataFineValidita = dataFineValidita,
                    DataInizioValidita = dataInizioValidita,
                    Descrizione = descrizione,
                    MatricolaDestinatario = matricolaDestinatario,
                    MatricolaOperatore = matricolaOperatore,
                    Note = note,
                    Parametri = parametri,
                    Style = style,
                    Url = url,
                    Tipologia = (int)tipologia
                };

                using (IncentiviEntities db = new IncentiviEntities())
                {
                    db.XR_HRIS_WIDGET_DA_FARE.Add(nuovoItem);
                    db.SaveChanges();
                    esito = true;
                }
            }
            catch(Exception ex)
            {
                HrisHelper.LogOperazione("SetAzioniDaFare", $"matricolaOperatore:{matricolaOperatore} matricolaDestinatario:{matricolaDestinatario} " +
                    $"descrizione:{descrizione} dataInizioValidita:{dataInizioValidita}" +
                    $"dataFineValidita:{dataFineValidita} url:{url}" +
                    $"controller:{controller} azione:{azione}" +
                    $"parametri:{parametri} azioneJavascript:{azioneJavascript}" +
                    $"classe:{classe} style:{style}" +
                    $"note:{note}", false, ex.Message, null, ex);
            }

            return esito;
        }

        public static bool EliminaAzioneDaFare (int idAzione)
        {
            bool esito = false;
            string matricolaOperatore = UtenteHelper.Matricola();
            
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var toRemove = db.XR_HRIS_WIDGET_DA_FARE.Where(w => w.Id == idAzione).FirstOrDefault();

                    if (toRemove != null)
                    {
                        esito = true;
                        toRemove.DataFineValidita = DateTime.Today;
                        db.SaveChanges();
                        HrisHelper.LogOperazione("EliminaAzioneDaFare", $"matricolaOperatore:{matricolaOperatore} idAzione:{idAzione} ", false, null, null, null);
                    }
                    else
                    {
                        throw new Exception("Nessuna azione trovata");
                    }
                }
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("EliminaAzioneDaFare", $"matricolaOperatore:{matricolaOperatore} idAzione:{idAzione} ", false, ex.Message, null, ex);
            }

            return esito;
        }

        public static bool EliminaAzioneDaFare(string matricolaDestinatario, WidgetDaFareManagerEnum tipologia )
        {
            bool esito = false;
            string matricolaOperatore = UtenteHelper.Matricola();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var toRemove = db.XR_HRIS_WIDGET_DA_FARE.Where(w => w.MatricolaDestinatario == matricolaDestinatario 
                                                                    && w.Tipologia == (int)tipologia).FirstOrDefault();

                    if (toRemove != null)
                    {
                        esito = true;
                        toRemove.DataFineValidita = DateTime.Today;
                        db.SaveChanges();
                        HrisHelper.LogOperazione("EliminaAzioneDaFare", $"matricolaDestinatario:{matricolaDestinatario} tipologia:{(int)tipologia} ", false, null, null, null);
                    }
                    else
                    {
                        throw new Exception("Nessuna azione trovata");
                    }
                }
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("EliminaAzioneDaFare", $"matricolaDestinatario:{matricolaDestinatario} tipologia:{(int)tipologia} ", false, ex.Message, null, ex);
            }

            return esito;
        }
    }
}

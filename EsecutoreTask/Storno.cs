
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wcf = MyRaiServiceInterface.MyRaiServiceReference1;

namespace EsecutoreTask
{
    public  class Storno
    {
        public  string StornoEccezione(string matricola, string eccezione, DateTime data)
        {
            try
            {
                WSDigigappDataController ws = new WSDigigappDataController();
                dayResponse verifica = ws.GetEccezioniForBatch(matricola, data.ToString("ddMMyyyy"), "BU", 75);

                if (verifica.eccezioni != null && verifica.eccezioni.Any())
                {
                    bool presente = verifica.eccezioni.Where(w => !String.IsNullOrEmpty(w.cod) && w.cod.Trim().Equals(eccezione)).Count() > 0;

                    if (presente)
                    {
                        validationResponse resp = new validationResponse();
                        WSDigigapp datiBack = new WSDigigapp();
                        List<Eccezione> listaEccezioni = new List<Eccezione>();

                        #region STORNO
                        listaEccezioni = verifica.eccezioni.Where(w => !String.IsNullOrEmpty(w.cod) && w.cod.Trim().Equals(eccezione)).ToList();

                        string matricolaApprovatore = "UFFPERS";

                        if (listaEccezioni != null && listaEccezioni.Any())
                        {
                            foreach (var e in listaEccezioni)
                            {
                                e.matricola = matricola;
                            }

                            resp = ServiceWrapper.validaEccezioni(datiBack, matricolaApprovatore, "01", listaEccezioni.ToArray(), false, 75);

                            if (!resp.esito)
                            {
                                return (resp.errore);
                            }
                        }

                        #endregion

                        #region AGGIORNAMENTO RECORD SU DIGIGAPP
                        int idRichiesta = 0;
                        try
                        {
                            var db = new digiGappEntities();
                            var EccRichiesta = db.MyRai_Eccezioni_Richieste.Where(x => x.cod_eccezione == eccezione && x.MyRai_Richieste.matricola_richiesta == matricola
                             && x.data_eccezione == data).FirstOrDefault();
                            if (EccRichiesta == null)
                            {
                                return "Richiesta non trovata nel DB";
                            }
                            idRichiesta = EccRichiesta.id_eccezioni_richieste;
                            var EccDB = db.MyRai_Eccezioni_Richieste.Where(x => x.id_richiesta == idRichiesta).Where(x => x.azione == "I" && x.id_stato != 60 && x.id_stato != 50 && x.id_stato != 70 && x.MyRai_Richieste.matricola_richiesta == matricola).ToList();

                            Boolean DBtoUpdate = false;
                            List<int> Lid = new List<int>();

                            if (EccDB == null || !EccDB.Any())
                            {
                                //
                                return ("Richiesta con ID " + idRichiesta + " non trovata sul DB");
                            }
                            else
                            {
                                foreach (MyRai_Eccezioni_Richieste m in EccDB)
                                {
                                    if (m.numero_documento == 0)
                                        continue;

                                    if (!verifica.eccezioni.Select(x => x.ndoc)
                                        .ToList()
                                        .Contains(m.numero_documento.ToString().PadLeft(6, '0')))
                                    {
                                        var stornoDB = db.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C" && x.numero_documento_riferimento == m.numero_documento && x.codice_sede_gapp == m.codice_sede_gapp && x.id_stato == (int)EnumStatiRichiesta.Approvata).FirstOrDefault();

                                        if (m.id_stato == (int)EnumStatiRichiesta.Approvata && stornoDB != null)
                                        {
                                            m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                            m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                            MyRai_LogAzioni la = new MyRai_LogAzioni()
                                            {
                                                applicativo = "StornoGiorno",
                                                data = DateTime.Now,
                                                matricola = matricola,
                                                provenienza = "StornoEccezioneGiorno",
                                                operazione = "ELIMINATA SU GAPP",
                                                descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                            };
                                            db.MyRai_LogAzioni.Add(la);
                                            DBtoUpdate = true;
                                            Lid.Add(m.MyRai_Richieste.id_richiesta);
                                        }
                                        else
                                        {
                                            if (m.cod_eccezione.Trim().Equals(eccezione))
                                            {
                                                m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                MyRai_LogAzioni la = new MyRai_LogAzioni()
                                                {
                                                    applicativo = "StornoGiorno",
                                                    data = DateTime.Now,
                                                    matricola = matricola,
                                                    provenienza = "StornoEccezioneGiorno",
                                                    operazione = "ELIMINATA SU GAPP",
                                                    descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                                };
                                                db.MyRai_LogAzioni.Add(la);
                                                DBtoUpdate = true;
                                                Lid.Add(m.MyRai_Richieste.id_richiesta);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (m.cod_eccezione.Trim().Equals(eccezione))
                                        {
                                            m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                            m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                            MyRai_LogAzioni la = new MyRai_LogAzioni()
                                            {
                                                applicativo = "StornoGiorno",
                                                data = DateTime.Now,
                                                matricola = matricola,
                                                provenienza = "StornoEccezioneGiorno",
                                                operazione = "ELIMINATA SU GAPP",
                                                descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                            };
                                            db.MyRai_LogAzioni.Add(la);
                                            DBtoUpdate = true;
                                            Lid.Add(m.MyRai_Richieste.id_richiesta);
                                        }
                                    }
                                }
                                if (DBtoUpdate)
                                {
                                    db.SaveChanges();
                                    return "Storno cancellato";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return ("Richiesta ID " + idRichiesta + ". Errore in bonifica DB " + ex.Message);
                        }

                        #endregion
                    }
                    else
                    {
                        return "Storno non presente";
                    }
                }
            }
            catch (Exception ex)
            {
                return ("Errore in storno eccezione per la matricola " + matricola + " Errore: " + ex.Message);
            }
            return null;
        }
    }
}
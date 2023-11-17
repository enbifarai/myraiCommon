using System;
using System.Collections.Generic;
using System.Linq;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonManager
{
    public class ProntuarioManager
    {

        public static Boolean L7Gday(dayResponse R)
        {
          
            var dayStart = R.giornata.data.AddDays(-7);
            var dayEnd = R.giornata.data;

            //WSDigigapp service = new WSDigigapp() { Credentials = System.Net.CredentialCache.DefaultCredentials };
            WSDigigapp service = new WSDigigapp() { Credentials = CommonHelper.GetUtenteServizioCredentials() };
            var resp = service.getDatiPeriodo(CommonHelper.GetCurrentUserMatricola(), dayStart.ToString("ddMMyyyy"), dayEnd.ToString("ddMMyyyy"), 75);

			if ( resp.data.giornate != null &&
				resp.data.giornate.Any() && resp.data.giornate.Count() > 7 )
			{
				for ( int i = 7; i > 0; i-- )
				{
					var g = resp.data.giornate[i];
					if ( g.orarioReale.StartsWith( "96" ) )
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
            return true;
        }


        public static List<OpzioneProposta> OpzioniProposte(dayResponse f)
        {
            List<OpzioneProposta> listaAppo = new List<OpzioneProposta>();
            Giornata g = f.giornata;
            if (g == null) return null;

            String tipoGiorno = "";
            string idregola = "";

            if (g.tipoGiornata == "N") tipoGiorno = "FER";
            if (g.siglaGiornata == "DO") tipoGiorno = "DOM";
            if (g.tipoGiornata == "R") tipoGiorno = "FES";
            if (g.tipoGiornata == "P") tipoGiorno = "SFE";

            string orarioReale = g.orarioReale;
            string orarioPrevisto = g.orarioTeorico;

         

            if (new string[] { "90", "91", "92", "95", "96" }.Contains(orarioPrevisto) == false) orarioPrevisto = "LV";
            if (new string[] { "90", "91", "92", "95", "96" }.Contains(orarioReale) == false) orarioReale = "LV";

            idregola = tipoGiorno + orarioPrevisto + orarioReale;
            bool isl7g = L7Gday(f);
            try
            {
                var db = new digiGappEntities();
                var items = db.MyRai_Opzioni_Proposte.Where(x =>
                                                    x.valore_opzione_1.Contains(g.tipoDipendente)
                                                    && x.idpilotaregola.StartsWith(idregola));

                if (items == null)
                {
                    return null;
                }
                else
                {
                    bool fermati = false;
                    int l7g = 0;
                    foreach (MyRai_Opzioni_Proposte op in items)
                    {
                        //verifico se vuole il settimo giorno
                        fermati = false;
                        if (op.valore_opzione_2 == "1")
                        {
                            if (l7g == 2)
                            {
                                fermati = true;
                            }
                            else if (l7g == 0)
                            {
                                if (!isl7g)
                                {
                                    fermati = true;
                                    l7g = 2;
                                }
                                else
                                {
                                    l7g = 1;
                                    //è settimo giorno
                                }
                            }

                        }

                        if (op.valore_opzione_2 == null)
                        {
                            if (l7g == 1)
                            {
                                fermati = true;
                            }
                            else if (l7g == 0)
                            {
                                if (isl7g)
                                {
                                    fermati = true;
                                    l7g = 1;
                                }
                            }
                        }

                        //verifico se ci sono timbrature
                        if ((f.timbrature == null) || (f.timbrature.Count() == 0))
                        {
                            fermati = true;
                        }



                        if (!fermati)
                        {
                            OpzioneProposta singolaAppoggio = new OpzioneProposta();
                            singolaAppoggio.testo = op.testo_opzione;
                            //cerco le eccezioni nella tabella MyRai_Eccezioni_Opzioni_Proposte

                            var itemsEcc = db.MyRai_Eccezioni_Opzioni_Proposte.Where(a => a.idopzioneproposta == op.ID);
                            singolaAppoggio.eccezioniProposte = new List<Eccezione>();
                            bool eccassente = true;
                            foreach (MyRai_Eccezioni_Opzioni_Proposte eccop in itemsEcc)
                            {

                                if ((f.eccezioni != null) && (f.eccezioni.Any(x => x.cod.Trim() == eccop.cod_eccezione)))
                                {
                                    eccassente = false;
                                }
                                else
                                {
                                    eccassente = true;
                                }

                                if (eccassente)
                                {
                                    L2D_ECCEZIONE datiEccFrom = db.L2D_ECCEZIONE.Where(a => a.cod_eccezione.StartsWith(eccop.cod_eccezione)).FirstOrDefault();
                                    Eccezione datiEccTo = new Eccezione();
                                    datiEccTo.cod = eccop.cod_eccezione;
                                    datiEccTo.qta = "1";
                                    datiEccTo.data = f.giornata.data.ToString("ddMMyyyy");
                                    datiEccTo.descrittiva_lunga = datiEccFrom.desc_eccezione;
                                    CommonHelper.Copy(datiEccFrom, datiEccTo);
                                    singolaAppoggio.eccezioniProposte.Add(datiEccTo);
                                }

                            }

                            if (singolaAppoggio.eccezioniProposte .Count >0)  listaAppo.Add(singolaAppoggio);
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                //non faccio nulla...
                
            }


            if (listaAppo != null && listaAppo.Count > 0)
            {
                foreach (var item in listaAppo)
                {
                    if (item != null && item.eccezioniProposte != null && item.eccezioniProposte.Count > 0)
                    {
                        item.eccezioniProposte = TimbratureCore.TimbratureManager.SetCaratteriObbligatori(item.eccezioniProposte);
                    }
                }
            }
            return listaAppo;
        
        
        }



        public static RisultatoAnalisiEccezione AnalisiEccezioneConsentita(dayResponse g, string e)
        {
            Giornata f = g.giornata;
            RisultatoAnalisiEccezione res = new RisultatoAnalisiEccezione();
            res.esito = false;
            res.descrizioneErrore = "Codice " + e + " non previsto per questa giornata";


            if (f == null) 
            {
                res.esito = false;
                res.descrizioneErrore = "Giornata da analizzare inesistente";
                return res;
            };

            String tipoGiorno = "";

            if (f.tipoGiornata == "N") tipoGiorno = "FER";
            if (f.siglaGiornata == "DO") tipoGiorno = "DOM";
            if (f.tipoGiornata == "R") tipoGiorno = "FES";
            if (f.tipoGiornata == "P") tipoGiorno = "SFE";

            string orarioReale = f.orarioReale;
            string orarioPrevisto = f.orarioTeorico;
            if (new string[] { "90", "91", "92", "95", "96" }.Contains(orarioPrevisto) == false) orarioPrevisto = "LV";
            if (new string[] { "90", "91", "92", "95", "96" }.Contains(orarioReale) == false) orarioReale = "LV";

            #region Tipo dipendente T

            #region giorniferiali
            if (tipoGiorno == "FER")
                    {
                        if (orarioPrevisto == "LV")
                        {
                            #region codice orario reale lavorativo
                            if (orarioReale == "LV")
                            {
                                if (e == "LNH5")
                                {
                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice LNH5 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }


                                #region se in settimo giorno
                                if (L7Gday(g))
                                {
                                    //allora prevde srh6
                                    if (e == "SRH8")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if (!g.eccezioni.Any(x => x.cod.Trim() == "L7G"))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice L7G (lavoro in settimo giorno) obblicatorio in questa giornata per SRH8";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice SRH8 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }

                                    }
                                    if (e == "SRH6")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if (!g.eccezioni.Any(x => x.cod.Trim() == "L7G"))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice L7G (lavoro in settimo giorno) obblicatorio in questa giornata per SRH6";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice SRH6 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }


                                    }


                                }
                                #endregion




                            }
                            #endregion
                        }
                        if (orarioPrevisto == "95")
                        {
                            #region codice orario reale lavorativo
                            if (orarioReale == "LV")
                            {
                                if (!L7Gday(g))
                                {
                                    if (e == "MN70")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice MN70 non previsto in caso di MN nella stessa giornata";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice MN70 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }

                                    }
                                    if (e == "MN30")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice MN30 non previsto in caso di MN nella stessa giornata";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice MN30 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }


                                    }
                                }
                                else
                                {
                                    //lavoro in settimo giorno
                                    if (e == "SRH8")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if ((!g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MN")))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codici L7G e MN (lavoro in settimo giorno) obblicatorio in questa giornata per SRH8";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice SRH8 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }

                                    }
                                    if (e == "SRH6")
                                    {
                                        //Se ho mancato non posso chiedere maggiorazione
                                        if ((!g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MN")))
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codici L7G e MN  (lavoro in settimo giorno) obblicatorio in questa giornata per SRH6";
                                            return res;
                                        }


                                        if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                        {
                                            res.esito = true;
                                            res.descrizioneErrore = "";
                                            return res;
                                        }
                                        else
                                        {
                                            res.esito = false;
                                            res.descrizioneErrore = "Codice SRH6 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                            return res;
                                        }


                                    }
                                
                                
                                }
                                if (e == "MN")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MNS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MN non previsto in caso di MNS nella stessa giornata";
                                        return res;
                                    }
                                    
                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                    
                                }
                                if (e == "MNS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MNS non previsto in caso di MN nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }


                            }
                            #endregion
                            #region  codice orario reale 95
                            if (orarioReale == "95")
                            {
                                if (e == "MNS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MNS non previsto in caso di MN nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }
                            }
                            
                            #endregion
                        }
                        if (orarioPrevisto == "96")
                        {
                            #region codice orario reale lavorativo o 96
                            if (orarioReale == "LV" || orarioReale == "96")
                            {
                                if (e == "MR")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MR non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }
                                if (e == "MRS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MR"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MRS non previsto in caso di MR nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }
                                if (e == "SRH6")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH6 non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }

                                    if (((L7Gday(g)) && !g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MR")))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codici L7G e MR (lavoro in settimo giorno) obblicatorio in questa giornata per SRH8";
                                        return res;
                                    }




                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH6 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }
                                }
                                if (e == "SRH8")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH8 non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }

                                    if (((L7Gday(g)) && !g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MR")))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codici L7G e MR (lavoro in settimo giorno) obblicatorio in questa giornata per SRH8";
                                        return res;
                                    }

                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH8 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }

                            }
                            #endregion
                        }

                    }
            #endregion

            #region giornidomenicali
            if (tipoGiorno == "DOM")
                    {
                        if (orarioPrevisto == "LV")
                        {
                            #region codice orario reale lavorativo
                            if (orarioReale == "LV")
                            {
                                if (e == "DH40")
                                {
                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH40 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }
                                if (e == "DH60")
                                {
                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH60 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }
                            }
                            #endregion
                        }
                        if (orarioPrevisto == "95")
                        {
                            #region codice orario reale lavorativo
                            if (orarioReale == "LV")
                            {

                                if (e == "DH60")
                                {
                                    //Se ho mancato non posso chiedere maggiorazione
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MNS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH60 non previsto in caso di MNS nella stessa giornata";
                                        return res;
                                    }


                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH40 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }
                                if (e == "DH40")
                                {
                                    //Se ho mancato non posso chiedere maggiorazione
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MNS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH40 non previsto in caso di MNS nella stessa giornata";
                                        return res;
                                    }


                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice DH40 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }


                                }
                                if (e == "MN")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MNS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MN non previsto in caso di MNS nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;

                                }
                                if (e == "MNS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MNS non previsto in caso di MN nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }


                            }
                            #endregion
                            #region  codice orario reale 95
                            if (orarioReale == "95")
                            {
                                if (e == "MNS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MN"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MNS non previsto in caso di MN nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }
                            }

                            #endregion
                        }
                        if (orarioPrevisto == "96")
                        {
                            #region codice orario reale lavorativo
                            if (orarioReale == "LV")
                            {

                                if (e == "SRH8")
                                {
                                    //Se ho mancato non posso chiedere maggiorazione
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH8 non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }

                                    if (((L7Gday(g)) && !g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MR")))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codici L7G e MR (lavoro in settimo giorno) obblicatorio in questa giornata per SRH8";
                                        return res;
                                    }
                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 0 && Convert.ToInt16(g.orario.hh_entrata_48) <= 6)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else if (Convert.ToInt16(g.orario.hh_entrata_48) >= 20 && Convert.ToInt16(g.orario.hh_entrata_48) <= 30)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH8 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }

                                }
                                if (e == "SRH6")
                                {
                                    //Se ho mancato non posso chiedere maggiorazione
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH6 non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }
                                    if (((L7Gday(g)) && !g.eccezioni.Any(x => x.cod.Trim() == "L7G")) && (!g.eccezioni.Any(x => x.cod.Trim() == "MR")))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codici L7G e MR (lavoro in settimo giorno) obblicatorio in questa giornata per SRH6";
                                        return res;
                                    }

                                    if (Convert.ToInt16(g.orario.hh_entrata_48) >= 6 && Convert.ToInt16(g.orario.hh_entrata_48) <= 20)
                                    {
                                        res.esito = true;
                                        res.descrizioneErrore = "";
                                        return res;
                                    }
                                    else
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice SRH6 non previsto per orario con inizio turno: " + g.orario.hhmm_entrata_48 + " e fine turno: " + g.orario.hhmm_uscita_48;
                                        return res;
                                    }


                                }
                                if (e == "MR")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MR non previsto in caso di MRS nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;

                                }
                                if (e == "MRS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MR"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MRS non previsto in caso di MR nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }


                            }
                            #endregion
                            #region  codice orario reale 96
                            if (orarioReale == "96")
                            {
                                if (e == "MRS")
                                {
                                    if (g.eccezioni.Any(x => x.cod.Trim() == "MR"))
                                    {
                                        res.esito = false;
                                        res.descrizioneErrore = "Codice MRS non previsto in caso di MR nella stessa giornata";
                                        return res;
                                    }

                                    res.esito = true;
                                    res.descrizioneErrore = "";
                                    return res;
                                }
                            }

                            #endregion
                        }
                    }
            #endregion

            #region giorni super festivi
            if (tipoGiorno == "SFE")
            {
                if (orarioPrevisto == "LV" || orarioPrevisto == "90" || orarioPrevisto == "91")
                {
                    #region codice orario reale lavorativo o 90 o 91
                    if (orarioReale == "LV" || orarioReale == "90" || orarioReale == "91")
                    {
                        if (e == "MFS")
                        {
                            if (g.eccezioni.Any(x => x.cod.Trim() == "MF"))
                            {
                                res.esito = false;
                                res.descrizioneErrore = "Codice MF non previsto in caso di MFS nella stessa giornata";
                                return res;
                            }

                            res.esito = true;
                            res.descrizioneErrore = "";
                            return res;
                        }
                    }
                   
                    #endregion


                    if (orarioPrevisto == "92")
                    {
                        #region codice orario reale lavorativo 
                        if (orarioReale == "LV")
                        {
                            if (e == "MR")
                            {
                                if (g.eccezioni.Any(x => x.cod.Trim() == "MRS"))
                                {
                                    res.esito = false;
                                    res.descrizioneErrore = "Codice MR non previsto in caso di MRS nella stessa giornata";
                                    return res;
                                }

                                res.esito = true;
                                res.descrizioneErrore = "";
                                return res;
                            }
                        }
                        #endregion
                        #region codice orario reale 92
                        if (orarioReale == "92")
                        {
                            if (e == "RCF")
                            {
                                res.esito = true;
                                res.descrizioneErrore = "";
                                return res;
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion


            #endregion

            return res;
/*
          

            if (item == null)
                return true;
            else
                return item.Consentito;*/
        }
    }
}
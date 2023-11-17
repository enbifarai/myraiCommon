using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;


namespace myRaiService.classi
{
    public class Eccezione
    {

        public Eccezione()
        {

        }

        public Eccezione(string cod, string min, string tip)
        {
            this.cod = cod;
            this.min = min;
            this.tipEcc = tip;
        }

        public Eccezione(string cod, string min, string tip, string desc)
        {
            this.cod = cod;
            this.min = min;
            this.tipEcc = tip;
            this.descrittiva_lunga = desc;
        }


        public StatoEccezione TipoEccezione(string Stato2000, bool esisteStorno, string storno, string PrimoLivello, string SecondoLivello)
        {
            switch (Stato2000)
            {
                case "D":
                    if (esisteStorno)
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() == "")
                            {
                                if (SecondoLivello.Trim() == "")
                                {
                                    return StatoEccezione.StornatoinGapp;
                                }

                            }
                            else if (PrimoLivello.Trim() != "")
                            {
                                return StatoEccezione.Rifiutato;
                            }
                        }

                    }
                    else
                    {
                        if (PrimoLivello.Trim() == "")
                        {
                            if (SecondoLivello.Trim() == "")
                            {
                                return StatoEccezione.InApprovazione;
                            }
                        }
                    }
                    break;
                case "":
                    if (esisteStorno == false)
                    {
                        if (PrimoLivello.Trim() != "")
                        {
                            return StatoEccezione.Approvato;
                        }
                    }
                    else
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() != "")
                            {

                                return StatoEccezione.StornatoinGapp;

                            }
                        }
                        else if (storno.Trim() == "D")
                        {
                            if (PrimoLivello.Trim() != "")
                            {
                                return StatoEccezione.Approvato;
                            }
                        }
                    }
                    break;
                case "S":
                    if (esisteStorno == false)
                    {
                        if (PrimoLivello.Trim() != "")
                        {
                            //verificare se basta il primo livello
                            if (SecondoLivello.Trim() != "")
                            {
                                return StatoEccezione.Stampato;
                            }

                        }
                    }
                    else
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() != "")
                            {
                                //verificare se basta il primo livello
                                if (SecondoLivello.Trim() != "")
                                {
                                    return StatoEccezione.StornatoinGapp;
                                }

                            }
                        }
                    }
                    break;
                case "C":
                    if (esisteStorno == false)
                    {
                        if (PrimoLivello.Trim() != "")
                        {
                            //verificare se basta il primo livello
                            if (SecondoLivello.Trim() != "")
                            {
                                return StatoEccezione.Convalidato;
                            }

                        }
                    }
                    else
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() != "")
                            {
                                //verificare se basta il primo livello
                                if (SecondoLivello.Trim() != "")
                                {
                                    return StatoEccezione.StornatoinGapp;
                                }

                            }
                        }
                    }
                    break;
                case "P":
                    if (esisteStorno == false)
                    {
                        if (PrimoLivello.Trim() != "")
                        {
                            //verificare se basta il primo livello
                            if (SecondoLivello.Trim() != "")
                            {
                                return StatoEccezione.Pagato;
                            }

                        }
                    }
                    else
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() != "")
                            {
                                //verificare se basta il primo livello
                                if (SecondoLivello.Trim() != "")
                                {
                                    return StatoEccezione.StornatoinGapp;
                                }

                            }
                        }
                    }
                    break;
                case "R":
                    if (esisteStorno == false)
                    {
                        if (PrimoLivello.Trim() != "")
                        {
                            //verificare se basta il primo livello
                            if (SecondoLivello.Trim() != "")
                            {
                                return StatoEccezione.RifiutatodaAmministrazione;
                            }

                        }
                    }
                    else
                    {
                        if (storno.Trim() == "")
                        {
                            if (PrimoLivello.Trim() != "")
                            {
                                //verificare se basta il primo livello
                                if (SecondoLivello.Trim() != "")
                                {
                                    return StatoEccezione.StornatoinGapp;
                                }

                            }
                        }
                    }
                    break;
            }

            return StatoEccezione.NonValorizzato;
        }

        public enum StatoEccezione
        {
            InApprovazione = 1,
            Approvato = 2,
            Rifiutato = 3,
            StornatoinGapp = 4,
            Stampato = 5,
            Convalidato = 6,
            Pagato = 7,
            RifiutatodaAmministrazione = 8,
            NonValorizzato = 0
        }


        public string cod { get; set; }
        public string min { get; set; }
        public string tipEcc { get; set; }

        //Dettaglio
        public string ndoc { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
        public string unita_mis { get; set; }
        public string qta { get; set; }
        public string tipo_eccezione { get; set; }
        public string stato_eccezione { get; set; }
        public string storno_possibile { get; set; }
        public string flg_storno { get; set; }
        public string flg_abbinato { get; set; }
        public string flg_forzato { get; set; }
        public string importo { get; set; }
        public string data { get; set; }
        public string descrittivaConCodice { get; set; }
        public string bustaPaga { get; set; }
        public string dataInserimento { get; set; }
        public string dataValidazione { get; set; }
        public string dataStampa { get; set; }
        public string dataConvalida { get; set; }
        public StatoEccezione statoperDipendente { get; set; }

        //Inserimento
        public string descrittiva { get; set; }
        public string flg_eccezione { get; set; }
        public string descrittiva_lunga { get; set; }
        public string note { get; set; }
        public string rawData { get; set; }

        public string flg_importo { get; set; }
        public string flg_addebito { get; set; }
        public string flg_macroassenza { get; set; }
        public string flg_note { get; set; }
        public string flg_ordinamento { get; set; }

        //Da Validare
        public string sedeGapp { get; set; }
        public string descrittivaSedeGapp { get; set; }
        public string matricola { get; set; }
        public string tipoDipendente { get; set; }
        public string matricolaPrimoLivello { get; set; }
        public string matricolaSecondoLivello { get; set; }
      //  public Dipendente validatoDa { get; set; }
        public bool datoAutomatico { get; set; }
        public string altriDati { get; set; }
        public string descTipoEccezione { get; set; }
        public string descServizioContabile { get; set; }
        public string descSede { get; set; }
        public string dataReversed { get; set; }
        public DateTime dataCompleta { get; set; }
        public bool flg_aggiornato { get; set; }
        public Eccezione eccezionePadre { get; set; }

        //Riepilogo
        public string arretrato { get; set; }
        public bool storno { get; set; }

     //   public Dipendente dipendente { get; set; }


        //Varie
        public string text { get; set; }
        public string value { get; set; }
        public Errore errore { get; set; }

    }
}
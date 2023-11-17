//using myRaiDataTalentia;
using myRaiData.Incentivi;
using System;
using System.Linq;

namespace myRaiGestionale.Services
{
    public class GeneraNuovaMatricolaSevizio
    {
        public static string CalcoloImmmatricolazioneNuoviDipendenti(string nome, string cognome)
        {
            IncentiviEntities db = new IncentiviEntities();

            bool trovatoSpazioMatricola = false;
            string nuova_Matricola = "";
            string lMat = "";
            int trovatoMin = 0;
            int trovatoMax = 0;
            string lMatNext = "";
            int i = 0;
            int distanzaUp = 0;
            int distanzaDown = 0;
            string ricercaMatricola = "";
            int matricolaNewUp = 0;
            int matricolaNewDown = 0;
            int distanza = 0;
            int rsDatiMin = 0;
            int rsDatiMax = 0;
            string nominativo = (cognome + " " + nome).ToUpper();
            bool omonimo = db.V_COMPREL_CM.Any(w => w.NOME == nominativo);

            var resultForMin = db.V_COMPREL_CM.Where(x => x.NOME.CompareTo(nominativo) <= 0)
                                                .OrderByDescending(x => x.NOME)
                                                .ThenBy(x => x.DES_COGNOMEPERS)
                                                .ThenBy(x => x.DES_NOMEPERS)
                                                .ThenBy(x => x.COD_MATLIBROMAT)
                                                .Select(x => x.COD_MATLIBROMAT)
                                                .ToList();
            
            var resultForMax = db.V_COMPREL_CM.Where(x => x.NOME.CompareTo(nominativo) > 0)
                                                .OrderBy(x => x.NOME)
                                                .ThenBy(x => x.DES_COGNOMEPERS)
                                                .ThenBy(x => x.DES_NOMEPERS)
                                                .ThenBy(x => x.COD_MATLIBROMAT)
                                                .Select(x => x.COD_MATLIBROMAT)
                                                .ToList();

            if ((resultForMin.Count() > 0) && (resultForMax.Count() > 0))
            {
                var l_rsdatiMIN = resultForMin.First();
                var l_rsdatiMAX = resultForMax.First();
                distanza = CalcolaDistanza(l_rsdatiMIN, l_rsdatiMAX);
                // Se distanza == -1 Non c'è spazio
                if (distanza != -1)
                {
                    string genera_Matricola = Convert.ToString(distanza).PadLeft(6, '0');
                    var recordMatricolaEsistente = db.V_COMPREL_CM.FirstOrDefault(w => w.COD_MATLIBROMAT == genera_Matricola);
                    if (recordMatricolaEsistente == null)
                    {
                        string nuova_matricola = genera_Matricola.PadLeft(6, '0');
                        trovatoSpazioMatricola = true;
                        return nuova_matricola;
                    }
                }

                if (!trovatoSpazioMatricola)
                {

                    for (i = 0; i <= resultForMin.Count() - 2; i++)
                    {
                        lMat = resultForMin[i];
                        lMatNext = resultForMin[i + 1];
                        if ((CalcolaDistanza(lMatNext, lMat)) != -1)
                        {
                            distanzaUp = i;
                            if (omonimo)
                            {
                                //ricercaMatricola = "000000" + (lMat + 1.ToString()).PadLeft(3);
                                //lMat = (Convert.ToInt32(lMat) + 1).ToString();
                                ricercaMatricola = (Convert.ToInt32(lMat) + 1).ToString().PadLeft(6, '0');
                                var reconrdMatricolaRicerca = db.V_COMPREL_CM.FirstOrDefault(w => w.COD_MATLIBROMAT == ricercaMatricola);
                                if (reconrdMatricolaRicerca == null)
                                {
                                    trovatoMin = i;
                                    matricolaNewUp = Convert.ToInt32(lMat) + 1;
                                    break;

                                }
                            }
                            else
                            {
                                matricolaNewUp = CalcolaDistanza(lMat, lMatNext);
                                ricercaMatricola = Convert.ToString(matricolaNewUp).PadLeft(6, '0');
                                string matricolaNewUpString = (matricolaNewUp).ToString();
                                var controlloRicercaMatricola = db.V_COMPREL_CM.FirstOrDefault(w => w.COD_MATLIBROMAT == ricercaMatricola & w.COD_MATLIBROMAT == matricolaNewUpString);
                                if (controlloRicercaMatricola == null)
                                {
                                    trovatoMin = rsDatiMin;
                                    break;
                                }
                                controlloRicercaMatricola = null;
                            }
                        }
                        rsDatiMin = i;
                    }

                    for (i = 0; i <= resultForMax.Count() - 2; i++)
                    {
                        lMat = resultForMax[i];
                        lMatNext = resultForMax[i + 1];
                        if ((CalcolaDistanza(lMatNext, lMat)) != -1)
                        {
                            distanzaDown = i;
                            if (omonimo)
                            {
                                //lMat = (Convert.ToInt32(lMat) + 1).ToString();
                                ricercaMatricola = (Convert.ToInt32(lMat) + 1).ToString().PadLeft(6, '0');
                                var reconrdMatricolaRicerca = db.V_COMPREL_CM.FirstOrDefault(w => w.COD_MATLIBROMAT == ricercaMatricola);
                                if (reconrdMatricolaRicerca == null)
                                {
                                    matricolaNewDown = Convert.ToInt32(lMat) + 1;
                                    trovatoMax = i;
                                    break;
                                }
                            }
                            else
                            {
                                matricolaNewDown = CalcolaDistanza(lMat.ToString(), lMatNext);
                                ricercaMatricola = Convert.ToString(matricolaNewDown).PadLeft(6, '0');
                                var controlloRicercaMatricola = db.V_COMPREL_CM.FirstOrDefault(w => w.COD_MATLIBROMAT == ricercaMatricola);
                                if (controlloRicercaMatricola == null)
                                {
                                    trovatoMax = i;
                                    break;
                                }
                            }
                            rsDatiMax = i;

                        }
                    }


                    if (distanzaDown >= distanzaUp)
                    {
                        ricercaMatricola = Convert.ToString(matricolaNewUp).PadLeft(6, '0');
                    }
                    else
                    {
                        ricercaMatricola = Convert.ToString(matricolaNewDown).PadLeft(6, '0');
                    }

                    return ricercaMatricola;

                }
            }
            else if (resultForMax.Count() == 0 & resultForMin.Count() > 0)
            {
                for (int j = 0; j < resultForMin.Count; j++)
                {
                    nuova_Matricola = CalcolaDistanza(resultForMin[j], "989999").ToString();
                }
                nuova_Matricola = Convert.ToString(nuova_Matricola).PadLeft(6, '0');
            }
            else if (resultForMin.Count() == 0 & resultForMax.Count() > 0)
            {
                for (int j = 0; j < resultForMax.Count; j++)
                {
                    nuova_Matricola = CalcolaDistanza("0", resultForMax[j]).ToString();
                }
                nuova_Matricola = Convert.ToString(nuova_Matricola).PadLeft(6, '0');
            }
            else
            {
                nuova_Matricola = "000001";
            }
            return nuova_Matricola;
        }

        private static int CalcolaDistanza(string matricola, string matricolaNext)
        {
            int delta = 0;
            int distanza = -1;
            int intMatr = Convert.ToInt32(matricola);
            int intOtherMatr = Convert.ToInt32(matricolaNext);

            int differenza = Math.Abs(intOtherMatr - intMatr);

            if (differenza > 1)
            {
                delta = differenza / 2;
                if (intOtherMatr > intMatr)
                {
                    distanza = intMatr + delta;
                }
                else
                {
                    distanza = intOtherMatr + delta;
                }
            }
            return distanza;
        }
    }
}
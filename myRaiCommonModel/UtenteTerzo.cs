using System;
using System.Linq;
using MyRaiServiceInterface.MyRaiServiceReference1;
using myRaiData;
using myRaiHelper;

namespace myRaiCommonModel
{
    public class UtenteTerzo
    {

        public UtenteTerzoAnagrafica EsponiAnagrafica(string matricola)
        {
            string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
              
            string[] temp = str_temp.ToString().Split(';');

            if ((temp != null) && (temp.Count() > 16))
            {
                return this.CaricaAnagrafica(temp, matricola);
            }
            else return null;
        }

        public UtenteTerzoAnagrafica CaricaAnagrafica(string[] resp, string matricola)
        {
            if ((resp != null) && (resp.Count() > 1))
            {
                UtenteTerzoAnagrafica anag = new UtenteTerzoAnagrafica();

                anag._cognome = resp[2];
                anag._nome = resp[1];
                anag._foto = CommonHelper.GetImmagineBase64(matricola);
                anag._comuneNascita = resp[12];
                anag._contratto = resp[6];
                anag._figProfessionale = resp[8];
                anag._qualifica = resp[10];
                anag._dataNascita = (resp[11]) != null ? CommonHelper.ConvertToDate(resp[11]) : Convert.ToDateTime(null);
                anag._comuneNascita = resp[12];
                anag._matricola = resp[0];
                anag._logo = resp[14];
                anag._dataAssunzione = (resp[3]) != null ? CommonHelper.ConvertToDate(resp[3]) : Convert.ToDateTime(null);

                anag._codiceFigProf = resp[7];
                anag._codiceContratto = resp[5];
                anag._codiceQualifica = resp[9];

                anag._statoNascita = resp[13];
                anag._email = resp[15];
                anag._telefono = resp[16];

                using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
                {
                    var param = new System.Data.SqlClient.SqlParameter("@param", resp[4]);

                    try
                    {
                        var tmp = ctx.Database.SqlQuery<string>("exec sp_GERARSEZIONE @param", param).ToList();
                        anag._inquadramento = tmp[0].ToString();// "RUO;Amministrazione;Sistemi del personale";//freak - nella function dbo.GERARSEZIONE dal secondo elemento è la sezione - resp[4]

                    }
                    catch (Exception)
                    {

                        // throw;
                    }

                }

                return anag;
            }
            else
            {
                return null;
            }
        }


        public GetAnalisiEccezioniResponse GetAnalisiEcc(string currentMatricola, string matricola)
        {
            try
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni(matricola,
                                                                            new DateTime(DateTime.Now.Year, 1, 1),
                                                                            DateTime.Now,
                                                                            "POH",
                                                                            "ROH",
                                                                            null
                                                                            );

                foreach (var d in response.DettagliEccezioni) d.data = new DateTime(DateTime.Now.Year, d.data.Month, d.data.Day);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = currentMatricola,
                    provenienza = "UtenteTerzo.GetAnalisiEcc()",
                    error_message = ex.ToString()
                });
                return null;
            }
        }
    }



    public class CV_DescTitoloLogo
    {
        public string DescTipoTitolo { get; set; }
        public string DescTitolo { get; set; }
        public string Logo { get; set; }
    }


}

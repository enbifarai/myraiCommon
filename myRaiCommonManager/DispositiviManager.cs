using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace myRaiCommonManager
{
    public class DispositiviManager
    {
        public static string SendPushNotification(string apikey, string token, string title, string text, PushNotificationType? PushType = null, string SpecificData = null)
        {
            WebClient w = new WebClient();
            w.Headers.Add(HttpRequestHeader.Authorization, "key=" + apikey);
            w.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            string content = "{" +
                                  "\"data\": {" +
                                                "\"title\": \"" + title + "\"," +
                                        "\"body\": \"" + text + "\"" +
                                      (PushType != null ? ",\"type\" :\"" + PushType.ToString() + "\"" +
                                                        ",\"specdata\" :\"" + SpecificData + "\"" : "") +
                                    "}," +

                                  "\"to\":\"" + token + "\"" +
                            "}";
            string HtmlResult = w.UploadString(new Uri("https://fcm.googleapis.com/fcm/send"), content);

            return HtmlResult;
        }
        public static void SendPushMessaggioSegreteria(string sedegapp)
        {
            try
            {
                SendPushMessaggioSegreteria_execute(sedegapp);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "SendPushMessaggioSegreteria",
                    error_message = ex.ToString()
                });
            }
        }
        private static void SendPushMessaggioSegreteria_execute(string sedegapp)
        {
            var abs = BatchManager.GetAbilitazioniSeg();
            var listabs = abs.ListaAbilitazioni.Where(x => x.Sede == sedegapp).Select(z => z.MatrLivello1).FirstOrDefault();
            var listaMatrSegreterie = listabs.Select(x => x.Matricola).ToList();

            var abu = BatchManager.GetAbilitazioniUffPers();
            var listabu = abu.ListaAbilitazioni.Where(x => x.Sede == sedegapp).Select(z => z.MatrLivello1).FirstOrDefault();
            listaMatrSegreterie.AddRange(listabu.Select(x => x.Matricola).ToList());

            listaMatrSegreterie = listaMatrSegreterie.Distinct().ToList();

            var db = new myRaiData.digiGappEntities();
            string pmatr = CommonHelper.GetCurrentUserPMatricola();
            string apiKey = CommonHelper.GetParametro<string>(EnumParametriSistema.PushAPIkey);
            string nominativo = myRai.Models.Utente.Nominativo().Trim();

            foreach (var m in listaMatrSegreterie)
            {
                var listaDevices = db.MyRai_MobileRegistration.Where(x =>
                x.pmatricola == m
                && x.abilitato == true
                && x.token != null
                && x.token != "").ToList();

                if (listaDevices.Any())
                {
                    var resp = MobileManager.GetMessaggiSegreteria(m.ToLower().Trim('p'), false);
                    int counter = 0;
                    if (resp != null && resp.note != null)
                        counter = resp.note.Count;

                    foreach (var d in listaDevices)
                    {
                        string msg = nominativo + " (" + sedegapp + ")" + " ha aggiunto una nota!";
                        SendPushNotification(apiKey, d.token, "RaiPerMe", msg,
                            PushNotificationType.MesSeg, counter.ToString());

                        Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            matricola = CommonHelper.GetCurrentUserMatricola(),
                            provenienza = "SendPushMessaggioSegreteria_execute",
                            operazione = "SendPushNotification",
                            descrizione_operazione = msg + " - Sent to:" + m + " Token:" + d.token
                        });
                    }
                }
            }
        }
        public static void SendPushTest(int id)
        {
            string PushResult = "";
            var db = new myRaiData.digiGappEntities();
            var mobile = db.MyRai_MobileRegistration.Where(x => x.id == id).FirstOrDefault();
            if (mobile != null)
            {
                string apikey = CommonHelper.GetParametro<string>(EnumParametriSistema.PushAPIkey);
                if (!String.IsNullOrWhiteSpace(apikey))
                {
                    PushResult = DispositiviManager.SendPushNotification(apikey, mobile.token, 
                        "RaiPerMe", "Questa e' una notifica di prova dal sistema RaiPerme", PushNotificationType.MesSeg, "8");
                }
            }
        }
    }
    public enum PushNotificationType
    {
        MesSeg
    }
}

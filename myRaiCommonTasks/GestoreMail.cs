using myRaiCommonTasks.sendMail;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
    public class GestoreMail
    {
        public class MailResponse
        {
            public bool Esito { get; set; }
            public string Errore { get; set; }
        }

        /// <summary>
        /// Metodo che si occupa di inviare una mail in formato standard, vedi template MailTemplateDefault
        /// Prende in input il mittente, l'oggetto, il/i destinatario/i, il/i destinatario/i CC, ed una serie di valori che serviranno
        /// per sostituire il contenuto della mail di default.
        /// Il template di default ha un titolo, sottotitolo, corpo, ed un bottone formato da testo e link
        /// </summary>
        /// <param name="mittente">Mittente della mail</param>
        /// <param name="oggetto">Oggetto della mail</param>
        /// <param name="destinatari">Destinatario o destinatari della mail. Nel caso di più destinatari gli indirizzi vanno separati con la virgola</param>
        /// <param name="destinatariCC">Destinatario o destinatari in copia alla mail. Nel caso di più destinatari gli indirizzi vanno separati con la virgola</param>
        /// <param name="titolo">Testo che prenderà il posto del titolo nel template</param>
        /// <param name="sottotitolo">Testo che prenderà il posto del sottotitolo nel template</param>
        /// <param name="corpo">Testo che prenderà il posto del corpo nel template</param>
        /// <param name="testoBottone">Testo dell'eventuale bottone. Di default sarà presente il testo "VAI AL SITO"</param>
        /// <param name="linkBottone">Eventuale link raggiungibile al click del bottone. Di default http://raiperme.rai.it</param>
        /// <param name="attachments">Eventuali allegati alla mail</param>
        /// <returns></returns>
        public MailResponse InvioMail(string mittente, string oggetto, string destinatari, string destinatariCC = null, string titolo = null, string sottotitolo = null, string corpo = null, string testoBottone = "VAI AL SITO", string linkBottone = "http://raiperme.rai.it", List<Attachement> attachments = null, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse response = new MailResponse();
            response.Esito = true;
            response.Errore = null;
            try
            {
                string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.MailTemplateDefault);

                string body = MailParams[0];

                if (!String.IsNullOrEmpty(titolo))
                {
                    body = body.Replace("##TITOLO##", titolo);
                }
                else
                {
                    body = body.Replace("##TITOLO##", "");
                }

                if (!String.IsNullOrEmpty(sottotitolo))
                {
                    body = body.Replace("##SOTTOTITOLO##", sottotitolo);
                }
                else
                {
                    body = body.Replace("##SOTTOTITOLO##", "");
                }

                if (!String.IsNullOrEmpty(corpo))
                {
                    body = body.Replace("##CORPO##", corpo);
                }
                else
                {
                    body = body.Replace("##CORPO##", "");
                }

                if (!String.IsNullOrEmpty(testoBottone))
                {
                    body = body.Replace("##BOTTONE##", testoBottone);
                }
                else
                {
                    body = body.Replace("##BOTTONE##", "");
                }

                if (!String.IsNullOrEmpty(linkBottone))
                {
                    body = body.Replace("##LINK##", linkBottone);
                }
                else
                {
                    body = body.Replace("##LINK##", "");
                }

                string contentType = "text/html";
                int priority = 2;

                response = InternalInvioMail(body, oggetto, destinatari, destinatariCC, mittente, priority, contentType, attachments, dataProgrammata, destinatariCCN);

                Logger.Log(String.Format("Inviata email a {0}, con oggetto: {1} in data {2} ", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }
            return response;
        }

        /// <summary>
        /// Attraverso questo metodo è possibile inviare una mail con un template preso dal db in base al parametro passato
        /// Il dictionary "toReplace" è formato da un insieme di chiave, valore utili per sostituire i dati all'interno del template
        /// </summary>
        /// <param name="mittente">Mittente della mail</param>
        /// <param name="oggetto">Oggetto della mail</param>
        /// <param name="template">Nome del template da cercare sul db</param>
        /// <param name="destinatari">Destinatario o destinatari della mail. Nel caso di più destinatari gli indirizzi vanno separati con la virgola</param>
        /// <param name="destinatariCC">Destinatario o destinatari in copia alla mail. Nel caso di più destinatari gli indirizzi vanno separati con la virgola</param>
        /// <param name="toReplace">Chiave, valore che verranno sostituiti nel template. La chiave è il tag presente nel template, valore è il testo che verrà inserito al suo posto</param>
        /// <param name="attachments">Eventuali allegati alla mail</param>
        /// <returns></returns>
        public MailResponse InvioMail(string mittente, string oggetto, CommonTasks.EnumParametriSistema template, string destinatari, string destinatariCC = null, Dictionary<string, string> toReplace = null, List<Attachement> attachments = null, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse response = new MailResponse();
            response.Esito = true;
            response.Errore = null;
            try
            {
                string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>(template);

                string body = MailParams[0];

                if (toReplace != null && toReplace.Any())
                {
                    foreach (var d in toReplace)
                    {
                        body = body.Replace(d.Key, d.Value);
                    }
                }

                string contentType = "text/html";
                int priority = 2;

                response = InternalInvioMail(body, oggetto, destinatari, destinatariCC, mittente, priority, contentType, attachments, dataProgrammata, destinatariCCN);

                Logger.Log(String.Format("Inviata email a {0}, con oggetto: {1} in data {2} ", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }
            return response;
        }

        public MailResponse InvioMail(string corpo, string oggetto, string destinatari, string destinatariCC, string from, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse response = new MailResponse();
            response.Esito = true;
            response.Errore = null;

            try
            {
                string contentType = "text/html";
                int priority = 2;

                response = InternalInvioMail(corpo, oggetto, destinatari, destinatariCC, from, priority, contentType, null, dataProgrammata, destinatariCCN);

                Logger.Log(String.Format("Inviata email a {0}, con oggetto: {1} in data {2} ", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }
            return response;
        }

        public MailResponse InvioMail(string corpo, string oggetto, string destinatari, string destinatariCC, myRaiCommonTasks.CommonTasks.EnumParametriSistema from, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse response = new MailResponse();
            response.Esito = true;
            response.Errore = null;

            try
            {
                string _from = CommonTasks.GetParametro<string>(from);
                string contentType = "text/html";
                int priority = 2;

                response = InternalInvioMail(corpo, oggetto, destinatari, destinatariCC, _from, priority, contentType, null, dataProgrammata, destinatariCCN);

                Logger.Log(String.Format("Inviata email a {0}, con oggetto: {1} in data {2} ", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }
            return response;
        }

        public MailResponse InvioMail(string corpo, string oggetto, string destinatari, string destinatariCC, string from, List<Attachement> attachments = null, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse response = new MailResponse();
            response.Esito = true;
            response.Errore = null;

            try
            {
                string contentType = "text/html";
                int priority = 2;

                response = InternalInvioMail(corpo, oggetto, destinatari, destinatariCC, from, priority, contentType, attachments, dataProgrammata, destinatariCCN);

                Logger.Log(String.Format("Inviata email a {0}, con oggetto: {1} in data {2} ", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }
            return response;
        }

        private MailResponse InternalInvioMail(string corpo, string oggetto, string destinatari, string destinatariCC, string from, int priority = 2, string formatoMail = null, List<Attachement> attachments = null, DateTime? dataProgrammata = null, string destinatariCCN = null)
        {
            MailResponse result = new MailResponse();
            result.Errore = null;
            result.Esito = true;

            try
            {
                digiGappEntities db = new digiGappEntities();

                var destinatarioCCNAutomatico = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("DestinatarioCCNAutomatico")).FirstOrDefault();

                if (destinatarioCCNAutomatico != null)
                {
                    string sAttivo = destinatarioCCNAutomatico.Valore1;
                    string sIndirizzo = destinatarioCCNAutomatico.Valore2;
                    bool attivo = false;

                    bool conversione = bool.TryParse(sAttivo, out attivo);
                    // se la conversione è ok
                    // è attiva l'aggiunta del ccn automatico e
                    // sIndirizzo è valorizzato
                    if (conversione && attivo && !String.IsNullOrEmpty(sIndirizzo))
                    {
                        if (String.IsNullOrEmpty(destinatariCCN))
                        {
                            destinatariCCN = sIndirizzo;
                        }
                        else
                        {
                            if (!destinatariCCN.Contains(sIndirizzo))
                            {
                                destinatariCCN += "," + sIndirizzo;
                            }
                        }
                    }
                }

                if (String.IsNullOrEmpty(formatoMail))
                {
                    formatoMail = "text/html";
                }

                MyRai_InvioMail data = new MyRai_InvioMail()
                {
                    Body = corpo,
                    DataUltimoTentativo = DateTime.Now,
                    Destinatario = destinatari,
                    DestinatarioCC = destinatariCC,
                    Errore = null,
                    Esito = null,
                    FormatoHtml = true,
                    Mittente = from,
                    Oggetto = oggetto,
                    Priorita = priority,
                    DataProgrammata = dataProgrammata,
                    TentativiInvio = 1,
                    DestinatariCCN = destinatariCCN
                };

                if (attachments != null && attachments.Any())
                {
                    List<MyRai_InvioMail_Allegati> all = new List<MyRai_InvioMail_Allegati>();
                    foreach (var a in attachments)
                    {
                        all.Add(new MyRai_InvioMail_Allegati()
                        {
                            NomeFile = a.AttachementName,
                            ContentByte = a.AttachementValue,
                            TipoFile = a.AttachementType
                        });
                    }

                    data.MyRai_InvioMail_Allegati = all;
                }

                db.MyRai_InvioMail.Add(data);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errore = ex.Message;
                result.Esito = false;
                Logger.Log(String.Format("Si è verificato un errore durante l'invio della email a {0}, con oggetto: {1} in data {2} \r\nErrore: {3}", destinatari, oggetto, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), ex.Message));
            }

            return result;
        }
    }
}
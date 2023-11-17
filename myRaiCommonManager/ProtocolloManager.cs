using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.Business;
using myRaiCommonManager._Model;
using myRaiCommonModel;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.raiconnectcoll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace myRaiCommonManager
{
    public class ProtocolloManager
    {
        /// <summary>
        /// Metodo che si occupa di chiamare il servizio di protocollazione ed ottiene il numero protocollo ed il riferimento all'iddocumento.
        /// </summary>
        /// <param name="matricolaMittente"></param>
        /// <param name="matricolaDestinatario"></param>
        /// <param name="oggetto"></param>
        /// <param name="leftPosition"></param>
        /// <param name="topPosition"></param>
        /// <param name="codiceProtocollatore"></param>
        /// <returns></returns>
        public static CreaProtocolloResult CreaProtocollo(string matricolaMittente, string matricolaDestinatario, string oggetto, string codiceProtocollatore, DateTime? dataProtocollo = null)
        {
            DateTime ora = DateTime.Now;
            if (dataProtocollo.HasValue)
            {
                ora = dataProtocollo.GetValueOrDefault();
            }

            CreaProtocolloResult result = new CreaProtocolloResult();
            result.ServiceResult = new CreaProtocolloServiceResult();
            result.ServiceResult.File = null;
            result.ServiceResult.Id_Documento = "";
            result.ServiceResult.Protocollo = "";
            result.Esito = true;

            try
            {
                #region CreaProtocollo

                Protocollo prot = new Protocollo();
                prot.Mittente = matricolaMittente;
                string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola(matricolaDestinatario);

                if (!String.IsNullOrEmpty(dest))
                {
                    List<Destinatario> _d = new List<Destinatario>();

                    prot.Destinatari = new Destinatari();
                    prot.Destinatari.Destinatario = new List<Destinatario>();
                    _d.Add(new Destinatario()
                    {
                        IndirizzoMail = dest,
                        TipoCanale = "mail",
                        Text = dest
                    });

                    prot.Destinatari.Destinatario = _d;
                }
                else
                {
                    throw new Exception("Impossibile reperire l'indirizzo mail destinatario " + dest);
                }

                string nominativoDestinatario = CezanneHelper.GetNominativoByMatricola(matricolaDestinatario);
                prot.Oggetto = oggetto;
                prot.DataSpedizione = ora.ToString("yyyy-MM-dd");
                string strProt = SerializerHelper.SerializeObject(prot);
                //string codiceProtocollatore = "42936";

                //URL COLLAUDO:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                //URL PRODUZIONE: http://raiconnect.intranet.rai.it/rai_ruo_ws.asmx
                string user = "srv_raiconnect_ruo";
                string pwd = "6E6asOXO";
                rai_ruo_ws client = new rai_ruo_ws();
                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                //client.UseDefaultCredentials = true;

                ScriviLogAzioni(client.Url.ToString(), "LOG_URL_ApplicaProtocollo", matricolaMittente);

                // codice creatore del documento                
                var tmpProt = client.creaProtocollo(codiceProtocollatore, "P" + matricolaMittente, strProt);

                CreaProtocollo outputServizio = new CreaProtocollo();

                var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                outputServizio = (CreaProtocollo)p;

                if (outputServizio.Errore != null &&
                    outputServizio.Errore.Id_errore != "0" &&
                    !String.IsNullOrEmpty(outputServizio.Errore.Text))
                {
                    throw new Exception(outputServizio.Errore.Text);
                }

                result.ServiceResult.Id_Documento = outputServizio.Id_documento;
                result.ServiceResult.Protocollo = outputServizio.Identificativo;

                return result;
                #endregion

            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
                result.ServiceResult = null;
                ScriviLogErrore(ex.Message, "CreaProtocollo", matricolaMittente);
            }

            return result;
        }

        /// <summary>
        /// Il metodo si occupa di applicare il numero di protocollo e la data di protocollazione 
        /// sul documento passato 
        /// </summary>
        /// <param name="protocollo"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="dataPosLeft"></param>
        /// <param name="dataPosTop"></param>
        /// <param name="originePDF"></param>
        /// <returns></returns>
        public static byte[] ApplicaProtocolloSulPDF(string protocollo, float left, float top,
                            float? dataPosLeft, float? dataPosTop, DateTime? data, byte[] originePDF)
        {
            byte[] result = null;

            try
            {
                using (var reader = new PdfReader(originePDF))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document document = null;
                        document = new Document(reader.GetPageSizeWithRotation(1));

                        var writer = PdfWriter.GetInstance(document, ms);
                        float width = document.PageSize.Width;
                        float height = document.PageSize.Height;

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            var contentByte = writer.DirectContent;

                            var rotation = reader.GetPageRotation(i);

                            switch (rotation)
                            {
                                case 90:
                                    contentByte.AddTemplate(importedPage, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                    break;
                                // TODO case 180
                                case 270:
                                    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                    break;
                                default:
                                    contentByte.AddTemplate(importedPage, 0, 0);
                                    break;
                            }

                            contentByte.BeginText();
                            contentByte.SetFontAndSize(baseFont, 12);

                            if ((595 - left) < 142)
                            {
                                // il margine sinistro va oltre il documento
                                left = 595 - 142;
                            }

                            if (i == 1)
                            {
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, protocollo, left, 842 - top - 12, 0);

                                if (dataPosLeft.HasValue && dataPosTop.HasValue && data.HasValue)
                                {
                                    contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT,
                                        data.GetValueOrDefault().ToString("dd/MM/yyyy"),
                                        dataPosLeft.GetValueOrDefault(),
                                        842 - dataPosTop.GetValueOrDefault() - 12,
                                        0);
                                }
                            }

                            contentByte.EndText();
                        }

                        document.Close();
                        writer.Close();
                        result = ms.ToArray();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Upload di un file al protocollo con id riferimento : idDocumento
        /// </summary>
        /// <param name="matricolaMittente"></param>
        /// <param name="matricolaDestinatario"></param>
        /// <param name="oggetto"></param>
        /// <param name="codiceProtocollatore"></param>
        /// <param name="idDocumento"></param>
        /// <param name="nomeFile"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ApplicaDocumentoAlProtocolloResult ApplicaDocumentoAlProtocollo(string matricolaMittente, string matricolaDestinatario, string oggetto, string codiceProtocollatore, string idDocumento, string nomeFile, byte[] file, string filePrincipale="1", string idAttach="0")
        {
            ApplicaDocumentoAlProtocolloResult result = new ApplicaDocumentoAlProtocolloResult();
            result.Esito = false;

            try 
            {
                string nominativoDestinatario = CezanneHelper.GetNominativoByMatricola(matricolaDestinatario);
                string pmatr = "";

                if (!matricolaMittente.ToUpper().StartsWith("P"))
                {
                    pmatr = "P" + matricolaMittente;
                }

                if (oggetto.Length > 80)
                {
                    oggetto = oggetto.Substring(0, 80);
                }

                string base64String = Convert.ToBase64String(file, 0, file.Length);

                ApplicaDocumentoAlProtocolloResult uploadDocumentoProtocollato = InserisciAllegatoProtocollo(codiceProtocollatore, pmatr, idDocumento,
                    nomeFile, oggetto, base64String, filePrincipale, idAttach);

                if (!uploadDocumentoProtocollato.Esito)
                {
                    throw new Exception(uploadDocumentoProtocollato.DescrizioneErrore);
                }

                result.Esito = true;
                result.Id_Documento = uploadDocumentoProtocollato.Id_Documento;

                string txLog = String.Format("Allegato aggiunto al protocollo per la matricola: {0} riferimento idProtocollo: {1}", pmatr, idDocumento, idDocumento);
                ScriviLogAzioni(txLog, "ApplicaDocumentoAlProtocollo", matricolaMittente);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
                ScriviLogErrore("Errore durante l'upload del file al protocollo " + ex.Message, "ApplicaDocumentoAlProtocollo", matricolaMittente);
            }
            return result;
        }

        /// <summary>
        /// Metodo interno che si occupa dell'upload di un file, riferito ad un particolare numero di protocollo.
        /// </summary>
        /// <param name="codiceProtocollatore"></param>
        /// <param name="pMatricola"></param>
        /// <param name="id_documento"></param>
        /// <param name="nome_file"></param>
        /// <param name="descrizione_file"></param>
        /// <param name="base64"></param>
        /// <param name="filePrincipale"></param>
        /// <param name="idAttach"></param>
        /// <returns></returns>
        public static ApplicaDocumentoAlProtocolloResult InserisciAllegatoProtocollo(string codiceProtocollatore, string pMatricola,
                                                        string id_documento, string nome_file, string descrizione_file,
                                                        string base64, string filePrincipale, string idAttach)
        {
            DateTime ora = DateTime.Now;
            ApplicaDocumentoAlProtocolloResult result = new ApplicaDocumentoAlProtocolloResult();
            result.Esito = false;

            try
            {
                //id_ruolo = "42936";

                //URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                string user = "srv_raiconnect_ruo";
                string pwd = "6E6asOXO";
                rai_ruo_ws client = new rai_ruo_ws();
                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                //client.UseDefaultCredentials = true;
                ScriviLogAzioni(client.Url.ToString(), "LOG_URL_InserisciAllegatoProtocollo", pMatricola);
                var tmpProt = client.inserisciAllegato(codiceProtocollatore, pMatricola, id_documento, nome_file, descrizione_file, base64, filePrincipale, idAttach);
                InserisciAllegato outputServizio = new InserisciAllegato();

                var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                outputServizio = (InserisciAllegato)p;

                if (outputServizio.Errore != null &&
                    outputServizio.Errore.Id_errore != "0" &&
                    !String.IsNullOrEmpty(outputServizio.Errore.Text))
                {
                    throw new Exception(outputServizio.Errore.Text);
                }

                result.Esito = true;
                result.Id_Documento = outputServizio.Id_attach;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
                ScriviLogErrore("Errore durante l'upload del file al protocollo " + ex.Message, "InserisciAllegatoProtocollo", pMatricola);
            }

            return result;
        }
        public static string EseguiRicercaGetIdDocByProtocol(string id_ruolo, string serfiltri, bool AllResponse=false)
        {
            string user = "srv_raiconnect_ruo";
            string pwd = "6E6asOXO";
            rai_ruo_ws client = new rai_ruo_ws();
            client.Credentials = new System.Net.NetworkCredential(user, pwd);
            //client.UseDefaultCredentials = true;
            string tmpProt = client.eseguiRicerca(
                id_ruolo: id_ruolo, 
                matricola: "P103650", 
                id_ricerca: "1502", 
                top: "1", 
                filtro_ricerca: serfiltri);

            if (AllResponse) return tmpProt;

            XElement xdoc = XElement.Parse(tmpProt);
            if (xdoc == null) return null;

            try
            {
                IEnumerable<XElement> t = xdoc.Descendants("Recordset").Descendants("Record").Descendants("id_documento");
                if (t != null && t.Count() > 0)
                {
                    return t.FirstOrDefault().Value;
                }
                else return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// Metodo che si occupa di chiamare il servizio di protocollazione per generare un numero protocollo
        /// applica il numero ed eventuale data di protocollazione al documento passato, infine
        /// fa l'upload del documento tramite il servizio di protocollazione così da associare il file al numero
        /// di protocollo generato
        /// </summary>
        /// <param name="matricolaMittente">Matricola dell'utente mittente</param>
        /// <param name="matricolaDestinatario">Matricola dell'utente per il quale si sta protocollando il documento</param>
        /// <param name="oggetto">Oggetto del protocollo</param>
        /// <param name="codiceProtocollatore">Codice protocollatore, vedi tabella [XR_HRIS_PROTOCOLLI]</param>
        /// <param name="left">Margine sinistro dove verrà applicato il numero di protocollo</param>
        /// <param name="top">Margine superiore dove verrà applicato il numero di protocollo</param>
        /// <param name="dataPosLeft">Margine sinistro dove verrà applicata la data di protocollazione</param>
        /// <param name="dataPosTop">Margine superiore dove verrà applicata la data di protocollazione</param>
        /// <param name="nomeFile">Nome del file che si sta protocollando</param>
        /// <param name="file">ByteArray. Contenuto del file in bytearray</param>
        /// <returns>Restituisce un oggetto che contiene l'esito dell'operazione, eventuale descrizione dell'errore, codice protocollo generato e id documento di riferimento, eventuale file
        /// non su applicato il protocollo appena generato se l'operazione va a buon fine</returns>
        public static CreaProtocolloResult CreaProtocolloCompleto(string matricolaMittente, string matricolaDestinatario, string oggetto, string codiceProtocollatore, float left, float top, float? dataPosLeft, float? dataPosTop, string nomeFile, byte[] file)
        {
            CreaProtocolloResult result = new CreaProtocolloResult();
            result.Esito = false;

            try
            {
                DateTime oraProtocollo = DateTime.Now;
                if (file == null)
                {
                    throw new Exception("Il file è obbligatorio");
                }

                string matricolaSenzaP = matricolaMittente.ToUpper().Replace("P", "");

                // crea il protocollo
                result = CreaProtocollo(matricolaSenzaP, matricolaDestinatario, oggetto, codiceProtocollatore, oraProtocollo);

                if (!result.Esito)
                {
                    throw new Exception(result.DescrizioneErrore);
                }

                if (result.ServiceResult == null)
                {
                    throw new Exception("Errore in creazione protocollo");
                }

                ScriviLogAzioni("Generato protocollo n° " + result.ServiceResult.Protocollo + " id documento riferimento: " + result.ServiceResult.Id_Documento +
                                " matricola mittente: " + matricolaMittente + " matricola destinatario: " + matricolaDestinatario + " oggetto: " + oggetto +
                                " codice protocollatore: " + codiceProtocollatore, "CreaProtocolloCompleto");

                // applica il protocollo sul documento PDF
                file = ApplicaProtocolloSulPDF(result.ServiceResult.Protocollo, left, top, dataPosLeft, dataPosTop, oraProtocollo, file);

                if (file == null)
                {
                    throw new Exception("Si è verificato un errore nell'applicazione del protocollo al documento");
                }

                ScriviLogAzioni("Applicato al documento il protocollo n° " + result.ServiceResult.Protocollo + " id documento riferimento: " + result.ServiceResult.Id_Documento +
                                " matricola mittente: " + matricolaMittente + " matricola destinatario: " + matricolaDestinatario + " oggetto: " + oggetto +
                                " codice protocollatore: " + codiceProtocollatore, "CreaProtocolloCompleto");

                ApplicaDocumentoAlProtocolloResult _resultInternal = new ApplicaDocumentoAlProtocolloResult();
                _resultInternal.Esito = false;

                _resultInternal = ApplicaDocumentoAlProtocollo(matricolaMittente, matricolaDestinatario, oggetto, codiceProtocollatore, result.ServiceResult.Id_Documento, nomeFile, file);

                if (!_resultInternal.Esito)
                {
                    throw new Exception(_resultInternal.DescrizioneErrore);
                }

                ScriviLogAzioni("Upload del documento protocollato, protocollo n° " + result.ServiceResult.Protocollo + " id documento riferimento: " + result.ServiceResult.Id_Documento +
                " matricola mittente: " + matricolaMittente + " matricola destinatario: " + matricolaDestinatario + " oggetto: " + oggetto +
                " codice protocollatore: " + codiceProtocollatore, "CreaProtocolloCompleto");

                result.Esito = true;
                result.ServiceResult.File = file;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
                ScriviLogErrore(ex.Message, "CreaProtocolloCompleto");

            }

            return result;
        }

        private static void ScriviLogErrore(string testo, string operazione, string matricola = null)
        {
            var db = new myRaiData.digiGappEntities();

            myRaiData.MyRai_LogErrori a = new myRaiData.MyRai_LogErrori()
            {
                matricola = (!String.IsNullOrEmpty(matricola) ? matricola : CommonManager.GetCurrentUserMatricola()),
                data = DateTime.Now,
                error_message = testo,
                applicativo = "PORTALE",
                provenienza = "ProtocolloManager." + operazione,
                feedback = ""
            };

            db.MyRai_LogErrori.Add(a);
            db.SaveChanges();
        }

        private static void ScriviLogAzioni(string testo, string operazione, string matricola = null)
        {
            var db = new myRaiData.digiGappEntities();

            myRaiData.MyRai_LogAzioni a = new myRaiData.MyRai_LogAzioni()
            {
                matricola = (!String.IsNullOrEmpty(matricola) ? matricola : CommonManager.GetCurrentUserMatricola()),
                data = DateTime.Now,
                descrizione_operazione = testo,
                applicativo = "PORTALE",
                provenienza = "ProtocolloManager." + operazione,
                operazione = operazione
            };

            db.MyRai_LogAzioni.Add(a);
            db.SaveChanges();
        }
    }
}
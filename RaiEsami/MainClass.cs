using myRai.DataAccess;
using myRaiCommonTasks;
using myRaiCommonTasks.sendMail;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RaiEsami
{
    public class InfoDirezioni
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string Sigla { get; set; }
        public string DescrizioneEstesa { get; set; }
        public string CasellaEmail { get; set; }
    }

    #region Pianificazioni
    public class Laboratori
    {
        public string Descrizione { get; set; }
        public List<string> Sedi { get; set; }
        public List<FasciaOraria> Fasce { get; set; }
        public List<DateTime> ExclDate { get; set; }
    }
    public class FasciaOraria
    {
        public int OraInizio { get; set; }
        public int MinInizio { get; set; }
        public int OraFine { get; set; }
        public int MinFine { get; set; }
        public int DurataSlot { get; set; }
        public int PausaTraSlot { get; set; }
        public int PersonPerSlot { get; set; }

        public FasciaOraria(int oraI, int minI, int oraF, int minF, int durSlot, int breakTime, int personPerSlot)
        {
            OraInizio = oraI;
            MinInizio = minI;
            OraFine = oraF;
            MinFine = minF;

            DurataSlot = durSlot;
            PausaTraSlot = breakTime;

            PersonPerSlot = personPerSlot;
        }
    }
    public class Slot
    {
        public Slot()
        {
            Sedi = new List<string>();
        }

        public string Lab { get; set; }
        public DateTime OraInizio { get; set; }
        public DateTime OraFine { get; set; }
        public bool Disponibile { get; set; }
        public List<string> Sedi { get; set; }

        public bool CanUseThisSlot(string sede)
        {
            return Sedi.Contains(sede);
        }

        public override string ToString()
        {
            return String.Format("{3} - Data {0:dd/MM/yyyy} - dalle {0:HH:mm} alle {1:HH:mm} - {2} - {4}", OraInizio, OraFine, Lab, Disponibile ? "S" : "N", String.Join(",", Sedi));
        }
    }
    #endregion

    #region Esami
    public class ExamParam
    {
        //public List<string> EsamiAttivi { get; set; }
        //public List<ExamWidget> WidgetAttivi { get; set; }

        //public List<ExamAdesioneModel> ModelAdesione { get; set; }

        public List<ExamModel> DatiEsame { get; set; }
    }
    public class ExamMail
    {
        public string Mittente { get; set; }
        public string Oggetto { get; set; }
        public string Testo { get; set; }
        public string CC { get; set; }
        public string CCN { get; set; }
        public List<ExamMailAttach> Allegati { get; set; }
    }

    public class ExamMailAttach
    {
        public string NomeTemplate { get; set; }
        public string NomeAllegato { get; set; }
        public string ContentType { get; set; }
        public bool IsPrivacy { get; set; }
        public ExamWidgetFilter Filtri { get; set; }
    }

    public class ExamModel
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string DenominazioneSing { get; set; }
        public string DenominazionePlur { get; set; }
        public int GiorniAdesione { get; set; }
        public bool MailAbilita { get; set; }
        public ExamMail Mail { get; set; }
        public bool NotificaAbilita { get; set; }
        public string NotificaTesto { get; set; }
        public string DenominazioneAppuntamento { get; set; }
        public bool NotaRifiuto { get; set; }
        public bool AutoApprovazione { get; set; }
        public int MinimoGiorniPrenotazione { get; set; }
        public bool RipianificaSeNonEffettuato { get; set; }
        public List<ExtraFieldModel> CampiExtra { get; set; }
        public bool? ConsentiRinuncia { get; set; }
        public ExamRinunciaModel ParametriRinuncia { get; set; }
    }
    public class ExamRinunciaModel
    {
        public int? LimiteOre { get; set; }
        public string Messaggio { get; set; }
        public string TestoBottone { get; set; }
        public string Alert { get; set; }
        public ExamMail Mail { get; set; }
    }
    public class ExtraFieldModel
    {
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public bool Obbligatorio { get; set; }
        public XR_EXAM_ABIL Abil { get; set; }
        public List<ExtraFieldValue> ValoriAmmessi { get; set; }
    }
    public class ExtraFieldValue
    {
        public string Nome { get; set; }
        public string Valore { get; set; }
        public string Descrizione { get; set; }
    }

    public class ExamWidgetFilter
    {
        public string[] MatrIncluse { get; set; }
        public string[] MatrEscluse { get; set; }
        public string[] DirIncluse { get; set; }
        public string[] DirEscluse { get; set; }
        public string[] SediIncluse { get; set; }
        public string[] SediEscluse { get; set; }
        public bool VincoloAdesione { get; set; }
        public string[] SocietaIncluse { get; set; }
        public string[] SocietaEscluse { get; set; }
    }
    #endregion

    public class MainClass : BatchBaseClass
    {
        public override void Entry(string[] args)
        {
            string mainOper = args[1];
            switch (mainOper)
            {
                case "MailDirezioni":
                    MailDirezioni(args);
                    break;
                case "Pianificazione":
                    PianificazioneAppuntamenti(args);
                    break;
                default:
                    break;
            }
        }

        private void MailDirezioni(string[] args)
        {
            string codExam = args[2];

            var db = new IncentiviEntities();

            DateTime yesterday = DateTime.Today.AddDays(-1);
            DateTime minLimit = DateTime.Today.AddDays(1);
            DateTime maxLimit = DateTime.Today.AddDays(2);

            //var list = db.XR_EXAM_RICHIESTE.Where(x => x.COD_EXAM == codExam && !x.IND_EFFETTUATO.HasValue && x.PROPOSTA_DATA_RICH.HasValue && x.PROPOSTA_DATA_RICH.Value==yesterday)
            //                               .Join(db.SINTESI1, x => x.ID_PERSONA, y => y.ID_PERSONA, (x, y) => new { Esame = x, Sintesi = y })
            //                               .GroupBy(x => x.Sintesi.COD_SERVIZIO).ToList();

            var list = db.XR_EXAM_RICHIESTE.Where(x => x.COD_EXAM == codExam && !x.IND_EFFETTUATO.HasValue && x.DTA_PROPOSTA_INI.Value >= minLimit && x.DTA_PROPOSTA_INI <= maxLimit)
                                           .Join(db.SINTESI1, x => x.ID_PERSONA, y => y.ID_PERSONA, (x, y) => new { Esame = x, Sintesi = y })
                                           .GroupBy(x => x.Sintesi.COD_SERVIZIO).ToList();

            if (!list.Any())
            {
                _log.Warn("Nessun nuovo aggiornamento");
                return;
            }

            List<InfoDirezioni> infos = GetInfoDirezioni();

            string mailTemplate = System.IO.File.ReadAllText("MailTemplate.html");
            string trTemplate = ConfigurationManager.AppSettings["TrTemplate"];
            string mailFrom = ConfigurationManager.AppSettings["MailFrom"];
            string mailCCN = ConfigurationManager.AppSettings["MailCCN"];
            string mailOggetto = ConfigurationManager.AppSettings["MailOggetto"];

            string mailDestinatari = "";
            string mailCC = "";

            GestoreMail mailSender = new GestoreMail();

            foreach (var serv in list)
            {
                mailDestinatari = "";
                mailCC = "";

                var infoDir = infos.FirstOrDefault(x => x.Codice == serv.Key);
                if (infoDir == null)
                {
                    _log.Error(String.Format("Informazioni per la direzione {0} non trovate", serv.Key));
                    continue;
                }

                mailCC = infoDir.CasellaEmail;

                string tbodyTemplate = "";
                foreach (var dip in serv.OrderBy(x => x.Sintesi.COD_MATLIBROMAT))
                {
                    tbodyTemplate += trTemplate.Replace("#MATRICOLA#", dip.Sintesi.COD_MATLIBROMAT)
                                              .Replace("#NOMINATIVO#", dip.Sintesi.Nominativo())
                                              .Replace("#ORARIO#", dip.Esame.DTA_PROPOSTA_INI.Value.ToString("HH:mm") + " - " + dip.Esame.DTA_PROPOSTA_END.Value.ToString("HH:mm"));
                }

                mailTemplate = mailTemplate.Replace("#TBODY#", tbodyTemplate)
                                           .Replace("#DATA_APPUNTAMENTO#", minLimit.ToString("dd/MM/yyyyy"));



                mailSender.InvioMail(mailTemplate, mailOggetto, "", mailCC, mailFrom, null, mailCCN);
            }
        }
        private List<InfoDirezioni> GetInfoDirezioni()
        {
            List<InfoDirezioni> result = new List<InfoDirezioni>();

            string[] lines = System.IO.File.ReadAllLines("TabellaServiziCasellaCC.csv");
            foreach (var line in lines.Skip(1))
            {
                string[] element = line.Split(';');
                InfoDirezioni info = new InfoDirezioni()
                {
                    Codice = element[0],
                    Descrizione = element[1],
                    Sigla = element[2],
                    DescrizioneEstesa = element[3],
                    CasellaEmail = element[4]
                };
                result.Add(info);
            }

            return result;
        }

        private void PianificazioneAppuntamenti(string[] args)
        {
            string codExam = args[2];

            ExamModel examModel = null;
            GetExamModel(codExam, out examModel);

            var db = new IncentiviEntities();
            DateTime limit = DateTime.Today.AddDays(-(examModel.GiorniAdesione));
            DateTime limitMax = limit.AddDays(1);
            //Controllo prenotazioni pendendenti
            var listPendenti = db.XR_EXAM_RICHIESTE.Where(x => x.COD_EXAM == codExam && !x.MATRICOLA.StartsWith("$") && x.PROPOSTA_DATA_RICH != null && x.PROPOSTA_DATA_RICH.Value >= limit && x.PROPOSTA_DATA_RICH < limitMax && x.PROPOSTA_ACCEPT == null);
            if (listPendenti.Any())
            {
                foreach (var pendente in listPendenti)
                {
                    pendente.PROPOSTA_ACCEPT = false;
                    pendente.PROPOSTA_NOTA = "Proposta annullata per scadenza termine";
                }
                db.SaveChanges();
            }

            string dtPian = args.Length >= 4 ? args[3] : "";
            DateTime minDate = DateTime.Today.AddDays(examModel.MinimoGiorniPrenotazione);//o GiorniAdesione
            if (!String.IsNullOrWhiteSpace(dtPian))
                DateTime.TryParseExact(dtPian, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out minDate);

            int durataSlotMin = 10;
            int breakTime = 0;
            int personPerSlot = 3;
            DateTime dataPianificazione = minDate;
            DateTime dataPianificazioneMax = dataPianificazione.AddDays(1);

            _log.Info("Pianificazione data " + dataPianificazione.ToString("dd/MM/yyyy"));

            //Definizione slot
            List<Laboratori> laboratori = new List<Laboratori>()
            {
                new Laboratori()
                {
                    Descrizione = "Roma - Saxa Rubra",
                    Fasce = new List<FasciaOraria>()
                    {
                        new FasciaOraria(9, 0, 13, 0, durataSlotMin, breakTime, personPerSlot),
                        new FasciaOraria(14, 0, 18, 0, durataSlotMin, breakTime, personPerSlot)
                    },
                    Sedi = new List<string>(){"680","730","750","760"},
                    ExclDate = new List<DateTime>()
                    {
                        new DateTime(2021,06,02),//Repubblica
                        new DateTime(2021,06,29),//Patrono
                        new DateTime(2021,08,15),//Ferragosto
                        new DateTime(2021,11,01),//Tutti i santi
                        new DateTime(2021,12,08),//Immacolata
                        new DateTime(2021,12,25),//Natale
                        new DateTime(2021,12,26),//Santo Stefano
                    }
                },
                new Laboratori()
                {
                    Descrizione = "Torino - Via Verdi",
                    Fasce = new List<FasciaOraria>()
                    {
                        new FasciaOraria(9, 0, 13, 0, durataSlotMin, breakTime, personPerSlot),
                        new FasciaOraria(14, 0, 18, 0, durataSlotMin, breakTime, personPerSlot)
                    },
                    Sedi = new List<string>(){"770","790"},
                    ExclDate = new List<DateTime>()
                    {
                        new DateTime(2021,06,02),//Repubblica
                        new DateTime(2021,06,24),//Patrono
                        new DateTime(2021,08,15),//Ferragosto
                        new DateTime(2021,11,01),//Tutti i santi
                        new DateTime(2021,12,08),//Immacolata
                        new DateTime(2021,12,25),//Natale
                        new DateTime(2021,12,26),//Santo Stefano
                    }
                },
                new Laboratori()
                {
                    Descrizione = "Milano",
                    Fasce = new List<FasciaOraria>()
                    {
                        new FasciaOraria(9, 0, 13, 0, durataSlotMin, breakTime, personPerSlot),
                        new FasciaOraria(14, 0, 18, 0, durataSlotMin, breakTime, personPerSlot)
                    },
                    Sedi = new List<string>(){"410"},
                    ExclDate = new List<DateTime>()
                    {
                        new DateTime(2021,06,02),//Repubblica
                        new DateTime(2021,08,15),//Ferragosto
                        new DateTime(2021,11,01),//Tutti i santi
                        new DateTime(2021,12,07),//Patrono
                        new DateTime(2021,12,08),//Immacolata
                        new DateTime(2021,12,25),//Natale
                        new DateTime(2021,12,26),//Santo Stefano
                    }
                },
                new Laboratori()
                {
                    Descrizione = "Napoli",
                    Fasce = new List<FasciaOraria>()
                    {
                        new FasciaOraria(9, 0, 13, 0, durataSlotMin, breakTime, personPerSlot),
                        new FasciaOraria(14, 0, 18, 0, durataSlotMin, breakTime, personPerSlot)
                    },
                    Sedi = new List<string>(){"540"},
                    ExclDate = new List<DateTime>()
                    {
                        new DateTime(2021,06,02),//Repubblica
                        new DateTime(2021,08,15),//Ferragosto
                        new DateTime(2021,09,19),//Patrono
                        new DateTime(2021,11,01),//Tutti i santi
                        new DateTime(2021,12,08),//Immacolata
                        new DateTime(2021,12,25),//Natale
                        new DateTime(2021,12,26),//Santo Stefano
                    }
                }
            };

            //Reperimento prenotazioni vacanti e della data odierna
            var listDaPrenotare = db.XR_EXAM_RICHIESTE.Where(x => x.COD_EXAM == codExam && !x.MATRICOLA.StartsWith("$") && (x.DTA_PROPOSTA_INI == null || (x.DTA_PROPOSTA_INI != null && x.PROPOSTA_ACCEPT == false)));

            //Se nella data precedente a quella iniziale c'è qualche slot "rifiutato", lo rimetto nella pianificazione
            DateTime dataPianificazionePrec = DateTime.Today.AddDays(examModel.GiorniAdesione);//  dataPianificazione.AddDays(-1);
            if (listPendenti.Any(x => x.DTA_PROPOSTA_INI != null && x.DTA_PROPOSTA_INI >= dataPianificazionePrec && x.DTA_PROPOSTA_INI <= dataPianificazione))
                dataPianificazione = dataPianificazionePrec;

            int countDaPrenotare = listDaPrenotare.Count();
            _log.Info("Richieste da pianificare " + countDaPrenotare.ToString());

            if (listDaPrenotare == null || !listDaPrenotare.Any())
                return;

            //Prendo l'elenco delle sedi delle persone da prenotare cosi da costruire solo gli slot necessari
            var sedi = listDaPrenotare.Select(x => x.COD_SEDE).Distinct();
            var labDaUsare = laboratori.Where(x => x.Sedi.Any(a => sedi.Contains(a)));
            var sediLab = labDaUsare.SelectMany(x => x.Sedi).Distinct().ToList();

            //Leggo le prenotazioni solo per i laboratori interessati
            var listPrenotati = db.XR_EXAM_RICHIESTE.Where(x => x.COD_EXAM == codExam && !x.MATRICOLA.StartsWith("$") && sediLab.Contains(x.COD_SEDE) && x.DTA_PROPOSTA_INI >= dataPianificazione && x.DTA_PROPOSTA_INI < dataPianificazioneMax && (x.PROPOSTA_ACCEPT == null || x.PROPOSTA_ACCEPT == true)).ToList();

            List<Slot> slots = new List<Slot>();
            //Costruisco gli slot
            _log.Info("Costruzione slot...");
            int countSlot = 0;
            foreach (var lab in labDaUsare)
            {
                //int countDaPrenPerLab = listDaPrenotare.Count(x => lab.Sedi.Contains(x.COD_SEDE));

                DateTime dtStart = dataPianificazione, dtEnd;
                while (dtStart < dataPianificazioneMax)
                {
                    foreach (var fascia in lab.Fasce)
                    {
                        dtStart = dtStart.Date.AddHours(fascia.OraInizio).AddMinutes(fascia.MinInizio);
                        dtEnd = dtStart.Date.AddHours(fascia.OraFine).AddMinutes(fascia.MinFine);

                        while (dtStart.AddMinutes(fascia.DurataSlot) <= dtEnd)
                        {
                            //Definisco lo slot
                            DateTime iniSlot = dtStart;
                            DateTime endSlot = dtStart.AddMinutes(fascia.DurataSlot);

                            for (int i = 0; i < fascia.PersonPerSlot; i++)
                            {
                                if (!listPrenotati.Any(x => lab.Sedi.Contains(x.COD_SEDE) && x.DTA_PROPOSTA_INI <= endSlot && iniSlot <= x.DTA_PROPOSTA_END && x.DES_LABORATORIO == lab.Descrizione))
                                {
                                    slots.Add(new Slot()
                                    {
                                        Lab = lab.Descrizione,
                                        OraInizio = iniSlot,
                                        OraFine = endSlot,
                                        Disponibile = true,
                                        Sedi = lab.Sedi
                                    });
                                    countSlot++;
                                }
                            }

                            dtStart = endSlot.AddMinutes(fascia.PausaTraSlot);
                        }

                    }

                    do
                    {
                        dtStart = dtStart.Date.AddDays(1);
                    }
                    while (dtStart.DayOfWeek == DayOfWeek.Saturday || dtStart.DayOfWeek == DayOfWeek.Sunday || lab.ExclDate.Contains(dtStart));
                }
            }
            slots = slots.OrderBy(x => x.OraInizio).ThenBy(x => x.Lab).ToList();

            _log.Info("Pianificazione appuntamenti...");
            List<XR_EXAM_RICHIESTE> mailToSend = new List<XR_EXAM_RICHIESTE>();
            foreach (var daPren in listDaPrenotare)
            {
                var slotFree = slots.FirstOrDefault(x => x.CanUseThisSlot(daPren.COD_SEDE) && (daPren.DTA_PROPOSTA_INI == null || x.OraInizio.Date != daPren.DTA_PROPOSTA_INI.Value.Date));
                if (slotFree == null)
                    break;
                
                //WSDigigappDataController service = new WSDigigappDataController();
                //var resp = service.GetEccezioni(daPren.MATRICOLA, daPren.MATRICOLA, slotFree.OraInizio.Date.ToString("ddMMyyyy"), "BU", 70);

                daPren.DTA_PROPOSTA_INI = slotFree.OraInizio;
                daPren.DTA_PROPOSTA_END = slotFree.OraFine;
                daPren.DES_LABORATORIO = slotFree.Lab;

                daPren.PROPOSTA_DATA_RICH = DateTime.Now;
                daPren.PROPOSTA_ACCEPT = null;
                daPren.PROPOSTA_NOTA = null;

                slotFree.Disponibile = false;

                mailToSend.Add(daPren);

                _log.Info(String.Format("Matricola {0} - {1:dd/MM/yyyy} - dalle {1:HH:mm} alle {2:HH:mm} - {3}", daPren.MATRICOLA, daPren.DTA_PROPOSTA_INI, daPren.DTA_PROPOSTA_END, daPren.DES_LABORATORIO));
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
            }

            _log.Info("Invio mail...");
            if (examModel.MailAbilita && examModel.Mail != null && mailToSend.Any())
            {
                foreach (var item in mailToSend)
                {
                    string mittente = examModel.Mail.Mittente;
                    string testo = FormattazioneMail(item, examModel.Mail.Testo);
                    string oggetto = FormattazioneMail(item, examModel.Mail.Oggetto);
                    string destinatariCC = examModel.Mail.CC;
                    string destinatario = item.DES_EMAIL;

                    var sint = db.SINTESI1.Find(item.ID_PERSONA);
                    List<Attachement> allegati = new List<Attachement>();
                    if (examModel.Mail.Allegati != null && examModel.Mail.Allegati.Any())
                    {
                        foreach (var mailAtt in examModel.Mail.Allegati)
                        {
                            if (!CheckAbil(mailAtt.Filtri, sint)) continue;

                            var template = db.XR_TEMPLATES.FirstOrDefault(x => x.NomeFile == mailAtt.NomeTemplate);
                            if (template != null)
                            {
                                allegati.Add(new Attachement()
                                {
                                    AttachementName = FormattazioneMail(item, mailAtt.NomeAllegato),
                                    AttachementType = mailAtt.ContentType,
                                    AttachementValue = template.ContentByte
                                });
                            }
                        }
                    }

                    GestoreMail mail = new GestoreMail();
                    var response = mail.InvioMail(testo, oggetto, destinatario, destinatariCC, mittente, allegati, null, null);
                }
            }
        }

        public static bool CheckAbil(ExamWidgetFilter filtri, SINTESI1 sint)
        {
            return filtri == null || (
                       (filtri.MatrIncluse == null || filtri.MatrIncluse.Contains(sint.COD_MATLIBROMAT))
                    && (filtri.MatrEscluse == null || !filtri.MatrEscluse.Contains(sint.COD_MATLIBROMAT))
                    && (filtri.DirIncluse == null || filtri.DirIncluse.Contains(sint.COD_SERVIZIO))
                    && (filtri.DirEscluse == null || !filtri.DirEscluse.Contains(sint.COD_SERVIZIO))
                    && (filtri.SediIncluse == null || filtri.SediIncluse.Contains(sint.COD_SEDE))
                    && (filtri.SediEscluse == null || !filtri.SediEscluse.Contains(sint.COD_SEDE))
                    && (filtri.SocietaIncluse == null || filtri.SocietaIncluse.Contains(sint.COD_IMPRESACR))
                    && (filtri.SocietaEscluse == null || !filtri.SocietaEscluse.Contains(sint.COD_IMPRESACR))
                    );
        }

        private static string FormattazioneMail(XR_EXAM_RICHIESTE richiesta, string inputText)
        {
            string outputText = inputText;

            DateTime dataAppIni = richiesta.DTA_APPUNTAMENTO_INI.HasValue ? richiesta.DTA_APPUNTAMENTO_INI.Value : richiesta.DTA_PROPOSTA_INI.HasValue ? richiesta.DTA_PROPOSTA_INI.Value : new DateTime();
            DateTime dataAppFine = richiesta.DTA_APPUNTAMENTO_END.HasValue ? richiesta.DTA_APPUNTAMENTO_END.Value : richiesta.DTA_APPUNTAMENTO_END.HasValue ? richiesta.DTA_APPUNTAMENTO_END.Value : new DateTime();

            outputText = outputText.Replace("#APP_LUOGO#", richiesta.DES_LABORATORIO)
                                   .Replace("#APP_DATA#", dataAppIni.ToString("dd/MM/yyyy"))
                                   .Replace("#APP_ORA_INI#", dataAppIni.ToString("HH:mm"))
                                   .Replace("#APP_ORA_FINE#", dataAppFine.ToString("HH:mm"))
                                   .Replace("#NOMINATIVO#", richiesta.DES_COGNOMEPERS.TitleCase() + " " + richiesta.DES_NOMEPERS.TitleCase());

            return outputText;
        }

        public static ExamParam GetDbParams()
        {
            ExamParam param = null;
            digiGappEntities db = new digiGappEntities();
            var parametro = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EsamiParametri");
            if (parametro != null && !String.IsNullOrWhiteSpace(parametro.Valore1))
            {
                try
                {
                    param = Newtonsoft.Json.JsonConvert.DeserializeObject<ExamParam>(parametro.Valore1);
                }
                catch (Exception ex)
                {
                }
            }

            if (param == null)
                param = new ExamParam();

            return param;
        }

        public static bool GetExamModel(string codExam, out ExamModel model)
        {
            bool found = false;
            model = null;
            var dbParams = GetDbParams();
            var dati = dbParams.DatiEsame.FirstOrDefault(x => x.Codice == codExam);
            if (dati != null)
            {
                found = true;
                model = dati;
            }

            return found;
        }


    }
}

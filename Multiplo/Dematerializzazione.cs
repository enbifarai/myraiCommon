using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Multiplo.Helpers;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonTasks.Helpers;
using myRaiData.Incentivi;
using myRaiHelper;
using Newtonsoft.Json;
using MimeTypeMap = myRaiHelper.MimeTypeMap;
using Output = Multiplo.Helpers.Output;

namespace Multiplo
{
    public static class Dematerializzazione
    {
        public static void StartImportazione()
        {
            IncentiviEntities db = new IncentiviEntities();

            string localPath = System.Configuration.ConfigurationManager.AppSettings["TempOutput"] ?? System.Reflection.Assembly.GetEntryAssembly().Location;
            string directoryPath = Path.GetDirectoryName(localPath);
            string lPath = Path.Combine(directoryPath);
            if (!Directory.Exists(lPath))
                Directory.CreateDirectory(lPath);

            // cartella per data
            string cartellaData = "Importazioni dematerializzazione";
            string pathData = Path.Combine(lPath, cartellaData);

            if (!Directory.Exists(pathData))
                Directory.CreateDirectory(pathData);

            if (Directory.Exists(pathData))
            {
                foreach (var filename in Directory.GetFiles(pathData))
                {
                    Console.WriteLine("File " + filename);
                    string nomeFileCSV = Path.GetFileName(filename);
                    if (!Path.GetExtension(nomeFileCSV).ToUpper().Contains(".CSV"))
                    {
                        continue;
                    }

                    string riga = "";
                    int i = 1;
                    StreamReader file = new StreamReader(filename);

                    riga = file.ReadLine();

                    if (riga == null)
                    {
                        Output.WriteLine("File: " + filename + " vuoto");
                        continue;
                    }

                    // esempio TRACCIATO:  EXTRUO,31,20,912685,,332783,123445,,72.5-140.0-1,106.5-182.0-1,351.5-619.0-1,FinRUTes$COMPETENZE PERSONALE MESE DI FEBBRAIO 2022 SEDE DI TRIESTE
                    Output.WriteLine(riga);
                    List<string> rigaSplittata = riga.Split(',').ToList();
                    string Cod_Tipologia_Documentale = rigaSplittata[0];
                    string sId_Tipo_Doc = rigaSplittata[1];
                    string sId_Stato = rigaSplittata[2];
                    string MatricolaCreatore = rigaSplittata[3];
                    string MatricolaDestinatario = rigaSplittata[4];
                    string MatricolaApprovatore = rigaSplittata[5];
                    string MatricolaFirma = rigaSplittata[6];
                    string Note = rigaSplittata[7];
                    string POSIZIONEPROTOCOLLO = rigaSplittata[8];
                    string DATAPROTOCOLLO = rigaSplittata[9];
                    string Firma = rigaSplittata[10];
                    string ATTRIBUTIAGGIUNTIVI = rigaSplittata[11];
                    string UFFICIO_DESTINATARIO = "";
                    string OGGETTOPERPROTOCOLLO = "";
                    int idAllegato = 0;

                    int id_Tipo_Doc = int.Parse(sId_Tipo_Doc);
                    int id_Stato = int.Parse(sId_Stato);

                    List<string> sPOSIZIONEPROTOCOLLO = new List<string>();
                    sPOSIZIONEPROTOCOLLO = POSIZIONEPROTOCOLLO.Split('-').ToList();

                    List<string> sDATAPROTOCOLLO = new List<string>();
                    sDATAPROTOCOLLO = DATAPROTOCOLLO.Split('-').ToList();

                    List<string> sFirme = new List<string>();
                    sFirme = Firma.Split('/').ToList();

                    List<string> sATTRIBUTIAGGIUNTIVI = new List<string>();
                    sATTRIBUTIAGGIUNTIVI = ATTRIBUTIAGGIUNTIVI.Split('$').ToList();
                    UFFICIO_DESTINATARIO = sATTRIBUTIAGGIUNTIVI[0];
                    OGGETTOPERPROTOCOLLO = sATTRIBUTIAGGIUNTIVI[1];

                    List<PosizioneProtocolloOBJ> obj = new List<PosizioneProtocolloOBJ>();

                    float protL = float.Parse(sPOSIZIONEPROTOCOLLO[0].Replace(".", ","));
                    float protTop = float.Parse(sPOSIZIONEPROTOCOLLO[1].Replace(".", ","));
                    int pagProt = int.Parse(sPOSIZIONEPROTOCOLLO[2].Replace(".", ","));

                    obj.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Protocollo",
                        PosizioneLeft = protL,
                        PosizioneTop = protTop,
                        NumeroPagina = pagProt
                    });

                    float dataLeft = float.Parse(sDATAPROTOCOLLO[0].Replace(".", ","));
                    float dataTop = float.Parse(sDATAPROTOCOLLO[1].Replace(".", ","));
                    int dataPagina = int.Parse(sDATAPROTOCOLLO[2].Replace(".", ","));

                    obj.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Data",
                        PosizioneLeft = dataLeft,
                        PosizioneTop = dataTop,
                        NumeroPagina = dataPagina
                    });

                    foreach(var s in sFirme)
                    {
                        List<string> sFirma = new List<string>();
                        sFirma = s.Split('-').ToList();

                        float firmaLeft = float.Parse(sFirma[0].Replace(".", ","));
                        float firmaTop = float.Parse(sFirma[1].Replace(".", ","));
                        int firmaPagina = int.Parse(sFirma[2].Replace(".", ","));

                        obj.Add(new PosizioneProtocolloOBJ()
                        {
                            Oggetto = "Firma",
                            PosizioneLeft = firmaLeft,
                            PosizioneTop = firmaTop,
                            NumeroPagina = firmaPagina
                        });
                    }

                    var jsonString = JsonConvert.SerializeObject(obj);

                    string nomeFilePDF = Path.GetFileNameWithoutExtension(nomeFileCSV);
                    nomeFilePDF += ".pdf";

                    string pathFilePDF = Path.Combine(pathData, nomeFilePDF);

                    if (File.Exists(pathFilePDF))
                    {
                        var file2 = File.ReadAllBytes(pathFilePDF);
                        if (file2 != null && file2.Length > 0)
                        {
                            byte[] data = file2;
                            int length = data.Length;
                            string est = Path.GetExtension(nomeFilePDF);
                            string tipoFile = MimeTypeMap.GetMimeType(est);
                            XR_ALLEGATI allegato = new XR_ALLEGATI()
                            {
                                NomeFile = nomeFilePDF,
                                MimeType = tipoFile,
                                Length = length,
                                ContentByte = data,
                                IsPrincipal = true,
                                PosizioneProtocollo = jsonString
                            };

                            db.XR_ALLEGATI.Add(allegato);
                            db.SaveChanges();
                            idAllegato = allegato.Id;
                        }

                    }
                    else
                    {
                        Output.WriteLine("File: " + nomeFilePDF + " non trovato");
                        continue;
                    }

                    RichiestaDoc richiesta = new RichiestaDoc();
                    richiesta.Documento = new XR_DEM_DOCUMENTI();
                    XR_DEM_TIPIDOC_COMPORTAMENTO comportamento = null;

                    // Da rivedere - non va bene l'if in questo modo, la tipologia va calcolata in base all'id_Tipo_Doc passato
                    string cod_tipo_doc = "";
                    string descrizione = "";

                    var XR_DEM_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_Tipo_Doc)).FirstOrDefault();

                    if (XR_DEM_TIPI_DOCUMENTO != null)
                    {
                        cod_tipo_doc = XR_DEM_TIPI_DOCUMENTO.Codice.Trim();
                        descrizione = XR_DEM_TIPI_DOCUMENTO.Descrizione.Trim();
                    }
                    else
                    {
                        throw new Exception("Tipo documento non trovato");
                    }

                    comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals("EXTRUO") &&
w.Codice_Tipo_Documento.Equals(cod_tipo_doc)).FirstOrDefault();                    

                    if (comportamento == null)
                    {
                        throw new Exception("Comportamento non trovato");
                    }

                    //matricola = "021935";

                    List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                    List<AttributiAggiuntivi> objFinale = new List<AttributiAggiuntivi>();


                    string indirizziEmail = "";

                    //UFFICIO_DESTINATARIO
                    //Tes = Tesoreria
                    //For = Fornitori
                    //Fin = Finanza
                    //RU = Risorse Umane Ray Way
                    //TesFor = Tesoreria e Fornitori
                    //FinRUTes = Finanza, Risorse Umane Ray Way  e Tesoreria

                    if (UFFICIO_DESTINATARIO == "Tes")
                    {
                        indirizziEmail = "[CG] DAF_Tesoreria";
                    }
                    else if (UFFICIO_DESTINATARIO == "For")
                    {
                        indirizziEmail = "[CG] DAF_Fornitori";
                    }
                    else if (UFFICIO_DESTINATARIO == "Fin")
                    {
                        indirizziEmail = "[CG] RW.Finanza";
                    }
                    else if (UFFICIO_DESTINATARIO == "RU")
                    {
                        indirizziEmail = "[CG] RW-Risorse Umane";
                    }
                    else if (UFFICIO_DESTINATARIO == "TesFor")
                    {
                        indirizziEmail = "[CG] DAF_Tesoreria, [CG] DAF_Fornitori";
                    }
                    else if (UFFICIO_DESTINATARIO == "FinRUTes")
                    {
                        indirizziEmail = "[CG] RW.Finanza, [CG] RW-Risorse Umane, [CG] DAF_Tesoreria";
                    }
                    else if (UFFICIO_DESTINATARIO == "FinRU")
                    {
                        indirizziEmail = "[CG] RW.Finanza, [CG] RW-Risorse Umane";
                    }

                    foreach (var attr in objD)
                    {
                        if (attr.Id == "ufficiodestinatario")
                        {
                            objFinale.Add(new AttributiAggiuntivi()
                            {
                                Id = attr.Id,
                                Valore = indirizziEmail,
                                Gruppo = attr.Gruppo,
                                Checked = attr.Checked,
                                Label = attr.Label,
                                Title = attr.Title,
                                DBRefAttribute = attr.DBRefAttribute,
                                Ordinamento = attr.Ordinamento,
                                Tipo = attr.Tipo
                            });
                        }

                        if (attr.Id == "Oggetto")
                        {
                            objFinale.Add(new AttributiAggiuntivi()
                            {
                                Id = attr.Id,
                                Valore = OGGETTOPERPROTOCOLLO,
                                Gruppo = attr.Gruppo,
                                Checked = attr.Checked,
                                Label = attr.Label,
                                Title = attr.Title,
                                DBRefAttribute = attr.DBRefAttribute,
                                Ordinamento = attr.Ordinamento,
                                Tipo = attr.Tipo
                            });
                        }

                        if (attr.Id == "DestinatarioMail")
                        {
                            objFinale.Add(new AttributiAggiuntivi()
                            {
                                Id = attr.Id,
                                Valore = indirizziEmail,
                                Gruppo = attr.Gruppo,
                                Checked = attr.Checked,
                                Label = attr.Label,
                                Title = attr.Title,
                                DBRefAttribute = attr.DBRefAttribute,
                                Ordinamento = attr.Ordinamento,
                                Tipo = attr.Tipo
                            });
                        }

                        if (attr.Id == "EccezionePerAutomatismo")
                        {
                            objFinale.Add(attr);
                        }

                        if (attr.Id == "EccezioneSelezionataNascosta")
                        {
                            objFinale.Add(attr);
                        }
                    }

                    string JSON = JsonConvert.SerializeObject(objFinale);

                    XR_WKF_TIPOLOGIA xr_wkf_tipologia = new XR_WKF_TIPOLOGIA();
                    string cod_WKF_Tipologia = "DEMDOC_EXTRUO_" + cod_tipo_doc;
                    cod_WKF_Tipologia = cod_WKF_Tipologia.Trim();

                    xr_wkf_tipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(cod_WKF_Tipologia)).FirstOrDefault();

                    if (String.IsNullOrEmpty(MatricolaDestinatario))
                    {
                        MatricolaDestinatario = null;
                    }

                    if (String.IsNullOrEmpty(Note))
                    {
                        Note = null;
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = descrizione,
                        Note = Note,
                        Id_Stato = id_Stato,
                        Id_Tipo_Doc = id_Tipo_Doc,
                        MatricolaCreatore = MatricolaCreatore,
                        IdPersonaCreatore = CezanneHelper.GetIdPersona(MatricolaCreatore),
                        MatricolaDestinatario = null,
                        IdPersonaDestinatario = null,
                        Id_WKF_Tipologia = xr_wkf_tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = Cod_Tipologia_Documentale,
                        MatricolaApprovatore = MatricolaApprovatore,
                        IdPersonaApprovatore = CezanneHelper.GetIdPersona(MatricolaApprovatore),
                        MatricolaFirma = MatricolaFirma,
                        IdPersonaFirma = CezanneHelper.GetIdPersona(MatricolaFirma),
                        MatricolaIncaricato = null,
                        Id = -1,
                        CustomDataJSON = JSON,
                        PraticaAttiva = true
                    };


                    try
                    {
                        List<XR_DEM_VERSIONI_DOCUMENTO> versioni = new List<XR_DEM_VERSIONI_DOCUMENTO>();

                        XR_DEM_DOCUMENTI doc = null;

                        doc = new XR_DEM_DOCUMENTI()
                        {
                            Descrizione = richiesta.Documento.Descrizione,
                            DataCreazione = DateTime.Now,
                            Id_Stato = richiesta.Documento.Id_Stato,
                            Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                            Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                            MatricolaCreatore = richiesta.Documento.MatricolaCreatore,
                            IdPersonaCreatore = richiesta.Documento.IdPersonaCreatore,
                            MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                            IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                            Note = richiesta.Documento.Note,
                            Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                            MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                            MatricolaFirma = richiesta.Documento.MatricolaFirma,
                            MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato,
                            CustomDataJSON = richiesta.Documento.CustomDataJSON,
                            PraticaAttiva = true
                        };

                        int tempId = 0;
                        bool exist = db.XR_DEM_ALLEGATI_VERSIONI.Count(w => w.IdAllegato.Equals(idAllegato)) > 0;

                        // Se non esiste allora deve creare l'associazione
                        if (!exist)
                        {
                            tempId--;
                            XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                            {
                                N_Versione = 1,
                                DataUltimaModifica = DateTime.Now,
                                Id_Documento = doc.Id,
                                Id = tempId
                            };

                            db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                            XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                            {
                                IdAllegato = idAllegato,
                                IdVersione = version.Id
                            };
                            db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                        }

                        db.XR_DEM_DOCUMENTI.Add(doc);
                        db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                        {
                            COD_TERMID = "1.1.1.1",
                            COD_USER = richiesta.Documento.MatricolaCreatore,
                            DTA_OPERAZIONE = 0,
                            COD_TIPO_PRATICA = "DEM",
                            ID_GESTIONE = doc.Id,
                            ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                            ID_STATO = doc.Id_Stato,
                            ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                            NOMINATIVO = "BATCH",
                            VALID_DTA_INI = DateTime.Now,
                            TMS_TIMESTAMP = DateTime.Now
                        });

                        db.SaveChanges();

                        Output.WriteLine("Documento generato, id: " + doc.Id);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

















        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string codiceWKF_Tipologia)
        {
            XR_WKF_TIPOLOGIA result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(codiceWKF_Tipologia)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(int idWKF)
        {
            XR_WKF_TIPOLOGIA result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_WKF_TIPOLOGIA.Where(w => w.ID_TIPOLOGIA.Equals(idWKF)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// In alcuni casi ad esempio per i documenti con dati custom
        /// il valore dell'eccezione selezionata potrebbe modificare la 
        /// tipologia di workflow da seguire
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(XR_DEM_DOCUMENTI doc)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(doc.Id_WKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(int idWKF, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(idWKF);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string codiceWKF_Tipologia, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(codiceWKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string tipologiaDocumentale, string codiceWKF_Tipologia, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_XR_WKF_TIPOLOGIA(tipologiaDocumentale, codiceWKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string tipologiaDocumentale, int idTipoDoc, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                XR_DEM_TIPI_DOCUMENTO tipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(idTipoDoc)).FirstOrDefault();

                if (tipoDoc == null)
                {
                    throw new Exception("Tipo documento non trovato");
                }

                string nomeWKF = String.Format("DEMDOC_{0}_{1}", tipologiaDocumentale.Trim(), tipoDoc.Codice.Trim());

                XR_WKF_TIPOLOGIA WKF_Tipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeWKF)).FirstOrDefault();
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static XR_WKF_TIPOLOGIA Get_XR_WKF_TIPOLOGIA(string tipologiaDocumentale, string tipologiaDocumento)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                string tipologia = String.Format("DEMDOC_{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                result = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();

                if (result == null)
                {
                    // se è null allora la tipologia è formata dalla sola DEMDOC_TipologiaDocumentale senza 
                    // il tipo documento
                    tipologia = String.Format("DEMDOC_{0}", tipologiaDocumentale.Trim());

                    result = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();
                }

                if (result == null)
                {
                    // se anche in questo caso è null allora c'è un errore
                    throw new Exception("XR_WKF_TIPOLOGIA non trovata");
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }
    }
}

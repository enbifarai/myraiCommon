using myRaiData;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace myRaiCommonTasks.Helpers
{

    #region DEFINIZIONE CLASSI

    public class FileResult
    {
        public FileResult()
        {
            this.Errore = string.Empty;
            this.Esito = false;
            this.Files = new List<MyRai_Files>();
        }

        public bool Esito { get; set; }
        public string Errore { get; set; }
        public int CodiceErrore { get; set; }
        public List<MyRai_Files> Files { get; set; }
    }

    public class ParametroFile
    {
        public string Nome { get; set; }
        public object Valore { get; set; }
    }

    public class ParametriFSP
    {
        /// <summary>
        /// Matricola creatore
        /// </summary>
        public string NumeroFoglioSpese { get; set; }
        /// <summary>
        /// Tipologia di file
        /// </summary>
        public string NumeroVoce { get; set; }
    }

    public class GetFilesRequest
    {
        /// <summary>
        /// Nome dell'applicazione per l'autenticazione
        /// </summary>
        public string Applicazione { get; set; }
        /// <summary>
        /// Password utilizzata dall'applicazione per l'autenticazione
        /// per l'accesso al servizio
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Matricola dell'utente chiamante
        /// </summary>
        public string MatricolaCaller { get; set; }
        /// <summary>
        /// Matricola creatore
        /// </summary>
        public string MatricolaCreatore { get; set; }
        /// <summary>
        /// Ambito al quale fa riferimento il documento.
        /// Ad esempio per i documenti relativi al foglio spese
        /// la tipologià avrà valore FSP
        /// </summary>
        public string Tipologia { get; set; }
        /// <summary>
        /// False default, indica che non verrà restituito il bytearray del file.
        /// Questo perchè nel caso fossero molti files il servizio potrebbe andare in timeout
        /// </summary>
        public bool GetContentByte { get; set; }
        /// <summary>
        /// ES: fogliospesa,numerovoce
        /// </summary>
        public List<string> Parametri { get; set; }
    }

    public class HRISFileResult
    {
        public HRISFileResult()
        {
            this.Errore = string.Empty;
            this.Esito = false;
            this.Files = new List<XR_TB_FILES>();
        }

        public bool Esito { get; set; }
        public string Errore { get; set; }
        public int CodiceErrore { get; set; }
        public List<XR_TB_FILES> Files { get; set; }
    }




    /// <summary>
    /// Oggetto utilizzato per una richiesta di tipo GetFSPFiles
    /// La tipologia avrà già valore FSP non serve valorizzarlo
    /// </summary>
    //public class GetFSPFilesRequest: GetFilesRequest
    //{
    //    public GetFSPFilesRequest()
    //    {
    //        this.Tipologia = "FSP";
    //    }

    //    /// <summary>
    //    /// Parametri definiti per filtrare i fogli spese
    //    /// </summary>
    //    public ParametriFSP ParametriFSP { get; set; }

    //    /// <summary>
    //    /// Parametri che verranno ricercati nel json dei files
    //    /// </summary>
    //    public List<ParametroFile> Parametri { get; set; }
    //}

    #endregion

    public static class FileManager
    {
        #region UPLOAD

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        //public static FileResult UploadFile(string matricola, string tipologia, HttpPostedFileBase file, string chiave = null, string password = null, string json = null)
        //{
        //    FileResult result = new FileResult();
        //    List<MyRai_Files> files = new List<MyRai_Files>();

        //    try
        //    {
        //        if (file.ContentLength > 0)
        //        {
        //            string fileName = Path.GetFileName(file.FileName);

        //            byte[] data;
        //            using (Stream inputStream = file.InputStream)
        //            {
        //                MemoryStream memoryStream = inputStream as MemoryStream;
        //                if (memoryStream == null)
        //                {
        //                    memoryStream = new MemoryStream();
        //                    inputStream.CopyTo(memoryStream);
        //                }
        //                data = memoryStream.ToArray();
        //            }

        //            int length = data.Length;
        //            string est = Path.GetExtension(fileName);
        //            string tipoFile = MimeTypeMap.GetMimeType(est);

        //            // salva sul db
        //            MyRai_Files newItem = new MyRai_Files()
        //            {
        //                Chiave = chiave,
        //                ContentByte = data,
        //                DataCreazione = DateTime.Now,
        //                Json = json,
        //                Length = length,
        //                MatricolaCreatore = matricola,
        //                MimeType = tipoFile,
        //                NomeFile = fileName,
        //                Password = password,
        //                Tipologia = tipologia
        //            };

        //            files.Add(newItem);

        //            if (files != null && files.Any())
        //            {
        //                result = UploadFileSuDB(files);
        //            }
        //            else
        //            {
        //                throw new Exception("L'elenco dei files da salvare è vuoto.");
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Nessun file da caricare");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.Errore = ex.Message;
        //    }
        //    return result;
        //}

        //public static FileResult UploadFile(string matricola, string tipologia, HttpRequestBase request, string chiave = null, string password = null, string json = null)
        //{
        //    FileResult result = new FileResult();
        //    result.Esito = true;
        //    List<MyRai_Files> files = new List<MyRai_Files>();

        //    try
        //    {
        //        if (request.Files != null && request.Files.Count > 0)
        //        {
        //            bool almenoUnoVuoto = false;
        //            for (int idx = 0; idx < request.Files.Count; idx++)
        //            {
        //                var file = request.Files[idx];
        //                if (file.ContentLength <= 0)
        //                {
        //                    almenoUnoVuoto = true;
        //                }
        //            }

        //            if (almenoUnoVuoto)
        //            {
        //                throw new Exception("Uno o più file sono vuoti");
        //            }

        //            for (int idx = 0; idx < request.Files.Count; idx++)
        //            {
        //                var file = request.Files[idx];
        //                string fileName = Path.GetFileName(file.FileName);

        //                byte[] data;
        //                using (Stream inputStream = file.InputStream)
        //                {
        //                    MemoryStream memoryStream = inputStream as MemoryStream;
        //                    if (memoryStream == null)
        //                    {
        //                        memoryStream = new MemoryStream();
        //                        inputStream.CopyTo(memoryStream);
        //                    }
        //                    data = memoryStream.ToArray();
        //                }

        //                int length = data.Length;
        //                string est = Path.GetExtension(fileName);
        //                string tipoFile = MimeTypeMap.GetMimeType(est);

        //                // salva sul db
        //                MyRai_Files newItem = new MyRai_Files()
        //                {
        //                    Chiave = chiave,
        //                    ContentByte = data,
        //                    DataCreazione = DateTime.Now,
        //                    Json = json,
        //                    Length = length,
        //                    MatricolaCreatore = matricola,
        //                    MimeType = tipoFile,
        //                    NomeFile = fileName,
        //                    Password = password,
        //                    Tipologia = tipologia
        //                };

        //                files.Add(newItem);
        //            }

        //            if (files != null && files.Any())
        //            {
        //                result = UploadFileSuDB(files);
        //            }
        //            else
        //            {
        //                throw new Exception("L'elenco dei files da salvare è vuoto.");
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Non è stato trovato alcun file da salvare");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.Errore = ex.Message;
        //    }
        //    return result;
        //}

        //public static FileResult UploadFile(string matricola, string tipologia, string fileName, byte[] fileContent, string chiave = null, string password = null, string json = null)
        //{
        //    FileResult result = new FileResult();
        //    List<MyRai_Files> files = new List<MyRai_Files>();

        //    try
        //    {
        //        if (fileContent.Length > 0 && !String.IsNullOrEmpty(fileName))
        //        {
        //            int length = fileContent.Length;
        //            string est = Path.GetExtension(fileName);
        //            string tipoFile = MimeTypeMap.GetMimeType(est);

        //            // salva sul db
        //            MyRai_Files newItem = new MyRai_Files()
        //            {
        //                Chiave = chiave,
        //                ContentByte = fileContent,
        //                DataCreazione = DateTime.Now,
        //                Json = json,
        //                Length = length,
        //                MatricolaCreatore = matricola,
        //                MimeType = tipoFile,
        //                NomeFile = fileName,
        //                Password = password,
        //                Tipologia = tipologia
        //            };

        //            files.Add(newItem);

        //            if (files != null && files.Any())
        //            {
        //                result = UploadFileSuDB(files);
        //            }
        //            else
        //            {
        //                throw new Exception("L'elenco dei files da salvare è vuoto.");
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Nessun file da caricare");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.Errore = ex.Message;
        //    }
        //    return result;
        //}


        public static FileResult UploadFile(string matricola, string tipologia, HttpPostedFileBase file, string chiave = null, string password = null, string json = null, bool attivo = true)
        {
            FileResult result = new FileResult();
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                if (file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);

                    byte[] data;
                    using (Stream inputStream = file.InputStream)
                    {
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        data = memoryStream.ToArray();
                    }

                    int length = data.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    // salva sul db
                    MyRai_Files newItem = new MyRai_Files()
                    {
                        Chiave = chiave,
                        ContentByte = data,
                        DataCreazione = DateTime.Now,
                        Json = json,
                        Length = length,
                        MatricolaCreatore = matricola,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Password = password,
                        Tipologia = tipologia,
                        Attivo = attivo
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = UploadFileSuDB(files);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        public static FileResult UploadFile(string matricola, string tipologia, HttpRequestBase request, string chiave = null, string password = null, string json = null, bool attivo = true)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                if (request.Files != null && request.Files.Count > 0)
                {
                    bool almenoUnoVuoto = false;
                    for (int idx = 0; idx < request.Files.Count; idx++)
                    {
                        var file = request.Files[idx];
                        if (file.ContentLength <= 0)
                        {
                            almenoUnoVuoto = true;
                        }
                    }

                    if (almenoUnoVuoto)
                    {
                        throw new Exception("Uno o più file sono vuoti");
                    }

                    for (int idx = 0; idx < request.Files.Count; idx++)
                    {
                        var file = request.Files[idx];
                        string fileName = Path.GetFileName(file.FileName);

                        byte[] data;
                        using (Stream inputStream = file.InputStream)
                        {
                            MemoryStream memoryStream = inputStream as MemoryStream;
                            if (memoryStream == null)
                            {
                                memoryStream = new MemoryStream();
                                inputStream.CopyTo(memoryStream);
                            }
                            data = memoryStream.ToArray();
                        }

                        int length = data.Length;
                        string est = Path.GetExtension(fileName);
                        string tipoFile = MimeTypeMap.GetMimeType(est);

                        // salva sul db
                        MyRai_Files newItem = new MyRai_Files()
                        {
                            Chiave = chiave,
                            ContentByte = data,
                            DataCreazione = DateTime.Now,
                            Json = json,
                            Length = length,
                            MatricolaCreatore = matricola,
                            MimeType = tipoFile,
                            NomeFile = fileName,
                            Password = password,
                            Tipologia = tipologia,
                            Attivo = attivo
                        };

                        files.Add(newItem);
                    }

                    if (files != null && files.Any())
                    {
                        result = UploadFileSuDB(files);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Non è stato trovato alcun file da salvare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        public static FileResult UploadFile(string matricola, string tipologia, string fileName, byte[] fileContent, string chiave = null, string password = null, string json = null, bool attivo = true)
        {
            FileResult result = new FileResult();
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                if (fileContent.Length > 0 && !String.IsNullOrEmpty(fileName))
                {
                    int length = fileContent.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    // salva sul db
                    MyRai_Files newItem = new MyRai_Files()
                    {
                        Chiave = chiave,
                        ContentByte = fileContent,
                        DataCreazione = DateTime.Now,
                        Json = json,
                        Length = length,
                        MatricolaCreatore = matricola,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Password = password,
                        Tipologia = tipologia,
                        Attivo = attivo
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = UploadFileSuDB(files);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Salvataggio del file sul db
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileResult UploadFileSuDB(List<MyRai_Files> files)
        {
            FileResult result = new FileResult();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    foreach (var file in files)
                    {
                        db.MyRai_Files.Add(file);
                    }
                    db.SaveChanges();

                    if (files.Count(w => w.Id == 0) > 1)
                    {
                        var items = files.Where(w => w.Id == 0).ToList();

                        if (items != null && items.Any())
                        {
                            string tx = "";
                            foreach (var i in items)
                            {
                                tx = tx + "Si è verificato un errore durante il salvataggio del file: " + i.NomeFile;
                                tx = tx + System.Environment.NewLine;
                            }
                            throw new Exception(tx);
                        }
                    }
                    else
                    {
                        result.Errore = String.Empty;
                        result.Esito = true;
                        result.Files.AddRange(files);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        #endregion

        #region Download

        public static FileResult GetFile(int id)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Files.Where(w => w.Id.Equals(id)).FirstOrDefault();
                    if (item != null)
                    {
                        result.Files.Add(item);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        public static FileResult GetFileByChiave(string chiave)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Files.Where(w => w.Chiave.Equals(chiave)).FirstOrDefault();

                    if (item != null)
                    {
                        result.Files.Add(item);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        public static FileResult GetFilesByMatricola(string matricolaCreatore)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var items = db.MyRai_Files.Where(w => w.MatricolaCreatore.Equals(matricolaCreatore)).ToList();
                    if (items != null && items.Any())
                    {
                        result.Files.AddRange(items);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        public static FileResult GetFiles(string matricola = null, string tipologia = null, string chiave = null, string password = null, bool getContentByte = false)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    IQueryable<MyRai_Files> temp = db.MyRai_Files;

                    if (!String.IsNullOrEmpty(matricola))
                    {
                        temp = temp.Where(w => w.MatricolaCreatore.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(tipologia))
                    {
                        temp = temp.Where(w => w.Tipologia.Equals(tipologia));
                    }

                    if (!String.IsNullOrEmpty(chiave))
                    {
                        temp = temp.Where(w => w.Chiave.Equals(chiave));
                    }

                    if (!String.IsNullOrEmpty(password))
                    {
                        temp = temp.Where(w => w.Password.Equals(password));
                    }

                    if (getContentByte)
                    {
                        files = temp.ToList().Select(x =>
                           new MyRai_Files()
                           {
                               Id = x.Id,
                               Chiave = x.Chiave,
                               ContentByte = x.ContentByte,
                               DataCreazione = x.DataCreazione,
                               Json = x.Json,
                               Length = x.Length,
                               MatricolaCreatore = x.MatricolaCreatore,
                               MimeType = x.MimeType,
                               NomeFile = x.NomeFile,
                               Password = x.Password,
                               Tipologia = x.Tipologia,
                               MyRai_TipologieFiles = x.MyRai_TipologieFiles
                           }
                        ).ToList();
                    }
                    else
                    {
                        files = temp.ToList().Select(x =>

                           new MyRai_Files()
                           {
                               Id = x.Id,
                               Chiave = x.Chiave,
                               ContentByte = null,
                               DataCreazione = x.DataCreazione,
                               Json = x.Json,
                               Length = x.Length,
                               MatricolaCreatore = x.MatricolaCreatore,
                               MimeType = x.MimeType,
                               NomeFile = x.NomeFile,
                               Password = x.Password,
                               Tipologia = x.Tipologia,
                               MyRai_TipologieFiles = x.MyRai_TipologieFiles
                           }
                        ).ToList();
                    }

                    if (files != null && files.Any())
                    {
                        result.Files.AddRange(files);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        /// <summary>
        /// Reperimento dei file associati a documenti di tipo FSP
        /// Foglio spesa
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static FileResult GetFSPFiles(GetFilesRequest request)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    IQueryable<MyRai_Files> temp = db.MyRai_Files;

                    if (!String.IsNullOrEmpty(request.MatricolaCreatore))
                    {
                        temp = temp.Where(w => w.MatricolaCreatore.Equals(request.MatricolaCreatore));
                    }

                    temp = temp.Where(w => w.Tipologia.Equals(request.Tipologia));

                    if (request.Parametri != null && request.Parametri.Any())
                    {
                        string foglio = "";
                        string nvoce = "";
                        foglio = request.Parametri[0];

                        if (request.Parametri.Count > 1)
                        {
                            nvoce = request.Parametri[1];
                        }

                        string filtroFSP = "";
                        if (!String.IsNullOrEmpty(foglio) && !String.IsNullOrEmpty(nvoce))
                        {
                            filtroFSP = foglio + "_" + nvoce;
                            temp = temp.Where(w => w.Chiave.Equals(filtroFSP));
                        }
                        else if (!String.IsNullOrEmpty(foglio))
                        {
                            filtroFSP = foglio;
                            temp = temp.Where(w => w.Chiave.StartsWith(filtroFSP));
                        }
                        else if (!String.IsNullOrEmpty(nvoce))
                        {
                            filtroFSP = "_" + nvoce;
                            temp = temp.Where(w => w.Chiave.Contains(filtroFSP));
                        }
                    }

                    if (request.GetContentByte)
                    {
                        files = temp.ToList().Select(x =>
                           new MyRai_Files()
                           {
                               Id = x.Id,
                               Chiave = x.Chiave,
                               ContentByte = x.ContentByte,
                               DataCreazione = x.DataCreazione,
                               Json = x.Json,
                               Length = x.Length,
                               MatricolaCreatore = x.MatricolaCreatore,
                               MimeType = x.MimeType,
                               NomeFile = x.NomeFile,
                               Password = x.Password,
                               Tipologia = x.Tipologia,
                               MyRai_TipologieFiles = null
                           }
                        ).ToList();
                    }
                    else
                    {
                        files = temp.ToList().Select(x =>

                           new MyRai_Files()
                           {
                               Id = x.Id,
                               Chiave = x.Chiave,
                               ContentByte = null,
                               DataCreazione = x.DataCreazione,
                               Json = x.Json,
                               Length = x.Length,
                               MatricolaCreatore = x.MatricolaCreatore,
                               MimeType = x.MimeType,
                               NomeFile = x.NomeFile,
                               Password = x.Password,
                               Tipologia = x.Tipologia,
                               MyRai_TipologieFiles = null
                           }
                        ).ToList();
                    }

                    if (files != null && files.Any())
                    {
                        result.Files.AddRange(files);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        /// <summary>
        /// Reperimento dei file associati ad una determinata tipologia
        /// </summary>
        /// <param name="request">Tipologia campo obbligatorio</param>
        /// <returns></returns>
        public static FileResult GetFiles(GetFilesRequest request)
        {
            FileResult result = new FileResult();
            result.Esito = true;
            result.Errore = String.Empty;
            List<MyRai_Files> files = new List<MyRai_Files>();

            try
            {
                if (String.IsNullOrEmpty(request.Tipologia))
                {
                    result.CodiceErrore = 412;
                    throw new Exception("La tipologia è un campo obbligatorio");
                }

                if (!String.IsNullOrEmpty(request.Applicazione) && !String.IsNullOrEmpty(request.Password))
                {
                    // verifica l'abilitazione
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        var raw = db.MyRai_AccessoAPI.Where(w => w.Utente.Equals(request.Applicazione) && w.KeyString.Equals(request.Password)).FirstOrDefault();

                        if (raw == null)
                        {
                            result.CodiceErrore = 401;
                            throw new Exception("Accesso negato");
                        }

                        if (!String.IsNullOrEmpty(raw.ActionPermesse) && !raw.ActionPermesse.ToUpper().Contains("GETFILES"))
                        {
                            result.CodiceErrore = 401;
                            throw new Exception("Non si dispone dei diritti necessari per l'azione richiesta");
                        }

                        if (!String.IsNullOrEmpty(raw.ActionNegate) && raw.ActionPermesse.ToUpper().Contains("GETFILES"))
                        {
                            result.CodiceErrore = 401;
                            throw new Exception("Non si dispone dei diritti necessari per l'azione richiesta");
                        }
                    }
                }
                else if (String.IsNullOrEmpty(request.Applicazione) && String.IsNullOrEmpty(request.Password))
                {

                }
                else
                {
                    result.CodiceErrore = 401;
                    throw new Exception("Accesso negato");
                }

                switch (request.Tipologia.ToUpper())
                {
                    case "FSP":
                        result = GetFSPFiles(request);
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                result.CodiceErrore = result.CodiceErrore == 0 ? 400 : result.CodiceErrore;
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }

        #endregion

        #region UPDATE
        /// <summary>
        /// Il metodo cerca l'oggetto con id pari a file.Id e ne aggiorna le proprietà
        /// ma senza effettuare il db.savechanges che dovrà essere effettuato dal chiamante
        /// </summary>
        /// <param name="file"></param>
        /// <param name="db"></param>
        public static void UpdateFile(MyRai_Files file, ref digiGappEntities db)
        {
            var item = db.MyRai_Files.Where(w => w.Id.Equals(file.Id)).FirstOrDefault();
            if (item != null)
            {
                item.Chiave = file.Chiave;
                item.ContentByte = file.ContentByte;
                item.Json = file.Json;
                item.Length = file.Length;
                item.MimeType = file.MimeType;
                item.MyRai_TipologieFiles = file.MyRai_TipologieFiles;
                item.NomeFile = file.NomeFile;
                item.Tipologia = file.Tipologia;
            }
            else
            {
                throw new Exception("Nessun file trovato");
            }
        }

        #endregion

        public static FileResult DeleteFile(int id)
        {
            FileResult result = new FileResult();
            result.Esito = false;
            result.Errore = String.Empty;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Files.Where(w => w.Id.Equals(id)).FirstOrDefault();

                    if (item != null)
                    {
                        db.MyRai_Files.Remove(item);
                        db.SaveChanges();
                    }
                }
                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<MyRai_Files>();
            }
            return result;
        }
    }

    /// <summary>
    /// Classe manager per la gestione dei Fogli Spese
    /// </summary>
    public static class FSPManager
    {
        /// <summary>
        /// Generazione della chiave per il documento associato ad un foglio spese
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nVoce"></param>
        /// <returns></returns>
        public static string GeneraChiave(this TFoglio_Spese value, int nVoce = 0)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string numFoglio = value.ID.ToString();
            string numVoce = "";
            if (nVoce != 0)
            {
                numVoce = nVoce.ToString();
            }

            string result = "";

            if (nVoce != 0)
            {
                result = String.Format("{0}_{1}", numFoglio, numVoce);
            }
            else
            {
                result = String.Format("{0}", numFoglio);
            }

            return result;
        }

        /// <summary>
        /// Generazione della chiave per il documento associato ad un foglio spese
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nVoce"></param>
        /// <returns></returns>
        public static string GeneraChiave(decimal idFoglio, int nVoce = 0)
        {
            if (idFoglio == 0)
            {
                return string.Empty;
            }

            string numFoglio = idFoglio.ToString();
            string numVoce = "";
            if (nVoce != 0)
            {
                numVoce = nVoce.ToString();
            }

            string result = "";

            if (nVoce != 0)
            {
                result = String.Format("{0}_{1}", numFoglio, numVoce);
            }
            else
            {
                result = String.Format("{0}", numFoglio);
            }

            return result;
        }
    }

    /// <summary>
    /// Classe manager per la gestione dell'incentivazione
    /// </summary>
    public static class INCFileManager
    {
        public static string GeneraChiave(int codice, bool fromTemplate, int idStato, int id_File)
        {
            return String.Format("{0}_{1}_{2:00}_{3}", codice, fromTemplate ? "T" : "N", idStato, id_File);
        }
        public static string GeneraChiaveRicerca(int codice, bool fromTemplate, int idStato)
        {
            return String.Format("{0}_{1}_{2:00}_", codice, fromTemplate ? "T" : "N", idStato);
        }
    }

    public static class HRISFileManager
    {
        private static string GeneraChiave(this XR_TB_FILES value)
        {
            return String.Format("{0}_{1}", value.Tipologia, Guid.NewGuid());
        }

        public static HRISFileResult UploadFile(List<XR_TB_FILES> files, int idRif, 
                                            string chiave = null, string password = null,
                                            string json = null)
        {
            HRISFileResult result = new HRISFileResult();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    int id = -1;
                    foreach (var file in files)
                    {
                        file.Id = id;
                        db.XR_TB_FILES.Add(file);

                        if (String.IsNullOrEmpty(chiave))
                        {
                            chiave = GeneraChiave(file);
                        }

                        XR_TB_ASSOCIATIVEFILES associativa = new XR_TB_ASSOCIATIVEFILES()
                        {
                            Id = id,
                            Chiave = chiave,
                            Password = password,
                            Json = json,
                            IdFile = file.Id
                        };

                        switch (file.Tipologia)
                        {
                            case "ASS":
                                associativa.IdAssunzione = idRif;
                                break;
                            case "ASSGEN":
                                associativa.IdAssunzione = idRif;
                                break;
                            case "ASSGENCUSTOM":
                                associativa.IdAssunzione = idRif;
                                break;


                            default:
                                throw new Exception("Tipologia di file non gestita");
                        }

                        db.XR_TB_ASSOCIATIVEFILES.Add(associativa);
                        id--;
                    }
                    db.SaveChanges();

                    if (files.Count(w => w.Id == 0) > 1)
                    {
                        var items = files.Where(w => w.Id == 0).ToList();

                        if (items != null && items.Any())
                        {
                            string tx = "";
                            foreach (var i in items)
                            {
                                tx = tx + "Si è verificato un errore durante il salvataggio del file: " + i.NomeFile;
                                tx = tx + System.Environment.NewLine;
                            }
                            throw new Exception(tx);
                        }
                    }
                    else
                    {
                        result.Errore = String.Empty;
                        result.Esito = true;
                        result.Files.AddRange(files);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        #region Download

        public static HRISFileResult GetFile(int id)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_TB_FILES.Include("XR_TB_ASSOCIATIVEFILES").Where(w => w.Id.Equals(id)).FirstOrDefault();
                    if (item != null)
                    {
                        result.Files.Add(item);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        public static HRISFileResult GetFileByChiave(string chiave)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    XR_TB_FILES item = db.XR_TB_ASSOCIATIVEFILES.Include("XR_TB_FILES").Where(w => w.Chiave.Equals(chiave)).FirstOrDefault().XR_TB_FILES;

                    if (item != null)
                    {
                        result.Files.Add(item);
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        public static HRISFileResult GetFilesByMatricola(string matricolaCreatore)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var items = db.XR_TB_FILES.Include("XR_TB_ASSOCIATIVEFILES").Where(w => w.MatricolaCreatore.Equals(matricolaCreatore)).ToList();

                    if (items != null && items.Any())
                    {
                        result.Files.AddRange(items.ToList());
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        public static HRISFileResult GetFilesByTipologia(string tipologia)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var items = db.XR_TB_FILES.Include("XR_TB_ASSOCIATIVEFILES").Where(w => w.Tipologia.ToUpper().Equals(tipologia.ToUpper())).ToList();

                    if (items != null && items.Any())
                    {
                        result.Files.AddRange(items.ToList());
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        public static HRISFileResult GetFilesByIdParent(int id, string tipologia = null)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var items = (from a in db.XR_TB_ASSOCIATIVEFILES
                                 join f in db.XR_TB_FILES
                                 on a.IdFile equals f.Id
                                 where a.IdAssunzione == id
                                 && f.Attivo
                                 select f);

                    if (items != null && items.Any() && !String.IsNullOrEmpty(tipologia))
                    {
                        items = items.Where(w => w.Tipologia.Trim() == tipologia);
                    }

                    List<XR_TB_FILES> _tempFiles = new List<XR_TB_FILES>();
                    _tempFiles = items.ToList();

                    if (_tempFiles != null && _tempFiles.Any())
                    {
                        result.Files.AddRange(_tempFiles.ToList());
                    }
                    else
                    {
                        result.CodiceErrore = 404;
                        throw new Exception("Nessun file trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }

        #endregion

        #region Delete
        public static HRISFileResult DeleteFile(int id)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = false;
            result.Errore = String.Empty;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_TB_FILES.Where(w => w.Id.Equals(id)).FirstOrDefault();
                    var item2 = db.XR_TB_ASSOCIATIVEFILES.Where(w => w.IdFile.Equals(id)).FirstOrDefault();
                    if (item2 != null)
                    {
                        db.XR_TB_ASSOCIATIVEFILES.Remove(item2);
                        db.SaveChanges();
                    }
                    if (item != null)
                    {
                        db.XR_TB_FILES.Remove(item);
                        db.SaveChanges();
                    }
                    
                }
                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Files = new List<XR_TB_FILES>();
            }
            return result;
        }
        #endregion
    }

    public static class FileAssunzioneManager
    {
        #region Upload
        public static HRISFileResult UploadFile(int id_XR_IMM_IMMATRICOLAZIONI, string matricola, 
                                            HttpPostedFileBase file, string chiave = null, bool attivo = true)
        {
            HRISFileResult result = new HRISFileResult();
            List<XR_TB_FILES> files = new List<XR_TB_FILES>();

            try
            {
                if (file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);

                    byte[] data;
                    using (Stream inputStream = file.InputStream)
                    {
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        data = memoryStream.ToArray();
                    }

                    int length = data.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    // prepara l'oggetto da salvare
                    XR_TB_FILES newItem = new XR_TB_FILES()
                    {
                        Attivo = attivo,
                        ContentByte = data,
                        Length = length,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Tipologia = "ASS",
                        MatricolaCreatore = matricola
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = HRISFileManager.UploadFile(files, id_XR_IMM_IMMATRICOLAZIONI, chiave);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }


        public static HRISFileResult UploadFile(int id_XR_IMM_IMMATRICOLAZIONI, string matricola, 
                                                HttpRequestBase request, string chiave = null, bool attivo = true)
        {
            HRISFileResult result = new HRISFileResult();
            result.Esito = true;
            List<XR_TB_FILES> files = new List<XR_TB_FILES>();

            try
            {
                if (request.Files != null && request.Files.Count > 0)
                {
                    bool almenoUnoVuoto = false;
                    for (int idx = 0; idx < request.Files.Count; idx++)
                    {
                        var file = request.Files[idx];
                        if (file.ContentLength <= 0)
                        {
                            almenoUnoVuoto = true;
                        }
                    }

                    if (almenoUnoVuoto)
                    {
                        throw new Exception("Uno o più file sono vuoti");
                    }

                    for (int idx = 0; idx < request.Files.Count; idx++)
                    {
                        var file = request.Files[idx];
                        string fileName = Path.GetFileName(file.FileName);

                        byte[] data;
                        using (Stream inputStream = file.InputStream)
                        {
                            MemoryStream memoryStream = inputStream as MemoryStream;
                            if (memoryStream == null)
                            {
                                memoryStream = new MemoryStream();
                                inputStream.CopyTo(memoryStream);
                            }
                            data = memoryStream.ToArray();
                        }

                        int length = data.Length;
                        string est = Path.GetExtension(fileName);
                        string tipoFile = MimeTypeMap.GetMimeType(est);

                        XR_TB_FILES newItem = new XR_TB_FILES()
                        {
                            Attivo = attivo,
                            ContentByte = data,
                            Length = length,
                            MimeType = tipoFile,
                            NomeFile = fileName,
                            Tipologia = "ASS",
                            MatricolaCreatore = matricola
                        };

                        files.Add(newItem);
                    }
                    
                    if (files != null && files.Any())
                    {
                        result = HRISFileManager.UploadFile(files, id_XR_IMM_IMMATRICOLAZIONI, chiave);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Non è stato trovato alcun file da salvare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        public static HRISFileResult UploadFile(int id_XR_IMM_IMMATRICOLAZIONI, string matricola, string fileName, 
                                                byte[] fileContent, string chiave = null, bool attivo = true)
        {
            HRISFileResult result = new HRISFileResult();
            List<XR_TB_FILES> files = new List<XR_TB_FILES>();

            try
            {
                if (fileContent.Length > 0 && !String.IsNullOrEmpty(fileName))
                {
                    int length = fileContent.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    XR_TB_FILES newItem = new XR_TB_FILES()
                    {
                        Attivo = attivo,
                        ContentByte = fileContent,
                        Length = length,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Tipologia = "ASS",
                        MatricolaCreatore = matricola
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = HRISFileManager.UploadFile(files, id_XR_IMM_IMMATRICOLAZIONI, chiave);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }
        public static HRISFileResult UploadFileContratto(int id_XR_IMM_IMMATRICOLAZIONI, string matricola,
                                            HttpPostedFileBase file, string chiave = null, bool attivo = true, string tipologia = "ASSGEN", bool firmato = false)
        {
            HRISFileResult result = new HRISFileResult();
            List<XR_TB_FILES> files = new List<XR_TB_FILES>();

            try
            {
                if (file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);

                    byte[] data;
                    using (Stream inputStream = file.InputStream)
                    {
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        data = memoryStream.ToArray();
                    }

                    int length = data.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    // prepara l'oggetto da salvare
                    XR_TB_FILES newItem = new XR_TB_FILES()
                    {
                        Attivo = attivo,
                        ContentByte = data,
                        Length = length,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Tipologia = tipologia,
                        MatricolaCreatore = matricola,
                        Firmato = firmato
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = HRISFileManager.UploadFile(files, id_XR_IMM_IMMATRICOLAZIONI, chiave);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }
        public static HRISFileResult UploadFileContrattoPostFirmaDigitale(int id_XR_IMM_IMMATRICOLAZIONI, string matricola,
                                            byte[] file, string chiave = null, bool attivo = true, string tipologia = "ASSGEN", bool firmato = false, string fileName = "")
        {
            HRISFileResult result = new HRISFileResult();
            List<XR_TB_FILES> files = new List<XR_TB_FILES>();

            try
            {
                if (file.Length > 0)
                {
                    //string fileName = FileName;

                    //byte[] data;
                    //using (Stream inputStream = file.InputStream)
                    //{
                        //MemoryStream memoryStream = inputStream as MemoryStream;
                        //if (memoryStream == null)
                        //{
                        //    memoryStream = new MemoryStream();
                        //    inputStream.CopyTo(memoryStream);
                        //}
                        //data = memoryStream.ToArray();
                    //}

                    int length = file.Length;
                    string est = Path.GetExtension(fileName);
                    string tipoFile = MimeTypeMap.GetMimeType(est);

                    // prepara l'oggetto da salvare
                    XR_TB_FILES newItem = new XR_TB_FILES()
                    {
                        Attivo = attivo,
                        ContentByte = file,
                        Length = length,
                        MimeType = tipoFile,
                        NomeFile = fileName,
                        Tipologia = tipologia,
                        MatricolaCreatore = matricola,
                        Firmato = firmato
                    };

                    files.Add(newItem);

                    if (files != null && files.Any())
                    {
                        result = HRISFileManager.UploadFile(files, id_XR_IMM_IMMATRICOLAZIONI, chiave);
                    }
                    else
                    {
                        throw new Exception("L'elenco dei files da salvare è vuoto.");
                    }
                }
                else
                {
                    throw new Exception("Nessun file da caricare");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        #endregion

        #region Download

        public static HRISFileResult GetFile(int id)
        {
            return HRISFileManager.GetFile(id);
        }

        public static HRISFileResult GetFileByChiave(string chiave)
        {
            return HRISFileManager.GetFileByChiave(chiave);
        }

        public static HRISFileResult GetFilesByMatricola(string matricolaCreatore)
        {
            return HRISFileManager.GetFilesByMatricola(matricolaCreatore);
        }

        public static HRISFileResult GetFilesByTipologia()
        {
            return HRISFileManager.GetFilesByTipologia("ASS");
        }

        public static HRISFileResult GetFilesByIdAssunzione(int id, string tipologia = null)
        {
            return HRISFileManager.GetFilesByIdParent(id, tipologia);
        }


        #endregion

        #region Delete
        public static HRISFileResult DeleteFile(int id)
        {
            return HRISFileManager.DeleteFile(id);
        }
        #endregion
    }
}


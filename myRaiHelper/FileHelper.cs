using myRai.DataAccess;
using myRaiData;
using myRaiDataTalentia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace myRaiHelper
{
    public enum FileOperError
    {
        None,
        CategoryNotFound,
        FileNotFound,
        Exception,
        Generic
    }

    public class FileData
    {
        public FileData()
        {

        }
        public FileData(MyRai_DocumentiDipendente doc)
        {
            FileName = doc.nomefile;
            ContentType = doc.formato;
            Content = doc.byte_content;
            Categoria = doc.categoria;
        }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public string Categoria { get; set; }
    }

    public class FileOperResult
    {
        public FileOperResult()
        {
            ErrorType = FileOperError.None;
        }
        public bool Esito { get; set; }
        public string Errore { get; set; }
        public FileOperError ErrorType { get; set; }
        public FileData File { get; set; }
        public List<FileData> Files { get; set; }
    }

    public class FileOperLog
    {
        public FileOperLog(string user, string terminal)
        {
            User = user;
            Terminal = terminal;
        }

        public string User { get; set; }
        public string Terminal { get; set; }
    }

    public class FileHelper
    {
        public static FileOperResult UploadFile(string matricola, string categoria, FileOperLog logInfo, HttpPostedFileBase postedFile, bool overwriteExistingFile = false)
        {
            return UploadFile(matricola, categoria, logInfo, postedFile.FileName, postedFile.ContentType, postedFile.ContentLength, postedFile.InputStream, overwriteExistingFile);
        }
        public static FileOperResult UploadFile(string matricola, string categoria, FileOperLog logInfo, string fileName, string fileContentType, int fileContentLength, Stream fileStream, bool overwriteExistingFile = false)
        {
            byte[] fileBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return UploadFile(matricola, categoria, logInfo, fileName, fileContentType, fileContentLength, fileBytes, overwriteExistingFile);
        }
        public static FileOperResult UploadFile(string matricola, string categoria, FileOperLog logInfo, string fileName, string fileContentType, int fileContentLength, byte[] fileBytes, bool overwriteExistingFile = false)
        {
            FileOperResult result = new FileOperResult();
            digiGappEntities db = new digiGappEntities();


            bool isNew = false;
            MyRai_DocumentiDipendente doc = null;
            if (overwriteExistingFile)
                doc = db.MyRai_DocumentiDipendente.FirstOrDefault(x => x.matricola == matricola && x.categoria == categoria && x.nomefile == fileName);

            isNew = doc == null;
            if (isNew)
                doc = new MyRai_DocumentiDipendente()
                {
                    matricola = matricola,
                    categoria = categoria,
                    nomefile = fileName
                };

            doc.byte_content = fileBytes;
            doc.formato = fileContentType;

            doc.data_inserito = DateTime.Now;

            if (isNew)
                db.MyRai_DocumentiDipendente.Add(doc);

            //TODO: inserimento log
            try
            {
                db.SaveChanges();

                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.ErrorType = FileOperError.Exception;
                result.Errore = ex.Message;
            }


            return result;
        }

        public static FileOperResult DownloadFile(string matricola, string categoria, string fileName)
        {
            FileOperResult result = new FileOperResult();
            digiGappEntities db = new digiGappEntities();


            MyRai_DocumentiDipendente doc = db.MyRai_DocumentiDipendente.FirstOrDefault(x => x.matricola == matricola && x.categoria == categoria && x.nomefile == fileName);
            if (doc == null)
            {
                result.ErrorType = FileOperError.FileNotFound;
                result.Errore = "File non trovato";
            }
            else
            {
                FileData file = new FileData(doc);
                result.Esito = true;
                result.File = file;
            }


            return result;
        }
        public static FileOperResult DownloadFile(string matricola, int idFile)
        {
            FileOperResult result = new FileOperResult();
            digiGappEntities db = new digiGappEntities();

            MyRai_DocumentiDipendente doc = db.MyRai_DocumentiDipendente.FirstOrDefault(x => x.id == idFile);
            if (doc == null)
            {
                result.ErrorType = FileOperError.FileNotFound;
                result.Errore = "File non trovato";
            }
            else
            {
                FileData file = new FileData(doc);
                result.Esito = true;
                result.File = file;
            }

            return result;
        }

        public static FileOperResult DeleteFile(string matricola, string categoria, string fileName)
        {
            FileOperResult result = new FileOperResult();
            digiGappEntities db = new digiGappEntities();

            MyRai_DocumentiDipendente doc = db.MyRai_DocumentiDipendente.FirstOrDefault(x => x.matricola == matricola && x.nomefile == fileName);
            if (doc == null)
            {
                result.ErrorType = FileOperError.FileNotFound;
                result.Errore = "File non trovato";
            }
            else
            {
                db.MyRai_DocumentiDipendente.Remove(doc);
                try
                {
                    db.SaveChanges();

                    result.Esito = true;
                }
                catch (Exception ex)
                {
                    result.ErrorType = FileOperError.Exception;
                    result.Errore = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Permette il download multiplo dei file di una matricola
        /// </summary>
        /// <param name="matricola">Matricola del dipendente.</param>
        /// <param name="categorie">Categorie da scaricare, separate dal carattere da virgola</param>
        /// <param name="fileName">Filtro sul nome file. Ammette l'utilizzo del carattere '*' come wildcard (inizio e/o fine stringa)</param>
        /// <returns></returns>
        public static FileOperResult DownloadFiles(string matricola, string categorie = "", string fileName = "")
        {
            FileOperResult result = new FileOperResult();
            digiGappEntities db = new digiGappEntities();

            var query = db.MyRai_DocumentiDipendente.Where(x => x.matricola == matricola);
            if (!String.IsNullOrWhiteSpace(categorie))
            {
                string[] listCat = categorie.Split('|');
                query = query.Where(x => categorie.Contains(x.categoria));
            }

            if (!String.IsNullOrWhiteSpace(fileName))
            {
                string tmp = fileName.Replace("*", "");

                if (fileName.StartsWith("*") && fileName.EndsWith("*"))
                    query = query.Where(x => x.nomefile.Contains(tmp));
                else if (fileName.StartsWith("*"))
                    query = query.Where(x => x.nomefile.StartsWith(tmp));
                else if (fileName.EndsWith("*"))
                    query = query.Where(x => x.nomefile.EndsWith(tmp));
                else
                    query = query.Where(x => x.nomefile== tmp);
            }

            if (query.Any())
            {
                result.Esito = true;
                result.Files = new List<FileData>();
                result.Files.AddRange(query.ToList().Select(x => new FileData(x)));
            }
            else
            {
                result.ErrorType = FileOperError.FileNotFound;
                result.Errore = "Nessun file trovato";
            }

            return result;
        }
    }
}

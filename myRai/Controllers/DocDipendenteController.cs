using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiHelper;

namespace myRai.Controllers
{
    public class DocDipendenteController : BaseCommonController
    {
        public ActionResult Index(DocDipendenteModel model)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var db = new myRaiData.digiGappEntities();
            foreach (var item in db.MyRai_Tipologia_Documento)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.id.ToString(),
                    Text = item.TipologiaDocumento,
                    Selected = false
                });
            }
            model.listaTipologie = new SelectList(list, "Value", "Text");
            return View(model);
        }

        public ActionResult listaDoc(string matr = null)
        {
            if (matr == null) matr = CommonHelper.GetCurrentUserMatricola();

            DocDipendenteModel model = new DocDipendenteModel();
            var db = new myRaiData.digiGappEntities();
            model.Documenti = db.MyRai_DocumentiDipendente
                .Where(x => x.matricola == matr && x.attivo == true)
                .Select(x =>
                        new DocDipendente()
                        {
                            datainserito = x.data_inserito,
                            descrizione = x.descrizione,
                            id = x.id,
                            nomefile = x.nomefile,
                            tipologia = x.MyRai_Tipologia_Documento.TipologiaDocumento,
                            docDB=x
                        }).ToList();

            return View(model.Documenti);
        }

        public ActionResult Upload(FileUploadModel model)
        {
            var db = new myRaiData.digiGappEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            if (db.MyRai_DocumentiDipendente.Any(x => x.matricola == matr && x.attivo == true && x.nomefile == model.filename))
            {
                return RedirectToAction("index", new { err = "Un file con lo stesso nome è già presente." });
            }

            if (!model.filename.ToLower().EndsWith("pdf"))
            {
                return RedirectToAction("index", new { err = "Sono ammessi solo file PDF" });
         
            }
            myRaiData.MyRai_DocumentiDipendente doc = new myRaiData.MyRai_DocumentiDipendente();
            doc.attivo = true;
            using (var binaryReader = new BinaryReader(model.fileupload.InputStream))
            {
                doc.byte_content = binaryReader.ReadBytes(model.fileupload.ContentLength);
            }
            doc.categoria = "ESS";
            doc.data_inserito = DateTime.Now;
            doc.descrizione = model.note;
            doc.formato = model.fileupload.ContentType;
            doc.nomefile = model.filename;
            doc.matricola = CommonHelper.GetCurrentUserMatricola();
            doc.id_tipologia_documento = model.idTipologia;
            
            db.MyRai_DocumentiDipendente.Add(doc);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return RedirectToAction("index", new { err = ex.Message });
            }

            return RedirectToAction("index");
        }

        public ActionResult GetPdfBinary(int idPdf)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_DocumentiDipendente.Where(x => x.id == idPdf).FirstOrDefault();

            if (pdf == null) return null;
            if ( !UtenteHelper.IsBoss( CommonHelper.GetCurrentUserMatricola( ) ) && !UtenteHelper.IsBossLiv2( CommonHelper.GetCurrentUserMatricola( ) ) && pdf.matricola != CommonHelper.GetCurrentUserMatricola( ) )
                return null;

            byte[] byteArray = pdf.byte_content;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.nomefile);
            return new FileStreamResult(pdfStream, "application/pdf");
        }
        public ActionResult DelDoc(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_DocumentiDipendente.Where(x => x.id == id).FirstOrDefault();
            if (pdf.matricola != CommonHelper.GetCurrentUserMatricola()) 
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "NON AUTORIZZATO" }
                };
            }
            pdf.attivo = false;
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Errore DB" }
                };
            }
        }
    }

    public class FileUploadModel
    {
        public string note { get; set; }
        public string filename { get; set; }
        public HttpPostedFileBase fileupload { get; set; }
        public int idTipologia { get; set; }
    }
}
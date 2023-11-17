using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using myRai.DataAccess;
using myRaiData;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class BackupManager
    {
        public static string CheckSavedData(string matricola)
        {
            string[] files = System.IO.Directory.GetFiles(HttpContext.Current.Server.MapPath("~/app_data/savedata"), "*.xml");
            if (files == null || files.Length == 0) return null;

            string returnMessage = null;
            foreach (string file in files)
            {
                returnMessage += "<br />Processo file " + file;
                try
                {
                    List<MyRai_Eccezioni_Richieste> LM = Deserialize(file);
                    if (LM != null && LM.Count > 0)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        int IdRichiestaPadre = Convert.ToInt32(fileName);
                        var db = new digiGappEntities();
                        var rich = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();
                        if (rich == null)
                        {
                            returnMessage += " / IdRichiesta non trovato:" + IdRichiestaPadre;
                            continue;
                        }
                        if (rich.MyRai_Eccezioni_Richieste.Count > 0)
                        {
                            returnMessage += " / La richiesta Id " + IdRichiestaPadre + " possiede già eccezioni nel DB :" + rich.MyRai_Eccezioni_Richieste.Count.ToString();
                            continue;
                        }

                        foreach (MyRai_Eccezioni_Richieste mr in LM)
                        {
                            rich.MyRai_Eccezioni_Richieste.Add(mr);
                        }
                        if (DBHelper.Save(db, matricola))
                        {
                            returnMessage += " / Aggiunte al DB " + LM.Count + " eccezioni";
                            try
                            {
                                System.IO.File.Delete(file);
                                returnMessage += " / File cancellato";
                            }
                            catch (Exception ex)
                            {
                                returnMessage += " / File non cancellato:" + ex.Message;
                            }
                        }
                        else returnMessage += " / Errore salvataggio DB"; 
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = file + ":" + ex.ToString(),
                        matricola = CommonHelper.GetCurrentUserMatricola(),
                        provenienza = "EccezioniManager CheckSavedData"
                    });
                    return ex.ToString();
                }
            }
            return returnMessage;
        }
        public static string SaveBackupData(int IdRichiestaPadre, List<MyRai_Eccezioni_Richieste> value, string matricola = null)
        {
            try
            {
                string serialized = Serialize(value);
                string filename = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~/app_data/savedata"),
                    IdRichiestaPadre.ToString() + ".xml");
                System.IO.File.WriteAllText(filename, serialized);
                Logger.LogAzione ( new MyRai_LogAzioni ( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = String.IsNullOrEmpty ( matricola ) ? CommonHelper.GetCurrentUserMatricola ( ) : matricola ,
                    provenienza = "BackupManager.SaveBackupData" ,
                    descrizione_operazione = "Creato xml file backup dati:" + filename
                } );
            }
            catch (Exception ex)
            {
                return ex.Message + "-" + ex.InnerException != null ? ex.InnerException.Message : "";
            }
            return null;
        }
        public static string Serialize(List<MyRai_Eccezioni_Richieste> value)
        {
            //    var db = new Data.digiGappEntities();
            //    var results = db.MyRai_Eccezioni_Richieste.Where(x => x.id_richiesta == 1088).FirstOrDefault();
       

            //  System.Dynamic.ExpandoObject expando = new System.Dynamic.ExpandoObject();
            //    var expandoDict = expando as IDictionary<string, object>;
            //    foreach (var pS in value.GetType().GetProperties())
            //    {
            //        if (pS.Name.ToLower().StartsWith("myrai") || pS.Name.ToLower().StartsWith("id_richiesta")) continue;
            //        expandoDict.Add(pS.Name, pS.GetValue(value, null));
            //    };

            //string ssss = Newtonsoft.Json.JsonConvert.SerializeObject(expandoDict,  Newtonsoft.Json.Formatting.Indented,
            //            new Newtonsoft.Json.JsonSerializerSettings
            //            {
            //                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //            });

            //    MyRai_Eccezioni_Richieste mrn = Newtonsoft.Json.JsonConvert.DeserializeObject<MyRai_Eccezioni_Richieste>(ssss);

            List<EccezioniRichiesteShadow> ListaShadow = new List<EccezioniRichiesteShadow>();
            foreach (var v in value)
            {
                EccezioniRichiesteShadow e = CommonHelper.CopyToShadow(v);
                ListaShadow.Add(e);
            }
            var xmlserializer = new XmlSerializer(typeof(List<EccezioniRichiesteShadow>));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings()
            {
                OmitXmlDeclaration = true
            }))
            {
                xmlserializer.Serialize(writer, ListaShadow);
                string serialized = stringWriter.ToString();
                return serialized;
            }
        }
        public static List<MyRai_Eccezioni_Richieste> Deserialize(string filename)
        {
            using (Stream loadstream = new FileStream(filename, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<EccezioniRichiesteShadow>));
                List<EccezioniRichiesteShadow> list = (List<EccezioniRichiesteShadow>)ser.Deserialize(loadstream);

                List<MyRai_Eccezioni_Richieste> LM = new List<MyRai_Eccezioni_Richieste>();
                foreach (var e in list)
                {
                    MyRai_Eccezioni_Richieste m = CommonHelper.CopyFromShadow(e);
                    LM.Add(m);
                }
                return LM;
            }

        }

        public static void Bulkcopy()
        {
            int quanti = 1000000;

            var cmd = new SqlCommand();

            string conn = "data source=stocav420,6000;Timeout=10000;initial catalog=digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(conn);
            cmd.Connection = new SqlConnection(conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select top 1 * from myrai_richieste";
            cmd.CommandTimeout = 50000;

            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            DataTable copyDt = dt.Clone();
            for (int i = 0; i < quanti; i++)
            {
                copyDt.ImportRow(dt.Rows[0]);
                copyDt.Rows[copyDt.Rows.Count - 1]["matricola_richiesta"] = (103650 + i).ToString();
            }


            SqlBulkCopy bulkCopy =
                new SqlBulkCopy
                (
                connection,

                SqlBulkCopyOptions.TableLock |
                SqlBulkCopyOptions.FireTriggers |
                SqlBulkCopyOptions.UseInternalTransaction,
                null
                );
            bulkCopy.BulkCopyTimeout = 10000;

            bulkCopy.DestinationTableName = "MyRai_Richieste";
            connection.Open();
            bulkCopy.WriteToServer(copyDt);
            connection.Close();






            //                                                                        
            int idPartenza = 1000103;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select top 1 * from myrai_eccezioni_richieste";


            SqlDataAdapter adapter2 = new SqlDataAdapter();
            adapter2.SelectCommand = cmd;

            DataTable dt2 = new DataTable();
            adapter2.Fill(dt2);

            DataTable copyDt2 = dt2.Clone();
            for (int i = 0; i < quanti; i++)
            {
                copyDt2.ImportRow(dt2.Rows[0]);
                copyDt2.Rows[copyDt2.Rows.Count - 1]["id_richiesta"] = (idPartenza + i).ToString();
            }


            SqlBulkCopy bulkCopy2 =
                new SqlBulkCopy
                (
                connection,

                SqlBulkCopyOptions.TableLock |
                SqlBulkCopyOptions.FireTriggers |
                SqlBulkCopyOptions.UseInternalTransaction,
                null
                );
            bulkCopy2.BulkCopyTimeout = 10000;

            bulkCopy2.DestinationTableName = "MyRai_Eccezioni_Richieste";

            connection.Open();
            bulkCopy2.WriteToServer(copyDt2);
            connection.Close();








        }
    }
}
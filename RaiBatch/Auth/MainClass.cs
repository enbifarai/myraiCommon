using myRaiCommonManager;
using myRaiCommonModel.Gestionale;
using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static myRaiHelper.AccessoFileHelper;

namespace RaiBatch.Auth
{
    public class MainClass : BatchBaseClass
    {
        public override void Entry(string[] args)
        {
            string mainOper = args[1];
            switch (mainOper)
            {
                case "ImportUtehra":
                    ImportUtehra(args.Length > 2 ? args[2] : null);
                    break;
                default:
                    break;
            }
        }

        public void ImportUtehra(string inputFile = "", string baseOutput=null)
        {
            if (baseOutput != null) _baseOutput = baseOutput;

            string directoryPath = Path.Combine(_baseOutput, "Auth");
            string fileNameOrig = $"utehra{DateTime.Today.ToString("yyyyMMdd")}.txt";
            string fileNameDest = String.Format("utehra_{0}.txt", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            var pathDestinazione = Path.Combine(directoryPath, "ImportUtehra");
            if (!Directory.Exists(pathDestinazione))
                Directory.CreateDirectory(pathDestinazione);

            string fileDest = Path.Combine(pathDestinazione, fileNameDest);

            if (String.IsNullOrWhiteSpace(inputFile))
            {
                ImpersonationHelper.Impersonate("RAI", "srvruofpo", "zaq22?mk", delegate
                {
                    string fileOrig = Path.Combine(@"\\nasrm3\PFOAC\BATCH\ABILITAZIONI\", fileNameOrig);

                    if (File.Exists(fileOrig))
                        File.Copy(fileOrig, fileDest);
                    else
                        _log.Error("File utehra.txt non trovato");
                });
            }
            else
            {
                File.Copy(inputFile, fileDest);
            }

            if (File.Exists(fileDest))
            {
                IncentiviEntities db = new IncentiviEntities();
                Dictionary<string, int> hraFunz = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "HRA").ToDictionary(x => x.COD_SUBFUNZIONE, y => y.ID_SUBFUNZ);

                if (hraFunz == null || !hraFunz.Any())
                {
                    _log.Fatal("Non è stata trovata alcuna funzione/sottofunzione HRA");
                    return;
                }

                bool anyChanges = false;

                string[] lines = File.ReadAllLines(fileDest);

                List<UtehraRow> utehraRows = lines.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => new UtehraRow(x)).ToList();

                var listAllAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE").Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "HRA").ToList();

                var listMatr = utehraRows.Where(x => x.Matricola.StartsWith("P")).Select(x => x.Matricola.Replace("P", "")).ToList();
                var listMatrToRemove = listAllAbil.Where(x => !listMatr.Contains(x.MATRICOLA)).ToList();
                foreach (var gr in listMatrToRemove.GroupBy(x => x.MATRICOLA))
                {
                    _log.Info("Rimosse abilitazioni matricola " + gr.Key);
                    foreach (var item in gr)
                    {
                        anyChanges = true;
                        db.XR_HRIS_ABIL.Remove(item);
                    }
                }

                foreach (var row in utehraRows)
                {
                    if (!row.Matricola.StartsWith("P")) continue;

                    string matr = row.Matricola.Replace("P", "");
                    var listAbil = row.Funzioni;

                    var listCurrentAbil = listAllAbil.Where(x => x.MATRICOLA == matr).ToList();

                    var listToRemove = listCurrentAbil.Where(x => !listAbil.Contains(x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE)).ToList();

                    foreach (var oldAbil in listToRemove)
                    {
                        _log.Info("Rimossa abilitazione matricola " + oldAbil.MATRICOLA+" sottofunzione "+oldAbil.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE);
                        anyChanges = true;
                        db.XR_HRIS_ABIL.Remove(oldAbil);
                    }

                    foreach (var newAbil in listAbil)
                    {
                        if (!listCurrentAbil.Any(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == newAbil))
                        {
                            if (hraFunz.TryGetValue(newAbil, out int idSubFunz))
                            {
                                myRaiData.Incentivi.XR_HRIS_ABIL dbABil = new myRaiData.Incentivi.XR_HRIS_ABIL()
                                {
                                    MATRICOLA = matr,
                                    ID_SUBFUNZ = hraFunz[newAbil],
                                    IND_ATTIVO = true
                                };
                                db.XR_HRIS_ABIL.Add(dbABil);
                                anyChanges = true;
                                _log.Info("Aggiunta abilitazione matricola " + matr + " sottofunzione " + newAbil);
                            }
                            else
                            {
                                _log.Error("Sottofunzione "+newAbil+" non trovata -  Abilitazione matricola " + matr + " non completata");
                            }

                        }
                    }
                }

                if (anyChanges)
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal("Errore durante il salvataggio", ex);
                    }
                }
            }
            else
                _log.Error("Errore durante la copia del file");
        }


    }
}

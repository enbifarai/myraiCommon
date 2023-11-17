using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Data.Entity.Infrastructure;
using myRaiData;
using System.Data.Entity;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using myRai.Data.CurriculumVitae;

namespace myRai.DataAccess
{
    public class DBHelper
    {
        public static bool SaveNoSession(digiGappEntities context, string matricola)
        {
            try
            {
                var changes = context.ChangeTracker.Entries();
                var changesI = changes.Where(x => x.State == EntityState.Added).ToList();
                var changesUD = changes.Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted).ToList();
                List<MyRai_LogDB> listUD = new List<MyRai_LogDB>();
                try
                {
                    //recupera modifiche/delete DB 
                    listUD = GetLogDBitemsNoSession(changesUD, context, matricola);
                }
                catch (Exception ex)
                {
                }

                context.SaveChanges();

                try
                {
                    //recupera insert DB 
                    List<MyRai_LogDB> listI = GetLogDBitemsInsertNoSession(changesI, context, matricola);

                    //salva in logDB:
                    SaveLogChanges(listUD, context);
                    SaveLogChanges(listI, context);
                }
                catch (Exception ex)
                {
                }

                MyRai_LogAzioni act = new MyRai_LogAzioni();
                act.matricola = matricola;
                act.operazione = "Save";
                act.provenienza = new StackFrame(1, true).GetMethod().Name;
                act.data = DateTime.Now;
                act.applicativo = "Portale";
                return true;
            }
            catch (Exception Exception)
            {
                LogEccezione("", "", Exception, matricola );
                return false;
            }
        }

        public static List<MyRai_LogDB> GetLogDBitemsInsertNoSession(IEnumerable<DbEntityEntry> changes, digiGappEntities db, string matricola)
        {

            List<MyRai_LogDB> L = new List<MyRai_LogDB>();
            foreach (var entry in changes)
            {
                if (entry.Entity.GetType().BaseType.Name.ToLower().Contains("log")) continue;

                EntityKey key = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager
                      .GetObjectStateEntry(entry.Entity).EntityKey;

                var properties = entry.CurrentValues.PropertyNames;
                string desc = "";
                foreach (string prop in properties)
                {
                    desc += prop + ":" + entry.Property(prop).CurrentValue + ", ";
                }
                L.Add(new MyRai_LogDB()
                {
                    Data = DateTime.Now,
                    Matricola = matricola,
                    Modifiche = desc,
                    Operazione = "I",
                    NomeTabella = key.EntitySetName,
                    IdTabella = Convert.ToInt32(key.EntityKeyValues[0].Value)
                });
            }
            return L;
        }

        public static List<MyRai_LogDB> GetLogDBitemsNoSession(IEnumerable<DbEntityEntry> changes, digiGappEntities db, string matricola)
        {
            try
            {
                List<MyRai_LogDB> L = new List<MyRai_LogDB>();
                foreach (var entry in changes)
                {
                    if (entry.Entity.GetType().BaseType.Name.ToLower().Contains("log")) continue;

                    EntityKey key = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager
                           .GetObjectStateEntry(entry.Entity).EntityKey;

                    MyRai_LogDB l = new MyRai_LogDB();
                    l.Operazione = entry.State == EntityState.Modified ? "U" : "D";
                    l.Data = DateTime.Now;
                    l.IdTabella = Convert.ToInt32(key.EntityKeyValues[0].Value);
                    l.NomeTabella = entry.Entity.GetType().BaseType.Name;
                    l.Matricola = matricola;

                    foreach (string prop in entry.GetDatabaseValues().PropertyNames)
                    {
                        string orig = "";
                        if (entry.Property(prop).OriginalValue != null) orig = entry.Property(prop).OriginalValue.ToString();
                        string curr = "";
                        if (entry.Property(prop).CurrentValue != null) curr = entry.Property(prop).CurrentValue.ToString();
                        if (entry.State == EntityState.Deleted)
                        {
                            l.Modifiche += prop + ":" + orig + ", ";
                        }
                        else
                        {
                            if (curr != orig) l.Modifiche += prop + ":" + orig + "-->" + curr + ", ";
                        }
                    }
                    L.Add(l);
                }
                return L;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool Save(digiGappEntities context, string matricola)
        {
            try
            {
                var changes = context.ChangeTracker.Entries();
                var changesI = changes.Where(x => x.State == EntityState.Added).ToList();
                var changesUD = changes.Where(x => x.State == EntityState.Modified || x.State==EntityState.Deleted).ToList();
                List<MyRai_LogDB> listUD = new List<MyRai_LogDB>();
                try
                {
                    //recupera modifiche/delete DB 
                    listUD = GetLogDBitems(changesUD, context, matricola );
                }
                catch (Exception ex)
                {
                }                

                context.SaveChanges();
                                
                try
                {
                    //recupera insert DB 
                    List<MyRai_LogDB> listI = GetLogDBitemsInsert( changesI , context , matricola );
                  
                    //salva in logDB:
                    SaveLogChanges(listUD, context);
                    SaveLogChanges(listI, context);
                }
                catch (Exception ex)
                {
                }

                MyRai_LogAzioni act = new MyRai_LogAzioni();
                act.matricola = matricola;
                act.operazione = "Save";
                act.provenienza = new StackFrame(1, true).GetMethod().Name;
                act.data = DateTime.Now;
                act.applicativo = "Portale";
                return true;
            }
            catch (Exception Exception)
            {
                LogEccezione("", "", Exception, matricola );
                return false;
            }
        }
        
        public static List<MyRai_LogDB> GetLogDBitemsInsert(IEnumerable <DbEntityEntry> changes, digiGappEntities db, string matricola)
        {
            List<MyRai_LogDB> L = new List<MyRai_LogDB>();
            foreach (var entry in changes)
            {
                if (entry.Entity.GetType().BaseType.Name.ToLower().Contains("log")) continue;

                EntityKey key = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager
                      .GetObjectStateEntry(entry.Entity).EntityKey;

                var properties = entry.CurrentValues.PropertyNames;
                string desc = "";
                foreach (string prop in properties)
                {
                    desc += prop + ":" + entry.Property(prop).CurrentValue + ", ";
                }
                L.Add(new MyRai_LogDB()
                {
                    Data = DateTime.Now,
                    Matricola = matricola + " N:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    Modifiche = desc,
                    Operazione = "I",
                    NomeTabella = key.EntitySetName,
                    IdTabella = Convert.ToInt32(key.EntityKeyValues[0].Value)
                });
            }
            return L;
        }

        public static List<MyRai_LogDB> GetLogDBitems(IEnumerable <DbEntityEntry> changes, digiGappEntities db, string matricola)
        {
            try
            {
                List<MyRai_LogDB> L = new List<MyRai_LogDB>();
                foreach (var entry in changes)
                {
                    if (entry.Entity.GetType().BaseType.Name.ToLower().Contains("log")) continue;

                    EntityKey key = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager
                           .GetObjectStateEntry(entry.Entity).EntityKey;

                    MyRai_LogDB l = new MyRai_LogDB();
                    l.Operazione = entry.State == EntityState.Modified ? "U" : "D";
                    l.Data = DateTime.Now;
                    l.IdTabella = Convert.ToInt32(key.EntityKeyValues[0].Value);
                    l.NomeTabella = entry.Entity.GetType().BaseType.Name;
                    l.Matricola = matricola + " N:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    foreach (string prop in entry.GetDatabaseValues().PropertyNames)
                    {
                        string orig = "";
                        if (entry.Property(prop).OriginalValue != null) orig = entry.Property(prop).OriginalValue.ToString();
                        string curr = "";
                        if (entry.Property(prop).CurrentValue != null) curr = entry.Property(prop).CurrentValue.ToString();
                        if (entry.State == EntityState.Deleted)
                        {
                            l.Modifiche += prop + ":" + orig + ", ";
                        }
                        else
                        {
                            if (curr != orig) l.Modifiche += prop + ":" + orig + "-->" + curr + ", ";
                        }
                    }
                    L.Add(l);
                }
                return L;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Salvataggio protetto ad uso CV, logga in MyRai_LogErrori :
        public static bool Save(cv_ModelEntities context, string matricola , string descrOperazione = "", int percentuale= 0, bool updatePerc=true, string provenienza="CV")
        {
            try
            {
                context.SaveChanges();

                if (updatePerc)
                {
                    TCVLogin l = context.TCVLogin.Where(x => x.Matricola == matricola).FirstOrDefault();
                    if (l != null)
                    {
                        l.Percentuale = percentuale;
                        l.DataUltimoAgg = DateTime.Now;
                    }
                    context.SaveChanges();
                }
                return true;
            }
            catch (Exception Exception)
            {
                LogEccezione(provenienza,descrOperazione, Exception, matricola);
                return false;
            }
        }

        private static Data.CurriculumVitae.TCVLogin aggiornaDatiCV( string matricola, int percentuale) {
            Data.CurriculumVitae.TCVLogin login = new Data.CurriculumVitae.TCVLogin();
            login.Percentuale = percentuale;
            login.DataUltimoAgg = DateTime.Today;

            return login;
        }

        public static bool Save ( digiGappEntities context , string descrOperazione , string matricola )
        {
            try
            {
                context.SaveChanges( );
                MyRai_LogAzioni act = new MyRai_LogAzioni( );
                act.matricola = matricola;
                act.operazione = "Save";
                act.provenienza = new StackFrame( 1 , true ).GetMethod( ).Name;
                act.data = DateTime.Now;
                act.applicativo = "Portale";
                act.descrizione_operazione = descrOperazione;
                return true;
            }
            catch ( Exception Exception )
            {
                LogEccezione( "" , descrOperazione , Exception, matricola );
                return false;
            }
        }

        public static void SaveLogChanges(List<MyRai_LogDB> Logs, digiGappEntities db)
        {
            if (Logs == null) return;

            foreach (MyRai_LogDB log in Logs)
                db.MyRai_LogDB.Add(log);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public static bool Save(TalentiaEntities context, string matricola, string provenienza )
        {
            bool saved = false;
            
            try
            {
                context.SaveChanges();
                saved = true;
            }
            catch (Exception ex)
            {
                LogEccezione(provenienza, "", ex, matricola);
            }


            return saved;
        }
        public static int GeneraOID(DbContext context, string tabella, string fieldId)
        {
            int oid = 0;
            bool trovato = true;
            Random rnd = new Random();
            string SQLStat = "";

            while (trovato)
            {
                oid = rnd.Next();
                SQLStat = String.Format("SELECT TOP 1 {0} FROM {1} WHERE {0}={2}", fieldId, tabella, oid);
                var res = context.Database.SqlQuery<int>(SQLStat).ToList();
                if (res==null || res.Count() == 0)
                    trovato=false;
            }

            return oid;
        }

        public static bool Save(IncentiviEntities context , string matricola, string provenienza = "")
        {
            try
            {
                context.SaveChanges();            
                return true;
            }
            catch (Exception Exception)
            {
                LogEccezione(provenienza, "", Exception, matricola);
                return false;
            }
        }

        private static void LogEccezione ( string prefixProvenienza , string descrOperazione , System.Exception Exception , string matricola )
        {
            string errorMessage = "";
            try
            {
                if ( Exception is System.Data.Entity.Validation.DbEntityValidationException )
                {
                    var e = Exception as System.Data.Entity.Validation.DbEntityValidationException;
                    foreach ( var ev in e.EntityValidationErrors )
                    {
                        errorMessage += "\r\nEntità: " + ev.Entry.Entity.GetType( ).ToString( );
                        errorMessage += "\r\nStato: " + ev.Entry.State;
                        foreach ( var error in ev.ValidationErrors )
                        {
                            errorMessage += "\r\n" + error.ErrorMessage;
                            try
                            {
                                if ( !String.IsNullOrWhiteSpace( error.PropertyName ) )
                                {
                                    var tempValue = ev.Entry.CurrentValues.GetValue<object>( error.PropertyName );
                                    errorMessage += "\r\n" + error.PropertyName + ":'";
                                    if ( tempValue != null )
                                        errorMessage += tempValue.ToString( );
                                    else
                                        errorMessage += "null";
                                    errorMessage += "'";
                                }
                            }
                            catch
                            {
                                errorMessage += "\r\nValore non disponibile";
                            }
                        }
                    }
                }
                else if ( Exception is DbUpdateException )
                {
                    var e = Exception as DbUpdateException;
                    foreach ( var entry in e.Entries )
                    {
                        errorMessage += "\r\nStato: " + entry.State.ToString( );
                        if ( entry.State != EntityState.Deleted && entry.CurrentValues != null )
                        {
                            errorMessage += "\r\nValori entità:\r\n";
                            foreach ( var propName in entry.CurrentValues.PropertyNames )
                            {
                                errorMessage += String.Format( "{0} = '{1}'\r\n" , propName , entry.CurrentValues.GetValue<object>( propName ) );
                            }
                        }
                    }
                }
            }
            catch ( Exception eee )
            {
            }

            MyRai_LogErrori err = new MyRai_LogErrori( );
            err.matricola = matricola;
            err.error_message = Exception.ToString( ) + errorMessage;
            err.applicativo = "Portale";
            err.data = DateTime.Now;
            err.provenienza = ( !String.IsNullOrWhiteSpace( prefixProvenienza ) ? prefixProvenienza + " " : "" ) + new StackFrame( 2 , true ).GetMethod( ).Name + descrOperazione;

            Logger.LogErrori( err );
        }
    }
}
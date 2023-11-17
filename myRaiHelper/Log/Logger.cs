using System;
using myRaiData;

namespace myRaiHelper
{
    public class Logger
    {
        public static Boolean LogAzione(MyRai_LogAzioni azione, string matricola = null)
        {
            using (var db = new digiGappEntities())
            {
                azione.data = DateTime.Now;
                azione.applicativo = "Portale";
                azione.matricola = !String.IsNullOrWhiteSpace(matricola)?matricola:CommonHelper.GetCurrentUserMatricola();

                azione.provenienza = GetServerName() + azione.provenienza;
                if (azione.provenienza != null && azione.provenienza.Length > 100)
                    azione.provenienza = azione.provenienza.Substring(0, 99);

                db.MyRai_LogAzioni.Add(azione);

                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static string GetServerName()
        {
            try
            {
                return System.Environment.MachineName+"-";
            }
            catch (Exception ex)
            {
                return "e-";
            }
        }
        public static Boolean LogErrori(MyRai_LogErrori errore, string matricola = null)
        {
            using (var db = new digiGappEntities())
            {
                errore.data = DateTime.Now;
                errore.applicativo = "Portale";
                errore.matricola = !String.IsNullOrWhiteSpace(matricola)?matricola:CommonHelper.GetCurrentUserMatricola();
                errore.provenienza = GetServerName() + errore.provenienza;
                if (errore.provenienza != null && errore.provenienza.Length > 100)
                    errore.provenienza = errore.provenienza.Substring(0, 99);

                errore.error_message+="\nIdentity name:"+ System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                db.MyRai_LogErrori.Add(errore);

                try
                {
                    db.SaveChanges();
                    string s = "";
                    if (errore.error_message.ToLower().Contains("data.entity"))
                    {
                        if (db.Database.Connection.ConnectionString.Contains("ZTO"))
                        {
                            s = "PROD-";
                        }
                        else
                        {
                            s = "SVIL-";
                        }
                        SlackBot.PostMessage(s + errore.error_message);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }
    }
}

namespace myRaiCommonDatacontrollers
{
    public class Logger
    {
        public static Boolean LogAzione ( MyRai_LogAzioni azione )
        {
            using ( var db = new digiGappEntities( ) )
            {
                azione.data = DateTime.Now;
                azione.applicativo = "Portale";
                azione.provenienza = GetServerName( ) + azione.provenienza;
                if ( azione.provenienza != null && azione.provenienza.Length > 100 )
                    azione.provenienza = azione.provenienza.Substring( 0 , 99 );

                db.MyRai_LogAzioni.Add( azione );

                try
                {
                    db.SaveChanges( );
                    return true;
                }
                catch ( Exception ex )
                {
                    return false;
                }
            }
        }

        public static string GetServerName ( )
        {
            try
            {
                return System.Environment.MachineName + "-";
            }
            catch ( Exception ex )
            {
                return "e-";
            }
        }

        public static Boolean LogErrori ( MyRai_LogErrori errore )
        {
            using ( var db = new digiGappEntities( ) )
            {
                errore.data = DateTime.Now;
                errore.applicativo = "Portale";
                errore.provenienza = GetServerName( ) + errore.provenienza;
                if ( errore.provenienza != null && errore.provenienza.Length > 100 )
                    errore.provenienza = errore.provenienza.Substring( 0 , 99 );

                db.MyRai_LogErrori.Add( errore );

                try
                {
                    db.SaveChanges( );

                    return true;
                }
                catch ( Exception ex )
                {
                    return false;
                }
            }

        }
    }
}
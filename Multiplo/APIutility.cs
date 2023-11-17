using myRaiData;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo
{
    public class APIutility
    {
        public PAT_INAIL GetPatInail(string matricola, DateTime D1, DateTime D2)
        {
            PAT_INAIL P = new PAT_INAIL();
            List<PatInailRule> rules = new List<PatInailRule>();
            string pathOverridePathInail = "RulePatInail.txt";
            string pathLocalExe = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "RulePatInail.txt");

            if (File.Exists(pathOverridePathInail))
            {
                var lines = File.ReadAllLines(pathOverridePathInail);
                foreach (var line in lines.Skip(1))
                {
                    var token = line.Split(';');
                    rules.Add(new PatInailRule()
                    {
                        Matricola = token[0],
                        Categoria = token[1],
                        Mansione = token[2],
                        File = token[3],
                        Pat = token[4],
                        Inail = token[5]
                    });
                }
            }
            else if (File.Exists(pathLocalExe))
            {
                var lines = File.ReadAllLines(pathLocalExe);
                foreach (var line in lines.Skip(1))
                {
                    var token = line.Split(';');
                    rules.Add(new PatInailRule()
                    {
                        Matricola = token[0],
                        Categoria = token[1],
                        Mansione = token[2],
                        File = token[3],
                        Pat = token[4],
                        Inail = token[5]
                    });
                }
            }
            else
            {
                P.Errore = "File RulePatInail non trovato";
                return P;
            }

            var dbCzn = new IncentiviEntities();
            string query = @" select t0.[matricola_dp]   
                                   ,substring(t1.[CODICINI],3,1) as voce_te   
                                   ,t0.data_cessazione
                                   ,t2.[cod_mansione]    
                                   ,t2.[desc_mansione]   
                                   ,t2.[istituto_assicuratore_mansione]   
                                   ,t3.[tipo_minimo]   
                              	  ,t10.[cod_serv_cont] 
                              	  ,t10.[desc_serv_cont] 
                                  ,t10.[societa] 
                              	  ,t11.[desc_macro_categoria] 
                              	  ,t11.[cod_categoria] 
                              	  ,t11.[desc_categoria] 
                              	  ,t11.[desc_liv_professionale] 
                                  ,t11.[ccl] 
                                  ,t12.[desc_aggregato_sede] 
                                  ,t12.[desc_sede] 
                                  ,t13.cod_insediamento 
                                  ,t13.desc_insediamento 
                              FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0   
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica])   
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] t2 on(t2.[sky_mansione] = t1.[sky_mansione])   
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ])   
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t10 on (t1.sky_servizio_contabile=t10.sky_serv_cont) 
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] t11 on (t1.sky_categoria = t11.sky_categoria) 
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE] t12 on (t1.sky_sede = t12.sky_sede) 
                              INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO] t13 on (t0.cod_insediamento_ubicazione = t13.cod_insediamento) 
                              where t1.[flg_ultimo_record]='$' and data_cessazione>{d'2020-01-01'} ";

            List<Dati> listData = dbCzn.Database.SqlQuery<Dati>(query).ToList();

            var db = new digiGappEntities();
            List<MyRai_CacheFunzioni> listModAI = new List<MyRai_CacheFunzioni>();
            listModAI.AddRange(db.MyRai_CacheFunzioni.Where(x => x.funzione == "AssicurazioneInfortuni"));

            var sint = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            if (sint == null)
            {
                P.Errore = "Matricola non trovata in sintesi";
                return P;

            }
            string datainizio = D1.ToString("dd/MM/yyyy");
            string datafine = D2.ToString("dd/MM/yyyy");
            String Parametro15 = null;
            var Importaz = db.MyRai_Importazioni.Where(x => x.Parametro1 == "P" + matricola && x.Matricola == "ImportaProrogheSWDaCSV"
                    && x.Tabella == "ProrogaModuloSmartWorking2020" && x.Parametro4 == datainizio && x.Parametro5 == datafine).FirstOrDefault();
            if (Importaz != null)
            {
                Parametro15 = Importaz.Parametro15;
            }

            var dataElement = listData.FirstOrDefault(x => x.matricola_dp == matricola);
            var customAI = listModAI.Where(x => x.oggetto == matricola).OrderByDescending(x => x.data_creazione).FirstOrDefault();



            if (CheckRulePatInail(rules, sint.COD_MATLIBROMAT, sint.COD_QUALIFICA, sint.COD_RUOLO, Parametro15, out string pat, out string inail))
            {
                P.Pat = pat;
                P.Inail = inail;
                return P;
            }
            else if (dataElement != null || customAI != null)
            {
                string aiCode = customAI != null ? customAI.dati_serial : dataElement.voce_te;

                switch (aiCode)
                {
                    case "F":
                        P.Pat = "7979998";
                        P.Inail = "0541";
                        break;
                    case "A":
                    case "L":
                        P.Pat = "93149632";
                        P.Inail = "4220";
                        break;
                    case "C":
                    case "N":
                        P.Pat = "93149632";
                        P.Inail = "0530";
                        break;
                    case "D":
                    case "P":
                        P.Pat = "32160456";
                        P.Inail = "0722";
                        break;
                    case "E":
                    case "R":
                        P.Pat = "93150473";
                        P.Inail = "0723";
                        break;
                    case "B":
                    case "M":
                        P.Pat = "32519983";
                        P.Inail = "0511";
                        break;
                    case "W":
                        P.Pat = "0000000";
                        P.Inail = "0000";
                        break;
                    case "6":
                        P.Pat = "99990001";
                        P.Inail = "0000";
                        break;
                    default:
                        P.Errore = "Decodifica non trovata";
                        return P;

                }
                return P;
            }
            else
            {
                P.Errore = "Impossibile trovare Pat Inail";
                return P;
            }
        }

        private bool CheckRulePatInail(List<PatInailRule> rules, string matricola, string qual, string ruolo, string parametro15, out string pat, out string inail)
        {
            bool found = false;
            pat = "";
            inail = "";

            if (!String.IsNullOrWhiteSpace(matricola) && rules.Any(x => x.Matricola == matricola))
            {
                found = true;
                var rule = rules.First(x => x.Matricola == matricola);
                pat = rule.Pat;
                inail = rule.Inail;
            }
            else if (!String.IsNullOrWhiteSpace(parametro15) && rules.Any(x => parametro15.Contains(x.File)))
            {
                found = true;
                var rule = rules.First(x => parametro15.Contains(x.File));
                pat = rule.Pat;
                inail = rule.Inail;
            }

            return found;
        }

        

        public bool IsApprendistato(string matricola)
        {

            /* SELECT c.cod_matlibromat FROM  COMPREL c
inner join jomprel j on c.id_comprel = j.id_comprel
inner join assqual a on c.id_persona = a.id_persona and getdate() between a.dta_inizio and a.dta_fine
inner join jssqual y on a.id_assqual = y.id_assqual
where COD_TIPORLAV = 'TI' AND J.cod_causalemov IN ('BB','BR','BZ')
and y.cod_tipominimo <> '0'
            */
            string[] codiciAppr = new string[] { "BB", "BR", "BZ" };
            var dbCzn = new IncentiviEntities();
            var matricoleApprendistato = (from c in dbCzn.COMPREL
                     join j in dbCzn.JOMPREL on c.ID_COMPREL equals j.ID_COMPREL
                     join a in dbCzn.ASSQUAL.Where(x => DateTime.Now >= x.DTA_INIZIO && DateTime.Now <= x.DTA_FINE) on c.ID_PERSONA
                                     equals a.ID_PERSONA
                     join y in dbCzn.JSSQUAL on a.ID_ASSQUAL equals y.ID_ASSQUAL
                     where c.COD_TIPORLAV == "TI" && codiciAppr.Contains(j.COD_CAUSALEMOV) && y.COD_TIPOMINIMO != "0"
                     select c.COD_MATLIBROMAT).ToList();

            return matricoleApprendistato.Contains(matricola);

            var comprel = dbCzn.COMPREL.Where(x => x.COD_MATLIBROMAT == matricola && DateTime.Now >= x.DTA_INIZIO && DateTime.Now <= x.DTA_FINE)
                .FirstOrDefault();
            if (comprel == null) return false;

            var jomprel = dbCzn.JOMPREL.Where(x => x.ID_COMPREL == comprel.ID_COMPREL).FirstOrDefault();
           
            return jomprel != null && codiciAppr.Contains(jomprel.COD_CAUSALEMOV);
        }

        internal string GetCodiceTipoContratto(SINTESI1 sintesi)
        {
            if (IsApprendistato(sintesi.COD_MATLIBROMAT))
                return "A03";
            else
                return sintesi.COD_TPCNTR == "9" || sintesi.COD_TPCNTR == "K"
                ? "A01" //INDET
                : "A02"; //DET
        }
    }
    public class PAT_INAIL
    {
        public string Pat { get; set; }
        public string Inail { get; set; }
        public string Errore { get; set; }
    }

    public class PatInailRule
    {
        public string Matricola { get; set; }
        public string Categoria { get; set; }
        public string Mansione { get; set; }
        public string File { get; set; }
        public string Pat { get; set; }
        public string Inail { get; set; }
    }
    public class Dati
    {
        public string matricola_dp { get; set; }
        public string voce_te { get; set; }
        public DateTime? data_cessazione { get; set; }
        public string cod_mansione { get; set; }
        public string desc_mansione { get; set; }
        public string istituto_assicuratore_mansione { get; set; }
        public string tipo_minimo { get; set; }
        public string cod_serv_cont { get; set; }
        public string desc_serv_cont { get; set; }
        public string cod_macro_categoria { get; set; }
        public string desc_macro_categoria { get; set; }
        public string cod_categoria { get; set; }
        public string desc_categoria { get; set; }
        public string desc_liv_professionale { get; set; }
        public string desc_aggregato_sede { get; set; }
        public string desc_sede { get; set; }
        public string ccl { get; set; }
        public string societa { get; set; }
        public string cod_insediamento { get; set; }
        public string desc_insediamento { get; set; }
    }
}



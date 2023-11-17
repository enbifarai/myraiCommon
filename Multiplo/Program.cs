using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using log4net;
using Multiplo.it.rai.servizi.anagraficaws1;
using Multiplo.Models;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiCommonTasks.Helpers;
using myRaiData;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;
using OpenXmlPowerTools;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Output = Multiplo.Helpers.Output;

namespace Multiplo
{
    public enum EnumTipologiaDomanda
    {
        Si_No = 1,
        Risposta_singola_Lista_RadioButton = 2,
        Risposta_Singola_Lista_Tendina = 3,
        Risposta_Multipla_CheckBox = 4,
        ShortText = 5,
        LongText = 6,
        Precompilato = 7,
        MasterPerMatrixRating = 8,
        SlavePerMatrixRating = 9,
        SlavePerRating_6 = 10,
        Risposte_da_ordinare = 11,
        SlavePerRating_5 = 23,
        MasterPerMatrixRatingNoLabel = 24,
    }
    public class matrFrag
    {
        public string matr { get; set; }
        public string ipotesiFrag { get; set; }
        public XR_STATO_RAPPORTO SR { get; set; }
        public XR_STATO_RAPPORTO_INFO SRI { get; set; }
    }
    class Program
    {
        private static log4net.ILog _log;

        private static Attachment ScriviLogSlack(string nomeBatch = "", string testo = "", string color = "good", bool istantaneo = false, string alternativeTx = "")
        {
            string SlackMainTitle = "Batch " + nomeBatch;

            string tx = String.Format("{0} - {1}\r\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), testo);

            Attachment slackA = new Attachment() { Color = color, Title = tx, Text = alternativeTx };
            if (istantaneo)
            {
                List<Attachment> SlackAttachments = new List<Attachment>();
                SlackAttachments.Add(slackA);

                CommonTasks.PostMessageAdvanced(SlackMainTitle, SlackAttachments.ToArray());
                SlackAttachments.Clear();
            }

            return slackA;
        }

        private static void AggiornaDati()
        {
            var locationBKPath = System.Configuration.ConfigurationManager.AppSettings["PercorsoFileUpdateDati"] ?? System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory_lavoro = Path.GetDirectoryName(locationBKPath);
            string nomeFile = "file.csv";
            string percorsoCompleto = Path.Combine(directory_lavoro, nomeFile);

            IncentiviEntities db = new IncentiviEntities();
            StreamReader file = new StreamReader(percorsoCompleto, Encoding.Default);
            //string line = file.ReadLine();
            string line = null;

            while ((line = file.ReadLine()) != null)
            {
                decimal budget_FSuper = 0;
                decimal budget_periodo_FSuper = 0;
                decimal budget_F1 = 0;
                decimal budget_periodo_F1 = 0;
                decimal budget_Altro = 0;
                decimal budget_periodo_Altro = 0;

                List<string> rigaSplittata = line.Split(';').ToList();
                string nomeDirezione = rigaSplittata[0].Trim();
                budget_FSuper = decimal.Parse(rigaSplittata[1]);
                budget_periodo_FSuper = decimal.Parse(rigaSplittata[2]);
                budget_F1 = decimal.Parse(rigaSplittata[3]);
                budget_periodo_F1 = decimal.Parse(rigaSplittata[4]);
                budget_Altro = decimal.Parse(rigaSplittata[5]);
                budget_periodo_Altro = decimal.Parse(rigaSplittata[6]);

                string queryUpdate = "UPDATE [CZNDB].[dbo].[XR_PRV_CAMPAGNA_DIREZIONE]" +
                                 " set budget = #BUDGET#," +
                                 " budget_periodo = #PERIODO#" +
                                 " where id_campagna = 1396886732" +
                                 " and id_direzione in (SELECT[ID_DIREZIONE]" +
                                 " FROM [CZNDB].[dbo].[XR_PRV_DIREZIONE]" +
                                 " where nome = '#DIREZIONE#')";

                queryUpdate = queryUpdate.Replace("#BUDGET#", budget_FSuper.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#PERIODO#", budget_periodo_FSuper.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#DIREZIONE#", nomeDirezione);

                Output.WriteLine(queryUpdate);

                queryUpdate = "UPDATE [CZNDB].[dbo].[XR_PRV_CAMPAGNA_DIREZIONE]" +
                 " set budget = #BUDGET#," +
                 " budget_periodo = #PERIODO#" +
                 " where id_campagna = 997716921" +
                 " and id_direzione in (SELECT[ID_DIREZIONE]" +
                 " FROM [CZNDB].[dbo].[XR_PRV_DIREZIONE]" +
                 " where nome = '#DIREZIONE#')";

                queryUpdate = queryUpdate.Replace("#BUDGET#", budget_F1.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#PERIODO#", budget_periodo_F1.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#DIREZIONE#", nomeDirezione);

                Output.WriteLine(queryUpdate);

                queryUpdate = "UPDATE [CZNDB].[dbo].[XR_PRV_CAMPAGNA_DIREZIONE]" +
                             " set budget = #BUDGET#," +
                             " budget_periodo = #PERIODO#" +
                             " where id_campagna = 465852180" +
                             " and id_direzione in (SELECT[ID_DIREZIONE]" +
                             " FROM [CZNDB].[dbo].[XR_PRV_DIREZIONE]" +
                             " where nome = '#DIREZIONE#')";

                queryUpdate = queryUpdate.Replace("#BUDGET#", budget_Altro.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#PERIODO#", budget_periodo_Altro.ToString("0.00").Replace(",", "."));
                queryUpdate = queryUpdate.Replace("#DIREZIONE#", nomeDirezione);

                Output.WriteLine(queryUpdate);
                Output.WriteLine("//--------------------------------------------------------------");
            }

            file.Dispose();
        }


        private static void TestElenchi()
        {
            List<string> elencoLoro = new List<string>();
            List<string> elencoNostro = new List<string>();
            elencoLoro.Add("279909");
            elencoLoro.Add("690925");
            elencoLoro.Add("717598");
            elencoLoro.Add("144445");
            elencoLoro.Add("328452");
            elencoLoro.Add("335222");
            elencoLoro.Add("357540");
            elencoLoro.Add("639815");
            elencoLoro.Add("790375");
            elencoLoro.Add("144585");
            elencoLoro.Add("289975");
            elencoLoro.Add("304240");
            elencoLoro.Add("541364");
            elencoLoro.Add("774550");
            elencoLoro.Add("214315");
            elencoLoro.Add("000577");
            elencoLoro.Add("091070");
            elencoLoro.Add("132784");
            elencoLoro.Add("166582");
            elencoLoro.Add("250105");
            elencoLoro.Add("324166");
            elencoLoro.Add("505250");
            elencoLoro.Add("451076");
            elencoLoro.Add("498478");
            elencoLoro.Add("044090");
            elencoLoro.Add("156237");
            elencoLoro.Add("325900");
            elencoLoro.Add("387693");
            elencoLoro.Add("214405");
            elencoLoro.Add("231728");
            elencoLoro.Add("301508");
            elencoLoro.Add("317260");
            elencoLoro.Add("399536");
            elencoLoro.Add("454155");
            elencoLoro.Add("485004");
            elencoLoro.Add("653610");
            elencoLoro.Add("678992");
            elencoLoro.Add("679639");
            elencoLoro.Add("681600");
            elencoLoro.Add("969045");
            elencoLoro.Add("989181");
            elencoLoro.Add("007660");
            elencoLoro.Add("021935");
            elencoLoro.Add("036882");
            elencoLoro.Add("060871");
            elencoLoro.Add("180483");
            elencoLoro.Add("222403");
            elencoLoro.Add("289544");
            elencoLoro.Add("346685");
            elencoLoro.Add("417105");
            elencoLoro.Add("444298");
            elencoLoro.Add("496895");
            elencoLoro.Add("507026");
            elencoLoro.Add("559860");
            elencoLoro.Add("614570");
            elencoLoro.Add("622854");
            elencoLoro.Add("662438");
            elencoLoro.Add("686323");
            elencoLoro.Add("729071");
            elencoLoro.Add("831160");
            elencoLoro.Add("860350");
            elencoLoro.Add("890685");
            elencoLoro.Add("944644");
            elencoLoro.Add("966826");
            elencoLoro.Add("983114");
            elencoLoro.Add("342058");
            elencoLoro.Add("839058");
            elencoLoro.Add("921275");
            elencoLoro.Add("852135");
            elencoLoro.Add("270097");
            elencoLoro.Add("284096");
            elencoLoro.Add("443017");
            elencoLoro.Add("662239");
            elencoLoro.Add("010517");
            elencoLoro.Add("171930");
            elencoLoro.Add("178230");
            elencoLoro.Add("240360");
            elencoLoro.Add("242130");
            elencoLoro.Add("560700");
            elencoLoro.Add("765656");
            elencoLoro.Add("766035");
            elencoLoro.Add("128790");
            elencoLoro.Add("138177");
            elencoLoro.Add("332405");
            elencoLoro.Add("816371");
            elencoLoro.Add("817010");
            elencoLoro.Add("945094");
            elencoLoro.Add("319368");
            elencoLoro.Add("356240");
            elencoLoro.Add("628174");
            elencoLoro.Add("713530");
            elencoLoro.Add("310052");
            elencoLoro.Add("426566");
            elencoLoro.Add("031610");
            elencoLoro.Add("526521");
            elencoLoro.Add("330298");
            elencoLoro.Add("766866");
            elencoLoro.Add("908215");
            elencoLoro.Add("910165");
            elencoLoro.Add("313117");
            elencoLoro.Add("372280");
            elencoLoro.Add("512593");
            elencoLoro.Add("816380");
            elencoLoro.Add("818028");
            elencoLoro.Add("304713");
            elencoLoro.Add("480805");
            elencoLoro.Add("614261");
            elencoLoro.Add("723199");
            elencoLoro.Add("734166");
            elencoLoro.Add("004095");
            elencoLoro.Add("225550");
            elencoLoro.Add("244470");
            elencoLoro.Add("987495");
            elencoLoro.Add("257313");
            elencoLoro.Add("404197");
            elencoLoro.Add("812952");
            elencoLoro.Add("244732");
            elencoLoro.Add("305932");
            elencoLoro.Add("341086");
            elencoLoro.Add("959408");
            elencoLoro.Add("053640");
            elencoLoro.Add("514711");
            elencoLoro.Add("559432");
            elencoLoro.Add("584842");
            elencoLoro.Add("717510");
            elencoLoro.Add("054767");
            elencoLoro.Add("083268");
            elencoLoro.Add("155785");
            elencoLoro.Add("295407");
            elencoLoro.Add("485454");
            elencoLoro.Add("714250");
            elencoLoro.Add("727450");
            elencoLoro.Add("964724");
            elencoLoro.Add("003907");
            elencoLoro.Add("036878");
            elencoLoro.Add("879815");
            elencoLoro.Add("937011");
            elencoLoro.Add("542183");
            elencoLoro.Add("807099");
            elencoLoro.Add("976193");
            elencoLoro.Add("562130");
            elencoLoro.Add("649891");
            elencoLoro.Add("154017");
            elencoLoro.Add("363430");
            elencoLoro.Add("723210");
            elencoLoro.Add("800422");
            elencoLoro.Add("821739");
            elencoLoro.Add("843990");
            elencoLoro.Add("911825");
            elencoLoro.Add("855270");
            elencoLoro.Add("345870");
            elencoLoro.Add("927302");
            elencoLoro.Add("286272");
            elencoLoro.Add("946110");
            elencoLoro.Add("442916");
            elencoLoro.Add("553268");
            elencoLoro.Add("983840");
            elencoLoro.Add("620876");
            elencoLoro.Add("606715");
            elencoLoro.Add("788350");
            elencoLoro.Add("120938");
            elencoLoro.Add("318257");
            elencoLoro.Add("604710");
            elencoLoro.Add("465416");
            elencoLoro.Add("515434");
            elencoLoro.Add("540110");
            elencoLoro.Add("545085");
            elencoLoro.Add("035343");
            elencoLoro.Add("818208");
            elencoLoro.Add("343617");
            elencoLoro.Add("615660");
            elencoLoro.Add("967945");
            elencoLoro.Add("685050");
            elencoLoro.Add("733910");
            elencoLoro.Add("863829");
            elencoLoro.Add("030668");
            elencoLoro.Add("562456");
            elencoLoro.Add("682719");
            elencoLoro.Add("966237");
            elencoLoro.Add("126949");
            elencoLoro.Add("612345");
            elencoLoro.Add("175790");
            elencoLoro.Add("234899");
            elencoLoro.Add("542520");
            elencoLoro.Add("530941");
            elencoLoro.Add("339727");
            elencoLoro.Add("688040");
            elencoLoro.Add("525151");
            elencoLoro.Add("043490");
            elencoLoro.Add("029521");
            elencoLoro.Add("281345");
            elencoLoro.Add("475320");
            elencoLoro.Add("901105");
            elencoLoro.Add("697405");
            elencoLoro.Add("678650");
            elencoLoro.Add("252566");
            elencoLoro.Add("106090");
            elencoLoro.Add("703223");
            elencoLoro.Add("094610");
            elencoLoro.Add("871883");
            elencoLoro.Add("303360");
            elencoLoro.Add("326133");
            elencoLoro.Add("805318");
            elencoLoro.Add("191301");
            elencoLoro.Add("480670");
            elencoLoro.Add("201561");
            elencoLoro.Add("328139");
            elencoLoro.Add("509149");
            elencoLoro.Add("580090");
            elencoLoro.Add("219794");
            elencoLoro.Add("023829");
            elencoLoro.Add("825208");
            elencoLoro.Add("938845");
            elencoLoro.Add("548151");
            elencoLoro.Add("597497");
            elencoLoro.Add("318800");
            elencoLoro.Add("234690");
            elencoLoro.Add("765930");
            elencoLoro.Add("985228");
            elencoLoro.Add("209286");
            elencoLoro.Add("078286");
            elencoLoro.Add("135785");
            elencoLoro.Add("503740");
            elencoLoro.Add("802948");
            elencoLoro.Add("973052");
            elencoLoro.Add("981187");
            elencoLoro.Add("987250");
            elencoLoro.Add("104178");
            elencoLoro.Add("228250");
            elencoLoro.Add("315035");
            elencoLoro.Add("583997");
            elencoLoro.Add("595393");
            elencoLoro.Add("679565");
            elencoLoro.Add("722310");
            elencoLoro.Add("741932");
            elencoLoro.Add("791420");
            elencoLoro.Add("807096");
            elencoLoro.Add("828020");
            elencoLoro.Add("893044");
            elencoLoro.Add("903855");
            elencoLoro.Add("921315");
            elencoLoro.Add("968285");
            elencoLoro.Add("229344");
            elencoLoro.Add("013925");
            elencoLoro.Add("017206");
            elencoLoro.Add("051265");
            elencoLoro.Add("162785");
            elencoLoro.Add("402065");
            elencoLoro.Add("405990");
            elencoLoro.Add("445082");
            elencoLoro.Add("863240");
            elencoLoro.Add("947304");
            elencoLoro.Add("009990");
            elencoLoro.Add("329140");
            elencoLoro.Add("365772");
            elencoLoro.Add("369074");
            elencoLoro.Add("007825");
            elencoLoro.Add("157077");
            elencoLoro.Add("207985");
            elencoLoro.Add("440328");
            elencoLoro.Add("450960");
            elencoLoro.Add("593263");
            elencoLoro.Add("627470");
            elencoLoro.Add("746807");
            elencoLoro.Add("791165");
            elencoLoro.Add("863945");
            elencoLoro.Add("889080");
            elencoLoro.Add("892033");
            elencoLoro.Add("947911");
            elencoLoro.Add("204630");
            elencoLoro.Add("251503");
            elencoLoro.Add("650410");
            elencoLoro.Add("824686");
            elencoLoro.Add("854335");
            elencoLoro.Add("859312");
            elencoLoro.Add("304540");
            elencoLoro.Add("840160");
            elencoLoro.Add("209601");
            elencoLoro.Add("333323");
            elencoLoro.Add("445067");
            elencoLoro.Add("506510");
            elencoLoro.Add("512564");
            elencoLoro.Add("797635");
            elencoLoro.Add("965574");
            elencoLoro.Add("195605");
            elencoLoro.Add("277494");
            elencoLoro.Add("857451");
            elencoLoro.Add("837284");
            elencoLoro.Add("841645");
            elencoLoro.Add("957424");
            elencoLoro.Add("198030");
            elencoLoro.Add("243410");
            elencoLoro.Add("471490");
            elencoLoro.Add("734687");
            elencoLoro.Add("521180");
            elencoLoro.Add("175632");
            elencoLoro.Add("320595");
            elencoLoro.Add("765040");
            elencoLoro.Add("842030");
            elencoLoro.Add("946257");
            elencoLoro.Add("159861");
            elencoLoro.Add("267548");
            elencoLoro.Add("301911");
            elencoLoro.Add("345244");
            elencoLoro.Add("359842");
            elencoLoro.Add("505065");
            elencoLoro.Add("758545");
            elencoLoro.Add("787295");
            elencoLoro.Add("498081");
            elencoLoro.Add("176511");
            elencoLoro.Add("351130");
            elencoLoro.Add("355198");
            elencoLoro.Add("412066");
            elencoLoro.Add("611399");
            elencoLoro.Add("675025");
            elencoLoro.Add("770595");
            elencoLoro.Add("186133");
            elencoLoro.Add("150780");
            elencoLoro.Add("494122");
            elencoLoro.Add("496199");
            elencoLoro.Add("604301");
            elencoLoro.Add("609120");
            elencoLoro.Add("656073");
            elencoLoro.Add("879801");
            elencoLoro.Add("971665");
            elencoLoro.Add("047596");
            elencoLoro.Add("233381");
            elencoLoro.Add("143300");
            elencoLoro.Add("575350");
            elencoLoro.Add("815230");
            elencoLoro.Add("022670");
            elencoLoro.Add("192181");
            elencoLoro.Add("887917");
            elencoLoro.Add("389353");
            elencoLoro.Add("602951");
            elencoLoro.Add("578345");
            elencoLoro.Add("718894");
            elencoLoro.Add("153822");
            elencoLoro.Add("047878");
            elencoLoro.Add("314706");
            elencoLoro.Add("897189");
            elencoLoro.Add("031394");
            elencoLoro.Add("104137");
            elencoLoro.Add("298895");
            elencoLoro.Add("450841");
            elencoLoro.Add("727031");
            elencoLoro.Add("197867");
            elencoLoro.Add("440140");
            elencoLoro.Add("070930");
            elencoLoro.Add("023537");
            elencoLoro.Add("389568");
            elencoLoro.Add("588236");
            elencoLoro.Add("759027");
            elencoLoro.Add("964840");
            elencoLoro.Add("970220");
            elencoLoro.Add("056297");
            elencoLoro.Add("033774");
            elencoLoro.Add("198032");
            elencoLoro.Add("524677");
            elencoLoro.Add("273889");
            elencoLoro.Add("787756");
            elencoLoro.Add("105252");
            elencoLoro.Add("111142");
            elencoLoro.Add("206191");
            elencoLoro.Add("316522");
            elencoLoro.Add("451598");
            elencoLoro.Add("543288");
            elencoLoro.Add("611035");
            elencoLoro.Add("718129");
            elencoLoro.Add("801811");
            elencoLoro.Add("878588");
            elencoLoro.Add("560423");
            elencoLoro.Add("271405");
            elencoLoro.Add("832760");
            elencoLoro.Add("564780");
            elencoLoro.Add("336057");
            elencoLoro.Add("417760");
            elencoLoro.Add("187090");
            elencoLoro.Add("194718");
            elencoLoro.Add("613840");
            elencoLoro.Add("330450");
            elencoLoro.Add("917361");
            elencoLoro.Add("264656");
            elencoLoro.Add("167235");
            elencoLoro.Add("975153");
            elencoLoro.Add("040963");
            elencoLoro.Add("932133");
            elencoLoro.Add("003200");
            elencoLoro.Add("115414");
            elencoLoro.Add("902690");
            elencoLoro.Add("817520");
            elencoLoro.Add("560502");
            elencoLoro.Add("259229");
            elencoLoro.Add("327998");
            elencoLoro.Add("358938");
            elencoLoro.Add("445655");
            elencoLoro.Add("484621");
            elencoLoro.Add("597049");
            elencoLoro.Add("204548");


            elencoNostro.Add("679565");
            elencoNostro.Add("863829");
            elencoNostro.Add("734687");
            elencoNostro.Add("879815");
            elencoNostro.Add("144445");
            elencoNostro.Add("932133");
            elencoNostro.Add("389353");
            elencoNostro.Add("104137");
            elencoNostro.Add("505250");
            elencoNostro.Add("225550");
            elencoNostro.Add("548151");
            elencoNostro.Add("627470");
            elencoNostro.Add("611035");
            elencoNostro.Add("825208");
            elencoNostro.Add("604301");
            elencoNostro.Add("818028");
            elencoNostro.Add("013925");
            elencoNostro.Add("800422");
            elencoNostro.Add("138177");
            elencoNostro.Add("494122");
            elencoNostro.Add("588236");
            elencoNostro.Add("815230");
            elencoNostro.Add("358938");
            elencoNostro.Add("987250");
            elencoNostro.Add("339142");
            elencoNostro.Add("569463");
            elencoNostro.Add("496199");
            elencoNostro.Add("526521");
            elencoNostro.Add("345870");
            elencoNostro.Add("335222");
            elencoNostro.Add("319368");
            elencoNostro.Add("234690");
            elencoNostro.Add("521180");
            elencoNostro.Add("041370");
            elencoNostro.Add("231728");
            elencoNostro.Add("303360");
            elencoNostro.Add("475320");
            elencoNostro.Add("890685");
            elencoNostro.Add("614261");
            elencoNostro.Add("512564");
            elencoNostro.Add("622854");
            elencoNostro.Add("945094");
            elencoNostro.Add("818208");
            elencoNostro.Add("807096");
            elencoNostro.Add("083268");
            elencoNostro.Add("908215");
            elencoNostro.Add("399536");
            elencoNostro.Add("053640");
            elencoNostro.Add("816371");
            elencoNostro.Add("298895");
            elencoNostro.Add("022670");
            elencoNostro.Add("559860");
            elencoNostro.Add("214405");
            elencoNostro.Add("678650");
            elencoNostro.Add("620876");
            elencoNostro.Add("355198");
            elencoNostro.Add("440328");
            elencoNostro.Add("964840");
            elencoNostro.Add("233381");
            elencoNostro.Add("503740");
            elencoNostro.Add("343617");
            elencoNostro.Add("801811");
            elencoNostro.Add("717598");
            elencoNostro.Add("878588");
            elencoNostro.Add("917361");
            elencoNostro.Add("595393");
            elencoNostro.Add("405990");
            elencoNostro.Add("525151");
            elencoNostro.Add("115414");
            elencoNostro.Add("837284");
            elencoNostro.Add("685050");
            elencoNostro.Add("887917");
            elencoNostro.Add("860350");
            elencoNostro.Add("983840");
            elencoNostro.Add("690925");
            elencoNostro.Add("313117");
            elencoNostro.Add("333323");
            elencoNostro.Add("325900");
            elencoNostro.Add("946257");
            elencoNostro.Add("222403");
            elencoNostro.Add("091070");
            elencoNostro.Add("219794");
            elencoNostro.Add("807099");
            elencoNostro.Add("180483");
            elencoNostro.Add("003907");
            elencoNostro.Add("947304");
            elencoNostro.Add("966826");
            elencoNostro.Add("560423");
            elencoNostro.Add("402065");
            elencoNostro.Add("336057");
            elencoNostro.Add("186133");
            elencoNostro.Add("240360");
            elencoNostro.Add("578345");
            elencoNostro.Add("597497");
            elencoNostro.Add("966237");
            elencoNostro.Add("498478");
            elencoNostro.Add("957424");
            elencoNostro.Add("155785");
            elencoNostro.Add("480805");
            elencoNostro.Add("229344");
            elencoNostro.Add("831160");
            elencoNostro.Add("639815");
            elencoNostro.Add("259229");
            elencoNostro.Add("553268");
            elencoNostro.Add("228250");
            elencoNostro.Add("023537");
            elencoNostro.Add("007825");
            elencoNostro.Add("718129");
            elencoNostro.Add("369074");
            elencoNostro.Add("791165");
            elencoNostro.Add("159861");
            elencoNostro.Add("852135");
            elencoNostro.Add("682719");
            elencoNostro.Add("111142");
            elencoNostro.Add("284096");
            elencoNostro.Add("176511");
            elencoNostro.Add("035343");
            elencoNostro.Add("442916");
            elencoNostro.Add("530941");
            elencoNostro.Add("903855");
            elencoNostro.Add("679639");
            elencoNostro.Add("985228");
            elencoNostro.Add("175632");
            elencoNostro.Add("162785");
            elencoNostro.Add("976193");
            elencoNostro.Add("150780");
            elencoNostro.Add("330298");
            elencoNostro.Add("612345");
            elencoNostro.Add("327998");
            elencoNostro.Add("723199");
            elencoNostro.Add("009990");
            elencoNostro.Add("054767");
            elencoNostro.Add("812952");
            elencoNostro.Add("315035");
            elencoNostro.Add("250105");
            elencoNostro.Add("167235");
            elencoNostro.Add("106090");
            elencoNostro.Add("209286");
            elencoNostro.Add("686323");
            elencoNostro.Add("766035");
            elencoNostro.Add("788350");
            elencoNostro.Add("560700");
            elencoNostro.Add("816380");
            elencoNostro.Add("277494");
            elencoNostro.Add("191301");
            elencoNostro.Add("144585");
            elencoNostro.Add("559432");
            elencoNostro.Add("817520");
            elencoNostro.Add("135785");
            elencoNostro.Add("507026");
            elencoNostro.Add("450960");
            elencoNostro.Add("498081");
            elencoNostro.Add("047596");
            elencoNostro.Add("044090");
            elencoNostro.Add("036878");
            elencoNostro.Add("004095");
            elencoNostro.Add("301911");
            elencoNostro.Add("650410");
            elencoNostro.Add("975153");
            elencoNostro.Add("320595");
            elencoNostro.Add("545085");
            elencoNostro.Add("120938");
            elencoNostro.Add("901105");
            elencoNostro.Add("031394");
            elencoNostro.Add("717510");
            elencoNostro.Add("944644");
            elencoNostro.Add("273889");
            elencoNostro.Add("765930");
            elencoNostro.Add("279909");
            elencoNostro.Add("911825");
            elencoNostro.Add("893044");
            elencoNostro.Add("697405");
            elencoNostro.Add("703223");
            elencoNostro.Add("030668");
            elencoNostro.Add("198030");
            elencoNostro.Add("839058");
            elencoNostro.Add("192181");
            elencoNostro.Add("938845");
            elencoNostro.Add("242130");
            elencoNostro.Add("713530");
            elencoNostro.Add("270097");
            elencoNostro.Add("937011");
            elencoNostro.Add("746807");
            elencoNostro.Add("359842");
            elencoNostro.Add("157077");
            elencoNostro.Add("166582");
            elencoNostro.Add("662438");
            elencoNostro.Add("251503");
            elencoNostro.Add("365772");
            elencoNostro.Add("443017");
            elencoNostro.Add("564780");
            elencoNostro.Add("094610");
            elencoNostro.Add("821739");
            elencoNostro.Add("946110");
            elencoNostro.Add("187090");
            elencoNostro.Add("681600");
            elencoNostro.Add("832760");
            elencoNostro.Add("465416");
            elencoNostro.Add("613840");
            elencoNostro.Add("244470");
            elencoNostro.Add("959408");
            elencoNostro.Add("662239");
            elencoNostro.Add("733910");
            elencoNostro.Add("324166");
            elencoNostro.Add("722310");
            elencoNostro.Add("675025");
            elencoNostro.Add("301508");
            elencoNostro.Add("351130");
            elencoNostro.Add("252566");
            elencoNostro.Add("910165");
            elencoNostro.Add("363430");
            elencoNostro.Add("105252");
            elencoNostro.Add("727031");
            elencoNostro.Add("003200");
            elencoNostro.Add("243410");
            elencoNostro.Add("010517");
            elencoNostro.Add("328452");
            elencoNostro.Add("688040");
            elencoNostro.Add("264656");
            elencoNostro.Add("485454");
            elencoNostro.Add("201561");
            elencoNostro.Add("316522");
            elencoNostro.Add("304540");
            elencoNostro.Add("759027");
            elencoNostro.Add("451598");
            elencoNostro.Add("143300");
            elencoNostro.Add("207985");
            elencoNostro.Add("471490");
            elencoNostro.Add("871883");
            elencoNostro.Add("314706");
            elencoNostro.Add("542183");
            elencoNostro.Add("197867");
            elencoNostro.Add("879801");
            elencoNostro.Add("512593");
            elencoNostro.Add("485004");
            elencoNostro.Add("857451");
            elencoNostro.Add("417760");
            elencoNostro.Add("628174");
            elencoNostro.Add("484621");
            elencoNostro.Add("892033");
            elencoNostro.Add("509149");
            elencoNostro.Add("964724");
            elencoNostro.Add("802948");
            elencoNostro.Add("611399");
            elencoNostro.Add("727450");
            elencoNostro.Add("426566");
            elencoNostro.Add("791420");
            elencoNostro.Add("295407");
            elencoNostro.Add("947911");
            elencoNostro.Add("897189");
            elencoNostro.Add("194718");
            elencoNostro.Add("412066");
            elencoNostro.Add("602951");
            elencoNostro.Add("541364");
            elencoNostro.Add("656073");
            elencoNostro.Add("653610");
            elencoNostro.Add("480670");
            elencoNostro.Add("289975");
            elencoNostro.Add("304240");
            elencoNostro.Add("330450");
            elencoNostro.Add("971665");
            elencoNostro.Add("855270");
            elencoNostro.Add("515818");
            elencoNostro.Add("741932");
            elencoNostro.Add("341086");
            elencoNostro.Add("506510");
            elencoNostro.Add("286272");
            elencoNostro.Add("454155");
            elencoNostro.Add("214315");
            elencoNostro.Add("524677");
            elencoNostro.Add("902690");
            elencoNostro.Add("017206");
            elencoNostro.Add("342058");
            elencoNostro.Add("036882");
            elencoNostro.Add("543288");
            elencoNostro.Add("840160");
            elencoNostro.Add("859312");
            elencoNostro.Add("714250");
            elencoNostro.Add("649891");
            elencoNostro.Add("267548");
            elencoNostro.Add("070930");
            elencoNostro.Add("204630");
            elencoNostro.Add("593263");
            elencoNostro.Add("562130");
            elencoNostro.Add("614570");
            elencoNostro.Add("332405");
            elencoNostro.Add("209601");
            elencoNostro.Add("787295");
            elencoNostro.Add("305932");
            elencoNostro.Add("154017");
            elencoNostro.Add("329140");
            elencoNostro.Add("766866");
            elencoNostro.Add("989181");
            elencoNostro.Add("817010");
            elencoNostro.Add("774550");
            elencoNostro.Add("153822");
            elencoNostro.Add("562456");
            elencoNostro.Add("047878");
            elencoNostro.Add("060871");
            elencoNostro.Add("040963");
            elencoNostro.Add("317260");
            elencoNostro.Add("051265");
            elencoNostro.Add("967945");
            elencoNostro.Add("580090");
            elencoNostro.Add("927302");
            elencoNostro.Add("021935");
            elencoNostro.Add("257313");
            elencoNostro.Add("357540");
            elencoNostro.Add("604710");
            elencoNostro.Add("000577");
            elencoNostro.Add("388429");
            elencoNostro.Add("968285");
            elencoNostro.Add("372280");
            elencoNostro.Add("969045");
            elencoNostro.Add("787756");
            elencoNostro.Add("450841");
            elencoNostro.Add("289544");
            elencoNostro.Add("678992");
            elencoNostro.Add("496895");
            elencoNostro.Add("965574");
            elencoNostro.Add("583997");
            elencoNostro.Add("178230");
            elencoNostro.Add("445067");
            elencoNostro.Add("346685");
            elencoNostro.Add("033774");
            elencoNostro.Add("609120");
            elencoNostro.Add("304713");
            elencoNostro.Add("445655");
            elencoNostro.Add("387693");
            elencoNostro.Add("584842");
            elencoNostro.Add("204548");
            elencoNostro.Add("234899");
            elencoNostro.Add("514711");
            elencoNostro.Add("126949");
            elencoNostro.Add("765040");
            elencoNostro.Add("056297");
            elencoNostro.Add("505065");
            elencoNostro.Add("765656");
            elencoNostro.Add("828020");
            elencoNostro.Add("843990");
            elencoNostro.Add("326133");
            elencoNostro.Add("734166");
            elencoNostro.Add("854335");
            elencoNostro.Add("281345");
            elencoNostro.Add("339727");
            elencoNostro.Add("440140");
            elencoNostro.Add("171930");
            elencoNostro.Add("863945");
            elencoNostro.Add("356240");
            elencoNostro.Add("271405");
            elencoNostro.Add("244732");
            elencoNostro.Add("031610");
            elencoNostro.Add("987495");
            elencoNostro.Add("310052");
            elencoNostro.Add("007660");
            elencoNostro.Add("029521");
            elencoNostro.Add("156237");
            elencoNostro.Add("797635");
            elencoNostro.Add("889080");
            elencoNostro.Add("723210");
            elencoNostro.Add("078286");
            elencoNostro.Add("345244");
            elencoNostro.Add("615660");
            elencoNostro.Add("318800");
            elencoNostro.Add("805318");
            elencoNostro.Add("758545");
            elencoNostro.Add("790375");
            elencoNostro.Add("404197");
            elencoNostro.Add("195605");
            elencoNostro.Add("141268");
            elencoNostro.Add("970220");
            elencoNostro.Add("560502");
            elencoNostro.Add("198032");
            elencoNostro.Add("318257");
            elencoNostro.Add("983114");
            elencoNostro.Add("729071");
            elencoNostro.Add("389568");
            elencoNostro.Add("863240");
            elencoNostro.Add("104178");
            elencoNostro.Add("542520");
            elencoNostro.Add("718894");
            elencoNostro.Add("973052");
            elencoNostro.Add("770595");
            elencoNostro.Add("921315");
            elencoNostro.Add("606715");
            elencoNostro.Add("921275");
            elencoNostro.Add("417105");
            elencoNostro.Add("043490");
            elencoNostro.Add("445082");
            elencoNostro.Add("328139");
            elencoNostro.Add("451076");
            elencoNostro.Add("175790");
            elencoNostro.Add("842030");
            elencoNostro.Add("206191");
            elencoNostro.Add("444298");
            elencoNostro.Add("132784");
            elencoNostro.Add("575350");
            elencoNostro.Add("824686");
            elencoNostro.Add("023829");
            elencoNostro.Add("515434");
            elencoNostro.Add("981187");
            elencoNostro.Add("841645");
            elencoNostro.Add("597049");
            elencoNostro.Add("128790");
            elencoNostro.Add("540110");

            elencoLoro = elencoLoro.OrderBy(w => w).ToList();
            elencoNostro = elencoNostro.OrderBy(w => w).ToList();

            List<string> differenza = new List<string>();

            differenza = elencoLoro.Except(elencoNostro).ToList();
        }



        private static void CaricaTemplateIn_XR_TEMPLATES()
        {
            string pathOrigine = @"C:\RAI\Multiplo\Importa_XR_TEMPLATES\";
            string nomeFile = "autorizzazione Tesi argomento RAI.docx";

            string pathCompleto = Path.Combine(pathOrigine, nomeFile);

            using (IncentiviEntities db = new IncentiviEntities())
            {
                byte[] mioFile = File.ReadAllBytes(pathCompleto);
                string est = Path.GetExtension(nomeFile);
                string tipoFile = myRaiHelper.MimeTypeMap.GetMimeType(est);

                XR_TEMPLATES template = new XR_TEMPLATES()
                {
                    AreaUtilizzo = "DEMA",
                    ContentByte = mioFile,
                    Length = mioFile.Length,
                    MimeType = tipoFile,
                    NomeFile = nomeFile
                };

                db.XR_TEMPLATES.Add(template);
                db.SaveChanges();
            }

            return;
        }

        private static void CaricaFirme()
        {
            string pathOrigine = @"C:\RAI\Multiplo\ImportaFirme\";

            string[] subdirectoryEntries = Directory.GetDirectories(pathOrigine);
            foreach (string dir in subdirectoryEntries)
            {
                string[] fileEntries = Directory.GetFiles(dir);
                foreach (string fileName in fileEntries)
                {
                    string est = Path.GetExtension(fileName);
                    if (est.ToUpper() != ".PNG")
                    {
                        continue;
                    }

                    string fullPath = Path.Combine(dir, fileName);
                    byte[] mioFile = File.ReadAllBytes(fullPath);
                    string tipoFile = myRaiHelper.MimeTypeMap.GetMimeType(est);
                    string tipologia = Path.GetFileName(fileName).Split('_')[0];
                    string matricola = Path.GetFileNameWithoutExtension(fileName).Split('_')[1];

                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        var sintesi = db.SINTESI1.Where(w => w.COD_MATLIBROMAT == matricola).FirstOrDefault();

                        XR_HRIS_FIRME nuovaFirma = new XR_HRIS_FIRME()
                        {
                            ContentByte = mioFile,
                            Matricola = matricola,
                            MimeType = tipoFile,
                            Nominativo = sintesi.Nominativo(),
                            Tipologia = tipologia
                        };

                        db.XR_HRIS_FIRME.Add(nuovaFirma);
                        db.SaveChanges();
                    }
                }
            }
        }


        static void Main(string[] args)
        {  
            // LASCIARE ATTIVE SENNO VA IN ERRORE IL LOG
            log4net.GlobalContext.Properties["LogName"] = String.Format("{0:yyyyMMdd}.log", DateTime.Today);
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            ////////////////////////////////////
            ///
          
            if (args != null && args.Any())
            {
                if (args[0].ToUpper() == "ELABORA-PONTICELLI")
                {
                    Ponticelli.ElaboraPonticelliCongedi();
                    return;
                }
                if (args[0].ToUpper() == "API-SW")
                {
                    SendAPI();
                    return;
                }
                if (args[0].ToUpper() == "SIMULA-EXPORT-CONGEDI")/////// c:\windows\smdms\batch\verificaDatiCongedi
                {
                    string days=ConfigurationManager.AppSettings["SimulaCongediGiorniEsecuzione"].ToString();
                    int[] daysInt = days.Split(',').Select(int.Parse).ToArray();
                    if (!daysInt.Contains(DateTime.Today.Day))
                    {
                        CommonTasks.Log($"{DateTime.Today.ToString("dd/MM/yyyy")} non è un giorno richiesto ({days})");
                        return;
                    }
                    CommonTasks.Log($"{DateTime.Today.ToString("dd/MM/yyyy")} esecuzione in corso ({days})");
                    AggiungiNominativiCF();
                    ExportCongediDB2.Start(true);
                    return;
                }
                if (args[0].ToUpper() == "EXPORT_CONGEDI")
                {
                    ExportCongediDB2.Start(false);
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
            return;

            //file per sansonetti
            //ExportCongediDB2.Start();
            //return;


            Ponticelli.ElaboraPonticelliCongedi();
            return;
            //var dbtal = new TalentiaEntities();
            //var row = dbtal.XR_MOD_DIPENDENTI.Where(x => x.XR_MOD_DIPENDENTI1 == 2045221356).FirstOrDefault();
            //System.IO.File.WriteAllBytes("test.pdf", row.PDF_MODULO);

            // ProlungaPeriodiFragili();
            // return;




            //AssegnaAree();
            //return;

            StressTestDb2();
            return;






            // ModificaGiorni();
            //    return;

            Allunga30Giugno();
            return;

            CaricaFirme();
            return;

            //CaricaTemplateIn_XR_TEMPLATES();
            //return;

            //SendAPI();
            //return;

            //byte[] imgFirma = null;
            //int idTemplate = 781;
            //idTemplate = 2233;

            //using (IncentiviEntities db = new IncentiviEntities())
            //{
            //    var record = db.XR_INC_TEMPLATE.Where(w => w.ID_TEMPLATE == idTemplate).FirstOrDefault();
            //    if (record != null && record.TEMPLATE != null)
            //    {
            //        imgFirma = record.TEMPLATE;
            //    }
            //    else
            //    {
            //        throw new Exception("Firma non trovata");
            //    }

            //    if (idTemplate == 781)
            //    {
            //        File.WriteAllBytes(@"C:\RAI\Multiplo\EsportaFirme\123445.png", imgFirma);
            //    }
            //    else
            //    {
            //        File.WriteAllBytes(@"C:\RAI\Multiplo\EsportaFirme\951855.png", imgFirma);
            //    }
            //}

            //return;




            //MS RIPRISTINA

            //using (WordprocessingDocument docx = WordprocessingDocument.Open(@"C:\\RAI\\PDF QIO.docx", true))
            //{
            //    HtmlConverterSettings settings = new HtmlConverterSettings() { PageTitle = "My Page Title" };
            //    var html =  HtmlConverter.ConvertToHtml(wDoc: docx, htmlConverterSettings: settings);
            //    File.WriteAllText(@"C:\\RAI\\PDF QIO.html", html.ToStringNewLineOnAttributes());

            //    //PdfDocument pdf = PdfGenerator.GeneratePdf(html.ToStringNewLineOnAttributes(), PageSize.A4, 47);
            //    //pdf.Save(@"C:\\RAI\\PDF QIO.pdf");

            //    html.ToStringNewLineOnAttributes().Replace("[NOMINATIVO]", "Bifano Vincenzo");

            //    byte[] file = null;
            //    StringReader sr = new StringReader(html.ToStringNewLineOnAttributes());
            //    Document pdfDoc = new Document(iTextSharp.text.PageSize.A4, 50, 50, 40, 40);
            //    using (MemoryStream s = new MemoryStream())
            //    {
            //        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, s);
            //        pdfDoc.Open();
            //        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
            //        pdfDoc.Close();
            //        file = s.ToArray();
            //        File.WriteAllBytes(@"C:\\RAI\\PDF QIO.pdf", file);
            //    }


            //}

            //CaricaAttivitaCeitonPerApprovatori.PopolaTabellaApprovatoriProduzioni(null);
            return;
            //PoliticheRetributive.VerificaAssociativaTemplate();
            //return;

            //TestElenchi();
            //return;



            //GetInfoAssunzioni();
            //AllineaDaRestVersoDB();
            //RecuperaE036();
            //FindICT();

            //FindCF();

            SendAPI();
            return;



            //VerificaGiorniApprovati();
            //return;
            // AggiornaDati();


            // ReportSW();

            //ReportSW_ModDipendenti();
            //return;

            //ReportSWlast();
            //return;


            //SendAPI();
            //return;


            //APIsw.ModificaComunicazione(TR.access_token);
            // return;

            //AggiornaDirezioniPoliticheRetributive();
            //Console.ReadLine();
            //IncentiviEntities db = new IncentiviEntities();
            //XR_INC_OPERSTATI toAdd = new XR_INC_OPERSTATI();
            //toAdd.ID_OPER = db.XR_INC_OPERSTATI.GeneraPrimaryKey();
            //toAdd.ID_DIPENDENTE = 2027741264;
            //toAdd.ID_STATO = 3;
            //toAdd.DATA = DateTime.Now;
            //toAdd.ID_PERSONA = 810391366;
            //toAdd.COD_USER = "RAI\\P103650";
            //toAdd.COD_TERMID = "::1";
            //toAdd.TMS_TIMESTAMP = DateTime.Now;
            //db.XR_INC_OPERSTATI.Add(toAdd);
            //db.SaveChanges();

            //IncentiviEntities db = new IncentiviEntities();
            //XR_INC_OPERSTATI toAdd = new XR_INC_OPERSTATI();
            //toAdd.ID_OPER = db.XR_INC_OPERSTATI.GeneraPrimaryKey();
            //toAdd.ID_DIPENDENTE = 592899716;
            //toAdd.ID_STATO = 3;
            //toAdd.DATA = DateTime.Now;
            //toAdd.ID_PERSONA = 810391366;
            //toAdd.COD_USER = "RAI\\P103650";
            //toAdd.COD_TERMID = "::1";
            //toAdd.TMS_TIMESTAMP = DateTime.Now;
            //db.XR_INC_OPERSTATI.Add(toAdd);
            //db.SaveChanges();

            //log4net.GlobalContext.Properties["LogName"] = String.Format("{0:yyyyMMdd}.log", DateTime.Today);
            //_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //PopolaTabellaApprovatoriProduzioni( args );
            //CercaAttivitàPerMatricole( );
            //EsportaConteggioFerie( );

            //CalcolaFerie( );

            //CopiaEccezioniHRDWToDIGIGAPPDB( );
            //if ( args.Length == 1 && args[0].ToUpper( ).Trim( ) == "CACHE" )
            //{
            //    string esito = HRGAcache.Refresh( );
            //}

            //PopolaTabellaAttitivaCeiton( );
            //InserisciInfoDipendente( );
            //PopolaTabellaApprovatoriProduzioni( );
            //UpdateInfoDipendente( );
            //CercaAttivitàPerMatricole( );
            //CaricaTipologiaCarburante( );
            //ApplicaDomicilio();
            //TestLoadEdmx();
            //RendiFormAnonimi();

            // importazione di documenti da inserire in dematerializzazione
            //Dematerializzazione.StartImportazione();


            //string imgPath = @"C:\LOG_RAI\Firme\Firma Ventura.png";
            //byte[] firma = System.IO.File.ReadAllBytes(imgPath);

            //string localPath = System.Configuration.ConfigurationManager.AppSettings["TempOutput"] ?? System.Reflection.Assembly.GetEntryAssembly().Location;
            //string directoryPath = Path.GetDirectoryName(localPath);
            //string lPath = Path.Combine(directoryPath);

            //string nomeFile = "firmaborghese.png";

            //string pathCompleto = Path.Combine(lPath, nomeFile);

            //byte[] file = File.ReadAllBytes(pathCompleto);

            //if (firma != null && firma.Length > 0)
            //{
            //    IncentiviEntities db = new IncentiviEntities();

            //    XR_INC_TEMPLATE template = new XR_INC_TEMPLATE()
            //    {
            //        NME_TEMPLATE = "FIRMAVENTURA",
            //        DES_TEMPLATE = "Firma in png di Ventura",
            //        COD_TIPO = "FIRMA",
            //        TEMPLATE = firma,
            //        COD_USER = "ADMIN",
            //        IND_BODY = false,
            //        IND_FOOTER = false,
            //        IND_HEADER = false,
            //        IND_SIGN = false,
            //        TMS_TIMESTAMP = DateTime.Today,
            //        VALID_DTA_INI = DateTime.Today,
            //        COD_TERMID = "BATCHSESSION"
            //    };

            //    db.XR_INC_TEMPLATE.Add(template);
            //    db.SaveChanges();

            //    //var item = db.XR_INC_TEMPLATE.Where(w => w.ID_TEMPLATE == 125).FirstOrDefault();

            //    //if (item != null)
            //    //{
            //    //    item.TEMPLATE = firma;
            //    //    db.SaveChanges();
            //    //}
            //}

            //TestHRISFileManager();
            //AggiornaFileEsodi();
        }

        private static void AggiungiNominativiCF()
        {
            var cred = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1]);
            myRaiCommonTasks.Logger.Log($"");
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;

            var directoryPath = Path.GetDirectoryName(location);
            var excelPath = Path.Combine(directoryPath, "elencoCF_favale.xlsx");
            CommonTasks.Log($"Aperto file {excelPath}");

            var wb = new XLWorkbook(excelPath);
            var ws = wb.Worksheets.First();
            int row = 2;
            List<dynamic> LD = new List<dynamic>();


            //carica tutti i CF/Nominativi dal file excel di Favale
            while (true)
            {
                string cf = ws.Cell(row, 6).GetValue<string>();
                string nominativo = ws.Cell(row, 7).GetValue<string>();

                if (String.IsNullOrWhiteSpace(cf))
                    break;

                LD.Add(new { CF = cf.Trim().ToUpper(), Nominativo = nominativo.Trim().ToUpper() });
                row++;
            }
            CommonTasks.Log($"Caricate {row} righe" );




            var db = new IncentiviEntities();
            //aggiungi alla lista CF/Nominativi gia in tabella
            var listCF = db.XR_MAT_RICHIESTE.Where(x => x.CF_BAMBINO != null && x.CF_BAMBINO.Trim() != "" && x.NOME_BAMBINO != null &&
                         x.NOME_BAMBINO.Trim() != "").Select(x =>
                         new { x.CF_BAMBINO, x.NOME_BAMBINO }).Distinct().ToList();

            foreach (var item in listCF)
            {
                if (!LD.Any(x => x.CF.Trim().ToUpper() == item.CF_BAMBINO.Trim().ToUpper()))
                    LD.Add(new { CF = item.CF_BAMBINO.Trim().ToUpper(), Nominativo = item.NOME_BAMBINO.Trim().ToUpper() });
            }
            CommonTasks.Log($"Caricati cf/nomi da xr_mat_richieste");


            //aggiungi anche CF/nominativi provenienti da [XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA] (da widget)
            foreach (var item in listCF)
            {
                var rowExtra = db.XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA.Where(x => x.CF == item.CF_BAMBINO).FirstOrDefault();
                if (rowExtra != null && !String.IsNullOrWhiteSpace(rowExtra.NOMINATIVO))
                {
                    if (!LD.Any(x => x.CF.Trim().ToUpper() == item.CF_BAMBINO.Trim().ToUpper()))
                        LD.Add(new { CF = item.CF_BAMBINO.Trim().ToUpper(), Nominativo = rowExtra.NOMINATIVO.ToUpper().Trim() });
                }
            }
                

            //cerca il nome corrispondente al CF ed aggiorna XR_MAT_RICHIESTE
            var list = db.XR_MAT_RICHIESTE.Where(x =>
                       x.CF_BAMBINO != null && x.CF_BAMBINO.Trim() != ""
                       && (x.NOME_BAMBINO == null || x.NOME_BAMBINO.Trim() == "")).ToList();
            foreach (var item in list)
            {
                var RowExcel = LD.Where(x => x.CF.Trim().ToUpper() == item.CF_BAMBINO.Trim().ToUpper()).FirstOrDefault();
                if (RowExcel == null)
                {
                    if (item.XR_WKF_OPERSTATI.Any() && item.XR_WKF_OPERSTATI.Max(x => x.ID_STATO) <= 80)
                    {
                        string nominativo = GetNameByGetCodiceFiscaleInfo(item.MATRICOLA, item.CF_BAMBINO.Trim().ToUpper(), cred);
                        if (!String.IsNullOrWhiteSpace(nominativo))
                        {
                            item.NOME_BAMBINO = nominativo;
                            CommonTasks.Log($"id xr_mat_richieste:{item.ID} CF {item.CF_BAMBINO} aggiunto via WCF {item.NOME_BAMBINO}");
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    item.NOME_BAMBINO = RowExcel.Nominativo.Trim();
                    CommonTasks.Log($"id xr_mat_richieste:{item.ID} CF {item.CF_BAMBINO} aggiunto {item.NOME_BAMBINO}");
                    db.SaveChanges();
                }
            }

        }
        public static string GetNameByGetCodiceFiscaleInfo(string matricola, string cf, System.Net.NetworkCredential cred)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiService1Client();
            try
            {
                cl.ClientCredentials.Windows.ClientCredential = cred;

                CodiceFiscaleReponse resp = cl.GetCodiceFiscaleInfo(null, matricola);
                if (resp != null && resp.CFinfo != null && resp.CFinfo.Any())
                {
                    string nome = resp.CFinfo.Where(x => x.CFfiglioACarico != null && x.CFfiglioACarico.Trim().ToUpper() == cf)
                                .Select(x => x.NominativoFiglio).FirstOrDefault();
                    if (nome != null) nome = nome.Replace("\\", " ").Replace("/", " ").Trim();
                    return nome;
                }
            }
            catch (Exception ex)
            {
                myRaiCommonTasks.Logger.Log("Errore in GetCodiceFiscaleInfo " + ex.ToString());
            }
            return null;
        }
        private static void StressTestDb2()
        {
            int PAUSA = 1000;

            var db = new IncentiviEntities();
            string sql = @"INSERT OPENQUERY(DB2LINK, 
            'SELECT 
                    MATRICOLA,
                    DATA_ECCEZIONE,
                    ECCEZIONE,
                    CODICE_FISCALE,
                    DATA_NASCITA,
                    DATA_INSERIMENTO,
                    DATA_INIZIO,
                    DATA_FINE,
                    CTR_TRASFERIBILE,
                    CTR_NON_TRASFERIBILE,
                    CODICE_INPS,
                    DATA_LOG,
                    ID_HRIS,
                    ID_SELF
                    FROM DB2P.PROVA.EMEN_TB_CONG_PAR')  VALUES ('141266','2020-08-03','HC','','1900-01-01', '2023-07-14 11:06:05','2020-08-03','2020-08-31',0,0,'','2023-07-14 11:06:05',521,  NULL );";
            string sqlSelect = @"select ID_CONGEDO from  OPENQUERY(DB2LINK, 'SELECT * FROM DB2P.PROVA.EMEN_TB_CONG_PAR') where DATA_ECCEZIONE='2020-08-03' and ID_HRIS=521";

            for (int i = 0; i < 10; i++)
            {
                StressTestInsert(sql, db, PAUSA, i);
                StressTestSelect(sqlSelect, db, PAUSA, i);
            }
        }

        private static void StressTestSelect(string sqlSelect, IncentiviEntities db, int PAUSA, int i)
        {
            Console.WriteLine("delay...");
            System.Threading.Thread.Sleep(PAUSA);
            try
            {
                Console.WriteLine(" select in corso, counter ....... " + i);
                IEnumerable<int> rows = db.Database.SqlQuery<int>(sqlSelect);
                Console.WriteLine(" select OK, righe:" + rows.Count());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " - " + ex.InnerException);
            }
        }

        public static void StressTestInsert(string sql, IncentiviEntities db, int PAUSA, int i)
        {
            Console.WriteLine("delay...");
            System.Threading.Thread.Sleep(PAUSA);

            try
            {
                Console.WriteLine(" insert in corso, counter ....... " + i);
                var rows = db.Database.ExecuteSqlCommand(sql);
                Console.WriteLine(" insert OK , inserite:" + rows);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " - " + ex.InnerException);
            }
        }

        private static void ProlungaPeriodiFragili()
        {
            var db = new IncentiviEntities();
            var dbTal = new TalentiaEntities();
            DateTime DataInvio = new DateTime(2023, 3, 30);



            var listaRichieste = db.XR_MAT_RICHIESTE.Where(x => x.ECCEZIONE == "SW" &&
            x.DATA_INVIO_RICHIESTA >= DataInvio &&
            "DISA,PATO,IMMU".Contains(x.XR_MAT_CATEGORIE.SOTTO_CAT))
                                .ToList();
            DateTime Drif = new DateTime(2023, 6, 30);
            List<int> IDSRItoChange = new List<int>();

            foreach (var rich in listaRichieste)
            {
                var SR = dbTal.XR_STATO_RAPPORTO.Where(x =>
                x.MATRICOLA == rich.MATRICOLA &&
                  x.VALID_DTA_INI <= DateTime.Now &&
                  (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now) &&
                  x.DTA_INIZIO <= DateTime.Now &&
                  (x.DTA_FINE > DateTime.Now || x.DTA_FINE == Drif)

                ).ToList();
                if (SR.Any())
                {
                    int id = SR.Select(x => x.ID_STATO_RAPPORTO).First();
                    var SRI = dbTal.XR_STATO_RAPPORTO_INFO.Where(x =>
                       x.ID_STATO_RAPPORTO == id &&
                       x.VALID_DTA_INI < DateTime.Now &&
                       x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now &&
                       x.DTA_INIZIO < DateTime.Now &&
                       (x.VALID_DTA_END > DateTime.Now || x.VALID_DTA_END == Drif)).ToList();
                    Console.WriteLine("-");
                    foreach (var item in SRI)
                    {
                        bool tochange = false;

                        if (item.DTA_FINE == new DateTime(2023, 9, 30) && item.NUM_GIORNI_MAX == 10 && item.NUM_GIORNI_EXTRA != 2)
                        {
                            tochange = true;
                            IDSRItoChange.Add(item.ID_INFO);
                        }

                        Console.WriteLine($"{item.ID_INFO} {item.DTA_INIZIO.ToString("dd/MM/yyyy")} - {item.DTA_FINE.Value.ToString("dd/MM/yyyy")} :" +
                                          $"{item.NUM_GIORNI_MAX} + {item.NUM_GIORNI_EXTRA} {item.ID_RICH} {(tochange ? "*******" : "")}");
                    }

                }
                Console.WriteLine(rich.ID + " ---- " + SR.Count());
            }
            Console.WriteLine(String.Join(",", IDSRItoChange.ToArray()));
        }

        private static void AssegnaAree()
        {
            var db = new IncentiviEntities();
            var list = db.XR_DEM_DOCUMENTI.Where(x => x.IdArea == null && x.MatricolaDestinatario != null)
                .Select(x => new { x.Id, x.MatricolaDestinatario }).ToList();
            var sint = db.SINTESI1.Select(x => new { x.COD_SERVIZIO, x.COD_MATLIBROMAT }).ToList();
            int c = 0;
            foreach (var m in list)
            {
                c++;
                Console.WriteLine(c);

                var row = sint.Where(x => x.COD_MATLIBROMAT == m.MatricolaDestinatario).FirstOrDefault();
                if (row != null)
                {
                    int? area = db.XR_HRIS_DIR_FILTER.Where(x => x.DIR_INCLUDED != null && x.DIR_INCLUDED.Contains(row.COD_SERVIZIO))
                        .Select(x => x.ID_AREA_FILTER).FirstOrDefault();

                    if (area != null)
                    {
                        var doc = db.XR_DEM_DOCUMENTI.Where(x => x.Id == m.Id).FirstOrDefault();
                        if (doc != null)
                        {
                            doc.IdArea = area;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static void Allunga30Giugno()
        {
            var db = new IncentiviEntities();
            var dbtal = new TalentiaEntities();
            DateTime Dinvio = new DateTime(2023, 5, 1);
            var list = db.XR_MAT_RICHIESTE.Where(x => x.DATA_INVIO_RICHIESTA > Dinvio && new int[] { 91/*MI14*/ }// G : MI14, PATO, IMMU
            .Contains(x.CATEGORIA))
                .OrderBy(x => x.MATRICOLA).ToList();

            foreach (var rich in list)
            {
                var idStatoMax = db.XR_WKF_OPERSTATI.Where(x => x.ID_GESTIONE == rich.ID).Max(x => x.ID_STATO);
                var sr = dbtal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == rich.MATRICOLA && x.DTA_INIZIO > Dinvio && x.VALID_DTA_END == null).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
                Console.WriteLine($"{rich.DATA_INVIO_RICHIESTA.ToString("dd/MM/yyyy")} stato:{idStatoMax} {rich.MATRICOLA} {rich.CATEGORIA} APPR: {rich.GIORNI_APPROVATI} SR: {sr.DTA_INIZIO.ToString("dd/MM/yyyy")} {sr.DTA_FINE.ToString("dd/MM/yyyy")}");
                var sri = dbtal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_STATO_RAPPORTO == sr.ID_STATO_RAPPORTO && x.VALID_DTA_END == null).ToList();
                foreach (var item in sri)
                {
                    Console.WriteLine($"---- SRI {item.DTA_INIZIO.ToString("dd/MM/yyyy")} {item.DTA_FINE.Value.ToString("dd/MM/yyyy")}");
                }
            }
        }
        private void SpezzaSRI(int idStatoRapportoInfo, DateTime Dfine)
        {
            //var dbtal = new TalentiaEntities();
            //var sri = dbtal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_INFO == idStatoRapportoInfo).FirstOrDefault();
            //if (sri != null)
            //{
            //    sri.DTA_FINE = Dfine;
            //    XR_STATO_RAPPORTO_INFO info = new XR_STATO_RAPPORTO_INFO() {
            //         COD_TERMID=sri.COD_TERMID,
            //          COD_USER="BATCH",
            //           DTA_INVIO=sri.DTA_INVIO,
            //            DTA_VISITA_MEDICA = sri.DTA_VISITA_MEDICA,


            //    }
            //}
        }
        private static void ModificaGiorni()
        {
            var dbCzn = new IncentiviEntities();
            var dbtal = new TalentiaEntities();
            var rich = dbCzn.XR_MAT_RICHIESTE.Where(x => (x.CATEGORIA == 52 || x.CATEGORIA == 91) && x.DATA_INVIO_RICHIESTA > new DateTime(2023, 4, 11))
                        .ToList();

            foreach (var r in rich)
            {
                var rowSRI = dbtal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_RICH == r.ID).FirstOrDefault();
                if (rowSRI != null)
                {
                    if (rowSRI.DTA_FINE != r.DATA_FINE_SW)
                    {
                        DateTime Dnext = new DateTime(rowSRI.DTA_FINE.Value.Year, rowSRI.DTA_FINE.Value.Month, rowSRI.DTA_FINE.Value.Day).AddDays(1);

                        rowSRI.DTA_FINE = r.DATA_FINE_SW;
                        var rowSriNext = dbtal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_STATO_RAPPORTO == rowSRI.ID_STATO_RAPPORTO &&
                                         x.DTA_INIZIO == Dnext).FirstOrDefault();
                        if (rowSriNext != null)
                            dbtal.XR_STATO_RAPPORTO_INFO.Remove(rowSriNext);

                        dbtal.SaveChanges();
                    }
                }
            }

        }

        public static void FindCF()
        {
            var dbCzn = new IncentiviEntities();
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sw14.xlsx");

            var Richieste = dbCzn.XR_MAT_RICHIESTE.Where(x => x.CATEGORIA == 52 || x.CATEGORIA == 91).ToList();

            var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();
            int counter = 0;
            int trovati = 0;

            for (int row = 2; row < 100000; row++)
            {
                string EsitoCF = "";

                counter++;
                string matricola = ws.Cell(row, 1).GetValue<string>();
                if (String.IsNullOrWhiteSpace(matricola))
                    break;

                string tipologia = ws.Cell(row, 19).GetValue<string>();
                if (tipologia.ToLower().Contains("14 anni"))
                {
                    DateTime inizioPeriodo = Convert.ToDateTime(ws.Cell(row, 14).GetValue<string>());
                    DateTime finePeriodo = Convert.ToDateTime(ws.Cell(row, 15).GetValue<string>());

                    var Ric = Richieste.Where(x => x.MATRICOLA == matricola && x.DATA_INIZIO_SW == inizioPeriodo && x.DATA_FINE_SW == finePeriodo).FirstOrDefault();
                    if (Ric == null)
                    {
                        Ric = Richieste.Where(x => x.MATRICOLA == matricola).FirstOrDefault();
                        if (Ric != null && !String.IsNullOrWhiteSpace(Ric.CF_BAMBINO))
                        {
                            // EsitoCF = "** " + Ric.CF_BAMBINO;
                        }

                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(Ric.CF_BAMBINO))
                        {
                            EsitoCF = Ric.CF_BAMBINO;
                            trovati++;
                        }

                    }
                }

                ws.Cell(row, 20).SetValue<string>(EsitoCF.ToUpper());
            }
            wb.SaveAs(path.Replace(".xlsx", "") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");
            Console.WriteLine("Trovati " + trovati);
            Console.ReadKey();
        }

        public static bool IsValidTsez(XR_STR_TSEZIONE tsez)
        {
            if (tsez == null || String.IsNullOrWhiteSpace(tsez.data_fine_validita))
                return false;

            int FineVal = Convert.ToInt32(tsez.data_fine_validita);

            return (FineVal > 20230217);

        }
        public static void FindICT()
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Foglio1");
            var dbtal = new TalentiaEntities();

            ws.Cell(2, 1).SetValue<string>("DIREZIONE");
            ws.Cell(2, 2).SetValue<string>("ICT");
            ws.Cell(2, 3).SetValue<string>("GG");
            ws.Cell(2, 4).SetValue<string>("UT_GG");
            ws.Cell(2, 5).SetValue<string>("UT_GG_EXT");

            var ListaSecondiLivelli = dbtal.XR_STR_TALBERO.Where(x => x.subordinato_a == 6097).ToList();
            int row = 2;
            foreach (var secLivello in ListaSecondiLivelli)
            {
                var tsez = dbtal.XR_STR_TSEZIONE.Where(x => x.id == secLivello.id && x.data_fine_validita == "99991231").FirstOrDefault();
                if (tsez != null)
                {
                    row++;

                    ws.Cell(row, 1).SetValue<string>("Struttura");
                    ws.Cell(row, 2).SetValue<string>(tsez.descrizione_lunga);
                    ws.Cell(row, 3).SetValue<string>(tsez.codice_visibile);
                    ws.Cell(row, 4).SetValue<string>("UT_" + tsez.codice_visibile);
                    ws.Cell(row, 5).SetValue<string>("UT_" + tsez.codice_visibile + "_EXT");

                    Console.WriteLine(tsez.descrizione_lunga);
                    var TerziLivelli = dbtal.XR_STR_TALBERO.Where(x => x.subordinato_a == secLivello.id).ToList();
                    foreach (var liv3 in TerziLivelli)
                    {
                        var tsez3 = dbtal.XR_STR_TSEZIONE.Where(x => x.id == liv3.id && x.data_fine_validita == "99991231").FirstOrDefault();
                        if (tsez3 != null)
                        {
                            row++;
                            ws.Cell(row, 1).SetValue<string>("    Unità organizzativa");
                            ws.Cell(row, 2).SetValue<string>("    " + tsez3.descrizione_lunga);
                            ws.Cell(row, 3).SetValue<string>(tsez3.codice_visibile);
                            ws.Cell(row, 4).SetValue<string>("UT_" + tsez3.codice_visibile);
                            ws.Cell(row, 5).SetValue<string>("UT_" + tsez3.codice_visibile + "_EXT");
                            Console.WriteLine("-----" + tsez3.descrizione_lunga);
                        }
                    }
                }
            }
            wb.SaveAs("ict.xlsx");

        }
        public static string AllineaDaDbVersoRestMatricola(string matricola, string cf, WebClient wb)
        {
            var dbczn = new IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();
            DateTime Dnow = DateTime.Now;

            /*var matricole = dbtal.XR_STATO_RAPPORTO
                .Where(x => x.VALID_DTA_INI < Dnow && (x.VALID_DTA_END > Dnow || x.VALID_DTA_END == null)
                       && x.DTA_INIZIO <= Dnow && x.DTA_FINE >= Dnow
                      && x.COD_STATO_RAPPORTO == "SW" && //firmato
                      x.COD_TIPO_ACCORDO == "Consensuale")
                .Select(x => x.MATRICOLA).Distinct().OrderBy(x => x).ToList();*/
            var SR = dbtal.XR_STATO_RAPPORTO.Where(x =>
                 x.MATRICOLA == matricola
                 && x.VALID_DTA_INI < Dnow
                 && (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow)
                 && x.DTA_INIZIO < Dnow &&
                 x.DTA_FINE > Dnow &&
                     //&& x.COD_STATO_RAPPORTO == "SW" && //firmato
                     (x.COD_TIPO_ACCORDO == "Consensuale" || x.COD_TIPO_ACCORDO == "Deroga"))
            .FirstOrDefault();

            if (SR != null)
            {
                int IDswAPI = myRaiCommonManager.AnagraficaManager.InserisciApiNuova(SR, "BATCH");
                return null;
            }
            else
                return "StatoRapporto non trovato per " + matricola;
        }
        public static string AllineaDaRestVersoDBmatricola(string matricola, string cf, WebClient wb, string token, int? ID_RowSWAPI = null)
        {
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);
            var dbczn = new IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();

            string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFLavoratore=" + cf);
            myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse>(response);
            if (RCR.Comunicazioni != null && RCR.Comunicazioni.Any())
            {
                var comPresente = RCR.Comunicazioni.First();
                string CodiceRiferimentoAPI = comPresente.codiceComunicazione;
                //var respDettaglio = APIsw.DettaglioComunicazione(token, CodiceRiferimentoAPI);

                DateTime Dnow = DateTime.Now;
                var SR = dbtal.XR_STATO_RAPPORTO.Where(x =>
                     x.MATRICOLA == matricola
                     && x.VALID_DTA_INI < Dnow
                     && (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow)
                     && x.DTA_INIZIO < Dnow &&
                     x.DTA_FINE > Dnow)
                .FirstOrDefault();

                if (SR == null) //se non ha trovato periodo attuale, ignora il periodo (magari è un periodo scaduto)
                {
                    SR = dbtal.XR_STATO_RAPPORTO.Where(x =>
                     x.MATRICOLA == matricola
                     && x.VALID_DTA_INI < Dnow
                     && (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow)
                      ).FirstOrDefault();
                }

                //inserisci come nuova gia con codice riferimento api
                int IDswAPI =
                    (ID_RowSWAPI == null ? myRaiCommonManager.AnagraficaManager.InserisciApiNuova(SR)
                                        : (int)ID_RowSWAPI);

                var RowswAPI = dbczn.XR_SW_API.Where(x => x.ID == IDswAPI).FirstOrDefault();

                RowswAPI.CODICE_COMUNICAZIONE_API = CodiceRiferimentoAPI;
                RowswAPI.DATA_RISPOSTA_API = DateTime.Now;
                RowswAPI.DATA_INVIO_API = DateTime.Now;
                RowswAPI.ERRORE = null;
                dbczn.SaveChanges();
                return null;

            }
            else
                return "Non ci sono comunicazioni nella risposta " + response;
        }

        public bool IsGiaSuTabellaInvio(string matricola)
        {
            var dbczn = new IncentiviEntities();
            return (dbczn.XR_SW_API.Any(x => x.MATRICOLA == matricola));
        }
        public string IsGiaSuRest(string cf, WebClient wb)
        {
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);
            var dbczn = new IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();

            string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFLavoratore=" + cf);
            myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse>(response);
            if (RCR.Comunicazioni != null && RCR.Comunicazioni.Any())
            {
                var comPresente = RCR.Comunicazioni.First();
                return comPresente.codiceComunicazione;
            }
            else
                return null;
        }
        public static List<InfoAssunzioni> GetInfoAssunzioni()
        {
            List<InfoAssunzioni> Assunzioni = new List<InfoAssunzioni>();
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cfl_assunzioni.xlsx");

            var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();
            for (int row = 5; row < 100000; row++)
            {
                string contratto = ws.Cell(row, 6).GetValue<string>();
                if (!contratto.ToUpper().Contains("FORMAZIONE")) continue;

                string Matricola = ws.Cell(row, 2).GetValue<string>();
                string data = ws.Cell(row, 5).GetValue<string>();
                DateTime D = Convert.ToDateTime(data);
                Assunzioni.Add(new InfoAssunzioni() { matricola = Matricola, dataAssunzione = D });
            }

            return Assunzioni;
        }
        public static bool IsOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 < end2 && start2 < end1;
        }
        public static void AggiungiDaStatoRapporto()
        {
            CommonTasks.Log("AggiungiDaStatoRapporto() start");
            var dbczn = new IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();
            DateTime Dnow = DateTime.Now;

            var SR = dbtal.XR_STATO_RAPPORTO
                .Where(x => x.VALID_DTA_INI < Dnow && (x.VALID_DTA_END > Dnow || x.VALID_DTA_END == null)
                       && x.DTA_INIZIO <= Dnow && x.DTA_FINE >= Dnow
                      && x.COD_STATO_RAPPORTO == "SW"
                      && (x.COD_TIPO_ACCORDO == "Consensuale" || x.COD_TIPO_ACCORDO == "Deroga"))
                 .OrderBy(x => x.MATRICOLA).ToList();

            ////////////////////////////////////////////////////
            //SR = SR.Where(x => x.MATRICOLA == "103650").ToList();
            ///////////////////////////////////////////////////////////


            var matricolePeriodi = dbczn.XR_SW_API.Where(x =>
                x.TIPOLOGIA_API == "I").Select(x => new { x.MATRICOLA, x.PERIODO_DAL, x.PERIODO_AL, x.ID })
                .OrderBy(x => x.MATRICOLA).ToList();

            var IDannullate = dbczn.XR_SW_API.Where(x => x.TIPOLOGIA_API == "A").Select(x => x.ID_RIFERIMENTO_SW_API).ToList();
            matricolePeriodi.RemoveAll(x => IDannullate.Contains(x.ID));


            List<XR_STATO_RAPPORTO> DaInserire = new List<XR_STATO_RAPPORTO>();

            foreach (var statoRapporto in SR)
            {

                string inizio = "matr " + statoRapporto.MATRICOLA + " statorapporto "
                        + statoRapporto.DTA_INIZIO.ToString("dd/MM/yyyy") + "-" + statoRapporto.DTA_FINE.ToString("dd/MM/yyyy");

                var matrPeriodoPresente = matricolePeriodi.Where(x =>
                  (x.MATRICOLA == statoRapporto.MATRICOLA && x.PERIODO_DAL <= statoRapporto.DTA_INIZIO && x.PERIODO_AL >= statoRapporto.DTA_FINE)
                ).FirstOrDefault();
                if (matrPeriodoPresente != null)
                {
                    //CommonTasks.Log(inizio+" compreso nelle api presenti " + matrPeriodoPresente.ID + " " + matrPeriodoPresente.PERIODO_DAL.ToString("dd/MM/yyyy")
                    //    +"-"+ matrPeriodoPresente.PERIODO_AL.ToString("dd/MM/yyyy"));
                    continue;
                }

                else
                {
                    var recesso = dbczn.XR_SW_API.Where(x => x.MATRICOLA == statoRapporto.MATRICOLA && x.TIPOLOGIA_API == "R" &&
                           x.PERIODO_DAL == statoRapporto.DTA_INIZIO && x.PERIODO_AL == statoRapporto.DTA_FINE).FirstOrDefault();

                    if (recesso != null)
                    {
                        //CommonTasks.Log(inizio + " presente nel recesso id " + recesso.ID + " " + recesso.PERIODO_DAL.ToString("dd/MM/yyyy") + "-"
                        //    + recesso.PERIODO_AL.ToString("dd/MM/yyyy"));
                        continue; //è stato fatto un recesso portandolo a quelle date
                    }
                    else
                    {
                        CommonTasks.Log(inizio + " da inserire");
                        DaInserire.Add(statoRapporto);
                    }

                }
            }


            List<XR_STATO_RAPPORTO> DaInserireUniciperiodi = new List<XR_STATO_RAPPORTO>();
            var ragg = DaInserire.GroupBy(x => x.MATRICOLA).ToList();
            foreach (var gruppo in ragg)
            {
                if (gruppo.Count() == 1)
                {
                    DaInserireUniciperiodi.Add(gruppo.First());
                    CommonTasks.Log("Unico periodo: " + gruppo.First().MATRICOLA + " " + gruppo.First().DTA_INIZIO.ToString("dd/MM/yyyy") + "-" +
                        gruppo.First().DTA_FINE.ToString("dd/MM/yyyy"));
                }
                else
                {
                    foreach (var gr in gruppo)
                    {
                        CommonTasks.Log("periodi per " + gr.MATRICOLA + " " + gr.DTA_INIZIO.ToString("dd/MM/yyyy") + "-" +
                        gr.DTA_FINE.ToString("dd/MM/yyyy"));
                    }
                    var maxDays = gruppo.Max(x => (x.DTA_FINE - x.DTA_INIZIO).TotalDays);
                    var g = gruppo.Where(z => (z.DTA_FINE - z.DTA_INIZIO).TotalDays == maxDays).FirstOrDefault();
                    CommonTasks.Log("da inserire " + g.MATRICOLA + " " + g.DTA_INIZIO.ToString("dd/MM/yyyy") + "-" +
                                        g.DTA_FINE.ToString("dd/MM/yyyy") + "\r\n");

                    DaInserireUniciperiodi.Add(g);
                }
            }


            List<XR_STATO_RAPPORTO> DaInserireVerificatiPeriodi = new List<XR_STATO_RAPPORTO>();
            foreach (var itemSR in DaInserireUniciperiodi)
            {
                var rowApiInviati = dbczn.XR_SW_API.Where(x => x.TIPOLOGIA_API == "I" && x.MATRICOLA == itemSR.MATRICOLA).ToList();
                rowApiInviati.RemoveAll(x => dbczn.XR_SW_API.Any(z =>
                        z.TIPOLOGIA_API == "A" && z.MATRICOLA == x.MATRICOLA && z.ID_RIFERIMENTO_SW_API == x.ID));

                if (rowApiInviati.Any())
                {
                    bool overlap = false;
                    foreach (var rowApi in rowApiInviati)
                    {
                        if (IsOverlap(rowApi.PERIODO_DAL, rowApi.PERIODO_AL, itemSR.DTA_INIZIO, itemSR.DTA_FINE))
                        {
                            overlap = true;
                            CommonTasks.Log("\r\n" + itemSR.MATRICOLA + " da inviare: " + itemSR.DTA_INIZIO.ToString("dd/MM/yyyy") + "-"
                                + itemSR.DTA_FINE.ToString("dd/MM/yyyy")
                                + "    gia in tabella API:" +
                                rowApi.PERIODO_DAL.ToString("dd/MM/yyyy") + "-" + rowApi.PERIODO_AL.ToString("dd/MM/yyyy")
                                + " JsonInv:" + (rowApi.JSON_INVIATO == null ? "null" : "") + " id:" + rowApi.ID + "  Errore:" + rowApi.ERRORE);

                            bool compreso = rowApi.PERIODO_DAL <= itemSR.DTA_INIZIO && rowApi.PERIODO_AL >= itemSR.DTA_FINE;
                            CommonTasks.Log("Compreso: " + compreso);
                            var RigheSR = SR.Where(x => x.MATRICOLA == itemSR.MATRICOLA).ToList();
                            foreach (var R in RigheSR)
                            {
                                var rigaModDip = dbtal.XR_MOD_DIPENDENTI.Where(x => x.MATRICOLA == R.MATRICOLA
                                       && x.COD_MODULO == "AccordoIndividualeDipendentiSW2022" && x.SCELTA == "TRUE"
                                       && x.DATA_COMPILAZIONE != null && x.DATA_INIZIO == R.DTA_INIZIO
                                       && x.DATA_SCADENZA == R.DTA_FINE).FirstOrDefault();


                                CommonTasks.Log(R.MATRICOLA + " " + R.DTA_INIZIO.ToString("dd/MM/yyyy") + "-" +
                                    R.DTA_FINE.ToString("dd/MM/yyyy") + "  " + R.COD_STATO_RAPPORTO + " data_accordo:" +

                                     (rigaModDip != null ? rigaModDip.DATA_COMPILAZIONE.Value.ToString("dd/MM/yyyy") : "NON TROVATO"));
                            }
                        }
                    }
                    if (!overlap)
                    {
                        DaInserireVerificatiPeriodi.Add(itemSR);
                    }
                }
                else
                    DaInserireVerificatiPeriodi.Add(itemSR);
            }
            if (DaInserireVerificatiPeriodi.Any())
            {
                foreach (var st in DaInserireVerificatiPeriodi)
                {
                    int IDswAPI = myRaiCommonManager.AnagraficaManager.InserisciApiNuova(st, "BATCH");
                    CommonTasks.Log("Aggiunta XR_SW_API id " + IDswAPI + " matr: " + st.MATRICOLA + " " + st.DTA_INIZIO.ToString("dd/MM/yyyy") + "-" + st.DTA_FINE.ToString("dd/MM/yyyy"));
                }
            }
            CommonTasks.Log("AggiungiDaStatoRapporto() end");
        }
        //riporta lo stato delle API lato ministero nel nostro DB tabella XR_SW_API come fossero gia state inviate dal batch
        public static void AllineaDaRestVersoDB()
        {
            CommonTasks.Log("AllineaDaRestVersoDB()");

            API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
            WebClient wb = new WebClient();

            wb.Headers.Add("Authorization", "Bearer " + TR.access_token);
            wb.Headers.Add("Content-Type", "application/json");


            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);
            var dbczn = new IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();
            DateTime Dnow = DateTime.Now;

            var matricole = dbtal.XR_STATO_RAPPORTO
                .Where(x => x.VALID_DTA_INI < Dnow && (x.VALID_DTA_END > Dnow || x.VALID_DTA_END == null)
                       && x.DTA_INIZIO <= Dnow && x.DTA_FINE >= Dnow
                      && x.COD_STATO_RAPPORTO == "SW"
                      && //firmato
                      (
                      x.COD_TIPO_ACCORDO == "Consensuale" ||
                      x.COD_TIPO_ACCORDO == "Deroga"))
                .Select(x => x.MATRICOLA).Distinct().OrderBy(x => x).ToList();



            var sintesi = dbtal.SINTESI1.Select(x => new { x.COD_MATLIBROMAT, x.CSF_CFSPERSONA }).ToList();

            var MatricoleInTabellaInvio = dbczn.XR_SW_API.Where(x =>
                             !x.MATRICOLA.Contains("*")
                             && x.TIPOLOGIA_API == "I").Select(x => x.MATRICOLA).Distinct().ToList();

            matricole.RemoveAll(x => MatricoleInTabellaInvio.Contains(x));

            foreach (var matr in matricole)
            {
                CommonTasks.Log("Matr: " + matr);
                if (dbczn.XR_SW_API.Any(x => x.MATRICOLA == matr))
                {
                    CommonTasks.Log("Gia presente in tabella di invio");
                    continue;
                }
                string cf = sintesi.Where(x => x.COD_MATLIBROMAT == matr).Select(x => x.CSF_CFSPERSONA).FirstOrDefault();
                CommonTasks.Log(cf);
                if (String.IsNullOrWhiteSpace(cf))
                {
                    CommonTasks.Log("cod fiscale non presente");
                    continue;
                }
                try
                {

                    //CommonTasks.Log("Non presente su rest, allineamento da DB a rest...");
                    string esito = AllineaDaDbVersoRestMatricola(matr, cf, wb);
                    if (esito == null)
                        CommonTasks.Log("Aggiunta in tabella");
                    else
                        CommonTasks.Log(esito);

                    //string esito = AllineaDaRestVersoDBmatricola(matr, cf, wb, TR.access_token);
                    //if (esito == null)
                    //    CommonTasks.Log("Allineata");
                    //else
                    //     CommonTasks.Log(esito);
                    //else
                    //{
                    //    CommonTasks.Log("Non presente su rest, allineamento da DB a rest...");
                    //    esito = AllineaDaDbVersoRestMatricola(matr, cf, wb);
                    //    if (esito == null)
                    //        CommonTasks.Log("Allineata");
                    //    else
                    //        CommonTasks.Log(esito);
                    //}
                }
                catch (Exception ex)
                {
                    CommonTasks.Log(ex.ToString());
                }
            }
        }
        private static string QuanteRich(string matricola)
        {
            var db = new IncentiviEntities();
            var listRichieste = db.XR_MAT_RICHIESTE.Where(x => x.ECCEZIONE == "SW" && x.MATRICOLA == matricola).ToList();
            listRichieste = listRichieste.Where(x => x.XR_WKF_OPERSTATI.Any() && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO >= 20)).ToList();
            return listRichieste.Count() + " " + string.Join(",", listRichieste.Select(x => x.ID).ToArray());
        }

        private static void VerificaGiorniApprovati()
        {
            var db = new IncentiviEntities();
            var dbTal = new TalentiaEntities();

            var listRichieste = db.XR_MAT_RICHIESTE.Where(x => x.ECCEZIONE == "SW" && (x.XR_MAT_CATEGORIE.SOTTO_CAT == "MI14"
                               || x.XR_MAT_CATEGORIE.SOTTO_CAT == "ASSD" || x.XR_MAT_CATEGORIE.SOTTO_CAT == "BES")
                              ).ToList();
            // && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO >= 20)
            listRichieste = listRichieste.Where(x => x.XR_WKF_OPERSTATI.Any() && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO >= 20)).ToList();

            foreach (var rich in listRichieste)
            {
                DateTime D1 = rich.DATA_INIZIO_SW.Value;
                DateTime D2 = rich.DATA_FINE_SW.Value;

                var SR = dbTal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == rich.MATRICOLA && x.DTA_INIZIO == D1 && x.DTA_FINE == D2
                && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                    .FirstOrDefault();
                if (SR != null)
                {
                    myRaiCommonTasks.CommonTasks.Log("\r\n\r\nRich " + rich.ID + "   " + D1.ToString("yyyy-MM-dd") + " / " + D2.ToString("yyyy-MM-dd"));
                    foreach (var item in SR.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                    {

                        string msg = " Rich: " + rich.ID + " Matr: " + rich.MATRICOLA + " ,  Approvati: " + rich.GIORNI_APPROVATI + " - " + item.NUM_GIORNI_MAX + " + " +
                            item.NUM_GIORNI_EXTRA + "    " + item.DTA_INIZIO.ToString("yyyy-MM-dd") + " / " + item.DTA_FINE.Value.ToString("yyyy-MM-dd");

                        myRaiCommonTasks.CommonTasks.Log(msg);
                        if (rich.GIORNI_APPROVATI != item.NUM_GIORNI_EXTRA)
                        {
                            myRaiCommonTasks.CommonTasks.Log("********  Tot Rich: " + QuanteRich(rich.MATRICOLA).ToString() + " ID SR: " + item.ID_STATO_RAPPORTO + "\r\n\r\n");
                        }

                    }
                }

            }
        }
        public static void RecuperaE036()
        {
            var db = new IncentiviEntities();
            var list = db.XR_SW_API.Where(x => x.TIPOLOGIA_API == "I" && x.ERRORE.StartsWith("E036")).OrderBy(x => x.MATRICOLA).ToList();
            foreach (var row in list)
            {



                API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
                if (TR == null || String.IsNullOrWhiteSpace(TR.access_token))
                {
                    _log.Error("Token non ottenuto, impossibile procedere");
                    continue; ;
                }
                else
                    _log.Info("Token ok");


                WebClient WebCl = GetWebClient(TR.access_token);
                var cf = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == row.MATRICOLA).Select(x => x.CSF_CFSPERSONA).FirstOrDefault();
                if (String.IsNullOrWhiteSpace(cf))
                {
                    _log.Error(row.MATRICOLA + " CF non ottenuto");
                    continue;
                }
                try
                {
                    _log.Info("recupero matricola " + row.MATRICOLA);
                    string esito = AllineaDaRestVersoDBmatricola(row.MATRICOLA, cf, WebCl, TR.access_token, row.ID);
                    if (!String.IsNullOrWhiteSpace(esito))
                    {
                        _log.Error(row.MATRICOLA + esito);
                    }
                }
                catch (Exception ex)
                {

                    _log.Error(row.MATRICOLA + ex.ToString());
                }
            }
        }


        private static void Processa_I(XR_SW_API nuovaCom, IncentiviEntities db, List<InfoAssunzioni> AssunzioniFileExcel)
        {
            _log.Info("invio I xr_sw_api matr " + nuovaCom.MATRICOLA + " id " + nuovaCom.ID);

            XR_MOD_DIPENDENTI roWModDipendenti = APIsw.IsAccordoFirmato(nuovaCom.MATRICOLA, nuovaCom.PERIODO_DAL, nuovaCom.PERIODO_AL, nuovaCom);
            if (roWModDipendenti == null || roWModDipendenti.DATA_COMPILAZIONE == null)
            {
                string noDati = "Dati accordo firmato non trovati";
                _log.Error(noDati);

                if (nuovaCom.ERRORE != noDati)
                {
                    nuovaCom.ERRORE = noDati;
                    db.SaveChanges();
                }
                return;
            }

            API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
            if (TR == null || String.IsNullOrWhiteSpace(TR.access_token))
            {
                _log.Error("Token non ottenuto, impossibile procedere");
                return;
            }
            else
                _log.Info("Token ok");

            WebClient WebCl = GetWebClient(TR.access_token);

            myRaiHelper.APISW.Models.CreaComunicazione.CreaComunicazioneResponse Response = null;
            DateTime? Dassunzione = AssunzioniFileExcel.Where(x => x.matricola == nuovaCom.MATRICOLA).Select(x => x.dataAssunzione).FirstOrDefault();

            try
            {
                Response = APIsw.CreaComunicazione(TR.access_token, nuovaCom.ID, WebCl, _log, Dassunzione, roWModDipendenti);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return;
            }

            if (Response.Esito.First().codice == "Errore")
            {
                _log.Error("Invio in errore :" + Response.Esito.First().messaggio);
                nuovaCom.ERRORE = Response.Esito.First().messaggio;
                db.SaveChanges();
            }
        }

        private static void Processa_M(XR_SW_API modCom)
        {
            _log.Info("invio M xr_sw_api matr " + modCom.MATRICOLA + " id " + modCom.ID);

            API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
            if (TR == null || String.IsNullOrWhiteSpace(TR.access_token))
            {
                _log.Error("Token non ottenuto, impossibile procedere");
                return;
            }
            else
                _log.Info("Token ok");


            WebClient WebCl = GetWebClient(TR.access_token);

            API.models.ModificaComunicazione.ModificaComunicazioneResponse Response = null;

            Response = APIsw.ModificaComunicazione(TR.access_token, modCom.ID, WebCl, _log);

            if (Response.Esito.First().codice == "Errore")
            {
                _log.Error("Invio in errore :" + Response.Esito.First().messaggio);
            }
        }

        private static void Processa_A(XR_SW_API annCom)
        {
            _log.Info("invio A xr_sw_api matr " + annCom.MATRICOLA + " id " + annCom.ID);

            API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
            if (TR == null || String.IsNullOrWhiteSpace(TR.access_token))
            {
                _log.Error("Token non ottenuto, impossibile procedere");
                return;
            }
            else
                _log.Info("Token ok");


            WebClient WebCl = GetWebClient(TR.access_token);


            myRaiHelper.APISW.Models.AnnullaComunicazione.AnnullaComunicazioneResponse Response = null;

            Response = APIsw.AnnullaComunicazione(TR.access_token, annCom.ID, WebCl, _log);

            if (Response.Esito.First().codice == "Errore")
            {
                _log.Error("Invio in errore :" + Response.Esito.First().messaggio);
            }
        }

        private static void Processa_R(XR_SW_API RecCom)
        {
            _log.Info("invio R xr_sw_api matr " + RecCom.MATRICOLA + " id " + RecCom.ID);

            API.models.Token.TokenResponse TR = APIsw.GetToken(_log);
            if (TR == null || String.IsNullOrWhiteSpace(TR.access_token))
            {
                _log.Error("Token non ottenuto, impossibile procedere");
                return;
            }
            else
                _log.Info("Token ok");


            WebClient WebCl = GetWebClient(TR.access_token);


            API.models.RecediComunicazioneResponse Response = null;

            Response = APIsw.RecediComunicazione(TR.access_token, RecCom.ID, WebCl, _log);

            if (Response.Esito.First().codice == "Errore")
            {
                _log.Error("Invio in errore :" + Response.Esito.First().messaggio);
            }
        }

        private static void SendAPI()
        {
            //int? FORZA_API_ID = 5925;

            _log.Info("------------------- Batch Multiplo Metodo SENDAPI() avviato -------------------------------");

            // AnnullaNonRai();


            try
            {
                AggiungiDaStatoRapporto();
            }
            catch (Exception ex)
            {
                _log.Error("AggiungiDaStatoRapporto:" + ex.ToString());
            }

            //try
            //{
            //    RecuperaE036();//sovrapposizioni
            //}
            //catch (Exception ex)
            //{

            //    _log.Error("RecuperaE036:" + ex.ToString());
            //}

            _log.Info("lettura file excel CFL cfl_assunzioni.xlsx...");
            List<InfoAssunzioni> AssunzioniFileExcel = GetInfoAssunzioni();

            _log.Info("Ricerca record xr_sw_api da inviare...");
            var db = new IncentiviEntities();
            DateTime Dnow = DateTime.Now;
            List<XR_SW_API> list = db.XR_SW_API
                .Where(x =>
                x.ID== 6452)
                            //!x.MATRICOLA.Contains("*")
                            //&&
                            //x.PERIODO_DAL <= Dnow && x.PERIODO_AL >= Dnow
                            //&&
                            //(x.DATA_RISPOSTA_API == null || x.ERRORE != null))

                .OrderBy(x => x.MATRICOLA).ToList();


            var MatricoleRai = db.SINTESI1.Where(x => x.COD_SOGGETTOCR == "Rai").Select(x => x.COD_MATLIBROMAT).ToList();
            list = list.Where(x => MatricoleRai.Contains(x.MATRICOLA)).ToList();

            if (!list.Any())
            {
                _log.Info("0 record");
                return;
            }
            else _log.Info(list.Count() + " record");

            // NUOVE TIPO I ---------------------------------------------------------------

            var NuoveCom = list.Where(x => x.TIPOLOGIA_API == "I").ToList();

            if (NuoveCom.Any())
            {
                _log.Info("invio xr_sw_api di tipo I - trovate: " + NuoveCom.Count());

                foreach (XR_SW_API nuovaCom in NuoveCom)
                {
                    try
                    {

                        Processa_I(nuovaCom, db, AssunzioniFileExcel);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(nuovaCom.MATRICOLA + ":" + ex.ToString());
                    }
                }
            }

            //MODIFICA TIPO M ---------------------------------------------------------------

            var ModificaCom = list.Where(x => x.TIPOLOGIA_API == "M").ToList();
            if (ModificaCom.Any())
            {
                _log.Info("invio xr_sw_api di tipo M - trovate: " + ModificaCom.Count());
                foreach (var modCom in ModificaCom)
                {
                    try
                    {
                        Processa_M(modCom);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(modCom.MATRICOLA + ":" + ex.ToString());
                    }
                }
            }

            //ANNULLA TIPO A ---------------------------------------------------------------

            var AnnullaCom = list.Where(x => x.TIPOLOGIA_API == "A").ToList();
            if (AnnullaCom.Any())
            {
                _log.Info("invio xr_sw_api di tipo A - trovate: " + AnnullaCom.Count());
                foreach (var annCom in AnnullaCom)
                {
                    try
                    {
                        Processa_A(annCom);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(annCom.MATRICOLA + ":" + ex.ToString());
                    }
                }
            }

            //RECEDI TIPO R ---------------------------------------------------------------

            var RecediCom = list.Where(x => x.TIPOLOGIA_API == "R").ToList();
            if (RecediCom.Any())
            {
                _log.Info("invio xr_sw_api di tipo R - trovate: " + RecediCom.Count());
                foreach (var RecCom in RecediCom)
                {
                    try
                    {
                        Processa_R(RecCom);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(RecCom.MATRICOLA + ":" + ex.ToString());
                    }
                }
            }
        }

        private static void AnnullaNonRai()
        {
            var db = new IncentiviEntities();
            var ListNonRai = db.SINTESI1.Where(x => x.COD_SOGGETTOCR != "Rai").Select(x => x.COD_MATLIBROMAT).ToList();

            var rows = db.XR_SW_API.Where(x => x.TIPOLOGIA_API == "I" && x.CODICE_COMUNICAZIONE_API != null &&
                       ListNonRai.Contains(x.MATRICOLA)).ToList();
            foreach (var row in rows)
            {
                if (row.ID_STATORAPPORTO != null)
                    myRaiCommonManager.AnagraficaManager.InserisciApiAnnulla(row.ID_STATORAPPORTO.Value);

            }

        }

        private static WebClient GetWebClient(string access_token)
        {
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + access_token);
            wb.Headers.Add("Content-Type", "application/json");
            return wb;
        }

        private static void ReportSWlast()
        {
            var db = new IncentiviEntities();
            var dbtal = new TalentiaEntities();
            DateTime D = new DateTime(2022, 6, 30);
            Dictionary<string, string> Categorie = new Dictionary<string, string>();
            Categorie.Add("1", "Collocazione in regime di smart working unilaterale - lavoratori fragili che non ricadono negli accordi collettivi");
            Categorie.Add("2", "Incremento delle giornate 'da remoto' per i lavoratori fragili che ricadono negli accordi collettivi");
            Categorie.Add("2A", "Collocazione continuativa in prestazione 'da remoto' per i lavoratori fragili che ricadono negli accordi collettivi ad esito della sorveglianza sanitaria eccezionale");
            Categorie.Add("2C", "Comunicazione sull’incremento delle giornate 'da remoto' per i lavoratori fragili che ricadono nella disciplina collettiva, che non abbiano compilato il modulo e che abbiano attivato la sorveglianza sanitaria eccezionale con del giudizio del medico competente che dispone il lavoro “da remoto” in via continuativa.");
            Categorie.Add("3A", "Collocazione in regime di smart working unilaterale per i lavoratori fragili che non ricadono negli accordi collettivi ad esito della sorveglianza sanitaria eccezionale");
            Categorie.Add("4A", "Comunicazione Unilaterale di Smart Working per i lavoratori fragili in base al giudizio del medico competente");
            Categorie.Add("3B", "Collocazione in regime di smart working unilaterale per i lavoratori fragili che non ricadono negli accordi collettivi ad esito della sorveglianza sanitaria eccezionale");
            Categorie.Add("4B", "Comunicazione Unilaterale di Smart Working per i lavoratori fragili in base al giudizio del medico competente");


            var list = dbtal.XR_STATO_RAPPORTO_INFO.Where(x => x.IPOTESI_FRAGILI != null && x.DTA_FINE == D).ToList();
            List<matrFrag> matricole = new List<matrFrag>();
            foreach (var item in list)
            {
                var SR = dbtal.XR_STATO_RAPPORTO.Where(x => x.ID_STATO_RAPPORTO == item.ID_STATO_RAPPORTO).FirstOrDefault();
                if (SR != null && !matricole.Any(x => x.matr == SR.MATRICOLA))
                {
                    matricole.Add(new matrFrag()
                    {
                        matr = SR.MATRICOLA,
                        ipotesiFrag = item.IPOTESI_FRAGILI,
                        SR = SR,
                        SRI = item
                    });
                }
            }
            matricole = matricole.OrderBy(x => x.matr).ToList();

            string file = "WidgetSR_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
            System.IO.File.WriteAllText(file, "MATRICOLA,COGNOME,NOME,SCELTA,QUALIFICA,RUOLO,DIREZIONE,TIPO ACCORDO, DATA FINE, AREA\r\n");
            var sintesi = dbtal.SINTESI1.Select(x => new { x.COD_MATLIBROMAT, x.COD_SERVIZIO, x.DES_QUALIFICA, x.DES_RUOLO, x.DES_SERVIZIO, x.DES_COGNOMEPERS, x.DES_NOMEPERS }).ToList();
            string area = "";

            string[] AccordiFileLines = System.IO.File.ReadAllLines("Accordo_20221011_1438.csv");
            List<string> matricoleAccordi = AccordiFileLines.Select(x => x.Split(',')[0]).ToList();

            foreach (var item in matricole)
            {
                area = "";
                var sint = sintesi.Where(x => x.COD_MATLIBROMAT == item.matr).FirstOrDefault();
                var listd = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).ToList();
                if (listd.Count() > 1)
                {

                }
                if (listd != null)
                {
                    foreach (var d in listd)
                    {
                        area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
                        if (!area.Contains("CHIAVE"))
                        {
                            break;
                        }
                    }
                }
                if (area.Contains("CHIAVE"))
                {

                }
                string descr = "";
                if (item.ipotesiFrag != null)
                {
                    descr = item.ipotesiFrag + " - " + Categorie.Where(x => x.Key == item.ipotesiFrag).Select(x => x.Value).FirstOrDefault();
                    if (descr == null)
                    {

                    }
                }
                AddLine2(sint.DES_COGNOMEPERS, sint.DES_NOMEPERS, item.matr, sint.DES_RUOLO, sint.DES_SERVIZIO, sint.DES_QUALIFICA,
                       matricoleAccordi, file, descr.Replace(",", " "), area, item.SR, item.SRI);
            }
        }
        private static void ReportSW_ModDipendenti()
        {
            var db = new IncentiviEntities();
            var dbtal = new TalentiaEntities();
            Dictionary<string, string> Categorie = new Dictionary<string, string>();
            //aggiunta 9/11/22
            Categorie.Add(":7,", "OPZIONE A - lavoratore in condizione di grave disabilita', accertata ai sensi dell'art. 3, comma 3, L. n. 104/1992");

            Categorie.Add(":8,", "OPZIONE B - lavoratore in possesso di una certificazione, rilasciata dai competenti organi medico – legali della ASL di riferimento, attestante una situazione di rischio derivante da immunodepressione, esiti da patologie oncologiche o dallo svolgimento di relative terapie salvavita");
            Categorie.Add(":400,", "OPZIONE ALFA - lavoratore 'fragile' ai sensi del Decreto interministeriale del 4 febbraio 2022. Esercizio del diritto a rendere la prestazione nella modalita' del lavoro agile");
            Categorie.Add(":500,", "OPZIONE BETA - di essere in possesso di una certificazione, rilasciata dal medico di medicina generale, attestante il ricorrere di una delle patologie e condizioni previste dal Decreto 4 febbraio 2022");

            //aggiunta 9/11/22
            Categorie.Add(":600,", "OPZIONE GAMMA - di essere stato riconosciuto disabile in condizione di gravita' ai sensi dell'art. 3, comma 3, L. n. 104/1992");

            Categorie.Add(":700,", "OPZIONE DELTA - di essere lavoratore immunodepresso o con esiti di patologie oncologiche in possesso di una certificazione di rischiosita' rilasciata dai competenti organi medico-legali delle ASL in data anteriore all'entrata in vigore del Decreto del 4 febbraio 2022");
            Categorie.Add(":800,", "OPZIONE EPSILON - di essere lavoratore in possesso di una certificazione, rilasciata dai competenti servizi organi medico – legali delle ASL, attestante una condizione di rischio derivante da immunodepressione o da esiti da patologie oncologiche o dallo svolgimento di relative terapie salvavita");

            Categorie.Add(":1000,", "OPZIONE ETA - di essere lavoratore in possesso di una certificazione, rilasciata dai competenti servizi medico – legali delle ASL, attestante una condizione di rischio derivante da immunodepressione o da esiti da patologie oncologiche o dallo svolgimento di relative terapie salvavita");




            //var list500 = dbtal.XR_MOD_DIPENDENTI.Where(x => !x.MATRICOLA.Contains("*") && x.SCELTA.Contains(":500")).ToList();//decr
            //var list600 = dbtal.XR_MOD_DIPENDENTI.Where(x => !x.MATRICOLA.Contains("*") && x.SCELTA.Contains(":600")).ToList();//disa
            //var list800 = dbtal.XR_MOD_DIPENDENTI.Where(x => !x.MATRICOLA.Contains("*") && x.SCELTA.Contains(":800")).ToList();//asl
            string file = "Widget_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
            System.IO.File.WriteAllText(file, "MATRICOLA,COGNOME,NOME,SCELTA,QUALIFICA,RUOLO,DIREZIONE,TIPO ACCORDO, DATA FINE, AREA\r\n");

            string[] AccordiFileLines = System.IO.File.ReadAllLines("Accordo_20221011_1438.csv");
            List<string> matricoleAccordi = AccordiFileLines.Select(x => x.Split(',')[0]).ToList();

            var sintesi = dbtal.SINTESI1.Select(x => new
            {
                x.COD_MATLIBROMAT,
                x.COD_SERVIZIO,
                x.DES_QUALIFICA,
                x.DES_RUOLO,
                x.DES_SERVIZIO,
                x.DES_COGNOMEPERS,
                x.DES_NOMEPERS,
                x.COD_TPCNTR
            }).ToList();
            string area = "";


            foreach (var itemCat in Categorie)
            {
                var list = dbtal.XR_MOD_DIPENDENTI.Where(x => !x.MATRICOLA.Contains("*") && x.SCELTA.Contains(itemCat.Key)).ToList();
                foreach (var item in list.OrderBy(x => x.MATRICOLA))
                {
                    area = "";
                    var sint = sintesi.Where(x => x.COD_MATLIBROMAT == item.MATRICOLA).FirstOrDefault();
                    if (sint != null && sint.COD_TPCNTR == "K")//parttime vert
                    {
                        var d = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).FirstOrDefault();
                        if (d != null)
                        {
                            area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
                        }
                        AddLine(sint.DES_COGNOMEPERS, sint.DES_NOMEPERS, item.MATRICOLA, sint.DES_RUOLO, sint.DES_SERVIZIO, sint.DES_QUALIFICA,
                            matricoleAccordi, file, itemCat.Value.Replace(",", " "), area);
                    }

                }
            }
            int b = 10;



            //foreach (var item in list500.OrderBy(x => x.MATRICOLA))
            //{

            //    var sint = sintesi.Where(x => x.COD_MATLIBROMAT == item.MATRICOLA).FirstOrDefault();
            //    var d = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).FirstOrDefault();
            //    if (d != null)
            //    {
            //        area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
            //    }
            //    AddLine(sint.DES_COGNOMEPERS, sint.DES_NOMEPERS, item.MATRICOLA, sint.DES_RUOLO, sint.DES_SERVIZIO, sint.DES_QUALIFICA,
            //        matricoleAccordi, file, "Decreto 4/2/22", area);
            //}
            //foreach (var item in list600.OrderBy(x => x.MATRICOLA))
            //{
            //    area = "";
            //    var sint = sintesi.Where(x => x.COD_MATLIBROMAT == item.MATRICOLA).FirstOrDefault();
            //    var d = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).FirstOrDefault();
            //    if (d != null)
            //    {
            //        area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
            //    }
            //    AddLine(sint.DES_COGNOMEPERS, sint.DES_NOMEPERS, item.MATRICOLA, sint.DES_RUOLO, sint.DES_SERVIZIO, sint.DES_QUALIFICA,
            //        matricoleAccordi, file, "Disabilita' grave", area);
            //}
            //foreach (var item in list800.OrderBy(x => x.MATRICOLA))
            //{
            //    area = "";

            //    var sint = sintesi.Where(x => x.COD_MATLIBROMAT == item.MATRICOLA).FirstOrDefault();
            //    var d = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).FirstOrDefault();
            //    if (d != null)
            //    {
            //        area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
            //    }
            //    AddLine(sint.DES_COGNOMEPERS, sint.DES_NOMEPERS, item.MATRICOLA, sint.DES_RUOLO, sint.DES_SERVIZIO, sint.DES_QUALIFICA,
            //        matricoleAccordi, file, "Rilasciata ASL", area);
            //}

        }
        private static void AddLine2(string cognome, string nome, string matricola, string DES_RUOLO,
           string DES_SERVIZIO, string DES_QUALIFICA, List<string> matricoleAccordi, string file, string tipoFragile, string area,
           XR_STATO_RAPPORTO SR, XR_STATO_RAPPORTO_INFO SRI)
        {

            var dbtal = new TalentiaEntities();
            string ast = "";
            if (matricoleAccordi.Contains(matricola)) ast = "***";

            string m = @"=""" + ast + matricola + @"""";
            string Linea = m + "," + cognome + "," + nome + ",";
            Linea += tipoFragile + ",";

            Linea += DES_QUALIFICA + "," + DES_RUOLO + "," + DES_SERVIZIO + ",";

            Linea += SR.COD_TIPO_ACCORDO + "," + SRI.DTA_FINE.Value.ToString("dd/MM/yyyy") + ",";

            Linea += area + ",";

            System.IO.File.AppendAllText(file, Linea + "\r\n");
        }
        private static void AddLine(string cognome, string nome, string matricola, string DES_RUOLO,
            string DES_SERVIZIO, string DES_QUALIFICA, List<string> matricoleAccordi, string file, string tipoFragile, string area)
        {

            var dbtal = new TalentiaEntities();
            string ast = "";
            // if (matricoleAccordi.Contains(matricola)) ast = "***";

            string m = @"=""" + ast + matricola + @"""";
            string Linea = m + "," + cognome + "," + nome + ",";
            Linea += tipoFragile + ",";

            Linea += DES_QUALIFICA + "," + DES_RUOLO + "," + DES_SERVIZIO + ",";

            var statoRapp = dbtal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == matricola && x.VALID_DTA_END == null)
                            .OrderByDescending(x => x.DTA_FINE)
                            .FirstOrDefault();
            if (statoRapp != null)
            {
                Linea += statoRapp.COD_TIPO_ACCORDO + "," + statoRapp.DTA_FINE.ToString("dd/MM/yyyy") + ",";
            }
            else
                Linea += ",,";

            Linea += area + ",";

            System.IO.File.AppendAllText(file, Linea + "\r\n");
        }
        private static void ReportSW()
        {
            var db = new IncentiviEntities();
            var dbtal = new TalentiaEntities();

            int[] categorie = new int[] { 53, 72, 74, 93, 100, 101, 103 };
            var matricoleAccordoRichieste = db.XR_MAT_RICHIESTE.Where(x =>
            categorie.Contains(x.CATEGORIA) && x.MATRICOLA.Length == 6).OrderBy(x => x.MATRICOLA).ToList();

            var sintesi = db.SINTESI1.Select(x => new { x.COD_MATLIBROMAT, x.DES_QUALIFICA, x.COD_SERVIZIO, x.DES_RUOLO, x.DES_SERVIZIO, x.DES_COGNOMEPERS, x.DES_NOMEPERS }).ToList();

            string file = "Accordo_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

            System.IO.File.WriteAllText(file, "MATRICOLA,COGNOME,NOME,SCELTA,QUALIFICA,RUOLO,DIREZIONE,TIPO ACCORDO, DATA FINE,AREA\r\n");

            foreach (var rich in matricoleAccordoRichieste)
            {
                var sint = sintesi.Where(x => x.COD_MATLIBROMAT == rich.MATRICOLA).FirstOrDefault();

                string m = @"=""" + rich.MATRICOLA + @"""";
                string Linea = m + "," + sint.DES_COGNOMEPERS.Trim() + "," + sint.DES_NOMEPERS.Trim() + ",";

                string scelta = "";
                switch (rich.CATEGORIA)
                {
                    case 53:
                    case 93:
                    case 103:
                        scelta = "Disabilita' grave";
                        break;
                    case 72:
                    case 100:
                        scelta = "Decreto 4/2/22";
                        break;
                    case 74:
                    case 101:
                        scelta = "Rilasciata ASL";
                        break;
                }
                Linea += scelta + ",";

                if (sint != null)
                    Linea += sint.DES_QUALIFICA + "," + sint.DES_RUOLO + "," + sint.DES_SERVIZIO + ",";
                else
                    Linea += ",,,";

                var statoRapp = dbtal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == rich.MATRICOLA && x.VALID_DTA_END == null)
                                .OrderByDescending(x => x.DTA_FINE)
                                .FirstOrDefault();
                if (statoRapp != null)
                {
                    Linea += statoRapp.COD_TIPO_ACCORDO + "," + statoRapp.DTA_FINE.ToString("dd/MM/yyyy");
                }
                else
                    Linea += ",";
                string area = "";


                var d = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == sint.COD_SERVIZIO).FirstOrDefault();
                if (d != null)
                {
                    area = db.XR_PRV_AREA.Where(x => x.ID_AREA == d.ID_AREA).Select(x => x.NOME).FirstOrDefault();
                }
                Linea += "," + area;

                System.IO.File.AppendAllText(file, Linea + "\r\n");

            }
            /*LavoratoreDisabile
LavoratoreConCertificazione
ConFiglioDAD#05/03/2021-31/07/2021
Genitore
LavoratoreDisabile
FamiliareDisabile
ConFiglioDAD#06/11/2020-03/12/2020
LavoratoreImmunodepresso*/
            string[] scelte = new string[] { "LavoratoreDisabile", "LavoratoreImmunodepresso", "LavoratoreConCertificazione" };
            var matricoleWidget = dbtal.XR_MOD_DIPENDENTI.Where(x => x.COD_MODULO == "SMARTW2020" && scelte.Contains(x.SCELTA))
                .OrderBy(x => x.MATRICOLA).ToList();
        }

        /// <summary>
        /// Il metodo seguente si occupa di aggiornare la direzione di appartenenza dei dipendenti che fanno
        /// parte di una campagna di politiche retributive che risponde ai filtri iniziali
        /// </summary>
        private static void AggiornaDirezioniPoliticheRetributive()
        {
            const string NOMECARTELLAFILE = "AggiornaDirezioniPoliticheRetributive";
            const string DIPENDENTIMODIFICATI = "DIPENDENTIMODIFICATI";
            string tx = "Avvio AggiornaDirezioniPoliticheRetributive";
            Output.WriteLine(tx);
            ScriviFile(tx, NOMECARTELLAFILE);
            ScriviFile("ID_DIPENDENTE;MATRICOLA;ID_PERSONA;OLD_DIR;NEW_DIR;DESC_NUOVA_DIR", "LOG_DIPENDENTI_MODIFICATI");
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                // prima di tutto prendiamo tutti i piani attivi
                List<XR_PRV_CAMPAGNA> piani = new List<XR_PRV_CAMPAGNA>();
                DateTime dtaInizio = new DateTime(2022, 5, 1, 0, 0, 0);
                DateTime dtaFine = new DateTime(2022, 12, 31, 0, 0, 0);

                Output.WriteLine("Recupero piani attivi");
                piani = db.XR_PRV_CAMPAGNA.Where(w => w.DTA_FINE != null &&
                                                w.DTA_FINE >= dtaFine &&
                                                w.DTA_INIZIO >= dtaInizio).ToList();

                if (piani == null || !piani.Any())
                {
                    throw new Exception("Nessun piano attivo trovato");
                }

                Output.WriteLine("Trovati " + piani.Count() + " piani attivi");

                foreach (var c in piani)
                {
                    int dipModificati = 0;

                    try
                    {
                        tx = "Analisi campagna " + c.NOME + " ID_CAMPAGNA:" + c.ID_CAMPAGNA.ToString();
                        Output.WriteLine(tx);
                        ScriviFile(tx, NOMECARTELLAFILE);

                        // recupero dei dipendenti che sono stati inseriti nella campagna in esame
                        List<XR_PRV_DIPENDENTI> dip = new List<XR_PRV_DIPENDENTI>();

                        tx = "Recupero dei dipendenti che sono stati inseriti nella campagna in esame";
                        Output.WriteLine(tx);

                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA == c.ID_CAMPAGNA).ToList();

                        if (dip == null || !dip.Any())
                        {
                            throw new Exception("Nessun dipendente trovato nella campagna " + c.NOME + " ID_CAMPAGNA:" + c.ID_CAMPAGNA.ToString());
                        }

                        // per ogni dipendente deve controllare se la direzione è cambiata
                        foreach (var d in dip)
                        {
                            try
                            {
                                // recupero il codice servizio di appartenenza del dipendente in esame
                                myRaiData.Incentivi.SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == d.ID_PERSONA);
                                string codServ = sint.XR_SERVIZIO.OrderByDescending(x => x.DTA_FINE).FirstOrDefault().COD_SERVIZIO.Substring(0, 2);

                                //if (codServ != "1A")
                                //    continue;

                                if (String.IsNullOrEmpty(codServ))
                                    throw new Exception("Codice servizio non trovato ");

                                // sovrascrittura del codice servizio
                                var param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
                                if (param != null && param.OvverideDirReq != null && param.OvverideDirReq.Any())
                                {
                                    if (CommonHelper.IsProduzione())
                                    {
                                        var rule = param.OvverideDirReq.FirstOrDefault(x => x.Corrisponde(codServ, sint.COD_UNITAORG));
                                        if (rule != null)
                                            codServ = rule.DirDest;
                                    }
                                }
                                XR_PRV_DIREZIONE vecchiaDirezione = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE == d.ID_DIREZIONE);

                                //XR_PRV_DIREZIONE dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.CODICE == codServ && x.ID_AREA == vecchiaDirezione.ID_AREA);
                                XR_PRV_DIREZIONE dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.XR_PRV_AREA.LV_ABIL.Contains(c.LV_ABIL) && x.CODICE == codServ);
                                if (dir != null)
                                {
                                    if (d.ID_DIREZIONE != dir.ID_DIREZIONE)
                                    {

                                        string bk = String.Format("DIPENDENTE:{0}-{1}-{2}, VECCHIA DIREZIONE: {3}-{4} , NUOVA DIREZIONE: {5}-{6}", sint.Nominativo().Trim(), d.MATRICOLA, d.ID_DIPENDENTE, vecchiaDirezione.ID_DIREZIONE, vecchiaDirezione.NOME, dir.ID_DIREZIONE, dir.NOME);
                                        ScriviFile(bk, DIPENDENTIMODIFICATI);

                                        tx = String.Format("{0};{1};{2};{3};{4};{5}", d.ID_DIPENDENTE, d.MATRICOLA, d.ID_PERSONA, d.ID_DIREZIONE, dir.ID_DIREZIONE, dir.NOME);
                                        Output.WriteLine(tx);
                                        ScriviFile(tx, "LOG_DIPENDENTI_MODIFICATI");
                                        d.ID_DIREZIONE = dir.ID_DIREZIONE;
                                        CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
                                        d.COD_USER = campiFirma.CodUser;
                                        d.COD_TERMID = campiFirma.CodTermid;
                                        d.TMS_TIMESTAMP = campiFirma.Timestamp;
                                        db.SaveChanges();
                                        dipModificati++;
                                    }
                                }
                                else
                                {
                                    throw new Exception("Direzione " + codServ + " non trovata");
                                }
                            }
                            catch (Exception ex)
                            {
                                tx = "Errore ID_DIPENDENTE: " + d.ID_DIPENDENTE + " matricola: " + d.MATRICOLA + " ID_PERSONA: " + d.ID_PERSONA.ToString() + " - " + ex.Message;
                                Output.WriteLine(tx);
                                ScriviFile(tx, NOMECARTELLAFILE);
                            }
                        }

                        tx = "Piano: " + c.NOME + " ID_CAMPAGNA: " + c.ID_CAMPAGNA + " - modificati " + dipModificati + " dipendenti";
                        Output.WriteLine(tx);
                        ScriviFile(tx, NOMECARTELLAFILE);

                    }
                    catch (Exception ex)
                    {
                        tx = "Errore per il piano: " + c.NOME + " ID_CAMPAGNA: " + c.ID_CAMPAGNA + " - " + ex.Message;
                        Output.WriteLine(tx);
                        ScriviFile(tx, NOMECARTELLAFILE);
                    }
                }
            }
            catch (Exception e)
            {
                tx = "Errore " + e.Message;
                Output.WriteLine(tx);
                ScriviFile(tx, NOMECARTELLAFILE);
            }

            tx = "Fine AggiornaDirezioniPoliticheRetributive";
            Output.WriteLine(tx);
            ScriviFile(tx, NOMECARTELLAFILE);
        }

        private static void AggiornaFileEsodi()
        {
            Console.WriteLine("Avvio AggiornaFileEsodi");
            ScriviFile("Avvio AggiornaFileEsodi");

            try
            {
                byte[] output = null;
                string pathFile = @"C:\\RAI\\CaricamentoEsodi\\Esodi 2022 - Accettazione proposta Campanari Gianfranco.pdf";

                output = System.IO.File.ReadAllBytes(pathFile);

                var db = new IncentiviEntities();
                var dip = db.XR_INC_DIPENDENTI.Where(w => w.ID_DIPENDENTE.Equals(428858735)).FirstOrDefault();
                if (dip == null)
                {
                    throw new Exception("Utente non trovato");
                }

                Console.WriteLine(dip.MATRICOLA);
                ScriviFile(dip.MATRICOLA);

                var accTemplate = CessazioneHelper.GetTemplate(db, "Traccia_996", 0, "Accettazione", false);
                IncentivazioneFile fileInfo = new IncentivazioneFile()
                {
                    IdDipendente = dip.ID_DIPENDENTE,
                    Caricato = true,
                    Template = accTemplate.ID_TEMPLATE,
                    Chiave = INCFileManager.GeneraChiave(dip.ID_DIPENDENTE, true, (int)IncStato.TempFileAccettazione, accTemplate.ID_TEMPLATE),
                    FileName = "Esodi 2022 - Accettazione proposta Campanari Gianfranco.pdf",
                    Length = output.Length,
                    Modulo = dip.COD_GRUPPO,
                    Approvato = true,
                    Titolo = "Accettazione",
                    Descrizione = "Accettazione proposta",
                    ReadOnly = true,
                    Tag = "Accettazione",
                    Stato = (int)IncStato.RichiestaAccettata
                };
                FileManager.UploadFile(dip.MATRICOLA, "INC", "Accettazione firmata.pdf", output, fileInfo.Chiave, null, Newtonsoft.Json.JsonConvert.SerializeObject(fileInfo));
            }
            catch (Exception ex)
            {
                ScriviFile(ex.Message);
            }

            Console.WriteLine("Avvio AggiornaFileEsodi");
            ScriviFile("Avvio AggiornaFileEsodi");
        }

        private static void TestHRISFileManager()
        {
            // test inserimento file

            //byte[] mioFile = null;
            //string path = @"C:\RAI\RaiPolRetr\Output\Confluenze\013995_Alimonti Lucilla.pdf";
            //string fileName = Path.GetFileName(path);
            //mioFile = File.ReadAllBytes(path);

            //HRISFileResult result = FileAssunzioneManager.UploadFile(4725961, "103650", fileName, mioFile);

            HRISFileResult result = FileAssunzioneManager.GetFile(1);
            HRISFileResult result2 = FileAssunzioneManager.GetFileByChiave("ASS_f4365823-3b51-4233-9ded-55c9abff761b");
            HRISFileResult result3 = FileAssunzioneManager.GetFilesByMatricola("103650");
            HRISFileResult result4 = FileAssunzioneManager.GetFilesByTipologia();
        }

        private static void TestLoadEdmx()
        {
            //var dbTal = new myRaiDataTalentia.TalentiaEntities();
            //var sintTal = dbTal.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == "103650");
            //sintTal = dbTal.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == "909317");

            //var dbCzn = new IncentiviEntities();
            //var sintCzn = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == "103650");
            //sintCzn = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == "909317");

            var dbCzn = new IncentiviEntities();
            int count = dbCzn.XR_PRV_LOG.Count();
            dbCzn.XR_PRV_LOG.RemoveWhere(x => x.ID_LOG < 30);
            int countN = dbCzn.XR_PRV_LOG.Count();
            dbCzn.SaveChanges();

        }

        private static void RendiFormAnonimi()
        {
            var db = new digiGappEntities();
            MyRai_FormPrimario questionario = db.MyRai_FormPrimario.Find(46);
            var tmp = questionario.MyRai_FormCompletati.Select(x => x.matricola).ToList();

            Dictionary<string, string> dict = tmp.ToDictionary(x => x, x => Guid.NewGuid().ToString());

            foreach (var item in dict)
            {
                _log.Info(item.Key + " - " + item.Value);
            }

            foreach (var sec in questionario.MyRai_FormSecondario.OrderBy(x => x.progressivo))
            {
                foreach (var dom in sec.MyRai_FormDomande.Where(x => x.id_domanda_parent == null).OrderBy(x => x.progressivo))
                {
                    EnumTipologiaDomanda tipo = (EnumTipologiaDomanda)dom.id_tipologia;
                    if (tipo == EnumTipologiaDomanda.MasterPerMatrixRating
                        || tipo == EnumTipologiaDomanda.MasterPerMatrixRatingNoLabel)
                    {
                        foreach (var subDom in dom.MyRai_FormDomande1)
                        {
                            _log.Info("Subdomanda " + dom.id);
                            foreach (var risp in dom.MyRai_FormRisposteDate)
                            {
                                if (dict.TryGetValue(risp.matricola, out string guid))
                                    risp.matricola = guid;
                            }
                        }
                    }
                    else
                    {
                        _log.Info("Domanda " + dom.id);
                        foreach (var risp in dom.MyRai_FormRisposteDate)
                        {
                            if (dict.TryGetValue(risp.matricola, out string guid))
                                risp.matricola = guid;
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        //private static void ApplicaDomicilio()
        //{
        //    _log.Info("***Inizio esecuzione");
        //    SessionHelper.Set("GetCurrentUsername", "RAI\\ADMIN");

        //    DateTime min = DateTime.Today.AddDays(-1);

        //    var db = AnagraficaManager.GetDbIban();
        //    var elenco = db.CMINFOANAG_EXT
        //                    .Where(x => x.COD_CMEVENTO == "MODDOM" && x.DTA_CONVALIDA != null && x.DTA_APPLICAZIONE == null);//.ToList();

        //    var elencoPers = elenco.GroupBy(x => x.ID_PERSONA);
        //    int countSuccess = 0;
        //    int countAll = elencoPers.Count();

        //    _log.Info(String.Format("Trovate {0} richiesta/e di {1} persona/e", elenco.Count(), countAll));

        //    int lastIdEvento = 0;

        //    try
        //    {
        //        foreach (var grRich in elencoPers)
        //        {
        //            var rich = grRich
        //                        .OrderByDescending(x => x.DTA_CONVALIDA)
        //                        .First();

        //            lastIdEvento = rich.ID_EVENTO;

        //            ANAGPERS anagPers = db.ANAGPERS.Find(rich.ID_PERSONA);
        //            SINTESI1 sint = db.SINTESI1.Find(rich.ID_PERSONA);
        //            //CMINFOANAG_EXT info = db.CMINFOANAG_EXT.Find(rich.ID_EVENTO);

        //            if (anagPers != null)
        //            {
        //                anagPers.COD_CITTADOM = rich.COD_CITTADOM;
        //                anagPers.CAP_CAPDOM = rich.CAP_CAPDOM;
        //                anagPers.DES_INDIRDOM = rich.DES_INDIRDOM.ToUpper();

        //                SessionHelper.Set("GetCurrentUsername", "RAI\\P"+sint.COD_MATLIBROMAT);
        //                rich.IND_CICS_SENT = AnagraficaManager.Tracciato_Domicilio(sint.COD_MATLIBROMAT, TipoAnaVar.Domicilio, anagPers, null, rich.ID_EVENTO, rich.TMS_TIMESTAMP.Value);
        //                rich.DTA_APPLICAZIONE = DateTime.Now;

        //                int secDiff = 0;
        //                foreach (var subRich in grRich.OrderByDescending(x => x.DTA_CONVALIDA).Skip(1))
        //                {
        //                    secDiff++;
        //                    subRich.DTA_APPLICAZIONE = rich.DTA_APPLICAZIONE.Value.AddMilliseconds(-secDiff);
        //                }

        //                if (DBHelper.Save(db, "ADMIN"))
        //                {
        //                    _log.Info(String.Format("Cambio domicilio effettuato: ID_EVENTO: {0} - ID_PERSONA: {1}", rich.ID_EVENTO, rich.ID_PERSONA));
        //                    countSuccess++;
        //                }
        //                else
        //                    _log.Error(String.Format("Salvataggio non riuscito: ID_EVENTO: {0} - ID_PERSONA: {1} - verificare log DB", rich.ID_EVENTO, rich.ID_PERSONA));
        //            }
        //            else
        //            {
        //                _log.Error(String.Format("Anagrafica non trovata: ID_EVENTO: {0} - ID_PERSONA: {1}", rich.ID_EVENTO, rich.ID_PERSONA));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Fatal(String.Format("Errore sulla richiesta {0}", lastIdEvento), ex);
        //    }



        //    if (countSuccess!=countAll)
        //    {
        //        _log.Warn(String.Format("Applicate {0} su {1} richieste di modifiche", countSuccess, countAll));
        //    }
        //    else
        //    {
        //        _log.Info(String.Format("Applicate {0} su {1} richieste di modifiche", countSuccess, countAll));
        //    }

        //    _log.Info("***Fine esecuzione");
        //}

        private static void EsportaConteggioFerie()
        {
            List<ArretratiExcel2019Ext> lista = new List<ArretratiExcel2019Ext>();
            string linea = "";
            try
            {

                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1])
                };

                using (digiGappEntities db = new digiGappEntities())
                {
                    var dipendenti = db.MyRai_ArretratiExcel2019.Where(w => w.fruite_11_31 > 0).ToList();

                    linea = String.Format("MATRICOLA | NOMINATIVO | SEDEGAPP | FERIE RIMANENTI | FERIE MINIME | DA FARE | FRUITE 11 31 | RICHIESTE DA PIANO FERIE | EFFETTIVAMENTE DA FARE | FE ARRETRATI | RR ARRETRATI | RF ARRETRATI | FECE MRCE MNCE GAPP | GIA' APPROVATO | ESITO");
                    Output.WriteLine(linea);

                    DateTime dataLimite = new DateTime(2020, 5, 15, 0, 0, 0);
                    foreach (var dip in dipendenti)
                    {
                        bool exists = db.MyRai_PianoFerie.Where(w => w.matricola.Equals(dip.matricola) && (w.anno == DateTime.Now.Year)
                        && (w.data_consolidato != null && w.data_consolidato.Value < dataLimite)).Count() > 0;

                        if (exists)
                        {
                            var dato = db.MyRai_PianoFerie.Where(w => w.matricola.Equals(dip.matricola) && (w.anno == DateTime.Now.Year) && (w.data_consolidato != null && w.data_consolidato.Value < dataLimite)).FirstOrDefault();
                            int conteggioGiorni = db.MyRai_PianoFerieGiorni.Where(w => w.matricola.Equals(dip.matricola) && w.data_inserimento.Year == DateTime.Now.Year).Count();

                            ArretratiExcel2019Ext newItem = new ArretratiExcel2019Ext()
                            {
                                Categoria = dip.categoria,
                                DaFare = dip.da_fare.GetValueOrDefault(),
                                Fruite = dip.fruite_11_31.GetValueOrDefault(),
                                Matricola = dip.matricola,
                                Nominativo = dip.nominativo,
                                RapportoLavoro = dip.rapporto_lavoro,
                                Richiste = conteggioGiorni,
                                ServizioContabile = dip.servizio_contabile,
                                GiaApprovato = dato.data_approvato.HasValue,
                                SedeGapp = dato.sedegapp
                            };

                            var pianoFerieResult = service.getPianoFerie(dip.matricola, "01012020", 75, "");

                            if (pianoFerieResult != null && pianoFerieResult.esito)
                            {
                                try
                                {
                                    newItem.FerieRimanenti = pianoFerieResult.dipendente.ferie.ferieRimanenti;
                                    newItem.FerieMinime = pianoFerieResult.dipendente.ferie.ferieMinime;

                                    if (pianoFerieResult.dipendente.Accordi != null)
                                    {
                                        newItem.EffettivamenteDaFare = pianoFerieResult.dipendente.Accordi.EffettivamenteDaFare;
                                        newItem.FEArretrati = pianoFerieResult.dipendente.Accordi.FE_ArretratiDaInserire;
                                        newItem.RFArretrati = pianoFerieResult.dipendente.Accordi.RF_ArretratiDaInserire;
                                        newItem.RRArretrati = pianoFerieResult.dipendente.Accordi.RR_ArretratiDaInserire;
                                        newItem.FECE_MRCE_MNCE_Gapp = pianoFerieResult.dipendente.Accordi.FECE_MRCE_MNCE_gapp;
                                    }

                                    linea = String.Format("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} | {13} | OK", newItem.Matricola, newItem.Nominativo, newItem.SedeGapp, newItem.FerieRimanenti, newItem.FerieMinime, newItem.DaFare, newItem.Fruite, newItem.Richiste, newItem.EffettivamenteDaFare, newItem.FEArretrati, newItem.RRArretrati, newItem.RFArretrati, newItem.FECE_MRCE_MNCE_Gapp, newItem.GiaApprovato);
                                    Output.WriteLine(linea);
                                }
                                catch (Exception ex)
                                {
                                    linea = String.Format("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} | {13} | {14}", newItem.Matricola, newItem.Nominativo, newItem.SedeGapp, newItem.FerieRimanenti, newItem.FerieMinime, newItem.DaFare, newItem.Fruite, newItem.Richiste, newItem.EffettivamenteDaFare, newItem.FEArretrati, newItem.RRArretrati, newItem.RFArretrati, newItem.FECE_MRCE_MNCE_Gapp, newItem.GiaApprovato, ex.Message);
                                    Output.WriteLine(linea);
                                }
                            }
                            else
                            {
                                linea = String.Format("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} | {13} | {14}", newItem.Matricola, newItem.Nominativo, newItem.SedeGapp, newItem.FerieRimanenti, newItem.FerieMinime, newItem.DaFare, newItem.Fruite, newItem.Richiste, newItem.EffettivamenteDaFare, newItem.FEArretrati, newItem.RRArretrati, newItem.RFArretrati, newItem.FECE_MRCE_MNCE_Gapp, newItem.GiaApprovato, pianoFerieResult.errore);
                                Output.WriteLine(linea);
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Metodo per copiare i dati di una o più eccezioni presenti su HRDW nel db di digigapp.
        /// Se l'eccezione è già presente in DIGIGAPP, i records ad essa associati verranno eliminati
        /// e ricreati coi dati di HRDW
        /// </summary>
        public static void CopiaEccezioniHRDWToDIGIGAPPDB()
        {
            Console.WriteLine("Inizio allineamento eccezioni da HRDW a DIGIGAPP");
            List<string> elencoEccezioni = new List<string>();
            elencoEccezioni.Add("DE");
            int inseriti = 0;
            int cancellati = 0;
            try
            {
                foreach (var item in elencoEccezioni)
                {
                    #region cancellazione possibili duplicati da DIGIGAPP

                    Console.WriteLine("cancellazione possibili duplicati da DIGIGAPP");
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        string q1 = "DELETE FROM [digiGapp].[dbo].[L2D_ECCEZIONE] WHERE cod_eccezione = '#ECCEZIONE#'";
                        q1 = q1.Replace("#ECCEZIONE#", item);

                        cancellati = db.Database.ExecuteSqlCommand(q1);
                    }
                    #endregion

                    #region Copia dati eccezione da HRDW a Digigapp db
                    Console.WriteLine("Copia dati eccezione da HRDW a Digigapp db");
                    using (var hrdw = new PERSEOEntities())
                    {
                        string query = "SELECT [sky_eccezione] " +
                                        " ,[cod_eccezione] " +
                                        " ,[data_inizio_validita] " +
                                        " ,[data_fine_validita] " +
                                        " ,[desc_eccezione] " +
                                        " ,[unita_misura] " +
                                        " ,[flag_macroassen] " +
                                        " ,[flag_eccez] " +
                                        " ,[cod_eccez_padre] " +
                                        " ,[desc_cod_eccez_padre] " +
                                        " ,[data_inizio_val] " +
                                        " ,[Data_Ins] " +
                                        " ,[Data_Fine_Val] " +
                                        " ,[Data_Agg] " +
                                        " ,[Data_Elim] " +
                                        " ,[cod_cedo] " +
                                        " ,[flag_costo_hrdw] " +
                                        " ,[des_cod_cedo] " +
                                        " ,[cod_aggregato] " +
                                        " ,[descr_aggregato] " +
                                        "  FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] " +
                                        " where cod_eccezione = '#ECCEZIONE#'";

                        query = query.Replace("#ECCEZIONE#", item);

                        L2D_ECCEZIONE eccezione = hrdw.Database.SqlQuery<L2D_ECCEZIONE>(query).FirstOrDefault();

                        if (eccezione != null)
                        {
                            using (digiGappEntities db = new digiGappEntities())
                            {
                                String insertQuery = "INSERT INTO [dbo].[L2D_ECCEZIONE] " +
                                                    " ([sky_eccezione] " +
                                                    " ,[cod_eccezione] " +
                                                    " ,[data_inizio_validita] " +
                                                    " ,[data_fine_validita] " +
                                                    " ,[desc_eccezione] " +
                                                    " ,[unita_misura] " +
                                                    " ,[flag_macroassen] " +
                                                    " ,[flag_eccez] " +
                                                    " ,[cod_eccez_padre] " +
                                                    " ,[desc_cod_eccez_padre] " +
                                                    " ,[data_inizio_val] " +
                                                    " ,[Data_Ins] " +
                                                    " ,[Data_Fine_Val] " +
                                                    " ,[Data_Agg] " +
                                                    " ,[Data_Elim] " +
                                                    " ,[cod_cedo] " +
                                                    " ,[flag_costo_hrdw] " +
                                                    " ,[des_cod_cedo] " +
                                                    " ,[cod_aggregato] " +
                                                    " ,[descr_aggregato]) " +
                                                    " VALUES " +
                                                    " ( @sky_eccezione " +
                                                    " , @cod_eccezione " +
                                                    " , @data_inizio_validita " +
                                                    " , @data_fine_validita " +
                                                    " , @desc_eccezione " +
                                                    " , @unita_misura " +
                                                    " , @flag_macroassen " +
                                                    " , @flag_eccez " +
                                                    " , @cod_eccez_padre " +
                                                    " , @desc_cod_eccez_padre " +
                                                    " , @data_inizio_val " +
                                                    " , @Data_Ins " +
                                                    " , @Data_Fine_Val " +
                                                    " , @Data_Agg " +
                                                    " , @Data_Elim " +
                                                    " , @cod_cedo " +
                                                    " , @flag_costo_hrdw " +
                                                    " , @des_cod_cedo " +
                                                    " , @cod_aggregato " +
                                                    " , @descr_aggregato ) ";

                                insertQuery = insertQuery.Replace("@sky_eccezione", eccezione.sky_eccezione.ToString());
                                insertQuery = insertQuery.Replace("@cod_eccezione", String.IsNullOrEmpty(eccezione.cod_eccezione) ? "NULL" : "'" + eccezione.cod_eccezione + "'");
                                insertQuery = insertQuery.Replace("@data_inizio_validita", !eccezione.data_inizio_validita.HasValue ? "NULL" : "'" + eccezione.data_inizio_validita.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                                insertQuery = insertQuery.Replace("@data_fine_validita", !eccezione.data_fine_validita.HasValue ? "NULL" : "'" + eccezione.data_fine_validita.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                                insertQuery = insertQuery.Replace("@desc_eccezione", String.IsNullOrEmpty(eccezione.desc_eccezione) ? "NULL" : "'" + eccezione.desc_eccezione + "'");
                                insertQuery = insertQuery.Replace("@unita_misura", String.IsNullOrEmpty(eccezione.unita_misura) ? "NULL" : "'" + eccezione.unita_misura + "'");
                                insertQuery = insertQuery.Replace("@flag_macroassen", String.IsNullOrEmpty(eccezione.flag_macroassen) ? "NULL" : "'" + eccezione.flag_macroassen + "'");
                                insertQuery = insertQuery.Replace("@flag_eccez", String.IsNullOrEmpty(eccezione.flag_eccez) ? "NULL" : "'" + eccezione.flag_eccez + "'");
                                insertQuery = insertQuery.Replace("@cod_eccez_padre", String.IsNullOrEmpty(eccezione.cod_eccez_padre) ? "NULL" : "'" + eccezione.cod_eccez_padre + "'");
                                insertQuery = insertQuery.Replace("@desc_cod_eccez_padre", String.IsNullOrEmpty(eccezione.desc_cod_eccez_padre) ? "NULL" : "'" + eccezione.desc_cod_eccez_padre + "'");

                                insertQuery = insertQuery.Replace("@data_inizio_val", !eccezione.data_inizio_val.HasValue ? "NULL" : "'" + eccezione.data_inizio_val.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                                insertQuery = insertQuery.Replace("@Data_Ins", "'" + eccezione.Data_Ins.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                                insertQuery = insertQuery.Replace("@Data_Fine_Val", !eccezione.Data_Fine_Val.HasValue ? "NULL" : "'" + eccezione.Data_Fine_Val.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                                insertQuery = insertQuery.Replace("@Data_Agg", !eccezione.Data_Agg.HasValue ? "NULL" : "'" + eccezione.Data_Agg.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                                insertQuery = insertQuery.Replace("@Data_Elim", !eccezione.Data_Elim.HasValue ? "NULL" : "'" + eccezione.Data_Elim.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                                insertQuery = insertQuery.Replace("@cod_cedo", String.IsNullOrEmpty(eccezione.cod_cedo) ? "NULL" : "'" + eccezione.cod_cedo + "'");
                                insertQuery = insertQuery.Replace("@flag_costo_hrdw", String.IsNullOrEmpty(eccezione.flag_costo_hrdw) ? "NULL" : "'" + eccezione.flag_costo_hrdw + "'");
                                insertQuery = insertQuery.Replace("@des_cod_cedo", String.IsNullOrEmpty(eccezione.des_cod_cedo) ? "NULL" : "'" + eccezione.des_cod_cedo + "'");
                                insertQuery = insertQuery.Replace("@cod_aggregato", String.IsNullOrEmpty(eccezione.cod_aggregato) ? "NULL" : "'" + eccezione.cod_aggregato + "'");
                                insertQuery = insertQuery.Replace("@descr_aggregato", String.IsNullOrEmpty(eccezione.descr_aggregato) ? "NULL" : "'" + eccezione.descr_aggregato + "'");

                                inseriti = db.Database.ExecuteSqlCommand(insertQuery);
                            }
                        }
                    }
                    #endregion
                }

                Console.WriteLine("Eliminati " + cancellati + " records dal db digigapp");
                Console.WriteLine("Inserite " + inseriti + " eccezioni nel db digigapp");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Fine allineamento eccezioni da HRDW a DIGIGAPP");
        }

        /// <summary>
        /// Metodo che legge il file carburanti.csv formato da targa;tipologia carburante e inserisce tali dati
        /// nella tabella T_SkCarburantiAlim su HRPADB
        /// </summary>
        public static void CaricaTipologiaCarburante()
        {
            Console.WriteLine("Avvio CaricaTipologiaCarburante");
            ScriviFile("Avvio CaricaTipologiaCarburante");
            string line = "";
            int i = 1;
            int trovati = 0;
            int aggiunti = 0;
            int aggiornati = 0;

            Console.WriteLine("Lettura file");

            try
            {
                StreamReader file = new StreamReader(@"D:\carburanti.csv");

                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine("Riga " + i);
                    string riga = line;
                    List<string> rigaSplittata = riga.Split(';').ToList();

                    using (HRPADBEntities db = new HRPADBEntities())
                    {
                        // legge dati
                        string targa = rigaSplittata[0];
                        string tipoCarburante = rigaSplittata[1];

                        string f = String.Format("verifica esistenza della targa {0} sul db", targa);
                        Console.WriteLine(f);
                        ScriviFile(f);

                        // verifica esistenza della targa sul db
                        int exists = db.T_SkCarburantiAlim.Count(w => w.Targa_SkCarburantiAlim.Equals(targa) && w.Alimentazione_SkCarburantiAlim.Equals(tipoCarburante));

                        // se esiste non fa nulla perchè già presente
                        if (exists > 0)
                        {
                            trovati++;
                            f = String.Format("targa {0} presente sul db", targa);
                            Console.WriteLine(f);
                            ScriviFile(f);
                        }
                        else
                        {
                            // se non lo trova verifica se esite già la targa ma non c'è l'alimentazione
                            exists = db.T_SkCarburantiAlim.Count(w => w.Targa_SkCarburantiAlim.Equals(targa));
                            T_SkCarburantiAlim item = new T_SkCarburantiAlim();

                            // se esiste deve aggiornare il record
                            if (exists > 0)
                            {
                                aggiornati++;
                                f = String.Format("targa {0} presente sul db, ma senza alimentazione", targa);
                                Console.WriteLine(f);
                                ScriviFile(f);

                                item = db.T_SkCarburantiAlim.Where(w => w.Targa_SkCarburantiAlim.Equals(targa)).FirstOrDefault();
                                item.Alimentazione_SkCarburantiAlim = tipoCarburante;
                            }
                            else
                            {
                                // altrimenti lo deve inserire
                                f = String.Format("targa {0} non presente sul db", targa);
                                Console.WriteLine(f);
                                ScriviFile(f);
                                aggiunti++;
                                item.Targa_SkCarburantiAlim = targa;
                                item.Alimentazione_SkCarburantiAlim = tipoCarburante;
                                db.T_SkCarburantiAlim.Add(item);
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore, " + ex.ToString());
                ScriviFile("Errore, " + ex.ToString());
            }
            Console.WriteLine("Trovati " + trovati);
            Console.WriteLine("Aggiunti " + aggiunti);
            Console.WriteLine("Aggiornati " + aggiornati);
            Console.WriteLine("Terminato, premi un tasto");
            ScriviFile("Fine CaricaTipologiaCarburante");
        }

        public static void UpdateInfoDipendente()
        {
            Console.WriteLine("Avvio UpdateInfoDipendente");
            ScriviFile("Avvio UpdateInfoDipendente");
            string line = "";
            DateTime inizio = new DateTime(2019, 12, 01);
            DateTime fine = new DateTime(2020, 01, 31);
            DateTime dataPrecedente = new DateTime(2019, 12, 31);
            int i = 1;
            int trovati = 0;
            int aggiunti = 0;
            int giaelaborati = 0;

            Console.WriteLine("Lettura file");

            try
            {
                StreamReader file = new StreamReader(@"D:\updateturnisti.txt");

                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine("Riga " + i);
                    string riga = line;
                    List<string> rigaSplittata = riga.Split(',').ToList();

                    using (var db = new digiGappEntities())
                    {
                        string matricola = rigaSplittata[0];

                        int exists = db.MyRai_InfoDipendente.Count(w => w.matricola.Equals(matricola) &&
                                                                    EntityFunctions.TruncateTime(dataPrecedente) == EntityFunctions.TruncateTime(w.data_fine));

                        if (exists > 0)
                        {
                            var toUpdateList = db.MyRai_InfoDipendente.Where(w => w.matricola.Equals(matricola) &&
                                                                    EntityFunctions.TruncateTime(dataPrecedente) == EntityFunctions.TruncateTime(w.data_fine)).ToList();

                            if (toUpdateList != null && toUpdateList.Any())
                            {
                                trovati++;
                                foreach (var aggiorna in toUpdateList)
                                {
                                    aggiorna.data_fine = fine;
                                    db.SaveChanges();
                                    string f = String.Format("Modificata matricola {0}", matricola);
                                    Console.WriteLine(f);
                                    ScriviFile(f);
                                }
                            }
                        }
                        else
                        {
                            int ext2 = db.MyRai_InfoDipendente.Count(w => w.matricola.Equals(matricola) &&
                                            EntityFunctions.TruncateTime(fine) == EntityFunctions.TruncateTime(w.data_fine));

                            if (ext2 > 0)
                            {
                                giaelaborati++;
                            }
                            else
                            {
                                aggiunti++;
                                Console.WriteLine(matricola);
                            }
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore, " + ex.ToString());
                ScriviFile("Errore, " + ex.ToString());
            }
            Console.WriteLine("Trovati " + trovati);
            Console.WriteLine("Gia' aggiornati " + trovati);
            Console.WriteLine("Da inserire " + aggiunti);
            Console.WriteLine("Terminato, premi un tasto");
            ScriviFile("Fine UpdateInfoDipendente");
        }

        private static void Write(string msg)
        {
            Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + msg);
        }

        public static void PopolaTabellaApprovatoriProduzioni(string[] args)
        {
            List<string> toSlack = new List<string>();
            bool giaScritto = false;
            string messaggio = "";

            try
            {
                var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;
                messaggio = "Avvio batch popola MyRai_ApprovatoriProduzioni";
                Write(messaggio);
                Console.WriteLine(messaggio);
                ScriviFile(messaggio);
                if (connection != null && connection.Contains("ZTOLS420"))
                {
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio + " PRODUZIONE", "good", true);
                    ScriviFile("PopolaTabellaApprovatoriProduzioni PRODUZIONE");

                }
                else
                {
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio + " SVILUPPO", "good", true);
                    ScriviFile("PopolaTabellaApprovatoriProduzioni SVILUPPO");
                }

                List<string> matricole = new List<string>();
                using (digiGappEntities db = new digiGappEntities())
                {
                    messaggio = "Reperimento delle matricole degli approvatori di produzione";
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);
                    Write(messaggio);

                    matricole = db.MyRai_ApprovatoriProduzioni.Where(w => !w.MatricolaApprovatore.StartsWith("UFF")).ToList().Select(w => w.MatricolaApprovatore).Distinct().ToList();

                    messaggio = String.Format("Trovati {0} approvatori", matricole.Count());
                    Console.WriteLine(messaggio);
                    Write(messaggio);
                    ScriviFile(messaggio);
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio, "good", true);
                }

                if (matricole != null && matricole.Any())
                {
                    messaggio = String.Format("Avvio reperimento attività");
                    Write(messaggio);
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);

                    int riga = 0;

                    DateTime oggi = DateTime.Now;
                    DateTime inizioMese = new DateTime(oggi.Year, oggi.Month, 1);
                    //DateTime inizioMese = new DateTime(oggi.Year, 1, 1);

                    // da 1 a 15
                    DateTime inizioSecondoGiro = inizioMese.AddDays(14);
                    DateTime currentMaxDate = inizioSecondoGiro;

                    // fine mese
                    DateTime fineMese = inizioMese.AddMonths(1);
                    fineMese = fineMese.AddDays(-1);

                    if (args != null && args.Any())
                    {
                        string dt1 = args[0];
                        string dt2 = "";
                        if (args.Count() > 1)
                        {
                            dt2 = args[1];
                        }

                        Boolean datavalida = DateTime.TryParseExact(dt1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out inizioMese);

                        if (!datavalida)
                        {
                            throw new Exception("La data inizio inserita non è valida");
                        }

                        if (!String.IsNullOrEmpty(dt2))
                        {
                            datavalida = DateTime.TryParseExact(dt2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fineMese);

                            if (!datavalida)
                            {
                                throw new Exception("La data fine inserita non è valida");
                            }
                        }
                        else
                        {
                            fineMese = new DateTime(inizioMese.Year, inizioMese.Month, 1);
                            fineMese = fineMese.AddDays(-1);
                        }

                        inizioSecondoGiro = inizioMese.AddDays(14);
                        currentMaxDate = inizioSecondoGiro;
                    }

                    DateTime inizio = inizioMese;

                    do
                    {
                        matricole.ForEach(w =>
                        {
                            riga++;
                            messaggio = String.Format("{0}) Reperimento attività per la matricola {1}", riga, w);
                            Console.WriteLine(messaggio);
                            ScriviFile(messaggio);

                            int inseriti = Helper.GetWeekPlan("P" + w, inizio, currentMaxDate);
                            toSlack.Add(String.Format("Inserite {0} attività per la matricola {1}", inseriti, w));
                            Write(String.Format("Inserite {0} attività per la matricola {1} per il periodo dal {2} al {3}", inseriti, w, inizio, currentMaxDate));
                        });

                        inizio = currentMaxDate.AddDays(1);
                        currentMaxDate = currentMaxDate.AddDays(15);
                        if (currentMaxDate > fineMese)
                        {
                            currentMaxDate = fineMese;
                        }

                    } while (inizio <= currentMaxDate);
                }
            }
            catch (Exception ex)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Riepilogo", "good", true, scarico);
                ScriviFile("Si è verificato un errore: " + ex.ToString());
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Si è verificato un errore: " + ex.ToString(), "error", true);
                Write("Si è verificato un errore: " + ex.ToString());
                giaScritto = true;
            }

            if (!giaScritto)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Riepilogo", "good", true, scarico);
            }

            messaggio = "Fine batch popola MyRai_ApprovatoriProduzioni";
            Console.WriteLine(messaggio);
            ScriviFile(messaggio);
            ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio, "good", true);
        }

        public static string GetProduzioneByMatricola(string term)
        {
            try
            {
                APWS sr = new APWS();
                sr.Credentials = new NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                ObjTVRicercaAnagrafieResult res = new ObjTVRicercaAnagrafieResult();
                ObjInputRicercaMatricola ricercaMatricola = new ObjInputRicercaMatricola();

                ricercaMatricola.Matricola = term;
                ricercaMatricola.StatiInVita = true;
                ricercaMatricola.RicercaAncheNelleUorgDiProduzione = true;
                ricercaMatricola.RicercaAncheNelleUorgRaiTrade = true;

                res = sr.TvRicercaAnagrafiaMatricola(ricercaMatricola);

                var results = res.RisultatoTVRicercaAnagrafie.ToList();

                if (results != null && results.Any())
                {
                    return results.First().TITOLO;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void PopolaTabellaAttitivaCeiton()
        {
            string line;
            List<string> log = new List<string>();
            int counter = 0;

            string riga = "";
            int inseriti = 0;

            string sedeGapp = "";
            string matricolaApprovatore = "";
            string nominativo = "";

            StreamReader file = new StreamReader(@"D:\AttivitaProduzione9.csv");

            while ((line = file.ReadLine()) != null)
            {
                counter++;
                log = new List<string>();
                Console.WriteLine(counter);
                try
                {
                    riga = line;
                    // Es: 9FEA1;024236;ANGELI ANDREA;9FEE0;9FEA1;9FEA2;9FEA3;9FEA4;9FEB0;9FEB1;9FEB2;9FEB3;9FEC0
                    List<string> rigaSplittata = riga.Split(';').ToList();

                    matricolaApprovatore = rigaSplittata[0].PadLeft(6, '0');
                    nominativo = rigaSplittata[1];

                    using (digiGappEntities db = new digiGappEntities())
                    {
                        for (int i = 2; i < rigaSplittata.Count; i++)
                        {
                            try
                            {
                                sedeGapp = rigaSplittata[i].PadLeft(5, '0');

                                if (!String.IsNullOrEmpty(sedeGapp) && !sedeGapp.Equals("00000"))
                                {
                                    MyRai_ApprovatoriProduzioni toInsert = new MyRai_ApprovatoriProduzioni()
                                    {
                                        MatricolaApprovatore = matricolaApprovatore,
                                        Nominativo = nominativo,
                                        SedeGapp = sedeGapp,
                                        MatricolaSpettacolo = "",
                                        Titolo = ""
                                    };

                                    db.MyRai_ApprovatoriProduzioni.Add(toInsert);
                                    db.SaveChanges();
                                    inseriti++;

                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(riga + "\n" + ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(riga + "\n" + ex.ToString());
                }
            }
            Console.WriteLine("Inseriti: " + inseriti);
        }

        public static void InserisciInfoDipendente()
        {
            Console.WriteLine("Avvio InserisciInfoDipendente");
            ScriviFile("Avvio InserisciInfoDipendente");
            string line = "";
            DateTime inizio = new DateTime(2019, 12, 01);
            DateTime fine = DateTime.MaxValue;
            int i = 1;
            int trovati = 0;
            int aggiunti = 0;

            Console.WriteLine("Lettura file");

            try
            {
                StreamReader file = new StreamReader(@"D:\turnisti.txt");

                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine("Riga " + i);
                    string riga = line;
                    List<string> rigaSplittata = riga.Split(',').ToList();

                    using (var db = new digiGappEntities())
                    {
                        string matricola = rigaSplittata[0];
                        int tipo = int.Parse(rigaSplittata[1]);
                        string dtINI = rigaSplittata[2];
                        string dtFIN = rigaSplittata[3];
                        string note = rigaSplittata[4];

                        if (!String.IsNullOrEmpty(dtINI))
                        {
                            Boolean datavalida = DateTime.TryParseExact(dtINI, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out inizio);

                            if (!datavalida)
                                continue;
                        }
                        else
                        {
                            inizio = new DateTime(2020, 01, 01);
                        }

                        if (!String.IsNullOrEmpty(dtFIN))
                        {
                            Boolean datavalida = DateTime.TryParseExact(dtFIN, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fine);

                            if (!datavalida)
                                continue;
                        }
                        else
                        {
                            // se non turnista
                            if (tipo != 9)
                            {
                                fine = new DateTime(2020, 12, 31);
                            }
                            else
                            {
                                fine = DateTime.MaxValue;
                            }
                        }

                        int exists = db.MyRai_InfoDipendente.Count(w => w.matricola.Equals(matricola) && w.id_infodipendente_tipologia.Equals(tipo) &&
                        EntityFunctions.TruncateTime(fine) == EntityFunctions.TruncateTime(w.data_fine));

                        if (exists == 0)
                        {
                            db.MyRai_InfoDipendente.Add(new MyRai_InfoDipendente
                            {
                                matricola = matricola,
                                id_infodipendente_tipologia = tipo,
                                valore = "True",
                                data_inizio = inizio,
                                data_fine = fine,
                                note = note
                            });

                            db.SaveChanges();
                            string f = String.Format("Aggiunta matricola {0} di tipo {1}", matricola, tipo);
                            Console.WriteLine(f);
                            ScriviFile(f);
                            aggiunti++;
                        }
                        else
                        {
                            trovati++;
                        }

                        i++;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore, " + ex.ToString());
                ScriviFile("Errore, " + ex.ToString());
            }
            Console.WriteLine("Trovati " + trovati);
            Console.WriteLine("Aggiunti " + aggiunti);
            Console.WriteLine("Terminato, premi un tasto");
            ScriviFile("Fine InserisciInfoDipendente");
        }

        private static void ScriviFile(string msg, string nomeFile = "")
        {
            if (!String.IsNullOrEmpty(msg))
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;

                var directoryPath = Path.GetDirectoryName(location);
                var logPath = Path.Combine(directoryPath, nomeFile);
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                filelog1 = Path.Combine(logPath, "ConsoleLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");

                System.IO.File.AppendAllText(filelog1, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + msg + "\r\n");
            }
        }

        public static void CercaAttivitàPerMatricole()
        {
            List<string> toSlack = new List<string>();
            bool giaScritto = false;
            string messaggio = "";

            try
            {
                var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;
                messaggio = "Avvio batch popola MyRai_ApprovatoriProduzioni";
                Console.WriteLine(messaggio);
                ScriviFile(messaggio);
                if (connection != null && connection.Contains("ZTOLS420"))
                {
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio + " PRODUZIONE", "good", true);
                }
                else
                {
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio + " SVILUPPO", "good", true);
                }

                List<string> matricole = new List<string>();
                using (digiGappEntities db = new digiGappEntities())
                {
                    messaggio = "Reperimento delle matricole degli approvatori di produzione";
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);

                    matricole.Add("608670");

                    //matricole = db.MyRai_ApprovatoriProduzioni.Where( w => !w.MatricolaApprovatore.StartsWith( "UFF" ) ).ToList( ).Select( w => w.MatricolaApprovatore ).Distinct( ).ToList( );

                    messaggio = String.Format("Trovati {0} approvatori", matricole.Count());
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);
                    ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio, "good", true);
                }

                if (matricole != null && matricole.Any())
                {
                    messaggio = String.Format("Avvio reperimento attività");
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);

                    int riga = 0;

                    DateTime oggi = DateTime.Now;
                    DateTime inizioMese = new DateTime(oggi.Year, 2, 28);
                    DateTime fineMese = new DateTime(oggi.Year, 3, 1);
                    //fineMese = fineMese.AddDays( -1 );

                    matricole.ForEach(w =>
                    {
                        riga++;
                        messaggio = String.Format("{0}) Reperimento attività per la matricola {1}", riga, w);
                        Console.WriteLine(messaggio);
                        ScriviFile(messaggio);

                        int inseriti = Helper.GetWeekPlanPerMatricola("P" + w, inizioMese, fineMese);
                        toSlack.Add(String.Format("Inserite {0} attività per la matricola {1}", inseriti, w));
                    });
                }
            }
            catch (Exception ex)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Riepilogo", "good", true, scarico);
                ScriviFile("Si è verificato un errore: " + ex.ToString());
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Si è verificato un errore: " + ex.ToString(), "error", true);
                giaScritto = true;
            }

            if (!giaScritto)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
                ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", "Riepilogo", "good", true, scarico);
            }

            messaggio = "Fine batch popola MyRai_ApprovatoriProduzioni";
            Console.WriteLine(messaggio);
            ScriviFile(messaggio);
            ScriviLogSlack("PopolaTabellaApprovatoriProduzioni", messaggio, "good", true);
        }

        private static void CalcolaFerie()
        {
            try
            {
                List<string> elencoSedi = new List<string>();

                MyRaiService1Client wcf = new MyRaiService1Client();
                wcf.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1]);

                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1])
                };

                #region Recupero dati da HRDW

                try
                {
                    using (var sediDB = new PERSEOEntities())
                    {
                        string query = "SELECT DISTINCT cod_sede_gapp " +
                                        "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE_GAPP] " +
                                        "WHERE cod_sede_gapp not like'%$%' " +
                                        "AND data_fine_validita >= GETDATE() " +
                                        "AND (scadenza is null OR scadenza >= GETDATE())";

                        elencoSedi = sediDB.Database.SqlQuery<string>(query).ToList();
                    }

                    if (elencoSedi != null && elencoSedi.Any())
                    {
                        foreach (var s in elencoSedi)
                        {
                            var resp = wcf.presenzeGiornaliere("000000", s, DateTime.Now.ToString("ddMMyyyy"));

                            if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
                            {
                                Output.WriteLine("Sede " + s + " errore: " + resp.errore);
                                continue;
                            }

                            if (resp.dati == null || resp.dati.Count() == 0)
                            {
                                Output.WriteLine("Sede " + s + " non ci sono dati disponibili");
                                continue;
                            }

                            foreach (var item in resp.dati)
                            {
                                if (item.tipoDip.Trim().ToUpper().Equals("D"))
                                {
                                    var pianoFerieResult = ServiceWrapper.GetPianoFerieWrapped(service, item.matricola, DateTime.Now.ToString("ddMMyyyy"), 75, "D");

                                    if (pianoFerieResult != null && pianoFerieResult.esito)
                                    {
                                        // il servizio restituisce la matricola con 7 char. 0 iniziale
                                        ArretratiExcel2019 model = new ArretratiExcel2019()
                                        {
                                            Matricola = item.matricola.Substring(1),
                                            Categoria = "",
                                            DaFare = 0,
                                            Fruite = 0,
                                            Nominativo = item.nominativo.Trim(),
                                            RapportoLavoro = "",
                                            ServizioContabile = ""
                                        };

                                        /*
                                         * 
                                         * Esempio Dirigente con smaltimento dentro 31.10.2020
                                         * Residuo 2019	120
                                         * Cessione 10%	12
                                         * Nuovo Residuo 2019	108
                                         * Franchigia 20 gg	20
                                         * Base calcolo (A3-A4)	88
                                         * 15% entro 31.10.2020	13,2
                                         * Arrotondato	13
                                         * da o a 0,49 => difetto
                                         * da 0,50 a 0,99 => eccesso
                                         * 
                                         * */

                                        float residuo = 0;
                                        string msgFinale = "";

                                        try
                                        {
                                            residuo = pianoFerieResult.dipendente.ferie.ferieAnniPrecedenti;
                                        }
                                        catch (Exception ex)
                                        {
                                            msgFinale = String.Format("Errore nel reperimento residuo 2019 per la matricola {0}, Nominativo {1}. Errore: {2}", model.Matricola, model.Nominativo, ex.Message);
                                            Output.WriteLine(msgFinale);
                                            continue;
                                        }

                                        MyRai_ArretratiExcel2019 newItem = new MyRai_ArretratiExcel2019()
                                        {
                                            matricola = model.Matricola,
                                            nominativo = model.Nominativo.Trim(),
                                            da_fare = model.DaFare,
                                            fruite_11_31 = 0,
                                            categoria = null,
                                            rapporto_lavoro = null,
                                            servizio_contabile = null
                                        };

                                        if (residuo > 22)
                                        {
                                            decimal cessione10 = (((decimal)residuo * 10) / 100);
                                            decimal nuovoResiduo = (decimal)residuo - cessione10;
                                            int franchigia = 20;
                                            decimal baseCalcolo = nuovoResiduo - franchigia;

                                            decimal percento15 = ((baseCalcolo * 15) / 100);
                                            decimal arrotondato = Decimal.Round(percento15);

                                            newItem.da_fare = (int)arrotondato;
                                        }

                                        // a questo punto salva sul db i dati calcolati
                                        using (digiGappEntities db = new digiGappEntities())
                                        {
                                            // verifica se la matricola esiste già
                                            bool exists = db.MyRai_ArretratiExcel2019.Where(w => w.matricola.Equals(newItem.matricola)).Count() > 0;

                                            if (exists)
                                            {
                                                newItem.matricola = "P" + model.Matricola;
                                            }

                                            db.MyRai_ArretratiExcel2019.Add(newItem);
                                            db.SaveChanges();
                                        }

                                        msgFinale = String.Format("Matricola {0}, Nominativo {1}, Residuo 2019 {2}, Da fare {3}", model.Matricola, model.Nominativo, residuo, newItem.da_fare);

                                        Output.WriteLine(msgFinale);
                                    }
                                    else
                                    {
                                        if (pianoFerieResult != null)
                                        {
                                            throw new Exception(pianoFerieResult.errore);
                                        }
                                        else
                                        {
                                            throw new Exception("Errore in GetPianoFerieWrapped");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                #endregion


            }
            catch (Exception ex)
            {
                Output.WriteLine("Si è verificato un errore " + ex.Message);
            }
        }
    }

    public class InfoAssunzioni
    {
        public string matricola { get; set; }
        public DateTime dataAssunzione { get; set; }
    }

    public class HRGAcache
    {
        public static string Refresh()
        {
            it.rai.servizi.svilhrga.Sedi service = new it.rai.servizi.svilhrga.Sedi();
            try
            {

                var response = service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "01GEST");
                if (response.Cod_Errore == "0")
                {
                    var db = new digiGappEntities();
                    string chiave = "GetCategoriaDatoNetCached";
                    var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                    if (par != null)
                    {
                        par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                        db.SaveChanges();
                    }
                }


                response = service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "01GEST|02GEST|03GEST");
                if (response.Cod_Errore == "0")
                {
                    var db = new digiGappEntities();
                    string chiave = "GetCategoriaDatoNetCachedNolevel";
                    var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                    if (par != null)
                    {
                        par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                        db.SaveChanges();
                    }
                }


                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
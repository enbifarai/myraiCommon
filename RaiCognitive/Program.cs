using Azure;
using Azure.AI.TextAnalytics;
using ClosedXML.Excel;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RaiCognitive
{
    class Program
    {
        /*
         *  Chiave 1:   afd92d8fa7b54399b86dd298ac4ef80d
         *  Chiave 2:   4286843c88fd4cf397d8dd189b13cc31
         *  Endpoint:   https://raiperme-analisi-testo.cognitiveservices.azure.com/
         *  Posizione:  westeurope
         */
        //private static readonly AzureKeyCredential credentials = new AzureKeyCredential("afd92d8fa7b54399b86dd298ac4ef80d");
        //private static readonly Uri endpoint = new Uri("https://raiperme-analisi-testo.cognitiveservices.azure.com/");

        /*
         *  Chiave 1:   4fbf9d89807b4438811872081043a7dc
         *  Chiave 2:   12fae797ceee4fdba72506957546891a
         *  Endpoint:   https://mytestcognitive.cognitiveservices.azure.com/
         *  Posizione:  southcentralus
         */
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("4fbf9d89807b4438811872081043a7dc");
        private static readonly Uri endpoint = new Uri("https://mytestcognitive.cognitiveservices.azure.com/");

        // Max size of a single doc = 5.120 char. as measured in StringInfo.LenghtInTextElements
        // Max sie of a request = 1 MB
        // Max n. doc per request = 10

        // Request per second 100
        // Request per minute 300

        static void Main(string[] args)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            SentimentAnalysisForm(client);
        }

        private static void SentimentAnalysisForm(TextAnalyticsClient client)
        {
            digiGappEntities db = new digiGappEntities();
            var rispList = db.MyRai_FormRisposteDate.Where(x => x.id_domanda == 518 && !db.MyRai_FormRisposteDate_Sentiment.Any(y => y.id_risposta == x.id)).ToList();

            if (!rispList.Any())
                return;

            List<RaiAnswerAnalyzed> raiAnswers = new List<RaiAnswerAnalyzed>();

            int currentSize = 0;
            int maxSize = 5120 - 100;//100 aggiunto per evitare problemi
            int maxDoc = 10;

            string document = "";
            List<TextDocumentInput> list = new List<TextDocumentInput>();
            int i = 0;

            foreach (var item in rispList.Where(x => !String.IsNullOrWhiteSpace(x.risposta_text)))
            {
                string preText = item.risposta_text.Trim();

                preText = String.Join(";", preText.Split('.').Select(x => x.Trim()).Where(x => x != "")).Replace("\n\r", " ");

                if (String.IsNullOrWhiteSpace(preText)) continue;

                if (!preText.EndsWith("."))
                    preText += ".";

                StringInfo tmp = new StringInfo(preText);
                if (currentSize + tmp.LengthInTextElements >= maxSize)
                {
                    var newDoc = new TextDocumentInput(String.Format("{0}", ++i), document);
                    newDoc.Language = "it";
                    list.Add(newDoc);
                    currentSize = 0;
                    document = "";

                    if (i == maxDoc - 1)
                    {
                        CallSentimentAnalysis(db, client, raiAnswers, list);

                        list.Clear();
                        i = 0;
                    }
                }

                currentSize += tmp.LengthInTextElements;
                document += preText + " ";
                RaiAnswerAnalyzed answ = new RaiAnswerAnalyzed()
                {
                    Id = item.id,
                    Risposta = preText
                };
                raiAnswers.Add(answ);
            }

            if (i < maxDoc)
            {
                var newDoc = new TextDocumentInput(String.Format("{0}", ++i), document);
                newDoc.Language = "it";
                list.Add(newDoc);
                currentSize = 0;
                document = "";
            }

            CallSentimentAnalysis(db, client, raiAnswers, list);

        }

        private static void CallSentimentAnalysis(digiGappEntities db, TextAnalyticsClient client, List<RaiAnswerAnalyzed> raiAnswers, List<TextDocumentInput> list)
        {
            var respt = client.AnalyzeSentimentBatch(list);
            if (respt != null)
            {
                foreach (var sentence in respt.Value.SelectMany(x => x.DocumentSentiment.Sentences))
                {

                    var risp = raiAnswers.FirstOrDefault(x => x.Risposta == sentence.Text);
                    if (risp == null)
                        risp = raiAnswers.FirstOrDefault(x => x.Risposta.StartsWith(sentence.Text) || x.Risposta.EndsWith(sentence.Text));

                    if (risp != null)
                    {
                        db.MyRai_FormRisposteDate_Sentiment.Add(new MyRai_FormRisposteDate_Sentiment()
                        {
                            id_risposta = risp.Id,
                            sentiment = sentence.Sentiment.ToString(),
                            positive = Convert.ToDecimal(sentence.ConfidenceScores.Positive),
                            neutral = Convert.ToDecimal(sentence.ConfidenceScores.Neutral),
                            negative = Convert.ToDecimal(sentence.ConfidenceScores.Negative)
                        });
                    }
                    else
                    {
                        var rispStart = raiAnswers.FirstOrDefault(x => x.Risposta.StartsWith(sentence.Text));
                        var rispEnd = raiAnswers.FirstOrDefault(x => x.Risposta.EndsWith(sentence.Text));

                        if (rispStart != null && rispEnd != null)
                        {
                            ;
                        }
                        else
                        {
                            ;
                        }
                    }
                }

                db.SaveChanges();
            }
        }
    }
}

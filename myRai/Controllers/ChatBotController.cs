using myRai.Models;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class ChatBotController : Controller
    {
        /// <summary>
        /// Per ChatBot
        /// </summary>
        public ActionResult SendMessage(string message)
        {
            string typeMessage = "";
            string retMessage = "";
            List<Tuple<string, string,string>> keyValues = new List<Tuple<string, string, string>>();

            if (message.StartsWith("/"))
            {
                string command = message.Substring(1);
                switch (command)
                {
                    case "data":
                        typeMessage = "simple";
                        retMessage = String.Format("Oggi è il {0:dd/MM/yyyy}", DateTime.Today);
                        break;

                    case "ora":
                        typeMessage = "simple";
                        retMessage = String.Format("Sono le {0:HH:mm}", DateTime.Now);
                        break;
                    case "nome":
                        typeMessage = "simple";
                        retMessage = String.Format("Tu sei {0}", UtenteHelper.Nominativo());
                        break;
                    case "help":
                        typeMessage = "commandList";
                        retMessage = "Ecco l'elenco dei comandi disponibili";
                        keyValues.Add(new Tuple<string, string, string>("help", "Cosa sai fare?",""));
                        keyValues.Add(new Tuple<string, string, string>("data", "Che giorno è oggi?","fa fa-calendar"));
                        keyValues.Add(new Tuple<string, string, string>("ora", "Che ora è?","icon icon-hourglass"));
                        keyValues.Add(new Tuple<string, string, string>("nome", "Come mi chiamo?", "icon icon-user"));
                        break;
                    default:
                        typeMessage = "simple";
                        retMessage = "Scusa non ho capito. Digita un comando valido";
                        break;
                }
            }
            else
            {
                typeMessage = "simple";
                retMessage = "Scusa non ho capito. Digita un comando valido";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { typeMessage, message = retMessage, commands = keyValues }
            };
        }

    }
}

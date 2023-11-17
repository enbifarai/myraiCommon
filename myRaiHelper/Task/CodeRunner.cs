using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper.Task
{
    public class CodeRunner : BaseTask
    {
        public string Codice { get; set; }
        public string FullName { get; set; }
        public string MethodName { get; set; }

        public override bool CheckParam(out string errore)
        {
            bool result = true;
            errore = null;

            if (String.IsNullOrWhiteSpace(Codice))
            {
                if (String.IsNullOrWhiteSpace(FullName))
                {
                    errore += "Tipo non indicato\r\n";
                    result = false;
                }
                if (String.IsNullOrWhiteSpace(MethodName))
                {
                    errore += "Metodo non indicato\r\n";
                    result = false;
                }
            }
            
            return result;
        }

        public override bool Esegui(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            try
            {
                if (!String.IsNullOrWhiteSpace(Codice))
                {
                    result = EseguiCodice(out output, out errore);
                }
                else
                {
                    Type c = CezanneHelper.GetTypeByName(FullName);
                    if (c != null)
                    {
                        var method = c.GetMethod(MethodName);
                        if (method != null)
                        {
                            object[] parameters = new object[] { null, null };
                            object res = method.Invoke(null, parameters);
                            result = (bool)res;
                            output = (string)parameters[0];
                            errore = (string)parameters[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }
            
            return result;
        }

        private bool EseguiCodice(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            try
            {
                string[] CodiceCompilatore = CommonHelper.GetParametri<string>(EnumParametriSistema.CodiceCSharp);
                if (Codice.Trim().StartsWith("("))// se inizia con '(' è un metodo complesso 
                {
                    result = CommonHelper.EseguiViaCSharpCompiler(Codice, CodiceCompilatore[1]);
                }
                else
                {
                    result = CommonHelper.EseguiViaReflection(Codice); // altrimenti lo esegue al volo reflection
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }

            return result;
        }
    }
}

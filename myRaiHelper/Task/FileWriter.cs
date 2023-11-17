using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static myRaiHelper.AccessoFileHelper;

namespace myRaiHelper.Task
{
    public class FileWriter : BaseTask
    {
        public string Path { get; set; }
        public string TextContent { get; set; }
        public byte[] ByteContent { get; set; }

        public override bool CheckParam(out string errore)
        {
            bool result = true;
            errore = null;

            if (String.IsNullOrWhiteSpace(Path))
            {
                result = false;
                errore += "Path non indicato\r\n";
            }

            if (String.IsNullOrWhiteSpace(TextContent) && ByteContent == null)
            {
                result = false;
                errore += "Nessun contenuto indicato";
            }

            if (Impersonate && !ImpersonateHrisParam.HasValue)
            {
                result = false;
                errore += "Parametro Impersonate non indicato";
            }
            else if (Impersonate)
            {
                string[] credenziali = HrisHelper.GetParametri<string>(ImpersonateHrisParam.Value);
                if (credenziali==null)
                {
                    result = false;
                    errore += "Credenziali Impersonate non trovate";
                }
            }

            return result;
        }

        public override bool Esegui(out string output, out string errore)
        {
            output = null;
            errore = null;

            try
            {
                if (Impersonate)
                {
                    string[] credenziali = HrisHelper.GetParametri<string>(ImpersonateHrisParam.Value);
                    ImpersonationHelper.Impersonate(credenziali[2], credenziali[0], credenziali[1], delegate
                    {
                        if (!String.IsNullOrWhiteSpace(TextContent))
                            System.IO.File.WriteAllText(Path, TextContent);
                        else if (ByteContent!=null)
                            System.IO.File.WriteAllBytes(Path, ByteContent);
                    });
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(TextContent))
                        System.IO.File.WriteAllText(Path, TextContent);
                    else if (ByteContent != null)
                        System.IO.File.WriteAllBytes(Path, ByteContent);
                }
                return true;
            }
            catch (Exception ex)
            {
                errore = ex.Message;
                return false;
            }
        }

        
    }
}

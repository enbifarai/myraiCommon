using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaiBatch.Auth
{
    public class UtehraRow
    {
        public UtehraRow(string line)
        {
            string abilStr = line.Trim();
            abilStr = Regex.Replace(abilStr, " +", ";");

            string[] abilToken = abilStr.Split(';');
            Matricola = abilToken[0].Substring(4);

            Funzioni = new List<string>();
            Funzioni.AddRange(abilToken.Skip(1));
        }
        public string Matricola { get; set; }
        public List<string> Funzioni { get; set; }
    }
}

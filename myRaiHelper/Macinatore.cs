using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.CSharp;


namespace myRaiHelper
{
    public class Macinatore
    {
        

        public static object Compila(String Codice, String NameSpaceClass, string AssemblyRefs, IEnumerable<string> extraAssemblies=null)
        {
            

            CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> 
              { 
                 { "CompilerVersion", "v4.0" } 
              });

            CompilerParameters parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                WarningLevel = 3,
                CompilerOptions = "/optimize",
                TreatWarningsAsErrors = false
            };
            foreach (string assembly in AssemblyRefs.Split(','))
            {
                parameters.ReferencedAssemblies.Add(assembly);
            }
            //aggiunge come riferimento il chiamante:
             //
            foreach (var a in Assembly.GetCallingAssembly().GetReferencedAssemblies())
            {
                if (a.Name.ToLower().StartsWith("myrai"))
                {
                    Assembly asse = Assembly.Load(a.Name);
                    parameters.ReferencedAssemblies.Add(asse.Location);
                }


                if (a.Name.StartsWith("EntityFramework"))
                {
                    Assembly asse = Assembly.Load(a.Name);
                    parameters.ReferencedAssemblies.Add(asse.Location);
                   }
                if (a.Name.StartsWith("System.DateTime"))
                {
                    Assembly asse = Assembly.Load(a.Name);
                    parameters.ReferencedAssemblies.Add(asse.Location);
                }

            }

            if (extraAssemblies!=null)
            {
                foreach (var item in extraAssemblies)
                {
                    if (item.ToLower().StartsWith("myrai"))
                    {
                        Assembly asse = Assembly.Load(item);
                        parameters.ReferencedAssemblies.Add(asse.Location);
                    }
                }

            }
            
                  
            //parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location.ToUpper().Replace("MYRAI.DLL","MYRAIDATA.dll") );
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, Codice);

            if (results.Errors.Count != 0 )
            {
                foreach (CompilerError err in results.Errors)
                {
                    if (err.IsWarning == false) return results.Errors;
                }
            }

            //restituisce l'oggetto della classe
            return results.CompiledAssembly.CreateInstance(NameSpaceClass); ;
        }
    }
}
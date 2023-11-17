using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace myRaiHelper
{
      public class CsvFileResult<T> : FileResult 
    { 
        private readonly IEnumerable<T> _list; 
        private readonly string _separator;

        public CsvFileResult(IEnumerable<T> list, string filename) 
            : base("text/csv") 
        { 
            _list = list; 
            _separator = "||"; 
            FileDownloadName = filename; 
        }

        protected override void WriteFile(HttpResponseBase response) 
        { 
            var outputStream = response.OutputStream;

            using (var memoryStream = new MemoryStream()) 
            { 
                this.WriteData(memoryStream); 
                
                outputStream.Write( 
                    memoryStream.GetBuffer(), 
                    0, 
                    (int)memoryStream.Length); 
            } 
        }

        private void WriteData(MemoryStream memoryStream) 
        { 
            StreamWriter streamWriter = new StreamWriter(memoryStream);

        
            foreach (T line in _list) 
            { 
                foreach (MemberInfo member in typeof(T).GetProperties()) 
                { 
                    streamWriter.Write( 
                        string.Concat( 
                            this.GetPropertyValue(line, member.Name), 
                            _separator)); 
                }

                streamWriter.WriteLine(); 
            }

            streamWriter.Flush(); 
        }

        private string GetPropertyValue(object item, string propName) 
        { 
            return item.GetType() 
                .GetProperty(propName) 
                .GetValue(item, null) == null ? "" :
                item.GetType() 
                .GetProperty(propName) 
                .GetValue(item, null).ToString() ?? ""; 
        } 
    }
}
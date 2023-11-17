using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper
{
    public class BatchBaseClass
    {
        public log4net.ILog _log;
        public string _baseOutput;

        public void InitLog(ILog log, string baseOutput)
        {
            _log = log;
            _baseOutput = baseOutput;
        }

        public virtual void Entry( string[] args)
        {
            throw new NotImplementedException();
        }
    }
}

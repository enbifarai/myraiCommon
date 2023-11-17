using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRai.Data.Interface
{
    public interface ISintesi1
    {
        string COD_QUALIFICA { get; }
        string COD_SERVIZIO { get;  }
        string COD_SEDE { get;  }
        string COD_IMPRESACR { get;  }
        string COD_MATLIBROMAT { get;  }
        string COD_TPCNTR { get;  }

        string DES_COGNOMEPERS { get; }
        string DES_NOMEPERS { get; }
    }
}

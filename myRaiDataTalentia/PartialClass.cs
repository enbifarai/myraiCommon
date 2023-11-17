using myRai.Data.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace myRaiDataTalentia
{
    public partial class TalentiaEntities : DbContext
    {
        public TalentiaEntities(string connectionString)
           : base(connectionString)
        {
        }
    }

    public partial class SINTESI1 : ISintesi1
    {

    }
}

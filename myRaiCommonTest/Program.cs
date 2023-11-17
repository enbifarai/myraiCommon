using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiHelper;

namespace myRaiCommonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (SessionHelper.Get("pippo")==null)
                SessionHelper.Set("pippo", 3);

            int pippo = (int)SessionHelper.Get("pippo");
        }
    }
}

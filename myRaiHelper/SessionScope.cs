using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiHelper
{
    public class SessionScope<T> where T: SessionScope<T>, new()
    {
        public SessionScope()
        {
        }

        public static T Instance
        {
            get
            {
                Type t = typeof(T);
                string key = String.Format("___{0}.{1}", t.Namespace, t.Name);

                if (SessionHelper.Get(key) == null)
                    SessionHelper.Set(key, new T());

                return (T)SessionHelper.Get(key);
            }
        }
    }
}
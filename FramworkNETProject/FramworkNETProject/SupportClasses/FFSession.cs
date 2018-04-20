using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DLMS.SupportClasses
{
    public class FFSession
    {
        public Dictionary<string, object> FakeSession = new Dictionary<string, object>();
        public HttpSessionStateBase Session;

        public object this[string s]{
            get
            {
                if (Session != null)
                {
                    return Session[s];
                }
                else
                {
                    if (FakeSession.ContainsKey(s))
                    {
                        return FakeSession[s];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                if (Session != null)
                {
                    Session[s] = value;
                }
                else
                {
                    FakeSession[s] = value;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nop.Services.Common
{
    public interface IConnectionService
    {
        HttpWebRequest GetConnection(string record);
    }
}

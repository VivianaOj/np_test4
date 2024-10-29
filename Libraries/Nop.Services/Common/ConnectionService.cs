using Newtonsoft.Json;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Nop.Services.Common
{
    public class ConnectionService : IConnectionService
    {
        #region Fields
        private readonly ISettingService _settingService;

        #endregion
        public ConnectionService(ISettingService settingService)
        {
            _settingService = settingService;
        }
        public HttpWebRequest GetConnection(string record)
        {
            var uri = _settingService.GetSetting("creditcardvaultauthorizenetsettings.UrlAuthorizeNet").Value + "/" + record; // "https://apitest.authorize.net/xml/v1/request.api/" 
            Uri url = new Uri(uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Method = "POST";
            return request;
        }
    }
}

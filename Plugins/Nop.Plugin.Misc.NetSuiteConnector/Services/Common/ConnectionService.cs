using Nop.Services.Configuration;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public class ConnectionService : IConnectionServices
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IOAuthBaseHelper _oAuthBaseHelper;

        #endregion

        #region Ctor

        public ConnectionService(ISettingService settingService, IOAuthBaseHelper oAuthBaseHelper)
        {
            _settingService = settingService;
            _oAuthBaseHelper = oAuthBaseHelper;
        }

        #endregion

        #region Methods

        public string GetConnection(string record, string method, string bodyContent = null)
        {
            var netSuiteConnectorSettings = _settingService.LoadSetting<NetSuiteConnectorSettings>();

            var uri = netSuiteConnectorSettings.RestServicesUrl + record;
            Uri url = new Uri(uri);

            string timestamp = _oAuthBaseHelper.GenerateTimeStamp();
            string nonce = _oAuthBaseHelper.GenerateNonce();
            string ckey = netSuiteConnectorSettings.ConsumerKey; //Consumer Key
            string csecret = netSuiteConnectorSettings.ConsumerSecret; //Consumer Secret
            string tkey = netSuiteConnectorSettings.TokenId; //Token ID
            string tsecret = netSuiteConnectorSettings.TokenSecret; //Token Secret
            string realm = netSuiteConnectorSettings.AccountId;
            string norm = "";
            string norm1 = "";

            string basedata = _oAuthBaseHelper.GenerateSignatureBase(url, ckey, tkey, tsecret, method, timestamp, nonce, "HMAC-SHA256", out norm, out norm1);
            var hash = new HMACSHA256
            {
                Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", _oAuthBaseHelper.UrlEncode(csecret), string.IsNullOrEmpty(tsecret) ? "" : _oAuthBaseHelper.UrlEncode(tsecret)))
            };

            string signature = _oAuthBaseHelper.GenerateSignatureUsingHash(basedata, hash);

            var bodynfo = @"{""AccessRequest"": { ""AccessLicenseNumber"": ""8D9C50C7162BC8D6"",""UserId"": ""Boxes719"",
    ""Password"": ""MakeMoney123!""},""AddressValidationRequest"": {    ""Request"": {
      ""TransactionReference"": {
        ""CustomerContext"": ""Your Customer Context""
      },
      ""RequestAction"":""AV""},
    ""Address"": {
      ""City"": ""ALPHARETTA"",
      ""StateProvinceCode"": ""GA"",
      ""PostalCode"": ""1111""
    }
  }
}";

            //var data = responseText("/AV", "POST", body);
            //GetDataNetsuiteUPS(uri, timestamp, nonce, ckey, tkey, realm, signature, method, bodynfo);

            return GetDataNetsuite(uri, timestamp, nonce, ckey, tkey, realm, signature, method, bodyContent);
        }

        private string GetDataNetsuite(string uri, string timestamp, string nonce, string ckey, string tkey, string realm, string signature, string method, string body)
        {
            string header = "OAuth  ";
            header += "realm=\"" + realm + "\",";
            header += "oauth_consumer_key=\"" + ckey + "\",";
            header += "oauth_token=\"" + tkey + "\",";
            header += "oauth_signature_method=\"HMAC-SHA256\",";
            header += "oauth_timestamp=\"" + timestamp + "\",";
            header += "oauth_nonce=\"" + nonce + "\",";
            header += "oauth_version=\"1.0\",";
            header += "oauth_signature=\"" + _oAuthBaseHelper.UrlEncode(signature) + "\"";

            var client = new RestClient(uri);
            client.Timeout = -1;

            System.Enum.TryParse(method, out Method restSharpMethod);

            var request = new RestRequest(restSharpMethod);

            request.AddHeader("Authorization", header);

            if (!string.IsNullOrEmpty(body))
            {
                request.AddHeader("prefer", "transient");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }

            IRestResponse response = client.Execute(request);

            string responseText = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Content != null)
                    responseText = response.Content;
                else
                    responseText = response.StatusCode.ToString();
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    if (response.Content != "")
                        responseText = response.Content;
                    else
                        responseText = response.StatusCode.ToString();
                }
            }

            return responseText;
        }

		#endregion

		#region async methods



		public async Task<string> GetDataNetsuiteAsync(string uri, string timestamp, string nonce, string ckey, string tkey, string realm, string signature, string method, string body)
		{
			string header = "OAuth  ";
			header += "realm=\"" + realm + "\",";
			header += "oauth_consumer_key=\"" + ckey + "\",";
			header += "oauth_token=\"" + tkey + "\",";
			header += "oauth_signature_method=\"HMAC-SHA256\",";
			header += "oauth_timestamp=\"" + timestamp + "\",";
			header += "oauth_nonce=\"" + nonce + "\",";
			header += "oauth_version=\"1.0\",";
			header += "oauth_signature=\"" + _oAuthBaseHelper.UrlEncode(signature) + "\"";

			using (var httpClient = new HttpClient())
			{
				httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a timeout if needed

				var request = new HttpRequestMessage(new HttpMethod(method), uri);

				request.Headers.Add("Authorization", header);

				if (!string.IsNullOrEmpty(body))
				{
					request.Content = new StringContent(body, Encoding.UTF8, "application/json");
				}

				HttpResponseMessage response = await httpClient.SendAsync(request);

				string responseText = string.Empty;

				if (response.IsSuccessStatusCode)
				{
					responseText = await response.Content.ReadAsStringAsync();
				}
				else
				{
					responseText = response.StatusCode.ToString();
				}

				return responseText;
			}
		}

		public async Task<string> GetConnectionAsync(string record, string method, string bodyContent = null)
		{
			var netSuiteConnectorSettings = _settingService.LoadSetting<NetSuiteConnectorSettings>();

			var uri = netSuiteConnectorSettings.RestServicesUrl + record;
			Uri url = new Uri(uri);

			string timestamp = _oAuthBaseHelper.GenerateTimeStamp();
			string nonce = _oAuthBaseHelper.GenerateNonce();
			string ckey = netSuiteConnectorSettings.ConsumerKey; //Consumer Key
			string csecret = netSuiteConnectorSettings.ConsumerSecret; //Consumer Secret
			string tkey = netSuiteConnectorSettings.TokenId; //Token ID
			string tsecret = netSuiteConnectorSettings.TokenSecret; //Token Secret
			string realm = netSuiteConnectorSettings.AccountId;
			string norm = "";
			string norm1 = "";

			string basedata = _oAuthBaseHelper.GenerateSignatureBase(url, ckey, tkey, tsecret, method, timestamp, nonce, "HMAC-SHA256", out norm, out norm1);
			var hash = new HMACSHA256
			{
				Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", _oAuthBaseHelper.UrlEncode(csecret), string.IsNullOrEmpty(tsecret) ? "" : _oAuthBaseHelper.UrlEncode(tsecret)))
			};

			string signature = _oAuthBaseHelper.GenerateSignatureUsingHash(basedata, hash);

			return await GetDataNetsuiteAsync(uri, timestamp, nonce, ckey, tkey, realm, signature, method, bodyContent);
		}

		#endregion
		#region TestUPS

		private string GetDataNetsuiteUPS(string uri, string timestamp, string nonce, string ckey, string tkey, string realm, string signature, string method, string body)
        {
            string header = "OAuth  ";
            header += "realm=\"" + realm + "\",";
            //header += "oauth_consumer_key=\"" + ckey + "\",";
            //header += "oauth_token=\"" + tkey + "\",";
            header += "oauth_signature_method=\"HMAC-SHA256\",";
            header += "oauth_timestamp=\"" + timestamp + "\",";
            header += "oauth_nonce=\"" + nonce + "\",";
            header += "oauth_version=\"1.0\",";
            header += "oauth_signature=\"" + _oAuthBaseHelper.UrlEncode(signature) + "\"";

            var client = new RestClient("https://wwwcie.ups.com/rest/AV");
            client.Timeout = -1;

            System.Enum.TryParse(method, out Method restSharpMethod);

            var request = new RestRequest(restSharpMethod);

            request.AddHeader("Authorization", header);

            if (!string.IsNullOrEmpty(body))
            {
                request.AddHeader("prefer", "transient");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }

            IRestResponse response = client.Execute(request);

            string responseText = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Content != null)
                    responseText = response.Content;
                else
                    responseText = response.StatusCode.ToString();
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    if (response.Content != "")
                        responseText = response.Content;
                    else
                        responseText = response.StatusCode.ToString();
                }
            }

            return responseText;
        }
   
        #endregion
    }

}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Common
{
    public partial class ZidCodeService : IZipCodeService, ILocalizedEntity
    {
        #region Fields
        private readonly IRepository<Zipcode> _zipCodeRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        #endregion

        #region Ctor
        public ZidCodeService(IRepository<Zipcode> zipCodeRepository, IHttpClientFactory httpClientFactory, ISettingService settingService, IStateProvinceService stateProvinceService)
        {
            _zipCodeRepository = zipCodeRepository;
            _httpClientFactory = httpClientFactory;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
        }
        #endregion

        /// <summary>
        /// Gets all zipCode attributes
        /// </summary>
        /// <returns>zipCode attributes</returns>
        public virtual IList<Zipcode> GetAllZipcode()
        {
            var query = _zipCodeRepository.Table;
            return query.ToList();
        }

        public virtual bool IsValidCode(int countryId, int stateProvinceId, string code)
        {
            return _zipCodeRepository.Table.Any(x => x.CountryId == countryId && x.StateProvinceId == stateProvinceId && x.Code == code);
        }
        
        public AddressValidation GetAddressValidation(Address address)
        {
            var addressValidTask = GetAddressValidationUPS(address);
            addressValidTask.Wait();
            var addressValid = addressValidTask.Result;

            RootObject root = JsonConvert.DeserializeObject<RootObject>(addressValid);

            AddressValidation addressValidation = new AddressValidation();
            addressValidation.Address = address;
            addressValidation.IsValid = false;
            
            if (root != null)
			{
                if (root.XAVResponse.Candidate != null)
                {
                    addressValidation.IsValid = true;

                    var address1 = address.Address1.TrimStart().TrimEnd().ToUpper();
                    var city1 = address.City.TrimStart().TrimEnd().ToUpper();
                    var zip = address.ZipPostalCode.TrimStart().TrimEnd().ToUpper();
                    var similarityThreshold = 0.9;

                    var AddressValid = root.XAVResponse.Candidate.Where(r =>(r.AddressKeyFormat.PostcodePrimaryLow == zip || $"{r.AddressKeyFormat.PostcodePrimaryLow}-{r.AddressKeyFormat.PostcodeExtendedLow}" == zip)
                            && r.AddressKeyFormat.PoliticalDivision2.ToUpper() == city1
                             && r.AddressKeyFormat.AddressLine.Any(al => Similarity(al.ToUpper(), address1.ToUpper()) > similarityThreshold)).ToList();

                    if (AddressValid.Count()==0)
					{
                        if (root.XAVResponse.Candidate.Count() > 0)
                        {
                            foreach (var candidate in root.XAVResponse.Candidate.Take(5))
                            {

                                var Address1 = candidate.AddressKeyFormat.AddressLine[0].ToString();
                                var Address2 = "";
                                if (candidate.AddressKeyFormat.AddressLine.Count()>1)
                                    Address2 = candidate.AddressKeyFormat.AddressLine[1].ToString();

                                var ZipPostalCode = candidate.AddressKeyFormat.PostcodePrimaryLow.ToString();
                                var ZipPostalCodeExtendedLow = candidate.AddressKeyFormat.PostcodeExtendedLow.ToString();

                                var City = candidate.AddressKeyFormat.PoliticalDivision2.ToString();
                                var StateProvince = candidate.AddressKeyFormat.PoliticalDivision1.ToString();

                                var candidateAddressList = new Address();
                                var stateProvinceData = _stateProvinceService.GetStateProvinceByAbbreviation(StateProvince);
                                candidateAddressList.Address1 = Address1.ToString();
                                candidateAddressList.ZipPostalCode = ZipPostalCode + "-" + ZipPostalCodeExtendedLow;
                                candidateAddressList.City = City;
                                candidateAddressList.StateProvince = stateProvinceData;
                                candidateAddressList.StateProvinceId = stateProvinceData.Id;

                                addressValidation.CandidateAddress.Add(candidateAddressList);
                            }
                        }
                        else
                            addressValidation.IsValid = false;

                    }
                }
            }
            


            return addressValidation;
        }

        #region Start N&N Validate Address with UPS
        private async Task<string> GetAddressValidationUPS(Address address)
        {
            var tokenResponse = GetTokenConnectionUps();
            tokenResponse.Wait();

            var token = tokenResponse.Result;
            var result = "";

            if (token != null && token != "10429")
			{
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

				JObject json = JObject.Parse(@"{
                  XAVRequest: {
                    AddressKeyFormat: {
                      AddressLine: [
                        """ + address?.Address1+  @"""
                      ],
                      PoliticalDivision2: """ + address?.City + @""",
                      PoliticalDivision1: """ + address?.StateProvince?.Abbreviation + @""",
                      PostcodePrimaryLow: """ + address?.ZipPostalCode + @""",
                      CountryCode: 'US'
                    }
                  }
                }");

				try
				{
                    var postData = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                    var Requestoption = "3";
                    var Version = "v2";
                    //https://wwwcie.ups.com/api/addressvalidation

                    var request = await client.PostAsync("https://onlinetools.ups.com/api/addressvalidation/" + Version + "/" + Requestoption + "", postData);
                    var response = await request.Content.ReadAsStringAsync();

                    // Parse the JSON response to extract error information
                    JObject errorJson = JObject.Parse(response);

                    if (request.StatusCode == HttpStatusCode.NotFound)
                    {
                        JToken errors = errorJson["response"]["errors"];
                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                string code = (string)error["code"];
                                string message = (string)error["message"];
                                result = ($"Error {code}: {message}");
                            }
                        }
                    }
                    else if (request.IsSuccessStatusCode)
                    {
                       
                        return response;
                    }
                    else
                    {
                        return response;
                    }
                }
				catch (Exception ex)
				{
                    var addressAgain=GetAddressValidationUPS(address);
                    //throw;
                }
				
            }
			else
			{
                return "Too Many Request";
            }
            return result;
        }

        private async Task<string> GetTokenConnectionUps()
        {
			try
			{
                var client = _httpClientFactory.CreateClient();

                var myMerchantId = _settingService.GetSetting("upssettings.merchantId").Value;
                var myClientID = _settingService.GetSetting("upssettings.clientid").Value;
                var mySecretID = _settingService.GetSetting("upssettings.secretid").Value;

                client.DefaultRequestHeaders.Add("x-merchant-id", myMerchantId);

                // "wSWpVsSzxbNB9X50g7fRzSpI5M5u00OrQQaRF2mgQecjCtCG";
                //var mySecretID = "SRXIzAiLy5eigBBbMss1QqAYTlQeGiup79SmQGy4VHAuRRwxhHaFD68UNgPBnoPI";

                var accessID = $"{myClientID}:{mySecretID}";
                var base64AccessID = Convert.ToBase64String(Encoding.ASCII.GetBytes(accessID));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64AccessID);

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                var request = await client.PostAsync("https://wwwcie.ups.com/security/v1/oauth/token", new FormUrlEncodedContent(postData));
                var response = await request.Content.ReadAsStringAsync();
                // Parse JSON response
                JObject json = JObject.Parse(response);

                return json["access_token"].ToString();
            }
			catch (Exception)
			{
                return "10429";
            }
        }

        #endregion


        public double Similarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return 0;
            }

            int maxLength = Math.Max(a.Length, b.Length);
            int distance = ComputeLevenshteinDistance(a, b);

            return 1.0 - (double)distance / maxLength;
        }

        public int ComputeLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return !string.IsNullOrEmpty(b) ? b.Length : 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                return !string.IsNullOrEmpty(a) ? a.Length : 0;
            }

            int[] costs = new int[b.Length + 1];
            for (int j = 0; j < costs.Length; j++)
            {
                costs[j] = j;
            }

            int i = 0;
            foreach (char aChar in a)
            {
                costs[0] = i + 1;
                int corner = i++;

                for (int j = 0; j < b.Length; j++)
                {
                    int upper = costs[j + 1];
                    costs[j + 1] = aChar == b[j] ? corner : Math.Min(Math.Min(costs[j], upper), corner) + 1;
                    corner = upper;
                }
            }

            return costs[b.Length];
        }
    }
  

    public class AccessTokenResponse
    {
        public string TokenType { get; set; }
        public string IssuedAt { get; set; }
        public string ClientId { get; set; }
        public string AccessToken { get; set; }
        public string ExpiresIn { get; set; }
        public string Status { get; set; }
    }

    public class ResponseStatus
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class AddressClassification
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class AddressKeyFormat
    {
        public List<string> AddressLine { get; set; }
        public string PoliticalDivision2 { get; set; }
        public string PoliticalDivision1 { get; set; }
        public string PostcodePrimaryLow { get; set; }
        public string PostcodeExtendedLow { get; set; }
        public string Region { get; set; }
        public string CountryCode { get; set; }
    }

    public class Candidate
    {
        public AddressClassification AddressClassification { get; set; }
        public AddressKeyFormat AddressKeyFormat { get; set; }
    }

    public class XAVResponse
    {
        public Response Response { get; set; }
        public string AmbiguousAddressIndicator { get; set; }
        public AddressClassification AddressClassification { get; set; }
        public List<Candidate> Candidate { get; set; }
    }

    public class RootObject
    {
        public XAVResponse XAVResponse { get; set; }
    }

    public partial class AddressValidation
    {
        public AddressValidation()
        {
            CandidateAddress = new List<Address> ();
        }

        public Address Address { get; set; }
        public bool IsValid { get; set; }

        public List<Address> CandidateAddress { get; set; }

    }


}
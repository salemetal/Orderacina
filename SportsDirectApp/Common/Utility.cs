using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SportsDirectApp.Common
{
    public class Utility
    {
        public static SmtpClient GetMailClient(Smtp smtp)
        {
            SmtpClient smtpClient = new SmtpClient
            {
                Host = smtp.Host,
                Port = smtp.Port,
                EnableSsl = smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = smtp.UseDefaultCredentials,
                Credentials = new NetworkCredential(smtp.Address, smtp.Password)
            };

            return smtpClient;
        }

        public static decimal GetTecaj(string currencyName, string HNBApiTecajType)
        {
            decimal tecaj = 1m;
            if (currencyName == Constants.Currency.HRK) return tecaj;

            var apiUrl = $"http://api.hnb.hr/tecajn/v1?valuta={currencyName}";
            var request = (HttpWebRequest)WebRequest.Create(apiUrl);
            var responseJson = "";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            responseJson = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception("Get tecaj request failed!");
                }
            }

            if (!string.IsNullOrEmpty(responseJson))
            {
                responseJson = responseJson.Replace("[", "").Replace("]", "");

                try
                {
                    JObject jObject = JObject.Parse(responseJson);
                    tecaj = decimal.Parse((string)jObject.SelectToken($"['{HNBApiTecajType}']"));
                }
                catch (Exception)
                {
                    throw new Exception("Get tecaj request failed!");
                }
            }

            return tecaj;
        }
    }
}

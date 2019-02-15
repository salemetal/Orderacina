using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
    }
}

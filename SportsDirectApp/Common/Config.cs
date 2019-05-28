using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsDirectApp.Common
{
    public class Config
    {
        public string HNBApiTecaj { get; set; }
        public bool NewOrderNotificationEnabled { get; set; }
        public string NewOrderLink { get; set; }
    }

    public class Smtp
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}

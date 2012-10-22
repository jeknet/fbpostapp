using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacebookPost.Models
{
    public class FacebookToken
    {
        public String Token { get; set; }
        public DateTime creationTime { get; set; }
        public int TokenExpirationTime { get; set; }
        public String ApiKey { get; set; }
    }
}
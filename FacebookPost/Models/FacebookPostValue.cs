using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FacebookPost.Models
{
    public class FacebookPostValue
    { 
        public String Post { get; set; }
        public DateTime RequestTime { get; set; }
        public FacebookToken Token { get; set; }
    }
}
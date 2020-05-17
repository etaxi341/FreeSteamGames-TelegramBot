using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataManager.Models
{
    public class Subscribers
    {
        [Key]
        public long chatID { get; set; }
        public bool wantsDlcInfo { get; set; }
        public bool wantsGameInfo { get; set; }
    }
}

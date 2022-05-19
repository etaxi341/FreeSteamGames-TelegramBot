using System.ComponentModel.DataAnnotations;

namespace DataManager.Models;

public class Subscribers
{
    [Key]
    public long chatID { get; set; }
    public bool wantsDlcInfo { get; set; }
    public bool wantsGameInfo { get; set; }
}
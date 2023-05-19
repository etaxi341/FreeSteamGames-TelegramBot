using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataManager.Models;

[Index(nameof(chatID), nameof(steamLink))]
public class Notifications
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public long chatID { get; set; }
    public string steamLink { get; set; }
}
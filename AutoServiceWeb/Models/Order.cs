using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiceWeb.Models
{
    public class Order
    {
        public string Id { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public DateTime CreationDateAndTime { get; set; }
    }
}

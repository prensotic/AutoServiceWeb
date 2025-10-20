using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiceWeb.Models
{
    public class Service
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MainImage { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public string Time { get; set; } = string.Empty;
        public List<ServiceImage> AdditionalImages { get; set; } = new List<ServiceImage>();
    }

    public class ServiceImage
    {
        public int Id { get; set; }
        public string ServiceId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}

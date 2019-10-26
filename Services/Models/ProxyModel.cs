using System;

namespace YMParseWeb.Models
{
    public class ProxyModel
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public string Speed { get; set; }
        public string Type { get; set; }
        public string Upd { get; set; }
        public string Work { get; set; }
        public DateTime AddedDate { get; set; }
        public string[] Proxies { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Models
{
    public record ProtocolDetails
    {
        public string ProtocolNo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Objective { get; set; }
        public string? NctId { get; set; }
        public string? Phase { get; set; }
        public List<string>? DiseaseSites { get; set; }
        public string? DetailedEligibility { get; set; }
    }
}

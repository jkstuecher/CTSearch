using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Models
{
    public record ProtocolDetails
    {
        string ProtocolNo { get; set; }
        string Title { get; set; }
        string? Objective { get; set; }
        string? NctId { get; set; }
        string? Phase { get; set; }
        List<string>? DiseaseSites { get; set; }
        string? DetailedEligibility { get; set; }
    }
}

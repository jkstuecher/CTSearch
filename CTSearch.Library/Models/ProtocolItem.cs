using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Models
{
    public record ProtocolItem
    {
        string ProtocolNo { get; init; } 
        string Title { get; set; }
        string Status { get; set; }
    }
}

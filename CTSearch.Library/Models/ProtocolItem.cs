using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Models
{
    public record ProtocolItem
    {
        public string ProtocolNo { get; init; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

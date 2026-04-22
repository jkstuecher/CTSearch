using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Models
{
    public record ProtocolSearchResult(int Count, List<ProtocolItem> Values);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PH.TinyMapper.Net8
{
    public record ExampleRecord(Guid Id, string Name, DateTime UtcNow, DateTime? DateTimeNullable = null);
}
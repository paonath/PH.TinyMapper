using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PH.TinyMapper.TestCommon
{
    public class Source : SourceBase
    {
        public string Name { get; set; }

        public DateTime UtcNow { get; }

        public Source()
        {
            UtcNow = DateTime.UtcNow;
        }

        public string GetLastNameExample() => $"LastNameOf {UtcNow:yy-MM-dd}";


    }
}
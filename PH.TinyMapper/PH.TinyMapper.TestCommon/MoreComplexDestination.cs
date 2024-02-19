using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PH.TinyMapper.TestCommon
{
    public class MoreComplexDestination : Destination
    {
        public Destination Some { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PH.TinyMapper.TestCommon
{
    public class Destination : SourceBase
    {
        public string Name     { get; set; }
        public string LastName { get; set; }

        public DateTime? DateTimeNullable2 { get; set; }
    }

    public class DestinationWithSkip : SourceBase
    {
        /// <summary>
        /// Gets or sets the name of the destination.
        /// </summary>
        /// <remarks>
        /// This property is marked with the SkipMapping attribute, 
        /// which means it will not be included in the mapping process by the TinyMapper.
        /// </remarks>
        [SkipMapping]
        public string Name     { get; set; }
        public string LastName { get; set; }

        public DateTime? DateTimeNullable2 { get; set; }
    }
}
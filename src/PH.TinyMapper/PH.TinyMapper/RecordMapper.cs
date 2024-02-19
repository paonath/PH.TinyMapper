using System;
using System.Linq;

namespace PH.TinyMapper
{
     #if NET8_0_OR_GREATER

    public partial class Mapper
    {

        /// <summary>
        /// Finds a mapping record for the specified source type and record type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TRecord">The type of the record object.</typeparam>
        /// <param name="source">The source object to find a mapping record for.</param>
        /// <param name="recordType">The type of the record to find a mapping record for.</param>
        /// <returns>A <see cref="MapBase"/> object representing the mapping record if found; otherwise, a new mapping record is created, added to the map array, and returned.</returns>
        /// <remarks>
        /// This method first checks if a mapping record already exists for the specified source type and record type. If a mapping record does not exist, a new one is created using the <see cref="MapBase.BuildRecordMap{TSource, TRecord}(TSource)"/> method, added to the map array, and then returned.
        /// </remarks>
        private MapBase FindMappingRecord<TSource, TRecord>(TSource source, Type recordType) where TSource : class
            where TRecord : class
        {
            var sourceType = source.GetType();
            

            var map = MapArray.FirstOrDefault(m => m.SourceType == sourceType && m.TargetType == recordType && m.IsRecordTarget);
            if (null == map)
            {
                var mymap = MapBase.BuildRecordMap<TSource, TRecord>(source);
                AddRecordMap(mymap);
                return mymap;
            }

            return map;
        }

        /// <summary>
        /// Maps the source object of type TSource to a new object of type TRecord.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TRecord">The type of the target object.</typeparam>
        /// <param name="source">The source object to map from.</param>
        /// <returns>A new object of type TRecord that is a mapped version of the source object, or null if the source object is null.</returns>
        public TRecord? Record<TSource, TRecord>(TSource? source) where TSource : class
                                                               where TRecord : class
        {
            if (source == null)
            {
                return null;
            }

            var map = (RecordMap<TSource,TRecord>)FindMappingRecord<TSource,TRecord>(source, typeof(TRecord));

            var target = map._func.Invoke(source, map._constructor , map.Properties);
            return target;
        }
        

    }
    
    #endif
}
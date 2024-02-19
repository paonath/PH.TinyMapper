using System.Collections.Generic;
using System.Reflection;
using System;

namespace PH.TinyMapper
{
     #if NET8_0_OR_GREATER

    /// <summary>
    /// Represents a mapping between a source type and a target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDest">The type of the target object.</typeparam>
    /// <remarks>
    /// This class extends the MapBase class and provides a concrete implementation for mapping between specific source and target types.
    /// </remarks>
    public class Map<TSource, TDest> : MapBase where TSource : class
                                               where TDest : class, new()
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TSource, TDest}"/> class.
        /// </summary>
        /// <param name="map">The dictionary that maps properties of the source type to properties of the target type.</param>
        /// <param name="isRecord">A boolean value indicating whether the target object is a record type.</param>
        internal Map(Dictionary<PropertyInfo, TargetInfo> map, bool isRecord)
            : base(map, typeof(TSource), typeof(TDest), isRecord)
        {

        }



    }

    public class RecordMap<TSource, TRecord> : MapBase
        where TSource : class
    {

        internal Func<TSource, ConstructorInfo, Dictionary<PropertyInfo, TargetInfo>, TRecord> _func;
        internal ConstructorInfo                                                               _constructor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapBase"/> class.
        /// </summary>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <param name="func">An expression that defines the mapping function from the source type to the record type.</param>
        public RecordMap(Dictionary<PropertyInfo, TargetInfo> map,Type sourceType, Type targetType,
                         Func<TSource, ConstructorInfo, Dictionary<PropertyInfo, TargetInfo>, TRecord> func,
                         ConstructorInfo ctorInfo) : base(map, sourceType, targetType, true)
        {
            _func = func;
            _constructor = ctorInfo;
        }

        
    }
    
    #else

    /// <summary>
    /// Represents a mapping between a source type and a target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDest">The type of the target object.</typeparam>
    /// <remarks>
    /// This class extends the MapBase class and provides a concrete implementation for mapping between specific source and target types.
    /// </remarks>
    public class Map<TSource, TDest> : MapBase where TSource : class 
                                            where TDest : class , new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TSource, TDest}"/> class.
        /// </summary>
        /// <param name="map">The dictionary that maps properties of the source type to properties of the target type.</param>
        internal Map(Dictionary<PropertyInfo, TargetInfo> map)
            :base(map, typeof(TSource) ,typeof(TDest))
        {
            
        }
        

        
    }
    
    #endif
    
}
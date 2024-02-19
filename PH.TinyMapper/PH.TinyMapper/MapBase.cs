#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace PH.TinyMapper
{
    /// <summary>
    ///     Represents the base class for mapping between source and target types.
    /// </summary>
    /// <remarks>
    ///     This class holds the mapping information between properties of source and target types.
    /// </remarks>
    public abstract class MapBase
    {
        /// <summary>
        ///     Gets the dictionary that maps properties of the source type to properties of the target type.
        /// </summary>
        /// <value>
        ///     The dictionary where the key is a property of the source type and the value is a corresponding property of the
        ///     target type.
        /// </value>
        internal readonly Dictionary<PropertyInfo, TargetInfo> Properties;

        /// <summary>
        ///     Gets the type of the source object in the mapping.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Type" /> of the source object.
        /// </value>
        internal Type SourceType { get; }

        /// <summary>
        ///     Gets the type of the target object in the mapping.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Type" /> of the target object.
        /// </value>
        internal Type TargetType { get; }


        #if NET8_0_OR_GREATER
        /// <summary>
        /// Gets a value indicating whether the target type is a record type.
        /// </summary>
        /// <value>
        /// <c>true</c> if the target type is a record type; otherwise, <c>false</c>.
        /// </value>
        internal bool IsRecordTarget { get; }


        #endif


        #if NET8_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="MapBase"/> class.
        /// </summary>
        /// <param name="map">The dictionary that maps properties of the source type to properties of the target type.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <param name="isRecord">A boolean value indicating whether the target object is a record type.</param>
        protected MapBase(Dictionary<PropertyInfo, TargetInfo> map, Type sourceType, Type targetType, bool isRecord)
        {
            Properties = map;
            SourceType = sourceType;
            TargetType = targetType;
            IsRecordTarget = isRecord;
        }


        /// <summary>
        /// Builds a map between the properties of the source and target types.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDest">The type of the target object.</typeparam>
        /// <param name="instance">An instance of the source type.</param>
        /// <param name="target">An instance of the target type.</param>
        /// <returns>A map that contains the properties of the source type as keys and the corresponding properties of the target type as values.</returns>
        /// <remarks>
        /// This method uses reflection to get the properties of the source and target types. It then creates a dictionary that maps the properties of the source type to the corresponding properties of the target type.
        /// </remarks>
        internal static Map<TSource, TDest> BuildMap<TSource, TDest>(TSource instance, TDest target) where TSource : class
                                                                                                     where TDest : class, new()
        {
            
            var src = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)
                              .ToArray();
            var dst = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)
                            .ToArray();
            var pDict = new Dictionary<PropertyInfo, TargetInfo>();
            foreach (var property in src)
            {

                if (property.GetCustomAttribute<SkipMappingAttribute>() != null)
                {
                    continue;
                }
                
                var matchingProperty =
                    dst.FirstOrDefault(p => p.Name == property.Name);
                
                if (matchingProperty != null && property.CanWrite)
                {
                    if (matchingProperty.GetCustomAttribute<SkipMappingAttribute>() != null)
                    {
                        continue;
                    }
                    
                    if (property.PropertyType == matchingProperty.PropertyType)
                    {
                        pDict.Add(property, new TargetInfo(matchingProperty, false));
                        continue;
                    }

                    if (property.PropertyType != matchingProperty.PropertyType && property.PropertyType.IsClass &&
                        matchingProperty.PropertyType.IsClass)
                    {
                        var tS = Activator.CreateInstance(property.PropertyType);
                        var tD = Activator.CreateInstance(matchingProperty.PropertyType);
                        if (null != tS && null != tD)
                        {
                            var internalMap = Mapper.Instance.FindMapping(tS, tD);
                            if (internalMap?.Properties.Count > 0)
                            {
                                pDict.Add(property, new TargetInfo(matchingProperty, true));
                            }
                        }
                        
                    }
                    
                }
                
                
            }

            return new Map<TSource, TDest>(pDict,false);
        }

      


        /// <summary>
        /// Builds a mapping record for the specified source and destination types.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDest">The type of the destination object.</typeparam>
        /// <param name="instance">An instance of the source type.</param>
        /// <returns>A <see cref="RecordMap{TSource, TDest}"/> object representing the mapping record.</returns>
        /// <exception cref="ArgumentException">Thrown when unable to create a constructor map for the destination record type or unable to build a map to convert the source type to the destination type.</exception>
        /// <remarks>
        /// This method builds a mapping record by matching the properties of the source type with the parameters of the constructors of the destination type. If a match is found for all parameters of a constructor, a mapping record is created and returned. If no match is found for any constructor, an exception is thrown.
        /// </remarks>
        internal static RecordMap<TSource, TDest> BuildRecordMap<TSource, TDest>(TSource instance) where TSource : class
            where TDest : class
        {
            var srcType = instance.GetType();
            var tgtType = typeof(TDest);
            var src = srcType.GetProperties(BindingFlags.Public | BindingFlags.Instance)

                             .Where(x => x.CanRead)
                             .ToArray();
            
            
            var targetCtors = tgtType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            
            foreach (var constructorInfo in targetCtors)
            {
                var dic = new Dictionary<PropertyInfo, TargetInfo>();
                var parameters = constructorInfo.GetParameters();
                foreach (var parameter in parameters)
                {
                    var s = src.FirstOrDefault(x => x.PropertyType == parameter.ParameterType &&
                                                    x.Name         == parameter.Name);
                    if (null != s)
                    {
                        dic.Add(s,TargetInfo.Empty);
                    }
                }

                if (dic.Count == parameters.Length)
                {
                    //found, exit

                    Func<TSource, ConstructorInfo, Dictionary<PropertyInfo, TargetInfo>, TDest>
                        ctorExpression = (source, ctr, infos) =>
                        {
                            var l = new List<object?>();
                            foreach (var (key, value) in infos)
                            {
                                object? o = key.GetValue(source);
                                l.Add(o);
                            }

                            var r = ctr.Invoke(l.ToArray());
                            
                            return (TDest)r;
                            
                           
                        };

                    return new RecordMap<TSource, TDest>(dic, srcType, typeof(TDest), ctorExpression, constructorInfo);
                }
                else
                {
                    dic.Clear();
                }
            }



            throw new ArgumentException($"Unable to create constructor map for destination record type '{tgtType.Name}'" 
                                        , new ArgumentException($"Unable to build a map for convert { srcType.Name } to {tgtType.Name }",
                                                                tgtType.Name));



        }

        #else


        /// <summary>
        ///     Initializes a new instance of the <see cref="MapBase" /> class.
        /// </summary>
        /// <param name="map">The dictionary that maps properties of the source type to properties of the target type.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
        protected MapBase(Dictionary<PropertyInfo, TargetInfo> map, Type sourceType, Type targetType)
        {
            Properties = map;
            SourceType = sourceType;
            TargetType = targetType;
        }


        /// <summary>
        ///     Builds a map between the properties of the source and target types.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDest">The type of the target object.</typeparam>
        /// <param name="instance">An instance of the source type.</param>
        /// <param name="target">An instance of the target type.</param>
        /// <returns>
        ///     A map that contains the properties of the source type as keys and the corresponding properties of the target
        ///     type as values.
        /// </returns>
        /// <remarks>
        ///     This method uses reflection to get the properties of the source and target types. It then creates a dictionary that
        ///     maps the properties of the source type to the corresponding properties of the target type.
        /// </remarks>
        internal static Map<TSource, TDest> BuildMap<TSource, TDest>(TSource instance, TDest target)
            where TSource : class
            where TDest : class, new()
        {
            var src = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)
                              .ToArray();
            var dst = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)
                            .ToArray();
            var pDict = new Dictionary<PropertyInfo, TargetInfo>();
            foreach (var property in src)
            {
                if (property.GetCustomAttribute<SkipMappingAttribute>() != null)
                {
                    continue;
                }
                
                var matchingProperty =
                    dst.FirstOrDefault(p => p.Name == property.Name);

                if (matchingProperty != null && property.CanWrite)
                {
                    if (matchingProperty.GetCustomAttribute<SkipMappingAttribute>() != null)
                    {
                        continue;
                    }
                    
                    if (property.PropertyType == matchingProperty.PropertyType)
                    {
                        pDict.Add(property, new TargetInfo(matchingProperty, false));
                        continue;
                    }

                    if (property.PropertyType != matchingProperty.PropertyType && property.PropertyType.IsClass &&
                        matchingProperty.PropertyType.IsClass)
                    {
                        var tS = Activator.CreateInstance(property.PropertyType);
                        var tD = Activator.CreateInstance(matchingProperty.PropertyType);
                        if (null != tS && null != tD)
                        {
                            var internalMap = Mapper.Instance.FindMapping(tS, tD);
                            if (internalMap?.Properties.Count > 0)
                            {
                                pDict.Add(property, new TargetInfo(matchingProperty, true));
                            }
                        }
                    }
                }
            }

            return new Map<TSource, TDest>(pDict);
        }

        #endif
    }

    /// <summary>
    ///     Represents the target information for the mapping process.
    /// </summary>
    /// <remarks>
    ///     This class holds the information about the target property and whether the source and target are complex types.
    /// </remarks>
    public class TargetInfo
    {
        public TargetInfo(PropertyInfo target, bool isComplextSourceAndTarget) : this(isComplextSourceAndTarget)
        {
            Target = target;
        }

        private TargetInfo(bool isComplextSourceAndTarget)
        {
            IsComplextSourceAndTarget = isComplextSourceAndTarget;
            Target                    = null;
        }

        internal static TargetInfo Empty => new TargetInfo(false);


        /// <summary>
        ///     Gets or sets the target property information.
        /// </summary>
        /// <value>
        ///     The <see cref="PropertyInfo" /> instance representing the target property in the mapping process.
        /// </value>
        public PropertyInfo Target { get;  }

        /// <summary>
        ///     Gets or sets a value indicating whether both the source and target are complex types.
        /// </summary>
        /// <value>
        ///     <c>true</c> if both the source and target are complex types; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplextSourceAndTarget { get;  }
    }
}
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PH.TinyMapper
{

 
    
    /// <summary>
    /// The Mapper class is a Singleton class that provides functionality to map objects of one type to another.
    /// </summary>
    /// <remarks>
    /// This class holds an array of MapBase objects that define the mappings between source and target types.
    /// The Map method can be used to map a source object to a target object based on the defined mappings.
    /// </remarks>
    /// <example>
    ///
    /// <code>
    /// var mapper = PH.TinyMapper.Mapper.Instance;
    /// var now = DateTime.UtcNow;
    /// var source      = new Somesource { Id = Guid.NewGuid(), Name = "John Doe", DateTimeNullable = null };
    /// var destination = new SomeDestination() { DateTimeNullable = now, DateTimeNullable2 = now };
    /// mapper.Map(source, destination);
    /// //now destination.Name are 'John Doe' and destination.Id is the same Guid of source.Id
    /// // and desination.DateTimeNullable is null.
    /// </code>
    /// </example>
    public partial class Mapper
    {

        #if NET8_0_OR_GREATER

        private static Mapper? _instance;


        #else

        private static Mapper _instance;

        #endif
        

        
        private static readonly object Lock = new object();

        /// <summary>
        /// Gets the singleton instance of the Mapper class.
        /// </summary>
        /// <value>
        /// The singleton instance of the Mapper class.
        /// </value>
        /// <remarks>
        /// This property is thread-safe and will always return the same instance of the Mapper class. 
        /// It uses the double-check locking pattern to ensure that the instance is created only once when it's needed.
        /// </remarks>
        public static Mapper Instance => GetInstance();

        /// <summary>
        /// Gets the singleton instance of the Mapper class.
        /// </summary>
        /// <returns>
        /// The singleton instance of the Mapper class.
        /// </returns>
        /// <remarks>
        /// This method uses the double-check locking pattern to ensure that the instance is created only once when it's needed.
        /// </remarks>
        private static Mapper GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    _instance = new Mapper();
                }
            }

            return _instance;
        }


        /// <summary>
        /// An array of MapBase objects that holds the mappings between source and target types.
        /// </summary>
        /// <remarks>
        /// This array is used to store all the mappings added to the Mapper. Each mapping is represented by a MapBase object.
        /// </remarks>
        internal MapBase[] MapArray;

        /// <summary>
        /// Gets the unique identifier for the Mapper instance.
        /// </summary>
        /// <value>
        /// The unique identifier for the Mapper instance.
        /// </value>
        /// <remarks>
        /// This property is read-only. The identifier is generated when the Mapper instance is created and cannot be changed.
        /// </remarks>
        public Guid Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is private as the <see cref="Mapper"/> class implements the Singleton pattern. 
        /// Use the <see cref="Instance"/> property to get the instance of this class.
        /// </remarks>
        private Mapper()
        {
            MapArray = Array.Empty<MapBase>();
            Id       = Guid.NewGuid();
        }

        /// <summary>
        /// Adds a mapping between a source type and a target type to the MapArray.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="map">The map object that defines the mapping between the source and target types.</param>
        /// <remarks>
        /// This method is used to add a new mapping to the MapArray. The mapping is defined by the provided Map object.
        /// </remarks>
        public void AddMapping<TSource, TTarget>(Map<TSource, TTarget> map) where TSource : class
                                                                        where TTarget : class, new()
        {
            #if NET8_0_OR_GREATER
            
            MapArray = [.. MapArray, map];
            
            #else
            
            var l = new List<MapBase>();
            l.AddRange(MapArray);
            l.Add(map);
            
            MapArray = l.ToArray();

            #endif

        }

        #if NET8_0_OR_GREATER

        internal void AddRecordMap<TSource, TRecord>(RecordMap<TSource, TRecord> map) where TSource : class
                                                                    where TRecord : class
        {
            MapArray = [.. MapArray, map];
        }

        #endif

        /// <summary>
        /// Finds a mapping between the source and target types.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object. Must be a class.</typeparam>
        /// <typeparam name="TTarget">The type of the target object. Must be a class and have a parameterless constructor.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <returns>
        /// Returns a <see cref="MapBase"/> object that represents the mapping between the source and target types.
        /// If no mapping exists, a new one is created, added to the MapArray, and then returned.
        /// </returns>
        internal MapBase FindMapping<TSource, TTarget>(TSource source, TTarget target) where TSource : class
            where TTarget : class, new()
        {
            var sourceType = source.GetType();
            var destType   = target.GetType();
            
            var map = MapArray.FirstOrDefault(m => m.SourceType == sourceType && m.TargetType == destType);
            if (null == map)
            {
                var mymap = MapBase.BuildMap<TSource, TTarget>(source, target);
                AddMapping(mymap);
                return mymap;
            }
            return map;
        }

        #if NET8_0_OR_GREATER

        /// <summary>
        /// Maps the properties of a source object to a new target object of a different type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A new target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to a new target object.
        /// If the source object is null, the method returns null.
        /// </remarks>
        public TTarget? Map<TSource, TTarget>(TSource? source) where TSource : class
                                                               where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }
            var target = new TTarget();
            target = Map<TSource, TTarget>(source, target);
            return target;
        }

        

        /// <summary>
        /// Maps the properties of a source object to a target object and performs an action after the mapping.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="afterMap">An action to perform after the mapping. The action takes the source object and the mapped target object as parameters.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method returns null.
        /// The provided action is invoked after the mapping with the source object and the mapped target object as parameters.
        /// </remarks>
        public TTarget? Map<TSource, TTarget>(TSource? source, Action<TSource?, TTarget?>? afterMap) where TSource : class
                                                               where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }

            var target = new TTarget();
            target = Map<TSource, TTarget>(source, target);
            afterMap?.Invoke(source, target);
            return target;
        }
        


        /// <summary>
        /// Maps the properties of a source object to a target object.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method sets the target object to null and returns null.
        /// </remarks>
        public TTarget? Map<TSource, TTarget>(TSource? source, TTarget? target) where TSource : class
                                                                               where TTarget : class, new() =>
            Map(source,  target, null);

        /// <summary>
        /// Maps the properties of a source object to a target object and performs an optional action after mapping.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="afterMap">An optional action to perform after mapping. The action takes the source and target objects as parameters.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method sets the target object to null and returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method sets the target object to null and returns null.
        /// If an action is provided in the afterMap parameter, it is invoked after the mapping process.
        /// </remarks>
        public TTarget? Map<TSource, TTarget>(TSource? source,  TTarget? target, Action<TSource?, TTarget?>? afterMap)
            where TSource : class
            where TTarget : class, new()
        {
            if (null == source)
            {
                target = null;
                return target;
            }
            else
            {
                target ??= new TTarget();

                var map = FindMapping(source, target);
                foreach (var (key, value) in map.Properties)
                {
                    var src = key.GetValue(source);
                    if (value.IsComplextSourceAndTarget)
                    {
                        var t    = Activator.CreateInstance(value.Target.PropertyType);
                        var cplx = Map(src, t);
                        value.Target.SetValue(target,cplx);
                    }
                    else
                    {
                        value.Target.SetValue(target, src);     
                    }
                   
                }
            }
            
            
            if (null != afterMap)
            {
                afterMap.Invoke(source, target);
            }

            return target;
        }
        
        #else
        
        
        /// <summary>
        /// Maps the properties of a source object to a new target object of a different type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A new target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to a new target object.
        /// If the source object is null, the method returns null.
        /// </remarks>
        public TTarget Map<TSource, TTarget>(TSource source) where TSource : class
                                                               where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }
            var target = new TTarget();
            target = Map<TSource, TTarget>(source, target);
            return target;
        }

        /// <summary>
        /// Maps the properties of a source object to a target object and performs an action after the mapping.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="afterMap">An action to perform after the mapping. The action takes the source object and the mapped target object as parameters.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method returns null.
        /// The provided action is invoked after the mapping with the source object and the mapped target object as parameters.
        /// </remarks>
        public TTarget Map<TSource, TTarget>(TSource source, Action<TSource, TTarget> afterMap) where TSource : class
                                                               where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }

            var target = new TTarget();
            target = Map<TSource, TTarget>(source, target);
            afterMap?.Invoke(source, target);
            return target;
        }
        
        
        /// <summary>
        /// Maps the properties of a source object to a target object.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method sets the target object to null and returns null.
        /// </remarks>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target) where TSource : class
                                                                               where TTarget : class, new() =>
            Map(source,  target, null);

        /// <summary>
        /// Maps the properties of a source object to a target object and performs an optional action after mapping.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="afterMap">An optional action to perform after mapping. The action takes the source and target objects as parameters.</param>
        /// <returns>The target object with mapped properties from the source object. If the source object is null, the method sets the target object to null and returns null.</returns>
        /// <remarks>
        /// This method uses the mappings defined in the MapArray to map the properties of the source object to the target object.
        /// If the source object is null, the method sets the target object to null and returns null.
        /// If an action is provided in the afterMap parameter, it is invoked after the mapping process.
        /// </remarks>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<TSource, TTarget> afterMap)
            where TSource : class
            where TTarget : class, new()
        {
            if (null == source)
            {
                target = null;
                return target;
            }
            else
            {
                if (null == target)
                {
                    target = new TTarget();
                }

                var map = FindMapping(source, target);

                foreach (var keyValuePair in map.Properties)
                {
                    var src = keyValuePair.Key.GetValue(source);
                    if (keyValuePair.Value.IsComplextSourceAndTarget)
                    {
                        var t    = Activator.CreateInstance(keyValuePair.Value.Target.PropertyType);
                        var cplx = Map(src, t);
                        keyValuePair.Value.Target.SetValue(target, cplx);
                    }
                    else
                    {
                        keyValuePair.Value.Target.SetValue(target, src);
                    }
                    
                    
                }
               
               
                
            }
            
            
            if (null != afterMap)
            {
                afterMap.Invoke(source, target);
            }

            return target;
        }
                
        #endif
        
        
        
        

    }
}

using System;

namespace PH.TinyMapper
{

    /// <summary>
    /// The SkipMappingAttribute class is used to indicate that a specific property should not be included in the mapping process.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a property within a class to exclude it from being mapped by the TinyMapper.
    /// </remarks>
    /// <example>
    /// Here is an example of how to use this attribute:
    /// <code>
    /// public class ExampleClass
    /// {
    ///     [SkipMapping]
    ///     public int PropertyToSkip { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SkipMappingAttribute : Attribute
    {
    }

}
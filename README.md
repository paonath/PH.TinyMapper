# PH.TinyMapper

A c# tiny object mapper 

The mapper remaps properties with the same name between source and destination (if these have the same type or if they are complex properties and a mapping between left and right is possible)

## Code Sample

### Copy Property from source to target
```csharp
 //first define a class for Source:
public class Somesource
{
    public Guid Id { get; set; }

    public DateTime? DateTimeNullable { get; set; }
    public string    Name             { get; set; }

    public DateTime UtcNow { get; }
}

// then a class for Target:
public class SomeDestination
{
    public Guid Id { get; set; }

    public DateTime? DateTimeNullable { get; set; }
    public string    Name             { get; set; }
    public string    LastName         { get; set; }

    public DateTime? DateTimeNullable2 { get; set; }
}


//Init a Mapper            
var mapper      = Mapper.Instance;
var now         = DateTime.UtcNow;
            
 var source      = new Somesource { Id = Guid.NewGuid(), Name = "John Doe", DateTimeNullable = null };
 var destination = new SomeDestination(){ DateTimeNullable = now, DateTimeNullable2 = now };
 
 
 mapper.Map(source, destination);
 
 //now destination.Name are 'John Doe' and destination.Id is the same Guid of source.Id
 // and desination.DateTimeNullable is null.
            
            

```

### Create a new instance of target class within mapper
```csharp
var src = new Somesource { Id = Guid.NewGuid(), Name = "John Doe" };
//create a new instance of target class within mapper:
var dst = PH.TinyMapper.Mapper.Instance.Map<Somesource, SomeDestination>(src);

```
### Skipping properties

If you need to exclude a source or destination property you need to decorate it with the `SkipMappingAttribute` attribute

```csharp
public class DestinationOrSourceWithSkip 
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
    
}
```
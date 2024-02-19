using System.Diagnostics;
using PH.TinyMapper.TestCommon;

namespace PH.TinyMapper.Net8
{
    public class Net8Tests
    {
       
        [Fact]
        public void IfSOurceIsNullWillReturnNullTarget()
        {
            Source?      s = null;
            Destination? d = new Destination() { Id = Guid.NewGuid(), LastName = "last name", Name = "first name" };

            var mapper = Mapper.Instance;
            
            d = mapper.Map<Source, Destination>(s,  d);

            
            Assert.Null(d);
            
        }

        [Fact]
        public void SameMapAreNotRebuilt()
        {
            var id = Guid.NewGuid();
            
            var         src = new Source { Id = id , Name = "a sample string" };
            Destination? tgt = null;

            tgt = Mapper.Instance.Map<Source, Destination>(src,  tgt);

            Guid mapperId = Mapper.Instance.Id;
            
            Assert.Equal(src.Id, tgt?.Id);
            Assert.Equal(src.Name, tgt?.Name);
            
            var newMapper = Mapper.Instance;

            var secondId = newMapper.Id;

            Source      newSrc  = new Source { Id        = Guid.NewGuid(), Name = "a string" };
            var         lastNameExample = newSrc.GetLastNameExample();
            
            Destination? newDest = new Destination() { Id = id, Name             = "not set", LastName = "not set" };
            
            Assert.Equal(mapperId, secondId);

            newMapper.Map(newSrc,  newDest, (source, destination) =>
            {
                if (null != destination)
                {
                    destination.LastName = $"LastNameOf {source?.UtcNow:yy-MM-dd}";    
                    destination.DateTimeNullable2 = DateTime.Now;
                }
                
            });


            var date = DateTime.UtcNow;
            src.DateTimeNullable = date;

            var last = newMapper.Map<Source, Destination>(src);
            
            
            Assert.NotNull(newDest);
            Assert.Equal(lastNameExample , newDest?.LastName);
            Assert.True(newDest?.DateTimeNullable2.HasValue);
            
            Assert.True(last?.DateTimeNullable.HasValue);
            Assert.Equal(date , last?.DateTimeNullable.GetValueOrDefault());
        }

        [Fact]
        public void IfPropertyAreSignedWithSKipWillNotMapped()
        {
            var mapper      = Mapper.Instance;
            var source      = new Source { Id = Guid.NewGuid(), Name = "John Doe" };
            var notSkipped  = new Destination();
            var destination = new DestinationWithSkip();
            mapper.Map(source, destination);
            mapper.Map(source, notSkipped);
            

            Assert.NotNull(notSkipped);
            Assert.NotNull(destination);
            Assert.Equal("John Doe",notSkipped.Name);
            Assert.Null(destination.Name);
            Assert.Equal(source.Id, notSkipped.Id);
            Assert.Equal(source.Id, destination.Id);
        }

        [Fact]
        public void TestingSpeed()
        {
            int last        = 1000000;
            Destination? destination = null;

            Stopwatch s1 = new Stopwatch();
            s1.Start();
            for (int i = 0; i < last; i++)
            {
                
                destination = Mapper.Instance.Map(new Source() { Id = Guid.NewGuid(), Name = "Fake" },  destination,
                                    (s, d) => { d.LastName = $"{i}"; });


            }
            s1.Stop();
            Console.WriteLine("Mapper on for Elapsed {0}", s1.Elapsed);

            Stopwatch s2 = new Stopwatch();
            s2.Start();
            var       largeAmountOfSource = GetAmountOfSource(last);
            s2.Stop();
            
            
            
            
            Stopwatch stopwatch           = new Stopwatch();
            stopwatch.Start();
            var       largeAmountOfDests  = new LargeDestination[last];
            int       c                   = 0;
            foreach (var source in largeAmountOfSource)
            {
                largeAmountOfDests[c] = Mapper.Instance.Map<LargeSource,LargeDestination >(largeAmountOfSource[c]);
                c++;
            }

            stopwatch.Stop();

            var result1 = stopwatch.Elapsed;
           
            

            
            Assert.True(destination.LastName == $"{last-1}");


        }


        [Fact]
        public void TestARecord()
        {
            var id  = Guid.NewGuid();
            var src = new Source { Id = id, Name = "a sample string" };

            var src2 = new Source() { Id = Guid.Empty, Name = "A fake name" , DateTimeNullable = DateTime.Now};
           
            var dest       = Mapper.Instance.Record<Source, ExampleRecord>(src);
            var dest2      = Mapper.Instance.Record<Source, ExampleRecord>(src2);
            var nullRecord = Mapper.Instance.Record<Source, ExampleRecord>((Source?)null);
            
            
            Assert.Null(nullRecord);
            Assert.NotNull(dest);
            Assert.NotNull(dest2);
            Assert.Equal(id, dest.Id);
            Assert.Equal("a sample string" , dest.Name);
            Assert.Equal(src.UtcNow, dest.UtcNow);
            Assert.Null(dest.DateTimeNullable);
            Assert.False(dest.DateTimeNullable.HasValue);
            Assert.Equal(src2.Name, dest2.Name);
            Assert.NotNull(dest2.DateTimeNullable);
            Assert.True(dest2.DateTimeNullable.HasValue);
            Assert.Equal(src2.DateTimeNullable.Value, dest2.DateTimeNullable.Value);
                        
        }

        [Fact]
        public void TestComplexType()
        {
            var m = Mapper.Instance;
            Guid id = Guid.NewGuid();
            Guid idInternal = Guid.NewGuid();

            var s = new ComplexSource()
            {
                Id   = id,
                Name = "A name",
                Some = new Source()
                {
                    Id               = idInternal,
                    Name             = "Internal Name",
                    DateTimeNullable = DateTime.UtcNow
                }
            };

            var t = m.Map<ComplexSource, ComplexDestination>(s);
            
            Assert.NotNull(t);
            Assert.NotNull(t.Some);
            Assert.Equal(idInternal,t.Some.Id);


            var r = m.Map<ComplexSource, MoreComplexDestination>(s, (source, destination) =>
            {
                
            });
            
            Assert.NotNull(r.Some);
            Assert.Equal(typeof(Destination),r.Some.GetType());
            Assert.Equal(s.Some.Id, r.Some.Id);
            

        }


        [Fact]
        public void BadSourceObjectWillThrowAnExceptionFromRecord()
        {
            var bad = new BadClass() { Username = "me" };

            Exception ex = null;
            try
            {
                var t = Mapper.Instance.Record<BadClass, ExampleRecord>(bad);
            }
            catch (Exception e)
            {
                ex = e;
            }
            
            Assert.NotNull(ex);

        }

        class BadClass
        {
            public string Username { get; set; }
        }


        private LargeSource[] GetAmountOfSource(int amount)
        {
            var l = new LargeSource[amount];
            for (int i = 0; i < amount; i++)
            {
                l[i] = new LargeSource()
                {
                    Id               = Guid.NewGuid(),
                    Name             = $"Fake no {i}",
                    DateTimeNullable = DateTime.UtcNow,
                    Str1             = $"value {i} - 1",
                    Str2             = $"value {i} - 2",
                    Str3             = $"value {i} - 3",
                    Str4             = $"value {i} - 4",
                    Str5             = $"value {i} - 5",
                    Str6             = $"value {i} - 6",
                    Str7             = $"value {i} - 7",
                    Str8             = $"value {i} - 8",
                    Str9             = $"value {i} - 9",
                    Str10            = $"value {i} - 10",
                    Str11            = $"value {i} - 11",
                    Str12            = $"value {i} - 12",
                    Str13            = $"value {i} - 13",
                    Str14            = $"value {i} - 14",
                    Str15            = $"value {i} - 15",
                    Str16            = $"value {i} - 16",
                    Str17            = $"value {i} - 17",
                    Str18            = $"value {i} - 18",
                    Str19            = $"value {i} - 19",
                    Str20            = $"value {i} - 20",
                    Str21            = $"value {i} - 21",
                    Str22            = $"value {i} - 22",
                    Str23            = $"value {i} - 23",
                    Str24            = $"value {i} - 24",
                    Str25            = $"value {i} - 25",
                    Str26            = $"value {i} - 26",
                    Str27            = $"value {i} - 27",
                    Str28            = $"value {i} - 28",
                    Str29            = $"value {i} - 29",
                    Str30            = $"value {i} - 30",
                    Str31            = $"value {i} - 31",
                    Str32            = $"value {i} - 32",
                    Str33            = $"value {i} - 33",
                    Str34            = $"value {i} - 34",
                    Str35            = $"value {i} - 35",
                    Str36            = $"value {i} - 36",
                    Str37            = $"value {i} - 37",
                    Str38            = $"value {i} - 38",
                    Str39            = $"value {i} - 39",
                    Str40            = $"value {i} - 40",
                    Str41            = $"value {i} - 41",
                    Str42            = $"value {i} - 42",
                    Str43            = $"value {i} - 43",
                    Str44            = $"value {i} - 44",
                    Str45            = $"value {i} - 45",
                    Str46            = $"value {i} - 46",
                    Str47            = $"value {i} - 47",
                    Str48            = $"value {i} - 48",
                    Str49            = $"value {i} - 49",
                    Str50            = $"value {i} - 50",
                };
            }

            return l;
        }

        [Fact]
        public void WriteCodeSampleForReadme()
        {
            //first define a class for Source:
            
            
            
            var mapper = PH.TinyMapper.Mapper.Instance;
            var now    = DateTime.UtcNow;
            
            var source      = new Somesource { Id = Guid.NewGuid(), Name = "John Doe", DateTimeNullable = null };
            var destination = new SomeDestination(){ DateTimeNullable = now, DateTimeNullable2 = now };
            mapper.Map(source, destination);
            
            //now destination.Name are 'John Doe' and destination.Id is the same Guid of source.Id
            // and desination.DateTimeNullable is null.
            
            
            Assert.NotNull(destination);
            Assert.Equal("John Doe", destination.Name);
            Assert.Equal(source.Id, destination.Id);
            Assert.Null(destination.DateTimeNullable);


            var src = new Somesource { Id = Guid.NewGuid(), Name = "John Doe" };
            //create a new instance of target class within mapper:
            var dst = PH.TinyMapper.Mapper.Instance.Map<Somesource, SomeDestination>(src);


            Assert.NotNull(destination);
            Assert.Equal("John Doe", dst.Name);
            Assert.Equal(src.Id, dst.Id);
            Assert.Null(dst.DateTimeNullable);

        }
    }

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
}
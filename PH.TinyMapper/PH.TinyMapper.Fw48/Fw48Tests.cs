using Microsoft.VisualStudio.TestTools.UnitTesting;
using PH.TinyMapper.TestCommon;
using System;
using System.Diagnostics;

namespace PH.TinyMapper.Fw48
{
    [TestClass]
    public class Fw48Tests
    {
        [TestMethod]
        public void IfSOurceIsNullWillReturnNullTarget()
        {
            Source      s = null;
            Destination d = new Destination() { Id = Guid.NewGuid(), LastName = "last name", Name = "first name" };

            var mapper = Mapper.Instance;
            
            d = mapper.Map(s,  d);

            
            Assert.IsNull(d);
            
        }

        [TestMethod]
        public void SameMapAreNotRebuilt()
        {
            var id = Guid.NewGuid();
            
            var         src = new Source { Id = id , Name = "a sample string" };
            Destination tgt = null;

            tgt = Mapper.Instance.Map<Source, Destination>(src,  tgt);

            Guid mapperId = Mapper.Instance.Id;
            
            
            
            var newMapper = Mapper.Instance;

            var secondId = newMapper.Id;

            Source      newSrc  = new Source { Id        = Guid.NewGuid(), Name = "a string" };
            var         lastNameExample = newSrc.GetLastNameExample();
            
            Destination newDest = new Destination() { Id = id, Name             = "not set", LastName = "not set" };
            
            Assert.AreEqual(src.Id, tgt?.Id);
            Assert.AreEqual(src.Name, tgt?.Name);
            Assert.AreEqual(mapperId, secondId);

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
            
            
            Assert.IsNotNull(newDest);
            Assert.AreEqual(lastNameExample , newDest?.LastName);
            Assert.IsTrue(newDest?.DateTimeNullable2.HasValue);
            
            Assert.IsTrue(last?.DateTimeNullable.HasValue);
            Assert.AreEqual(date , last?.DateTimeNullable.GetValueOrDefault());
        }

        [TestMethod]
        public void IfPropertyAreSignedWithSKipWillNotMapped()
        {
            var mapper      = Mapper.Instance;
            var source      = new Source { Id = Guid.NewGuid(), Name = "John Doe" };
            var notSkipped  = new Destination();
            var destination = new DestinationWithSkip();
            mapper.Map(source, destination);
            mapper.Map(source, notSkipped);
            

            Assert.IsNotNull(notSkipped);
            Assert.IsNotNull(destination);
            Assert.AreEqual("John Doe",notSkipped.Name);
            Assert.IsNull(destination.Name);
            Assert.AreEqual(source.Id, notSkipped.Id);
            Assert.AreEqual(source.Id, destination.Id);
        }

        [TestMethod]
        public void TestingSpeed()
        {
            int last        = 1000000;
            Destination destination = null;

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
           
            

            
            Assert.IsTrue(destination.LastName == $"{last-1}");


        }


        

        [TestMethod]
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
            
            Assert.IsNotNull(t);
            Assert.IsNotNull(t.Some);
            Assert.AreEqual(idInternal,t.Some.Id);


            var r = m.Map<ComplexSource, MoreComplexDestination>(s, (source, destination) =>
            {
                
            });
            
            Assert.IsNotNull(r.Some);
            Assert.AreEqual(typeof(Destination),r.Some.GetType());
            Assert.AreEqual(s.Some.Id, r.Some.Id);
            

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
    }
}

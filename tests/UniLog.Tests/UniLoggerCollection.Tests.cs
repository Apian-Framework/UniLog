using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using Moq;
using UniLog;

namespace UniLogTests
{

    public class CollectionSingletonForTest : UniLoggerCollectionSingleton
    {
        public static UniLoggerCollection InstanceRef => _collectionInstance;
        public static void SetInstance(UniLoggerCollection newInstance)
        {
            _collectionInstance = newInstance;
        }
    }

    public class CollectionForTest : UniLoggerCollection
    {
        public Dictionary<string, UniLogger> Loggers => _loggers;
    }

    [TestFixture]
    public class UniLoggerCollectionSingletonTests
    {
        [Test]
        public void GetInstanceStuff()
        {
            const string name1 = "Fred";
            const string name2 = "Bob";
            Assert.That(CollectionSingletonForTest.InstanceRef, Is.Null);

            UniLogger newLogger = CollectionSingletonForTest.GetLogger(name1); // creates collection instance
            UniLoggerCollection inst = CollectionSingletonForTest.InstanceRef;
            Assert.That(inst, Is.Not.Null);

            UniLogger newLogger2 = CollectionSingletonForTest.GetLogger(name2); // creates collection instance
            UniLoggerCollection inst2 = CollectionSingletonForTest.InstanceRef;
            Assert.That(inst2, Is.Not.Null);
            Assert.That(inst, Is.EqualTo(inst2));
        }

    }

    [TestFixture]
    public class UniLoggerCollectionTests
    {
        [Test]
        public void CollectionWorks()
        {
            CollectionSingletonForTest.SetInstance(null); // unset
            Assert.That(CollectionSingletonForTest.InstanceRef, Is.Null);

            CollectionForTest ct = new CollectionForTest();
            CollectionSingletonForTest.SetInstance(ct);
            Assert.That(CollectionSingletonForTest.InstanceRef, Is.EqualTo(ct));

            Assert.That(ct.Loggers, Is.Not.Null);
            Assert.That(ct.Loggers.Count, Is.EqualTo(0));

            UniLogger bobLog = ct.GetLogger("Bob"); // adds
            Assert.That(ct.Loggers.Count, Is.EqualTo(1));

            ct.GetLogger("Fred"); // adds
            Assert.That(ct.Loggers.Count, Is.EqualTo(2));

            UniLogger bobLog2 = ct.GetLogger("Bob"); // fetches
            Assert.That(ct.Loggers.Count, Is.EqualTo(2));

            Assert.That(bobLog, Is.EqualTo(bobLog2));

        }

        // [Test]
        // public void AddLoggers()
        // {


        // }

    }

}
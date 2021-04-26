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
    [TestFixture]
    public class UniLoggerTests
    {
        [TestCase(UniLogger.Level.Debug, "Debug")]
        [TestCase(UniLogger.Level.Verbose, "Verbose")]
        [TestCase(UniLogger.Level.Info, "Info")]
        [TestCase(UniLogger.Level.Warn, "Warn")]
        [TestCase(UniLogger.Level.Error, "Error")]
        [TestCase(UniLogger.Level.Off, "Off")]
        public void LevelFromName_Works(UniLogger.Level lvl, string name)
        {
            Assert.That(UniLogger.LevelFromName(name), Is.EqualTo(lvl));
        }
    }

}
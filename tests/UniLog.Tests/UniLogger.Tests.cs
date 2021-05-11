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
        [TestCase(UniLogger.Level.Warn, "NoLevelNamedThis")]
        public void LevelFromName_Works(UniLogger.Level lvl, string name)
        {
            Assert.That(UniLogger.LevelFromName(name), Is.EqualTo(lvl));
        }

        [TestCase( "Debug1232234234", 12)]
        [TestCase("asdasddfghhjk", 3)]
        [TestCase( "12-232-45454-5656-33", 8)]
        public void StaticShortId(string fullId, int len)
        {
            Assert.That(UniLogger.SID(fullId, len), Is.EqualTo(fullId.Substring(0,len)));
        }


        [Test]
        public void StaticGetLogger()
        {
            UniLogger l = UniLogger.GetLogger("frob");
            Assert.That(l, Is.Not.Null);
            Assert.That(l.LoggerName, Is.EqualTo("frob"));
        }

        [Test]
        public void SetupLevels()
        {
	         Dictionary<string, string> loggers = new Dictionary<string, string>()
	        {
                {"DebugLogger","Debug"},
                {"ErrorLogger","Error"},
                {"VerboseLogger","Verbose"},
                {"WarnLogger","Warn"},
                {"InfoLogger","Info"},
            };

            UniLogger.SetupLevels(loggers);

            Assert.That(UniLogger.GetLogger("DebugLogger").LogLevel, Is.EqualTo(UniLogger.Level.Debug));
            Assert.That(UniLogger.GetLogger("ErrorLogger").LogLevel, Is.EqualTo(UniLogger.Level.Error));
            Assert.That(UniLogger.GetLogger("VerboseLogger").LogLevel, Is.EqualTo(UniLogger.Level.Verbose));
            Assert.That(UniLogger.GetLogger("WarnLogger").LogLevel, Is.EqualTo(UniLogger.Level.Warn));
            Assert.That(UniLogger.GetLogger("InfoLogger").LogLevel, Is.EqualTo(UniLogger.Level.Info));

        }



    }

}
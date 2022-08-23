using System.Linq;
using System.Collections.Generic;
using System;

namespace UniLog
{
    //
    // UniLogger
    //

    public class UniLogger
    {
        public enum Level
        {
            Debug = 10,
            Verbose = 20,
            Info = 30,
            Warn = 40,
            Error = 50,
            Off = 1000,
        }

        //
        // Statics
        //

        // Public API
        // ReSharper disable MemberCanBePrivate.Global,UnusedMember.Global,FieldCanBeMadeReadOnly.Global
        public static Dictionary<Level, string> LevelNames {get;} = new Dictionary<Level, string>()
        {
            {Level.Debug, "Debug"},
            {Level.Verbose, "Verbose"},
            {Level.Info, "Info"},
            {Level.Warn, "Warn"},
            {Level.Error, "Error"},
            {Level.Off, "Off"},
        };

        // 0 = name
        // 1 = level
        // 2 = message

        public static Level DefaultLevel = Level.Warn;
        public static bool DefaultThrowOnError = false;
        public static string DefaultTimeFormat = "[HH:mm:ss.fff] ";  //  [14:23:04.030] - note the trailing space
        //public static string DefaultTimeFormat = null;  // Set to null to not show time
        public static UniLogger GetLogger(string name)
        {
            return  UniLoggerCollectionSingleton.GetLogger(name);
        }

        public static IList<UniLogger> AllLoggers => UniLoggerCollectionSingleton.AllLoggers;

        public static Level LevelFromName(string name)
        {
            Level l = LevelNames.FirstOrDefault(x => x.Value == name).Key;
            return l==0 ? DefaultLevel : l;
        }

        public static void SetupLevels(Dictionary<string,string> levels)
        {
            foreach (string lName in levels.Keys)
            {
                GetLogger(lName).LogLevel = LevelNames.FirstOrDefault(x => x.Value == levels[lName]).Key;
            }
        }

        public static Dictionary<string, string> CurrentLoggerLevels()
        {
             return UniLoggerCollectionSingleton.AllLoggers.ToDictionary(l => l.LoggerName, l => UniLogger.LevelNames[l.LogLevel]);
        }

        public static string SID(string str, int len=8)  // "short ID" - just how the leftmost "n" chars of an id
        {
            return str == null ? "" : str.Substring(0, len);
        }

#if UNITY_2019_1_OR_NEWER

        public string DefaultFormat = "{1} {2}";

        //
        // Unity Implementation
        //

        protected UnityEngine.Logger unityLogger;

        public UniLogger(string name)
        {
            LoggerName = name;
            LogLevel = DefaultLevel;
            LogFormat = DefaultFormat;
            ThrowOnError = DefaultThrowOnError;
            TimeFormat = DefaultTimeFormat;
            unityLogger  = new UnityEngine.Logger(UnityEngine.Debug.unityLogger.logHandler);
        }

        // The unity formatting is a litte hinky because I'm trying to have warn, error, and info messages appear
        // the same (including timestamps.) I'm not putting the timestamp in an exceptio strin - tho I may change my mind
        private void _Write(string loggerName, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string timeStr = TimeFormat == null ? "" : DateTime.UtcNow.ToString(TimeFormat);
                string outMsg = string.Format(LogFormat, loggerName, LevelNames[lvl], msg);
                switch (lvl)
                {
                case Level.Debug:
                case Level.Verbose:
                case Level.Info:
                    unityLogger.Log($"{timeStr}{loggerName}:{outMsg}");
                    break;
                case Level.Warn:
                    unityLogger.LogWarning(timeStr+loggerName, outMsg);
                    break;
                case Level.Error:
                    if (ThrowOnError)
                        throw new Exception($"{loggerName}:{outMsg}");
                    else
                        unityLogger.LogError(timeStr+loggerName, outMsg);
                    break;
                }
            }
        }

#else
        //
        // Non-unity
        //

        public string DefaultFormat = "{0}{1}:{2} {3}"; // timestamp, loggerName, level, msg

        public UniLogger(string name)
        {
            LoggerName = name;
            LogLevel = DefaultLevel;
            LogFormat = DefaultFormat;
            ThrowOnError = DefaultThrowOnError;
            TimeFormat = DefaultTimeFormat;
        }

        private void _Write(string loggerName, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string timeStr = TimeFormat == null ? "" : DateTime.UtcNow.ToString(TimeFormat);
                string outMsg = string.Format(LogFormat, timeStr, loggerName, LevelNames[lvl], msg);

                if (lvl >= Level.Error && ThrowOnError)
                    throw new Exception(outMsg);
                else
                    Console.WriteLine(outMsg);
            }
        }

#endif

        // Instance
        public string LoggerName {get;}
        public Level LogLevel;
        public string LogFormat;
        public string TimeFormat;
        public bool ThrowOnError;

        public void Info(string msg) => _Write(LoggerName, Level.Info, msg);
        public void Verbose(string msg) => _Write(LoggerName, Level.Verbose, msg);
        public void Debug(string msg) => _Write(LoggerName, Level.Debug, msg);
        public void Warn(string msg) => _Write(LoggerName, Level.Warn, msg);
        public void Error(string msg) => _Write(LoggerName, Level.Error, msg);

        // End  API
        // ReSharper enable MemberCanBePrivate.Global,UnusedMember.Global,FieldCanBeMadeReadOnly.Global

    }

    public class UniLoggerCollection
    {
        protected readonly Dictionary<string, UniLogger> _loggers;

        public UniLoggerCollection()
        {
            _loggers = new Dictionary<string, UniLogger>();
        }

        public IList< UniLogger> AllLoggers => _loggers.Values.ToList();

        public UniLogger GetLogger(string name)
        {
            return _loggers.ContainsKey(name) ? _loggers[name] : _AddLogger(name);
        }

        private UniLogger _AddLogger(string name)
        {
            return _loggers[name] = new UniLogger(name);
        }
    }

    public class UniLoggerCollectionSingleton
    {
        // This class isn't really the singleton: the collection it manages is.
        protected static UniLoggerCollection _collectionInstance;

        public static IList<UniLogger> AllLoggers => _collectionInstance?.AllLoggers;

        public static UniLogger GetLogger(string name)
        {
            _collectionInstance = _collectionInstance ?? new UniLoggerCollection();
            return _collectionInstance.GetLogger(name);
        }
    }
}

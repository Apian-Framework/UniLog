using System.Linq;
using System.Collections.Generic;
using System;

#if UNITY_2019_1_OR_NEWER
using UnityEngine;
#endif

//
// TODO: this is a horrible mess of #if macros. Probably would be better to separate non-Unity,
//       Unity, and Unity running under the editor
//

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
        // Unity
        //

#if UNITY_EDITOR
        protected UnityEngine.Logger unityLogger;
#endif
        public UniLogger(string name)
        {
            LoggerName = name;
            LogLevel = DefaultLevel;
            LogFormat = DefaultFormat;
            ThrowOnError = DefaultThrowOnError;
            TimeFormat = DefaultTimeFormat;
#if UNITY_EDITOR
            // Running in the editor, this looger writes better messages
            unityLogger  = new UnityEngine.Logger(UnityEngine.Debug.unityLogger.logHandler);
#endif
        }

        // The unity formatting is a litte hinky because I'm trying to have warn, error, and info messages appear
        // the same (including timestamps.) I'm not putting the timestamp in an exceptio string - tho I may change my mind
        // ALSO: by using LogFormat() instead of Log() I can supress the stack trace that is otherwise included.
        // Turns out that logging itself is not necessarily much of a performance hit, but stack traces involve reflection,
        // and that is super slow.
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
#if UNITY_EDITOR
                    unityLogger.Log($"{timeStr}{loggerName}:{outMsg}");
#else
                     UnityEngine.Debug.LogFormat( LogType.Log, LogOption.NoStacktrace, null, "{0}", $"{timeStr}{loggerName}:{outMsg}");
#endif
                    break;
                case Level.Warn:
#if UNITY_EDITOR
                    unityLogger.LogWarning(timeStr+loggerName, outMsg);
#else
                    UnityEngine.Debug.LogFormat( LogType.Warning, LogOption.NoStacktrace, null, "{0}", $"{timeStr}{loggerName}:{outMsg}");
#endif
                    break;
                case Level.Error:
                    if (ThrowOnError)
                        throw new Exception($"{loggerName}:{outMsg}");
                    else
#if UNITY_EDITOR
                        unityLogger.LogError(timeStr+loggerName, outMsg);
#else
                        UnityEngine.Debug.LogFormat( LogType.Error, LogOption.None, null, "{0}", $"{timeStr}{loggerName}:{outMsg}");
#endif
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

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
        public enum LogLevel
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

        private static Dictionary<string, UniLogger> _loggers = new Dictionary<string, UniLogger>();

        public static Dictionary<LogLevel, string> LogLevelNames {get;} = new Dictionary<LogLevel, string>()
        {
            {LogLevel.Debug, "Debug"},
            {LogLevel.Verbose, "Verbose"},
            {LogLevel.Info, "Info"},
            {LogLevel.Warn, "Warn"},
            {LogLevel.Error, "Error"},
            {LogLevel.Off, "Off"},
        };


        //
        // Default values for new loggers. Changing these will not affect existing loggers.
        //

        public static LogLevel DefaultLogLevel = LogLevel.Warn;

        public static bool DefaultThrowOnError = false;
        // DefaultTimeFormat = null results in time not being shown

        public static string DefaultTimeFormat = "[HH:mm:ss.fff] ";  //  [14:23:04.030] - note the trailing space.

#if UNITY_2019_1_OR_NEWER
        public static string DefaultLogFormat = "{1} {2}"; //
#else
        public static string DefaultLogFormat = "{0}{1}:{2} {3}"; // timestamp, loggerName, level, msg
#endif

        public static UniLogger GetLogger(string name)
        {
            // if not in list, create/add/return a new one
            return _loggers.ContainsKey(name) ? _loggers[name] :  _loggers[name] = new UniLogger(name);
        }

        // Either clear the loggers dictionary, or set it to a pre-built list
        public static void Initialize(Dictionary<string, UniLogger> loggers = null)
        {
            _loggers = loggers ?? new Dictionary<string,UniLogger>();
        }

        public static IList<UniLogger> AllLoggers => _loggers.Values.ToList();

        public static LogLevel LogLevelFromName(string name)
        {
            LogLevel l = LogLevelNames.FirstOrDefault(x => x.Value == name).Key;
            return l==0 ? DefaultLogLevel : l;
        }

        public static void SetupLogLevels(Dictionary<string,string> levels)
        {
            foreach (string lName in levels.Keys)
            {
                LogLevel lvl = LogLevelNames.FirstOrDefault(x => x.Value == levels[lName]).Key;
                GetLogger(lName).CurrentLogLevel = LogLevelNames.FirstOrDefault(x => x.Value == levels[lName]).Key;
            }
        }

        public static Dictionary<string, string> CurrentLoggerLevels()
        {
             return AllLoggers.ToDictionary(l => l.LoggerName, l => LogLevelNames[l.CurrentLogLevel]);
        }

        // utility to create a "short ID" - the leftmost "n" chars of an id.
        public static string SID(string str, int len=8) =>  str == null ? "" : str.Substring(0, len);

#if UNITY_2019_1_OR_NEWER
        //
        // Unity
        //

#if UNITY_EDITOR
        protected UnityEngine.Logger unityLogger;
#endif
        public UniLogger(string name)
        {
            LoggerName = name;
            CurrentLogLevel = DefaultLogLevel;
            LogFormat = DefaultLogFormat;
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
        private void _Write(string loggerName, LogLevel lvl, string msg)
        {
            if (lvl >= CurrentLogLevel)
            {
                string timeStr = TimeFormat == null ? "" : DateTime.UtcNow.ToString(TimeFormat);
                string outMsg = string.Format(LogFormat, loggerName, LogLevelNames[lvl], msg);
                switch (lvl)
                {
                case LogLevel.Debug:
                case LogLevel.Verbose:
                case LogLevel.Info:
#if UNITY_EDITOR
                    unityLogger.Log($"{timeStr}{loggerName}:{outMsg}");
#else
                     UnityEngine.Debug.LogFormat( LogType.Log, LogOption.NoStacktrace, null, "{0}", $"{timeStr}{loggerName}:{outMsg}");
#endif
                    break;
                case LogLevel.Warn:
#if UNITY_EDITOR
                    unityLogger.LogWarning(timeStr+loggerName, outMsg);
#else
                    UnityEngine.Debug.LogFormat( LogType.Warning, LogOption.NoStacktrace, null, "{0}", $"{timeStr}{loggerName}:{outMsg}");
#endif
                    break;
                case LogLevel.Error:
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

        public UniLogger(string name)
        {
            LoggerName = name;
            CurrentLogLevel = DefaultLogLevel;
            LogFormat = DefaultLogFormat;
            ThrowOnError = DefaultThrowOnError;
            TimeFormat = DefaultTimeFormat;
        }

        private void _Write(string loggerName, LogLevel lvl, string msg)
        {
            if (lvl >= CurrentLogLevel)
            {
                string timeStr = TimeFormat == null ? "" : DateTime.UtcNow.ToString(TimeFormat);
                string outMsg = string.Format(LogFormat, timeStr, loggerName, LogLevelNames[lvl], msg);

                if (lvl >= LogLevel.Error && ThrowOnError)
                    throw new Exception(outMsg);
                else
                    Console.WriteLine(outMsg);
            }
        }
#endif


        // Logger Instance


        public string LoggerName {get;}
        public LogLevel CurrentLogLevel;
        public string LogFormat;
        public string TimeFormat;
        public bool ThrowOnError;

        public void Info(string msg) => _Write(LoggerName, LogLevel.Info, msg);
        public void Verbose(string msg) => _Write(LoggerName, LogLevel.Verbose, msg);
        public void Debug(string msg) => _Write(LoggerName, LogLevel.Debug, msg);
        public void Warn(string msg) => _Write(LoggerName, LogLevel.Warn, msg);
        public void Error(string msg) => _Write(LoggerName, LogLevel.Error, msg);

    }
}

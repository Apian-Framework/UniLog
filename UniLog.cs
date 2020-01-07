using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
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
            Info = 20,
            Warn = 30,         
            Error = 40,
            Off = 1000,            
        }

        //
        // Statics
        //
        public static Dictionary<Level, string> LevelNames = new Dictionary<Level, string>()
        {
            {Level.Debug, "Debug"},
            {Level.Info, "Info"},
            {Level.Warn, "Warn"},                                                  
            {Level.Error, "Error"},         
            {Level.Off, "Off"},              
        };

        // 0 = name
        // 1 = level
        // 2 = message

        public static Level defaultLevel = Level.Warn;
        public static bool defaultThrowOnError = false;

        public static UniLogger GetLogger(string name)
        {
            // level and format only get applied if the logger is new
            return  UniLoggerCollection.GetLogger(name);
        }

        public static void SetupLevels(Dictionary<string,string> levels)
        {
            foreach (string lName in levels.Keys)
            {
                GetLogger(lName).LogLevel = LevelNames.FirstOrDefault(x => x.Value == levels[lName]).Key;
            }
        }

#if UNITY_2019_1_OR_NEWER

        public const string defaultFormat = "{1}: {2}";

        //
        // Unity Implementation
        //

        protected UnityEngine.Logger unityLogger;

        public UniLogger(string name)
        {
            Name = name;
            LogLevel = defaultLevel;
            LogFormat = defaultFormat;
            ThrowOnError = defaultThrowOnError;
            unityLogger  = new UnityEngine.Logger(UnityEngine.Debug.unityLogger.logHandler);            
        }

        protected void _Write(string name, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string outMsg = string.Format(LogFormat, name, LevelNames[lvl], msg);                
                //string outMsg = string.Format(LogFormat, LevelNames[lvl], msg);
                switch (lvl)
                {
                case Level.Debug:
                case Level.Info:
                    unityLogger.Log($"{name}: {outMsg}");
                    break;
                case Level.Warn:
                    unityLogger.LogWarning(name, outMsg);
                    break;
                case Level.Error:
                    if (ThrowOnError)
                        throw new Exception($"{name}: {outMsg}");
                    else
                        unityLogger.LogError(name, outMsg);
                    break;   
                }             
            }
        }

#else
        //
        // Non-unity
        //

        public const string defaultFormat = "[{0}] {1}: {2}";

        public UniLogger(string name)
        {
            Name = name;
            LogLevel = defaultLevel;
            LogFormat = defaultFormat;
            ThrowOnError = defaultThrowOnError;
        }

        protected void _Write(string name, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string outMsg = string.Format(LogFormat, name, LevelNames[lvl], msg);

                if (lvl >= Level.Error && ThrowOnError)
                    throw new Exception(outMsg);
                else
                    Console.WriteLine(outMsg);
            }
        }

#endif
        // Instance
        public string Name {get; private set;}
        public Level LogLevel {get; set;}            
        public string LogFormat {get; set;}

        public bool ThrowOnError {get; set;}

        public void Info(string msg) => _Write(Name, Level.Info, msg);
        public void Debug(string msg) => _Write(Name, Level.Debug, msg);        
        public void Warn(string msg) => _Write(Name, Level.Warn, msg);
        public void Error(string msg) => _Write(Name, Level.Error, msg);                


    }

    public class UniLoggerCollection
    {
        protected Dictionary<string, UniLogger> loggers;

        //
        // Yeah, it's a singleton
        //
        private static UniLoggerCollection instance = null;
        public static UniLoggerCollection GetInstance()
        {
            if (instance == null)
            {
                instance = new UniLoggerCollection();
            }
            return instance;
        }   

        protected UniLoggerCollection()
        {
            loggers = new Dictionary<string, UniLogger>();
        }
        public static UniLogger GetLogger(string name)
        {
            UniLoggerCollection inst = GetInstance();
            return inst.loggers.ContainsKey(name) ? inst.loggers[name] : inst.AddLogger(name);
        }

        protected  UniLogger AddLogger(string name)
        {
            return loggers[name] = new UniLogger(name);
        }

    }
}

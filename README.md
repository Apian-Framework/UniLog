
# UniLog

**Unity 3D compatible C# logging (works without Unity, too)**

Unilog is a tag-based C# logging facility for code that might be built and run under Unity 3D. When run in the Unity editor Unilog makes use of native Unity logger.

---

## Status

![Build/Test](https://github.com/Apian-Framework/UniLog/actions/workflows/build-test.yaml/badge.svg)
![Line Coverage](https://github.com/Apian-Framework/Apian-CI-Badges/blob/UniLog/UniLog_linecoverage.svg)
![Branch Coverage](https://github.com/Apian-Framework/Apian-CI-Badges/blob/UniLog/UniLog_branchcoverage.svg)

---

## Usage

### Import:

```
using UniLog;
```

### Static Setup:

These static `UniLogger` properties define defaults for newly created loggers.

|  Property | Default | Description|
| ------------- |--------------------------|-----------|
| `Level DefaultLevel` | `Level.Warn` | Only display messages of at least this severity |
| `bool DefaultThrowOnError` | `false` | For calls to `logger.Error()` throw an exception.|
| `tring DefaultTimeFormat` | `[HH:mm:ss.fff]` | Use this time format in every message. |

### Basic logging

Fetch the instance of a named logger, or create it if it does not exist:
```
UniLogger loggerInst = UniLogger.GetLogger("ArbitraryLoggerInstanceName");
```

Available `UniLogger.Level` logLevels are, in order of precedence:
```
Debug = 10,
Verbose = 20,
Info = 30,
Warn = 40,
Error = 50,
Off = 1000,
```

Set the log level for this logger:
```
loggerInst.LogLevel = UniLogger.Level.Verbose;
```

Display log messages:
```
loggerInst.Debug("This is a debug message");
loggerInst.Verbose("This is verbose message");
loggerInst.Info("This is an informational message");
loggerInst.Warn("This is a warning message");
loggerInst.Error("This is an error message");
```

### Level Names

To convert levels to test names and vice versa, use:

```
Level verboseLevelVal = UniLogger.LevelFromName("Verbose");

string verboseLevelName = Unilogger.LevelNames[UniLogger.Level.Verbose];
```

### Current Loggers

To get a list of all currently instantiated loggers:

```
static IList<UniLogger> allOfTheLoggers = UniLogger.AllLoggers;
```

To initialize the system so there are no loggers.

```
UniLogger.Initialize();
```

### Loading / Saving logger instances

To get a dictionary of the logger levels (as strings) for all current loggers keyed by logger name:
```
Dictionary<string, string> theLevelsDict = UniLogger.CurrentLoggerLevels();
```

To create a saved set of loggers matching one returned by `CurrentLoggerLevels()`:
```
    UniLogger.SetupLevels( theLevelsDict );
```


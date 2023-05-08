
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

### Static Setup API:

Newly created loggers will have these properties:

|  Property | Default | Description|
| ------------- |--------------------------|-----------|
| `public static Level DefaultLevel` | `Level.Warn` | Only display messages of at least this severity |
| `public static bool DefaultThrowOnError` | `false` | For calls to `logger.Error()` throw an exception.|
| `public static string DefaultTimeFormat` | `[HH:mm:ss.fff]` | Use this time format in every message. |

### Basic logging

`UniLogger loggerInst = GetLogger("ArbitraryLoggerInstanceName");`






public static IList<UniLogger> AllLoggers
public static Dictionary<string, string> CurrentLoggerLevels()

public static Level LevelFromName(string name)
public static void SetupLevels(Dictionary<string,string> levels)
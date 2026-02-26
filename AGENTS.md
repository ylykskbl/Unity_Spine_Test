# AGENTS.md

## Project Overview

Unity game project demonstrating Spine animation reuse (sharing one skeleton/animation dataset across multiple characters via Skins). Built with **Unity 2022.3.61f1** and **URP** (Universal Render Pipeline).

## Cursor Cloud specific instructions

### Unity Editor

- Installed at `/home/ubuntu/Unity/Hub/Editor/2022.3.61f1/Editor/Unity`
- Unity Hub CLI: `xvfb-run unityhub --headless <command>`
- The editor requires a valid license. See "License Activation" below.

### License Activation

Unity **requires** an activated license before any batch-mode operations (compile, test, build).

**CLI activation (recommended for CI/headless):**
```bash
UNITY_LC="/home/ubuntu/Unity/Hub/Editor/2022.3.61f1/Editor/Data/Resources/Licensing/Client/Unity.Licensing.Client"
$UNITY_LC --activate-all --include-personal --username "$UNITY_EMAIL" --password "$UNITY_PASSWORD"
```
Secrets needed: `UNITY_EMAIL`, `UNITY_PASSWORD` (Unity ID account credentials).

**Note:** Unity no longer supports manual activation of Personal licenses via the web portal. The CLI approach above is the recommended way.

### Batch-Mode Commands

All Unity batch-mode commands use this pattern:
```bash
UNITY_PATH="/home/ubuntu/Unity/Hub/Editor/2022.3.61f1/Editor/Unity"
$UNITY_PATH -batchmode -nographics -projectPath /workspace -logFile <logfile> <flags>
```

| Task | Additional flags |
|---|---|
| Open & compile | `-quit` |
| Run EditMode tests | `-runTests -testPlatform EditMode -testResults /tmp/test-results.xml` |
| Run PlayMode tests | `-runTests -testPlatform PlayMode -testResults /tmp/test-results.xml` |
| Build (Linux standalone) | `-buildLinux64Player /tmp/build/game -quit` |

### C# Linting

CSharpier (dotnet tool) is installed for C# formatting checks:
```bash
export PATH="$PATH:/home/ubuntu/.dotnet/tools"
csharpier check Assets/Scripts/ Assets/Editor/
```
Note: formatting style differences are expected; CSharpier validates syntax correctness.

### Spine-Unity Dependency Gotcha

The `Packages/manifest.json` references Spine packages via Git URL pointing to the large `spine-runtimes` repository. The README explicitly warns this causes Unity to **freeze/hang** during package resolution. If Unity hangs on first open, the recommended workaround is:
1. Remove the Spine Git URL entries from `Packages/manifest.json`
2. Import Spine-Unity 4.2 via `.unitypackage` (download from https://esotericsoftware.com/spine-unity-download)
3. Re-open the project

### Project Has No Automated Tests

The `com.unity.test-framework` package is referenced in `manifest.json`, but no test assemblies (`.asmdef`) or test scripts exist. Running `-runTests` will succeed with zero tests executed.

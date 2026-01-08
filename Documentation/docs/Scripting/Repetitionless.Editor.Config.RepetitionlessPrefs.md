# Editor.Config.RepetitionlessPrefs

## Description

`Unity Editor Only`

Used to update repetitionless prefs

Prefs are project relative and Stored in &quot;Library/com.williamschack.repetitionless/prefs.json&quot;

## Variables

| Variable | Description |
|----------|-------------|
| Data | The prefs stored for this project |

---

## UpdatePrefs(Action<Prefs>)

### Declaration

``` csharp
public static void UpdatePrefs(Action<RepetitionlessPrefs.Prefs> updater)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| updater | The action used to modify the prefs before writing them |

### Description

Writes the prefs after calling the updater action

---


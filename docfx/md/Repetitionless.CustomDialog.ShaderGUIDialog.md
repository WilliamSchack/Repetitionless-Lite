# CustomDialog.ShaderGUIDialog

## Description

`Unity Editor Only`

Wrapper to remove the "PropertiesGUI() is being called recursively" warning when calling EditorUtility.DisplayDialog in OnGUI within a ShaderGUI inherited class

### Variables

| Variable | Description |
|----------|-------------|

---

## DisplayDialog(string, string, string, string)

### Declaration

``` csharp
public static bool DisplayDialog(string title, string message, string ok, string cancel)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| title | Title of the window |
| message | Message displayed in the dialog |
| ok | Left button underneath the message |
| cancel | Right button underneath the message |

### Returns

If the ok button was pressed

### Description

Displays a modal dialog removing the ShaderGUI specific warning

---

## DisplayDialogComplex(string, string, string, string, string)

### Declaration

``` csharp
public static int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| title | Title of the window |
| message | Message displayed in the dialog |
| ok | Left button underneath the message |
| cancel | Right button underneath the message |
| alt | Middle button underneath the  message |

### Returns

0, 1, 2 for ok, cancel, alt responses respectively

### Description

Displays a modal dialog with three buttons removing the ShaderGUI specific warning

---


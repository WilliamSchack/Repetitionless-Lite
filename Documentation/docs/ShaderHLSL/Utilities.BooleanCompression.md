# Utilities/BooleanCompression

## Description

Used to compress and extract an array of boolean values in a single integer

---

## AddCompressedValue()

### Declaration

``` csharp
int AddCompressedValue(int CompressedValues, bool Value, int Index)
```

### Parameters

| Parameter        | Description                               |
| ---------------- | ----------------------------------------- |
| CompressedValues | Compressed int of bool values             |
| Value            | Value inserted into the compressedValue   |
| Index            | Index that the input value will overwrite |

### Returns

New compressed integer with the added value

### Description

Overwrites a value at a given index into the input compressedValues

---

## GetCompressedValue()

### Declaration

``` csharp
bool GetCompressedValue(int CompressedValues, int Index)
```

### Parameters

| Parameter        | Description                                |
| ---------------- | ------------------------------------------ |
| CompressedValues | Compressed int of bool values              |
| Index            | Index of which bool value will be returned |

### Returns

Value at the given index

### Description

Gets a compressed value at a given index from the input compressedValues

---

## AddCompressedValue_float()

### Declaration

``` csharp
void AddCompressedValue_float(int CompressedValues, bool Value, int Index, out int Out)
```

### Parameters

| Parameter        | Description                               |
| ---------------- | ----------------------------------------- |
| CompressedValues | Compressed int of bool values             |
| Value            | Value inserted into the compressedValue   |
| Index            | Index that the input value will overwrite |

### Returns

| Output | Description                                 |
| ------ | ------------------------------------------- |
| Out    | New compressed integer with the added value |

### Description

Overwrites a value at a given index into the input compressedValues

**Used in Add Compressed Value sub graph**

---

## GetCompressedValue_float()

### Declaration

``` csharp
void GetCompressedValue_float(int CompressedValues, int Index, out bool Out)
```

### Parameters

| Parameter        | Description                                |
| ---------------- | ------------------------------------------ |
| CompressedValues | Compressed int of bool values              |
| Index            | Index of which bool value will be returned |

### Returns

| Output | Description              |
| ------ | ------------------------ |
| Out    | Value at the given index |

### Description

Gets a compressed value at a given index from the input compressedValues

**Used in Get Compressed Value sub graph**

---


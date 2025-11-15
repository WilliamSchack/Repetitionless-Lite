# Compression.BooleanCompression

## Description

Used to compress and extract an array of boolean values in a single integer

## Variables

| Variable | Description |
|----------|-------------|

---

## CompressValues(params bool[])

### Declaration

``` csharp
public static int CompressValues(params bool[] values)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| values | Bools that will be compressed |

### Returns

Compressed int of bools

### Description

Compresses the input array of bools into an int

---

## GetValues(int, int)

### Declaration

``` csharp
public static bool[] GetValues(int compressedValues, int valueCount)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedValues | Compressed int of bool values |
| valueCount | The total amount of values stored in the compressedValues |

### Returns

Array of all the values stored in the input compressedValues

### Description

Retrieves all the boolean values stored in a compressed int

---

## AddValue(int, int, bool)

### Declaration

``` csharp
public static int AddValue(int compressedValues, int index, bool value)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedValues | Compressed int of bool values |
| index | Index that the input value will overwrite |
| value | Value inserted into the compressedValue |

### Returns

New compressed integer with the added value

### Description

Overwrites a value at a given index into the input compressedValues

---

## GetValue(int, int)

### Declaration

``` csharp
public static bool GetValue(int compressedValues, int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedValues | Compressed int of bool values |
| index | Index of which bool value will be returned |

### Returns

Value at the given index

### Description

Gets a compressed value at a given index from the input compressedValues

---


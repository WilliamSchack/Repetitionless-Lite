# Variables.SerializableDictionary`2

## Description

A simple serializable dictionary that uses two lists for keys and values

## Variables

| Variable | Description |
|----------|-------------|
| Count | The amount of values stored |

---

## Get(TKey)

### Declaration

``` csharp
public TValue Get(TKey key)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| key | The key used to index into values  |

### Returns

A value at the index of key

### Description

Gets a value at the index of key

---

## Set(TKey, TValue)

### Declaration

``` csharp
public void Set(TKey key, TValue value)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| key | The key to overwrite or create |
| value | The value to set at key |

### Description

Sets a value at a key and creates a new entry if not found

---

## Remove(TKey)

### Declaration

``` csharp
public void Remove(TKey key)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| key | The key used to index into values |

### Description

Removes a value at the index of key

---

## ContainsKey(TKey)

### Declaration

``` csharp
public bool ContainsKey(TKey key)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| key | The key that will be checked |

### Returns

If the key has been set

### Description

Gets if a value has been set at a key

---


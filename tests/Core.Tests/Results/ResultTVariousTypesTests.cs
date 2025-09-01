using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Comprehensive tests for Result&lt;T&gt; with various value types, reference types, 
/// nullable types, and complex generic types to ensure proper type support.
/// </summary>
public class ResultTVariousTypesTests
{
    #region Value Types Tests

    [Fact]
    public void Result_WithByteType_ShouldWorkCorrectly()
    {
        // Arrange
        const byte value = 255;

        // Act
        Result<byte> successResult = Result.Success(value);
        Result<byte> failureResult = Result.Failure<byte>("Byte error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out byte retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out byte defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(byte));
    }

    [Fact]
    public void Result_WithSByteType_ShouldWorkCorrectly()
    {
        // Arrange
        const sbyte value = -128;

        // Act
        Result<sbyte> successResult = Result.Success(value);
        Result<sbyte> failureResult = Result.Failure<sbyte>("SByte error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out sbyte retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out sbyte defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(sbyte));
    }

    [Fact]
    public void Result_WithShortType_ShouldWorkCorrectly()
    {
        // Arrange
        const short value = -32768;

        // Act
        Result<short> successResult = Result.Success(value);
        Result<short> failureResult = Result.Failure<short>("Short error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out short retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out short defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(short));
    }

    [Fact]
    public void Result_WithUShortType_ShouldWorkCorrectly()
    {
        // Arrange
        const ushort value = 65535;

        // Act
        Result<ushort> successResult = Result.Success(value);
        Result<ushort> failureResult = Result.Failure<ushort>("UShort error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out ushort retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out ushort defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(ushort));
    }

    [Fact]
    public void Result_WithUIntType_ShouldWorkCorrectly()
    {
        // Arrange
        const uint value = 4294967295U;

        // Act
        Result<uint> successResult = Result.Success(value);
        Result<uint> failureResult = Result.Failure<uint>("UInt error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out uint retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out uint defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(uint));
    }

    [Fact]
    public void Result_WithLongType_ShouldWorkCorrectly()
    {
        // Arrange
        const long value = -9223372036854775808L;

        // Act
        Result<long> successResult = Result.Success(value);
        Result<long> failureResult = Result.Failure<long>("Long error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out long retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out long defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(long));
    }

    [Fact]
    public void Result_WithULongType_ShouldWorkCorrectly()
    {
        // Arrange
        const ulong value = 18446744073709551615UL;

        // Act
        Result<ulong> successResult = Result.Success(value);
        Result<ulong> failureResult = Result.Failure<ulong>("ULong error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out ulong retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out ulong defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(ulong));
    }

    [Fact]
    public void Result_WithFloatType_ShouldWorkCorrectly()
    {
        // Arrange
        const float value = 3.14159f;

        // Act
        Result<float> successResult = Result.Success(value);
        Result<float> failureResult = Result.Failure<float>("Float error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out float retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out float defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(float));
    }

    [Fact]
    public void Result_WithDoubleType_ShouldWorkCorrectly()
    {
        // Arrange
        const double value = 3.141592653589793;

        // Act
        Result<double> successResult = Result.Success(value);
        Result<double> failureResult = Result.Failure<double>("Double error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out double retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out double defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(double));
    }

    [Fact]
    public void Result_WithDecimalType_ShouldWorkCorrectly()
    {
        // Arrange
        const decimal value = 999999999999999999999999999m;

        // Act
        Result<decimal> successResult = Result.Success(value);
        Result<decimal> failureResult = Result.Failure<decimal>("Decimal error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out decimal retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out decimal defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(decimal));
    }

    [Fact]
    public void Result_WithCharType_ShouldWorkCorrectly()
    {
        // Arrange
        const char value = 'X';

        // Act
        Result<char> successResult = Result.Success(value);
        Result<char> failureResult = Result.Failure<char>("Char error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out char retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out char defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(char));
    }

    [Fact]
    public void Result_WithBoolType_ShouldWorkCorrectly()
    {
        // Arrange
        const bool value = true;

        // Act
        Result<bool> successResult = Result.Success(value);
        Result<bool> failureResult = Result.Failure<bool>("Bool error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out bool retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out bool defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(bool));
    }

    [Fact]
    public void Result_WithDateTimeType_ShouldWorkCorrectly()
    {
        // Arrange
        DateTime value = new(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc);

        // Act
        Result<DateTime> successResult = Result.Success(value);
        Result<DateTime> failureResult = Result.Failure<DateTime>("DateTime error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out DateTime retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out DateTime defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Result_WithDateOnlyType_ShouldWorkCorrectly()
    {
        // Arrange
        DateOnly value = new(2023, 12, 25);

        // Act
        Result<DateOnly> successResult = Result.Success(value);
        Result<DateOnly> failureResult = Result.Failure<DateOnly>("DateOnly error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out DateOnly retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out DateOnly defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(DateOnly));
    }

    [Fact]
    public void Result_WithTimeOnlyType_ShouldWorkCorrectly()
    {
        // Arrange
        TimeOnly value = new(14, 30, 45);

        // Act
        Result<TimeOnly> successResult = Result.Success(value);
        Result<TimeOnly> failureResult = Result.Failure<TimeOnly>("TimeOnly error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TimeOnly retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TimeOnly defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(TimeOnly));
    }

    [Fact]
    public void Result_WithTimeSpanType_ShouldWorkCorrectly()
    {
        // Arrange
        TimeSpan value = TimeSpan.FromMinutes(125.5);

        // Act
        Result<TimeSpan> successResult = Result.Success(value);
        Result<TimeSpan> failureResult = Result.Failure<TimeSpan>("TimeSpan error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TimeSpan retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TimeSpan defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(TimeSpan));
    }

    [Fact]
    public void Result_WithGuidType_ShouldWorkCorrectly()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        Result<Guid> successResult = Result.Success(value);
        Result<Guid> failureResult = Result.Failure<Guid>("Guid error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out Guid retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out Guid defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(Guid));
    }

    [Fact]
    public void Result_WithBigIntegerType_ShouldWorkCorrectly()
    {
        // Arrange
        BigInteger value = BigInteger.Parse("123456789012345678901234567890");

        // Act
        Result<BigInteger> successResult = Result.Success(value);
        Result<BigInteger> failureResult = Result.Failure<BigInteger>("BigInteger error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out BigInteger retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out BigInteger defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(BigInteger));
    }

    [Fact]
    public void Result_WithEnumType_ShouldWorkCorrectly()
    {
        // Arrange
        const TestEnum value = TestEnum.Second;

        // Act
        Result<TestEnum> successResult = Result.Success(value);
        Result<TestEnum> failureResult = Result.Failure<TestEnum>("Enum error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TestEnum retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TestEnum defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(TestEnum));
    }

    #endregion Value Types Tests

    #region Nullable Value Types Tests

    [Fact]
    public void Result_WithNullableIntType_ShouldWorkWithValidValue()
    {
        // Arrange
        int? value = 42;

        // Act
        Result<int?> result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out int? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
    }

    [Fact]
    public void Result_WithNullableIntType_ShouldThrowForNullValue()
    {
        // Arrange
        int? nullValue = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Success(nullValue));
    }

    [Fact]
    public void Result_WithNullableDecimalType_ShouldWorkWithValidValue()
    {
        // Arrange
        decimal? value = 123.45m;

        // Act
        Result<decimal?> result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out decimal? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
    }

    [Fact]
    public void Result_WithNullableGuidType_ShouldWorkWithValidValue()
    {
        // Arrange
        Guid? value = Guid.NewGuid();

        // Act
        Result<Guid?> result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out Guid? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
    }

    [Fact]
    public void Result_WithNullableDateTimeType_ShouldWorkWithValidValue()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow;

        // Act
        Result<DateTime?> result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out DateTime? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
    }

    [Fact]
    public void Result_WithNullableEnumType_ShouldWorkWithValidValue()
    {
        // Arrange
        TestEnum? value = TestEnum.Third;

        // Act
        Result<TestEnum?> result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestEnum? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
    }

    #endregion Nullable Value Types Tests

    #region Reference Types Tests

    [Fact]
    public void Result_WithArrayType_ShouldWorkCorrectly()
    {
        // Arrange
        int[] value = [1, 2, 3, 4, 5];

        // Act
        Result<int[]> successResult = Result.Success(value);
        Result<int[]> failureResult = Result.Failure<int[]>("Array error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out int[]? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Length.ShouldBe(5);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out int[]? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithListType_ShouldWorkCorrectly()
    {
        // Arrange
        List<string> value = ["apple", "banana", "cherry"];

        // Act
        Result<List<string>> successResult = Result.Success(value);
        Result<List<string>> failureResult = Result.Failure<List<string>>("List error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out List<string>? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Count.ShouldBe(3);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out List<string>? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithDictionaryType_ShouldWorkCorrectly()
    {
        // Arrange
        Dictionary<string, int> value = new() { ["one"] = 1, ["two"] = 2, ["three"] = 3 };

        // Act
        Result<Dictionary<string, int>> successResult = Result.Success(value);
        Result<Dictionary<string, int>> failureResult = Result.Failure<Dictionary<string, int>>("Dictionary error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out Dictionary<string, int>? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Count.ShouldBe(3);
        retrievedValue["two"].ShouldBe(2);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out Dictionary<string, int>? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithComplexObjectType_ShouldWorkCorrectly()
    {
        // Arrange
        TestClass value = new()
        {
            Id = 123,
            Name = "Test Object",
            CreatedAt = DateTime.UtcNow,
            Tags = ["tag1", "tag2"],
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act
        Result<TestClass> successResult = Result.Success(value);
        Result<TestClass> failureResult = Result.Failure<TestClass>("Complex object error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TestClass? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Id.ShouldBe(123);
        retrievedValue.Name.ShouldBe("Test Object");
        retrievedValue.Tags.Count.ShouldBe(2);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TestClass? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithRecordType_ShouldWorkCorrectly()
    {
        // Arrange
        TestRecord value = new(42, "Test Record", DateOnly.FromDateTime(DateTime.Today));

        // Act
        Result<TestRecord> successResult = Result.Success(value);
        Result<TestRecord> failureResult = Result.Failure<TestRecord>("Record error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TestRecord? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Id.ShouldBe(42);
        retrievedValue.Name.ShouldBe("Test Record");

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TestRecord? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithStructType_ShouldWorkCorrectly()
    {
        // Arrange
        TestStruct value = new(100, "Struct Value");

        // Act
        Result<TestStruct> successResult = Result.Success(value);
        Result<TestStruct> failureResult = Result.Failure<TestStruct>("Struct error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out TestStruct retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Id.ShouldBe(100);
        retrievedValue.Value.ShouldBe("Struct Value");

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out TestStruct defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default(TestStruct));
    }

    #endregion Reference Types Tests

    #region Complex Generic Types Tests

    [Fact]
    public void Result_WithGenericTupleType_ShouldWorkCorrectly()
    {
        // Arrange
        (string Name, int Age, bool IsActive) value = ("John Doe", 30, true);

        // Act
        Result<(string Name, int Age, bool IsActive)> successResult = Result.Success(value);
        Result<(string Name, int Age, bool IsActive)> failureResult = Result.Failure<(string Name, int Age, bool IsActive)>("Tuple error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out (string Name, int Age, bool IsActive) retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Name.ShouldBe("John Doe");
        retrievedValue.Age.ShouldBe(30);
        retrievedValue.IsActive.ShouldBeTrue();

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out (string Name, int Age, bool IsActive) defaultValue).ShouldBeFalse();
        defaultValue.ShouldBe(default((string, int, bool)));
    }

    [Fact]
    public void Result_WithNestedGenericType_ShouldWorkCorrectly()
    {
        // Arrange
        List<Dictionary<string, int[]>> value = [
            new Dictionary<string, int[]> { ["numbers"] = [1, 2, 3] },
            new Dictionary<string, int[]> { ["values"] = [4, 5, 6] }
        ];

        // Act
        Result<List<Dictionary<string, int[]>>> successResult = Result.Success(value);
        Result<List<Dictionary<string, int[]>>> failureResult = Result.Failure<List<Dictionary<string, int[]>>>("Nested generic error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out List<Dictionary<string, int[]>>? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Count.ShouldBe(2);
        retrievedValue[0]["numbers"][1].ShouldBe(2);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out List<Dictionary<string, int[]>>? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    [Fact]
    public void Result_WithGenericCollectionTypes_ShouldWorkCorrectly()
    {
        // Arrange
        HashSet<string> hashSet = ["alpha", "beta", "gamma"];
        Queue<int> queue = new([10, 20, 30]);
        Stack<double> stack = new([1.1, 2.2, 3.3]);

        // Act
        Result<HashSet<string>> hashSetResult = Result.Success(hashSet);
        Result<Queue<int>> queueResult = Result.Success(queue);
        Result<Stack<double>> stackResult = Result.Success(stack);

        // Assert
        hashSetResult.IsSuccess.ShouldBeTrue();
        hashSetResult.TryGetValue(out HashSet<string>? retrievedHashSet).ShouldBeTrue();
        retrievedHashSet.ShouldBe(hashSet);
        retrievedHashSet.Count.ShouldBe(3);

        queueResult.IsSuccess.ShouldBeTrue();
        queueResult.TryGetValue(out Queue<int>? retrievedQueue).ShouldBeTrue();
        retrievedQueue.ShouldBe(queue);
        retrievedQueue.Count.ShouldBe(3);

        stackResult.IsSuccess.ShouldBeTrue();
        stackResult.TryGetValue(out Stack<double>? retrievedStack).ShouldBeTrue();
        retrievedStack.ShouldBe(stack);
        retrievedStack.Count.ShouldBe(3);
    }

    [Fact]
    public void Result_WithReadOnlyCollectionTypes_ShouldWorkCorrectly()
    {
        // Arrange
        ReadOnlyCollection<string> readOnlyCollection = new List<string> { "item1", "item2", "item3" }.AsReadOnly();
        IEnumerable<int> enumerable = Enumerable.Range(1, 5);

        // Act
        Result<ReadOnlyCollection<string>> readOnlyResult = Result.Success(readOnlyCollection);
        Result<IEnumerable<int>> enumerableResult = Result.Success(enumerable);

        // Assert
        readOnlyResult.IsSuccess.ShouldBeTrue();
        readOnlyResult.TryGetValue(out ReadOnlyCollection<string>? retrievedReadOnly).ShouldBeTrue();
        retrievedReadOnly.ShouldBe(readOnlyCollection);
        retrievedReadOnly.Count.ShouldBe(3);

        enumerableResult.IsSuccess.ShouldBeTrue();
        enumerableResult.TryGetValue(out IEnumerable<int>? retrievedEnumerable).ShouldBeTrue();
        retrievedEnumerable.ShouldBe(enumerable);
        retrievedEnumerable.Count().ShouldBe(5);
    }

    [Fact]
    public void Result_WithGenericInterfaceType_ShouldWorkCorrectly()
    {
        // Arrange
        IList<string> value = new List<string> { "interface", "test", "data" };

        // Act
        Result<IList<string>> successResult = Result.Success(value);
        Result<IList<string>> failureResult = Result.Failure<IList<string>>("Interface error");

        // Assert
        successResult.IsSuccess.ShouldBeTrue();
        successResult.TryGetValue(out IList<string>? retrievedValue).ShouldBeTrue();
        retrievedValue.ShouldBe(value);
        retrievedValue.Count.ShouldBe(3);

        failureResult.IsFailure.ShouldBeTrue();
        failureResult.TryGetValue(out IList<string>? defaultValue).ShouldBeFalse();
        defaultValue.ShouldBeNull();
    }

    #endregion Complex Generic Types Tests

    #region JSON Serialization Tests

    [Fact]
    public void Result_WithVariousTypes_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        Result<int> intResult = Result.Success(42);
        Result<string> stringResult = Result.Success("test");
        Result<DateTime> dateTimeResult = Result.Success(new DateTime(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc));
        Result<decimal> decimalResult = Result.Success(123.45m);
        Result<bool> boolResult = Result.Success(true);

        // Act & Assert - Int
        string intJson = JsonSerializer.Serialize(intResult);
        Result<int> deserializedIntResult = JsonSerializer.Deserialize<Result<int>>(intJson);
        deserializedIntResult.IsSuccess.ShouldBeTrue();
        deserializedIntResult.TryGetValue(out int intValue).ShouldBeTrue();
        intValue.ShouldBe(42);

        // Act & Assert - String
        string stringJson = JsonSerializer.Serialize(stringResult);
        Result<string> deserializedStringResult = JsonSerializer.Deserialize<Result<string>>(stringJson);
        deserializedStringResult.IsSuccess.ShouldBeTrue();
        deserializedStringResult.TryGetValue(out string? stringValue).ShouldBeTrue();
        stringValue.ShouldBe("test");

        // Act & Assert - DateTime
        string dateTimeJson = JsonSerializer.Serialize(dateTimeResult);
        Result<DateTime> deserializedDateTimeResult = JsonSerializer.Deserialize<Result<DateTime>>(dateTimeJson);
        deserializedDateTimeResult.IsSuccess.ShouldBeTrue();
        deserializedDateTimeResult.TryGetValue(out DateTime dateTimeValue).ShouldBeTrue();
        dateTimeValue.ShouldBe(new DateTime(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc));

        // Act & Assert - Decimal
        string decimalJson = JsonSerializer.Serialize(decimalResult);
        Result<decimal> deserializedDecimalResult = JsonSerializer.Deserialize<Result<decimal>>(decimalJson);
        deserializedDecimalResult.IsSuccess.ShouldBeTrue();
        deserializedDecimalResult.TryGetValue(out decimal decimalValue).ShouldBeTrue();
        decimalValue.ShouldBe(123.45m);

        // Act & Assert - Bool
        string boolJson = JsonSerializer.Serialize(boolResult);
        Result<bool> deserializedBoolResult = JsonSerializer.Deserialize<Result<bool>>(boolJson);
        deserializedBoolResult.IsSuccess.ShouldBeTrue();
        deserializedBoolResult.TryGetValue(out bool boolValue).ShouldBeTrue();
        boolValue.ShouldBe(true);
    }

    [Fact]
    public void Result_WithComplexObjectSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        TestClass complexObject = new()
        {
            Id = 999,
            Name = "Serialization Test",
            CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Tags = ["serialize", "test"],
            Metadata = new Dictionary<string, string> { ["format"] = "json" }
        };
        Result<TestClass> result = Result.Success(complexObject);

        // Act
        string json = JsonSerializer.Serialize(result);
        Result<TestClass> deserializedResult = JsonSerializer.Deserialize<Result<TestClass>>(json);

        // Assert
        deserializedResult.IsSuccess.ShouldBeTrue();
        deserializedResult.TryGetValue(out TestClass? retrievedObject).ShouldBeTrue();
        retrievedObject.ShouldNotBeNull();
        retrievedObject.Id.ShouldBe(999);
        retrievedObject.Name.ShouldBe("Serialization Test");
        retrievedObject.Tags.Count.ShouldBe(2);
        retrievedObject.Tags[0].ShouldBe("serialize");
        retrievedObject.Metadata["format"].ShouldBe("json");
    }

    [Fact]
    public void Result_WithFailureSerialization_ShouldPreserveErrorInformation()
    {
        // Arrange
        Result<TestClass> result = Result.Failure<TestClass>("Serialization error test");

        // Act
        string json = JsonSerializer.Serialize(result);
        Result<TestClass> deserializedResult = JsonSerializer.Deserialize<Result<TestClass>>(json);

        // Assert
        deserializedResult.IsFailure.ShouldBeTrue();
        deserializedResult.Error.ShouldBe("Serialization error test");
        deserializedResult.FailureType.ShouldBe(ResultFailureType.Error);
        deserializedResult.TryGetValue(out TestClass? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    #endregion JSON Serialization Tests

    #region Test Types

    public enum TestEnum
    {
        First,
        Second,
        Third
    }

    public class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public record TestRecord(int Id, string Name, DateOnly Date);

    public readonly struct TestStruct
    {
        public TestStruct(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }
        public string Value { get; }
    }

    #endregion Test Types
}
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Tests for IResult and IResult{T} interfaces to ensure proper contract definition
/// and behavior expectations for Result pattern implementation.
/// </summary>
public class IResultTests
{
    #region IResult Interface Tests

    [Fact]
    public void IResult_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResult);
        
        // Assert
        interfaceType.GetProperty(nameof(IResult.Failures)).ShouldNotBeNull();
        interfaceType.GetProperty(nameof(IResult.FailureType)).ShouldNotBeNull();
        interfaceType.GetProperty(nameof(IResult.IsFailure)).ShouldNotBeNull();
        interfaceType.GetProperty(nameof(IResult.IsSuccess)).ShouldNotBeNull();
        interfaceType.GetProperty(nameof(IResult.ResultType)).ShouldNotBeNull();
    }

    [Fact]
    public void IResult_Failures_ShouldReturnIDictionaryOfStringArrays()
    {
        // Arrange & Act
        Type propertyType = typeof(IResult).GetProperty(nameof(IResult.Failures))!.PropertyType;
        
        // Assert
        propertyType.ShouldBe(typeof(IDictionary<string, string[]>));
    }

    [Fact]
    public void IResult_FailureType_ShouldReturnResultFailureType()
    {
        // Arrange & Act
        Type propertyType = typeof(IResult).GetProperty(nameof(IResult.FailureType))!.PropertyType;
        
        // Assert
        propertyType.ShouldBe(typeof(ResultFailureType));
    }

    [Fact]
    public void IResult_IsFailure_ShouldReturnBool()
    {
        // Arrange & Act
        Type propertyType = typeof(IResult).GetProperty(nameof(IResult.IsFailure))!.PropertyType;
        
        // Assert
        propertyType.ShouldBe(typeof(bool));
    }

    [Fact]
    public void IResult_IsSuccess_ShouldReturnBool()
    {
        // Arrange & Act
        Type propertyType = typeof(IResult).GetProperty(nameof(IResult.IsSuccess))!.PropertyType;
        
        // Assert
        propertyType.ShouldBe(typeof(bool));
    }

    [Fact]
    public void IResult_ResultType_ShouldReturnResultType()
    {
        // Arrange & Act
        Type propertyType = typeof(IResult).GetProperty(nameof(IResult.ResultType))!.PropertyType;
        
        // Assert
        propertyType.ShouldBe(typeof(ResultType));
    }

    #endregion

    #region IResult<T> Interface Tests

    [Fact]
    public void IResultT_ShouldInheritFromIResult()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResult<>);
        
        // Assert
        interfaceType.GetInterfaces().ShouldContain(typeof(IResult));
    }

    [Fact]
    public void IResultT_ShouldInheritFromIResultError()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResult<>);
        Type[] interfaces = interfaceType.GetInterfaces();
        
        // Assert
        interfaces.ShouldContain(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultError<>));
    }

    [Fact]
    public void IResultT_ShouldBeCovariant()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResult<>);
        Type genericParameter = interfaceType.GetGenericArguments()[0];
        
        // Assert - The 'out' keyword makes it covariant
        (genericParameter.GenericParameterAttributes & GenericParameterAttributes.Covariant).ShouldBe(GenericParameterAttributes.Covariant);
    }

    [Fact]
    public void IResultT_ShouldHaveMatchMethods()
    {
        // Arrange
        Type interfaceType = typeof(IResult<>);
        
        // Act
        MethodInfo[] matchMethods = interfaceType.GetMethods()
            .Where(m => m.Name == "Match")
            .ToArray();
        
        // Assert
        matchMethods.Length.ShouldBe(2);
        
        // Simple Match method
        MethodInfo simpleMatch = matchMethods.First(m => m.GetParameters().Length == 2);
        simpleMatch.ShouldNotBeNull();
        simpleMatch.IsGenericMethodDefinition.ShouldBeTrue();
        
        // Complex Match method with all failure types
        MethodInfo complexMatch = matchMethods.First(m => m.GetParameters().Length == 5);
        complexMatch.ShouldNotBeNull();
        complexMatch.IsGenericMethodDefinition.ShouldBeTrue();
    }

    [Fact]
    public void IResultT_ShouldHaveSwitchMethods()
    {
        // Arrange
        Type interfaceType = typeof(IResult<>);
        
        // Act
        MethodInfo[] switchMethods = interfaceType.GetMethods()
            .Where(m => m.Name == "Switch")
            .ToArray();
        
        // Assert
        switchMethods.Length.ShouldBe(2);
        
        // Simple Switch method
        MethodInfo simpleSwitch = switchMethods.First(m => m.GetParameters().Length == 3);
        simpleSwitch.ShouldNotBeNull();
        
        // Complex Switch method with all failure types
        MethodInfo complexSwitch = switchMethods.First(m => m.GetParameters().Length == 5);
        complexSwitch.ShouldNotBeNull();
    }

    #endregion

    #region IResultError<T> Interface Tests

    [Fact]
    public void IResultError_ShouldHaveErrorProperty()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResultError<>);
        PropertyInfo? errorProperty = interfaceType.GetProperty("Error");
        
        // Assert
        errorProperty.ShouldNotBeNull();
    }

    [Fact]
    public void IResultError_ShouldBeCovariant()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResultError<>);
        Type genericParameter = interfaceType.GetGenericArguments()[0];
        
        // Assert - The 'out' keyword makes it covariant
        (genericParameter.GenericParameterAttributes & GenericParameterAttributes.Covariant).ShouldBe(GenericParameterAttributes.Covariant);
    }

    [Fact]
    public void IResultError_ErrorProperty_ShouldHaveGenericType()
    {
        // Arrange & Act
        Type interfaceType = typeof(IResultError<string>);
        PropertyInfo? errorProperty = interfaceType.GetProperty("Error");
        
        // Assert
        errorProperty.ShouldNotBeNull();
        errorProperty.PropertyType.ShouldBe(typeof(string));
    }

    #endregion

    #region ResultType Enum Tests

    [Fact]
    public void ResultType_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        Enum.GetNames<ResultType>().ShouldBe(new[] { "Success", "Information", "Warning", "Error" });
        
        ((int)ResultType.Success).ShouldBe(0);
        ((int)ResultType.Information).ShouldBe(1);
        ((int)ResultType.Warning).ShouldBe(2);
        ((int)ResultType.Error).ShouldBe(3);
    }

    #endregion

    #region ResultFailureType Enum Tests

    [Fact]
    public void ResultFailureType_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        Enum.GetNames<ResultFailureType>().ShouldBe(new[] { "None", "Error", "Security", "Validation", "OperationCanceled" });
        
        ((int)ResultFailureType.None).ShouldBe(0);
        ((int)ResultFailureType.Error).ShouldBe(1);
        ((int)ResultFailureType.Security).ShouldBe(2);
        ((int)ResultFailureType.Validation).ShouldBe(3);
        ((int)ResultFailureType.OperationCanceled).ShouldBe(4);
    }

    #endregion
}
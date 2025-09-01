using FlowRight.Validation.Builders;
using FlowRight.Validation.Context;
using FlowRight.Validation.Tests.TestModels;
using Shouldly;

namespace FlowRight.Validation.Tests.Context;

/// <summary>
/// Basic tests for ValidationContext functionality that verify the core features work correctly.
/// These tests validate the ValidationContext implementation without requiring advanced validation rule integration.
/// </summary>
public class BasicValidationContextTests
{
    #region ValidationContext Creation Tests

    [Fact]
    public void ValidationContext_Create_ShouldReturnNonNullContext()
    {
        // Act
        IValidationContext context = ValidationContext.Create();

        // Assert
        context.ShouldNotBeNull();
        context.RootObject.ShouldBeNull();
        context.ServiceProvider.ShouldBeNull();
        context.CustomData.ShouldNotBeNull();
        context.CustomData.ShouldBeEmpty();
    }

    [Fact]
    public void ValidationContext_CreateWithRootObject_ShouldStoreRootObject()
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        IValidationContext context = ValidationContext.Create(user);

        // Assert
        context.RootObject.ShouldBe(user);
        context.GetRootObject<User>().ShouldBe(user);
    }

    [Fact]
    public void ValidationContext_CreateWithServiceProvider_ShouldStoreServiceProvider()
    {
        // Arrange
        TestServiceProvider serviceProvider = new();

        // Act
        IValidationContext context = ValidationContext.Create(serviceProvider);

        // Assert
        context.ServiceProvider.ShouldBe(serviceProvider);
    }

    [Fact]
    public void ValidationContext_CreateWithBothParameters_ShouldStoreBoth()
    {
        // Arrange
        User user = new UserBuilder().Build();
        TestServiceProvider serviceProvider = new();

        // Act
        IValidationContext context = ValidationContext.Create(user, serviceProvider);

        // Assert
        context.RootObject.ShouldBe(user);
        context.ServiceProvider.ShouldBe(serviceProvider);
    }

    #endregion

    #region Custom Data Tests

    [Fact]
    public void ValidationContext_SetAndGetCustomData_ShouldWorkCorrectly()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();
        string testKey = "testKey";
        string testValue = "testValue";

        // Act
        context.SetCustomData(testKey, testValue);

        // Assert
        context.GetCustomData<string>(testKey).ShouldBe(testValue);
        context.HasCustomData(testKey).ShouldBeTrue();
    }

    [Fact]
    public void ValidationContext_GetNonExistentCustomData_ShouldReturnDefault()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();

        // Act & Assert
        context.GetCustomData<string>("nonexistent").ShouldBeNull();
        context.GetCustomData<int>("nonexistent").ShouldBe(0);
        context.HasCustomData("nonexistent").ShouldBeFalse();
    }

    [Fact]
    public void ValidationContext_RemoveCustomData_ShouldWork()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();
        context.SetCustomData("testKey", "testValue");

        // Act
        bool removed = context.RemoveCustomData("testKey");

        // Assert
        removed.ShouldBeTrue();
        context.HasCustomData("testKey").ShouldBeFalse();
        context.GetCustomData<string>("testKey").ShouldBeNull();
    }

    [Fact]
    public void ValidationContext_RemoveNonExistentData_ShouldReturnFalse()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();

        // Act
        bool removed = context.RemoveCustomData("nonexistent");

        // Assert
        removed.ShouldBeFalse();
    }

    #endregion

    #region ValidationBuilder Integration Tests

    [Fact]
    public void ValidationBuilder_WithContext_ShouldWork()
    {
        // Arrange
        User user = new UserBuilder().WithName("John Doe").Build();
        IValidationContext context = ValidationContext.Create(user);

        // Act
        ValidationBuilder<User> builder = new(context);

        // Assert
        builder.ShouldNotBeNull();
        builder.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void ValidationBuilder_WithoutContext_ShouldWork()
    {
        // Act
        ValidationBuilder<User> builder = new();

        // Assert
        builder.ShouldNotBeNull();
        builder.HasErrors.ShouldBeFalse();
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public void ValidationContext_GetService_ShouldWorkCorrectly()
    {
        // Arrange
        TestServiceProvider serviceProvider = new();
        TestService testService = new();
        serviceProvider.RegisterService<ITestService>(testService);
        
        IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);

        // Act
        ITestService? retrievedService = context.GetService<ITestService>();

        // Assert
        retrievedService.ShouldNotBeNull();
        retrievedService.ShouldBe(testService);
    }

    [Fact]
    public void ValidationContext_GetNonExistentService_ShouldReturnNull()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();

        // Act
        ITestService? service = context.GetService<ITestService>();

        // Assert
        service.ShouldBeNull();
    }

    #endregion

    #region Child Context Tests

    [Fact]
    public void ValidationContext_CreateChildContext_ShouldInheritData()
    {
        // Arrange
        User user = new UserBuilder().Build();
        TestServiceProvider serviceProvider = new();
        IValidationContext parentContext = ValidationContext.Create(user, serviceProvider);
        parentContext.SetCustomData("parentData", "parentValue");

        Profile childProfile = new("Test bio", DateTime.UtcNow);

        // Act
        IValidationContext childContext = parentContext.CreateChildContext(childProfile, "Profile");

        // Assert
        childContext.Parent.ShouldBe(parentContext);
        childContext.RootObject.ShouldBe(user); // Should inherit root object
        childContext.ServiceProvider.ShouldBe(serviceProvider); // Should inherit service provider
        childContext.HasCustomData("parentData").ShouldBeTrue(); // Should inherit custom data
        childContext.GetCustomData<string>("parentData").ShouldBe("parentValue");
    }

    [Fact]
    public void ValidationContext_GetCurrentPropertyPath_ShouldWork()
    {
        // Arrange
        User user = new UserBuilder().Build();
        IValidationContext parentContext = ValidationContext.Create(user);
        Profile childProfile = new("Test bio", DateTime.UtcNow);

        // Act
        IValidationContext childContext = parentContext.CreateChildContext(childProfile, "Profile");

        // Assert
        parentContext.GetCurrentPropertyPath().ShouldBe("");
        childContext.GetCurrentPropertyPath().ShouldBe("Profile");
    }

    #endregion

    #region Rule Execution Tracking Tests

    [Fact]
    public void ValidationContext_RecordRuleExecution_ShouldWork()
    {
        // Arrange
        IValidationContext context = ValidationContext.Create();

        // Act
        context.RecordRuleExecution("NotEmpty", "Name", true);
        context.RecordRuleExecution("MaxLength", "Name", false);

        // Assert
        IReadOnlyList<string> executedRules = context.GetExecutedRules();
        executedRules.Count.ShouldBe(2);
        executedRules[0].ShouldBe("Name.NotEmpty:Success");
        executedRules[1].ShouldBe("Name.MaxLength:Failed");
    }

    #endregion
}

#region Test Support Classes

/// <summary>
/// Test implementation of IServiceProvider for ValidationContext testing.
/// </summary>
public class TestServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public void RegisterService<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public object? GetService(Type serviceType)
    {
        return _services.GetValueOrDefault(serviceType);
    }
}

/// <summary>
/// Test service interface for demonstrating service integration.
/// </summary>
public interface ITestService
{
    string GetValue();
}

/// <summary>
/// Test service implementation.
/// </summary>
public class TestService : ITestService
{
    public string GetValue() => "TestValue";
}

#endregion
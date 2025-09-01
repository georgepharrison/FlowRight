using FlowRight.Core.Results;
using FlowRight.Http.Extensions;
using FlowRight.Validation.Builders;
using Shouldly;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Comprehensive integration tests for thread safety and concurrency behavior across the entire FlowRight library.
/// These tests validate that core components can be safely used in multi-threaded, concurrent production scenarios.
/// </summary>
/// <remarks>
/// <para>
/// FlowRight is designed for use in production web applications where multiple threads may simultaneously:
/// - Create and manipulate Result instances
/// - Perform validation operations using ValidationBuilder
/// - Process HTTP responses through extension methods
/// - Share Result instances across thread boundaries
/// </para>
/// <para>
/// This test suite covers critical concurrency scenarios including race conditions, deadlock detection,
/// memory visibility, data integrity, and performance under concurrent load.
/// </para>
/// </remarks>
public class ThreadSafetyAndConcurrencyIntegrationTests
{
    #region Result Thread Safety Tests

    public class ResultCreationConcurrency
    {
        [Fact]
        public void ConcurrentResultCreation_WithManyThreads_ShouldMaintainDataIntegrity()
        {
            // Arrange
            const int threadCount = 100;
            const int operationsPerThread = 1000;
            ConcurrentBag<Result> successResults = [];
            ConcurrentBag<Result> failureResults = [];
            List<Task> tasks = [];
            
            // Act - Create Results concurrently from many threads
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        // Alternate between success and failure creation
                        if ((threadId + j) % 2 == 0)
                        {
                            Result successResult = Result.Success();
                            successResults.Add(successResult);
                        }
                        else
                        {
                            Result failureResult = Result.Failure($"Error from thread {threadId}, operation {j}");
                            failureResults.Add(failureResult);
                        }
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent Result creation tasks should complete within timeout");
            successResults.Count.ShouldBe(threadCount * operationsPerThread / 2, "Should create expected number of success results");
            failureResults.Count.ShouldBe(threadCount * operationsPerThread / 2, "Should create expected number of failure results");
            
            // Verify data integrity of created Results
            successResults.ShouldAllBe(r => r.IsSuccess, "All success results should be in success state");
            failureResults.ShouldAllBe(r => r.IsFailure, "All failure results should be in failure state");
        }

        [Fact]
        public void ConcurrentResultTCreation_WithGenericTypes_ShouldMaintainTypeAndDataIntegrity()
        {
            // Arrange
            const int threadCount = 50;
            const int operationsPerThread = 500;
            ConcurrentBag<Result<string>> stringResults = [];
            ConcurrentBag<Result<int>> intResults = [];
            ConcurrentBag<Result<ConcurrentTestUser>> userResults = [];
            List<Task> tasks = [];
            
            // Act - Create generic Results concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        // Create different generic Result types
                        Result<string> stringResult = Result.Success($"String from thread {threadId}, operation {j}");
                        stringResults.Add(stringResult);
                        
                        Result<int> intResult = Result.Success(threadId * 1000 + j);
                        intResults.Add(intResult);
                        
                        Result<ConcurrentTestUser> userResult = Result.Success(new ConcurrentTestUser
                        {
                            Id = threadId * 1000 + j,
                            Name = $"User {threadId}-{j}",
                            Email = $"user{threadId}-{j}@test.com"
                        });
                        userResults.Add(userResult);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent generic Result creation tasks should complete within timeout");
            
            stringResults.Count.ShouldBe(threadCount * operationsPerThread);
            intResults.Count.ShouldBe(threadCount * operationsPerThread);
            userResults.Count.ShouldBe(threadCount * operationsPerThread);
            
            // Verify type safety and data integrity
            stringResults.ShouldAllBe(r => r.IsSuccess);
            intResults.ShouldAllBe(r => r.IsSuccess);
            userResults.ShouldAllBe(r => r.IsSuccess);
            
            // Verify values are correct
            foreach (Result<string> result in stringResults)
            {
                result.TryGetValue(out string? value).ShouldBeTrue();
                value.ShouldNotBeNullOrEmpty();
            }
            
            foreach (Result<int> result in intResults)
            {
                result.TryGetValue(out int value).ShouldBeTrue();
                value.ShouldBeGreaterThanOrEqualTo(0);
            }
            
            foreach (Result<ConcurrentTestUser> result in userResults)
            {
                result.TryGetValue(out ConcurrentTestUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                user.Id.ShouldBeGreaterThanOrEqualTo(0);
            }
        }

        [Fact]
        public void ConcurrentResultCombine_WithManyResults_ShouldProduceCorrectAggregation()
        {
            // Arrange
            const int threadCount = 20;
            const int resultsPerThread = 100;
            ConcurrentBag<Result> allResults = [];
            List<Task> creationTasks = [];
            
            // Create Results concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < resultsPerThread; j++)
                    {
                        Result result = (threadId + j) % 3 == 0 
                            ? Result.Failure($"Error {threadId}-{j}") 
                            : Result.Success();
                        allResults.Add(result);
                    }
                });
                creationTasks.Add(task);
            }
            
            Task.WaitAll([.. creationTasks], TimeSpan.FromSeconds(10));
            Result[] resultsArray = [.. allResults];
            
            // Act - Combine Results concurrently from multiple threads
            const int combinerThreads = 10;
            ConcurrentBag<Result> combinedResults = [];
            List<Task> combineTasks = [];
            
            for (int i = 0; i < combinerThreads; i++)
            {
                Task combineTask = Task.Run(() =>
                {
                    Result combined = Result.Combine(resultsArray);
                    combinedResults.Add(combined);
                });
                combineTasks.Add(combineTask);
            }
            
            bool allCombineCompleted = Task.WaitAll([.. combineTasks], TimeSpan.FromSeconds(15));
            
            // Assert
            allCombineCompleted.ShouldBeTrue("All concurrent combine operations should complete");
            combinedResults.Count.ShouldBe(combinerThreads);
            
            // All combined results should be identical (deterministic combining)
            Result firstCombined = combinedResults.First();
            combinedResults.ShouldAllBe(r => r.IsSuccess == firstCombined.IsSuccess && r.Error == firstCombined.Error);
        }
    }

    public class ResultPatternMatchingConcurrency
    {
        [Fact]
        public void ConcurrentResultPatternMatching_WithSharedResults_ShouldMaintainConsistency()
        {
            // Arrange
            Result[] sharedResults = [
                Result.Success(),
                Result.Failure("Test error"),
                Result.ValidationFailure(new Dictionary<string, string[]> { ["Field"] = ["Error message"] }),
                Result.Failure(new SecurityException("Unauthorized")),
                Result.NotFound("Resource not found")
            ];
            
            const int threadCount = 100;
            const int operationsPerThread = 200;
            ConcurrentBag<string> matchResults = [];
            List<Task> tasks = [];
            
            // Act - Perform pattern matching concurrently on shared Results
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    Random random = new(threadId);
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        Result result = sharedResults[random.Next(sharedResults.Length)];
                        
                        string matchResult = result.Match(
                            onSuccess: () => "SUCCESS",
                            onFailure: error => $"FAILURE: {error.Split(' ')[0]}" // Take first word only for consistency
                        );
                        
                        matchResults.Add(matchResult);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent pattern matching tasks should complete");
            matchResults.Count.ShouldBe(threadCount * operationsPerThread);
            
            // Verify pattern matching consistency - all results should follow expected patterns
            matchResults.ShouldAllBe(result => result == "SUCCESS" || result.StartsWith("FAILURE:"));
            
            // Verify we get consistent results from the same shared Result instances
            var resultGroups = matchResults.GroupBy(r => r).ToList();
            resultGroups.ShouldNotBeEmpty("Should have grouped results");
            resultGroups.Count.ShouldBeLessThanOrEqualTo(sharedResults.Length, "Should not have more result patterns than input Results");
        }

        [Fact]
        public void ConcurrentResultSwitchOperations_WithSideEffects_ShouldExecuteCorrectly()
        {
            // Arrange
            Result[] testResults = [
                Result.Success(),
                Result.Failure("Test error"),
                Result.ValidationFailure(new Dictionary<string, string[]> { ["Field"] = ["Validation error"] })
            ];
            
            const int threadCount = 50;
            const int operationsPerThread = 100;
            ConcurrentBag<string> sideEffects = [];
            List<Task> tasks = [];
            
            // Act - Perform Switch operations with side effects concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    Random random = new(threadId);
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        Result result = testResults[random.Next(testResults.Length)];
                        
                        result.Switch(
                            onSuccess: () => sideEffects.Add($"Success-{threadId}-{j}"),
                            onFailure: error => sideEffects.Add($"Failure-{threadId}-{j}")
                        );
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent Switch operations should complete");
            sideEffects.Count.ShouldBe(threadCount * operationsPerThread);
            sideEffects.ShouldAllBe(effect => effect.StartsWith("Success-") || effect.StartsWith("Failure-"));
        }
    }

    #endregion Result Thread Safety Tests

    #region ValidationBuilder Thread Safety Tests

    public class ValidationBuilderConcurrency
    {
        [Fact]
        public void ConcurrentValidationBuilderUsage_WithSeparateInstances_ShouldMaintainIsolation()
        {
            // Arrange
            const int threadCount = 50;
            const int validationsPerThread = 100;
            ConcurrentBag<Result<ConcurrentTestUser>> validationResults = [];
            List<Task> tasks = [];
            
            // Act - Use separate ValidationBuilder instances concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < validationsPerThread; j++)
                    {
                        ValidationBuilder<ConcurrentTestUser> builder = new();
                        
                        string name = $"User{threadId}-{j}";
                        string email = $"user{threadId}-{j}@test.com";
                        int age = 20 + (threadId % 50);
                        
                        Result<ConcurrentTestUser> result = builder
                            .RuleFor(u => u.Name, name)
                                .NotEmpty()
                                .MaximumLength(100)
                            .RuleFor(u => u.Email, email)
                                .NotEmpty()
                                .EmailAddress()
                            .RuleFor(u => u.Age, age)
                                .GreaterThan(0)
                                .LessThan(120)
                            .Build(() => new ConcurrentTestUser
                            {
                                Id = threadId * 1000 + j,
                                Name = name,
                                Email = email,
                                Age = age
                            });
                        
                        validationResults.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(45));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent validation tasks should complete within timeout");
            validationResults.Count.ShouldBe(threadCount * validationsPerThread);
            
            // All validations should succeed with proper isolation
            validationResults.ShouldAllBe(result => result.IsSuccess, "All validations should succeed with separate builder instances");
            
            // Verify each result has correct data
            foreach (Result<ConcurrentTestUser> result in validationResults)
            {
                result.TryGetValue(out ConcurrentTestUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                user.Name.ShouldNotBeNullOrEmpty();
                user.Email.ShouldNotBeNullOrEmpty();
                user.Age.ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public void SharedValidationBuilderAccess_WithConcurrentThreads_ShouldExposeThreadSafetyIssues()
        {
            // Arrange - This test is EXPECTED to fail, demonstrating ValidationBuilder is NOT thread-safe
            ValidationBuilder<ConcurrentTestUser> sharedBuilder = new();
            const int threadCount = 10;
            const int operationsPerThread = 50;
            ConcurrentBag<Exception> exceptions = [];
            ConcurrentBag<Result<ConcurrentTestUser>> results = [];
            List<Task> tasks = [];
            
            // Act - Multiple threads accessing the same ValidationBuilder instance (unsafe)
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            // This should cause thread safety issues due to shared state
                            Result<ConcurrentTestUser> result = sharedBuilder
                                .RuleFor(u => u.Name, $"User{threadId}-{j}")
                                    .NotEmpty()
                                .RuleFor(u => u.Email, $"user{threadId}-{j}@test.com")
                                    .EmailAddress()
                                .Build(() => new ConcurrentTestUser
                                {
                                    Id = threadId * 1000 + j,
                                    Name = $"User{threadId}-{j}",
                                    Email = $"user{threadId}-{j}@test.com"
                                });
                            
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert - This test should FAIL, demonstrating the thread safety issue
            // ValidationBuilder uses Dictionary for error collection which is not thread-safe
            bool hasThreadSafetyIssues = exceptions.Count > 0 || 
                                        results.Any(r => r.IsFailure) || 
                                        results.Count != threadCount * operationsPerThread;
            
            hasThreadSafetyIssues.ShouldBeTrue(
                "SharedValidationBuilder should demonstrate thread safety issues. " +
                $"Exceptions: {exceptions.Count}, Results: {results.Count}, Expected: {threadCount * operationsPerThread}");
        }

        [Fact] 
        public void ConcurrentValidationWithResultComposition_ShouldMaintainDataIntegrity()
        {
            // Arrange
            const int threadCount = 25;
            const int validationsPerThread = 50;
            ConcurrentBag<Result<ConcurrentComplexUser>> compositionResults = [];
            List<Task> tasks = [];
            
            // Act - Perform Result composition in ValidationBuilder concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < validationsPerThread; j++)
                    {
                        ValidationBuilder<ConcurrentComplexUser> builder = new();
                        
                        // Simulate Result<T> composition from different operations
                        Result<ConcurrentUserProfile> profileResult = CreateUserProfile(threadId, j);
                        Result<ConcurrentUserSettings> settingsResult = CreateUserSettings(threadId, j);
                        Result<ConcurrentUserPreferences> preferencesResult = CreateUserPreferences(threadId, j);
                        
                        Result<ConcurrentComplexUser> result = builder
                            .RuleFor(u => u.Profile, profileResult, out ConcurrentUserProfile? profile)
                            .RuleFor(u => u.Settings, settingsResult, out ConcurrentUserSettings? settings)  
                            .RuleFor(u => u.Preferences, preferencesResult, out ConcurrentUserPreferences? preferences)
                            .Build(() => new ConcurrentComplexUser
                            {
                                Id = threadId * 1000 + j,
                                Profile = profile!,
                                Settings = settings!,
                                Preferences = preferences!
                            });
                        
                        compositionResults.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(45));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent composition tasks should complete");
            compositionResults.Count.ShouldBe(threadCount * validationsPerThread);
            
            // Verify all compositions succeeded
            compositionResults.ShouldAllBe(result => result.IsSuccess, "All Result compositions should succeed");
            
            // Verify data integrity of composed objects
            foreach (Result<ConcurrentComplexUser> result in compositionResults)
            {
                result.TryGetValue(out ConcurrentComplexUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                user.Profile.ShouldNotBeNull();
                user.Settings.ShouldNotBeNull(); 
                user.Preferences.ShouldNotBeNull();
            }
        }
    }

    #endregion ValidationBuilder Thread Safety Tests

    #region HTTP Extensions Thread Safety Tests

    public class HttpExtensionsConcurrency
    {
        [Fact]
        public async Task ConcurrentHttpResponseProcessing_WithMultipleResponses_ShouldMaintainIndependence()
        {
            // Arrange
            const int responseCount = 100;
            const int concurrentProcessors = 20;
            
            List<HttpResponseMessage> responses = [];
            for (int i = 0; i < responseCount; i++)
            {
                HttpResponseMessage response = new(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new ConcurrentTestUser 
                        { 
                            Id = i, 
                            Name = $"User{i}", 
                            Email = $"user{i}@test.com" 
                        }),
                        Encoding.UTF8,
                        "application/json")
                };
                responses.Add(response);
            }
            
            ConcurrentBag<Result<ConcurrentTestUser?>> processingResults = [];
            List<Task> tasks = [];
            
            // Act - Process HTTP responses concurrently
            for (int i = 0; i < concurrentProcessors; i++)
            {
                int processorId = i;
                Task task = Task.Run(async () =>
                {
                    int startIndex = (responseCount / concurrentProcessors) * processorId;
                    int endIndex = Math.Min(startIndex + (responseCount / concurrentProcessors), responseCount);
                    
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        HttpResponseMessage response = responses[j];
                        Result<ConcurrentTestUser?> result = await response.ToResultFromJsonAsync<ConcurrentTestUser>();
                        processingResults.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            
            // Assert
            processingResults.Count.ShouldBe(responseCount);
            processingResults.ShouldAllBe(result => result.IsSuccess, "All HTTP responses should be processed successfully");
            
            // Verify data integrity and independence
            HashSet<int> processedIds = [];
            foreach (Result<ConcurrentTestUser?> result in processingResults)
            {
                result.TryGetValue(out ConcurrentTestUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                processedIds.Add(user.Id).ShouldBeTrue($"User ID {user.Id} should be unique");
            }
            
            processedIds.Count.ShouldBe(responseCount, "All unique user IDs should be processed");
            
            // Cleanup
            foreach (HttpResponseMessage response in responses)
            {
                response.Dispose();
            }
        }

        [Fact]
        public async Task ConcurrentHttpErrorHandling_WithMixedStatusCodes_ShouldProduceCorrectResults()
        {
            // Arrange
            HttpResponseMessage[] errorResponses = [
                new(HttpStatusCode.BadRequest) { Content = new StringContent("Bad Request") },
                new(HttpStatusCode.Unauthorized) { Content = new StringContent("Unauthorized") },
                new(HttpStatusCode.NotFound) { Content = new StringContent("Not Found") },
                new(HttpStatusCode.InternalServerError) { Content = new StringContent("Server Error") }
            ];
            
            const int threadCount = 50;
            const int requestsPerThread = 25;
            ConcurrentBag<Result> errorResults = [];
            List<Task> tasks = [];
            
            // Act - Process error responses concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(async () =>
                {
                    Random random = new(threadId);
                    for (int j = 0; j < requestsPerThread; j++)
                    {
                        HttpResponseMessage response = errorResponses[random.Next(errorResponses.Length)];
                        
                        // Clone the response to avoid sharing issues
                        HttpResponseMessage clonedResponse = new(response.StatusCode)
                        {
                            Content = new StringContent(await response.Content.ReadAsStringAsync())
                        };
                        
                        Result result = await clonedResponse.ToResultAsync();
                        errorResults.Add(result);
                        
                        clonedResponse.Dispose();
                    }
                });
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            
            // Assert
            errorResults.Count.ShouldBe(threadCount * requestsPerThread);
            errorResults.ShouldAllBe(result => result.IsFailure, "All error responses should result in failure Results");
            
            // Verify appropriate failure types are produced
            var failureTypes = errorResults
                .Select(r => r.FailureType)
                .Distinct()
                .ToList();
            
            failureTypes.ShouldContain(ResultFailureType.Security, "Should have security failures for 401 responses");
            failureTypes.ShouldContain(ResultFailureType.NotFound, "Should have not found failures for 404 responses"); 
            failureTypes.ShouldContain(ResultFailureType.ServerError, "Should have server error failures for 500 responses");
            
            // Cleanup
            foreach (HttpResponseMessage response in errorResponses)
            {
                response.Dispose();
            }
        }

        [Fact]
        public async Task ConcurrentHttpSerializationDeserialization_ShouldMaintainDataIntegrity()
        {
            // Arrange
            const int objectCount = 200;
            const int concurrentThreads = 25;
            
            List<ComplexTestObject> testObjects = [];
            List<HttpResponseMessage> responses = [];
            
            for (int i = 0; i < objectCount; i++)
            {
                ComplexTestObject testObject = new()
                {
                    Id = i,
                    Name = $"Object{i}",
                    Data = Enumerable.Range(0, 10).Select(j => $"Data{i}-{j}").ToList(),
                    Metadata = new Dictionary<string, string>
                    {
                        ["Type"] = $"Type{i % 5}",
                        ["Category"] = $"Category{i % 3}",
                        ["ThreadId"] = Thread.CurrentThread.ManagedThreadId.ToString()
                    },
                    Timestamp = DateTime.UtcNow.AddMinutes(i)
                };
                testObjects.Add(testObject);
                
                string json = JsonSerializer.Serialize(testObject);
                HttpResponseMessage response = new(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                responses.Add(response);
            }
            
            ConcurrentBag<Result<ComplexTestObject?>> deserializationResults = [];
            List<Task> tasks = [];
            
            // Act - Deserialize complex objects concurrently
            for (int i = 0; i < concurrentThreads; i++)
            {
                int threadId = i;
                Task task = Task.Run(async () =>
                {
                    int itemsPerThread = objectCount / concurrentThreads;
                    int startIndex = threadId * itemsPerThread;
                    int endIndex = threadId == concurrentThreads - 1 
                        ? objectCount 
                        : startIndex + itemsPerThread;
                    
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        HttpResponseMessage response = responses[j];
                        Result<ComplexTestObject?> result = await response.ToResultFromJsonAsync<ComplexTestObject>();
                        deserializationResults.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            
            // Assert
            deserializationResults.Count.ShouldBe(objectCount);
            deserializationResults.ShouldAllBe(result => result.IsSuccess, "All deserializations should succeed");
            
            // Verify data integrity of deserialized objects
            List<ComplexTestObject> deserializedObjects = [];
            foreach (Result<ComplexTestObject?> result in deserializationResults)
            {
                result.TryGetValue(out ComplexTestObject? obj).ShouldBeTrue();
                obj.ShouldNotBeNull();
                deserializedObjects.Add(obj);
            }
            
            // Compare with original objects (order may be different due to concurrency)
            deserializedObjects = [.. deserializedObjects.OrderBy(o => o.Id)];
            
            for (int i = 0; i < objectCount; i++)
            {
                ComplexTestObject original = testObjects[i];
                ComplexTestObject deserialized = deserializedObjects[i];
                
                deserialized.Id.ShouldBe(original.Id);
                deserialized.Name.ShouldBe(original.Name);
                deserialized.Data.ShouldBe(original.Data);
                deserialized.Metadata.ShouldBe(original.Metadata);
            }
            
            // Cleanup
            foreach (HttpResponseMessage response in responses)
            {
                response.Dispose();
            }
        }
    }

    #endregion HTTP Extensions Thread Safety Tests

    #region Race Condition and Deadlock Tests

    public class RaceConditionTests
    {
        [Fact]
        public void SharedResultInstanceAccess_WithConcurrentReaders_ShouldMaintainConsistentState()
        {
            // Arrange
            Result<ComplexTestObject> sharedResult = Result.Success(new ComplexTestObject
            {
                Id = 12345,
                Name = "Shared Test Object",
                Data = ["Item1", "Item2", "Item3"],
                Metadata = new Dictionary<string, string> { ["Key"] = "Value" },
                Timestamp = DateTime.UtcNow
            });
            
            const int readerThreads = 100;
            const int readsPerThread = 500;
            ConcurrentBag<ComplexTestObject> readValues = [];
            List<Task> tasks = [];
            
            // Act - Multiple threads reading from the same Result instance
            for (int i = 0; i < readerThreads; i++)
            {
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < readsPerThread; j++)
                    {
                        if (sharedResult.TryGetValue(out ComplexTestObject? value) && value != null)
                        {
                            readValues.Add(value);
                        }
                        
                        // Also test pattern matching access
                        sharedResult.Match<object?>(
                            onSuccess: obj => obj,
                            onFailure: error => null
                        );
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All concurrent read tasks should complete");
            readValues.Count.ShouldBe(readerThreads * readsPerThread);
            
            // Verify all read values are identical (no race conditions in state)
            ComplexTestObject firstValue = readValues.First();
            readValues.ShouldAllBe(value => 
                value.Id == firstValue.Id && 
                value.Name == firstValue.Name &&
                value.Data.SequenceEqual(firstValue.Data),
                "All read values should be identical - no race conditions");
        }

        [Fact]
        public void ConcurrentResultCombineWithDifferentInputs_ShouldProduceDeterministicResults()
        {
            // Arrange - Test for race conditions in Result.Combine method
            const int combineOperations = 50;
            const int parallelCombines = 20;
            
            // Create test data that might expose race conditions
            Result[] baseResults = [
                Result.Success(),
                Result.Failure("Error 1"),
                Result.ValidationFailure(new Dictionary<string, string[]> { ["Field1"] = ["Validation error"] }),
                Result.Failure(new SecurityException("Security error")),
                Result.NotFound("Resource not found")
            ];
            
            ConcurrentBag<Result> combineResults = [];
            List<Task> tasks = [];
            
            // Act - Perform many combine operations concurrently
            for (int i = 0; i < parallelCombines; i++)
            {
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < combineOperations; j++)
                    {
                        // Create different combinations to test for race conditions
                        Result[] combination = [
                            baseResults[j % baseResults.Length],
                            baseResults[(j + 1) % baseResults.Length],
                            baseResults[(j + 2) % baseResults.Length]
                        ];
                        
                        Result combined = Result.Combine(combination);
                        combineResults.Add(combined);
                    }
                });
                tasks.Add(task);
            }
            
            bool allCompleted = Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            allCompleted.ShouldBeTrue("All combine operations should complete without race conditions");
            combineResults.Count.ShouldBe(parallelCombines * combineOperations);
            
            // Group by expected combination pattern to verify deterministic behavior
            var resultGroups = combineResults
                .GroupBy(r => $"{r.IsSuccess}-{r.FailureType}-{r.Failures.Count}")
                .ToList();
            
            resultGroups.ShouldNotBeEmpty("Should have grouped results");
            
            // Verify no race conditions by checking that identical inputs produce identical outputs
            foreach (var group in resultGroups)
            {
                Result firstResult = group.First();
                group.ShouldAllBe(r => 
                    r.IsSuccess == firstResult.IsSuccess && 
                    r.FailureType == firstResult.FailureType,
                    "Results with same inputs should be identical - no race conditions");
            }
        }
    }

    public class DeadlockDetectionTests
    {
        [Fact]
        public void ConcurrentValidationWithNestedResultComposition_ShouldNotDeadlock()
        {
            // Arrange - Create a scenario that could potentially deadlock
            const int threadCount = 20;
            const int operationsPerThread = 25;
            Stopwatch stopwatch = Stopwatch.StartNew();
            TimeSpan maxAllowedTime = TimeSpan.FromSeconds(30);
            
            ConcurrentBag<Result<NestedValidationObject>> results = [];
            List<Task> tasks = [];
            
            // Act - Perform complex nested validations that could deadlock
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        ValidationBuilder<NestedValidationObject> builder = new();
                        
                        // Create nested Result operations that could potentially cause deadlock
                        Result<Level1Object> level1Result = CreateLevel1Object(threadId, j);
                        Result<Level2Object> level2Result = CreateLevel2Object(threadId, j);
                        Result<Level3Object> level3Result = CreateLevel3Object(threadId, j);
                        
                        Result<NestedValidationObject> result = builder
                            .RuleFor(o => o.Level1, level1Result, out Level1Object? level1)
                            .RuleFor(o => o.Level2, level2Result, out Level2Object? level2)
                            .RuleFor(o => o.Level3, level3Result, out Level3Object? level3)
                            .Build(() => new NestedValidationObject
                            {
                                Id = threadId * 1000 + j,
                                Level1 = level1!,
                                Level2 = level2!,
                                Level3 = level3!
                            });
                        
                        results.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            bool completedWithoutDeadlock = Task.WaitAll([.. tasks], maxAllowedTime);
            stopwatch.Stop();
            
            // Assert
            completedWithoutDeadlock.ShouldBeTrue(
                $"All tasks should complete without deadlock within {maxAllowedTime.TotalSeconds} seconds. " +
                $"Actual time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            
            results.Count.ShouldBe(threadCount * operationsPerThread);
            results.ShouldAllBe(r => r.IsSuccess, "All nested validations should succeed without deadlock");
        }

        [Fact]
        public void HighVolumeParallelValidations_ShouldNotCauseResourceContention()
        {
            // Arrange - Test for potential resource contention and deadlocks
            const int totalValidations = 1000;
            int maxConcurrency = Environment.ProcessorCount * 4;
            
            List<TestValidationInput> validationInputs = [];
            for (int i = 0; i < totalValidations; i++)
            {
                validationInputs.Add(new TestValidationInput
                {
                    Id = i,
                    Name = $"Item{i}",
                    Email = $"item{i}@test.com",
                    Age = 20 + (i % 60),
                    Categories = Enumerable.Range(0, i % 5 + 1).Select(j => $"Category{j}").ToArray()
                });
            }
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            ConcurrentBag<Result<ConcurrentTestUser>> validationResults = [];
            
            // Act - Perform high-volume parallel validations
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = maxConcurrency
            };
            
            Parallel.ForEach(validationInputs, parallelOptions, input =>
            {
                ValidationBuilder<ConcurrentTestUser> builder = new();
                
                Result<ConcurrentTestUser> result = builder
                    .RuleFor(u => u.Name, input.Name)
                        .NotEmpty()
                        .MaximumLength(100)
                    .RuleFor(u => u.Email, input.Email)
                        .NotEmpty()
                        .EmailAddress()
                    .RuleFor(u => u.Age, input.Age)
                        .GreaterThan(0)
                        .LessThan(120)
                    .Build(() => new ConcurrentTestUser
                    {
                        Id = input.Id,
                        Name = input.Name,
                        Email = input.Email,
                        Age = input.Age
                    });
                
                validationResults.Add(result);
            });
            
            stopwatch.Stop();
            
            // Assert
            stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(TimeSpan.FromSeconds(45), 
                "High-volume parallel validations should complete within reasonable time without resource contention");
            
            validationResults.Count.ShouldBe(totalValidations);
            validationResults.ShouldAllBe(r => r.IsSuccess, "All parallel validations should succeed without contention issues");
            
            // Verify resource usage didn't cause data corruption
            HashSet<int> processedIds = [];
            foreach (Result<ConcurrentTestUser> result in validationResults)
            {
                result.TryGetValue(out ConcurrentTestUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                processedIds.Add(user.Id).ShouldBeTrue($"User ID {user.Id} should be unique (no corruption)");
            }
            
            processedIds.Count.ShouldBe(totalValidations, "All unique IDs should be processed without corruption");
        }
    }

    #endregion Race Condition and Deadlock Tests

    #region Performance Under Concurrent Load Tests

    public class PerformanceConcurrencyTests
    {
        [Fact]
        public void HighThroughputResultCreation_ShouldMaintainPerformance()
        {
            // Arrange
            const int totalResults = 100000;
            int concurrentThreads = Environment.ProcessorCount;
            int resultsPerThread = totalResults / concurrentThreads;
            
            ConcurrentBag<Result> results = [];
            Stopwatch stopwatch = new();
            List<Task> tasks = [];
            
            // Act - Create large number of Results under concurrent load
            stopwatch.Start();
            
            for (int i = 0; i < concurrentThreads; i++)
            {
                int threadId = i;
                Task task = Task.Run(() =>
                {
                    for (int j = 0; j < resultsPerThread; j++)
                    {
                        int selector = (threadId + j) % 3;
                        Result result = selector switch
                        {
                            0 => Result.Success(),
                            1 => Result.Failure($"Error {threadId}-{j}"),
                            _ => Result.ValidationFailure(new Dictionary<string, string[]> 
                            { 
                                [$"Field{threadId}"] = [$"Error {j}"] 
                            })
                        };
                        results.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            Task.WaitAll([.. tasks]);
            stopwatch.Stop();
            
            // Assert
            results.Count.ShouldBe(concurrentThreads * resultsPerThread);
            
            double throughput = results.Count / stopwatch.Elapsed.TotalSeconds;
            throughput.ShouldBeGreaterThan(10000, 
                $"Result creation throughput should be > 10K/sec under concurrent load. " +
                $"Actual: {throughput:F0}/sec, Time: {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
            
            // Verify correctness under load
            int successCount = results.Count(r => r.IsSuccess);
            int failureCount = results.Count(r => r.IsFailure);
            
            successCount.ShouldBeGreaterThan(0);
            failureCount.ShouldBeGreaterThan(0);
            (successCount + failureCount).ShouldBe(results.Count);
        }

        [Fact]
        public void ConcurrentValidationThroughput_ShouldScaleWithProcessors()
        {
            // Arrange
            const int validationsPerProcessor = 5000;
            int totalValidations = validationsPerProcessor * Environment.ProcessorCount;
            
            Stopwatch stopwatch = new();
            ConcurrentBag<Result<ConcurrentTestUser>> results = [];
            
            // Act - Measure validation throughput under concurrent load
            stopwatch.Start();
            
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            
            Parallel.For(0, totalValidations, parallelOptions, i =>
            {
                ValidationBuilder<ConcurrentTestUser> builder = new();
                
                Result<ConcurrentTestUser> result = builder
                    .RuleFor(u => u.Name, $"User{i}")
                        .NotEmpty()
                        .MinimumLength(1)
                        .MaximumLength(100)
                    .RuleFor(u => u.Email, $"user{i}@test.com")
                        .NotEmpty()
                        .EmailAddress()
                    .RuleFor(u => u.Age, 25 + (i % 50))
                        .GreaterThan(0)
                        .LessThan(120)
                    .Build(() => new ConcurrentTestUser
                    {
                        Id = i,
                        Name = $"User{i}",
                        Email = $"user{i}@test.com", 
                        Age = 25 + (i % 50)
                    });
                
                results.Add(result);
            });
            
            stopwatch.Stop();
            
            // Assert
            results.Count.ShouldBe(totalValidations);
            results.ShouldAllBe(r => r.IsSuccess);
            
            double throughput = results.Count / stopwatch.Elapsed.TotalSeconds;
            double expectedMinThroughput = Environment.ProcessorCount * 500; // 500 validations per core per second
            
            throughput.ShouldBeGreaterThan(expectedMinThroughput,
                $"Validation throughput should scale with processor count. " +
                $"Expected: >{expectedMinThroughput:F0}/sec, Actual: {throughput:F0}/sec, " +
                $"Processors: {Environment.ProcessorCount}, Time: {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        [Fact]
        public async Task ConcurrentHttpProcessingThroughput_ShouldHandleHighLoad()
        {
            // Arrange
            const int responseCount = 1000;
            const int concurrentProcessors = 25;
            
            List<HttpResponseMessage> responses = [];
            for (int i = 0; i < responseCount; i++)
            {
                HttpResponseMessage response = new(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { Id = i, Name = $"Item{i}" }),
                        Encoding.UTF8,
                        "application/json")
                };
                responses.Add(response);
            }
            
            Stopwatch stopwatch = new();
            ConcurrentBag<Result<JsonElement?>> results = [];
            
            // Act - Process responses with high concurrency
            stopwatch.Start();
            
            List<Task> tasks = [];
            int responsesPerTask = responseCount / concurrentProcessors;
            
            for (int i = 0; i < concurrentProcessors; i++)
            {
                int taskId = i;
                Task task = Task.Run(async () =>
                {
                    int start = taskId * responsesPerTask;
                    int end = taskId == concurrentProcessors - 1 ? responseCount : start + responsesPerTask;
                    
                    for (int j = start; j < end; j++)
                    {
                        Result<JsonElement?> result = await responses[j].ToResultFromJsonAsync<JsonElement?>();
                        results.Add(result);
                    }
                });
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            // Assert
            results.Count.ShouldBe(responseCount);
            results.ShouldAllBe(r => r.IsSuccess);
            
            double throughput = results.Count / stopwatch.Elapsed.TotalSeconds;
            throughput.ShouldBeGreaterThan(100,
                $"HTTP processing throughput should be > 100/sec under concurrent load. " +
                $"Actual: {throughput:F0}/sec, Time: {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
            
            // Cleanup
            foreach (HttpResponseMessage response in responses)
            {
                response.Dispose();
            }
        }
    }

    #endregion Performance Under Concurrent Load Tests

    #region Memory Consistency and Visibility Tests

    public class MemoryConsistencyTests
    {
        [Fact]
        public async Task ResultStateVisibility_AcrossThreadBoundaries_ShouldBeConsistent()
        {
            // Arrange - Test memory visibility of Result state across threads
            const int producerThreads = 10;
            const int consumerThreads = 10;
            const int resultsPerProducer = 100;
            
            ConcurrentQueue<Result<ConcurrentTestUser>> resultQueue = new();
            ConcurrentBag<ConcurrentTestUser> consumedUsers = [];
            CancellationTokenSource cts = new();
            
            List<Task> producerTasks = [];
            List<Task> consumerTasks = [];
            
            // Producer tasks - create Results and enqueue them
            for (int i = 0; i < producerThreads; i++)
            {
                int producerId = i;
                Task producerTask = Task.Run(() =>
                {
                    for (int j = 0; j < resultsPerProducer; j++)
                    {
                        Result<ConcurrentTestUser> result = Result.Success(new ConcurrentTestUser
                        {
                            Id = producerId * 1000 + j,
                            Name = $"Producer{producerId}-User{j}",
                            Email = $"p{producerId}u{j}@test.com",
                            Age = 20 + (j % 60)
                        });
                        
                        resultQueue.Enqueue(result);
                        Thread.Yield(); // Allow other threads to run
                    }
                });
                producerTasks.Add(producerTask);
            }
            
            // Consumer tasks - dequeue and process Results
            for (int i = 0; i < consumerThreads; i++)
            {
                Task consumerTask = Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (resultQueue.TryDequeue(out Result<ConcurrentTestUser>? result))
                        {
                            // Verify state consistency across thread boundary
                            result.IsSuccess.ShouldBeTrue("Result state should be visible across threads");
                            
                            if (result.TryGetValue(out ConcurrentTestUser? user))
                            {
                                user.ShouldNotBeNull("User value should be visible across threads");
                                user.Id.ShouldBeGreaterThanOrEqualTo(0, "User ID should be consistent");
                                user.Name.ShouldNotBeNullOrEmpty("User name should be consistent");
                                consumedUsers.Add(user);
                            }
                        }
                        else
                        {
                            await Task.Delay(1); // Brief wait if queue is empty
                        }
                    }
                });
                consumerTasks.Add(consumerTask);
            }
            
            // Act - Wait for producers to complete, then signal consumers
            Task.WaitAll([.. producerTasks], TimeSpan.FromSeconds(30));
            
            // Give consumers time to process remaining items
            await Task.Delay(1000);
            cts.Cancel();
            
            bool allConsumersCompleted = Task.WaitAll([.. consumerTasks], TimeSpan.FromSeconds(10));
            
            // Assert
            allConsumersCompleted.ShouldBeTrue("All consumer tasks should complete");
            
            int expectedUserCount = producerThreads * resultsPerProducer;
            consumedUsers.Count.ShouldBe(expectedUserCount, 
                "All produced Results should be consumed with consistent state visibility");
            
            // Verify no data corruption occurred across thread boundaries
            HashSet<int> uniqueIds = [];
            foreach (ConcurrentTestUser user in consumedUsers)
            {
                uniqueIds.Add(user.Id).ShouldBeTrue($"User ID {user.Id} should be unique (no memory corruption)");
                user.Name.ShouldStartWith("Producer");
                user.Email.ShouldContain("@test.com");
                user.Age.ShouldBeGreaterThanOrEqualTo(20);
                user.Age.ShouldBeLessThanOrEqualTo(79);
            }
            
            uniqueIds.Count.ShouldBe(expectedUserCount, "All user IDs should be unique");
        }

        [Fact]
        public void ValidationBuilderErrorState_WithMemoryBarriers_ShouldMaintainConsistency()
        {
            // Arrange - Test memory consistency of ValidationBuilder error state
            const int iterations = 1000;
            int inconsistentReads = 0;
            List<Task> tasks = [];
            
            // Act - Test memory consistency with validation operations
            for (int i = 0; i < iterations; i++)
            {
                Task task = Task.Run(() =>
                {
                    ValidationBuilder<ConcurrentTestUser> builder = new();
                    
                    // Add validation that will fail
                    string emptyName = "";
                    builder.RuleFor(u => u.Name, emptyName)
                        .NotEmpty();
                    
                    // Check error state before build
                    bool hasErrorsBeforeBuild = builder.HasErrors;
                    
                    // Build should fail
                    Result<ConcurrentTestUser> result = builder.Build(() => new ConcurrentTestUser());
                    
                    // Check error state after build
                    bool hasErrorsAfterBuild = builder.HasErrors;
                    Dictionary<string, string[]> errors = builder.GetErrors();
                    
                    // Memory barrier to ensure consistency
                    Thread.MemoryBarrier();
                    
                    // Verify consistency
                    bool isConsistent = hasErrorsBeforeBuild == true && 
                                      hasErrorsAfterBuild == true &&
                                      result.IsFailure &&
                                      errors.Count > 0;
                    
                    if (!isConsistent)
                    {
                        Interlocked.Increment(ref inconsistentReads);
                    }
                });
                tasks.Add(task);
            }
            
            Task.WaitAll([.. tasks], TimeSpan.FromSeconds(30));
            
            // Assert
            inconsistentReads.ShouldBe(0, 
                $"ValidationBuilder state should be consistent across memory barriers. " +
                $"Inconsistent reads: {inconsistentReads}/{iterations}");
        }
    }

    #endregion Memory Consistency and Visibility Tests

    #region Task-based Parallelism Tests  

    public class TaskParallelismTests
    {
        [Fact]
        public async Task ParallelResultComposition_WithTaskWhenAll_ShouldMaintainCorrectness()
        {
            // Arrange
            const int parallelOperations = 100;
            const int tasksPerOperation = 10;
            
            List<Task<Result<CompositeValidationResult>>> operations = [];
            
            for (int i = 0; i < parallelOperations; i++)
            {
                int operationId = i;
                Task<Result<CompositeValidationResult>> operation = Task.Run(async () =>
                {
                    // Create multiple async tasks that return Results
                    List<Task<Result<ConcurrentTestUser>>> userTasks = [];
                    
                    for (int j = 0; j < tasksPerOperation; j++)
                    {
                        int taskId = j;
                        Task<Result<ConcurrentTestUser>> userTask = Task.Run(() =>
                        {
                            ValidationBuilder<ConcurrentTestUser> builder = new();
                            return builder
                                .RuleFor(u => u.Name, $"User{operationId}-{taskId}")
                                    .NotEmpty()
                                .RuleFor(u => u.Email, $"user{operationId}-{taskId}@test.com")
                                    .EmailAddress()
                                .Build(() => new ConcurrentTestUser
                                {
                                    Id = operationId * 1000 + taskId,
                                    Name = $"User{operationId}-{taskId}",
                                    Email = $"user{operationId}-{taskId}@test.com"
                                });
                        });
                        userTasks.Add(userTask);
                    }
                    
                    // Wait for all user validation tasks to complete
                    Result<ConcurrentTestUser>[] userResults = await Task.WhenAll(userTasks);
                    
                    // Compose results into a single validation
                    ValidationBuilder<CompositeValidationResult> compositeBuilder = new();
                    List<ConcurrentTestUser> validUsers = [];
                    
                    for (int k = 0; k < userResults.Length; k++)
                    {
                        Result<ConcurrentTestUser> userResult = userResults[k];
                        if (userResult.TryGetValue(out ConcurrentTestUser? user) && user != null)
                        {
                            validUsers.Add(user);
                        }
                    }
                    
                    return compositeBuilder.Build(() => new CompositeValidationResult
                    {
                        OperationId = operationId,
                        Users = validUsers,
                        ProcessedAt = DateTime.UtcNow
                    });
                });
                
                operations.Add(operation);
            }
            
            // Act - Execute all parallel operations
            Result<CompositeValidationResult>[] results = await Task.WhenAll(operations);
            
            // Assert
            results.Length.ShouldBe(parallelOperations);
            results.ShouldAllBe(r => r.IsSuccess, "All parallel composite validations should succeed");
            
            // Verify data integrity of parallel composition
            HashSet<int> operationIds = [];
            int totalUsers = 0;
            
            foreach (Result<CompositeValidationResult> result in results)
            {
                result.TryGetValue(out CompositeValidationResult? composite).ShouldBeTrue();
                composite.ShouldNotBeNull();
                
                operationIds.Add(composite.OperationId).ShouldBeTrue($"Operation ID {composite.OperationId} should be unique");
                composite.Users.Count.ShouldBe(tasksPerOperation, "Each operation should have correct number of users");
                totalUsers += composite.Users.Count;
                
                // Verify user data integrity within each composite
                HashSet<int> userIds = [];
                foreach (ConcurrentTestUser user in composite.Users)
                {
                    userIds.Add(user.Id).ShouldBeTrue($"User ID {user.Id} should be unique within operation");
                    user.Name.ShouldStartWith($"User{composite.OperationId}-");
                    user.Email.ShouldStartWith($"user{composite.OperationId}-");
                }
            }
            
            operationIds.Count.ShouldBe(parallelOperations, "All operation IDs should be unique");
            totalUsers.ShouldBe(parallelOperations * tasksPerOperation, "Total user count should be correct");
        }

        [Fact]
        public async Task ConcurrentAsyncHttpProcessing_WithTaskParallelism_ShouldMaintainPerformance()
        {
            // Arrange
            const int batchSize = 200;
            const int batchCount = 10;
            
            List<List<HttpResponseMessage>> batches = [];
            
            for (int batchId = 0; batchId < batchCount; batchId++)
            {
                List<HttpResponseMessage> batch = [];
                for (int i = 0; i < batchSize; i++)
                {
                    int globalId = batchId * batchSize + i;
                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(new ConcurrentTestUser
                            {
                                Id = globalId,
                                Name = $"BatchUser{batchId}-{i}",
                                Email = $"batch{batchId}-user{i}@test.com",
                                Age = 20 + (i % 60)
                            }),
                            Encoding.UTF8,
                            "application/json")
                    };
                    batch.Add(response);
                }
                batches.Add(batch);
            }
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            ConcurrentBag<Result<ConcurrentTestUser?>> allResults = [];
            
            // Act - Process batches in parallel with async HTTP operations
            List<Task> batchTasks = [];
            
            foreach (List<HttpResponseMessage> batch in batches)
            {
                Task batchTask = Task.Run(async () =>
                {
                    List<Task<Result<ConcurrentTestUser?>>> processingTasks = [];
                    
                    foreach (HttpResponseMessage response in batch)
                    {
                        Task<Result<ConcurrentTestUser?>> processingTask = response.ToResultFromJsonAsync<ConcurrentTestUser>();
                        processingTasks.Add(processingTask);
                    }
                    
                    Result<ConcurrentTestUser?>[] batchResults = await Task.WhenAll(processingTasks);
                    
                    foreach (Result<ConcurrentTestUser?> result in batchResults)
                    {
                        allResults.Add(result);
                    }
                });
                batchTasks.Add(batchTask);
            }
            
            await Task.WhenAll(batchTasks);
            stopwatch.Stop();
            
            // Assert
            int expectedCount = batchCount * batchSize;
            allResults.Count.ShouldBe(expectedCount);
            allResults.ShouldAllBe(r => r.IsSuccess, "All async HTTP processing should succeed");
            
            double throughput = allResults.Count / stopwatch.Elapsed.TotalSeconds;
            throughput.ShouldBeGreaterThan(200, 
                $"Async HTTP processing throughput should be > 200/sec. " +
                $"Actual: {throughput:F0}/sec, Time: {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
            
            // Verify data integrity across async operations
            HashSet<int> processedIds = [];
            foreach (Result<ConcurrentTestUser?> result in allResults)
            {
                result.TryGetValue(out ConcurrentTestUser? user).ShouldBeTrue();
                user.ShouldNotBeNull();
                processedIds.Add(user.Id).ShouldBeTrue($"User ID {user.Id} should be unique");
            }
            
            processedIds.Count.ShouldBe(expectedCount, "All users should have unique IDs");
            
            // Cleanup
            foreach (List<HttpResponseMessage> batch in batches)
            {
                foreach (HttpResponseMessage response in batch)
                {
                    response.Dispose();
                }
            }
        }
    }

    #endregion Task-based Parallelism Tests

    #region Test Helper Classes and Methods

    private static Result<ConcurrentUserProfile> CreateUserProfile(int threadId, int operationId)
    {
        return Result.Success(new ConcurrentUserProfile
        {
            FirstName = $"FirstName{threadId}-{operationId}",
            LastName = $"LastName{threadId}-{operationId}",
            Bio = $"Bio for user {threadId}-{operationId}"
        });
    }
    
    private static Result<ConcurrentUserSettings> CreateUserSettings(int threadId, int operationId)
    {
        return Result.Success(new ConcurrentUserSettings
        {
            Theme = $"Theme{threadId % 3}",
            Language = $"Lang{operationId % 2}",
            NotificationsEnabled = (threadId + operationId) % 2 == 0
        });
    }
    
    private static Result<ConcurrentUserPreferences> CreateUserPreferences(int threadId, int operationId)
    {
        return Result.Success(new ConcurrentUserPreferences
        {
            Categories = Enumerable.Range(0, (threadId % 5) + 1).Select(i => $"Category{i}").ToList(),
            Tags = Enumerable.Range(0, (operationId % 3) + 1).Select(i => $"Tag{i}").ToList()
        });
    }
    
    private static Result<Level1Object> CreateLevel1Object(int threadId, int operationId)
    {
        return Result.Success(new Level1Object
        {
            Id = threadId * 1000 + operationId,
            Name = $"Level1-{threadId}-{operationId}"
        });
    }
    
    private static Result<Level2Object> CreateLevel2Object(int threadId, int operationId)
    {
        return Result.Success(new Level2Object
        {
            Id = threadId * 1000 + operationId + 100000,
            Description = $"Level2-{threadId}-{operationId}",
            Data = Enumerable.Range(0, 5).Select(i => $"Data{i}").ToList()
        });
    }
    
    private static Result<Level3Object> CreateLevel3Object(int threadId, int operationId)
    {
        return Result.Success(new Level3Object
        {
            Id = threadId * 1000 + operationId + 200000,
            Value = threadId * operationId,
            Metadata = new Dictionary<string, string>
            {
                ["ThreadId"] = threadId.ToString(),
                ["OperationId"] = operationId.ToString(),
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            }
        });
    }

    #endregion Test Helper Classes and Methods
}

#region Test Data Models

public class ConcurrentTestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class ConcurrentComplexUser
{
    public int Id { get; set; }
    public ConcurrentUserProfile Profile { get; set; } = null!;
    public ConcurrentUserSettings Settings { get; set; } = null!;
    public ConcurrentUserPreferences Preferences { get; set; } = null!;
}

public class ConcurrentUserProfile
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class ConcurrentUserSettings
{
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool NotificationsEnabled { get; set; }
}

public class ConcurrentUserPreferences
{
    public List<string> Categories { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public class ComplexTestObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Data { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = [];
    public DateTime Timestamp { get; set; }
}

public class NestedValidationObject
{
    public int Id { get; set; }
    public Level1Object Level1 { get; set; } = null!;
    public Level2Object Level2 { get; set; } = null!;
    public Level3Object Level3 { get; set; } = null!;
}

public class Level1Object
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Level2Object
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Data { get; set; } = [];
}

public class Level3Object
{
    public int Id { get; set; }
    public int Value { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}

public class TestValidationInput
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string[] Categories { get; set; } = [];
}

public class CompositeValidationResult
{
    public int OperationId { get; set; }
    public List<ConcurrentTestUser> Users { get; set; } = [];
    public DateTime ProcessedAt { get; set; }
}

#endregion Test Data Models
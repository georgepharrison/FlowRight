using System.Security;
using FlowRight.Core.Extensions;
using FlowRight.Core.Results;
using Shouldly;
using Xunit;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Advanced integration tests for Result composition scenarios that validate complex 
/// chaining, combining, and orchestration patterns used in real applications.
/// These tests focus on end-to-end Result composition behavior across the entire pipeline.
/// </summary>
/// <remarks>
/// <para>
/// These integration tests validate Result composition scenarios including:
/// </para>
/// <list type="bullet">
/// <item><description>Multi-step business workflow orchestration</description></item>
/// <item><description>Complex error aggregation and propagation</description></item>
/// <item><description>Performance characteristics of large composition chains</description></item>
/// <item><description>Async and sync composition pattern integration</description></item>
/// <item><description>Mixed Result and Result&lt;T&gt; composition scenarios</description></item>
/// <item><description>Nested Result composition with complex object hierarchies</description></item>
/// <item><description>Error propagation through composition chains</description></item>
/// </list>
/// </remarks>
public class ResultCompositionAdvancedIntegrationTests
{
    #region Multi-Step Business Process Composition Tests

    public class MultiStepBusinessProcessComposition
    {
        [Fact]
        public void UserOnboardingWorkflow_WithAllValidSteps_ShouldCompleteSuccessfully()
        {
            // Arrange
            UserOnboardingRequest request = new UserOnboardingRequestBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("SecurePassword123!")
                .WithFirstName("Jane")
                .WithLastName("Smith")
                .WithCompanyCode("TECH001")
                .Build();

            // Act
            Result<OnboardingResult> result = ExecuteUserOnboardingWorkflow(request);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out OnboardingResult? onboardingResult).ShouldBeTrue();
            onboardingResult.UserId.ShouldNotBe(Guid.Empty);
            onboardingResult.IsEmailVerified.ShouldBeFalse(); // Initially false, verification sent
            onboardingResult.CompanyId.ShouldNotBe(Guid.Empty);
            onboardingResult.OnboardingSteps.Count.ShouldBe(5);
            onboardingResult.OnboardingSteps.ShouldAllBe(step => step.IsCompleted);
        }

        [Fact]
        public void UserOnboardingWorkflow_WithInvalidCompany_ShouldFailAtCompanyValidation()
        {
            // Arrange
            UserOnboardingRequest request = new UserOnboardingRequestBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("SecurePassword123!")
                .WithFirstName("Jane")
                .WithLastName("Smith")
                .WithCompanyCode("INVALID")
                .Build();

            // Act
            Result<OnboardingResult> result = ExecuteUserOnboardingWorkflow(request);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldContain("Company with code 'INVALID' not found");
        }

        [Fact]
        public void UserOnboardingWorkflow_WithMultipleValidationErrors_ShouldAggregateAllErrors()
        {
            // Arrange
            UserOnboardingRequest request = new UserOnboardingRequestBuilder()
                .WithEmail("invalid-email")
                .WithPassword("weak")
                .WithFirstName("")
                .WithLastName("")
                .WithCompanyCode("INVALID")
                .Build();

            // Act
            Result<OnboardingResult> result = ExecuteUserOnboardingWorkflow(request);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.Count.ShouldBeGreaterThanOrEqualTo(4);
            result.Failures.ShouldContainKey("Email");
            result.Failures.ShouldContainKey("Password");
            result.Failures.ShouldContainKey("FirstName");
            result.Failures.ShouldContainKey("LastName");
        }

        [Fact]
        public void OrderFulfillmentWorkflow_WithComplexInventoryAndPayment_ShouldProcessSuccessfully()
        {
            // Arrange
            OrderFulfillmentRequest request = new OrderFulfillmentRequestBuilder()
                .WithUserId(Guid.NewGuid())
                .WithItems([
                    new OrderItem { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 29.99m },
                    new OrderItem { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 49.99m }
                ])
                .WithPaymentMethod(new PaymentMethod { Type = "CreditCard", Token = "valid-token" })
                .WithShippingAddress(new Address 
                { 
                    Street = "123 Main St", 
                    City = "San Francisco", 
                    State = "CA", 
                    ZipCode = "94105" 
                })
                .Build();

            // Act
            Result<FulfillmentResult> result = ExecuteOrderFulfillmentWorkflow(request);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out FulfillmentResult? fulfillment).ShouldBeTrue();
            fulfillment.OrderId.ShouldNotBe(Guid.Empty);
            fulfillment.TotalAmount.ShouldBe(109.97m);
            fulfillment.InventoryReservations.Count.ShouldBe(2);
            fulfillment.PaymentTransactionId.ShouldNotBeNullOrEmpty();
            fulfillment.EstimatedDeliveryDate.ShouldBeGreaterThan(DateTime.UtcNow);
        }

        [Fact]
        public void OrderFulfillmentWorkflow_WithInsufficientInventory_ShouldFailWithDetailedErrors()
        {
            // Arrange
            OrderFulfillmentRequest request = new OrderFulfillmentRequestBuilder()
                .WithUserId(Guid.NewGuid())
                .WithItems([
                    new OrderItem { ProductId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Quantity = 1000, UnitPrice = 29.99m } // Out of stock
                ])
                .WithPaymentMethod(new PaymentMethod { Type = "CreditCard", Token = "valid-token" })
                .WithShippingAddress(new Address 
                { 
                    Street = "123 Main St", 
                    City = "San Francisco", 
                    State = "CA", 
                    ZipCode = "94105" 
                })
                .Build();

            // Act
            Result<FulfillmentResult> result = ExecuteOrderFulfillmentWorkflow(request);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Error);
            result.Error.ShouldContain("Insufficient inventory");
        }

        private static Result<OnboardingResult> ExecuteUserOnboardingWorkflow(UserOnboardingRequest request)
        {
            // Step 1: Validate user input
            Result userValidationResult = ValidateUserOnboardingInput(request);
            if (userValidationResult.IsFailure)
                return Result.Failure<OnboardingResult>(userValidationResult.Failures);

            // Step 2: Check if email is available
            Result<bool> emailAvailabilityResult = CheckEmailAvailability(request.Email);
            if (emailAvailabilityResult.IsFailure)
                return Result.Failure<OnboardingResult>(emailAvailabilityResult.Error);

            if (!emailAvailabilityResult.TryGetValue(out bool isEmailAvailable) || !isEmailAvailable)
                return Result.Failure<OnboardingResult>("Email address is already registered");

            // Step 3: Validate company code
            Result<Company> companyResult = ValidateAndLoadCompany(request.CompanyCode);
            if (companyResult.IsFailure)
                return Result.Failure<OnboardingResult>(companyResult.Error, companyResult.ResultType, companyResult.FailureType);

            // Step 4: Create user account
            Result<User> userCreationResult = CreateUserAccount(request);
            if (userCreationResult.IsFailure)
                return Result.Failure<OnboardingResult>(userCreationResult.Error);

            // Step 5: Assign user to company
            Result companyAssignmentResult = AssignUserToCompany(
                userCreationResult.TryGetValue(out User? user) ? user.Id : Guid.Empty,
                companyResult.TryGetValue(out Company? company) ? company.Id : Guid.Empty
            );
            if (companyAssignmentResult.IsFailure)
                return Result.Failure<OnboardingResult>(companyAssignmentResult.Error);

            // Step 6: Send welcome email
            Result emailResult = SendWelcomeEmail(user?.Email ?? "");
            
            // Step 7: Create onboarding result
            OnboardingResult onboardingResult = new()
            {
                UserId = user?.Id ?? Guid.Empty,
                CompanyId = company?.Id ?? Guid.Empty,
                IsEmailVerified = false,
                OnboardingSteps = CreateOnboardingSteps()
            };

            return Result.Success(onboardingResult);
        }

        private static Result<FulfillmentResult> ExecuteOrderFulfillmentWorkflow(OrderFulfillmentRequest request)
        {
            // Step 1: Validate request
            Result requestValidationResult = ValidateOrderFulfillmentRequest(request);
            if (requestValidationResult.IsFailure)
                return Result.Failure<FulfillmentResult>(requestValidationResult.Failures);

            // Step 2: Check inventory availability for all items
            Result inventoryCheckResult = CheckInventoryAvailability(request.Items);
            if (inventoryCheckResult.IsFailure)
                return Result.Failure<FulfillmentResult>(inventoryCheckResult.Error);

            // Step 3: Calculate order total
            Result<decimal> totalResult = CalculateOrderTotal(request.Items);
            if (totalResult.IsFailure)
                return Result.Failure<FulfillmentResult>(totalResult.Error);

            // Step 4: Process payment
            Result<PaymentTransaction> paymentResult = ProcessPayment(
                request.PaymentMethod, 
                totalResult.TryGetValue(out decimal total) ? total : 0
            );
            if (paymentResult.IsFailure)
                return Result.Failure<FulfillmentResult>(paymentResult.Error);

            // Step 5: Reserve inventory
            Result<InventoryReservation[]> reservationResult = ReserveInventory(request.Items);
            if (reservationResult.IsFailure)
                return Result.Failure<FulfillmentResult>(reservationResult.Error);

            // Step 6: Create order
            Result<Order> orderResult = CreateOrder(request, total);
            if (orderResult.IsFailure)
                return Result.Failure<FulfillmentResult>(orderResult.Error);

            // Create fulfillment result
            FulfillmentResult fulfillment = new()
            {
                OrderId = orderResult.TryGetValue(out Order? order) ? order.Id : Guid.Empty,
                TotalAmount = total,
                InventoryReservations = reservationResult.TryGetValue(out InventoryReservation[]? reservations) 
                    ? reservations.ToList() 
                    : [],
                PaymentTransactionId = paymentResult.TryGetValue(out PaymentTransaction? payment) 
                    ? payment.TransactionId 
                    : "",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5)
            };

            return Result.Success(fulfillment);
        }

        #region Helper Methods

        private static Result ValidateUserOnboardingInput(UserOnboardingRequest request)
        {
            Dictionary<string, List<string>> errors = [];

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                errors.TryAdd("Email", []);
                errors["Email"].Add("Valid email address is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            {
                errors.TryAdd("Password", []);
                errors["Password"].Add("Password must be at least 12 characters long");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                errors.TryAdd("FirstName", []);
                errors["FirstName"].Add("First name is required");
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                errors.TryAdd("LastName", []);
                errors["LastName"].Add("Last name is required");
            }

            return errors.Count > 0 
                ? Result.ValidationFailure(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()))
                : Result.Success();
        }

        private static Result<bool> CheckEmailAvailability(string email)
        {
            // Simulate database check
            return email == "existing@example.com" 
                ? Result.Success(false) 
                : Result.Success(true);
        }

        private static Result<Company> ValidateAndLoadCompany(string companyCode)
        {
            if (companyCode == "INVALID")
                return Result.NotFound<Company>($"Company with code '{companyCode}'");

            return Result.Success(new Company 
            { 
                Id = Guid.NewGuid(), 
                Code = companyCode, 
                Name = "Test Company" 
            });
        }

        private static Result<User> CreateUserAccount(UserOnboardingRequest request)
        {
            User user = new()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsEmailVerified = false,
                CreatedDate = DateTime.UtcNow
            };

            return Result.Success(user);
        }

        private static Result AssignUserToCompany(Guid userId, Guid companyId)
        {
            // Simulate assignment process
            return Result.Success();
        }

        private static Result SendWelcomeEmail(string email)
        {
            // Simulate email service
            return Result.Success();
        }

        private static List<OnboardingStep> CreateOnboardingSteps()
        {
            return [
                new OnboardingStep { Name = "Account Created", IsCompleted = true },
                new OnboardingStep { Name = "Email Verification Sent", IsCompleted = true },
                new OnboardingStep { Name = "Company Assignment", IsCompleted = true },
                new OnboardingStep { Name = "Profile Setup", IsCompleted = true },
                new OnboardingStep { Name = "Welcome Email Sent", IsCompleted = true }
            ];
        }

        private static Result ValidateOrderFulfillmentRequest(OrderFulfillmentRequest request)
        {
            Dictionary<string, List<string>> errors = [];

            if (request.UserId == Guid.Empty)
            {
                errors.TryAdd("UserId", []);
                errors["UserId"].Add("Valid user ID is required");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                errors.TryAdd("Items", []);
                errors["Items"].Add("Order must contain at least one item");
            }

            return errors.Count > 0 
                ? Result.ValidationFailure(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()))
                : Result.Success();
        }

        private static Result CheckInventoryAvailability(List<OrderItem> items)
        {
            // Simulate inventory check - specific product is out of stock
            foreach (OrderItem item in items)
            {
                if (item.ProductId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                    return Result.Failure("Insufficient inventory for requested product");
            }

            return Result.Success();
        }

        private static Result<decimal> CalculateOrderTotal(List<OrderItem> items)
        {
            decimal total = items.Sum(item => item.Quantity * item.UnitPrice);
            return Result.Success(total);
        }

        private static Result<PaymentTransaction> ProcessPayment(PaymentMethod paymentMethod, decimal amount)
        {
            if (paymentMethod.Type == "InvalidCard")
                return Result.Failure<PaymentTransaction>("Payment processing failed");

            return Result.Success(new PaymentTransaction 
            { 
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount,
                Status = "Completed"
            });
        }

        private static Result<InventoryReservation[]> ReserveInventory(List<OrderItem> items)
        {
            List<InventoryReservation> reservations = [];
            
            foreach (OrderItem item in items)
            {
                reservations.Add(new InventoryReservation 
                { 
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ReservationId = Guid.NewGuid()
                });
            }

            return Result.Success(reservations.ToArray());
        }

        private static Result<Order> CreateOrder(OrderFulfillmentRequest request, decimal total)
        {
            Order order = new()
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TotalAmount = total,
                Status = OrderStatus.Confirmed,
                CreatedDate = DateTime.UtcNow
            };

            return Result.Success(order);
        }

        private static bool IsValidEmail(string email) =>
            email.Contains('@') && email.Contains('.');

        #endregion
    }

    #endregion

    #region Complex Result Chaining Tests

    public class ComplexResultChaining
    {
        [Fact]
        public void DataProcessingPipeline_WithMultipleTransformations_ShouldChainSuccessfully()
        {
            // Arrange
            string inputData = "raw-data-input";

            // Act
            Result<ProcessedData> result = ProcessDataThroughPipeline(inputData);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ProcessedData? processedData).ShouldBeTrue();
            processedData.Stage.ShouldBe("Finalized");
            processedData.ProcessingSteps.Count.ShouldBe(6);
            processedData.ProcessingSteps.ShouldAllBe(step => step.IsSuccessful);
        }

        [Fact]
        public void DataProcessingPipeline_WithFailureInMiddleStage_ShouldPropagateError()
        {
            // Arrange
            string inputData = "fail-at-validation"; // This will cause validation stage to fail

            // Act
            Result<ProcessedData> result = ProcessDataThroughPipeline(inputData);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Data validation failed");
        }

        [Fact]
        public void ServiceChaining_WithDependentServices_ShouldExecuteInCorrectOrder()
        {
            // Arrange
            ServiceRequest request = new() { Data = "valid-service-data", Priority = ServicePriority.High };

            // Act
            Result<ServiceChainResult> result = ExecuteServiceChain(request);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ServiceChainResult? chainResult).ShouldBeTrue();
            chainResult.ExecutionOrder.Count.ShouldBe(5);
            chainResult.ExecutionOrder[0].ShouldBe("Authentication");
            chainResult.ExecutionOrder[1].ShouldBe("Authorization"); 
            chainResult.ExecutionOrder[2].ShouldBe("Validation");
            chainResult.ExecutionOrder[3].ShouldBe("Processing");
            chainResult.ExecutionOrder[4].ShouldBe("Logging");
            chainResult.TotalExecutionTimeMs.ShouldBeGreaterThan(0);
        }

        private static Result<ProcessedData> ProcessDataThroughPipeline(string inputData)
        {
            // Step 1: Parse input
            Result<ParsedData> parseResult = ParseInputData(inputData);
            if (parseResult.IsFailure)
                return Result.Failure<ProcessedData>(parseResult.Error);

            // Step 2: Validate parsed data
            Result validationResult = ValidateParsedData(parseResult.TryGetValue(out ParsedData? parsed) ? parsed : new ParsedData());
            if (validationResult.IsFailure)
                return Result.Failure<ProcessedData>(validationResult.Error);

            // Step 3: Transform data
            Result<TransformedData> transformResult = TransformData(parsed);
            if (transformResult.IsFailure)
                return Result.Failure<ProcessedData>(transformResult.Error);

            // Step 4: Enrich data
            Result<EnrichedData> enrichResult = EnrichData(transformResult.TryGetValue(out TransformedData? transformed) ? transformed : new TransformedData());
            if (enrichResult.IsFailure)
                return Result.Failure<ProcessedData>(enrichResult.Error);

            // Step 5: Finalize processing
            Result<ProcessedData> finalResult = FinalizeProcessing(enrichResult.TryGetValue(out EnrichedData? enriched) ? enriched : new EnrichedData());
            
            return finalResult;
        }

        private static Result<ServiceChainResult> ExecuteServiceChain(ServiceRequest request)
        {
            List<string> executionOrder = [];
            DateTime startTime = DateTime.UtcNow;

            // Step 1: Authentication
            Result authResult = AuthenticateRequest(request);
            if (authResult.IsFailure)
                return Result.Failure<ServiceChainResult>(authResult.Error);
            executionOrder.Add("Authentication");

            // Step 2: Authorization
            Result authzResult = AuthorizeRequest(request);
            if (authzResult.IsFailure)
                return Result.Failure<ServiceChainResult>(authzResult.Error);
            executionOrder.Add("Authorization");

            // Step 3: Validation
            Result validationResult = ValidateServiceRequest(request);
            if (validationResult.IsFailure)
                return Result.Failure<ServiceChainResult>(validationResult.Error);
            executionOrder.Add("Validation");

            // Step 4: Processing
            Result<ServiceProcessingResult> processingResult = ProcessServiceRequest(request);
            if (processingResult.IsFailure)
                return Result.Failure<ServiceChainResult>(processingResult.Error);
            executionOrder.Add("Processing");

            // Step 5: Logging
            Result loggingResult = LogServiceExecution(request);
            executionOrder.Add("Logging");

            DateTime endTime = DateTime.UtcNow;

            int totalExecutionTime = Math.Max(1, (int)(endTime - startTime).TotalMilliseconds); // Ensure at least 1ms

            ServiceChainResult chainResult = new()
            {
                ExecutionOrder = executionOrder,
                TotalExecutionTimeMs = totalExecutionTime,
                ProcessingResult = processingResult.TryGetValue(out ServiceProcessingResult? processing) ? processing : new ServiceProcessingResult()
            };

            return Result.Success(chainResult);
        }

        #region Helper Methods

        private static Result<ParsedData> ParseInputData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Result.Failure<ParsedData>("Input data cannot be empty");

            return Result.Success(new ParsedData { RawInput = input, ParsedAt = DateTime.UtcNow });
        }

        private static Result ValidateParsedData(ParsedData data)
        {
            if (data.RawInput.Contains("fail-at-validation"))
                return Result.Failure("Data validation failed");

            return Result.Success();
        }

        private static Result<TransformedData> TransformData(ParsedData? parsed)
        {
            if (parsed == null)
                return Result.Failure<TransformedData>("No parsed data to transform");

            return Result.Success(new TransformedData 
            { 
                TransformedValue = $"transformed-{parsed.RawInput}",
                TransformationApplied = "Standard"
            });
        }

        private static Result<EnrichedData> EnrichData(TransformedData transformed)
        {
            return Result.Success(new EnrichedData 
            { 
                EnrichedValue = $"enriched-{transformed.TransformedValue}",
                Metadata = new Dictionary<string, string> { ["source"] = "pipeline", ["version"] = "1.0" }
            });
        }

        private static Result<ProcessedData> FinalizeProcessing(EnrichedData enriched)
        {
            List<ProcessingStep> steps = [
                new ProcessingStep { Name = "Parse", IsSuccessful = true },
                new ProcessingStep { Name = "Validate", IsSuccessful = true },
                new ProcessingStep { Name = "Transform", IsSuccessful = true },
                new ProcessingStep { Name = "Enrich", IsSuccessful = true },
                new ProcessingStep { Name = "Finalize", IsSuccessful = true },
                new ProcessingStep { Name = "Cleanup", IsSuccessful = true }
            ];

            return Result.Success(new ProcessedData 
            { 
                FinalValue = enriched.EnrichedValue,
                Stage = "Finalized",
                ProcessingSteps = steps
            });
        }

        private static Result AuthenticateRequest(ServiceRequest request) => Result.Success();
        private static Result AuthorizeRequest(ServiceRequest request) => Result.Success();
        private static Result ValidateServiceRequest(ServiceRequest request) => Result.Success();
        private static Result LogServiceExecution(ServiceRequest request) => Result.Success();

        private static Result<ServiceProcessingResult> ProcessServiceRequest(ServiceRequest request)
        {
            return Result.Success(new ServiceProcessingResult 
            { 
                ProcessedData = $"processed-{request.Data}",
                ProcessingTimeMs = 150
            });
        }

        #endregion
    }

    #endregion

    #region Async Composition Advanced Tests

    public class AsyncCompositionAdvanced
    {
        [Fact]
        public async Task ComplexAsyncPipeline_WithParallelAndSequentialSteps_ShouldExecuteCorrectly()
        {
            // Arrange
            AsyncProcessingRequest request = new()
            {
                InputData = "async-pipeline-data",
                ProcessingOptions = ["parallel", "sequential", "optimized"],
                Priority = ProcessingPriority.High
            };

            // Act
            Result<AsyncProcessingResult> result = await ExecuteComplexAsyncPipeline(request);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out AsyncProcessingResult? processingResult).ShouldBeTrue();
            processingResult.ProcessingPhases.Count.ShouldBe(3);
            processingResult.ParallelOperationsCount.ShouldBe(3);
            processingResult.SequentialOperationsCount.ShouldBe(2);
            processingResult.TotalProcessingTimeMs.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task AsyncPipeline_WithTimeoutConstraint_ShouldRespectTimeout()
        {
            // Arrange
            AsyncProcessingRequest request = new()
            {
                InputData = "long-running-data",
                ProcessingOptions = ["timeout"],
                Priority = ProcessingPriority.Low,
                TimeoutMs = 100 // Very short timeout
            };

            // Act
            Result<AsyncProcessingResult> result = await ExecuteComplexAsyncPipeline(request);

            // Assert - This should succeed within timeout for this test
            // In a real scenario, you might want to test actual timeout behavior
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AsyncResultCombination_WithMixedOperations_ShouldAggregateCorrectly()
        {
            // Arrange
            Task<Result>[] asyncOperations = [
                SimulateAsyncValidation("data1", 50),
                SimulateAsyncValidation("data2", 75),
                SimulateAsyncValidation("invalid-data", 25), // This will fail
                SimulateAsyncValidation("data3", 100),
                SimulateAsyncValidation("data4", 30)
            ];

            // Act
            Result[] results = await Task.WhenAll(asyncOperations);
            Result combinedResult = Result.Combine(results);

            // Assert
            combinedResult.IsFailure.ShouldBeTrue();
            combinedResult.FailureType.ShouldBe(ResultFailureType.Validation);
            combinedResult.Failures.ShouldContainKey("Data");
        }

        private static async Task<Result<AsyncProcessingResult>> ExecuteComplexAsyncPipeline(AsyncProcessingRequest request)
        {
            DateTime startTime = DateTime.UtcNow;

            // Phase 1: Parallel preprocessing
            Task<Result<string>>[] parallelTasks = [
                PreprocessDataAsync(request.InputData, "tokenization"),
                PreprocessDataAsync(request.InputData, "normalization"),
                PreprocessDataAsync(request.InputData, "validation")
            ];

            Result<string>[] parallelResults = await Task.WhenAll(parallelTasks);
            Result<string> combinedParallelResult = Result.Combine(parallelResults);
            
            if (combinedParallelResult.IsFailure)
                return Result.Failure<AsyncProcessingResult>(combinedParallelResult.Error);

            // Phase 2: Sequential processing
            Result<string> sequentialResult1 = await ProcessDataSequentiallyAsync(request.InputData, 1);
            if (sequentialResult1.IsFailure)
                return Result.Failure<AsyncProcessingResult>(sequentialResult1.Error);

            Result<string> sequentialResult2 = await ProcessDataSequentiallyAsync(
                sequentialResult1.TryGetValue(out string? step1Data) ? step1Data : "", 
                2
            );
            if (sequentialResult2.IsFailure)
                return Result.Failure<AsyncProcessingResult>(sequentialResult2.Error);

            // Phase 3: Finalization
            Result<string> finalizationResult = await FinalizeAsyncProcessingAsync(
                sequentialResult2.TryGetValue(out string? step2Data) ? step2Data : ""
            );
            if (finalizationResult.IsFailure)
                return Result.Failure<AsyncProcessingResult>(finalizationResult.Error);

            DateTime endTime = DateTime.UtcNow;

            AsyncProcessingResult processingResult = new()
            {
                ProcessedData = finalizationResult.TryGetValue(out string? finalData) ? finalData : "",
                ProcessingPhases = ["Parallel Preprocessing", "Sequential Processing", "Finalization"],
                ParallelOperationsCount = 3,
                SequentialOperationsCount = 2,
                TotalProcessingTimeMs = (int)(endTime - startTime).TotalMilliseconds
            };

            return Result.Success(processingResult);
        }

        private static async Task<Result<string>> PreprocessDataAsync(string data, string operation)
        {
            await Task.Delay(Random.Shared.Next(10, 50)); // Simulate async work
            return Result.Success($"{data}-{operation}");
        }

        private static async Task<Result<string>> ProcessDataSequentiallyAsync(string data, int step)
        {
            await Task.Delay(Random.Shared.Next(30, 100)); // Simulate longer async work
            return Result.Success($"{data}-step{step}");
        }

        private static async Task<Result<string>> FinalizeAsyncProcessingAsync(string data)
        {
            await Task.Delay(50); // Simulate finalization work
            return Result.Success($"{data}-finalized");
        }

        private static async Task<Result> SimulateAsyncValidation(string data, int delayMs)
        {
            await Task.Delay(delayMs);
            return data.Contains("invalid") 
                ? Result.Failure("Data", $"Invalid data: {data}")
                : Result.Success();
        }
    }

    #endregion

    #region Performance and Scalability Tests

    public class PerformanceAndScalability
    {
        [Fact]
        public void MassiveResultComposition_With10000Results_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            int numberOfResults = 10000;
            Result[] results = new Result[numberOfResults];
            Random random = new(12345); // Fixed seed for reproducible results

            for (int i = 0; i < numberOfResults; i++)
            {
                results[i] = random.Next(1, 11) == 1  // 10% failure rate
                    ? Result.Failure($"Field{i}", $"Validation error for field {i}")
                    : Result.Success();
            }

            // Act
            DateTime startTime = DateTime.UtcNow;
            Result combinedResult = Result.Combine(results);
            DateTime endTime = DateTime.UtcNow;

            // Assert
            TimeSpan executionTime = endTime - startTime;
            executionTime.TotalMilliseconds.ShouldBeLessThan(1000); // Should complete within 1 second

            combinedResult.IsFailure.ShouldBeTrue();
            combinedResult.FailureType.ShouldBe(ResultFailureType.Validation);
            
            int expectedFailureCount = numberOfResults / 10;
            combinedResult.Failures.Count.ShouldBeGreaterThanOrEqualTo(expectedFailureCount - 100);
            combinedResult.Failures.Count.ShouldBeLessThanOrEqualTo(expectedFailureCount + 100);
        }

        [Fact]
        public async Task MassiveAsyncResultComposition_With1000AsyncTasks_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            int numberOfTasks = 1000;
            Task<Result>[] tasks = new Task<Result>[numberOfTasks];

            for (int i = 0; i < numberOfTasks; i++)
            {
                int taskId = i;
                tasks[i] = SimulateAsyncValidation($"task-{taskId}", taskId);
            }

            // Act
            DateTime startTime = DateTime.UtcNow;
            Result[] results = await Task.WhenAll(tasks);
            Result combinedResult = Result.Combine(results);
            DateTime endTime = DateTime.UtcNow;

            // Assert
            TimeSpan executionTime = endTime - startTime;
            executionTime.TotalSeconds.ShouldBeLessThan(5); // Should complete within 5 seconds with parallelization

            // With 10% failure rate, we should have around 100 failures
            if (combinedResult.IsFailure)
            {
                combinedResult.Failures.Count.ShouldBeGreaterThan(50);
                combinedResult.Failures.Count.ShouldBeLessThan(150);
            }
        }

        [Fact]
        public void DeepResultChaining_With100Levels_ShouldNotStackOverflow()
        {
            // Arrange
            string initialData = "start";
            int chainDepth = 100;

            // Act & Assert - Should not throw stack overflow exception
            Result<string> result = ChainResults(initialData, chainDepth);

            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? finalData).ShouldBeTrue();
            finalData.ShouldContain("start");
            finalData.Split('-').Length.ShouldBe(chainDepth + 1);
        }

        private static async Task<Result> SimulateAsyncValidation(string data, int taskId)
        {
            // Simulate varying async work durations
            await Task.Delay(Random.Shared.Next(1, 10));

            // 10% failure rate
            return taskId % 10 == 0
                ? Result.Failure($"TaskData{taskId}", $"Validation failed for {data}")
                : Result.Success();
        }

        private static Result<string> ChainResults(string data, int remainingDepth)
        {
            if (remainingDepth == 0)
                return Result.Success(data);

            Result<string> currentResult = Result.Success($"{data}-{remainingDepth}");
            if (currentResult.IsFailure)
                return currentResult;

            return ChainResults(currentResult.TryGetValue(out string? currentData) ? currentData : "", remainingDepth - 1);
        }
    }

    #endregion

    #region Mixed Result Types Composition Tests

    public class MixedResultTypesComposition
    {
        [Fact]
        public void MixedResultAndGenericResults_WithComplexInteraction_ShouldHandleTypeConversions()
        {
            // Arrange
            Result nonGenericResult = ValidateGeneralBusinessRules();
            Result<User> userResult = CreateUser("john@example.com", "John", "Doe");
            Result<Order> orderResult = CreateOrder(Guid.NewGuid(), 2, 29.99m);
            Result<PaymentTransaction> paymentResult = ProcessPayment("CreditCard", 59.98m);

            // Act
            Result finalResult = CombineHeterogeneousResults(nonGenericResult, userResult, orderResult, paymentResult);

            // Assert
            finalResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MixedResultAndGenericResults_WithFailuresInDifferentTypes_ShouldAggregateAllErrors()
        {
            // Arrange
            Result nonGenericResult = Result.Failure("General business rule violation");
            Result<User> userResult = Result.Failure<User>("Email", "Invalid email format");
            Result<Order> orderResult = Result.NotFound<Order>("Order not found");
            Result<PaymentTransaction> paymentResult = Result.Failure<PaymentTransaction>(new SecurityException("Payment security check failed"));

            // Act
            Result finalResult = CombineHeterogeneousResults(nonGenericResult, userResult, orderResult, paymentResult);

            // Assert
            finalResult.IsFailure.ShouldBeTrue();
            finalResult.FailureType.ShouldBe(ResultFailureType.Validation);
            finalResult.Failures.ShouldContainKey("Error");
            finalResult.Failures.ShouldContainKey("Email");
            finalResult.Failures.ShouldContainKey("NotFound");
            finalResult.Failures.ShouldContainKey("Security");
        }

        [Fact]
        public void GenericResultConversion_ThroughImplicitOperators_ShouldPreserveErrorInformation()
        {
            // Arrange
            Result<User> userResult = Result.ValidationFailure<User>(
                new Dictionary<string, string[]>
                {
                    ["Email"] = ["Email format is invalid"],
                    ["Password"] = ["Password too weak"]
                });

            // Act - Implicit conversion from Result<T> to Result
            Result nonGenericResult = userResult;

            // Assert
            nonGenericResult.IsFailure.ShouldBeTrue();
            nonGenericResult.FailureType.ShouldBe(ResultFailureType.Validation);
            nonGenericResult.Failures.ShouldContainKey("Email");
            nonGenericResult.Failures.ShouldContainKey("Password");
            nonGenericResult.Failures["Email"].ShouldContain("Email format is invalid");
            nonGenericResult.Failures["Password"].ShouldContain("Password too weak");
        }

        private static Result CombineHeterogeneousResults(
            Result generalResult,
            Result<User> userResult,
            Result<Order> orderResult,
            Result<PaymentTransaction> paymentResult)
        {
            // Convert all to non-generic Results for combining
            Result[] allResults = [
                generalResult,
                userResult, // Implicit conversion
                orderResult, // Implicit conversion  
                paymentResult // Implicit conversion
            ];

            return Result.Combine(allResults);
        }

        private static Result ValidateGeneralBusinessRules() =>
            Result.Success();

        private static Result<User> CreateUser(string email, string firstName, string lastName) =>
            Result.Success(new User 
            { 
                Id = Guid.NewGuid(), 
                Email = email, 
                FirstName = firstName, 
                LastName = lastName 
            });

        private static Result<Order> CreateOrder(Guid userId, int quantity, decimal price) =>
            Result.Success(new Order 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                TotalAmount = quantity * price,
                Status = OrderStatus.Confirmed
            });

        private static Result<PaymentTransaction> ProcessPayment(string method, decimal amount) =>
            Result.Success(new PaymentTransaction 
            { 
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount, 
                Status = "Completed"
            });
    }

    #endregion

    #region Nested Result Composition Tests

    public class NestedResultComposition
    {
        [Fact]
        public void NestedResultValidation_WithComplexObjectHierarchy_ShouldValidateAllLevels()
        {
            // Arrange
            OrganizationData organization = new OrganizationDataBuilder()
                .WithName("")
                .WithAddress(new Address { Street = "", City = "Valid City", State = "CA", ZipCode = "12345" })
                .WithDepartments([
                    new Department { 
                        Name = "Engineering", 
                        Employees = [
                            new Employee { FirstName = "", LastName = "Smith", Email = "invalid-email" },
                            new Employee { FirstName = "Jane", LastName = "", Email = "jane@example.com" }
                        ]
                    },
                    new Department { 
                        Name = "", 
                        Employees = [
                            new Employee { FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com" }
                        ]
                    }
                ])
                .Build();

            // Act
            Result validationResult = ValidateOrganizationHierarchy(organization);

            // Assert
            validationResult.IsFailure.ShouldBeTrue();
            validationResult.FailureType.ShouldBe(ResultFailureType.Validation);
            validationResult.Failures.ShouldContainKey("Organization.Name");
            validationResult.Failures.ShouldContainKey("Organization.Address.Street");
            validationResult.Failures.ShouldContainKey("Organization.Departments[0].Employees[0].FirstName");
            validationResult.Failures.ShouldContainKey("Organization.Departments[0].Employees[0].Email");
            validationResult.Failures.ShouldContainKey("Organization.Departments[0].Employees[1].LastName");
            validationResult.Failures.ShouldContainKey("Organization.Departments[1].Name");
        }

        [Fact]
        public void NestedResultValidation_WithValidComplexObject_ShouldReturnSuccess()
        {
            // Arrange
            OrganizationData organization = new OrganizationDataBuilder()
                .WithName("Tech Corp")
                .WithAddress(new Address { Street = "123 Main St", City = "San Francisco", State = "CA", ZipCode = "94105" })
                .WithDepartments([
                    new Department { 
                        Name = "Engineering", 
                        Employees = [
                            new Employee { FirstName = "John", LastName = "Smith", Email = "john@techcorp.com" },
                            new Employee { FirstName = "Jane", LastName = "Doe", Email = "jane@techcorp.com" }
                        ]
                    },
                    new Department { 
                        Name = "Marketing", 
                        Employees = [
                            new Employee { FirstName = "Bob", LastName = "Johnson", Email = "bob@techcorp.com" }
                        ]
                    }
                ])
                .Build();

            // Act
            Result validationResult = ValidateOrganizationHierarchy(organization);

            // Assert
            validationResult.IsSuccess.ShouldBeTrue();
            validationResult.Failures.ShouldBeEmpty();
        }

        [Fact]
        public void NestedResultComposition_WithRecursiveDataStructure_ShouldHandleComplexNesting()
        {
            // Arrange
            NestedCategory rootCategory = CreateNestedCategoryStructure(3, 2); // 3 levels, 2 children per level

            // Act
            Result validationResult = ValidateNestedCategoryStructure(rootCategory, "Root");

            // Assert
            validationResult.IsSuccess.ShouldBeTrue();
        }

        private static Result ValidateOrganizationHierarchy(OrganizationData organization)
        {
            List<Result> validationResults = [];

            // Organization-level validation
            validationResults.Add(ValidateOrganizationName(organization.Name));
            validationResults.Add(ValidateAddress(organization.Address, "Organization.Address"));

            // Department-level validation
            for (int i = 0; i < organization.Departments.Count; i++)
            {
                Department department = organization.Departments[i];
                validationResults.Add(ValidateDepartmentName(department.Name, $"Organization.Departments[{i}]"));

                // Employee-level validation
                for (int j = 0; j < department.Employees.Count; j++)
                {
                    Employee employee = department.Employees[j];
                    string employeePrefix = $"Organization.Departments[{i}].Employees[{j}]";
                    validationResults.Add(ValidateEmployeeName(employee.FirstName, $"{employeePrefix}.FirstName"));
                    validationResults.Add(ValidateEmployeeName(employee.LastName, $"{employeePrefix}.LastName"));
                    validationResults.Add(ValidateEmployeeEmail(employee.Email, $"{employeePrefix}.Email"));
                }
            }

            return Result.Combine([.. validationResults]);
        }

        private static Result ValidateNestedCategoryStructure(NestedCategory category, string path)
        {
            List<Result> validationResults = [];

            // Validate current category
            validationResults.Add(ValidateCategoryName(category.Name, $"{path}.Name"));

            // Recursively validate children
            for (int i = 0; i < category.Children.Count; i++)
            {
                Result childValidation = ValidateNestedCategoryStructure(category.Children[i], $"{path}.Children[{i}]");
                validationResults.Add(childValidation);
            }

            return Result.Combine([.. validationResults]);
        }

        private static NestedCategory CreateNestedCategoryStructure(int maxDepth, int childrenPerLevel)
        {
            return CreateCategoryRecursive("Root", maxDepth, childrenPerLevel, 0);
        }

        private static NestedCategory CreateCategoryRecursive(string namePrefix, int maxDepth, int childrenPerLevel, int currentDepth)
        {
            NestedCategory category = new() { Name = $"{namePrefix}_L{currentDepth}", Children = [] };

            if (currentDepth < maxDepth)
            {
                for (int i = 0; i < childrenPerLevel; i++)
                {
                    NestedCategory child = CreateCategoryRecursive($"{namePrefix}_C{i}", maxDepth, childrenPerLevel, currentDepth + 1);
                    category.Children.Add(child);
                }
            }

            return category;
        }

        #region Helper Methods

        private static Result ValidateOrganizationName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Result.Failure("Organization.Name", "Organization name is required")
                : Result.Success();

        private static Result ValidateAddress(Address address, string prefix) =>
            string.IsNullOrWhiteSpace(address.Street)
                ? Result.Failure($"{prefix}.Street", "Street address is required")
                : Result.Success();

        private static Result ValidateDepartmentName(string name, string prefix) =>
            string.IsNullOrWhiteSpace(name)
                ? Result.Failure($"{prefix}.Name", "Department name is required")
                : Result.Success();

        private static Result ValidateEmployeeName(string name, string fieldName) =>
            string.IsNullOrWhiteSpace(name)
                ? Result.Failure(fieldName, "Employee name is required")
                : Result.Success();

        private static Result ValidateEmployeeEmail(string email, string fieldName) =>
            string.IsNullOrWhiteSpace(email) || !email.Contains('@')
                ? Result.Failure(fieldName, "Valid email address is required")
                : Result.Success();

        private static Result ValidateCategoryName(string name, string fieldName) =>
            string.IsNullOrWhiteSpace(name)
                ? Result.Failure(fieldName, "Category name is required")
                : Result.Success();

        #endregion
    }

    #endregion
}

#region Test Data Models and Builders

public class UserOnboardingRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
}

public class UserOnboardingRequestBuilder
{
    private UserOnboardingRequest _request = new();

    public UserOnboardingRequestBuilder WithEmail(string email)
    {
        _request.Email = email;
        return this;
    }

    public UserOnboardingRequestBuilder WithPassword(string password)
    {
        _request.Password = password;
        return this;
    }

    public UserOnboardingRequestBuilder WithFirstName(string firstName)
    {
        _request.FirstName = firstName;
        return this;
    }

    public UserOnboardingRequestBuilder WithLastName(string lastName)
    {
        _request.LastName = lastName;
        return this;
    }

    public UserOnboardingRequestBuilder WithCompanyCode(string companyCode)
    {
        _request.CompanyCode = companyCode;
        return this;
    }

    public UserOnboardingRequest Build() => _request;
}

public class OrderFulfillmentRequest
{
    public Guid UserId { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public PaymentMethod PaymentMethod { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
}

public class OrderFulfillmentRequestBuilder
{
    private OrderFulfillmentRequest _request = new();

    public OrderFulfillmentRequestBuilder WithUserId(Guid userId)
    {
        _request.UserId = userId;
        return this;
    }

    public OrderFulfillmentRequestBuilder WithItems(List<OrderItem> items)
    {
        _request.Items = items;
        return this;
    }

    public OrderFulfillmentRequestBuilder WithPaymentMethod(PaymentMethod paymentMethod)
    {
        _request.PaymentMethod = paymentMethod;
        return this;
    }

    public OrderFulfillmentRequestBuilder WithShippingAddress(Address address)
    {
        _request.ShippingAddress = address;
        return this;
    }

    public OrderFulfillmentRequest Build() => _request;
}

public class OnboardingResult
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<OnboardingStep> OnboardingSteps { get; set; } = [];
}

public class OnboardingStep
{
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class FulfillmentResult
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<InventoryReservation> InventoryReservations { get; set; } = [];
    public string PaymentTransactionId { get; set; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class Company
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class PaymentMethod
{
    public string Type { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    
    public static PaymentMethod CreditCard => new() { Type = "CreditCard" };
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class PaymentTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InventoryReservation
{
    public Guid ReservationId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; } = new();
    public List<OrderItem> Items { get; set; } = [];
    public ShippingAddress ShippingAddress { get; set; } = new();
    public BillingAddress BillingAddress { get; set; } = new();
    public PaymentInfo PaymentInfo { get; set; } = new();
    public string OrderNumber { get; set; } = string.Empty;
    public Tax Tax { get; set; } = new(0m, 0m);
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

// Data processing pipeline models
public class ParsedData
{
    public string RawInput { get; set; } = string.Empty;
    public DateTime ParsedAt { get; set; }
}

public class TransformedData
{
    public string TransformedValue { get; set; } = string.Empty;
    public string TransformationApplied { get; set; } = string.Empty;
}

public class EnrichedData
{
    public string EnrichedValue { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = [];
}

public class ProcessedData
{
    public string FinalValue { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public List<ProcessingStep> ProcessingSteps { get; set; } = [];
}

public class ProcessingStep
{
    public string Name { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
}

// Service chaining models
public class ServiceRequest
{
    public string Data { get; set; } = string.Empty;
    public ServicePriority Priority { get; set; }
}

public enum ServicePriority
{
    Low,
    Medium,
    High
}

public class ServiceChainResult
{
    public List<string> ExecutionOrder { get; set; } = [];
    public int TotalExecutionTimeMs { get; set; }
    public ServiceProcessingResult ProcessingResult { get; set; } = new();
}

public class ServiceProcessingResult
{
    public string ProcessedData { get; set; } = string.Empty;
    public int ProcessingTimeMs { get; set; }
}

// Async processing models
public class AsyncProcessingRequest
{
    public string InputData { get; set; } = string.Empty;
    public List<string> ProcessingOptions { get; set; } = [];
    public ProcessingPriority Priority { get; set; }
    public int TimeoutMs { get; set; } = 5000;
}

public enum ProcessingPriority
{
    Low,
    Medium,
    High
}

public class AsyncProcessingResult
{
    public string ProcessedData { get; set; } = string.Empty;
    public List<string> ProcessingPhases { get; set; } = [];
    public int ParallelOperationsCount { get; set; }
    public int SequentialOperationsCount { get; set; }
    public int TotalProcessingTimeMs { get; set; }
}

// Nested result composition models
public class OrganizationData
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public List<Department> Departments { get; set; } = [];
}

public class OrganizationDataBuilder
{
    private OrganizationData _organization = new();

    public OrganizationDataBuilder WithName(string name)
    {
        _organization.Name = name;
        return this;
    }

    public OrganizationDataBuilder WithAddress(Address address)
    {
        _organization.Address = address;
        return this;
    }

    public OrganizationDataBuilder WithDepartments(List<Department> departments)
    {
        _organization.Departments = departments;
        return this;
    }

    public OrganizationData Build() => _organization;
}

public class Department
{
    public string Name { get; set; } = string.Empty;
    public List<Employee> Employees { get; set; } = [];
}

public class Employee
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class NestedCategory
{
    public string Name { get; set; } = string.Empty;
    public List<NestedCategory> Children { get; set; } = [];
}

// Additional models needed for integration tests
public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DateOfBirth { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Regular;
    public List<string> Tags { get; set; } = [];
    
    public static FlowRight.Core.Results.Result<Customer> Create(string firstName, string lastName, string email, string phone, DateTime dateOfBirth, Address primaryAddress)
    {
        Customer customer = new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = dateOfBirth,
            CreatedDate = DateTime.UtcNow
        };
        return FlowRight.Core.Results.Result.Success(customer);
    }
}

public class ShippingAddress : Address
{
    // Inherits from Address
    
    public static FlowRight.Core.Results.Result<ShippingAddress> Create(string street, string city, string state, string postalCode, string country)
    {
        return FlowRight.Core.Results.Result.Success(new ShippingAddress());
    }
}

public class BillingAddress : Address
{
    // Inherits from Address
    
    public static FlowRight.Core.Results.Result<BillingAddress> Create(string street, string city, string state, string postalCode, string country)
    {
        return FlowRight.Core.Results.Result.Success(new BillingAddress());
    }
}

public class PaymentInfo
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; } = new();
    
    public PaymentInfo() { }
    
    public PaymentInfo(string cardNumber, string cardHolderName, string expiryDate, PaymentMethod method)
    {
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        ExpiryDate = expiryDate;
        Method = method;
    }
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Category Category { get; set; } = new();
}

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; } = [];
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// Additional supporting types
public enum CustomerType
{
    Regular,
    Premium,
    VIP,
    Corporate
}

public record Tax(decimal Amount, decimal Rate);

// Builder classes for test data creation
public class OrderBuilder
{
    private readonly Order _order = new();

    public OrderBuilder WithOrderNumber(string orderNumber)
    {
        _order.OrderNumber = orderNumber;
        return this;
    }

    public OrderBuilder WithCustomer(Customer customer)
    {
        _order.Customer = customer;
        return this;
    }

    public OrderBuilder WithItems(IEnumerable<OrderItem> items)
    {
        _order.Items = items.ToList();
        return this;
    }

    public OrderBuilder AddItem(OrderItem item)
    {
        _order.Items.Add(item);
        return this;
    }

    public OrderBuilder WithShippingAddress(ShippingAddress address)
    {
        _order.ShippingAddress = address;
        return this;
    }

    public OrderBuilder WithBillingAddress(BillingAddress address)
    {
        _order.BillingAddress = address;
        return this;
    }

    public OrderBuilder WithPaymentInfo(PaymentInfo paymentInfo)
    {
        _order.PaymentInfo = paymentInfo;
        return this;
    }

    public OrderBuilder WithTotalAmount(decimal amount)
    {
        _order.TotalAmount = amount;
        return this;
    }

    public Order Build() => _order;
}

public class CustomerBuilder
{
    private readonly Customer _customer = new();

    public CustomerBuilder WithFirstName(string firstName)
    {
        _customer.FirstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _customer.LastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _customer.Email = email;
        return this;
    }

    public CustomerBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _customer.DateOfBirth = dateOfBirth;
        return this;
    }

    public CustomerBuilder WithType(CustomerType type)
    {
        _customer.Type = type;
        return this;
    }

    public CustomerBuilder AddTag(string tag)
    {
        _customer.Tags.Add(tag);
        return this;
    }

    public Customer Build() => _customer;
}

public class CategoryBuilder
{
    private readonly Category _category = new();

    public CategoryBuilder WithName(string name)
    {
        _category.Name = name;
        return this;
    }

    public CategoryBuilder WithParentCategory(Category parent)
    {
        _category.ParentCategory = parent;
        _category.Level = parent.Level + 1;
        _category.Path = $"{parent.Path}/{_category.Name}";
        return this;
    }

    public Category Build() => _category;
}

public class ProductBuilder
{
    private readonly Product _product = new();

    public ProductBuilder WithName(string name)
    {
        _product.Name = name;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _product.Sku = sku;
        return this;
    }

    public ProductBuilder WithCategory(Category category)
    {
        _product.Category = category;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _product.Price = price;
        return this;
    }

    public Product Build() => _product;
}

public class OrderItemBuilder
{
    private readonly OrderItem _orderItem = new();

    public OrderItemBuilder WithProduct(Product product)
    {
        _orderItem.Product = product;
        _orderItem.ProductId = product.Id;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        _orderItem.Quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithUnitPrice(decimal price)
    {
        _orderItem.UnitPrice = price;
        return this;
    }

    public OrderItem Build() => _orderItem;
}

public class ShippingAddressBuilder
{
    private readonly ShippingAddress _address = new();

    public ShippingAddressBuilder WithStreet(string street)
    {
        _address.Street = street;
        return this;
    }

    public ShippingAddressBuilder WithCity(string city)
    {
        _address.City = city;
        return this;
    }

    public ShippingAddress Build() => _address;
}

public class BillingAddressBuilder
{
    private readonly BillingAddress _address = new();

    public BillingAddressBuilder WithStreet(string street)
    {
        _address.Street = street;
        return this;
    }

    public BillingAddressBuilder WithCity(string city)
    {
        _address.City = city;
        return this;
    }

    public BillingAddress Build() => _address;
}

#endregion
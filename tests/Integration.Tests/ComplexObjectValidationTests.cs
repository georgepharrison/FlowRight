using System.Linq;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using FlowRight.Validation.Tests.TestModels;
using Shouldly;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Comprehensive integration tests for ValidationBuilder with complex objects including nested objects,
/// collections, deep hierarchies, and real-world scenarios. These tests follow TDD principles and validate
/// the entire validation pipeline working together.
/// 
/// Test Coverage:
/// - Nested object validation scenarios
/// - Collection validation within complex objects
/// - Complex validation scenarios with multiple validation rules
/// - Edge cases with deep object hierarchies
/// - Performance validation with large complex objects
/// - Circular reference handling
/// 
/// These tests are designed to fail initially (TDD Red phase) and serve as executable specifications
/// for the ValidationBuilder's behavior with complex object structures.
/// </summary>
public class ComplexObjectValidationTests
{
    #region Nested Object Validation Tests

    /// <summary>
    /// Tests for validating objects that contain nested object properties.
    /// </summary>
    public class NestedObjectValidation
    {
        [Fact]
        public void ValidationBuilder_WithValidNestedCustomerObject_ShouldCreateSuccessfulOrder()
        {
            // Arrange
            Customer validCustomer = new CustomerBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithEmail("john.doe@example.com")
                .Build();

            ShippingAddress validShippingAddress = new ShippingAddressBuilder()
                .WithStreet("123 Main St")
                .Build();

            BillingAddress validBillingAddress = new BillingAddressBuilder()
                .WithStreet("123 Billing St")
                .Build();

            PaymentInfo validPayment = new("1234-5678-9012-3456", "John Doe", "12/25", PaymentMethod.CreditCard);

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Customer, validCustomer)
                .NotNull()
                .Must(c => !string.IsNullOrEmpty(c.Email), "Customer must have email");

            builder.RuleFor(o => o.ShippingAddress, validShippingAddress)
                .NotNull()
                .Must(a => !string.IsNullOrEmpty(a.Street), "Shipping address must have street");

            builder.RuleFor(o => o.BillingAddress, validBillingAddress)
                .NotNull()
                .Must(a => !string.IsNullOrEmpty(a.Street), "Billing address must have street");

            builder.RuleFor(o => o.PaymentInfo, validPayment)
                .NotNull()
                .Must(p => !string.IsNullOrEmpty(p.CardNumber), "Payment info must have card number");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(validCustomer)
                .WithShippingAddress(validShippingAddress)
                .WithBillingAddress(validBillingAddress)
                .Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidationBuilder_WithInvalidNestedCustomerObject_ShouldReturnValidationFailure()
        {
            // Arrange
            Customer invalidCustomer = new CustomerBuilder()
                .WithFirstName("")  // Invalid: empty first name
                .WithLastName("Doe")
                .WithEmail("invalid-email")  // Invalid: malformed email
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Customer, invalidCustomer)
                .NotNull()
                .Must(c => !string.IsNullOrEmpty(c.FirstName), "Customer first name is required")
                .Must(c => c.Email.Contains('@'), "Customer email must be valid");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(invalidCustomer)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
        }

        [Fact]
        public void ValidationBuilder_WithNestedResultComposition_ShouldAggregateAllErrors()
        {
            // Arrange
            Result<Customer> invalidCustomerResult = Customer.Create("", "", "bad-email", "", DateTime.Today, null!);
            Result<ShippingAddress> invalidShippingResult = ShippingAddress.Create("", "", "", "", "");

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Customer, invalidCustomerResult, out Customer? customer);
            builder.RuleFor(o => o.ShippingAddress, invalidShippingResult, out ShippingAddress? shipping);

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(customer!)
                .WithShippingAddress(shipping!)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            Dictionary<string, string[]> errors = result.Match(
                onSuccess: _ => new Dictionary<string, string[]>(),
                onValidationException: validationErrors => new Dictionary<string, string[]>(validationErrors),
                onError: _ => new Dictionary<string, string[]>(),
                onSecurityException: _ => new Dictionary<string, string[]>(),
                onOperationCanceledException: _ => new Dictionary<string, string[]>()
            );

            // Should contain errors from both nested Result validations
            errors.ShouldNotBeEmpty();
            errors.Keys.ShouldContain(key => key.StartsWith("Customer"));
            errors.Keys.ShouldContain(key => key.StartsWith("ShippingAddress"));
        }

        [Fact]
        public void ValidationBuilder_WithDeepNestedObjectStructure_ShouldValidateAllLevels()
        {
            // Arrange
            Category parentCategory = new CategoryBuilder().WithName("Electronics").Build();
            Category childCategory = new CategoryBuilder()
                .WithName("Smartphones")
                .WithParentCategory(parentCategory)
                .Build();

            Product product = new ProductBuilder()
                .WithName("iPhone")
                .WithCategory(childCategory)
                .WithPrice(-100m)  // Invalid: negative price
                .Build();

            OrderItem orderItem = new OrderItemBuilder()
                .WithProduct(product)
                .WithQuantity(0)  // Invalid: zero quantity
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, new[] { orderItem })
                .NotEmpty()
                .Must(items => items.All(item => item.Quantity > 0), "All items must have positive quantity")
                .Must(items => items.All(item => item.Product.Price > 0), "All products must have positive price");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(new[] { orderItem })
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Items");
            errors["Items"].Length.ShouldBe(2);  // Two validation failures
        }
    }

    #endregion

    #region Collection Validation Tests

    /// <summary>
    /// Tests for validating collections within complex objects.
    /// </summary>
    public class CollectionValidationInComplexObjects
    {
        [Fact]
        public void ValidationBuilder_WithValidItemCollection_ShouldCreateSuccessfulOrder()
        {
            // Arrange
            Product product1 = new ProductBuilder().WithName("Product 1").WithSku("PROD-001").Build();
            Product product2 = new ProductBuilder().WithName("Product 2").WithSku("PROD-002").Build();

            OrderItem item1 = new OrderItemBuilder().WithProduct(product1).WithQuantity(2).Build();
            OrderItem item2 = new OrderItemBuilder().WithProduct(product2).WithQuantity(1).Build();

            IEnumerable<OrderItem> validItems = new[] { item1, item2 };

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, validItems)
                .NotEmpty()
                .Must(items => items.Any(), "Must have at least one item")
                .MaxCount(10)
                .Must(items => items.All(item => item.Quantity > 0), "All items must have positive quantity")
                .Must(items => items.Select(i => i.Product.Sku).Distinct().Count() == items.Count(), "All items must have unique SKUs");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(validItems)
                .Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidationBuilder_WithInvalidItemCollection_ShouldReturnValidationFailure()
        {
            // Arrange
            Product product = new ProductBuilder().WithSku("DUPLICATE-SKU").Build();
            OrderItem item1 = new OrderItemBuilder().WithProduct(product).WithQuantity(1).Build();
            OrderItem item2 = new OrderItemBuilder().WithProduct(product).WithQuantity(0).Build();  // Invalid: zero quantity

            IEnumerable<OrderItem> invalidItems = new[] { item1, item2 };

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, invalidItems)
                .NotEmpty()
                .Must(items => items.All(item => item.Quantity > 0), "All items must have positive quantity")
                .Must(items => items.Select(i => i.Product.Sku).Distinct().Count() == items.Count(), "All items must have unique SKUs");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(invalidItems)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Items");
            errors["Items"].Length.ShouldBe(2);  // Two validation failures
        }

        [Fact]
        public void ValidationBuilder_WithEmptyCollectionWhenRequired_ShouldReturnValidationFailure()
        {
            // Arrange
            IEnumerable<OrderItem> emptyItems = [];

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, emptyItems)
                .NotEmpty()
                .Must(items => items.Any(), "Must have at least one item");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(emptyItems)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Items");
        }

        [Fact]
        public void ValidationBuilder_WithTooManyItemsInCollection_ShouldReturnValidationFailure()
        {
            // Arrange
            IEnumerable<OrderItem> tooManyItems = Enumerable.Range(1, 15)
                .Select(i => new OrderItemBuilder()
                    .WithProduct(new ProductBuilder().WithSku($"ITEM-{i:D3}").Build())
                    .Build());

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, tooManyItems)
                .MaxCount(10)
                .WithMessage("Order cannot contain more than 10 items");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(tooManyItems)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Items");
            errors["Items"].ShouldContain("Order cannot contain more than 10 items");
        }

        [Fact]
        public void ValidationBuilder_WithNestedCollectionValidation_ShouldValidateEachElement()
        {
            // Arrange
            IEnumerable<string> customerTags = new[] { "VIP", "", "Premium", "Active" };  // Empty string is invalid
            Customer customer = new CustomerBuilder()
                .AddTag("VIP")
                .AddTag("")  // Invalid: empty tag
                .AddTag("Premium")
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Customer, customer)
                .NotNull()
                .Must(c => c.Tags.All(tag => !string.IsNullOrWhiteSpace(tag)), "Customer tags cannot be empty");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(customer)
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Customer");
        }
    }

    #endregion

    #region Complex Multi-Rule Validation Tests

    /// <summary>
    /// Tests for complex validation scenarios with multiple validation rules across different object types.
    /// </summary>
    public class ComplexMultiRuleValidation
    {
        [Fact]
        public void ValidationBuilder_WithMultipleComplexRules_ShouldValidateAllConditions()
        {
            // Arrange
            Customer customer = new CustomerBuilder()
                .WithType(CustomerType.VIP)
                .WithEmail("vip@example.com")
                .Build();

            Product expensiveProduct = new ProductBuilder()
                .WithPrice(1500m)
                .WithName("Expensive Item")
                .Build();

            OrderItem expensiveItem = new OrderItemBuilder()
                .WithProduct(expensiveProduct)
                .WithQuantity(2)
                .WithUnitPrice(1500m)
                .Build();

            decimal orderTotal = 3000m;

            ValidationBuilder<Order> builder = new();

            // Act - Complex business rules
            builder.RuleFor(o => o.Customer, customer)
                .NotNull()
                .Must(c => c.Type == CustomerType.VIP || orderTotal <= 1000m, 
                      "Orders over $1000 require VIP customer status");

            builder.RuleFor(o => o.Items, new[] { expensiveItem })
                .NotEmpty()
                .Must(items => items.All(i => i.UnitPrice <= 2000m), "No item can exceed $2000")
                .Must(items => items.Sum(i => i.TotalPrice) >= 100m, "Order minimum is $100");

            builder.RuleFor(o => o.TotalAmount, orderTotal)
                .GreaterThan(0)
                .Must(total => customer.Type == CustomerType.VIP || total <= 1000m, 
                      "Non-VIP customers have $1000 order limit");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(customer)
                .WithItems(new[] { expensiveItem })
                .WithTotalAmount(orderTotal)
                .Build());

            // Assert - Should pass because customer is VIP
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidationBuilder_WithMultipleComplexRulesViolations_ShouldReturnAllErrors()
        {
            // Arrange
            Customer regularCustomer = new CustomerBuilder()
                .WithType(CustomerType.Regular)  // Not VIP
                .WithEmail("regular@example.com")
                .Build();

            Product expensiveProduct = new ProductBuilder()
                .WithPrice(2500m)  // Exceeds limit
                .Build();

            OrderItem expensiveItem = new OrderItemBuilder()
                .WithProduct(expensiveProduct)
                .WithQuantity(2)
                .WithUnitPrice(2500m)  // Exceeds item limit
                .Build();

            decimal orderTotal = 5000m;  // Exceeds customer limit

            ValidationBuilder<Order> builder = new();

            // Act - Complex business rules that will fail
            builder.RuleFor(o => o.Customer, regularCustomer)
                .Must(c => c.Type == CustomerType.VIP || orderTotal <= 1000m, 
                      "Orders over $1000 require VIP customer status");

            builder.RuleFor(o => o.Items, new[] { expensiveItem })
                .Must(items => items.All(i => i.UnitPrice <= 2000m), "No item can exceed $2000");

            builder.RuleFor(o => o.TotalAmount, orderTotal)
                .Must(total => regularCustomer.Type == CustomerType.VIP || total <= 1000m, 
                      "Non-VIP customers have $1000 order limit");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(regularCustomer)
                .WithItems(new[] { expensiveItem })
                .WithTotalAmount(orderTotal)
                .Build());

            // Assert - Should fail with multiple errors
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.Keys.Count.ShouldBe(3);  // Three different properties failed validation
            errors.ShouldContainKey("Customer");
            errors.ShouldContainKey("Items");
            errors.ShouldContainKey("TotalAmount");
        }

        [Fact]
        public void ValidationBuilder_WithCrossPropertyValidation_ShouldValidateRelatedProperties()
        {
            // Arrange
            ShippingAddress shippingAddress = new ShippingAddressBuilder()
                .WithStreet("123 Main St")
                .Build();

            BillingAddress billingAddress = new BillingAddressBuilder()
                .WithStreet("456 Different St")  // Different from shipping
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act - Cross-property validation
            builder.RuleFor(o => o.ShippingAddress, shippingAddress)
                .NotNull();

            builder.RuleFor(o => o.BillingAddress, billingAddress)
                .NotNull()
                .Must(billing => billing.Street == shippingAddress.Street || billing.Street != "", 
                      "Billing address must match shipping address or be explicitly different");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithShippingAddress(shippingAddress)
                .WithBillingAddress(billingAddress)
                .Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();  // Different addresses are allowed
        }

        [Fact]
        public void ValidationBuilder_WithConditionalValidationRules_ShouldApplyRulesBasedOnConditions()
        {
            // Arrange
            Customer vipCustomer = new CustomerBuilder()
                .WithType(CustomerType.VIP)
                .Build();

            decimal highOrderAmount = 5000m;

            ValidationBuilder<Order> builder = new();

            // Act - Conditional validation
            builder.RuleFor(o => o.TotalAmount, highOrderAmount)
                .GreaterThan(0)
                .LessThanOrEqualTo(1000m)
                .When(amount => vipCustomer.Type != CustomerType.VIP)  // Only apply limit to non-VIP
                .WithMessage("Regular customers have $1000 order limit");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(vipCustomer)
                .WithTotalAmount(highOrderAmount)
                .Build());

            // Assert - Should pass because VIP customer bypasses the limit
            result.IsSuccess.ShouldBeTrue();
        }
    }

    #endregion

    #region Deep Object Hierarchy Tests

    /// <summary>
    /// Tests for edge cases with deep object hierarchies and complex nesting.
    /// </summary>
    public class DeepObjectHierarchyEdgeCases
    {
        [Fact]
        public void ValidationBuilder_WithDeepCategoryHierarchy_ShouldValidateAllLevels()
        {
            // Arrange - Create 5-level deep category hierarchy
            Category level1 = new CategoryBuilder().WithName("Electronics").Build();
            Category level2 = new CategoryBuilder().WithName("Computers").WithParentCategory(level1).Build();
            Category level3 = new CategoryBuilder().WithName("Laptops").WithParentCategory(level2).Build();
            Category level4 = new CategoryBuilder().WithName("Gaming").WithParentCategory(level3).Build();
            Category level5 = new CategoryBuilder().WithName("High-End").WithParentCategory(level4).Build();

            Product product = new ProductBuilder()
                .WithCategory(level5)
                .WithName("Gaming Laptop")
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act - Validate deep hierarchy
            builder.RuleFor(o => o.Items, new[] { new OrderItemBuilder().WithProduct(product).Build() })
                .NotEmpty()
                .Must(items => items.All(i => i.Product.Category.Level <= 5), "Category depth cannot exceed 5 levels")
                .Must(items => items.All(i => i.Product.Category.Path.Split('/').Length <= 5), "Category path too deep");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(new[] { new OrderItemBuilder().WithProduct(product).Build() })
                .Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidationBuilder_WithTooDeepHierarchy_ShouldReturnValidationFailure()
        {
            // Arrange - Create 7-level deep category hierarchy (exceeds limit of 5)
            Category level1 = new CategoryBuilder().WithName("Electronics").Build();
            Category level2 = new CategoryBuilder().WithName("Computers").WithParentCategory(level1).Build();
            Category level3 = new CategoryBuilder().WithName("Laptops").WithParentCategory(level2).Build();
            Category level4 = new CategoryBuilder().WithName("Gaming").WithParentCategory(level3).Build();
            Category level5 = new CategoryBuilder().WithName("High-End").WithParentCategory(level4).Build();
            Category level6 = new CategoryBuilder().WithName("Ultra").WithParentCategory(level5).Build();
            Category level7 = new CategoryBuilder().WithName("Premium").WithParentCategory(level6).Build();

            Product product = new ProductBuilder()
                .WithCategory(level7)
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act
            builder.RuleFor(o => o.Items, new[] { new OrderItemBuilder().WithProduct(product).Build() })
                .Must(items => items.All(i => i.Product.Category.Level <= 5), "Category depth cannot exceed 5 levels");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(new[] { new OrderItemBuilder().WithProduct(product).Build() })
                .Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Items");
            errors["Items"].ShouldContain("Category depth cannot exceed 5 levels");
        }

        [Fact]
        public void ValidationBuilder_WithCircularReferenceDetection_ShouldHandleGracefully()
        {
            // Arrange - Note: We can't create true circular references with immutable objects
            // but we can simulate the validation logic that would detect them
            Category parentCategory = new CategoryBuilder().WithName("Parent").Build();
            
            ValidationBuilder<Order> builder = new();

            // Act - Simulate circular reference detection logic
            builder.RuleFor(o => o.Items, new[] { new OrderItemBuilder().Build() })
                .Must(items => items.All(i => ValidateCategoryHierarchy(i.Product.Category)), 
                      "Category hierarchy cannot contain circular references");

            Result<Order> result = builder.Build(() => new OrderBuilder().Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();  // No circular reference in our test data
        }

        private static bool ValidateCategoryHierarchy(Category category)
        {
            // Simulate circular reference detection
            HashSet<Guid> visitedCategories = [];
            Category? current = category;

            while (current is not null)
            {
                if (!visitedCategories.Add(current.Id))
                {
                    return false;  // Circular reference detected
                }
                current = current.ParentCategory;
            }

            return true;
        }

        [Fact]
        public void ValidationBuilder_WithLargeObjectGraph_ShouldMaintainPerformance()
        {
            // Arrange - Create order with many items
            IEnumerable<OrderItem> manyItems = Enumerable.Range(1, 100)
                .Select(i => new OrderItemBuilder()
                    .WithProduct(new ProductBuilder()
                        .WithName($"Product {i}")
                        .WithSku($"SKU-{i:D3}")
                        .Build())
                    .Build());

            ValidationBuilder<Order> builder = new();
            DateTime startTime = DateTime.UtcNow;

            // Act - Validate large object graph
            builder.RuleFor(o => o.Items, manyItems)
                .NotEmpty()
                .Must(items => items.Select(i => i.Product.Sku).Distinct().Count() == items.Count(), 
                      "All product SKUs must be unique")
                .Must(items => items.All(i => i.Quantity > 0), "All quantities must be positive");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(manyItems)
                .Build());

            DateTime endTime = DateTime.UtcNow;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            TimeSpan duration = endTime - startTime;
            duration.TotalMilliseconds.ShouldBeLessThan(1000);  // Should complete in under 1 second
        }
    }

    #endregion

    #region Performance and Scalability Tests

    /// <summary>
    /// Tests for performance validation with large complex objects.
    /// </summary>
    public class PerformanceAndScalabilityTests
    {
        [Fact]
        public void ValidationBuilder_WithLargeComplexObject_ShouldValidateWithinTimeLimit()
        {
            // Arrange - Create complex order with many nested objects
            Customer complexCustomer = new CustomerBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithEmail("john.doe@example.com")
                .AddTag("VIP")
                .AddTag("Premium")
                .AddTag("Active")
                .Build();

            IEnumerable<OrderItem> manyComplexItems = Enumerable.Range(1, 50)
                .Select(i =>
                {
                    Category category = new CategoryBuilder()
                        .WithName($"Category {i}")
                        .Build();

                    Product product = new ProductBuilder()
                        .WithName($"Product {i}")
                        .WithSku($"PROD-{i:D3}")
                        .WithCategory(category)
                        .WithPrice(99.99m * i)
                        .Build();

                    return new OrderItemBuilder()
                        .WithProduct(product)
                        .WithQuantity(i % 5 + 1)
                        .WithUnitPrice(product.Price)
                        .Build();
                });

            ValidationBuilder<Order> builder = new();
            DateTime startTime = DateTime.UtcNow;

            // Act - Comprehensive validation of large complex object
            builder.RuleFor(o => o.Customer, complexCustomer)
                .NotNull()
                .Must(c => !string.IsNullOrEmpty(c.FirstName), "First name required")
                .Must(c => !string.IsNullOrEmpty(c.LastName), "Last name required")
                .Must(c => c.Email.Contains('@'), "Valid email required");

            builder.RuleFor(o => o.Items, manyComplexItems)
                .NotEmpty()
                .Must(items => items.Any(), "Must have at least one item")
                .MaxCount(100)
                .Must(items => items.All(i => i.Quantity > 0), "All quantities positive")
                .Must(items => items.All(i => i.UnitPrice > 0), "All prices positive")
                .Must(items => items.Select(i => i.Product.Sku).Distinct().Count() == items.Count(), "SKUs unique");

            decimal totalAmount = manyComplexItems.Sum(i => i.TotalPrice);
            builder.RuleFor(o => o.TotalAmount, totalAmount)
                .GreaterThan(0)
                .LessThan(500000m);

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(complexCustomer)
                .WithItems(manyComplexItems)
                .WithTotalAmount(totalAmount)
                .Build());

            DateTime endTime = DateTime.UtcNow;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            TimeSpan duration = endTime - startTime;
            duration.TotalMilliseconds.ShouldBeLessThan(2000);  // Should complete in under 2 seconds
        }

        [Fact]
        public void ValidationBuilder_WithManyValidationRules_ShouldMaintainPerformance()
        {
            // Arrange
            Order complexOrder = new OrderBuilder()
                .WithOrderNumber("PERF-TEST-001")
                .WithTotalAmount(1500m)
                .Build();

            ValidationBuilder<Order> builder = new();
            DateTime startTime = DateTime.UtcNow;

            // Act - Apply many validation rules
            builder.RuleFor(o => o.Id, complexOrder.Id).NotEqual(Guid.Empty);
            builder.RuleFor(o => o.OrderNumber, complexOrder.OrderNumber).NotEmpty().MinimumLength(5).MaximumLength(50);
            builder.RuleFor(o => o.Customer, complexOrder.Customer).NotNull();
            builder.RuleFor(o => o.Items, complexOrder.Items).NotEmpty().Must(items => items.Count >= 1, "Must have at least one item");
            builder.RuleFor(o => o.ShippingAddress, complexOrder.ShippingAddress).NotNull();
            builder.RuleFor(o => o.BillingAddress, complexOrder.BillingAddress).NotNull();
            builder.RuleFor(o => o.PaymentInfo, complexOrder.PaymentInfo).NotNull();
            builder.RuleFor(o => o.Status, complexOrder.Status).NotEqual(OrderStatus.Cancelled);
            builder.RuleFor(o => o.CreatedAt, complexOrder.CreatedAt).Must(date => date <= DateTime.UtcNow, "Created date must not be in the future");
            builder.RuleFor(o => o.TotalAmount, complexOrder.TotalAmount).GreaterThan(0).Must(amount => amount < 50000m, "Total amount must be less than $50,000");
            builder.RuleFor(o => o.Tax, complexOrder.Tax).NotNull();

            // Apply conditional rules
            for (int i = 0; i < 20; i++)
            {
                int index = i;  // Capture for closure
                builder.RuleFor(o => o.TotalAmount, complexOrder.TotalAmount)
                    .When(amount => index % 2 == 0)
                    .GreaterThan(index * 10);
            }

            Result<Order> result = builder.Build(() => complexOrder);

            DateTime endTime = DateTime.UtcNow;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            TimeSpan duration = endTime - startTime;
            duration.TotalMilliseconds.ShouldBeLessThan(500);  // Should complete quickly even with many rules
        }

        [Fact]
        public void ValidationBuilder_WithDeepValidationChains_ShouldHandleComplexity()
        {
            // Arrange - Create order with deeply nested validation requirements
            Category rootCategory = new CategoryBuilder().WithName("Electronics").Build();
            Category subCategory = new CategoryBuilder().WithName("Computers").WithParentCategory(rootCategory).Build();
            Category leafCategory = new CategoryBuilder().WithName("Laptops").WithParentCategory(subCategory).Build();

            Product product = new ProductBuilder()
                .WithCategory(leafCategory)
                .WithName("High-End Laptop")
                .WithPrice(2500m)
                .Build();

            OrderItem item = new OrderItemBuilder()
                .WithProduct(product)
                .WithQuantity(1)
                .WithUnitPrice(2500m)
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act - Deep validation chain
            builder.RuleFor(o => o.Items, new[] { item })
                .NotEmpty()
                .Must(items => items.All(i =>
                    i.Product.Category.ParentCategory is not null &&
                    i.Product.Category.ParentCategory.ParentCategory is not null), 
                    "Products must have at least 3-level category hierarchy")
                .Must(items => items.All(i =>
                    i.Product.Category.Path.Split('/').Length >= 3), 
                    "Category path must have at least 3 levels")
                .Must(items => items.All(i =>
                    i.UnitPrice == i.Product.Price), 
                    "Item price must match product price")
                .Must(items => items.Sum(i => i.TotalPrice) <= 10000m, 
                    "Order total cannot exceed $10,000");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithItems(new[] { item })
                .Build());

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }
    }

    #endregion

    #region Error Aggregation and Reporting Tests

    /// <summary>
    /// Tests for proper error aggregation across complex object validation.
    /// </summary>
    public class ErrorAggregationAndReporting
    {
        [Fact]
        public void ValidationBuilder_WithMultipleNestedValidationFailures_ShouldAggregateAllErrors()
        {
            // Arrange - Create order with multiple validation failures at different levels
            Customer invalidCustomer = new CustomerBuilder()
                .WithFirstName("")  // Error 1
                .WithEmail("bad-email")  // Error 2
                .Build();

            Product invalidProduct = new ProductBuilder()
                .WithName("")  // Error 3
                .WithPrice(-100m)  // Error 4
                .Build();

            OrderItem invalidItem = new OrderItemBuilder()
                .WithProduct(invalidProduct)
                .WithQuantity(-1)  // Error 5
                .Build();

            ShippingAddress invalidShipping = new ShippingAddressBuilder()
                .WithStreet("")  // Error 6
                .Build();

            ValidationBuilder<Order> builder = new();

            // Act - Apply validations that will all fail
            builder.RuleFor(o => o.Customer, invalidCustomer)
                .Must(c => !string.IsNullOrEmpty(c.FirstName), "Customer first name required")
                .Must(c => c.Email.Contains('@'), "Valid customer email required");

            builder.RuleFor(o => o.Items, new[] { invalidItem })
                .Must(items => items.All(i => !string.IsNullOrEmpty(i.Product.Name)), "Product name required")
                .Must(items => items.All(i => i.Product.Price > 0), "Product price must be positive")
                .Must(items => items.All(i => i.Quantity > 0), "Item quantity must be positive");

            builder.RuleFor(o => o.ShippingAddress, invalidShipping)
                .Must(a => !string.IsNullOrEmpty(a.Street), "Shipping street address required");

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(invalidCustomer)
                .WithItems(new[] { invalidItem })
                .WithShippingAddress(invalidShipping)
                .Build());

            // Assert - All errors should be collected
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            
            errors.ShouldContainKey("Customer");
            errors["Customer"].Length.ShouldBe(2);  // FirstName and Email errors
            
            errors.ShouldContainKey("Items");
            errors["Items"].Length.ShouldBe(3);  // Product name, price, and quantity errors
            
            errors.ShouldContainKey("ShippingAddress");
            errors["ShippingAddress"].Length.ShouldBe(1);  // Street address error
        }

        [Fact]
        public void ValidationBuilder_WithNestedResultFailures_ShouldPreserveErrorPaths()
        {
            // Arrange - Create Results with nested validation errors
            Result<Customer> customerResult = Customer.Create("", "", "bad-email", "", DateTime.Today, null!);
            Result<ShippingAddress> shippingResult = ShippingAddress.Create("", "", "", "", "");
            Result<BillingAddress> billingResult = BillingAddress.Create("", "", "", "", "");

            ValidationBuilder<Order> builder = new();

            // Act - Use Result composition
            builder.RuleFor(o => o.Customer, customerResult, out Customer? customer);
            builder.RuleFor(o => o.ShippingAddress, shippingResult, out ShippingAddress? shipping);
            builder.RuleFor(o => o.BillingAddress, billingResult, out BillingAddress? billing);

            Result<Order> result = builder.Build(() => new OrderBuilder()
                .WithCustomer(customer!)
                .WithShippingAddress(shipping!)
                .WithBillingAddress(billing!)
                .Build());

            // Assert - Error paths should be preserved with proper prefixing
            result.IsFailure.ShouldBeTrue();
            Dictionary<string, string[]> errors = result.Match(
                onSuccess: _ => new Dictionary<string, string[]>(),
                onValidationException: validationErrors => new Dictionary<string, string[]>(validationErrors),
                onError: _ => new Dictionary<string, string[]>(),
                onSecurityException: _ => new Dictionary<string, string[]>(),
                onOperationCanceledException: _ => new Dictionary<string, string[]>()
            );

            // Should contain prefixed error paths from nested Results
            errors.Keys.ShouldContain(key => key.StartsWith("Customer"));
            errors.Keys.ShouldContain(key => key.StartsWith("ShippingAddress"));
            errors.Keys.ShouldContain(key => key.StartsWith("BillingAddress"));
        }
    }

    #endregion
}
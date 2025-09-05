using FlowRight.Core.Results;

namespace FlowRight.Validation.Tests.TestModels;

/// <summary>
/// Represents an e-commerce order with complex nested structure for testing ValidationBuilder with complex objects.
/// This model includes nested objects, collections, and multiple levels of hierarchy.
/// </summary>
public class Order
{
    public Guid Id { get; }
    public string OrderNumber { get; }
    public Customer Customer { get; }
    public IEnumerable<OrderItem> Items { get; }
    public ShippingAddress ShippingAddress { get; }
    public BillingAddress BillingAddress { get; }
    public PaymentInfo PaymentInfo { get; }
    public OrderStatus Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime? CompletedAt { get; }
    public decimal TotalAmount { get; }
    public Tax Tax { get; }
    public IEnumerable<OrderNote> Notes { get; }
    public Promotion? AppliedPromotion { get; }

    public Order(Guid id, string orderNumber, Customer customer, IEnumerable<OrderItem> items,
                 ShippingAddress shippingAddress, BillingAddress billingAddress, PaymentInfo paymentInfo,
                 OrderStatus status, DateTime createdAt, DateTime? completedAt, decimal totalAmount,
                 Tax tax, IEnumerable<OrderNote> notes, Promotion? appliedPromotion = null)
    {
        Id = id;
        OrderNumber = orderNumber;
        Customer = customer;
        Items = items;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        PaymentInfo = paymentInfo;
        Status = status;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        TotalAmount = totalAmount;
        Tax = tax;
        Notes = notes;
        AppliedPromotion = appliedPromotion;
    }

    /// <summary>
    /// Factory method that returns a Result&lt;Order&gt; for testing Result&lt;T&gt; composition with complex validation.
    /// </summary>
    public static Result<Order> Create(string orderNumber, Customer customer, IEnumerable<OrderItem> items,
                                       ShippingAddress shippingAddress, BillingAddress billingAddress,
                                       PaymentInfo paymentInfo)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return Result.Failure<Order>("Order number is required");

        if (customer is null)
            return Result.Failure<Order>("Customer is required");

        if (items is null || !items.Any())
            return Result.Failure<Order>("Order must contain at least one item");

        decimal totalAmount = items.Sum(item => item.Quantity * item.UnitPrice);
        if (totalAmount <= 0)
            return Result.Failure<Order>("Order total must be greater than zero");

        Tax tax = new(totalAmount * 0.08m, 0.08m);
        
        return Result.Success(new Order(
            Guid.NewGuid(),
            orderNumber,
            customer,
            items,
            shippingAddress,
            billingAddress,
            paymentInfo,
            OrderStatus.Pending,
            DateTime.UtcNow,
            null,
            totalAmount + tax.Amount,
            tax,
            [],
            null));
    }
}

/// <summary>
/// Represents a customer with nested address and contact information.
/// </summary>
public class Customer
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string Phone { get; }
    public DateTime DateOfBirth { get; }
    public Address PrimaryAddress { get; }
    public IEnumerable<Address> AlternativeAddresses { get; }
    public CustomerPreferences Preferences { get; }
    public CustomerType Type { get; }
    public bool IsActive { get; }
    public DateTime CreatedAt { get; }
    public IEnumerable<string> Tags { get; }

    public Customer(Guid id, string firstName, string lastName, string email, string phone,
                   DateTime dateOfBirth, Address primaryAddress, IEnumerable<Address> alternativeAddresses,
                   CustomerPreferences preferences, CustomerType type, bool isActive,
                   DateTime createdAt, IEnumerable<string> tags)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        PrimaryAddress = primaryAddress;
        AlternativeAddresses = alternativeAddresses;
        Preferences = preferences;
        Type = type;
        IsActive = isActive;
        CreatedAt = createdAt;
        Tags = tags;
    }

    public static Result<Customer> Create(string firstName, string lastName, string email, string phone,
                                         DateTime dateOfBirth, Address primaryAddress)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(firstName))
            errors["FirstName"] = ["First name is required"];

        if (string.IsNullOrWhiteSpace(lastName))
            errors["LastName"] = ["Last name is required"];

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            errors["Email"] = ["Valid email address is required"];

        if (dateOfBirth >= DateTime.Today)
            errors["DateOfBirth"] = ["Date of birth must be in the past"];

        if (primaryAddress is null)
            errors["PrimaryAddress"] = ["Primary address is required"];

        if (errors.Count is not 0)
            return Result.Failure<Customer>(errors);

        CustomerPreferences defaultPreferences = new(true, true, false);
        
        return Result.Success(new Customer(
            Guid.NewGuid(),
            firstName,
            lastName,
            email,
            phone ?? string.Empty,
            dateOfBirth,
            primaryAddress,
            [],
            defaultPreferences,
            CustomerType.Regular,
            true,
            DateTime.UtcNow,
            []));
    }

    private static bool IsValidEmail(string email) =>
        email.Contains('@') && email.Contains('.') && email.Length > 5;
}

/// <summary>
/// Base address class with common properties.
/// </summary>
public abstract class Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }
    public AddressType Type { get; }

    protected Address(string street, string city, string state, string postalCode, string country, AddressType type)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Type = type;
    }
}

/// <summary>
/// Shipping address with delivery instructions.
/// </summary>
public class ShippingAddress : Address
{
    public string? DeliveryInstructions { get; }
    public bool IsResidential { get; }

    public ShippingAddress(string street, string city, string state, string postalCode, string country,
                          string? deliveryInstructions = null, bool isResidential = true)
        : base(street, city, state, postalCode, country, AddressType.Shipping)
    {
        DeliveryInstructions = deliveryInstructions;
        IsResidential = isResidential;
    }

    public static Result<ShippingAddress> Create(string street, string city, string state, string postalCode, string country)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(street))
            errors["Street"] = ["Street address is required"];

        if (string.IsNullOrWhiteSpace(city))
            errors["City"] = ["City is required"];

        if (string.IsNullOrWhiteSpace(postalCode))
            errors["PostalCode"] = ["Postal code is required"];

        if (errors.Count is not 0)
            return Result.Failure<ShippingAddress>(errors);

        return Result.Success(new ShippingAddress(street, city, state, postalCode, country));
    }
}

/// <summary>
/// Billing address with additional billing information.
/// </summary>
public class BillingAddress : Address
{
    public string? CompanyName { get; }
    public string? TaxId { get; }

    public BillingAddress(string street, string city, string state, string postalCode, string country,
                         string? companyName = null, string? taxId = null)
        : base(street, city, state, postalCode, country, AddressType.Billing)
    {
        CompanyName = companyName;
        TaxId = taxId;
    }

    public static Result<BillingAddress> Create(string street, string city, string state, string postalCode, string country)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(street))
            errors["Street"] = ["Street address is required"];

        if (string.IsNullOrWhiteSpace(city))
            errors["City"] = ["City is required"];

        if (string.IsNullOrWhiteSpace(postalCode))
            errors["PostalCode"] = ["Postal code is required"];

        if (errors.Count is not 0)
            return Result.Failure<BillingAddress>(errors);

        return Result.Success(new BillingAddress(street, city, state, postalCode, country));
    }
}

/// <summary>
/// Represents an order item with product details and nested category structure.
/// </summary>
public class OrderItem
{
    public Guid Id { get; }
    public Product Product { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public IEnumerable<OrderItemNote> Notes { get; }
    public OrderItemStatus Status { get; }

    public OrderItem(Guid id, Product product, int quantity, decimal unitPrice,
                    IEnumerable<OrderItemNote> notes, OrderItemStatus status = OrderItemStatus.Pending)
    {
        Id = id;
        Product = product;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
        Status = status;
    }

    public static Result<OrderItem> Create(Product product, int quantity, decimal unitPrice)
    {
        Dictionary<string, string[]> errors = [];

        if (product is null)
            errors["Product"] = ["Product is required"];

        if (quantity <= 0)
            errors["Quantity"] = ["Quantity must be greater than zero"];

        if (unitPrice <= 0)
            errors["UnitPrice"] = ["Unit price must be greater than zero"];

        if (errors.Count is not 0)
            return Result.Failure<OrderItem>(errors);

        return Result.Success(new OrderItem(Guid.NewGuid(), product, quantity, unitPrice, []));
    }
}

/// <summary>
/// Represents a product with nested category hierarchy.
/// </summary>
public class Product
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Sku { get; }
    public Category Category { get; }
    public decimal Price { get; }
    public int StockQuantity { get; }
    public ProductStatus Status { get; }
    public IEnumerable<ProductAttribute> Attributes { get; }
    public IEnumerable<ProductImage> Images { get; }
    public ProductDimensions? Dimensions { get; }
    public decimal Weight { get; }

    public Product(Guid id, string name, string description, string sku, Category category,
                  decimal price, int stockQuantity, ProductStatus status,
                  IEnumerable<ProductAttribute> attributes, IEnumerable<ProductImage> images,
                  ProductDimensions? dimensions, decimal weight)
    {
        Id = id;
        Name = name;
        Description = description;
        Sku = sku;
        Category = category;
        Price = price;
        StockQuantity = stockQuantity;
        Status = status;
        Attributes = attributes;
        Images = images;
        Dimensions = dimensions;
        Weight = weight;
    }

    public static Result<Product> Create(string name, string sku, Category category, decimal price)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors["Name"] = ["Product name is required"];

        if (string.IsNullOrWhiteSpace(sku))
            errors["Sku"] = ["Product SKU is required"];

        if (category is null)
            errors["Category"] = ["Product category is required"];

        if (price <= 0)
            errors["Price"] = ["Product price must be greater than zero"];

        if (errors.Count is not 0)
            return Result.Failure<Product>(errors);

        return Result.Success(new Product(
            Guid.NewGuid(),
            name,
            string.Empty,
            sku,
            category,
            price,
            0,
            ProductStatus.Active,
            [],
            [],
            null,
            0));
    }
}

/// <summary>
/// Represents a product category with hierarchical structure (parent-child relationships).
/// </summary>
public class Category
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public Category? ParentCategory { get; }
    public IEnumerable<Category> SubCategories { get; }
    public int Level { get; }
    public string Path { get; }
    public bool IsActive { get; }

    public Category(Guid id, string name, string description, Category? parentCategory,
                   IEnumerable<Category> subCategories, int level, string path, bool isActive = true)
    {
        Id = id;
        Name = name;
        Description = description;
        ParentCategory = parentCategory;
        SubCategories = subCategories;
        Level = level;
        Path = path;
        IsActive = isActive;
    }

    public static Result<Category> Create(string name, Category? parentCategory = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>("Category name is required");

        int level = parentCategory?.Level + 1 ?? 0;
        string path = parentCategory is null ? name : $"{parentCategory.Path}/{name}";

        return Result.Success(new Category(
            Guid.NewGuid(),
            name,
            string.Empty,
            parentCategory,
            [],
            level,
            path));
    }
}

/// <summary>
/// Supporting classes for complex object structure.
/// </summary>
public record CustomerPreferences(bool EmailNotifications, bool SmsNotifications, bool MarketingEmails);
public record Tax(decimal Amount, decimal Rate);
public record OrderNote(Guid Id, string Content, DateTime CreatedAt, string CreatedBy);
public record OrderItemNote(Guid Id, string Content, DateTime CreatedAt);
public record Promotion(Guid Id, string Code, string Description, decimal DiscountAmount, decimal DiscountPercentage);
public record PaymentInfo(string CardNumber, string CardHolderName, string ExpiryDate, PaymentMethod Method);
public record ProductAttribute(string Name, string Value);
public record ProductImage(Guid Id, string Url, string Alt, bool IsPrimary);
public record ProductDimensions(decimal Length, decimal Width, decimal Height);

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum OrderItemStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned
}

public enum CustomerType
{
    Regular,
    Premium,
    VIP,
    Corporate
}

public enum AddressType
{
    Shipping,
    Billing,
    Primary
}

public enum ProductStatus
{
    Active,
    Inactive,
    Discontinued
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    PayPal,
    BankTransfer
}

/// <summary>
/// Builder pattern for creating complex Order test objects.
/// </summary>
public class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _orderNumber = "ORD-001";
    private Customer _customer;
    private List<OrderItem> _items = [];
    private ShippingAddress _shippingAddress;
    private BillingAddress _billingAddress;
    private PaymentInfo _paymentInfo;
    private OrderStatus _status = OrderStatus.Pending;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _completedAt;
    private decimal _totalAmount = 100m;
    private Tax _tax = new(8m, 0.08m);
    private List<OrderNote> _notes = [];
    private Promotion? _appliedPromotion;

    public OrderBuilder()
    {
        // Set up default complex object structure
        Category category = new CategoryBuilder().Build();
        Product product = new ProductBuilder().WithCategory(category).Build();
        OrderItem item = new OrderItemBuilder().WithProduct(product).Build();
        _items.Add(item);

        Address primaryAddress = new AddressBuilder().Build();
        _customer = new CustomerBuilder().WithPrimaryAddress(primaryAddress).Build();
        _shippingAddress = new ShippingAddressBuilder().Build();
        _billingAddress = new BillingAddressBuilder().Build();
        _paymentInfo = new PaymentInfo("1234-5678-9012-3456", "John Doe", "12/25", PaymentMethod.CreditCard);
    }

    public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder WithOrderNumber(string orderNumber)
    {
        _orderNumber = orderNumber;
        return this;
    }

    public OrderBuilder WithCustomer(Customer customer)
    {
        _customer = customer;
        return this;
    }

    public OrderBuilder WithItems(IEnumerable<OrderItem> items)
    {
        _items = items.ToList();
        return this;
    }

    public OrderBuilder AddItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    public OrderBuilder WithShippingAddress(ShippingAddress address)
    {
        _shippingAddress = address;
        return this;
    }

    public OrderBuilder WithBillingAddress(BillingAddress address)
    {
        _billingAddress = address;
        return this;
    }

    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithTotalAmount(decimal amount)
    {
        _totalAmount = amount;
        return this;
    }

    public Order Build() =>
        new(_id, _orderNumber, _customer, _items, _shippingAddress, _billingAddress,
            _paymentInfo, _status, _createdAt, _completedAt, _totalAmount, _tax, _notes, _appliedPromotion);
}

/// <summary>
/// Builder pattern for creating Customer test objects with complex nested structure.
/// </summary>
public class CustomerBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john.doe@example.com";
    private string _phone = "555-1234";
    private DateTime _dateOfBirth = DateTime.Today.AddYears(-30);
    private Address _primaryAddress = new AddressBuilder().Build();
    private List<Address> _alternativeAddresses = [];
    private CustomerPreferences _preferences = new(true, false, true);
    private CustomerType _type = CustomerType.Regular;
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private List<string> _tags = [];

    public CustomerBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithPrimaryAddress(Address address)
    {
        _primaryAddress = address;
        return this;
    }

    public CustomerBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public CustomerBuilder WithType(CustomerType type)
    {
        _type = type;
        return this;
    }

    public CustomerBuilder AddTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public Customer Build() =>
        new(_id, _firstName, _lastName, _email, _phone, _dateOfBirth,
            _primaryAddress, _alternativeAddresses, _preferences, _type,
            _isActive, _createdAt, _tags);
}

/// <summary>
/// Supporting builder classes for complex object construction.
/// </summary>
public class CategoryBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Electronics";
    private string _description = "Electronic products";
    private Category? _parentCategory;
    private List<Category> _subCategories = [];
    private int _level = 0;
    private string _path = "Electronics";

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        _path = name;
        return this;
    }

    public CategoryBuilder WithParentCategory(Category parent)
    {
        _parentCategory = parent;
        _level = parent.Level + 1;
        _path = $"{parent.Path}/{_name}";
        return this;
    }

    public Category Build() =>
        new(_id, _name, _description, _parentCategory, _subCategories, _level, _path);
}

public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Product";
    private string _sku = "TEST-001";
    private Category _category = new CategoryBuilder().Build();
    private decimal _price = 99.99m;

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductBuilder WithCategory(Category category)
    {
        _category = category;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public Product Build() =>
        new(_id, _name, string.Empty, _sku, _category, _price, 10, ProductStatus.Active,
            [], [], null, 1.5m);
}

public class OrderItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private Product _product = new ProductBuilder().Build();
    private int _quantity = 1;
    private decimal _unitPrice = 99.99m;

    public OrderItemBuilder WithProduct(Product product)
    {
        _product = product;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithUnitPrice(decimal price)
    {
        _unitPrice = price;
        return this;
    }

    public OrderItem Build() =>
        new(_id, _product, _quantity, _unitPrice, []);
}

public class AddressBuilder
{
    private string _street = "123 Main St";
    private string _city = "Anytown";
    private string _state = "CA";
    private string _postalCode = "12345";
    private string _country = "USA";

    public AddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public AddressBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public AddressBuilder WithPostalCode(string postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    public Address Build() =>
        new ShippingAddress(_street, _city, _state, _postalCode, _country);
}

public class ShippingAddressBuilder
{
    private string _street = "123 Main St";
    private string _city = "Anytown";
    private string _state = "CA";
    private string _postalCode = "12345";
    private string _country = "USA";

    public ShippingAddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public ShippingAddress Build() =>
        new(_street, _city, _state, _postalCode, _country);
}

public class BillingAddressBuilder
{
    private string _street = "123 Billing St";
    private string _city = "Anytown";
    private string _state = "CA";
    private string _postalCode = "12345";
    private string _country = "USA";

    public BillingAddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public BillingAddress Build() =>
        new(_street, _city, _state, _postalCode, _country);
}
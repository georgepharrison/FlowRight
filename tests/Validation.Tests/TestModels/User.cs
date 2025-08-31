using FlowRight.Core.Results;

namespace FlowRight.Validation.Tests.TestModels;

/// <summary>
/// Test domain model representing a user for ValidationBuilder testing scenarios.
/// This class provides realistic property types and validation requirements.
/// </summary>
public class User
{
    public string Name { get; }
    public string Email { get; }
    public int Age { get; }
    public Guid? Id { get; }
    public IEnumerable<string> Roles { get; }
    public Profile? Profile { get; }
    public decimal Salary { get; }
    public long Phone { get; }
    public short Priority { get; }
    public double Score { get; }
    public float Rating { get; }

    // New properties for extended property type testing
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public bool IsActive { get; }
    public bool? IsVerified { get; }
    public byte Status { get; }
    public sbyte Balance { get; }
    public uint Points { get; }
    public ulong Token { get; }
    public char Grade { get; }
    public char? MiddleInitial { get; }
    public int? OptionalAge { get; }

    public User(string name, string email, int age, Guid? id, IEnumerable<string> roles,
                Profile? profile = null, decimal salary = 0m, long phone = 0L,
                short priority = 0, double score = 0.0, float rating = 0.0f,
                DateTime createdAt = default, DateTime? updatedAt = null, bool isActive = true,
                bool? isVerified = null, byte status = 0, sbyte balance = 0,
                uint points = 0, ulong token = 0, char grade = 'A', char? middleInitial = null, int? optionalAge = null)
    {
        Name = name;
        Email = email;
        Age = age;
        Id = id;
        Roles = roles;
        Profile = profile;
        Salary = salary;
        Phone = phone;
        Priority = priority;
        Score = score;
        Rating = rating;
        CreatedAt = createdAt == default ? DateTime.UtcNow : createdAt;
        UpdatedAt = updatedAt;
        IsActive = isActive;
        IsVerified = isVerified;
        Status = status;
        Balance = balance;
        Points = points;
        Token = token;
        Grade = grade;
        MiddleInitial = middleInitial;
        OptionalAge = optionalAge;
    }
}

/// <summary>
/// Test model for Result<T> composition scenarios.
/// </summary>
public class Profile
{
    public string Bio { get; }
    public DateTime CreatedAt { get; }

    public Profile(string bio, DateTime createdAt)
    {
        Bio = bio;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory method that returns a Result<Profile> for testing Result<T> composition.
    /// </summary>
    public static Result<Profile> Create(string bio)
    {
        if (string.IsNullOrWhiteSpace(bio))
            return Result.Failure<Profile>("Profile bio is required");

        if (bio.Length > 500)
            return Result.Failure<Profile>("Profile bio cannot exceed 500 characters");

        return Result.Success(new Profile(bio, DateTime.UtcNow));
    }
}

/// <summary>
/// Builder pattern for creating User test objects with fluent interface.
/// </summary>
public class UserBuilder
{
    private string _name = "John Doe";
    private string _email = "john.doe@example.com";
    private int _age = 30;
    private Guid? _id = Guid.NewGuid();
    private IEnumerable<string> _roles = new[] { "User" };
    private Profile? _profile;
    private decimal _salary = 50000m;
    private long _phone = 5551234567L;
    private short _priority = 1;
    private double _score = 85.5;
    private float _rating = 4.2f;

    // New properties for extended property type testing
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _updatedAt;
    private bool _isActive = true;
    private bool? _isVerified = true;
    private byte _status = 1;
    private sbyte _balance = 0;
    private uint _points = 0;
    private ulong _token = 0;
    private char _grade = 'A';
    private char? _middleInitial;
    private int? _optionalAge;

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithAge(int age)
    {
        _age = age;
        return this;
    }

    public UserBuilder WithId(Guid? id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithRoles(IEnumerable<string> roles)
    {
        _roles = roles;
        return this;
    }

    public UserBuilder WithProfile(Profile? profile)
    {
        _profile = profile;
        return this;
    }

    public UserBuilder WithSalary(decimal salary)
    {
        _salary = salary;
        return this;
    }

    public UserBuilder WithPhone(long phone)
    {
        _phone = phone;
        return this;
    }

    public UserBuilder WithPriority(short priority)
    {
        _priority = priority;
        return this;
    }

    public UserBuilder WithScore(double score)
    {
        _score = score;
        return this;
    }

    public UserBuilder WithRating(float rating)
    {
        _rating = rating;
        return this;
    }

    public User Build() =>
        new(_name, _email, _age, _id, _roles, _profile, _salary, _phone, _priority, _score, _rating,
            _createdAt, _updatedAt, _isActive, _isVerified, _status, _balance, _points, _token, _grade, _middleInitial, _optionalAge);
}
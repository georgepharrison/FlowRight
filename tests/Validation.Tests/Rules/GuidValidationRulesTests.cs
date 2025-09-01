using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for GUID validation rules that define expected behavior
/// for GUID-specific validation patterns. These tests follow TDD principles 
/// and will initially fail until the validation rule implementations are complete.
/// 
/// Test Coverage:
/// - GuidNotEmptyRule - validates GUID is not empty (00000000-0000-0000-0000-000000000000)
/// - ValidGuidFormatRule - validates string is properly formatted GUID
/// - SpecificGuidValueRule - validates GUID matches specific expected value
/// - GuidNonZeroTimestampRule - validates GUID has non-zero timestamp (for sequential GUIDs)
/// - GuidValidFormatRule - validates GUID format and structure
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for GUID validation rules.
/// </summary>
public class GuidValidationRulesTests
{
    #region GuidNotEmptyRule Tests

    /// <summary>
    /// Tests for GuidNotEmptyRule - validates GUID is not the empty GUID
    /// </summary>
    public class GuidNotEmptyRuleTests
    {
        [Fact]
        public void Validate_WithNonEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid validGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(validGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid emptyGuid = Guid.Empty;

            // Act
            string? result = rule.Validate(emptyGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("cannot be empty");
        }

        [Fact]
        public void Validate_WithNullableNonEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid? validGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(validGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid? nullGuid = null;

            // Act
            string? result = rule.Validate(nullGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("cannot be empty");
        }

        [Fact]
        public void Validate_WithSpecificNonEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid specificGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");

            // Act
            string? result = rule.Validate(specificGuid, "UserID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithMinimalNonEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid minimalGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Act
            string? result = rule.Validate(minimalGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion GuidNotEmptyRule Tests

    #region ValidGuidFormatRule Tests

    /// <summary>
    /// Tests for ValidGuidFormatRule - validates string is properly formatted as GUID
    /// </summary>
    public class ValidGuidFormatRuleTests
    {
        [Fact]
        public void Validate_WithValidGuidString_ShouldReturnNull()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string validGuidString = "12345678-1234-5678-9012-123456789012";

            // Act
            string? result = rule.Validate(validGuidString, "GUID String");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidGuidStringUpperCase_ShouldReturnNull()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string validGuidString = "12345678-1234-5678-9012-123456789ABC";

            // Act
            string? result = rule.Validate(validGuidString, "GUID String");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidGuidStringLowerCase_ShouldReturnNull()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string validGuidString = "12345678-1234-5678-9012-123456789abc";

            // Act
            string? result = rule.Validate(validGuidString, "GUID String");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithInvalidGuidStringFormat_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string invalidGuidString = "12345678-1234-5678-9012";

            // Act
            string? result = rule.Validate(invalidGuidString, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithNonHexCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string invalidGuidString = "12345678-1234-5678-9012-12345678901G";

            // Act
            string? result = rule.Validate(invalidGuidString, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithMissingHyphens_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string invalidGuidString = "12345678123456789012123456789012";

            // Act
            string? result = rule.Validate(invalidGuidString, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithTooManyHyphens_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string invalidGuidString = "1234-5678-1234-5678-9012-1234-5678-9012";

            // Act
            string? result = rule.Validate(invalidGuidString, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithEmptyString_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string emptyString = string.Empty;

            // Act
            string? result = rule.Validate(emptyString, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithNullString_ShouldReturnErrorMessage()
        {
            // Arrange
            ValidGuidFormatRule rule = new();

            // Act
            string? result = rule.Validate(null!, "GUID String");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID format");
        }

        [Fact]
        public void Validate_WithBracesFormat_ShouldReturnNull()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string guidWithBraces = "{12345678-1234-5678-9012-123456789012}";

            // Act
            string? result = rule.Validate(guidWithBraces, "GUID String");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithParenthesesFormat_ShouldReturnNull()
        {
            // Arrange
            ValidGuidFormatRule rule = new();
            string guidWithParentheses = "(12345678-1234-5678-9012-123456789012)";

            // Act
            string? result = rule.Validate(guidWithParentheses, "GUID String");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion ValidGuidFormatRule Tests

    #region SpecificGuidValueRule Tests

    /// <summary>
    /// Tests for SpecificGuidValueRule - validates GUID matches specific expected value
    /// </summary>
    public class SpecificGuidValueRuleTests
    {
        [Fact]
        public void Validate_WithMatchingGuid_ShouldReturnNull()
        {
            // Arrange
            Guid expectedGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");
            SpecificGuidValueRule rule = new(expectedGuid);

            // Act
            string? result = rule.Validate(expectedGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            Guid expectedGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");
            Guid differentGuid = Guid.Parse("87654321-4321-8765-2109-210987654321");
            SpecificGuidValueRule rule = new(expectedGuid);

            // Act
            string? result = rule.Validate(differentGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to");
            result.ShouldContain("12345678-1234-5678-9012-123456789012");
        }

        [Fact]
        public void Validate_WithNullableMatchingGuid_ShouldReturnNull()
        {
            // Arrange
            Guid expectedGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");
            SpecificGuidValueRule rule = new(expectedGuid);
            Guid? matchingGuid = expectedGuid;

            // Act
            string? result = rule.Validate(matchingGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            Guid expectedGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");
            SpecificGuidValueRule rule = new(expectedGuid);
            Guid? nullGuid = null;

            // Act
            string? result = rule.Validate(nullGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to");
        }

        [Fact]
        public void Validate_WithEmptyGuidExpected_ShouldWork()
        {
            // Arrange
            Guid expectedGuid = Guid.Empty;
            SpecificGuidValueRule rule = new(expectedGuid);

            // Act
            string? result = rule.Validate(Guid.Empty, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithWellKnownGuidValues_ShouldWork()
        {
            // Arrange
            Guid wellKnownGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            SpecificGuidValueRule rule = new(wellKnownGuid);

            // Act
            string? result = rule.Validate(wellKnownGuid, "WellKnownID");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion SpecificGuidValueRule Tests

    #region GuidNonZeroTimestampRule Tests

    /// <summary>
    /// Tests for GuidNonZeroTimestampRule - validates GUID has non-zero timestamp component
    /// This is useful for sequential/time-based GUIDs where timestamp matters
    /// </summary>
    public class GuidNonZeroTimestampRuleTests
    {
        [Fact]
        public void Validate_WithNewGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            Guid newGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(newGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            Guid emptyGuid = Guid.Empty;

            // Act
            string? result = rule.Validate(emptyGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must have non-zero timestamp");
        }

        [Fact]
        public void Validate_WithZeroTimestampGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            // This GUID has zero timestamp components (first part all zeros)
            Guid zeroTimestampGuid = Guid.Parse("00000000-0000-0000-1234-123456789012");

            // Act
            string? result = rule.Validate(zeroTimestampGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must have non-zero timestamp");
        }

        [Fact]
        public void Validate_WithValidTimestampGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            // This GUID has non-zero timestamp components
            Guid validTimestampGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");

            // Act
            string? result = rule.Validate(validTimestampGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullableValidGuid_ShouldReturnNull()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            Guid? validGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(validGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            Guid? nullGuid = null;

            // Act
            string? result = rule.Validate(nullGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must have non-zero timestamp");
        }

        [Fact]
        public void Validate_WithPartialTimestampZero_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            // This GUID has some timestamp parts zero
            Guid partialZeroGuid = Guid.Parse("12345678-0000-0000-9012-123456789012");

            // Act
            string? result = rule.Validate(partialZeroGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must have non-zero timestamp");
        }
    }

    #endregion GuidNonZeroTimestampRule Tests

    #region GuidValidFormatRule Tests

    /// <summary>
    /// Tests for GuidValidFormatRule - validates GUID format and structure integrity
    /// </summary>
    public class GuidValidFormatRuleTests
    {
        [Fact]
        public void Validate_WithValidGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid validGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(validGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid emptyGuid = Guid.Empty;

            // Act
            string? result = rule.Validate(emptyGuid, "ID");

            // Assert
            result.ShouldBeNull(); // Empty GUID is still a valid format
        }

        [Fact]
        public void Validate_WithSpecificValidGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid specificGuid = Guid.Parse("12345678-1234-5678-9012-123456789012");

            // Act
            string? result = rule.Validate(specificGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullableValidGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid? validGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(validGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid? nullGuid = null;

            // Act
            string? result = rule.Validate(nullGuid, "ID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid GUID");
        }

        [Fact]
        public void Validate_WithMaxValueGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid maxGuid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

            // Act
            string? result = rule.Validate(maxGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithMixedCaseGuid_ShouldReturnNull()
        {
            // Arrange
            GuidValidFormatRule rule = new();
            Guid mixedCaseGuid = Guid.Parse("12345678-ABCD-efab-9012-123456789ABC");

            // Act
            string? result = rule.Validate(mixedCaseGuid, "ID");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion GuidValidFormatRule Tests

    #region Edge Cases and Integration Tests

    /// <summary>
    /// Tests for edge cases and integration scenarios with GUID rules
    /// </summary>
    public class GuidEdgeCasesTests
    {
        [Fact]
        public void Validate_MultipleRulesOnSameGuid_ShouldAllPass()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();
            GuidNotEmptyRule notEmptyRule = new();
            GuidValidFormatRule validFormatRule = new();
            GuidNonZeroTimestampRule nonZeroTimestampRule = new();

            // Act & Assert
            notEmptyRule.Validate(testGuid, "ID").ShouldBeNull();
            validFormatRule.Validate(testGuid, "ID").ShouldBeNull();
            nonZeroTimestampRule.Validate(testGuid, "ID").ShouldBeNull();
        }

        [Fact]
        public void Validate_MultipleRulesOnEmptyGuid_ShouldFailAppropriately()
        {
            // Arrange
            Guid emptyGuid = Guid.Empty;
            GuidNotEmptyRule notEmptyRule = new();
            GuidValidFormatRule validFormatRule = new();
            GuidNonZeroTimestampRule nonZeroTimestampRule = new();

            // Act & Assert
            notEmptyRule.Validate(emptyGuid, "ID").ShouldNotBeNull();
            validFormatRule.Validate(emptyGuid, "ID").ShouldBeNull(); // Empty is valid format
            nonZeroTimestampRule.Validate(emptyGuid, "ID").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithVersionSpecificGuids_ShouldWork()
        {
            // Arrange - Different GUID versions
            Guid version1Guid = Guid.Parse("12345678-1234-1678-9012-123456789012"); // Version 1
            Guid version4Guid = Guid.Parse("12345678-1234-4678-9012-123456789012"); // Version 4
            GuidValidFormatRule rule = new();

            // Act & Assert
            rule.Validate(version1Guid, "V1 GUID").ShouldBeNull();
            rule.Validate(version4Guid, "V4 GUID").ShouldBeNull();
        }

        [Fact]
        public void Validate_WithBoundaryGuidValues_ShouldWork()
        {
            // Arrange
            Guid minGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            Guid maxGuid = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffffE");
            GuidValidFormatRule rule = new();

            // Act & Assert
            rule.Validate(minGuid, "Min GUID").ShouldBeNull();
            rule.Validate(maxGuid, "Max GUID").ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSequentialGuids_ShouldDetectTimestampDifferences()
        {
            // Arrange
            GuidNonZeroTimestampRule rule = new();
            
            // These represent sequential GUIDs with different timestamp components
            Guid guid1 = Guid.Parse("12345678-1234-5678-9012-123456789012");
            Guid guid2 = Guid.Parse("12345679-1234-5678-9012-123456789012");

            // Act & Assert
            rule.Validate(guid1, "GUID 1").ShouldBeNull();
            rule.Validate(guid2, "GUID 2").ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCustomGuidFormats_ShouldHandleAppropriately()
        {
            // Arrange
            ValidGuidFormatRule stringRule = new();
            
            // Various GUID string formats
            string standardFormat = "12345678-1234-5678-9012-123456789012";
            string bracesFormat = "{12345678-1234-5678-9012-123456789012}";
            string parenthesesFormat = "(12345678-1234-5678-9012-123456789012)";
            string noHyphensFormat = "12345678123456789012123456789012";

            // Act & Assert
            stringRule.Validate(standardFormat, "Standard").ShouldBeNull();
            stringRule.Validate(bracesFormat, "Braces").ShouldBeNull();
            stringRule.Validate(parenthesesFormat, "Parentheses").ShouldBeNull();
            stringRule.Validate(noHyphensFormat, "NoHyphens").ShouldNotBeNull(); // Should fail
        }

        [Fact]
        public void Validate_NullableGuidsWithMixedNullAndValid_ShouldBehaveConsistently()
        {
            // Arrange
            GuidNotEmptyRule rule = new();
            Guid?[] testGuids = { null, Guid.Empty, Guid.NewGuid(), null };

            // Act & Assert
            foreach ((Guid? guid, int index) in testGuids.Select((g, i) => (g, i)))
            {
                string? result = rule.Validate(guid, $"GUID_{index}");
                
                if (guid == null || guid == Guid.Empty)
                {
                    result.ShouldNotBeNull($"Expected error for GUID at index {index}");
                }
                else
                {
                    result.ShouldBeNull($"Expected no error for GUID at index {index}");
                }
            }
        }
    }

    #endregion Edge Cases and Integration Tests
}
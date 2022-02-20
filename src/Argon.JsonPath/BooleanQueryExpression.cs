﻿using System.Text.RegularExpressions;
using Argon;

class BooleanQueryExpression : QueryExpression
{
    public readonly object Left;
    public readonly object? Right;

    public BooleanQueryExpression(QueryOperator @operator, object left, object? right) : base(@operator)
    {
        Left = left;
        Right = right;
    }

    static IEnumerable<JToken> GetResult(JToken root, JToken t, object? o)
    {
        if (o is JToken resultToken)
        {
            return new[] { resultToken };
        }

        if (o is List<PathFilter> pathFilters)
        {
            return JPath.Evaluate(pathFilters, root, t, JTokenExtensions.DefaultSettings);
        }

        return CollectionUtils.ArrayEmpty<JToken>();
    }

    public override bool IsMatch(JToken root, JToken t, JsonSelectSettings settings)
    {
        if (Operator == QueryOperator.Exists)
        {
            return GetResult(root, t, Left).Any();
        }

        using var leftResults = GetResult(root, t, Left).GetEnumerator();
        if (leftResults.MoveNext())
        {
            var rightResultsEn = GetResult(root, t, Right);
            var rightResults = rightResultsEn as ICollection<JToken> ?? rightResultsEn.ToList();

            do
            {
                var leftResult = leftResults.Current;
                foreach (var rightResult in rightResults)
                {
                    if (MatchTokens(leftResult, rightResult, settings))
                    {
                        return true;
                    }
                }
            } while (leftResults.MoveNext());
        }

        return false;
    }

    bool MatchTokens(JToken leftResult, JToken rightResult, JsonSelectSettings settings)
    {
        if (leftResult is JValue leftValue && rightResult is JValue rightValue)
        {
            switch (Operator)
            {
                case QueryOperator.RegexEquals:
                    if (RegexEquals(leftValue, rightValue, settings))
                    {
                        return true;
                    }
                    break;
                case QueryOperator.Equals:
                    if (EqualsWithStringCoercion(leftValue, rightValue))
                    {
                        return true;
                    }
                    break;
                case QueryOperator.StrictEquals:
                    if (EqualsWithStrictMatch(leftValue, rightValue))
                    {
                        return true;
                    }
                    break;
                case QueryOperator.NotEquals:
                    if (!EqualsWithStringCoercion(leftValue, rightValue))
                    {
                        return true;
                    }
                    break;
                case QueryOperator.StrictNotEquals:
                    if (!EqualsWithStrictMatch(leftValue, rightValue))
                    {
                        return true;
                    }
                    break;
                case QueryOperator.GreaterThan:
                    if (leftValue.CompareTo(rightValue) > 0)
                    {
                        return true;
                    }
                    break;
                case QueryOperator.GreaterThanOrEquals:
                    if (leftValue.CompareTo(rightValue) >= 0)
                    {
                        return true;
                    }
                    break;
                case QueryOperator.LessThan:
                    if (leftValue.CompareTo(rightValue) < 0)
                    {
                        return true;
                    }
                    break;
                case QueryOperator.LessThanOrEquals:
                    if (leftValue.CompareTo(rightValue) <= 0)
                    {
                        return true;
                    }
                    break;
                case QueryOperator.Exists:
                    return true;
            }
        }
        else
        {
            switch (Operator)
            {
                case QueryOperator.Exists:
                // you can only specify primitive types in a comparison
                // notequals will always be true
                case QueryOperator.NotEquals:
                    return true;
            }
        }

        return false;
    }

    static bool RegexEquals(JValue input, JValue pattern, JsonSelectSettings settings)
    {
        if (input.Type != JTokenType.String || pattern.Type != JTokenType.String)
        {
            return false;
        }

        var regexText = (string)pattern.Value!;
        var patternOptionDelimiterIndex = regexText.LastIndexOf('/');

        var patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
        var optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

        var timeout = settings.RegexMatchTimeout ?? Regex.InfiniteMatchTimeout;
        return Regex.IsMatch((string)input.Value!, patternText, MiscellaneousUtils.GetRegexOptions(optionsText), timeout);
    }

    internal static bool EqualsWithStringCoercion(JValue value, JValue queryValue)
    {
        if (value.Equals(queryValue))
        {
            return true;
        }

        // Handle comparing an integer with a float
        // e.g. Comparing 1 and 1.0
        if ((value.Type == JTokenType.Integer && queryValue.Type == JTokenType.Float)
            || (value.Type == JTokenType.Float && queryValue.Type == JTokenType.Integer))
        {
            return JValue.Compare(value.Type, value.Value, queryValue.Value) == 0;
        }

        if (queryValue.Type != JTokenType.String)
        {
            return false;
        }

        var queryValueString = (string)queryValue.Value!;

        string currentValueString;

        // potential performance issue with converting every value to string?
        switch (value.Type)
        {
            case JTokenType.Date:
                using (var writer = StringUtils.CreateStringWriter(64))
                {
                    if (value.Value is DateTimeOffset offset)
                    {
                        DateTimeUtils.WriteDateTimeOffsetString(writer, offset, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTimeUtils.WriteDateTimeString(writer, (DateTime)value.Value!, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                    }

                    currentValueString = writer.ToString();
                }
                break;
            case JTokenType.Bytes:
                currentValueString = Convert.ToBase64String((byte[])value.Value!);
                break;
            case JTokenType.Guid:
            case JTokenType.TimeSpan:
                currentValueString = value.Value!.ToString();
                break;
            case JTokenType.Uri:
                currentValueString = ((Uri)value.Value!).OriginalString;
                break;
            default:
                return false;
        }

        return string.Equals(currentValueString, queryValueString, StringComparison.Ordinal);
    }

    internal static bool EqualsWithStrictMatch(JValue value, JValue queryValue)
    {
        MiscellaneousUtils.Assert(value != null);
        MiscellaneousUtils.Assert(queryValue != null);

        // Handle comparing an integer with a float
        // e.g. Comparing 1 and 1.0
        if ((value.Type == JTokenType.Integer && queryValue.Type == JTokenType.Float)
            || (value.Type == JTokenType.Float && queryValue.Type == JTokenType.Integer))
        {
            return JValue.Compare(value.Type, value.Value, queryValue.Value) == 0;
        }

        // we handle floats and integers the exact same way, so they are pseudo equivalent
        return value.Type == queryValue.Type &&
               value.Equals(queryValue);
    }
}
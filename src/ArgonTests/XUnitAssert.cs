﻿public class XUnitAssert
{
    public static void AreEqual(double expected, double actual, double r) =>
        Assert.Equal(expected, actual, 5); // hack

    public static void AreEqualNormalized(string expected, string actual)
    {
        expected = Normalize(expected);
        actual = Normalize(actual);

        Assert.Equal(expected, actual);
    }

    public static bool EqualsNormalized(string s1, string s2)
    {
        s1 = Normalize(s1);
        s2 = Normalize(s2);

        return string.Equals(s1, s2);
    }

    public static string Normalize(string s) =>
        s
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

    public static TException Throws<TException>(Action action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            action();

            Assert.Fail($"Exception of type {typeof(TException).Name} expected. No exception thrown.");
            return null;
        }
        catch (TException exception)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return exception;
            }

            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, exception.Message))
                {
                    return exception;
                }
            }

            throw new($"""
                       Unexpected exception message.
                       Expected one of:
                        * {string.Join(Environment.NewLine + " * ", possibleMessages)}
                       Got: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception}
                       """);
        }
        catch (Exception exception)
        {
            throw new($"Exception of type {typeof(TException).Name} expected; got exception of type {exception.GetType().Name}.", exception);
        }
    }

    public static void Throws<TException>(Action action, string messages)
        where TException : Exception
    {
        var exception = Assert.Throws<TException>(action);
        Assert.Equal(messages, exception.Message);
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            await action();

            Assert.Fail($"Exception of type {typeof(TException).Name} expected. No exception thrown.");
            return null;
        }
        catch (TException exception)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return exception;
            }

            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, exception.Message))
                {
                    return exception;
                }
            }

            throw new($"Unexpected exception message.{Environment.NewLine}Expected one of: {string.Join(Environment.NewLine, possibleMessages)}{Environment.NewLine}Got: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception}");
        }
        catch (Exception exception)
        {
            throw new($"Exception of type {typeof(TException).Name} expected; got exception of type {exception.GetType().Name}.", exception);
        }
    }
}
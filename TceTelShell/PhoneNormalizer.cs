using System;
using PhoneNumbers;

namespace TceTelShell;

public static class PhoneNormalizer
{
    private const string DefaultRegion = "US";

    public static string ToE164(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Phone number is empty.");

        var util = PhoneNumberUtil.GetInstance();
        var parsed = util.Parse(input.Trim(), DefaultRegion);

        if (!util.IsPossibleNumber(parsed) || !util.IsValidNumber(parsed))
            throw new InvalidOperationException($"Invalid phone number: {input}");

        return util.Format(parsed, PhoneNumberFormat.E164);
    }
}
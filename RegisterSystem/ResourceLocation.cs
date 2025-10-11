using System;

namespace RegisterSystem;

public class ResourceLocation : IComparable<ResourceLocation> {

    public string domain { get; private set; }

    public string path { get; private set; }

    private readonly string cachedToString;
    private readonly int cachedHashCode;

    public ResourceLocation(string domain, string path) {
        validateString(domain, nameof(domain));
        validateString(path, nameof(path));

        this.domain = domain;
        this.path = path;

        cachedToString = $"{domain}:{path}";
        cachedHashCode = HashCode.Combine(domain, path);
    }

    public ResourceLocation(string location) {
        if (string.IsNullOrEmpty(location)) {
            throw new ArgumentException("Location string cannot be null or empty", nameof(location));
        }

        int colonIndex = location.IndexOf(':');
        if (colonIndex == -1) {
            throw new ArgumentException("Location string must contain a colon separator", nameof(location));
        }
        if (colonIndex == 0 || colonIndex == location.Length - 1) {
            throw new ArgumentException("Domain and path cannot be empty", nameof(location));
        }

        domain = location.Substring(0, colonIndex);
        path = location.Substring(colonIndex + 1);

        validateString(domain, "domain");
        validateString(path, "path");

        cachedToString = $"{domain}:{path}";
        cachedHashCode = HashCode.Combine(domain, path);
    }

    private static void validateString(string value, string parameterName) {
        if (string.IsNullOrEmpty(value)) {
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
        }

        foreach (char c in value) {
            if (!isValidChar(c)) {
                throw new ArgumentException($"{parameterName} contains invalid character '{c}'. Only letters, numbers, '.', '/', '_', '-' are allowed", parameterName);
            }
        }
    }

    private static bool isValidChar(char c) {
        return char.IsLetterOrDigit(c) || c == '.' || c == '/' || c == '_' || c == '-';
    }

    public override string ToString() => cachedToString;

    public override int GetHashCode() => cachedHashCode;

    public override bool Equals(object? obj) {
        if (obj is not ResourceLocation other) {
            return false;
        }
        if (cachedHashCode != other.cachedHashCode) {
            return false;
        }
        return Equals(domain, other.domain) && Equals(path, other.path);
    }

    public int CompareTo(ResourceLocation? other) {
        if (other == null) return 1;
            
        int domainComparison = string.Compare(domain, other.domain, StringComparison.Ordinal);
        if (domainComparison != 0) {
            return domainComparison;
        }
            
        return string.Compare(path, other.path, StringComparison.Ordinal);
    }

    public static implicit operator string(ResourceLocation location) {
        return location.ToString();
    }

}
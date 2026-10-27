using Crm.Domain.Exceptions;
using Crm.Domain.Rules;

namespace Crm.Domain.Entities
{
    public sealed class PhoneNumberEntity
    {
        public Guid Id { get; private set; }
        public string Number { get; private set; } = null!;
        public string MatchKey { get; private set; } = null!;
        public string? Name { get; private set; }
        public string Source { get; private set; } = null!;
        public DateTimeOffset? CalledAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        // Derived, not stored: it can never contradict CalledAt
        public bool IsCalled => CalledAt is not null;

        private PhoneNumberEntity() { }   // for EF Core

        public static PhoneNumberEntity Create(string number, string? name, string source, DateTimeOffset now)
        {
            var phone = new PhoneNumberEntity
            {
                Id = Guid.CreateVersion7(),
                CreatedAt = now.ToUniversalTime()
            };
            phone.SetDetails(number, name, source);
            return phone;
        }

        public void Edit(string number, string? name, string source) =>
            SetDetails(number, name, source);

        public void MarkCalled(DateTimeOffset now) =>
            CalledAt = now.ToUniversalTime();

        public void MarkNotCalled() =>
            CalledAt = null;

        private void SetDetails(string number, string? name, string source)
        {
            // Validate everything first, so a failed Edit leaves the entity unchanged
            if (string.IsNullOrWhiteSpace(number))
                throw new DomainException("Phone number is required.");

            var parsed = PhoneNumberRules.Parse(number)
                ?? throw new DomainException(
                    $"'{number}' is not a valid phone number. Use digits only, with an optional leading '+' or '00'. " +
                    "Georgian numbers need 9 digits; international numbers need 7 to 15 digits after the prefix.");

            if (string.IsNullOrWhiteSpace(source))
                throw new DomainException("Source is required.");

            // All valid: now apply
            Number = parsed.Stored;
            MatchKey = parsed.MatchKey;
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            Source = source.Trim();
        }
    }
}

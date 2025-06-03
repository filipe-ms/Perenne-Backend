using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace perenne.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this Enum value)
        {
            if (value == null) return string.Empty;

            var member = value.GetType()
                              .GetMember(value.ToString())
                              .FirstOrDefault();

            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }

        public static TEnum FromDisplayName<TEnum>(string displayName) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Novo cargo não pode ser vazio.", nameof(displayName));

            var normalizedInput = displayName.Trim().ToLowerInvariant();

            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                var displayNameNormalized = displayAttr?.Name?.Trim().ToLowerInvariant();

                if (displayNameNormalized == normalizedInput)
                    return (TEnum)field.GetValue(null);

                if (field.Name.ToLowerInvariant() == normalizedInput)
                    return (TEnum)field.GetValue(null);
            }

            throw new ArgumentException(
                $"Valor '{displayName}' não corresponde a nenhum DisplayName ou nome de enum em {typeof(TEnum).Name}.",
                nameof(displayName)
            );
        }

    }
}

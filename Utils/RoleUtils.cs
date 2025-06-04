using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace perenne.Utils
{
    public static class RoleUtils
    {
        public static string EnumToName(this Enum value)
        {
            if (value == null) return string.Empty;

            var member = value.GetType()
                              .GetMember(value.ToString())
                              .FirstOrDefault();

            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }

        public static TEnum NameToEnum<TEnum>(string displayName) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("O nome para conversão não pode ser vazio.", nameof(displayName));
            }

            var normalizedInput = displayName.Trim().ToLowerInvariant();

            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                // Verifica o DisplayAttribute
                var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr?.Name?.Trim().ToLowerInvariant() == normalizedInput)
                {
                    return (TEnum)field.GetValue(null);
                }

                // Verifica o nome do próprio membro do enum
                if (field.Name.ToLowerInvariant() == normalizedInput)
                {
                    return (TEnum)field.GetValue(null);
                }
            }

            return default(TEnum);
        }
    }
}

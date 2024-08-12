namespace RagChatLogic.Extension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Static class to handle shared logic.
    /// </summary>
    public static class ExtensionLogic
    {
        /// <summary>
        /// Verifies that object is not null and its properties are not null or empty.
        /// </summary>
        /// <param name="obj">Object instance to verify.</param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this object obj)
        {
            if (obj == null)
                return false;

            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (value == null)
                {
                    return false;
                }

                var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), true).FirstOrDefault() as RequiredAttribute;
                if (requiredAttribute == null)
                {
                    continue;
                }

                var propertyType = property.PropertyType;
                if (propertyType == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)value))
                        return false;
                }
                else if (propertyType == typeof(int))
                {
                    if ((int)value == 0)
                        return false;
                }
                else if (propertyType == typeof(long))
                {
                    if ((int)value == 0)
                        return false;
                }
            }

            return true;
        }
    }
}

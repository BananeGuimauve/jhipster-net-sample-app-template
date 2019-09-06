using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace JHipsterNetSampleApplication.Test.Web.Rest {
    public static class TestUtil {
        private static readonly Random random = new Random();

        public static HttpContent ToJsonContent(object model)
        {
            return ToJsonContent(model, Encoding.UTF8);
        }

        public static HttpContent ToJsonContent(object model, Encoding encoding)
        {
            return new StringContent(JsonConvert.SerializeObject(model), encoding, "application/json");
        }

        public static string RandomAlphabetic(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void EqualsVerifier(Type type)
        {
            var domainObject1 = Activator.CreateInstance(type);
            domainObject1.ToString().Should().NotBeNullOrEmpty();
            domainObject1.Should().Be(domainObject1);
            domainObject1.GetHashCode().Should().Be(domainObject1.GetHashCode());
            // Test with an instance another class
            var testOtherObject = new object();
            domainObject1.Should().NotBe(testOtherObject);
            domainObject1.Should().NotBeNull();
            // Test with an instance of the same class
            var domainObject2 = Activator.CreateInstance(type);
            domainObject1.Should().NotBe(domainObject2);
        }

        public static void BuildHttpContextWithMockUser(string username)
        {
//            var mock = new Mock<HttpContext>();
//            mock.Setup(httpContext => httpContext.User).Returns(null);
        }

        /// <summary>
        /// Compare entity instances of a DbContext using Reflection.
        /// Precondition : objects must be of the same type.
        /// Returns true if the objects are the same, false otherwise.
        /// </summary>
        /// <param name="inputObjectA"></param>
        /// <param name="inputObjectB"></param>
        /// <param name="ignorePropertiesList"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CompareObjects(object inputObjectA, object inputObjectB, HashSet<string> ignorePropertiesList, DbContext context)
        {
            bool areObjectsEqual = true;

            if (inputObjectA != null && inputObjectB != null) {
                object value1, value2;
                HashSet<string> ignorePropertiesListForChildren = TestUtil.GetNavigationProperties(context, inputObjectA.GetType());
                PropertyInfo[] properties = inputObjectA.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo propertyInfo in properties) {
                    if (!propertyInfo.CanRead || ignorePropertiesList.Contains(propertyInfo.Name))
                        continue;

                    value1 = propertyInfo.GetValue(inputObjectA);
                    value2 = propertyInfo.GetValue(inputObjectB);

                    if (IsAssignableFrom(propertyInfo.PropertyType) || IsPrimitiveType(propertyInfo.PropertyType) || IsValueType(propertyInfo.PropertyType)) {
                        if (!CompareValues(value1, value2)) {
                            areObjectsEqual = false;
                        }
                    }
                    else if (IsEnumerableType(propertyInfo.PropertyType)) {
                        HashSet<string> ignorePropertiesListUnion = new HashSet<string>(ignorePropertiesList);
                        ignorePropertiesListUnion.UnionWith(ignorePropertiesListForChildren);
                        if (!CompareEnumerations(value1, value2, ignorePropertiesListUnion, context)) {
                            areObjectsEqual = false;
                        }
                    }
                    else if (propertyInfo.PropertyType.IsClass) {
                        HashSet<string> ignorePropertiesListUnion = new HashSet<string>(ignorePropertiesList);
                        ignorePropertiesListUnion.UnionWith(ignorePropertiesListForChildren);
                        if (!CompareObjects(propertyInfo.GetValue(inputObjectA), propertyInfo.GetValue(inputObjectB), ignorePropertiesListUnion, context)) {
                            areObjectsEqual = false;
                        }
                    }
                    else {
                        areObjectsEqual = false;
                    }

                    if (!areObjectsEqual)
                        break;
                }
            }
            else if (inputObjectA == null && inputObjectB != null || inputObjectA != null && inputObjectB == null)
                areObjectsEqual = false;

            return areObjectsEqual;
        }

        private static bool IsAssignableFrom(Type type)
        {
            return typeof(IComparable).IsAssignableFrom(type);
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive;
        }

        private static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        private static bool IsEnumerableType(Type type)
        {
            return (typeof(IEnumerable).IsAssignableFrom(type));
        }
  
        private static bool CompareValues(object value1, object value2)
        {
            bool areValuesEqual = true;
            IComparable selfValueComparer = value1 as IComparable;

            if (value1 == null && value2 != null || value1 != null && value2 == null)
                areValuesEqual = false;
            else if (selfValueComparer != null && selfValueComparer.CompareTo(value2) != 0)
                areValuesEqual = false;
            else if (!object.Equals(value1, value2))
                areValuesEqual = false;

            return areValuesEqual;
        }

        private static bool CompareEnumerations(object value1, object value2, HashSet<string> ignorePropertiesList, DbContext context)
        {
            if (value1 == null && value2 != null || value1 != null && value2 == null)
                return false;
            else if (value1 != null && value2 != null) {
                IEnumerable<object> enumValue1, enumValue2;
                enumValue1 = ((IEnumerable)value1).Cast<object>();
                enumValue2 = ((IEnumerable)value2).Cast<object>();

                if (enumValue1.Count() != enumValue2.Count())
                    return false;
                else {
                    object enumValue1Item, enumValue2Item;
                    Type enumValue1ItemType;
                    for (int itemIndex = 0; itemIndex < enumValue1.Count(); itemIndex++) {
                        enumValue1Item = enumValue1.ElementAt(itemIndex);
                        enumValue2Item = enumValue2.ElementAt(itemIndex);
                        enumValue1ItemType = enumValue1Item.GetType();
                        if (IsAssignableFrom(enumValue1ItemType) || IsPrimitiveType(enumValue1ItemType) || IsValueType(enumValue1ItemType)) {
                            if (!CompareValues(enumValue1Item, enumValue2Item))
                                return false;
                        }
                        else if (!CompareObjects(enumValue1Item, enumValue2Item, ignorePropertiesList, context))
                            return false;
                    }
                }
            }
            return true;
        }

        public static HashSet<string> GetNavigationProperties(DbContext context, Type clrEntityType)
        {
            HashSet<string> navigationProperties = new HashSet<string>();
            var entityType = context.Model.FindEntityType(clrEntityType);

            if (entityType != null) {
                var navigations = entityType.GetNavigations();
                PropertyInfo[] properties = clrEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (INavigation nav in navigations) {
                    navigationProperties.Add(nav.PropertyInfo.Name);
                }

                foreach (PropertyInfo propertyInfo in properties) {
                    if (propertyInfo.GetCustomAttributes(typeof(NotMappedAttribute), true).Count() > 0) {
                        navigationProperties.Add(propertyInfo.Name);
                    }
                }
            }

            return navigationProperties;
        }
    }
}

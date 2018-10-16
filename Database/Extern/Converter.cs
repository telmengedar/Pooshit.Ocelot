using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace NightlyCode.Database.Extern {

    /// <summary>
    /// converter used to convert data types
    /// </summary>
    internal static class Converter {
        static readonly Dictionary<ConversionKey, Func<object, object>> specificconverters = new Dictionary<ConversionKey, Func<object, object>>();

        /// <summary>
        /// cctor
        /// </summary>
        static Converter() {
            specificconverters[new ConversionKey(typeof(double), typeof(string))] = o => ((double)o).ToString(CultureInfo.InvariantCulture);
            specificconverters[new ConversionKey(typeof(string), typeof(int))] = o => int.Parse((string)o);
            specificconverters[new ConversionKey(typeof(string), typeof(int[]))] = o => ((string)o).Split(';').Select(int.Parse).ToArray();
            specificconverters[new ConversionKey(typeof(long), typeof(TimeSpan))] = o => TimeSpan.FromTicks((long)o);
            specificconverters[new ConversionKey(typeof(TimeSpan), typeof(long))] = v => ((TimeSpan)v).Ticks;
            specificconverters[new ConversionKey(typeof(string), typeof(Type))] = o => Type.GetType((string)o);
            specificconverters[new ConversionKey(typeof(long), typeof(DateTime))] = v => new DateTime((long)v);
            specificconverters[new ConversionKey(typeof(DateTime), typeof(long))] = v => ((DateTime)v).Ticks;
            specificconverters[new ConversionKey(typeof(Version), typeof(string))] = o => o.ToString();
            specificconverters[new ConversionKey(typeof(string), typeof(Version))] = s => Version.Parse((string)s);
            specificconverters[new ConversionKey(typeof(string), typeof(TimeSpan))] = s => TimeSpan.Parse((string)s);
            specificconverters[new ConversionKey(typeof(long), typeof(Version))] = l => new Version((int)((long)l >> 48), (int)((long)l >> 32) & 65535, (int)((long)l >> 16) & 65535, (int)(long)l & 65535);
            specificconverters[new ConversionKey(typeof(Version), typeof(long))] = v => (long)((Version)v).Major << 48 | ((long)((Version)v).Minor << 32) | ((long)((Version)v).Build << 16) | (long)((Version)v).Revision;
            specificconverters[new ConversionKey(typeof(IntPtr), typeof(int))] = v => ((IntPtr)v).ToInt32();
            specificconverters[new ConversionKey(typeof(IntPtr), typeof(long))] = v => ((IntPtr)v).ToInt64();
            specificconverters[new ConversionKey(typeof(UIntPtr), typeof(int))] = v => ((UIntPtr)v).ToUInt32();
            specificconverters[new ConversionKey(typeof(UIntPtr), typeof(long))] = v => ((UIntPtr)v).ToUInt64();
            specificconverters[new ConversionKey(typeof(int), typeof(IntPtr))] = v => new IntPtr((int)v);
            specificconverters[new ConversionKey(typeof(long), typeof(IntPtr))] = v => new IntPtr((long)v);
            specificconverters[new ConversionKey(typeof(int), typeof(UIntPtr))] = v => new UIntPtr((uint)v);
            specificconverters[new ConversionKey(typeof(long), typeof(UIntPtr))] = v => new UIntPtr((ulong)v);
            specificconverters[new ConversionKey(typeof(string), typeof(bool))] = v => ((string)v).ToLower() == "true" || ((string)v != "" && (string)v != "0");
            specificconverters[new ConversionKey(typeof(string), typeof(byte[]))] = v => System.Convert.FromBase64String((string)v);
            specificconverters[new ConversionKey(typeof(byte[]), typeof(Guid))] = v => new Guid((byte[])v);
            specificconverters[new ConversionKey(typeof(Guid), typeof(byte[]))] = v => ((Guid)v).ToByteArray();
            specificconverters[new ConversionKey(typeof(string), typeof(Guid))] = v => Guid.Parse((string) v);
        }

        /// <summary>
        /// registers a specific converter to be used for a specific conversion
        /// </summary>
        /// <param name="key"></param>
        /// <param name="converter"></param>
        public static void RegisterConverter(ConversionKey key, Func<object, object> converter) {
            specificconverters[key] = converter;
        }

        /// <summary>
        /// converts the value to a specific target type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targettype"></param>
        /// <param name="allownullonvaluetypes"> </param>
        /// <returns></returns>
        public static object Convert(object value, Type targettype, bool allownullonvaluetypes=false) {
            if(value == null || value is DBNull) {

#if WINDOWS_UWP
                if(targettype.GetTypeInfo().IsValueType && !(targettype.GetTypeInfo().IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>))) {
#else
                if (targettype.IsValueType && !(targettype.IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>))) {
#endif
                    if(allownullonvaluetypes)
                        return Activator.CreateInstance(targettype);
                    throw new InvalidOperationException("Unable to convert null to a value type");
                }
                return null;
            }

#if WINDOWS_UWP
            if (value.GetType() == targettype || value.GetType().GetTypeInfo().IsSubclassOf(targettype))
#else
            if (value.GetType() == targettype || value.GetType().IsSubclassOf(targettype))
#endif
                return value;

#if WINDOWS_UWP
            if (targettype.GetTypeInfo().IsEnum)
            {
#else
            if (targettype.IsEnum) {
#endif
                Type valuetype;
                if(value is string) {
                    if(((string)value).Length == 0) {
                        if(allownullonvaluetypes)
                            return null;
                        throw new ArgumentException("Empty string is invalid for an enum type");
                    }

                    if(((string)value).All(char.IsDigit)) {
                        valuetype = Enum.GetUnderlyingType(targettype);
                        return Convert(value, valuetype, allownullonvaluetypes);                        
                    }
                    return Enum.Parse(targettype, (string)value, true);
                }
                valuetype = Enum.GetUnderlyingType(targettype);
                return Enum.ToObject(targettype, Convert(value, valuetype, allownullonvaluetypes));
            }

            ConversionKey key = new ConversionKey(value.GetType(), targettype);
            Func<object, object> specificconverter;
            if(specificconverters.TryGetValue(key, out specificconverter))
                return specificconverter(value);


#if WINDOWS_UWP
            if(targettype.GetTypeInfo().IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                // the value is never null at this point
                value=Convert(value, targettype.GetGenericArguments()[0]);
                return Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(targettype.GetGenericArguments()[0]), value);
#else
            if (targettype.IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                // the value is never null at this point
                return new NullableConverter(targettype).ConvertFrom(Convert(value, targettype.GetGenericArguments()[0], true));
#endif
            }

            if(targettype == typeof(string))
                return value.ToString();

            return System.Convert.ChangeType(value, targettype, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// converts the value to the specified target type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="allownullonvaluetypes"> </param>
        /// <returns></returns>
        public static T Convert<T>(object value, bool allownullonvaluetypes=false) {
            return (T)Convert(value, typeof(T), allownullonvaluetypes);
        }
    }
}
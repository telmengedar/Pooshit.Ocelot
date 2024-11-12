using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Pooshit.Ocelot.Extern;

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
        specificconverters[new ConversionKey(typeof(TimeSpan), typeof(int))] = v => (int)((TimeSpan)v).Ticks;
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
        specificconverters[new ConversionKey(typeof(string), typeof(BigInteger))] = v => BigInteger.Parse((string) v);
        specificconverters[new ConversionKey(typeof(long), typeof(BigInteger))] = v => new BigInteger((long) v);
        specificconverters[new ConversionKey(typeof(ulong), typeof(BigInteger))] = v => new BigInteger((ulong) v);
        specificconverters[new ConversionKey(typeof(int), typeof(BigInteger))] = v => new BigInteger((int) v);
        specificconverters[new ConversionKey(typeof(uint), typeof(BigInteger))] = v => new BigInteger((uint) v);
        specificconverters[new ConversionKey(typeof(float), typeof(BigInteger))] = v => new BigInteger((float) v);
        specificconverters[new ConversionKey(typeof(double), typeof(BigInteger))] = v => new BigInteger((double) v);
        specificconverters[new ConversionKey(typeof(decimal), typeof(BigInteger))] = v => new BigInteger((decimal) v);
        specificconverters[new ConversionKey(typeof(byte[]), typeof(BigInteger))] = v => new BigInteger((byte[]) v);
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(string))] = v => ((BigInteger)v).ToString();
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(long))] = v => (long)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(ulong))] = v => (ulong)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(int))] = v => (int)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(uint))] = v => (uint)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(float))] = v => (float)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(double))] = v => (double)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(decimal))] = v => (decimal)(BigInteger)v;
        specificconverters[new ConversionKey(typeof(BigInteger), typeof(byte[]))] = v => ((BigInteger)v).ToByteArray();
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
        if(specificconverters.TryGetValue(key, out Func<object, object> specificconverter))
            return specificconverter(value);


        if (targettype.IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>)) {
            // the value is never null at this point
            return new NullableConverter(targettype).ConvertFrom(Convert(value, targettype.GetGenericArguments()[0], true));
        }

        if(targettype == typeof(string))
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);

        try {
            return System.Convert.ChangeType(value, targettype, CultureInfo.InvariantCulture);
        }
        catch (Exception e) {
            throw new($"Unable to convert '{value}' to '{targettype}'", e);
        }
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
using System;

namespace StardewModdingAPI
{
    /// <summary>Simplifies access to private game code.</summary>
    public interface IReflectionHelper
    {
        IReflectedField<TValue>    GetField<TValue>   (object obj,   string name, bool required = true);
        IReflectedField<TValue>    GetField<TValue>   (Type   type,  string name, bool required = true);
        IReflectedProperty<TValue> GetProperty<TValue>(object obj,   string name, bool required = true);
        IReflectedProperty<TValue> GetProperty<TValue>(Type   type,  string name, bool required = true);
        IReflectedMethod           GetMethod          (object obj,   string name, bool required = true);
        IReflectedMethod           GetMethod          (Type   type,  string name, bool required = true);
    }

    public interface IReflectedField<TValue>
    {
        string   FieldName { get; }
        TValue   GetValue();
        void     SetValue(TValue value);
    }

    public interface IReflectedProperty<TValue>
    {
        string   PropertyName { get; }
        TValue   GetValue();
        void     SetValue(TValue value);
    }

    public interface IReflectedMethod
    {
        string   MethodName { get; }
        TReturn  Invoke<TReturn>(params object[] args);
        void     Invoke(params object[] args);
    }
}

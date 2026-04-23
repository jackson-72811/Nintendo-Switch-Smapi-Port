using System;
using System.Reflection;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchReflectionHelper : IReflectionHelper
    {
        private readonly IMonitor _monitor;
        public SwitchReflectionHelper(IMonitor monitor) { _monitor = monitor; }

        public IReflectedField<TValue> GetField<TValue>(object obj, string name, bool required = true) {
            var field = obj?.GetType().GetField(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null && required)
                throw new InvalidOperationException($"Field '{name}' not found on {obj?.GetType()}.");
            return field != null ? new ReflectedField<TValue>(field, obj) : null;
        }

        public IReflectedField<TValue> GetField<TValue>(Type type, string name, bool required = true) {
            var field = type.GetField(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null && required)
                throw new InvalidOperationException($"Static field '{name}' not found on {type}.");
            return field != null ? new ReflectedField<TValue>(field, null) : null;
        }

        public IReflectedProperty<TValue> GetProperty<TValue>(object obj, string name, bool required = true) {
            var prop = obj?.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null && required)
                throw new InvalidOperationException($"Property '{name}' not found on {obj?.GetType()}.");
            return prop != null ? new ReflectedProperty<TValue>(prop, obj) : null;
        }

        public IReflectedProperty<TValue> GetProperty<TValue>(Type type, string name, bool required = true) {
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null && required)
                throw new InvalidOperationException($"Static property '{name}' not found on {type}.");
            return prop != null ? new ReflectedProperty<TValue>(prop, null) : null;
        }

        public IReflectedMethod GetMethod(object obj, string name, bool required = true) {
            var method = obj?.GetType().GetMethod(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null && required)
                throw new InvalidOperationException($"Method '{name}' not found on {obj?.GetType()}.");
            return method != null ? new ReflectedMethod(method, obj) : null;
        }

        public IReflectedMethod GetMethod(Type type, string name, bool required = true) {
            var method = type.GetMethod(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null && required)
                throw new InvalidOperationException($"Static method '{name}' not found on {type}.");
            return method != null ? new ReflectedMethod(method, null) : null;
        }
    }

    internal sealed class ReflectedField<TValue> : IReflectedField<TValue>
    {
        private readonly FieldInfo _field;
        private readonly object    _target;
        public string FieldName => _field.Name;
        public ReflectedField(FieldInfo field, object target) { _field = field; _target = target; }
        public TValue GetValue() => (TValue)_field.GetValue(_target);
        public void   SetValue(TValue value) => _field.SetValue(_target, value);
    }

    internal sealed class ReflectedProperty<TValue> : IReflectedProperty<TValue>
    {
        private readonly PropertyInfo _prop;
        private readonly object       _target;
        public string PropertyName => _prop.Name;
        public ReflectedProperty(PropertyInfo prop, object target) { _prop = prop; _target = target; }
        public TValue GetValue() => (TValue)_prop.GetValue(_target);
        public void   SetValue(TValue value) => _prop.SetValue(_target, value);
    }

    internal sealed class ReflectedMethod : IReflectedMethod
    {
        private readonly MethodInfo _method;
        private readonly object     _target;
        public string MethodName => _method.Name;
        public ReflectedMethod(MethodInfo method, object target) { _method = method; _target = target; }
        public TReturn Invoke<TReturn>(params object[] args) => (TReturn)_method.Invoke(_target, args);
        public void    Invoke(params object[] args) => _method.Invoke(_target, args);
    }
}

using System;

namespace CFisobs
{
    public struct LazyEnum<T> where T : struct, Enum
    {
        public LazyEnum(string name)
        {
            Name = name;
            value = null;
        }

        public string Name { get; }

        private T? value;

        public T Value {
            get {
                if (Name == null) {
                    throw new InvalidOperationException($"LazyEnum<{typeof(T).FullName}>.Value was called but Name is null.");
                }
                if (value == null) {
                    value = RWCustom.Custom.ParseEnum<T>(Name);
                }
                return value ?? throw new InvalidOperationException($"LazyEnum<{typeof(T).FullName}>.Value was called but {Name} hasn't been registered yet.");
            }
        }

        public static implicit operator T(LazyEnum<T> lazyEnum) => lazyEnum.Value;
        public static implicit operator LazyEnum<T>(T value) => new LazyEnum<T>(value.ToString());

        public override string ToString()
        {
            return Name;
        }
    }
}

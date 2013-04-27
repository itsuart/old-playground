using System;

namespace Lib.Monads
{
    public class Maybe<T>
    {
        public readonly bool HasValue;
        private readonly T _value;

        public Maybe(T item)
        {
            _value = item;
            HasValue = true;
        } 

        public Maybe()
        {
            _value = default(T);
            HasValue = false;
        }

        public T Value
        {
            get
            {
                if (!HasValue) throw new InvalidOperationException("No value available");
                return _value;
            }
        }

        public static readonly Maybe<T> Nothing = new Maybe<T>();
    }
}

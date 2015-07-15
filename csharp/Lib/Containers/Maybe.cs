using System;

namespace Lib.Monads
{
    public struct Maybe<T> : IFunctor<T>
    {
        public readonly bool HasValue;

        public readonly bool IsNothing;

        private readonly T _value;

        private Maybe(T item)
        {
            _value = item;
            HasValue = true;
            IsNothing = false;
        } 

        private Maybe(object stub1, object stub2)
        {
            _value = default(T);
            HasValue = false;
            IsNothing = true;
        }

        public T Value
        {
            get
            {
                if (!HasValue) throw new InvalidOperationException("No value available");
                return _value;
            }
        }

        IFunctor<b> IFunctor<T>.Fmap<b>(Func<T, b> f)
        {
            return Fmap(f);
        } 

        public Maybe<T2> Fmap<T2>(Func<T, T2> f)
        {
            if (f == null) throw new ArgumentNullException("f");

            if (HasValue)
            {
                return new Maybe<T2>(f(Value));
            }
            else
            {
                return new Maybe<T2>(null, null);
            }
        } 

        public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            if (HasValue) return f(Value);
            return Maybe<T2>.Nothing;
        }

        public void Run(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (HasValue) action(_value);
        }

        public static readonly Maybe<T> Nothing = new Maybe<T>(null, null);

        public static Maybe<T> Just(T t)
        {
            return new Maybe<T>(t);
        } 
    }
}

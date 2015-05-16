﻿using System;

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

        public Maybe<T2> Fmap<T2>(Func<T, T2> f)
        {
            if (f == null) throw new ArgumentNullException("f");

            if (HasValue)
            {
                return new Maybe<T2>(f(Value));
            }
            else
            {
                return new Maybe<T2>();   
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

        public static readonly Maybe<T> Nothing = new Maybe<T>();
    }
}

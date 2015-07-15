using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Monads;

namespace Lib.Containers
{
    public class AbortableComputation<T>
    {
        private readonly Func<bool> _shouldAbort;
        private readonly Func<Maybe<T>> _value;

        public AbortableComputation(Func<bool> shouldAbort, Func<T> value)
        {
            if (shouldAbort == null) throw new ArgumentNullException("shouldAbort");
            if (value == null) throw new ArgumentNullException("value");

            _shouldAbort = shouldAbort;
            _value = () => Run(value);
        }

        private AbortableComputation(Func<bool> shouldAbort, Func<Maybe<T>> value)
        {
            _shouldAbort = shouldAbort;
            _value = value;
        }  

        private AbortableComputation<T2> Make<T2>(Func<Maybe<T2>> f)
        {
            return new AbortableComputation<T2>(_shouldAbort, f);
        } 

        public AbortableComputation<T2> Fmap<T2>(Func<T, T2> f)
        {
            return Make(() => Run().Bind(x => Run(() => f(x))));
        }

        public AbortableComputation<T2> Seq<T2>(AbortableComputation<T2> next)
        {
            if (next == null) throw new ArgumentNullException("next");
            return Make(() => Run().Bind(_ => next.Run()));
        }

        public AbortableComputation<T2> Bind<T2>(Func<T, AbortableComputation<T2>> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return Make(() => Run().Bind(x => f(x).Run()));
        } 

        private Maybe<Tx> Run<Tx>(Func<Tx> f)
        {
            if (_shouldAbort()) return Maybe<Tx>.Nothing;
            return Maybe<Tx>.Just(f());
        } 

        public Maybe<T> Run()
        {
            return _value();
        }
 
        public static AbortableComputation<T> Seed(Func<bool> shouldAbort, T value)
        {
            return new AbortableComputation<T>(shouldAbort, () => value);
        } 
    }
}

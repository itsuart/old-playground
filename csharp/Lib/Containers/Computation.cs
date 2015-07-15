using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Monads;

namespace Lib.Containers
{
    public class Computation<T>
    {
        private readonly Func<bool> _shouldAbort;
        private readonly Action _pause;
        private readonly Func<Maybe<T>> _value;

        public Computation(Func<bool> shouldAbort, Action pause, Func<T> value)
        {
            if (shouldAbort == null) throw new ArgumentNullException("shouldAbort");
            if (pause == null) throw new ArgumentNullException("pause");
            if (value == null) throw new ArgumentNullException("value");

            _shouldAbort = shouldAbort;
            _value = () => Run(value);
            _pause = pause;
        }

        private Computation(Func<bool> shouldAbort, Action pause, Func<Maybe<T>> value)
        {
            _shouldAbort = shouldAbort;
            _value = value;
            _pause = pause;
        }

        private Computation<T2> Make<T2>(Func<Maybe<T2>> f)
        {
            return new Computation<T2>(_shouldAbort, _pause, f);
        }

        public Computation<T2> Fmap<T2>(Func<T, T2> f)
        {
            return Make(() => Run().Bind(x => Run(() => f(x))));
        }

        public Computation<T2> Seq<T2>(Computation<T2> next)
        {
            if (next == null) throw new ArgumentNullException("next");
            return Make(() => Run().Bind(_ => next.Run()));
        }

        public Computation<T2> Bind<T2>(Func<T, Computation<T2>> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return Make(() => Run().Bind(x => f(x).Run()));
        }

        private Maybe<Tx> Run<Tx>(Func<Tx> f)
        {
            if (_shouldAbort()) return Maybe<Tx>.Nothing;
            _pause();
            if (_shouldAbort()) return Maybe<Tx>.Nothing;
            return Maybe<Tx>.Just(f());
        }

        public Maybe<T> Run()
        {
            return _value();
        }

        public static Computation<T> Seed(Func<bool> shouldAbort, Action pause, T value)
        {
            return new Computation<T>(shouldAbort, pause, () => value);
        }
    }
}

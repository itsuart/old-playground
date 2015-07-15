using System;

namespace Lib.Containers
{
    public class FuncChain<T>
    {
        private readonly Func<T> _value;


        public FuncChain(Func<T> value)
        {
            if (value == null) throw new ArgumentNullException("value");
            _value = value;
        } 

        public FuncChain<T2> Fmap<T2>(Func<T, T2> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return new FuncChain<T2>(() =>
                {
                    var forcedValue = Run();
                    return f(forcedValue);
                });
        }
 
        public FuncChain<T2> Seq<T2>(FuncChain<T2> next)
        {
            if (next == null) throw new ArgumentNullException();
            return new FuncChain<T2>(() =>
                {
                    RunIgnoringResult();
                    return next.Run();
                });
        } 

        public FuncChain<T2> Bind<T2>(Func<T, FuncChain<T2>> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return new FuncChain<T2>(() =>
                {
                    var forcedValue = Run();
                    return f(forcedValue).Run();
                });
        } 

        public T Run()
        {
            return _value();
        }

        public void RunIgnoringResult()
        {
            _value();
        }

        public static FuncChain<T> Wrap(T value)
        {
            return new FuncChain<T>(() => value);
        } 
    }
}

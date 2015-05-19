using System;

namespace Lib.Containers
{
    public class Either<TLeft,TRight>
    {
        private readonly TLeft _left;
        private readonly TRight _right;
        public readonly bool IsLeft;

        public TLeft LeftValue
        {
            get
            {
                if (IsLeft) return _left;
                throw new InvalidOperationException("No Left Vlaue");
            }
        }

        public TRight RightValue
        {
            get
            {
                if (IsLeft) throw new InvalidOperationException("No Right Value");
                return _right;
            }
        }

        private Either(bool isLeft, TLeft left, TRight right)
        {
            _left = left;
            _right = right;
            IsLeft = isLeft;
        }
 
        public static Either<TLeft, TRight> Left(TLeft value)
        {
            return new Either<TLeft, TRight>(true, value, default(TRight));
        }
 
        public static Either<TLeft, TRight> Right(TRight value)
        {
            return new Either<TLeft, TRight>(false, default(TLeft), value);
        }
 
        public Either<TLeft, TRight> Fmap(Func<TRight, TRight> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            if (IsLeft) return this;
            return Right(f(RightValue));
        } 

        public Either<TLeft, TRight2> Bind<TRight2>(Func<TRight, Either<TLeft, TRight2>> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            if (IsLeft) return Either<TLeft, TRight2>.Left(LeftValue);
            return f(RightValue);
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Monads
{
    public interface IFunctor<a>
    {
        IFunctor<b> Fmap<b>(Func<a, b> f);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ISourceGroup<T>: IEnumerable where T : IPoint
    {
        public T[] Sources { get; }

        public int SourceAmount { get; }
    }
}

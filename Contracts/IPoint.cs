using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IPoint
    {
        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double Rho { get; }

        public double Phi { get; }

        public double Theta { get; }
    }
}

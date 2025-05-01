using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DAL.Exceptions
{
    public class SimulatedException : Exception
    {
        public SimulatedException(string message) : base(message)
        {

        }
    }
}

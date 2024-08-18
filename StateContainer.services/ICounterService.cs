using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateContainer.services
{
    public interface ICounterService
    {
        public int incrementValue(int sum, int increment);
    }
}

using DemoApp.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Services
{
    public interface IDatabaseService : IDatabase
    {
        T GetDatabase<T>();
    }
}

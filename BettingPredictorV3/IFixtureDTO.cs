using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public interface IDatabaseFixture
    {
        Fixture AddToDatabase(Database database);
    }
}

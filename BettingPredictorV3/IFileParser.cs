using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    interface IFileParser
    {
        void ParseFiles(Database database, Splash splash);
        Database PopulateDatabase(Database database, Splash splash);
    }
}

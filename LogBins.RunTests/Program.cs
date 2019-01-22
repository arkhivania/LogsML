using LogBins.Base;
using LogBins.ZipBuckets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace LogBins.Import
{
    class Program
    {
        static int Main(string[] args)
        {   
            return new NUnitLite
                .AutoRun(typeof(LogBins.Tests.Chk).Assembly)
                .Execute(args);
        }
    }
}

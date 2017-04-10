using System.Collections.Generic;
using System.IO;

namespace TestTask
{
    internal interface IConsumer
    {
        void BeginConsume();

        Queue<HierarchicalLink<FileSystemInfo>> GetEntriesQueue();
    }
}

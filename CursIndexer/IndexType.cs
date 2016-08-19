using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursIndexer
{ 
    public class TPositions : SortedSet<ushort> { } 
    public class TPostings : SortedDictionary<int, int> { } 
    public class TIndex : Dictionary<int, TPostings> { }
    public class TDocumentTF : Dictionary<int, float> { }
    public class TCacheTF : Dictionary<int, TDocumentTF> { }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreDBSchema
{
    public class TableInfo
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public override string ToString()
        {
            return "SchemaName: " + SchemaName + "   TableName: " + TableName;
        }
    }
}

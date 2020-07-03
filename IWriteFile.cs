using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DtoGenerate
{
    /// <summary>
    /// ファイル書き込みインターフェース
    /// </summary>
    interface IWriteFile
    {
        void Write(StreamWriter sw, WriteItem item);
    }
}

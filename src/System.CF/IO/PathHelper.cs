using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace System.IO
{
    public class PathHelper
    {
        public static string GetFullPath(string fileName) 
        {
            if (Path.IsPathRooted(fileName))
                return fileName;

            var codebase = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            var directory = Path.GetDirectoryName(codebase).Replace(@"file:\", String.Empty);

            return Path.Combine(directory, fileName);
        }

    }
}

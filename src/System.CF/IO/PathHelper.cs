using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace System.IO
{
    public class PathHelper
    {
        public static string GetFullPath(params string[] fileNameParts)
        {
            Assumes.NotNullOrEmpty(fileNameParts);

            if (fileNameParts.Length == 1)
                return GetFullPathFromFileName(fileNameParts[0]);

            var fileName = String.Empty;
            foreach (var fileNamePart in fileNameParts)
            {
                fileName = Path.Combine(fileName, fileNamePart);
            }
            return GetFullPathFromFileName(fileName);
        }

        private static string GetFullPathFromFileName(string fileName)
        {
            if (Path.IsPathRooted(fileName))
                return fileName;

            var codebase = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            var directory = Path.GetDirectoryName(codebase).Replace(@"file:\", String.Empty);

            return Path.Combine(directory, fileName);
        }

    }
}

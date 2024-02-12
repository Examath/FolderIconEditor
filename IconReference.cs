using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderIconEditor
{
    public class IconReference
    {
        /// <summary>
        /// The URL location of the icon
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the name of this icon
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// gets the name of the subfolder
        /// </summary>
        public string? Category { get; private set; }

        public IconReference(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            string? dir = System.IO.Path.GetDirectoryName(path);
            if (dir != null) Category = new DirectoryInfo(dir).Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

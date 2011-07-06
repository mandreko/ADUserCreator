using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADUserCreator
{
    /// <summary>
    /// 
    /// </summary>
    public class Group : IComparable<Group>
    {
        private string _name;
        private string _path;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public Group(string name, string path)
        {
            _path = path;
            _name = name;
        }

        public override string ToString()
        {
            return string.Format("Name: {0} Path: {1}", _name, _path);
        }

        public int CompareTo(Group other)
        {
            return Name.CompareTo(other.Name);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADUserCreator
{
    /// <summary>
    /// Holds a Login, Password, and Email address for a specified user.
    /// </summary>
    class Login
    {
		#region Constructors (1) 

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        public Login()
        {
            Password = RandomPassword.Generate(10);
            Action = UserAction.None;
        }

		#endregion Constructors 

		#region Properties (4) 

        public UserAction Action { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string Log { get; set; }

		#endregion Properties 
    }

    /// <summary>
    /// Used to compare 2 <see cref="Login"/> objects
    /// </summary>
    class LoginComparer : IEqualityComparer<Login>
    {
        // Logins are equal if their names are equal.
        public bool Equals(Login x, Login y)
        {

            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the logins' properties are equal.
            return x.UserName == y.UserName;
        }

        // If Equals() returns true for a pair of objects,
        // GetHashCode must return the same value for these objects.

        public int GetHashCode(Login login)
        {
            // Check whether the object is null.
            if (Object.ReferenceEquals(login, null)) return 0;

            // Get the hash code for the Name field if it is not null.
            int hashUserName = login.UserName == null ? 0 : login.UserName.GetHashCode();

            // Calculate the hash code for the product.
            return hashUserName;
        }

    }

    /// <summary>
    /// The type of action that will be done with this Login
    /// </summary>
    enum UserAction
    {
        /// <summary>
        /// No action
        /// </summary>
        None,
        /// <summary>
        /// Add the user to Active Directory
        /// </summary>
        Add,
        /// <summary>
        /// Remove this user from Active Directory
        /// </summary>
        Remove
    }
}

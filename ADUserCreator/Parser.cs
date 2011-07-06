using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using LinqToExcel;

namespace ADUserCreator
{
    /// <summary>
    /// Parser class handles all the reading of the file
    /// </summary>
    class Parser
    {
		#region Fields (7) 

        private string _domain;
        private string _emailColumn;
        private string _filePath;
        private string _groupPath;
        private string _userColumn;
        private string _worksheetName;

		#endregion Fields 

		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="worksheetName">Name of the worksheet.</param>
        /// <param name="usernameColumn">The username column.</param>
        /// <param name="emailColumn">The email column.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="group">The group.</param>
        public Parser(string filePath, string worksheetName, string usernameColumn, string emailColumn, string domain, string groupPath)
        {
            _filePath = filePath;
            _worksheetName = worksheetName;
            _userColumn = usernameColumn;
            _emailColumn = emailColumn;
            _domain = domain;
            _groupPath = groupPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public Parser(string filePath)
        {
            _filePath = filePath;
        }

		#endregion Constructors 

		#region Methods (4) 

		// Internal Methods (4) 

        /// <summary>
        /// Gets the AD group users.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        internal ArrayList GetADGroupUsers(string groupName) 
        { 
            SearchResult result; 
            DirectorySearcher search = new DirectorySearcher(); 
            search.Filter = String.Format("(cn={0})", groupName); search.PropertiesToLoad.Add("member"); 
            result = search.FindOne(); 
            ArrayList userNames = new ArrayList(); 
            if (result != null) 
            { 
                for (int counter = 0; counter < result.Properties["member"].Count; counter++) 
                { 
                    string user = (string)result.Properties["member"][counter];
                    userNames.Add(user); 
                } 
            } 
            return userNames; 
        }

        /// <summary>
        /// Parses the file.
        /// </summary>
        /// <returns></returns>
        internal List<Login> ParseFile()
        {
            IExcelRepository<Login> repo = new ExcelRepository<Login>(_filePath);
            repo.AddMapping(x => x.UserName, _userColumn);
            repo.AddMapping(x => x.EmailAddress, _emailColumn);

            var Logins = from p in repo.Worksheet(_worksheetName)
                         select p;

            return Logins.ToList<Login>();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        internal void Run()
        {
            // Parse the file
            List<Login> logins = ParseFile();

            // Get a list of users from AD
            List<Login> loginsInAD = GetLoginsInGroupFromAD();

            // consolidate the lists into 1 list (logins) and set their actions according to what needs done
            logins.Except(loginsInAD, new LoginComparer()).ToList().ForEach(p => p.Action = UserAction.Add);
            loginsInAD.Except(logins, new LoginComparer()).ToList().ForEach(p => p.Action = UserAction.Remove);
            logins.AddRange(loginsInAD.Where(p=>p.Action == UserAction.Remove));
            
            // Do actions on users
            foreach(Login login in logins.Where(l=>l.Action == UserAction.Add))
            {
                AddUser(login);
            }

            foreach (Login login in logins.Where(l => l.Action == UserAction.Remove))
            {
                DeleteUser(login);
            }

            foreach (Login login in logins.Where(l => l.Action == UserAction.None))
            {
                DoNothing(login);
            }

            // Report actions to screen
            Report report = new Report();
            report.DataSource = logins;
            report.ShowDialog();

        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="login">The login.</param>
        private void DoNothing(Login login)
        {
            // even though nothing is happening, fill out the log to show a success
            login.Log = "No action was performed on this login.";
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="login">The login.</param>
        private void DeleteUser(Login login)
        {
            try
            {
                string connectionPrefix = "LDAP://CN=" + login.UserName + ",CN=Users";

                foreach (string dc in _domain.Split('.'))
                {
                    connectionPrefix += ",DC=" + dc;
                }

                DirectoryEntry directoryObject = new DirectoryEntry(connectionPrefix);

                directoryObject.DeleteTree();

                directoryObject.CommitChanges();

                login.Log = "Login successfully deleted.";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine(e.Message);

                login.Log = string.Format("Login failed to be deleted. Error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Adds the user.
        /// </summary>
        /// <param name="login">The login.</param>
        private void AddUser(Login login)
        {
            try
            {
                string guid = string.Empty;
                string connectionPrefix = "LDAP://CN=Users";

                foreach (string dc in _domain.Split('.'))
                {
                    connectionPrefix += ",DC=" + dc;
                }

                DirectoryEntry directoryObject = new DirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = directoryObject.Children.Add("CN=" + login.UserName, "user");

                newUser.Properties["samAccountName"].Value = login.UserName;
                newUser.Properties["userPrincipalName"].Value = login.EmailAddress;
                newUser.CommitChanges();

                int val = (int)newUser.Properties["userAccountControl"].Value;
                newUser.Properties["userAccountControl"].Value = val & ~0x2;

                newUser.CommitChanges();
                guid = newUser.Guid.ToString();

                DirectoryEntry groupObject = new DirectoryEntry(_groupPath);
                groupObject.Properties["member"].Add(newUser.Path.Replace("LDAP://", ""));
                groupObject.CommitChanges();
                
                newUser.Invoke("SetPassword", new object[] { login.Password });
                newUser.CommitChanges();
                directoryObject.Close();
                newUser.Close();
                groupObject.Close();

                login.Log = "Login successfully added.";
                
            }
            catch(DirectoryServicesCOMException e)
            {
                Console.WriteLine(e.Message);

                login.Log = string.Format("Login failed to be added. Error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        internal bool ValidateUser(string username)
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://" + _domain);
            DirectorySearcher deSearch = new DirectorySearcher();

            deSearch.SearchRoot = de;
            deSearch.Filter = "(&(objectClass=user) (cn=" + username + "))";

            SearchResultCollection results = deSearch.FindAll();

            return results.Count > 0;
        }

        internal List<Login> GetLoginsInGroupFromAD()
        {
            List<Login> loginsInAD = new List<Login>();

            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + _domain);
                DirectorySearcher searcher = new DirectorySearcher(directoryObject);
                searcher.Filter = string.Format("(memberOf={0})", _groupPath.Replace("LDAP://" + _domain + "/", string.Empty));

                foreach (SearchResult result in searcher.FindAll())
                {
                    ArrayList groupMemberships = new ArrayList();
                    string Name = result.GetDirectoryEntry().Name;

                    loginsInAD.Add(new Login() { UserName = result.GetDirectoryEntry().Name.Replace("CN=", string.Empty) });

                }

                directoryObject.Close();
                directoryObject.Dispose();

            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message.ToString());
            }


            return loginsInAD;
        }

        public ArrayList AttributeValuesMultiString(string attributeName, string objectDn, ArrayList valuesCollection, bool recursive)
        {
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            PropertyValueCollection ValueCollection = ent.Properties[attributeName];
            IEnumerator en = ValueCollection.GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current != null)
                {
                    if (!valuesCollection.Contains(en.Current.ToString()))
                    {
                        valuesCollection.Add(en.Current.ToString());
                        if (recursive)
                        {
                            AttributeValuesMultiString(attributeName, "LDAP://" +
                            en.Current.ToString(), valuesCollection, true);
                        }
                    }
                }
            }
            ent.Close();
            ent.Dispose();
            return valuesCollection;
        }

		#endregion Methods 
    }
}

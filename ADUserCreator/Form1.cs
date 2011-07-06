using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.DirectoryServices;

namespace ADUserCreator
{
    /// <summary>
    /// Main user form
    /// </summary>
    public partial class Form1 : Form
    {
		#region Constructors (1) 

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

		#endregion Constructors 

		#region Methods (7) 

		/// <summary>
        /// Enumerates the domains.
        /// </summary>
        /// <returns></returns>
        public static ArrayList EnumerateDomains()
        {
            ArrayList alDomains = new ArrayList();
            Forest currentForest = Forest.GetCurrentForest();
            DomainCollection myDomains = currentForest.Domains;

            foreach (Domain objDomain in myDomains)
            {
                alDomains.Add(objDomain.Name);
            }
            return alDomains;
        }

        /// <summary>
        /// Enumerates the groups.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
        public static List<Group> EnumerateGroups(string domain)
        {
            List<Group> groups = new List<Group>();

            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + domain);
                DirectorySearcher searcher = new DirectorySearcher(directoryObject);
                searcher.Filter = "(ObjectCategory=group)";

                foreach (SearchResult result in searcher.FindAll())
                {
                    groups.Add(new Group(result.GetDirectoryEntry().Name.Replace("CN=", string.Empty), result.GetDirectoryEntry().Path));
                }

                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message.ToString());
            }

            groups.Sort();

            return groups;
        }
		
        /// <summary>
        /// Handles the Click event of the btnBrowse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshDomains control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnRefreshDomains_Click(object sender, EventArgs e)
        {
            cboDomains.DataSource = EnumerateDomains();
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnRefreshGroups_Click(object sender, EventArgs e)
        {
            cboGroups.DataSource = EnumerateGroups((string)cboDomains.SelectedValue);
            cboGroups.DisplayMember = "Name";
            cboGroups.ValueMember = "Path";
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Parser parser = new Parser(txtFileName.Text, txtWorksheetName.Text, txtUsernameColumn.Text, txtEmailColumn.Text, cboDomains.SelectedItem.ToString(), cboGroups.SelectedValue.ToString());
            
            parser.Run();
            
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // load the configuration items if they're available.  
            txtFileName.Text = ConfigurationManager.AppSettings["FilePath"];
            txtWorksheetName.Text = ConfigurationManager.AppSettings["WorksheetName"];
            txtUsernameColumn.Text = ConfigurationManager.AppSettings["UsernameColumn"];
            txtEmailColumn.Text = ConfigurationManager.AppSettings["EmailColumn"];
        }

		#endregion Methods 
    }

    
}

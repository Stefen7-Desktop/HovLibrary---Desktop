using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Library
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                new FormLogin().Show();
                this.Hide();
            }
        }

        private void memberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormMember member = new FormMember();
            member.Show();
        }

        private void bookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormMasterBook formMasterBook = new FormMasterBook();
            formMasterBook.Show();
        }

        private void newBorrowingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAddNewBorrowing formAddNewBorrowing = new FormAddNewBorrowing();
            formAddNewBorrowing.Show();
        }

        private void allBorowingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAllBorrowing formAllBorrowing = new FormAllBorrowing();
            formAllBorrowing.Show();
        }
    }

}

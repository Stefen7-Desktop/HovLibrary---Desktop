using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Library
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtEmail.Text == "")
            {
                MessageBox.Show("Email tidak boleh kosong!", "Kosong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text == "")
            {
                MessageBox.Show("Password tidak boleh kosong!","Kosong", MessageBoxButtons.OK ,MessageBoxIcon.Warning);
                return;
            }

            string email = txtEmail.Text;
            string password = txtPassword.Text;

           

            using (StefenDataContext db = new StefenDataContext())
            {
                var Employee = db.Employees.FirstOrDefault(u => u.email == email && u.password == password);

                if (Employee != null)
                {
                    MessageBox.Show("Login sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    new MainForm().Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Login gagal! Email atau password salah.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}

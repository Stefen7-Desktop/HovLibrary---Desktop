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
    public partial class FormMember : Form
    {
        private int editingMemberId = -1;
        public FormMember()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            StefenDataContext db = new StefenDataContext();

            var member = from b in db.Members
                         where b.deleted_at == null
                         select new
                         {
                             b.id,
                             b.name,
                             b.phone_number,
                             b.email,
                             b.city_of_birth,
                             b.date_of_birth,
                             b.address,
                             b.gender,
                         };

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("name", "Name");
            dataGridView1.Columns.Add("phone_number", "Phone");
            dataGridView1.Columns.Add("email", "Email");
            dataGridView1.Columns.Add("city_of_birth", "City of Birth");
            dataGridView1.Columns.Add("date_of_birth", "Date of Birth");
            dataGridView1.Columns.Add("address", "Address");
            dataGridView1.Columns.Add("gender", "Gender");

            DataGridViewButtonColumn edit = new DataGridViewButtonColumn();
            edit.Name = "Edit";
            edit.Text = "Edit";
            edit.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(edit);

            DataGridViewButtonColumn delete = new DataGridViewButtonColumn();
            delete.Name = "Delete";
            delete.Text = "Delete";
            delete.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(delete);

            foreach (var members in member)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = members.id;
                dataGridView1.Rows[index].Cells[1].Value = members.name;
                dataGridView1.Rows[index].Cells[2].Value = members.phone_number;
                dataGridView1.Rows[index].Cells[3].Value = members.email;
                dataGridView1.Rows[index].Cells[4].Value = members.city_of_birth;
                dataGridView1.Rows[index].Cells[5].Value = members.date_of_birth;
                dataGridView1.Rows[index].Cells[6].Value = members.address;
                dataGridView1.Rows[index].Cells[7].Value = members.gender;
            }
        }

        private void FormMember_Load(object sender, EventArgs e)
        {
            LoadData();
            ResetForm();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Edit"].Index && e.RowIndex >= 0)
            {
                editingMemberId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                txtName.Text = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString() ?? "";
                txtPhone.Text = dataGridView1.Rows[e.RowIndex].Cells["phone_number"].Value.ToString() ?? "";
                txtEmail.Text = dataGridView1.Rows[e.RowIndex].Cells["email"].Value.ToString() ?? "";
                txtAddress.Text = dataGridView1.Rows[e.RowIndex].Cells["address"].Value.ToString() ?? "";
                txtCity.Text = dataGridView1.Rows[e.RowIndex].Cells["city_of_birth"].Value.ToString() ?? "";
                dateBirth.Value = Convert.ToDateTime(dataGridView1.Rows[e.RowIndex].Cells["date_of_birth"].Value);
                string gender = dataGridView1.Rows[e.RowIndex].Cells["gender"].Value.ToString() ?? "";
                if (gender == "Male")
                    radioButton1.Checked = true;
                else
                    radioButton2.Checked = true;
            }

            else if (e.ColumnIndex == dataGridView1.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                int memberId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this member?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    StefenDataContext db = new StefenDataContext();
                    var member = db.Members.SingleOrDefault(m => m.id == memberId);

                    if (member != null)
                    {
                        var borrowings = db.Borrowings.Where(b => b.member_id == memberId).ToList();
                        db.Borrowings.DeleteAllOnSubmit(borrowings);

                        db.Members.DeleteOnSubmit(member);
                        db.SubmitChanges();
                        MessageBox.Show("Member deleted successfully.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Member not found.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (editingMemberId == -1)
            {
                MessageBox.Show("Please select a member to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StefenDataContext db = new StefenDataContext();
            var member = db.Members.SingleOrDefault(m => m.id == editingMemberId);

            if (member != null)
            {
                member.name = txtName.Text;
                member.phone_number = txtPhone.Text;
                member.email = txtEmail.Text;
                member.address = txtAddress.Text;
                member.city_of_birth = txtCity.Text;
                member.date_of_birth = dateBirth.Value;
                member.gender = radioButton1.Checked ? "Male" : "Female";

                db.SubmitChanges();
                MessageBox.Show("Member updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else
            {
                MessageBox.Show("Member not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            editingMemberId = -1;
            txtName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtCity.Text = "";
            dateBirth.Value = DateTime.Now;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
        }
    }
}

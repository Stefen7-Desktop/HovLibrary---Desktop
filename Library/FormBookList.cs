using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Library
{
    public partial class FormBookList : Form
    {
        private int selectedBookId;
        public FormBookList(string title, int bookId)
        {
            InitializeComponent();
            textBox1.Text = title;
            selectedBookId = bookId;
            LoadBooks(bookId);
        }

        private void LoadBooks(int bookId)
        {
            StefenDataContext db = new StefenDataContext();
            var bookDetails = from bd in db.BookDetails
                              join b in db.Books on bd.book_id equals b.id
                              join l in db.Locations on bd.location_id equals l.id
                              where bd.book_id == bookId && bd.deleted_at == null
                              select new
                              {
                                  bd.id,
                                  bd.code,
                                  location = l.name,
                                  status = db.Borrowings.Any(br => br.bookdetails_id == bd.id && br.return_date == null) ? "Unavailable" : "Available"
                              };

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("code", "Code");
            dataGridView1.Columns.Add("location", "Location");
            dataGridView1.Columns.Add("status", "Status");

            DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn();
            deleteButton.Name = "Delete";
            deleteButton.HeaderText = "Delete";
            deleteButton.Text = "Delete";
            deleteButton.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(deleteButton);

            foreach (var bookDetail in bookDetails)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells["id"].Value = bookDetail.id;
                dataGridView1.Rows[index].Cells["code"].Value = bookDetail.code;
                dataGridView1.Rows[index].Cells["location"].Value = bookDetail.location;
                dataGridView1.Rows[index].Cells["status"].Value = bookDetail.status;
            }
        }

        private void LoadLocations()
        {
            StefenDataContext db = new StefenDataContext();
            var locations = from l in db.Locations
                            where l.deleted_at == null
                            select l;

            comboBox1.DataSource = locations.ToList();
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "id";
        }

        private void FormBookList_Load(object sender, EventArgs e)
        {
            LoadLocations();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please select a location and enter a code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StefenDataContext db = new StefenDataContext();
            int locationId = ((Location)comboBox1.SelectedItem).id;
            string code = textBox2.Text; // Menggunakan kode yang dimasukkan di TextBox

            BookDetail newBookDetail = new BookDetail
            {
                book_id = selectedBookId,
                location_id = locationId,
                code = code,
                created_at = DateTime.Now
            };

            db.BookDetails.InsertOnSubmit(newBookDetail);

            try
            {
                db.SubmitChanges();
                MessageBox.Show("New book added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks(selectedBookId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log error or take additional action if needed
            }
        }

        private string GenerateBookCode(int bookDetailsId, int bookId, int locationId, DateTime publishDate)
        {
            return $"{bookDetailsId.ToString("D4")}.{bookId.ToString("D4")}.{locationId.ToString("D2")}.{publishDate.Year.ToString("D4")}";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                int bookDetailId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                string status = dataGridView1.Rows[e.RowIndex].Cells["status"].Value.ToString();

                if (status == "Unavailable")
                {
                    MessageBox.Show("Cannot delete an unavailable book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this book?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    StefenDataContext db = new StefenDataContext();
                    var bookDetail = db.BookDetails.SingleOrDefault(bd => bd.id == bookDetailId);

                    if (bookDetail != null)
                    {
                        // Check and delete related borrowings
                        var borrowings = db.Borrowings.Where(br => br.bookdetails_id == bookDetail.id).ToList();
                        if (borrowings.Any())
                        {
                            db.Borrowings.DeleteAllOnSubmit(borrowings);
                        }

                        db.BookDetails.DeleteOnSubmit(bookDetail);

                        try
                        {
                            db.SubmitChanges();
                            MessageBox.Show("Book deleted successfully.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadBooks(bookDetail.book_id);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Log error or take additional action if needed
                        }
                    }
                    else
                    {
                        MessageBox.Show("Book not found.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}

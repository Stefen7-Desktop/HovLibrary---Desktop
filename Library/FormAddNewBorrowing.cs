using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Library
{
    public partial class FormAddNewBorrowing : Form
    {
        public FormAddNewBorrowing()
        {
            InitializeComponent();
            LoadAutoCompleteData();
            InitializeDataGridView();

            // Set AutoComplete properties for textBox1 (Title)
            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Set AutoComplete properties for textBox2 (Member Name)
            textBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox2.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void LoadAutoCompleteData()
        {
            // Load AutoComplete data for book titles
            StefenDataContext db = new StefenDataContext();
            var bookTitles = db.Books.Select(b => b.title).Distinct().ToArray();
            var bookAutoComplete = new AutoCompleteStringCollection();
            bookAutoComplete.AddRange(bookTitles);
            textBox1.AutoCompleteCustomSource = bookAutoComplete;

            // Load AutoComplete data for member names
            var memberNames = db.Members.Select(m => m.name).Distinct().ToArray();
            var memberAutoComplete = new AutoCompleteStringCollection();
            memberAutoComplete.AddRange(memberNames);
            textBox2.AutoCompleteCustomSource = memberAutoComplete;
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("code", "Code");
            dataGridView1.Columns.Add("location", "Location");
            dataGridView1.Columns.Add("status", "Status");

            DataGridViewCheckBoxColumn selectColumn = new DataGridViewCheckBoxColumn();
            selectColumn.Name = "select";
            selectColumn.HeaderText = "Select";
            dataGridView1.Columns.Add(selectColumn);

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["select"].Index && e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Cells["select"].Value = !(Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells["select"].Value));
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadAvailableBooks(textBox1.Text);
        }

        private void LoadAvailableBooks(string title)
        {
            Console.WriteLine($"Loading available books for title: {title}"); // Debug log
            StefenDataContext db = new StefenDataContext();
            var availableBooks = from bd in db.BookDetails
                                 join b in db.Books on bd.book_id equals b.id
                                 join l in db.Locations on bd.location_id equals l.id
                                 where b.title == title && !db.Borrowings.Any(br => br.bookdetails_id == bd.id && br.return_date == null)
                                 select new
                                 {
                                     bd.id,
                                     bd.code,
                                     location = l.name,
                                     status = "Available"
                                 };

            Console.WriteLine($"Found {availableBooks.Count()} available books"); // Debug log

            dataGridView1.Rows.Clear();
            foreach (var book in availableBooks)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells["id"].Value = book.id;
                dataGridView1.Rows[index].Cells["code"].Value = book.code;
                dataGridView1.Rows[index].Cells["location"].Value = book.location;
                dataGridView1.Rows[index].Cells["status"].Value = book.status;
                dataGridView1.Rows[index].Cells["select"].Value = false;
                Console.WriteLine($"Added book {book.code} to DataGridView"); // Debug log
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please enter a member name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StefenDataContext db = new StefenDataContext();
            var member = db.Members.SingleOrDefault(m => m.name == textBox2.Text);
            if (member == null)
            {
                MessageBox.Show("Member not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedBooks = new List<int>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["select"].Value))
                {
                    selectedBooks.Add((int)row.Cells["id"].Value);
                }
            }

            if (selectedBooks.Count == 0)
            {
                MessageBox.Show("Please select at least one book to borrow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (int bookDetailId in selectedBooks)
            {
                Borrowing newBorrowing = new Borrowing
                {
                    member_id = member.id,
                    bookdetails_id = bookDetailId,
                    borrow_date = DateTime.Now,
                    created_at = DateTime.Now
                };
                db.Borrowings.InsertOnSubmit(newBorrowing);
            }

            db.SubmitChanges();
            MessageBox.Show("Books borrowed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void FormAddNewBorrowing_Load(object sender, EventArgs e)
        {
            InitializeDataGridView();
        }
    }
}

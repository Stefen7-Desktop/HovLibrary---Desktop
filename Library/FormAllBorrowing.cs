using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Library
{
    public partial class FormAllBorrowing : Form
    {
        public FormAllBorrowing()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadBorrowStatusOptions();
            LoadBorrowingData();
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("member_name", "Member Name");
            dataGridView1.Columns.Add("book_title", "Book Title");
            dataGridView1.Columns.Add("book_code", "Book Code");
            dataGridView1.Columns.Add("borrow_date", "Borrow Date");
            dataGridView1.Columns.Add("return_date", "Return Date");
            dataGridView1.Columns.Add("fine", "Fine");

            DataGridViewButtonColumn returnColumn = new DataGridViewButtonColumn();
            returnColumn.Name = "Return";
            returnColumn.HeaderText = "Return";
            returnColumn.Text = "Return";
            returnColumn.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(returnColumn);

            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
        }

        private void LoadBorrowStatusOptions()
        {
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Ongoing");
            comboBox1.Items.Add("Late");
            comboBox1.Items.Add("Returned");
            comboBox1.SelectedIndex = 0;
        }

        private void LoadBorrowingData()
        {
            StefenDataContext db = new StefenDataContext();
            var borrowings = from br in db.Borrowings
                             join m in db.Members on br.member_id equals m.id
                             join bd in db.BookDetails on br.bookdetails_id equals bd.id
                             join b in db.Books on bd.book_id equals b.id
                             select new BorrowingDetails
                             {
                                 Id = br.id,
                                 MemberName = m.name,
                                 BookTitle = b.title,
                                 BookCode = bd.code,
                                 BorrowDate = br.borrow_date,
                                 ReturnDate = br.return_date,
                                 Fine = br.return_date.HasValue ? (br.borrow_date - br.return_date.Value).Days * 1000 : 0
                             };

            if (comboBox1.SelectedItem != null)
            {
                ApplyFilters(ref borrowings);
            }

            dataGridView1.Rows.Clear();
            foreach (var borrowing in borrowings)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells["id"].Value = borrowing.Id;
                dataGridView1.Rows[index].Cells["member_name"].Value = borrowing.MemberName;
                dataGridView1.Rows[index].Cells["book_title"].Value = borrowing.BookTitle;
                dataGridView1.Rows[index].Cells["book_code"].Value = borrowing.BookCode;
                dataGridView1.Rows[index].Cells["borrow_date"].Value = borrowing.BorrowDate.ToString("yyyy-MM-dd");
                dataGridView1.Rows[index].Cells["return_date"].Value = borrowing.ReturnDate?.ToString("yyyy-MM-dd");
                dataGridView1.Rows[index].Cells["fine"].Value = borrowing.Fine;

                if (borrowing.ReturnDate.HasValue)
                {
                    dataGridView1.Rows[index].Cells["Return"].Value = null; // Hide the return button
                }
            }
        }

        private void ApplyFilters(ref IQueryable<BorrowingDetails> borrowings)
        {
            string selectedStatus = comboBox1.SelectedItem.ToString();
            DateTime dateFrom = dateTimePicker1.Value.Date;
            DateTime dateTo = dateTimePicker2.Value.Date;

            if (selectedStatus == "Ongoing")
            {
                borrowings = borrowings.Where(br => !br.ReturnDate.HasValue);
            }
            else if (selectedStatus == "Late")
            {
                borrowings = borrowings.Where(br => !br.ReturnDate.HasValue && (DateTime.Now - br.BorrowDate).Days > 7);
            }
            else if (selectedStatus == "Returned")
            {
                borrowings = borrowings.Where(br => br.ReturnDate.HasValue);
            }

            borrowings = borrowings.Where(br => br.BorrowDate >= dateFrom && br.BorrowDate <= dateTo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadBorrowingData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Return"].Index && e.RowIndex >= 0)
            {
                int borrowingId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                ReturnBook(borrowingId);
            }
        }

        private void ReturnBook(int borrowingId)
        {
            StefenDataContext db = new StefenDataContext();
            var borrowing = db.Borrowings.SingleOrDefault(br => br.id == borrowingId);
            if (borrowing != null)
            {
                borrowing.return_date = DateTime.Now;
                int fine = (DateTime.Now - borrowing.borrow_date).Days * 1000;
                db.SubmitChanges();

                MessageBox.Show("Book returned successfully!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBorrowingData();
            }
        }
    }

    public class BorrowingDetails
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public string BookCode { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int Fine { get; set; }
    }
}

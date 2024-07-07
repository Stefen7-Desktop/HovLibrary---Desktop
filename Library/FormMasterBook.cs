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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Xml.Linq;

namespace Library
{
    public partial class FormMasterBook : Form
    {
        private int selectedBookId;
        public FormMasterBook()
        {
            InitializeComponent();
        }

        private void LoadData(string searchBy = null, string keyword = null, string language = null, DateTime? publishDateFrom = null, DateTime? publishDateTo = null, int? pageCountFrom = null, int? pageCountTo = null, int? ratingsFrom = null, int? ratingsTo = null)
        {
            StefenDataContext db = new StefenDataContext();
            var bookQuery = from b in db.Books
                       join d in db.Languages
                       on b.language_id equals d.id
                       join s in db.Publishers
                       on b.publisher_id equals s.id
                       where b.deleted_at == null
                       select new
                       {
                           b.id,
                           d.long_text,
                           b.title,
                           b.isbn,
                           b.isbn13,
                           b.authors,
                           s.name,
                           b.publication_date,
                           b.number_of_pages,
                           b.ratings_count,
                       };

           

            if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower(); // Convert keyword to lower case for case-insensitive search
                switch (searchBy)
                {
                    case "Title":
                        bookQuery = bookQuery.Where(b => b.title.ToLower().Contains(keyword));
                        break;
                    case "Authors":
                        bookQuery = bookQuery.Where(b => b.authors.ToLower().Contains(keyword));
                        break;
                    case "Publisher":
                        bookQuery = bookQuery.Where(b => b.name.ToLower().Contains(keyword));
                        break;
                }
            }

            if (!string.IsNullOrEmpty(language))
            {
                bookQuery = bookQuery.Where(b => b.long_text == language);
            }

            if (publishDateFrom.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.publication_date >= publishDateFrom.Value);
            }

            if (publishDateTo.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.publication_date <= publishDateTo.Value);
            }

            if (pageCountFrom.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.number_of_pages >= pageCountFrom.Value);
            }

            if (pageCountTo.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.number_of_pages <= pageCountTo.Value);
            }

            if (ratingsFrom.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.ratings_count >= ratingsFrom.Value);
            }

            if (ratingsTo.HasValue)
            {
                bookQuery = bookQuery.Where(b => b.ratings_count <= ratingsTo.Value);
            }

            var bookss = bookQuery.ToList();

            


         
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("long_text", "Language");
            dataGridView1.Columns.Add("title", "Title");
            dataGridView1.Columns.Add("isbn", "ISBN");
            dataGridView1.Columns.Add("isbn13", "ISBN13");
            dataGridView1.Columns.Add("authors", "Authors");
            dataGridView1.Columns.Add("name", "Publisher");
            dataGridView1.Columns.Add("publication_date", "Publish Date");
            dataGridView1.Columns.Add("number_of_pages", "Page Count");
            dataGridView1.Columns.Add("ratings_count", "Ratings");

            DataGridViewButtonColumn show = new DataGridViewButtonColumn();
            show.Name = "Show";
            show.Text = "Show";
            show.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(show);

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

            foreach (var books in bookQuery)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = books.id;
                dataGridView1.Rows[index].Cells[1].Value = books.long_text;
                dataGridView1.Rows[index].Cells[2].Value = books.title;
                dataGridView1.Rows[index].Cells[3].Value = books.isbn;
                dataGridView1.Rows[index].Cells[4].Value = books.isbn13;
                dataGridView1.Rows[index].Cells[5].Value = books.authors;
                dataGridView1.Rows[index].Cells[6].Value = books.name;
                dataGridView1.Rows[index].Cells[7].Value = books.publication_date;
                dataGridView1.Rows[index].Cells[8].Value = books.number_of_pages;
                dataGridView1.Rows[index].Cells[9].Value = books.ratings_count;
            }
        }

        private void FormMasterBook_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Title");
            comboBox1.Items.Add("Authors");
            comboBox1.Items.Add("Publisher");

            StefenDataContext db = new StefenDataContext();
            var languages = from l in db.Languages
                            where l.deleted_at == null
                            select l.long_text;

            foreach (var language in languages)
            {
                comboBox2.Items.Add(language);
            }

            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string searchBy = comboBox1.SelectedItem?.ToString();
            string keyword = textBox1.Text;
            LoadData(searchBy, keyword);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string language = comboBox2.SelectedItem?.ToString();
            DateTime? publishDateFrom = dateTimePicker1.Value.Date;
            DateTime? publishDateTo = dateTimePicker2.Value.Date;
            int? pageCountFrom = string.IsNullOrEmpty(textBox2.Text) ? (int?)null : int.Parse(textBox2.Text);
            int? pageCountTo = string.IsNullOrEmpty(textBox3.Text) ? (int?)null : int.Parse(textBox3.Text);
            int? ratingsFrom = string.IsNullOrEmpty(textBox4.Text) ? (int?)null : int.Parse(textBox4.Text);
            int? ratingsTo = string.IsNullOrEmpty(textBox5.Text) ? (int?)null : int.Parse(textBox5.Text);

            LoadData(null, null, language, publishDateFrom, publishDateTo, pageCountFrom, pageCountTo, ratingsFrom, ratingsTo);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Edit"].Index && e.RowIndex >= 0)
            {
                selectedBookId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                textBox6.Text = dataGridView1.Rows[e.RowIndex].Cells["long_text"].Value.ToString() ?? "";
                textBox7.Text = dataGridView1.Rows[e.RowIndex].Cells["title"].Value.ToString() ?? "";
                textBox8.Text = dataGridView1.Rows[e.RowIndex].Cells["isbn"].Value.ToString() ?? "";
                textBox9.Text = dataGridView1.Rows[e.RowIndex].Cells["isbn13"].Value.ToString() ?? "";
                textBox10.Text = dataGridView1.Rows[e.RowIndex].Cells["authors"].Value.ToString() ?? "";
                textBox11.Text = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString() ?? "";
                dateTimePicker3.Value = Convert.ToDateTime(dataGridView1.Rows[e.RowIndex].Cells["publication_date"].Value);
                textBox12.Text = dataGridView1.Rows[e.RowIndex].Cells["number_of_pages"].Value.ToString() ?? "";
                textBox13.Text = dataGridView1.Rows[e.RowIndex].Cells["ratings_count"].Value.ToString() ?? "";

            }

            else if (e.ColumnIndex == dataGridView1.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                int bookId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this book?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    StefenDataContext db = new StefenDataContext();
                    var book = db.Books.SingleOrDefault(b => b.id == bookId);

                    if (book != null)
                    {
                        // Delete related BookDetails entries first
                        var bookDetails = db.BookDetails.Where(bd => bd.book_id == bookId).ToList();
                        db.BookDetails.DeleteAllOnSubmit(bookDetails);

                        // Delete related Borrowings entries
                        var borrowings = db.Borrowings.Where(b => bookDetails.Select(bd => bd.id).Contains(b.bookdetails_id)).ToList();
                        db.Borrowings.DeleteAllOnSubmit(borrowings);

                        // Now delete the book entry
                        db.Books.DeleteOnSubmit(book);
                        db.SubmitChanges();
                        MessageBox.Show("Book deleted successfully.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Book not found.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            else if (e.ColumnIndex == dataGridView1.Columns["Show"].Index && e.RowIndex >= 0)
            {
                string title = dataGridView1.Rows[e.RowIndex].Cells["title"].Value.ToString();
                int bookId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;

                FormBookList bookListForm = new FormBookList(title, bookId);
                bookListForm.ShowDialog();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox6.Text == "")
            {
                MessageBox.Show("Language is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox7.Text == "")
            {
                MessageBox.Show("Title is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox8.Text == "")
            {
                MessageBox.Show("ISBN is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox9.Text == "")
            {
                MessageBox.Show("ISBN13 is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox10.Text == "")
            {
                MessageBox.Show("Authors is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox11.Text == "")
            {
                MessageBox.Show("Publisher is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox12.Text == "")
            {
                MessageBox.Show("Number of pages is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox13.Text == "")
            {
                MessageBox.Show("Ratings count is still empty!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StefenDataContext db = new StefenDataContext();
            var book = db.Books.SingleOrDefault(b => b.id == selectedBookId);

            if (book != null)
            {
                book.language_id = db.Languages.SingleOrDefault(l => l.long_text == textBox6.Text)?.id ?? book.language_id;
                book.title = textBox7.Text;
                book.isbn = textBox8.Text;
                book.isbn13 = textBox9.Text;
                book.authors = textBox10.Text;
                book.publisher_id = db.Publishers.SingleOrDefault(p => p.name == textBox11.Text)?.id ?? book.publisher_id;
                book.publication_date = dateTimePicker3.Value;
                book.number_of_pages = int.Parse(textBox12.Text);
                book.ratings_count = int.Parse(textBox13.Text);

                db.SubmitChanges();
                MessageBox.Show("Book data updated successfully.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else
            {
                MessageBox.Show("Book not found.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

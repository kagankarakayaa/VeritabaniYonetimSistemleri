using System;
using System.Windows.Forms;
using Npgsql;

namespace veritabani
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnUpdate.Click += new EventHandler(btnUpdate_Click);
            btnDelete.Click += new EventHandler(btnDelete_Click);
            btnSearch.Click += new EventHandler(btnSearch_Click);

            dgvUsers.Columns.Add("kullaniciid", "Kullanıcı ID");
            dgvUsers.Columns.Add("adsoyad", "Ad Soyad");
            dgvUsers.Columns.Add("eposta", "E-posta");
        }


        private NpgsqlConnection GetDatabaseConnection()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=EtkinlikSistemi";
            return new NpgsqlConnection(connectionString);
        }

        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Ad, e-posta ve şifre boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                string query = "INSERT INTO kullanicilar (adsoyad, eposta, sifre) VALUES (@name, @email, @password)";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("password", password); 

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Kullanıcı başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                       
                        GetUsers();
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı eklenirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(txtUserId.Text, out userId) || string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtEmail.Text))
            {
                MessageBox.Show("Geçersiz kullanıcı ID veya boş alanlar.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string name = txtName.Text;
            string email = txtEmail.Text;

            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                string query = "UPDATE kullanicilar SET adsoyad = @name, eposta = @email WHERE kullaniciid = @id";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("id", userId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Kullanıcı başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GetUsers();  
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı güncellenirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int userId;

          
            if (!int.TryParse(txtUserId.Text, out userId))
            {
                MessageBox.Show("Geçersiz kullanıcı ID.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

          
            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM kullanicilar WHERE kullaniciid = @id";
                using (var cmd = new NpgsqlCommand(checkQuery, connection))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    int userExists = Convert.ToInt32(cmd.ExecuteScalar());

                    if (userExists == 0)
                    {
                        MessageBox.Show("Bu kullanıcı ID'si veritabanında bulunmamaktadır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            using (var connection = GetDatabaseConnection())
            {
                try
                {
                    connection.Open();
                   
                    string deleteQuery = "DELETE FROM kullanicilar WHERE kullaniciid = @id";
                    using (var cmd = new NpgsqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("id", userId);

                       
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Kullanıcı başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            GetUsers();  
                        }
                        else
                        {
                            MessageBox.Show("Kullanıcı silinirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                  
                    MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetUsers();
        }            

      
        private void GetUsers()
        {
          
            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                string query = "SELECT kullaniciid, adsoyad, eposta FROM kullanicilar";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dgvUsers.Rows.Clear(); 
                        while (reader.Read())
                        {
                           
                            dgvUsers.Rows.Add(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                        }
                    }
                }
            }
        }


    }
}

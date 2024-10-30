using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UserManagementApp
{
    public partial class MainForm : Form
    {
        const string UsersFilePath = "users.txt"; // Путь к файлу с пользователями

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnCreateUser_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Text;

            if (!IsValidLogin(login))
            {
                MessageBox.Show("Логин должен содержать только латинские буквы, хотя бы одну заглавную букву и одну цифру.");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов и хотя бы одну заглавную букву.");
                return;
            }

            string passwordHash = GetPasswordHash(password);

            User newUser = new User { Login = login, PasswordHash = passwordHash };

            try
            {
                using (StreamWriter writer = new StreamWriter(UsersFilePath, true))
                {
                    writer.WriteLine($"{newUser.Login} {newUser.PasswordHash}");
                }
                MessageBox.Show("Пользователь успешно создан.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}");
            }
        }

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            User[] users = LoadUsers(UsersFilePath);
            if (users == null)
            {
                MessageBox.Show("Не удалось загрузить пользователей из файла.");
                return;
            }

            string inputLogin = txtLogin.Text;
            string inputPassword = txtPassword.Text;
            string inputPasswordHash = GetPasswordHash(inputPassword);

            if (Authenticate(users, inputLogin, inputPasswordHash))
            {
                MessageBox.Show("Аутентификация успешна.");
            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль.");
            }
        }

        static User[] LoadUsers(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                List<User> users = new List<User>();
                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    users.Add(new User { Login = parts[0], PasswordHash = parts[1] });
                }
                return users.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
                return null;
            }
        }

        static bool IsValidLogin(string login)
        {
            string pattern = @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d]+$";
            return Regex.IsMatch(login, pattern);
        }

        static bool IsValidPassword(string password)
        {
            return password.Length >= 6 && Regex.IsMatch(password, @"[A-Z]");
        }

        static string GetPasswordHash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        static bool Authenticate(User[] users, string login, string passwordHash)
        {
            foreach (var user in users)
            {
                if (user != null && user.Login == login && user.PasswordHash == passwordHash)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class User
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
    }
}

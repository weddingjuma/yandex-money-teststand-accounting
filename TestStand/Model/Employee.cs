using SQLite;
using System;

namespace TestStand.Model
{
    public class Employee
    {
        /// <summary>
        /// Номер пропуска сотрудника
        /// </summary>
        [PrimaryKey]
        public string BadgeId { get; set; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// E-mail сотрудника
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// E-mail сотрудника
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Дата, когда сотрудник взял телефон
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// ссылка на фото сотрудника
        /// </summary>
        public string Photo { get; set; }
    }
}


using System;
using SQLite;
using TestStand.Enum;

namespace TestStand.Model
{
    /// <summary>
    /// Запись в истории
    /// </summary>
    [Table("History")]
    public class HistoryEntry
    {
        /// <summary>
        /// Бейдж сотрудника
        /// </summary>
        public string BadgeId { get; set; }

        /// <summary>
        /// Действие с девайсом
        /// </summary>
        public DeviceAction Action { get; set; }

        /// <summary>
        /// Id устройства
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
    }
}

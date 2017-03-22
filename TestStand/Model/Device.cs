using SQLite;
using System;
using TestStand.Enum;

namespace TestStand.Model
{
    public class Device
    {
        /// <summary>
        /// Id девайса
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Место девайса в шкафу
        /// </summary>
        public int PlaceNumber { get; set; }

        /// <summary>
        /// Модель девайса
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Семейство девайса: Windows/iOS/Android
        /// </summary>
        public DeviceFamily Family { get; set; }

        /// <summary>
        /// Версия ОС девайса
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// Тип девайса: телефон/планшет/носимое устройство
        /// </summary>
        public DeviceType Type { get; set; }

        /// <summary>
        /// Размер экрана
        /// </summary>
        public string ScreenSize { get; set; }

        /// <summary>
        /// Разрешение экрана девайса
        /// </summary>
        public string ScreenResolution { get; set; }

        /// <summary>
        /// Доступны ли на девайсе бесконтактные платежи
        /// </summary>
        public bool IsContactlessPaymentsSupported { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Номер пропуска сотрудника
        /// </summary>
        public string BadgeId { get; set; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// Дата, когда в последний раз взяли девайс
        /// </summary>
        public DateTime? TakenDate { get; set; }
    }
}


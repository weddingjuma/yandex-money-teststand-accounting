using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using TestStand.Enum;
using TestStand.Model;

namespace TestStand.Services
{
    public class HistoryService
    {
        private readonly SQLiteAsyncConnection _db;

        public HistoryService(SQLiteAsyncConnection connection)
        {
            _db = connection;
        }

        public async Task AddEntry(Device device, string employeeBadgeId, DeviceAction action)
        {
            var historyEntry = new HistoryEntry();
            historyEntry.BadgeId = employeeBadgeId;
            historyEntry.Action = action;
            historyEntry.DeviceId = device.Id;
            historyEntry.Date = DateTime.Now;

            await _db.InsertAsync(historyEntry);
        }

        public async Task<List<HistoryEntry>> GetHistory()
        {
            return await _db.Table<HistoryEntry>().ToListAsync();
        }

        public async Task<List<HistoryEntry>> GetHistoryByDevice(int deviceId)
        {
            return await _db.Table<HistoryEntry>().Where(d => d.DeviceId == deviceId).ToListAsync();
        }
        public async Task<List<Device>> GetHistoryByEmployee(string badgeId)
        {
            return await _db.Table<Device>().Where(d => d.BadgeId == badgeId).ToListAsync();
        }
    }
}
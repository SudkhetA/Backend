using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Services;

public abstract class ServiceBase<TModel, TSearch>(DataContext context)
    where TModel : class
    where TSearch : class
{
    public long UserId { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? UserAgent { get; set; }

    protected DataContext _context = context;

    public abstract Task<ReadResult> Read(TSearch search, int page, int pageSize);
    public abstract Task<List<TModel>> Create(List<TModel> entities);
    public abstract Task<List<TModel>> Update(List<TModel> entities);

    public async Task<int> Delete(List<long> ids)
    {
        if (ids.Count != 0)
        {
            var propertyInfo = typeof(TModel).GetProperty("Id");

            var data = _context.Set<TModel>()
                .Where(x => ids.Contains((long)propertyInfo!.GetValue(x)!))
                .ToList();

            if (data.Count != 0)
            {
                var transactionLogs = new List<TransactionLog>();
                foreach (var item in data)
                {
                    var transactionLog = new TransactionLog
                    {
                        TimeStamp = DateTime.Now,
                        UserId = UserId,
                        OperationType = EnumOperationType.Delete,
                        TableName = "system." + nameof(User),
                        RecordId = (long)propertyInfo!.GetValue(item)!,
                        OldData = JsonSerializer.Serialize(item),
                        IpAddress = RemoteIpAddress,
                        UserAgent = UserAgent,
                    };

                    transactionLogs.Add(transactionLog);
                }

                _context.Set<TModel>().RemoveRange(data);
                await _context.TransactionLogs.AddRangeAsync(transactionLogs);
                await _context.SaveChangesAsync();

                return data.Count;
            }
        }
        return 0;
    }

    public async Task TransactionLogRead(List<TModel> oldData, string tableName)
    {
        var propertyInfo = typeof(TModel).GetProperty("Id");
        var transactionLogs = new List<TransactionLog>();
        foreach (var item in oldData)
        {
            var transactionLog = new TransactionLog()
            {
                TimeStamp = DateTime.Now,
                UserId = UserId,
                OperationType = EnumOperationType.Read,
                TableName = tableName,
                RecordId = (long)propertyInfo!.GetValue(item)!,
                OldData = JsonSerializer.Serialize(item),
                IpAddress = RemoteIpAddress,
                UserAgent = UserAgent,
            };

            transactionLogs.Add(transactionLog);
        }

        await _context.TransactionLogs.AddRangeAsync(transactionLogs);
        await _context.SaveChangesAsync();
    }

    public async Task TransactionLogCreate(List<TModel> newData, string tableName)
    {
        var propertyInfo = typeof(TModel).GetProperty("Id");
        var transactionLogs = new List<TransactionLog>();
        foreach (var item in newData)
        {
            var transactionLog = new TransactionLog()
            {
                TimeStamp = DateTime.Now,
                UserId = UserId,
                OperationType = EnumOperationType.Create,
                TableName = tableName,
                RecordId = (long)propertyInfo!.GetValue(item)!,
                NewData = JsonSerializer.Serialize(item),
                IpAddress = RemoteIpAddress,
                UserAgent = UserAgent,
            };

            transactionLogs.Add(transactionLog);
        }

        await _context.TransactionLogs.AddRangeAsync(transactionLogs);
        await _context.SaveChangesAsync();
    }

    public async Task TransactionLogUpdate(List<TModel> oldData, List<TModel> newData, string tableName)
    {
        if (oldData.Count == newData.Count)
        {
            var propertyInfo = typeof(TModel).GetProperty("Id");
            var transactionLogs = new List<TransactionLog>();

            for (var index = 0; index < oldData.Count; index++)
            {
                var transactionLog = new TransactionLog()
                {
                    TimeStamp = DateTime.Now,
                    UserId = UserId,
                    OperationType = EnumOperationType.Update,
                    TableName = tableName,
                    RecordId = (long)propertyInfo!.GetValue(oldData[index])!,
                    OldData = JsonSerializer.Serialize(oldData[index]),
                    NewData = JsonSerializer.Serialize(newData[index]),
                    IpAddress = RemoteIpAddress,
                    UserAgent = UserAgent,
                };

                transactionLogs.Add(transactionLog);
            }

            await _context.TransactionLogs.AddRangeAsync(transactionLogs);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidDataException("oldData and newData different number of element");
        }
    }

    public async Task TransactionLogDelete(List<TModel> oldData, string tableName)
    {
        var propertyInfo = typeof(TModel).GetProperty("Id");
        var transactionLogs = new List<TransactionLog>();
        foreach (var item in oldData)
        {
            var transactionLog = new TransactionLog()
            {
                TimeStamp = DateTime.Now,
                UserId = UserId,
                OperationType = EnumOperationType.Delete,
                TableName = tableName,
                RecordId = (long)propertyInfo!.GetValue(item)!,
                OldData = JsonSerializer.Serialize(item),
                IpAddress = RemoteIpAddress,
                UserAgent = UserAgent,
            };

            transactionLogs.Add(transactionLog);
        }

        await _context.TransactionLogs.AddRangeAsync(transactionLogs);
        await _context.SaveChangesAsync();
    }

    public class ReadResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long Count { get; set; }
        public dynamic? Data { get; set; }
    }
}

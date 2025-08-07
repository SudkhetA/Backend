using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Services.System;

public class NotificationService(DataContext _context) : ServiceBase<Notification, NotificationSearch>(_context)
{
    public override Task<ReadResult> Read(NotificationSearch search, int page, int pageSize)
    {
        var query = _context.Notifications.AsQueryable();

        if (search.MemberId?.Length > 0)
        {
            query = query.Where(x => _context.MemberNotifications.Any(a => a.NotificationId == x.Id && search.MemberId.Contains(a.MemberId)));
        }

        query = query
            .Where(x => search.Id == null || search.Id.Length == 0 || search.Id.Contains(x.Id));

        var result = new ReadResult
        {
            Page = page,
            PageSize = pageSize,
            Count = query.Count(),
            Data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        return Task.FromResult(result);
    }

    public override async Task<List<Notification>> Create(List<Notification> entities)
    {
        if (entities.Count != 0)
        {
            foreach (var entity in entities)
            {
                entity.Id = 0;
                entity.CreatedBy = MemberId;
                entity.CreatedDate = DateTime.Now;
            }

            await _context.Notifications.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            _ = TransactionLogCreate(entities, "system." + nameof(Notification));
        }

        return entities;
    }

    public override async Task<List<Notification>> Update(List<Notification> entities)
    {
        if (entities.Count != 0)
        {
            var contain = entities.Select(x => x.Id);
            var data = await _context.Notifications
                .Where(x => contain.Contains(x.Id))
                .ToListAsync();

            var storage = new List<Notification>();
            foreach(var entity in entities)
            {
                var item = data.Find(x => x.Id == entity.Id);

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.Header))
                    {
                        item.Header = entity.Header;
                    }

                    if (!string.IsNullOrEmpty(item.Message))
                    {
                        item.Message = entity.Message;
                    }

                    if (!string.IsNullOrEmpty(item.LinkPage))
                    {
                        item.LinkPage = entity.LinkPage;
                    }

                    item.UpdatedBy = MemberId;
                    item.UpdatedDate = DateTime.Now;

                    storage.Add(item);
                }
            }
            
            _context.Notifications.UpdateRange(storage);
            await _context.SaveChangesAsync();

            _ = TransactionLogUpdate(entities, storage, "system." + nameof(Notification));

            return storage;
        }

        return [];
    }
}
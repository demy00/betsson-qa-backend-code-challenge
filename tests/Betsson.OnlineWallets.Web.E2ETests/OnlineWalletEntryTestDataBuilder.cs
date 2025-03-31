using Betsson.OnlineWallets.Data;
using Betsson.OnlineWallets.Data.Models;

namespace Betsson.OnlineWallets.Web.E2ETests;

public class OnlineWalletEntryTestDataBuilder
{
    private string? _id;
    private DateTimeOffset? _eventTime;
    private decimal _amount = 0;
    private decimal _balanceBefore = 0;

    public OnlineWalletEntryTestDataBuilder WithEventTime(DateTimeOffset eventTime)
    {
        _eventTime = eventTime;
        return this;
    }

    public OnlineWalletEntryTestDataBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public OnlineWalletEntryTestDataBuilder WithDateTimeOffset(DateTimeOffset eventTime)
    {
        _eventTime = eventTime;
        return this;
    }

    public OnlineWalletEntryTestDataBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public OnlineWalletEntryTestDataBuilder WithBalanceBefore(decimal balanceBefore)
    {
        _balanceBefore = balanceBefore;
        return this;
    }

    public OnlineWalletEntry Build()
    {
        var entry = new OnlineWalletEntry()
        {
            Amount = _amount,
            BalanceBefore = _balanceBefore
        };

        if (_id is not null)
            entry.Id = _id;

        if (_eventTime.HasValue)
            entry.EventTime = _eventTime.Value;

        return entry;
    }

    internal async Task<OnlineWalletEntry> BuildAsync(OnlineWalletContext context)
    {
        var entry = Build();

        context.Transactions.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }
}

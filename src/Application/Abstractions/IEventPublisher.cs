﻿namespace Application.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync(string eventName, object payload, CancellationToken ct = default);
    }
}

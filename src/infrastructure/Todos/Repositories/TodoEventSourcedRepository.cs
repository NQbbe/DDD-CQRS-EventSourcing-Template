using System.Diagnostics.Tracing;
using core.Interfaces.Events;
using core.Todos.Aggregates;
using core.Todos.Events;

namespace infrastructure.Todos.Repositories;

// This is an extremely crude implementation, obviously you'd have something better here.
// Do note how the class is internal, as no other projects should ever know if even exists as we're using dependency injection to ensure the interface has an implementation.
internal class TodoEventSourcedRepository : ITodoEventSourcedRepository
{
	private static IList<ITodoEvent> _events = new List<ITodoEvent>();
	private static object _lock = new();

	// This method isn't used any where in the template project, however it's included to show how you'd go about read full aggregates from an event sourced repository
	public Task<Todo?> GetAsync(Guid id, CancellationToken cancellationToken)
	{
		lock (_lock)
		{
			var aggregateEvents = _events
									.Where(@event => @event.AggregateId == id)
									.ToList();
			if (aggregateEvents.Any())
			{
				var todo = new Todo(aggregateEvents);
				return Task.FromResult((Todo?)todo);
			}
		}
		return Task.FromResult((Todo?)null);
	}

	public Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : ITodoEvent
	{
		lock (_lock)
		{
			_events.Add(@event);
		}
		return Task.CompletedTask;
	}
}
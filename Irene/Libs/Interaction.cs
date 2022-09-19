﻿namespace Irene;

// A wrapper class for DiscordInteraction that also handles some related
// functionality (e.g. timers).
class Interaction {
	// --------
	// Properties, constructors, and basic access methods:
	// --------

	// List of allowed events to register time points at.
	// These aren't required, e.g., none may be registered.
	public enum Events {
		InitialResponse,
		FinalResponse,
	}

	// Properties with backing fields.
	public DateTimeOffset TimeReceived { get; }
	public IReadOnlyDictionary<Events, TimeSpan> TimeOffsets { get; } =
		new ReadOnlyDictionary<Events, TimeSpan>(new ConcurrentDictionary<Events, TimeSpan>());
	public DiscordInteraction Object { get; }
	public DiscordMessage? TargetMessage { get; } = null;
	public DiscordUser? TargetUser { get; } = null;
	// Timer is automatically managed.
	private Stopwatch Timer { get; }

	// Calculated properties.
	// These are provided as syntax sugar for common properties.
	public InteractionType Type => Object.Type;
	public string Name => Object.Data.Name;
	public string CustomId => Object.Data.CustomId;
	public IList<string> Values => new List<string>(Object.Data.Values);
	public DiscordUser User => Object.User;
	public DiscordInteractionData Data => Object.Data;

	private Interaction(
		DiscordInteraction interaction,
		DiscordMessage? targetMessage=null,
		DiscordUser? targetUser=null
	) {
		Timer = Stopwatch.StartNew();
		TimeReceived = DateTimeOffset.UtcNow;
		Object = interaction;
		TargetMessage = targetMessage;
		TargetUser = targetUser;
	}

	// Public factory constructors.
	// These cannot be instance methods because some processing needs
	// to be done before calling the actual constructor, and that isn't
	// allowed in C#.
	// The alternative would be a shared Init() method, which is still
	// clunkier than just using factory methods.
	public static Interaction FromCommand(InteractionCreateEventArgs e) =>
		new (e.Interaction);
	public static Interaction FromContextMenu(ContextMenuInteractionCreateEventArgs e) =>
		e.Type switch {
			ApplicationCommandType.MessageContextMenu =>
				new (e.Interaction, targetMessage: e.TargetMessage),
			ApplicationCommandType.UserContextMenu =>
				new (e.Interaction, targetUser: e.TargetUser),
			_ => throw new ArgumentException("Event args must be a context menu interaction.", nameof(e)),
		};
	public static Interaction FromModal(ModalSubmitEventArgs e) =>
		new (e.Interaction);
	public static Interaction FromComponent(ComponentInteractionCreateEventArgs e) =>
		new (e.Interaction);

	// Methods relating to event time points.
	// RegisterEvent() overwrites any current events of that type.
	public void RegisterEvent(Events id) {
		TimeOffsets[id] = Timer.Elapsed;
	}
	public TimeSpan GetEventDuration(Events id) => TimeOffsets[id];
	public DateTimeOffset GetEventTime(Events id) =>
		TimeReceived + TimeOffsets[id];


	// --------
	// Convenience methods for responding to interactions:
	// --------

	// Responses to autocomplete interactions:
	public Task AutocompleteAsync(IList<(string, string)> choices) =>
		AutocompleteAsync<string>(choices);
	public Task AutocompleteAsync(IList<(string, int)> choices) =>
		AutocompleteAsync<int>(choices);
	// This method does not check for the choices having valid types.
	// The caller must ensure `T` is either `string` or `int`.
	private Task AutocompleteAsync<T>(IList<(string, T)> pairs) {
		// Create list of choice objects.
		List<DiscordAutoCompleteChoice> list = new ();
		foreach ((string, T) pair in pairs)
			list.Add(new (pair.Item1, pair.Item2));

		// Create interaction response object.
		DiscordInteractionResponseBuilder builder = new ();
		builder.AddAutoCompleteChoices(list);

		return Object.CreateResponseAsync(
			InteractionResponseType.AutoCompleteResult,
			builder
		);
	}

	// Responses to command interactions:
	public Task RespondCommandAsync(DiscordMessageBuilder message, bool isEphemeral=false) =>
		Object.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder(message)
				.AsEphemeral(isEphemeral)
		);
	public Task DeferCommandAsync(bool isEphemeral=false) =>
		Object.CreateResponseAsync(
			InteractionResponseType.DeferredChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AsEphemeral(isEphemeral)
		);

	// Responses to component interactions:
	public Task UpdateComponentAsync(DiscordMessageBuilder message) =>
		Object.CreateResponseAsync(
			InteractionResponseType.UpdateMessage,
			new (message)
		);
	public Task DeferComponentAsync() =>
		Object.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

	// Responses to either command or component interactions:
	public Task RespondModalAsync(DiscordInteractionResponseBuilder modal) =>
		Object.CreateResponseAsync(InteractionResponseType.Modal, modal);
	public Task<DiscordMessage> FollowupAsync(DiscordFollowupMessageBuilder builder) =>
		Object.CreateFollowupMessageAsync(builder);

	// Methods for manipulating responses/followups.
	public Task<DiscordMessage> GetResponseAsync() =>
		Object.GetOriginalResponseAsync();
	public Task<DiscordMessage> GetFollowupAsync(ulong id) =>
		Object.GetFollowupMessageAsync(id);
	public Task DeleteResponseAsync() =>
		Object.DeleteOriginalResponseAsync();
	public Task DeleteFollowupAsync(ulong id) =>
		Object.DeleteFollowupMessageAsync(id);
	public Task<DiscordMessage> EditResponseAsync(DiscordWebhookBuilder message) =>
		Object.EditOriginalResponseAsync(message);


	// --------
	// Convenience methods for accessing response data:
	// --------

	// Data relating to command options.
	public IList<DiscordInteractionDataOption> Args =>
		(Data.Options is not null)
			? new List<DiscordInteractionDataOption>(Data.Options)
			: new List<DiscordInteractionDataOption>();
	public DiscordInteractionDataOption? GetFocusedArg() {
		foreach (DiscordInteractionDataOption arg in Args) {
			if (arg.Focused)
				return arg;
		}
		return null;
	}

	// Data relating to modals.
	public IReadOnlyDictionary<string, DiscordComponent> GetModalData() { 
		Dictionary<string, DiscordComponent> components = new ();
		foreach (DiscordActionRowComponent row in Data.Components) {
			foreach (DiscordComponent component in row.Components) {
				if (component.Type is ComponentType.FormInput or ComponentType.Select)
					components.Add(component.CustomId, component);
			}
		}
		return new ReadOnlyDictionary<string, DiscordComponent>(components);
	}
}
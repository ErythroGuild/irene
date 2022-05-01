﻿using System.Reflection;

using Irene.Commands;

using LogFunc = System.Action<string, object[]>;
using RoleList = System.Collections.Generic.List<DSharpPlus.Entities.DiscordRole>;

namespace Irene;

public record class TimedInteraction
	(DiscordInteraction Interaction, Stopwatch Timer);

class Command {
	public static ConcurrentDictionary<string, HelpPageGetter> HelpPages { get; private set; }
	public static List<DiscordApplicationCommand> Commands { get; private set; }
	public static Dictionary<string, InteractionHandler> Deferrers { get; private set; }
	public static Dictionary<string, InteractionHandler> Handlers { get; private set; }
	public static Dictionary<string, InteractionHandler> AutoCompletes { get; private set; }

	// Force static initializer to run.
	public static void Init() { return; }
	static Command() {
		Stopwatch stopwatch = Stopwatch.StartNew();

		// Set the static properties.
		HelpPages = new ();
		Commands = new ();
		Deferrers = new ();
		Handlers = new ();
		AutoCompletes = new ();

		// Find all classes inheriting from ICommand, and collate their application
		// commands into a single Dictionary.
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types) {
			List<Type> interfaces = new (type.GetInterfaces());
			if (interfaces.Contains(typeof(ICommand))) {
				// Fetch the property, null-checking at every step.
				// If any step fails, simply return early.
				// Also fetch help pages if property is SlashCommand.
				void AddPropertyInteractions(string name) {
					PropertyInfo? property =
						type.GetProperty(name, typeof(List<InteractionCommand>));
					if (property is null)
						return;

					List<InteractionCommand>? commands =
						property?.GetValue(null) as List<InteractionCommand>
						?? null;
					if (commands is null)
						return;

					foreach (InteractionCommand command in commands) {
						Commands.Add(command.Command);
						Deferrers.Add(command.Command.Name, command.Deferrer);
						Handlers.Add(command.Command.Name, command.Handler);
					}

					if (name == nameof(ICommand.SlashCommands)) {
						PropertyInfo? property_help = type.GetProperty(
							nameof(ICommand.HelpPages),
							typeof(List<string>)
						);
						if (property_help is null)
							return;
						HelpPageGetter? func_help =
							property_help
								?.GetGetMethod()
								?.CreateDelegate(typeof(HelpPageGetter))
								as HelpPageGetter
							?? null;
						if (func_help is not null) {
							foreach (InteractionCommand command in commands)
								HelpPages.TryAdd(command.Command.Name, func_help);
						}
					}
				}
				void AddAutoCompletes() {
					PropertyInfo? property = type.GetProperty(
						nameof(ICommand.AutoComplete),
						typeof(List<AutoCompleteHandler>)
					);
					if (property is null)
						return;

					List<AutoCompleteHandler>? handlers =
						property?.GetValue(null) as List<AutoCompleteHandler>
						?? null;
					if (handlers is null)
						return;

					foreach (AutoCompleteHandler handler in handlers)
						AutoCompletes.Add(handler.CommandName, handler.Handler);
				}
				
				AddPropertyInteractions(nameof(ICommand.SlashCommands));
				AddPropertyInteractions(nameof(ICommand.UserCommands));
				AddPropertyInteractions(nameof(ICommand.MessageCommands));
				AddAutoCompletes();
			}
		}

		Log.Information("  Initialized module: Commands");
		Log.Debug("    Commands fetched and collated.");
		stopwatch.LogMsecDebug("    Took {Time} msec.");
	}

	// Returns the highest available access level of the user who invoked the
	// the interaction.
	public static async Task<AccessLevel> GetAccessLevel(DiscordInteraction interaction) {
		// Extract channel/user data.
		DiscordChannel channel = interaction.Channel;
		DiscordUser user = interaction.User;
		DiscordMember? member = channel.IsPrivate
			? await user.ToMember()
			: user as DiscordMember;

		// Warn if could not cast to DiscordMember.
		if (member is null) {
			Log.Warning("    Could not convert user ({UserTag}) to member.", user.Tag());
			return AccessLevel.None;
		}

		// Return no results if guild not initialized yet.
		if (Guild is null) {
			Log.Warning("    Guild not initialized yet. Assigning default permissions.");
			return AccessLevel.None;
		}

		return GetAccessLevel(member);
	}
	// Returns the highest access level the member has access to.
	public static AccessLevel GetAccessLevel(DiscordMember member) {
		static bool HasRole(RoleList r, ulong id) =>
			r.Contains(Program.Roles[id]);
		RoleList roles = new (member.Roles);
		return roles switch {
			RoleList r when HasRole(r, id_r.admin  ) => AccessLevel.Admin,
			RoleList r when HasRole(r, id_r.officer) => AccessLevel.Officer,
			RoleList r when HasRole(r, id_r.member ) => AccessLevel.Member,
			RoleList r when HasRole(r, id_r.guest  ) => AccessLevel.Guest,
			_ => AccessLevel.None,
		};
	}

	// Convenience functions for deferring as always/never ephemeral.
	public static async Task DeferEphemeralAsync(TimedInteraction interaction) =>
		await DeferAsync(interaction, true);
	public static async Task DeferVisibleAsync(TimedInteraction interaction) =>
		await DeferAsync(interaction, false);

	// Defer a command as ephemeral or not.
	public static async Task DeferAsync(
		DeferrerHandler handler,
		bool isEphemeral
	) =>
		await DeferAsync(handler.Interaction, isEphemeral);
	public static async Task DeferAsync(
		TimedInteraction interaction,
		bool isEphemeral
	) =>
		await interaction.Interaction.DeferMessageAsync(isEphemeral);

	// Logs that a response was sent, sends said response, and logs all
	// relevant timing information.
	public static async Task<DiscordMessage> SubmitResponseAsync(
		TimedInteraction interaction,
		string response,
		string log_summary,
		LogLevel log_level,
		Lazy<string> log_preview,
		params object[] log_params
	) =>
		await SubmitResponseAsync(
			interaction,
			new DiscordWebhookBuilder()
				.WithContent(response),
			log_summary,
			log_level,
			log_preview,
			log_params
		);
	public static async Task<DiscordMessage> SubmitResponseAsync(
		TimedInteraction interaction,
		DiscordWebhookBuilder response,
		string log_summary,
		LogLevel log_level,
		Lazy<string> log_preview,
		params object[] log_params
	) {
		DiscordMessage? message = await SubmitResponseAsync(
			interaction,
			Task.Run<DiscordMessage?>(async () => {
				DiscordInteraction i = interaction.Interaction;
				return await
					i.EditOriginalResponseAsync(response);
			}),
			log_summary,
			log_level,
			log_preview,
			log_params
		);
		return (message is null)
			? throw new InvalidOperationException("DiscordMessage not returned from followup response.")
			: message;
	}
	public static async Task<DiscordMessage?> SubmitResponseAsync(
		TimedInteraction interaction,
		Task<DiscordMessage?> task_response,
		string log_summary,
		LogLevel log_level,
		Lazy<string> log_preview,
		params object[] log_params
	) {
		Log.Debug("  " +  log_summary);
		interaction.Timer.LogMsecDebug("    Responded in {Time} msec.", false);
		DiscordMessage? message = await task_response;
		LogFunc log_func = GetLogFunc(log_level);
		log_func("  " + log_preview.Value, log_params);
		interaction.Timer.LogMsecDebug("    Response completed in {Time} msec.");
		return message;
	}

	private static LogFunc GetLogFunc(LogLevel logLevel) =>
		logLevel switch {
			LogLevel.Critical    => Log.Fatal,
			LogLevel.Error       => Log.Error,
			LogLevel.Warning     => Log.Warning,
			LogLevel.Information => Log.Information,
			LogLevel.Debug       => Log.Debug,
			LogLevel.Trace       => Log.Verbose,
			LogLevel.None        => (s, o) => { },
			_ => throw new ArgumentException("Unrecognized log level.", nameof(logLevel)),
		};
}

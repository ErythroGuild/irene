﻿using static Irene.RecurringEvent;

namespace Irene.Modules;

static partial class RecurringEvents {
	// Used in module initialization.
	// For timezone conversion, see:
	// https://www.timeanddate.com/worldclock/converter.html?p1=234
	private static async Task<List<Event>> GetEvents_Maintenance() {
		TimeSpan t0 = TimeSpan.Zero;
		List<Task<Event?>> event_tasks = new () {
			Event.Create(
				"Irene maintenance: backup data",
				RecurPattern.FromNthDayOfWeek(
					new (new (19, 30), TimeZone_PT),
					n: 1, DayOfWeek.Tuesday,
					months: 2
				),
				new (
					new (2022, 4, 6, 2, 30, 0, t0),
					new (2022, 6, 1)
				),
				Event_IreneBackupData,
				TimeSpan.FromDays(21) // 3 weeks
			),
			Event.Create(
				"Irene maintenance: backup logs",
				RecurPattern.FromNthDayOfWeek(
					new (new (19, 30), TimeZone_PT),
					n: 1, DayOfWeek.Tuesday,
					months: 4
				),
				new (
					new (2022, 4, 6, 2, 30, 0, t0),
					new (2022, 8, 1)
				),
				Event_IreneBackupLogs,
				TimeSpan.FromDays(21) // 3 weeks
			),
		};

		return await InitEventListAsync(event_tasks);
	}

	private static async Task Event_IreneBackupData(DateTimeOffset _) {
		if (Guild is null) {
			Log.Error("  Guild not loaded yet.");
			return;
		}

		const string t = "\u2003";
		const string a = "\u21D2";

		// Read in path data.
		string? dir_data = null;
		string? dir_backup = null;
		string? dir_repo = null;
		string? id_owner_str = null;
		lock (_lockDirData) {
			using StreamReader file = File.OpenText(_pathDirData);
			dir_data = file.ReadLine();
			dir_backup = file.ReadLine();
			dir_repo = file.ReadLine();
			id_owner_str = file.ReadLine();
		}

		// Exit early if bot owner not found.
		if (id_owner_str is null) {
			Log.Error("  No user ID for bot owner found.");
			Log.Debug("    File: {Path}", _pathDirData);
			return;
		}

		// Construct message.
		List<string> text = new ()
			{ $":file_cabinet: Bi-monthly reminder to back up my `/data` folder! Copy all the contents in the folder," };
		if (dir_data is not null && dir_backup is not null && dir_repo is not null) {
			text.Add($"{t}{a} from:   `{dir_data}`");
			text.Add($"{t}{a} to:        `{dir_backup}`");
			text.Add("Also copy `events.txt` and `raids.txt` (from the same folder),");
			text.Add($"{t}{a} to:        `{dir_repo}`");
		}

		// Send message.
		ulong id_owner = ulong.Parse(id_owner_str);
		DiscordMember member_owner =
			await Guild.GetMemberAsync(id_owner);
		await member_owner.SendMessageAsync(string.Join("\n", text));
	}

	private static async Task Event_IreneBackupLogs(DateTimeOffset _) {
		if (Guild is null) {
			Log.Error("  Guild not loaded yet.");
			return;
		}

		const string t = "\u2003";
		const string a = "\u21D2";

		// Read in path data.
		string? dir_logs = null;
		string? dir_backup = null;
		string? id_owner_str = null;
		lock (_lockDirLogs) {
			using StreamReader file = File.OpenText(_pathDirLogs);
			dir_logs = file.ReadLine();
			dir_backup = file.ReadLine();
			id_owner_str = file.ReadLine();
		}

		// Exit early if bot owner not found.
		if (id_owner_str is null) {
			Log.Error("  No user ID for bot owner found.");
			Log.Debug("    File: {Path}", _pathDirLogs);
			return;
		}

		// Construct message.
		List<string> text = new()
			{ $":file_cabinet: Quad-monthly reminder to back up my `/logs` folder! Copy all the contents in the folder," };
		if (dir_logs is not null && dir_backup is not null) {
			text.Add($"{t}{a} from:   `{dir_logs}`");
			text.Add($"{t}{a} to:        `{dir_backup}`");
		}

		// Send message.
		ulong id_owner = ulong.Parse(id_owner_str);
		DiscordMember member_owner =
			await Guild.GetMemberAsync(id_owner);
		await member_owner.SendMessageAsync(string.Join("\n", text));
	}
}
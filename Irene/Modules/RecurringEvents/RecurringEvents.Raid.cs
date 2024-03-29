namespace Irene.Modules;

using static Irene.RecurringEvent;
using static Irene.Modules.Raid;

using RaidObj = Raid;
using TimestampStyle = Util.TimestampStyle;

static partial class RecurringEvents {
	// Used in module initialization.
	// For timezone conversion, see:
	// https://www.timeanddate.com/worldclock/converter.html?p1=234
	private static async Task<List<Event>> GetEvents_Raid() {
		TimeSpan t0 = TimeSpan.Zero;
		List<Task<Event?>> event_tasks = new () {
			Event.Create(
				"Weekly raid plan announcement",
				RecurPattern.FromWeekly(
					new (new (8, 30), TimeZone_PT),
					DayOfWeek.Tuesday
				),
				new (
					new (2022, 3, 1, 16, 30, 0, t0),
					new (2022, 3, 1)
				),
				Event_WeeklyRaidPlanAnnouncement,
				TimeSpan.FromDays(2) // Tue. ~ Thu.
			),

			// Friday raid.
			Event.Create(
				"Friday: morning raid announcement",
				RecurPattern.FromWeekly(
					new (new (7, 30), TimeZone_PT),
					DayOfWeek.Friday
				),
				new (
					new (2022, 3, 4, 15, 30, 0, t0),
					new (2022, 3, 4)
				),
				Event_RaidDayMorningAnnouncement,
				TimeSpan.FromHours(4.5) // until noon PT
			),
			Event.Create(
				"Friday: raid forming soon reminder",
				RecurPattern.FromWeekly(
					new (new (17, 45), TimeZone_PT),
					DayOfWeek.Friday
				),
				new (
					new (2022, 3, 5, 1, 45, 0, t0),
					new (2022, 3, 4)
				),
				Event_RaidFormingSoonReminder,
				TimeSpan.FromMinutes(35) // until 18:20 PT
			),
			Event.Create(
				"Friday: raid forming announcement",
				RecurPattern.FromWeekly(
					new (new (18, 40), TimeZone_PT),
					DayOfWeek.Friday
				),
				new (
					new (2022, 3, 5, 2, 40, 0, t0),
					new (2022, 3, 4)
				),
				Event_RaidFormingAnnouncement,
				TimeSpan.FromMinutes(15) // until 18:55 PT
			),
			Event.Create(
				"Friday: set raid logs reminder",
				RecurPattern.FromWeekly(
					new (new (18, 50), TimeZone_PT),
					DayOfWeek.Friday
				),
				new (
					new (2022, 3, 5, 2, 50, 0, t0),
					new (2022, 3, 4)
				),
				Event_RaidSetLogsReminder,
				TimeSpan.FromHours(3) // until 21:50 PT
			),
			Event.Create(
				"Friday: raid break reminder",
				RecurPattern.FromWeekly(
					new (new (19, 50), TimeZone_PT),
					DayOfWeek.Friday
				),
				new (
					new (2022, 3, 5, 3, 50, 0, t0),
					new (2022, 3, 4)
				),
				Event_RaidBreakReminder,
				TimeSpan.FromMinutes(5) // until 19:55 PT
			),

			// Saturday raid.
			Event.Create(
				"Saturday: morning raid announcement",
				RecurPattern.FromWeekly(
					new (new (7, 30), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 5, 15, 30, 0, t0),
					new (2022, 3, 5)
				),
				Event_RaidDayMorningAnnouncement,
				TimeSpan.FromHours(4.5) // until noon PT
			),
			Event.Create(
				"Saturday: raid forming soon reminder",
				RecurPattern.FromWeekly(
					new (new (17, 45), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 1, 45, 0, t0),
					new (2022, 3, 5)
				),
				Event_RaidFormingSoonReminder,
				TimeSpan.FromMinutes(35) // until 18:20 PT
			),
			Event.Create(
				"Saturday: raid forming announcement",
				RecurPattern.FromWeekly(
					new (new (18, 40), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 2, 40, 0, t0),
					new (2022, 3, 5)
				),
				Event_RaidFormingAnnouncement,
				TimeSpan.FromMinutes(15) // until 18:55 PT
			),
			Event.Create(
				"Saturday: set raid logs reminder",
				RecurPattern.FromWeekly(
					new (new (18, 50), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 2, 50, 0, t0),
					new (2022, 3, 5)
				),
				Event_RaidSetLogsReminder,
				TimeSpan.FromHours(3) // until 21:50 PT
			),
			Event.Create(
				"Saturday: raid break reminder",
				RecurPattern.FromWeekly(
					new (new (19, 50), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 3, 50, 0, t0),
					new (2022, 3, 5)
				),
				Event_RaidBreakReminder,
				TimeSpan.FromMinutes(5) // until 19:55 PT
			),

			// Officer meeting.
			Event.Create(
				"Weekly officer meeting reminder",
				RecurPattern.FromWeekly(
					new (new (21, 5), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 5, 5, 0, t0),
					new (2022, 3, 5)
				),
				Event_OfficerMeetingReminder,
				TimeSpan.FromMinutes(5) // until 21:10 PT
			),
			Event.Create(
				"Weekly set raid plans reminder",
				RecurPattern.FromWeekly(
					new (new (21, 10), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 5, 10, 0, t0),
					new (2022, 3, 5)
				),
				Event_OfficerRaidPlansReminder,
				TimeSpan.FromMinutes(5) // until 21:15 PT
			),
			Event.Create(
				"Weekly promote trials reminder",
				RecurPattern.FromWeekly(
					new (new (21, 15), TimeZone_PT),
					DayOfWeek.Saturday
				),
				new (
					new (2022, 3, 6, 5, 15, 0, t0),
					new (2022, 3, 5)
				),
				Event_OfficerPromoteTrialsReminder,
				TimeSpan.FromMinutes(5) // until 21:20 PT
			),
		};

		return await InitEventListAsync(event_tasks);
	}

	private static async Task Event_WeeklyRaidPlanAnnouncement(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Calculate raid dates and fetch data.
		DateTimeOffset dateTime_friday = timeTriggered.AddDays(3);
		DateTimeOffset dateTime_saturday = timeTriggered.AddDays(4);
		RaidDate date_friday =
			RaidDate.TryCreate(dateTime_friday)!.Value;
		RaidDate date_saturday =
			RaidDate.TryCreate(dateTime_saturday)!.Value;
		RaidObj raid_friday = Fetch(date_friday);
		RaidObj raid_saturday = Fetch(date_saturday);
		DiscordEmoji emoji_raid =
			DiscordEmoji.FromName(Erythro.Client, raid_friday.Emoji);

		// Exit early if nothing to announce:
		// Neither raid canceled and no plans set.
		string? plans_friday = raid_friday.Summary;
		string? plans_saturday = raid_saturday.Summary;
		bool doCancel_friday = raid_friday.DoCancel;
		bool doCancel_saturday = raid_saturday.DoCancel;
		if (!(doCancel_friday || doCancel_saturday) &&
			plans_friday is null &&
			plans_saturday is null
		) {
			Log.Information("  No plans and nothing to announce this week (yet).");
			return;
		}

		// Compose announcement.
		List<string> announcement = new ()
			{ $"Happy reset day! {Erythro.Emoji(id_e.eryLove)} {emoji_raid}" };
		switch (doCancel_friday, doCancel_saturday) {
		case (true, true):
			announcement.Add("No raid this week. :slight_smile:");
			break;
		case (true, false):
			announcement.Add("Raid is canceled on Friday (but not Saturday)!");
			break;
		case (false, true):
			announcement.Add("Raid is canceled on Saturday (but not Friday)!");
			break;
		}
		// If only one plan is set, format as the entire week's plans.
		switch (plans_friday, plans_saturday) {
		case (string f, string s) when f is not null && s is null:
			announcement.Add($"**Raid plans this week:** {f}");
			break;
		case (string f, string s) when f is null && s is not null:
			announcement.Add($"**Raid plans this week:** {s}");
			break;
		case (string f, string s) when f is not null && s is not null:
			announcement.Add($"**Fri. raid plans:** {f}");
			announcement.Add($"**Sat. raid plans:** {s}");
			break;
		}

		// Send announcement and react.
		DiscordMessage message = await
			Erythro.Channel(id_ch.announce).SendMessageAsync(announcement.ToLines());
		DiscordEmoji emoji_sunrise =
			DiscordEmoji.FromName(Erythro.Client, ":sunrise_over_mountains:");
		await message.CreateReactionAsync(emoji_raid);
		await message.CreateReactionAsync(emoji_sunrise);
	}

	private static async Task Event_RaidDayMorningAnnouncement(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Fetch saved raid data.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);

		// Don't fire event if raid was canceled.
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Calculate raid time info.
		DateTime today = TimeZoneInfo.ConvertTime(
			DateTime.UtcNow,
			TimeZone_Server
		);
		today -= today.TimeOfDay;
		DateTime time_raid = today +
			Time_RaidStart.TimeOnly.ToTimeSpan();

		// Format raid time as timestamp.
		DateTimeOffset dateTime_raid =
			new (TimeZoneInfo.ConvertTime(
				time_raid,
				TimeZone_Server,
				TimeZoneInfo.Utc
			));
		string time_raid_str = dateTime_raid
			.Timestamp(TimestampStyle.TimeShort);

		// Compose and send announcement.
		string announcement = $"{raid.Emoji} Happy raid day! • *✰";
		if (raid.Summary is not null)
			announcement += $"\n**Plans for today:** {raid.Summary}";
		announcement += $"\nSee you at {time_raid_str}. :wine_glass:";
		DiscordMessage message = await
			Erythro.Channel(id_ch.announce).SendMessageAsync(announcement);
		DiscordEmoji emoji_raid =
			DiscordEmoji.FromName(Erythro.Client, raid.Emoji);
		await message.CreateReactionAsync(emoji_raid);
	}

	private static async Task Event_RaidFormingSoonReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Fetch saved raid data.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);

		// Don't fire event if raid was canceled.
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Calculate raid time / forming time info.
		DateTime today = TimeZoneInfo.ConvertTime(
			DateTime.UtcNow,
			TimeZone_Server
		);
		today -= today.TimeOfDay;
		DateTime time_raid = today +
			Time_RaidStart.TimeOnly.ToTimeSpan();
		DateTime time_forming = time_raid -
			TimeSpan.FromMinutes(20);

		// Convert times to UTC, then format as timestamps.
		DateTimeOffset dateTime_raid =
			new (TimeZoneInfo.ConvertTime(
				time_raid,
				TimeZone_Server,
				TimeZoneInfo.Utc
			));
		DateTimeOffset dateTime_forming =
			new (TimeZoneInfo.ConvertTime(
				time_forming,
				TimeZone_Server,
				TimeZoneInfo.Utc
			));
		string time_raid_str = dateTime_raid
			.Timestamp(TimestampStyle.TimeShort);
		string time_forming_str = dateTime_forming
			.Timestamp(TimestampStyle.Relative);

		// Compose raid announcement.
		string announcement =
			$"{raid.Emoji} {Erythro.Role(id_r.raid).Mention} - " +
			$"Forming for raid {time_forming_str} " +
			$"(pulling at {time_raid_str}).";
		if (raid.Summary is not null)
			announcement += $"\n{raid.Summary}";
		announcement += "\nIf you aren't sure about requirements, check the pinned posts. :thumbsup:";

		// Respond.
		DiscordMessage message = await
			Erythro.Channel(id_ch.announce).SendMessageAsync(announcement);
		DiscordEmoji emoji_raid =
			DiscordEmoji.FromName(Erythro.Client, raid.Emoji);
		await message.CreateReactionAsync(emoji_raid);
	}

	private static async Task Event_RaidFormingAnnouncement(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Fetch saved raid data.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);

		// Don't fire event if raid was canceled.
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Send a message and then save the message back to the data file.
		string text = raid.AnnouncementText;
		DiscordMessage message = await
			Erythro.Channel(id_ch.announce).SendMessageAsync(text);
		raid.MessageId = message.Id;
		raid.UpdateData();

		// Update announcement message with logs (if available).
		await raid.UpdateAnnouncement();

		// Add reactions.
		DiscordEmoji emoji_raid =
			DiscordEmoji.FromName(Erythro.Client, raid.Emoji);
		await message.CreateReactionAsync(emoji_raid);
		await message.CreateReactionAsync(Erythro.Emoji(id_e.kyrian));
		await message.CreateReactionAsync(Erythro.Emoji(id_e.necrolord));
		await message.CreateReactionAsync(Erythro.Emoji(id_e.nightfae));
		await message.CreateReactionAsync(Erythro.Emoji(id_e.venthyr));
	}

	private static async Task Event_RaidSetLogsReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Fetch saved raid data.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);

		// Don't fire event if raid was canceled.
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Skip reminder if logs are already set.
		foreach (RaidGroup group in raid.Data.Keys) {
			if (raid.Data[group].LogId is not null) {
				Log.Information("  Skipping reminder: logs already set.");
				return;
			}
		}

		// Send reminder.
		string announcement = new List<string> {
			$"{Erythro.Role(id_r.raidOfficer).Mention} -",
			$"{_t}Reminder to set logs for tonight. :ok_hand: :card_box:",
			$"{_t}`/raid set-logs`",
		}.ToLines();
		await Erythro.Channel(id_ch.officerBots).SendMessageAsync(announcement);
	}

	private static async Task Event_RaidBreakReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Don't fire event if raid was canceled.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Send reminder.
		string announcement = new List<string> {
			$"{Erythro.Role(id_r.raidOfficer).Mention} -",
			$"{_t}Raid break soon. :slight_smile: :tropical_drink:",
		}.ToLines();
		await Erythro.Channel(id_ch.officer).SendMessageAsync(announcement);
	}

	private static async Task Event_OfficerMeetingReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Don't fire event if raid was canceled.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Send reminder.
		string officer = Erythro.Role(id_r.officer).Mention;
		string announcement =
			$"Weekly {officer} meeting after raid. :slight_smile:";
		await Erythro.Channel(id_ch.officer).SendMessageAsync(announcement);
	}

	private static async Task Event_OfficerRaidPlansReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Don't fire event if raid was canceled.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Check next week's data.
		// Skip reminder if (any) plans are already set.
		RaidTier tier = date.Tier;
		int week = date.Week + 1;
		RaidDate date_F = new (tier, week, RaidDay.Friday);
		RaidDate date_S = new (tier, week, RaidDay.Saturday);
		RaidObj raid_F = Fetch(date_F);
		RaidObj raid_S = Fetch(date_S);
		if (raid_F.Summary is not null ||
			raid_S.Summary is not null
		) {
			Log.Information("  Skipping reminder: plans already set.");
			return;
		}

		// Send reminder.
		string announcement = new List<string> {
			$"{Erythro.Role(id_r.raidOfficer).Mention} -",
			$"{_t}Decide on the raid plans for next week (if you haven't already). :ok_hand:",
			$"{_t}`/raid set-plan`",
		}.ToLines();
		await Erythro.Channel(id_ch.officerBots).SendMessageAsync(announcement);
	}

	private static async Task Event_OfficerPromoteTrialsReminder(DateTimeOffset timeTriggered) {
		CheckErythroInit();

		// Don't fire event if raid was canceled.
		RaidDate date = RaidDate.TryCreate(timeTriggered)!.Value;
		RaidObj raid = Fetch(date);
		if (raid.DoCancel) {
			Log.Information("  Skipping event (raid was canceled).");
			return;
		}

		// Send reminder.
		string announcement = new List<string> {
			$"{Erythro.Role(id_r.recruiter).Mention} -",
			$"{_t}Go over the 2-week+-trials this week (if there are any). :seedling:",
			$"{_t}`/rank list-trials`",
		}.ToLines();
		await Erythro.Channel(id_ch.officerBots).SendMessageAsync(announcement);
	}
}
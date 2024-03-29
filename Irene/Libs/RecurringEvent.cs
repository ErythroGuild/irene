﻿namespace Irene;

using BasisType = RecurringEvent.RecurBasis.BasisType;
using RuleType = RecurringEvent.RecurBasis.RuleType;
using RuleDirection = RecurringEvent.RecurBasis.RuleDirection;

class RecurringEvent {
	// Types used as indices to define RecurBases.
	public readonly record struct DateOfYear {
		public readonly int Month { get; init; }
		public readonly int Day { get; init; }

		// Construct a new DateOfYear, checking that values are valid.
		// Feb. 29th is permitted as a value.
		public DateOfYear(int month, int day) {
			if (month is <1 or >12)
				throw new ArgumentOutOfRangeException(nameof(month), "Month for DateOfYear was invalid.");
			int maxDays = _monthLengths[month];
			if (day < 1 || day > maxDays)
				throw new ArgumentOutOfRangeException(nameof(day), "Day for DateOfYear was invalid.");

			Month = month;
			Day = day;
		}
		public DateOfYear(DateOnly date) {
			Month = date.Month;
			Day = date.Day;
		}
		private static readonly ReadOnlyDictionary<int, int> _monthLengths =
			new (new ConcurrentDictionary<int, int> {
				[ 1] = 31, [ 2] = 29, [ 3] = 31,
				[ 4] = 30, [ 5] = 31, [ 6] = 30,
				[ 7] = 31, [ 8] = 31, [ 9] = 30,
				[10] = 31, [11] = 30, [12] = 31,
			});
	};
	public enum LunarPhase {
		NewMoon,
		WaxingCrescent,
		FirstQuarter,
		WaxingGibbous,
		FullMoon,
		WaningGibbous,
		LastQuarter,
		WaningCrescent,
	};

	// A RecurPattern is defined from an ordered list of RecurBases.
	public readonly record struct RecurBasis {
		public enum BasisType {
			Days,
			Weeks,
			Months,
			Years,
			DaysOfWeek,
			DatesOfYear,
			LunarPhase,
		};
		public enum RuleType {
			Base,
			BeforeStart,
			AfterStart,
			ClosestStart,
			BeforeEnd,
			AfterEnd,
			ClosestEnd,
		};
		public enum RuleDirection {
			Before,
			After,
			Closest,
		};

		public readonly BasisType Basis { get; init; }
		public readonly RuleType Rule { get; init; }
		public readonly RuleDirection Direction => Rule switch {
			RuleType.BeforeStart or
			RuleType.BeforeEnd =>
				RuleDirection.Before,
			RuleType.Base or
			RuleType.AfterStart or
			RuleType.AfterEnd =>
				RuleDirection.After,
			RuleType.ClosestStart or
			RuleType.ClosestEnd =>
				RuleDirection.Closest,
			_ =>
				throw new UnclosedEnumException(typeof(RuleType), Rule),
		};
		public readonly object Index { get; init; }

		public RecurBasis(BasisType basis, RuleType rule, object index) {
			if (index.GetType() != _typeTable[basis])
				throw new ArgumentException("Index was an invalid type for the BasisType.", nameof(index));

			if (_typeTable[basis] == typeof(int)) {
				if ((int)index < 1)
					throw new ArgumentOutOfRangeException(nameof(index), "Index must be positive.");
			}

			Basis = basis;
			Rule = rule;
			Index = index;
		}
		private static readonly ReadOnlyDictionary<BasisType, Type> _typeTable =
			new (new ConcurrentDictionary<BasisType, Type> {
				[BasisType.Days  ] = typeof(int),
				[BasisType.Weeks ] = typeof(int),
				[BasisType.Months] = typeof(int),
				[BasisType.Years ] = typeof(int),
				[BasisType.DaysOfWeek ] = typeof(DayOfWeek) ,
				[BasisType.DatesOfYear] = typeof(DateOfYear),
				[BasisType.LunarPhase ] = typeof(LunarPhase),
			});

		// Return the max/min length of the period of the RecurBasis,
		// as an absolute value, counting from the start of the rule.
		// Rule type and surrounding rules are not accounted for.
		public int PeriodMaxDays => Basis switch {
			BasisType.Days or
			BasisType.Weeks or
			BasisType.Months or
			BasisType.Years =>
				(int)Index * _periodTable[Basis],
			BasisType.DaysOfWeek or
			BasisType.DatesOfYear or
			BasisType.LunarPhase =>
				_periodTable[Basis],
			_ => throw new UnclosedEnumException(typeof(BasisType), Basis),
		};
		public int PeriodMinDays => Basis switch {
			BasisType.Days or
			BasisType.Weeks or
			BasisType.Months or
			BasisType.Years =>
				(int)Index * _periodTable[Basis],
			BasisType.DaysOfWeek or
			BasisType.DatesOfYear or
			BasisType.LunarPhase =>
				0,
			_ => throw new UnclosedEnumException(typeof(BasisType), Basis),
		};
		private static readonly ReadOnlyDictionary<BasisType, int> _periodTable =
			new (new ConcurrentDictionary<BasisType, int> {
				[BasisType.Days  ] =   1,
				[BasisType.Weeks ] =   7,
				[BasisType.Months] =  31,
				[BasisType.Years ] = 366,
				[BasisType.DaysOfWeek ] =   7,
				[BasisType.DatesOfYear] = 366,
				[BasisType.LunarPhase ] =  30,
			});
	};

	// A RecurPattern holds all data necessary to specify a RecurringEvent.
	// The List of RecurBases are listed in order of evaluation, and the
	// RecurBasisIndex points to the item in the list (at the end of) to
	// begin evaluating the next recurrence from.
	// This record struct also defines some convenience functions (factory
	// methods) to construct common RecurPatterns.
	public readonly record struct RecurTime
		(TimeOnly TimeOnly, TimeZoneInfo TimeZone);
	private readonly record struct RecurDateTime
		(DateTime DateTime, TimeZoneInfo TimeZone);
	public readonly record struct RecurResult
		(DateTimeOffset OutputDateTime, DateOnly CycleDate);
	private readonly record struct RecurDateResult
		(DateOnly OutputDate, DateOnly CycleDate);
	public record class RecurPattern {
		public RecurTime Time { get; init; }
		public TimeOnly TimeOnly => Time.TimeOnly;
		public TimeZoneInfo TimeZone => Time.TimeZone;
		public int RecurIndex { get; init; }
		public List<RecurBasis> Bases { get; init; }

		// Constructor does a sanity check to ensure, in the best-case,
		// the next event will happen after the previous event.
		// (This does NOT ensure the RecurPattern itself can never fail,
		// as this would be equivalent to solving the Halting Problem.)
		public RecurPattern(RecurTime time, int recurIndex, IReadOnlyList<RecurBasis> basesCollection) {
			List<RecurBasis> bases = new (basesCollection);

			// Check that the index is valid.
			int listSize = bases.Count;
			if (recurIndex < 0 || recurIndex > listSize) {
				throw new ArgumentOutOfRangeException(nameof(recurIndex),
					"Basis index points outside the range of the RecurBases list.");
			}

			// Check that first basis is RuleType.Base.
			// Previous condition actually guarantees that at least one
			// basis exists, so there doesn't need to be a separate check.
			if (bases[0].Rule != RuleType.Base)
				throw new ArgumentException("The first basis was not a Base rule.", nameof(basesCollection));

			// Check that the only basis with RuleType.Base is at 0.
			int indexLastBase = bases.FindLastIndex(
				basis => basis.Rule == RuleType.Base
			);
			if (indexLastBase != 0)
				throw new ArgumentException("There was more than one Base rule given.", nameof(basesCollection));

			// Calculate the longest possible recur cycle, and check that
			// it is greater than zero (it always advances).
			int daysCycle = 0;
			int daysRulePrev = 0;
			for (int i=0; i<listSize; i++) {
				RecurBasis basis = bases[i];
				int daysRule = basis.Rule switch {
					RuleType.Base or
					RuleType.AfterStart or
					RuleType.ClosestStart or
					RuleType.AfterEnd or
					RuleType.ClosestEnd =>
						basis.PeriodMaxDays,
					RuleType.BeforeStart or
					RuleType.BeforeEnd =>
						-basis.PeriodMinDays,
					_ =>
						throw new UnclosedEnumException(typeof(RecurBasis), basis.Rule),
				};
				daysCycle += daysRule;
				daysCycle -= basis.Rule switch {
					RuleType.BeforeStart or
					RuleType.AfterStart or
					RuleType.ClosestStart =>
						daysRulePrev,
					_ => 0,
				};
				daysRulePrev = daysRule;
			}

			// Throw an exception if the given pattern can definitively be
			// determined as invalid.
			if (daysCycle <= 0) {
				throw new ArgumentException(
					"RecurPattern definition will always give earlier dates",
					nameof(basesCollection)
				);
			}

			// Assign fields.
			Time = time;
			RecurIndex = recurIndex;
			Bases = bases;
		}

		// Syntactic sugar for common RecurPattern definitions.
		public static RecurPattern FromDaily(RecurTime time, int n=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Days, RuleType.Base, n)
			});
		public static RecurPattern FromWeekly(RecurTime time, DayOfWeek dayOfWeek, int n=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Weeks, RuleType.Base, n),
				new (BasisType.DaysOfWeek, RuleType.AfterStart, dayOfWeek)
			});
		public static RecurPattern FromMonthly(RecurTime time, int day, int n=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Months, RuleType.Base, n),
				new (BasisType.Days, RuleType.AfterStart, day)
			});
		public static RecurPattern FromAnnually(RecurTime time, int month, int day, int n=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Years, RuleType.Base, n),
				new (BasisType.DatesOfYear, RuleType.AfterStart, new DateOfYear(month, day))
			});
		public static RecurPattern FromAnnually(RecurTime time, DateOnly date, int n=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Years, RuleType.Base, n),
				new (BasisType.DatesOfYear, RuleType.AfterStart, new DateOfYear(date))
			});
		public static RecurPattern FromNthDayOfWeek(RecurTime time, int n, DayOfWeek dayOfWeek, int months=1) =>
			new (time, 0, new List<RecurBasis> {
				new (BasisType.Months, RuleType.Base, months),
				new (BasisType.Weeks, RuleType.AfterStart, n),
				new (BasisType.DaysOfWeek, RuleType.AfterStart, dayOfWeek)
			});
	}

	// Properties.
	// The .Next property is calculated every time (on-the-fly).
	// A null value indicates an invalid calculation (the result did
	// not occur after the reference timepoint).
	// There are convenience functions for either peeking at it or
	// "permanently" fetching it, and these are the only publically
	// visible methods.
	public DateTimeOffset? PeekNext() => CalculateNext?.OutputDateTime ?? null;
	public DateTimeOffset? GetNext() {
		RecurResult? next = CalculateNext;
		if (next is null)
			return null;
		Previous = next!.Value;
		return next!.Value.OutputDateTime;
	}

	public RecurPattern Pattern { get; init; }
	public RecurResult Previous { get; private set; }
	private RecurResult? CalculateNext { get {
		// Calculate output dates.
		RecurDateResult resultDates = NextDate(Previous.CycleDate, Pattern);

		// Convert to UTC DateTimeOffset.
		DateTime resultDateTime =
			resultDates.OutputDate.ToDateTime(Pattern.TimeOnly);
		DateTimeOffset resultDateTimeOffset =
			ToUtc(resultDateTime, Pattern.TimeZone);
		RecurResult result =
			new (resultDateTimeOffset, resultDates.CycleDate);

		// If the calculated output is not after the .Previous value,
		// indicate an invalid result by returning null.
		return (Previous.OutputDateTime >= resultDateTimeOffset)
			? null
			: result;
	} }

	// Constructor (trivial).
	// A valid "prior" RecurResult must be given, in order to properly
	// initialize calculations for further iterations.
	// The given RecurResult is not validated, and constructing with an
	// invalid RecurResult will result in undefined behavior.
	public RecurringEvent(RecurPattern pattern, RecurResult previous) {
		Pattern = pattern;
		Previous = previous;
	}

	// Helper functions.
	// Convert a DateTime to a regular DateTimeOffset (fixed with an
	// offset of 0 for UTC).
	private static DateTimeOffset ToUtc(DateTime dateTime, TimeZoneInfo timezone) {
		DateTime timeUtc =
			TimeZoneInfo.ConvertTimeToUtc(dateTime, timezone);
		return new DateTimeOffset(timeUtc);
	}

	// Returns the next DateOnly (not including the current date,
	// even if the current date fits the defined RecurPattern.)
	// Presumably starting from a prior cycle's end date.
	private static RecurDateResult NextDate(DateOnly cyclePrev, RecurPattern pattern) {
		int basesCount = pattern.Bases.Count;
		DateOnly dateNext = cyclePrev;
		DateOnly dateCycle = cyclePrev;
		DateOnly datePrev = cyclePrev;

		// Iterate through each rule in order.
		for (int i=0; i<basesCount; i++) {
			RecurBasis basis = pattern.Bases[i];

			// Dial back starting date if rule is based on last
			// rule's starting position.
			switch (basis.Rule) {
			case RuleType.BeforeStart:
			case RuleType.AfterStart:
			case RuleType.ClosestStart:
				dateNext = datePrev;
				break;
			}

			// Save starting position for next iteration.
			datePrev = dateNext;

			// Process each rule.
			RuleDirection direction = basis.Direction;
			switch (basis.Basis) {
			case BasisType.Days: {
				int days = (int)basis.Index;
				if (direction is RuleDirection.Before)
					days *= -1;
				dateNext = dateNext.AddDays(days);
				break; }
			case BasisType.Weeks: {
				int days = 7 * (int)basis.Index;
				if (direction is RuleDirection.Before)
					days *= -1;
				dateNext = dateNext.AddDays(days);
				break; }
			case BasisType.Months: {
				int months = (int)basis.Index;
				if (direction is RuleDirection.Before)
					months *= -1;
				dateNext = dateNext.AddMonths(months);
				break; }
			case BasisType.Years: {
				int years = (int)basis.Index;
				if (direction is RuleDirection.Before)
					years *= -1;
				dateNext = dateNext.AddYears(years);
				break; }
			case BasisType.DaysOfWeek: {
				DayOfWeek dayOfWeek = (DayOfWeek)basis.Index;
				switch (direction) {
				case RuleDirection.Before:
					dateNext = dateNext.PreviousDayOfWeek(dayOfWeek);
					break;
				case RuleDirection.After:
					dateNext = dateNext.NextDayOfWeek(dayOfWeek);
					break;
				case RuleDirection.Closest: {
					DateOnly dateBefore =
						dateNext.PreviousDayOfWeek(dayOfWeek);
					DateOnly dateAfter =
						dateNext.NextDayOfWeek(dayOfWeek);
					dateNext =
						dateNext.Closest(dateBefore, dateAfter);
					break; }
				}
				break; }
			case BasisType.DatesOfYear: {
				DateOfYear dateOfYear = (DateOfYear)basis.Index;
				int month = dateOfYear.Month;
				int day = dateOfYear.Day;
				switch (direction) {
				case RuleDirection.Before:
					dateNext = dateNext.PreviousDateOfYear(month, day);
					break;
				case RuleDirection.After:
					dateNext = dateNext.NextDateOfYear(month, day);
					break;
				case RuleDirection.Closest: {
					DateOnly dateBefore =
						dateNext.PreviousDateOfYear(month, day);
					DateOnly dateAfter =
						dateNext.NextDateOfYear(month, day);
					dateNext =
						dateNext.Closest(dateBefore, dateAfter);
					break; }
				}
				break; }
			case BasisType.LunarPhase: {
				LunarPhase lunarPhase = (LunarPhase)basis.Index;
				switch (direction) {
				case RuleDirection.Before:
					dateNext = dateNext.PreviousLunarPhase(lunarPhase);
					break;
				case RuleDirection.After:
					dateNext = dateNext.NextLunarPhase(lunarPhase);
					break;
				case RuleDirection.Closest:
					DateOnly dateBefore =
						dateNext.PreviousLunarPhase(lunarPhase);
					DateOnly dateAfter =
						dateNext.NextLunarPhase(lunarPhase);
					dateNext =
						dateNext.Closest(dateBefore, dateAfter);
					break;
				}
				break; }
			}

			// Set cycle date once we reach the designated rule.
			if (pattern.RecurIndex == i)
				dateCycle = dateNext;
		}

		return new RecurDateResult(dateNext, dateCycle);
	}
}

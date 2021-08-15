﻿using System;

using static Irene.Const;

namespace Irene {
	using id_r = RoleIDs;
	using id_ch = ChannelIDs;
	using id_e = EmojiIDs;

	static class Const {
		// Patch release days
		// 7:00 PST = 8:00 PDT = 15:00 UTC
		public static readonly DateTime
			date_patch_902 = new (2020, 11, 17, 15, 0, 0, DateTimeKind.Utc),
			date_season_1  = new (2020, 12,  8, 15, 0, 0, DateTimeKind.Utc),
			date_patch_905 = new (2021,  3,  9, 15, 0, 0, DateTimeKind.Utc),
			date_patch_910 = new (2021,  6, 29, 15, 0, 0, DateTimeKind.Utc),
			date_season_2  = new (2021,  7,  6, 15, 0, 0, DateTimeKind.Utc);

		// Discord entity IDs
		public static class RoleIDs {
			public const ulong
				// Colors
				ambassador = 724762507612520530,
				stylist    = 777570683159838760,
				bot        = 614642181587599439,
				admin      = 452298394967343105,
				officer    = 542021861727272981,
				member     = 452299909371265042,
				guest      = 452299336123023360,

				// Titles
				acclaimed = 780161029111021630,
				beloved   = 780159178202284053,
				esteemed  = 780159754676338700,

				// Officers
				raidOfficer   = 723061867127373874,
				eventPlanner  = 723061585156898836,
				mythicOfficer = 824852094493917194,
				recruiter     = 723060994859073607,
				banker        = 723061777213947974,

				// Guilds
				erythro = 542021676884557824,
				glaive  = 529135445712568323,
				dragons = 830985516639584257,
				angels  = 529135186382946345,
				asgard  = 539515941877448728,
				enclave = 676356017461919744,

				// Subscriptions
				raid    = 654062159956803664,
				mythics = 653334284299534336,
				ksm     = 665203067767226368,
				gearing = 854879377930453002,
				events  = 771584406724935710,
				herald  = 712469431573544972;
		}
		public static class ChannelIDs {
			public const ulong
				// Broadcast
				rules     = 443002035604815872,
				announce  = 443001903123791873,
				resources = 542093130502111252,
				pins      = 648461869979271179,
				starboard = 778102974551293962,
				audit     = 778102942141382678,

				// Text
				general  = 317723973968461824,
				sharing  = 443000872968912896,
				spoilers = 454338639933997068,
				memes    = 543476538125844511,
				tts      = 444792161158823936,
				lfg      = 542093438238326804,
				bots     = 613034262823698452,
				news     = 612307690613637125,

				// Officer
				officer     = 542023200549371906,
				officerBots = 779385198533804062,
				officerInfo = 650603757113049088,

				// Voice chat
				v_hangout = 442986379392319490,
				v_request = 854884304362274816,
				v_raid1   = 443001346678063104,
				v_raid2   = 670853551869919232,
				v_officer = 542093594618757140,
				v_afk     = 545060010967957534,

				// Testing
				test       = 489274692255875091,
				ingest     = 777935219193020426,
				v_heartwood = 498636532530282512;
		}
		public static class EmojiIDs {
			public const ulong
				erythro = 651475835387248660,

				wooloo   = 588112288628604928,
				toxic    = 839929134326349825,
				pooh     = 756560918208446514,
				mrrogers = 777861300423884803,

				// Analysis websites
				raiderio     = 699710975451856967,
				warcraftlogs = 699709399094132884,
				wipefest     = 699647884819169700,
				wowanalyzer  = 699648774099828796,

				// Roles
				tank = 708431859369115790,
				heal = 708431859435962418,
				dps  = 708431859385630862,

				// Covenants
				kyrian    = 697164668866658385,
				necrolord = 697164668506079332,
				nightfae  = 697164668975972433,
				venthyr   = 697166461164322927,

				// Classes
				dk      = 676750707759513611,
				dh      = 676750708175011860,
				druid   = 676750708447641611,
				hunter  = 676750708879523850,
				mage    = 676750710699720736,
				monk    = 676750843286126624,
				paladin = 676750876433711114,
				priest  = 676750889922330665,
				rogue   = 676750902895312901,
				shaman  = 676750915843260436,
				warlock = 676750927889170437,
				warrior = 676750939910045707;
		}
	}
}
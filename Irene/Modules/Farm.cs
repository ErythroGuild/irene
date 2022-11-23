﻿using System.Text;

namespace Irene.Modules;

class Farm {
	public enum Quality {
		Poor, Common, Uncommon, Rare, Epic,
		Legendary, Artifact, Heirloom
	}

	public record class Material(
		string Name,
		Quality Quality,
		string Icon,
		string Guide,
		string Wowhead,
		IReadOnlyList<Route> Routes,
		DateOnly Timestamp
	);
	public record class Route(
		string Id,
		string Name,
		string Comments,
		string Image
	);

	// A list of all normalized names to canonical names, for autocomplete.
	// E.g.: zinanthid -> Zin'anthid
	private static ConcurrentDictionary<string, string> _ids = new ();
	// A database of all materials, indexed by normalized names (`_ids`).
	// E.g.: zinanthid -> Zin'anthid
	private static ConcurrentDictionary<string, Material> _data = new ();

	// Parser & renderer definitions.
	private const string _formatDate = @"\!\!\!\ yyyy\-MM\-dd\ \!\!\!";
	private const string
		_prefixComment = "#",
		_prefixIndent = "\t",
		_prefixGuide = "guide: ",
		_prefixWowhead = "wh: ";
	private const string _separator = " >>> ";
	private const char _surround = '"';
	private const string
		_footerText = "wow-professions.com",
		_footerIcon = @"https://imgur.com/x0enbeT.png";
	private const string _bullet = "\u2022";

	// Configuration data.
	private const int _maxOptions = 20;
	private const string _pathVotes = @"data/farm-votes.txt";
	private readonly static string[] _pathData = new string[] {
		@"data/farm/herbs.txt",
		@"data/farm/meats.txt",
		@"data/farm/leathers.txt",
		@"data/farm/cloths.txt",
		@"data/farm/ores.txt",
		@"data/farm/elementals.txt",
		@"data/farm/other.txt",
	};

	static Farm() {
		Util.CreateIfMissing(_pathVotes);

		// Read in and cache all data.
		foreach (string path in _pathData)
			ParseDataFile(path);
	}

	public static DiscordColor GetColor(Quality quality) => quality switch {
		Quality.Poor      => new ("#9D9D9D"),
		Quality.Common    => new ("#FFFFFF"),
		Quality.Uncommon  => new ("#1EFF00"),
		Quality.Rare      => new ("#0070DD"),
		Quality.Epic      => new ("#A335EE"),
		Quality.Legendary => new ("#FF8000"),
		Quality.Artifact  => new ("#E6CC80"),
		Quality.Heirloom  => new ("#00CCFF"),
		_ => throw new ArgumentException("Unknown quality level.", nameof(quality)),
	};

	// Returns a Material object if one matches the query string, otherwise
	// returns null.
	public static Material? ParseMaterial(string query) {
		string id = NormalizeName(query);
		return (_data.ContainsKey(id))
			? _data[id]
			: null;
	}

	// Respond to an interaction with a message.
	// The response has to occur here, in order to set the message promise
	// for the select menu component (instead of in `Commands.Farm`).
	public static async Task RespondAsync(Interaction interaction, Material material) {
		DiscordEmbed embed = GetEmbed(material, material.Routes[0]);

		DiscordMessageBuilder response =
			new DiscordMessageBuilder()
			.WithEmbed(embed);

		interaction.RegisterFinalResponse();
		await interaction.RespondCommandAsync(response);
		interaction.SetResponseSummary($"Farming guide for: {material.Name}");
	}
	
	// Autocomplete valid options for a given query.
	public static List<(string, string)> AutocompleteOptions(string query) {
		string id = NormalizeName(query);

		// Search for matching options.
		List<(string, string)> options = new ();
		foreach (string option in _ids.Keys) {
			if (option.Contains(id))
				options.Add((_ids[option], _ids[option]));
		}

		// Limit the number of provided options.
		if (options.Count > _maxOptions)
			options = options.GetRange(0, _maxOptions);

		// Sort options.
		options.Sort();

		return options;
	}
	
	// Lower-case and strip all non-alpha characters from an input string,
	// resulting in a normalized identifier string.
	private static string NormalizeName(string name) {
		string id = name.Trim().ToLower();
		
		if (id == "")
			return id;

		StringBuilder builder = new ();
		foreach (char c in id) {
			// Only need to check lower-case characters.
			if (c is >='a' and <='z')
				builder.Append(c);
		}
		return builder.ToString();
	}

	// Render an embed object for the given Material and Route.
	private static DiscordEmbed GetEmbed(Material material, Route route) {
		// Construct main body of embed.
		string description = (route.Comments != "")
			? $"\n{route.Comments.Unescape()}\n"
			: "";
		string content =
			$"""
			**{route.Name}**
			{description}
			[Original guide]({material.Guide}) {_bullet} [Wowhead]({material.Wowhead})
			""";

		// Set all embed fields.
		DiscordEmbedBuilder embed =
			new DiscordEmbedBuilder()
			.WithTitle(material.Name)
			.WithUrl(material.Guide)
			.WithColor(GetColor(material.Quality))
			.WithThumbnail(material.Icon)
			.WithDescription(content)
			.WithImageUrl(route.Image)
			.WithFooter(_footerText, _footerIcon)
			.WithTimestamp(material.Timestamp.ToDateTime(new (0)));
		
		return embed.Build();
	}

	// Helper method to read in and cache all data from a file.
	// The input format must be exact (no error-checking is performed).
	private static void ParseDataFile(string path) {
		List<string> lines = new (File.ReadAllLines(path));

		// Parse the last updated date of the file.
		// This must be the first line, and follow the format exactly.
		DateOnly date = DateOnly.ParseExact(lines[0], _formatDate);

		// Start parsing after the first line (date).
		for (var i = 1; i<lines.Count; i++) {
			string line = lines[i];

			// Skip comment lines and empty lines.
			if (line.StartsWith(_prefixComment) || line.Trim() == "")
				continue;

			string name = line;

			// Parse quality + icon line.
			i++; line = lines[i].Trim();
			string[] split = line.Split(_separator, 2);
			Quality quality = Enum.Parse<Quality>(split[0]);
			string icon = split[1];

			// Parse links.
			i++; line = lines[i].Trim();
			string guide = line.Remove(0, _prefixGuide.Length);
			i++; line = lines[i].Trim();
			string wowhead = line.Remove(0, _prefixWowhead.Length);

			// Parse routes.
			List<Route> routes = new ();
			while (i+1 < lines.Count && lines[i+1].StartsWith(_prefixIndent)) {
				i++; line = lines[i].Trim();
				split = line.Split(_separator, 4);
				string routeId = split[0];
				string routeName = split[1];
				string routeComments = split[2].Trim(_surround);
				string routeImage = split[3];
				Route route = new (
					routeId,
					routeName,
					routeComments,
					routeImage
				);
				routes.Add(route);
			}

			// Instantiate object from parsed data.
			Material material = new (
				name,
				quality, icon, guide, wowhead,
				routes,
				date
			);

			// Add parsed material to caches.
			string materialId = NormalizeName(name);
			_ids.TryAdd(materialId, name);
			_data.TryAdd(materialId, material);
		}
	}
}
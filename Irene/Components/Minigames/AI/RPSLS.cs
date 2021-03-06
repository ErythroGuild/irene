using static Irene.Components.Minigames.RPSLS;

namespace Irene.Components.Minigames.AI;

static class RPSLS {
	public static async Task<Choice> NextChoice(ulong opponent_id) {
		// Select choice.
		Choice choice = (Choice)Random.Shared.Next(5);

		// Fuzzed delay.
		await Task.Delay(Random.Shared.Next(0, 1800));

		// Return result.
		return choice;
	}
}

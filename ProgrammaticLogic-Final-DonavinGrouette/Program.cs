using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ProgrammaticLogic_Final_DonavinGrouette
{
    class Program
    {
        public static List<PlayersHand> AllPlayers = new List<PlayersHand>();

        static void Main(string[] args)
        {
            // Never wait a week todo a project, 'cuz apparently I can forget how to code. Lesson learned.
            // I also learned late into this, most of these could have been done with linq queries, group by, and count. Oops.

            // Example Decks
            // Five of a kind -> Ace Spades, Ace Hearts, Ace Clubs, Ace Diamonds, Joker
            // Straight Flush -> Jack Spades, Ten Spades, Nine Spades, Eight Spades, Seven Spades
            // Full House -> Six Spades, Six Hearts, Six Diamonds, King Clubs, King Hearts 
            // Ace high straight -> Ace Clubs, King Spades, Queen Clubs, Jack Clubs, Ten Clubs
            // Pair -> Seven Hearts, Seven Diamonds, eight Clubs, ten spades, ace hearts
            string input = "";
            while (input != "0")
            {
                Console.WriteLine("Please Choose and Option");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Add a player and their deck");
                Console.WriteLine("2. Play Game");

                if (AllPlayers.Count != 0)
                {
                    foreach (var player in AllPlayers)
                    {
                        Console.Write(player.Name + ": With - ");
                        foreach (var card in player.Cards)
                        {
                            Console.Write(card.Combined + " ");
                        }
                        Console.WriteLine();
                    }
                }

                if (input == "1")
                {
                    string playerName = "";
                    string playerDeck = "";

                    Console.Write("Player Name: ");
                    playerName = Console.ReadLine();
                    Console.WriteLine("Cards: Ace, Two, Three, Four, ..., Jack, Queen, King, Joker Suites: Spades, Hearts, Diamonds, Clubs");
                    Console.WriteLine("Example Cards: Ace Spades, Four Clubs, King Diamonds, Jack Hearts, Five Clubs, Joker");
                    Console.WriteLine("Requirements: ");
                    Console.WriteLine("> Only five cards allowed. (EX: jack hearts, ace clubs, ace diamonds, king clubs, joker)");
                    Console.WriteLine("> Rank and Suite must be separated by a space  (EX: Ace Clubs)");
                    Console.WriteLine("> Cards must be separated by a comma (EX: Four Clubs, King Diamonds)");

                    do
                    {
                        Console.WriteLine();
                        Console.WriteLine("Please input a correct deck.");
                        Console.Write("Player Deck: ");
                        playerDeck = Console.ReadLine();
                    } while (!Game.CreatePlayer(playerName, playerDeck));

                    Console.WriteLine("Deck Added..");
                }
                else if (input == "2")
                {
                    if (AllPlayers.Count > 1)
                    {
                        var game = new Game(AllPlayers);
                    }
                    else
                    {
                        Console.WriteLine("Not enough players to play. (Needs two players atleast.)");
                    }
                }

                input = Console.ReadLine();
            }

        }
    }

    class Game
    {
        public Game(List<PlayersHand> players)
        {
            var handsToBeChecked = new List<PlayersHand>();

            foreach (var player in players)
            {
                player.HandResults = CheckCards(player.Cards);
                handsToBeChecked.Add(player);
            }


            // If there's two or more players with the same winning hand -> compare by their highest cards -> grab the player with the highest card.
            Console.WriteLine("=======MATCH RESULTS========");
            if (handsToBeChecked.All(p => p.HandResults == Card.HandRank.NONE))
            {
                // List all winners if there's a tie.
                Console.WriteLine(" > NO WINNER");
            }
            else
            {
                Console.WriteLine("========= WINNER ==========");

                var winnerList = GetWinners(handsToBeChecked);

                if (winnerList == null)
                {
                    Console.WriteLine(" > NO WINNER");
                }
                else
                {
                    foreach (var player in winnerList)
                    {
                        handsToBeChecked.Remove(player);

                        Console.Write(player.Name + ": ");
                        foreach (var card in player.Cards)
                        {
                            Console.Write($"{card.Combined} ");
                        }
                        Console.WriteLine($"With a {player.HandResults}");
                    }

                    Console.WriteLine("============================");
                    // List remainder players
                    foreach (var player in handsToBeChecked)
                    {
                        Console.Write(player.Name + ": ");
                        foreach (var card in player.Cards)
                        {
                            Console.Write($"{card.Combined} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        private List<PlayersHand> GetWinners(List<PlayersHand> players)
        {
            // If there's players in the list at the top with the same winning HandResult, return only those players; else return the single winner (top result)
            // Sort list by decending HandResults
            // if their's more than one player with the same "Winning" handResult
            // Return those tied players
            // Else 
            // Return the single winning player.

            List<PlayersHand> ListOfWinners = new List<PlayersHand>();

            // Order by decending places highest ranked card at the top,
            var OrderedHands = players.OrderByDescending(p => p.HandResults);

            var Query = from p in OrderedHands
                        orderby p.HandResults descending
                        group p by p.HandResults into newGroup
                        select newGroup;

            if (Query.All(p => p.Count() == 1))
            {
                // Because there's a chance more than one item can be returned, have to return a list. but we only want to send one winner right here.
                List<PlayersHand> winner = new List<PlayersHand>
                {
                    OrderedHands.FirstOrDefault()
                };

                return winner;
            }
            else
            {
                List<PlayersHand> winners = new List<PlayersHand>();

                foreach (var item in Query.FirstOrDefault())
                {
                    winners.Add(item);
                }

                return winners;
            }
        }

        private static Card.HandRank CheckCards(List<Card> cards)
        {
            // CASES:
            // Ordered by weight of hands. Straight flush would win over a Four of a kind, Four of a kind would win over a Full house, etc, etc.
            // Five of a kind   - Using a wild card to achieve five of a kind
            // Straight flush   - Use check from Straight and Flush
            // Four of a kind   - (a7 == b7) && (a7 == c7) && (a7 == d7)
            // Full House       - use Two of a kind and Three of a kind check here.
            // Flush            - Hand is all same kind of suite (for card in decks) if all pass first card suite then it's a straight
            // Straight         - ++++ 6 7 8 9 10 | A 2 3 4 5 | 10 J Q K A ++++ - (cards match a loop counter. loop x (a1 == x1 && b2 == x2 && c3 == x3 && d4 == x4 && e5 == x5))
            // Three of a kind  - (a7 == b7) && (a7 == c7)
            // Two Pair         - (a7 == b7) && (c2 == d2)
            // Pair             - (a7 == b7)

            // Sort the hand first
            var sortedhand = cards.OrderBy(p => (int)(p.Value)).ToList();
            // List for pairs
            var matches = new List<Card>();
            // List for straight
            var straightMatchList = new List<Card>();

            for (int i = 0; i < sortedhand.Count; i++)
            {
                // Checking for a Straight / Straight Flush first.
                if (!straightMatchList.Contains(sortedhand[i]))
                {
                    // Check for a loop count 1 -> 5 etc etc add all the cards.
                    // we add the card to matches if there's a match of value
                    // This is for the last num in the players hand.
                    if (i == sortedhand.Count - 1)
                    {
                        if (sortedhand[i - 1].Value == sortedhand[sortedhand.Count - 1].Value - 1)
                        {
                            straightMatchList.Add(sortedhand[i]);
                        }
                    }
                    // This is where most of the straight checks will happen. 
                    // If the cards value next in the enum list (+1) is equal to the next card in the players hand; add it to the match list.
                    else if (sortedhand[i].Value + 1 == sortedhand[i + 1].Value)
                    {
                        straightMatchList.Add(sortedhand[i]);
                    }
                    else if (sortedhand[i].Value == Card.Ranks.Ace && sortedhand[i + 1].Value == Card.Ranks.Ten || sortedhand[i].Value == Card.Ranks.King && sortedhand.Any(p => p.Value == Card.Ranks.Ace))
                    {
                        // This case specifically hands Ace high in a straight
                        straightMatchList.Add(sortedhand[i]);
                    }
                }

                // Checking for pairs
                if (!matches.Contains(cards[i]))
                {
                    // If the match count for that card is greater than one, add it.
                    if (cards.Count(p => p.Value == cards[i].Value) > 1)
                    {
                        matches.Add(cards[i]);
                    }
                    else if (cards[i].Value == Card.Ranks.Joker)
                    {
                        // Because wild cards always match
                        matches.Add(cards[i]);
                    }
                    // we add it if there's a match of somekind
                }
            }

            // : two in matches -> pair 
            // : three in matches -> three of a kind 
            // : four in matches -> either two pair or four of a kind. Four of a kind has a higher weight however so it's checked much earlier.
            // : five in matches -> full house or five of a kind
            if (matches.Count == 5 && matches.Any(p => p.Value == Card.Ranks.Joker))
            {
                return Card.HandRank.FIVE_OF_A_KIND;
            }
            if (straightMatchList.All(p => p.Suite == cards[0].Suite) && straightMatchList.Count == cards.Count)
            {
                return Card.HandRank.STRAIGHT_FLUSH;
            }
            if (matches.Count(p => p.Value == matches[0].Value) == 4)
            {
                return Card.HandRank.FOUR_OF_A_KIND;
            }
            if (matches.Count == 5)
            {
                return Card.HandRank.FULL_HOUSE;
            }
            if (cards.All(p => p.Suite == cards[0].Suite))
            {
                return Card.HandRank.FLUSH;
            }
            if (straightMatchList.Count == sortedhand.Count)
            {
                return Card.HandRank.STRAIGHT;
            }
            if (matches.Count == 3)
            {
                return Card.HandRank.THREE_OF_A_KIND;
            }
            if (matches.Count == 4)
            {
                return Card.HandRank.TWO_PAIR;
            }
            if (matches.Count == 2)
            {
                return Card.HandRank.PAIR;
            }

            return Card.HandRank.NONE;
        }

        public static bool CreatePlayer(string playerName, string playerDeck)
        {
            if (playerName == "" || playerDeck == "") // Input String Check -> Checks to ensure either inputs aren't empty
            {
                return false;
            }
            else
            {
                var parsedDeck = playerDeck.Split(',');
                if (parsedDeck.Count() != 5) // Input String Check -> Checks to ensure there's atleast 5 strings between commas
                {
                    return false;
                }

                var convertedDeck = new List<Card>();

                foreach (var item in parsedDeck)
                {

                    var splitCardValues = item.Trim().Split(' ');

                    if (splitCardValues.Count() != 2) // Input String Check -> Checks to ensure there's atleast 2 strings separated by spaces
                    {
                        return false;
                    }

                    if (splitCardValues[0].ToUpper() == "JOKER")
                    {
                        if (Enum.TryParse("joker", true, out Card.Ranks rankTP) && Enum.TryParse("none", true, out Card.Suites suiteTP))
                        {
                            convertedDeck.Add(new Card(suiteTP, rankTP));
                        }
                    }
                    else
                    {
                        var rankString = splitCardValues[0];
                        var suiteString = splitCardValues[1];

                        if (Enum.TryParse(rankString, true, out Card.Ranks rankTP) && Enum.TryParse(suiteString, true, out Card.Suites suiteTP))
                        {
                            convertedDeck.Add(new Card(suiteTP, rankTP));
                        }
                        else
                        {
                            return false;
                        }

                    }

                }

                Program.AllPlayers.Add(new PlayersHand(playerName, convertedDeck));
                return true;
            }
        }
    }

    class PlayersHand
    {
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
        public Card.HandRank HandResults { get; set; }

        public PlayersHand(string name, List<Card> deltCards)
        {
            Name = name;
            Cards = deltCards;
        }
    }

    class Card
    {
        public Suites Suite { get; set; }
        public Ranks Value { get; set; }
        public string Combined { get; set; }

        public Card(Suites suite, Ranks rank)
        {
            Suite = suite;
            Value = rank;
            Combined = $"{rank}{suite}";
        }

        // List of possible Suites
        public enum Suites
        {
            None, // Cuz the Joker doesn't have a suite.
            Spades,
            Clubs,
            Hearts,
            Diamonds
        }

        // List of possible Values/ Ranks
        // A 2 3 4 5 6 7 8 9 10 J Q K Jk
        public enum Ranks
        {
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King,
            Joker,
        }

        public enum HandRank
        {
            NONE,
            PAIR,
            TWO_PAIR,
            THREE_OF_A_KIND,
            STRAIGHT,
            FULL_HOUSE,
            FLUSH,
            FOUR_OF_A_KIND,
            STRAIGHT_FLUSH,
            FIVE_OF_A_KIND
        }
    }
}

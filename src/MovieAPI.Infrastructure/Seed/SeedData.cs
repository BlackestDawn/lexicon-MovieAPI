using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;

namespace MovieAPI.Infrastructure.Seed;

public static class SeedData
{
  public static void Seed(AppDbContext context)
  {
    if (context.Movies.Any())
      return;

    var now = DateTime.UtcNow;

    // Genres
    var action = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Action", Slug = "action" };
    var animation = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Animation", Slug = "animation" };
    var comedy = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Comedy", Slug = "comedy" };
    var documentary = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Documentary", Slug = "documentary" };
    var drama = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Drama", Slug = "drama" };
    var fantasy = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Fantasy", Slug = "fantasy" };
    var horror = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Horror", Slug = "horror" };
    var romance = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Romance", Slug = "romance" };
    var sciFi = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Sci-Fi", Slug = "sci-fi" };
    var thriller = new Genre { CreatedAt = now, UpdatedAt = now, Name = "Thriller", Slug = "thriller" };

    // People
    var elenaMarsh = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Elena", LastName = "Marsh", DateOfBirth = new DateOnly(1978, 4, 12) };
    var marcusWebb = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Marcus", LastName = "Webb", DateOfBirth = new DateOnly(1970, 1, 15) };
    var sofiaLindqvist = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Sofia", LastName = "Lindqvist", DateOfBirth = new DateOnly(1990, 9, 8) };
    var jonahAde = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Jonah", LastName = "Ade", DateOfBirth = new DateOnly(1985, 3, 30) };
    var lucasOwusu = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Lucas", LastName = "Owusu", DateOfBirth = new DateOnly(1987, 3, 3) };
    var claraVoss = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Clara", LastName = "Voss", DateOfBirth = new DateOnly(1993, 12, 19) };
    var oliverFinch = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Oliver", LastName = "Finch", DateOfBirth = new DateOnly(1995, 4, 21) };
    var raviChandran = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Ravi", LastName = "Chandran", DateOfBirth = new DateOnly(1975, 10, 5) };
    var tomasReyes = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Tomas", LastName = "Reyes", DateOfBirth = new DateOnly(1965, 11, 2) };
    var diegoMarin = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Diego", LastName = "Marin", DateOfBirth = new DateOnly(1979, 6, 11) };
    var ninaPark = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Nina", LastName = "Park", DateOfBirth = new DateOnly(1988, 2, 27) };
    var helenaBrandt = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Helena", LastName = "Brandt", DateOfBirth = new DateOnly(1969, 8, 14) };
    var priyaAnand = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Priya", LastName = "Anand", DateOfBirth = new DateOnly(1982, 7, 23) };
    var inesCastillo = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Ines", LastName = "Castillo", DateOfBirth = new DateOnly(1983, 5, 17) };
    var meiTanaka = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Mei", LastName = "Tanaka", DateOfBirth = new DateOnly(1991, 7, 9) };
    var aaronCole = new Person { CreatedAt = now, UpdatedAt = now, FirstName = "Aaron", LastName = "Cole", DateOfBirth = new DateOnly(1972, 12, 30) };

    // Movies
    var echoesOfTomorrow = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Echoes of Tomorrow",
      ReleaseDate = new DateOnly(2021, 3, 12),
      PlotSummery = "A scientist discovers a way to send messages across time, but every message she sends changes the present in ways she never expected.",
      RuntimeMinutes = 142
    };
    var midnightGarden = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "The Midnight Garden",
      ReleaseDate = new DateOnly(2019, 10, 31),
      PlotSummery = "On the night of the autumn equinox, a lonely florist finds a door in her shop that leads to a garden where lost loves wait to be found again.",
      RuntimeMinutes = 109
    };
    var silentEngines = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Silent Engines",
      ReleaseDate = new DateOnly(2022, 6, 17),
      PlotSummery = "When a cargo freighter goes dark in international waters, an investigator uncovers a conspiracy that reaches the highest levels of government.",
      RuntimeMinutes = 128
    };
    var paperMoons = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Paper Moons",
      ReleaseDate = new DateOnly(2017, 2, 14),
      PlotSummery = "Two strangers exchange love letters meant for other people and slowly fall for the voices behind the words.",
      RuntimeMinutes = 101
    };
    var glassForest = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "The Glass Forest",
      ReleaseDate = new DateOnly(2020, 9, 25),
      PlotSummery = "A group of hikers stumble upon an abandoned research station deep in the woods, where something in the trees has been waiting for them.",
      RuntimeMinutes = 97
    };
    var wanderingStars = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Wandering Stars",
      ReleaseDate = new DateOnly(2023, 11, 22),
      PlotSummery = "A young astronomer and a wandering comet spirit travel across a hand-painted sky to find the star that grants one wish.",
      RuntimeMinutes = 95
    };
    var pancakeTuesdays = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Pancake Tuesdays",
      ReleaseDate = new DateOnly(2016, 5, 6),
      PlotSummery = "A failing diner owner enlists his eccentric regulars to save his restaurant with a once-a-week all-you-can-eat pancake special.",
      RuntimeMinutes = 88
    };
    var concreteSky = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Concrete Sky",
      ReleaseDate = new DateOnly(2015, 8, 21),
      PlotSummery = "A demolition crew foreman fights to save his neighborhood from a corrupt redevelopment deal that threatens to bulldoze his childhood home.",
      RuntimeMinutes = 135
    };
    var cartographersDaughter = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "The Cartographer's Daughter",
      ReleaseDate = new DateOnly(2024, 1, 19),
      PlotSummery = "After inheriting her father's map shop, a young woman discovers a hand-drawn map to a place that, according to every other map, does not exist.",
      RuntimeMinutes = 124
    };
    var staticHorizon = new Movie
    {
      CreatedAt = now,
      UpdatedAt = now,
      Title = "Static Horizon",
      ReleaseDate = new DateOnly(2022, 12, 3),
      PlotSummery = "A documentary crew embedded with a remote radio observatory captures the months leading up to an unexplained signal from deep space.",
      RuntimeMinutes = 105
    };

    context.Genres.AddRange(action, animation, comedy, documentary, drama, fantasy, horror, romance, sciFi, thriller);
    context.Persons.AddRange(
      elenaMarsh, marcusWebb, sofiaLindqvist, jonahAde, lucasOwusu, claraVoss, oliverFinch, raviChandran,
      tomasReyes, diegoMarin, ninaPark, helenaBrandt, priyaAnand, inesCastillo, meiTanaka, aaronCole);
    context.Movies.AddRange(
      echoesOfTomorrow, midnightGarden, silentEngines, paperMoons, glassForest,
      wanderingStars, pancakeTuesdays, concreteSky, cartographersDaughter, staticHorizon);

    // Movie <-> Genre links
    context.MovieGenres.AddRange(
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Genre = sciFi },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Genre = drama },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Genre = fantasy },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Genre = romance },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Genre = action },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Genre = thriller },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Genre = drama },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Genre = romance },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Genre = horror },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Genre = thriller },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Genre = animation },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Genre = fantasy },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, Genre = comedy },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Genre = drama },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Genre = action },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Genre = drama },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Genre = fantasy },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, Genre = documentary },
      new MovieGenre { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, Genre = sciFi });

    // Cast & crew
    context.CastCrews.AddRange(
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Person = elenaMarsh, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Person = marcusWebb, Role = PersonRole.Writer },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Person = sofiaLindqvist, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, Person = jonahAde, Role = PersonRole.Cast },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Person = lucasOwusu, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Person = claraVoss, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Person = oliverFinch, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, Person = raviChandran, Role = PersonRole.Composer },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Person = tomasReyes, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Person = diegoMarin, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Person = ninaPark, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, Person = helenaBrandt, Role = PersonRole.Cinematographer },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Person = priyaAnand, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Person = marcusWebb, Role = PersonRole.Writer },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Person = inesCastillo, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, Person = oliverFinch, Role = PersonRole.Cast },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Person = tomasReyes, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Person = meiTanaka, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Person = jonahAde, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = glassForest, Person = aaronCole, Role = PersonRole.Producer },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Person = lucasOwusu, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Person = claraVoss, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Person = sofiaLindqvist, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, Person = raviChandran, Role = PersonRole.Composer },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, Person = elenaMarsh, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, Person = diegoMarin, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, Person = ninaPark, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, Person = meiTanaka, Role = PersonRole.Cast },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Person = priyaAnand, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Person = jonahAde, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Person = inesCastillo, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, Person = aaronCole, Role = PersonRole.Producer },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Person = tomasReyes, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Person = marcusWebb, Role = PersonRole.Writer },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Person = claraVoss, Role = PersonRole.Cast },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, Person = oliverFinch, Role = PersonRole.Cast },

      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, Person = lucasOwusu, Role = PersonRole.Director },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, Person = helenaBrandt, Role = PersonRole.Cinematographer },
      new CastCrew { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, Person = sofiaLindqvist, Role = PersonRole.Cast });

    // Reviews
    context.Reviews.AddRange(
      new Review { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, AuthorName = "Grace Tan", Body = "A mind-bending take on time travel that never loses sight of its emotional core. The final act left the whole theatre in silence.", Score = 9 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = echoesOfTomorrow, AuthorName = "Leo Martins", Body = "Visually stunning, though the plot occasionally gets tangled in its own timelines. Still, well worth the watch.", Score = 7 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, AuthorName = "Hana Kobayashi", Body = "Whimsical and bittersweet in equal measure. The garden set design alone is worth the price of admission.", Score = 8 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = midnightGarden, AuthorName = "Sam Whitfield", Body = "A lovely premise that runs a little long in the middle, but the ending ties everything together beautifully.", Score = 6 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, AuthorName = "Priya Nair", Body = "Tense from the opening scene to the last. One of the smartest thrillers in years.", Score = 9 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = silentEngines, AuthorName = "Carlos Bett", Body = "Great pacing and a genuinely surprising twist. The sound design deserves special mention.", Score = 8 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, AuthorName = "Maya Lindstrom", Body = "A quiet, tender story about connection in the digital age. The lead performances carry it effortlessly.", Score = 8 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = paperMoons, AuthorName = "Theo Brooks", Body = "Charming and a little predictable, but the chemistry between the leads makes up for it.", Score = 7 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = glassForest, AuthorName = "Aisha Bello", Body = "Genuinely unsettling without relying on cheap jump scares. The forest itself feels like a character.", Score = 8 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = glassForest, AuthorName = "Noah Park", Body = "Atmospheric and creepy, though the ending felt a bit rushed compared to the slow build.", Score = 6 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, AuthorName = "Ivy Chen", Body = "Absolutely gorgeous animation paired with a story that's as moving for adults as it is for kids.", Score = 10 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = wanderingStars, AuthorName = "Marcus Lee", Body = "Every frame looks like a painting. A new favorite for family movie night.", Score = 9 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, AuthorName = "Della Frost", Body = "Silly, warm, and exactly the comfort-food comedy it sets out to be.", Score = 7 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = pancakeTuesdays, AuthorName = "Ezra Quinn", Body = "Some jokes land better than others, but the cast's chemistry keeps it afloat.", Score = 6 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, AuthorName = "Nora Saleh", Body = "A grounded, angry, and ultimately hopeful look at a neighborhood fighting for its survival.", Score = 8 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = concreteSky, AuthorName = "Pete Conway", Body = "Strong performances anchor a story that sometimes leans too hard on familiar beats.", Score = 7 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, AuthorName = "Yuki Mori", Body = "A beautifully crafted adventure with a real sense of wonder. The map reveals are a highlight.", Score = 9 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = cartographersDaughter, AuthorName = "Ben Okafor", Body = "Inventive world-building and a satisfying mystery at its core.", Score = 8 },

      new Review { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, AuthorName = "Rosa Delgado", Body = "A quietly gripping documentary that builds dread without a single special effect.", Score = 9 },
      new Review { CreatedAt = now, UpdatedAt = now, Movie = staticHorizon, AuthorName = "Tariq Hassan", Body = "Fascinating subject matter, even if the pacing drags in the second act.", Score = 7 });

    context.SaveChanges();
  }
}
